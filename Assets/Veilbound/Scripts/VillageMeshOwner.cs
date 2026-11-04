using System.Collections.Generic;using UnityEngine;
namespace Veilbound {
 // Procedural village meshes belong to a single expedition, unlike shared materials.
 public sealed class VillageMeshOwner:MonoBehaviour {
  public readonly List<Mesh> meshes=new List<Mesh>();
  void OnDestroy(){foreach(var mesh in meshes)if(mesh){if(Application.isPlaying)Destroy(mesh);else DestroyImmediate(mesh);}meshes.Clear();}
 }
}
