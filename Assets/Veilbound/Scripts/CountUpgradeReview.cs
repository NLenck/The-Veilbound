using System;using System.IO;using System.Collections;using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  int countChecks;
  void CountCheck(bool ok,string message){Require(ok,message);countChecks++;}
  Choice CountCard(string id,UpgradeRarity rarity)=>AbilitySpecs.Upgrade(Array.Find(RunBuild.Pool,c=>c.id==id),AbilitySpecs.Definition(id,id=="cleave"?"repeats":"projectiles"),rarity);
  IEnumerator CountUpgradeReview(){
   yield return null;combatReviewFreeze=true;city=FreshCharacterTown();city.InitializeWoodenStarter();RebuildHero();
   foreach(string id in new[]{"cleave","lance","orbit"})foreach(UpgradeRarity rarity in Enum.GetValues(typeof(UpgradeRarity))){
    build=new RunBuild();if(build.Rank(id)==0)build.Pick(TestCard(id,UpgradeRarity.Common));var card=CountCard(id,rarity);int rank=build.Rank(id);
    float expected=id=="orbit"?(rarity==UpgradeRarity.Uncommon?1:rarity==UpgradeRarity.Rare?2:0):(rarity==UpgradeRarity.Rare?1:rarity==UpgradeRarity.Legendary?2:0);
    CountCheck(AbilitySpecs.SupportsRarity(card,rarity)==(expected>0),"Only authored count rarities are supported");
    if(expected>0)CountCheck(build.Describe(card).StartsWith("+"+expected+" "),"Count preview uses exact per-rarity amount");
    build.Pick(card);CountCheck(build.Attribute(id,card.attribute)==expected&&build.Rank(id)==rank+(expected>0?1:0),"Actual count pick matches preview and rejects unsupported rarities");
   }
   build=new RunBuild();foreach(string id in new[]{"lance","orbit"})build.Pick(TestCard(id,UpgradeRarity.Common));var random=new System.Random(815);int countOffers=0;bool[] ordinaryRarities=new bool[5];bool[] variants=new bool[6];
   for(int i=0;i<2000;i++){build.Roll(random,3);CountCheck(build.offers.Count==3,"Upgrade rolls continue to produce three distinct options");foreach(var offer in build.offers){if(AbilitySpecs.IsCountUpgrade(offer)){CountCheck(AbilitySpecs.SupportsRarity(offer,offer.rarity),"Rolled count bonuses never appear at forbidden rarities");countOffers++;int index=offer.id=="cleave"?0:offer.id=="lance"?2:4;variants[index+(AbilitySpecs.UpgradeAmount(offer)==2?1:0)]=true;}else ordinaryRarities[(int)offer.rarity]=true;}}
   CountCheck(countOffers>0&&Array.TrueForAll(variants,x=>x)&&Array.TrueForAll(ordinaryRarities,x=>x),"Every requested count offer and ordinary rarity can still appear");
   build=new RunBuild();build.Pick(CountCard("cleave",UpgradeRarity.Legendary));var capped=CountCard("cleave",UpgradeRarity.Legendary);CountCheck(build.Describe(capped).StartsWith("+1 "),"Near-cap preview shows only the remaining swing");build.Pick(capped);CountCheck(build.Attribute("cleave","repeats")==3&&!build.CanUpgrade(capped),"Existing extra swing cap remains enforced");
   foreach(int branch in new[]{0,1,2}){
    PressureStart(1,0);position=Vector3.zero;hero.root.position=position;facing=Vector3.forward;criticalRandom=new NonCriticalReviewRandom();build.Pick(TestCard("orbit",UpgradeRarity.Common));build.Pick(CountCard("orbit",UpgradeRarity.Rare));
    if(branch>0){build.ranks["orbit"]=7;Require(build.BeginAscension(),"Oathblade fork eligible");build.Pick(build.offers[branch-1]);}
    int expected=branch==1?5:3;CountCheck(OathBladeCount==expected,"Extra blades survive both ascension choices");
    foreach(float clock in new[]{0f,1.3f,8f}){elapsed=clock;float radius=2.4f*SkillArea("orbit");for(int i=0;i<expected;i++){Vector3 a=OathPositionAt(i)-position,b=OathPositionAt((i+1)%expected)-position;CountCheck(Mathf.Abs(a.magnitude-radius)<.0001f&&Mathf.Abs(Vector3.Dot(a.normalized,b.normalized)-Mathf.Cos(2*Mathf.PI/expected))<.0001f,"All blades maintain equal angular spacing and radius while orbiting");}}
    UpdateCombatWeapons(0);TickAscensionEffects(0);CountCheck(crownSwords.Count==(branch==2?0:expected-1),"Additional orbiting visuals match the live blade count");
    if(branch!=2){for(int i=0;i<crownSwords.Count;i++)CountCheck(Vector3.Distance(crownSwords[i].position,OathPositionAt(i+1)+Vector3.up*1.05f)<.0001f,"Visible extra blades share damage anchor positions");}
    foreach(var f in foes)Destroy(f.t.gameObject);foes.Clear();ResetCombatReadouts();var target=ReadoutTarget(OathPositionAt(expected-1));float hp=target.hp;AscendedOrbitAttack();
    CountCheck(branch==2?ascensionFlights.Count==expected:target.hp<hp,"Every added blade participates in attacks, including hunter flights");
   }
   PressureStart(1,0);build.Pick(CountCard("cleave",UpgradeRarity.Rare));extraSwings.Clear();ExecuteBasicAttack("cleave");CountCheck(extraSwings.Count==1,"Rare extra swing schedules one repeat");
   PressureStart(1,0);build.Pick(TestCard("lance",UpgradeRarity.Common));build.Pick(CountCard("lance",UpgradeRarity.Legendary));ExecuteRiftBlades();CountCheck(bolts.Count==3,"Legendary extra projectiles fires three total daggers");
   string folder=Path.Combine(Application.dataPath,"..","Smoke","CountUpgrades");Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"validation.json"),"{\"passed\":true,\"checks\":"+countChecks+",\"randomUpgradeRolls\":2000,\"countOffers\":"+countOffers+"}");Debug.Log("VEILBOUND_COUNT_UPGRADE_PASS checks="+countChecks);Application.Quit(0);
  }
 }
}
