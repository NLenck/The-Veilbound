using System;
using UnityEngine;
namespace Veilbound {
 public partial class Settlement {
  // Explicit new-character initialization only. Never run this on a loaded character.
  public void InitializeWoodenStarter(){
   armour=sword=equippedArmour=equippedSword=0;bronzeHeroVersion=metalVersion=1;gearRollVersion=0;
   ownedGear=new int[27];equippedGear=new int[9];ownedPlate=new bool[90];ownedMetalWeapons=new bool[WeaponRules.Count];
   equippedMetalWeapon=-2;craftedGear=null;equippedRollIds=null;nextGearRollId=1;lastCraftedRollId=0;
   EnsureGearRolls();
  }
 }
 public static class StarterEquipment {
  public static Transform CreateEquippedSword(Transform parent,Settlement city)
  {
   var weapon=city.EquippedWeaponDefinition;
   if(weapon.PrefabResource!=null)return CreateWeaponPrefab(parent,weapon);
   var library=Resources.Load<Veilbound.Greatswords.GreatswordVisualLibrary>(weapon.VisualLibraryResource);
   if(library){var visual=library.CreateVisual(parent,weapon.VisualId);if(visual){visual.name=weapon.Name;return visual;}}
   var fallback=RoundedArt.Greatsword(parent,city.equippedSword);
   MetalAppearance.Apply(fallback,weapon.MaterialIndex);
   return fallback;
  }
  public static Transform CreateSword(Transform parent)=>CreateWeaponPrefab(parent,WeaponCatalog.Find(-1));
  static Transform CreateWeaponPrefab(Transform parent,WeaponDefinition weapon)
  {
   var prefab=Resources.Load<GameObject>(weapon.PrefabResource);
   if(!prefab)throw new InvalidOperationException("Missing weapon prefab: "+weapon.Name);
   return UnityEngine.Object.Instantiate(prefab,parent,false).transform;
  }
 }
}
