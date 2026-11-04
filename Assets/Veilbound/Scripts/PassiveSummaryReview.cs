using System;using System.IO;using System.Collections;using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  IEnumerator PassiveSummaryReview(){
   yield return null;combatReviewFreeze=true;city=FreshCharacterTown();city.InitializeWoodenStarter();RebuildHero();int checks=0;
   string[] ids={"wisdom","crit","critdamage","haste","power","guard","speed"};
   float[][] expected={new float[]{15,25,40,55,80},new float[]{1,2,3,4,5},new float[]{10,20,30,40,50},new float[]{2,4,6,8,10},new float[]{4,8,12,16,20},new float[]{2,3,4,5,8},new float[]{3,6,9,12,15}};
   for(int rarity=0;rarity<5;rarity++){
    ReturnHome();for(int i=0;i<10;i++)city.expeditionRanks[i]=0;StartRun();float baseDamage=Damage,baseSpeed=Speed,baseCooldown=AbilityCooldown("cleave");
    for(int row=0;row<ids.Length;row++){var card=TestCard(ids[row],(UpgradeRarity)rarity);Require(build.Describe(card).Contains("+"+expected[row][rarity].ToString("0.#")+"%"),"Card describes exact rarity value");build.Pick(card);Require(Mathf.Abs(build.PassivePercent(ids[row])-expected[row][rarity])<.001f,"Exact rarity bonus is recorded");checks+=2;}
    Require(Mathf.Abs(Damage/baseDamage-1-expected[4][rarity]*.01f)<.001f&&Mathf.Abs(Speed/baseSpeed-1-expected[6][rarity]*.01f)<.001f,"Actual damage and movement apply exact bonuses");
    Require(Mathf.Abs(CritChance-.05f-expected[1][rarity]*.01f)<.001f&&Mathf.Abs(CritMultiplier-1.5f-expected[2][rarity]*.01f)<.001f,"Actual critical stats apply exact bonuses");
    Require(Mathf.Abs(GlobalHasteRating-expected[3][rarity])<.001f&&Mathf.Abs(AbilityCooldown("cleave")/baseCooldown-(1-HasteRules.Reduction(expected[3][rarity])))<.001f,"Real cooldown uses exact Haste bonus");
    health=MaxHealth;invulnerability=0;float hp=health;Hurt(20);Require(Mathf.Abs(hp-health-20*(1-expected[5][rarity]*.01f))<.001f,"Actual incoming damage uses exact reduction");
    xp=0;shards.Add(new Shard{t=WorldArt.Group("XP bonus fixture",effects),value=100});shards[shards.Count-1].t.position=position;UpdateShards(0);Require(Mathf.Abs(xp-100*(1+expected[0][rarity]*.01f))<.001f,"Actual XP pickup uses exact rarity bonus");checks+=5;
   }
   foreach(string id in ids){var sample=new RunBuild();sample.Pick(TestCard(id,UpgradeRarity.Common));sample.Pick(TestCard(id,UpgradeRarity.Legendary));int row=Array.IndexOf(ids,id);Require(Mathf.Abs(sample.PassivePercent(id)-expected[row][0]-expected[row][4])<.001f,"Different rarities accumulate exact bonuses");checks++;}
   build=new RunBuild();for(int i=0;i<20;i++)build.Pick(TestCard("crit",UpgradeRarity.Legendary));for(int i=0;i<7;i++)build.Pick(TestCard("guard",UpgradeRarity.Legendary));Require(!build.Eligible(TestCard("crit",UpgradeRarity.Common))&&!build.Eligible(TestCard("guard",UpgradeRarity.Common))&&CritChance==1,"Capped passives stop appearing at actual percent caps");checks++;
   int[] counts=new int[5],luckyCounts=new int[5];var rng=new System.Random(7801);var passiveBuild=new RunBuild();
   for(int i=0;i<3000;i++){passiveBuild.RollPassives(rng);foreach(var card in passiveBuild.offers){Require(!card.ability,"Relics offer only passives");counts[(int)card.rarity]++;}}
   for(int i=0;i<10;i++)passiveBuild.Pick(TestCard("luck",UpgradeRarity.Common));rng=new System.Random(7801);
   for(int i=0;i<3000;i++){passiveBuild.RollPassives(rng);foreach(var card in passiveBuild.offers)luckyCounts[(int)card.rarity]++;}
   for(int i=0;i<5;i++)Require(counts[i]>0,"Every rarity can appear in passive choices");Require(luckyCounts[4]>counts[4],"Luck improves Legendary passive odds");checks+=6;
   foreach(int map in new[]{0,1}){
    PressureStart(map,20);Require(SpawnRelic()&&CollectRelic(0),"Actual relic opens passive selection");var selected=build.offers[0];int[] before=build.passivePicks.TryGetValue(selected.id,out var history)?(int[])history.Clone():new int[5];Choose(0);Require(build.passivePicks[selected.id][(int)selected.rarity]==before[(int)selected.rarity]+1,"Selecting a relic preserves its rolled rarity");checks++;
    var normal=SpawnFoe(0,0);var boss=SpawnFoe(1);var heavy=SpawnFoe(0,2);Require(normal!=null&&boss!=null&&heavy!=null,"All health categories spawn");
    Require(Mathf.Abs(normal.maxHp-EnemyTypes.RegularHealth(1,0,0)*6.9f)<.01f&&Mathf.Abs(boss.maxHp-480*7.475f)<.01f,"Depth-scaled health is multiplied by selected normal and boss modifiers");
    Require(Mathf.Abs(heavy.maxHp-EnemyTypes.RegularHealth(1,0,2)*(map==1?8.97f:6.9f))<.01f,"Elite health includes both applicable health modifiers");
    Require(Mathf.Abs(RunModifierFactor(1)-3.19f)<.001f&&Mathf.Abs(RunModifierFactor(3)-3.48f)<.001f&&Mathf.Abs(RunModifierFactor(5)-2.0416f)<.001f,"Summary combines depth and damage/spawn modifiers");
    Require(RunModifierSummary(0)=="+590%"&&RunModifierSummary(4)=="-10%","Summary displays combined health increase and actual cooldown reduction");checks+=4;
   }
   string folder=Path.Combine(Application.dataPath,"..","Smoke","PassiveSummary");Directory.CreateDirectory(folder);ReturnHome();tab=0;for(int i=0;i<10;i++)city.expeditionRanks[i]=2;
   yield return CardScreenshot(Path.Combine(folder,"01-combined-run-summary.png"));
   StartRun();build.Pick(TestCard("luck",UpgradeRarity.Legendary));var previewRandom=new System.Random(401);
   for(int i=0;i<500;i++){build.RollPassives(previewRandom);if(build.offers.Exists(c=>c.rarity==UpgradeRarity.Legendary)&&build.offers.Exists(c=>c.rarity==UpgradeRarity.Epic))break;}
   SetState(ScreenState.Choice);yield return CardScreenshot(Path.Combine(folder,"02-relic-rarity-choices.png"));
   File.WriteAllText(Path.Combine(folder,"validation.json"),"{\"passed\":true,\"checks\":"+checks+",\"baselineRarityCounts\":["+string.Join(",",counts)+"],\"luck100RarityCounts\":["+string.Join(",",luckyCounts)+"]}");Debug.Log("VEILBOUND_PASSIVE_SUMMARY_PASS checks="+checks);Application.Quit(0);
  }
 }
}
