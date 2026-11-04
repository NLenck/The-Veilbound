using System;using System.Linq;using System.IO;using System.Collections;using System.Collections.Generic;using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  [Serializable]class PressureSample {public int map,depth;public float enemyHealth,bossHealth,meleeDamage,bossDamage,spawnRate,starterDamage,starterHealth,starterProtection,standardHitsToKill,unarmouredHitsToDie;}
  [Serializable]class PressureReport {public bool passed;public int spawnSamples;public List<PressureSample> samples=new List<PressureSample>();public float stationaryStarterDepth1Seconds,stationaryStarterDepth20Seconds;}
  void PressureStart(int map,int depth){ReturnHome();city.expeditionBestDepth=100;city.EnsureExpeditionModifiers();for(int i=0;i<10;i++)city.expeditionRanks[i]=depth/10+(i<depth%10?1:0);SelectExpeditionMap(map);StartRun();combatReviewFreeze=true;TownCamera(Vector3.zero,10);navigation.Tick(position,1,true);}
  float StationaryStarterPressure(int depth){PressureStart(0,depth);position=FlowTestGround();TownCamera(position,10);navigation.Tick(position,1,true);for(int i=0;i<900&&state!=ScreenState.Result;i++){if(state==ScreenState.Choice){Choose(0);continue;}Step(.1f);}return elapsed;}
  IEnumerator EnemyPressureReview(){
   yield return null;combatReviewFreeze=true;city.InitializeWoodenStarter();RebuildHero();string folder=Path.Combine(Application.dataPath,"..","Smoke","EnemyPressure");Directory.CreateDirectory(folder);var report=new PressureReport();
   foreach(int map in new[]{0,1}){
    PressureStart(map,0);var points=new List<Vector3>{Vector3.zero,collision.Resolve(Vector3.right*(collision.PlayerRadius-6),.42f,collision.PlayerRadius),collision.Resolve(Vector3.back*(collision.PlayerRadius-6),.42f,collision.PlayerRadius)};var rooms=map==0?GraveyardArena.Rooms:RuinedTownArena.Rooms;points.Add(rooms[0].transform.position);
    foreach(var point in points){TownCamera(point,10);navigation.Tick(point,1,true);for(int i=0;i<30;i++)foreach(bool boss in new[]{false,true}){
     float body=boss?1.1f:.43f;Require(EnemySpawnPlacement.TryFind(random,collision,navigation,point,body,boss,cameraRig,out Vector3 spawn),"Nearby spawn available from centre, edge and interior");float distance=Vector3.Distance(spawn,point);Require(EnemySpawnPlacement.OutsideNoSpawnZone(cameraRig,point,new Bounds(Vector3.up*(boss?3:1.5f),new Vector3(boss?8:3,boss?6:3,boss?8:3)),spawn),"Spawns stay outside the screen-based no-spawn zone");Require(spawn.magnitude+body<collision.Radius&&collision.Free(spawn,body)&&navigation.Reachable(spawn,body),"Nearby spawn retains bounds, wall clearance and navigation");report.spawnSamples++;
    }}
    foreach(int depth in new[]{0,1,10,20}){
     PressureStart(map,depth);var regular=SpawnFoe(0,0);var boss=SpawnFoe(1);Require(regular!=null&&boss!=null,"Normal and boss actual spawns succeed");
     float expected=EnemyTypes.RegularHealth(1,0,0)*ModifierFactor(0)*ExpeditionDifficulty.Health(depth);Require(Mathf.Abs(regular.maxHp-expected)<.01f,"Regular enemy receives baseline plus selected health modifier");Require(Mathf.Abs(boss.maxHp-480*ModifierFactor(2)*ExpeditionDifficulty.Health(depth))<.01f,"Boss receives baseline plus boss health modifier");
     health=MaxHealth;invulnerability=0;float initial=health;Hurt(9,regular);float normalHit=initial-health;Require(Mathf.Abs(normalHit-9*ExpeditionDifficulty.Damage(depth)*ModifierFactor(1))<.01f,"Actual melee damage scales with depth and enemy modifier");
     health=MaxHealth;invulnerability=0;initial=health;Hurt(18,boss);float bossHit=initial-health;Require(Mathf.Abs(bossHit-18*ExpeditionDifficulty.Damage(depth)*ModifierFactor(3))<.01f,"Actual boss damage scales with depth and boss modifier");
     report.samples.Add(new PressureSample{map=map,depth=depth,enemyHealth=regular.maxHp,bossHealth=boss.maxHp,meleeDamage=normalHit,bossDamage=bossHit,spawnRate=ExpeditionDifficulty.SpawnRate(depth)*ModifierFactor(5),starterDamage=AbilityDamage("cleave"),starterHealth=MaxHealth,starterProtection=city.Protection,standardHitsToKill=Mathf.Ceil(regular.maxHp/AbilityDamage("cleave")),unarmouredHitsToDie=Mathf.Ceil(MaxHealth/normalHit)});
     Require(city.Protection==0&&city.IsWoodenSword&&Multiplier==CityTimeMultiplier(Math.Max(1,depth)),"Starter remains unarmoured and time dilation is unchanged");
    }
   }
   report.stationaryStarterDepth1Seconds=StationaryStarterPressure(1);report.stationaryStarterDepth20Seconds=StationaryStarterPressure(20);Require(state==ScreenState.Result&&health<=0,"Depth twenty overwhelms a stationary unarmoured starter");
   PressureStart(1,20);for(int i=0;i<14;i++)Require(SpawnFoe(0)!=null,"Pressure wave spawns near the player");var arrival=SpawnFoe(1);Require(arrival!=null&&enemyArrivalFog.Count==15,"Boss and regular enemies have Veil arrival mist");UpdateEnemyArrivalFog(.2f);
   cameraRig.transform.position=arrival.t.position+new Vector3(5,7,-9);cameraRig.transform.LookAt(arrival.t.position+Vector3.up*.8f);cameraRig.orthographicSize=5;yield return null;CaptureVillageCamera(Path.Combine(folder,"boss-fog-arrival.png"),1600,1000);
   TownCamera(Vector3.zero,23);yield return null;CaptureVillageCamera(Path.Combine(folder,"nearby-depth20-wave.png"),1600,1000);
   UpdateEnemyArrivalFog(2);Require(enemyArrivalFog.Count==0,"Arrival mist cleans up");ReturnHome();tab=0;yield return new WaitForSecondsRealtime(.35f);yield return CardScreenshot(Path.Combine(folder,"depth20-summary.png"));
   report.passed=true;File.WriteAllText(Path.Combine(folder,"validation.json"),JsonUtility.ToJson(report,true));File.WriteAllText(Path.Combine(folder,"PASS.txt"),"PASS: "+report.spawnSamples+" nearby spawn samples in both maps, centre/edges/interior, normal and boss; collision/reachability; baseline HP/damage/spawn growth; selected modifier multiplication; actual damage against unarmoured starter; unchanged time dilation; depth20 stationary starter pressure; arrival mist cleanup; actual player screenshots.");Debug.Log("VEILBOUND_ENEMY_PRESSURE_RUNTIME_PASS samples="+report.spawnSamples);Application.Quit(0);
  }
 }
}
