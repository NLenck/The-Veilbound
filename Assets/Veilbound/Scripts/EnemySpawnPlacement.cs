using System;
using UnityEngine;
namespace Veilbound {
 public static class EnemySpawnPlacement {
  public static Bounds Measure(Transform root){
   bool found=false;Bounds result=new Bounds(root.position,Vector3.zero);
   foreach(var renderer in root.GetComponentsInChildren<Renderer>())if(renderer.enabled){if(!found){result=renderer.bounds;found=true;}else result.Encapsulate(renderer.bounds);}
   Vector3 low=result.min-root.position,high=result.max-root.position;
   float x=Mathf.Max(Mathf.Abs(low.x),Mathf.Abs(high.x)),z=Mathf.Max(Mathf.Abs(low.z),Mathf.Abs(high.z));float radius=Mathf.Max(.5f,Mathf.Sqrt(x*x+z*z));
   return new Bounds(new Vector3(0,(low.y+high.y)*.5f,0),new Vector3(radius*2,Mathf.Max(.5f,high.y-low.y),radius*2));
  }
  public static bool IsVisible(Camera camera,Transform root){var planes=GeometryUtility.CalculateFrustumPlanes(camera);foreach(var renderer in root.GetComponentsInChildren<Renderer>())if(renderer.enabled&&GeometryUtility.TestPlanesAABB(planes,renderer.bounds))return true;return false;}
  static bool GameplayViewIntersects(Bounds bounds,Camera camera,Vector3 player){
   Quaternion rotation=Quaternion.Euler(51,0,0);Vector3 origin=player+new Vector3(0,20,-16),delta=bounds.center-origin;
   Vector3 right=Vector3.right,up=rotation*Vector3.up,forward=rotation*Vector3.forward,e=bounds.extents;
   float Extent(Vector3 axis)=>Mathf.Abs(axis.x)*e.x+Mathf.Abs(axis.y)*e.y+Mathf.Abs(axis.z)*e.z;
   float depth=Vector3.Dot(delta,forward);
   return Mathf.Abs(Vector3.Dot(delta,right))<=12*camera.aspect+Extent(right)&&Mathf.Abs(Vector3.Dot(delta,up))<=12+Extent(up)&&depth+Extent(forward)>=camera.nearClipPlane&&depth-Extent(forward)<=camera.farClipPlane;
  }
  static Bounds At(Bounds shape,Vector3 point){shape.center+=point;shape.Expand(1.5f);return shape;}
  static bool Outside(Plane[] planes,Camera camera,Vector3 player,Bounds shape,Vector3 point){var bounds=At(shape,point);return !GeometryUtility.TestPlanesAABB(planes,bounds)&&!GameplayViewIntersects(bounds,camera,player);}
  public static bool OutsideNoSpawnZone(Camera camera,Vector3 player,Bounds shape,Vector3 point)=>camera&&Outside(GeometryUtility.CalculateFrustumPlanes(camera),camera,player,shape,point);
  public static bool TryFind(System.Random random,ArenaCollision collision,ArenaNavigation navigation,Vector3 player,float bodyRadius,bool boss,Camera camera,out Vector3 point)=>TryFind(random,collision,navigation,player,bodyRadius,new Bounds(Vector3.up*(boss?3:1.5f),new Vector3(boss?8:3,boss?6:3,boss?8:3)),camera,out point);
  public static bool TryFind(System.Random random,ArenaCollision collision,ArenaNavigation navigation,Vector3 player,float bodyRadius,Bounds shape,Camera camera,out Vector3 point){
   point=Vector3.zero;if(!camera)return false;var planes=GeometryUtility.CalculateFrustumPlanes(camera);float edge=collision.Radius-bodyRadius-.16f,best=float.MaxValue;bool found=false;Vector3 selected=Vector3.zero;float preferredAngle=(float)random.NextDouble()*Mathf.PI*2;Vector3 preferred=new Vector3(Mathf.Cos(preferredAngle),0,Mathf.Sin(preferredAngle));
   bool Consider(Vector3 candidate){
    Vector3 delta=candidate-player;float score=delta.sqrMagnitude*(1+.75f*(1-Vector3.Dot(delta.normalized,preferred)));if(score>=best||candidate.magnitude>edge||!Outside(planes,camera,player,shape,candidate)||!collision.Free(candidate,bodyRadius)||!navigation.Reachable(candidate,bodyRadius))return false;
    best=score;selected=candidate;return true;
   }
   // All reachable ground outside the view is eligible. Prefer nearby legal
   // samples in varied directions, without a fixed-distance band.
   for(int i=0;i<160;i++){float angle=(float)random.NextDouble()*Mathf.PI*2,r=edge*Mathf.Sqrt((float)random.NextDouble());if(Consider(new Vector3(Mathf.Cos(angle)*r,0,Mathf.Sin(angle)*r)))found=true;}
   if(found){point=selected;return true;}
   // Resolve over the remaining arena immediately: no timed waiting or
   // on-screen fallback when the first candidates are blocked.
   for(float x=-edge;x<=edge;x+=1.5f)for(float z=-edge;z<=edge;z+=1.5f)if(Consider(new Vector3(x,0,z)))found=true;
   if(found)point=selected;return found;
  }
 }
}
