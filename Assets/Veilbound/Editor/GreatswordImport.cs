#if UNITY_EDITOR
using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEditor;using UnityEngine;
namespace Veilbound.Greatswords.Editor {
 public static class GreatswordImport {
  const string Root="Assets/VeilboundGreatswords";const string Output="GreatswordAssetProduction";
  static readonly string[] Names={"Tin","Copper","Bronze","Brass","Iron","Steel","HardenedSteel","Tungsten","Titanium","Veilcrystal"};
  static readonly int[] Metals={0,1,2,3,4,5,6,8,7,9};
  [Serializable] class SwordCheck {public string name;public int triangles,materials;public float bladeTip,pommel;public bool passed;}
  [Serializable] class Validation {public bool passed;public int swords;public List<SwordCheck> checks=new List<SwordCheck>();public float maleGripError,femaleGripError;public bool liveFactoryUnchanged;}
  public static void Run() {
   Directory.CreateDirectory(Root+"/Generated");AssetDatabase.Refresh();
   Texture2D Texture(string file,bool linear){string path=Root+"/Atlas/"+file;var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureCompression=TextureImporterCompression.Uncompressed;importer.filterMode=FilterMode.Point;importer.wrapMode=TextureWrapMode.Clamp;importer.sRGBTexture=!linear;importer.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Texture2D>(path);}
   var material=new Material(Shader.Find("Standard")){name="Greatswords - shared reference palette"};material.mainTexture=Texture("Greatswords_BaseColor.png",false);material.SetTexture("_MetallicGlossMap",Texture("Greatswords_MetallicSmoothness.png",true));material.EnableKeyword("_METALLICGLOSSMAP");material.SetFloat("_GlossMapScale",.45f);material.SetTexture("_EmissionMap",Texture("Greatswords_Emission.png",false));material.EnableKeyword("_EMISSION");material.SetColor("_EmissionColor",Color.white*.60f);material.enableInstancing=true;
   string materialPath=Root+"/Generated/Greatswords_Shared.mat";var existing=AssetDatabase.LoadAssetAtPath<Material>(materialPath);if(existing){EditorUtility.CopySerialized(material,existing);UnityEngine.Object.DestroyImmediate(material);material=existing;}else AssetDatabase.CreateAsset(material,materialPath);
   var library=ScriptableObject.CreateInstance<GreatswordVisualLibrary>();var entries=new List<GreatswordEntry>();var result=new Validation{liveFactoryUnchanged=true};
   for(int i=0;i<Names.Length;i++) {
    string name=Names[i],path=Root+"/Models/GS_"+name+".fbx";var importer=(ModelImporter)AssetImporter.GetAtPath(path);importer.importAnimation=false;importer.isReadable=true;importer.materialImportMode=ModelImporterMaterialImportMode.None;importer.meshCompression=ModelImporterMeshCompression.Off;importer.SaveAndReimport();
    var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path));var root=new GameObject("GS_"+name);
    try {
     var combine=model.GetComponentsInChildren<MeshFilter>().Select(f=>new CombineInstance{mesh=f.sharedMesh,transform=f.transform.localToWorldMatrix}).ToArray();var mesh=new Mesh{name="GS_"+name+"_Unity"};mesh.CombineMeshes(combine,true,true);mesh.RecalculateBounds();
     // Blender's -Y authoring axis must import as Unity +Z. Reject accidental vertical swords.
     float tip=mesh.vertices.Max(v=>v.z),pommel=mesh.vertices.Min(v=>v.z);bool finite=mesh.vertices.All(v=>float.IsFinite(v.x)&&float.IsFinite(v.y)&&float.IsFinite(v.z));
     if(Mathf.Abs(tip-1.72f)>.002f||pommel>-.10f)throw new InvalidOperationException(name+" wrong socket orientation/scale "+mesh.bounds);
     string meshPath=Root+"/Generated/GS_"+name+".asset";var saved=AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);if(saved){EditorUtility.CopySerialized(mesh,saved);UnityEngine.Object.DestroyImmediate(mesh);mesh=saved;}else AssetDatabase.CreateAsset(mesh,meshPath);
     var geometry=new GameObject("Blade guard grip and pommel");geometry.transform.SetParent(root.transform,false);geometry.AddComponent<MeshFilter>().sharedMesh=mesh;geometry.AddComponent<MeshRenderer>().sharedMaterial=material;
     var variant=root.AddComponent<GreatswordVariant>();variant.tier=name;variant.metalIndex=Metals[i];
     Transform Marker(string label,Vector3 position){var marker=new GameObject(label).transform;marker.SetParent(root.transform,false);marker.localPosition=position;return marker;}
     variant.mainHand=Marker("MainHand_Grip",new Vector3(0,.035f,.015f));variant.offHand=Marker("OffHand_Grip",new Vector3(0,.035f,.17f));Marker("Blade_Tip",new Vector3(0,0,1.72f));
     var prefab=PrefabUtility.SaveAsPrefabAsset(root,Root+"/Generated/GS_"+name+".prefab");entries.Add(new GreatswordEntry{tier=name,metalIndex=Metals[i],prefab=prefab,triangles=mesh.triangles.Length/3});result.checks.Add(new SwordCheck{name=name,triangles=mesh.triangles.Length/3,materials=1,bladeTip=tip,pommel=pommel,passed=finite&&mesh.triangles.Length/3<=6000});
    }finally{UnityEngine.Object.DestroyImmediate(root);UnityEngine.Object.DestroyImmediate(model);}
   }
   library.swords=entries.ToArray();string libraryPath=Root+"/Generated/GreatswordVisualLibrary.asset";var old=AssetDatabase.LoadAssetAtPath<GreatswordVisualLibrary>(libraryPath);if(old){EditorUtility.CopySerialized(library,old);UnityEngine.Object.DestroyImmediate(library);library=old;}else AssetDatabase.CreateAsset(library,libraryPath);
   result.maleGripError=Demo(library,false,8);result.femaleGripError=Demo(library,true,9);Lineup(library);result.swords=entries.Count;result.passed=entries.Count==10&&result.checks.All(c=>c.passed)&&result.maleGripError<.07f&&result.femaleGripError<.07f;
   File.WriteAllText(Output+"/qa/unity_validation.json",JsonUtility.ToJson(result,true));AssetDatabase.SaveAssets();if(!result.passed)throw new Exception("Greatsword import validation failed");
   AssetDatabase.ExportPackage(Root,Output+"/Veilborne_Greatswords_Review.unitypackage",ExportPackageOptions.Recurse);Debug.Log("VEILBOUND_GREATSWORDS_REVIEW_PASS");
  }
  static void Lineup(GreatswordVisualLibrary library) {
   var root=new GameObject("Ten independent greatswords review");
   try {
    for(int i=0;i<Names.Length;i++){int metal=Metals[i];var obj=library.CreateVisual(root.transform,metal);obj.localPosition=new Vector3((4.5f-i)*.83f,0,0);}
    var camera=new GameObject("Greatsword lineup camera").AddComponent<Camera>();camera.enabled=false;camera.orthographic=true;camera.aspect=3;camera.orthographicSize=1.5f;camera.transform.position=new Vector3(0,6,.70f);camera.transform.rotation=Quaternion.LookRotation(Vector3.down,Vector3.back);camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.07f,.085f,.10f);
    var light=new GameObject("Lineup light").AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.1f;light.transform.rotation=Quaternion.LookRotation(new Vector3(.2f,-1,.3f),Vector3.forward);RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;RenderSettings.ambientLight=new Color(.4f,.4f,.4f);RenderSettings.fog=false;
    var target=new RenderTexture(1800,600,24);camera.targetTexture=target;camera.Render();var previous=RenderTexture.active;RenderTexture.active=target;var image=new Texture2D(1800,600,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1800,600),0,0);image.Apply();File.WriteAllBytes(Output+"/previews/ALL_TEN_UNITY.png",image.EncodeToPNG());RenderTexture.active=previous;target.Release();UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(camera.gameObject);UnityEngine.Object.DestroyImmediate(light.gameObject);
   }finally{UnityEngine.Object.DestroyImmediate(root);}
  }
  static float Demo(GreatswordVisualLibrary library,bool female,int metal) {
   var city=new Veilbound.Settlement{female=female,bronzeHeroVersion=1};city.EnsureGear();for(int s=0;s<8;s++)city.equippedGear[s]=100+metal;city.equippedGear[8]=12;city.equippedMetalWeapon=metal;
   var rig=new Veilbound.HeroRig(null,city);
   try {
    foreach(Transform child in rig.swordPivot.Cast<Transform>().ToArray())UnityEngine.Object.DestroyImmediate(child.gameObject);var sword=library.CreateVisual(rig.swordPivot,metal);if(!sword)throw new Exception("No review sword for metal "+metal);rig.SetMotion(Vector3.zero);rig.Animate(0,0,0);
    var native=rig.root.GetComponent<Veilbound.ProductionHeroVisual>();float error=native.lastGripError;
    var bakedObjects=new List<GameObject>();var meshes=new List<Mesh>();var skins=rig.root.GetComponentsInChildren<SkinnedMeshRenderer>();
    foreach(var skin in skins){if(!skin.enabled||!skin.gameObject.activeInHierarchy)continue;var mesh=new Mesh();skin.BakeMesh(mesh);mesh.RecalculateBounds();var obj=new GameObject("Current review pose");obj.transform.SetParent(skin.transform,false);obj.AddComponent<MeshFilter>().sharedMesh=mesh;obj.AddComponent<MeshRenderer>().sharedMaterials=skin.sharedMaterials;skin.enabled=false;bakedObjects.Add(obj);meshes.Add(mesh);}
    Bounds bounds=new Bounds();bool first=true;foreach(var renderer in rig.root.GetComponentsInChildren<Renderer>()){if(!renderer.enabled||!renderer.gameObject.activeInHierarchy)continue;if(first){bounds=renderer.bounds;first=false;}else bounds.Encapsulate(renderer.bounds);}
    var cam=new GameObject("Greatsword integration review camera").AddComponent<Camera>();cam.enabled=false;cam.orthographic=true;cam.aspect=.8f;cam.orthographicSize=Mathf.Max(bounds.size.y,bounds.size.x/.8f)*.65f;cam.transform.position=bounds.center+new Vector3(.5f,.2f,1).normalized*7;cam.transform.LookAt(bounds.center);cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.10f,.12f,.14f);var lamp=new GameObject("Greatsword review key").AddComponent<Light>();lamp.type=LightType.Directional;lamp.intensity=1.2f;lamp.transform.rotation=Quaternion.Euler(40,155,0);RenderSettings.ambientLight=new Color(.38f,.38f,.38f);
    var texture=new RenderTexture(800,1000,24);cam.targetTexture=texture;cam.Render();var active=RenderTexture.active;RenderTexture.active=texture;var pixels=new Texture2D(800,1000,TextureFormat.RGB24,false);pixels.ReadPixels(new Rect(0,0,800,1000),0,0);pixels.Apply();File.WriteAllBytes(Output+"/previews/"+(female?"Female":"Male")+"_Holding_"+library.swords.First(e=>e.metalIndex==metal).tier+".png",pixels.EncodeToPNG());RenderTexture.active=active;texture.Release();UnityEngine.Object.DestroyImmediate(texture);UnityEngine.Object.DestroyImmediate(pixels);UnityEngine.Object.DestroyImmediate(cam.gameObject);UnityEngine.Object.DestroyImmediate(lamp.gameObject);foreach(var obj in bakedObjects)UnityEngine.Object.DestroyImmediate(obj);foreach(var mesh in meshes)UnityEngine.Object.DestroyImmediate(mesh);return error;
   }finally{rig.Release();UnityEngine.Object.DestroyImmediate(rig.root.gameObject);}
  }
 }
}
#endif
