using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Veilbound.PlateMasters;
namespace Veilbound {
 public sealed class ProductionHeroVisual:MonoBehaviour {
  public MasterPlateEquipment equipment;public BronzeHeroVisual facade;public bool female;
  public Transform sword;public float measuredSpeed,lastGripError;public int CurrentMotion;
  public GreatswordMotionPlayer greatsword;
  public void PlayBasicAttack(float interval){greatsword?.PlayAttack(interval);}
  public void PlayHeavyAttack(){greatsword?.PlayAttack(.7f,true);}
  PlayableGraph graph;AnimationMixerPlayable mixer;AnimationClipPlayable idle,walk,run;
  readonly List<Material> owned=new List<Material>();Vector3 resolvedVelocity;bool resolved;
  float previousTime=-1,smoothSpeed,walkDuration,runDuration;double gaitPhase;float height;
  public static bool Available=>ProductionModelCatalog.Current;
  public static ProductionHeroVisual Create(Transform parent,Settlement city) {
   var entry=ProductionModelCatalog.Current.Player(city.female);
   if(entry==null||!entry.prefab)throw new InvalidOperationException("Missing production player for selected appearance");
   var instance=Instantiate(entry.prefab,parent,false);instance.name="Veilborne - "+entry.gender;
   var v=instance.AddComponent<ProductionHeroVisual>();v.female=city.female;v.height=city.female?1.73f:1.85f;
   v.walkDuration=entry.walk.length;v.runDuration=entry.run.length;
   v.equipment=instance.GetComponentInChildren<MasterPlateEquipment>(true);
   if(!v.equipment)throw new InvalidOperationException("Production armour/body mask component is missing");
   var animator=instance.GetComponent<Animator>();
   foreach(var driver in instance.GetComponentsInChildren<Veilbound.PlayerStaging.VeilborneAnimationDriver>(true))driver.enabled=false;
   animator.enabled=true;animator.applyRootMotion=false;animator.runtimeAnimatorController=null;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
   v.graph=PlayableGraph.Create("Veilborne "+entry.gender+" direct skeleton locomotion");v.graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
   var output=AnimationPlayableOutput.Create(v.graph,"Locomotion",animator);v.mixer=AnimationMixerPlayable.Create(v.graph,3);output.SetSourcePlayable(v.mixer);
   v.idle=AnimationClipPlayable.Create(v.graph,entry.idle);v.walk=AnimationClipPlayable.Create(v.graph,entry.walk);v.run=AnimationClipPlayable.Create(v.graph,entry.run);
   v.graph.Connect(v.idle,0,v.mixer,0);v.graph.Connect(v.walk,0,v.mixer,1);v.graph.Connect(v.run,0,v.mixer,2);v.mixer.SetInputWeight(0,1);v.graph.Play();v.graph.Evaluate(0);
   string[] names={"Hips","Spine","Spine1","Neck","Head","LeftArm","LeftForeArm","LeftHand","RightArm","RightForeArm","RightHand","LeftUpLeg","LeftLeg","LeftFoot","RightUpLeg","RightLeg","RightFoot"};
   var all=instance.GetComponentsInChildren<Transform>(true);var bones=new Transform[17];var rest=new Vector3[17];
   for(int i=0;i<17;i++){bones[i]=Array.Find(all,b=>b.name=="mixamorig:"+names[i]);if(!bones[i])throw new InvalidOperationException("Missing production joint "+names[i]);rest[i]=instance.transform.InverseTransformPoint(bones[i].position);}
   // The compatibility facade keeps inventory thumbnails and existing combat callers on the same joints.
   v.facade=instance.AddComponent<BronzeHeroVisual>();v.facade.bones=bones;v.facade.rest=rest;v.facade.bodySkin=v.equipment.body;v.facade.parts=v.equipment.slots;
   v.facade.slots=new[]{0,1,2,3,4,5,6,7,8};v.facade.emberEyes=Array.Empty<Renderer>();v.facade.configured=true;
   v.greatsword=GreatswordMotionPlayer.TryCreate(animator,v.female);if(v.greatsword!=null)v.graph.Destroy();
   v.Configure(city);
   return v;
  }
  void Configure(Settlement city) {
   city.EnsureGear();int primary=Metals.Index(city.Item(2));if(primary<0)primary=city.WeaponMetalIndex;
   for(int slot=0;slot<9;slot++) {
    int item=city.Item(slot);if(item==0){equipment.SetSlotEnabled(slot,false);continue;}
    int metal=Metals.Index(item);if(slot==8)metal=primary;
    equipment.SetSlotTier(slot,Metals.Item(metal>=0?metal:0));
    if(metal<0){var material=new Material(equipment.slots[slot].sharedMaterial);material.name=Gear.Material(item)==2?"Fitted leather equipment":"Fitted cloth equipment";material.DisableKeyword("_METALLICGLOSSMAP");material.SetTexture("_MetallicGlossMap",null);material.SetFloat("_Metallic",0);material.SetFloat("_Glossiness",.15f);material.color=Gear.Material(item)==2?new Color(.40f,.24f,.14f):new Color(.48f,.52f,.58f);equipment.slots[slot].sharedMaterial=material;owned.Add(material);}
   }
   var fill=new GameObject("Hero readability fill");fill.transform.SetParent(transform,false);fill.transform.localPosition=new Vector3(0,1.45f,.6f);var lamp=fill.AddComponent<Light>();lamp.type=LightType.Point;lamp.range=2.4f;lamp.intensity=.6f;lamp.color=new Color(1,.87f,.77f);lamp.shadows=LightShadows.None;
  }
  public void SetMotion(Vector3 actualVelocity){resolvedVelocity=Vector3.ProjectOnPlane(actualVelocity,Vector3.up);resolved=true;}
  public void Animate(float time,float movement,float swing,string gesture,float amount,bool neutral=false) {
   if(greatsword!=null){AnimateGreatsword(time,movement,gesture,amount,neutral);return;}
   if(!graph.IsValid())return;
   bool rewind=previousTime<0||time<previousTime;float dt=rewind?0:Mathf.Min(.1f,time-previousTime);previousTime=time;if(rewind||time==0&&movement==0)gaitPhase=0;
   float speed=neutral?0:resolved?resolvedVelocity.magnitude:Mathf.Clamp01(movement)*3.6f;
   if(time==0&&movement==0)speed=0;
   smoothSpeed=dt>0?Mathf.Lerp(smoothSpeed,speed,1-Mathf.Exp(-dt/ .12f)):speed;measuredSpeed=smoothSpeed;
   float gait=Mathf.Clamp(smoothSpeed/3.6f,.55f,1.85f),walking=Mathf.Clamp01(smoothSpeed/1.2f),running=Mathf.Clamp01((smoothSpeed-1.2f)/2.4f);
   mixer.SetInputWeight(0,1-walking);mixer.SetInputWeight(1,walking*(1-running));mixer.SetInputWeight(2,walking*running);CurrentMotion=smoothSpeed<.1f?0:smoothSpeed<2?1:2;
   float cycles=Mathf.Lerp(Mathf.Clamp(smoothSpeed/1.2f,0,1.5f)/walkDuration,Mathf.Clamp(smoothSpeed/3.6f,0,1.85f)/runDuration,running);gaitPhase+=dt*cycles;
   idle.SetTime(time);walk.SetTime(gaitPhase*walkDuration);run.SetTime(gaitPhase*runDuration);graph.Evaluate(0);
   if(neutral||!sword)return;
   var b=facade.bones;Vector3 hip=transform.InverseTransformPoint(b[0].position);
   sword.localPosition=new Vector3(.015f,hip.y+.10f,.23f);sword.localRotation=Quaternion.Euler(-43,Mathf.Lerp(-28,74,swing),-3);
   if(gesture=="smite")sword.localRotation*=Quaternion.Euler(-60*amount,0,0);
   if(gesture=="warcry"||gesture=="rage"||gesture=="brand"||gesture=="bloodthirst")b[2].localRotation*=Quaternion.Euler(-7*amount,0,0);
   Vector3 right=sword.TransformPoint(new Vector3(0,0,.015f))+transform.up*.035f,left=sword.TransformPoint(new Vector3(0,0,.17f))+transform.up*.035f;
   Arm(8,right,transform.TransformDirection(new Vector3(1,-.4f,-.35f)));Arm(5,left,transform.TransformDirection(new Vector3(-1,-.4f,-.35f)));
   lastGripError=Mathf.Max(Vector3.Distance(b[10].position,right),Vector3.Distance(b[7].position,left));
   if(gesture=="lance"&&amount>0)Arm(5,Vector3.Lerp(left,transform.TransformPoint(new Vector3(-.25f,height*.70f,.74f)),amount),transform.TransformDirection(new Vector3(-1,-.3f,-.2f)));
  }
  void AnimateGreatsword(float time,float movement,string gesture,float amount,bool neutral) {
   Vector3 velocity=resolved?resolvedVelocity:transform.forward*Mathf.Clamp01(movement)*3.6f;
   if(time==0&&movement==0)velocity=Vector3.zero;
   greatsword.Evaluate(time,velocity,neutral);measuredSpeed=greatsword.Speed;CurrentMotion=measuredSpeed<.1f?0:measuredSpeed<2?1:2;
   var b=facade.bones;
   if(!neutral&&(gesture=="warcry"||gesture=="rage"||gesture=="brand"||gesture=="bloodthirst"))b[2].localRotation*=Quaternion.Euler(-7*amount,0,0);
   if(!sword)return;
   Vector3 right=b[10].position,left=b[7].position,blade=right-left;
   if(blade.sqrMagnitude<.00001f)blade=transform.forward;
   Vector3 up=Vector3.ProjectOnPlane(transform.up,blade.normalized);if(up.sqrMagnitude<.001f)up=Vector3.ProjectOnPlane(transform.forward,blade.normalized);
   sword.rotation=Quaternion.LookRotation(blade,up);
   sword.position=right-sword.TransformVector(new Vector3(0,.035f,.17f));
   Vector3 grip=sword.TransformPoint(new Vector3(0,.035f,.015f));Quaternion palm=b[7].rotation;
   Arm(5,grip,b[6].position-b[5].position);b[7].rotation=palm;
   lastGripError=Mathf.Max(Vector3.Distance(b[10].position,sword.TransformPoint(new Vector3(0,.035f,.17f))),Vector3.Distance(b[7].position,grip));
   if(!neutral&&gesture=="lance"&&amount>0)Arm(5,Vector3.Lerp(grip,transform.TransformPoint(new Vector3(-.25f,height*.70f,.74f)),amount),transform.TransformDirection(new Vector3(-1,-.3f,-.2f)));
  }
  void Aim(Transform at,Transform end,Vector3 target){at.rotation=Quaternion.FromToRotation(end.position-at.position,target-at.position)*at.rotation;}
  void Arm(int start,Vector3 target,Vector3 pole) {
   var b=facade.bones;float upper=Vector3.Distance(b[start].position,b[start+1].position),lower=Vector3.Distance(b[start+1].position,b[start+2].position);Vector3 at=b[start].position,delta=target-at;
   float distance=Mathf.Clamp(delta.magnitude,.025f,upper+lower-.001f);Vector3 direction=delta.normalized,p=Vector3.ProjectOnPlane(pole,direction).normalized;
   float along=Mathf.Clamp((upper*upper+distance*distance-lower*lower)/(2*upper*distance),-1,1);Vector3 joint=at+direction*(along*upper)+p*(Mathf.Sqrt(1-along*along)*upper);
   Aim(b[start],b[start+1],joint);Aim(b[start+1],b[start+2],at+direction*distance);
   // Preserve each imported hand's palm orientation relative to the forearm rather than zeroing its rest rotation.
  }
  public void Release(){greatsword?.Dispose();greatsword=null;if(graph.IsValid())graph.Destroy();foreach(var m in owned)if(m){if(Application.isPlaying)Destroy(m);else DestroyImmediate(m);}owned.Clear();}
  void OnDestroy(){Release();}
 }
}
