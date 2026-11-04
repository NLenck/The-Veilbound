using System;
using System.Collections.Generic;
using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  static readonly string[] PlayerSoundIds={"cleave","whirl","lance","orbit","smite","smite-impact","brand","bloodthirst","vengeance","warcry","rend","rage","dodge"};
  readonly Dictionary<string,AudioClip> playerClips=new Dictionary<string,AudioClip>();
  readonly Dictionary<string,float> nextAbilitySound=new Dictionary<string,float>();
  readonly AudioSource[] playerVoices=new AudioSource[8];
  readonly System.Random soundVariation=new System.Random(1291);
  int playerVoiceIndex,abilitySoundEvents;string lastAbilitySound;bool abilityAudioPaused;
  static Mesh thrownDaggerMesh,playerSlashMesh;
  sealed class PlayerFx {public string id;public Transform root,blade;public LineRenderer[] lines;public float age,duration,radius;public Quaternion rotation;}
  readonly List<PlayerFx> playerFx=new List<PlayerFx>();
  void InitAbilityPresentation() {
   foreach(string id in PlayerSoundIds)playerClips[id]=Resources.Load<AudioClip>("AbilityAudio/"+id);
   for(int i=0;i<playerVoices.Length;i++){var voice=WorldArt.Group("Player ability voice "+i,transform).gameObject.AddComponent<AudioSource>();voice.playOnAwake=false;voice.loop=false;voice.spatialBlend=0;voice.volume=.36f;voice.priority=100;playerVoices[i]=voice;}
  }
  bool PlayAbilitySound(string id) {
   if(state!=ScreenState.Expedition||preferences.effects<=0||preferences.master<=0||!playerClips.TryGetValue(id,out var clip)||!clip)return false;
   float now=Time.unscaledTime,minGap=id=="orbit"?.65f:id=="rend"?.30f:id=="whirl"?.38f:.14f;
   if(nextAbilitySound.TryGetValue(id,out float next)&&now<next)return false;
   nextAbilitySound[id]=now+minGap;
   AudioSource voice=null;for(int i=0;i<playerVoices.Length;i++){int slot=(playerVoiceIndex+i)%playerVoices.Length;if(!playerVoices[slot].isPlaying){voice=playerVoices[slot];playerVoiceIndex=(slot+1)%playerVoices.Length;break;}}
   if(!voice)return false;voice.clip=clip;voice.volume=.30f*preferences.effects;voice.pitch=.97f+(float)soundVariation.NextDouble()*.06f;voice.Play();BalanceAbilityVoices();abilitySoundEvents++;lastAbilitySound=id;return true;
  }
  void BalanceAbilityVoices(){int playing=0;foreach(var voice in playerVoices)if(voice&&voice.isPlaying)playing++;float volume=.30f*preferences.effects/Mathf.Sqrt(Mathf.Max(1,playing));foreach(var voice in playerVoices)if(voice)voice.volume=volume;}
  void TickAbilityAudio() {
   bool pause=state!=ScreenState.Expedition;
   foreach(var voice in playerVoices){if(!voice)continue;voice.volume=.36f*preferences.effects;if(pause&&!abilityAudioPaused)voice.Pause();else if(!pause&&abilityAudioPaused)voice.UnPause();}
   abilityAudioPaused=pause;BalanceAbilityVoices();
  }
  void ResetAbilityPresentation(){foreach(var voice in playerVoices)if(voice)voice.Stop();foreach(var fx in playerFx)if(fx.root)Destroy(fx.root.gameObject);playerFx.Clear();nextAbilitySound.Clear();playerVoiceIndex=0;abilitySoundEvents=0;lastAbilitySound=null;abilityAudioPaused=false;hero?.ClearAbilityGesture();}
  static Mesh DaggerMesh() {
   if(thrownDaggerMesh)return thrownDaggerMesh;
   thrownDaggerMesh=new Mesh{name="Rift Edge — pointed steel dagger with raised centre ridge"};
   thrownDaggerMesh.vertices=new[]{new Vector3(-.13f,0,.23f),new Vector3(.13f,0,.23f),new Vector3(0,0,.98f),new Vector3(0,0,.04f),new Vector3(0,.055f,.35f),new Vector3(0,-.055f,.35f)};
   thrownDaggerMesh.triangles=new[]{3,4,0,0,4,2,2,4,1,1,4,3,3,0,5,0,2,5,2,1,5,1,3,5};thrownDaggerMesh.RecalculateNormals();thrownDaggerMesh.RecalculateBounds();return thrownDaggerMesh;
  }
  static Mesh SlashMesh() {
   if(playerSlashMesh)return playerSlashMesh;var v=new List<Vector3>();var tris=new List<int>();
   for(int i=0;i<=24;i++){float u=i/24f,a=Mathf.Lerp(-1.25f,1.25f,u),width=.19f*Mathf.Sin(Mathf.PI*u);v.Add(new Vector3(Mathf.Sin(a),0,Mathf.Cos(a)));v.Add(new Vector3(Mathf.Sin(a)*(1-width),.025f,Mathf.Cos(a)*(1-width)));if(i==0)continue;int n=i*2;tris.AddRange(new[]{n-2,n,n-1,n-1,n,n+1,n-1,n,n-2,n+1,n,n-1});}
   playerSlashMesh=new Mesh{name="Tapered player sword sweep"};playerSlashMesh.SetVertices(v);playerSlashMesh.SetTriangles(tris,0);playerSlashMesh.RecalculateNormals();return playerSlashMesh;
  }
  Transform DaggerVisual(Transform parent,bool trail=true) {
   var model=WorldArt.Group("Dagger spin",parent);var blade=WorldArt.Group("Pointed steel blade",model);blade.gameObject.AddComponent<MeshFilter>().sharedMesh=DaggerMesh();blade.gameObject.AddComponent<MeshRenderer>().sharedMaterial=WorldArt.Mat(new Color(.66f,.72f,.76f));
   WorldArt.Part(model,"Bronze crossguard",PrimitiveType.Cube,new Vector3(0,0,.015f),new Vector3(.38f,.075f,.075f),new Color(.44f,.26f,.12f));
   WorldArt.Part(model,"Leather dagger grip",PrimitiveType.Cylinder,new Vector3(0,0,-.15f),new Vector3(.065f,.13f,.065f),new Color(.105f,.065f,.045f)).localRotation=Quaternion.Euler(90,0,0);
   WorldArt.Part(model,"Steel pommel",PrimitiveType.Sphere,new Vector3(0,0,-.30f),Vector3.one*.10f,new Color(.42f,.46f,.50f));
   var seam=FxLine(model,"Ember blade seam",WorldArt.VeilRed,.018f,2);seam.SetPosition(0,new Vector3(0,.057f,.22f));seam.SetPosition(1,new Vector3(0,.015f,.86f));
   if(trail){var tip=WorldArt.Group("Short ember flight trail",parent);tip.localPosition=new Vector3(0,0,-.3f);var t=tip.gameObject.AddComponent<TrailRenderer>();t.time=.14f;t.minVertexDistance=.045f;t.startWidth=.065f;t.endWidth=0;t.sharedMaterial=WorldArt.Glow(new Color(.7f,.16f,.055f));t.startColor=new Color(1,.42f,.16f,.65f);t.endColor=new Color(.4f,.025f,.01f,0);t.numCornerVertices=2;}
   foreach(var collider in model.GetComponentsInChildren<Collider>())collider.enabled=false;
   return model;
  }
  Transform CreateThrownDagger(Vector3 p,Vector3 direction,float area) {var root=WorldArt.Group("Rift Edge — thrown dagger",effects);root.position=p;root.rotation=Quaternion.LookRotation(direction);root.localScale=Vector3.one*area*.9f;DaggerVisual(root);return root;}
  LineRenderer FxLine(Transform root,string name,Color color,float width,int points=64) {var t=WorldArt.Group(name,root);var l=t.gameObject.AddComponent<LineRenderer>();l.useWorldSpace=false;l.positionCount=points;l.widthMultiplier=width;l.sharedMaterial=WorldArt.Glow(color);l.numCornerVertices=2;l.numCapVertices=2;return l;}
  void PlayerCast(string id,Vector3 origin,float radius) {
   if(state!=ScreenState.Expedition)return;PlayAbilitySound(id);hero?.PlayAbilityGesture(id);
   if(id=="lance")return;
   PlayerEffect(id,origin,radius,id=="smite"?.5f:id=="rage"?.8f:id=="warcry"?.75f:id=="bloodthirst"?.6f:id=="rend"?.35f:.38f);
  }
  void PlayerEffect(string id,Vector3 origin,float radius,float duration) {
   if(playerFx.Count>=72){var old=playerFx[0];if(old.root)Destroy(old.root.gameObject);playerFx.RemoveAt(0);}
   var fx=new PlayerFx{id=id,root=WorldArt.Group("Player ability animation — "+id,effects),duration=duration,radius=radius,rotation=Quaternion.LookRotation(facing)};fx.root.position=origin;
   Color color=id=="vengeance"?new Color(1,.63f,.27f):id=="warcry"?new Color(.95f,.30f,.08f):WorldArt.VeilRed;
   if(id=="cleave"||id=="vengeance") {fx.root.position+=Vector3.up*.85f;fx.root.rotation=fx.rotation;fx.blade=WorldArt.Group("Sweeping blade afterimage",fx.root);fx.blade.gameObject.AddComponent<MeshFilter>().sharedMesh=SlashMesh();fx.blade.gameObject.AddComponent<MeshRenderer>().sharedMaterial=WorldArt.Glow(color);}
   else if(id=="smite"){fx.blade=WorldArt.Group("Descending judgement sword",fx.root);DaggerVisual(fx.blade,false);fx.blade.localScale=new Vector3(1.6f,1.6f,2.5f);fx.blade.localRotation=Quaternion.Euler(90,0,0);}
   else {
    int count=id=="rend"?3:id=="bloodthirst"?5:id=="rage"?6:id=="brand"?2:3;fx.lines=new LineRenderer[count];
    for(int i=0;i<count;i++)fx.lines[i]=FxLine(fx.root,id+" ribbon "+i,color,id=="rend"?.035f:.065f,id=="rend"?4:64);
    if(id=="rend")fx.root.rotation=Quaternion.LookRotation(cameraRig.transform.position-origin);
   }
   playerFx.Add(fx);AnimatePlayerFx(fx);
  }
  void AnimatePlayerFx(PlayerFx fx) {
   float u=Mathf.Clamp01(fx.age/fx.duration),fade=1-u;
   if(fx.id=="cleave"||fx.id=="vengeance"){fx.blade.localRotation=Quaternion.Euler(0,Mathf.Lerp(-30,45,u),0);fx.blade.localScale=new Vector3(fx.radius,Mathf.Max(.001f,fade),fx.radius)*(1+.06f*u);return;}
   if(fx.id=="smite"){fx.blade.localPosition=Vector3.up*(2.55f+4*(1-u)*(1-u));return;}
   for(int i=0;i<fx.lines.Length;i++) {
    var l=fx.lines[i];l.widthMultiplier=(fx.id=="rend"?.035f:.065f)*fade*fade;
    if(fx.id=="rend"){for(int j=0;j<4;j++){float t=j/3f;l.SetPosition(j,new Vector3((i-1)*.17f+(t-.5f)*.27f+(j%2==0?-.025f:.025f),(t-.5f)*.65f,0));}continue;}
    for(int j=0;j<64;j++){
     float t=j/63f,a=t*Mathf.PI*2,r=fx.radius*Mathf.Clamp01(u*1.7f-i*.09f),height=.10f+i*.025f;
     if(fx.id=="brand"&&i==1){r=fx.radius*.4f*Mathf.Sin(Mathf.PI*u);a=Mathf.Floor(t*4)*Mathf.PI*.5f;}
     if(fx.id=="bloodthirst"){if(i<2)r=fx.radius*Mathf.Sin(Mathf.PI*u);else{a=i*Mathf.PI*2/3+t*.7f;r=fx.radius*(1-u)*(1-t*.5f);height=.15f+Mathf.Sin(t*Mathf.PI)*.55f;}}
     if(fx.id=="rage"&&i>=3){a=(i-3)*Mathf.PI*2/3+t*.5f;r=fx.radius*.5f*Mathf.Sin(Mathf.PI*u);height=Mathf.Sin(t*Mathf.PI)*2.5f*fade;}
     if(fx.id=="dodge"){a=Mathf.Lerp(2.0f,4.3f,t);r=fx.radius*(.3f+u);height=.15f+i*.12f;}
     l.SetPosition(j,new Vector3(Mathf.Cos(a)*r,height,Mathf.Sin(a)*r));
    }
   }
  }
  void UpdatePlayerPresentation(float dt) {
   hero?.AdvanceAbilityGesture(dt);
   for(int i=playerFx.Count-1;i>=0;i--){var fx=playerFx[i];fx.age+=Mathf.Max(0,dt);if(fx.age>=fx.duration||!fx.root){if(fx.root)Destroy(fx.root.gameObject);playerFx.RemoveAt(i);}else AnimatePlayerFx(fx);}
  }
 }
}
