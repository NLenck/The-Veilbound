#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using UnityEngine;
namespace Veilbound.Editor {
 public static class VillageTripoExport {
  [Serializable]class Part {public string name,path;public int vertices,triangles;}
  [Serializable]class Manifest {public int buildings,triangles,vertices;public float playerRadius,spawnRadius;public List<Part> parts=new List<Part>();}
  static string F(float value)=>value.ToString("R",CultureInfo.InvariantCulture);
  static string PathOf(Transform t,Transform root){string path=t.name;while(t.parent&&t.parent!=root){t=t.parent;path=t.name+"/"+path;}return path;}
  public static void Run(){
   var args=Environment.GetCommandLineArgs();int arg=Array.IndexOf(args,"-villageTripoOutput");if(arg<0)throw new Exception("Missing village export destination");string folder=args[arg+1];Directory.CreateDirectory(folder);
   var root=WorldArt.Group("Village texture input");var collision=new ArenaCollision();RuinedTownArena.Build(root,collision);
   var manifest=new Manifest{buildings=RuinedTownArena.Rooms.Count,playerRadius=collision.PlayerRadius,spawnRadius=collision.Radius};var mats=new Dictionary<Material,string>();int offset=1;
   using(var obj=new StreamWriter(Path.Combine(folder,"village.obj"))){
    obj.WriteLine("mtllib village.mtl");
    foreach(var filter in root.GetComponentsInChildren<MeshFilter>()){
     var renderer=filter.GetComponent<MeshRenderer>();if(!renderer||!renderer.enabled||!filter.sharedMesh)continue;
     var mesh=filter.sharedMesh;if(mesh.vertexCount==0)continue;string name="VillagePart_"+manifest.parts.Count.ToString("D3");obj.WriteLine("o "+name);
     foreach(var vertex in mesh.vertices){var p=filter.transform.TransformPoint(vertex);obj.WriteLine("v "+F(p.x)+" "+F(-p.z)+" "+F(p.y));}
     var normals=mesh.normals;if(normals.Length!=mesh.vertexCount){var temporary=UnityEngine.Object.Instantiate(mesh);temporary.RecalculateNormals();normals=temporary.normals;UnityEngine.Object.DestroyImmediate(temporary);}
     foreach(var normal in normals){var n=filter.transform.TransformDirection(normal).normalized;obj.WriteLine("vn "+F(n.x)+" "+F(-n.z)+" "+F(n.y));}
     for(int sub=0;sub<mesh.subMeshCount;sub++){
      var material=renderer.sharedMaterials[Math.Min(sub,renderer.sharedMaterials.Length-1)];if(!mats.TryGetValue(material,out var id)){id="VillageMaterial_"+mats.Count;mats.Add(material,id);}obj.WriteLine("usemtl "+id);
      var tris=mesh.GetTriangles(sub);for(int i=0;i<tris.Length;i+=3)obj.WriteLine("f "+string.Join(" ",tris.Skip(i).Take(3).Select(v=>(v+offset)+"//"+(v+offset))));
     }
     manifest.parts.Add(new Part{name=name,path=PathOf(filter.transform,root),vertices=mesh.vertexCount,triangles=mesh.triangles.Length/3});manifest.vertices+=mesh.vertexCount;manifest.triangles+=mesh.triangles.Length/3;offset+=mesh.vertexCount;
    }
   }
   using(var mtl=new StreamWriter(Path.Combine(folder,"village.mtl")))foreach(var pair in mats){Color c=pair.Key.HasProperty("_Color")?pair.Key.color:Color.gray;mtl.WriteLine("newmtl "+pair.Value+"\nKd "+F(c.r)+" "+F(c.g)+" "+F(c.b)+"\nKs 0 0 0\nNs 0\nd 1\nillum 1\n");}
   File.WriteAllText(Path.Combine(folder,"geometry-manifest.json"),JsonUtility.ToJson(manifest,true));Debug.Log("VILLAGE_TRIPO_EXPORT_PASS parts="+manifest.parts.Count+" triangles="+manifest.triangles);
   UnityEngine.Object.DestroyImmediate(root.gameObject);
  }
 }
}
#endif
