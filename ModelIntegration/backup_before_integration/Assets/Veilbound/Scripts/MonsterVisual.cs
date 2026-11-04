using System;
using UnityEngine;
namespace Veilbound {
 public sealed class MonsterVisual:MonoBehaviour {
  public int kind;
  public Transform[] bones;
  public Vector3[] rest;
  public SkinnedMeshRenderer skin;
  public string[] features;
  public float bodyWidth;
  public static readonly string[] Ids={"standard","lobber","brute","ghoul","volatile","elder"};
  Color[] emissions;MaterialPropertyBlock block;float flash,fuse=-1;
  public static MonsterVisual Create(Transform parent,int kind,int boss) {
   string id=Ids[boss>0?5:kind];var prefab=Resources.Load<GameObject>("MonsterFamily/"+id);
   if(!prefab)throw new InvalidOperationException("Missing authored monster prefab: "+id+". Run Veilbound/Import monster family.");
   var instance=Instantiate(prefab,parent,false);var visual=instance.GetComponent<MonsterVisual>();
   if(boss>0)instance.transform.localScale=Vector3.one*(1+(boss-1)*.07f);
   visual.Animate(0,0,0);return visual;
  }
  public bool Has(string name){foreach(string feature in features)if(feature.StartsWith(name,StringComparison.Ordinal))return true;return false;}
  public int Count(string name){int count=0;foreach(string feature in features)if(feature.StartsWith(name,StringComparison.Ordinal))count++;return count;}
  public Vector3 CastingOrigin(int side){int wrist=side<0?7:10;return bones[wrist].position+transform.TransformVector(new Vector3(side*.08f,.20f,-.025f));}
  public void Hit(){flash=.16f;RefreshEmission();}
  public void FuseGlow(float progress){fuse=progress;RefreshEmission();}
  void RefreshEmission() {
   if(block==null)block=new MaterialPropertyBlock();
   if(emissions==null){var mats=skin.sharedMaterials;emissions=new Color[mats.Length];for(int i=0;i<mats.Length;i++)emissions[i]=mats[i].GetColor("_EmissionColor");}
   float pulse=fuse<0?0:.12f+Mathf.Abs(Mathf.Sin(fuse*35))*(.20f+fuse*.5f);
   for(int i=0;i<emissions.Length;i++){block.Clear();block.SetColor("_EmissionColor",emissions[i]+new Color(.8f,.10f,.025f)*(flash/.16f+pulse));skin.SetPropertyBlock(block,i);}
  }
  void Aim(int bone,int child,Vector3 target){bones[bone].rotation=Quaternion.FromToRotation(bones[child].position-bones[bone].position,target-bones[bone].position)*bones[bone].rotation;}
  void Limb(int start,Vector3 target,Vector3 pole) {
   Vector3 at=bones[start].position;float upper=Vector3.Distance(rest[start],rest[start+1])*transform.lossyScale.x,lower=Vector3.Distance(rest[start+1],rest[start+2])*transform.lossyScale.x;
   Vector3 delta=target-at;float distance=Mathf.Clamp(delta.magnitude,.02f,upper+lower-.001f);Vector3 dir=delta.normalized;pole=Vector3.ProjectOnPlane(pole,dir).normalized;
   float c=Mathf.Clamp((upper*upper+distance*distance-lower*lower)/(2*upper*distance),-1,1);Vector3 elbow=at+dir*(upper*c)+pole*(upper*Mathf.Sqrt(1-c*c));
   Aim(start,start+1,elbow);Aim(start+1,start+2,at+dir*distance);
  }
  void AlignGhoulForearm(int elbow,int wrist) {
   Vector3 from=rest[wrist]-rest[elbow],to=bones[wrist].position-bones[elbow].position;
   // Define roll from the ground plane, rather than inheriting the shoulder IK roll.
   Vector3 restUp=Vector3.ProjectOnPlane(Vector3.up,from).normalized;
   Vector3 posedUp=Vector3.ProjectOnPlane(transform.up,to).normalized;
   if(restUp.sqrMagnitude>.1f&&posedUp.sqrMagnitude>.1f)
    bones[elbow].rotation=Quaternion.LookRotation(to,posedUp)*Quaternion.Inverse(Quaternion.LookRotation(from,restUp));
  }
  public void Animate(float time,float moving,float dt) {
   for(int i=0;i<bones.Length;i++){bones[i].localRotation=Quaternion.identity;bones[i].localPosition=rest[i]-(i==0?Vector3.zero:rest[Parents[i]]);}
   if(kind==3) {
    float cycle=time*2.6f,bob=Mathf.Sin(cycle*Mathf.PI*4)*.012f*moving;bones[0].localPosition+=Vector3.up*bob;
    for(int leg=0;leg<4;leg++) {
     bool front=leg<2;int start=front?(leg==0?5:8):(leg==2?11:14);int end=start+2;float side=(leg%2==0)?-1:1;
     float phase=Mathf.Repeat(cycle+((leg==0||leg==3)?0:.5f),1);float stride=phase<.6f?Mathf.Lerp(.10f,-.10f,phase/.6f):Mathf.Lerp(-.10f,.10f,(phase-.6f)/.4f);float lift=phase<.6f?0:Mathf.Sin((phase-.6f)/.4f*Mathf.PI)*.10f;
     Vector3 point=rest[end]+new Vector3(0,lift*moving,stride*moving);Limb(start,transform.TransformPoint(point),transform.TransformDirection(new Vector3(side*.5f,.15f,front?-.5f:.65f)));
     if(front)AlignGhoulForearm(start+1,end);
     bones[end].rotation=transform.rotation*Quaternion.Euler(-lift*60*moving,0,0);
    }
   } else {
    float speed=kind==5?3.45f:kind==2?4.1f:kind==4?4.6f:6.2f;float cycle=time*speed;float gait=Mathf.Sin(cycle)*18*moving;
    bones[0].localPosition+=Vector3.up*(Mathf.Abs(Mathf.Sin(cycle))*.016f*moving);
    bones[11].localRotation=Quaternion.Euler(gait,0,-2);bones[14].localRotation=Quaternion.Euler(-gait,0,2);
    bones[12].localRotation=Quaternion.Euler(Mathf.Max(0,-gait)*.9f,0,0);bones[15].localRotation=Quaternion.Euler(Mathf.Max(0,gait)*.9f,0,0);
    bones[13].localRotation=Quaternion.Euler(-gait*.25f,0,0);bones[16].localRotation=Quaternion.Euler(gait*.25f,0,0);
    bones[5].localRotation=Quaternion.Euler(-gait*.3f,0,0);bones[8].localRotation=Quaternion.Euler(gait*.35f,0,0);
    bones[1].localRotation=Quaternion.Euler(Mathf.Sin(time*1.6f)*1.1f,0,Mathf.Sin(cycle)*2*moving);
    bones[4].localRotation=Quaternion.Euler(0,Mathf.Sin(time*2.0f)*1.8f,Mathf.Sin(time*1.3f)*1.5f);
   }
   if(kind==5){float phase=Mathf.Repeat(time*.55f,1);float stride=phase<.68f?Mathf.Lerp(.22f,-.22f,phase/.68f):Mathf.Lerp(-.22f,.22f,(phase-.68f)/.32f);float lift=phase<.68f?0:Mathf.Sin((phase-.68f)/.32f*Mathf.PI)*.16f;bones[1].localRotation*=Quaternion.Euler(0,0,4+Mathf.Sin(time*3.45f)*1.5f*moving);Limb(5,transform.TransformPoint(rest[7]+new Vector3(0,lift*moving,stride*moving)),transform.TransformDirection(new Vector3(-1,.1f,.1f)));bones[7].rotation=transform.rotation;}
   flash=Mathf.Max(0,flash-dt);RefreshEmission();
  }
  public void GroundSlamPose(float progress) {
   if(kind!=2&&kind!=5)return;
   float u=Mathf.Clamp01(progress),raise=u<.72f?Mathf.SmoothStep(0,1,u/.72f):1-Mathf.SmoothStep(0,1,(u-.72f)/.28f);
   float recovery=progress>1?Mathf.Clamp01(2-progress):1;
   float crouch=(u<.72f?.25f*raise:Mathf.SmoothStep(0,1,(u-.72f)/.28f))*recovery;
   bones[0].localPosition+=Vector3.down*.38f*crouch;bones[1].localRotation*=Quaternion.Euler(22*crouch-5*raise,0,0);
   if(kind==5){Vector3 hand=rest[7]+new Vector3(0,2.5f*raise,.35f*(1-raise))*recovery;Limb(5,transform.TransformPoint(hand),transform.TransformDirection(new Vector3(-1,.2f,.3f)));bones[7].rotation=transform.rotation;bones[8].localRotation*=Quaternion.Euler(-65*raise*recovery,0,0);}
   else {bones[5].localRotation*=Quaternion.Euler(-145*raise*recovery,0,-12*raise);bones[8].localRotation*=Quaternion.Euler(-145*raise*recovery,0,12*raise);bones[6].localRotation*=Quaternion.Euler(-25*raise*recovery,0,0);bones[9].localRotation*=Quaternion.Euler(-25*raise*recovery,0,0);
    if(u>.72f&&crouch>0){Limb(5,Vector3.Lerp(bones[7].position,transform.TransformPoint(new Vector3(-.5f,.10f,.6f)),crouch),transform.TransformDirection(new Vector3(-1,0,-.2f)));Limb(8,Vector3.Lerp(bones[10].position,transform.TransformPoint(new Vector3(.5f,.10f,.6f)),crouch),transform.TransformDirection(new Vector3(1,0,-.2f)));}
   }
   Limb(11,transform.TransformPoint(rest[13]),transform.TransformDirection(new Vector3(-.1f,0,1)));Limb(14,transform.TransformPoint(rest[16]),transform.TransformDirection(new Vector3(.1f,0,1)));bones[13].rotation=bones[16].rotation=transform.rotation;
  }
  public static readonly int[] Parents={-1,0,1,2,3,2,5,6,2,8,9,0,11,12,0,14,15};
 }
}

