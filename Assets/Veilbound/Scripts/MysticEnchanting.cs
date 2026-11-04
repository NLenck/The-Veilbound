using System;
using System.Collections.Generic;
namespace Veilbound {
 public static class MysticRules {
  public static readonly string[] Categories={"Offense","Defense","Utility"};
  public static readonly string[][] CategoryStats={new[]{"crit","critdamage","power"},new[]{"vitality","regen"},new[]{"speed","haste","area"}};
  public static string Feature(int level)=>level==1?"One random enchantment per item":level==2?"Two enchantments per item":level==3?"Target offense, defense or utility":"Choose a specific stat";
  public static float Strength(CraftedGear item,string stat){int i=Array.IndexOf(GearRollRules.Stats,stat);return i<0?0:GearRollRules.Values[i][EnchantmentRules.PowerBand(item)]*GearRollRules.Scale(item.slot)*.5f;}
  public static double Cost(CraftedGear item,int mode,int slot)=>Math.Ceiling(10*(EnchantmentRules.PowerBand(item)+1)*(slot==1?1.5:1)*(mode==2?3:mode==1?2:1));
  public static string StatText(string stat,float value)=>"+"+value.ToString("0.###")+(stat=="vitality"?" Maximum health":stat=="regen"?" Health / sec":"% "+GearRollRules.Label(stat));
  public static string EnchantmentSummary(CraftedGear item){if(item?.enchantments==null||item.enchantments.Length==0)return "No enchantments";var text=new List<string>();foreach(var a in item.enchantments)text.Add(StatText(a.stat,a.value));return string.Join(" / ",text);}
 }
 public partial class Settlement {
  public int mysticLevel;
  [NonSerialized] System.Random enchantRandom;
  public int ItemEnchantmentLimit=>EnchantmentRules.SlotLimit(mysticLevel);
  public int EnchantmentCount(CraftedGear item)=>item?.enchantments==null?0:item.enchantments.Length;
  List<string> EnchantmentPool(CraftedGear item,int mode,int category,string stat,int target){
   var pool=new List<string>();if(mode==2){if(Array.IndexOf(GearRollRules.Stats,stat)>=0)pool.Add(stat);}else if(mode==1){if(category>=0&&category<MysticRules.Categories.Length)pool.AddRange(MysticRules.CategoryStats[category]);}else pool.AddRange(GearRollRules.Stats);
   if(item.enchantments!=null)for(int i=0;i<item.enchantments.Length;i++)if(i!=target)pool.Remove(item.enchantments[i].stat);
   // A paid replacement must change the enchantment; retired crafting affixes never contribute.
   if(item.enchantments!=null&&target<item.enchantments.Length)pool.Remove(item.enchantments[target].stat);return pool;
  }
  public string EnchantmentRequirement(int id,int mode,int category,string stat,int target){
   if(mysticLevel<1)return "Build the Mystic first.";var item=FindRoll(id);if(item==null||item.slot<0||item.slot>9)return "Choose an owned equipment item.";
   if(mode<0||mode>2||mode==1&&(category<0||category>2))return "Choose a valid enchantment method.";
   if(mode==1&&mysticLevel<3)return "Upgrade the Mystic to level 3 for category targeting.";if(mode==2&&mysticLevel<4)return "Upgrade the Mystic to level 4 for specific stats.";
   int count=EnchantmentCount(item);if(target<0||target>=ItemEnchantmentLimit||target>count)return "Choose an unlocked enchantment slot.";
   if(EnchantmentPool(item,mode,category,stat,target).Count==0)return "No different eligible stat. Choose another category or stat.";
   if(double.IsNaN(essence)||double.IsInfinity(essence)||essence+1e-9<MysticRules.Cost(item,mode,target))return "Not enough Veil Essence.";return "";
  }
  public bool EnchantGear(int id,int mode,int category,string stat,int target,out string message){
   message=EnchantmentRequirement(id,mode,category,stat,target);if(message!="")return false;var item=FindRoll(id);var pool=EnchantmentPool(item,mode,category,stat,target);
   if(enchantRandom==null)enchantRandom=new System.Random();string selected;
   if(mode==2)selected=pool[0];else{int total=0;foreach(string name in pool)total+=GearRollRules.Weights[Array.IndexOf(GearRollRules.Stats,name)];int pick=enchantRandom.Next(total);selected=pool[0];foreach(string name in pool){pick-=GearRollRules.Weights[Array.IndexOf(GearRollRules.Stats,name)];if(pick<0){selected=name;break;}}}
   var value=new GearAffix{stat=selected,value=MysticRules.Strength(item,selected)};var result=item.enchantments==null?new List<GearAffix>():new List<GearAffix>(item.enchantments);
   if(target==result.Count)result.Add(value);else result[target]=value;
   essence=Math.Max(0,essence-MysticRules.Cost(item,mode,target));item.enchantments=result.ToArray();message=GearRollRules.BaseName(item)+" #"+item.id+" enchanted: "+MysticRules.StatText(selected,value.value)+".";return true;
  }
 }
}
