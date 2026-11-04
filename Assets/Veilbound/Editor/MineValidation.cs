using System;
using UnityEngine;
namespace Veilbound.Editor {
 public static class MineValidation {
  static void Check(bool ok,string message){if(!ok)throw new Exception("Mine orders: "+message);}
  static Settlement Sample(int level=1){var c=Settlement.NewCamp();c.mine=level;c.EnsureGear();c.EnsureResearch();c.EnsureMineOrders();c.mineOrders.Clear();return c;}
  public static void Run(){
   var c=Sample();Check(c.QueueMining(0,2,out _)&&c.QueueMining(1,1,out _),"queue two ore types");double tin=c.ResourceAmount(0,0),copper=c.ResourceAmount(0,1);
   c.Tick(179,1);Check(c.ResourceAmount(0,0)==tin,"no fractional ore delivery before one unit finishes");
   c=JsonUtility.FromJson<Settlement>(JsonUtility.ToJson(c));c.Tick(1,1);Check(c.ResourceAmount(0,0)==tin+1&&c.mineOrders[0].completed==1,"partial progress survives save/reload");
   Check(c.ProjectedMineUnits(6)==2,"projection spans remaining orders");c.Tick(360,1);Check(c.mineOrders.Count==0&&c.ResourceAmount(0,0)==tin+2&&c.ResourceAmount(0,1)==copper+1,"large tick carries surplus work across orders");
   c.Tick(1440,1);Check(c.ResourceAmount(0,0)==tin+2,"finished queue never mines ore passively");
   c=Sample();Check(!c.QueueMining(2,1,out _)&&!c.QueueMining(0,0,out _)&&!c.QueueMining(-1,1,out _)&&!c.QueueMining(0,1000,out _),"locked/invalid orders rejected");
   c.mine=2;Check(!c.QueueMining(2,1,out _),"building level alone cannot bypass research");c.researchRanks[LibraryResearch.Index("mine")]=2;Check(c.QueueMining(2,1,out _),"researched and upgraded mine unlocks zinc");
   double unit=Settlement.MineUnitWork(2)/c.MiningSpeed;Check(Math.Abs(unit-4/1.2)<1e-8,"level two mines twenty percent faster");c.Tick(unit*30,2);Check(c.ResourceAmount(0,2)==1,"expedition clock applies to unit duration");
   c=Sample();for(int i=0;i<5;i++)Check(c.QueueMining(0,1,out _),"queue accepts five orders");Check(!c.QueueMining(0,1,out _),"queue limit enforced");int id=c.mineOrders[0].id;c.Tick(90,1);Check(c.CancelMining(id)&&!c.CancelMining(id)&&c.ResourceAmount(0,0)==0,"cancel partial unit exactly once");
   c=Sample();c.QueueMining(0,100,out _);var stepped=JsonUtility.FromJson<Settlement>(JsonUtility.ToJson(c));c.Tick(900,2);for(int i=0;i<18000;i++)stepped.Tick(.05,2);Check(c.ResourceAmount(0,0)==10&&stepped.ResourceAmount(0,0)==10&&Math.Abs(c.mineOrders[0].remainingWork-stepped.mineOrders[0].remainingWork)<1e-7,"small frames equal dilated large step");
   c=Sample();c.QueueMining(0,100,out _);c.activeConstruction="mine";c.constructionTarget=2;c.constructionTotal=c.constructionRemaining=3;c.Tick(360,1);Check(c.mine==2&&c.ResourceAmount(0,0)==2&&Math.Abs(c.mineOrders[0].remainingWork-2.4)<1e-8,"speed changes at upgrade completion boundary");
   var legacy=new Settlement{mine=1,industryVersion=1,mineEffort=new double[]{4,6,0,0,0,0,0,0},ores=new double[]{12.5,2.25,0,0,0,0,0,0}};legacy.EnsureMineOrders();Check(legacy.mineOrders.Count==2&&legacy.mineOrders[0].quantity==4&&legacy.ResourceAmount(0,0)==12.5,"legacy assignments become finite orders without losing fractional stock");legacy.EnsureMineOrders();Check(legacy.mineOrders.Count==2,"migration is idempotent");
   Check(ProductionTime.Duration(1.5)=="1h 30m"&&ProductionTime.PerUnit(2)=="1 / 30m","time-per-unit formatting");Debug.Log("VEILBOUND_MINE_LOGIC_PASS");
  }
 }
}
