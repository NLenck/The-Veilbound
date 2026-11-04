#if UNITY_EDITOR
using System;using System.IO;using System.Linq;using System.Reflection;using UnityEngine;using UnityEditor;using UnityEngine.SceneManagement;using UnityEditor.SceneManagement;
namespace Veilbound.Editor {
 public static class StarterGearImport {
  const string Root="Assets/Veilbound/Resources/StarterEquipment";
  public static void Run(){
   Directory.CreateDirectory(Root);AssetDatabase.Refresh();
   var importer=(ModelImporter)AssetImporter.GetAtPath(Root+"/GS_Wooden.fbx");importer.importAnimation=false;importer.isReadable=true;importer.materialImportMode=ModelImporterMaterialImportMode.None;importer.SaveAndReimport();
   var texture=(TextureImporter)AssetImporter.GetAtPath(Root+"/WoodenSword_Palette.png");texture.filterMode=FilterMode.Point;texture.textureCompression=TextureImporterCompression.Uncompressed;texture.SaveAndReimport();
   var mat=new Material(Shader.Find("Standard")){name="Worn dry wood",mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/WoodenSword_Palette.png")};mat.SetFloat("_Metallic",0);mat.SetFloat("_Glossiness",.12f);Store(mat,Root+"/Wood.mat");mat=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Wood.mat");
   var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/GS_Wooden.fbx"));var mesh=new Mesh{name="Wooden greatsword - complete"};mesh.CombineMeshes(model.GetComponentsInChildren<MeshFilter>().Select(f=>new CombineInstance{mesh=f.sharedMesh,transform=f.transform.localToWorldMatrix}).ToArray());mesh.RecalculateBounds();
   if(Mathf.Abs(mesh.vertices.Max(v=>v.z)-1.72f)>.02f)throw new Exception("Wooden sword axis/scale mismatch");Store(mesh,Root+"/Wood.asset");mesh=AssetDatabase.LoadAssetAtPath<Mesh>(Root+"/Wood.asset");UnityEngine.Object.DestroyImmediate(model);
   var root=new GameObject("Wooden greatsword");root.AddComponent<MeshFilter>().sharedMesh=AssetDatabase.LoadAssetAtPath<Mesh>(Root+"/Wood.asset");root.AddComponent<MeshRenderer>().sharedMaterial=mat;
   foreach(var p in new[]{("MainHand_Grip",new Vector3(0,.035f,.015f)),("OffHand_Grip",new Vector3(0,.035f,.17f)),("Blade_Tip",new Vector3(0,0,1.72f))}){var t=new GameObject(p.Item1).transform;t.SetParent(root.transform,false);t.localPosition=p.Item2;}
   PrefabUtility.SaveAsPrefabAsset(root,Root+"/WoodenGreatsword.prefab");UnityEngine.Object.DestroyImmediate(root);
   int changed=0;
   foreach(string folder in new[]{"Assets/VeilboundPlateMasters/Generated","Assets/VeilboundGreatswords/Generated"})foreach(string guid in AssetDatabase.FindAssets("t:Material",new[]{folder})){
    var m=AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));if(m.HasProperty("_GlossMapScale"))m.SetFloat("_GlossMapScale",.45f);if(m.HasProperty("_Glossiness"))m.SetFloat("_Glossiness",.20f);EditorUtility.SetDirty(m);changed++;
   }
   AssetDatabase.SaveAssets();
   var fresh=(Settlement)typeof(VeilGame).GetMethod("FreshCharacterTown",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,null);
   Require(fresh.IsWoodenSword&&fresh.blacksmith==0&&fresh.equippedGear.All(x=>x==0)&&fresh.ownedPlate.All(x=>!x)&&fresh.ownedMetalWeapons.All(x=>!x),"New character owns only wood and no armour");
   Require(fresh.craftedGear.Count==1&&fresh.EquippedRoll(9)?.item==-1&&fresh.WeaponDamage==10&&fresh.EquipmentArmour==0&&GearRollRules.BaseName(fresh.EquippedRoll(9))=="Wooden greatsword","Wooden inventory and base stats");
   string storage="StarterGearUpdate/SaveRoundTrip";var store=new CharacterSaveStore(storage);var roster=store.LoadRoster();var character=SavedCharacter.From("Starter QA",fresh);store.Save(character,fresh,roster);store.Load(character.id,out _,out var loaded);Require(loaded.IsWoodenSword&&loaded.Item(2)==0&&loaded.EquippedRoll(9)?.item==-1,"Wooden loadout survives actual save files");
   var legacy=new Settlement{armour=2,equippedArmour=2,sword=2,equippedSword=2};legacy.EnsureGearRolls();Require(!legacy.IsWoodenSword&&legacy.Item(2)==32&&legacy.WeaponMetalIndex==5,"Existing legacy progression preserved");
   loaded.ownedMetalWeapons[0]=true;var tin=loaded.RecordCraft(9,0);Require(loaded.EquipRoll(tin.id)&&!loaded.IsWoodenSword,"Equip metal upgrade");Require(loaded.EquipRoll(loaded.craftedGear.First(g=>g.item==-1).id)&&loaded.IsWoodenSword,"Re-equip owned wood");
   foreach(bool female in new[]{false,true}){fresh.female=female;var rig=new HeroRig(null,fresh);try{Require(rig.swordPivot.GetComponentsInChildren<MeshRenderer>().Length==1,"Held wood has one renderer");var equipment=rig.root.GetComponent<ProductionHeroVisual>().equipment;Require(equipment.activeSlots==0,"All armour masks disabled");Render(rig,female?"Female_Starter":"Male_Starter");}finally{rig.Release();UnityEngine.Object.DestroyImmediate(rig.root.gameObject);}}
   File.WriteAllText("StarterGearUpdate/validation.json",JsonUtility.ToJson(new Result{passed=true,woodTriangles=mesh.triangles.Length/3,materialsUpdated=changed},true));
   // Refresh the ten-sword Unity review with the reduced smoothness.
   Veilbound.Greatswords.Editor.GreatswordImport.Run();Debug.Log("VEILBOUND_STARTER_GEAR_PASS");
  }
  [Serializable]class Result{public bool passed;public int woodTriangles,materialsUpdated;}
  static void Store(UnityEngine.Object asset,string path){var old=AssetDatabase.LoadMainAssetAtPath(path);if(old){EditorUtility.CopySerialized(asset,old);UnityEngine.Object.DestroyImmediate(asset);EditorUtility.SetDirty(old);}else AssetDatabase.CreateAsset(asset,path);}
  static void Require(bool value,string label){if(!value)throw new Exception(label);}
  static void Render(HeroRig rig,string name){
   rig.SetMotion(Vector3.zero);rig.Animate(0,0,0);
   var skins=rig.root.GetComponentsInChildren<SkinnedMeshRenderer>();var baked=new System.Collections.Generic.List<Mesh>();
   foreach(var skin in skins){if(!skin.enabled||!skin.gameObject.activeInHierarchy)continue;var mesh=new Mesh();skin.BakeMesh(mesh);var go=new GameObject("Pose preview");go.transform.SetParent(skin.transform,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterials=skin.sharedMaterials;skin.enabled=false;baked.Add(mesh);}
   var camera=new GameObject("Starter camera").AddComponent<Camera>();camera.enabled=false;camera.orthographic=true;camera.orthographicSize=1.5f;camera.aspect=.8f;var centre=new Vector3(0,1,0);camera.transform.position=centre+new Vector3(.9f,.2f,4);camera.transform.LookAt(centre);camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.11f,.13f,.15f);
   var light=new GameObject("Starter key").AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.1f;light.transform.rotation=Quaternion.Euler(30,150,0);RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;RenderSettings.ambientLight=new Color(.4f,.4f,.4f);RenderSettings.fog=false;
   var target=new RenderTexture(800,1000,24);camera.targetTexture=target;camera.Render();var prior=RenderTexture.active;RenderTexture.active=target;var image=new Texture2D(800,1000,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,800,1000),0,0);image.Apply();File.WriteAllBytes("StarterGearUpdate/"+name+".png",image.EncodeToPNG());RenderTexture.active=prior;target.Release();UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(camera.gameObject);UnityEngine.Object.DestroyImmediate(light.gameObject);foreach(var mesh in baked)UnityEngine.Object.DestroyImmediate(mesh);
  }
 }
}
#endif

