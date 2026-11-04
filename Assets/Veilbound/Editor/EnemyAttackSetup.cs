#if UNITY_EDITOR
using System;using System.IO;using UnityEngine;using UnityEditor;using UnityEditor.Build.Reporting;
namespace Veilbound.Editor {
 public static class EnemyAttackSetup {
  public static void Build(){CultistModelSetup.Run();EnemyLocomotionSetup.Run();EnemyLocomotionSetup.Validate();GreatswordAnimationSetup.Validate();PrototypeBuild.Checks();ProductionModelImport.Validate();AssetDatabase.SaveAssets();var report=BuildPipeline.BuildPlayer(new[]{"Assets/Veilbound/Scenes/HollowCourt.unity"},"Builds/Integrated/TheVeilbound.exe",BuildTarget.StandaloneWindows64,BuildOptions.Development);if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Enemy attack build failed");Debug.Log("VEILBOUND_ENEMY_ATTACK_BUILD_PASS");}
 }
}
#endif
