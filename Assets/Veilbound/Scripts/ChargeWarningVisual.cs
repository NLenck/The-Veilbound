using UnityEngine;
namespace Veilbound {
 public sealed class ChargeWarningVisual:MonoBehaviour {
  public float Length{get;private set;}public float HalfWidth{get;private set;}public float Progress{get;private set;}
  Mesh fill;Material material;
  public static ChargeWarningVisual Create(Transform parent,Vector3 origin,Vector3 direction,float length,float halfWidth){
   var root=WorldArt.Group("Charge - two rails and advancing fill",parent);root.position=origin+Vector3.up*.065f;root.rotation=Quaternion.LookRotation(direction,Vector3.up);
   for(int side=-1;side<=1;side+=2){var line=WorldArt.Group(side<0?"Left boundary":"Right boundary",root).gameObject.AddComponent<LineRenderer>();line.useWorldSpace=false;line.positionCount=2;line.widthMultiplier=.075f;line.sharedMaterial=WorldArt.Glow(CombatIndicatorColors.Enemy);line.SetPosition(0,new Vector3(side*halfWidth,0,0));line.SetPosition(1,new Vector3(side*halfWidth,0,length));}
   var v=root.gameObject.AddComponent<ChargeWarningVisual>();v.Length=length;v.HalfWidth=halfWidth;v.fill=new Mesh{name="Charge growing lane"};root.gameObject.AddComponent<MeshFilter>().sharedMesh=v.fill;var renderer=root.gameObject.AddComponent<MeshRenderer>();v.material=new Material(Resources.Load<Shader>("EnemyWarningFill"));v.material.color=new Color(CombatIndicatorColors.Enemy.r,CombatIndicatorColors.Enemy.g,CombatIndicatorColors.Enemy.b,.32f);renderer.sharedMaterial=v.material;v.SetProgress(0);return v;
  }
  public void SetProgress(float progress){Progress=Mathf.Clamp01(progress);float z=Length*Progress;fill.vertices=new[]{new Vector3(-HalfWidth,.01f,0),new Vector3(HalfWidth,.01f,0),new Vector3(-HalfWidth,.01f,z),new Vector3(HalfWidth,.01f,z)};fill.triangles=new[]{0,2,1,1,2,3};fill.RecalculateBounds();}
  void OnDestroy(){if(fill)Destroy(fill);if(material)Destroy(material);}
 }
 public static class EnemyAttackGeometry {
  public static float SegmentDistance(Vector3 point,Vector3 start,Vector3 end){point.y=start.y=end.y=0;Vector3 d=end-start;float t=d.sqrMagnitude>.000001f?Mathf.Clamp01(Vector3.Dot(point-start,d)/d.sqrMagnitude):0;return Vector3.Distance(point,start+d*t);}
  public static bool SectorContains(Vector3 point,Vector3 origin,Vector3 direction,float radius,float halfAngle,float padding){
   Vector3 delta=point-origin;delta.y=0;direction.y=0;direction.Normalize();float angle=Vector3.Angle(direction,delta);
   if(angle<=halfAngle)return delta.magnitude<=radius+padding;
   var a=origin+Quaternion.Euler(0,-halfAngle,0)*direction*radius;var b=origin+Quaternion.Euler(0,halfAngle,0)*direction*radius;
   return Mathf.Min(SegmentDistance(point,origin,a),SegmentDistance(point,origin,b))<=padding;
  }
 }
}
