using UnityEngine;
namespace Veilbound {
 public sealed class AreaWarningVisual:MonoBehaviour {
  public float Radius {get;private set;}
  public float Progress {get;private set;}
  LineRenderer growing;bool sector;float halfAngle;Mesh fill;Material fillMaterial;
  const int Points=64;
  public static AreaWarningVisual Create(Transform parent,string name,Vector3 position,float radius,Color color) {
   var root=WorldArt.Group(name,parent);root.position=position;
   WorldArt.Ring(root,Vector3.zero,radius,color*.65f,.075f,Points);
   root.GetChild(0).name="Maximum damage radius";
   WorldArt.Ring(root,Vector3.up*.012f,radius,color,.105f,Points);
   root.GetChild(1).name="Time to impact";
   var visual=root.gameObject.AddComponent<AreaWarningVisual>();visual.Radius=radius;visual.growing=root.GetChild(1).GetComponent<LineRenderer>();visual.SetProgress(0);return visual;
  }
  public static AreaWarningVisual CreateSector(Transform parent,string name,Vector3 position,Vector3 direction,float radius,float halfAngle,Color color){
   var root=WorldArt.Group(name,parent);root.position=position;root.rotation=Quaternion.LookRotation(direction);var v=root.gameObject.AddComponent<AreaWarningVisual>();v.Radius=radius;v.sector=true;v.halfAngle=halfAngle;
   for(int i=0;i<2;i++){var line=WorldArt.Group(i==0?"Maximum forward damage area":"Time to impact",root).gameObject.AddComponent<LineRenderer>();line.useWorldSpace=false;line.positionCount=Points+3;line.widthMultiplier=i==0?.075f:.105f;line.sharedMaterial=WorldArt.Glow(color);if(i==0)v.SectorLine(line,radius);else v.growing=line;}
   v.fill=new Mesh{name="Frontal slam filling sector"};root.gameObject.AddComponent<MeshFilter>().sharedMesh=v.fill;v.fillMaterial=new Material(Resources.Load<Shader>("EnemyWarningFill"));v.fillMaterial.color=new Color(color.r,color.g,color.b,.22f);root.gameObject.AddComponent<MeshRenderer>().sharedMaterial=v.fillMaterial;v.SetProgress(0);return v;
  }
  void SectorLine(LineRenderer line,float radius){line.SetPosition(0,Vector3.zero);for(int i=0;i<=Points;i++){float a=Mathf.Lerp(-halfAngle,halfAngle,i/(float)Points)*Mathf.Deg2Rad;line.SetPosition(i+1,new Vector3(Mathf.Sin(a)*radius,.012f,Mathf.Cos(a)*radius));}line.SetPosition(Points+2,Vector3.zero);}
  void OnDestroy(){if(fill)Destroy(fill);if(fillMaterial)Destroy(fillMaterial);}
  public void SetProgress(float progress) {
   Progress=Mathf.Clamp01(progress);float r=Radius*Progress;
   if(sector){SectorLine(growing,r);var vertices=new Vector3[Points+2];var triangles=new int[Points*3];for(int i=0;i<=Points;i++){float a=Mathf.Lerp(-halfAngle,halfAngle,i/(float)Points)*Mathf.Deg2Rad;vertices[i+1]=new Vector3(Mathf.Sin(a)*r,.008f,Mathf.Cos(a)*r);if(i<Points){triangles[i*3]=0;triangles[i*3+1]=i+1;triangles[i*3+2]=i+2;}}fill.vertices=vertices;fill.triangles=triangles;fill.RecalculateBounds();return;}
   for(int i=0;i<Points;i++){float a=i*Mathf.PI*2/Points;growing.SetPosition(i,new Vector3(Mathf.Cos(a)*r,.012f,Mathf.Sin(a)*r));}
  }
 }
}
