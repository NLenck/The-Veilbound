using UnityEngine;
namespace Veilbound {
 public static class WeaponRules {
  public const float BaselineInterval=2f;public const int Count=11;
  // IDs 0..9 preserve existing saves. ID 10 is the second Veilcrystal variant.
  static readonly float[] damage={18,24,35,34,39,48,61,64,109,88,150};
  static readonly float[] intervals={2.24f,2.4f,2.32f,2.08f,1.92f,1.92f,2.24f,1.6f,2.72f,1.6f,2.72f};
  public static int Material(int id)=>id==10?9:Mathf.Clamp(id,0,9);
  public static string Name(int id)=>id<0?"Wooden greatsword":id==9?"Veilcrystal (Titanium base) greatsword":id==10?"Veilcrystal (Tungsten base) greatsword":Metals.All[Material(id)].name+" greatsword";
  public static string BarName(int id)=>id==9?"Veilcrystal (Titanium base) bar":id==10?"Veilcrystal (Tungsten base) bar":Metals.All[Material(id)].name+" bar";
  public static float Damage(int id)=>id<0?10:damage[Mathf.Clamp(id,0,Count-1)];
  public static float Interval(int id)=>id<0?2:intervals[Mathf.Clamp(id,0,Count-1)];
  public static float CooldownMultiplier(float interval)=>Mathf.Max(.01f,interval)/BaselineInterval;
  public static float Coefficient(string id)=>id=="orbit"?.6f:id=="smite"?1.8f:id=="bloodthirst"?.8f:id=="rend"?.35f:id=="whirl"?.8f:id=="brand"||id=="warcry"?0:1;
  public static float BaseCooldown(string id)=>id=="cleave"?2:id=="whirl"?3.1f:id=="lance"?2.1f:id=="orbit"?.45f:id=="smite"?2.5f:id=="brand"?5:id=="bloodthirst"?8:id=="warcry"?15:id=="vengeance"?1:0;
 }
 public partial class Settlement {public float WeaponInterval=>WeaponRules.Interval(WeaponItemIndex);}
}
