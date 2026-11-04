using System;
using System.Collections.Generic;
namespace Veilbound {
 [Serializable] public sealed class ProductionStep {public int resource,quantity;}
 public partial class Settlement {
  public const int RepeatStepLimit=12;
  public bool autoMineEnabled,autoBarsEnabled;
  public bool mineScheduleManualPriority,barScheduleManualPriority;
  public bool ManualMineScheduleReady=>mineScheduleManualPriority&&AutoMineReady;
  public bool ManualBarScheduleReady=>barScheduleManualPriority&&AutoBarsReady;
  public List<ProductionStep> mineSchedule=new List<ProductionStep>(),barSchedule=new List<ProductionStep>();
  public int mineScheduleIndex,barScheduleIndex,mineStepCompleted,barStepCompleted;
  public double autoMineRemainingWork;
  public SmithWorkOrder autoBarOrder;
  public void EnsureProductionSchedules(){
   // JsonUtility may reconstruct a null inline class as an empty object.
   if(autoBarOrder!=null&&(autoBarOrder.quantity!=1||autoBarOrder.workPerBar<=0||autoBarOrder.ingredients==null||autoBarOrder.ingredients.Length==0))autoBarOrder=null;
   if(mineSchedule==null)mineSchedule=new List<ProductionStep>();if(barSchedule==null)barSchedule=new List<ProductionStep>();
   mineScheduleIndex=mineSchedule.Count==0?0:Math.Max(0,mineScheduleIndex)%mineSchedule.Count;
   barScheduleIndex=barSchedule.Count==0?0:Math.Max(0,barScheduleIndex)%barSchedule.Count;
  }
  public ProductionStep MineStep {get{EnsureProductionSchedules();return mineSchedule.Count>0?mineSchedule[mineScheduleIndex]:null;}}
  public ProductionStep BarStep {get{EnsureProductionSchedules();return barSchedule.Count>0?barSchedule[barScheduleIndex]:null;}}
  public int autoMineResource=>MineStep!=null?MineStep.resource:0;
  public int autoBarMetal=>BarStep!=null?BarStep.resource:0;
  public bool AutoMineReady=>autoMineEnabled&&MineStep!=null&&MiningRequirement(autoMineResource)=="";
  public bool AddRepeatStep(bool bars,int resource,int quantity,out string message){
   EnsureProductionSchedules();message=bars?SmithRequirement(resource,"blacksmith"):MiningRequirement(resource);if(message!="")return false;
   if(quantity<1||quantity>999){message="Choose 1–999 units per schedule step.";return false;}
   var list=bars?barSchedule:mineSchedule;if(list.Count>=RepeatStepLimit){message="Repeat schedule is full (12 steps).";return false;}
   list.Add(new ProductionStep{resource=resource,quantity=quantity});if(list.Count==1){if(bars)autoBarsEnabled=true;else autoMineEnabled=true;}
   message="Added "+quantity+" "+(bars?WeaponRules.BarName(resource):ResourceNames[0][resource])+" to the repeating schedule.";return true;
  }
  public void PauseRepeat(bool bars){if(bars)autoBarsEnabled=!autoBarsEnabled;else autoMineEnabled=!autoMineEnabled;}
  void RefundAutoBar(){if(autoBarOrder!=null){EnsureGear();EnsureIndustry();foreach(var p in autoBarOrder.ingredients)AddResource(p.family,p.index,p.amount);autoBarOrder=null;}}
  public bool RemoveRepeatStep(bool bars,int index){
   EnsureProductionSchedules();var list=bars?barSchedule:mineSchedule;if(index<0||index>=list.Count)return false;
   var active=bars?BarStep:MineStep;bool removingActive=list[index]==active;list.RemoveAt(index);
   if(removingActive){if(bars){RefundAutoBar();barStepCompleted=0;}else {autoMineRemainingWork=0;mineStepCompleted=0;}}
   int next=removingActive?(list.Count==0?0:index%list.Count):list.IndexOf(active);
   if(bars){barScheduleIndex=next;if(list.Count==0)autoBarsEnabled=false;}else {mineScheduleIndex=next;if(list.Count==0)autoMineEnabled=false;}return true;
  }
  public bool MoveRepeatStep(bool bars,int index,int delta){
   EnsureProductionSchedules();var list=bars?barSchedule:mineSchedule;int target=index+delta;if(index<0||index>=list.Count||target<0||target>=list.Count)return false;
   var active=bars?BarStep:MineStep;var entry=list[index];list.RemoveAt(index);list.Insert(target,entry);if(bars)barScheduleIndex=list.IndexOf(active);else mineScheduleIndex=list.IndexOf(active);return true;
  }
  public double AutoMineWork=>autoMineRemainingWork>1e-9?autoMineRemainingWork:MineUnitWork(autoMineResource);
  public int ActiveMineResource {get{EnsureMineOrders();return mineOrders.Count>0?mineOrders[0].resource:PrimaryAssignedMineResource>=0?PrimaryAssignedMineResource:ManualMineScheduleReady?autoMineResource:dependencyMineResource>=0?dependencyMineResource:AutoMineReady?autoMineResource:-1;}}
  public double NextMineHours=>Math.Min(PrimaryNextMineHours,NextExtraMineHours);
  public double PrimaryNextMineHours {get{EnsureMineOrders();return MiningSpeed<=0?double.PositiveInfinity:mineOrders.Count>0?(PrimaryManualMineCanWork?mineOrders[0].remainingWork/MiningSpeed:double.PositiveInfinity):PrimaryAssignedMineResource>=0?PrimaryAssignedMineWork/MiningSpeed:ManualMineScheduleReady?AutoMineWork/MiningSpeed:dependencyMineResource>=0?PlannedMineHours:AutoMineReady?AutoMineWork/MiningSpeed:double.PositiveInfinity;}}
  void TickAutoMining(double work){
   while(work>1e-9&&AutoMineReady){double remaining=AutoMineWork,spend=Math.Min(work,remaining);autoMineRemainingWork=Math.Max(0,remaining-spend);work-=spend;if(autoMineRemainingWork>1e-9)return;
    AddResource(0,autoMineResource,1);totalMinedUnits++;mineStepCompleted++;if(mineStepCompleted>=MineStep.quantity){mineStepCompleted=0;mineScheduleIndex=(mineScheduleIndex+1)%mineSchedule.Count;}autoMineRemainingWork=MineUnitWork(autoMineResource);
   }
  }
  public double ProjectedAutoMineUnits(double work){
   if(!AutoMineReady)return 0;int index=mineScheduleIndex,completed=mineStepCompleted;double remaining=AutoMineWork,units=0;
   while(work+1e-9>=remaining){var step=mineSchedule[index];if(MiningRequirement(step.resource)!="")break;work=Math.Max(0,work-remaining);units++;completed++;if(completed>=step.quantity){completed=0;index=(index+1)%mineSchedule.Count;}remaining=MineUnitWork(mineSchedule[index].resource);}
   return units;
  }
  public bool AutoBarsReady=>autoBarsEnabled&&BarStep!=null&&SmithRequirement(autoBarMetal,"blacksmith")==""&&(autoBarOrder!=null||HasRecipe(BarRecipe(autoBarMetal)));
  public double AutoBarWork=>autoBarOrder!=null?autoBarOrder.remainingWork:SmithRefining.WorkHours[autoBarMetal];
  public int ActiveBarMetal {get{EnsureSmithOrders();return smithOrders.Count>0?smithOrders[0].metal:PrimaryAssignedBar>=0?PrimaryAssignedBar:ManualBarScheduleReady?autoBarMetal:dependencyBarOrder!=null?dependencyBarOrder.metal:!HasDependencyBars&&AutoBarsReady?autoBarMetal:-1;}}
  public double NextBarHours=>Math.Min(PrimaryBarHours,NextParallelBarHours);
  public double PrimaryBarHours {get{EnsureSmithOrders();return smithOrders.Count>0?(PrimarySmithCanWork?smithOrders[0].remainingWork/RefiningSpeed:double.PositiveInfinity):PrimaryAssignedBar>=0?PrimaryAssignedBarHours:ManualBarScheduleReady?AutoBarWork/RefiningSpeed:dependencyBarOrder!=null?dependencyBarOrder.remainingWork/RefiningSpeed:!HasDependencyBars&&AutoBarsReady?AutoBarWork/RefiningSpeed:double.PositiveInfinity;}}
  void TickAutoBars(double work){
   while(work>1e-9&&AutoBarsReady){
    if(autoBarOrder==null){var recipe=BarRecipe(autoBarMetal);if(!PayRecipe(recipe,1,out _))return;double unit=SmithRefining.WorkHours[autoBarMetal];autoBarOrder=new SmithWorkOrder{metal=autoBarMetal,quantity=1,workPerBar=unit,remainingWork=unit,ingredients=recipe};}
    double spend=Math.Min(work,autoBarOrder.remainingWork);autoBarOrder.remainingWork=Math.Max(0,autoBarOrder.remainingWork-spend);work-=spend;
    if(autoBarOrder.remainingWork>1e-9)return;AddResource(3,autoBarOrder.metal,1);autoBarOrder=null;barStepCompleted++;if(barStepCompleted>=BarStep.quantity){barStepCompleted=0;barScheduleIndex=(barScheduleIndex+1)%barSchedule.Count;}
   }
  }
 }
}
