#if UNITY_EDITOR
using System;using System.Linq;using System.IO;using UnityEngine;using UnityEditor;using UnityEditor.Build.Reporting;
namespace Veilbound.Editor {
 public static class CombatFlowBuild {
  static void Need(bool condition,string message){if(!condition)throw new Exception("Combat flow: "+message);}
  static void CheckOffers(){
   var rng=new System.Random(401);var build=new RunBuild();int rolls=0;
   for(int level=2;level<=150;level++){build.Roll(rng,level);Need(build.offers.Count>0&&build.offers.All(c=>c.ability),"level-ups offer abilities only");Need(build.offers.Select(c=>c.Key).Distinct().Count()==build.offers.Count,"distinct ability cards");build.Pick(build.offers[rng.Next(build.offers.Count)]);Need(build.Slots<=5,"existing ability slot cap");rolls++;}
   Need(RunBuild.Pool.Where(c=>!c.ability).All(c=>build.Rank(c.id)==0),"level-ups never grant passives");
   build=new RunBuild();build.locks=2;build.Roll(rng,3);var held=build.offers[0];Need(build.Lock(held),"lock ability");
   build.RollPassives(rng);Need(build.PassiveRound&&build.offers.Count==3&&build.offers.All(c=>!c.ability),"relic offers three rarity-rolled passives");Need(build.locked==null,"ability lock does not enter passive pool");
   var passive=build.offers[0];Need(build.Lock(passive),"lock passive");build.Roll(rng);Need(build.PassiveRound&&build.offers.Contains(passive),"relic reroll preserves context and lock");
   build.Roll(rng,5);Need(!build.PassiveRound&&build.offers.Contains(held)&&build.locked==held.Key,"ability lock survives relic choices");build.RollPassives(rng);Need(build.offers.Contains(passive),"passive lock stays in passive pool");
   Need(build.Banish(passive),"banish passive");build.Roll(rng);Need(build.offers.All(c=>!c.ability&&c.Key!=passive.Key),"passive banish keeps reward pool");
   for(int i=0;i<250;i++){build.RollPassives(rng);Need(build.offers.Count>0&&build.offers.All(c=>!c.ability),"repeated relic choices stay passive");build.Pick(build.offers[rng.Next(build.offers.Count)]);}
   var empty=new RunBuild();foreach(var c in RunBuild.Pool)empty.banished.Add(c.id);empty.Roll(rng,3);Need(empty.offers.Count==0,"exhausted pools do not fall back to wrong reward type");
   Directory.CreateDirectory("CombatFlowUpdate");File.WriteAllText("CombatFlowUpdate/editor-validation.json","{\"passed\":true,\"abilityLevelsChecked\":"+rolls+",\"passiveRoundsChecked\":250,\"separateLockPools\":true,\"banishAndReroll\":true}");Debug.Log("VEILBOUND_COMBAT_FLOW_EDITOR_PASS");
  }
  public static void Build(){CheckOffers();PrototypeBuild.Checks();AssetDatabase.SaveAssets();var result=BuildPipeline.BuildPlayer(new[]{"Assets/Veilbound/Scenes/HollowCourt.unity"},"Builds/Integrated/TheVeilbound.exe",BuildTarget.StandaloneWindows64,BuildOptions.Development);Need(result.summary.result==BuildResult.Succeeded,"Windows build");Debug.Log("VEILBOUND_COMBAT_FLOW_BUILD_PASS");}
 }
}
#endif

