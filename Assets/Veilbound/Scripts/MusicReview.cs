using System.Collections;
using UnityEngine;
namespace Veilbound {public partial class VeilGame {
 IEnumerator MusicReview(){
  yield return null;combatReviewFreeze=true;
  foreach(var v in musicVoices)Require(v&&v.clip&&v.loop&&v.clip.channels==2&&Mathf.Abs(v.clip.length-60)<.01f,"60-second stereo looping music loaded");
  preferences.music=1;SetState(ScreenState.CharacterSelect);TickMusic(2);
  Require(musicVoices[0].volume>0&&musicVoices[1].volume==0,"Menu uses medieval music");
  SetState(ScreenState.Settlement);TickMusic(2);float sample=musicVoices[0].time;
  StartRun();TickMusic(.5f);Require(musicVoices[0].volume>0&&musicVoices[1].volume>0,"Entering combat crossfades both tracks");
  TickMusic(2);Require(musicVoices[0].volume==0&&musicVoices[1].volume>0,"Expedition uses electronic music");
  float full=musicVoices[1].volume;SetState(ScreenState.Choice);TickMusic(.1f);Require(musicVoices[1].volume<full&&combatMusicMix==1,"Upgrade overlay ducks combat without replacing it");
  SetState(ScreenState.Paused);OpenSettings();TickMusic(2);Require(combatMusicMix==1,"Combat settings retain combat track");CloseSettings();
  preferences.music=0;TickMusic(.1f);Require(musicVoices[0].volume==0&&musicVoices[1].volume==0,"Music has an independent mute");
  preferences.music=.65f;ReturnHome();TickMusic(2);Require(musicVoices[0].volume>0&&musicVoices[1].volume==0,"Return home restores medieval music");
  preferences.music=.37f;SavePreferences();var saved=JsonUtility.FromJson<VeilPreferences>(System.IO.File.ReadAllText(preferencesPath));Require(Mathf.Abs(saved.music-.37f)<.001f,"Music volume persists");
  foreach(var voice in musicVoices){voice.time=59.9f;}
  yield return new WaitForSecondsRealtime(.35f);
  foreach(var voice in musicVoices)Require(voice.isPlaying&&voice.time<1,"Music wraps at the end of the minute");
  Debug.Log("VEILBOUND_MUSIC_REVIEW_PASS");Application.Quit(0);
 }
}}
