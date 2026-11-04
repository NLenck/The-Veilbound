using System;
using System.IO;
using System.Collections;
using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  Choice KitUpgrade(string id,string attribute,UpgradeRarity rarity=UpgradeRarity.Common)=>AbilitySpecs.Upgrade(Array.Find(RunBuild.Pool,c=>c.id==id),AbilitySpecs.Definition(id,attribute),rarity);
  void UnlockKit(string id){if(build.Rank(id)==0)build.Pick(TestCard(id,UpgradeRarity.Common));}
  void CheckRageAbilityKit() {
   var reviewRandom=criticalRandom;criticalRandom=new NonCriticalReviewRandom();try{
   StartRun();Require(build.WeaponSlots==1&&build.VirtueSlots==0&&build.Slots==1&&build.Rank("rage")==1,"The basic weapon occupies one equipped slot; active Fury is outside the limit");
   float damage=AbilityDamage("cleave"),radius=SkillRadius("cleave"),cooldown=AbilityCooldown("cleave");
   build.Pick(KitUpgrade("cleave","damage",UpgradeRarity.Rare));Require(Mathf.Abs(AbilityDamage("cleave")/damage-1.30f)<.001f&&SkillRadius("cleave")==radius&&AbilityCooldown("cleave")==cooldown,"Damage selection changes damage alone, with rarity strength");
   damage=AbilityDamage("cleave");build.Pick(KitUpgrade("cleave","area"));Require(AbilityDamage("cleave")==damage&&Mathf.Abs(SkillRadius("cleave")/radius-1.08f)<.001f&&AbilityCooldown("cleave")==cooldown,"Radius selection does not raise damage or change cadence");
   radius=SkillRadius("cleave");build.Pick(KitUpgrade("cleave","haste"));Require(AbilityDamage("cleave")==damage&&SkillRadius("cleave")==radius&&AbilityCooldown("cleave")<cooldown,"Cooldown selection changes cadence alone");
   foreach(string id in AbilitySpecs.Weapons)UnlockKit(id);
   foreach(string id in AbilitySpecs.Virtues)UnlockKit(id);
   Require(build.Slots==5&&build.WeaponSlots==5&&build.VirtueSlots==0,"Five weapon picks fill the shared limit and block a sixth ability");
   rage=100;Require(ActivateRage()&&build.Slots==5,"Active Fury works without occupying a sixth equipped slot");TickRage(RageDuration);
   StartRun();foreach(string id in new[]{"brand","bloodthirst","vengeance","warcry"})UnlockKit(id);
   Require(build.Slots==5&&build.WeaponSlots==1&&build.VirtueSlots==4,"Weapon and Virtue choices share the same five equipped slots");
   StartRun();UnlockKit("vengeance");
   float chance=VengeanceChance,guard=VengeanceProtection,cd=VengeanceCooldown;int rank=build.Rank("vengeance");
   build.Pick(KitUpgrade("vengeance","chance"));Require(Mathf.Abs(VengeanceChance-chance-.05f)<.001f&&VengeanceProtection==guard&&VengeanceCooldown==cd&&build.Rank("vengeance")==rank+1,"Retaliation chance investment does not grant hidden protection or cooldown scaling");
   chance=VengeanceChance;build.Pick(KitUpgrade("vengeance","guard"));Require(VengeanceChance==chance&&Mathf.Abs(VengeanceProtection-guard-.05f)<.001f&&VengeanceCooldown==cd,"Protection upgrade changes only the triggering-hit reduction");
   guard=VengeanceProtection;build.Pick(KitUpgrade("vengeance","haste"));Require(VengeanceChance==chance&&VengeanceProtection==guard&&VengeanceCooldown<cd,"Cooldown upgrade changes only retaliation cooldown");
   StartRun();UnlockKit("brand");var marked=ReadoutTarget(Vector3.forward*2);var outside=ReadoutTarget(Vector3.forward*9);float hp=marked.hp;
   CastRageBrand();Require(marked.hp==hp&&marked.brandUntil>elapsed&&outside.brandUntil==0,"Rage Brand marks enemies in range without causing explosive damage");
   Hit(marked,20,"cleave");Require(Mathf.Abs(hp-marked.hp-23)<.01f,"Brand amplifies player weapon damage");hp=marked.hp;Hit(marked,20,"volatile",false);Require(Mathf.Abs(hp-marked.hp-20)<.01f,"Brand does not amplify enemy-on-enemy damage");
   elapsed=BrandDuration+.01f;hp=marked.hp;Hit(marked,20,"cleave");Require(Mathf.Abs(hp-marked.hp-20)<.01f,"The vulnerability expires without a detonation");
   StartRun();UnlockKit("warcry");var weak=ReadoutTarget(Vector3.forward*2);CastWarCry();health=MaxHealth;invulnerability=0;hp=health;Hurt(20,weak);float baseline=20*(1-city.Protection);
   Require(Mathf.Abs(hp-health-baseline*.8f)<.01f,"War Cry reduces source-aware incoming damage by 20%");
   invulnerability=0;hp=health;Telegraph(position,2,.1f,20,owner:weak);UpdateHazards(.11f);Require(Mathf.Abs(hp-health-baseline*.8f)<.01f,"Boss telegraphs retain the weakened source through impact");
   invulnerability=0;hp=health;LobArea(weak.t.position,position,20,weak);UpdateLobs(1.5f);Require(Mathf.Abs(hp-health-baseline*.8f)<.01f,"Lobbed attacks retain the weakened source through impact");
   elapsed=WarCryDuration+.01f;invulnerability=0;hp=health;Hurt(20,weak);Require(Mathf.Abs(hp-health-baseline)<.01f,"Weaken expires on the combat clock");
   StartRun();UnlockKit("bloodthirst");var a=ReadoutTarget(Vector3.forward*2);var b=ReadoutTarget(new Vector3(1,0,2));a.hp=3;b.hp=5;health=MaxHealth-30;hp=health;CastBloodthirst();
   Require(Mathf.Abs(health-hp-.8f)<.01f&&Mathf.Abs(abilityDamageRecords["bloodthirst"].total-8)<.01f,"Bloodthirst heals from eight actual damage, excluding overkill");
   for(int i=0;i<8;i++)build.Pick(KitUpgrade("bloodthirst","damage",UpgradeRarity.Legendary));for(int i=0;i<10;i++)ReadoutTarget(new Vector3((i%3-1)*.6f,0,1.5f+i/3*.1f));health=MaxHealth*.1f;hp=health;CastBloodthirst();
   Require(Mathf.Abs(health-hp-BloodHealCap)<.02f,"Bloodthirst respects its per-activation healing cap even against a crowd");
   StartRun();UnlockKit("vengeance");for(int i=0;i<16;i++)build.Pick(KitUpgrade("vengeance","chance"));
   build.Pick(KitUpgrade("cleave","damage",UpgradeRarity.Rare));build.Pick(KitUpgrade("cleave","area"));build.Pick(KitUpgrade("cleave","repeats",UpgradeRarity.Rare));
   a=ReadoutTarget(Vector3.forward*2);b=ReadoutTarget(Vector3.back*2);hp=a.hp;float behind=b.hp;attacks[0]=.73f;float scheduled=attacks[0];health=MaxHealth;float oldHealth=health;invulnerability=0;Hurt(20,a);
   Require(Mathf.Abs(a.hp-hp+AbilityDamage("cleave"))<.02f&&b.hp==behind&&attacks[0]==scheduled,"Vengeance uses the modified directional basic attack and leaves the normal attack timer untouched");
   Require(Mathf.Abs(oldHealth-health-20*(1-city.Protection)*(1-VengeanceProtection))<.01f,"A successful retaliation reduces only its triggering hit");
   TickVirtueAbilities(.15f);Require(abilityDamageRecords["vengeance"].hits==2&&Mathf.Abs(abilityDamageRecords["vengeance"].total-2*AbilityDamage("cleave"))<.02f,"Counterattacks reuse extra swings and attribute their actual damage to Vengeance");
   oldHealth=health;invulnerability=0;Hurt(20,a);Require(Mathf.Abs(oldHealth-health-20*(1-city.Protection))<.01f&&abilityDamageRecords["vengeance"].hits==2,"No retaliation or triggering-hit protection occurs while Vengeance is cooling down");
   elapsed+=VengeanceCooldown+.01f;executingCounter=true;Require(!TryVengeance(0),"Recursive counterattack activation is blocked");executingCounter=false;
   UnlockKit("brand");brandClock=0;warCryClock=bloodClock=100;CastRageBrand();float normalRate=AttackRate;rage=100;Require(ActivateRage(),"Fury remains the full-resource active ability");float active=rage;Hit(a,20,"vengeance");Require(rage==active&&Mathf.Abs(AttackRate-1/(1-HasteRules.Reduction(GlobalHasteRating)))<.001f,"Vengeance also respects Fury's no-generation window and 30% attack speed baseline");
   TickRage(6);Require(!RageActive&&rage==0,"Fury returns to zero at the end of its six-second base duration");
   StartRun();Require(extraSwings.Count==0&&brandClock==0&&bloodClock==0&&vengeanceReadyAt==0&&abilityDamageRecords.Count==0,"A fresh run resets queued swings, Virtue timers and damage records");
   CheckHaste();
   CheckRend();
   Debug.Log("VEILBOUND_RAGE_ABILITY_KIT_LOGIC_PASS");
  
   }finally{criticalRandom=reviewRandom;}
  }
  void CheckHaste() {
   float[] ratings={0,40,100,160,280,520,1000};float[] reductions={0,.4f,.6f,.7f,.8f,.9f,.95f};
   for(int i=0;i<ratings.Length;i++)Require(Mathf.Abs(HasteRules.Reduction(ratings[i])-reductions[i])<.00001f,"Haste breakpoint "+ratings[i]+" rating");
   Require(Mathf.Abs(HasteRules.Reduction(43)-.41f)<.00001f&&Mathf.Abs(HasteRules.Reduction(106)-.61f)<.00001f,"Marginal Haste follows 3:1 and then 6:1 brackets");
   float previous=0;for(int i=0;i<10000;i++){float value=HasteRules.Reduction(i);Require(value>=previous&&value<1,"Haste is monotonic and never reaches a zero interval");previous=value;}
   StartRun();UnlockKit("brand");UnlockKit("vengeance");float baseDamage=AbilityDamage("cleave"),baseArea=SkillArea("cleave");
   for(int i=0;i<20;i++)build.Pick(TestCard("haste",UpgradeRarity.Common));
   Require(Mathf.Abs(GlobalHasteRating-40)<.001f&&Mathf.Abs(AbilityCooldown("cleave")-2f*WeaponCooldownMultiplier*.6f)<.001f,"40 global rating reduces timers by 40%");
   build.Pick(KitUpgrade("cleave","haste"));Require(Mathf.Abs(AbilityCooldown("cleave")-2f*WeaponCooldownMultiplier*(1-HasteRules.Reduction(48)))<.001f&&AbilityDamage("cleave")==baseDamage&&SkillArea("cleave")==baseArea,"Local and global Haste share one curve without improving damage or Area");
   Require(Mathf.Abs(AbilityCooldown("brand")-5*WeaponCooldownMultiplier*.6f)<.001f&&Mathf.Abs(VengeanceCooldown-WeaponCooldownMultiplier*.6f)<.001f,"Automatic spells and Vengeance use the same global Haste");
   float remaining=SkillBaseCooldown("cleave");attacks[0]=remaining;for(int i=1;i<5;i++)attacks[i]=100;float dt=AbilityCooldown("cleave")*.5f;Attack(dt);
   Require(Mathf.Abs(attacks[0]-remaining*.5f)<.001f,"Actual weapon timers match the displayed effective interval");
   brandClock=5;bloodClock=warCryClock=100;TickVirtueAbilities(AbilityCooldown("brand")*.5f);Require(Mathf.Abs(brandClock-2.5f)<.001f,"Actual Virtue timers match the Haste preview");
   rage=100;ActivateRage();Require(Mathf.Abs(GlobalHasteRating-70)<.001f&&Mathf.Abs(AbilityCooldown("cleave")-2f*WeaponCooldownMultiplier*(1-HasteRules.Reduction(78)))<.001f,"Fury adds its rating before the shared diminishing-return curve");
   StartRun();Debug.Log("VEILBOUND_HASTE_PASS");
  }
  void CheckRend() {
   Require(AbilitySpecs.Weapons.Length==5&&AbilitySpecs.Virtues.Length==5,"Five weapon and five selectable Virtue abilities are defined");
   StartRun();UnlockKit("rend");Require(build.Slots==2&&build.VirtueSlots==1,"Rend occupies one shared slot");
   var target=ReadoutTarget(Vector3.forward*2);target.hp=target.maxHp=100000;float hp=target.hp;float dps=RendDps;
   Require(!TryRend(target,"cleave",.21f)&&TryRend(target,"cleave",0),"Rend base proc chance is 20%");
   Require(!TryRend(target,"rend",0)&&!TryRend(target,"rage",0)&&!TryRend(target,"bloodthirst",0),"Bleed does not recurse or proc from non-weapon spells");
   TickBleeds(3);Require(Mathf.Abs(hp-target.hp-dps*3)<.02f&&target.bleedRemaining==0,"Bleed deals exactly three seconds of damage and expires even with a large frame step");
   hp=target.hp;TryRend(target,"vengeance",0);TickBleeds(.25f);TryRend(target,"cleave",0);TickBleeds(.25f);
   Require(Mathf.Abs(hp-target.hp-dps*.5f)<.02f,"Vengeance can apply Rend; reapplications preserve tick timing and do not stack damage");
   TickBleeds(10);float chance=RendChance,duration=RendDuration;build.Pick(KitUpgrade("rend","damage",UpgradeRarity.Rare));
   Require(Mathf.Abs(RendDps/dps-1.375f)<.001f&&RendChance==chance&&RendDuration==duration,"Rare Rend damage upgrade changes damage alone by 37.5%");
   dps=RendDps;build.Pick(KitUpgrade("rend","chance"));Require(Mathf.Abs(RendChance-chance-.05f)<.001f&&RendDps==dps&&RendDuration==duration,"Rend chance upgrade is independent");
   build.Pick(KitUpgrade("rend","duration"));Require(RendDuration==duration+1&&RendDps==dps,"Duration extends bleeding without changing DPS");
   build.Pick(TestCard("crit",UpgradeRarity.Legendary));for(int i=0;i<19;i++)build.Pick(TestCard("crit",UpgradeRarity.Legendary));
   TryRend(target,"cleave",0);hp=target.hp;TickBleeds(RendDuration);Require(Mathf.Abs(hp-target.hp-dps*RendDuration)<.05f&&abilityDamageRecords["rend"].crits==0,"Bleed cannot critically hit even at 100% crit chance and is tracked separately");
   StartRun();Require(!abilityDamageRecords.ContainsKey("rend")&&foes.Count==0,"New run clears bleeds and their damage record");
   Debug.Log("VEILBOUND_REND_PASS");
  }
  IEnumerator RageKitReview() {
   string folder=Path.Combine(Application.dataPath,"..","Smoke","RageAbilityKit");Directory.CreateDirectory(folder);string pass=Path.Combine(folder,"PASS.txt");if(File.Exists(pass))File.Delete(pass);
   yield return new WaitForSecondsRealtime(.5f);ReturnHome();combatReviewFreeze=true;
   city=new Settlement{blacksmith=3,bronzeHeroVersion=1};city.EnsureGear();for(int s=0;s<8;s++){city.ownedPlate[s*10+2]=true;city.equippedGear[s]=102;}city.ownedGear[24]=1;city.equippedGear[8]=11;city.equippedMetalWeapon=2;city.ownedMetalWeapons[2]=true;city.EnsureGearRolls();RebuildHero();
   CheckRageAbilityKit();StartRun();foreach(string id in new[]{"rend","brand","bloodthirst","vengeance"})UnlockKit(id);build.Pick(TestCard("power",UpgradeRarity.Common));
   level=18;elapsed=65;health=MaxHealth*.6f;xp=Threshold*.65f;hero.root.position=position;hero.root.rotation=Quaternion.identity;hero.Animate(0,0,0);
   var targets=new[]{ReadoutTarget(new Vector3(-2,0,2)),ReadoutTarget(new Vector3(0,0,3)),ReadoutTarget(new Vector3(2,0,2))};CastRageBrand();CastWarCry();CastBloodthirst();
   foreach(string id in new[]{"cleave","whirl","bloodthirst","vengeance"})foreach(var target in targets)Hit(target,AbilityDamage(id),id);
   foreach(var target in targets)TryRend(target,"cleave",0);TickBleeds(1);
   damageNumbers.Clear();foreach(var target in targets){Hit(target,AbilityDamage("cleave"),"cleave");target.rig.Animate(0,0,.2f);}UpdateFlashes(1);
   cameraRig.transform.position=new Vector3(7,13,-13);cameraRig.transform.LookAt(new Vector3(0,0,2));cameraRig.orthographicSize=9;rage=100;
   yield return CardScreenshot(Path.Combine(folder,"01-five-ability-hud.png"));reviewAbilityHover="brand";yield return CardScreenshot(Path.Combine(folder,"02-rage-brand-tooltip.png"));reviewAbilityHover="vengeance";yield return CardScreenshot(Path.Combine(folder,"03-vengeance-tooltip.png"));reviewAbilityHover="rend";yield return CardScreenshot(Path.Combine(folder,"09-rend-tooltip.png"));reviewAbilityHover=null;
   ActivateRage();TickRage(2);yield return CardScreenshot(Path.Combine(folder,"04-fury-active.png"));
   SetTestCards(19,KitUpgrade("vengeance","chance"),KitUpgrade("vengeance","guard",UpgradeRarity.Rare),KitUpgrade("vengeance","haste",UpgradeRarity.Epic));yield return new WaitForSecondsRealtime(.25f);yield return CardScreenshot(Path.Combine(folder,"05-independent-attribute-cards.png"));
   float protection=VengeanceProtection;int rank=build.Rank("vengeance");buttons[0]();Require(build.Rank("vengeance")==rank+1&&VengeanceProtection==protection,"Selecting an attribute card increments investment level without granting another attribute");
   OpenCharacterSheet(0);yield return CardScreenshot(Path.Combine(folder,"06-character-stats.png"));characterTab=2;yield return CardScreenshot(Path.Combine(folder,"07-damage-breakdown.png"));CloseCharacterSheet();
   build=new RunBuild();SetTestCards(2,TestCard("brand",UpgradeRarity.Common),TestCard("bloodthirst",UpgradeRarity.Rare),TestCard("rend",UpgradeRarity.Epic));yield return new WaitForSecondsRealtime(.25f);yield return CardScreenshot(Path.Combine(folder,"08-new-virtue-cards.png"));
   SetTestCards(20,KitUpgrade("cleave","area"),KitUpgrade("cleave","haste",UpgradeRarity.Rare),KitUpgrade("brand","haste",UpgradeRarity.Legendary));yield return new WaitForSecondsRealtime(.25f);yield return CardScreenshot(Path.Combine(folder,"10-area-haste-cards.png"));
   File.WriteAllText(pass,"PASS: Rend proc, tick damage, refresh, independent rarity upgrades, no recursive proc or crit, reset; five equipped abilities from the weapon/Virtue pool; active Fury excluded; independent attribute and rarity scaling; vulnerability/weakness expiry and source routing; actual-damage healing and activation cap; fully modified directional counterattack, extra swings, independent timer, recursion guard and triggering-hit-only protection; Fury generation gate/drain; run reset; selectable attribute cards; actual HUD/tooltip/stats/damage screenshots. Isolated test save.");Debug.Log("VEILBOUND_RAGE_ABILITY_KIT_PASS");Application.Quit(0);
  }
 }
}
