using System;
using System.Collections.Generic;
namespace Veilbound {
 [Serializable] public sealed class MaterialProcessOrder {public int id,index,quantity,completed;}
 [Serializable] public sealed class MaterialProcessSlot {public int assigned=-1,index=-1,mode,orderId;public double remaining;public bool paid;}
 public partial class Settlement {
  public double[] processedLeather,processedCloth;
  public List<MaterialProcessOrder> leatherProcessOrders,clothProcessOrders;
  public List<MaterialProcessSlot> leatherProcessSlots,clothProcessSlots;
  public int nextMaterialProcessId=1;
  [NonSerialized] double[][] plannerProcessDemand;
  [NonSerialized] double[][] plannerProcessReadyUnits;
  [NonSerialized] List<ResourceCost> plannerProcessPriority;
  public static readonly string[][] ProcessedNames={new[]{"Leather","Cured leather","Thick leather","Primal leather","Veil leather"},new[]{"Linen cloth","Wool cloth","Silk cloth","Ash-silk cloth","Veil cloth"}};
  public static readonly double[] ProcessUnitWork={1,1.25,1.5,2,2.5};
  public void EnsureProcessedStocks(){if(processedLeather==null||processedLeather.Length!=5)processedLeather=new double[5];if(processedCloth==null||processedCloth.Length!=5)processedCloth=new double[5];}
  public double[] ProcessedStock(int source){EnsureProcessedStocks();return source==0?processedLeather:processedCloth;}
  public List<MaterialProcessOrder> ProcessingOrders(int source){if(leatherProcessOrders==null)leatherProcessOrders=new List<MaterialProcessOrder>();if(clothProcessOrders==null)clothProcessOrders=new List<MaterialProcessOrder>();return source==0?leatherProcessOrders:clothProcessOrders;}
  public List<MaterialProcessSlot> ProcessingSlots(int source){if(leatherProcessSlots==null)leatherProcessSlots=new List<MaterialProcessSlot>();if(clothProcessSlots==null)clothProcessSlots=new List<MaterialProcessSlot>();var list=source==0?leatherProcessSlots:clothProcessSlots;while(list.Count<4)list.Add(new MaterialProcessSlot());for(int i=0;i<4;i++)if(list[i]==null)list[i]=new MaterialProcessSlot();return list;}
  public int MaterialProcessingSlotCount(int source)=>ProcessingLine(source+4).SlotCount;
  public double MaterialProcessingSpeed=>Crafter("clothier").Speed;
  public bool ProcessingCanSupply(int source,int index){for(int i=0;i<MaterialProcessingSlotCount(source);i++)if(ProcessingSlots(source)[i].assigned<0||ProcessingSlots(source)[i].assigned==index)return true;return false;}
  public ResourceCost[] MaterialProcessRecipe(int source,int index)=>new[]{new ResourceCost(source==0?1:2,index,1)};
  public string MaterialProcessingRequirement(int source,int index){if(source<0||source>1)return "Invalid processing recipe.";return ProcessingLine(source+4).Requirement(index);}
  void ClearMaterialWork(int source,MaterialProcessSlot slot,bool refund){var line=ProcessingLine(source+4);line.Clear(line.Wrap(slot),refund);}
  public bool AssignMaterialProcessingSlot(int source,int index,int resource,out string message){if(source<0||source>1){message="Invalid processing line.";return false;}return ProcessingLine(source+4).Assign(index,resource,out message);}
  public bool QueueMaterialProcessing(int source,int index,int quantity,out string message){if(source<0||source>1){message="Invalid processing line.";return false;}return ProcessingLine(source+4).Queue(index,quantity,out message);}
  public bool CancelMaterialProcessing(int source,int id){return ProcessingLine(source+4).Cancel(id);}
  int MaterialOrderClaims(int source,int id){int count=0;for(int i=0;i<MaterialProcessingSlotCount(source);i++){var slot=ProcessingSlots(source)[i];if(slot.mode==2&&slot.orderId==id)count++;}return count;}
  bool StartMaterialWork(int source,MaterialProcessSlot slot,int index,int mode){var line=ProcessingLine(source+4);return line.TryStart(line.Wrap(slot),index,mode);}
  public void PrepareMaterialProcessing(){
   for(int source=0;source<2;source++){
    var demand=new double[5];if(plannerProcessDemand!=null)for(int d=0;d<5;d++)demand[d]=Math.Min(plannerProcessDemand[source][d],plannerProcessReadyUnits[source][d]);var slots=ProcessingSlots(source);var orders=ProcessingOrders(source);
    for(int i=0;i<MaterialProcessingSlotCount(source);i++){var slot=slots[i];if(slot.mode==2&&orders.Find(o=>o.id==slot.orderId)==null)ClearMaterialWork(source,slot,false);if(slot.assigned>=0&&StartMaterialWork(source,slot,slot.assigned,1))demand[slot.assigned]=Math.Max(0,demand[slot.assigned]-1);}
    for(int i=0;i<MaterialProcessingSlotCount(source);i++){
     var slot=slots[i];if(slot.paid||slot.mode==2||slot.assigned>=0)continue;
     foreach(var order in orders)if(order.quantity-order.completed>MaterialOrderClaims(source,order.id)){slot.mode=2;slot.orderId=order.id;slot.index=order.index;slot.remaining=ProcessUnitWork[order.index];break;}
     if(slot.mode==2||orders.Count>0||plannerProcessPriority==null)continue;
     foreach(var item in plannerProcessPriority)if(item.family==source+4&&demand[item.index]>1e-9&&StartMaterialWork(source,slot,item.index,3)){demand[item.index]=Math.Max(0,demand[item.index]-1);break;}
    }
   }
  }
  void AddIncomingMaterialProcessing(double[][] stock){for(int source=0;source<2;source++){foreach(var o in ProcessingOrders(source))stock[source+4][o.index]+=o.quantity-o.completed;var slots=ProcessingSlots(source);for(int i=0;i<MaterialProcessingSlotCount(source);i++)if(slots[i].paid)stock[source+4][slots[i].index]++;}}
  bool RefundUnusedMaterialProcessing(double[][] stock){bool changed=false;for(int source=0;source<2;source++){var slots=ProcessingSlots(source);for(int i=0;i<MaterialProcessingSlotCount(source);i++){var slot=slots[i];if(slot.mode==3&&slot.paid&&stock[source+4][slot.index]>=1-1e-9){stock[source+4][slot.index]-=1;ClearMaterialWork(source,slot,true);changed=true;}}}return changed;}
  void RefundAutomaticMaterialProcessing(){for(int source=0;source<2;source++)foreach(var slot in ProcessingSlots(source))if(slot.mode==3)ClearMaterialWork(source,slot,true);}
  public double NextMaterialProcessingHours {get{double next=double.PositiveInfinity;for(int source=0;source<2;source++)for(int i=0;i<MaterialProcessingSlotCount(source);i++){var slot=ProcessingSlots(source)[i];if(slot.paid||slot.mode==2)next=Math.Min(next,slot.remaining/MaterialProcessingSpeed);}return next;}}
  public double NextManualMaterialInputHours {get{double next=double.PositiveInfinity;for(int source=0;source<2;source++)for(int i=0;i<MaterialProcessingSlotCount(source);i++){var slot=ProcessingSlots(source)[i];if(slot.assigned<0||slot.paid)continue;int family=source==0?1:2;double stock=ResourceAmount(family,slot.assigned),rate=ProductionRate(family,slot.assigned);if(stock<1-1e-9&&rate>0)next=Math.Min(next,(1-stock)/rate);}return next;}}
  public void TickMaterialProcessing(double h){for(int source=0;source<2;source++){var line=ProcessingLine(source+4);for(int i=0;i<line.SlotCount;i++)line.Advance(line.Slot(i),h*line.Speed);}}
  public double MaterialProcessingRate(int source,int index){int workers=0;for(int i=0;i<MaterialProcessingSlotCount(source);i++){var slot=ProcessingSlots(source)[i];if(slot.index==index&&(slot.paid||slot.mode==2))workers++;}return workers*MaterialProcessingSpeed/ProcessUnitWork[index];}
 }
}
