using System.Collections.Generic;
using UnityEngine;
namespace Veilbound {
 public sealed class EnemyMixamoMotion {
  static readonly Dictionary<string,EnemyMotionSamples> cache=new Dictionary<string,EnemyMotionSamples>();
  readonly MonsterVisual visual;readonly EnemyMotionSamples samples;
  float phase,speed,lastTime=-1;
  public float PlaybackRate{get;private set;}public float ActualSpeed=>speed;
  public EnemyMixamoMotion(MonsterVisual v,EnemyMotionSamples data){visual=v;samples=data;phase=Mathf.Repeat(Mathf.Abs(v.GetInstanceID())*.61803399f,1);}
  public static EnemyMixamoMotion TryCreate(MonsterVisual visual,string id){
   if(!cache.TryGetValue(id,out var data)){data=Resources.Load<EnemyMotionSamples>("EnemyLocomotion/"+id.Replace('/','_'));cache[id]=data;}
   return data&&data.movement!=null&&data.movement.frameCount>1?new EnemyMixamoMotion(visual,data):null;
  }
  public void Evaluate(float time,float moving,float actualSpeed){
   float dt=lastTime<0||time<lastTime?0:Mathf.Clamp(time-lastTime,0,.1f);lastTime=time;
   float target=moving>.001f?Mathf.Max(0,actualSpeed):0;
   speed=dt>0?Mathf.Lerp(speed,target,1-Mathf.Exp(-dt/.08f)):target;
   float scale=Mathf.Max(.01f,Mathf.Abs(visual.transform.lossyScale.x));
   var cycle=samples.movement;
   float fast=samples.HasFastMovement?Mathf.InverseLerp(cycle.NaturalSpeed*scale,samples.fastMovement.NaturalSpeed*scale,speed):0;
   float distance=Mathf.Lerp(cycle.strideDistance,samples.HasFastMovement?samples.fastMovement.strideDistance:cycle.strideDistance,fast)*scale;
   PlaybackRate=speed/Mathf.Max(.05f,distance)*Mathf.Lerp(cycle.duration,samples.HasFastMovement?samples.fastMovement.duration:cycle.duration,fast);
   phase=Mathf.Repeat(phase+dt*speed/Mathf.Max(.05f,distance),1);
   // Settle along the authored cycle instead of blending unrelated limb poses through the floor.
   if(moving<=.001f&&speed<.05f)phase=Mathf.Repeat(Mathf.MoveTowards(phase,phase>.5f?1:0,dt*1.5f),1);
   for(int i=0;i<17;i++){
    Quaternion q=SampleRotation(cycle,phase,i);Vector3 hips=SampleHips(cycle,phase);
    if(fast>0){q=Quaternion.Slerp(q,SampleRotation(samples.fastMovement,phase,i),fast);hips=Vector3.Lerp(hips,SampleHips(samples.fastMovement,phase),fast);}
    visual.bones[i].localRotation=q;
    visual.bones[i].localPosition=i==0?hips:visual.rest[i]-visual.rest[MonsterVisual.Parents[i]];
   }
  }
  static Quaternion SampleRotation(EnemyMotionCycle cycle,float phase,int bone){float frame=phase*(cycle.frameCount-1);int a=Mathf.FloorToInt(frame),b=Mathf.Min(a+1,cycle.frameCount-1);return Quaternion.Slerp(cycle.rotations[a*17+bone],cycle.rotations[b*17+bone],frame-a);}
  static Vector3 SampleHips(EnemyMotionCycle cycle,float phase){float frame=phase*(cycle.frameCount-1);int a=Mathf.FloorToInt(frame),b=Mathf.Min(a+1,cycle.frameCount-1);return Vector3.Lerp(cycle.hips[a],cycle.hips[b],frame-a);}
 }
}
