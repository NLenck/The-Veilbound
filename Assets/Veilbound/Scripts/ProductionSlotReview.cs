using System;
using System.Collections;
using System.IO;
using UnityEngine;
namespace Veilbound {public partial class VeilGame {
 void GrantSlots(Settlement c,int family,int rank){c.researchRanks[LibraryResearch.Index(Settlement.SlotResearchBranch(family))]=rank;}
 void CheckProductionSlots(){
  var c=QueueSample();QueueCheck(c.ProductionSlotCount(0)==1&&c.ProductionSlotCount(1)==1&&c.ProductionSlotCount(2)==1,"existing saves and buildings begin with one production slot");
  c.researchRanks[LibraryResearch.Index("mine")]=2;QueueCheck(!c.StartResearch("mine-slots",out _),"slot research requires main research tier three");c.researchRanks[LibraryResearch.Index("mine")]=3;c.metalBars[1]=3;
  double wood=c.wood,copper=c.metalBars[1];QueueCheck(c.StartResearch("mine-slots",out _)&&c.ProductionSlotCount(0)==1,"slot unlock is a paid timed side research");c.Tick(600,1);QueueCheck(c.ProductionSlotCount(0)==1&&c.CancelResearch()&&c.wood>=wood&&c.metalBars[1]==copper,"cancelling side research refunds its inputs without unlocking a slot");
  c.StartResearch("mine-slots",out _);c.Tick(LibraryResearch.Duration("mine-slots",1)*60,1);QueueCheck(c.ProductionSlotCount(0)==2&&c.ProductionSlotCount(1)==1,"finished side research adds a slot only to its own building");
  QueueCheck(!c.StartResearch("mine-slots",out _),"second slot side research requires tier six");c.researchRanks[LibraryResearch.Index("mine")]=6;c.metalBars[4]=6;QueueCheck(c.StartResearch("mine-slots",out _),"tier six unlocks the next paid slot branch");c.Tick(LibraryResearch.Duration("mine-slots",2)*60,1);QueueCheck(c.ProductionSlotCount(0)==3&&!c.StartResearch("mine-slots",out _),"third slot unlocks while the tier-nine branch remains gated");
  c=QueueSample();GrantSlots(c,0,3);c.QueueMining(0,5,out _);double unit=Settlement.MineUnitWork(0)/c.MiningSpeed;c.Tick(unit*2*60,1);QueueCheck(c.ResourceAmount(0,0)==5&&c.mineOrders.Count==0,"four Mine slots complete a five-unit batch in two waves without duplicate ore");
  c=QueueSample();GrantSlots(c,0,3);c.QueueSettlementJob(3,"",0,3,1,0,out _);c.Tick(30*60,1);QueueCheck(c.OwnsPlate(3,0)&&c.totalMinedUnits==4&&c.ResourceAmount(0,0)<1e-8,"parallel automatic slots gather exactly the missing ore for queued gear");
  c=QueueSample();GrantSlots(c,0,1);c.AssignProductionSlot(0,0,0,out _);c.AssignProductionSlot(0,1,1,out _);c.Tick(60,1);var loaded=JsonUtility.FromJson<Settlement>(JsonUtility.ToJson(c));c.Tick(600,1);loaded.Tick(600,1);QueueCheck(c.ResourceAmount(0,0)>0&&c.ResourceAmount(0,1)>0&&c.ResourceAmount(0,0)==loaded.ResourceAmount(0,0)&&c.ResourceAmount(0,1)==loaded.ResourceAmount(0,1),"different manual slot resources and partial work survive save/load");
  c=QueueSample();GrantSlots(c,0,3);c.QueueMining(0,2,out _);c.QueueMining(1,3,out _);var small=JsonUtility.FromJson<Settlement>(JsonUtility.ToJson(c));c.Tick(12*60,1);for(int i=0;i<720;i++)small.Tick(1,1);QueueCheck(c.ResourceAmount(0,0)==2&&c.ResourceAmount(0,1)==3&&small.ResourceAmount(0,0)==2&&small.ResourceAmount(0,1)==3,"parallel manual batches preserve quantity and order under bulk and frame ticks");
  c=QueueSample();GrantSlots(c,0,3);c.QueueMining(0,5,out _);double forecast=c.ProjectedMineUnits(unit*2);c.Tick(unit*2*60,1);QueueCheck(forecast==5&&c.totalMinedUnits==5,"Mine projection counts deliveries across parallel slots");
  c=QueueSample();for(int f=1;f<3;f++)GrantSlots(c,f,3);c.QueueSettlementJob(0,"housing",0,0,1,0,out _);c.Tick(3*60,1);QueueCheck(c.activeConstruction=="housing"&&c.ResourceAmount(1,0)<1e-7&&c.ResourceAmount(2,0)<1e-7,"four Hunter and Textile slots accelerate automatic supply without surplus");
  c=QueueSample();GrantSlots(c,2,1);c.AssignProductionSlot(2,1,1,out _);c.QueueSettlementJob(0,"housing",0,0,1,0,out _);c.Tick(60,1);QueueCheck(c.ResourceAmount(2,1)>0&&c.ResourceAmount(2,0)>0&&c.ProducerSlots(2)[1].resource==1,"manual textile slot keeps wool while the automatic slot supplies flax");
  QueueCheck(!c.AssignProductionSlot(2,3,0,out _)&&!c.AssignProductionSlot(2,1,4,out _),"locked slots and locked materials reject assignments");
  c=QueueSample();GrantSlots(c,0,1);c.QueueMining(0,5,out _);c.Tick(60,1);c.CancelMining(c.mineOrders[0].id);c.Tick(10*60,1);QueueCheck(c.ResourceAmount(0,0)==0,"cancelling a manual order cancels its in-flight parallel claims");
  c=QueueSample();c.researchRanks[LibraryResearch.Index("mine")]=3;c.metalBars[1]=3;QueueCheck(c.QueueSettlementJob(1,"mine-slots",0,0,1,1,out _),"side research participates in the shared priority queue");c.Tick(40*60,1);QueueCheck(c.ProductionSlotCount(0)==2&&c.settlementQueue.Count==0,"queued side research completes and activates its slot");
  Debug.Log("VEILBOUND_PRODUCTION_SLOTS_PASS");
 }
 IEnumerator ProductionSlotScreenshots(string output){
  city=QueueSample();for(int f=0;f<3;f++)GrantSlots(city,f,3);city.AssignProductionSlot(0,1,1,out _);city.QueueSettlementJob(3,"",2,2,1,0,out _);city.Tick(60,1);libraryResearchOpen=false;OpenProducer(0);producerItem=0;yield return CardScreenshot(Path.Combine(output,"05-mine-slots.png"));
  OpenProducer(1);city.AssignProductionSlot(1,1,1,out _);city.RefreshSettlementPlan();yield return CardScreenshot(Path.Combine(output,"06-hunter-slots.png"));
  OpenProducer(2);city.AssignProductionSlot(2,1,1,out _);city.RefreshSettlementPlan();yield return CardScreenshot(Path.Combine(output,"07-textile-slots.png"));
  city=QueueSample();city.researchRanks[LibraryResearch.Index("mine")]=3;OpenSlotResearch(0);yield return CardScreenshot(Path.Combine(output,"08-slot-research.png"));libraryResearchOpen=false;
 }
}}
