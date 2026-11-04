using System.Collections.Generic;
using UnityEngine;
namespace Veilbound {
 public sealed class MetalAppearance:MonoBehaviour {
  readonly List<Material> owned=new List<Material>();
  public static void Tint(Material material,int metal,bool preserveBronze=false) {
   if(!material.HasProperty("_Metallic")||material.GetFloat("_Metallic")<.4f||material.name.ToLowerInvariant().Contains("trim"))return;
   if(metal==2&&preserveBronze)return;material.color=Metals.All[metal].color;material.SetFloat("_Metallic",metal==9?.6f:.85f);material.SetFloat("_Glossiness",metal==9?.6f:.42f);
   if(metal==9){material.EnableKeyword("_EMISSION");material.SetColor("_EmissionColor",new Color(.30f,.012f,.008f));}
  }
  public static void Apply(Transform root,int metal) {var owner=root.gameObject.AddComponent<MetalAppearance>();foreach(var renderer in root.GetComponentsInChildren<Renderer>())foreach(var material in renderer.materials){owner.owned.Add(material);Tint(material,metal);}}
  void OnDestroy(){foreach(var material in owned)if(material)Destroy(material);}
 }
}

