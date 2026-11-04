using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  readonly AudioSource[] musicVoices=new AudioSource[2];
  float combatMusicMix;
  const float MusicGain=.42f;
  void InitMusic(){
   string[] clips={"settlement_medieval_loop_60s","combat_electronic_loop_60s"};
   for(int i=0;i<2;i++){
    var source=new GameObject(i==0?"Settlement music":"Combat music");source.transform.SetParent(transform,false);
    var voice=source.AddComponent<AudioSource>();musicVoices[i]=voice;
    voice.playOnAwake=false;voice.loop=true;voice.spatialBlend=0;voice.priority=64;voice.volume=0;
    voice.clip=Resources.Load<AudioClip>("Music/"+clips[i]);
    if(voice.clip)voice.Play();else Debug.LogWarning("Missing music: "+clips[i]);
   }
  }
  ScreenState MusicContext(){
   ScreenState context=state;
   if(context==ScreenState.Settings)context=settingsReturn;
   if(context==ScreenState.Character)context=characterReturn;
   return context;
  }
  void TickMusic(float dt){
   var context=MusicContext();bool combat=context==ScreenState.Expedition||context==ScreenState.Choice||context==ScreenState.Paused;
   combatMusicMix=Mathf.MoveTowards(combatMusicMix,combat?1:0,dt/1.25f);
   float gain=MusicGain*preferences.music*(combat&&state!=ScreenState.Expedition?.55f:1);
   if(musicVoices[0])musicVoices[0].volume=gain*(1-combatMusicMix);
   if(musicVoices[1])musicVoices[1].volume=gain*combatMusicMix;
  }
 }
}
