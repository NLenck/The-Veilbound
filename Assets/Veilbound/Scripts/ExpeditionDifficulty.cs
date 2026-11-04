using UnityEngine;
namespace Veilbound {
 public static class ExpeditionDifficulty {
  static int Steps(int depth)=>Mathf.Clamp(depth-1,0,99);
  public static float Health(int depth)=>1+.25f*Steps(depth);
  public static float Damage(int depth)=>1+.10f*Steps(depth);
  public static float SpawnRate(int depth)=>1+.04f*Steps(depth);
 }
}
