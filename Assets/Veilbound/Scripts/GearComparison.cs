using System;using System.Collections.Generic;
namespace Veilbound {
 public struct GearItemMetrics {
  public float armour,mass,damage,interval;public float Dps=>interval>0?damage/interval:0;
  public static GearItemMetrics From(CraftedGear item){
   if(item==null)return new GearItemMetrics();
   if(item.slot==9){var weapon=WeaponCatalog.ResolveLegacy(item.item);return new GearItemMetrics{damage=weapon.Damage,interval=weapon.AttackInterval,mass=weapon.Weight};}
   var definition=item.ArmourDefinition;return new GearItemMetrics{armour=definition?.Armour??0,mass=definition?.Weight??0};
  }
 }
 public struct GearStatChange {
  public string key,label,unit;public float before,after;public bool lowerIsBetter;
  public float Delta=>after-before;public bool Changed=>Math.Abs(Delta)>.0001f;public bool Better=>lowerIsBetter?Delta<0:Delta>0;
  public GearStatChange(string key,string label,string unit,float before,float after,bool lowerIsBetter=false){this.key=key;this.label=label;this.unit=unit;this.before=before;this.after=after;this.lowerIsBetter=lowerIsBetter;}
  public static string Number(float value)=>value.ToString("0.##");
  public string DeltaText=>!Changed?"Unchanged":(Delta>0?"+":"−")+(Math.Abs(Delta)<.005f?"<0.01":Number(Math.Abs(Delta)))+(unit=="%"?"%":unit==""?"":" "+unit);
  public string Transition=>Number(before)+" → "+Number(after)+(unit=="%"?"%":unit==""?"":" "+unit);
 }
 public sealed class GearComparisonData {
  public readonly CraftedGear selected,equipped;public readonly GearStatChange[] primary,bonuses;
  public bool AlreadyEquipped=>selected!=null&&selected.id==equipped?.id;
  public bool HasChanges {get{foreach(var p in primary)if(p.Changed)return true;return bonuses.Length>0;}}
  public static float Bonus(CraftedGear item,string stat)=>GearRollRules.Value(item,stat);
  public GearComparisonData(CraftedGear selected,CraftedGear equipped,EquipmentLoadout loadout=null){
   this.selected=selected;this.equipped=equipped;if(selected==null){primary=bonuses=new GearStatChange[0];return;}var a=GearItemMetrics.From(equipped);var b=GearItemMetrics.From(selected);
   primary=selected.slot==9?new[]{new GearStatChange("damage","BASE HIT DAMAGE","",a.damage,b.damage),new GearStatChange("interval","BASE ATTACK INTERVAL","sec",a.interval,b.interval,true),new GearStatChange("dps","BASE DPS","",a.Dps,b.Dps),new GearStatChange("mass","WEIGHT","kg",a.mass,b.mass,true)}:new[]{new GearStatChange("armour","ARMOUR","",a.armour,b.armour),new GearStatChange("mass","WEIGHT","kg",a.mass,b.mass,true)};
   var changes=new List<GearStatChange>();foreach(string stat in new[]{"power","crit","critdamage","speed","haste","vitality","area","regen","wisdom","attackspeed"}){var change=new GearStatChange(stat,stat=="haste"?"Haste rating":GearRollRules.Label(stat),stat=="haste"?"rating":stat=="vitality"?"health":stat=="regen"?"/ sec":"%",loadout==null?Bonus(equipped,stat):loadout.Bonus(stat),loadout==null?Bonus(selected,stat):loadout.Bonus(stat,selected));if(change.Changed)changes.Add(change);}bonuses=changes.ToArray();
  }
 }
}
