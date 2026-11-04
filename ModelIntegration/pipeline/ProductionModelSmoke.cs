using System;
using System.IO;
using System.Linq;
using System.Collections;
using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  [Serializable] class ProductionRuntimeAudit {public bool passed,appearanceSave,combat,bodyRestore;public int players,armourLooks,enemies,maps;public double crowdFps;}
  IEnumerator ProductionModelsReview() {
   string folder=Path.Combine(Application.dataPath,"..","Smoke","ProductionModels");Directory.CreateDirectory(folder);combatReviewFreeze=true;
   yield return new WaitForSecondsRealtime(.4f);ReturnHome();var audit=new ProductionRuntimeAudit();var catalog=ProductionModelCatalog.Current;
   Require(catalog&&catalog.players.Length==2&&catalog.enemies.Length==21,"All created player and enemy models are in the game catalog");
   CheckCharacterStore(folder);
   var saveTest=new CharacterSaveStore(Path.Combine(folder,"appearance-save-"+Guid.NewGuid().ToString("N")));var rosterTest=saveTest.LoadRoster();var femaleTown=FreshCharacterTown();femaleTown.female=true;var femaleSave=SavedCharacter.From("Female appearance test",femaleTown);saveTest.Save(femaleSave,femaleTown,rosterTest);saveTest.Load(femaleSave.id,out var loaded,out var town);
   audit.appearanceSave=loaded.female&&town.female;Require(audit.appearanceSave,"Female appearance survives character/town save round trip");
   foreach(bool female in new[]{false,true}) {
    city=new Settlement{female=female,bronzeHeroVersion=1,blacksmith=9,tailor=2,leatherworker=2};city.EnsureGear();city.EnsureGearRolls();
    for(int metal=0;metal<10;metal++) {
     for(int slot=0;slot<8;slot++){city.ownedPlate[slot*10+metal]=true;city.equippedGear[slot]=100+metal;}city.equippedGear[8]=12;city.equippedMetalWeapon=metal;city.ownedMetalWeapons[metal]=true;
     RebuildHero();var visual=hero.root.GetComponent<ProductionHeroVisual>();Require(visual&&visual.female==female&&hero.skin.bones.Length==52,"Live character uses the selected 52-bone model");
     hero.SetMotion(Vector3.forward*3.6f);hero.Animate(.23f+metal*.11f,1,.4f);
     Require(visual.lastGripError<.07f,"Both hands reach the equipped sword grips");Require(visual.equipment.activeSlots==511&&visual.equipment.slots.All(s=>s.gameObject.activeSelf&&s.sharedMaterials.Length==1),"Nine independently equipped single-material slots");
     foreach(var skin in hero.root.GetComponentsInChildren<SkinnedMeshRenderer>())ValidateProductionSkin(skin);
     hero.SetMotion(Vector3.zero);hero.NeutralPose(0);hero.swordPivot.gameObject.SetActive(false);CaptureProductionModel(hero.root,Path.Combine(folder,(female?"Female":"Male")+"_"+Metals.All[metal].name.Replace(' ','_')+".png"));hero.swordPivot.gameObject.SetActive(true);audit.armourLooks++;
    }
    var native=hero.root.GetComponent<ProductionHeroVisual>();for(int slot=0;slot<9;slot++)native.equipment.SetSlotEnabled(slot,false);
    Require(hero.skin.sharedMesh.triangles.Length==native.equipment.fullBodySource.triangles.Length,"Unequipping all armour restores the complete body");audit.bodyRestore=true;
    hero.NeutralPose(0);hero.swordPivot.gameObject.SetActive(false);CaptureProductionModel(hero.root,Path.Combine(folder,(female?"Female":"Male")+"_Unarmoured.png"));hero.swordPivot.gameObject.SetActive(true);
    city.equippedGear[0]=0;city.equippedGear[2]=11;city.equippedGear[5]=21;city.equippedGear[7]=21;RebuildHero();Require((hero.root.GetComponent<ProductionHeroVisual>().equipment.activeSlots&1)==0,"Mixed cloth/leather and removable helmet remain functional");
    tab=1;cachedEquipmentHero=null;RefreshEquipmentPortrait();Require(equipmentPortrait&&equipmentIcons[2],"Equipment portrait and slot thumbnails work on the new rig");audit.players++;
   }
   foreach(var entry in catalog.enemies) {
    var instance=Instantiate(entry.prefab,actors,false);instance.transform.position=new Vector3(500,0,0);var v=instance.GetComponent<MonsterVisual>();var mesh=v.skin.sharedMesh;
    Require(v.production&&mesh.triangles.Length/3==entry.triangles&&v.bones.Length==17,"Created enemy is locally rigged and retains its production geometry: "+entry.id);
    for(int phase=0;phase<4;phase++){v.Animate(phase*.21f,1,.21f);ValidateProductionSkin(v.skin);}v.Hit();v.FuseGlow(.5f);v.Animate(0,0,0);
    CaptureProductionModel(instance.transform,Path.Combine(folder,entry.id.Replace('/','_')+".png"));Destroy(instance);audit.enemies++;
   }
   combatReviewFreeze=false;
   for(int map=0;map<2;map++) {
    ReturnHome();SelectExpeditionMap(map);shortRun=true;StartRun();invulnerability=999;combatReviewFreeze=true;
    for(int kind=0;kind<5;kind++){var f=SpawnFoe(0,kind);Require(f!=null&&f.t.position.magnitude>ArenaCollision.Boundary-1.6f,"Production enemy still spawns through reachable arena edge");Require(f.rig.visual.production.assetId.StartsWith(map==1?"Town/":"Shrine/")||map==1&&f.rig.visual.production.assetId=="Forest/Wolf","Enemy selection matches the active biome");f.t.position=collision.Resolve(new Vector3(-7+kind*3,0,7),f.radius);f.rig.Hit();}
    var boss=SpawnFoe(1);Require(boss!=null,"Generated biome boss enters through the edge");boss.t.position=collision.Resolve(new Vector3(5,0,3),boss.radius);boss.rig.Animate(.3f,1,.03f);boss.rig.visual.GroundSlamPose(.6f);
    hero.SetMotion(Vector3.forward*3.6f);hero.Animate(.31f,1,.35f);cameraRig.orthographicSize=10;cameraRig.transform.position=hero.root.position+new Vector3(0,17,-14);cameraRig.transform.rotation=Quaternion.Euler(51,0,0);
    CaptureProductionCamera(cameraRig,Path.Combine(folder,map==1?"Town_Gameplay.png":"Shrine_Gameplay.png"),1280,900);
    Hit(boss,999999);Require(bossesDead>=1,"Boss combat and death still resolve with imported models");audit.maps++;
   }
   ReturnHome();StartRun();invulnerability=999;combatReviewFreeze=false;for(int i=0;i<110;i++)SpawnFoe(0,i%5);
   double began=Time.realtimeSinceStartupAsDouble;int frames=Time.frameCount;yield return new WaitForSecondsRealtime(2);audit.crowdFps=(Time.frameCount-frames)/(Time.realtimeSinceStartupAsDouble-began);
   Require(foes.Count>80,"Crowded wave runs with generated enemies");audit.combat=true;ReturnHome();
   audit.passed=audit.players==2&&audit.armourLooks==20&&audit.enemies==21&&audit.maps==2&&audit.bodyRestore&&audit.appearanceSave&&audit.combat;
   File.WriteAllText(Path.Combine(folder,"RUNTIME_VALIDATION.json"),JsonUtility.ToJson(audit,true));Require(audit.passed,"Production integration review completes");File.WriteAllText(Path.Combine(folder,"PASS.txt"),"PASS: two 52-bone players; saved appearances; twenty armour palettes; nine mixed/removable slots; restored bodies; two-hand grips; locomotion and gesture skinning; all 21 enemy meshes and local rigs; Town/Shrine biome spawns and bosses; combat/death; equipment thumbnails; 110-enemy crowd sample.\n");Debug.Log("VEILBOUND_PRODUCTION_RUNTIME_PASS");Application.Quit(0);
  }
  static void ValidateProductionSkin(SkinnedMeshRenderer skin) {var mesh=new Mesh();skin.BakeMesh(mesh);foreach(var v in mesh.vertices)Require(float.IsFinite(v.x)&&float.IsFinite(v.y)&&float.IsFinite(v.z)&&v.sqrMagnitude<400,"Production skin remains finite and bounded");Destroy(mesh);}
  void CaptureProductionModel(Transform root,string path) {
   var layers=root.GetComponentsInChildren<Transform>(true).ToDictionary(t=>t,t=>t.gameObject.layer);foreach(var t in layers.Keys)t.gameObject.layer=29;
   // Bake the current pose for immediate captures: GPU skinning may still contain the previous frame.
   var originalSkins=new System.Collections.Generic.List<SkinnedMeshRenderer>();var bakedObjects=new System.Collections.Generic.List<GameObject>();var bakedMeshes=new System.Collections.Generic.List<Mesh>();
   foreach(var skin in root.GetComponentsInChildren<SkinnedMeshRenderer>()) {
    if(!skin.enabled||!skin.gameObject.activeInHierarchy)continue;
    var mesh=new Mesh();skin.BakeMesh(mesh);mesh.RecalculateBounds();var obj=new GameObject("Current pose capture");obj.layer=29;obj.transform.SetParent(skin.transform,false);obj.AddComponent<MeshFilter>().sharedMesh=mesh;obj.AddComponent<MeshRenderer>().sharedMaterials=skin.sharedMaterials;skin.enabled=false;originalSkins.Add(skin);bakedObjects.Add(obj);bakedMeshes.Add(mesh);
   }
   Bounds bounds=new Bounds();bool found=false;
   foreach(var r in root.GetComponentsInChildren<Renderer>()) {
    if(!r.enabled||!r.gameObject.activeInHierarchy||r is TrailRenderer)continue;Bounds next=r.bounds;
    if(!found){bounds=next;found=true;}else bounds.Encapsulate(next);
   }
   var go=new GameObject("Production offscreen review camera");var cam=go.AddComponent<Camera>();cam.enabled=false;cam.cullingMask=1<<29;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.075f,.085f,.10f);cam.orthographic=true;cam.aspect=.8f;cam.orthographicSize=Mathf.Max(bounds.size.y,bounds.size.x/.8f)*.60f;cam.nearClipPlane=.01f;cam.farClipPlane=30;
   cam.transform.position=bounds.center+new Vector3(.28f,.08f,1).normalized*8;cam.transform.LookAt(bounds.center);
   var lamp=new GameObject("Production review key").AddComponent<Light>();lamp.type=LightType.Directional;lamp.intensity=.9f;lamp.color=new Color(1,.94f,.87f);lamp.cullingMask=1<<29;lamp.transform.rotation=Quaternion.Euler(38,155,0);
   bool fog=RenderSettings.fog;RenderSettings.fog=false;CaptureProductionCamera(cam,path,640,800);RenderSettings.fog=fog;
   foreach(var skin in originalSkins)skin.enabled=true;foreach(var obj in bakedObjects)DestroyImmediate(obj);foreach(var mesh in bakedMeshes)DestroyImmediate(mesh);
   foreach(var t in layers)t.Key.gameObject.layer=t.Value;DestroyImmediate(go);DestroyImmediate(lamp.gameObject);
  }
  static void CaptureProductionCamera(Camera camera,string path,int width,int height) {
   var texture=new RenderTexture(width,height,24);var prior=camera.targetTexture;var active=RenderTexture.active;camera.targetTexture=texture;camera.Render();RenderTexture.active=texture;var pixels=new Texture2D(width,height,TextureFormat.RGB24,false);pixels.ReadPixels(new Rect(0,0,width,height),0,0);pixels.Apply();Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllBytes(path,pixels.EncodeToPNG());camera.targetTexture=prior;RenderTexture.active=active;texture.Release();DestroyImmediate(texture);DestroyImmediate(pixels);
  }
 }
}
