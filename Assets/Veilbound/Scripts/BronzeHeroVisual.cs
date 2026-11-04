using System;
using UnityEngine;
namespace Veilbound {
 public sealed class BronzeHeroVisual : MonoBehaviour {
  public Transform[] bones;
  public Vector3[] rest;
  public SkinnedMeshRenderer bodySkin;
  public SkinnedMeshRenderer[] parts;
  public int[] slots;
  public Renderer[] emberEyes;
  public bool configured;
  readonly System.Collections.Generic.List<Material> ownedMaterials=new System.Collections.Generic.List<Material>();
  public static bool Available => Resources.Load<GameObject>("BronzeKnight/BronzeKnight") != null;
  public static BronzeHeroVisual Create(Transform parent,Settlement city) {
   var prefab=Resources.Load<GameObject>("BronzeKnight/BronzeKnight");
   if(!prefab)throw new InvalidOperationException("Bronze knight prefab has not been imported. Run Veilbound/Import bronze knight.");
   var instance=UnityEngine.Object.Instantiate(prefab,parent,false);instance.name="Veilborne - bronze knight";instance.transform.localScale=Vector3.one*1.08f;
   var visual=instance.GetComponent<BronzeHeroVisual>();visual.Configure(city);return visual;
  }
  public static void EquipStarterSet(Settlement city) {
   city.EnsureGear();if(city.bronzeHeroVersion>=1)return;
   if(city.armour==0&&city.sword==0&&city.gearRollVersion==0&&(city.craftedGear==null||city.craftedGear.Count==0)&&Array.TrueForAll(city.ownedGear,x=>x==0)&&Array.TrueForAll(city.ownedPlate,x=>!x)&&Array.TrueForAll(city.equippedGear,x=>x==0)){city.InitializeWoodenStarter();return;}
   for(int s=0;s<8;s++){city.ownedPlate[s*10]=true;city.equippedGear[s]=Metals.Item(0);}
   city.ownedGear[8*3]=Mathf.Max(city.ownedGear[8*3],1);city.equippedGear[8]=11;city.blacksmith=Math.Max(1,city.blacksmith);city.ownedMetalWeapons[0]=true;city.equippedMetalWeapon=0;city.bronzeHeroVersion=1;
  }
  void Configure(Settlement city) {
   city.EnsureGear();
   for(int i=0;i<parts.Length;i++) {
    int slot=slots[i];if(slot<0)continue;int item=city.Item(slot);parts[i].gameObject.SetActive(item!=0);if(item==0)continue;
    int material=Gear.Material(item);if(material==3){int metal=Metals.Index(item);if(metal>=0&&!parts[i].name.StartsWith("TRIM -",StringComparison.Ordinal))foreach(var mat in parts[i].materials){ownedMaterials.Add(mat);MetalAppearance.Tint(mat,metal,true);}continue;}if(slot==8)continue;
    var materials=parts[i].materials;
    foreach(var mat in materials){ownedMaterials.Add(mat);if(mat.name.ToLowerInvariant().Contains("trim")||!mat.HasProperty("_Metallic")||mat.GetFloat("_Metallic")<.4f)continue;mat.color=LightGear.Colour(material,Gear.Tier(item));mat.SetFloat("_Metallic",0);mat.SetFloat("_Glossiness",.18f);}
   }
   configured=true;
  }
  public void ReleaseMaterials() {foreach(var mat in ownedMaterials)if(mat)UnityEngine.Object.Destroy(mat);ownedMaterials.Clear();}
  public static Transform MakeSword(Transform parent,int tier) {
   var prefab=Resources.Load<GameObject>("BronzeKnight/Greatsword");if(!prefab)return null;
   var root=UnityEngine.Object.Instantiate(prefab,parent,false).transform;root.name="Equipped greatsword T"+tier;root.localScale=Vector3.one*(1+tier*.035f);return root;
  }
  public static float SwordTip(int tier)=>1.72f*(1+tier*.035f);
 }
}


