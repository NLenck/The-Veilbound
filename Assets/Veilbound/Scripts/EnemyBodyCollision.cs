using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  const float PlayerBodyRadius=.42f;
  // Earliest contact against a circle, including recovery from an existing overlap.
  static float BodyContact(Vector3 start,Vector3 delta,Vector3 centre,float radius){
   Vector3 offset=start-centre;offset.y=delta.y=0;float a=delta.sqrMagnitude;if(a<.0000001f)return 1;
   float c=offset.sqrMagnitude-radius*radius,b=Vector3.Dot(offset,delta);
   if(c<0)return b>=0?1:0;
   if(b>=0)return 1;float discriminant=b*b-a*c;if(discriminant<=0)return 1;
   float t=(-b-Mathf.Sqrt(discriminant))/a;return t>=0&&t<=1?t:1;
  }
  Vector3 ClipEnemyMovement(Foe foe,Vector3 start,Vector3 target){
   if(dodgeTime>0)return target;
   Vector3 delta=target-start;float t=BodyContact(start,delta,position,foe.radius+PlayerBodyRadius+.005f);
   return start+delta*t;
  }
  Vector3 MovePlayerBody(Vector3 start,Vector3 delta){
   if(dodgeTime>0)return collision.Move(start,delta,PlayerBodyRadius,collision.PlayerRadius);
   delta.y=0;int steps=Mathf.Max(1,Mathf.CeilToInt(delta.magnitude/.12f));Vector3 step=delta/steps;
   for(int i=0;i<steps;i++){
    Vector3 remaining=collision.Move(start,step,PlayerBodyRadius,collision.PlayerRadius)-start;
    for(int slide=0;slide<3&&remaining.sqrMagnitude>.0000001f;slide++){
     float fraction=1;Foe blocker=null;
     foreach(var foe in foes)if(!foe.dead&&foe.hp>0){float t=BodyContact(start,remaining,foe.t.position,foe.radius+PlayerBodyRadius+.005f);if(t<fraction){fraction=t;blocker=foe;}}
     Vector3 candidate=start+remaining*fraction;
     // Never trade an enemy overlap for an overlap with arena geometry.
     if(!collision.Free(candidate,PlayerBodyRadius)||candidate.magnitude>collision.PlayerRadius-PlayerBodyRadius)break;
     start=candidate;if(blocker==null)break;
     Vector3 normal=start-blocker.t.position;normal.y=0;if(normal.sqrMagnitude<.000001f)break;normal.Normalize();
     remaining*=1-fraction;remaining-=normal*Mathf.Min(0,Vector3.Dot(remaining,normal));
     remaining=collision.Move(start,remaining,PlayerBodyRadius,collision.PlayerRadius)-start;
    }
   }
   return start;
  }
 }
}
