using System;
using System.Collections.Generic;
namespace Veilbound {
 [Serializable] public sealed class SmithWorkOrder {
  public int id,metal,quantity,completed;public double workPerBar,remainingWork;public ResourceCost[] ingredients;
 }
 public static class SmithRefining {
  public const int QueueLimit=5,QuantityLimit=999;
  public static readonly double[] WorkHours={1,1.25,1.5,1.75,2,2.5,3,3.5,4,5,5};
 }
 public partial class Settlement {
  public List<SmithWorkOrder> smithOrders=new List<SmithWorkOrder>();public int nextSmithOrderId=1;
  public void EnsureSmithOrders(){EnsureProductionSchedules();if(smithOrders==null)smithOrders=new List<SmithWorkOrder>();foreach(var o in smithOrders)if(o!=null)nextSmithOrderId=Math.Max(nextSmithOrderId,o.id+1);}
  public double RefiningSpeed=>Crafter("blacksmith").Speed;
  public string SmithRequirement(int metal,string branch){if(metal<0||metal>=WeaponRules.Count)return "Invalid material.";string research=CraftResearchRequirement(metal,branch);if(research!="")return research;int level=Metals.All[WeaponRules.Material(metal)].unlock;return blacksmith>=level?"":"Requires Blacksmith level "+level+".";}
  public bool HasRecipe(ResourceCost[] recipe,int quantity=1){EnsureGear();EnsureIndustry();if(quantity<1||quantity>SmithRefining.QuantityLimit)return false;foreach(var p in recipe)if(ResourceAmount(p.family,p.index)+1e-9<p.amount*quantity)return false;return true;}
  public bool CanQueueBars(int metal,int quantity){EnsureSmithOrders();return quantity>=1&&quantity<=SmithRefining.QuantityLimit&&SmithRequirement(metal,"blacksmith")==""&&smithOrders.Count<SmithRefining.QueueLimit&&HasRecipe(BarRecipe(metal),quantity);}
  public bool QueueBars(int metal,int quantity,out string message){return ProcessingLine(3).Queue(metal,quantity,out message);}
  public bool CancelSmithOrder(int id){EnsureSmithOrders();return ProcessingLine(3).Cancel(id);}
  public double OrderHours(SmithWorkOrder o)=>Math.Max(0,o.remainingWork+Math.Max(0,o.quantity-o.completed-1)*o.workPerBar)/RefiningSpeed;
  public double SmithQueueHours(){EnsureSmithOrders();double hours=0;foreach(var o in smithOrders)hours+=OrderHours(o);return hours;}
  // The city-clock boundary splitter invokes this before applying any construction upgrade.
  public void TickSmithOrders(double hours){
   EnsureSmithOrders();bool manualAtStart=smithOrders.Count>0;double work=Math.Max(0,hours)*RefiningSpeed;
   while(smithOrders.Count>0&&work>1e-9&&PrimarySmithCanWork){var o=smithOrders[0];double spend=Math.Min(work,o.remainingWork);o.remainingWork=Math.Max(0,o.remainingWork-spend);work-=spend;
    if(o.remainingWork>1e-9)break;AddResource(3,o.metal,1);o.completed++;if(o.completed>=o.quantity)smithOrders.RemoveAt(0);else o.remainingWork=o.workPerBar;
   }
   if(smithOrders.Count==0){if(PrimaryAssignedBar>=0)TickAssignedPrimaryBar(work);else if(ManualBarScheduleReady)TickAutoBars(work);else if(dependencyBarOrder!=null)TickDependencyBars(work);else if(!HasDependencyBars)TickAutoBars(work);}
   TickParallelBars(hours,manualAtStart);
  }
 }
}
