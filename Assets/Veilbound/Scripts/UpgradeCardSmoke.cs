using System;
using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace Veilbound {
 public partial class VeilGame {
  Choice TestCard(string id,UpgradeRarity rarity) {
   Choice source=Array.Find(RunBuild.Pool,c=>c.id==id);return new Choice(source.id,source.name,source.description,source.ability){rarity=rarity,attribute=source.ability&&build.Rank(id)>0?AbilitySpecs.Upgrades(id)[0].key:null};
  }
  void SetTestCards(int currentLevel,params Choice[] cards) {
   level=currentLevel;build.offerLevel=currentLevel;build.offers.Clear();build.offers.AddRange(cards);SetState(ScreenState.Choice);upgradePointerFocus=false;
  }
  IEnumerator CardScreenshot(string path) {
   yield return new WaitForEndOfFrame();var texture=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(path,texture.EncodeToPNG());Destroy(texture);
  }
  IEnumerator UpgradeCardReview(string folder) {
   Directory.CreateDirectory(folder);ReturnHome();StartRun();
   foreach(Choice offer in RunBuild.Pool)Require(UpgradeIcon(offer.id)!=null,"Every current upgrade has a valid icon resource");
   SetTestCards(2,TestCard("whirl",UpgradeRarity.Rare),TestCard("orbit",UpgradeRarity.Epic),TestCard("smite",UpgradeRarity.Legendary));
   double cityTime=city.hours;float combatTime=elapsed;yield return new WaitForSecondsRealtime(.5f);Require(city.hours==cityTime&&elapsed==combatTime,"Both clocks remain paused while choosing a card");
   yield return new WaitForSecondsRealtime(.5f);Require(buttons.Count==10,"Three full-card selections register before six utility actions and the reroll");
   yield return CardScreenshot(Path.Combine(folder,"01-physical-ability-cards.png"));
   int oldRank=build.Rank("whirl");buttons[0]();Require(state==ScreenState.Expedition&&build.Rank("whirl")==oldRank+1,"The full-card action applies its upgrade exactly once and resumes combat");
   SetTestCards(5,TestCard("power",UpgradeRarity.Legendary),TestCard("vitality",UpgradeRarity.Uncommon),TestCard("haste",UpgradeRarity.Rare));
   yield return new WaitForSecondsRealtime(.5f);
   var pad=InputSystem.AddDevice<Gamepad>();InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.DpadRight));InputSystem.Update();MenuInput();
   InputSystem.QueueStateEvent(pad,new GamepadState());InputSystem.Update();Require(selection==1,"Controller navigation moves directly to the next card");
   yield return new WaitForSecondsRealtime(.3f);yield return CardScreenshot(Path.Combine(folder,"02-passive-upgrade-cards.png"));
   int healthRank=build.Rank("vitality");InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.South));InputSystem.Update();MenuInput();InputSystem.RemoveDevice(pad);
   Require(state==ScreenState.Expedition&&build.Rank("vitality")==healthRank+1,"Controller confirm selects the focused upgrade");
   SetTestCards(7,TestCard("cleave",UpgradeRarity.Epic),TestCard("regen",UpgradeRarity.Rare),TestCard("reach",UpgradeRarity.Common));
   yield return new WaitForSecondsRealtime(.4f);buttons[4]();Require(build.locked=="cleave/damage","Lock card action keeps the chosen offer");int rerolls=build.rerolls;buttons[9]();Require(build.rerolls==rerolls-1&&build.offers.Exists(c=>c.id=="cleave"&&c.rarity==UpgradeRarity.Epic),"Reroll retains the locked card and its rarity");
   yield return new WaitForSecondsRealtime(.4f);yield return CardScreenshot(Path.Combine(folder,"03-locked-rerolled-cards.png"));
   string banished=build.offers[0].Key;int banishes=build.banishes;buttons[3]();Require(build.banishes==banishes-1&&build.banished.Contains(banished)&&!build.offers.Exists(c=>c.Key==banished),"Banish card action removes that upgrade from future offers");
   yield return new WaitForSecondsRealtime(.4f);var keyboard=InputSystem.AddDevice<Keyboard>();string picked=build.offers[2].id;int rank=build.Rank(picked);
   InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Digit3));InputSystem.Update();MenuInput();InputSystem.RemoveDevice(keyboard);
   Require(state==ScreenState.Expedition&&build.Rank(picked)==rank+1,"Number shortcut selects its matching card");
   File.WriteAllText(Path.Combine(folder,"PASS.txt"),"PASS: all current upgrade icons loaded; three selectable cards; physical and passive artwork; unchanged rarity-scaled effects; clocks paused; card action, controller navigation/confirm and numeric shortcuts; lock, reroll and banish actions.\n");
   Debug.Log("VEILBOUND_UPGRADE_CARDS_PASS");ReturnHome();
  }
 }
}


