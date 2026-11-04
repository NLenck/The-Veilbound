using System;using System.IO;using System.Collections;using UnityEngine;using UnityEngine.InputSystem;using UnityEngine.InputSystem.LowLevel;
namespace Veilbound {
 public partial class VeilGame {
  int overviewChecks;
  void OverviewCheck(bool value,string message){Require(value,message);overviewChecks++;}
  IEnumerator BuildOverviewReview(){
   yield return null;combatReviewFreeze=true;city=FreshCharacterTown();city.InitializeWoodenStarter();RebuildHero();PressureStart(1,0);
   foreach(string id in new[]{"whirl","lance","bloodthirst","rend"})UnlockKit(id);build.ranks["cleave"]=7;build.BeginAscension();build.Pick(build.offers[0]);
   foreach(var passive in RunBuild.Pool)if(!passive.ability)for(int rarity=0;rarity<5;rarity++)build.Pick(TestCard(passive.id,(UpgradeRarity)rarity));build.Pick(TestCard("power",UpgradeRarity.Common));build.Pick(TestCard("power",UpgradeRarity.Common));
   OpenCharacterSheet(1);yield return null;RefreshBuildOverview();
   OverviewCheck(overviewAbilities.Count==6&&overviewPassives.Count==12,"Six equipped abilities including Fury sit above twelve unique passives");
   OverviewCheck(PassivePickCount("power")==7&&build.passivePicks["power"][0]==3,"Collapsed entry retains all rarity pick counts");
   OverviewCheck(PassiveTotal("power")=="+68%"&&PassiveTotal("wisdom")=="+215%","Grouped total uses exact rarity values");
   OverviewCheck(AbilityDamageSummary("cleave").Contains(AbilityDamage("cleave").ToString("0.#"))&&Mathf.Abs(AbilityDamage("cleave")-16.8f)<.001f,"Ability detail uses actual passive-scaled damage and ascended form");
   OverviewCheck(Mathf.Abs(AbilityCooldown("cleave")-1.4f)<.001f,"Ability cooldown includes collected Haste");
   string cleavePassives=AppliedAbilityPassives("cleave"),rendPassives=AppliedAbilityPassives("rend");
   OverviewCheck(cleavePassives.Contains("Tempered resolve")&&cleavePassives.Contains("Relentless")&&cleavePassives.Contains("Killing instinct")&&!cleavePassives.Contains("Veil insight"),"Ability footer includes directly applied damage passives only");
   OverviewCheck(rendPassives.Contains("Tempered resolve")&&!rendPassives.Contains("Killing instinct")&&!rendPassives.Contains("Relentless"),"Rend footer excludes crit and Haste that do not apply");
   OverviewCheck(AppliedAbilityPassives("bloodthirst").Contains("Second breath")&&!AppliedAbilityPassives("rage").Contains("Relentless"),"Health-based healing includes vitality; resource-based Fury does not claim cooldown Haste");
   overviewFocus=0;MoveBuildOverview(Vector2Int.down);OverviewCheck(overviewFocus==overviewAbilities.Count,"Down moves from ability to first passive");MoveBuildOverview(Vector2Int.up);OverviewCheck(overviewFocus==0,"Up returns to ability row");
   var keyboard=InputSystem.AddDevice<Keyboard>();var pad=InputSystem.AddDevice<Gamepad>();preferences.controller=-1;pad.MakeCurrent();
   yield return GridKey(keyboard,Key.RightArrow);OverviewCheck(overviewFocus==1,"Keyboard navigation moves between ability details");yield return GridPad(pad,GamepadButton.DpadDown);OverviewCheck(overviewFocus==overviewAbilities.Count+1,"D-pad navigates to matching passive column");
   yield return GridPad(pad,GamepadButton.South);OverviewCheck(expandedPassive==OverviewChoice(overviewFocus).id,"Controller A opens the focused passive breakdown");yield return GridPad(pad,GamepadButton.East);OverviewCheck(expandedPassive==null&&state==ScreenState.Character,"Controller B collapses breakdown before closing page");
   FocusOverview(overviewAbilities.Count);yield return GridKey(keyboard,Key.Enter);OverviewCheck(expandedPassive=="power","Enter opens rarity breakdown");yield return GridKey(keyboard,Key.RightArrow);OverviewCheck(expandedPassive==null,"Moving to another entry restores collapsed detail");
   float clock=elapsed;double cityClock=city.hours;combatReviewFreeze=false;yield return new WaitForSecondsRealtime(.2f);OverviewCheck(elapsed==clock&&city.hours==cityClock,"Build inspection pauses both clocks");combatReviewFreeze=true;
   InputSystem.RemoveDevice(keyboard);InputSystem.RemoveDevice(pad);overviewPointer=false;FocusOverview(0);
   string folder=Path.Combine(Application.dataPath,"..","Smoke","BuildOverview");Directory.CreateDirectory(folder);yield return CardScreenshot(Path.Combine(folder,"01-ability-details.png"));FocusOverview(overviewAbilities.Count);yield return CardScreenshot(Path.Combine(folder,"02-collapsed-passive-total.png"));ToggleOverviewEntry();yield return CardScreenshot(Path.Combine(folder,"03-passive-rarity-breakdown.png"));
   CloseCharacterSheet();StartRun();OpenCharacterSheet(1);yield return null;RefreshBuildOverview();OverviewCheck(overviewPassives.Count==0&&overviewAbilities.Count==2,"Fresh run clears grouped passives and shows starter/Fury abilities");MoveBuildOverview(Vector2Int.down);OverviewCheck(overviewFocus==0,"Empty passive section cannot trap navigation");CloseCharacterSheet();
   File.WriteAllText(Path.Combine(folder,"validation.json"),"{\"passed\":true,\"checks\":"+overviewChecks+",\"groupedPassives\":12,\"abilityEntries\":6,\"rarityGroupsPreserved\":60}");Debug.Log("VEILBOUND_BUILD_OVERVIEW_PASS checks="+overviewChecks);Application.Quit(0);
  }
 }
}
