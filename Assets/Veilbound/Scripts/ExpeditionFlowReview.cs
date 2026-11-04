using System;
using System.IO;
using System.Collections;
using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  IEnumerator FlowScreenshot(string path){
   yield return null;var target=new RenderTexture(1600,900,24);var previous=RenderTexture.active;var oldTarget=cameraRig.targetTexture;
   cameraRig.targetTexture=target;cameraRig.Render();RenderTexture.active=target;var pixels=new Texture2D(1600,900,TextureFormat.RGB24,false);pixels.ReadPixels(new Rect(0,0,1600,900),0,0);pixels.Apply();File.WriteAllBytes(path,pixels.EncodeToPNG());
   RenderTexture.active=previous;cameraRig.targetTexture=oldTarget;target.Release();Destroy(target);Destroy(pixels);
  }
  Vector3 FlowTestGround(){for(int z=-25;z<=25;z+=5)for(int x=-25;x<=25;x+=5){var p=new Vector3(x,0,z);if(collision.Free(p,4))return p;}throw new Exception("No open ground for melee validation");}
  Foe FlowMeleeTarget(int kind=0){StartRun();var f=SpawnFoe(0,kind);Require(f!=null,"Melee test target spawns");f.t.position=FlowTestGround();position=f.t.position+Vector3.forward*.8f;hero.root.position=position;f.speed=0;f.attack=0;invulnerability=0;return f;}
  IEnumerator ExpeditionFlowReview(){
   yield return null;combatReviewFreeze=true;city=Settlement.NewCamp();tier=1;shortRun=false;
   string folder=Path.Combine(Application.dataPath,"..","Smoke","ExpeditionFlow");Directory.CreateDirectory(folder);
   var f=FlowMeleeTarget();float hp=health;Vector3 planted=f.t.position;UpdateFoes(0);Require(f.meleeActive&&health==hp,"Touch begins a windup without damage");
   UpdateFoes(.25f);Require(health==hp&&f.t.position==planted,"Windup plants enemy feet and has no early hit");
   hero.root.position=position;cameraRig.transform.position=planted+new Vector3(5,6,-8);cameraRig.transform.LookAt(planted+Vector3.up);cameraRig.orthographicSize=4.5f;
   yield return FlowScreenshot(Path.Combine(folder,"01-melee-windup.png"));
   position=planted+Vector3.back*.8f;UpdateFoes(.26f);Require(health==hp&&f.meleeLanded&&Vector3.Dot(f.t.forward,Vector3.forward)>.99f,"Weaving behind committed strike avoids damage without enemy swivelling");
   f=FlowMeleeTarget();hp=health;UpdateFoes(0);UpdateFoes(.27f);Require(health==hp,"Husk cannot hit before point two eight second windup");UpdateFoes(.02f);Require(health<hp,"Husk applies damage at impact");hp=health;invulnerability=0;UpdateFoes(.3f);Require(health==hp,"Strike deals damage only once during recovery");
   f=FlowMeleeTarget(3);hp=health;UpdateFoes(0);UpdateFoes(.17f);Require(health==hp,"Ghoul has a visible windup");UpdateFoes(.02f);Require(health<hp,"Ghoul strike lands after point one eight seconds");
   f=FlowMeleeTarget();hp=health;UpdateFoes(0);position=f.t.position+Vector3.forward*4;UpdateFoes(.51f);Require(health==hp,"Moving out of reach avoids strike");
   f=FlowMeleeTarget();hp=health;UpdateFoes(0);Hit(f,100000,"cleave");UpdateFoes(.6f);Require(health==hp,"Killing winding-up enemy cancels attack");
   f=FlowMeleeTarget(1);f.attack=0;UpdateFoes(0);Require(f.lobWindup>0&&lobs.Count==0,"Lobber winds up before releasing projectile");UpdateFoes(.59f);Require(lobs.Count==0,"No early lob release");UpdateFoes(.02f);Require(lobs.Count==1,"Lob releases after windup");
   CheckGroundSlams();
   StartRun();activeMap=1;elapsed=0;for(int i=0;i<30;i++){var e=SpawnFoe(0);Require(e!=null&&e.kind==0&&e.variant==0&&e.rig.visual.production.assetId=="Town/Husk","Opening village spawns actual Husk model only");}
   StartRun();elapsed=320;Require(SpawnFoe(1)!=null,"First boss spawns");elapsed=349.99f;Require(SpawnStage==2,"No advanced wave before thirty seconds");elapsed=350;Require(SpawnStage==3,"Advanced wave unlocks thirty seconds after actual boss arrival");
   StartRun();float firstDue=nextRelic;elapsed=firstDue-.01f;UpdateRelics(0);Require(relics.Count==0,"No relic before scheduled time");elapsed=firstDue;UpdateRelics(0);Require(relics.Count==1,"First relic spawns within fifteen to thirty seconds");
   foreach(var r in relics)Require(Vector3.Distance(r.point,position)>=12&&r.point.magnitude<collision.PlayerRadius&&collision.Free(r.point,.65f)&&navigation.Reachable(r.point,.65f),"Relic lies on reachable ground outside immediate pickup range");
   Require(nextRelic-elapsed>=15&&nextRelic-elapsed<=30,"Subsequent relic delay is fifteen to thirty seconds");Require(SpawnRelic()&&relics.Count==2&&!SpawnRelic(),"Only two uncollected relics can exist");
   int passiveBefore=0;foreach(var c in RunBuild.Pool)if(!c.ability)passiveBefore+=build.Rank(c.id);int slots=build.Slots;float experience=xp;
   position=relics[0].point;hero.root.position=position;UpdateRelics(0);Require(state==ScreenState.Choice&&build.PassiveRound&&build.offers.TrueForAll(c=>!c.ability),"Relic opens passive choices");Choose(0);int passiveAfter=0;foreach(var c in RunBuild.Pool)if(!c.ability)passiveAfter+=build.Rank(c.id);
   Require(passiveAfter==passiveBefore+1&&relics.Count==1&&build.Slots==slots&&xp==experience,"Choosing grants exactly one passive without consuming ability slots or changing XP");
   for(int i=0;i<50;i++)PositionCamera(.1f);yield return FlowScreenshot(Path.Combine(folder,"02-relic-overview.png"));
   position=relics[0].point-Vector3.forward*3;hero.root.position=position;cameraRig.transform.position=position+new Vector3(5,6,-8);cameraRig.transform.LookAt(relics[0].point+Vector3.up);cameraRig.orthographicSize=5;
   yield return FlowScreenshot(Path.Combine(folder,"03-relic-beacon.png"));
   StartRun();Require(relics.Count==0&&nextRelic>=15&&nextRelic<=30&&bossArrival[0]<0,"New run resets pickups and boss unlock history");foreach(var c in RunBuild.Pool)if(!c.ability)Require(build.Rank(c.id)==0,"Relic passives expire with expedition");
   CheckExpeditionSetup();File.WriteAllText(Path.Combine(folder,"PASS.txt"),"PASS: timed model progression; heavy caps and weights; actual boss arrival delays; melee windup/impact/recovery, weaving, reach and death cancellation; Lobber windup; enlarged slam warning and damage; reachable relics, cadence and cap; one passive per pickup; run cleanup; existing economy and projected production.");
   Debug.Log("VEILBOUND_EXPEDITION_FLOW_RUNTIME_PASS");Application.Quit(0);
  }
 }
}
