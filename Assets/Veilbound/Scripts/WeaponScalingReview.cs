using System;using System.IO;using System.Linq;using System.Collections;using System.Collections.Generic;using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  [Serializable]class WeaponAbilityAudit{public bool passed;public int profiles,damageChecks,cooldownChecks,timerChecks;public bool rageRegression,kitRegression,resourceRegression,saveRoundtrip;}
  IEnumerator WeaponScalingReview(){
   var reviewRandom=criticalRandom;criticalRandom=new NonCriticalReviewRandom();try{
   yield return null;combatReviewFreeze=true;city=FreshCharacterTown();var audit=new WeaponAbilityAudit();
   foreach(int id in Enumerable.Range(-1,12)){
    city.equippedMetalWeapon=id<0?-2:id;RebuildHero();StartRun();
    Require(Mathf.Abs(Damage-city.WeaponDamage)<.001f,"Clean ability basis equals weapon damage");float beforeSmith=Damage;city.blacksmith=9;Require(Damage==beforeSmith,"Workshop level does not add hidden flat damage");city.blacksmith=0;
    foreach(string skill in new[]{"cleave","whirl","lance","orbit","smite","bloodthirst","rage","rend"}){
     float expected=city.WeaponDamage*WeaponRules.Coefficient(skill)*(skill=="rend"?RendDuration:1);Require(Mathf.Abs(AbilityDamage(skill)-expected)<.001f,skill+" derives from weapon coefficient for profile "+id);audit.damageChecks++;
    }
    foreach(string skill in new[]{"cleave","whirl","lance","orbit","smite","bloodthirst","brand","warcry","vengeance"}){Require(Mathf.Abs(AbilityCooldown(skill)-WeaponRules.BaseCooldown(skill)*city.WeaponInterval/2)<.001f,skill+" cooldown tracks weapon interval");audit.cooldownChecks++;}
    for(int i=0;i<5;i++)attacks[i]=100;attacks[0]=2;Attack(AbilityCooldown("cleave")*.5f);Require(Mathf.Abs(attacks[0]-1)<.001f,"Real basic timer and preview agree");audit.timerChecks++;
    UnlockKit("brand");brandClock=5;bloodClock=warCryClock=100;TickVirtueAbilities(AbilityCooldown("brand")*.5f);Require(Mathf.Abs(brandClock-2.5f)<.001f,"Real spell timer and preview agree");audit.timerChecks++;
    UnlockKit("vengeance");Require(TryVengeance(0)&&Mathf.Abs(vengeanceReadyAt-elapsed-AbilityCooldown("vengeance"))<.001f,"Reactive cooldown uses same weapon factor");audit.timerChecks++;
    var target=ReadoutTarget(Vector3.forward*2);target.hp=target.maxHp=100000;attacks[0]=0;Attack(0);Require(Mathf.Abs(abilityDamageRecords["cleave"].total-city.WeaponDamage)<.01f,"Actual basic hit equals preview");
    BeginWeaponSweep(AbilityDamage("whirl"));UpdateCombatWeapons(.01f);UpdateCombatWeapons(.31f);UpdateCombatWeapons(.31f);Require(Mathf.Abs(abilityDamageRecords["whirl"].total-AbilityDamage("whirl"))<.02f,"Three spin passes share a single cast damage budget");
    float raw=AbilityDamage("cleave");build.Pick(TestCard("haste",UpgradeRarity.Common));build.Pick(KitUpgrade("cleave","haste"));Require(Mathf.Abs(AbilityCooldown("cleave")-city.WeaponInterval*.90f)<.001f&&AbilityDamage("cleave")==raw,"Weapon timing and Haste compose once without double damage scaling");
    audit.profiles++;yield return null;
   }
   // Test the existing ability kit, status effects, bleed accounting and Fury as regressions.
   CheckRageAbilityKit();audit.kitRegression=true;CheckRageVirtue();audit.rageRegression=true;
   city.mine=7;city.EnsureIndustry();CheckResourceTopBar();audit.resourceRegression=true;
   city.blacksmith=9;city.EnsureIndustry();Debug.Log("WEAPON_QA_REQUIRED_RESEARCH "+city.CraftResearchRequirement(9,"weapons"));city.EnsureResearch();city.researchRanks[LibraryResearch.Index("weapons")]=10;for(int i=0;i<5;i++)city.AddResource(4,i,100);city.metalBars[9]=city.metalBars[10]=8;
   Require(city.CraftMetalWeapon(9,out _)&&city.EquipRoll(city.lastCraftedRollId),"Light Veil craft and equip in player");int light=city.lastCraftedRollId;Require(city.CraftMetalWeapon(10,out _)&&city.EquipRoll(city.lastCraftedRollId),"Heavy Veil craft and equip in player");int heavy=city.lastCraftedRollId;
   var store=new CharacterSaveStore(Path.Combine(Application.persistentDataPath,"weapon-scaling-qa",Guid.NewGuid().ToString("N")));var roster=store.LoadRoster();var character=SavedCharacter.From("Weapon scaling QA",city);store.Save(character,city,roster);store.Load(character.id,out _,out var loaded);Require(loaded.WeaponDamage==150&&loaded.WeaponInterval==2.72f&&loaded.FindRoll(light)!=null&&loaded.FindRoll(heavy)!=null,"Both distinct profiles survive real player save reload");audit.saveRoundtrip=true;
   audit.passed=true;string output=Path.Combine(Application.dataPath,"..","Smoke","WeaponScaling");Directory.CreateDirectory(output);File.WriteAllText(Path.Combine(output,"validation.json"),JsonUtility.ToJson(audit,true));Debug.Log("VEILBOUND_WEAPON_SCALING_RUNTIME_PASS "+JsonUtility.ToJson(audit));Application.Quit(0);
  
   }finally{criticalRandom=reviewRandom;}
  }
 }
}
