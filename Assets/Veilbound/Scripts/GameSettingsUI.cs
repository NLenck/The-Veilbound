using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Veilbound {
 public partial class VeilGame {
  readonly List<Vector2Int> windowResolutions=new List<Vector2Int>();
  void PrepareResolutions(){windowResolutions.Clear();int maxW=Math.Max(960,Display.main.systemWidth),maxH=Math.Max(540,Display.main.systemHeight);foreach(var r in new[]{new Vector2Int(960,540),new Vector2Int(1280,720),new Vector2Int(1600,900),new Vector2Int(1920,1080),new Vector2Int(2560,1440),new Vector2Int(maxW,maxH),new Vector2Int(preferences.width,preferences.height)})if(r.x<=maxW&&r.y<=maxH&&!windowResolutions.Contains(r))windowResolutions.Add(r);windowResolutions.Sort((a,b)=>a.x!=b.x?a.x.CompareTo(b.x):a.y.CompareTo(b.y));pendingResolution=Math.Max(0,windowResolutions.IndexOf(new Vector2Int(preferences.width,preferences.height)));}
  void SettingsAccessButton(){Button(213,36,93,30,"Settings",OpenSettings);}
  void SettingsScreen() {
   Box(new Rect(0,0,1600,900),new Color(.018f,.025f,.027f));HudFrame(new Rect(110,30,1380,820));GUI.Label(new Rect(145,56,1300,43),"GAME SETTINGS",CardStyle(33,parchment,bold:true));GUI.Label(new Rect(145,108,1300,24),string.IsNullOrEmpty(settingsNotice)?"Preferences save separately. Returning to settlement ends the run and keeps earned resources.":settingsNotice,CardStyle(14,muted));
   string[] tabs={"Display","Sound","Controls"};for(int i=0;i<3;i++){int page=i;Button(350+i*300,159,286,42,tabs[i],()=>{settingsTab=page;selection=page;});if(settingsTab==i)CardOutline(new Rect(350+i*300,159,286,42),hudLight,2);}
   if(settingsTab==0)DisplaySettings();else if(settingsTab==1)SoundSettings();else ControlSettings();
   Button(160,786,400,41,"Back",CloseSettings);Button(585,786,400,41,"Return to Settlement",SettingsReturnToSettlement,settingsReturn!=ScreenState.Settlement&&settingsReturn!=ScreenState.CharacterSelect);Button(1010,786,400,41,"Exit Game",ExitGame);
   if(captureAction>=0){Box(new Rect(0,0,1600,900),new Color(0,0,0,.87f));HudFrame(new Rect(350,302,900,240),true);GUI.Label(new Rect(383,333,834,41),"CHANGE "+ActionNames[captureAction].ToUpper(),CardStyle(28,parchment,TextAnchor.MiddleCenter,true));GUI.Label(new Rect(383,398,834,72),"Press a "+(capturePad?"controller button":"keyboard key")+".\nEscape cancels; duplicate bindings are swapped.",CardStyle(19,hudLight,TextAnchor.MiddleCenter));Button(650,483,300,36,"Cancel",()=>{captureAction=-1;settingsNotice="Binding change cancelled.";});}
  }
  void DisplaySettings() {
   GUI.Label(new Rect(160,242,1250,33),"DISPLAY MODE",CardStyle(18,hudLight,bold:true));for(int i=0;i<3;i++){int mode=i;Button(160+i*435,294,418,52,DisplayNames[i],()=>pendingMode=mode);if(pendingMode==i)CardOutline(new Rect(160+i*435,294,418,52),hudLight,2);}
   GUI.Label(new Rect(160,385,1250,30),"WINDOWED RESOLUTION",CardStyle(18,hudLight,bold:true));var resolution=windowResolutions[pendingResolution];Button(160,435,90,46,"−",()=>pendingResolution=(pendingResolution+windowResolutions.Count-1)%windowResolutions.Count,pendingMode==2);GUI.Label(new Rect(275,445,310,31),resolution.x+" × "+resolution.y,CardStyle(22,parchment));Button(620,435,90,46,"+",()=>pendingResolution=(pendingResolution+1)%windowResolutions.Count,pendingMode==2);
   GUI.Label(new Rect(160,512,1250,55),"Fullscreen and windowed fullscreen use the display’s native resolution.\nWindowed mode uses the chosen size.",CardStyle(15,muted));
   Button(760,435,605,46,"Menu text: "+(preferences.largeText?"Larger":"Standard"),()=>{preferences.largeText=!preferences.largeText;SavePreferences();});
   Button(760,610,605,52,"Combat damage meter: "+(preferences.damageMeterExpanded?"Expanded":"Compact"),()=>{preferences.damageMeterExpanded=!preferences.damageMeterExpanded;SavePreferences();});
   Button(160,610,550,52,"Apply display",()=>{preferences.displayMode=pendingMode;var size=windowResolutions[pendingResolution];preferences.width=size.x;preferences.height=size.y;ApplyDisplayPreferences();SavePreferences();settingsNotice="Display preferences applied.";});GUI.Label(new Rect(160,695,1250,34),"Current: "+Screen.width+" × "+Screen.height+" / "+Screen.fullScreenMode,CardStyle(14,muted));
  }
  void VolumeSetting(float y,string name,float value,Action<float> set) {
   GUI.Label(new Rect(160,y,330,31),name,CardStyle(21,parchment));Button(535,y-5,65,44,"−",()=>{set(Mathf.Max(0,value-.05f));ApplyAudioPreferences();});Rect track=new Rect(630,y+11,530,12);Box(track,new Color(.09f,.10f,.10f));Box(new Rect(track.x,track.y,track.width*value,track.height),hudGold);GUI.Label(new Rect(1175,y-1,100,31),Mathf.RoundToInt(value*100)+"%",CardStyle(22,hudLight,TextAnchor.UpperRight,true));Button(1300,y-5,65,44,"+",()=>{set(Mathf.Min(1,value+.05f));ApplyAudioPreferences();});
   int control=GUIUtility.GetControlID(FocusType.Passive);var ev=Event.current;
   if(ev.type==EventType.MouseDown&&ev.button==0&&new Rect(track.x,track.y-12,track.width,36).Contains(ev.mousePosition)){GUIUtility.hotControl=control;set(Mathf.Clamp01((ev.mousePosition.x-track.x)/track.width));ApplyAudioPreferences();ev.Use();}
   else if(ev.type==EventType.MouseDrag&&GUIUtility.hotControl==control){set(Mathf.Clamp01((ev.mousePosition.x-track.x)/track.width));ApplyAudioPreferences();ev.Use();}
   else if(ev.type==EventType.MouseUp&&GUIUtility.hotControl==control){GUIUtility.hotControl=0;ev.Use();}
  }
  void SoundSettings(){VolumeSetting(250,"Master volume",preferences.master,v=>preferences.master=v);VolumeSetting(335,"Music",preferences.music,v=>preferences.music=v);VolumeSetting(420,"Game effects",preferences.effects,v=>preferences.effects=v);VolumeSetting(505,"Menu sounds",preferences.menu,v=>preferences.menu=v);Button(160,622,510,48,"Preview sound",()=>Sound(pickSound));Button(690,622,675,48,"Restore sound defaults",()=>{preferences.master=preferences.effects=preferences.menu=1;preferences.music=.65f;audioSource.mute=false;ApplyAudioPreferences();});GUI.Label(new Rect(160,707,1250,30),"Set master volume to zero to mute everything.",CardStyle(14,muted));}
  void ControlSettings() {
   string[] modes={"Auto","Keyboard & mouse","Controller"};GUI.Label(new Rect(160,225,180,24),"Gameplay input",CardStyle(14,muted));Button(342,218,370,36,modes[preferences.inputMode],()=>{preferences.inputMode=(preferences.inputMode+1)%3;settingsNotice="Gameplay input preference updated.";});
   string controller=preferences.controller<0?(Gamepad.all.Count==0?"No controller detected (auto)":"Automatic controller"):preferences.controller<Gamepad.all.Count?(preferences.controller+1)+": "+Gamepad.all[preferences.controller].displayName:"Selected controller disconnected";Button(800,218,565,36,controller,()=>{preferences.controller++;if(preferences.controller>=Gamepad.all.Count)preferences.controller=-1;});
   GUI.Label(new Rect(160,269,550,23),"KEYBOARD / CLICK A BINDING TO CHANGE",CardStyle(13,hudLight,bold:true));GUI.Label(new Rect(800,269,565,23),"CONTROLLER / MOVEMENT & AIM USE THE STICKS",CardStyle(13,hudLight,bold:true));
   for(int i=0;i<12;i++){int action=i;float y=307+i*32;GUI.Label(new Rect(160,y+5,250,25),ActionNames[i],CardStyle(14,parchment));Button(425,y,287,28,KeyLabel((ControlAction)i),()=>BeginBinding(action,false));}
   for(int i=4;i<12;i++){int action=i;float y=307+(i-4)*38;GUI.Label(new Rect(800,y+5,250,25),ActionNames[i],CardStyle(14,parchment));Button(1058,y,307,30,PadLabel((ControlAction)i),()=>BeginBinding(action,true));}
   Button(800,624,565,32,"Dodge direction: "+(dodgeFacing?"Facing":"Movement"),()=>dodgeFacing=!dodgeFacing);Button(800,667,565,32,"Restore default bindings",()=>{preferences.keys=null;preferences.pad=null;preferences.Normalise();settingsNotice="Default controls restored.";});GUI.Label(new Rect(160,717,1210,45),"Menus keep A / Enter to confirm, B / Escape to return and LB / RB to cycle pages.\nDevice preference affects gameplay; menu navigation remains available for recovery.",CardStyle(12,muted));
  }
 }
}
