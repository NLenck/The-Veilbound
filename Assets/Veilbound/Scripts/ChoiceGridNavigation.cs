using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Veilbound {
 public partial class VeilGame {
  struct ChoiceNode {public int action,row,column;public ChoiceNode(int action,int row,int column){this.action=action;this.row=row;this.column=column;}}
  readonly List<ChoiceNode> choiceNodes=new List<ChoiceNode>();
  readonly int[] choiceBranches={0,2,4};
  int choiceReturnColumn=2;Vector2Int choiceHeldDirection;float choiceRepeatAt;
  void ResetChoiceNavigation(){choiceNodes.Clear();choiceHeldDirection=Vector2Int.zero;choiceReturnColumn=2;for(int i=0;i<3;i++)choiceBranches[i]=i*2;}
  void RegisterChoiceNode(int action,int row,int column){choiceNodes.Add(new ChoiceNode(action,row,column));}
  bool FocusChoiceNode(int row,int column){foreach(var node in choiceNodes)if(node.row==row&&node.column==column){selection=node.action;RememberChoiceNode(node);return true;}return false;}
  void RememberChoiceNode(ChoiceNode node){if(node.row==1){choiceReturnColumn=node.column;choiceBranches[node.column/2]=node.column;}}
  void MoveChoiceGrid(Vector2Int direction){
   if(choiceNodes.Count==0)return;var current=choiceNodes.Find(n=>n.action==selection);
   if(!choiceNodes.Exists(n=>n.action==selection)){selection=choiceNodes[0].action;current=choiceNodes[0];}
   RememberChoiceNode(current);
   if(direction.x!=0){
    if(current.row==2)return;
    int sign=direction.x>0?1:-1,limit=current.row==0?3:6;
    for(int column=current.column+sign;column>=0&&column<limit;column+=sign)if(FocusChoiceNode(current.row,column))return;
   }else if(direction.y<0){
    if(current.row==0){int first=current.column*2;if(FocusChoiceNode(1,choiceBranches[current.column])||FocusChoiceNode(1,first)||FocusChoiceNode(1,first+1))return;FocusChoiceNode(2,0);}
    else if(current.row==1)FocusChoiceNode(2,0);
   }else if(direction.y>0){
    if(current.row==1){FocusChoiceNode(0,current.column/2);return;}
    if(current.row==2){if(FocusChoiceNode(1,choiceReturnColumn))return;for(int offset=1;offset<6;offset++)if(FocusChoiceNode(1,choiceReturnColumn-offset)||FocusChoiceNode(1,choiceReturnColumn+offset))return;FocusChoiceNode(0,Mathf.Clamp(choiceReturnColumn/2,0,2));}
   }
  }
  void ReadChoiceGridInput(){
   var keyboard=Keyboard.current;var pad=PreferredPad;
   Vector2 input=Vector2.zero;
   if(keyboard!=null){input.x=(keyboard.rightArrowKey.isPressed?1:0)-(keyboard.leftArrowKey.isPressed?1:0);input.y=(keyboard.upArrowKey.isPressed?1:0)-(keyboard.downArrowKey.isPressed?1:0);}
   if(pad!=null){Vector2 dpad=pad.dpad.ReadValue(),stick=pad.leftStick.ReadValue();if(dpad.sqrMagnitude>.1f)input=dpad;else if(stick.sqrMagnitude>.36f)input=stick;}
   Vector2Int direction=input.sqrMagnitude<.25f?Vector2Int.zero:Mathf.Abs(input.x)>Mathf.Abs(input.y)?new Vector2Int(input.x>0?1:-1,0):new Vector2Int(0,input.y>0?1:-1);
   if(direction!=Vector2Int.zero&&(direction!=choiceHeldDirection||Time.unscaledTime>=choiceRepeatAt)){
    bool first=direction!=choiceHeldDirection;upgradePointerFocus=false;MoveChoiceGrid(direction);choiceRepeatAt=Time.unscaledTime+(first?.30f:.12f);
   }
   choiceHeldDirection=direction;
   if(keyboard?.tabKey.wasPressedThisFrame==true&&choiceNodes.Count>0){int index=choiceNodes.FindIndex(n=>n.action==selection),sign=keyboard.shiftKey.isPressed?-1:1;var node=choiceNodes[(index+sign+choiceNodes.Count)%choiceNodes.Count];selection=node.action;RememberChoiceNode(node);upgradePointerFocus=false;}
   if(choiceNodes.Count>0&&(keyboard?.enterKey.wasPressedThisFrame==true||pad?.buttonSouth.wasPressedThisFrame==true)){
    if(choiceNodes.Exists(n=>n.action==selection)&&selection>=0&&selection<buttons.Count)buttons[selection]();return;
   }
   if(keyboard?.digit1Key.wasPressedThisFrame==true)Choose(0);else if(keyboard?.digit2Key.wasPressedThisFrame==true)Choose(1);else if(keyboard?.digit3Key.wasPressedThisFrame==true)Choose(2);
  }
 }
}
