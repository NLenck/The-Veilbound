using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
namespace Veilbound {
 public sealed class GreatswordMotionPlayer:IDisposable {
  readonly GreatswordMotionSet set;readonly Transform root;
  PlayableGraph graph;AnimationMixerPlayable locomotion,attacks;AnimationLayerMixerPlayable layers;
  readonly AnimationClipPlayable[] moves=new AnimationClipPlayable[9],strikes=new AnimationClipPlayable[4];
  readonly float[] weights=new float[9];AvatarMask mask;
  float previousTime=-1,speed,attackAge=100,attackDuration=1;double phase;int strikeIndex=-1,sequence;
  public float Speed=>speed;public float AttackWeight{get;private set;}public Vector3 LocalVelocity{get;private set;}
  public int AttackSerial{get;private set;}public int ActiveAttack=>strikeIndex;
  public float Weight(int index)=>weights[index];
  public static GreatswordMotionPlayer TryCreate(Animator animator,bool female){var set=GreatswordMotionSet.Load(female);return set&&set.Complete?new GreatswordMotionPlayer(animator,set):null;}
  GreatswordMotionPlayer(Animator animator,GreatswordMotionSet motions){
   set=motions;root=animator.transform;graph=PlayableGraph.Create("Greatsword directional movement and attacks");graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
   locomotion=AnimationMixerPlayable.Create(graph,9);attacks=AnimationMixerPlayable.Create(graph,4);layers=AnimationLayerMixerPlayable.Create(graph,2);
   for(int i=0;i<9;i++){moves[i]=AnimationClipPlayable.Create(graph,i==0?set.ready:set.movement[i-1]);moves[i].SetSpeed(0);graph.Connect(moves[i],0,locomotion,i);}
   for(int i=0;i<4;i++){strikes[i]=AnimationClipPlayable.Create(graph,set.attacks[i]);strikes[i].SetSpeed(0);graph.Connect(strikes[i],0,attacks,i);}
   graph.Connect(locomotion,0,layers,0);graph.Connect(attacks,0,layers,1);layers.SetInputWeight(0,1);
   mask=new AvatarMask();var all=root.GetComponentsInChildren<Transform>(true);mask.transformCount=all.Length;
   for(int i=0;i<all.Length;i++){
    string path="";for(var t=all[i];t&&t!=root;t=t.parent)path=t.name+(path.Length>0?"/"+path:"");
    mask.SetTransformPath(i,path);bool upper=false;for(var t=all[i];t&&t!=root;t=t.parent)if(t.name=="mixamorig:Spine")upper=true;
    mask.SetTransformActive(i,upper);
   }
   layers.SetLayerMaskFromAvatarMask(1,mask);var output=AnimationPlayableOutput.Create(graph,"Greatsword",animator);output.SetSourcePlayable(layers);graph.Play();
  }
  public void PlayAttack(float interval,bool heavy=false){
   strikeIndex=heavy?3:sequence++%2==0?0:2;attackAge=0;attackDuration=Mathf.Clamp(Mathf.Min(set.attacks[strikeIndex].length,interval*.8f),.12f,1.35f);AttackSerial++;
  }
  public void Evaluate(float time,Vector3 worldVelocity,bool neutral){
   bool rewind=previousTime<0||time<previousTime;float dt=rewind?0:Mathf.Clamp(time-previousTime,0,.1f);previousTime=time;
   if(rewind||neutral){phase=0;attackAge=100;}
   var local=root.InverseTransformDirection(Vector3.ProjectOnPlane(worldVelocity,Vector3.up));if(neutral)local=Vector3.zero;
   float targetSpeed=local.magnitude;speed=rewind||neutral?targetSpeed:Mathf.Lerp(speed,targetSpeed,1-Mathf.Exp(-dt/.10f));LocalVelocity=local;
   float moving=Mathf.Clamp01(speed/.35f),running=Mathf.InverseLerp(1.5f,3.6f,speed);
   float total=Mathf.Abs(local.x)+Mathf.Abs(local.z);float f=total>.001f?Mathf.Max(0,local.z)/total:1,b=total>.001f?Mathf.Max(0,-local.z)/total:0,l=total>.001f?Mathf.Max(0,-local.x)/total:0,r=total>.001f?Mathf.Max(0,local.x)/total:0;
   weights[0]=1-moving;float[] directions={f,b,l,r};float cycleDuration=0;
   for(int i=0;i<8;i++){weights[i+1]=moving*directions[i%4]*(i<4?1-running:running);cycleDuration+=weights[i+1]*set.movement[i].length;}
   if(moving>.001f)phase+=dt*Mathf.Clamp(speed/Mathf.Lerp(1.2f,3.6f,running),.2f,2f)/Mathf.Max(.15f,cycleDuration/moving);
   for(int i=0;i<9;i++){locomotion.SetInputWeight(i,weights[i]);float length=i==0?set.ready.length:set.movement[i-1].length;moves[i].SetTime(i==0?Mathf.Repeat(time,length):(phase%1)*length);}
   attackAge+=dt;float progress=attackAge/attackDuration;
   AttackWeight=strikeIndex<0||neutral||progress>=1?0:Mathf.Min(Mathf.Clamp01(attackAge/.045f),Mathf.Clamp01((1-progress)/.22f));
   for(int i=0;i<4;i++){attacks.SetInputWeight(i,i==strikeIndex?1:0);if(i==strikeIndex)strikes[i].SetTime(Mathf.Clamp01(progress)*Mathf.Max(0,set.attacks[i].length-.001f));}
   layers.SetInputWeight(1,AttackWeight);graph.Evaluate(0);
  }
  public void Dispose(){if(graph.IsValid())graph.Destroy();if(mask){if(Application.isPlaying)UnityEngine.Object.Destroy(mask);else UnityEngine.Object.DestroyImmediate(mask);}}
 }
}
