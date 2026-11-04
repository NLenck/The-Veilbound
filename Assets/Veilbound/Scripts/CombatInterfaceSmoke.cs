using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace Veilbound {
 public partial class VeilGame {
  bool combatReviewFreeze;string reviewAbilityHover;
  Foe ReadoutTarget(Vector3 point,int kind=0) {var f=SpawnFoe(0,kind);Require(f!=null,"Readout test enemy spawns");f.t.position=point;f.hp=f.maxHp=100000;return f;}
  void CheckCombatReadouts() {
   var reviewRandom=criticalRandom;criticalRandom=new NonCriticalReviewRandom();try{
   StartRun();Require(Mathf.Abs(CritChance-.05f)<.001f&&Mathf.Abs(CritMultiplier-1.5f)<.001f&&AreaMultiplier==1,"Baseline critical and area stats are explicit");
   var target=ReadoutTarget(Vector3.forward*2);Hit(target,20,"cleave");Require(Mathf.Abs(abilityDamageRecords["cleave"].total-20)<.001f&&target.hp==99980,"Normal hit damage is attributed to its ability");
   for(int i=0;i<20;i++)build.Pick(TestCard("crit",UpgradeRarity.Legendary));build.Pick(TestCard("critdamage",UpgradeRarity.Legendary));
   Require(CritChance==1&&Mathf.Abs(CritMultiplier-2)<.001f&&!build.Eligible(Array.Find(RunBuild.Pool,c=>c.id=="crit")),"Critical chance caps at 100% and capped offers stop appearing");
   Hit(target,10,"lance");Require(abilityDamageRecords["lance"].total==20&&abilityDamageRecords["lance"].crits==1&&damageNumbers[damageNumbers.Count-1].critical,"Critical hit applies its multiplier and records its source");
   target.hp=3;Hit(target,100,"orbit");Require(abilityDamageRecords["orbit"].total==3,"Effective damage excludes overkill");
   float before=health;invulnerability=0;int count=damageNumbers.Count;Hurt(20);Require(damageNumbers.Count==count+1&&!damageNumbers[count].outgoing&&Mathf.Abs(damageNumbers[count].amount-(before-health))<.001f,"Red negative popup uses the actual damage after protection");
   Hurt(20);Require(damageNumbers.Count==count+1,"Invulnerable hits create neither damage nor misleading red numbers");
   target=ReadoutTarget(Vector3.forward*3);float total=TotalPlayerDamage;count=damageNumbers.Count;Hit(target,10,"volatile",false);Require(target.hp==99990&&TotalPlayerDamage==total&&damageNumbers.Count==count,"Enemy explosions are excluded from player damage, crits and DPS");
   StartRun();target=ReadoutTarget(Vector3.forward*2);for(int i=0;i<5;i++)attacks[i]=100;
   attacks[0]=0;Attack(0);Require(Mathf.Abs(abilityDamageRecords["cleave"].total-AbilityDamage("cleave"))<.01f,"Basic swing preview agrees with actual combat");
   build.Pick(TestCard("whirl",UpgradeRarity.Rare));attacks[1]=0;Attack(0);UpdateCombatWeapons(.31f);UpdateCombatWeapons(.31f);
   Require(abilityDamageRecords["whirl"].hits==3&&Mathf.Abs(abilityDamageRecords["whirl"].total-AbilityDamage("whirl"))<.01f,"Three spin passes sum to the cast damage shown on cards");
   build.Pick(TestCard("orbit",UpgradeRarity.Common));attacks[3]=0;Attack(0);Require(Mathf.Abs(abilityDamageRecords["orbit"].total-AbilityDamage("orbit"))<.01f,"Orbit hits have their own DPS source");
   build.Pick(TestCard("smite",UpgradeRarity.Legendary));attacks[4]=0;Attack(0);UpdateHazards(.51f);Require(Mathf.Abs(abilityDamageRecords["smite"].total-AbilityDamage("smite"))<.01f,"Delayed strikes retain their source and preview damage");
   build.Pick(TestCard("lance",UpgradeRarity.Common));FireBolt(target.t.position+Vector3.up*.7f,Vector3.forward,AbilityDamage("lance"),false);UpdateBolts(0);Require(Mathf.Abs(abilityDamageRecords["lance"].total-AbilityDamage("lance"))<.01f,"Piercing projectile damage is attributed to Rift edge");
   build.Pick(TestCard("area",UpgradeRarity.Common));Require(Mathf.Abs(AreaMultiplier-1.1f)<.001f,"Area passive increases the actual radius");// Keep the outside-old-radius target in clear line of sight on every randomized map.
   float unscaledRadius=SkillRadius("whirl")/SkillArea("whirl"),testDistance=unscaledRadius+target.radius+.05f;
   Debug.Log("AREA_QA_ORIGINAL_LINE_CLEAR "+collision.ClearLine(position,position+Vector3.forward*testDistance));
   bool clearTestTarget=false;for(int direction=0;direction<64;direction++){Vector3 point=position+Quaternion.Euler(0,direction*360f/64,0)*Vector3.forward*testDistance;if(!collision.ClearLine(position,point))continue;target.t.position=point;clearTestTarget=true;break;}
   Require(clearTestTarget,"Area fixture has an unobstructed target outside the unscaled radius");float hp=target.hp;BeginWeaponSweep(30);UpdateCombatWeapons(.01f);Require(target.hp<hp&&Mathf.Abs(weaponSweeps[weaponSweeps.Count-1].root.localScale.x-1.1f)<.001f,"Area scaling reaches targets outside the old spin radius and scales its visual");
   total=TotalPlayerDamage;elapsed=6;Require(RecentDps()==0&&TotalPlayerDamage==total,"Recent DPS expires after five combat seconds while run totals remain");
   StartRun();for(int i=0;i<3;i++)build.Pick(TestCard("power",UpgradeRarity.Common));build.Pick(TestCard("power",UpgradeRarity.Rare));Require(build.passivePicks["power"][0]==3&&build.passivePicks["power"][2]==1&&Mathf.Abs(build.Amount("power")-4.5f)<.001f,"Passive history groups exact rarity picks independently of weighted potency");
   StartRun();Require(build.passivePicks.Count==0&&TotalPlayerDamage==0&&damageNumbers.Count==0,"New runs clear history, floating numbers and DPS");
   Debug.Log("VEILBOUND_COMBAT_READOUTS_LOGIC_PASS");
  
   }finally{criticalRandom=reviewRandom;}
  }
  IEnumerator CombatInterfaceReview(string folder) {
   Directory.CreateDirectory(folder);CheckCombatReadouts();StartRun();combatReviewFreeze=true;
   foreach(var id in new[]{"whirl","orbit","smite"})build.Pick(TestCard(id,UpgradeRarity.Rare));
   foreach(var id in new[]{"power","speed","vitality","regen","guard","haste","wisdom","reach","luck","crit","critdamage","area"})build.Pick(TestCard(id,UpgradeRarity.Rare));
   build.Pick(TestCard("power",UpgradeRarity.Common));build.Pick(TestCard("power",UpgradeRarity.Common));build.Pick(TestCard("power",UpgradeRarity.Epic));build.Pick(TestCard("regen",UpgradeRarity.Legendary));
   level=20;elapsed=65;health=MaxHealth;hero.root.position=position;hero.root.rotation=Quaternion.identity;
   var targets=new[]{ReadoutTarget(new Vector3(-3,0,3)),ReadoutTarget(new Vector3(0,0,4),2),ReadoutTarget(new Vector3(3,0,3),1)};
   string[] sources={"cleave","whirl","orbit","smite"};for(int i=0;i<24;i++)Hit(targets[i%3],AbilityDamage(sources[i%4])/(sources[i%4]=="whirl"?3:1),sources[i%4]);
   damageNumbers.Clear();for(int i=0;i<3;i++)Hit(targets[i],AbilityDamage(sources[i])/(sources[i]=="whirl"?3:1),sources[i]);invulnerability=0;Hurt(35);
   cameraRig.transform.position=new Vector3(7,13,-13);cameraRig.transform.LookAt(new Vector3(0,0,2));cameraRig.orthographicSize=9;
   yield return new WaitForSecondsRealtime(.2f);yield return CardScreenshot(Path.Combine(folder,"01-combat-hud.png"));
   reviewAbilityHover="whirl";yield return CardScreenshot(Path.Combine(folder,"02-ability-tooltip.png"));reviewAbilityHover=null;
   var keyboard=InputSystem.AddDevice<Keyboard>();InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.C));InputSystem.Update();ReadCharacterSheetInput();InputSystem.RemoveDevice(keyboard);Require(state==ScreenState.Character&&characterTab==0,"Character hotkey opens the paused stat interface");
   double clock=city.hours;float timer=elapsed;yield return new WaitForSecondsRealtime(.3f);Require(city.hours==clock&&elapsed==timer,"Character sheet pauses both clocks");yield return CardScreenshot(Path.Combine(folder,"03-character-stats.png"));
   characterTab=1;selection=1;yield return CardScreenshot(Path.Combine(folder,"04-passive-history.png"));characterTab=2;selection=2;yield return CardScreenshot(Path.Combine(folder,"05-damage-breakdown.png"));CloseCharacterSheet();Require(state==ScreenState.Expedition,"Closing the character sheet returns to the prior state");
   SetTestCards(21,TestCard("crit",UpgradeRarity.Rare),TestCard("critdamage",UpgradeRarity.Epic),TestCard("area",UpgradeRarity.Legendary));yield return new WaitForSecondsRealtime(.5f);yield return CardScreenshot(Path.Combine(folder,"06-new-passive-cards.png"));
   build=new RunBuild();SetTestCards(2,TestCard("whirl",UpgradeRarity.Rare),TestCard("orbit",UpgradeRarity.Epic),TestCard("smite",UpgradeRarity.Legendary));yield return new WaitForSecondsRealtime(.5f);yield return CardScreenshot(Path.Combine(folder,"07-ability-damage-cards.png"));
   combatReviewFreeze=false;ReturnHome();File.WriteAllText(Path.Combine(folder,"PASS.txt"),"PASS: baseline and capped crits; rarity-scaled critical damage and area; all five damage sources; spin pulse totals; projectile and delayed hit attribution; overkill exclusion; protected/invulnerable player hits; exact passive-rarity counts; DPS expiry and run reset; character sheet hotkey and paused clocks; actual HUD, tooltip and sheet captures.\n");Debug.Log("VEILBOUND_COMBAT_INTERFACE_PASS");
  }
 }
}

