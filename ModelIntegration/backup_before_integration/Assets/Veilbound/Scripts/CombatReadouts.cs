using System;
using System.Collections.Generic;
using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  float CritChance=>Mathf.Clamp01(build.Amount("crit")*.05f+city.GearBonus("crit")*.01f);
  float CritMultiplier=>1.5f+build.Amount("critdamage")*.25f+city.GearBonus("critdamage")*.01f;
  float AreaMultiplier=>1+build.Amount("area")*.10f+city.GearBonus("area")*.01f;
  readonly System.Random criticalRandom=new System.Random();
  sealed class AbilityDamageRecord { public float total;public int hits,crits; }
  struct DamageSample {public string source;public float time,damage;}
  sealed class DamageNumber {public Vector3 point;public float amount,age;public bool outgoing,critical;}
  readonly Dictionary<string,AbilityDamageRecord> abilityDamageRecords=new Dictionary<string,AbilityDamageRecord>();
  readonly Queue<DamageSample> damageSamples=new Queue<DamageSample>();
  readonly List<DamageNumber> damageNumbers=new List<DamageNumber>();
  int damageNumberSequence;
  void ResetCombatReadouts(){abilityDamageRecords.Clear();damageSamples.Clear();damageNumbers.Clear();damageNumberSequence=0;}
  float AbilityDamage(string id,float amount=-1) {
   if(id=="rend")return RendDps*RendDuration;if(id=="brand"||id=="warcry")return 0;if(id=="rage")return RageBlastDamage;if(id=="vengeance")return AbilityDamage("cleave");
   float count=amount<0?build.Attribute(id,"damage"):Mathf.Max(0,amount-1);
   float multiplier=1+count*UpgradeStep(id,"damage")*.01f;
   return Damage*multiplier*(id=="orbit"?.6f:id=="smite"?1.8f:id=="bloodthirst"?.8f:1);
  }
  float AbilityCooldown(string id)=>id=="vengeance"?VengeanceCooldown:id=="rage"?0:SkillBaseCooldown(id)/SkillAttackRate(id);
  string AbilityCardDamage(Choice c) {
   if(build.Rank(c.id)>0&&!string.IsNullOrEmpty(c.attribute)) {
    var d=AbilitySpecs.Definition(c.id,c.attribute);if(c.attribute=="damage"){float before=AbilityDamage(c.id),count=build.Attribute(c.id,"damage"),after=before*(1+(count+c.Strength)*d.step*.01f)/(1+count*d.step*.01f);return $"{before:0.#} > {after:0.#} DAMAGE";}return d.label.ToUpperInvariant();
   }
   float extra=Mathf.Max(0,c.Strength-1);
   if(c.id=="rend")return $"{20+5*extra:0.#}% BLEED CHANCE";
   if(c.id=="brand")return $"+{15+5*extra:0.#}% ENEMY DAMAGE TAKEN";if(c.id=="warcry")return $"{20+5*extra:0.#}% LESS ENEMY DAMAGE";if(c.id=="vengeance")return $"{20+5*extra:0.#}% COUNTERATTACK CHANCE";
   float preview=AbilityDamage(c.id)*(1+extra*UpgradeStep(c.id,"damage")*.01f);
   return preview.ToString("0.#")+(c.id=="whirl"?" DAMAGE / CAST":" DAMAGE / HIT");
  }
  string AbilityCardSummary(Choice c) {
   if(build.Rank(c.id)>0)return build.Describe(c);
   switch(c.id){case "rend":return $"Weapon hits bleed for {RendDps:0.#} damage/sec over 3s. Refreshes without stacking. Rarity strengthens chance only.";case "rage":return "+25% damage / +30% Haste rating for 6s. Full Rage required.";case "brand":return "Mark enemies within 4m for 6s. 12s cooldown.";case "warcry":return "Weaken enemies within 4m for 6s. 15s cooldown.";case "bloodthirst":return "Heal 10% of actual damage, capped at 15% max health per cast. 8s cooldown.";case "vengeance":return $"{AbilityDamage("cleave"):0.#} damage per standard swing. 10% triggering-hit protection. {VengeanceCooldown:0.##}s cooldown.";default:return c.Strength>1?"Rarity strengthens damage only. Area and Haste stay at base.":"Base weapon values. Later picks improve one selected attribute.";}
  }
  void RecordAbilityDamage(string source,float amount,bool critical) {
   if(amount<=0||(source!="rage"&&!Array.Exists(RunBuild.Pool,c=>c.ability&&c.id==source)))return;
   if(!abilityDamageRecords.TryGetValue(source,out var record)){record=new AbilityDamageRecord();abilityDamageRecords[source]=record;}
   record.total+=amount;record.hits++;if(critical)record.crits++;
   damageSamples.Enqueue(new DamageSample{source=source,time=elapsed,damage=amount});PruneDamageSamples();
  }
  void PruneDamageSamples(){while(damageSamples.Count>0&&damageSamples.Peek().time<elapsed-5)damageSamples.Dequeue();}
  float RecentDps(string source=null) {PruneDamageSamples();float total=0;foreach(var sample in damageSamples)if(source==null||sample.source==source)total+=sample.damage;return total/Mathf.Clamp(elapsed,.1f,5);}
  float TotalPlayerDamage {get{float total=0;foreach(var item in abilityDamageRecords.Values)total+=item.total;return total;}}
  void AddDamageNumber(Vector3 point,float amount,bool outgoing,bool critical) {
   if(amount<=0)return;if(damageNumbers.Count>=128)damageNumbers.RemoveAt(0);
   int stack=damageNumberSequence++%4;point+=new Vector3((stack-1.5f)*.15f,stack*.09f,0);
   damageNumbers.Add(new DamageNumber{point=point,amount=amount,outgoing=outgoing,critical=critical});
  }
  void UpdateDamageReadouts(float dt) {PruneDamageSamples();for(int i=damageNumbers.Count-1;i>=0;i--){damageNumbers[i].age+=dt;if(damageNumbers[i].age>=1.1f)damageNumbers.RemoveAt(i);}}
  void DrawFloatingDamage() {
   float scale=Mathf.Min(Screen.width/1600f,Screen.height/900f);
   foreach(var number in damageNumbers){var point=cameraRig.WorldToScreenPoint(number.point+Vector3.up*number.age*.85f);if(point.z<=0)continue;
    float x=(point.x-(Screen.width-1600*scale)/2)/scale,y=(Screen.height-point.y-(Screen.height-900*scale)/2)/scale;
    string label=(number.outgoing?"":"-")+number.amount.ToString("0.#")+(number.critical?"!":"");
    var rect=new Rect(x-65,y-20,130,40);float alpha=Mathf.Clamp01((1.1f-number.age)*3);
    var style=CardStyle(number.critical?29:23,new Color(0,0,0,alpha),TextAnchor.MiddleCenter,true);
    GUI.Label(new Rect(rect.x+2,rect.y+2,rect.width,rect.height),label,style);
    style.normal.textColor=number.outgoing?new Color(1,.82f,.18f,alpha):new Color(1,.22f,.18f,alpha);GUI.Label(rect,label,style);
   }
  }
  void AbilityBar(){DrawAbilityLoadout();}
  void CompactDamageMeter() {TopDamageMeter();}
 }
}

