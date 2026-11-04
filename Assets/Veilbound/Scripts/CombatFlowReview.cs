using System;using System.Linq;using System.IO;using System.Collections;using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  int PassiveRankTotal()=>RunBuild.Pool.Where(c=>!c.ability).Sum(c=>build.Rank(c.id));
  void CheckIndicatorColor(Transform root,bool enemy,string label){
   var renderers=root.GetComponentsInChildren<Renderer>();Require(renderers.Length>0,label+" has visible indicators");
   foreach(var renderer in renderers){Color c=renderer.sharedMaterial.color;Require(enemy?c.r>.5f&&c.g<c.r*.2f&&c.b<c.r*.2f:c.r>.5f&&c.g/c.r>.7f&&c.b/c.r<.2f,label+" uses the ownership colour");}
  }
  IEnumerator CombatFlowReview(){
   yield return null;combatReviewFreeze=true;ReturnHome();string folder=Path.Combine(Application.dataPath,"..","Smoke","CombatFlow");Directory.CreateDirectory(folder);int intervals=0;
   foreach(int map in new[]{0,1}){
    ReturnHome();SelectExpeditionMap(map);StartRun();Require(nextRelic>=15&&nextRelic<=30,"First relic is scheduled within fifteen to thirty seconds");
    float due=nextRelic;elapsed=due-.01f;UpdateRelics(0);Require(relics.Count==0,"No premature relic spawn");elapsed=due;UpdateRelics(0);Require(relics.Count==1&&nextRelic-elapsed>=15&&nextRelic-elapsed<=30,"Relic spawns on schedule and reschedules within bounds");
    Require(Vector3.Distance(position,relics[0].point)>=12&&navigation.Reachable(relics[0].point,.65f),"Relic remains an exploration objective on reachable ground");
    int passives=PassiveRankTotal(),slots=build.Slots,oldLevel=level;xp=Threshold;float banked=xp;
    position=relics[0].point;hero.root.position=position;UpdateRelics(0);
    Require(state==ScreenState.Choice&&build.PassiveRound&&build.offers.Count==3&&build.offers.All(c=>!c.ability),"Touching a relic opens three passive choices");
    Require(PassiveRankTotal()==passives&&xp==banked&&level==oldLevel&&relics.Count==0,"Pickup consumes only the relic and grants no random passive or level");
    double cityClock=city.hours;float runClock=elapsed;combatReviewFreeze=false;yield return new WaitForSecondsRealtime(.25f);Require(city.hours==cityClock&&elapsed==runClock,"Both clocks pause during the relic choice");combatReviewFreeze=true;
    if(map==0)yield return CardScreenshot(Path.Combine(folder,"relic-passive-choice.png"));
    var selected=build.offers[0];int selectedRank=build.Rank(selected.id);Choose(0);Require(state==ScreenState.Expedition&&build.Rank(selected.id)==selectedRank+1&&PassiveRankTotal()==passives+1&&build.Slots==slots&&xp==banked,"Selecting the card grants exactly the chosen passive without XP or slot changes");
    Step(0);Require(state==ScreenState.Choice&&!build.PassiveRound&&level==oldLevel+1&&build.offers.All(c=>c.ability),"Pending XP becomes an ability-only level-up after the relic choice");
    if(map==0){yield return new WaitForSecondsRealtime(.25f);yield return CardScreenshot(Path.Combine(folder,"level-ability-choice.png"));}Choose(0);
    ReturnHome();SelectExpeditionMap(map);StartRun();
    bool shortInterval=false,longInterval=false;for(int i=0;i<60;i++){elapsed=nextRelic;UpdateRelics(0);Require(relics.Count==1,"Scheduled relic continues spawning");float wait=nextRelic-elapsed;Require(wait>=14.999f&&wait<=30.001f,"Every interval stays within the requested range");shortInterval|=wait<20;longInterval|=wait>25;Destroy(relics[0].root.gameObject);relics.Clear();intervals++;}Require(shortInterval&&longInterval,"Relic intervals vary across the requested range");
    Require(SpawnRelic()&&SpawnRelic()&&!SpawnRelic(),"Two uncollected relic cap remains");Require(Vector3.Distance(relics[0].point,relics[1].point)>=10,"Existing relic spacing remains");
   }
   ReturnHome();SelectExpeditionMap(1);StartRun();position=Vector3.zero;hero.root.position=position;
   LobArea(new Vector3(5,1,8),new Vector3(5,0,1),14);CheckIndicatorColor(lobs[0].warning,true,"Lobber warning");lobs[0].area.SetProgress(.6f);
   Telegraph(new Vector3(-4,0,1),2.8f,2,-10);CheckIndicatorColor(hazards[0].t,false,"Player judgement warning");hazards[0].warning.SetProgress(.6f);
   Telegraph(new Vector3(5,0,7),2,2,10);CheckIndicatorColor(hazards[1].t,true,"Enemy circular warning");
   var volatileEnemy=SpawnFoe(0,4);volatileEnemy.t.position=new Vector3(0,0,8);StartFuse(volatileEnemy,false);CheckIndicatorColor(volatileEnemy.fuseRing,true,"Volatile blast warning");
   var charge=ChargeWarningVisual.Create(effects,new Vector3(-8,0,5),Vector3.forward,5,.6f);charge.SetProgress(.6f);CheckIndicatorColor(charge.transform,true,"Enemy charge lane");
   var brute=SpawnFoe(0,2);brute.t.position=FlowTestGround();position=brute.t.position+Vector3.forward;brute.attack=0;Require(HandleGroundSlam(brute,0),"Brute begins its actual slam");CheckIndicatorColor(brute.slamHazard.t,true,"Brute slam sector");
   PlayerEffect("warcry",new Vector3(-4,0,-4),2,.75f);CheckIndicatorColor(playerFx[playerFx.Count-1].root,false,"Player ability rings");UpdatePlayerPresentation(.25f);
   position=Vector3.zero;hero.root.position=position;cameraRig.transform.position=new Vector3(7,18,-16);cameraRig.transform.LookAt(new Vector3(0,0,3));cameraRig.orthographicSize=12;yield return null;CaptureVillageCamera(Path.Combine(folder,"red-enemy-yellow-player.png"),1600,1000);
   CheckAreaWarningTiming(folder);
   File.WriteAllText(Path.Combine(folder,"PASS.txt"),"PASS: ability-only level-ups; three selectable relic passives; no passive before selection; paused clocks; pending XP retained; unchanged ability slots; "+intervals+" randomized 15-30 second intervals across both maps; reachable placement and two-relic cap; red Lobber, volatile, boss and Brute warnings and charge lanes; yellow player judgement and ability rings; unchanged warning timing and impact damage.");Debug.Log("VEILBOUND_COMBAT_FLOW_RUNTIME_PASS intervals="+intervals);Application.Quit(0);
  }
 }
}


