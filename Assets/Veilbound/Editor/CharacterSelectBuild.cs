using System;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace Veilbound.Editor {
 public static class CharacterSelectBuild {
  public static void Build(){
   PrototypeBuild.Checks();AssetDatabase.SaveAssets();
   var result=BuildPipeline.BuildPlayer(new[]{"Assets/Veilbound/Scenes/HollowCourt.unity"},"Builds/Integrated/TheVeilbound.exe",BuildTarget.StandaloneWindows64,BuildOptions.Development);
   if(result.summary.result!=BuildResult.Succeeded)throw new Exception("Character selection build failed: "+result.summary.result);
   Debug.Log("VEILBOUND_CHARACTER_SELECT_BUILD_PASS");
  }
 }
}
