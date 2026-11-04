using System;
using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  string cityActivityTooltip="";
  string BuildingActivity(string id,out string detail){
   detail="";string prefix="Next ",item="";double hours=double.PositiveInfinity;
   if(city.activeConstruction==id){prefix="Build ";item="Level "+city.constructionTarget+" construction";hours=city.constructionRemaining;}
   else if(id=="library"&&city.activeResearch!=""){prefix="Study ";item=city.activeResearch+" research";hours=city.researchRemaining;}
   else if(id=="blacksmith"){int metal=city.ActiveBarMetal;if(metal>=0){item=WeaponRules.BarName(metal);hours=city.NextBarHours;}else if(city.autoBarsEnabled){detail="Auto refining: "+WeaponRules.BarName(city.autoBarMetal)+" / waiting for ingredients or research.";return "Waiting";}}
   else if(id=="clothier"&&TryMaterialProcessingActivity(out string material,out double time,out _)){item=material;hours=time;}
   else if(id=="mine"){int resource=city.ActiveMineResource;if(resource>=0){item=Settlement.ResourceNames[0][resource];hours=city.NextMineHours;}}
   else if(id=="timber"||id=="quarry"){item=id=="timber"?"Timber":"Stone";hours=ProductionTime.NextUnit(id=="timber"?city.wood:city.stone,id=="timber"?city.WoodRate:city.StoneRate);}
   else {int source=CitySource(id);if(source>0)for(int i=0;i<Settlement.ResourceNames[source].Length;i++){double next=ProductionTime.NextUnit(city.ResourceAmount(source,i),city.ProductionRate(source,i));if(next<hours){hours=next;item=Settlement.ResourceNames[source][i];}}}
   if(double.IsInfinity(hours))return city.BuildingLevel(id)>0&&(id=="mine"||id=="library"||id=="blacksmith")?"Idle":"";
   detail=item+" due in "+ProductionTime.Duration(hours)+" city time\nAt town speed: about "+ProductionTime.Duration(hours/60)+" real time. Expeditions accelerate the city clock.";
   return prefix+ProductionTime.Duration(hours);
  }
  Rect CityActivityRect(int index){Rect label=CityBuildingLabel(index);bool left=index==1||label.xMax+95>CityMapRect.xMax;return new Rect(left?label.x-95:label.xMax+6,label.y+4,89,23);}
  void DrawCityActivity(int index){
   string text=BuildingActivity(CityMapBuildings[index],out string tooltip);if(text=="")return;Rect r=CityActivityRect(index);CityFrame(r);GUI.Label(r,text,CityType(10,cityIvory,false,TextAnchor.MiddleCenter));
   if(r.Contains(Event.current.mousePosition)){cityActivityTooltip=tooltip;if(Event.current.type==EventType.MouseDown&&Event.current.button==0){SelectCityBuilding(CityMapBuildings[index]);Event.current.Use();}}
  }
  void CityActivityTooltip(){if(cityActivityTooltip=="")return;Vector2 p=Event.current.mousePosition;Rect r=new Rect(Mathf.Clamp(p.x-130,30,746),Mathf.Clamp(p.y+28,195,784),308,65);CityFrame(r);GUI.Label(new Rect(r.x+9,r.y+7,r.width-18,r.height-14),cityActivityTooltip,CityType(11,cityIvory));}
 }
}
