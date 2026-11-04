#if UNITY_EDITOR
using System;using System.IO;using UnityEditor;using UnityEditor.Build.Reporting;using UnityEngine;
namespace Veilbound.Editor {public static class ArmIconsBuild {
 public static void Build(){RawGearBuild.Validate();PrototypeBuild.Checks();AssetDatabase.SaveAssets();Directory.CreateDirectory("Builds/ArmIconsReview");var report=BuildPipeline.BuildPlayer(new[]{"Assets/Veilbound/Scenes/HollowCourt.unity"},"Builds/ArmIconsReview/TheVeilbound.exe",BuildTarget.StandaloneWindows64,BuildOptions.Development);if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Arm/icon build failed");Debug.Log("VEILBOUND_ARM_ICONS_BUILD_PASS");}
}}
#endif
