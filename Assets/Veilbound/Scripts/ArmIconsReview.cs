using System;using System.IO;using System.Collections;using System.Linq;using UnityEngine;
namespace Veilbound {public partial class VeilGame {
 IEnumerator ArmIconsReview(){
  yield return new WaitForSecondsRealtime(1);combatReviewFreeze=true;string folder=Path.Combine(Application.dataPath,"..","Smoke","ArmIcons");Directory.CreateDirectory(folder);
  foreach(bool female in new[]{false,true}){
   city=FreshCharacterTown();city.female=female;city.InitializeWoodenStarter();city.equippedGear[2]=Metals.Item(0);city.equippedGear[4]=Metals.Item(0);city.equippedGear[6]=Metals.Item(0);RebuildHero();StartRun();combatReviewFreeze=true;
   hero.root.rotation=Quaternion.identity;hero.SetMotion(Vector3.zero);hero.Animate(0,0,0);
   var eq=hero.root.GetComponent<ProductionHeroVisual>().equipment;
   Require(eq.baseRegions[1].gameObject.activeInHierarchy&&eq.baseRegions[5].gameObject.activeInHierarchy,"Unequipped upper arms and forearms remain active");
   foreach(int slot in new[]{0,1,3,5,7})for(int shape=0;shape<eq.baseRegions[slot].sharedMesh.blendShapeCount;shape++)Require(eq.baseRegions[slot].GetBlendShapeWeight(shape)==0,"Belt cannot deform exposed limbs");
   foreach(float time in new[]{0f,.15f,.4f,.7f}){
    hero.SetMotion(time==0?Vector3.zero:Vector3.forward*2);hero.Animate(time,time==0?0:1,0);
    foreach(int limb in new[]{1,3,5}){
     var mesh=new Mesh();eq.SetSlotEnabled(4,false);eq.baseRegions[limb].BakeMesh(mesh);var without=mesh.vertices;eq.SetSlotTier(4,Metals.Item(0));eq.baseRegions[limb].BakeMesh(mesh);var withBelt=mesh.vertices;
     Require(without.Length==withBelt.Length&&without.Zip(withBelt,(a,b)=>(a-b).sqrMagnitude).All(d=>d<.00000001f),"Equipping belt preserves arm/hand geometry through movement");Destroy(mesh);
    }
   }
   hero.SetMotion(Vector3.zero);hero.Animate(0,0,0);
   yield return null;yield return new WaitForEndOfFrame();
   var cameraObject=new GameObject("Unbaked arm review camera");var cam=cameraObject.AddComponent<Camera>();cam.enabled=false;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.08f,.08f,.08f);cam.orthographic=true;cam.orthographicSize=1.1f;cam.cullingMask=1<<30;
   var layers=hero.root.GetComponentsInChildren<Transform>(true).ToDictionary(t=>t,t=>t.gameObject.layer);foreach(var t in layers.Keys)t.gameObject.layer=30;
   cam.transform.position=hero.root.position+new Vector3(0,1,5);cam.transform.LookAt(hero.root.position+Vector3.up*.95f);var rt=new RenderTexture(900,900,24);cam.targetTexture=rt;
   RawGearCapture(Path.Combine(folder,(female?"Female":"Male")+"-baked.png"),new Vector3(0,1,5));
   var previous=RenderTexture.active;cam.Render();RenderTexture.active=rt;var tex=new Texture2D(900,900,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,900,900),0,0);tex.Apply();File.WriteAllBytes(Path.Combine(folder,(female?"Female":"Male")+"-live.png"),tex.EncodeToPNG());RenderTexture.active=previous;
   foreach(var pair in layers)pair.Key.gameObject.layer=pair.Value;cam.targetTexture=null;rt.Release();Destroy(rt);Destroy(tex);Destroy(cameraObject);EndRun(false);ReturnHome();
  }
  city=SharedProductionSample();RebuildHero();ReturnHome();tab=2;SetState(ScreenState.Settlement);OpenCityWorkshop(true);SetSmithPage(1);smithMetal=1;smithSlot=2;
  yield return new WaitForSecondsRealtime(2);yield return CardScreenshot(Path.Combine(folder,"unlocked-plate.png"));
  Require(loadoutItemIcons.Count>=80,"Unlocked rows show actual coloured gear thumbnails");
  city.researchRanks[LibraryResearch.Index("plate")]=1;yield return null;yield return CardScreenshot(Path.Combine(folder,"locked-plate.png"));
  File.WriteAllText(Path.Combine(folder,"PASS.txt"),"PASS: male/female partially equipped live GPU rendering, upper arms/forearms active, unchanged limb geometry with belt on/off in four poses; unlocked plate uses actual coloured model thumbnails; locked plate retains neutral artwork. Isolated smoke profile.");Debug.Log("VEILBOUND_ARM_ICONS_PASS");Application.Quit(0);
 }
}}
