using System;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Veilbound {
 public partial class VeilGame {
  bool cityDetailsOpen;Vector2 citySidebarScroll;float citySidebarStickCooldown;
  readonly string[] CitySidebarBuildings={"timber","mine","blacksmith","hunters","weaver","quarry","library","clothier","herbalist","enchanter","housing"};
  Rect CitySidebarRect=>new Rect(1178,176,398,686);
  Rect CitySidebarViewport=>new Rect(1190,246,374,500);
  Rect CityHousingCard=>new Rect(1190,773,374,76);
  struct SidebarActivity {public string output,status;public double remaining;public float progress;public bool active;}
  SidebarActivity SidebarState(string id){
   var a=new SidebarActivity{output="Idle",status="Idle",remaining=double.PositiveInfinity};
   if(city.activeConstruction==id){a.output="Construction / Level "+city.constructionTarget;a.remaining=city.constructionRemaining;a.progress=city.constructionTotal>0?1-(float)(a.remaining/city.constructionTotal):0;a.active=true;}
   else if(id=="library"&&city.activeResearch!=""){int branch=LibraryResearch.Index(city.activeResearch);a.output=(branch>=0?LibraryResearch.Names[branch]:city.activeResearch)+" "+city.researchTarget;a.remaining=city.researchRemaining;a.progress=city.researchTotal>0?1-(float)(a.remaining/city.researchTotal):0;a.active=true;}
   else if(id=="clothier"&&TryMaterialProcessingActivity(out string material,out double time,out float processProgress)){a.output=material;a.remaining=time;a.progress=processProgress;a.active=true;}
   else if(id=="mine"){int item=city.ActiveMineResource;if(item>=0){a.output=Settlement.ResourceNames[0][item];a.remaining=city.NextMineHours;double work=city.mineOrders.Count>0?city.mineOrders[0].remainingWork:city.AutoMineWork;a.progress=1-(float)(work/Settlement.MineUnitWork(item));a.active=true;}else if(city.mineSchedule.Count>0){a.output=Settlement.ResourceNames[0][city.autoMineResource];a.status=city.autoMineEnabled?"Waiting":"Paused";}}
   else if(id=="blacksmith"){int metal=city.ActiveBarMetal;if(metal>=0){a.output=WeaponRules.BarName(metal);a.remaining=city.NextBarHours;double work=city.smithOrders.Count>0?city.smithOrders[0].remainingWork:city.AutoBarWork;a.progress=1-(float)(work/SmithRefining.WorkHours[metal]);a.active=true;}else if(city.barSchedule.Count>0){a.output=WeaponRules.BarName(city.autoBarMetal);a.status=city.autoBarsEnabled?"Waiting":"Paused";}}
   else if(id=="timber"||id=="quarry"){double rate=id=="timber"?city.WoodRate:city.StoneRate,stock=id=="timber"?city.wood:city.stone;if(rate>0){a.output=id=="timber"?"Wood":"Stone";a.remaining=ProductionTime.NextUnit(stock,rate);a.progress=(float)(stock-Math.Floor(stock));a.active=true;}}
   else {int source=CitySource(id);if(source>0)for(int item=0;item<Settlement.ResourceNames[source].Length;item++){double stock=city.ResourceAmount(source,item),next=ProductionTime.NextUnit(stock,city.ProductionRate(source,item));if(next<a.remaining){a.output=Settlement.ResourceNames[source][item];a.remaining=next;a.progress=(float)(stock-Math.Floor(stock));a.active=true;}}}
   if(city.BuildingLevel(id)>0&&city.activeConstruction!=id&&(id=="mine"||id=="blacksmith"||id=="clothier"||id=="hunters"||id=="weaver")){var activity=city.ProductionOverview(id);a.active=activity.working;a.output=activity.working?activity.item:activity.detail;a.status=activity.tag;a.remaining=activity.hours;a.progress=activity.progress;}
   if(a.active)a.status=ProductionTime.Duration(a.remaining);else if(city.BuildingLevel(id)==0){a.output=id=="herbalist"?"Reserved plot":"Not built";a.status=id=="herbalist"?"Planned":"Build";}
   else if(a.output=="Idle")a.output=id=="clothier"?"Ready to craft":id=="library"?"Choose research":"Choose production";
   a.progress=Mathf.Clamp01(a.progress);return a;
  }
  void KeepSidebarVisible(string id){var visible=VisibleSidebarBuildings();int i=Array.IndexOf(visible,id);if(i<0||id=="housing")return;float top=i*62,bottom=top+62;if(top<citySidebarScroll.y)citySidebarScroll.y=top;else if(bottom>citySidebarScroll.y+500)citySidebarScroll.y=bottom-500;citySidebarScroll.y=Mathf.Clamp(citySidebarScroll.y,0,Mathf.Max(0,(visible.Length-1)*62-500));}
  void CycleSidebarBuilding(int direction){var visible=VisibleSidebarBuildings();int i=Array.IndexOf(visible,cityBuilding);SelectCityBuilding(visible[(Math.Max(0,i)+visible.Length+direction)%visible.Length]);}
  void OpenCityBuilding(string id){
   if(!city.BuildingVisible(id))return;SelectCityBuilding(id);LeaveResourceBar();if(city.BuildingLevel(id)==0){cityDetailsOpen=true;return;}int source=CitySource(id);
   if(source>=0){OpenProducer(source);return;}if(id=="blacksmith"){OpenCityWorkshop(false);return;}if(id=="clothier"){OpenClothier(0);return;}if(id=="enchanter"){OpenMystic();return;}if(id=="library"){libraryResearchOpen=true;SelectResearchTab(researchTab);selection=0;return;}cityDetailsOpen=true;
  }
  void OpenCityBuildingDetails(string id){if(!city.BuildingVisible(id))return;SelectCityBuilding(id);LeaveResourceBar();cityDetailsOpen=true;selection=0;}
  bool ReadSidebarInput(){
   var key=Keyboard.current;var pad=PreferredPad;
   if(cityDetailsOpen){if(key?.escapeKey.wasPressedThisFrame==true||pad?.buttonEast.wasPressedThisFrame==true){cityDetailsOpen=false;selection=SidebarButtonIndex(cityBuilding);return true;}return false;}
   bool next=key?.downArrowKey.wasPressedThisFrame==true||key?.tabKey.wasPressedThisFrame==true||pad?.dpad.down.wasPressedThisFrame==true||pad?.rightShoulder.wasPressedThisFrame==true;
   bool previous=key?.upArrowKey.wasPressedThisFrame==true||pad?.dpad.up.wasPressedThisFrame==true||pad?.leftShoulder.wasPressedThisFrame==true;
   citySidebarStickCooldown-=Time.unscaledDeltaTime;if(pad!=null&&citySidebarStickCooldown<=0){float y=pad.leftStick.y.ReadValue();if(y<-.6f)next=true;else if(y>.6f)previous=true;if(Math.Abs(y)>.6f)citySidebarStickCooldown=.22f;}
   if(next||previous){CycleSidebarBuilding(previous?-1:1);gamepadCombat=pad!=null;return true;}
   if(key?.uKey.wasPressedThisFrame==true||pad?.buttonWest.wasPressedThisFrame==true){OpenCityBuildingDetails(cityBuilding);return true;}
   return false;
  }
  void DrawCitySidebar(){
   ArchivePlate(CitySidebarRect,0,13);GUI.Label(new Rect(1197,191,357,31),"SETTLEMENT",ArchiveType(24,archiveIvory,true));GUI.Label(new Rect(1199,226,355,18),"BUILDINGS",ArchiveType(10,archiveMuted,true));
   var visible=VisibleSidebarBuildings();if(Array.IndexOf(visible,cityBuilding)<0)SelectCityBuilding(visible[0]);float height=Mathf.Max(500,(visible.Length-1)*62),scrollMax=height-500;citySidebarScroll.y=Mathf.Clamp(citySidebarScroll.y,0,scrollMax);
   citySidebarScroll=GUI.BeginScrollView(CitySidebarViewport,citySidebarScroll,new Rect(0,0,362,height),false,false,GUIStyle.none,GUIStyle.none);
   for(int i=0;i<visible.Length-1;i++)DrawCitySidebarRow(visible[i],new Rect(0,i*62,362,57));GUI.EndScrollView();
   if(scrollMax>0){float thumb=500f*500/height;Box(new Rect(1567,246,2,500),new Color(.15f,.13f,.1f));Box(new Rect(1567,246+(500-thumb)*(citySidebarScroll.y/scrollMax),2,thumb),new Color(.53f,.40f,.23f));GUI.Label(new Rect(1195,750,365,17),"Scroll for more buildings",ArchiveType(10,archiveMuted));}DrawCityHousingCard();
  }
  void DrawCitySidebarRow(string id,Rect row){
   int button=buttons.Count;buttons.Add(()=>OpenCityBuilding(id));bool chosen=cityBuilding==id,focus=selection==button;ArchivePlate(row,chosen?5:0,8);if(chosen)CardOutline(new Rect(row.x+1,row.y+1,row.width-2,row.height-2),archiveGold);RegisterMenuFocus(row,button,CityBuildingName(id),true);
   Rect portrait=new Rect(row.x+6,row.y+5,42,42);Color prior=GUI.color;GUI.color=city.BuildingLevel(id)>0?Color.white:new Color(.48f,.48f,.48f);DrawTownPortrait(portrait,id);GUI.color=prior;
   var a=SidebarState(id);GUI.Label(new Rect(row.x+62,row.y+5,220,20),CityBuildingName(id),ArchiveType(14,archiveIvory,true));GUI.Label(new Rect(row.x+294,row.y+6,53,19),"Lv "+city.BuildingLevel(id)+(CityBuildingMax(id)?"":" ↑"),ArchiveType(10,CityBuildingMax(id)?archiveMuted:archiveGold,false,TextAnchor.UpperRight));
   GUI.Label(new Rect(row.x+62,row.y+26,210,19),a.output,ArchiveType(11,a.active?archiveIvory:archiveMuted));GUI.Label(new Rect(row.x+263,row.y+26,84,19),a.status,ArchiveType(11,a.active?archiveIvory:archiveMuted,false,TextAnchor.UpperRight));
   ArchivePlate(new Rect(row.x+62,row.y+47,286,7),3,2);if(a.active)Box(new Rect(row.x+64,row.y+49,282*a.progress,4),archiveGold);
   GUI.Label(new Rect(row.x+348,row.y+20,12,20),"›",ArchiveType(17,archiveGold));
   if(!cityDetailsOpen&&Event.current.type==EventType.MouseDown&&Event.current.button==0&&row.Contains(Event.current.mousePosition)){selection=button;if(!CityBuildingMax(id)&&new Rect(row.x+280,row.y+3,67,22).Contains(Event.current.mousePosition))OpenCityBuildingDetails(id);else OpenCityBuilding(id);Event.current.Use();}
  }
  void DrawCityHousingCard(){
   Rect r=CityHousingCard;int button=buttons.Count;buttons.Add(()=>OpenCityBuilding("housing"));ArchivePlate(r,cityBuilding=="housing"?5:0,10);RegisterMenuFocus(r,button,"Housing",true);DrawTownPortrait(new Rect(r.x+7,r.y+12,51,47),"housing");GUI.Label(new Rect(r.x+66,r.y+10,140,23),"Housing",ArchiveType(17,archiveIvory,true));GUI.Label(new Rect(r.x+67,r.y+36,139,24),Settlement.HousingNames[city.housingTier],ArchiveType(12,archiveMuted));GUI.Label(new Rect(r.x+273,r.y+11,94,20),"Productivity",ArchiveType(11,archiveMuted));GUI.Label(new Rect(r.x+273,r.y+35,94,27),"+"+(city.housingTier*20)+"%",ArchiveType(22,archiveGold,true));
   if(city.activeConstruction=="housing"){float p=city.constructionTotal>0?1-(float)(city.constructionRemaining/city.constructionTotal):0;Box(new Rect(r.x+8,r.yMax-7,(r.width-16)*Mathf.Clamp01(p),3),archiveGold);}
   if(!cityDetailsOpen&&Event.current.type==EventType.MouseDown&&Event.current.button==0&&r.Contains(Event.current.mousePosition)){OpenCityBuilding("housing");Event.current.Use();}
  }
  void DrawCityDetailsOverlay(){
   Box(new Rect(24,176,1552,686),new Color(0,0,0,.66f));buttons.Clear();CityDetails();CityButton(1446,177,120,27,"Back to city",()=>{cityDetailsOpen=false;selection=SidebarButtonIndex(cityBuilding);});
  }
 }
}
