using UnityEngine;
using UnityEngine.Rendering;
namespace Veilbound {
 public static class EnvironmentCrystalGlow {
  static Material material;
  // One depth-tested ground aura batch for all crystals in a map, without point lights.
  public static void Add(Transform root,Vector3[] positions){
   CrystalClouds.Add(root,positions);
   if(!material){material=new Material(Shader.Find("Veilbound/EnvironmentCrystalGlow")){name="Dark crimson environment crystal aura"};}
   var vertices=new Vector3[positions.Length*4];var uv=new Vector2[vertices.Length];var indices=new int[positions.Length*6];
   for(int i=0;i<positions.Length;i++){int a=i*4,t=i*6;var p=positions[i]+Vector3.up*.045f;float radius=1.6f;
    vertices[a]=p+new Vector3(-radius,0,-radius);vertices[a+1]=p+new Vector3(-radius,0,radius);vertices[a+2]=p+new Vector3(radius,0,radius);vertices[a+3]=p+new Vector3(radius,0,-radius);
    uv[a]=Vector2.zero;uv[a+1]=Vector2.up;uv[a+2]=Vector2.one;uv[a+3]=Vector2.right;
    indices[t]=a;indices[t+1]=a+1;indices[t+2]=a+2;indices[t+3]=a;indices[t+4]=a+2;indices[t+5]=a+3;
   }
   var mesh=new Mesh{name="Batched environment crystal auras",vertices=vertices,uv=uv,triangles=indices};mesh.RecalculateBounds();var owner=root.GetComponent<VillageMeshOwner>();if(owner)owner.meshes.Add(mesh);
   var go=new GameObject("Dark crimson crystal ground glow");go.transform.SetParent(root,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;var r=go.AddComponent<MeshRenderer>();r.sharedMaterial=material;r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;
  }
 }
}
