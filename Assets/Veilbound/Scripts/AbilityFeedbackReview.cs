using System;using System.IO;using System.Collections;using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  IEnumerator AbilityFeedbackReview(){
   var reviewRandom=criticalRandom;criticalRandom=new NonCriticalReviewRandom();try{
   yield return null;combatReviewFreeze=true;
   CheckRageAbilityKit();CheckRend();
   PressureStart(1,1);foreach(string id in new[]{"whirl","brand","rend","vengeance"})UnlockKit(id);
   Require(Mathf.Abs(RendDps/Damage-.35f)<.0001f&&Mathf.Abs(AbilityDamage("whirl")/Damage-.8f)<.0001f,"Requested damage changes use weapon scaling");
   for(int i=0;i<5;i++){string id=AbilitySpecs.Weapons[i];attacks[i]=SkillBaseCooldown(id)*.5f;Require(Mathf.Abs(CooldownSeconds(id)/AbilityCooldown(id)-.5f)<.001f,"All five weapon cooldowns read live timers");}
   brandClock=2.5f;bloodClock=4;warCryClock=7.5f;vengeanceReadyAt=elapsed+VengeanceCooldown*.5f;
   foreach(string id in new[]{"brand","bloodthirst","warcry","vengeance"})Require(Mathf.Abs(CooldownSeconds(id)/AbilityCooldown(id)-.5f)<.001f,"Virtue cooldowns read automatic and reactive timers");
   build.Pick(TestCard("haste",UpgradeRarity.Common));Require(Mathf.Abs(CooldownSeconds("brand")/AbilityCooldown("brand")-.5f)<.001f,"Haste changes seconds without breaking sweep progress");
   attacks[0]=-1;Require(CooldownSeconds("cleave")==0&&CooldownSeconds("rend")==0,"Ready and passive abilities have no cooldown mask");
   Require(CooldownMaskAlpha(.9f,.5f,1)>0&&CooldownMaskAlpha(.9f,.5f,0)==0&&CooldownMaskAlpha(.9f,.5f,.5f)==0&&CooldownMaskAlpha(.1f,.5f,.5f)>0,"Mask clears clockwise from twelve o'clock");
   Foe a=ReadoutTarget(new Vector3(-2,0,2)),b=ReadoutTarget(new Vector3(0,0,3),2),c=ReadoutTarget(new Vector3(2,0,2),1);
   CastRageBrand();foreach(var f in new[]{a,b,c}){Require(TryRend(f,"cleave",0)&&DebuffActive(f,"brand")&&DebuffActive(f,"rend"),"Brand and bleed display concurrently");Require(DebuffAnchor(f).y>f.t.position.y+1,"Status sits above model");}
   float hp=a.hp;TickBleeds(.5f);Require(Mathf.Abs(hp-a.hp-RendDps*.5f*(1+BrandBonus))<.02f,"Actual bleed tick uses new damage and vulnerability");
   float damageBefore=a.hp;BeginWeaponSweep(AbilityDamage("whirl"));UpdateCombatWeapons(.01f);UpdateCombatWeapons(.31f);UpdateCombatWeapons(.31f);
   Require(Mathf.Abs(damageBefore-a.hp-AbilityDamage("whirl")*(1+BrandBonus))<.02f,"All three sweep passes share the reduced damage budget");
   a.bleedRemaining=0;Require(!DebuffActive(a,"rend")&&DebuffActive(a,"brand"),"Bleed expiry removes only its own icon");TryRend(a,"cleave",0);float saved=elapsed;elapsed=a.brandUntil;Require(!DebuffActive(a,"brand"),"Brand icon expires exactly with debuff");elapsed=saved;
   a.dead=true;Require(!DebuffActive(a,"rend")&&!DebuffActive(a,"brand"),"Death hides both icons immediately");a.dead=false;
   string folder=Path.Combine(Application.dataPath,"..","Smoke","AbilityFeedback");Directory.CreateDirectory(folder);
   UpdateFlashes(5);damageNumbers.Clear();TownCamera(Vector3.zero,8);cameraRig.transform.position=new Vector3(0,16,-13);cameraRig.transform.LookAt(new Vector3(0,0,2));hero.root.position=position;
   for(int i=0;i<5;i++)attacks[i]=SkillBaseCooldown(AbilitySpecs.Weapons[i])*.65f;brandClock=3.75f;vengeanceReadyAt=elapsed+.6f;dodgeCooldown=1.4f;rage=100;
   yield return CardScreenshot(Path.Combine(folder,"01-cooldowns-and-debuffs.png"));
   build=new RunBuild();foreach(string id in new[]{"whirl","lance","orbit","smite"})UnlockKit(id);
   for(int i=0;i<5;i++)attacks[i]=SkillBaseCooldown(AbilitySpecs.Weapons[i])*(.1f+i*.2f);
   yield return CardScreenshot(Path.Combine(folder,"02-weapon-cooldown-phases.png"));
   build=new RunBuild();foreach(string id in new[]{"brand","bloodthirst","vengeance","warcry"})UnlockKit(id);brandClock=3.75f;bloodClock=4;warCryClock=3;vengeanceReadyAt=elapsed+.8f;
   yield return CardScreenshot(Path.Combine(folder,"03-virtue-cooldowns.png"));
   File.WriteAllText(Path.Combine(folder,"PASS.txt"),"PASS: existing Rage kit and Rend regression checks; live cooldowns for five weapons and four timed virtue skills; Haste; clockwise mask; passive and ready state; simultaneous status icons; expiry/death; actual new Rend ticks and three-pass Reaping Circle damage budget; real HUD screenshots. Isolated smoke profile.");
   Debug.Log("VEILBOUND_ABILITY_FEEDBACK_PASS");Application.Quit(0);
  
   }finally{criticalRandom=reviewRandom;}
  }
 }
}
