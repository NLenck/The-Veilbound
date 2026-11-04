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
  public static Transform CreateEquippedSword(Transform parent,Settlement city){if(city.IsWoodenSword)return CreateSword(parent);var library=Resources.Load<Veilbound.Greatswords.GreatswordVisualLibrary>("WeaponVisuals/GreatswordLibrary");if(library){var visual=library.CreateVisual(parent,city.WeaponItemIndex);if(visual){visual.name=city.WeaponName;return visual;}}var fallback=RoundedArt.Greatsword(parent,city.equippedSword);MetalAppearance.Apply(fallback,city.WeaponMetalIndex);return fallback;}
  public static Transform CreateSword(Transform parent){var prefab=Resources.Load<GameObject>("StarterEquipment/WoodenGreatsword");if(!prefab)throw new InvalidOperationException("Missing wooden starter sword prefab");return UnityEngine.Object.Instantiate(prefab,parent,false).transform;}
 }
}
