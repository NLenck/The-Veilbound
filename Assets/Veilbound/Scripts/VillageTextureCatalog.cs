using System;
using UnityEngine;
namespace Veilbound {
 public sealed class VillageTextureCatalog:ScriptableObject {
  [Serializable]public class Part {public string path;public int occurrence;public Mesh mesh;public Material material;}
  public Part[] parts;
  public static bool IsEffect(string path)=>path=="Low drifting dark clouds around crystal clusters"||path=="Dark crimson crystal ground glow";
  public static Transform FindPart(Transform root,string path,int occurrence){int slash=path.LastIndexOf('/');var parent=slash<0?root:root.Find(path.Substring(0,slash));string name=path.Substring(slash+1);if(!parent)return null;foreach(Transform child in parent)if(child.name==name&&occurrence--==0)return child;return null;}
  public static int Apply(Transform root){
   var catalog=Resources.Load<VillageTextureCatalog>("VillageTextures/Catalog");if(!catalog)return 0;int applied=0;
   foreach(var entry in catalog.parts){if(IsEffect(entry.path))continue;var target=FindPart(root,entry.path,entry.occurrence);if(!target)throw new InvalidOperationException("Village texture target missing: "+entry.path);var filter=target.GetComponent<MeshFilter>();var renderer=target.GetComponent<MeshRenderer>();if(!filter||!renderer)throw new InvalidOperationException("Village texture renderer missing: "+entry.path);filter.sharedMesh=entry.mesh;renderer.sharedMaterial=entry.material;applied++;}
   return applied;
  }
 }
}
