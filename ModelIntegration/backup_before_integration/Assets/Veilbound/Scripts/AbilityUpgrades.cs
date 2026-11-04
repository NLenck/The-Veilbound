using System;
using System.Collections.Generic;
namespace Veilbound {
 public sealed class AbilityUpgrade {
  public readonly string key,label,unit;public readonly float step,cap;
  public AbilityUpgrade(string key,string label,float step,string unit,float cap=1000){this.key=key;this.label=label;this.step=step;this.unit=unit;this.cap=cap;}
  public string Effect(float strength){float n=step*((key=="repeats"||key=="projectiles")?(float)Math.Ceiling(strength):strength);return (n>0?"+":"")+n.ToString("0.##")+unit+" "+label.ToLowerInvariant()+".";}
 }
 public static class AbilitySpecs {
  public static readonly string[] Weapons={"cleave","whirl","lance","orbit","smite"};
  public static readonly string[] Virtues={"brand","bloodthirst","vengeance","warcry","rend"};
  public static bool IsVirtue(string id)=>id=="rage"||Array.IndexOf(Virtues,id)>=0;
  public static bool IsWeapon(string id)=>Array.IndexOf(Weapons,id)>=0;
  static AbilityUpgrade D(float step=25)=>new AbilityUpgrade("damage","Damage",step,"%");
  static AbilityUpgrade A(float step=10)=>new AbilityUpgrade("area","Area",step,"%");
  static AbilityUpgrade H()=>new AbilityUpgrade("haste","Haste",8,"% rating");
  public static AbilityUpgrade[] Upgrades(string id) {
   switch(id) {
    case "cleave":return new[]{D(20),A(8),H(),new AbilityUpgrade("repeats","Extra swings",1,"",3)};
    case "whirl":return new[]{D(),A(),H()};
    case "lance":return new[]{D(),A(),H(),new AbilityUpgrade("projectiles","Extra blades",1,"",4)};
    case "orbit":return new[]{D(),A(),H()};
    case "smite":return new[]{D(),A(),H()};
    case "rage":return new[]{D(20),A(),new AbilityUpgrade("power","Frenzy damage bonus",5,"%"),new AbilityUpgrade("haste","Frenzy haste",5,"% rating"),new AbilityUpgrade("duration","Frenzy duration",1,"s",6)};
    case "brand":return new[]{new AbilityUpgrade("effect","Damage taken",5,"%",13),A(),new AbilityUpgrade("duration","Mark duration",1,"s",12),H()};
    case "warcry":return new[]{new AbilityUpgrade("effect","Enemy damage reduction",5,"%",10),A(),new AbilityUpgrade("duration","Weaken duration",1,"s",12),H()};
    case "bloodthirst":return new[]{D(20),A(),new AbilityUpgrade("healing","Damage returned as health",2,"%",15),new AbilityUpgrade("healcap","Healing cap",2.5f,"% max health",10),H()};
    case "rend":return new[]{new AbilityUpgrade("chance","Bleed chance",5,"%",16),D(25),new AbilityUpgrade("duration","Bleed duration",1,"s",7)};
    case "vengeance":return new[]{new AbilityUpgrade("chance","Retaliation chance",5,"%",16),new AbilityUpgrade("guard","Triggering-hit protection",5,"%",13),H()};
    default:return new AbilityUpgrade[0];
   }
  }
  public static AbilityUpgrade Definition(string id,string key)=>Array.Find(Upgrades(id),u=>u.key==key);
  public static Choice Upgrade(Choice ability,AbilityUpgrade upgrade,UpgradeRarity rarity=UpgradeRarity.Common)=>new Choice(ability.id,ability.name,ability.description,true){attribute=upgrade.key,rarity=rarity};
 }
 public sealed partial class RunBuild {
  readonly Dictionary<string,Dictionary<string,float>> attributes=new Dictionary<string,Dictionary<string,float>>();
  public float Attribute(string id,string attribute)=>attributes.TryGetValue(id,out var stats)&&stats.TryGetValue(attribute,out float n)?n:0;
  public int WeaponSlots {get{int n=0;foreach(string id in AbilitySpecs.Weapons)if(Rank(id)>0)n++;return n;}}
  public int VirtueSlots {get{int n=0;foreach(string id in AbilitySpecs.Virtues)if(Rank(id)>0)n++;return n;}}
  public bool CanUpgrade(Choice c){if(!c.ability||Rank(c.id)==0||string.IsNullOrEmpty(c.attribute))return true;var d=AbilitySpecs.Definition(c.id,c.attribute);return d!=null&&Attribute(c.id,c.attribute)<d.cap-.0001f;}
  void PickAbility(Choice c) {
   if(Rank(c.id)==0){
    if(c.id!="rage"&&Slots>=5)return;
    ranks[c.id]=1;var primary=AbilitySpecs.Upgrades(c.id)[0];float bonus=Math.Max(0,c.Strength-1);
    if(bonus>0)attributes[c.id]=new Dictionary<string,float>{{primary.key,bonus}};
    return;
   }
   string key=string.IsNullOrEmpty(c.attribute)?AbilitySpecs.Upgrades(c.id)[0].key:c.attribute;
   var d=AbilitySpecs.Definition(c.id,key);if(d==null)return;
   if(!attributes.TryGetValue(c.id,out var stats)){stats=new Dictionary<string,float>();attributes[c.id]=stats;}
   stats[key]=Math.Min(d.cap,Attribute(c.id,key)+((key=="repeats"||key=="projectiles")?(float)Math.Ceiling(c.Strength):c.Strength));ranks[c.id]=Rank(c.id)+1;
  }
  string DescribeAbility(Choice c) {
   if(Rank(c.id)==0){var first=AbilitySpecs.Upgrades(c.id)[0];return c.description+(c.Strength>1?" Enhanced "+first.label.ToLowerInvariant()+": "+first.Effect(c.Strength-1):" Unlocks at base values.");}
   var definitions=AbilitySpecs.Upgrades(c.id);string key=string.IsNullOrEmpty(c.attribute)?definitions[0].key:c.attribute;var d=AbilitySpecs.Definition(c.id,key);
   return d==null?c.description:d.Effect(Math.Min(c.Strength,Math.Max(0,d.cap-Attribute(c.id,key))))+" Only this attribute improves.";
  }
  Choice[] Candidates(bool acquisition) {
   var result=new List<Choice>();foreach(var c in Pool){
    if(c.ability&&Rank(c.id)>0&&!acquisition){foreach(var d in AbilitySpecs.Upgrades(c.id)){var variant=AbilitySpecs.Upgrade(c,d);if(Eligible(variant))result.Add(variant);}}
    else if(Eligible(c)&&(acquisition?c.ability&&Rank(c.id)==0:!c.ability))result.Add(c);
   }return result.ToArray();
  }
 }
}
