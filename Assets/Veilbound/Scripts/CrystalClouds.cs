using System.Collections.Generic;using UnityEngine;using UnityEngine.Rendering;
namespace Veilbound {
 public static class CrystalClouds {
  static Material material;
  public static void Add(Transform root,Vector3[] positions){
   if(!material)material=new Material(Resources.Load<Shader>("CrystalCloud")){name="Drifting dark crystal clouds"};
   var vertices=new List<Vector3>();var triangles=new List<int>();var cluster=new List<Vector4>();
   for(int i=0;i<positions.Length;i++){
    float scale=positions.Length==4?(i==0?.85f:.6f):1;
    Vector3 centre=positions[i]+Vector3.up*(.85f*scale);Vector3 half=new Vector3(1.8f,.85f,1.8f)*scale;
    int start=vertices.Count;
    for(int k=0;k<8;k++){vertices.Add(centre+Vector3.Scale(half,new Vector3((k&1)==0?-1:1,(k&2)==0?-1:1,(k&4)==0?-1:1)));cluster.Add(new Vector4(centre.x,centre.y,centre.z,scale));}
    int[] faces={0,2,3,0,3,1,4,5,7,4,7,6,0,4,6,0,6,2,1,3,7,1,7,5,2,6,7,2,7,3,0,1,5,0,5,4};foreach(int index in faces)triangles.Add(start+index);
   }
   var mesh=new Mesh{name="Batched crystal cloud volumes"};mesh.SetVertices(vertices);mesh.SetUVs(0,cluster);mesh.SetTriangles(triangles,0);mesh.RecalculateBounds();root.GetComponent<VillageMeshOwner>().meshes.Add(mesh);
   var go=WorldArt.Group("Low drifting dark clouds around crystal clusters",root);go.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;var renderer=go.gameObject.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
  }
 }
}
