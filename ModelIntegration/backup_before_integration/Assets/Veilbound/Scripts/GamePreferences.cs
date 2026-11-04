using System;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace Veilbound {
 public enum ControlAction { Up,Down,Left,Right,Dodge,DodgeAlt,Virtue,Character,Passives,Damage,Pause,Resources }
 [Serializable] public sealed class VeilPreferences {
  public int version=1,displayMode=2,width=1600,height=900,inputMode,controller=-1;
  public float master=1,effects=1,menu=1;public bool dodgeFacing;
  public int[] keys,pad;
  public static readonly Key[] DefaultKeys={Key.W,Key.S,Key.A,Key.D,Key.Space,Key.LeftShift,Key.Q,Key.C,Key.P,Key.V,Key.Escape,Key.F2};
  public static readonly int[] DefaultPad={-1,-1,-1,-1,(int)GamepadButton.South,(int)GamepadButton.RightShoulder,(int)GamepadButton.West,(int)GamepadButton.Select,(int)GamepadButton.DpadLeft,(int)GamepadButton.DpadRight,(int)GamepadButton.Start,(int)GamepadButton.North};
  public void Normalise(){displayMode=Mathf.Clamp(displayMode,0,2);width=Mathf.Clamp(width,960,7680);height=Mathf.Clamp(height,540,4320);inputMode=Mathf.Clamp(inputMode,0,2);controller=Math.Max(-1,controller);master=Finite(master);effects=Finite(effects);menu=Finite(menu);if(keys==null||keys.Length!=12){keys=new int[12];for(int i=0;i<12;i++)keys[i]=(int)DefaultKeys[i];}for(int i=0;i<12;i++)if(keys[i]==0||!Enum.IsDefined(typeof(Key),keys[i]))keys[i]=(int)DefaultKeys[i];if(pad==null||pad.Length!=12)pad=(int[])DefaultPad.Clone();for(int i=4;i<12;i++)if(pad[i]<0||pad[i]>13)pad[i]=DefaultPad[i];}
  static float Finite(float n)=>float.IsNaN(n)||float.IsInfinity(n)?1:Mathf.Clamp01(n);
  public bool Rebind(bool gamepad,int action,int binding){Normalise();if(action<0||action>=12||gamepad&&(action<4||binding<0||binding>13)||!gamepad&&(binding==0||!Enum.IsDefined(typeof(Key),binding)))return false;var values=gamepad?pad:keys;int old=values[action];for(int i=gamepad?4:0;i<12;i++)if(i!=action&&values[i]==binding){values[i]=old;break;}values[action]=binding;return true;}
 }
 public partial class VeilGame {
  VeilPreferences preferences=new VeilPreferences();string preferencesPath;ScreenState settingsReturn;int settingsTab,captureAction=-1,captureFrame;bool capturePad;string settingsNotice="";int pendingMode,pendingResolution;
  readonly string[] ActionNames={"Move up","Move down","Move left","Move right","Dodge","Alternate dodge","Virtue ability","Character","Passives","Damage","Pause / menu","Resource bar"};
  readonly string[] DisplayNames={"Fullscreen","Windowed fullscreen","Windowed"};
  Gamepad PreferredPad {get{if(preferences.controller<0)return Gamepad.current;return preferences.controller<Gamepad.all.Count?Gamepad.all[preferences.controller]:null;}}
  bool KeyboardGameplay=>preferences.inputMode!=2;
  bool ControllerGameplay=>preferences.inputMode!=1;
  bool KeyPressed(ControlAction action)=>KeyboardGameplay&&Keyboard.current?[(Key)preferences.keys[(int)action]].wasPressedThisFrame==true;
  bool PadPressed(ControlAction action)=>ControllerGameplay&&PreferredPad!=null&&preferences.pad[(int)action]>=0&&PreferredPad[(GamepadButton)preferences.pad[(int)action]].wasPressedThisFrame;
  bool ActionPressed(ControlAction action)=>KeyPressed(action)||PadPressed(action);
  bool Held(ControlAction action)=>KeyboardGameplay&&Keyboard.current?[(Key)preferences.keys[(int)action]].isPressed==true;
  string KeyLabel(ControlAction action)=>((Key)preferences.keys[(int)action]).ToString().Replace("LeftShift","L SHIFT").Replace("RightShift","R SHIFT").ToUpper();
  string PadLabel(ControlAction action){switch((GamepadButton)preferences.pad[(int)action]){case GamepadButton.South:return "A";case GamepadButton.East:return "B";case GamepadButton.West:return "X";case GamepadButton.North:return "Y";case GamepadButton.LeftShoulder:return "LB";case GamepadButton.RightShoulder:return "RB";case GamepadButton.Start:return "MENU";case GamepadButton.Select:return "VIEW";case GamepadButton.LeftStick:return "LS";case GamepadButton.RightStick:return "RS";case GamepadButton.DpadLeft:return "LEFT";case GamepadButton.DpadRight:return "RIGHT";case GamepadButton.DpadUp:return "UP";default:return "DOWN";}}
  FullScreenMode DisplayMode(int mode)=>mode==0?FullScreenMode.ExclusiveFullScreen:mode==1?FullScreenMode.FullScreenWindow:FullScreenMode.Windowed;
  void LoadPreferences(){preferencesPath=Path.Combine(Application.persistentDataPath,smoke?"smoke-settings.json":"settings-v1.json");try{if(!smoke&&File.Exists(preferencesPath))preferences=JsonUtility.FromJson<VeilPreferences>(File.ReadAllText(preferencesPath))??new VeilPreferences();}catch(Exception e){Debug.LogWarning("Settings could not be loaded: "+e.Message);}preferences.Normalise();dodgeFacing=preferences.dodgeFacing;ApplyAudioPreferences();if(!smoke)ApplyDisplayPreferences();}
  void SavePreferences(){if(string.IsNullOrEmpty(preferencesPath))return;try{Directory.CreateDirectory(Path.GetDirectoryName(preferencesPath));preferences.dodgeFacing=dodgeFacing;string temp=preferencesPath+".tmp";File.WriteAllText(temp,JsonUtility.ToJson(preferences,true));if(File.Exists(preferencesPath))File.Replace(temp,preferencesPath,preferencesPath+".bak");else File.Move(temp,preferencesPath);}catch(Exception e){settingsNotice="Could not save settings: "+e.Message;}}
  void ApplyAudioPreferences(){AudioListener.volume=smoke?0:preferences.master;}
  void ApplyDisplayPreferences(){int w=preferences.displayMode==2?preferences.width:Display.main.systemWidth,h=preferences.displayMode==2?preferences.height:Display.main.systemHeight;Screen.SetResolution(Math.Max(960,w),Math.Max(540,h),DisplayMode(preferences.displayMode));}
  void OpenSettings(){if(state==ScreenState.Choice||state==ScreenState.Result)return;settingsReturn=state==ScreenState.Expedition?ScreenState.Paused:state;settingsTab=0;captureAction=-1;pendingMode=preferences.displayMode;PrepareResolutions();SetState(ScreenState.Settings);settingsNotice="";}
  void CloseSettings(){captureAction=-1;SavePreferences();SetState(settingsReturn);}
  void BeginBinding(int action,bool pad){captureAction=action;capturePad=pad;captureFrame=Time.frameCount;settingsNotice="Press a "+(pad?"controller button":"keyboard key")+" for "+ActionNames[action]+". Escape cancels.";}
  void SettingsUpdate(float dt){if(captureAction>=0){CaptureBinding();return;}MenuInput();var keyboard=Keyboard.current;var pad=PreferredPad??Gamepad.current;if(keyboard?.escapeKey.wasPressedThisFrame==true||pad?.buttonEast.wasPressedThisFrame==true){CloseSettings();return;}if(pad?.leftShoulder.wasPressedThisFrame==true){settingsTab=(settingsTab+2)%3;selection=settingsTab;}else if(pad?.rightShoulder.wasPressedThisFrame==true){settingsTab=(settingsTab+1)%3;selection=settingsTab;}}
  void CaptureBinding(){if(Time.frameCount<=captureFrame)return;if(Keyboard.current?.escapeKey.wasPressedThisFrame==true){captureAction=-1;settingsNotice="Binding change cancelled.";return;}int binding=-1;if(capturePad){var p=PreferredPad??Gamepad.current;if(p!=null)for(int i=0;i<=13;i++)if(p[(GamepadButton)i].wasPressedThisFrame){binding=i;break;}}else if(Keyboard.current!=null)foreach(var key in Keyboard.current.allKeys)if(key.wasPressedThisFrame){binding=(int)key.keyCode;break;}if(binding<0)return;if(preferences.Rebind(capturePad,captureAction,binding)){settingsNotice=ActionNames[captureAction]+" updated. Conflicting bindings are swapped.";captureAction=-1;SavePreferences();}}
 }
}

