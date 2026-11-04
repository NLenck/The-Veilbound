#if UNITY_EDITOR
using System;using System.IO;using UnityEditor;using UnityEditor.Build.Reporting;using UnityEngine;
namespace Veilbound.Editor {public static class PointerScaleBuild {
 public static void Build(){RawGearBuild.Validate();PrototypeBuild.Checks();AssetDatabase.SaveAssets();Directory.CreateDirectory("Builds/PointerFixed");var report=BuildPipeline.BuildPlayer(new[]{"Assets/Veilbound/Scenes/HollowCourt.unity"},"Builds/PointerFixed/TheVeilbound.exe",BuildTarget.StandaloneWindows64,BuildOptions.Development);if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Pointer build failed");Debug.Log("VEILBOUND_POINTER_SCALE_BUILD_PASS");}
}}
#endif
