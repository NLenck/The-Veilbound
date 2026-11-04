using System.IO;
using UnityEngine;
namespace Veilbound.Editor {
 public static class ExpeditionAnimationDiagnostic {
  public static void Run(){
   var lines=new System.Collections.Generic.List<string>();
   for(int map=0;map<=1;map++){
    var root=new GameObject("Standalone existing animation diagnostic");
    try{
     var visual=MonsterVisual.Create(root.transform,2,0,map);visual.Animate(0,0,0);visual.GroundSlamPose(.7f);float raised=visual.bones[7].position.y;
     visual.Animate(0,0,0);visual.GroundSlamPose(1);
     float drop=raised-visual.bones[7].position.y,foot=Mathf.Abs(visual.bones[13].position.y-visual.transform.TransformPoint(visual.rest[13]).y);
     lines.Add("map="+map+" model="+visual.production.assetId+" armDrop="+drop+" footOffset="+foot+" originalAssertion="+(drop>.3f&&foot<.04f));
    }finally{Object.DestroyImmediate(root);}
   }
   Directory.CreateDirectory("ExpeditionModifierUpdate");File.WriteAllLines("ExpeditionModifierUpdate/animation-diagnostic.txt",lines);
   Debug.Log("VEILBOUND_STANDALONE_ANIMATION_DIAGNOSTIC "+string.Join("; ",lines));
  }
 }
}
