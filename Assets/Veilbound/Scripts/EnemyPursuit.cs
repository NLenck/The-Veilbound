using UnityEngine;
namespace Veilbound {
 public static class EnemyPursuit {
  public static int AliveMinutes(float seconds)=>Mathf.FloorToInt(Mathf.Max(0,seconds)/60);
  public static float EnrageBonus(float seconds){float minutes=AliveMinutes(seconds);return .025f*minutes*(minutes+1);}
 }
 public partial class VeilGame {
  int CrowdLayer(Foe foe){
   if(foe.crowdLayer<0)foe.crowdLayer=foe.boss>0?2:EnemyTypes.Speed(foe.kind)>1||foe.rig!=null&&IsChargingEnemy(foe)?1:0;
   return foe.crowdLayer;
  }
  bool SeparateEnemyBodies(Foe a,Foe b)=>CrowdLayer(a)==CrowdLayer(b);
  float AberrationAge(Foe foe)=>Mathf.Max(0,(foe.diedAt>=0?foe.diedAt:elapsed)-foe.spawnedAt);
  float AberrationAgeFactor(Foe foe)=>foe!=null&&foe.boss>0?1+EnemyPursuit.EnrageBonus(AberrationAge(foe)):1;
  float DistantEnemyFactor(Foe foe){Vector3 delta=foe.t.position-position;delta.y=0;return delta.sqrMagnitude>2500?2:1;}
  float EnemyTravelFactor(Foe foe)=>AberrationAgeFactor(foe)*DistantEnemyFactor(foe);
  float EnemyMovementSpeed(Foe foe)=>foe.speed*EnemyTravelFactor(foe);
  string AberrationStatus(Foe foe){float bonus=EnemyPursuit.EnrageBonus(AberrationAge(foe))*100;return BossName(foe.boss)+(bonus>0?"  ·  +"+bonus.ToString("0.#")+"% SPEED / DAMAGE":"");}
 }
}
