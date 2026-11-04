using System;
using System.Collections;
using System.IO;
using UnityEngine;
namespace Veilbound {public partial class VeilGame {
 void GrantRefiningSlots(Settlement c,int rank=3){c.researchRanks[LibraryResearch.Index("blacksmith-slots")]=rank;}
 void CheckRefiningSlots(){
  var c=QueueSample();QueueCheck(c.BarProductionSlotCount==1,"existing Blacksmith starts with one refining slot");c.researchRanks[LibraryResearch.Index("blacksmith")]=2;QueueCheck(!c.StartResearch("blacksmith-slots",out _),"refining slot research requires Blacksmith research tier three");
  c.researchRanks[LibraryResearch.Index("blacksmith")]=3;c.metalBars[1]=3;QueueCheck(c.StartResearch("blacksmith-slots",out _),"refining side research is paid and timed");c.Tick(LibraryResearch.Duration("blacksmith-slots",1)*60,1);QueueCheck(c.BarProductionSlotCount==2&&!c.StartResearch("blacksmith-slots",out _),"one refining slot unlocks while tier six stays gated");
  c=QueueSample();GrantRefiningSlots(c);c.AddResource(0,0,5);c.QueueBars(0,5,out _);c.Tick(2/c.RefiningSpeed*60,1);QueueCheck(c.metalBars[0]==5&&c.smithOrders.Count==0&&c.ResourceAmount(0,0)==0,"four refining slots complete five bars in two waves without duplicate ingredients");
  c=QueueSample();GrantRefiningSlots(c);FundPlateSupports(c,0,3);c.AddResource(0,0,4);c.QueueSettlementJob(3,"",0,3,1,0,out _);c.Tick(60/c.RefiningSpeed,1);QueueCheck(c.OwnsPlate(3,0)&&c.ResourceAmount(0,0)==0&&c.metalBars[0]==0&&c.totalMinedUnits==0,"automatic parallel refining makes exactly the required bars from existing ore");
  c=QueueSample();GrantRefiningSlots(c);c.AddResource(0,0,1);c.AddResource(0,1,2);c.QueueSettlementJob(2,"",2,0,1,0,out _);c.Tick(600,1);QueueCheck(c.metalBars[2]==1&&c.metalBars[0]==0&&c.metalBars[1]==0&&c.ResourceTotal(0)==0,"parallel component refining feeds bronze without overspending or excess bars");
  c=QueueSample();GrantRefiningSlots(c,1);c.AddResource(0,0,1);c.AddResource(0,1,1);c.AssignBarProductionSlot(0,0,out _);c.AssignBarProductionSlot(1,1,out _);c.Tick(10,1);var loaded=JsonUtility.FromJson<Settlement>(JsonUtility.ToJson(c));c.Tick(300,1);loaded.Tick(300,1);QueueCheck(c.metalBars[0]==1&&c.metalBars[1]==1&&loaded.metalBars[0]==1&&loaded.metalBars[1]==1,"different manual bar assignments preserve paid ingredients and partial work through save/load");
  c=QueueSample();GrantRefiningSlots(c);c.AddResource(0,0,5);c.QueueBars(0,5,out _);c.Tick(6,1);c.CancelSmithOrder(c.smithOrders[0].id);c.Tick(600,1);QueueCheck(c.ResourceAmount(0,0)==5&&c.metalBars[0]==0,"cancelling a parallel manual batch refunds each unfinished unit exactly once");
  c=QueueSample();GrantRefiningSlots(c);FundPlateSupports(c,0,3);c.AddResource(0,0,4);c.QueueSettlementJob(3,"",0,3,1,0,out _);c.Tick(6,1);c.RemoveSettlementJob(c.settlementQueue[0].id);c.Tick(600,1);QueueCheck(c.ResourceAmount(0,0)==4&&c.metalBars[0]==0,"cancelling automatic demand refunds all unfinished parallel refining slots");
  c=QueueSample();GrantRefiningSlots(c);c.AddResource(0,0,7);c.QueueBars(0,7,out _);var stepped=JsonUtility.FromJson<Settlement>(JsonUtility.ToJson(c));c.Tick(180,1);for(int i=0;i<180;i++)stepped.Tick(1,1);QueueCheck(c.metalBars[0]==7&&stepped.metalBars[0]==7&&c.ResourceAmount(0,0)==stepped.ResourceAmount(0,0),"parallel refining is consistent between bulk and frame ticks");
  c=QueueSample();GrantRefiningSlots(c);FundPlateSupports(c,0,3);c.AddResource(0,0,4);c.AddResource(0,1,1);c.QueueSettlementJob(3,"",0,3,1,0,out _);c.Tick(6,1);double work=c.RefiningSlots()[1].remainingWork;c.QueueBars(1,1,out _);c.Tick(6,1);QueueCheck(Math.Abs(c.RefiningSlots()[1].remainingWork-work)<1e-8,"manual batches pause automatic refining without losing its paid work");c.Tick(600,1);QueueCheck(c.OwnsPlate(3,0)&&c.metalBars[1]==1,"automatic refining resumes after the manual batch");
  QueueCheck(!c.AssignBarProductionSlot(0,9,out _),"refining assignments respect material research and building requirements");
  Debug.Log("VEILBOUND_REFINING_SLOTS_PASS");
 }
 IEnumerator RefiningSlotScreenshots(string output){
  city=QueueSample();GrantRefiningSlots(city);city.AddResource(0,0,8);city.AddResource(0,1,8);city.AssignBarProductionSlot(1,1,out _);city.QueueSettlementJob(3,"",0,3,1,0,out _);city.Tick(6,1);libraryResearchOpen=false;OpenCityWorkshop(true,3);SetSmithPage(3);yield return CardScreenshot(Path.Combine(output,"09-refining-slots.png"));
  OpenBarSlotResearch();yield return CardScreenshot(Path.Combine(output,"10-refining-research.png"));libraryResearchOpen=false;
 }
}}
