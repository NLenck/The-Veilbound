using System;using System.Collections.Generic;using UnityEngine;
namespace Veilbound {
 public static class LightGear {
  public const int MaxTier=10;
  static readonly float[] LeatherArmour={4,6,9,12,17,23,31,40,50,60},ClothArmour={1,1.5f,2,3,4,5.5f,7.5f,10,12.5f,15};
  public static int Item(int material,int tier)=>tier<=2?material*10+tier:(material==1?200:300)+tier;
  public static bool Extended(int item)=>item>=203&&item<=210||item>=303&&item<=310;
  public static int Material(int item)=>item>=303?2:1;
  public static int Tier(int item)=>item%100;
  public static int FabricIndex(int tier)=>Mathf.Clamp((tier-1)/2,0,4);
  public static float Progress(int tier)=>Mathf.Clamp01((tier-1)/9f);
  public static float Armour(int slot,int material,int tier)=>tier<1?0:(material==2?LeatherArmour:ClothArmour)[Mathf.Clamp(tier-1,0,9)]*Metals.SlotScale(slot);
  public static float Mass(int slot,int material,int tier)=>tier<1?0:(material==2?Mathf.Lerp(2,4,Progress(tier))/9.1f:Mathf.Lerp(.5f,1,Progress(tier))/10.3f)*Gear.Size(slot);
  public static float Bonus(int slot,int item,string stat){
   int material=Gear.Material(item),tier=Gear.Tier(item);if(item==0||material<1||material>2)return 0;
   float progress=Progress(tier),size=Gear.Size(slot);
   if(material==2&&(stat=="speed"||stat=="haste"))return Mathf.Lerp(2,5,progress)*size/9.1f;
   if(material==1&&stat=="wisdom")return Mathf.Lerp(5,10,progress)*size/10.3f;
   if(material==1&&stat=="area")return Mathf.Lerp(2,5,progress)*size/10.3f;
   return 0;
  }
  public static string Name(int material,int tier){string name=Settlement.ProcessedNames[material==2?0:1][FabricIndex(tier)];return name+" T"+tier+(tier%2==0?" / reinforced":"");}
  public static Color Colour(int material,int tier)=>Color.Lerp(material==2?new Color(.17f,.095f,.045f):new Color(.14f,.17f,.18f),material==2?new Color(.25f,.10f,.085f):new Color(.24f,.16f,.28f),Progress(tier));
  public static string Traits(int slot,int item){int material=Gear.Material(item);if(item==0||material<1||material>2)return "";return material==2?$"+{Bonus(slot,item,"speed"):0.##}% movement / +{Bonus(slot,item,"haste"):0.##} Haste rating":$"+{Bonus(slot,item,"wisdom"):0.##}% XP / +{Bonus(slot,item,"area"):0.##}% area";}
 }
 public partial class Settlement {
  public int NextLightTier(int slot,int material)=>Math.Min(LightGear.MaxTier,Owned(slot,material)+1);
  public int LightJobTier(SettlementJob job)=>job.target>0?job.target:NextLightTier(job.slot,job.index);
  public ResourceCost[] LightGearRecipe(int slot,int material,int tier)=>GearCraftCosts.Armour(slot,material,tier);
  public double LightGearEssence(int slot,int material,int tier){GearCost(slot,material,tier,out _,out _,out _,out double essence);return essence;}
  public string LightGearRequirement(int slot,int material,int tier){
   if(slot<0||slot>8||material<1||material>2||tier<1||tier>LightGear.MaxTier||slot==8&&material!=1)return "Invalid cloth or leather recipe.";
   if(Workshop(material)<tier)return "Requires Clothier building level "+tier+".";
   if(ResearchRank("clothier")<tier)return "Requires Clothier research tier "+tier+".";return "";
  }
  public bool CraftLightGear(int slot,int material,int tier,out string message){
   EnsureGearRolls();message=LightGearRequirement(slot,material,tier);if(message!="")return false;
   double veil=LightGearEssence(slot,material,tier);if(essence+1e-9<veil){message="Requires "+veil+" Veil Essence.";return false;}
   if(!PayRecipe(LightGearRecipe(slot,material,tier),1,out message))return false;essence-=veil;ownedGear[slot*3+material-1]=Math.Max(Owned(slot,material),tier);
   var roll=RecordCraft(slot,LightGear.Item(material,tier));message=GearRollRules.BaseName(roll)+" #"+roll.id+" crafted. Compare it in your bag.";return true;
  }
 }
}
