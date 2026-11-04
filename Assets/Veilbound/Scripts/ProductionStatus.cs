using System;using System.Collections.Generic;
namespace Veilbound {
 public struct ProductionActivity {
  public string tag,detail,item;public bool working;public double hours;public float progress;
  public ProductionActivity(string tag,string detail,string item="",bool working=false,double hours=0,float progress=0){this.tag=tag;this.detail=detail;this.item=item;this.working=working;this.hours=hours;this.progress=progress;}
 }
 public partial class Settlement {
  // Presentation only: these views never dispatch work, reserve inputs or refresh the plan.
  public ProductionActivity GoalActivity(SettlementJob job){
   string reason=SettlementJobStatus(job.id).Replace(" / checking lower priorities","");
   if(reason.StartsWith("Supplying "))return new ProductionActivity("GATHERING","Gathering "+reason.Substring(10));
   if(reason.Contains("Manual Blacksmith"))return new ProductionActivity("PAUSED","Manual Blacksmith production has priority");
   if(reason.Contains("manual allocations")||reason.Contains("manual assignments"))return new ProductionActivity("PAUSED",reason.Replace(" (manual allocations first)","").Replace(" (manual assignments first)",""));
   if(reason.Contains("Requires")||reason.Contains("Circular"))return new ProductionActivity("BLOCKED",reason);
   if(reason.StartsWith("Waiting for an earlier"))return new ProductionActivity("QUEUED",reason);
   if(reason.Contains("timber / stone"))return new ProductionActivity("GATHERING","Waiting for timber or stone production");
   if(reason=="Waiting for earlier reservations, incoming materials or an available workshop"){
    foreach(var part in JobCost(job).materials)if(part.family>=3&&ResourceAmount(part.family,part.index)+1e-9<part.amount)return new ProductionActivity("SUPPLYING","Waiting for "+ResourceName(part.family,part.index)+" to finish or become available");
    return new ProductionActivity("QUEUED","Waiting for reserved supplies or a free workshop");
   }
   return new ProductionActivity("WAITING",reason);
  }
  ProductionActivity NoProductionDemand(int family){
   foreach(var job in settlementQueue){int n=family==3?WeaponRules.Count:family>=4?5:ResourceNames[family].Length;for(int i=0;i<n;i++)if(PriorityResourceAmount(job,family,i)>0){var a=GoalActivity(job);if(a.tag=="QUEUED")return new ProductionActivity("WAITING",a.detail);if(a.tag=="GATHERING")return new ProductionActivity("WAITING","Waiting for gathered supplies");if(a.tag=="SUPPLYING")return new ProductionActivity("WAITING","Waiting for processing or reserved supplies");return a;}}
   return new ProductionActivity("AUTO",settlementQueue.Count==0?"Waiting for a queued task":"No supplies needed from this slot");
  }
  string MissingProductionInputs(ResourceCost[] recipe){foreach(var part in recipe){double missing=part.amount-ResourceAmount(part.family,part.index);if(missing>1e-9)return "Needs "+missing.ToString("0.##")+" "+ResourceName(part.family,part.index);}return "Waiting for a free production turn";}
  public ProductionActivity GatheringSlotActivity(int family,int index){
   var building=Gatherer(family);var status=building.Status(index);var slot=ProducerSlots(family)[index];int item=status.index>=0?status.index:slot.resource;
   bool manual=slot.resource>=0||family>0&&index==0&&UsedCapacity(family)>0;
   if(item>=0){string requirement=building.Requirement(item);if(requirement!="")return new ProductionActivity("BLOCKED",requirement,building.ResourceName(item));
    if(family==0&&index==0&&mineOrders.Count>0&&!PrimaryManualMineCanWork)return new ProductionActivity("BATCH","Other slots are finishing this batch",building.ResourceName(item));
    string tag=family==0&&(index==0&&mineOrders.Count>0||slot.mode==2)?"BATCH":manual?"MANUAL":family==0&&index==0&&(ManualMineScheduleReady||dependencyMineResource<0&&AutoMineReady)?"REPEAT":"AUTO";
    return new ProductionActivity(tag,(tag=="MANUAL"?"Stockpiling ":tag=="REPEAT"?"Repeating ":"Gathering ")+building.ResourceName(item),building.ResourceName(item),true,status.hours,status.progress);
   }
   var waiting=NoProductionDemand(family);if(family==0&&index==0&&settlementQueue.Count==0&&mineSchedule.Count>0&&!autoMineEnabled)waiting.detail="No queued task Â· repeat schedule paused";return waiting;
  }
  public ProductionActivity ProcessingSlotActivity(int family,int index){
   var line=ProcessingLine(family);var slot=line.Slot(index);var status=line.Status(index);int item=status.index>=0?status.index:slot.Assigned;
   if(line.IsBars&&smithOrders.Count>0&&slot.Mode==3)return new ProductionActivity("PAUSED","Manual bar batches have priority",item>=0?line.Name(item):"");
   bool primaryBatch=line.IsBars&&index==0&&smithOrders.Count>0;
   if(primaryBatch&&!PrimarySmithCanWork)return new ProductionActivity("BATCH","Other slots are finishing this batch",line.Name(smithOrders[0].metal));
   bool primaryWork=line.IsBars&&index==0&&(primaryBatch||ManualBarScheduleReady||dependencyBarOrder!=null||PrimaryAssignedBar<0&&!HasDependencyBars&&AutoBarsReady);
   if(slot.Paid||slot.Mode==2||primaryWork){string tag=primaryBatch||slot.Mode==2?"BATCH":slot.Assigned>=0?"MANUAL":line.IsBars&&index==0&&(ManualBarScheduleReady||dependencyBarOrder==null&&AutoBarsReady)?"REPEAT":"AUTO";return new ProductionActivity(tag,"Making "+line.Name(item),line.Name(item),true,status.hours,status.progress);}
   if(slot.Assigned>=0){string requirement=line.Requirement(slot.Assigned);return new ProductionActivity(requirement==""?"NEEDS INPUTS":"BLOCKED",requirement==""?MissingProductionInputs(line.Recipe(slot.Assigned)):requirement,line.Name(slot.Assigned));}
   if(line.IsBars&&index==0&&barSchedule.Count>0&&autoBarsEnabled&&!HasDependencyBars){int repeat=autoBarMetal;string requirement=line.Requirement(repeat);return new ProductionActivity(requirement==""?"NEEDS INPUTS":"BLOCKED",requirement==""?MissingProductionInputs(line.Recipe(repeat)):requirement,line.Name(repeat));}
   var waiting=NoProductionDemand(family);if(line.IsBars&&index==0&&settlementQueue.Count==0&&barSchedule.Count>0&&!autoBarsEnabled)waiting.detail="No queued task Â· repeat schedule paused";return waiting;
  }
  public ProductionActivity ProductionOverview(string id){
   var result=new ProductionActivity("AUTO","Waiting for a queued task");bool chosen=false;
   for(int family=0;family<6;family++){if((family<3?ProductionBuildingCatalog.Gathering[family].id:family==3?"blacksmith":"clothier")!=id)continue;int count=family<3?Gatherer(family).SlotCount:ProcessingLine(family).SlotCount;for(int i=0;i<count;i++){var a=family<3?GatheringSlotActivity(family,i):ProcessingSlotActivity(family,i);if(!chosen||a.working&&(!result.working||a.hours<result.hours)||!result.working&&result.tag=="AUTO"&&a.tag!="AUTO"){result=a;chosen=true;}}}return result;
  }
  public TownUpgradeCost GoalCostPreview(SettlementJob job)=>JobCost(job);
  public string GoalRequirementPreview(SettlementJob job)=>JobRequirement(job);
  public ResourceCost[] GoalIngredientsPreview(SettlementJob job){var cost=JobCost(job);int count=job.kind>=3&&job.kind<=5?job.quantity:1;var parts=new List<ResourceCost>();if(cost.wood>0)parts.Add(new ResourceCost(6,0,cost.wood));if(cost.stone>0)parts.Add(new ResourceCost(6,1,cost.stone));foreach(var part in cost.materials)parts.Add(new ResourceCost(part.family,part.index,part.amount*count));if(job.kind==5){double essence=LightGearEssence(job.slot,job.index,LightJobTier(job))*count;if(essence>0)parts.Add(new ResourceCost(7,0,essence));}return parts.ToArray();}
 }
}
