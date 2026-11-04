using System;using System.IO;using System.Collections;using System.Collections.Generic;using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  [Serializable]class RelicCoverage {public int map,samples,outer,inner;public int[] quadrants=new int[4];public float maxRadius,playerRadius;public List<Vector3> points=new List<Vector3>();}
  IEnumerator ArenaExpansionReview(){
   yield return null;combatReviewFreeze=true;ReturnHome();string folder=Path.Combine(Application.dataPath,"..","Smoke","ArenaExpansion");Directory.CreateDirectory(folder);
   foreach(int map in new[]{0,1}){
    ReturnHome();SelectExpeditionMap(map);StartRun();position=Vector3.zero;hero.root.position=position;navigation.Tick(position,1,true);
    Require(collision.PlayerRadius==(map==0?48:60)&&collision.Radius==(map==0?58:71),"expanded bounds active");
    var fog=cameraRig.GetComponent<VeilFogEffect>();Require(fog.playerRadius==collision.PlayerRadius&&fog.spawnRadius==collision.Radius,"matching Veil bounds on both maps");
    var rooms=map==0?GraveyardArena.Rooms:RuinedTownArena.Rooms;Require(rooms.Count==(map==0?2:10),"keeper cottage or ten village buildings");
    foreach(var room in rooms)Require(navigation.Reachable(room.transform.position,1.12f),"all interiors reachable by largest enemy");
    float firstDue=nextRelic;elapsed=firstDue-.01f;UpdateRelics(0);Require(relics.Count==0,"no relic before scheduled time");elapsed=firstDue;UpdateRelics(0);Require(relics.Count==1&&nextRelic-elapsed>=15&&nextRelic-elapsed<=30,"randomized fifteen-to-thirty-second cadence");
    Destroy(relics[0].root.gameObject);relics.Clear();relicPlacement.Reset();elapsed=0;yield return null;
    var coverage=new RelicCoverage{map=map,playerRadius=collision.PlayerRadius};
    for(int i=0;i<96;i++){
     Require(SpawnRelic()&&relics.Count==1,"actual relic creation succeeds");var relic=relics[0];Vector3 p=relic.point;
     Require(p.magnitude<=collision.PlayerRadius-3+.001f&&Vector3.Distance(p,position)>=12&&collision.Free(p,.65f)&&navigation.Reachable(p,.65f),"reachable relic with pickup clearance inside safe player bounds");
     coverage.points.Add(p);coverage.samples++;coverage.maxRadius=Mathf.Max(coverage.maxRadius,p.magnitude);coverage.quadrants[(p.x<0?1:0)+(p.z<0?2:0)]++;if(p.magnitude>(collision.PlayerRadius-3)*.7f)coverage.outer++;if(p.magnitude<(collision.PlayerRadius-3)*.4f)coverage.inner++;
     Destroy(relic.root.gameObject);relics.Clear();if(i%8==7)yield return null;
    }
    foreach(int count in coverage.quadrants)Require(count>=12,"relics cover every quadrant");Require(coverage.outer>=20&&coverage.inner>0&&coverage.maxRadius>collision.PlayerRadius-7,"inner and outer arena coverage");
    Vector3 south=coverage.points[0];foreach(var point in coverage.points)if(point.z<south.z)south=point;
    TownCamera(south,12);TickTownInteriors(1);yield return null;CaptureVillageCamera(Path.Combine(folder,"outer-edge-"+map+".png"),1600,1000);
    // A player near the new boundary must still receive an objective away from their feet.
    position=collision.Resolve(Vector3.right*(collision.PlayerRadius-5),.42f,collision.PlayerRadius);navigation.Tick(position,1,true);
    Require(SpawnRelic()&&SpawnRelic()&&!SpawnRelic(),"two uncollected relic cap remains");Require(Vector3.Distance(relics[0].point,relics[1].point)>=10,"relics do not bunch together");
    int before=0;foreach(var c in RunBuild.Pool)if(!c.ability)before+=build.Rank(c.id);int slots=build.Slots;float oldXp=xp;
    position=relics[0].point;hero.root.position=position;UpdateRelics(0);Require(relics.Count==1&&state==ScreenState.Choice&&build.PassiveRound&&build.offers.TrueForAll(c=>!c.ability),"touch opens passive choices");Choose(0);int after=0;foreach(var c in RunBuild.Pool)if(!c.ability)after+=build.Rank(c.id);Require(after==before+1&&build.Slots==slots&&xp==oldXp,"one passive reward without ability slot or XP change");
    File.WriteAllText(Path.Combine(folder,"relic-coverage-"+map+".json"),JsonUtility.ToJson(coverage,true));
    cameraRig.transform.position=new Vector3(0,map==0?85:100,-22);cameraRig.transform.LookAt(Vector3.zero);cameraRig.orthographicSize=map==0?64:80;yield return null;CaptureVillageCamera(Path.Combine(folder,map==0?"shrine-overview.png":"village-overview.png"),1800,1600);
    var added=rooms[rooms.Count-1];TownCamera(added.transform.position,12);TickTownInteriors(1);yield return null;CaptureVillageCamera(Path.Combine(folder,map==0?"keeper-house.png":"destroyed-house.png"),1600,1000);
    ReturnHome();StartRun();Require(relics.Count==0&&nextRelic>=15&&nextRelic<=30,"new runs reset relics and timing");
   }
   Debug.Log("VEILBOUND_ARENA_EXPANSION_RUNTIME_PASS");File.WriteAllText(Path.Combine(folder,"PASS.txt"),"Expanded maps, reachable interiors, matching fog; 96 actual relic samples per map with quadrant, inner/outer and far-boundary coverage; moved player; spacing, cap, passive choice and reset.");Application.Quit(0);
  }
 }
}
