using System;
using System.Collections.Generic;
namespace Veilbound {
 [Serializable] public sealed class MineWorkOrder {
  public int id,resource,quantity,completed;public double workPerUnit,remainingWork;
 }
 public partial class Settlement {
  public const int MineQueueLimit=5,MineQuantityLimit=999;
  public int mineOrderVersion,nextMineOrderId=1;
  public List<MineWorkOrder> mineOrders=new List<MineWorkOrder>();
  // Preserve the fully assigned level-one economy; upgrades shorten these unit durations.
  public static double MineUnitWork(int resource)=>CityHoursPerDay/(ProductionBuildingCatalog.Gathering[0].baseCapacity*ResourceYields[0][resource]*PassiveProductionMultiplier);
  public double MiningSpeed=>Gatherer(0).Speed;
  public void EnsureMineOrders(){
   EnsureIndustry();EnsureProductionSchedules();if(mineOrders==null)mineOrders=new List<MineWorkOrder>();
   if(mineOrderVersion==0){
    // Convert old assignments once, retaining existing stock and fractional stock.
    for(int i=0;i<mineEffort.Length;i++)if(mineEffort[i]>0&&mine>=ResourceUnlocks[0][i]){
     int quantity=Math.Min(MineQuantityLimit,(int)Math.Ceiling(mineEffort[i]));double work=MineUnitWork(i);
     mineOrders.Add(new MineWorkOrder{id=nextMineOrderId++,resource=i,quantity=quantity,workPerUnit=work,remainingWork=work});
    }
    mineOrderVersion=1;
   }
   foreach(var o in mineOrders)nextMineOrderId=Math.Max(nextMineOrderId,o.id+1);
  }
  public string MiningRequirement(int resource){
   if(resource<0||resource>=ResourceNames[0].Length)return "Invalid material.";
   int rank=ResourceUnlocks[0][resource];if(mine<rank)return "Requires Mine level "+rank+".";
   return ResearchRank("mine")<rank?"Research Mining tier "+rank+" at the Library.":"";
  }
  public bool QueueMining(int resource,int quantity,out string message){
   EnsureMineOrders();message=MiningRequirement(resource);if(message!="")return false;
   if(quantity<1||quantity>MineQuantityLimit){message="Choose 1–999 units.";return false;}
   if(mineOrders.Count>=MineQueueLimit){message="Mining work-order queue is full.";return false;}
   double work=MineUnitWork(resource);mineOrders.Add(new MineWorkOrder{id=nextMineOrderId++,resource=resource,quantity=quantity,workPerUnit=work,remainingWork=work});
   message="Queued "+quantity+" "+ResourceNames[0][resource]+". Ore arrives one unit at a time.";return true;
  }
  public bool CancelMining(int id){EnsureMineOrders();int at=mineOrders.FindIndex(o=>o.id==id);if(at<0)return false;mineOrders.RemoveAt(at);return true;}
  public double MineOrderHours(MineWorkOrder o)=>MiningSpeed<=0?double.PositiveInfinity:(o.remainingWork+Math.Max(0,o.quantity-o.completed-1)*o.workPerUnit)/MiningSpeed;
  public double MineQueueHours(){EnsureMineOrders();double h=0;foreach(var o in mineOrders)h+=MineOrderHours(o);return h;}
  public double ProjectedMineUnits(double hours){
   EnsureMineOrders();if(ProductionSlotCount(0)>1){var copy=UnityEngine.JsonUtility.FromJson<Settlement>(UnityEngine.JsonUtility.ToJson(this));double before=copy.totalMinedUnits;copy.Tick(Math.Max(0,hours)*60,1);return copy.totalMinedUnits-before;}double work=Math.Max(0,hours)*MiningSpeed,units=0;
   foreach(var o in mineOrders){if(work+1e-9<o.remainingWork)return units;int n=Math.Min(o.quantity-o.completed,1+(int)Math.Floor(Math.Max(0,work-o.remainingWork+1e-9)/o.workPerUnit));units+=n;work-=o.remainingWork+(n-1)*o.workPerUnit;if(n<o.quantity-o.completed)return units;}
   units+=ProjectedAutoMineUnits(work);
   return units;
  }
  public void TickMineOrders(double hours){
   EnsureMineOrders();double work=Math.Max(0,hours)*MiningSpeed;
   while(mineOrders.Count>0&&work>1e-9&&PrimaryManualMineCanWork){var o=mineOrders[0];double spend=Math.Min(work,o.remainingWork);o.remainingWork=Math.Max(0,o.remainingWork-spend);work-=spend;
    if(o.remainingWork>1e-9)break;AddResource(0,o.resource,1);totalMinedUnits++;o.completed++;if(o.completed>=o.quantity)mineOrders.RemoveAt(0);else o.remainingWork=o.workPerUnit;
   }
   if(mineOrders.Count==0){if(PrimaryAssignedMineResource>=0)TickPrimaryAssignedMine(work);else if(ManualMineScheduleReady)TickAutoMining(work);else if(dependencyMineResource>=0)TickDependencyMining(work);else TickAutoMining(work);}
  }
 }
 public static class ProductionTime {
  public static string Duration(double hours){if(double.IsNaN(hours)||double.IsInfinity(hours))return "Paused";int minutes=Math.Max(1,(int)Math.Ceiling(Math.Max(0,hours)*60-1e-7));return minutes<60?minutes+"m":minutes%60==0?(minutes/60)+"h":(minutes/60)+"h "+(minutes%60)+"m";}
  public static string PerUnit(double hourlyRate)=>hourlyRate>0?"1 / "+Duration(1/hourlyRate):"Idle";
  public static double NextUnit(double stock,double hourlyRate)=>hourlyRate>0?(1-(stock-Math.Floor(stock+1e-9)))/hourlyRate:double.PositiveInfinity;
 }
}
