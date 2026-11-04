using UnityEngine;
namespace Veilbound {
 public sealed class AreaWarningVisual:MonoBehaviour {
  public float Radius {get;private set;}
  public float Progress {get;private set;}
  LineRenderer growing;
  const int Points=64;
  public static AreaWarningVisual Create(Transform parent,string name,Vector3 position,float radius,Color color) {
   var root=WorldArt.Group(name,parent);root.position=position;
   WorldArt.Ring(root,Vector3.zero,radius,color*.65f,.075f,Points);
   root.GetChild(0).name="Maximum damage radius";
   WorldArt.Ring(root,Vector3.up*.012f,radius,color,.105f,Points);
   root.GetChild(1).name="Time to impact";
   var visual=root.gameObject.AddComponent<AreaWarningVisual>();visual.Radius=radius;visual.growing=root.GetChild(1).GetComponent<LineRenderer>();visual.SetProgress(0);return visual;
  }
  public void SetProgress(float progress) {
   Progress=Mathf.Clamp01(progress);float r=Radius*Progress;
   for(int i=0;i<Points;i++){float a=i*Mathf.PI*2/Points;growing.SetPosition(i,new Vector3(Mathf.Cos(a)*r,.012f,Mathf.Sin(a)*r));}
  }
 }
}
