using System;
using UnityEngine;
namespace Veilbound {public partial class VeilGame {
 Settlement StockpileSample(){var c=QueueSample();c.clothier=c.huntersLodge=c.weaver=5;c.mine=10;c.blacksmith=9;c.researchRanks[LibraryResearch.Index("blacksmith")]=9;c.researchRanks[LibraryResearch.Index("plate")]=9;c.QueueSettlementJob(3,"",9,3,1,0,out _);return c;}
 void CheckManualStockpiles(){
  var c=StockpileSample();QueueCheck(c.AddRepeatStep(false,7,10,out _),"high-level Mine can manually schedule VeilCrystal before future smithing is unlocked");c.mineScheduleManualPriority=true;c.Tick(12*60,1);
  QueueCheck(c.ResourceAmount(0,7)>=2&&c.metalBars[9]==0&&!c.OwnsPlate(3,9)&&c.settlementQueue.Count==1,"manual VeilCrystal stockpiles while lower-tier Blacksmith gear waits");
  c=JsonUtility.FromJson<Settlement>(JsonUtility.ToJson(c));QueueCheck(c.mineScheduleManualPriority&&c.ResourceAmount(0,7)>=2,"manual stockpile mode and stored material survive save/load");
  c.blacksmith=10;c.researchRanks[LibraryResearch.Index("blacksmith")]=10;c.researchRanks[LibraryResearch.Index("plate")]=10;c.metalBars[7]=4;c.AddResource(0,4,8);c.autoMineEnabled=false;c.Tick(400*60,1);
  QueueCheck(c.OwnsPlate(3,9)&&c.settlementQueue.Count==0&&c.ResourceAmount(0,7)<1e-7,"unlocked gear reuses stockpiled VeilCrystal and automatically gathers only the remainder");
  c=QueueSample();c.mine=10;c.AddRepeatStep(false,7,10,out _);c.mineScheduleManualPriority=true;c.QueueSettlementJob(3,"",0,3,1,0,out _);c.Tick(8*60,1);
  QueueCheck(c.ActiveMineResource==7&&c.ResourceAmount(0,7)>0&&c.ResourceAmount(0,0)==0&&!c.OwnsPlate(3,0),"manual repeating mining overrides conflicting automatic demand");
  double veil=c.ResourceAmount(0,7);c.autoMineEnabled=false;c.Tick(30*60,1);QueueCheck(c.OwnsPlate(3,0)&&c.ResourceAmount(0,7)==veil,"pausing the manual stockpile returns the Mine to automatic city demand");
  c=QueueSample();c.AddResource(0,0,4);c.AddResource(0,1,20);c.AddRepeatStep(true,1,1,out _);c.barScheduleManualPriority=true;c.QueueSettlementJob(3,"",0,3,1,0,out _);c.Tick(3*60,1);
  QueueCheck(c.metalBars[1]>0&&c.metalBars[0]==0&&!c.OwnsPlate(3,0),"manual repeating refining takes priority over automatic component supply");
  c.autoBarsEnabled=false;c.Tick(40*60,1);QueueCheck(c.OwnsPlate(3,0),"pausing manual refining resumes the queued gear chain");
  c=QueueSample();c.mine=1;c.researchRanks[LibraryResearch.Index("mine")]=2;c.researchRanks[LibraryResearch.Index("hunters")]=1;c.wood=24;c.stone=4;c.metalBars[0]=2;c.QueueSettlementJob(0,"mine",0,0,1,0,out _);c.QueueSettlementJob(1,"hunters",0,0,1,2,out _);c.Tick(.01,1);
  QueueCheck(c.activeConstruction==""&&c.activeResearch==""&&c.settlementQueue.Count==2,"partial stone stock stays reserved to the first job, preventing a cheaper second job from taking it");
  c.stone+=4;c.Tick(.01,1);QueueCheck(c.activeConstruction=="mine"&&c.activeResearch=="","new stone completes the first reservation before being offered to the second job");
  c=QueueSample();c.QueueSettlementJob(3,"",1,2,1,0,out _);c.metalBars[0]=4;c.QueueSettlementJob(3,"",0,3,1,0,out _);c.Tick(.01,1);
  QueueCheck(!c.OwnsPlate(3,0)&&c.settlementQueue.Count==2&&c.SettlementJobStatus(c.settlementQueue[1].id).Contains("earlier Blacksmith"),"a second craft cannot overtake an earlier waiting job at the same Blacksmith");
  c.Tick(100*60,1);QueueCheck(c.OwnsPlate(2,1)&&c.OwnsPlate(3,0),"same-building jobs complete in order as automatic materials arrive");
  Debug.Log("VEILBOUND_MANUAL_STOCKPILES_PASS");
 }
}}
