using System;using System.Collections.Generic;
namespace Veilbound {
 public enum WorkshopPageKind { Processing, Equipment, Slots }
 public enum EquipmentRecipeKind { Plate, Weapon, Cloth, Leather }
 public sealed class WorkshopPageDefinition {
  public readonly string title;public readonly WorkshopPageKind kind;public readonly int family;public readonly EquipmentRecipeKind equipment;
  public WorkshopPageDefinition(string title,WorkshopPageKind kind,int family=-1,EquipmentRecipeKind equipment=EquipmentRecipeKind.Plate){this.title=title;this.kind=kind;this.family=family;this.equipment=equipment;}
 }
 public abstract class ProductionBuildingDefinition {
  public readonly string id,title,subtitle,backdrop;public readonly int icon;
  protected ProductionBuildingDefinition(string id,string title,string subtitle,string backdrop,int icon){this.id=id;this.title=title;this.subtitle=subtitle;this.backdrop=backdrop;this.icon=icon;}
 }
 public sealed class GatheringBuildingDefinition:ProductionBuildingDefinition {
  public readonly int family;public readonly string emblem;public readonly bool manualBatches,repeats;public readonly double baseCapacity,capacityGrowth;public readonly string[] tabs;
  public GatheringBuildingDefinition(string id,string title,string subtitle,string backdrop,string emblem,int family,bool manualBatches=false,bool repeats=false,double baseCapacity=16,double capacityGrowth=1.2):base(id,title,subtitle,backdrop,-1){this.family=family;this.emblem=emblem;this.manualBatches=manualBatches;this.repeats=repeats;this.baseCapacity=baseCapacity;this.capacityGrowth=capacityGrowth;tabs=repeats?new[]{"Materials & production","Repeat schedule","Stockpile"}:new[]{"Materials & production","Stockpile"};}
 }
 public sealed class CraftingBuildingDefinition:ProductionBuildingDefinition {
  public readonly WorkshopPageDefinition[] pages;public readonly double speedPerLevel;
  public CraftingBuildingDefinition(string id,string title,string subtitle,string backdrop,int icon,WorkshopPageDefinition[] pages,double speedPerLevel=.1):base(id,title,subtitle,backdrop,icon){this.pages=pages;this.speedPerLevel=speedPerLevel;}
 }
 public static class ProductionBuildingCatalog {
  public static readonly GatheringBuildingDefinition[] Gathering={
   new GatheringBuildingDefinition("mine","THE MINE","SETTLEMENT GATHERING / THE DEEP SEAMS","ProducerUI/MineBackdrop","group-ore",0,true,true),
   new GatheringBuildingDefinition("hunters","HUNTER'S CABIN","SETTLEMENT GATHERING / THE WILDS","ProducerUI/HuntersCabinBackdrop","rawhide",1),
   new GatheringBuildingDefinition("weaver","TEXTILE TAVERN","SETTLEMENT GATHERING / THREAD & LOOM","ProducerUI/TextileTavernBackdrop","flax",2)
  };
  public static readonly CraftingBuildingDefinition Blacksmith=new CraftingBuildingDefinition("blacksmith","THE BLACKSMITH","SETTLEMENT CRAFTING / THE EMBER FORGE","BlacksmithUI/BlacksmithBackdrop",10,new[]{
   new WorkshopPageDefinition("Bars",WorkshopPageKind.Processing,3),new WorkshopPageDefinition("Armour",WorkshopPageKind.Equipment,equipment:EquipmentRecipeKind.Plate),new WorkshopPageDefinition("Weapons",WorkshopPageKind.Equipment,equipment:EquipmentRecipeKind.Weapon),new WorkshopPageDefinition("Refining slots",WorkshopPageKind.Slots,3)});
  public static readonly CraftingBuildingDefinition Clothier=new CraftingBuildingDefinition("clothier","THE CLOTHIER","SETTLEMENT CRAFTING / THREAD & HIDE","ProducerUI/TextileTavernBackdrop",2,new[]{
   new WorkshopPageDefinition("Cloth",WorkshopPageKind.Processing,5),new WorkshopPageDefinition("Leather",WorkshopPageKind.Processing,4),new WorkshopPageDefinition("Cloth Armour",WorkshopPageKind.Equipment,equipment:EquipmentRecipeKind.Cloth),new WorkshopPageDefinition("Leather Armour",WorkshopPageKind.Equipment,equipment:EquipmentRecipeKind.Leather)});
 }
 // Runtime facades own behavior; the old serialized fields remain a storage adapter.
 public abstract class ProductionBuilding {
  protected readonly Settlement city;public readonly ProductionBuildingDefinition Definition;
  protected ProductionBuilding(Settlement city,ProductionBuildingDefinition definition){this.city=city;Definition=definition;}
  public int Level=>city.BuildingLevel(Definition.id);
  public static int UnlockedSlots(int level,int research)=>level<=0?0:1+Math.Max(0,Math.Min(3,research));
 }
 public sealed class GatheringBuilding:ProductionBuilding {
  public readonly GatheringBuildingDefinition Settings;public int Family=>Settings.family;public int ResourceCount=>Settlement.ResourceNames[Family].Length;
  public GatheringBuilding(Settlement city,GatheringBuildingDefinition settings):base(city,settings){Settings=settings;}
  public double Capacity=>Level<=0?0:Settings.baseCapacity*Math.Pow(Settings.capacityGrowth,Math.Min(10,Level)-1);
  public double Speed=>Level<=0?0:Math.Pow(Settings.capacityGrowth,Math.Min(10,Level)-1)*city.Productivity;
  public int SlotCount=>UnlockedSlots(Level,city.ResearchRank(Settlement.SlotResearchBranch(Family)));
  public string ResourceName(int index)=>Settlement.ResourceNames[Family][index];
  public string Requirement(int index){if(index<0||index>=ResourceCount)return "Invalid material.";if(Family==0)return city.MiningRequirement(index);int rank=Settlement.ResourceUnlocks[Family][index];return Level<rank||city.ResearchRank(Definition.id)<rank?"Requires "+Definition.title+" building and research tier "+rank+".":"";}
  public double Rate(int index){city.EnsureIndustry();if(Family==0){city.EnsureMineOrders();return city.ParallelMineRate(index);}return city.ManualGatherRate(Family,index)+city.PlannedGatherRate(Family,index);}
  public bool Assign(int index,int resource,out string message){
   city.EnsureIndustry();if(index<0||index>=SlotCount){message="Research this production slot first.";return false;}if(resource>=0){message=Requirement(resource);if(message!="")return false;}
   var slot=city.ProducerSlots(Family)[index];slot.resource=resource;slot.activeResource=-1;slot.remainingWork=0;slot.mode=slot.orderId=0;
   if(Family==0&&index==0&&resource<0)city.mineScheduleManualPriority=false;
   if(index==0&&Family>0){Array.Clear(city.Allocation(Family),0,city.Allocation(Family).Length);if(resource>=0)city.Allocate(Family,resource,Capacity);}
   message=resource<0?"Slot follows city priorities.":"Slot manually produces "+ResourceName(resource)+".";return true;
  }
  public bool Queue(int resource,int quantity,out string message){if(!Settings.manualBatches){message="Use a manual slot assignment for this gatherer.";return false;}return city.QueueMining(resource,quantity,out message);}
  public ProcessingSlotStatus Status(int index){
   var slot=city.ProducerSlots(Family)[index];int item=slot.activeResource;string mode=slot.resource>=0?"Manual assignment":slot.mode==2?"Manual batch":slot.mode==3?"Automatic city demand":"Automatic / idle";double remaining=slot.remainingWork,rate=0;
   if(index==0&&Family==0){item=city.ActiveMineResource;remaining=city.mineOrders.Count>0?city.mineOrders[0].remainingWork:city.PrimaryAssignedMineResource>=0?city.PrimaryAssignedMineWork:city.ManualMineScheduleReady?city.AutoMineWork:city.dependencyMineResource>=0?city.dependencyMineWork:city.AutoMineWork;mode=city.mineOrders.Count>0?"Manual batch":city.PrimaryAssignedMineResource>=0?"Manual assignment":city.ManualMineScheduleReady?"Manual repeat":city.dependencyMineResource>=0?"Automatic city demand":city.AutoMineReady?"Fallback repeat":"Automatic / idle";}
   if(index==0&&Family>0){item=city.PlannedGatherResource(Family);if(city.UsedCapacity(Family)>0)for(int i=0;i<ResourceCount;i++)if(city.Allocation(Family)[i]>0){item=i;break;}mode=city.UsedCapacity(Family)>0?"Manual capacity allocation":"Automatic city demand";}
   if(item<0)return new ProcessingSlotStatus{index=-1,mode=mode};
   if(Family==0)return new ProcessingSlotStatus{index=item,mode=mode,hours=remaining/city.MiningSpeed,progress=(float)Math.Max(0,Math.Min(1,1-remaining/Settlement.MineUnitWork(item)))};
   rate=Rate(item);double stock=city.ResourceAmount(Family,item);return new ProcessingSlotStatus{index=item,mode=mode,hours=rate>0?ProductionTime.NextUnit(stock,rate):0,progress=(float)(stock-Math.Floor(stock))};
  }
 }
 public sealed class CraftingBuilding:ProductionBuilding {
  public readonly CraftingBuildingDefinition Settings;
  public CraftingBuilding(Settlement city,CraftingBuildingDefinition settings):base(city,settings){Settings=settings;}
  public double Speed=>1+Settings.speedPerLevel*Math.Max(0,Math.Min(10,Level)-1);
  public ProcessingLine Line(int family){if(family<3||family>5||family==3&&Definition.id!="blacksmith"||family>3&&Definition.id!="clothier")throw new ArgumentException("Processing line is not owned by this workshop.");return city.ProcessingLine(family);}
  public CraftingRecipeSet Equipment(WorkshopPageDefinition page){if(page.kind!=WorkshopPageKind.Equipment||Array.IndexOf(Settings.pages,page)<0)throw new ArgumentException("Equipment page is not owned by this workshop.");return city.EquipmentRecipes(page.equipment);}
 }
 public partial class Settlement {
  [NonSerialized] GatheringBuilding[] gatheringBuildings;[NonSerialized] CraftingBuilding blacksmithBuilding,clothierBuilding;[NonSerialized] ProcessingLine[] processingLines;
  public GatheringBuilding Gatherer(int family){if(family<0||family>2)throw new ArgumentOutOfRangeException(nameof(family));if(gatheringBuildings==null)gatheringBuildings=new GatheringBuilding[3];return gatheringBuildings[family]??(gatheringBuildings[family]=new GatheringBuilding(this,ProductionBuildingCatalog.Gathering[family]));}
  public CraftingBuilding Crafter(string id){if(id=="blacksmith")return blacksmithBuilding??(blacksmithBuilding=new CraftingBuilding(this,ProductionBuildingCatalog.Blacksmith));if(id=="clothier")return clothierBuilding??(clothierBuilding=new CraftingBuilding(this,ProductionBuildingCatalog.Clothier));throw new ArgumentException("Unknown crafting building.");}
  public ProcessingLine ProcessingLine(int family){if(family<3||family>5)throw new ArgumentOutOfRangeException(nameof(family));if(processingLines==null)processingLines=new ProcessingLine[3];return processingLines[family-3]??(processingLines[family-3]=new ProcessingLine(this,family));}
 }
}
