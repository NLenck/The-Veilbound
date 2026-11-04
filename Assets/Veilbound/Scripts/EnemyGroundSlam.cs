using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  const float SlamCastSeconds=1,SlamRecoverySeconds=.45f,SlamCooldownSeconds=4.5f;
  bool IsLargeSlammingEnemy(Foe f)=>f.boss>0||f.kind==2;
  bool IsFrontalBrute(Foe f)=>f.rig.visual.production&&f.rig.visual.production.assetId=="Town/Brute";
  float GroundSlamRadius(Foe f)=>f.boss>0?3.36f*f.t.localScale.x:2.82f;
  float GroundSlamDamage(Foe f)=>(f.boss>0?26+f.boss*4:24);
  bool HandleGroundSlam(Foe f,float dt) {
   if(f.dead||!IsLargeSlammingEnemy(f))return false;
   if(f.slamHazard!=null){f.rig.Animate(animClock,0,dt);f.rig.visual.GroundSlamPose(1-f.slamHazard.remaining/SlamCastSeconds);return true;}
   if(f.slamRecovery>0){f.slamRecovery=Mathf.Max(0,f.slamRecovery-dt);f.rig.Animate(animClock,0,dt);f.rig.visual.GroundSlamPose(1+(1-f.slamRecovery/SlamRecoverySeconds));return true;}
   if(f.attack>0||Vector3.Distance(position,f.t.position)>GroundSlamRadius(f)+.35f||!collision.ClearLine(f.t.position,position,.1f))return false;
   bool frontal=IsFrontalBrute(f);Vector3 direction=position-f.t.position;direction.y=0;if(direction.sqrMagnitude<.001f)direction=f.t.forward;direction.Normalize();f.t.rotation=Quaternion.LookRotation(direction);
   float radius=GroundSlamRadius(f);Telegraph(f.t.position,radius,SlamCastSeconds,GroundSlamDamage(f),"enemy-slam",f);
   if(frontal){var h=hazards[hazards.Count-1];Destroy(h.t.gameObject);h.directional=true;h.direction=direction;h.warning=AreaWarningVisual.CreateSector(effects,"Brute - frontal slam",f.t.position+Vector3.up*.06f,direction,radius,55,CombatIndicatorColors.Enemy);h.t=h.warning.transform;}
   f.slamHazard=hazards[hazards.Count-1];f.rig.Animate(animClock,0,dt);f.rig.visual.GroundSlamPose(0);return true;
  }
  bool SlamCancelled(Hazard h)=>h.source=="enemy-slam"&&(h.owner==null||h.owner.dead||!h.owner.t||!foes.Contains(h.owner));
  void CompleteGroundSlam(Hazard h) {
   var f=h.owner;f.slamHazard=null;f.slamRecovery=SlamRecoverySeconds;f.attack=SlamCooldownSeconds;f.rig.Animate(animClock,0,0);f.rig.visual.GroundSlamPose(1);
   var root=WorldArt.Group("Ground slam impact — radial fractures",effects);root.position=h.p+Vector3.up*.035f;
   for(int i=0;i<7;i++){
    var line=WorldArt.Group("Ground fracture "+i,root).gameObject.AddComponent<LineRenderer>();line.useWorldSpace=false;line.sharedMaterial=WorldArt.Glow(CombatIndicatorColors.Enemy);line.widthMultiplier=.035f;line.positionCount=4;
    float a=h.directional?(Mathf.Atan2(h.direction.z,h.direction.x)+Mathf.Lerp(-h.halfAngle,h.halfAngle,i/6f)*Mathf.Deg2Rad):i*Mathf.PI*2/7;for(int j=0;j<4;j++){float r=h.radius*(.18f+j*.23f),angle=a+(j%2==0?-.055f:.06f);line.SetPosition(j,new Vector3(Mathf.Cos(angle)*r,0,Mathf.Sin(angle)*r));}
   }
   flashes.Add(new Flash{t=root,life=.35f,duration=.35f});
  }
 }
}
