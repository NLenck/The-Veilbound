using System;
using System.Collections.Generic;
using UnityEngine;
namespace Veilbound {
 public sealed partial class RunBuild {
  public void GrantRelic(Choice card){var held=lockedOffer;string key=locked;Pick(card);locked=key;lockedOffer=held;}
 }
 public partial class VeilGame {
  readonly float[] bossArrival={-1,-1,-1};
  readonly System.Random relicRandom=new System.Random();
  readonly RelicPlacement relicPlacement=new RelicPlacement();
  class Relic {public Transform root,crystal;public Vector3 point;}
  readonly List<Relic> relics=new List<Relic>();
  float nextRelic=30,relicMessageUntil;string relicMessage="";
  int SpawnStage=>ExpeditionSpawns.Stage(elapsed,bossArrival[0],bossArrival[1]);
  void SelectEncounter(out int kind,out int variant) {
   int brutes=0,bulwarks=0;foreach(var f in foes)if(!f.dead&&f.boss==0&&f.kind==2){if(f.variant==1)bulwarks++;else brutes++;}
   float spawnRoll=ModifierRank(6)>0?(float)random.NextDouble()*100:random.Next(100);
   ExpeditionSpawns.Pick(activeMap,SpawnStage,spawnRoll,random.Next(100),brutes,bulwarks,out kind,out variant,ModifierFactor(6));
  }
  float NextRelicDelay()=>15+(float)relicRandom.NextDouble()*15;
  void ResetExpeditionFlow(){relics.Clear();relicPlacement.Reset();nextRelic=NextRelicDelay();relicMessage="";relicMessageUntil=0;for(int i=0;i<3;i++)bossArrival[i]=-1;}
  float MeleeWindup(Foe f)=>f.kind==3?.18f:.28f;
  float MeleeRecovery(Foe f)=>f.kind==3?.34f:.50f;
  float MeleeReach(Foe f)=>f.radius+1.15f;
  bool MeleeTargetInside(Foe f) {
   Vector3 delta=position-f.t.position;delta.y=0;
   return delta.magnitude<=MeleeReach(f)&&(delta.sqrMagnitude<.0001f||Vector3.Dot(delta.normalized,f.strikeDirection)>=.5f)&&collision.ClearLine(f.t.position,position,.1f);
  }
  bool HandleMelee(Foe f,float dt) {
   if(f.dead||f.boss>0||(f.kind!=0&&f.kind!=3))return false;
   if(!f.meleeActive){Vector3 delta=position-f.t.position;delta.y=0;if(f.attack>0||delta.magnitude>MeleeReach(f)||!collision.ClearLine(f.t.position,position,.1f))return false;f.meleeActive=true;f.meleeAge=0;f.meleeLanded=false;f.strikeDirection=delta.sqrMagnitude>.0001f?delta.normalized:f.t.forward;f.t.rotation=Quaternion.LookRotation(f.strikeDirection);}
   else f.meleeAge+=dt*ModifierFactor(8);
   float windup=MeleeWindup(f),strike=.12f;
   if(!f.meleeLanded&&f.meleeAge>=windup){f.meleeLanded=true;if(MeleeTargetInside(f))Hurt(f.contact,f);}
   f.rig.Animate(animClock,0,dt);f.rig.visual.MeleePose(f.meleeAge,windup,strike,MeleeRecovery(f));
   if(f.meleeAge>=windup+strike+MeleeRecovery(f)){f.meleeActive=false;f.attack=.05f;f.side=-f.side;}
   return true;
  }
  bool HandleLobWindup(Foe f,float dt) {
   if(f.lobWindup<=0||f.dead)return false;
   f.lobWindup=Mathf.Max(0,f.lobWindup-dt*ModifierFactor(8));f.rig.Animate(animClock,0,dt);f.rig.visual.GroundSlamPose((.6f-f.lobWindup)/.6f);
   if(f.lobWindup==0){LobArea(f.rig.visual.CastingOrigin(f.side),f.lobTarget,14,f);f.side=-f.side;f.attack=3.6f;}return true;
  }
  bool SpawnRelic() {
   if(relics.Count>=2)return false;
   var occupied=new List<Vector3>();foreach(var relic in relics)occupied.Add(relic.point);
   if(!relicPlacement.TryFind(relicRandom,collision,navigation,position,occupied,out Vector3 p))return false;
    var root=WorldArt.Group("Passive relic — touch to collect",effects);root.position=p;
    var crystal=WorldArt.Crystal(root,Vector3.up*.85f,.55f,WorldArt.Gold);
    WorldArt.Ring(root,Vector3.up*.06f,1.05f,WorldArt.Gold,.07f,48);
    var beam=RoundedArt.Shape(root,"Relic beacon",Vector3.up*3.5f,new Vector3(.10f,7,.10f),WorldArt.Gold);beam.GetComponent<Renderer>().sharedMaterial=WorldArt.Glow(new Color(1,.72f,.2f));
    var light=root.gameObject.AddComponent<Light>();light.color=new Color(1,.7f,.25f);light.range=4;light.intensity=1.3f;light.shadows=LightShadows.None;
    relics.Add(new Relic{root=root,crystal=crystal,point=p});return true;
  }
  bool CollectRelic(int index) {
   if(state!=ScreenState.Expedition||health<=0||index<0||index>=relics.Count)return false;
   build.RollPassives(relicRandom);if(build.offers.Count==0)return false;
   Destroy(relics[index].root.gameObject);relics.RemoveAt(index);SetState(ScreenState.Choice);Sound(pickSound);return true;
  }
  void UpdateRelics(float dt) {
   if(state!=ScreenState.Expedition)return;
   if(elapsed>=nextRelic){if(relics.Count>=2||SpawnRelic())nextRelic=elapsed+NextRelicDelay();else nextRelic=elapsed+5;}
   for(int i=relics.Count-1;i>=0;i--){var r=relics[i];r.crystal.Rotate(0,dt*60,0);r.crystal.localPosition=Vector3.up*(.85f+Mathf.Sin(elapsed*2)*.13f);if(Vector3.Distance(position,r.point)<1.1f&&CollectRelic(i))break;}
  }

 }
}
