using System;
using System.IO;
using System.Collections;
using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  bool ExpeditionArtPreview=>smoke&&Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-expeditionArtReview");
  IEnumerator ExpeditionArtReview(){
   yield return null;combatReviewFreeze=true;city=Settlement.NewCamp();city.clearedTier=14;city.EnsureGear();city.EnsureIndustry();city.EnsureExpeditionModifiers();city.expeditionBestDepth=5;SelectHighestExpeditionTier();ReturnHome();tab=0;
   int[] sample={2,2,2,1,1,2,1,1,1,2};city.expeditionRanks=sample;modifierFocus=9;
   string folder=Path.Combine(Application.dataPath,"..","Smoke","ExpeditionArt");Directory.CreateDirectory(folder);
   yield return new WaitForSecondsRealtime(30);
   yield return CardScreenshot(Path.Combine(folder,"expedition-design.png"));
   Debug.Log("VEILBOUND_EXPEDITION_ART_PREVIEW_PASS");
   yield return new WaitForSecondsRealtime(60);Application.Quit(0);
  }
 }
}
