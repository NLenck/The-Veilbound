using System;
using System.Collections.Generic;
using UnityEngine;
namespace Veilbound {
 [Serializable] public sealed class GearAffix {public string stat;public float value;}
 [Serializable] public sealed class CraftedGear : EquipmentInstance {
  // Retained for old-save compatibility and weapon grades; armour always normalizes to Common.
  public UpgradeRarity rarity;
 }
 public static class GearRollRules {
  // Mystic enchantment stat tables; crafting never rolls affixes or quality.
  public static readonly string[] Stats={"crit","speed","critdamage","power","vitality","haste","area","regen"};
  public static readonly string[] Labels={"Critical chance","Movement speed","Critical damage","Ability damage","Maximum health","Haste","Area","Health / sec"};
  public static readonly int[] Weights={20,16,10,18,12,10,8,6};
  public static readonly float[][] Values={new[]{1f,1.3f,1.6f,1.9f,2f},new[]{2f,2.5f,3f,3.5f,4.2f},new[]{8f,10f,12f,16f,20f},new[]{2f,2.6f,3.2f,3.8f,4.2f},new[]{8f,10f,13f,16f,20f},new[]{1.5f,2f,2.5f,3f,3.5f},new[]{2f,2.5f,3f,3.5f,4.2f},new[]{.05f,.07f,.09f,.12f,.15f}};
  public static bool Major(int slot)=>slot==0||slot==2||slot==6||slot==9;
  public static float Scale(int slot)=>Major(slot)?1:.5f;
  public static int Count(UpgradeRarity rarity)=>rarity>=UpgradeRarity.Legendary?3:rarity>=UpgradeRarity.Rare?2:1;
  public static float Value(CraftedGear gear,string stat){float n=0;if(gear?.enchantments!=null)foreach(var a in gear.enchantments)if(a!=null&&a.stat==stat)n+=a.value;return n;}
  public static string Label(string stat){if(stat=="wisdom")return "Experience gain";if(stat=="attackspeed")return "Attack speed";int i=Array.IndexOf(Stats,stat);return i>=0?Labels[i]:stat;}
  public static string Amount(string stat,float value,bool sign=true)=>(sign&&value>=0?"+":"")+value.ToString("0.###")+(stat=="vitality"?" health":stat=="regen"?" / sec":"%");
  public static string Summary(CraftedGear gear)=>MysticRules.EnchantmentSummary(gear);
  public static string BaseName(CraftedGear gear)=>gear.slot==9?WeaponCatalog.ResolveLegacy(gear.item).Name:gear.ArmourDefinition?.Name??Gear.Name(gear.item);
  public static UpgradeRarity MaterialGrade(int slot,int item){
   if(slot==9)return WeaponCatalog.ResolveLegacy(item).Grade;
   return UpgradeRarity.Common;
  }
  public static CraftedGear Roll(int slot,int item,int id,System.Random random) {
   return new CraftedGear{id=id,slot=slot,item=item,rarity=MaterialGrade(slot,item),affixes=new GearAffix[0],enchantments=new GearAffix[0]};
  }
 }
 public partial class Settlement {
  public int gearRollVersion,nextGearRollId=1,lastCraftedRollId;public List<CraftedGear> craftedGear;public int[] equippedRollIds;
  public void EnsureGearRolls() {
   EnsureGear();if(craftedGear==null)craftedGear=new List<CraftedGear>();if(equippedRollIds==null||equippedRollIds.Length!=10)equippedRollIds=new int[10];
   nextGearRollId=Math.Max(1,nextGearRollId);
   // Craft bonuses were retired. Never convert them into free enchantments.
   foreach(var g in craftedGear){if(g.affixes!=null&&g.affixes.Length>0)g.affixes=new GearAffix[0];g.rarity=GearRollRules.MaterialGrade(g.slot,g.item);EnchantmentRules.Normalize(g);}
   if(gearRollVersion!=0)return;
   // Old items retain their exact base and receive no free random stats on upgrade.
   for(int s=0;s<9;s++){
    for(int m=1;m<=2;m++)for(int t=1;t<=ownedGear[s*3+m-1];t++)ImportGear(s,LightGear.Item(m,t));
    if(s<8)for(int m=0;m<10;m++)if(ownedPlate[s*10+m])ImportGear(s,Metals.Item(m));
    if(equippedGear[s]>0){var g=ImportGear(s,equippedGear[s]);equippedRollIds[s]=g.id;}
   }
   for(int m=0;m<ownedMetalWeapons.Length;m++)if(ownedMetalWeapons[m])ImportGear(9,m);equippedRollIds[9]=ImportGear(9,WeaponItemIndex).id;gearRollVersion=1;
  }
  CraftedGear ImportGear(int slot,int item){foreach(var g in craftedGear)if(g.slot==slot&&g.item==item)return g;var entry=new CraftedGear{id=nextGearRollId++,slot=slot,item=item,inherited=true,rarity=GearRollRules.MaterialGrade(slot,item),affixes=new GearAffix[0]};craftedGear.Add(entry);return entry;}
  public CraftedGear FindRoll(int id){EnsureGearRolls();return craftedGear.Find(g=>g.id==id);}
  public CraftedGear EquippedRoll(int slot)=>Loadout.Equipped(slot);
  [NonSerialized] EquipmentLoadout equipmentLoadout;
  public EquipmentLoadout Loadout {get{EnsureGearRolls();if(equipmentLoadout==null)equipmentLoadout=new EquipmentLoadout();equipmentLoadout.Bind(equippedGear,equippedRollIds,craftedGear,WeaponItemIndex,EquippedWeaponDefinition.Weight);return equipmentLoadout;}}
  public float GearBonus(string stat)=>Loadout.Bonus(stat);
  public CraftedGear RecordCraft(int slot,int item) {
   EnsureGearRolls();var g=GearRollRules.Roll(slot,item,nextGearRollId++,null);craftedGear.Add(g);lastCraftedRollId=g.id;
   if(slot<9&&equippedGear[slot]==0)EquipRoll(g.id);return g;
  }
  public bool EquipRoll(int id){var g=FindRoll(id);if(g==null)return false;if(g.slot==9){equippedMetalWeapon=g.item<0?-2:g.item;equippedSword=0;}else equippedGear[g.slot]=g.item;equippedRollIds[g.slot]=g.id;return true;}
  public void EquipKnownRoll(int slot,int item){EnsureGearRolls();var g=craftedGear.FindLast(r=>r.slot==slot&&(slot==9?r.item==item:Metals.IsMetal(r.item)&&Metals.IsMetal(item)?Metals.Index(r.item)==Metals.Index(item):r.item==item));if(g!=null)EquipRoll(g.id);else{if(slot<9)equippedRollIds[slot]=0;}}
 }
}
