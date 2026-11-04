using System;using System.IO;using System.Collections;using UnityEngine;using UnityEngine.InputSystem;using UnityEngine.InputSystem.LowLevel;
namespace Veilbound {
 public partial class VeilGame {
  int gridChecks;
  void GridExpect(int row,int column,string message){Require(choiceNodes.Exists(n=>n.action==selection&&n.row==row&&n.column==column),message);gridChecks++;}
  void GridTestOpen(){build=new RunBuild();build.RollPassives(new System.Random(382));SetState(ScreenState.Choice);upgradePointerFocus=false;}
  IEnumerator GridKey(Keyboard device,Key key){InputSystem.QueueStateEvent(device,new KeyboardState(key));yield return null;InputSystem.QueueStateEvent(device,new KeyboardState());yield return null;}
  IEnumerator GridPad(Gamepad device,GamepadButton button){InputSystem.QueueStateEvent(device,new GamepadState().WithButton(button));yield return null;InputSystem.QueueStateEvent(device,new GamepadState());yield return null;}
  IEnumerator ChoiceGridReview(){
   yield return null;combatReviewFreeze=true;city=FreshCharacterTown();RebuildHero();StartRun();GridTestOpen();yield return null;
   Require(choiceNodes.Count==10,"Three cards, six utility controls and reroll register as ten navigation nodes");gridChecks++;
   FocusChoiceNode(0,0);MoveChoiceGrid(Vector2Int.right);GridExpect(0,1,"Right moves across cards");MoveChoiceGrid(Vector2Int.left);GridExpect(0,0,"Left reverses card navigation");MoveChoiceGrid(Vector2Int.left);GridExpect(0,0,"Card edge does not wrap into another row");
   for(int column=0;column<6;column++){
    FocusChoiceNode(1,column);MoveChoiceGrid(Vector2Int.up);GridExpect(0,column/2,"Either utility returns to owning card");MoveChoiceGrid(Vector2Int.down);GridExpect(1,column,"Card remembers its Banish or Lock branch");
    MoveChoiceGrid(Vector2Int.down);GridExpect(2,0,"Every utility connects down to reroll");MoveChoiceGrid(Vector2Int.up);GridExpect(1,column,"Reroll returns to the originating utility");
   }
   FocusChoiceNode(1,0);for(int i=1;i<6;i++){MoveChoiceGrid(Vector2Int.right);GridExpect(1,i,"Right traverses utility row");}for(int i=4;i>=0;i--){MoveChoiceGrid(Vector2Int.left);GridExpect(1,i,"Left traverses utility row");}
   var keyboard=InputSystem.AddDevice<Keyboard>();var pad=InputSystem.AddDevice<Gamepad>();preferences.controller=-1;pad.MakeCurrent();
   FocusChoiceNode(0,0);yield return GridKey(keyboard,Key.RightArrow);GridExpect(0,1,"Real keyboard right input uses grid");choiceBranches[1]=2;yield return GridKey(keyboard,Key.DownArrow);GridExpect(1,2,"Real keyboard down enters matching utility pair");
   yield return GridPad(pad,GamepadButton.DpadRight);GridExpect(1,3,"Real D-pad moves horizontally");yield return GridPad(pad,GamepadButton.DpadDown);GridExpect(2,0,"Real D-pad down reaches reroll");yield return GridPad(pad,GamepadButton.DpadUp);GridExpect(1,3,"Real D-pad up returns to same utility");
   FocusChoiceNode(1,0);InputSystem.QueueStateEvent(pad,new GamepadState{leftStick=Vector2.right});yield return new WaitForSecondsRealtime(.46f);Require(choiceNodes.Exists(n=>n.action==selection&&n.row==1&&n.column>=2),"Held stick repeats across the utility row");gridChecks++;
   InputSystem.QueueStateEvent(pad,new GamepadState());yield return null;int released=selection;yield return new WaitForSecondsRealtime(.20f);Require(selection==released,"Released stick stops moving focus");gridChecks++;
   FocusChoiceNode(1,1);string key=build.offers[0].Key;yield return GridKey(keyboard,Key.Enter);Require(build.locked==key&&build.locks==0,"Enter activates the focused Lock");gridChecks++;
   FocusChoiceNode(1,2);MoveChoiceGrid(Vector2Int.right);GridExpect(1,4,"Navigation skips disabled lock buttons");
   FocusChoiceNode(1,1);yield return GridPad(pad,GamepadButton.South);Require(build.locked==null,"Controller A activates Unlock");gridChecks++;
   build.banishes=0;yield return null;FocusChoiceNode(0,1);MoveChoiceGrid(Vector2Int.down);GridExpect(2,0,"Card skips exhausted Banish and Lock row");
   build.rerolls=0;yield return null;FocusChoiceNode(0,1);MoveChoiceGrid(Vector2Int.down);GridExpect(0,1,"Disabled reroll cannot receive focus");
   GridTestOpen();build.offers.RemoveAt(2);yield return null;FocusChoiceNode(0,1);MoveChoiceGrid(Vector2Int.right);GridExpect(0,1,"Two-card offer cannot focus missing third card");build.offers.RemoveAt(1);yield return null;FocusChoiceNode(1,1);MoveChoiceGrid(Vector2Int.up);GridExpect(0,0,"Single-card utility returns to its card");
   GridTestOpen();yield return null;int banishes=build.banishes;FocusChoiceNode(1,2);yield return GridKey(keyboard,Key.Enter);Require(build.banishes==banishes-1,"Focused Banish consumes exactly one charge");gridChecks++;
   int rerolls=build.rerolls;FocusChoiceNode(2,0);yield return GridPad(pad,GamepadButton.South);Require(build.rerolls==rerolls-1,"Focused Reroll consumes exactly one charge");gridChecks++;
   FocusChoiceNode(0,1);var picked=build.offers[1];int rank=build.Rank(picked.id);yield return GridKey(keyboard,Key.Enter);Require(state==ScreenState.Expedition&&build.Rank(picked.id)==rank+1,"Enter on card selects exactly that passive");gridChecks++;
   GridTestOpen();build.offers.Clear();yield return null;Require(choiceNodes.Count==1,"Empty pool has only Continue");yield return GridKey(keyboard,Key.Enter);Require(state==ScreenState.Expedition,"Continue remains reachable for exhausted pools");gridChecks++;
   InputSystem.RemoveDevice(keyboard);InputSystem.RemoveDevice(pad);GridTestOpen();yield return null;FocusChoiceNode(1,3);upgradePointerFocus=false;
   string folder=Path.Combine(Application.dataPath,"..","Smoke","ChoiceGrid");Directory.CreateDirectory(folder);yield return CardScreenshot(Path.Combine(folder,"01-utility-focus.png"));MoveChoiceGrid(Vector2Int.down);yield return CardScreenshot(Path.Combine(folder,"02-reroll-focus.png"));
   File.WriteAllText(Path.Combine(folder,"validation.json"),"{\"passed\":true,\"checks\":"+gridChecks+",\"keyboard\":true,\"dpad\":true,\"stickRepeat\":true}");Debug.Log("VEILBOUND_CHOICE_GRID_PASS checks="+gridChecks);Application.Quit(0);
  }
 }
}
