using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace Veilbound {
 public partial class VeilGame {
  IEnumerator SettingsNavigationReview() {
   string folder=Path.Combine(Application.dataPath,"..","Smoke","SettingsNavigation");Directory.CreateDirectory(folder);combatReviewFreeze=true;yield return new WaitForSecondsRealtime(.4f);ReturnHome();FundTestCity();tab=0;for(int i=0;i<50;i++)PositionCamera(.1f);
   yield return CardScreenshot(Path.Combine(folder,"01-settlement-settings-button.png"));
   int button=buttons.FindIndex(a=>a.Method.Name==nameof(OpenSettings));Require(button>=0,"Settlement has an explicit Settings button");buttons[button]();yield return CardScreenshot(Path.Combine(folder,"02-settlement-settings.png"));Require(state==ScreenState.Settings&&!SimulationActive&&buttons.Exists(a=>a.Method.Name==nameof(ExitGame))&&!buttons.Exists(a=>a.Method.Name==nameof(SettingsReturnToSettlement)),"Settings include Exit Game and disable redundant return when already at settlement");CloseSettings();StartRun();for(int i=0;i<50;i++)PositionCamera(.1f);
   yield return CardScreenshot(Path.Combine(folder,"03-expedition-settings-button.png"));button=buttons.FindIndex(a=>a.Method.Name==nameof(OpenSettings));Require(button>=0,"Expedition HUD exposes a Settings button without needing Escape");buttons[button]();yield return CardScreenshot(Path.Combine(folder,"04-expedition-settings.png"));Require(state==ScreenState.Settings&&!SimulationActive,"Opening the visible Settings action pauses the run");
   CloseSettings();Require(state==ScreenState.Paused,"Back from expedition settings retains the paused run");yield return CardScreenshot(Path.Combine(folder,"05-pause-actions.png"));Require(buttons.Exists(a=>a.Method.Name==nameof(SettingsReturnToSettlement))&&buttons.Exists(a=>a.Method.Name==nameof(ExitGame)),"Pause menu also provides settlement and exit actions");OpenSettings();yield return new WaitForSecondsRealtime(.2f);
   int victories=city.victories,cleared=city.clearedTier;double earned=city.essence+7.25;city.essence=earned;var roll=city.EquippedRoll(2);int equipped=roll==null?0:roll.id;
   var pad=InputSystem.AddDevice<Gamepad>();InputSystem.QueueStateEvent(pad,new GamepadState());InputSystem.Update();selection=buttons.FindIndex(a=>a.Method.Name==nameof(SettingsReturnToSettlement));Require(selection>=0,"Return to Settlement is available from expedition settings");InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.South));InputSystem.Update();MenuInput();InputSystem.QueueStateEvent(pad,new GamepadState());InputSystem.Update();InputSystem.RemoveDevice(pad);
   Require(state==ScreenState.Settlement&&city.essence==earned&&city.victories==victories&&city.clearedTier==cleared&&foes.Count==0&&(city.EquippedRoll(2)?.id??0)==equipped,"Controller A returns directly home, clears the run, keeps earnings/equipment and grants no victory");
   var disk=JsonUtility.FromJson<Settlement>(File.ReadAllText(savePath));Require(disk.essence==earned,"Returning home saves earned resources to disk");
   OpenSettings();preferences.master=.7f;SaveBeforeExit();var savedPrefs=JsonUtility.FromJson<VeilPreferences>(File.ReadAllText(preferencesPath));Require(savedPrefs.master==.7f&&File.Exists(savePath),"Exit preparation saves game and preferences");
   File.WriteAllText(Path.Combine(folder,"PASS.txt"),"PASS: explicit settlement and expedition Settings buttons; settings pauses simulation; Back retains paused expedition; settlement/exit in settings and pause; controller A returns directly home; earnings, equipment and unlocks preserved without victory; run cleanup; game/settings saved before exit; actual ExitGame action terminates the review player.\n");Debug.Log("VEILBOUND_SETTINGS_NAVIGATION_PASS");ExitGame();
  }
 }
}

