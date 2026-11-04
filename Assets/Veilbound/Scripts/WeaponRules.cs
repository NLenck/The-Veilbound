using UnityEngine;
namespace Veilbound {
 public static class WeaponRules {
  public const float BaselineInterval=2f;public const int Count=WeaponCatalog.CraftableCount;
  // Compatibility API for existing combat, UI and save callers. Weapon data lives in the catalogue.
  public static int Material(int id)=>WeaponCatalog.MaterialIndexFor(id);
  public static string Name(int id)=>WeaponCatalog.ResolveLegacy(id).Name;
  public static string BarName(int id)=>WeaponCatalog.ResolveLegacy(Mathf.Max(0,id)).BarName;
  public static float Damage(int id)=>WeaponCatalog.ResolveLegacy(id).Damage;
  public static float Interval(int id)=>WeaponCatalog.ResolveLegacy(id).AttackInterval;
  public static float CooldownMultiplier(float interval)=>Mathf.Max(.01f,interval)/BaselineInterval;
  public static float Coefficient(string id)=>id=="orbit"?.6f:id=="smite"?1.8f:id=="bloodthirst"?.8f:id=="rend"?.35f:id=="whirl"?.8f:id=="brand"||id=="warcry"?0:1;
  public static float BaseCooldown(string id)=>id=="cleave"?2:id=="whirl"?3.1f:id=="lance"?2.1f:id=="orbit"?.45f:id=="smite"?2.5f:id=="brand"?5:id=="bloodthirst"?8:id=="warcry"?15:id=="vengeance"?1:0;
 }
 public partial class Settlement {public float WeaponInterval=>EquippedWeaponDefinition.AttackInterval;}
}
