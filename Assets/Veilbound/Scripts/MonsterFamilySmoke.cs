using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  IEnumerator MonsterFamilyReview(string folder) {
   Directory.CreateDirectory(folder);CheckAreaWarningTiming(folder);state=ScreenState.Paused;
   var stage=new GameObject("Monster family verification stage").transform;var models=new List<MonsterVisual>();
   for(int kind=0;kind<6;kind++) {
    var visual=MonsterVisual.Create(stage,kind==5?0:kind,kind==5?1:0);models.Add(visual);visual.skin.updateWhenOffscreen=true;
    Require(visual.bones.Length==17&&visual.skin.sharedMesh.vertexCount>5000,"Every monster has authored anatomy and a fitted 17-joint skin");
    if(kind!=3)Require(visual.Has("CLOTH - fitted torn trousers"),"Every clothed monster uses fitted two-leg trousers");
    foreach(var w in visual.skin.sharedMesh.boneWeights)Require(Mathf.Abs(w.weight0+w.weight1+w.weight2+w.weight3-1)<.0002f,"Monster skin weights are normalised");
    if(kind!=3){int frontFractureCorners=0;var materials=visual.skin.sharedMaterials;var normals=visual.skin.sharedMesh.normals;for(int material=0;material<materials.Length;material++)if(materials[material].name.StartsWith("EMBER - fracture"))foreach(int vertex in visual.skin.sharedMesh.GetTriangles(material))if(normals[vertex].z>.35f)frontFractureCorners++;Require(frontFractureCorners>100,"Ember fracture faces point outward and render on the front of the monster");}
    var a=new Mesh();var b=new Mesh();visual.Animate(0,0,0);visual.skin.BakeMesh(a);visual.Animate(.35f,1,.03f);visual.skin.BakeMesh(b);float change=0;var av=a.vertices;var bv=b.vertices;
    for(int i=0;i<bv.Length;i++){Require(!float.IsNaN(bv[i].x)&&!float.IsInfinity(bv[i].x)&&bv[i].sqrMagnitude<40,"Monster movement stays finite and bounded");change=Mathf.Max(change,Vector3.Distance(av[i],bv[i]));}Require(change>.025f,"Each connected monster skin deforms during movement");Destroy(a);Destroy(b);
    visual.Hit();var feedback=new MaterialPropertyBlock();visual.skin.GetPropertyBlock(feedback,0);Require(feedback.GetColor("_EmissionColor").r>.5f,"Damage flash reaches the authored monster surface");visual.Animate(0,0,.2f);MonsterCapture(Path.Combine(folder,MonsterVisual.Ids[kind]+"-front.png"),visual,false);
    if(kind==0||kind==4){visual.Animate(.75f,1,0);MonsterCapture(Path.Combine(folder,MonsterVisual.Ids[kind]+"-walk.png"),visual,false);visual.Animate(0,0,0);}
    if(kind==5){visual.Animate(.35f,1,0);Require(Mathf.Abs(visual.transform.InverseTransformPoint(visual.bones[7].position).y-visual.rest[7].y)<.012f,"Elder support hand stays planted during the stance phase");MonsterCapture(Path.Combine(folder,"elder-support-stance.png"),visual,false);visual.Animate(1.53f,1,0);Require(visual.transform.InverseTransformPoint(visual.bones[7].position).y>visual.rest[7].y+.10f,"Elder lifts its huge hand during the recovery step");MonsterCapture(Path.Combine(folder,"elder-support-step.png"),visual,false);visual.Animate(0,0,0);}
    if(kind==1){Require(visual.Count("LOBBER - wrist crystal")==6&&!visual.Has("GROWTH - shoulder shard"),"Lobber crystal clusters are on both wrists and removed from the back");Require(Vector3.Distance(visual.CastingOrigin(-1),visual.bones[7].position)<.25f,"Lob projectile originates at the casting wrist cluster");}
    if(kind==2||kind==5)MonsterCapture(Path.Combine(folder,MonsterVisual.Ids[kind]+"-back.png"),visual,true);
    if(kind==3){visual.Animate(.13f,1,0);MonsterCapture(Path.Combine(folder,"ghoul-crawl-a.png"),visual,false);visual.Animate(.30f,1,0);MonsterCapture(Path.Combine(folder,"ghoul-crawl-b.png"),visual,false);}
    if(kind==4){foreach(var t in visual.GetComponentsInChildren<Transform>())t.gameObject.layer=30;FamilyFrame(Path.Combine(folder,"volatile-side.png"),new Vector3(5,1.4f,.15f),new Vector3(0,1.05f,0),1.15f,1400,1400);foreach(var t in visual.GetComponentsInChildren<Transform>())t.gameObject.layer=0;visual.FuseGlow(.82f);MonsterCapture(Path.Combine(folder,"volatile-fuse.png"),visual,false);}
    visual.FuseGlow(-1);visual.Animate(0,0,0);visual.gameObject.SetActive(false);
   }
   for(int rank=1;rank<=3;rank++){var elder=MonsterVisual.Create(stage,0,rank);Require(elder.kind==5&&elder.Has("ELDER - crown shard"),"Every boss rank uses the authored elder");DestroyImmediate(elder.gameObject);}
   var oldHeroPos=hero.root.position;var oldHeroRot=hero.root.rotation;hero.root.position=new Vector3(-7,0,0);hero.root.rotation=Quaternion.identity;hero.Animate(0,0,0);
   float[] positions={-4.9f,-3.4f,-1.45f,.65f,2.6f,5.5f};for(int i=0;i<models.Count;i++){models[i].gameObject.SetActive(true);models[i].transform.localPosition=Vector3.right*positions[i];foreach(var t in models[i].GetComponentsInChildren<Transform>())t.gameObject.layer=30;}
   foreach(var t in hero.root.GetComponentsInChildren<Transform>())t.gameObject.layer=30;
   var ground=WorldArt.Part(stage,"Verification floor",PrimitiveType.Cube,new Vector3(0,-.06f,0),new Vector3(20,.10f,7),new Color(.08f,.09f,.10f));ground.gameObject.layer=30;
   FamilyFrame(Path.Combine(folder,"family-with-player.png"),new Vector3(3.3f,4.4f,14),new Vector3(-.25f,1.3f,0),4.3f,2200,1100);
   foreach(var t in hero.root.GetComponentsInChildren<Transform>())t.gameObject.layer=0;hero.root.position=oldHeroPos;hero.root.rotation=oldHeroRot;DestroyImmediate(stage.gameObject);
   StartRun();position=Vector3.zero;hero.root.position=position;invulnerability=5;
   for(int kind=0;kind<5;kind++){var foe=SpawnFoe(0,kind);Require(foe!=null,"Updated enemy type spawns through the existing combat pipeline");float angle=(kind-2)*.58f;foe.t.position=collision.Resolve(new Vector3(Mathf.Sin(angle)*7,0,Mathf.Cos(angle)*7),foe.radius);foe.rig.Animate(.35f,1,.02f);}
   var boss=SpawnFoe(1);Require(boss!=null&&boss.rig.visual.kind==5,"The boss spawns with its authored elder model");boss.t.position=collision.Resolve(new Vector3(6,0,8),boss.radius);
   for(int i=0;i<20;i++)Step(.016f);yield return null;
   cameraRig.transform.position=new Vector3(10,14,-14);cameraRig.transform.LookAt(new Vector3(1,0,3));cameraRig.orthographicSize=10;BronzeGameFrame(Path.Combine(folder,"encounter-in-game.png"));
   File.WriteAllText(Path.Combine(folder,"PASS.txt"),"PASS: six authored monster prefabs; normalized 17-joint skins; finite deformation; crawling contact IK; all three boss ranks; existing enemy spawning/combat rules; hit and volatile fuse feedback; runtime portraits and an actual encounter.\n");Debug.Log("VEILBOUND_MONSTER_FAMILY_PASS");
  }
  void MonsterCapture(string path,MonsterVisual visual,bool back) {
   foreach(var t in visual.GetComponentsInChildren<Transform>())t.gameObject.layer=30;
   var bound=visual.skin.sharedMesh.bounds;float height=bound.max.y;var focus=new Vector3(0,height*.48f,0);float framing=Mathf.Max(height*.61f,visual.kind==3?.90f:.95f);
   FamilyFrame(path,new Vector3(2.8f,height*.7f+1,back?-5:5),focus,framing,1400,1400);
   foreach(var t in visual.GetComponentsInChildren<Transform>())t.gameObject.layer=0;
  }
  void FamilyFrame(string path,Vector3 at,Vector3 target,float size,int width,int height) {
   var cameraObject=new GameObject("Monster verification camera");var cam=cameraObject.AddComponent<Camera>();cam.cullingMask=1<<30;cam.orthographic=true;cam.orthographicSize=size;cam.nearClipPlane=.01f;cam.farClipPlane=100;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.09f,.115f,.12f);cam.transform.position=at;cam.transform.LookAt(target);
   var keyObj=new GameObject("Monster verification key");var light=keyObj.AddComponent<Light>();light.type=LightType.Directional;light.color=new Color(1,.88f,.75f);light.intensity=.9f;keyObj.transform.rotation=Quaternion.LookRotation(target-at);bool fog=RenderSettings.fog;RenderSettings.fog=false;
   var bakedObjects=new List<GameObject>();var bakedMeshes=new List<Mesh>();var bakedSkins=new List<SkinnedMeshRenderer>();foreach(var sk in UnityEngine.Object.FindObjectsByType<SkinnedMeshRenderer>(FindObjectsSortMode.None)){if(!sk.enabled||sk.gameObject.layer!=30)continue;var baked=new Mesh();sk.BakeMesh(baked);var item=new GameObject("Captured current pose");item.layer=30;item.transform.SetParent(sk.transform,false);item.AddComponent<MeshFilter>().sharedMesh=baked;item.AddComponent<MeshRenderer>().sharedMaterials=sk.sharedMaterials;sk.enabled=false;bakedObjects.Add(item);bakedMeshes.Add(baked);bakedSkins.Add(sk);}var rt=new RenderTexture(width,height,24){antiAliasing=4};cam.targetTexture=rt;cam.Render();foreach(var sk in bakedSkins)sk.enabled=true;foreach(var item in bakedObjects)DestroyImmediate(item);foreach(var baked in bakedMeshes)DestroyImmediate(baked);var old=RenderTexture.active;RenderTexture.active=rt;var tex=new Texture2D(width,height,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,width,height),0,0);tex.Apply();File.WriteAllBytes(path,tex.EncodeToPNG());RenderTexture.active=old;cam.targetTexture=null;rt.Release();Destroy(rt);Destroy(tex);DestroyImmediate(cameraObject);DestroyImmediate(keyObj);RenderSettings.fog=fog;
  }
 }
}



