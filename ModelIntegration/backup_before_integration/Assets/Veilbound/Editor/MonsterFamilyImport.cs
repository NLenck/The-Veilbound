using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace Veilbound.Editor {
 public static class MonsterFamilyImport {
  [Serializable] class Family{public int version;public Surface[] materials;public Monster[] monsters;}
  [Serializable] class Surface{public string name;public float[] color,emission;public float metallic,roughness;public bool vertex;}
  [Serializable] class Monster{public string id,name;public int kind;public string[] boneNames;public int[] parents;public float[] rest;public Part[] parts;}
  [Serializable] class Part{public string name;public float[] v,n,c,bw;public int[] bi,materials;public Indices[] submeshes;}
  [Serializable] class Indices{public int[] i;}
  const string Folder="Assets/Veilbound/Resources/MonsterFamily";
  static Vector3 V(float[] a,int i)=>new Vector3(a[i*3],a[i*3+1],a[i*3+2]);
  static string Safe(string name)=>string.Concat(name.Select(c=>char.IsLetterOrDigit(c)?c:'_'));
  [MenuItem("Veilbound/Import monster family")]
  public static void Import(){
   var data=JsonUtility.FromJson<Family>(File.ReadAllText("Assets/Veilbound/Import/MonsterFamilyData.json"));Directory.CreateDirectory(Folder+"/Meshes");Directory.CreateDirectory(Folder+"/Materials");AssetDatabase.Refresh();
   var mats=new Material[data.materials.Length];
   for(int i=0;i<mats.Length;i++){
    var s=data.materials[i];var shader=Shader.Find(s.vertex?"Veilbound/MonsterSurface":"Standard");if(!shader)throw new Exception("Missing monster shader");string path=Folder+"/Materials/"+Safe(s.name)+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);bool existing=mat;if(!mat)mat=new Material(shader);else mat.shader=shader;mat.name=s.name;
    mat.color=new Color(s.color[0],s.color[1],s.color[2],1).gamma;mat.SetFloat("_Metallic",s.metallic);mat.SetFloat("_Glossiness",1-s.roughness);mat.SetColor("_EmissionColor",new Color(s.emission[0],s.emission[1],s.emission[2]));mat.EnableKeyword("_EMISSION");mat.enableInstancing=true;if(existing)EditorUtility.SetDirty(mat);else AssetDatabase.CreateAsset(mat,path);mats[i]=mat;
   }
   foreach(var monster in data.monsters){
    var root=new GameObject(monster.name);var visual=root.AddComponent<MonsterVisual>();visual.kind=monster.kind;visual.features=monster.parts.Select(p=>p.name).ToArray();visual.bones=new Transform[17];visual.rest=new Vector3[17];
    for(int i=0;i<17;i++){visual.rest[i]=V(monster.rest,i);var bone=new GameObject(monster.boneNames[i]).transform;bone.SetParent(monster.parents[i]<0?root.transform:visual.bones[monster.parents[i]],false);bone.localPosition=visual.rest[i]-(monster.parents[i]<0?Vector3.zero:visual.rest[monster.parents[i]]);visual.bones[i]=bone;}
    int[] used=monster.parts.SelectMany(p=>p.materials).Distinct().ToArray();var vertices=new List<Vector3>();var normals=new List<Vector3>();var colors=new List<Color>();var weights=new List<BoneWeight>();var triangles=used.Select(i=>new List<int>()).ToArray();
    foreach(var part in monster.parts){int offset=vertices.Count;
     for(int j=0;j<part.v.Length/3;j++){vertices.Add(V(part.v,j));normals.Add(V(part.n,j));var color=new Color(part.c[j*4],part.c[j*4+1],part.c[j*4+2],1);colors.Add(QualitySettings.activeColorSpace==ColorSpace.Gamma?color.gamma:color);weights.Add(new BoneWeight{boneIndex0=part.bi[j*4],boneIndex1=part.bi[j*4+1],boneIndex2=part.bi[j*4+2],boneIndex3=part.bi[j*4+3],weight0=part.bw[j*4],weight1=part.bw[j*4+1],weight2=part.bw[j*4+2],weight3=part.bw[j*4+3]});}
     for(int sub=0;sub<part.submeshes.Length;sub++){int destination=Array.IndexOf(used,part.materials[sub]);triangles[destination].AddRange(part.submeshes[sub].i.Select(index=>index+offset));}
     if(part.name.StartsWith("BODY -")){float lo=float.MaxValue,hi=float.MinValue;for(int j=0;j<part.v.Length/3;j++){lo=Mathf.Min(lo,part.v[j*3]);hi=Mathf.Max(hi,part.v[j*3]);}visual.bodyWidth=hi-lo;}
    }
    string mp=Folder+"/Meshes/"+monster.id+".asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(mp);bool exists=mesh;if(!mesh)mesh=new Mesh();else mesh.Clear();mesh.name=monster.name;mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;mesh.SetVertices(vertices);mesh.SetNormals(normals);mesh.SetColors(colors);mesh.boneWeights=weights.ToArray();mesh.bindposes=visual.bones.Select(b=>b.worldToLocalMatrix).ToArray();mesh.subMeshCount=used.Length;for(int i=0;i<used.Length;i++)mesh.SetTriangles(triangles[i],i);mesh.RecalculateBounds();
    if(exists)EditorUtility.SetDirty(mesh);else AssetDatabase.CreateAsset(mesh,mp);
    var child=new GameObject("Authored connected monster");child.transform.SetParent(root.transform,false);var skin=child.AddComponent<SkinnedMeshRenderer>();skin.sharedMesh=mesh;skin.sharedMaterials=used.Select(i=>mats[i]).ToArray();skin.bones=visual.bones;skin.rootBone=visual.bones[0];skin.localBounds=new Bounds(new Vector3(0,1.5f,0),new Vector3(4,5,4));skin.quality=SkinQuality.Bone4;skin.forceMatrixRecalculationPerRender=true;skin.updateWhenOffscreen=false;visual.skin=skin;
    PrefabUtility.SaveAsPrefabAsset(root,Folder+"/"+monster.id+".prefab");UnityEngine.Object.DestroyImmediate(root);Debug.Log("MONSTER_IMPORTED "+monster.id+" vertices="+mesh.vertexCount+" triangles="+mesh.triangles.Length/3);
   }
   AssetDatabase.SaveAssets();AssetDatabase.Refresh();Debug.Log("MONSTER_FAMILY_IMPORT_PASS");
  }
  public static void Build(){Import();PrototypeBuild.Build();}
 }
}
