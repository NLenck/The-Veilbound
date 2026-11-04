using System;using System.IO;using System.Collections;using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  int pursuitChecks;
  void PursuitCheck(bool value,string text){Require(value,text);pursuitChecks++;}
  void PursuitPair(int kind,int boss,int otherKind,int otherBoss,bool separate,int map=1,int variant=0){
   var first=AttackTestFoe(kind,map,variant,boss);first.speed=0;first.attack=100;var rig=new HuskRig(actors,otherKind,otherBoss,map,0);rig.root.position=Vector3.right*.1f;
   var other=new Foe{rig=rig,t=rig.root,kind=otherKind,boss=otherBoss,radius=otherBoss>0?1.1f:EnemyTypes.Radius(otherKind),hp=10000,maxHp=10000,attack=100,speed=0};foes.Add(other);position=Vector3.forward*10;navigation.Tick(position,1,true);
   Vector3 beforeFirst=first.t.position,beforeOther=other.t.position;UpdateFoes(.1f);
   PursuitCheck(separate?Vector3.Distance(first.t.position,other.t.position)>.11f:(first.t.position-beforeFirst).sqrMagnitude<.000001f&&(other.t.position-beforeOther).sqrMagnitude<.000001f,"Crowd pair honors ordinary/fast/aberration grouping");
   if(!separate){var blocked=MovePlayerBody(Vector3.back*4,Vector3.forward*8);PursuitCheck(blocked.z<0,"Crowd phasing never bypasses collision with the player");}
  }
  IEnumerator EnemyPursuitReview(){
   yield return null;combatReviewFreeze=true;city=FreshCharacterTown();city.InitializeWoodenStarter();RebuildHero();StartRun();
   PursuitPair(0,0,0,0,true);PursuitPair(3,0,0,0,false);PursuitPair(3,0,2,0,false);PursuitPair(0,1,0,0,false);PursuitPair(2,2,3,0,false);PursuitPair(3,0,3,0,true);PursuitPair(0,1,0,2,true);PursuitPair(0,0,0,0,false,0,1);
   float[] seconds={0,59.99f,60,119.99f,120,180,240,300,360},factors={1,1,1.05f,1.05f,1.15f,1.30f,1.50f,1.75f,2.05f};
   for(int i=0;i<seconds.Length;i++)PursuitCheck(Mathf.Abs(1+EnemyPursuit.EnrageBonus(seconds[i])-factors[i])<.00001f,"Minute boundaries match the requested additive bonus sequence");
   var walker=AttackTestFoe(0,1,0);walker.attack=100;walker.speed=2;position=Vector3.left*25;navigation.Tick(position,1,true);
   walker.t.position=Vector3.right*25;PursuitCheck(DistantEnemyFactor(walker)==1,"Exactly fifty metres has normal speed");walker.t.position=Vector3.right*25.01f;PursuitCheck(DistantEnemyFactor(walker)==2,"Beyond fifty metres has double speed");
   walker.t.position=Vector3.right*24;Vector3 before=walker.t.position;UpdateFoes(.05f);float nearMove=Vector3.Distance(before,walker.t.position);walker.t.position=Vector3.right*26;before=walker.t.position;UpdateFoes(.05f);float farMove=Vector3.Distance(before,walker.t.position);PursuitCheck(Mathf.Abs(nearMove-.1f)<.02f&&Mathf.Abs(farMove-.2f)<.02f,"Actual navigation movement doubles beyond fifty metres");
   walker.t.position=Vector3.right*25.1f;UpdateFoes(.1f);PursuitCheck(DistantEnemyFactor(walker)==1,"Catch-up switches off immediately after closing inside threshold");
   PressureStart(1,20);elapsed=100;var firstBoss=SpawnFoe(1);var normal=SpawnFoe(0,0);PursuitCheck(firstBoss!=null&&normal!=null&&firstBoss.spawnedAt==100,"Actual spawned enemies record their own birth time");firstBoss.t.position=Vector3.forward*10;normal.t.position=Vector3.right*10;float baseSpeed=firstBoss.speed,maxHealth=firstBoss.maxHp;
   elapsed=160;var secondBoss=SpawnFoe(2);secondBoss.t.position=Vector3.left*10;PursuitCheck(Mathf.Abs(AberrationAgeFactor(firstBoss)-1.05f)<.001f&&AberrationAgeFactor(secondBoss)==1,"Separately spawned aberrations enrage independently");
   for(int minute=1;minute<=6;minute++){elapsed=100+minute*60;float expected=1+EnemyPursuit.EnrageBonus(minute*60);PursuitCheck(Mathf.Abs(EnemyMovementSpeed(firstBoss)-baseSpeed*expected)<.0001f&&Mathf.Abs(ExpeditionDamageFactor(firstBoss)-RunModifierFactor(3)*firstBoss.spawnDamageFactor*expected)<.0001f,"Actual aberration movement and damage multiply depth/modifier values");PursuitCheck(firstBoss.maxHp==maxHealth&&AberrationAgeFactor(normal)==1,"Aging changes neither health nor ordinary monster stats");}
   elapsed=220;health=MaxHealth;invulnerability=0;float hp=health;Hurt(5,firstBoss);float expectedDamage=5*RunModifierFactor(3)*firstBoss.spawnDamageFactor*1.15f;PursuitCheck(Mathf.Abs(hp-health-expectedDamage)<.001f,"Direct incoming damage applies aberration age bonus");
   health=MaxHealth;invulnerability=0;hp=health;Telegraph(position,2,.1f,5,owner:firstBoss);UpdateHazards(.11f);PursuitCheck(Mathf.Abs(hp-health-expectedDamage)<.001f,"Delayed attacks retain owner and age-scaled damage");
   position=Vector3.left*25;firstBoss.t.position=Vector3.right*26;PursuitCheck(Mathf.Abs(EnemyMovementSpeed(firstBoss)-baseSpeed*1.15f*2)<.0001f&&Mathf.Abs(ExpeditionDamageFactor(firstBoss)-RunModifierFactor(3)*firstBoss.spawnDamageFactor*1.15f)<.0001f,"Catch-up multiplies aged movement but never damage");
   float clock=elapsed,age=AberrationAgeFactor(firstBoss);SetState(ScreenState.Paused);combatReviewFreeze=false;yield return new WaitForSecondsRealtime(.2f);combatReviewFreeze=true;PursuitCheck(elapsed==clock&&AberrationAgeFactor(firstBoss)==age,"Pausing cannot advance aberration age");SetState(ScreenState.Expedition);
   elapsed=280;position=Vector3.zero;hero.root.position=position;TownCamera(position,12);firstBoss.t.position=Vector3.forward*8;secondBoss.t.position=new Vector3(-8,0,7);PursuitCheck(AberrationStatus(firstBoss).Contains("+30% SPEED / DAMAGE"),"Boss bar exposes current enrage bonus");
   string folder=Path.Combine(Application.dataPath,"..","Smoke","EnemyPursuit");Directory.CreateDirectory(folder);yield return CardScreenshot(Path.Combine(folder,"01-aberration-enrage.png"));
   float atDeath=AberrationAgeFactor(firstBoss);Hit(firstBoss,1000000,"cleave",false);elapsed+=120;PursuitCheck(Mathf.Abs(AberrationAgeFactor(firstBoss)-atDeath)<.00001f,"Death stops age growth for any lingering owned attacks");
   File.WriteAllText(Path.Combine(folder,"validation.json"),"{\"passed\":true,\"checks\":"+pursuitChecks+",\"catchupDistance\":50,\"catchupMultiplier\":2,\"enragePercentByMinute\":[5,15,30,50,75,105]}");Debug.Log("VEILBOUND_ENEMY_PURSUIT_PASS checks="+pursuitChecks);Application.Quit(0);
  }
 }
}
