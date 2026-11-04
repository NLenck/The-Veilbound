using System;using System.IO;using System.Collections;using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  IEnumerator BodyCollisionReview(){
   yield return null;combatReviewFreeze=true;city=FreshCharacterTown();RebuildHero();StartRun();int checks=0;
   foreach(int kind in new[]{0,1,2,3,4,5}){
    var f=AttackTestFoe(kind==5?0:kind,1,0,kind==5?1:0);float radius=f.radius+PlayerBodyRadius;
    var p=MovePlayerBody(Vector3.back*4,Vector3.forward*8);Require(p.z<0&&p.magnitude>=radius,"Walk cannot cross any enemy body including bosses");checks++;
    for(int frame=0;frame<120;frame++)p=MovePlayerBody(p,Vector3.forward*.15f);Require(p.z<0&&p.magnitude>=radius-.0001f,"Holding movement cannot squeeze through enemy");checks++;
    dodgeTime=.23f;p=MovePlayerBody(Vector3.back*2,Vector3.forward*(22*.23f));Require(p.z>3,"Dodge crosses every enemy body including bosses");dodgeTime=0;checks++;
    p=MovePlayerBody(Vector3.back*2,Vector3.forward*4);Require(p.z<0,"Walking blocks again immediately after dodge");checks++;
    Vector3 retreat=MovePlayerBody(p,Vector3.back*2);Require(retreat.z<p.z-1.9f,"Player can retreat from body contact");checks++;
    f.dead=true;p=MovePlayerBody(Vector3.back*2,Vector3.forward*4);Require(p.z>1.9f,"Dead enemies do not block movement");checks++;
   }
   var target=AttackTestFoe(0,1,0);Vector3 slide=MovePlayerBody(new Vector3(-.6f,0,-3),Vector3.forward*6);Require(slide.z>1&&slide.magnitude>target.radius+PlayerBodyRadius,"Glancing movement slides around body");checks++;
   collision.AddWall(new Vector3(-1.2f,0,-6),new Vector3(-1.2f,0,6),.1f);Vector3 trapped=MovePlayerBody(new Vector3(-.65f,0,-3),Vector3.forward*6);Require(trapped.z<0&&collision.Free(trapped,PlayerBodyRadius),"Cannot squeeze between enemy and wall or be pushed into wall");checks++;
   dodgeTime=.23f;collision.AddWall(new Vector3(-5,0,1),new Vector3(5,0,1),.15f);
   Vector3 wallDodge=MovePlayerBody(Vector3.back*2,Vector3.forward*6);Require(wallDodge.z<1&&collision.Free(wallDodge,PlayerBodyRadius),"Dodge still stops at arena walls");checks++;
   Vector3 edgeDodge=MovePlayerBody(Vector3.right*46,Vector3.right*10);Require(edgeDodge.magnitude<=collision.PlayerRadius-PlayerBodyRadius,"Dodge respects outer player boundary");checks++;
   dodgeTime=0;
   foreach(int variant in new[]{0,1}){
    var spirit=AttackTestFoe(3,0,variant);Require(IsEtherealEnemy(spirit)==(variant==1),"Ash Wraith phases while Gargoyle remains physical");checks++;
   }
   foreach(int kind in new[]{1,3}){
    var spirit=AttackTestFoe(kind,0,1);position=Vector3.forward*15;spirit.speed=4;spirit.attack=100;
    collision.AddWall(new Vector3(-20,0,1),new Vector3(20,0,1),.2f);collision.Add(Vector3.forward*2,1);
    navigation=new ArenaNavigation(collision);navigation.Tick(position,1,true);
    for(int i=0;i<20;i++)UpdateFoes(.05f);
    Require(IsEtherealEnemy(spirit)&&spirit.t.position.z>3.9f&&Mathf.Abs(spirit.t.position.x)<.001f,"Wraith and Lamenter travel directly through wall and solid prop");checks++;
    Vector3 inside=new Vector3(0,0,1);Vector3 separated=MoveEnemyTerrain(spirit,inside,Vector3.right*.1f);Require(Mathf.Abs(separated.z-1)<.001f,"Separation cannot eject ethereal enemies from walls");checks++;
    Vector3 edge=MoveEnemyTerrain(spirit,Vector3.right*57,Vector3.right*20);Require(edge.magnitude<=collision.Radius-spirit.radius,"Ethereal movement preserves arena boundary");checks++;
   }
   target=AttackTestFoe(0,1,0);position=Vector3.forward*2;navigation.Tick(position,1,true);target.speed=10;target.attack=100;UpdateFoes(1);Require(Vector3.Distance(target.t.position,position)>=target.radius+PlayerBodyRadius-.0001f,"Enemy movement also respects player body");checks++;
   foreach(bool evade in new[]{false,true}){
    var charger=AttackTestFoe(3,1,1);position=Vector3.forward*6;hero.root.position=position;health=MaxHealth;invulnerability=0;float hp=health;
    Require(HandleCharge(charger,0)&&charger.chargeWarning,"Real charging unit begins warning");if(evade)position=new Vector3(4,0,6);
    HandleCharge(charger,1.3f);HandleCharge(charger,1);
    if(evade)Require(!charger.chargeHit&&health==hp&&charger.t.position.z>7,"Dodged charge still completes its marked path");
    else Require(charger.chargeHit&&health<hp&&charger.t.position.z<6&&Vector3.Distance(charger.t.position,position)>=charger.radius+PlayerBodyRadius-.0001f,"Charge hits once and stops against player without tunnelling");checks++;
   }
   target=AttackTestFoe(0,1,0);
   for(int i=1;i<440;i++){var root=WorldArt.Group("Body stress fixture",actors);root.position=new Vector3((i%22-11)*1.8f,0,(i/22-10)*1.8f);foes.Add(new Foe{t=root,hp=100,maxHp=100,radius=.43f});}
   Vector3 crowded=MovePlayerBody(new Vector3(-24,0,0),Vector3.right*48);Require(crowded.x<-15&&collision.Free(crowded,PlayerBodyRadius),"Full crowd blocks passage safely");checks++;
   string folder=Path.Combine(Application.dataPath,"..","Smoke","BodyCollision");Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"validation.json"),"{\"passed\":true,\"checks\":"+checks+",\"crowd\":440}");Debug.Log("VEILBOUND_BODY_COLLISION_PASS checks="+checks);Application.Quit(0);
  }
 }
}
