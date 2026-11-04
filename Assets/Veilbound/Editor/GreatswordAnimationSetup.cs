#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
namespace Veilbound.Editor {
 public static class GreatswordAnimationSetup {
  const string ResourcesRoot="Assets/Veilbound/Resources/GreatswordAnimations",Output="GreatswordAnimationIntegration";
  static readonly string[] Labels={"Ready","Walk","WalkBackward","WalkLeft","WalkRight","Run","RunBackward","RunLeft","RunRight","AttackA","AttackCombo","AttackB","AttackHeavy","BlockEnter","BlockHold","BlockExit","BlockImpact"};
  static readonly HashSet<string> Loops=new HashSet<string>(new[]{"Ready","Walk","WalkBackward","WalkLeft","WalkRight","Run","RunBackward","RunLeft","RunRight","BlockHold"});
  [Serializable]class Audit {public bool passed;public int clips,directionalCases,diagonalCases,skinnedSamples,attackSamples;public float maxGripError,maxRootDrift,maxLegDifference;public List<string> checks=new List<string>();}
  static void Need(bool ok,string message){if(!ok)throw new Exception("Greatsword: "+message);}
  public static void Run(){
   Directory.CreateDirectory(Output);Directory.CreateDirectory(ResourcesRoot+"/Clips");AssetDatabase.Refresh();
   foreach(string gender in new[]{"Male","Female"}){
    var clips=new Dictionary<string,AnimationClip>();
    var prefab=ProductionModelCatalog.Current.Player(gender=="Female").prefab;
    var skeleton=prefab.GetComponentsInChildren<Transform>(true).First(t=>t.name=="mixamorig:Root");
    string bonePath=Relative(skeleton,prefab.transform),prefix=bonePath.Substring(0,bonePath.Length-skeleton.name.Length);
    foreach(string label in Labels){
     string path="Assets/Veilbound/GreatswordAnimations/"+gender+"/"+label+".fbx";
     var importer=(ModelImporter)AssetImporter.GetAtPath(path);Need(importer,"importer "+path);
     // Animation-only Blender exports store meter-valued bone translations under centimeter FBX units.
     importer.globalScale=100;importer.animationType=ModelImporterAnimationType.Generic;importer.avatarSetup=ModelImporterAvatarSetup.NoAvatar;importer.importAnimation=true;importer.importCameras=importer.importLights=false;importer.materialImportMode=ModelImporterMaterialImportMode.None;importer.optimizeGameObjects=false;importer.animationCompression=ModelImporterAnimationCompression.Off;importer.resampleCurves=true;importer.SaveAndReimport();
     var source=AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().First(c=>!c.name.StartsWith("__preview__"));
     var clip=UnityEngine.Object.Instantiate(source);clip.name=gender+"_Greatsword_"+label;clip.hideFlags=HideFlags.None;
     foreach(var binding in AnimationUtility.GetCurveBindings(clip)){
      var curve=AnimationUtility.GetEditorCurve(clip,binding);AnimationUtility.SetEditorCurve(clip,binding,null);
      if(binding.path.Length>0){var mapped=binding;mapped.path=prefix+binding.path;AnimationUtility.SetEditorCurve(clip,mapped,curve);}
     }
     var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=Loops.Contains(label);settings.loopBlend=false;AnimationUtility.SetAnimationClipSettings(clip,settings);clip.wrapMode=Loops.Contains(label)?WrapMode.Loop:WrapMode.ClampForever;
     string destination=ResourcesRoot+"/Clips/"+gender+"_"+label+".anim";var existing=AssetDatabase.LoadAssetAtPath<AnimationClip>(destination);
     if(existing){EditorUtility.CopySerialized(clip,existing);UnityEngine.Object.DestroyImmediate(clip);clip=existing;}else AssetDatabase.CreateAsset(clip,destination);
     clips.Add(label,clip);
    }
    var set=ScriptableObject.CreateInstance<GreatswordMotionSet>();set.ready=clips["Ready"];set.movement=new[]{clips["Walk"],clips["WalkBackward"],clips["WalkLeft"],clips["WalkRight"],clips["Run"],clips["RunBackward"],clips["RunLeft"],clips["RunRight"]};set.attacks=new[]{clips["AttackA"],clips["AttackCombo"],clips["AttackB"],clips["AttackHeavy"]};set.blockEnter=clips["BlockEnter"];set.blockHold=clips["BlockHold"];set.blockExit=clips["BlockExit"];set.blockImpact=clips["BlockImpact"];
    string resource=ResourcesRoot+"/"+gender+".asset";var old=AssetDatabase.LoadAssetAtPath<GreatswordMotionSet>(resource);if(old){EditorUtility.CopySerialized(set,old);UnityEngine.Object.DestroyImmediate(set);}else AssetDatabase.CreateAsset(set,resource);
   }
   AssetDatabase.SaveAssets();AssetDatabase.Refresh();Validate();
  }
  static string Relative(Transform t,Transform root){string path="";for(;t&&t!=root;t=t.parent)path=t.name+(path.Length>0?"/"+path:"");return path;}
  static void MeshCheck(ProductionHeroVisual visual,Audit audit){float headHeight=Vector3.Dot(visual.facade.bones[4].position-visual.transform.position,visual.transform.up);Need(headHeight>.7f&&headHeight<2.5f,"correct animated character scale "+headHeight);var mesh=new Mesh();visual.equipment.body.BakeMesh(mesh);Need(mesh.vertices.All(v=>float.IsFinite(v.x)&&float.IsFinite(v.y)&&float.IsFinite(v.z)&&v.sqrMagnitude<100),"finite skin deformation");UnityEngine.Object.DestroyImmediate(mesh);audit.skinnedSamples++;audit.maxGripError=Mathf.Max(audit.maxGripError,visual.lastGripError);if(visual.lastGripError>=.035f)File.WriteAllLines(Output+"/grip_diagnostic.txt",new[]{"sample "+audit.directionalCases,"sword "+visual.sword.position+" grip "+visual.sword.TransformPoint(new Vector3(0,.035f,.015f)),"localvelocity "+visual.greatsword.LocalVelocity}.Concat(visual.facade.bones.Select(b=>b.name+" world "+b.position.ToString("R")+" local "+b.localPosition.ToString("R")+" scale "+b.lossyScale.ToString("R"))));Need(visual.lastGripError<.035f,"two hand grip "+visual.lastGripError);}
  public static void Validate(){
   var audit=new Audit();
   foreach(bool female in new[]{false,true}){
    var city=new Settlement{female=female,bronzeHeroVersion=1};city.EnsureGear();var rig=new HeroRig(null,city);var baseline=new HeroRig(null,city);
    try{
     var visual=rig.root.GetComponent<ProductionHeroVisual>();var other=baseline.root.GetComponent<ProductionHeroVisual>();Need(visual.greatsword!=null,"live factory uses greatsword graph");
     var paths=new HashSet<string>(rig.root.GetComponentsInChildren<Transform>(true).Select(t=>Relative(t,rig.root)));var set=GreatswordMotionSet.Load(female);
     foreach(var clip in new[]{set.ready}.Concat(set.movement).Concat(set.attacks).Concat(new[]{set.blockEnter,set.blockHold,set.blockExit,set.blockImpact})){
      var bindings=AnimationUtility.GetCurveBindings(clip);if(bindings.Any(b=>!paths.Contains(b.path)))File.WriteAllText(Output+"/binding_diagnostic.txt","PREFAB:\n"+string.Join("\n",paths)+"\nCLIP:\n"+string.Join("\n",bindings.Select(b=>b.path).Distinct()));Need(bindings.Length>100&&bindings.All(b=>paths.Contains(b.path)),"all animation curves bind to production skeleton: "+clip.name);audit.clips++;
     }
     Vector3[] directions={Vector3.forward,Vector3.back,Vector3.left,Vector3.right};float time=0;
     foreach(float yaw in new[]{0f,90f,180f,270f})foreach(float speed in new[]{1.2f,3.6f})for(int d=0;d<4;d++){
      rig.root.rotation=Quaternion.Euler(0,yaw,0);Vector3 before=rig.root.position;rig.SetMotion(rig.root.TransformDirection(directions[d]*speed));
      for(int f=0;f<24;f++){time+=1/30f;rig.Animate(time,1,0);}
      int input=1+d+(speed>2?4:0);Need(visual.greatsword.Weight(input)>.98f,"correct local direction "+yaw+"/"+speed+"/"+d);Need(Quaternion.Angle(rig.root.rotation,Quaternion.Euler(0,yaw,0))<.001f,"animation preserves mouse-facing orientation");
      audit.maxRootDrift=Mathf.Max(audit.maxRootDrift,Vector3.Distance(before,rig.root.position));MeshCheck(visual,audit);audit.directionalCases++;
     }
     foreach(var dir in new[]{new Vector3(1,0,1),new Vector3(-1,0,1),new Vector3(1,0,-1),new Vector3(-1,0,-1)}){
      rig.SetMotion(rig.root.TransformDirection(dir.normalized*3.6f));for(int f=0;f<24;f++){time+=1/30f;rig.Animate(time,1,0);}
      float a=visual.greatsword.Weight(dir.z>0?5:6),b=visual.greatsword.Weight(dir.x<0?7:8);Need(Mathf.Abs(a-.5f)<.005f&&Mathf.Abs(b-.5f)<.005f,"diagonal movement blends adjacent directions");MeshCheck(visual,audit);audit.diagonalCases++;
     }
     // Two independent graphs at identical times prove that attacks preserve locomotion on the legs.
     rig.root.rotation=baseline.root.rotation=Quaternion.identity;rig.SetMotion(Vector3.right*3.6f);baseline.SetMotion(Vector3.right*3.6f);rig.Animate(0,0,0);baseline.Animate(0,0,0);
     bool upperMoved=false;
     for(int f=1;f<=90;f++){
      if(f==15||f==55)rig.PlayBasicAttack(.9f);
      float t=f/60f;rig.Animate(t,1,0);baseline.Animate(t,1,0);
      for(int i=11;i<17;i++){float angle=Quaternion.Angle(visual.facade.bones[i].localRotation,other.facade.bones[i].localRotation);audit.maxLegDifference=Mathf.Max(audit.maxLegDifference,angle);Need(angle<.1f,"upper-body mask preserves leg motion "+angle);}
      if(visual.greatsword.AttackWeight>.5f)upperMoved|=Quaternion.Angle(visual.facade.bones[8].localRotation,other.facade.bones[8].localRotation)>2;
      MeshCheck(visual,audit);audit.attackSamples++;
     }
     Need(upperMoved&&visual.greatsword.AttackSerial==2,"actual alternating attacks animate upper body");
     rig.SetMotion(Vector3.zero);for(int f=0;f<40;f++)rig.Animate(2+f/30f,0,0);Need(visual.greatsword.Weight(0)>.99f,"returns to ready when collision stops movement");
    }finally{rig.Release();baseline.Release();UnityEngine.Object.DestroyImmediate(rig.root.gameObject);UnityEngine.Object.DestroyImmediate(baseline.root.gameObject);}
   }
   Need(audit.maxRootDrift<.00001f,"no animation-driven actor travel");audit.checks.AddRange(new[]{"34 clips bind to existing production skeletons","mouse-relative cardinal and diagonal locomotion","upper-body attacks retain leg motion","two hand grip and finite skinned meshes","ready stance after stopping; root motion disabled"});audit.passed=true;File.WriteAllText(Output+"/editor_validation.json",JsonUtility.ToJson(audit,true));Debug.Log("VEILBOUND_GREATSWORD_EDITOR_PASS "+JsonUtility.ToJson(audit));
  }
  public static void Build(){Run();BuildPlayer();}
  public static void BuildExisting(){Validate();BuildPlayer();}
  static void BuildPlayer(){PrototypeBuild.Checks();ProductionModelImport.Validate();AssetDatabase.SaveAssets();Directory.CreateDirectory("Builds/Integrated");var result=BuildPipeline.BuildPlayer(new[]{"Assets/Veilbound/Scenes/HollowCourt.unity"},"Builds/Integrated/TheVeilbound.exe",BuildTarget.StandaloneWindows64,BuildOptions.Development);Need(result.summary.result==BuildResult.Succeeded,"Windows player build");Debug.Log("VEILBOUND_GREATSWORD_BUILD_PASS");}
 }
}
#endif
