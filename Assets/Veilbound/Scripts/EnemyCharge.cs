using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  const float ChargeWindup=1.25f,ChargeSpeed=12,ChargeDistance=8,ChargeRest=.65f,ChargeCooldown=6;
  bool IsChargingEnemy(Foe f){string id=f.rig.visual.production?f.rig.visual.production.assetId:"";return f.boss==0&&(id=="Town/Impaler"||id=="Shrine/Blade_Sentinel");}
  void CancelEnemyCharge(Foe f){if(f.chargeWarning)Destroy(f.chargeWarning.gameObject);f.chargeWarning=null;f.chargeRemaining=f.chargeWindup=f.chargeRecovery=0;}
  float ClearChargeLength(Vector3 origin,Vector3 direction,float radius){float length=0;for(float d=.15f;d<=ChargeDistance+.001f;d+=.15f){var p=origin+direction*d;if(!collision.Free(p,radius)||!collision.ClearLine(origin,p,radius))break;length=d;}return length;}
  bool HandleCharge(Foe f,float dt){
   if(f.dead){CancelEnemyCharge(f);return false;}if(!IsChargingEnemy(f))return false;
   f.chargeCooldown=Mathf.Max(0,f.chargeCooldown-dt);
   if(f.chargeRecovery>0){f.chargeRecovery=Mathf.Max(0,f.chargeRecovery-dt);f.rig.SetMotion(Vector3.zero);f.rig.Animate(animClock,0,dt);return true;}
   if(f.chargeWarning){
    f.chargeWindup=Mathf.Max(0,f.chargeWindup-dt);f.chargeWarning.SetProgress(1-f.chargeWindup/ChargeWindup);f.t.rotation=Quaternion.LookRotation(f.chargeDirection);f.rig.SetMotion(Vector3.zero);f.rig.Animate(animClock,0,dt);f.rig.visual.bones[1].localRotation*=Quaternion.Euler(10*f.chargeWarning.Progress,0,0);
    if(f.chargeWindup<=0){Destroy(f.chargeWarning.gameObject);f.chargeWarning=null;f.chargeRemaining=f.chargeLength;}
    return true;
   }
   if(f.chargeRemaining>0){
    Vector3 before=f.t.position;float move=Mathf.Min(f.chargeRemaining,ChargeSpeed*EnemyTravelFactor(f)*dt);Vector3 desired=before+f.chargeDirection*move;Vector3 next=ClipEnemyMovement(f,before,desired);bool bodyBlocked=(next-desired).sqrMagnitude>.0000001f;
    if(!collision.Free(next,f.radius)||!collision.ClearLine(before,next,f.radius)){f.chargeRemaining=0;}
    else{
     f.t.position=next;f.chargeRemaining=bodyBlocked?0:f.chargeRemaining-move;
     if(!f.chargeHit&&EnemyAttackGeometry.SegmentDistance(position,before,next)<=f.radius+.30f+.35f&&collision.ClearLine(next,position,.1f)){f.chargeHit=true;Hurt(18,f);}
    }
    f.t.rotation=Quaternion.LookRotation(f.chargeDirection);f.rig.SetMotion(dt>0?(f.t.position-before)/dt:Vector3.zero);f.rig.Animate(animClock,1,dt);f.rig.visual.bones[1].localRotation*=Quaternion.Euler(12,0,0);
    if(f.chargeRemaining<=.001f){f.chargeRemaining=0;f.chargeRecovery=ChargeRest;f.chargeCooldown=ChargeCooldown;f.attack=0;}return true;
   }
   Vector3 delta=position-f.t.position;delta.y=0;float distance=delta.magnitude;
   if(f.attack>0||f.chargeCooldown>0||distance<3.5f||distance>12||!collision.ClearLine(f.t.position,position,.1f))return false;
   Vector3 direction=delta.normalized;float length=ClearChargeLength(f.t.position,direction,f.radius);if(length<3)return false;
   f.chargeOrigin=f.t.position;f.chargeDirection=direction;f.chargeLength=length;f.chargeWindup=ChargeWindup;f.chargeHit=false;f.t.rotation=Quaternion.LookRotation(direction);f.chargeWarning=ChargeWarningVisual.Create(effects,f.chargeOrigin,direction,length,f.radius+.30f);return true;
  }
 }
}
