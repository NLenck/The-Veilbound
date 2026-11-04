using System;
using UnityEngine;
namespace Veilbound {public partial class VeilGame {
 Settlement ResourcePrioritySample(){
  var c=QueueSample();c.researchRanks[LibraryResearch.Index("mine")]=2;c.metalBars[1]=c.PlateRecipe(3,1)[0].amount;FundPlateSupports(c,1,3);
  Require(c.QueueSettlementJob(1,"mine",0,0,1,3,out _),"copper-bar research queued");
  Require(c.QueueSettlementJob(0,"housing",0,0,1,0,out _),"unrelated construction queued");
  Require(c.QueueSettlementJob(3,"",1,3,1,0,out _),"copper gear queued");return c;
 }
 void CheckResourcePriorities(){
  var c=ResourcePrioritySample();var matches=c.FilterSettlementPriorities(3,1);
  QueueCheck(matches.Count==2&&matches[0].kind==1&&matches[1].kind==3,"copper-bar filter shows competing research and gear only");
  QueueCheck(c.PriorityResourceAmount(matches[0],3,1)==3&&c.PriorityResourceAmount(matches[1],3,1)==c.PlateRecipe(3,1)[0].amount,"filtered jobs show correct remaining recipe quantities");
  QueueCheck(c.FilterSettlementPriorities(0,1).Count==2,"raw copper filter follows bar dependencies");
  int unrelated=c.settlementQueue[1].id,research=matches[0].id,gear=matches[1].id;
  var original=JsonUtility.FromJson<Settlement>(JsonUtility.ToJson(c));
  QueueCheck(c.MoveFilteredPriority(gear,-1,3,1)&&c.settlementQueue[0].id==gear&&c.settlementQueue[1].id==unrelated&&c.settlementQueue[2].id==research,"filtered priority swap preserves unrelated row positions in the shared order");
  var oreView=c.FilterSettlementPriorities(0,1);QueueCheck(oreView[0].id==gear&&oreView[1].id==research,"other resource views use the same order, preventing contradictory rankings");
  string before=JsonUtility.ToJson(c);QueueCheck(!c.MoveFilteredPriority(unrelated,-1,3,1)&&JsonUtility.ToJson(c)==before,"hidden jobs cannot be moved through another resource filter");
  var loaded=JsonUtility.FromJson<Settlement>(JsonUtility.ToJson(c));QueueCheck(loaded.FilterSettlementPriorities(3,1)[0].id==gear,"resource-priority order survives save/load");
  original.Tick(.01,1);c.Tick(.01,1);QueueCheck(original.activeResearch=="mine"&&!original.OwnsPlate(3,1)&&c.activeResearch==""&&c.OwnsPlate(3,1),"raising gear changes which workflow gets the shared copper bars");
  c=QueueSample();c.housingTier=3;c.researchRanks[LibraryResearch.Index("weaver")]=3;c.wood=2000;c.stone=1400;c.timber=c.quarry=0;c.metalBars[2]=4;
  c.QueueSettlementJob(0,"housing",0,0,1,0,out _);c.QueueSettlementJob(1,"weaver",0,0,1,4,out _);c.Tick(.01,1);
  QueueCheck(c.activeResearch=="weaver"&&c.activeConstruction=="","unavailable supply releases timber and stone so lower prerequisite research can run");
  QueueCheck(!Settlement.RecipeGraphHasCycle(c.BarRecipe,WeaponRules.Count),"all current metal recipes form an acyclic dependency graph");
  QueueCheck(Settlement.RecipeGraphHasCycle(i=>new[]{new ResourceCost(3,(i+1)%2,1)},2),"two-recipe circular dependency is detected");
  QueueCheck(Settlement.RecipeGraphHasCycle(i=>new[]{new ResourceCost(3,i,1)},1),"self-dependent recipe is detected");
  QueueCheck(!Settlement.RecipeGraphHasCycle(i=>i==0?new[]{new ResourceCost(3,1,1),new ResourceCost(3,2,1)}:i==1?new[]{new ResourceCost(3,2,1)}:new ResourceCost[0],3),"shared ingredients across recipes do not create false cycle reports");
  Debug.Log("VEILBOUND_RESOURCE_PRIORITIES_PASS");
 }
}}
