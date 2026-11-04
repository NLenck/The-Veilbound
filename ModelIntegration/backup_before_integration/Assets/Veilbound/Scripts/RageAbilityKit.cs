using System;
using System.Collections.Generic;
using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  const float VengeanceBaseCooldown=1f;
  float brandClock,bloodClock,warCryClock,vengeanceReadyAt;
  bool executingCounter;
  readonly System.Random retaliationRandom=new System.Random();
  sealed class ExtraSwing {public float delay;public string source;}
  readonly List<ExtraSwing> extraSwings=new List<ExtraSwing>();
  float UpgradeStep(string id,string key)=>AbilitySpecs.Definition(id,key)?.step??0;
  float SkillArea(string id)=>AreaMultiplier*(1+build.Attribute(id,"area")*UpgradeStep(id,"area")*.01f);
  float DamageAttribute(string id)=>1+build.Attribute(id,"damage")*UpgradeStep(id,"damage")*.01f;
  float SkillBaseCooldown(string id) {
   float initial=id=="cleave"?1.05f:id=="whirl"?3.1f:id=="lance"?2.1f:id=="orbit"?.45f:id=="smite"?2.5f:id=="brand"?12:id=="bloodthirst"?8:id=="warcry"?15:VengeanceBaseCooldown;
   return initial;
  }
  float SkillRadius(string id)=> (id=="cleave"?3.1f:id=="whirl"?4.1f:id=="orbit"?1.6f:id=="smite"?1.3f:id=="bloodthirst"?3:id=="lance"?1:4)*SkillArea(id);
  float BrandDuration=>6+build.Attribute("brand","duration");
  float BrandBonus=>Mathf.Min(.8f,.15f+.05f*build.Attribute("brand","effect"));
  float WarCryDuration=>6+build.Attribute("warcry","duration");
  float WarCryReduction=>Mathf.Min(.7f,.20f+.05f*build.Attribute("warcry","effect"));
  float BloodHealing=>.10f+.02f*build.Attribute("bloodthirst","healing");
  float BloodHealCap=>MaxHealth*Mathf.Min(.4f,.15f+.025f*build.Attribute("bloodthirst","healcap"));
  float VengeanceChance=>Mathf.Clamp01(.20f+.05f*build.Attribute("vengeance","chance"));
  float VengeanceProtection=>Mathf.Min(.75f,.10f+.05f*build.Attribute("vengeance","guard"));
  float VengeanceCooldown=>SkillBaseCooldown("vengeance")/SkillAttackRate("vengeance");
  void ResetVirtueAbilities(){brandClock=bloodClock=warCryClock=vengeanceReadyAt=0;executingCounter=false;extraSwings.Clear();}
  bool NearbyEnemy(float radius){foreach(var f in foes)if(!f.dead&&Vector3.Distance(position,f.t.position)<=radius+f.radius&&collision.ClearLine(position,f.t.position))return true;return false;}
  void TickVirtueAbilities(float dt) {
   if(state!=ScreenState.Expedition)return;
   TickBleeds(dt);
   for(int i=extraSwings.Count-1;i>=0;i--){var swing=extraSwings[i];swing.delay-=dt;if(swing.delay<=0){extraSwings.RemoveAt(i);PerformBasicSwing(swing.source);}}
   float step=Mathf.Max(0,dt);brandClock-=step*SkillAttackRate("brand");bloodClock-=step*SkillAttackRate("bloodthirst");warCryClock-=step*SkillAttackRate("warcry");
   if(build.Rank("brand")>0&&brandClock<=0&&NearbyEnemy(SkillRadius("brand")))CastRageBrand();
   if(build.Rank("warcry")>0&&warCryClock<=0&&NearbyEnemy(SkillRadius("warcry")))CastWarCry();
   if(build.Rank("bloodthirst")>0&&bloodClock<=0&&NearbyEnemy(SkillRadius("bloodthirst")))CastBloodthirst();
  }
  float RendChance=>Mathf.Clamp01(.20f+.05f*build.Attribute("rend","chance"));
  float RendDuration=>3+build.Attribute("rend","duration");
  float RendDps=>Damage*.20f*DamageAttribute("rend");
  bool TryRend(Foe f,string source,float roll) {
   if(build.Rank("rend")==0||f.dead||f.hp<=0||(!AbilitySpecs.IsWeapon(source)&&source!="vengeance")||roll>=RendChance)return false;
   bool fresh=f.bleedRemaining<=0;f.bleedRemaining=RendDuration;f.bleedDps=fresh?RendDps:Mathf.Max(f.bleedDps,RendDps);
   if(fresh){f.bleedClock=.5f;f.bleedPending=0;PlayerCast("rend",f.t.position+Vector3.up*1.2f,.5f);}return true;
  }
  void TickBleeds(float dt) {
   foreach(var f in foes.ToArray()){
    float remaining=Mathf.Min(Mathf.Max(0,dt),f.bleedRemaining);
    while(remaining>0&&!f.dead&&f.hp>0&&foes.Contains(f)){
     float step=Mathf.Min(remaining,f.bleedClock);remaining-=step;f.bleedRemaining=Mathf.Max(0,f.bleedRemaining-step);f.bleedClock-=step;f.bleedPending+=f.bleedDps*step;
     if(f.bleedClock<=.00001f||f.bleedRemaining<=.00001f){float damage=f.bleedPending;f.bleedPending=0;f.bleedClock=.5f;Hit(f,damage,"rend");}
    }
   }
  }
  void StatusMark(Foe f,bool brand) {
   Transform mark=brand?f.brandMark:f.weakMark;
   if(!mark){mark=WorldArt.Group(brand?"Rage Brand mark":"War Cry weakened",f.t);mark.localPosition=Vector3.up*.10f;WorldArt.Ring(mark,Vector3.zero,f.radius*(brand?1.15f:1.4f),brand?new Color(.90f,.055f,.025f):new Color(.55f,.24f,.13f),.04f,brand?6:3);if(brand)f.brandMark=mark;else f.weakMark=mark;}
   mark.gameObject.SetActive(true);
  }
  void RefreshVirtueMarks(Foe f){if(f.brandMark)f.brandMark.gameObject.SetActive(!f.dead&&f.brandUntil>elapsed);if(f.weakMark)f.weakMark.gameObject.SetActive(!f.dead&&f.weakUntil>elapsed);}
  void CastRageBrand() {
   if(state!=ScreenState.Expedition||build.Rank("brand")==0)return;PlayerCast("brand",position,SkillRadius("brand"));
   foreach(var f in foes)if(!f.dead&&Vector3.Distance(position,f.t.position)<=SkillRadius("brand")+f.radius&&collision.ClearLine(position,f.t.position)){f.brandUntil=elapsed+BrandDuration;f.brandBonus=BrandBonus;StatusMark(f,true);}
   brandClock=SkillBaseCooldown("brand");Burst(position,SkillRadius("brand"),new Color(.8f,.035f,.02f),.45f);
  }
  void CastWarCry() {
   if(state!=ScreenState.Expedition||build.Rank("warcry")==0)return;PlayerCast("warcry",position,SkillRadius("warcry"));
   foreach(var f in foes)if(!f.dead&&Vector3.Distance(position,f.t.position)<=SkillRadius("warcry")+f.radius&&collision.ClearLine(position,f.t.position)){f.weakUntil=elapsed+WarCryDuration;f.weakReduction=WarCryReduction;StatusMark(f,false);}
   warCryClock=SkillBaseCooldown("warcry");Burst(position,SkillRadius("warcry"),new Color(1,.32f,.12f),.5f);
  }
  void CastBloodthirst() {
   if(state!=ScreenState.Expedition||build.Rank("bloodthirst")==0)return;PlayerCast("bloodthirst",position,SkillRadius("bloodthirst"));
   float actual=0;foreach(var f in foes.ToArray())if(!f.dead&&Vector3.Distance(position,f.t.position)<=SkillRadius("bloodthirst")+f.radius&&collision.ClearLine(position,f.t.position))actual+=Hit(f,AbilityDamage("bloodthirst"),"bloodthirst");
   health=Mathf.Min(MaxHealth,health+Mathf.Min(BloodHealCap,actual*BloodHealing));bloodClock=SkillBaseCooldown("bloodthirst");Burst(position,SkillRadius("bloodthirst"),new Color(.75f,.09f,.04f),.4f);
  }
  float EnemyDamageMultiplier(Foe source)=>source!=null&&source.weakUntil>elapsed?1-source.weakReduction:1;
  bool TryVengeance(float roll) {
   if(build.Rank("vengeance")==0||executingCounter||elapsed<vengeanceReadyAt||roll>=VengeanceChance)return false;
   vengeanceReadyAt=elapsed+VengeanceCooldown;return true;
  }
  void Counterattack() {
   if(executingCounter||health<=0)return;executingCounter=true;
   try{ExecuteBasicAttack("vengeance");}finally{executingCounter=false;}
  }
  void PerformBasicSwing(string source) {PlayerCast(source,position,SkillRadius("cleave"));AreaDamage(position,SkillRadius("cleave"),AbilityDamage("cleave"),true,source);Arc(position,SkillRadius("cleave"));swing=1;}
  void ExecuteBasicAttack(string source="cleave") {
   PerformBasicSwing(source);int repeats=Mathf.FloorToInt(build.Attribute("cleave","repeats"));
   for(int i=0;i<repeats;i++)extraSwings.Add(new ExtraSwing{delay=(i+1)*.14f,source=source});
  }
  void ExecuteRiftBlades() {
   PlayerCast("lance",position,SkillArea("lance"));int count=1+Mathf.FloorToInt(build.Attribute("lance","projectiles"));
   for(int i=0;i<count;i++){float degrees=(i-(count-1)*.5f)*8;FireBolt(position+Vector3.up*1.15f+facing*.45f-hero.root.right*.25f,Quaternion.Euler(0,degrees,0)*facing,AbilityDamage("lance"),false);}
  }
  string AbilityEffectText(string id) {
   switch(id) {
    case "rend":return $"{RendChance*100:0.#}% chance on weapon hits to bleed for {RendDps:0.#} damage/sec over {RendDuration:0.#}s ({RendDps*RendDuration:0.#} total before vulnerability). Refreshes without stacking; bleed cannot crit or trigger itself.";
    case "rage":return $"{RageBlastDamage:0.#} damage / {SkillRadius(id):0.#}m explosion. +{RageDamageBonus*100:0.#}% damage and +{RageAttackBonus*100:0.#}% Haste rating for {RageDuration:0.#}s. Requires 100 Rage; no generation while active.";
    case "brand":return $"Enemies within {SkillRadius(id):0.#}m take +{BrandBonus*100:0.#}% damage for {BrandDuration:0.#}s. No explosion; marks refresh without stacking.";
    case "warcry":return $"Enemies within {SkillRadius(id):0.#}m deal {WarCryReduction*100:0.#}% less damage for {WarCryDuration:0.#}s. Weakening refreshes without stacking.";
    case "bloodthirst":return $"{AbilityDamage(id):0.#} damage within {SkillRadius(id):0.#}m. Heal {BloodHealing*100:0.#}% of actual damage dealt, up to {BloodHealCap:0.#} health per cast.";
    case "vengeance":return $"{VengeanceChance*100:0.#}% chance when hit: {VengeanceProtection*100:0.#}% protection on that hit, then your fully upgraded standard attack ({AbilityDamage("cleave"):0.#} damage per swing). {VengeanceCooldown:0.##}s cooldown. Normal attack timing is unchanged.";
    case "lance":return $"{AbilityDamage(id):0.#} damage per piercing blade. {1+Mathf.FloorToInt(build.Attribute(id,"projectiles"))} blade(s) / {SkillArea(id):0.##}× size / {AbilityCooldown(id):0.##}s cooldown.";
    default:return $"{AbilityDamage(id):0.#} damage / {(id=="whirl"?"cast":"hit")} / {SkillRadius(id):0.#}m Area / {AbilityCooldown(id):0.##}s cooldown.";
   }
  }
 }
}
