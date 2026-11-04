using System;
using System.Collections.Generic;
namespace Veilbound {
 [Serializable] public sealed class RefiningSlot {
  public int assignedMetal=-1,metal=-1,mode,orderId;public double remainingWork;public ResourceCost[] ingredients;public bool paid;
 }
 public partial class Settlement {
  public List<RefiningSlot> barProductionSlots;
  [NonSerialized] double[] plannerBarDemand,plannerBarReadyUnits;
  int RecipeBudgetUnits(ResourceCost[] recipe,double[][] stock,int limit){int available=limit;foreach(var part in recipe)if(part.amount>0)available=Math.Min(available,(int)Math.Floor((stock[part.family][part.index]+1e-9)/part.amount));return Math.Max(0,available);}
  public int BarProductionSlotCount=>ProcessingLine(3).SlotCount;
  public List<RefiningSlot> RefiningSlots(){if(barProductionSlots==null)barProductionSlots=new List<RefiningSlot>();while(barProductionSlots.Count<4)barProductionSlots.Add(new RefiningSlot());for(int i=0;i<4;i++)if(barProductionSlots[i]==null)barProductionSlots[i]=new RefiningSlot();return barProductionSlots;}
  void ClearRefiningWork(RefiningSlot slot,bool refund){var line=ProcessingLine(3);line.Clear(line.Wrap(slot),refund);}
  public bool AssignBarProductionSlot(int index,int metal,out string message){return ProcessingLine(3).Assign(index,metal,out message);}
  int BarClaims(int id){int count=0;var slots=RefiningSlots();for(int i=1;i<BarProductionSlotCount;i++)if(slots[i].mode==2&&slots[i].orderId==id)count++;return count;}
  public bool PrimarySmithCanWork=>smithOrders.Count==0||smithOrders[0].quantity-smithOrders[0].completed>BarClaims(smithOrders[0].id);
  public int PrimaryAssignedBar=>RefiningSlots()[0].assignedMetal;
  public double PrimaryAssignedBarHours=>RefiningSlots()[0].paid?RefiningSlots()[0].remainingWork/RefiningSpeed:double.PositiveInfinity;
  bool StartRefiningSlot(RefiningSlot slot,int metal,int mode){var line=ProcessingLine(3);return line.TryStart(line.Wrap(slot),metal,mode);}
  public void PrepareBarProductionSlots(){
   var slots=RefiningSlots();int count=BarProductionSlotCount;var demand=new double[WeaponRules.Count];if(plannerBarDemand!=null)for(int d=0;d<demand.Length;d++)demand[d]=Math.Min(plannerBarDemand[d],plannerBarReadyUnits[d]);
   for(int i=0;i<count;i++){
    var slot=slots[i];if(slot.mode==2&&smithOrders.Find(o=>o.id==slot.orderId)==null)ClearRefiningWork(slot,false);
    if(slot.assignedMetal>=0&&(i>0||smithOrders.Count==0)&&StartRefiningSlot(slot,slot.assignedMetal,1))demand[slot.assignedMetal]=Math.Max(0,demand[slot.assignedMetal]-1);
   }
   for(int i=1;i<count;i++){
    var slot=slots[i];if(slot.assignedMetal>=0||slot.mode==2||slot.paid)continue;
    foreach(var order in smithOrders){int primary=smithOrders[0]==order?1:0;if(order.quantity-order.completed<=BarClaims(order.id)+primary)continue;
     slot.mode=2;slot.orderId=order.id;slot.metal=order.metal;slot.remainingWork=order.workPerBar;break;
    }
   }
   bool prior=dependencyBarOrder!=null;BeginDependencyBar();if(!prior&&dependencyBarOrder!=null)demand[dependencyBarOrder.metal]=Math.Max(0,demand[dependencyBarOrder.metal]-1);
   if(smithOrders.Count>0||plannerBars==null)return;
   for(int i=1;i<count;i++){
    var slot=slots[i];if(slot.assignedMetal>=0||slot.mode==2||slot.paid)continue;
    foreach(int metal in plannerBars)if(demand[metal]>1e-9&&HasRecipe(BarRecipe(metal))&&StartRefiningSlot(slot,metal,3)){demand[metal]=Math.Max(0,demand[metal]-1);break;}
   }
  }
  void AddIncomingRefiningSlots(double[][] stock){var slots=RefiningSlots();for(int i=0;i<BarProductionSlotCount;i++)if(slots[i].paid)stock[3][slots[i].metal]++;}
  bool RefundUnneededRefiningSlots(double[][] stock){bool changed=false;var slots=RefiningSlots();for(int i=1;i<BarProductionSlotCount;i++)if(slots[i].mode==3&&slots[i].paid&&stock[3][slots[i].metal]>=1-1e-9){stock[3][slots[i].metal]-=1;ClearRefiningWork(slots[i],true);changed=true;}return changed;}
  void RefundAutomaticRefiningSlots(){var slots=RefiningSlots();for(int i=1;i<4;i++)if(slots[i].mode==3)ClearRefiningWork(slots[i],true);}
  public double NextParallelBarHours {get{double next=double.PositiveInfinity;var slots=RefiningSlots();for(int i=1;i<BarProductionSlotCount;i++)if((slots[i].paid||slots[i].mode==2)&&!(smithOrders.Count>0&&slots[i].mode==3))next=Math.Min(next,slots[i].remainingWork/RefiningSpeed);return next;}}
  public void TickAssignedPrimaryBar(double work){ProcessingLine(3).Advance(ProcessingLine(3).Slot(0),work);}
  public void TickParallelBars(double hours,bool manualAtStart){var line=ProcessingLine(3);for(int i=1;i<line.SlotCount;i++){var slot=line.Slot(i);if(manualAtStart&&slot.Mode==3)continue;line.Advance(slot,hours*line.Speed);}}
 }
}
