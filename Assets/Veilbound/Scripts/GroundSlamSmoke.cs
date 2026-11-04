using System;
using System.IO;
using System.Collections;
using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  Foe SlamTestEnemy(bool elder=false){StartRun();position=Vector3.forward*2;hero.root.position=position;var enemy=elder?SpawnFoe(1):ReadoutTarget(Vector3.zero,2);enemy.t.position=Vector3.zero;enemy.attack=0;return enemy;}
  void CheckGroundSlams() {
   float oldMaster=preferences.master;preferences.master=1;ApplyAudioPreferences();Require(smoke&&AudioListener.volume==0,"Automated game tests are silent even with master preference at full");preferences.master=oldMaster;ApplyAudioPreferences();
   var enemy=SlamTestEnemy();float hp=health;invulnerability=0;Vector3 planted=enemy.t.position;int audioEvents=abilitySoundEvents;UpdateFoes(0);
   Require(hazards.Count==1&&enemy.slamHazard==hazards[0]&&hazards[0].source=="enemy-slam"&&hazards[0].duration==1&&Mathf.Abs(hazards[0].radius-2.82f)<.001f,"Nearby Brute starts a one-second enlarged circular slam");
   var slam=enemy.slamHazard;UpdateFoes(.5f);UpdateHazards(.5f);Require(health==hp&&enemy.t.position==planted&&Mathf.Abs(slam.warning.Progress-.5f)<.001f,"Enemy stays planted, deals no early contact damage and shows a half-grown warning after half a second");
   enemy.rig.Animate(0,0,0);enemy.rig.visual.GroundSlamPose(.7f);float raisedHand=enemy.rig.visual.bones[7].position.y;
   UpdateHazards(.49f);Require(health==hp,"Slam does no damage before its full one-second cast");UpdateHazards(.02f);
   Require(enemy.rig.visual.bones[7].position.y<raisedHand-.3f&&Mathf.Abs(enemy.rig.visual.bones[13].position.y-enemy.t.TransformPoint(enemy.rig.visual.rest[13]).y)<.04f,"Impact brings the arms down and keeps the feet planted");
   Require(Mathf.Abs(hp-health-24*(1-city.Protection))<.01f&&hazards.Count==0&&enemy.slamHazard==null&&enemy.slamRecovery>0,"Slam lands once at full cast and starts recovery");
   hp=health;UpdateHazards(2);Require(health==hp&&abilitySoundEvents==audioEvents,"Slam cannot deal repeated impact damage or emit an enemy sound");
   UpdateFoes(.2f);Require(enemy.t.position==planted&&hazards.Count==0,"Enemy stays planted during its impact recovery");UpdateFoes(.3f);UpdateFoes(0);Require(hazards.Count==0&&enemy.attack>4,"Cooldown prevents immediate repeated ground slams");
   enemy=SlamTestEnemy();hp=health;UpdateFoes(0);position=Vector3.forward*6;UpdateHazards(1.01f);Require(health==hp,"Leaving the marked radius avoids the slam");
   enemy=SlamTestEnemy();hp=health;UpdateFoes(0);Hit(enemy,200000,"cleave");UpdateHazards(1.01f);Require(health==hp&&hazards.Count==0,"Killing a caster cancels its slam and removes its warning");
   enemy=SlamTestEnemy();UnlockKit("warcry");CastWarCry();hp=health;invulnerability=0;UpdateFoes(0);UpdateHazards(1.01f);Require(Mathf.Abs(hp-health-24*(1-city.Protection)*.8f)<.01f,"War Cry reduces the actual caster's ground-slam damage");
   enemy=SlamTestEnemy(true);UpdateFoes(0);Require(enemy.slamHazard!=null&&Mathf.Abs(enemy.slamHazard.radius-3.36f*enemy.t.localScale.x)<.001f,"Nearby Elder bosses use the enlarged ground slam");
   enemy.rig.Animate(0,0,0);float lowHand=enemy.rig.visual.bones[7].position.y;enemy.rig.visual.GroundSlamPose(.65f);Require(enemy.rig.visual.bones[7].position.y>lowHand+.2f,"Elder visibly raises its huge support arm during the wind-up");
   StartRun();enemy=ReadoutTarget(Vector3.forward*8,3);enemy.attack=0;UpdateFoes(0);Require(hazards.Count==0,"Small enemies do not acquire the large-enemy slam");
   enemy=SlamTestEnemy(true);enemy.t.position=Vector3.forward*9;UpdateFoes(0);Require(hazards.Count==1&&hazards[0].source!="enemy-slam","Elder keeps its existing ranged boss attack when far away");
   StartRun();Require(hazards.Count==0&&AudioListener.volume==0,"New-run cleanup clears slam hazards and testing remains muted");Debug.Log("VEILBOUND_GROUND_SLAM_LOGIC_PASS");
  }
  void SlamCamera(Foe enemy) {hero.root.position=position;hero.root.rotation=Quaternion.LookRotation((enemy.t.position-position).normalized);hero.Animate(0,0,0);enemy.t.rotation=Quaternion.LookRotation(position-enemy.t.position);cameraRig.transform.position=new Vector3(6,7,-9);cameraRig.transform.LookAt(new Vector3(0,.8f,0));cameraRig.orthographicSize=4.6f;}
  IEnumerator GroundSlamReview() {
   string folder=Path.Combine(Application.dataPath,"..","Smoke","GroundSlam");Directory.CreateDirectory(folder);string pass=Path.Combine(folder,"PASS.txt");if(File.Exists(pass))File.Delete(pass);
   yield return new WaitForSecondsRealtime(.5f);ReturnHome();combatReviewFreeze=true;CheckGroundSlams();
   foreach(bool elder in new[]{false,true}) {
    var enemy=SlamTestEnemy(elder);position=Vector3.back*2.1f;SlamCamera(enemy);UpdateFoes(0);string name=elder?"elder":"brute";
    yield return CardScreenshot(Path.Combine(folder,name+"-01-warning.png"));
    UpdateHazards(.5f);enemy.rig.Animate(0,0,0);enemy.rig.visual.GroundSlamPose(.5f);yield return CardScreenshot(Path.Combine(folder,name+"-02-windup.png"));
    UpdateHazards(.25f);enemy.rig.Animate(0,0,0);enemy.rig.visual.GroundSlamPose(.75f);yield return CardScreenshot(Path.Combine(folder,name+"-03-raised-arm.png"));
    UpdateHazards(.26f);yield return CardScreenshot(Path.Combine(folder,name+"-04-impact.png"));
   }
   Require(AudioListener.volume==0,"Capture session stayed silent throughout");File.WriteAllText(pass,"PASS: Brute and Elder proximity slam; one-second cast and growing circle; planted caster; no contact damage during wind-up/recovery; single impact; dodge escape; death cancellation; source-aware War Cry mitigation; cooldown; Elder support-arm pose; existing ranged boss attack; excluded small enemies; cleanup; automated audio stays zero including full master preference. Isolated smoke save.");Debug.Log("VEILBOUND_GROUND_SLAM_PASS");Application.Quit(0);
  }
 }
}
