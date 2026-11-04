using System;
using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace Veilbound {
 public partial class VeilGame {
  IEnumerator BronzeSmokeTest() {
   string folder=Path.Combine(Application.dataPath,"..","Smoke","BronzeKnight");Directory.CreateDirectory(folder);string pass=Path.Combine(folder,"PASS.txt");if(File.Exists(pass))File.Delete(pass);
   yield return new WaitForSecondsRealtime(.8f);
   yield return CheckMenuConfirmDoesNotDodge();
   var visual=hero.root.GetComponent<BronzeHeroVisual>();Require(visual&&visual.bodySkin==hero.skin,"The playable main character is the saved bronze knight");Require(visual.bones.Length==17,"Fitted 17 joint skeleton is present");
   foreach(var renderer in visual.parts) {
    Require(renderer.sharedMesh&&renderer.bones.Length==17,"Every body, cloth and plate mesh is attached to the fitted skeleton");
    foreach(var w in renderer.sharedMesh.boneWeights)Require(Mathf.Abs(w.weight0+w.weight1+w.weight2+w.weight3-1)<.0002f,"All skin weights are normalised");
   }
   int silverTrims=0;
   foreach(var renderer in visual.parts)if(renderer.name.StartsWith("TRIM -")) {
    silverTrims++;var colour=renderer.sharedMaterials[0].color;Require(colour.b>=colour.r&&Mathf.Abs(colour.b-colour.r)<.15f,"Rolled trim retains iron/silver colour against bronze plates");
    if(renderer.name.Contains("pauldron"))foreach(var weight in renderer.sharedMesh.boneWeights)Require((weight.boneIndex0==5||weight.boneIndex0==8)&&weight.weight0>.999f,"Shoulder trim shares the upper-arm mount of its fitted canopy");
   }
   Require(silverTrims>=10,"Authored trim is present throughout the equipped set");
   Require(hero.root.Find("HELM - closed pointed visor with vents"),"Closed knight visor is present");Require(hero.swordPivot.Find("Equipped greatsword T"+city.equippedSword),"Authored two-handed greatsword is equipped");
   hero.Animate(0,0,0);Vector3 knee=visual.bones[12].position;var neutral=new Mesh();hero.skin.BakeMesh(neutral);hero.Animate(.47f,1,.65f);var posed=new Mesh();hero.skin.BakeMesh(posed);var pverts=posed.vertices;var nverts=neutral.vertices;float change=0;for(int i=0;i<pverts.Length;i++){Require((!float.IsNaN(pverts[i].x)&&!float.IsInfinity(pverts[i].x))&&pverts[i].sqrMagnitude<30,"Posed body stays finite and bounded");change=Mathf.Max(change,Vector3.Distance(nverts[i],pverts[i]));}Require(change>.08f,"Body vertices deform during walk and attack");
   Require(Vector3.Distance(visual.bones[10].position,hero.swordPivot.TransformPoint(new Vector3(0,0,.015f))+hero.root.up*.055f)<.03f,"Right hand reaches the greatsword grip");Require(Vector3.Distance(visual.bones[7].position,hero.swordPivot.TransformPoint(new Vector3(0,0,.17f))+hero.root.up*.055f)<.03f,"Left hand reaches the second grip");Destroy(neutral);Destroy(posed);
   BronzePortrait(Path.Combine(folder,"01-guard.png"),0,0,0,new Vector3(3,2.1f,5),1.38f,1.1f);
   BronzePortrait(Path.Combine(folder,"02-walk.png"),.47f,1,0,new Vector3(3,2.1f,5),1.38f,1.1f);
   BronzePortrait(Path.Combine(folder,"03-attack.png"),.25f,1,.65f,new Vector3(3,2.1f,5),1.38f,1.1f);
   BronzePortrait(Path.Combine(folder,"04-helmet.png"),0,0,0,new Vector3(.5f,1.95f,4),.31f,1.83f);
   BronzePortrait(Path.Combine(folder,"05-back.png"),0,0,0,new Vector3(-3,2.1f,-5),1.38f,1.1f);
   BronzePortrait(Path.Combine(folder,"08-back-walk.png"),.47f,1,0,new Vector3(-3,2.1f,-5),1.38f,1.1f);
   BronzePortrait(Path.Combine(folder,"09-back-attack.png"),.25f,1,.65f,new Vector3(-3,2.1f,-5),1.38f,1.1f);
   BronzePortrait(Path.Combine(folder,"10-cape-shoulder-detail.png"),0,0,0,new Vector3(-2,1.9f,-5),.42f,1.57f);
   int helm=city.equippedGear[0];city.equippedGear[0]=0;RebuildHero();Require(!hero.root.Find("HELM - closed pointed visor with vents").gameObject.activeSelf,"Helmet can be removed independently");city.equippedGear[0]=helm;RebuildHero();
   yield return new WaitForSecondsRealtime(.15f);BronzeGameFrame(Path.Combine(folder,"06-settlement.png"));yield return new WaitForSecondsRealtime(.3f);
   StartRun();Vector3 old=position;var testPad=InputSystem.AddDevice<Gamepad>();InputSystem.QueueStateEvent(testPad,new GamepadState{leftStick=Vector2.up});InputSystem.Update();for(int i=0;i<20;i++)Step(.02f);InputSystem.RemoveDevice(testPad);Require(Vector3.Distance(old,position)>.2f,"Main character responds to game movement");movement=Vector2.zero;
   yield return new WaitForSecondsRealtime(.3f);BronzeGameFrame(Path.Combine(folder,"07-expedition.png"));yield return new WaitForSecondsRealtime(.3f);
   FundTestCity();city.sword=2;city.equippedSword=2;city.blacksmith=Mathf.Max(2,city.blacksmith);RebuildHero();CheckEnemyRules();
   yield return MonsterFamilyReview(Path.Combine(Application.dataPath,"..","Smoke","MonsterFamily"));
   yield return UpgradeCardReview(Path.Combine(Application.dataPath,"..","Smoke","UpgradeCards"));
   yield return CombatInterfaceReview(Path.Combine(Application.dataPath,"..","Smoke","CombatInterface"));
   yield return HudStyleReview(Path.Combine(Application.dataPath,"..","Smoke","GlobeHud"));
   yield return EquipmentPaperDollReview(Path.Combine(Application.dataPath,"..","Smoke","EquipmentPaperDoll"));
   yield return GearRollReview(Path.Combine(Application.dataPath,"..","Smoke","GearRolls"));
   yield return MetalProgressionReview(Path.Combine(Application.dataPath,"..","Smoke","MetalProgression"));
   yield return RageVirtueReview(Path.Combine(Application.dataPath,"..","Smoke","RageVirtue"));
   yield return CleanHudReview(Path.Combine(Application.dataPath,"..","Smoke","CleanHud"));
   yield return ResourceTopBarReview(Path.Combine(Application.dataPath,"..","Smoke","ResourceTopBar"));
   yield return GameSettingsReview(Path.Combine(Application.dataPath,"..","Smoke","GameSettings"));
   File.WriteAllText(pass,"PASS: imported bronze hero; 17-bone deformation; normalised weights; two-hand grip IK; helmet equipment toggle; settlement and expedition rendering; real movement; existing enemy/combat rules.\n");Debug.Log("VEILBOUND_BRONZE_KNIGHT_SMOKE_PASS");Application.Quit(0);
  }
  void BronzeGameFrame(string path) {
   var rt=new RenderTexture(1600,900,24);var previous=cameraRig.targetTexture;cameraRig.targetTexture=rt;cameraRig.Render();var old=RenderTexture.active;RenderTexture.active=rt;var tex=new Texture2D(1600,900,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1600,900),0,0);tex.Apply();File.WriteAllBytes(path,tex.EncodeToPNG());RenderTexture.active=old;cameraRig.targetTexture=previous;rt.Release();Destroy(rt);Destroy(tex);
  }
  void BronzePortrait(string path,float time,float move,float attack,Vector3 at,float scale,float height) {
   hero.root.position=Vector3.zero;hero.root.rotation=Quaternion.identity;hero.Animate(time,move,attack);foreach(var t in hero.root.GetComponentsInChildren<Transform>(true))t.gameObject.layer=30;
   bool fog=RenderSettings.fog;RenderSettings.fog=false;var obj=new GameObject("Bronze hero verification camera");var cam=obj.AddComponent<Camera>();cam.cullingMask=1<<30;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.11f,.13f,.14f);cam.orthographic=true;cam.orthographicSize=scale;cam.nearClipPlane=.01f;cam.farClipPlane=50;cam.transform.position=at;cam.transform.LookAt(new Vector3(0,height,0));var rt=new RenderTexture(1400,1400,24){antiAliasing=4};cam.targetTexture=rt;
   var bakedObjects=new System.Collections.Generic.List<GameObject>();var bakedMeshes=new System.Collections.Generic.List<Mesh>();var skins=hero.root.GetComponentsInChildren<SkinnedMeshRenderer>();
   foreach(var sk in skins){var baked=new Mesh();sk.BakeMesh(baked);var item=new GameObject("CPU baked "+sk.name);item.layer=30;item.transform.SetParent(sk.transform,false);item.AddComponent<MeshFilter>().sharedMesh=baked;item.AddComponent<MeshRenderer>().sharedMaterials=sk.sharedMaterials;sk.enabled=false;bakedObjects.Add(item);bakedMeshes.Add(baked);}
   var keyObject=new GameObject("Portrait key light");var key=keyObject.AddComponent<Light>();key.type=LightType.Directional;key.intensity=1.3f;key.color=new Color(1,.92f,.82f);keyObject.transform.rotation=Quaternion.LookRotation(new Vector3(0,height,0)-at);cam.Render();foreach(var sk in skins)sk.enabled=true;foreach(var item in bakedObjects)DestroyImmediate(item);foreach(var item in bakedMeshes)DestroyImmediate(item);var old=RenderTexture.active;RenderTexture.active=rt;var tex=new Texture2D(1400,1400,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1400,1400),0,0);tex.Apply();File.WriteAllBytes(path,tex.EncodeToPNG());cam.Render();tex.ReadPixels(new Rect(0,0,1400,1400),0,0);tex.Apply();File.WriteAllBytes(path.Replace(".png","-gpu.png"),tex.EncodeToPNG());DestroyImmediate(keyObject);RenderTexture.active=old;cam.targetTexture=null;rt.Release();Destroy(rt);Destroy(tex);Destroy(obj);RenderSettings.fog=fog;
   foreach(var t in hero.root.GetComponentsInChildren<Transform>(true))t.gameObject.layer=0;hero.root.position=position;
  }
 }
}





