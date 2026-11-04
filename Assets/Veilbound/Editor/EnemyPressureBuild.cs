#if UNITY_EDITOR
using System;using System.IO;using UnityEngine;using UnityEditor;using UnityEditor.Build.Reporting;
namespace Veilbound.Editor {
 public static class EnemyPressureBuild {
  static void Need(bool condition,string message){if(!condition)throw new Exception("Enemy pressure: "+message);}
  public static void Build(){
   Need(ExpeditionDifficulty.Health(0)==1&&ExpeditionDifficulty.Health(1)==1&&ExpeditionDifficulty.Damage(1)==1&&ExpeditionDifficulty.SpawnRate(1)==1,"entry depth preserved");
   Need(Mathf.Abs(ExpeditionDifficulty.Health(20)-5.75f)<.001f&&Mathf.Abs(ExpeditionDifficulty.Damage(20)-2.9f)<.001f&&Mathf.Abs(ExpeditionDifficulty.SpawnRate(20)-1.76f)<.001f,"depth twenty baseline");
   for(int depth=2;depth<=100;depth++){Need(Mathf.Abs(ExpeditionDifficulty.Health(depth)-ExpeditionDifficulty.Health(depth-1)-.25f)<.0001f,"flat health progression");Need(Mathf.Abs(ExpeditionDifficulty.Damage(depth)-ExpeditionDifficulty.Damage(depth-1)-.10f)<.0001f,"flat damage progression");Need(Mathf.Abs(ExpeditionDifficulty.SpawnRate(depth)-ExpeditionDifficulty.SpawnRate(depth-1)-.04f)<.0001f,"flat spawn progression");}
   PrototypeBuild.Checks();Directory.CreateDirectory("EnemyPressureUpdate");File.WriteAllText("EnemyPressureUpdate/editor-validation.json","{\"passed\":true,\"depthsChecked\":101,\"depth20Health\":5.75,\"depth20Damage\":2.9,\"depth20SpawnRate\":1.76}");
   AssetDatabase.SaveAssets();var result=BuildPipeline.BuildPlayer(new[]{"Assets/Veilbound/Scenes/HollowCourt.unity"},"Builds/Integrated/TheVeilbound.exe",BuildTarget.StandaloneWindows64,BuildOptions.Development);Need(result.summary.result==BuildResult.Succeeded,"Windows player build");Debug.Log("VEILBOUND_ENEMY_PRESSURE_BUILD_PASS");
  }
 }
}
#endif
