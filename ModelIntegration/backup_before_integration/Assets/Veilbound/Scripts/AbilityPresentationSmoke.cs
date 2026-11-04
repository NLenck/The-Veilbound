using System;
using System.IO;
using System.Collections;
using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  void CheckAbilityPresentation() {
   StartRun();
   Require(playerClips.Count==13&&playerVoices.Length==8,"Thirteen player-only clips and a bounded eight-voice mixer are present");
   foreach(string id in PlayerSoundIds){var clip=playerClips[id];Require(clip&&clip.samples>4000&&clip.channels==2,"Stereo ability audio loads: "+id);var data=new float[clip.samples*clip.channels];clip.GetData(data,0);float peak=0;foreach(float s in data){Require(!float.IsNaN(s)&&Mathf.Abs(s)<.99f,"Audio sample is finite and unclipped");peak=Mathf.Max(peak,Mathf.Abs(s));}Require(peak>.1f,"Ability sound is not silent: "+id);}
   float effectVolume=preferences.effects,master=preferences.master;preferences.effects=preferences.master=1;
   Require(PlayAbilitySound("lance")&&lastAbilitySound=="lance"&&!PlayAbilitySound("lance"),"A player ability plays once; duplicate volley sounds are throttled");
   int events=abilitySoundEvents;Require(!PlayAbilitySound("enemy")&&abilitySoundEvents==events,"Enemy identifiers cannot enter the player-only sound bus");
   preferences.effects=0;Require(!PlayAbilitySound("brand"),"Game effects volume can mute player abilities");preferences.effects=1;preferences.master=0;Require(!PlayAbilitySound("warcry"),"Master mute also suppresses player ability sounds");preferences.master=1;
   ResetAbilityPresentation();audioSource.Stop();var enemy=ReadoutTarget(Vector3.forward*3);invulnerability=0;Hurt(5,enemy);
   Require(abilitySoundEvents==0&&!audioSource.isPlaying&&enemy.t.GetComponentsInChildren<AudioSource>().Length==0,"Taking an enemy hit creates no sound");
   var volatileEnemy=ReadoutTarget(Vector3.forward*8,4);Detonate(volatileEnemy);Require(abilitySoundEvents==0&&!audioSource.isPlaying,"Enemy explosions remain silent");
   StartRun();UnlockKit("lance");build.Pick(KitUpgrade("lance","projectiles",UpgradeRarity.Rare));float damage=AbilityDamage("lance");ExecuteRiftBlades();Require(bolts.Count==3,"The dagger volley preserves rarity-scaled projectile count");
   var bolt=bolts[1];Require(bolt.damage==damage&&bolt.t.Find("Dagger spin/Pointed steel blade")&&bolt.t.GetComponentsInChildren<TrailRenderer>().Length==1&&Array.TrueForAll(bolt.t.GetComponentsInChildren<Collider>(),c=>!c.enabled),"Rift Edge is a pointed steel dagger with an ember trail and no active physics collider");
   Vector3 before=bolt.t.position;Quaternion rotation=bolt.t.Find("Dagger spin").localRotation;UpdateBolts(.05f);Require(Mathf.Abs(Vector3.Distance(before,bolt.t.position)-.7f)<.001f&&Quaternion.Angle(rotation,bolt.t.Find("Dagger spin").localRotation)>1,"Thrown daggers travel at the existing speed and visibly spin");
   StartRun();UnlockKit("lance");var grazing=ReadoutTarget(Vector3.forward*3);float hp=grazing.hp;FireBolt(grazing.t.position+Vector3.right*(grazing.radius+.6f-.02f)+Vector3.up*1.15f,Vector3.forward,AbilityDamage("lance"),false);UpdateBolts(0);Require(Mathf.Abs(hp-grazing.hp-AbilityDamage("lance"))<.01f,"Hand-height presentation preserves the original ground-plane projectile hit width");
   hero.ClearAbilityGesture();hero.Animate(0,0,0);var rig=hero.root.GetComponent<BronzeHeroVisual>();Vector3 hand=rig.bones[7].position;hero.PlayAbilityGesture("lance");hero.AdvanceAbilityGesture(.21f);hero.Animate(0,0,0);Require(Vector3.Distance(hand,rig.bones[7].position)>.03f,"Dagger cast releases the off hand into a throw gesture");hero.ClearAbilityGesture();
   PlayerEffect("warcry",position,4,.75f);var wave=playerFx[playerFx.Count-1];float oldRadius=wave.lines[0].GetPosition(0).magnitude;UpdatePlayerPresentation(.2f);Require(wave.lines[0].GetPosition(0).magnitude>oldRadius,"War Cry rings expand over their lifetime");
   UpdatePlayerPresentation(2);Require(playerFx.Count==0,"Transient ability visuals clean up after completion");
   Require(PlayAbilitySound("brand"),"Ability sounds resume after reset");SetState(ScreenState.Paused);TickAbilityAudio();foreach(var voice in playerVoices)Require(!voice.isPlaying,"Opening a paused menu pauses ability voices");SetState(ScreenState.Expedition);TickAbilityAudio();
   ResetAbilityPresentation();Require(playerFx.Count==0&&abilitySoundEvents==0&&nextAbilitySound.Count==0,"New-run cleanup clears sounds and animation state");
   preferences.effects=effectVolume;preferences.master=master;ApplyAudioPreferences();StartRun();Debug.Log("VEILBOUND_PLAYER_ABILITY_PRESENTATION_LOGIC_PASS");
  }
  void PrepareAbilityShot(string id) {
   StartRun();combatReviewFreeze=true;string ability=id=="smite-impact"?"smite":id;
   if(ability!="dodge"&&ability!="rage")UnlockKit(ability);
   level=12;elapsed=55;health=MaxHealth*.72f;xp=Threshold*.4f;hero.root.position=position;hero.root.rotation=Quaternion.identity;hero.Animate(0,0,0);
   for(int i=0;i<3;i++)ReadoutTarget(new Vector3((i-1)*1.6f,0,id=="lance"?5.5f:2.8f));
   cameraRig.transform.position=new Vector3(5,8,-9);cameraRig.transform.LookAt(new Vector3(0,.6f,2));cameraRig.orthographicSize=5.7f;
   switch(id){
    case "cleave":ExecuteBasicAttack();break;
    case "whirl":BeginWeaponSweep(AbilityDamage("whirl"));UpdateCombatWeapons(.17f);break;
    case "lance":build.Pick(KitUpgrade("lance","projectiles",UpgradeRarity.Rare));ExecuteRiftBlades();UpdateBolts(.12f);break;
    case "orbit":for(int i=0;i<5;i++)attacks[i]=100;attacks[3]=0;Attack(0);UpdateCombatWeapons(.1f);break;
    case "smite":Telegraph(new Vector3(0,0,2.8f),SkillRadius("smite"),.5f,-AbilityDamage("smite"));UpdateHazards(.22f);break;
    case "smite-impact":Telegraph(new Vector3(0,0,2.8f),SkillRadius("smite"),.5f,-AbilityDamage("smite"));UpdatePlayerPresentation(.51f);UpdateHazards(.51f);break;
    case "brand":CastRageBrand();break;
    case "bloodthirst":CastBloodthirst();break;
    case "warcry":CastWarCry();break;
    case "vengeance":for(int i=0;i<16;i++)build.Pick(KitUpgrade("vengeance","chance"));invulnerability=0;Hurt(20,foes[1]);break;
    case "rend":foreach(var enemy in foes)TryRend(enemy,"cleave",0);break;
    case "rage":rage=100;ActivateRage();break;
    case "dodge":movement=Vector2.right;dodgeCooldown=0;TryDodge();break;
   }
   UpdatePlayerPresentation(id=="smite"?.22f:id=="smite-impact"?.12f:.17f);hero.Animate(0,0,id=="cleave"||id=="vengeance"?.7f:0);
  }
  IEnumerator AbilityPresentationReview() {
   string folder=Path.Combine(Application.dataPath,"..","Smoke","PlayerAbilities");Directory.CreateDirectory(folder);string pass=Path.Combine(folder,"PASS.txt");if(File.Exists(pass))File.Delete(pass);
   yield return new WaitForSecondsRealtime(.5f);ReturnHome();combatReviewFreeze=true;
   CheckAbilityPresentation();CheckCombatReadouts();CheckRageAbilityKit();
   int index=0;foreach(string id in PlayerSoundIds){PrepareAbilityShot(id);yield return new WaitForSecondsRealtime(.12f);yield return CardScreenshot(Path.Combine(folder,(++index).ToString("00")+"-"+id+".png"));}
   File.WriteAllText(pass,"PASS: thirteen original stereo ability sounds; finite unclipped waveforms; master/effects mute; per-ability duplicate throttling; bounded polyphony; enemy hits and explosions silent; actual thrown dagger mesh, trail, spin, speed and volley count; off-hand throw gesture; expanding transient visuals and cleanup; audio pause/reset; unchanged Rage/Haste/Rend/Vengeance logic. Isolated smoke save.");Debug.Log("VEILBOUND_PLAYER_ABILITIES_PASS");Application.Quit(0);
  }
 }
}
