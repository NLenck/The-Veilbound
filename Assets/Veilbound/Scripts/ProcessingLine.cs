using System;using System.Collections.Generic;
namespace Veilbound {
 // Both legacy slot shapes implement this live view. No timers or ingredients are copied.
 public abstract class ProcessingSlotAccess {
  public abstract object Storage {get;}public abstract int Assigned {get;set;}public abstract int Index {get;set;}public abstract int Mode {get;set;}public abstract int OrderId {get;set;}public abstract double Remaining {get;set;}public abstract bool Paid {get;set;}public abstract ResourceCost[] PaidRecipe {get;set;}
 }
 sealed class BarSlotAccess:ProcessingSlotAccess {
  readonly RefiningSlot slot;public BarSlotAccess(RefiningSlot slot){this.slot=slot;}public override object Storage=>slot;
  public override int Assigned{get=>slot.assignedMetal;set=>slot.assignedMetal=value;}public override int Index{get=>slot.metal;set=>slot.metal=value;}public override int Mode{get=>slot.mode;set=>slot.mode=value;}public override int OrderId{get=>slot.orderId;set=>slot.orderId=value;}public override double Remaining{get=>slot.remainingWork;set=>slot.remainingWork=value;}public override bool Paid{get=>slot.paid;set=>slot.paid=value;}public override ResourceCost[] PaidRecipe{get=>slot.ingredients;set=>slot.ingredients=value;}
 }
 sealed class MaterialSlotAccess:ProcessingSlotAccess {
  readonly MaterialProcessSlot slot;readonly Settlement city;readonly int source;public MaterialSlotAccess(Settlement city,int source,MaterialProcessSlot slot){this.city=city;this.source=source;this.slot=slot;}public override object Storage=>slot;
  public override int Assigned{get=>slot.assigned;set=>slot.assigned=value;}public override int Index{get=>slot.index;set=>slot.index=value;}public override int Mode{get=>slot.mode;set=>slot.mode=value;}public override int OrderId{get=>slot.orderId;set=>slot.orderId=value;}public override double Remaining{get=>slot.remaining;set=>slot.remaining=value;}public override bool Paid{get=>slot.paid;set=>slot.paid=value;}public override ResourceCost[] PaidRecipe{get=>slot.index<0?null:city.MaterialProcessRecipe(source,slot.index);set{}}
 }
 public sealed class ProcessingOrderAccess {
  readonly SmithWorkOrder bar;readonly MaterialProcessOrder material;readonly ProcessingLine line;
  public ProcessingOrderAccess(ProcessingLine line,SmithWorkOrder order){this.line=line;bar=order;}public ProcessingOrderAccess(ProcessingLine line,MaterialProcessOrder order){this.line=line;material=order;}
  public int Id=>bar!=null?bar.id:material.id;public int Index=>bar!=null?bar.metal:material.index;public int Quantity=>bar!=null?bar.quantity:material.quantity;public int Completed{get=>bar!=null?bar.completed:material.completed;set{if(bar!=null)bar.completed=value;else material.completed=value;}}
  public ResourceCost[] Recipe=>bar!=null?bar.ingredients:line.Recipe(Index);
 }
 public struct ProcessingSlotStatus {public int index;public string mode;public double hours;public float progress;}
 public sealed class ProcessingLine {
  readonly Settlement city;readonly ProcessingSlotAccess[] slots=new ProcessingSlotAccess[4];public readonly int Family;public int Source=>Family-4;public bool IsBars=>Family==3;public bool SupportsRepeat=>IsBars;public int RecipeCount=>IsBars?WeaponRules.Count:Settlement.ProcessedNames[Source].Length;
  public CraftingBuilding Building=>city.Crafter(IsBars?"blacksmith":"clothier");public double Speed=>Building.Speed;public string ResearchBranch=>IsBars?"blacksmith-slots":Source==0?"leather-slots":"cloth-slots";
  public int SlotCount=>ProductionBuilding.UnlockedSlots(Building.Level,city.ResearchRank(ResearchBranch));
  public ProcessingLine(Settlement city,int family){this.city=city;Family=family;}
  public string Name(int index)=>IsBars?WeaponRules.BarName(index):Settlement.ProcessedNames[Source][index];
  public double Work(int index)=>IsBars?SmithRefining.WorkHours[index]:Settlement.ProcessUnitWork[index];
  public ResourceCost[] Recipe(int index)=>IsBars?city.BarRecipe(index):city.MaterialProcessRecipe(Source,index);
  public string Requirement(int index){if(index<0||index>=RecipeCount)return "Invalid processing material.";if(IsBars)return city.SmithRequirement(index,"blacksmith");int rank=index+1;return city.clothier<rank||city.ResearchRank("clothier")<rank?"Requires Clothier building and research tier "+rank+".":"";}
  public ProcessingSlotAccess Slot(int index){object storage=IsBars?(object)city.RefiningSlots()[index]:city.ProcessingSlots(Source)[index];if(slots[index]==null||slots[index].Storage!=storage)slots[index]=IsBars?(ProcessingSlotAccess)new BarSlotAccess((RefiningSlot)storage):new MaterialSlotAccess(city,Source,(MaterialProcessSlot)storage);return slots[index];}
  public ProcessingSlotAccess Wrap(object storage){for(int i=0;i<4;i++)if(Slot(i).Storage==storage)return Slot(i);throw new ArgumentException("Slot is not owned by this line.");}
  public List<ProcessingOrderAccess> Orders(){var result=new List<ProcessingOrderAccess>();if(IsBars){city.EnsureSmithOrders();foreach(var order in city.smithOrders)result.Add(new ProcessingOrderAccess(this,order));}else foreach(var order in city.ProcessingOrders(Source))result.Add(new ProcessingOrderAccess(this,order));return result;}
  ProcessingOrderAccess FindOrder(int id){if(IsBars){var order=city.smithOrders.Find(o=>o.id==id);return order==null?null:new ProcessingOrderAccess(this,order);}var material=city.ProcessingOrders(Source).Find(o=>o.id==id);return material==null?null:new ProcessingOrderAccess(this,material);}
  void RemoveOrder(int id){if(IsBars)city.smithOrders.RemoveAll(o=>o.id==id);else city.ProcessingOrders(Source).RemoveAll(o=>o.id==id);}
  public void Clear(ProcessingSlotAccess slot,bool refund){if(refund&&slot.Paid){var ingredients=slot.PaidRecipe;if(ingredients!=null)foreach(var part in ingredients)city.AddResource(part.family,part.index,part.amount);}slot.Paid=false;slot.PaidRecipe=null;slot.Mode=slot.OrderId=0;slot.Index=-1;slot.Remaining=0;}
  public bool TryStart(ProcessingSlotAccess slot,int index,int mode){if(slot.Paid||slot.Mode==2||Requirement(index)!="")return false;var recipe=Recipe(index);if(!city.PayRecipe(recipe,1,out _))return false;slot.Index=index;slot.Mode=mode;slot.Remaining=Work(index);slot.PaidRecipe=recipe;slot.Paid=true;return true;}
  public void Advance(ProcessingSlotAccess slot,double work){if(!slot.Paid&&slot.Mode!=2)return;slot.Remaining=Math.Max(0,slot.Remaining-work);if(slot.Remaining>1e-9)return;if(slot.Mode==2){var order=FindOrder(slot.OrderId);if(order!=null){city.AddResource(Family,slot.Index,1);order.Completed++;if(order.Completed>=order.Quantity)RemoveOrder(order.Id);}}else city.AddResource(Family,slot.Index,1);Clear(slot,false);}
  public bool Assign(int index,int recipe,out string message){city.EnsureGear();city.EnsureIndustry();if(index<0||index>=SlotCount){message="Research this processing slot first.";return false;}if(recipe>=0){message=Requirement(recipe);if(message!="")return false;}var slot=Slot(index);Clear(slot,true);slot.Assigned=recipe;if(IsBars&&index==0&&recipe<0)city.barScheduleManualPriority=false;message=recipe<0?"Slot follows city priorities.":"Slot manually produces "+Name(recipe)+".";return true;}
  public bool Queue(int index,int quantity,out string message){city.EnsureGear();city.EnsureIndustry();if(IsBars)city.EnsureSmithOrders();message=Requirement(index);if(message!="")return false;if(quantity<1||quantity>999||Orders().Count>=5){message="Choose 1–999 units; at most five manual processing orders.";return false;}var ingredients=Recipe(index);if(!city.PayRecipe(ingredients,quantity,out message))return false;if(IsBars){double work=Work(index);city.smithOrders.Add(new SmithWorkOrder{id=city.nextSmithOrderId++,metal=index,quantity=quantity,workPerBar=work,remainingWork=work,ingredients=ingredients});}else city.ProcessingOrders(Source).Add(new MaterialProcessOrder{id=city.nextMaterialProcessId++,index=index,quantity=quantity});message="Queued "+quantity+" "+Name(index)+". Inputs reserved.";return true;}
  public bool Cancel(int id){var order=FindOrder(id);if(order==null)return false;foreach(var part in order.Recipe)city.AddResource(part.family,part.index,part.amount*Math.Max(0,order.Quantity-order.Completed));RemoveOrder(id);for(int i=0;i<SlotCount;i++){var slot=Slot(i);if(slot.Mode==2&&slot.OrderId==id)Clear(slot,false);}return true;}
  public ProcessingSlotStatus Status(int index){var slot=Slot(index);int item=slot.Index>=0?slot.Index:slot.Assigned;double remaining=slot.Remaining;string mode=slot.Mode==2?"Manual batch":slot.Assigned>=0?slot.Paid?"Manual assignment":"Waiting for inputs":slot.Mode==3?"Automatic city demand":"Automatic / idle";
   if(IsBars&&index==0){item=city.ActiveBarMetal;remaining=city.smithOrders.Count>0?city.smithOrders[0].remainingWork:city.PrimaryAssignedBar>=0?slot.Remaining:city.ManualBarScheduleReady?city.AutoBarWork:city.dependencyBarOrder!=null?city.dependencyBarOrder.remainingWork:city.AutoBarsReady?city.AutoBarWork:0;mode=city.smithOrders.Count>0?"Manual batch":city.PrimaryAssignedBar>=0?slot.Paid?"Manual assignment":"Waiting for inputs":city.ManualBarScheduleReady?"Manual repeat":city.dependencyBarOrder!=null?"Automatic city demand":"Fallback / idle";}
   else if(IsBars&&city.smithOrders.Count>0&&slot.Mode==3)mode="Paused for manual batches";
   return new ProcessingSlotStatus{index=item,mode=mode,hours=remaining>0?remaining/Speed:0,progress=item>=0&&remaining>0?(float)Math.Max(0,Math.Min(1,1-remaining/Work(item))):0};
  }
 }
}
