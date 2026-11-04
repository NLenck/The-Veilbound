using System;using System.IO;using System.Collections;using System.Collections.Generic;using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  [Serializable]class EnemyMotionAudit{public bool passed,actualChase;public int models,finiteSamples,overlays,stops;public List<string> rates=new List<string>();}
  IEnumerator EnemyLocomotionReview(){
   yield return null;combatReviewFreeze=true;var audit=new EnemyMotionAudit();string folder=Path.Combine(Application.dataPath,"..","Smoke","EnemyLocomotion");Directory.CreateDirectory(folder);
   var catalog=ProductionModelCatalog.Current;
   foreach(string id in new[]{"Town/Husk","Town/Brute","Town/Bulwark","Shrine/Shrine_Guardian","Town/Ghoul"}){
    var entry=catalog.Enemy(id);var go=Instantiate(entry.prefab);go.name=id;go.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);
    var v=go.GetComponent<MonsterVisual>();v.Animate(0,0,0);Require(v.production.UsesImportedMotion,"selected enemy uses imported Mixamo movement "+id);
    float actualSpeed=id=="Town/Ghoul"?2.94f:id=="Town/Husk"?2.1f:1.155f;
    try{
     for(int frame=0;frame<60;frame++){
      float time=frame/30f;v.production.SetMotion(Vector3.forward*actualSpeed);v.Animate(time,1,1/30f);
      var mesh=new Mesh();v.skin.BakeMesh(mesh);foreach(var p in mesh.vertices)Require(float.IsFinite(p.x)&&float.IsFinite(p.y)&&float.IsFinite(p.z)&&p.sqrMagnitude<400,"finite enemy skin "+id);Destroy(mesh);audit.finiteSamples++;
      if(frame==45)audit.rates.Add(id+" @ "+actualSpeed+"m/s: "+v.production.PlaybackRate+"x");
      yield return null;yield return null;EnemyMotionCapture(go,Path.Combine(folder,id.Replace('/','_')+"_"+frame.ToString("D2")+".png"));
     }
     v.GroundSlamPose(.5f);v.MeleePose(.2f,.35f,.18f,.35f);audit.overlays++;
     v.production.SetMotion(Vector3.zero);for(int frame=0;frame<30;frame++)v.Animate(2+frame/30f,0,1/30f);Require(v.production.PlaybackRate<.01f,"stopped enemies stop stepping "+id);audit.stops++;
     audit.models++;
    }finally{Destroy(go);}
   }
   city=FreshCharacterTown();RebuildHero();StartRun();combatReviewFreeze=true;var chase=SpawnFoe(0,2);Require(chase!=null,"real pursuing enemy spawns");Vector3 initial=chase.t.position;
   for(int frame=0;frame<90;frame++){animClock+=1/30f;navigation.Tick(position,1/30f,true);UpdateFoes(1/30f);}
   Require(chase.rig.visual.production.UsesImportedMotion&&Vector3.Distance(initial,chase.t.position)>.1f&&chase.rig.visual.production.PlaybackRate>.01f,"actual navigation and UpdateFoes feed resolved movement into imported locomotion");audit.actualChase=true;
   audit.passed=true;File.WriteAllText(Path.Combine(folder,"validation.json"),JsonUtility.ToJson(audit,true));Debug.Log("VEILBOUND_ENEMY_LOCOMOTION_RUNTIME_PASS "+JsonUtility.ToJson(audit));Application.Quit(0);
  }
  void EnemyMotionCapture(GameObject actor,string path){
   var cameraObject=new GameObject("Enemy motion QA camera");var cam=cameraObject.AddComponent<Camera>();cam.enabled=false;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.055f,.065f,.085f);cam.cullingMask=1<<29;cam.orthographic=true;
   foreach(var tr in actor.GetComponentsInChildren<Transform>(true))tr.gameObject.layer=29;
   var floor=GameObject.CreatePrimitive(PrimitiveType.Plane);floor.name="QA floor";floor.layer=29;floor.transform.position=Vector3.down*.025f;floor.transform.localScale=Vector3.one*.8f;floor.GetComponent<Collider>().enabled=false;
   var material=new Material(Shader.Find("Standard"));material.color=new Color(.13f,.14f,.16f);material.SetFloat("_Glossiness",.05f);floor.GetComponent<Renderer>().sharedMaterial=material;
   var renderer=actor.GetComponent<MonsterVisual>().skin;float height=renderer.sharedMesh.bounds.size.y;cam.orthographicSize=Mathf.Max(1,height*.72f);cam.transform.position=actor.transform.position+new Vector3(3,2.1f,4)*height/2;cam.transform.LookAt(actor.transform.position+Vector3.up*height*.45f);
   var rt=new RenderTexture(540,600,24);cam.targetTexture=rt;var previous=RenderTexture.active;cam.Render();RenderTexture.active=rt;var texture=new Texture2D(540,600,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,540,600),0,0);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());RenderTexture.active=previous;cam.targetTexture=null;rt.Release();Destroy(rt);Destroy(texture);Destroy(cameraObject);Destroy(floor);Destroy(material);
  }
 }
}
