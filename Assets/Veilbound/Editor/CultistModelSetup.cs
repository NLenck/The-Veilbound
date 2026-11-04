#if UNITY_EDITOR
using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEditor;using UnityEngine;
namespace Veilbound.Editor {
 public static class CultistModelSetup {
  const string Folder="Assets/Veilbound/Resources/EnemyLocomotion/";
  static int Find(int[] parents,int a){while(parents[a]!=a){parents[a]=parents[parents[a]];a=parents[a];}return a;}
  static void Join(int[] parents,int a,int b){parents[Find(parents,a)]=Find(parents,b);}
  static T Store<T>(T value,string path)where T:UnityEngine.Object{var old=AssetDatabase.LoadAssetAtPath<T>(path);if(old){EditorUtility.CopySerialized(value,old);UnityEngine.Object.DestroyImmediate(value);return old;}AssetDatabase.CreateAsset(value,path);return value;}
  static Mesh Extract(Mesh source,int[] triangles,Vector3 offset,string name){
   int[] used=triangles.Distinct().OrderBy(i=>i).ToArray();var map=used.Select((v,i)=>new{v,i}).ToDictionary(p=>p.v,p=>p.i);var mesh=new Mesh{name=name};mesh.vertices=used.Select(i=>source.vertices[i]-offset).ToArray();if(source.normals.Length==source.vertexCount)mesh.normals=used.Select(i=>source.normals[i]).ToArray();if(source.uv.Length==source.vertexCount)mesh.uv=used.Select(i=>source.uv[i]).ToArray();mesh.triangles=triangles.Select(i=>map[i]).ToArray();mesh.RecalculateBounds();return mesh;
  }
  static BoneWeight Weight(Vector3 vertex,Vector3[] rest){int[] children={1,2,3,4,-1,6,7,-1,9,10,-1,12,13,-1,15,16,-1};var weights=new List<KeyValuePair<int,float>>();for(int i=0;i<17;i++){Vector3 end=children[i]>=0?rest[children[i]]:rest[i]+Vector3.up*.035f;Vector3 delta=end-rest[i];float t=Mathf.Clamp01(Vector3.Dot(vertex-rest[i],delta)/Mathf.Max(.00001f,delta.sqrMagnitude));float d=(vertex-rest[i]-delta*t).sqrMagnitude;if(i>=11&&vertex.y>rest[0].y+.08f)d+=10;weights.Add(new KeyValuePair<int,float>(i,1/Mathf.Pow(.025f+d,2)));}var best=weights.OrderByDescending(p=>p.Value).Take(4).ToArray();float sum=best.Sum(p=>p.Value);return new BoneWeight{boneIndex0=best[0].Key,weight0=best[0].Value/sum,boneIndex1=best[1].Key,weight1=best[1].Value/sum,boneIndex2=best[2].Key,weight2=best[2].Value/sum,boneIndex3=best[3].Key,weight3=best[3].Value/sum};}
  public static void Run(){
   string prefab=AssetDatabase.GetAssetPath(ProductionModelCatalog.Current.Enemy("Shrine/Acolyte").prefab);string backup="EnemyAttackUpdate/Backup/"+prefab;Directory.CreateDirectory(Path.GetDirectoryName(backup));if(!File.Exists(backup))File.Copy(prefab,backup);
   var root=PrefabUtility.LoadPrefabContents(prefab);
   try{
    var visual=root.GetComponent<MonsterVisual>();if(root.GetComponent<CultistDaggerPose>())return;
    var source=visual.skin.sharedMesh;var vertices=source.vertices;var triangles=source.triangles;var parents=Enumerable.Range(0,vertices.Length).ToArray();var weld=new Dictionary<Vector3Int,int>();
    for(int i=0;i<vertices.Length;i++){var p=vertices[i];var k=new Vector3Int(Mathf.RoundToInt(p.x*10000),Mathf.RoundToInt(p.y*10000),Mathf.RoundToInt(p.z*10000));if(weld.TryGetValue(k,out int other))Join(parents,i,other);else weld.Add(k,i);}
    for(int i=0;i<triangles.Length;i+=3){Join(parents,triangles[i],triangles[i+1]);Join(parents,triangles[i],triangles[i+2]);}
    var groups=Enumerable.Range(0,vertices.Length).GroupBy(i=>Find(parents,i)).Select(g=>g.ToArray()).OrderByDescending(g=>g.Length).ToArray();
    var bladeGroup=groups.First(g=>g.Length<vertices.Length*.25f&&g.Average(i=>vertices[i].x)>.2f&&g.Max(i=>vertices[i].z)-g.Min(i=>vertices[i].z)>.6f);
    var selected=new HashSet<int>(bladeGroup);var bodyTriangles=new List<int>();var bladeTriangles=new List<int>();for(int i=0;i<triangles.Length;i+=3){var list=selected.Contains(triangles[i])?bladeTriangles:bodyTriangles;list.Add(triangles[i]);list.Add(triangles[i+1]);list.Add(triangles[i+2]);}
    var bodyGroup=groups[0];float centerZ=(bodyGroup.Min(i=>vertices[i].z)+bodyGroup.Max(i=>vertices[i].z))*.5f;var center=new Vector3(0,0,centerZ);
    float zmin=bladeGroup.Min(i=>vertices[i].z),zmax=bladeGroup.Max(i=>vertices[i].z);Vector3 grip=new Vector3(bladeGroup.Average(i=>vertices[i].x),bladeGroup.Average(i=>vertices[i].y),Mathf.Lerp(zmin,zmax,.2f));
    var bladeMesh=Store(Extract(source,bladeTriangles.ToArray(),grip,"Acolyte rigid ritual dagger"),Folder+"Cultist_Dagger.asset");
    var body=Extract(source,bodyTriangles.ToArray(),center,"Acolyte centered body without embedded blade");body.boneWeights=body.vertices.Select(v=>Weight(v,visual.rest)).ToArray();body.bindposes=visual.bones.Select(b=>b.worldToLocalMatrix*visual.skin.transform.localToWorldMatrix).ToArray();body=Store(body,Folder+"Cultist_Body.asset");visual.skin.sharedMesh=body;visual.skin.localBounds=new Bounds(body.bounds.center,body.bounds.size+Vector3.one*1.8f);
    var weapon=new GameObject("Raised-hand ritual dagger");weapon.transform.SetParent(root.transform,false);weapon.AddComponent<MeshFilter>().sharedMesh=bladeMesh;weapon.AddComponent<MeshRenderer>().sharedMaterial=visual.skin.sharedMaterial;
    var pose=root.AddComponent<CultistDaggerPose>();pose.hand=visual.bones[10];pose.elbow=visual.bones[9];pose.blade=weapon.transform;
    Vector3 expected=grip-center-visual.rest[10];pose.gripOffset=Vector3.ClampMagnitude(expected,.12f);pose.Sync();
    PrefabUtility.SaveAsPrefabAsset(root,prefab);Debug.Log("CULTIST_RIGID_DAGGER_PASS vertices="+bladeMesh.vertexCount+" offset="+pose.gripOffset);
   }finally{PrefabUtility.UnloadPrefabContents(root);}
   AssetDatabase.SaveAssets();AssetDatabase.Refresh();
  }
 }
}
#endif
