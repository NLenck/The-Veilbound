using System;
namespace Veilbound {
 // One cost policy for display, direct crafting and automatic settlement supply.
 public static class GearCraftCosts {
  static readonly int[] FittingMetals={0,1,1,4,4,5,5,7,8,9};
  static int Rank(int tier)=>Math.Max(1,Math.Min(10,tier));
  public static int Fabric(int tier)=>(Rank(tier)-1)/2;
  public static int FittingMetal(int tier)=>FittingMetals[Rank(tier)-1];
  public static int Main(int slot,int material,int tier)=>
   (int)Math.Ceiling(Gear.CraftUnits(slot)*(material==3?(20+3*(Rank(tier)-1))/20d:(Rank(tier)+1)/2d));
  public static int Lining(int slot,int tier)=>(int)Math.Ceiling(Gear.CraftUnits(slot)*(Rank(tier)+3)/16d);
  public static int Fittings(int slot,int tier)=>(int)Math.Ceiling(Gear.CraftUnits(slot)*(Rank(tier)+3)/32d);
  public static ResourceCost[] Armour(int slot,int material,int tier,int metal=-1){
   int fabric=Fabric(tier),main=Main(slot,material,tier),lining=Lining(slot,tier),fittings=Fittings(slot,tier);
   if(material==3)return new[]{new ResourceCost(3,metal,main),new ResourceCost(4,fabric,lining),new ResourceCost(5,fabric,lining)};
   if(material==2)return new[]{new ResourceCost(4,fabric,main),new ResourceCost(5,fabric,lining),new ResourceCost(3,FittingMetal(tier),fittings)};
   return new[]{new ResourceCost(5,fabric,main),new ResourceCost(4,fabric,lining),new ResourceCost(3,FittingMetal(tier),fittings)};
  }
  public static ResourceCost[] Weapon(int metal){
   int tier=WeaponRules.Material(metal)+1,grip=Metals.GripGrade(WeaponRules.Material(metal)),lining=Lining(9,tier);
   return new[]{new ResourceCost(3,metal,Main(9,3,tier)),new ResourceCost(4,grip,lining),new ResourceCost(5,grip,lining)};
  }
 }
}
