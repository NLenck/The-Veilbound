using System;using System.IO;using System.Collections;using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  sealed class ZeroSpawnRandom:System.Random {public override double NextDouble()=>0;}
  [Serializable]class ScreenSpawnReport {public bool passed;public int modelChecks,fallbackChecks,maps=2;public float maximumDistance;}
  void CheckScreenSpawn(Transform model,float radius,ScreenSpawnReport report){
   Require(!EnemySpawnPlacement.IsVisible(cameraRig,model),"Entire spawned enemy renderer stays outside the screen");Require(collision.Free(model.position,radius)&&navigation.Reachable(model.position,radius),"Off-screen spawn remains collision-safe and reachable");Require(model.position.magnitude+radius<collision.Radius,"Off-screen spawn remains inside arena bounds");report.maximumDistance=Mathf.Max(report.maximumDistance,Vector3.Distance(model.position,position));report.modelChecks++;
  }
  IEnumerator ScreenSpawnReview(){
   yield return null;combatReviewFreeze=true;var report=new ScreenSpawnReport();string folder=Path.Combine(Application.dataPath,"..","Smoke","ScreenSpawns");Directory.CreateDirectory(folder);
   foreach(int map in new[]{0,1}){
    PressureStart(map,20);var rooms=map==0?GraveyardArena.Rooms:RuinedTownArena.Rooms;var points=new[]{Vector3.zero,collision.Resolve(Vector3.right*(collision.PlayerRadius-6),.42f,collision.PlayerRadius),collision.Resolve(Vector3.back*(collision.PlayerRadius-6),.42f,collision.PlayerRadius),rooms[0].transform.position};
    foreach(var point in points)foreach(float aspect in new[]{16f/9,4f/3,21f/9})foreach(float size in new[]{3.8f,12f,16f}){
     TownCamera(point,12);cameraRig.aspect=aspect;cameraRig.orthographicSize=size;cameraRig.transform.position=point+(size<4?new Vector3(-3,7.5f,-10):new Vector3(0,20,-16));cameraRig.transform.rotation=Quaternion.Euler(size<4?32:51,0,0);navigation.Tick(point,1,true);
     for(int kind=0;kind<5;kind++){
      var foe=SpawnFoe(0,kind);Require(foe!=null,"Normal enemy spawns immediately outside view");CheckScreenSpawn(foe.t,foe.radius,report);
      if(size<4){cameraRig.orthographicSize=12;cameraRig.transform.position=point+new Vector3(0,20,-16);cameraRig.transform.rotation=Quaternion.Euler(51,0,0);Require(!EnemySpawnPlacement.IsVisible(cameraRig,foe.t),"Start-run zoom cannot reveal a fresh spawn");cameraRig.orthographicSize=size;cameraRig.transform.position=point+new Vector3(-3,7.5f,-10);cameraRig.transform.rotation=Quaternion.Euler(32,0,0);}
      foes.Remove(foe);foe.t.gameObject.SetActive(false);Destroy(foe.t.gameObject);
      var rare=new HuskRig(actors,kind,0,map,1);Bounds shape=EnemySpawnPlacement.Measure(rare.root);Require(EnemySpawnPlacement.TryFind(random,collision,navigation,point,EnemyTypes.Radius(kind),shape,cameraRig,out Vector3 p),"Rare variant finds an off-screen position immediately");rare.root.position=p;rare.root.rotation=Quaternion.LookRotation(point-p);CheckScreenSpawn(rare.root,EnemyTypes.Radius(kind),report);rare.root.gameObject.SetActive(false);Destroy(rare.root.gameObject);
     }
     for(int number=1;number<=3;number++){var boss=SpawnFoe(number);Require(boss!=null,"Large boss spawns immediately outside view");CheckScreenSpawn(boss.t,boss.radius,report);foes.Remove(boss);boss.t.gameObject.SetActive(false);Destroy(boss.t.gameObject);}
     UpdateEnemyArrivalFog(2);yield return null;
    }
    TownCamera(Vector3.zero,12);cameraRig.transform.position=new Vector3(0,20,-16);cameraRig.transform.rotation=Quaternion.Euler(51,0,0);cameraRig.aspect=16f/9;navigation.Tick(Vector3.zero,1,true);
    foreach(bool boss in new[]{false,true}){Require(EnemySpawnPlacement.TryFind(new ZeroSpawnRandom(),collision,navigation,position,boss?1.1f:.43f,boss,cameraRig,out Vector3 p),"Full-arena fallback resolves in the same call when all random samples land on-screen");Require(Vector3.Distance(p,position)>18,"The old maximum spawn distance no longer applies");report.fallbackChecks++;}
   }
   PressureStart(1,20);cameraRig.orthographicSize=12;cameraRig.aspect=16f/9;cameraRig.transform.position=new Vector3(0,20,-16);cameraRig.transform.rotation=Quaternion.Euler(51,0,0);
   for(int i=0;i<12;i++){var foe=SpawnFoe(0,i%5);Require(foe!=null&&!EnemySpawnPlacement.IsVisible(cameraRig,foe.t),"Live wave spawns off-screen");}var elder=SpawnFoe(1);Require(elder!=null&&!EnemySpawnPlacement.IsVisible(cameraRig,elder.t),"Live boss spawns off-screen");
   yield return null;CaptureVillageCamera(Path.Combine(folder,"gameplay-no-visible-spawns.png"),1600,900);
   cameraRig.orthographicSize=33;yield return null;CaptureVillageCamera(Path.Combine(folder,"overview-offscreen-positions.png"),1600,900);
   report.passed=true;File.WriteAllText(Path.Combine(folder,"validation.json"),JsonUtility.ToJson(report,true));Debug.Log("VEILBOUND_SCREEN_SPAWN_RUNTIME_PASS models="+report.modelChecks+" fallbacks="+report.fallbackChecks);Application.Quit(0);
  }
 }
}
