using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace Veilbound.Editor {
 public static class BronzeKnightImport {
  [Serializable] class Asset {public int version;public string[] boneNames;public int[] parents;public float[] rest;public Surface[] materials;public Part[] parts;}
  [Serializable] class Surface {public string name;public float[] color;public float metallic,roughness;public float[] emission;public bool skin;}
  [Serializable] class Indices {public int[] i;}
  [Serializable] class Part {public string name;public int slot;public bool weapon;public float[] v,n,uv,bw;public int[] bi,materials;public Indices[] submeshes;}
  const string Folder="Assets/Veilbound/Resources/BronzeKnight";
  static Vector3 V(float[] a,int i)=>new Vector3(a[i*3],a[i*3+1],a[i*3+2]);
  static string Safe(string s)=>string.Concat(s.Select(c=>char.IsLetterOrDigit(c)||c=='-'||c=='_'?c:'_'));
  [MenuItem("Veilbound/Import bronze knight")]
  public static void Import() {
   string source="Assets/Veilbound/Import/BronzeHeroData.json";if(!File.Exists(source))throw new FileNotFoundException(source);
   var data=JsonUtility.FromJson<Asset>(File.ReadAllText(source));Directory.CreateDirectory(Folder);Directory.CreateDirectory(Folder+"/MeshesVerified");Directory.CreateDirectory(Folder+"/Materials");AssetDatabase.Refresh();
   Material[] mats=new Material[data.materials.Length];
   for(int i=0;i<mats.Length;i++) {
    var s=data.materials[i];var shader=Shader.Find(s.name.StartsWith("CAPE -")?"Veilbound/BronzeCape":"Standard");if(!shader)throw new Exception("Missing character shader");
    var m=new Material(shader){name=s.name};m.color=new Color(s.color[0],s.color[1],s.color[2],s.color[3]);m.SetFloat("_Metallic",s.metallic);m.SetFloat("_Glossiness",1-s.roughness);
    if(s.skin) {
     m.color=Color.white;m.mainTexture=Texture("skin-colour",false);m.SetTexture("_BumpMap",Texture("skin-normal",true));m.SetFloat("_BumpScale",.6f);m.EnableKeyword("_NORMALMAP");m.SetTexture("_EmissionMap",Texture("skin-emission",false));m.SetColor("_EmissionColor",new Color(.9f,.20f,.035f));m.EnableKeyword("_EMISSION");
    } else if(s.emission!=null&&s.emission.Max()>.01f){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",new Color(s.emission[0],s.emission[1],s.emission[2]));}
    string mp=Folder+"/Materials/"+Safe(s.name)+".mat";var old=AssetDatabase.LoadAssetAtPath<Material>(mp);if(old){EditorUtility.CopySerialized(m,old);UnityEngine.Object.DestroyImmediate(m);m=old;}else AssetDatabase.CreateAsset(m,mp);mats[i]=m;
   }
   var root=new GameObject("Veilborne - bronze knight");var visual=root.AddComponent<BronzeHeroVisual>();visual.bones=new Transform[data.boneNames.Length];visual.rest=new Vector3[data.boneNames.Length];
   for(int i=0;i<visual.bones.Length;i++){visual.rest[i]=V(data.rest,i);var b=new GameObject(data.boneNames[i]).transform;b.SetParent(data.parents[i]<0?root.transform:visual.bones[data.parents[i]],false);b.localPosition=visual.rest[i]-(data.parents[i]<0?Vector3.zero:visual.rest[data.parents[i]]);visual.bones[i]=b;}
   var sword=new GameObject("Greatsword");var renderers=new List<SkinnedMeshRenderer>();var slots=new List<int>();var eyes=new List<Renderer>();
   int ix=0;
   foreach(var p in data.parts) {
    string path=Folder+"/MeshesVerified/"+(ix++)+"_"+Safe(p.name)+".asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);bool existing=mesh;
    if(!mesh)mesh=new Mesh();else mesh.Clear();mesh.name=p.name;mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;
    int count=p.v.Length/3;var v=new Vector3[count];var n=new Vector3[count];var uv=new Vector2[count];var w=new BoneWeight[count];
    for(int j=0;j<count;j++){v[j]=V(p.v,j);n[j]=V(p.n,j);uv[j]=new Vector2(p.uv[j*2],p.uv[j*2+1]);w[j]=new BoneWeight{boneIndex0=p.bi[j*4],boneIndex1=p.bi[j*4+1],boneIndex2=p.bi[j*4+2],boneIndex3=p.bi[j*4+3],weight0=p.bw[j*4],weight1=p.bw[j*4+1],weight2=p.bw[j*4+2],weight3=p.bw[j*4+3]};}
    mesh.vertices=v;mesh.normals=n;mesh.uv=uv;mesh.subMeshCount=p.submeshes.Length;for(int j=0;j<p.submeshes.Length;j++){var indices=(int[])p.submeshes[j].i.Clone();for(int t=0;t<indices.Length;t+=3){int swap=indices[t];indices[t]=indices[t+2];indices[t+2]=swap;}mesh.SetTriangles(indices,j);}
    if(!p.weapon){mesh.boneWeights=w;mesh.bindposes=visual.bones.Select(b=>b.worldToLocalMatrix*root.transform.localToWorldMatrix).ToArray();}
    mesh.RecalculateTangents();mesh.RecalculateBounds();if(existing)EditorUtility.SetDirty(mesh);else AssetDatabase.CreateAsset(mesh,path);
    var obj=new GameObject(p.name);obj.transform.SetParent(p.weapon?sword.transform:root.transform,false);var material=p.materials.Select(index=>mats[index]).ToArray();
    if(p.weapon){obj.AddComponent<MeshFilter>().sharedMesh=mesh;obj.AddComponent<MeshRenderer>().sharedMaterials=material;}
    else{var sk=obj.AddComponent<SkinnedMeshRenderer>();sk.sharedMesh=mesh;sk.sharedMaterials=material;sk.bones=visual.bones;sk.rootBone=visual.bones[0];sk.localBounds=new Bounds(new Vector3(0,1,0),new Vector3(3,3,3));sk.quality=SkinQuality.Bone4;sk.updateWhenOffscreen=true;sk.forceMatrixRecalculationPerRender=true;renderers.Add(sk);slots.Add(p.slot);if(p.name.StartsWith("BODY -"))visual.bodySkin=sk;if(p.name.StartsWith("EYE -"))eyes.Add(sk);}
   }
   visual.parts=renderers.ToArray();visual.slots=slots.ToArray();visual.emberEyes=eyes.ToArray();if(!visual.bodySkin)throw new Exception("Imported body missing");
   PrefabUtility.SaveAsPrefabAsset(root,Folder+"/BronzeKnight.prefab");PrefabUtility.SaveAsPrefabAsset(sword,Folder+"/Greatsword.prefab");UnityEngine.Object.DestroyImmediate(root);UnityEngine.Object.DestroyImmediate(sword);AssetDatabase.SaveAssets();AssetDatabase.Refresh();Debug.Log("BRONZE_KNIGHT_IMPORT_PASS parts="+data.parts.Length+" bones="+data.boneNames.Length);
  }
  static Texture2D Texture(string name,bool normal){string p=Folder+"/Textures/"+name+".png";var importer=AssetImporter.GetAtPath(p) as TextureImporter;if(importer==null)throw new Exception("Missing texture "+p);importer.textureType=normal?TextureImporterType.NormalMap:TextureImporterType.Default;importer.sRGBTexture=!normal;importer.maxTextureSize=2048;importer.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Texture2D>(p);}
  public static void Build(){Import();PrototypeBuild.Build();}
 }
}
