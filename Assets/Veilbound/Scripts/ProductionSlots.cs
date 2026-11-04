using System;
using System.Collections.Generic;
namespace Veilbound {
 [Serializable] public sealed class ProducerSlot {
  public int resource=-1,activeResource=-1,mode,orderId;public double remainingWork;
 }
 public partial class Settlement {
  public List<ProducerSlot> mineProductionSlots,hunterProductionSlots,textileProductionSlots;
  public double totalMinedUnits;
  public static string SlotResearchBranch(int family)=>family==0?"mine-slots":family==1?"hunters-slots":family==2?"weaver-slots":family==3?"blacksmith-slots":family==4?"leather-slots":"cloth-slots";
  public static int SlotResearchSource(string id)=>id=="mine-slots"?0:id=="hunters-slots"?1:id=="weaver-slots"?2:id=="blacksmith-slots"?3:id=="leather-slots"?4:id=="cloth-slots"?5:-1;
  public int ProductionSlotCount(int source)=>Gatherer(source).SlotCount;
  public List<ProducerSlot> ProducerSlots(int source){
   if(mineProductionSlots==null)mineProductionSlots=new List<ProducerSlot>();if(hunterProductionSlots==null)hunterProductionSlots=new List<ProducerSlot>();if(textileProductionSlots==null)textileProductionSlots=new List<ProducerSlot>();
   var list=source==0?mineProductionSlots:source==1?hunterProductionSlots:textileProductionSlots;
   while(list.Count<4)list.Add(new ProducerSlot());for(int i=0;i<4;i++)if(list[i]==null)list[i]=new ProducerSlot();return list;
  }
  public string SlotMaterialRequirement(int source,int resource){if(source<0||source>2){return "Invalid material.";}return Gatherer(source).Requirement(resource);}
  public bool AssignProductionSlot(int source,int slot,int resource,out string message){if(source<0||source>2){message="Invalid producer.";return false;}return Gatherer(source).Assign(slot,resource,out message);}
  int ExtraClaimsForOrder(int id){int count=0;var slots=ProducerSlots(0);for(int i=1;i<ProductionSlotCount(0);i++)if(slots[i].mode==2&&slots[i].orderId==id)count++;return count;}
  public bool PrimaryManualMineCanWork=>mineOrders.Count==0||mineOrders[0].quantity-mineOrders[0].completed>ExtraClaimsForOrder(mineOrders[0].id);
  public int PrimaryAssignedMineResource=>ProducerSlots(0)[0].resource;
  public double PrimaryAssignedMineWork=>ProducerSlots(0)[0].remainingWork>1e-9?ProducerSlots(0)[0].remainingWork:MineUnitWork(PrimaryAssignedMineResource);
  void SetSlotWork(ProducerSlot slot,int mode,int resource,int orderId=0){
   if(slot.mode!=mode||slot.activeResource!=resource||slot.orderId!=orderId||slot.remainingWork<=1e-9){slot.remainingWork=resource<0?0:MineUnitWork(resource);}
   slot.mode=mode;slot.activeResource=resource;slot.orderId=orderId;
  }
  public void PrepareProductionSlots(){
   var mineSlots=ProducerSlots(0);int count=ProductionSlotCount(0);var needed=plannerDemand==null?new double[8]:(double[])plannerDemand[0].Clone();
   if(mineOrders.Count==0&&!ManualMineScheduleReady&&PrimaryAssignedMineResource<0&&dependencyMineResource>=0)needed[dependencyMineResource]=Math.Max(0,needed[dependencyMineResource]-1);
   // Existing in-flight manual claims stay attached to their order until delivery.
   for(int i=1;i<count;i++){var slot=mineSlots[i];if(slot.resource>=0)SetSlotWork(slot,1,slot.resource);else if(slot.mode==2&&mineOrders.Find(o=>o.id==slot.orderId)==null)SetSlotWork(slot,0,-1);}
   for(int i=1;i<count;i++){
    var slot=mineSlots[i];if(slot.resource>=0||slot.mode==2)continue;
    MineWorkOrder order=null;foreach(var candidate in mineOrders){int primary=mineOrders.Count>0&&mineOrders[0]==candidate?1:0;if(candidate.quantity-candidate.completed>ExtraClaimsForOrder(candidate.id)+primary){order=candidate;break;}}
    if(order!=null){SetSlotWork(slot,2,order.resource,order.id);continue;}
    if(mineOrders.Count>0){SetSlotWork(slot,0,-1);continue;}
    int resource=-1;if(slot.mode==3&&slot.activeResource>=0&&needed[slot.activeResource]>1e-9)resource=slot.activeResource;
    if(resource<0){int first=PlannedGatherResource(0);if(first>=0&&needed[first]>1e-9)resource=first;else for(int r=0;r<needed.Length;r++)if(needed[r]>1e-9&&MiningRequirement(r)==""){resource=r;break;}}
    if(resource>=0){SetSlotWork(slot,3,resource);needed[resource]=Math.Max(0,needed[resource]-1);}else SetSlotWork(slot,0,-1);
   }
   for(int f=1;f<3;f++)for(int i=1;i<ProductionSlotCount(f);i++){var slot=ProducerSlots(f)[i];slot.activeResource=slot.resource>=0?slot.resource:PlannedGatherResource(f);slot.mode=slot.resource>=0?1:slot.activeResource>=0?3:0;}
  }
  public double NextExtraMineHours {get{double next=double.PositiveInfinity;if(MiningSpeed<=0)return next;var slots=ProducerSlots(0);for(int i=1;i<ProductionSlotCount(0);i++)if(slots[i].mode>0&&slots[i].activeResource>=0)next=Math.Min(next,slots[i].remainingWork/MiningSpeed);return next;}}
  public void TickPrimaryAssignedMine(double work){var slot=ProducerSlots(0)[0];while(work>1e-9&&slot.resource>=0&&MiningRequirement(slot.resource)==""){SetSlotWork(slot,1,slot.resource);double spend=Math.Min(work,slot.remainingWork);slot.remainingWork=Math.Max(0,slot.remainingWork-spend);work-=spend;if(slot.remainingWork>1e-9)break;AddResource(0,slot.resource,1);totalMinedUnits++;}}
  void TickExtraMine(double h){
   var slots=ProducerSlots(0);for(int i=1;i<ProductionSlotCount(0);i++){var slot=slots[i];if(slot.mode==0||slot.activeResource<0)continue;
    slot.remainingWork=Math.Max(0,slot.remainingWork-h*MiningSpeed);if(slot.remainingWork>1e-9)continue;
    if(slot.mode==2){var order=mineOrders.Find(o=>o.id==slot.orderId);if(order!=null){AddResource(0,slot.activeResource,1);totalMinedUnits++;order.completed++;if(order.completed>=order.quantity)mineOrders.Remove(order);}}
    else {AddResource(0,slot.activeResource,1);totalMinedUnits++;}
    slot.mode=slot.orderId=0;slot.activeResource=-1;
   }
  }
  public double ManualGatherRate(int source,int resource){
   double capacity=Allocation(source)[resource];var slots=ProducerSlots(source);for(int i=1;i<ProductionSlotCount(source);i++)if(slots[i].resource==resource)capacity+=Capacity(source);
   return capacity*ResourceYields[source][resource]*PassiveProductionMultiplier*Productivity/CityHoursPerDay;
  }
  public double AutomaticGatherCapacity(int source){double capacity=Math.Max(0,Capacity(source)-UsedCapacity(source));var slots=ProducerSlots(source);for(int i=1;i<ProductionSlotCount(source);i++)if(slots[i].resource<0)capacity+=Capacity(source);return capacity;}
  public double ExtraAutomaticGatherRate(int source,int resource){double capacity=0;var slots=ProducerSlots(source);for(int i=1;i<ProductionSlotCount(source);i++)if(slots[i].resource<0&&slots[i].activeResource==resource&&slots[i].mode==3)capacity+=Capacity(source);return capacity*ResourceYields[source][resource]*PassiveProductionMultiplier*Productivity/CityHoursPerDay;}
  public double ParallelMineRate(int resource){
   double rate=ActiveMineResource==resource&&(mineOrders.Count==0||PrimaryManualMineCanWork)?MiningSpeed/MineUnitWork(resource):0;var slots=ProducerSlots(0);for(int i=1;i<ProductionSlotCount(0);i++)if(slots[i].mode>0&&slots[i].activeResource==resource)rate+=MiningSpeed/MineUnitWork(resource);return rate;
  }
  public void TickExtraProduction(double hours){TickExtraMine(hours);for(int f=1;f<3;f++){var slots=ProducerSlots(f);for(int i=1;i<ProductionSlotCount(f);i++)if(slots[i].resource>=0&&SlotMaterialRequirement(f,slots[i].resource)=="")AddResource(f,slots[i].resource,hours*Capacity(f)*ResourceYields[f][slots[i].resource]*PassiveProductionMultiplier*Productivity/CityHoursPerDay);}}
 }
}
