using System.Collections;
using System.IO;
using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  IEnumerator EnemyStyleReview() {
   yield return new WaitForSecondsRealtime(.5f);
   string folder=Path.Combine(Application.dataPath,"..","Smoke","MonsterMature");
   Directory.CreateDirectory(folder);string pass=Path.Combine(folder,"STYLE-PASS.txt");if(File.Exists(pass))File.Delete(pass);
   yield return MonsterFamilyReview(folder);
   for(int kind=0;kind<6;kind++) {
    var v=MonsterVisual.Create(null,kind==5?0:kind,kind==5?1:0);
    Require(v.Has("BODY - connected"),"Each enemy retains a connected sculpted head and body");
    if(kind!=3)Require(v.Has("CLOTH - layered waist binding")&&v.Count("CLOTH - torn overskirt panel")==2,"Waist and fitted torn panels replace the plain shorts appearance");
    if(kind==2||kind==5)Require(v.Has("ARMOUR - broken Sanctuary breastplate"),"Heavy enemies carry close-fitting ruined Sanctuary plate");
    if(kind==1)Require(v.Count("LOBBER - wrist crystal")==6&&v.Count("LEATHER - crystal casting cuff")==2,"Lobber wrist crystals retain their casting mounts");
    if(kind==3)Require(v.Count("CLAW - rooted finger")==8,"Ghoul keeps all eight connected fingertip claws");
    if(kind==5)Require(v.Has("CLOTH - torn warden mantle"),"Elder retains its huge support arm and gains the ruined warden mantle");
    if(kind==0||kind==5) {
     foreach(var t in v.GetComponentsInChildren<Transform>())t.gameObject.layer=30;
     Vector3 focus=v.bones[4].position+new Vector3(0,.015f,.07f);
     FamilyFrame(Path.Combine(folder,kind==0?"husk-face.png":"elder-face.png"),focus+new Vector3(.24f,.12f,1.4f),focus,kind==0?.25f:.32f,1400,1400);
    }
    if(kind==3) {
     var restVertices=v.skin.sharedMesh.vertices;var ws=v.skin.sharedMesh.boneWeights;
     for(int frame=0;frame<=12;frame++) {
      v.Animate(frame/24f,1,0);var baked=new Mesh();v.skin.BakeMesh(baked);var posed=baked.vertices;
      for(int side=0;side<2;side++) {
       int elbow=side==0?6:9,wrist=elbow+1;Vector3 e=v.transform.InverseTransformPoint(v.bones[elbow].position),w=v.transform.InverseTransformPoint(v.bones[wrist].position),axis=(w-e).normalized;
       Require(Vector3.Dot(v.bones[wrist].up,Vector3.up)>.98f,"Ghoul palms stay down throughout the full crawl cycle");
       Vector3 a=v.rest[wrist]-v.rest[elbow];float length=a.magnitude;a/=length;int samples=0;float smallest=1;
       for(int i=0;i<restVertices.Length;i++) {
        var weight=ws[i];float ew=0,ww=0;
        if(weight.boneIndex0==elbow)ew+=weight.weight0;if(weight.boneIndex1==elbow)ew+=weight.weight1;if(weight.boneIndex0==wrist)ww+=weight.weight0;if(weight.boneIndex1==wrist)ww+=weight.weight1;
        float t=Vector3.Dot(restVertices[i]-v.rest[elbow],a)/length;
        if(ew>.05f&&ww>.05f&&ew+ww>.99f&&t>.74f&&t<.94f) {float r=Vector3.ProjectOnPlane(posed[i]-e,axis).magnitude;smallest=Mathf.Min(smallest,r);samples++;}
       }
       Require(samples>10&&smallest>.012f,"Ghoul distal forearm retains volume during crawling");
      }
      Object.DestroyImmediate(baked);
     }
     foreach(var t in v.GetComponentsInChildren<Transform>())t.gameObject.layer=30;
     v.Animate(0,0,0);FamilyFrame(Path.Combine(folder,"ghoul-wrists-rest.png"),new Vector3(.80f,.37f,1.55f),new Vector3(0,.10f,.53f),.48f,1400,1100);
     v.Animate(.30f,1,0);FamilyFrame(Path.Combine(folder,"ghoul-wrists-stride.png"),new Vector3(.80f,.37f,1.55f),new Vector3(0,.10f,.53f),.48f,1400,1100);
    }
    Object.DestroyImmediate(v.gameObject);
   }
   File.WriteAllText(pass,"PASS: six refreshed enemies; connected original anatomy; fitted rag and plate additions; specialist mounts and animations; existing spawn, hit, fuse and area warning checks. Isolated smoke save.");
   Debug.Log("VEILBOUND_MATURE_ENEMY_WRISTS_PASS");Application.Quit(0);
  }
 }
}
