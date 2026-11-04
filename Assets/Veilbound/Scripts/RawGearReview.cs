using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Veilbound.PlateMasters;
namespace Veilbound {
 public partial class VeilGame {
  void RawGearCapture(string path,Vector3 offset){
   var obj=new GameObject("Raw gear inspection camera");var cam=obj.AddComponent<Camera>();cam.CopyFrom(cameraRig);cam.enabled=false;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.045f,.055f,.065f);cam.cullingMask=1<<30;cam.orthographicSize=1.25f;
   cam.transform.position=hero.root.position+offset;cam.transform.LookAt(hero.root.position+Vector3.up*.95f);
   var layers=new Dictionary<Transform,int>();foreach(var tr in hero.root.GetComponentsInChildren<Transform>(true)){layers[tr]=tr.gameObject.layer;tr.gameObject.layer=30;}
   // Manual pose changes can share one frame. Snapshot the current CPU pose,
   // so the skinned armour cannot reuse a previous camera's GPU skinning pose.
   var snapshots=new List<GameObject>();var snapshotMeshes=new List<Mesh>();var originalSkins=new List<SkinnedMeshRenderer>();
   foreach(var skin in hero.root.GetComponentsInChildren<SkinnedMeshRenderer>()){
    if(!skin.enabled||!skin.gameObject.activeInHierarchy)continue;
    var mesh=new Mesh();skin.BakeMesh(mesh);var snapshot=new GameObject("Current inspection pose");snapshot.layer=30;snapshot.transform.SetParent(skin.transform,false);snapshot.AddComponent<MeshFilter>().sharedMesh=mesh;snapshot.AddComponent<MeshRenderer>().sharedMaterials=skin.sharedMaterials;skin.enabled=false;originalSkins.Add(skin);snapshots.Add(snapshot);snapshotMeshes.Add(mesh);
   }
   var lampObject=new GameObject("Raw gear studio light");var lamp=lampObject.AddComponent<Light>();lamp.type=LightType.Directional;lamp.intensity=.8f;lamp.color=new Color(1,.94f,.85f);lamp.cullingMask=1<<30;lamp.transform.rotation=Quaternion.Euler(35,-25,0);
   bool fog=RenderSettings.fog;RenderSettings.fog=false;var rt=new RenderTexture(1200,1200,24){antiAliasing=4};cam.targetTexture=rt;var previous=RenderTexture.active;cam.Render();RenderTexture.active=rt;var texture=new Texture2D(1200,1200,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,1200,1200),0,0);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());RenderTexture.active=previous;
   foreach(var snapshot in snapshots){snapshot.SetActive(false);Destroy(snapshot);}foreach(var mesh in snapshotMeshes)Destroy(mesh);foreach(var skin in originalSkins)skin.enabled=true;
   foreach(var pair in layers)pair.Key.gameObject.layer=pair.Value;RenderSettings.fog=fog;cam.targetTexture=null;rt.Release();Destroy(rt);Destroy(texture);Destroy(obj);Destroy(lampObject);
  }
  void EquipRawGearSample(bool female,int metal){
   city=FreshCharacterTown();city.female=female;city.EnsureGear();
   for(int slot=0;slot<8;slot++){city.ownedPlate[slot*10+metal]=true;city.equippedGear[slot]=Metals.Item(metal);}city.ownedGear[8*3]=1;city.equippedGear[8]=11;city.ownedMetalWeapons[metal]=true;city.equippedMetalWeapon=metal;
   RebuildHero();StartRun();combatReviewFreeze=true;hero.root.rotation=Quaternion.identity;
  }
  void CheckRawSkinning(){
   var visual=hero.root.GetComponent<ProductionHeroVisual>();var equipment=visual.equipment;
   Require(equipment.replacementMode&&!equipment.body.enabled,"Live production hero uses complete section replacement");
   foreach(var r in equipment.slots.Concat(equipment.baseRegions)){
    Require(r!=null&&r.sharedMesh!=null&&r.sharedMesh.boneWeights.All(w=>Mathf.Abs(w.weight0+w.weight1+w.weight2+w.weight3-1)<.001f),"Every replacement/base vertex is bound to the existing skeleton");
    if(!r.gameObject.activeInHierarchy)continue;var mesh=new Mesh();r.BakeMesh(mesh);var baked=mesh.vertices;Require(baked.All(p=>float.IsFinite(p.x)&&float.IsFinite(p.y)&&float.IsFinite(p.z)&&p.sqrMagnitude<36),"Animated replacement sections stay finite and in character bounds");
    int slot=Array.IndexOf(equipment.slots,r);if(slot==0||slot==1||slot==3||slot==5||slot==6||slot==7){var source=r.sharedMesh.vertices;var triangles=r.sharedMesh.triangles;for(int i=0;i<triangles.Length;i+=3)for(int edge=0;edge<3;edge++){int a=triangles[i+edge],b=triangles[i+(edge+1)%3];float rest=Vector3.Distance(source[a],source[b]),posed=Vector3.Distance(baked[a],baked[b]);Require(Mathf.Abs(rest-posed)<.002f,"Rigid armour triangles retain their original edge lengths through animation");}}
    Destroy(mesh);
   }
   Require(visual.greatsword!=null&&visual.lastGripError<.04f,"Original greatsword animation and hand grips remain intact");
  }
  IEnumerator RawGearReview(){
   yield return null;combatReviewFreeze=true;string folder=Path.Combine(Application.dataPath,"..","Smoke","RawGearReplacement");Directory.CreateDirectory(folder);int poses=0;
   foreach(bool female in new[]{false,true})foreach(int metal in new[]{0,1}){
    EquipRawGearSample(female,metal);float hp=MaxHealth,damage=Damage,speed=Speed;var visual=hero.root.GetComponent<ProductionHeroVisual>();var equipment=visual.equipment;
    string label=(female?"Female":"Male")+"_"+(metal==0?"Tin":"Copper");hero.SetMotion(Vector3.zero);hero.NeutralPose(0);RawGearCapture(Path.Combine(folder,label+"_front.png"),new Vector3(0,1.1f,5));RawGearCapture(Path.Combine(folder,label+"_back.png"),new Vector3(0,1.1f,-5));
    hero.Animate(0,0,0);RawGearCapture(Path.Combine(folder,label+"_ready.png"),new Vector3(3,2,5));
    float time=0;foreach(var direction in new[]{Vector3.forward,Vector3.back,Vector3.left,Vector3.right}){
     hero.SetMotion(direction*3.6f);for(int frame=0;frame<24;frame++){time+=1/30f;hero.Animate(time,1,0);CheckRawSkinning();poses++;}
     if(direction==Vector3.forward){RawGearCapture(Path.Combine(folder,label+"_walk.png"),new Vector3(3,2,5));RawGearCapture(Path.Combine(folder,label+"_walk_side.png"),new Vector3(5,1.1f,0));}
    }
    hero.PlayBasicAttack(.9f);for(int frame=0;frame<30;frame++){time+=1/60f;hero.Animate(time,1,0);CheckRawSkinning();poses++;}
    RawGearCapture(Path.Combine(folder,label+"_attack.png"),new Vector3(3,2,5));
    for(int slot=0;slot<8;slot++){equipment.SetSlotEnabled(slot,false);Require(equipment.baseRegions[slot].gameObject.activeSelf&&!equipment.slots[slot].gameObject.activeSelf,"Unequip restores exactly its base region");equipment.SetSlotTier(slot,Metals.Item(slot%2==0?metal:(metal==0?1:0)));}
    hero.SetMotion(Vector3.zero);hero.NeutralPose(0);RawGearCapture(Path.Combine(folder,label+"_mixed.png"),new Vector3(3,2,5));
    equipment.SetSlotEnabled(0,false);equipment.SetSlotEnabled(2,false);RawGearCapture(Path.Combine(folder,label+"_partial.png"),new Vector3(3,2,5));
    for(int slot=0;slot<9;slot++)equipment.SetSlotEnabled(slot,false);Require(!equipment.body.enabled&&equipment.baseRegions.All(r=>r.gameObject.activeSelf),"Fully unequipped character uses all base sections with no full-body overlay");
    hero.Animate(0,0,0);RawGearCapture(Path.Combine(folder,label+"_base.png"),new Vector3(0,1.1f,5));
    equipment.SetSlotTier(2,Metals.Item(metal));equipment.SetSlotTier(7,Metals.Item(metal));
    for(int slot=0;slot<8;slot++)Require(equipment.baseRegions[slot].gameObject.activeSelf==(slot!=2&&slot!=7),"Chest and boots alone restore all other base sections");
    hero.swordPivot.gameObject.SetActive(false);
    RawGearCapture(Path.Combine(folder,label+"_boots_chest.png"),new Vector3(0,1.1f,5));
    RawGearCapture(Path.Combine(folder,label+"_boots_chest_side.png"),new Vector3(3,1.1f,5));
    equipment.SetSlotTier(4,Metals.Item(metal));equipment.SetSlotTier(5,Metals.Item(metal));
    RawGearCapture(Path.Combine(folder,label+"_boots_chest_belt_bracers.png"),new Vector3(0,1.1f,5));
    RawGearCapture(Path.Combine(folder,label+"_boots_chest_belt_bracers_side.png"),new Vector3(3,1.1f,5));
    equipment.SetSlotTier(1,Metals.Item(metal));
    RawGearCapture(Path.Combine(folder,label+"_shoulder_fit.png"),new Vector3(0,1.1f,5));
    RawGearCapture(Path.Combine(folder,label+"_shoulder_fit_side.png"),new Vector3(3,1.1f,5));
    RawGearCapture(Path.Combine(folder,label+"_shoulder_fit_back.png"),new Vector3(0,1.1f,-5));
    // Sample the original unarmed idle directly, bypassing sword stance and IK.
    ProductionModelCatalog.Current.Player(female).idle.SampleAnimation(hero.root.gameObject,0);
    RawGearCapture(Path.Combine(folder,label+"_shoulder_relaxed.png"),new Vector3(0,1.1f,5));
    RawGearCapture(Path.Combine(folder,label+"_shoulder_relaxed_side.png"),new Vector3(3,1.1f,5));
    RawGearCapture(Path.Combine(folder,label+"_shoulder_relaxed_back.png"),new Vector3(0,1.1f,-5));
    hero.swordPivot.gameObject.SetActive(true);
    Require(MaxHealth==hp&&Damage==damage&&Speed==speed,"Visual-only section toggles preserve combat stats");
    EndRun(false);ReturnHome();yield return null;
   }
   foreach(int metal in Enumerable.Range(0,10)){EquipRawGearSample(false,metal);hero.Animate(0,0,0);CheckRawSkinning();EndRun(false);ReturnHome();}
   EquipRawGearSample(false,1);EndRun(false);ReturnHome();activeCharacter=null;InitializeCharacterProfiles(Path.Combine(folder,"preview-account-"+Guid.NewGuid().ToString("N")),true);activeCharacter.name="Copper Veilborne";Save();ResetForCharacter();SetState(ScreenState.CharacterSelect);for(int i=0;i<50;i++)PositionCamera(.1f);yield return new WaitForSecondsRealtime(.3f);yield return CardScreenshot(Path.Combine(folder,"character-select-copper.png"));
   EnterSelectedCharacter();tab=1;yield return new WaitForSecondsRealtime(.2f);yield return CardScreenshot(Path.Combine(folder,"loadout-copper.png"));
   File.WriteAllText(Path.Combine(folder,"PASS.txt"),"PASS: original-source replacement mode; all base/equipped slot toggles; male/female Tin/Copper; all ten palettes; finite weighted deformation through cardinal movement and attacks; greatsword grip; mixed families; complete unequip restoration; unchanged combat stats; actual character-select and loadout previews. Poses: "+poses+". Isolated test profile.");Debug.Log("VEILBOUND_RAW_GEAR_RUNTIME_PASS poses="+poses);Application.Quit(0);
  }
 }
}
