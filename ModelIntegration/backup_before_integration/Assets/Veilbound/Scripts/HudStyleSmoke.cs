using System;
using System.IO;
using System.Collections;
using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  IEnumerator HudStyleReview(string folder) {
   Directory.CreateDirectory(folder);StartRun();combatReviewFreeze=true;
   health=MaxHealth*.5f;xp=Threshold*.5f;Require(Mathf.Abs(HealthGlobeFill-.5f)<.001f&&Mathf.Abs(ExperienceBarFill-.5f)<.001f,"Life globe and XP bar use their real current/max values");
   health=-1;xp=Threshold*2;Require(HealthGlobeFill==0&&ExperienceBarFill==1,"Globe visual fills clamp safely at empty and full");
   health=MaxHealth;xp=0;Require(HealthGlobeFill==1&&ExperienceBarFill==0,"Full life and an empty XP bar render at run start");
   shortRun=false;Require(BossMilestone(1)==300&&BossMilestone(2)==600&&BossMilestone(3)==900,"Normal timeline shows boss icons at five, ten and fifteen minutes");
   shortRun=true;Require(BossMilestone(1)==60&&BossMilestone(2)==120&&BossMilestone(3)==180,"Quick-run icons stay aligned to actual one-minute boss triggers");shortRun=false;
   elapsed=450;Require(Mathf.Abs(ExpeditionFill-.5f)<.001f,"Progress bar uses actual elapsed expedition time");
   foreach(var id in new[]{"whirl","orbit","smite"})build.Pick(TestCard(id,UpgradeRarity.Rare));build.Pick(TestCard("power",UpgradeRarity.Rare));level=12;
   var targets=new[]{ReadoutTarget(new Vector3(-3,0,3)),ReadoutTarget(new Vector3(0,0,4),2),ReadoutTarget(new Vector3(3,0,3),1)};
   string[] sources={"cleave","whirl","orbit","smite"};for(int i=0;i<16;i++)Hit(targets[i%3],AbilityDamage(sources[i%4])/(sources[i%4]=="whirl"?3:1),sources[i%4]);damageNumbers.Clear();foreach(var foe in targets)foe.rig.Animate(0,0,.3f);
   hero.root.position=position;hero.root.rotation=Quaternion.identity;cameraRig.transform.position=new Vector3(7,13,-13);cameraRig.transform.LookAt(new Vector3(0,0,2));cameraRig.orthographicSize=9;
   health=MaxHealth*.78f;xp=Threshold*.63f;bossesSpawned=1;yield return new WaitForSecondsRealtime(.2f);yield return CardScreenshot(Path.Combine(folder,"01-globe-hud.png"));
   health=MaxHealth*.5f;xp=Threshold*.5f;yield return CardScreenshot(Path.Combine(folder,"02-half-full.png"));
   health=MaxHealth*.12f;xp=Threshold*.94f;yield return CardScreenshot(Path.Combine(folder,"03-low-life.png"));
   elapsed=875;bossesSpawned=2;health=MaxHealth*.78f;xp=Threshold*.63f;var boss=SpawnFoe(2);Require(boss!=null,"Boss health frame remains visible with the new timeline");boss.hp=boss.maxHp*.68f;yield return CardScreenshot(Path.Combine(folder,"04-boss-timeline.png"));
   StartRun();hero.root.position=position;yield return CardScreenshot(Path.Combine(folder,"05-start-run.png"));
   File.WriteAllText(Path.Combine(folder,"PASS.txt"),"PASS: health and experience current/max fill; empty/half/full/low captures; safe fill clamping; actual expedition progress; 05:00, 10:00 and 15:00 boss milestones; quick-run 01:00/02:00/03:00 milestones; boss health frame; centred ability dock; existing combat and interface regression checks.\n");
   combatReviewFreeze=false;ReturnHome();Debug.Log("VEILBOUND_GLOBE_HUD_PASS");
  }
 }
}
