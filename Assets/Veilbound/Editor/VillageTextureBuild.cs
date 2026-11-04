#if UNITY_EDITOR
using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.Build.Reporting;
namespace Veilbound.Editor {
 public static class VillageTextureBuild {
  const string Root="Assets/VeilboundVillageTextures",Resource=Root+"/Resources/VillageTextures";
  [Serializable]class Mapping {public Part[] parts;public bool passed;public float maximum_vertex_error;}
  [Serializable]class Part {public string name,path,image;public int triangles;}
  static void Need(bool condition,string text){if(!condition)throw new Exception("Village textures: "+text);}
  public static void RebuildPlayer(){PrototypeBuild.Checks();var result=BuildPipeline.BuildPlayer(new[]{"Assets/Veilbound/Scenes/HollowCourt.unity"},"Builds/Integrated/TheVeilbound.exe",BuildTarget.StandaloneWindows64,BuildOptions.Development);Need(result.summary.result==BuildResult.Succeeded,"player build");Debug.Log("VILLAGE_TRIPO_BUILD_PASS review updated");}
  public static void PolishPreview(){var catalog=Resources.Load<VillageTextureCatalog>("VillageTextures/Catalog");Need(catalog,"texture catalogue");var parts=catalog.parts.Where(p=>!VillageTextureCatalog.IsEffect(p.path)).ToArray();catalog.parts=Array.Empty<VillageTextureCatalog.Part>();var root=WorldArt.Group("Original village palette");RuinedTownArena.Build(root,new ArenaCollision());catalog.parts=parts;EditorUtility.SetDirty(catalog);foreach(var entry in parts){var source=VillageTextureCatalog.FindPart(root,entry.path,entry.occurrence).GetComponent<MeshRenderer>().sharedMaterial;entry.material.color=source.HasProperty("_Color")?source.color:Color.gray;EditorUtility.SetDirty(entry.material);}UnityEngine.Object.DestroyImmediate(root.gameObject);AssetDatabase.SaveAssets();VillageValidation.Run();RebuildPlayer();}
  public static void Build(){
   string[] args=Environment.GetCommandLineArgs();int arg=Array.IndexOf(args,"-villageTextureInput");Need(arg>=0,"mapping directory");string input=args[arg+1];Directory.CreateDirectory(Resource);Directory.CreateDirectory(Root+"/Meshes");Directory.CreateDirectory(Root+"/Textures");Directory.CreateDirectory(Root+"/Materials");
   var mapping=JsonUtility.FromJson<Mapping>(File.ReadAllText(Path.Combine(input,"mapping.json")));Need(mapping.passed&&mapping.parts.Length==138&&mapping.maximum_vertex_error<.001,"validated original geometry mapping");
   foreach(var part in mapping.parts)File.Copy(Path.Combine(input,part.image),Root+"/Textures/"+part.image,true);AssetDatabase.Refresh();
   foreach(var part in mapping.parts){var importer=(TextureImporter)AssetImporter.GetAtPath(Root+"/Textures/"+part.image);importer.sRGBTexture=true;importer.maxTextureSize=8192;importer.mipmapEnabled=true;importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Bilinear;importer.textureCompression=TextureImporterCompression.CompressedHQ;importer.SaveAndReimport();}
   // Prevent an earlier catalogue from being applied while constructing source geometry.
   VillageTextureCatalog existing=Resources.Load<VillageTextureCatalog>("VillageTextures/Catalog");var oldParts=existing?existing.parts:null;if(existing)existing.parts=Array.Empty<VillageTextureCatalog.Part>();
   var root=WorldArt.Group("Village original geometry validation");var collision=new ArenaCollision();RuinedTownArena.Build(root,collision);if(existing)existing.parts=oldParts;
   var entries=new List<VillageTextureCatalog.Part>();var occurrences=new Dictionary<string,int>();int total=0;
   using(var reader=new BinaryReader(File.OpenRead(Path.Combine(input,"uvs.bin")))){
    Need(reader.ReadInt32()==0x56494C4C&&reader.ReadInt32()==mapping.parts.Length,"UV archive header");
    for(int index=0;index<mapping.parts.Length;index++){
     var part=mapping.parts[index];occurrences.TryGetValue(part.path,out int occurrence);occurrences[part.path]=occurrence+1;Need(reader.ReadInt32()==index,"stable part order");int count=reader.ReadInt32();var target=VillageTextureCatalog.FindPart(root,part.path,occurrence);Need(target,"renderer path "+part.path);var source=target.GetComponent<MeshFilter>().sharedMesh;var renderer=target.GetComponent<MeshRenderer>();var sourceTriangles=source.triangles;Need(count==sourceTriangles.Length&&count==part.triangles*3,"original triangle count "+part.path);
     var uv=new Vector2[count];for(int i=0;i<count;i++){uv[i]=new Vector2(reader.ReadSingle(),reader.ReadSingle());Need(float.IsFinite(uv[i].x)&&float.IsFinite(uv[i].y),"finite texture coordinates");}
     var positions=source.vertices;var normals=source.normals;if(normals.Length!=source.vertexCount){var temp=UnityEngine.Object.Instantiate(source);temp.RecalculateNormals();normals=temp.normals;UnityEngine.Object.DestroyImmediate(temp);}
     var mesh=new Mesh{name=part.name+" original geometry with Tripo UVs"};if(count>65535)mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;mesh.vertices=sourceTriangles.Select(i=>positions[i]).ToArray();mesh.normals=sourceTriangles.Select(i=>normals[i]).ToArray();mesh.uv=uv;mesh.triangles=Enumerable.Range(0,count).ToArray();mesh.RecalculateBounds();
     var actual=mesh.vertices;for(int i=0;i<count;i++)Need(actual[i]==positions[sourceTriangles[i]],"exact original triangle corners "+part.name);total+=count/3;
     string meshPath=Root+"/Meshes/"+part.name+".asset";var saved=AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);if(saved){saved.Clear();saved.indexFormat=mesh.indexFormat;saved.vertices=mesh.vertices;saved.normals=mesh.normals;saved.uv=mesh.uv;saved.triangles=mesh.triangles;saved.bounds=mesh.bounds;EditorUtility.SetDirty(saved);UnityEngine.Object.DestroyImmediate(mesh);}else{saved=mesh;AssetDatabase.CreateAsset(saved,meshPath);}
     string matPath=Root+"/Materials/"+part.name+".mat";var material=AssetDatabase.LoadAssetAtPath<Material>(matPath);if(!material){material=new Material(Shader.Find("Veilbound/VillageTextured"));AssetDatabase.CreateAsset(material,matPath);}material.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Textures/"+part.image);material.color=Color.white;material.enableInstancing=true;
     var originalMaterial=renderer.sharedMaterial;material.color=originalMaterial.HasProperty("_Color")?originalMaterial.color:Color.gray;if(originalMaterial.HasProperty("_Crystal")){material.SetFloat("_Crystal",originalMaterial.GetFloat("_Crystal"));material.SetColor("_Emission",originalMaterial.GetColor("_Emission"));}EditorUtility.SetDirty(material);
     if(!VillageTextureCatalog.IsEffect(part.path))entries.Add(new VillageTextureCatalog.Part{path=part.path,occurrence=occurrence,mesh=saved,material=material});
    }
    Need(reader.BaseStream.Position==reader.BaseStream.Length,"complete UV archive");
   }
   var catalog=existing;if(!catalog){catalog=ScriptableObject.CreateInstance<VillageTextureCatalog>();AssetDatabase.CreateAsset(catalog,Resource+"/Catalog.asset");}catalog.parts=entries.ToArray();EditorUtility.SetDirty(catalog);AssetDatabase.SaveAssets();UnityEngine.Object.DestroyImmediate(root.gameObject);
   VillageValidation.Run();Need(total==158712,"entire village preserved");Directory.CreateDirectory("VillageTextureUpdate");File.WriteAllText("VillageTextureUpdate/validation.json","{\"passed\":true,\"parts\":138,\"triangles\":"+total+",\"exactOriginalTriangleCorners\":true,\"credits\":30}");
   PrototypeBuild.Checks();var build=BuildPipeline.BuildPlayer(new[]{"Assets/Veilbound/Scenes/HollowCourt.unity"},"Builds/Integrated/TheVeilbound.exe",BuildTarget.StandaloneWindows64,BuildOptions.Development);Need(build.summary.result==BuildResult.Succeeded,"player build");Debug.Log("VILLAGE_TRIPO_BUILD_PASS parts=138 triangles="+total);
  }
 }
}
#endif


