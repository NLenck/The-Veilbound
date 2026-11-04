using System;
using UnityEngine;
namespace Veilbound.Editor {
 public static class SmithValidation {
  static void Check(bool ok,string why){if(!ok)throw new Exception("Blacksmith: "+why);}
  public static Settlement Sample(){var c=Settlement.NewCamp();c.EnsureResearch();c.blacksmith=10;c.library=1;c.wood=c.stone=10000;c.EnsureIndustry();for(int i=0;i<c.researchRanks.Length;i++)c.researchRanks[i]=LibraryResearch.IsSlotResearch(LibraryResearch.Ids[i])?0:LibraryResearch.Limit(LibraryResearch.Ids[i]);for(int f=0;f<3;f++)for(int i=0;i<Settlement.ResourceNames[f].Length;i++)c.AddResource(f,i,10000);for(int family=4;family<6;family++)for(int i=0;i<5;i++)c.AddResource(family,i,100000);return c;}
  public static void Run(){
   var c=Sample();c.researchRanks[LibraryResearch.Index("plate")]=3;for(int s=0;s<8;s++){c.metalBars[3]=100;double bars=c.metalBars[3];int count=c.craftedGear.Count;Check(!c.CraftPlate(s,3,out _)&&c.metalBars[3]==bars&&c.craftedGear.Count==count,"all locked Brass plate slots reject crafting without charging");}
   c.researchRanks[LibraryResearch.Index("plate")]=4;Check(c.CraftPlate(2,3,out _)&&c.metalBars[3]==100-c.PlateRecipe(2,3)[0].amount,"research unlocks the exact tier-scaled Brass chest recipe");Check(c.FindRoll(c.lastCraftedRollId).slot==2&&c.OwnsPlate(2,3),"craft produces real inventory item");
   c=Sample();c.blacksmith=1;double tin=c.ResourceAmount(0,0);Check(c.QueueBars(0,3,out _)&&c.metalBars[0]==0&&c.ResourceAmount(0,0)==tin-3,"ingredients reserved and no instant output");int id=c.smithOrders[0].id;c.Tick(30,1);Check(c.metalBars[0]==0&&Math.Abs(c.smithOrders[0].remainingWork-.5)<1e-8,"fractional city-hour work");
   c.Tick(30,1);Check(c.metalBars[0]==1&&c.smithOrders[0].completed==1,"one bar arrives independently at completion");c.Tick(15,2);Check(c.metalBars[0]==1&&Math.Abs(c.smithOrders[0].remainingWork-.5)<1e-8,"expedition dilation advances refining");
   c=JsonUtility.FromJson<Settlement>(JsonUtility.ToJson(c));c.EnsureResearch();c.EnsureSmithOrders();Check(c.smithOrders[0].id==id&&c.smithOrders[0].completed==1&&Math.Abs(c.smithOrders[0].remainingWork-.5)<1e-8,"partial work and IDs survive save/reload");double ore=c.ResourceAmount(0,0);Check(c.CancelSmithOrder(id)&&c.ResourceAmount(0,0)==ore+2&&c.metalBars[0]==1&&!c.CancelSmithOrder(id),"cancel returns only unfinished ingredients exactly once");
   c=Sample();c.blacksmith=2;Check(c.QueueBars(0,1,out _)&&c.QueueBars(1,2,out _),"two researched orders queue");c.Tick(60*3.5/c.RefiningSpeed,1);Check(c.smithOrders.Count==0&&c.metalBars[0]==1&&c.metalBars[1]==2,"large tick carries surplus time across queued orders");
   c=Sample();string before=JsonUtility.ToJson(c);Check(!c.QueueBars(-1,1,out _)&&!c.QueueBars(99,1,out _)&&!c.QueueBars(0,0,out _)&&!c.QueueBars(0,1000,out _),"invalid IDs and quantities rejected");Check(before==JsonUtility.ToJson(c),"invalid orders mutate no stock or queue");
   for(int i=0;i<SmithRefining.QueueLimit;i++)Check(c.QueueBars(0,1,out _),"bounded queue accepts available slots");double stock=c.ResourceAmount(0,0);Check(!c.QueueBars(0,1,out _)&&c.ResourceAmount(0,0)==stock,"full queue cannot charge materials");
   c=Sample();c.researchRanks[LibraryResearch.Index("blacksmith")]=3;stock=c.ResourceAmount(0,1);Check(!c.QueueBars(3,1,out _)&&c.ResourceAmount(0,1)==stock,"refining research gate independent of plate research");
   c=Sample();double crystal=c.ResourceAmount(0,7),coal=c.ResourceAmount(0,4);c.metalBars[7]=c.metalBars[8]=2;Check(c.QueueBars(9,1,out _)&&c.QueueBars(10,1,out _),"both Veilcrystal variants accept separate orders");Check(c.ResourceAmount(0,7)==crystal-4&&c.ResourceAmount(0,4)==coal-4&&c.metalBars[7]==1&&c.metalBars[8]==1,"Veilcrystal recipes reserve correct base bars and coal");c.TickSmithOrders(c.SmithQueueHours());Check(c.metalBars[9]==1&&c.metalBars[10]==1,"Veilcrystal variants remain separate stock");
   c=Sample();c.blacksmith=1;c.QueueBars(0,100,out _);var cost=c.UpgradeCost("blacksmith");Check(c.UpgradeBuilding("blacksmith",out _),"researched Blacksmith upgrade starts");c.Tick(60*25,1);Check(c.blacksmith==2&&c.metalBars[0]==25&&Math.Abs(c.smithOrders[0].remainingWork-.9)<1e-7,"refining rate changes at construction completion, not before");
   var stepped=JsonUtility.FromJson<Settlement>(JsonUtility.ToJson(c));c.Tick(600,2);for(int i=0;i<6000;i++)stepped.Tick(.1,2);Check(c.metalBars[0]==stepped.metalBars[0]&&Math.Abs(c.smithOrders[0].remainingWork-stepped.smithOrders[0].remainingWork)<1e-7,"frame steps match one large dilated step");
   var old=new Settlement{blacksmith=9,mine=7,wood=123.25};old.EnsureGear();old.EnsureResearch();old.EnsureSmithOrders();Check(old.smithOrders.Count==0&&old.wood==123.25&&old.ResearchRank("plate")==10,"legacy upgrade entitlements and stocks survive new queue fields");
   c=Sample();foreach(string path in new[]{"blacksmith","weapons","plate"})c.researchRanks[LibraryResearch.Index(path)]=3;
   for(int metal=3;metal<WeaponRules.Count;metal++){
    c.metalBars[metal]=100;int items=c.craftedGear.Count,orders=c.smithOrders.Count;double barsBefore=c.metalBars[metal],rawBefore=c.ResourceTotal(0);
    Check(!c.CraftMetalWeapon(metal,out _)&&c.metalBars[metal]==barsBefore&&c.craftedGear.Count==items,"all unresearched weapon tiers and both Veilcrystal variants reject crafting: "+metal);
    Check(!c.CanQueueBars(metal,1)&&!c.QueueBars(metal,1,out _)&&c.smithOrders.Count==orders&&c.ResourceTotal(0)==rawBefore,"all unresearched bar tiers and variants reject queuing: "+metal);
   }
   c.metalBars[2]=50;Check(c.StartResearch("weapons",out _),"next weapon tier research starts");c.Tick(LibraryResearch.Duration(4)*60,1);
   Check(c.CraftMetalWeapon(3,out _)&&!c.QueueBars(3,1,out _)&&!c.CraftPlate(2,3,out _),"completed weapon research unlocks only weapons, not refining or armor");
   Debug.Log("VEILBOUND_SMITH_LOGIC_PASS");
  }
 }
}
