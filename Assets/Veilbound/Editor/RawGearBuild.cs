#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using Veilbound.PlateMasters;
namespace Veilbound.Editor {
 public static class RawGearBuild {
  const string Root="Assets/VeilboundRawGear",Old="Assets/VeilboundPlateMasters/Generated";
  static readonly string[] Slots={"Head","Shoulders","Chest","Hands","Belt","Bracers","Legs","Feet","Cape"};
  static readonly string[] Tiers={"Tin","Copper","Bronze","Brass","Iron","Steel","HardenedSteel","Tungsten","Titanium","Veilcrystal"};
  static readonly int[] Ids={100,101,102,103,104,105,106,108,107,109};
  static void Need(bool value,string message){if(!value)throw new Exception("Raw gear: "+message);}
  static T Store<T>(T obj,string path) where T:UnityEngine.Object {
   var previous=AssetDatabase.LoadAssetAtPath<T>(path);if(previous){
    if(obj is Mesh from&&previous is Mesh to){
     to.ClearBlendShapes();
     to.Clear();to.indexFormat=from.indexFormat;to.vertices=from.vertices;to.normals=from.normals;to.uv=from.uv;to.uv2=from.uv2;to.tangents=from.tangents;to.colors=from.colors;to.boneWeights=from.boneWeights;to.bindposes=from.bindposes;to.subMeshCount=from.subMeshCount;for(int s=0;s<from.subMeshCount;s++)to.SetTriangles(from.GetTriangles(s),s,false);
     for(int shape=0;shape<from.blendShapeCount;shape++)for(int frame=0;frame<from.GetBlendShapeFrameCount(shape);frame++){var dv=new Vector3[from.vertexCount];var dn=new Vector3[dv.Length];var dt=new Vector3[dv.Length];from.GetBlendShapeFrameVertices(shape,frame,dv,dn,dt);to.AddBlendShapeFrame(from.GetBlendShapeName(shape),from.GetBlendShapeFrameWeight(shape,frame),dv,dn,dt);}to.bounds=from.bounds;
    }else EditorUtility.CopySerialized(obj,previous);
    UnityEngine.Object.DestroyImmediate(obj);EditorUtility.SetDirty(previous);return previous;
   }
   AssetDatabase.CreateAsset(obj,path);return obj;
  }
  static Mesh Combine(SkinnedMeshRenderer[] sources,Transform target,Transform[] bones,string name){
   var names=bones.Select((b,i)=>(b.name,i)).ToDictionary(x=>x.name,x=>x.i);var vertices=new List<Vector3>();var normals=new List<Vector3>();var uv=new List<Vector2>();var weights=new List<BoneWeight>();var tris=new List<int>();
   foreach(var r in sources){var m=r.sharedMesh;var matrix=target.worldToLocalMatrix*r.localToWorldMatrix;var normal=matrix.inverse.transpose;int offset=vertices.Count;var p=m.vertices;var n=m.normals;var tex=m.uv;var skin=m.boneWeights;
    int Map(int i)=>names[r.bones[i].name];
    for(int i=0;i<p.Length;i++){vertices.Add(matrix.MultiplyPoint3x4(p[i]));normals.Add(normal.MultiplyVector(n[i]).normalized);uv.Add(tex.Length>i?tex[i]:Vector2.zero);var w=skin[i];w.boneIndex0=Map(w.boneIndex0);w.boneIndex1=Map(w.boneIndex1);w.boneIndex2=Map(w.boneIndex2);w.boneIndex3=Map(w.boneIndex3);weights.Add(w);}
    foreach(int i in m.triangles)tris.Add(offset+i);
   }
   // Explicit per-triangle normals retain source facets without changing positions or skin-weight correspondence in FBX.
   var flatV=new List<Vector3>();var flatN=new List<Vector3>();var flatUV=new List<Vector2>();var flatW=new List<BoneWeight>();
   for(int i=0;i<tris.Count;i+=3){Vector3 normal=Vector3.Cross(vertices[tris[i+1]]-vertices[tris[i]],vertices[tris[i+2]]-vertices[tris[i]]).normalized;for(int j=0;j<3;j++){int source=tris[i+j];flatV.Add(vertices[source]);flatN.Add(normal);flatUV.Add(uv[source]);flatW.Add(weights[source]);}}
   vertices=flatV;normals=flatN;uv=flatUV;weights=flatW;tris=Enumerable.Range(0,vertices.Count).ToList();
   var mesh=new Mesh{name=name};if(vertices.Count>65535)mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;
   mesh.SetVertices(vertices);mesh.SetNormals(normals);mesh.SetUVs(0,uv);mesh.SetTriangles(tris,0);mesh.boneWeights=weights.ToArray();mesh.bindposes=bones.Select(b=>b.worldToLocalMatrix*target.localToWorldMatrix).ToArray();mesh.RecalculateBounds();return mesh;
  }
  static int BoneRegion(string name){
   if(name.Contains("Head"))return 0;if(name.Contains("Hand"))return 3;if(name.Contains("ForeArm"))return 5;if(name.Contains("Shoulder")||name.EndsWith("Arm",StringComparison.Ordinal))return 1;
   if(name.Contains("Foot")||name.Contains("Toe"))return 7;if(name.Contains("Leg"))return 6;if(name.Contains("Hips"))return 4;return 2;
  }
  static Mesh[] SplitBody(Mesh source,Transform[] bones,string gender,Transform body){
   int[] boneRegion=bones.Select(b=>BoneRegion(b.name)).ToArray();var weights=source.boneWeights;var bySlot=Enumerable.Range(0,8).Select(_=>Enumerable.Range(0,source.subMeshCount).Select(__=>new List<int>()).ToArray()).ToArray();
   for(int sub=0;sub<source.subMeshCount;sub++){var tris=source.GetTriangles(sub);for(int i=0;i<tris.Length;i+=3){var scores=new float[8];for(int j=0;j<3;j++){var w=weights[tris[i+j]];scores[boneRegion[w.boneIndex0]]+=w.weight0;scores[boneRegion[w.boneIndex1]]+=w.weight1;scores[boneRegion[w.boneIndex2]]+=w.weight2;scores[boneRegion[w.boneIndex3]]+=w.weight3;}
    int slot=Array.IndexOf(scores,scores.Max());for(int j=0;j<3;j++)bySlot[slot][sub].Add(tris[i+j]);}}
   var result=new Mesh[8];var v=source.vertices;var n=source.normals;var u=source.uv;var u2=source.uv2;var tangents=source.tangents;var colors=source.colors;
   for(int slot=0;slot<8;slot++){
    var used=bySlot[slot].SelectMany(t=>t).Distinct().OrderBy(i=>i).ToArray();Need(used.Length>0,"base region missing "+Slots[slot]);var remap=used.Select((value,index)=>(value,index)).ToDictionary(x=>x.value,x=>x.index);
    var mesh=new Mesh{name=gender+"_Base_"+Slots[slot]};mesh.vertices=used.Select(i=>v[i]).ToArray();if(n.Length==v.Length)mesh.normals=used.Select(i=>n[i]).ToArray();if(u.Length==v.Length)mesh.uv=used.Select(i=>u[i]).ToArray();if(u2.Length==v.Length)mesh.uv2=used.Select(i=>u2[i]).ToArray();if(tangents.Length==v.Length)mesh.tangents=used.Select(i=>tangents[i]).ToArray();if(colors.Length==v.Length)mesh.colors=used.Select(i=>colors[i]).ToArray();
    mesh.boneWeights=used.Select(i=>weights[i]).ToArray();mesh.bindposes=source.bindposes;mesh.subMeshCount=source.subMeshCount;
    for(int sub=0;sub<source.subMeshCount;sub++)mesh.SetTriangles(bySlot[slot][sub].Select(i=>remap[i]).ToArray(),sub,false);
    for(int shape=0;shape<source.blendShapeCount;shape++)for(int frame=0;frame<source.GetBlendShapeFrameCount(shape);frame++){var dv=new Vector3[v.Length];var dn=new Vector3[v.Length];var dt=new Vector3[v.Length];source.GetBlendShapeFrameVertices(shape,frame,dv,dn,dt);mesh.AddBlendShapeFrame(source.GetBlendShapeName(shape),source.GetBlendShapeFrameWeight(shape,frame),used.Select(i=>dv[i]).ToArray(),used.Select(i=>dn[i]).ToArray(),used.Select(i=>dt[i]).ToArray());}
    if(slot==6){
     // The raw tassets overlap the baggy upper trousers. Tuck only this soft
     // base garment while a belt is equipped, retaining the original armour.
     var delta=new Vector3[used.Length];var zero=new Vector3[used.Length];Vector3 hip=bones.First(b=>b.name=="mixamorig:Hips").position;
     float factor=gender=="Female"?1.73f/1.85f:1;
     for(int i=0;i<used.Length;i++){
      Vector3 point=body.TransformPoint(v[used[i]]);
      float lower=Mathf.SmoothStep(0,1,Mathf.InverseLerp(hip.y-.43f*factor,hip.y-.23f*factor,point.y));
      float upper=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(hip.y+.02f*factor,hip.y+.15f*factor,point.y));
      Vector3 radial=Vector3.ProjectOnPlane(point-hip,Vector3.up);
      delta[i]=body.InverseTransformVector(-radial*(.22f*lower*upper));
     }
     mesh.AddBlendShapeFrame("RawBeltTuck",100,delta,zero,zero);
    }
    mesh.RecalculateBounds();result[slot]=Store(mesh,Root+"/Generated/"+mesh.name+".asset");
   }
   Need(result.Sum(m=>m.triangles.Length)==source.triangles.Length,"body partition must preserve every triangle exactly once");return result;
  }
  public static void Run(){
   Directory.CreateDirectory(Root+"/Generated");AssetDatabase.Refresh();var tierPrefabs=new List<GameObject>();var catalog=AssetDatabase.LoadAssetAtPath<ProductionModelCatalog>("Assets/Veilbound/Resources/ProductionModels/Catalog.asset");Need(catalog,"production catalog");
   foreach(string gender in new[]{"Male","Female"}){
    // Always clone the backed-up implementation's untouched original prefab.
    var character=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Old+"/Tin_"+gender+"_Master.prefab"));var imports=new List<GameObject>();
    try{
     var equipment=character.GetComponentInChildren<MasterPlateEquipment>();var body=equipment.body;var bones=body.bones;var source=equipment.fullBodySource;
     equipment.replacementMode=true;equipment.baseRegions=new SkinnedMeshRenderer[8];var baseMeshes=SplitBody(source,bones,gender,body.transform);
     for(int slot=0;slot<8;slot++){var go=new GameObject("Base region - "+Slots[slot]);go.transform.SetParent(body.transform,false);var r=go.AddComponent<SkinnedMeshRenderer>();r.sharedMesh=baseMeshes[slot];r.sharedMaterials=body.sharedMaterials;r.bones=bones;r.rootBone=body.rootBone;r.updateWhenOffscreen=true;equipment.baseRegions[slot]=r;}
     equipment.familySlotMeshes=new Mesh[18];
     for(int family=0;family<2;family++){
      string tier=family==0?"Tin":"Copper",path=Root+"/Models/"+gender+"/"+tier+"_"+gender+"_RawSlots.fbx";var importer=(ModelImporter)AssetImporter.GetAtPath(path);Need(importer!=null,"raw FBX "+path);importer.importAnimation=false;importer.isReadable=true;importer.meshCompression=ModelImporterMeshCompression.Off;importer.materialImportMode=ModelImporterMaterialImportMode.None;importer.SaveAndReimport();
      var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path));imports.Add(model);var renderers=model.GetComponentsInChildren<SkinnedMeshRenderer>();Need(renderers.Length>=14,"all original parts imported");
      for(int slot=0;slot<9;slot++){
       int index=slot;var pieces=renderers.Where(r=>Regex.IsMatch(r.name,"_S"+index+"_")).ToArray();Need(pieces.Length>0,"raw slot missing "+tier+" "+Slots[slot]);var target=equipment.slots[slot];target.bones=bones;target.rootBone=body.rootBone;target.updateWhenOffscreen=true;
       string name=tier+"_"+gender+"_Replacement_"+Slots[slot];equipment.familySlotMeshes[family*9+slot]=Store(Combine(pieces,target.transform,bones,name),Root+"/Generated/"+name+".asset");
      }
     }
     body.sharedMesh=source;body.enabled=false;
     for(int i=0;i<Tiers.Length;i++){
      equipment.EquipWholeTier(Ids[i]);character.name=Tiers[i]+"_"+gender+"_RawReplacement";var prefab=PrefabUtility.SaveAsPrefabAsset(character,Root+"/Generated/"+character.name+".prefab");tierPrefabs.Add(prefab);
      if(i==0)catalog.players.First(p=>p.gender==gender).prefab=prefab;
     }
    }finally{foreach(var obj in imports)UnityEngine.Object.DestroyImmediate(obj);UnityEngine.Object.DestroyImmediate(character);}
   }
   catalog.tierPrefabs=tierPrefabs.ToArray();EditorUtility.SetDirty(catalog);AssetDatabase.SaveAssets();AssetDatabase.Refresh();Validate();Debug.Log("VEILBOUND_RAW_GEAR_IMPORT_PASS");
  }
  public static void Validate(){
   var catalog=AssetDatabase.LoadAssetAtPath<ProductionModelCatalog>("Assets/Veilbound/Resources/ProductionModels/Catalog.asset");int cases=0;
   foreach(var player in catalog.players){var obj=(GameObject)PrefabUtility.InstantiatePrefab(player.prefab);try{
    var equipment=obj.GetComponentInChildren<MasterPlateEquipment>();Need(equipment.replacementMode&&equipment.baseRegions.Length==8,"replacement prefab active");
    for(int mask=0;mask<512;mask++){
     for(int slot=0;slot<9;slot++)equipment.SetSlotEnabled(slot,(mask&(1<<slot))!=0);
     Need(!equipment.body.enabled,"full body renderer stays disabled");for(int slot=0;slot<8;slot++)Need(equipment.baseRegions[slot].gameObject.activeSelf!=equipment.slots[slot].gameObject.activeSelf,"exactly one base/equipped section visible");cases++;
     for(int regionSlot=0;regionSlot<equipment.baseRegions.Length;regionSlot++){var region=equipment.baseRegions[regionSlot];for(int shape=0;shape<region.sharedMesh.blendShapeCount;shape++)if(region.sharedMesh.GetBlendShapeName(shape).Contains("BeltTuck"))Need(Mathf.Abs(region.GetBlendShapeWeight(shape)-((regionSlot==4||regionSlot==6)&&(mask&(1<<4))!=0?100:0))<.01f,"belt tucks only waist/trousers and preserves exposed limbs");}
    }
    foreach(int item in Ids)for(int slot=0;slot<9;slot++){equipment.SetSlotTier(slot,item);var mesh=equipment.slots[slot].sharedMesh;Need(mesh!=null&&mesh.triangles.Length>0&&mesh.boneWeights.Length==mesh.vertexCount,"valid bound slot "+slot);Need(mesh.vertices.All(p=>float.IsFinite(p.x)&&float.IsFinite(p.y)&&float.IsFinite(p.z)&&p.sqrMagnitude<100),"finite raw mesh");}
   }finally{UnityEngine.Object.DestroyImmediate(obj);}}
   File.WriteAllText("GearReplacementUpdate/editor-validation.json","{\"passed\":true,\"variants\":2,\"visibilityCombinations\":"+cases+",\"tiers\":10,\"slots\":9}");Debug.Log("VEILBOUND_RAW_GEAR_EDITOR_PASS "+cases);
  }
  public static void Build(){Run();PrototypeBuild.Checks();AssetDatabase.SaveAssets();var result=BuildPipeline.BuildPlayer(new[]{"Assets/Veilbound/Scenes/HollowCourt.unity"},"Builds/Integrated/TheVeilbound.exe",BuildTarget.StandaloneWindows64,BuildOptions.Development);Need(result.summary.result==BuildResult.Succeeded,"Windows player build");Debug.Log("VEILBOUND_RAW_GEAR_BUILD_PASS");}
 }
}
#endif
