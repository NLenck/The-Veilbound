using UnityEngine;
using UnityEngine.InputSystem;
namespace Veilbound {
 public partial class VeilGame {
  bool resourceFocus;int resourceFocusCategory,resourceFocusRow;
  void LeaveResourceBar(){resourceFocus=false;resourceCategory=-1;resourceFocusRow=0;selection=0;}
  void FocusResourceBar(){if(state!=ScreenState.Settlement)return;resourceFocus=!resourceFocus;if(!resourceFocus){LeaveResourceBar();return;}resourceCategory=-1;resourceFocusCategory=Mathf.Clamp(resourceFocusCategory,0,5);resourceFocusRow=0;selection=resourceFocusCategory;}
  void CycleResourceGroup(int direction){resourceFocusCategory=(resourceFocusCategory+direction+6)%6;resourceFocusRow=0;if(resourceCategory>=0)resourceCategory=resourceFocusCategory;}
  bool ReadResourceController() {
   if(state!=ScreenState.Settlement){resourceFocus=false;return false;}
   if(ActionPressed(ControlAction.Resources)){if(PadPressed(ControlAction.Resources))gamepadCombat=true;FocusResourceBar();return true;}
   if(!resourceFocus)return false;
   var pad=PreferredPad;var key=Keyboard.current;
   if(pad?.buttonEast.wasPressedThisFrame==true||key?.escapeKey.wasPressedThisFrame==true||ActionPressed(ControlAction.Pause)){LeaveResourceBar();return true;}
   if(pad?.leftShoulder.wasPressedThisFrame==true||pad?.dpad.left.wasPressedThisFrame==true||key?.leftArrowKey.wasPressedThisFrame==true)CycleResourceGroup(-1);
   else if(pad?.rightShoulder.wasPressedThisFrame==true||pad?.dpad.right.wasPressedThisFrame==true||key?.rightArrowKey.wasPressedThisFrame==true)CycleResourceGroup(1);
   if(pad?.buttonSouth.wasPressedThisFrame==true||key?.enterKey.wasPressedThisFrame==true){resourceCategory=resourceCategory==resourceFocusCategory?-1:resourceFocusCategory;resourceFocusRow=0;}
   if(resourceCategory>=0){int count=ResourceRows(resourceCategory).Count;if(pad?.dpad.down.wasPressedThisFrame==true||key?.downArrowKey.wasPressedThisFrame==true)resourceFocusRow=(resourceFocusRow+1)%count;else if(pad?.dpad.up.wasPressedThisFrame==true||key?.upArrowKey.wasPressedThisFrame==true)resourceFocusRow=(resourceFocusRow+count-1)%count;}
   return true;
  }
 }
}
