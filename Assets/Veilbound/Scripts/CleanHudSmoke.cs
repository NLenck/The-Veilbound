using System;
using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace Veilbound {
 public partial class VeilGame {
  void CheckCleanHudControls() {
   StartRun();combatReviewFreeze=true;Require(UpgradeIcon("dodge")&&UpgradeIcon("rage"),"Dodge and virtue artwork load from the game asset library");Require(DodgeActionRect.size==new Vector2(64,64)&&VirtueActionRect.size==new Vector2(64,64)&&624-DodgeActionRect.xMax==40&&VirtueActionRect.x-(624+4*72+64)==40,"Utility slots match ability size with forty-pixel separating gaps");
   movement=Vector2.right;dodgeCooldown=0;Require(TryDodge()&&dodgeCooldown==2.8f&&dodgeVector.x>.99f&&!TryDodge(),"Dodge icon and bindings share the same gated movement action");
   var pad=InputSystem.AddDevice<Gamepad>();OpenCharacterSheet(0);float time=elapsed,charge=rage,resource=xp;
   InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.RightShoulder));InputSystem.Update();ReadCharacterSheetInput();Require(characterTab==1&&state==ScreenState.Character,"RB selects the next character page without dodging");
   InputSystem.QueueStateEvent(pad,new GamepadState());InputSystem.Update();InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.LeftShoulder));InputSystem.Update();ReadCharacterSheetInput();Require(characterTab==0,"LB selects the previous page");
   InputSystem.QueueStateEvent(pad,new GamepadState());InputSystem.Update();InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.LeftShoulder));InputSystem.Update();ReadCharacterSheetInput();Require(characterTab==2&&elapsed==time&&rage==charge&&xp==resource,"Page cycling wraps and does not advance the paused expedition");
   InputSystem.QueueStateEvent(pad,new GamepadState());InputSystem.Update();InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.East));InputSystem.Update();ReadCharacterSheetInput();Require(state==ScreenState.Expedition,"Xbox B returns from the character sheets");
   InputSystem.QueueStateEvent(pad,new GamepadState());InputSystem.Update();InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.DpadLeft));InputSystem.Update();ReadCharacterSheetInput();Require(state==ScreenState.Character&&characterTab==1,"Controller passive launcher shortcut opens its corresponding page");
   CloseCharacterSheet();InputSystem.RemoveDevice(pad);ReturnHome();Debug.Log("VEILBOUND_CLEAN_HUD_CONTROLS_PASS");
  }
  IEnumerator CleanHudReview(string folder) {
   Directory.CreateDirectory(folder);CheckCleanHudControls();StartRun();combatReviewFreeze=true;foreach(var id in new[]{"whirl","orbit","smite"})build.Pick(TestCard(id,UpgradeRarity.Rare));level=12;elapsed=225;health=MaxHealth*.82f;xp=Threshold*.63f;rage=100;
   var target=ReadoutTarget(new Vector3(0,0,3));ReadoutTarget(new Vector3(-3,0,2));ReadoutTarget(new Vector3(3,0,2));Hit(target,75,"cleave");Hit(target,30,"whirl");damageNumbers.Clear();hero.root.position=position;hero.root.rotation=Quaternion.identity;hero.Animate(0,0,0);cameraRig.transform.position=new Vector3(7,13,-13);cameraRig.transform.LookAt(new Vector3(0,0,2));cameraRig.orthographicSize=9;dodgeCooldown=0;gamepadCombat=false;
   yield return CardScreenshot(Path.Combine(folder,"01-clean-keyboard-hud.png"));TryDodge();ActivateRage();TickRage(2);dodgeCooldown=1.6f;damageNumbers.Clear();yield return CardScreenshot(Path.Combine(folder,"02-utility-cooldowns.png"));ResetRage();rage=100;gamepadCombat=true;dodgeCooldown=0;yield return CardScreenshot(Path.Combine(folder,"03-controller-hud.png"));
   OpenCharacterSheet(0);yield return CardScreenshot(Path.Combine(folder,"04-character.png"));CycleCharacterPage(1);build.Pick(TestCard("crit",UpgradeRarity.Rare));build.Pick(TestCard("speed",UpgradeRarity.Common));yield return CardScreenshot(Path.Combine(folder,"05-passives.png"));CycleCharacterPage(1);yield return CardScreenshot(Path.Combine(folder,"06-damage.png"));CloseCharacterSheet();gamepadCombat=false;combatReviewFreeze=false;ReturnHome();
   File.WriteAllText(Path.Combine(folder,"PASS.txt"),"PASS: Dodge and Rage assets; matching 64px utility tiles and wider gaps; shared dodge cooldown/input; real LB/RB page cycling with wrap; B return; controller launcher shortcuts; frozen expedition during sheets; keyboard/controller HUD and three simplified page labels.\n");Debug.Log("VEILBOUND_CLEAN_HUD_PASS");
  }
 }
}
