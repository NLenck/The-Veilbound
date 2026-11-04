using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace Veilbound.Editor {
 public static class ExpeditionModifierBuild {
  public static void Build(){
   PrototypeBuild.Checks();
   for(int best=0;best<=100;best++){
    var city=new Settlement{expeditionModifierVersion=1,expeditionBestDepth=best};city.EnsureExpeditionModifiers();
    int expected=Math.Min(10,best<5?1:2+(best-5)/10);
    if(city.ExpeditionColumns!=expected)throw new Exception("Incorrect column threshold at "+best);
   }
   for(int stage=0;stage<=4;stage++){
    int heavy=0,bulwarks=0;
    for(int roll=0;roll<10000;roll++){
     ExpeditionSpawns.Pick(1,stage,roll/100f,0,0,0,out int kind,out int variant,2.5f);
     if(kind==2){heavy++;if(variant==1)bulwarks++;}
     ExpeditionSpawns.Pick(1,stage,roll/100f,0,2,1,out kind,out variant,2.5f);
     if(kind==2)throw new Exception("Elite modifier bypassed the existing simultaneous-heavy cap");
     ExpeditionSpawns.Pick(0,stage,roll/100f,0,0,0,out kind,out variant,2.5f);
     ExpeditionSpawns.Pick(0,stage,roll/100f,0,0,0,out int baselineKind,out int baselineVariant);
     if(kind!=baselineKind||variant!=baselineVariant)throw new Exception("Elite modifier changed an unconfirmed rare enemy outside the Brute/Bulwark pool");
    }
    int expectedHeavy=stage==4?1750:stage==3?1250:0,expectedBulwarks=stage==4?500:0;
    if(Math.Abs(heavy-expectedHeavy)>1||Math.Abs(bulwarks-expectedBulwarks)>1)throw new Exception("Elite frequency weights or wave gates changed incorrectly at stage "+stage);
   }
   Directory.CreateDirectory("Builds/Integrated");AssetDatabase.SaveAssets();
   var result=BuildPipeline.BuildPlayer(new[]{"Assets/Veilbound/Scenes/HollowCourt.unity"},"Builds/Integrated/TheVeilbound.exe",BuildTarget.StandaloneWindows64,BuildOptions.Development);
   if(result.summary.result!=BuildResult.Succeeded)throw new Exception("Expedition build failed: "+result.summary.result);
   Debug.Log("VEILBOUND_EXPEDITION_MODIFIER_BUILD_PASS");
  }
 }
}
