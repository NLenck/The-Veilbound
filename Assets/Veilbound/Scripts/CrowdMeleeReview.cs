using System;using System.IO;using System.Collections;using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  [Serializable]class CrowdMeleeReport {public bool passed;public int cap=RegularEnemyCap,maps;public float map0AiMilliseconds,map1AiMilliseconds;}
  IEnumerator CrowdMeleeReview(){
   yield return null;combatReviewFreeze=true;var report=new CrowdMeleeReport();city=FreshCharacterTown();RebuildHero();StartRun();
   foreach(int kind in new[]{0,3}){
    var f=AttackTestFoe(kind,1,0);position=Vector3.forward*(MeleeReach(f)-.02f);hero.root.position=position;health=MaxHealth;invulnerability=0;
    float hp=health;Require(HandleMelee(f,0)&&health==hp,"Melee starts at increased reach with no instant hit");
    HandleMelee(f,MeleeWindup(f)-.01f);Require(health==hp,"No damage before new windup ends");HandleMelee(f,.02f);Require(health<hp,"Impact hits inside the added reach");
    hp=health;invulnerability=0;HandleMelee(f,.1f);Require(health==hp,"Strike hits once");HandleMelee(f,MeleeRecovery(f)+.03f);Require(!f.meleeActive,"Shorter recovery completes");
    f.attack=0;position=Vector3.forward;HandleMelee(f,0);position=Vector3.back;hp=health;invulnerability=0;HandleMelee(f,MeleeWindup(f)+.01f);Require(health==hp,"Moving behind committed swing avoids damage");
    f.meleeActive=false;f.attack=0;position=Vector3.forward*(MeleeReach(f)+.1f);Require(!HandleMelee(f,0),"Outside reach does not start a melee attack");
    f.meleeActive=false;f.attack=0;position=Vector3.forward;collision.AddWall(new Vector3(-3,0,.5f),new Vector3(3,0,.5f),.1f);Require(!HandleMelee(f,0),"Melee cannot start through a wall");
   }
   foreach(int map in new[]{0,1}){
    PressureStart(map,1);for(int i=0;i<5;i++)attacks[i]=10000;invulnerability=100;bossesSpawned=0;
    for(int i=0;i<439;i++){Require(SpawnFoe(0,i%4)!=null,"Full-size crowd can find off-screen reachable spawns");if(i%40==0)yield return null;}
    spawnClock=-1;Step(0);Require(foes.Count==440,"Regular spawning continues beyond old cap to 440");
    spawnClock=-1;Step(0);Require(foes.Count==440,"Regular spawning stops at new cap");
    Hit(foes[0],1000000,"cleave",false);Require(foes.Count==439,"A killed enemy frees a slot");spawnClock=-1;Step(0);Require(foes.Count==440,"Regular spawning resumes when a slot frees");
    Require(SpawnFoe(1)!=null&&foes.Count==441,"Boss spawning remains independent of regular cap");
    double ms=0;var timer=new System.Diagnostics.Stopwatch();
    for(int frame=0;frame<90;frame++){animClock+=1/60f;timer.Restart();UpdateFoes(1/60f);UpdateEnemyArrivalFog(1/60f);timer.Stop();if(frame>=30)ms+=timer.Elapsed.TotalMilliseconds;yield return null;}
    if(map==0)report.map0AiMilliseconds=(float)(ms/60);else report.map1AiMilliseconds=(float)(ms/60);report.maps++;
   }
   report.passed=true;string folder=Path.Combine(Application.dataPath,"..","Smoke","CrowdMelee");Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"validation.json"),JsonUtility.ToJson(report,true));Debug.Log("VEILBOUND_CROWD_MELEE_PASS "+JsonUtility.ToJson(report));Application.Quit(0);
  }
 }
}
