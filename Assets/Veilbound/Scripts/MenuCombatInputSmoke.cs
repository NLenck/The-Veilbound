using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace Veilbound {
 public partial class VeilGame {
  IEnumerator CheckMenuConfirmDoesNotDodge(){
   var oldButtons=buttons.ToArray();int oldMode=preferences.inputMode,oldController=preferences.controller;bool oldFreeze=combatReviewFreeze;
   var pad=InputSystem.AddDevice<Gamepad>();preferences.inputMode=0;preferences.controller=-1;
   try{
    StartRun();combatReviewFreeze=true;build.Roll(random,2);SetState(ScreenState.Choice);buttons.Clear();buttons.Add(()=>Choose(0));selection=0;movement=Vector2.right;dodgeTime=0;dodgeCooldown=0;
    InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.South));InputSystem.Update();MenuInput();
    Require(state==ScreenState.Expedition,"A confirms the selected upgrade and resumes play");
    ReadCombatActionInput();Require(dodgeTime==0&&dodgeCooldown==0,"Upgrade confirmation does not also dodge on the resume frame");
    yield return null;
    ReadCombatActionInput();Require(dodgeTime==0&&dodgeCooldown==0,"Holding the confirmation button after resuming does not dodge");
    InputSystem.QueueStateEvent(pad,new GamepadState());InputSystem.Update();yield return null;
    InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.South));InputSystem.Update();ReadCombatActionInput();
    Require(dodgeTime>0&&dodgeCooldown>0,"A fresh A press after confirmation still dodges normally");
    InputSystem.QueueStateEvent(pad,new GamepadState());InputSystem.Update();SetState(ScreenState.Paused);dodgeTime=dodgeCooldown=0;buttons.Clear();buttons.Add(()=>SetState(ScreenState.Expedition));selection=0;
    InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.South));InputSystem.Update();MenuInput();ReadCombatActionInput();
    Require(state==ScreenState.Expedition&&dodgeTime==0&&dodgeCooldown==0,"Confirming resume from pause also consumes A without dodging");
    Require(AudioListener.volume==0,"Menu-confirm regression remains silent");Debug.Log("VEILBOUND_MENU_CONFIRM_NO_DODGE_PASS");
   }finally{InputSystem.RemoveDevice(pad);preferences.inputMode=oldMode;preferences.controller=oldController;buttons.Clear();buttons.AddRange(oldButtons);combatReviewFreeze=oldFreeze;ReturnHome();}
  }
 }
}
