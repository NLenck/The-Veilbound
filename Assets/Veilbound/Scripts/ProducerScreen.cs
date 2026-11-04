using System;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Veilbound {
 public partial class VeilGame {
  bool producerOpen,producerEditing;int producerSource,producerItem,producerPage;string producerAmount="10",producerFeedback="";Vector2 producerScroll;
  readonly Texture2D[] producerBackdrops=new Texture2D[3];
  static readonly string[] ProducerArtPaths=Array.ConvertAll(ProductionBuildingCatalog.Gathering,d=>d.backdrop);
  static readonly string[] ProducerTitles=Array.ConvertAll(ProductionBuildingCatalog.Gathering,d=>d.title);
  static readonly string[] ProducerSubtitles=Array.ConvertAll(ProductionBuildingCatalog.Gathering,d=>d.subtitle);
  string ProducerBuilding=>city.Gatherer(producerSource).Definition.id;
  bool ProducerScreenOpen=>producerOpen&&state==ScreenState.Settlement&&tab==2&&!gearBagOpen&&!libraryResearchOpen&&!sanctuaryWorkshopOpen;
  int ProducerQuantity=>int.TryParse(producerAmount,out int n)?n:0;
  bool ProducerValidAmount=>double.TryParse(producerAmount,out double n)&&!double.IsNaN(n)&&!double.IsInfinity(n)&&n>=0;
  void OpenProducer(int source){producerSource=Mathf.Clamp(source,0,2);if(!producerBackdrops[producerSource])producerBackdrops[producerSource]=Resources.Load<Texture2D>(ProducerArtPaths[producerSource]);SelectCityBuilding(ProducerBuilding);producerOpen=true;producerItem=producerPage=0;producerScroll=Vector2.zero;producerFeedback="";producerEditing=false;ProducerSelect(0);selection=0;LeaveResourceBar();}
  void CloseProducer(){producerOpen=producerEditing=false;LeaveResourceBar();selection=SidebarButtonIndex(cityBuilding);}
  void ProducerSelect(int item){producerItem=item;producerAmount=producerSource==0?"10":city.Allocation(producerSource)[item].ToString("0.##");producerFeedback="";producerEditing=false;}
  string ProducerRequirement(int item){return city.Gatherer(producerSource).Requirement(item);}
  void ProducerQueue(){if(!city.Gatherer(producerSource).Settings.manualBatches)return;if(city.Gatherer(producerSource).Queue(producerItem,ProducerQuantity,out producerFeedback))Save();}
  void ProducerRepeat(){if(producerSource!=0)return;if(city.AddRepeatStep(false,producerItem,ProducerQuantity,out producerFeedback)){producerPage=1;Save();}}
  void ProducerAssign(){if(producerSource==0||!ProducerValidAmount)return;producerFeedback=ProducerRequirement(producerItem);if(producerFeedback!="")return;if(city.Allocate(producerSource,producerItem,double.Parse(producerAmount))){producerFeedback="Production allocation saved.";Save();}else producerFeedback="Not enough free capacity. Reduce another allocation first.";}
  void ProducerAll(){if(producerSource==0||ProducerRequirement(producerItem)!="")return;CityAllocateAll(producerSource,producerItem);producerAmount=city.Allocation(producerSource)[producerItem].ToString("0.##");producerFeedback="All capacity assigned to "+Settlement.ResourceNames[producerSource][producerItem]+".";}
  void ProducerButton(Rect r,string title,Action action,bool enabled=true,bool active=false){ArchiveButton(r,title,action,enabled,active);}
  bool ReadProducerInput(){
   if(!ProducerScreenOpen)return false;if(resourceFocus||ActionPressed(ControlAction.Resources))return ReadResourceController();var key=Keyboard.current;var pad=PreferredPad;
   if(key?.escapeKey.wasPressedThisFrame==true||pad?.buttonEast.wasPressedThisFrame==true){if(resourceCategory>=0)LeaveResourceBar();else if(producerEditing)producerEditing=false;else CloseProducer();return true;}
   if(resourceCategory>=0)return true;
   if(producerEditing){if(key?.enterKey.wasPressedThisFrame==true){producerEditing=false;if(producerSource==0)ProducerQueue();else ProducerAssign();}return true;}
   int pages=city.Gatherer(producerSource).Settings.tabs.Length;int side=pad?.rightShoulder.wasPressedThisFrame==true?1:pad?.leftShoulder.wasPressedThisFrame==true?-1:0;if(side!=0){producerPage=(producerPage+pages+side)%pages;producerScroll=Vector2.zero;selection=0;return true;}
   MenuInput();return true;
  }
  void ProducerScreen(){DrawGatheringBuilding(city.Gatherer(producerSource));}
  void ProducerCatalog(){
   GUI.Label(new Rect(81,361,650,24),producerSource==0?"ORE CATALOG":producerSource==1?"HIDE CATALOG":"TEXTILE CATALOG",ArchiveType(15,archiveGold,true));GUI.Label(new Rect(125,389,440,20),"MATERIAL",ArchiveType(10,archiveMuted,true));GUI.Label(new Rect(704,389,160,20),"IN STOCK",ArchiveType(10,archiveMuted,true,TextAnchor.UpperRight));GUI.Label(new Rect(887,389,203,20),"CITY TIME / UNIT",ArchiveType(10,archiveMuted,true,TextAnchor.UpperRight));
   int count=Settlement.ResourceNames[producerSource].Length;float rowHeight=producerSource==0?30:46;
   for(int i=0;i<count;i++){int item=i;float y=410+i*rowHeight;Rect row=new Rect(78,y,1027,rowHeight-1);bool locked=ProducerRequirement(i)!="";int button=buttons.Count;buttons.Add(()=>ProducerSelect(item));if(producerItem==i)ArchivePlate(row,2,5);else if(i%2==1)Box(row,new Color(.08f,.075f,.064f,.3f));RegisterMenuFocus(row,button,"Material row",true);
    var icon=MaterialIcon(ResourceIconNames[producerSource][i]);if(icon){GUI.color=locked?new Color(.4f,.4f,.4f):Color.white;GUI.DrawTexture(new Rect(87,y+2,31,rowHeight-5),icon,ScaleMode.ScaleToFit);GUI.color=Color.white;}
    GUI.Label(new Rect(129,y+(rowHeight-20)/2,530,24),Settlement.ResourceNames[producerSource][i],ArchiveType(14,locked?archiveMuted:archiveIvory));if(locked)SmithLock(new Rect(648,y+(rowHeight-18)/2,14,18));GUI.Label(new Rect(698,y+(rowHeight-20)/2,166,23),Math.Floor(city.ResourceAmount(producerSource,i)).ToString("N0"),ArchiveType(13,archiveIvory,false,TextAnchor.UpperRight));
    double rate=producerSource==0?city.MiningSpeed/Settlement.MineUnitWork(i):city.ProductionRate(producerSource,i);GUI.Label(new Rect(881,y+(rowHeight-20)/2,207,23),locked?"Locked":ProductionTime.PerUnit(rate),ArchiveType(12,locked?archiveMuted:archiveIvory,false,TextAnchor.UpperRight));
    if(Event.current.type==EventType.MouseDown&&Event.current.button==0&&row.Contains(Event.current.mousePosition)){selection=button;ProducerSelect(i);GUI.FocusControl(null);Event.current.Use();}
   }
  }
  void ProducerDetails(){DrawGatheringDetails(city.Gatherer(producerSource),SharedWorkshopLayout.EquipmentDetails);}
  void ProducerAdjustAmount(int delta){double.TryParse(producerAmount,out double n);producerAmount=Math.Max(producerSource==0?1:0,Math.Min(producerSource==0?999:city.Capacity(producerSource),n+delta)).ToString(producerSource==0?"0":"0.##");producerEditing=false;}
 }
}
