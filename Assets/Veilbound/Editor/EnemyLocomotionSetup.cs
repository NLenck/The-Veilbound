#if UNITY_EDITOR
using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.Build.Reporting;
namespace Veilbound.Editor {
 public static class EnemyLocomotionSetup {
  const string Output="EnemyAnimationIntegration",ResourcesFolder="Assets/Veilbound/Resources/EnemyLocomotion";
  [Serializable]class SourceFrame{public Vector3[] points,tips;}
  [Serializable]class SourceClip{public string name,source;public float duration,distance,legLength;public SourceFrame[] frames;}
  [Serializable]class SourceLibrary{public SourceClip[] clips;}
  static readonly int[] Children={1,2,3,4,-1,6,7,-1,9,10,-1,12,13,-1,15,16,-1};
  static void Need(bool value,string message){if(!value)throw new Exception("Enemy locomotion: "+message);}
  public static void Run(){
   Directory.CreateDirectory(ResourcesFolder);Directory.CreateDirectory(Output);
   var library=JsonUtility.FromJson<SourceLibrary>(File.ReadAllText(Output+"/source_samples.json"));var clips=library.clips.ToDictionary(c=>c.name);
   foreach(string id in new[]{"Town/Husk","Town/Brute","Town/Bulwark","Shrine/Shrine_Guardian","Town/Ghoul","Shrine/Acolyte"}){
    var entry=ProductionModelCatalog.Current.Enemy(id);Need(entry!=null&&entry.prefab,"catalog entry "+id);
    var instance=UnityEngine.Object.Instantiate(entry.prefab);instance.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);instance.transform.localScale=Vector3.one;
    try{
     var visual=instance.GetComponent<MonsterVisual>();bool crawl=id=="Town/Ghoul",cultist=id=="Shrine/Acolyte",mutant=id!="Town/Husk"&&!crawl&&!cultist;
     var data=ScriptableObject.CreateInstance<EnemyMotionSamples>();data.assetId=id;
     data.movement=Bake(visual,clips[cultist?"CultistWalk":crawl?"LowCrawl":mutant?"MutantWalk":"ZombieRun"],crawl);
     if(mutant)data.fastMovement=Bake(visual,clips["MutantRun"],false);
     string path=ResourcesFolder+"/"+id.Replace('/','_')+".asset";var existing=AssetDatabase.LoadAssetAtPath<EnemyMotionSamples>(path);
     if(existing){EditorUtility.CopySerialized(data,existing);UnityEngine.Object.DestroyImmediate(data);}else AssetDatabase.CreateAsset(data,path);
    }finally{UnityEngine.Object.DestroyImmediate(instance);}
   }
   AssetDatabase.SaveAssets();Debug.Log("ENEMY_LOCOMOTION_BAKE_COMPLETE");
  }
  static Quaternion Frame(Vector3 right,Vector3 up){right.Normalize();up=Vector3.ProjectOnPlane(up,right).normalized;return Quaternion.LookRotation(Vector3.Cross(right,up).normalized,up);}
  static EnemyMotionCycle Bake(MonsterVisual visual,SourceClip source,bool crawl){
   var bones=visual.bones;var rest=visual.rest;
   float targetLeg=Vector3.Distance(rest[11],rest[12])+Vector3.Distance(rest[12],rest[13]);
   float ratio=targetLeg/Mathf.Max(.01f,source.legLength);
   var cycle=new EnemyMotionCycle{source=source.source,duration=source.duration,strideDistance=source.distance*ratio,frameCount=source.frames.Length,rotations=new Quaternion[source.frames.Length*17],hips=new Vector3[source.frames.Length]};
   Need(cycle.strideDistance>.02f,"source must include forward travel before in-place baking: "+source.name);
   Quaternion restPelvis=Frame(rest[14]-rest[11],rest[1]-rest[0]);
   float sourceFloor=source.frames.Min(f=>new[]{f.points[7].y,f.points[10].y,f.points[13].y,f.points[16].y,f.tips[13].y,f.tips[16].y}.Min());
   var mesh=new Mesh();
   try{
    for(int frame=0;frame<source.frames.Length;frame++){
     var sample=source.frames[frame];
     for(int i=0;i<17;i++){bones[i].localPosition=i==0?rest[i]:rest[i]-rest[MonsterVisual.Parents[i]];bones[i].localRotation=Quaternion.identity;}
     Quaternion pelvis=Frame(sample.points[14]-sample.points[11],sample.points[1]-sample.points[0])*Quaternion.Inverse(restPelvis);
     bones[0].rotation=pelvis;
     for(int i=1;i<17;i++){
      Vector3 restDirection;
      if(Children[i]>=0)restDirection=rest[Children[i]]-rest[i];
      else if(i==4)restDirection=Vector3.up;
      else if(i==7||i==10)restDirection=crawl?Vector3.forward:rest[i]-rest[i-1];
      else restDirection=Vector3.forward;
      Vector3 desired=sample.tips[i]-sample.points[i];
      Need(desired.sqrMagnitude>.000001f,"source bone direction "+source.name+"/"+i);
      bones[i].rotation=Quaternion.FromToRotation(restDirection,desired);
     }
     // Fit the animated skin to the floor, retaining the source's airborne clearance.
     visual.skin.BakeMesh(mesh);var vertices=mesh.vertices;
     float minimum=vertices.Min(v=>visual.skin.transform.TransformPoint(v).y);
     float airborne=Mathf.Max(0,new[]{sample.points[7].y,sample.points[10].y,sample.points[13].y,sample.points[16].y,sample.tips[13].y,sample.tips[16].y}.Min()-sourceFloor)*ratio;
     bones[0].position+=Vector3.up*(airborne-minimum);
     cycle.hips[frame]=bones[0].localPosition;
     for(int i=0;i<17;i++){
      var q=bones[i].localRotation;
      if(frame>0&&Quaternion.Dot(cycle.rotations[(frame-1)*17+i],q)<0)q=new Quaternion(-q.x,-q.y,-q.z,-q.w);
      cycle.rotations[frame*17+i]=q;
     }
    }
   }finally{UnityEngine.Object.DestroyImmediate(mesh);}
   Debug.Log("ENEMY_CYCLE "+visual.production.assetId+" "+source.name+" speed="+cycle.NaturalSpeed+" stride="+cycle.strideDistance+" duration="+cycle.duration);
   return cycle;
  }
  [Serializable]class Audit{public bool passed;public int models,samples;public float maxLoopAngle,maxRootTravel,minSkinY=999;public List<string> rates=new List<string>();}
  public static void Validate(){
   var audit=new Audit();
   foreach(string id in new[]{"Town/Husk","Town/Brute","Town/Bulwark","Shrine/Shrine_Guardian","Town/Ghoul","Shrine/Acolyte"}){
    var data=Resources.Load<EnemyMotionSamples>("EnemyLocomotion/"+id.Replace('/','_'));Need(data,"motion data "+id);
    foreach(var cycle in new[]{data.movement,data.fastMovement}.Where(c=>c!=null&&c.frameCount>1)){
     Need(cycle.rotations.Length==cycle.frameCount*17&&cycle.hips.Length==cycle.frameCount,"complete baked samples "+id);
     for(int i=0;i<17;i++)audit.maxLoopAngle=Mathf.Max(audit.maxLoopAngle,Quaternion.Angle(cycle.rotations[i],cycle.rotations[(cycle.frameCount-1)*17+i]));
     Need(cycle.strideDistance>.05f&&cycle.duration>.1f,"measured travel and duration "+id);
    }
    var instance=UnityEngine.Object.Instantiate(ProductionModelCatalog.Current.Enemy(id).prefab);var visual=instance.GetComponent<MonsterVisual>();
    try{
     visual.Animate(0,0,0);Need(visual.production.UsesImportedMotion,"live enemy uses selected source "+id);
     var start=instance.transform.position;float time=0;
     foreach(float size in new[]{1f,2f})foreach(float speed in new[]{0f,.8f,1.3f,2.1f,2.94f,3.78f}){
      instance.transform.localScale=Vector3.one*size;visual.production.SetMotion(Vector3.forward*speed);
      for(int frame=0;frame<60;frame++){
       time+=1/30f;visual.Animate(time,speed>.01f?1:0,1/30f);
       if(frame%10!=0)continue;
       var mesh=new Mesh();visual.skin.BakeMesh(mesh);Need(mesh.vertices.All(p=>float.IsFinite(p.x)&&float.IsFinite(p.y)&&float.IsFinite(p.z)&&p.sqrMagnitude<400),"finite skin "+id);
       float min=mesh.vertices.Min(v=>visual.skin.transform.TransformPoint(v).y);audit.minSkinY=Mathf.Min(audit.minSkinY,min);Need(min>-.05f*size,"ground penetration "+id+" "+min+" speed "+speed+" size "+size+" frame "+frame);
       UnityEngine.Object.DestroyImmediate(mesh);audit.samples++;
      }
      audit.rates.Add(id+" size "+size+" speed "+speed+": "+visual.production.PlaybackRate+"x");
     }
     audit.maxRootTravel=Mathf.Max(audit.maxRootTravel,Vector3.Distance(start,instance.transform.position));
     visual.GroundSlamPose(.5f);visual.MeleePose(.2f,.35f,.18f,.35f);audit.models++;
    }finally{UnityEngine.Object.DestroyImmediate(instance);}
   }
   Need(audit.maxLoopAngle<35,"loop transition "+audit.maxLoopAngle);Need(audit.maxRootTravel<.00001f,"animation does not move the enemy actor");audit.passed=true;File.WriteAllText(Output+"/editor_validation.json",JsonUtility.ToJson(audit,true));Debug.Log("VEILBOUND_ENEMY_LOCOMOTION_EDITOR_PASS "+JsonUtility.ToJson(audit));
  }
  public static void Build(){Run();BuildExisting();}
  public static void BuildExisting(){Validate();GreatswordAnimationSetup.Validate();PrototypeBuild.Checks();ProductionModelImport.Validate();AssetDatabase.SaveAssets();var report=BuildPipeline.BuildPlayer(new[]{"Assets/Veilbound/Scenes/HollowCourt.unity"},"Builds/Integrated/TheVeilbound.exe",BuildTarget.StandaloneWindows64,BuildOptions.Development);Need(report.summary.result==BuildResult.Succeeded,"Windows build");Debug.Log("VEILBOUND_ENEMY_LOCOMOTION_BUILD_PASS");}
 }
}
#endif
