using System;
using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  void CheckEconomyBalance() {
   var c=new Settlement{mine=1,weaver=1,huntersLodge=1,blacksmith=9,tailor=2,leatherworker=2};c.EnsureGear();c.EnsureIndustry();
   for(int s=0;s<3;s++){Array.Clear(c.Allocation(s),0,c.Allocation(s).Length);Require(c.Allocate(s,0,16),"Level one supports sixteen capacity units");}
   double tin=c.ResourceAmount(0,0),cloth=c.cloth,hide=c.leather,wood=c.wood;
   c.Tick(900,2);Require(Math.Abs(c.ResourceAmount(0,0)-tin-10*Settlement.PassiveProductionMultiplier)<1e-8&&Math.Abs(c.cloth-cloth-10*Settlement.PassiveProductionMultiplier)<1e-8&&Math.Abs(c.leather-hide-10*Settlement.PassiveProductionMultiplier)<1e-8&&Math.Abs(c.hours-30)<1e-8,"A fifteen-minute Tier 1 run advances thirty city hours at daily production rates");
   Require(Math.Abs(c.wood-wood-c.WoodRate*30)<1e-8,"Building material hourly rate matches actual production");
   c.Tick(1,1);var saved=JsonUtility.FromJson<Settlement>(JsonUtility.ToJson(c));saved.EnsureGear();saved.EnsureIndustry();Require(Math.Abs(saved.cloth-c.cloth)<1e-8&&c.cloth%1>0,"Fractional stock persists at the slower pace");
   int[] units={6,6,8,4,4,4,6,4,4};
   for(int slot=0;slot<9;slot++){
    Require(Gear.CraftUnits(slot)==units[slot],"Slot costs use the requested 8/6/4 structure");
    if(slot<8)for(int metal=0;metal<10;metal++)Require(c.PlateRecipe(slot,metal).Length==3&&c.PlateRecipe(slot,metal)[0].amount>=units[slot],"Plate uses tier-scaled bars with finished leather and cloth");
    for(int tier=1;tier<=2;tier++){Settlement.GearCost(slot,1,tier,out double bars,out double textile,out double leather,out double essence);Require(textile>=units[slot]&&bars>0&&leather>0,"Cloth needs its primary fabric plus leather and bar fittings");Settlement.GearCost(slot,2,tier,out bars,out textile,out leather,out essence);Require(leather>=units[slot]&&textile>0&&bars>0,"Leather needs its primary material plus cloth and bar fittings");}
   }
   c.metalBars[0]=7;int count=c.craftedGear==null?0:c.craftedGear.Count;Require(!c.CraftPlate(2,0,out string message)&&c.metalBars[0]==7,"Insufficient primary bars consume nothing");
   c.metalBars[0]=8;Require(!c.CraftPlate(2,0,out message)&&c.metalBars[0]==8,"Bars alone cannot pay for the mixed-material plate recipe");c.AddResource(4,0,2);c.AddResource(5,0,2);Require(c.CraftPlate(2,0,out message)&&c.metalBars[0]==0&&c.ResourceAmount(4,0)==0&&c.ResourceAmount(5,0)==0&&c.craftedGear.Count>count,"Exact bars, leather and cloth pay for one chest");
   Require(c.WeaponRecipe(0).Length==3&&c.WeaponRecipe(0)[0].amount==8&&c.WeaponRecipe(0)[1].amount==2&&c.WeaponRecipe(0)[2].amount==2,"Starter greatsword requires bars, finished leather grip and cloth wrap");
   Debug.Log("VEILBOUND_ECONOMY_BALANCE_PASS");
  }
 }
}
