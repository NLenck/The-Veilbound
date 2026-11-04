using System;
using System.IO;
using System.Collections;
using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  sealed class SequenceMapRandom:System.Random {public int draws;readonly int[] maps={0,1,1,0};public override int Next(int max){if(max!=2)throw new Exception("Unexpected map pool size");return maps[draws++%maps.Length];}}
  void CheckRandomExpeditionMaps(){
   var savedRandom=expeditionMapRandom;bool previousTrial=shortRun;int previousTier=tier;var sequence=new SequenceMapRandom();
   try {expeditionMapRandom=sequence;testRandomExpeditionMaps=true;shortRun=true;tier=3;int[] expected={0,1,1,0};
    for(int i=0;i<expected.Length;i++){ReturnHome();tier=3;shortRun=true;StartRun();Require(activeMap==expected[i]&&sequence.draws==i+1,"Every new expedition uses one fresh map draw, including consecutive repeats");Require(shortRun&&tier==3,"Map selection preserves the selected run duration and does not alter difficulty");int draws=sequence.draws;SetState(ScreenState.Paused);SetState(ScreenState.Expedition);OpenCharacterSheet(0);CloseCharacterSheet();Require(activeMap==expected[i]&&sequence.draws==draws,"Pausing and reopening a run never rerolls its map");Require(activeMap==0?collision.walls.Count==8:collision.walls.Count>=64,"Random selection builds the matching map collision and navigation");}
   } finally {testRandomExpeditionMaps=false;expeditionMapRandom=savedRandom;shortRun=previousTrial;tier=previousTier;ReturnHome();SelectExpeditionMap(0);StartRun();}
   Require(AudioListener.volume==0,"Random-map testing stays muted");Debug.Log("VEILBOUND_RANDOM_MAP_LOGIC_PASS");
  }
  IEnumerator RandomMapReview(){
   string folder=Path.Combine(Application.dataPath,"..","Smoke","RandomMaps");Directory.CreateDirectory(folder);string pass=Path.Combine(folder,"PASS.txt");if(File.Exists(pass))File.Delete(pass);
   yield return new WaitForSecondsRealtime(.5f);ReturnHome();combatReviewFreeze=true;CheckRandomExpeditionMaps();ReturnHome();tab=0;yield return new WaitForSecondsRealtime(.2f);yield return CardScreenshot(Path.Combine(folder,"01-random-destination.png"));
   foreach(int map in new[]{0,1}){ReturnHome();SelectExpeditionMap(map);StartRun();TownCamera(Vector3.zero,11);yield return CardScreenshot(Path.Combine(folder,map==0?"02-sanctuary-destination.png":"03-town-destination.png"));}
   File.WriteAllText(pass,"PASS: a fresh equally weighted destination draw for each expedition; consecutive repeats allowed; actual sanctuary/town construction and matching collision; no reroll on pause/character sheets; tier and duration preserved; muted tests and UI captures.");Debug.Log("VEILBOUND_RANDOM_MAP_PASS");Application.Quit(0);
  }
 }
}

