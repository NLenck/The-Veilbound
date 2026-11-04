using UnityEngine;
using M=Veilbound.RuinedTownArena.Meshes;
namespace Veilbound {
 public static class CrystalClusters {
  public static readonly Color Dark=new Color(.25f,.014f,.025f),Ruby=new Color(.48f,.025f,.04f),Edge=new Color(.68f,.032f,.055f);
  static float R(System.Random r,float a,float b)=>Mathf.Lerp(a,b,(float)r.NextDouble());
  static Vector3 Rad(float a,float radius)=>new Vector3(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius);
  static void Shard(M[] faces,Vector3 p,float height,float width,Vector3 lean,float rotation,System.Random r){
   const int sides=6;var lower=new Vector3[sides];var shoulder=new Vector3[sides];
   for(int i=0;i<sides;i++){
    float a=rotation+i*Mathf.PI/3;
    lower[i]=p+Rad(a,width*.78f)+Vector3.up*height*.04f;
    shoulder[i]=p+lean*.72f+Rad(a,width)+Vector3.up*(height*(.69f+.055f*Mathf.Sin(a+rotation)));
   }
   Vector3 tip=p+lean+Vector3.up*height;
   for(int i=0;i<sides;i++){int j=(i+1)%sides;
    // Long hexagonal faces and short angled terminations give a mineral silhouette.
    faces[(i+1)%3].Face(lower[i],shoulder[i],shoulder[j],lower[j]);
    faces[i%3].Face(shoulder[i],tip,shoulder[j]);
    faces[0].Face(p,lower[i],lower[j]);
   }
  }
  public static void Add(M dark,M ruby,M edge,Vector3 centre,float scale,int seed){
   var rng=new System.Random(seed);var faces=new[]{dark,ruby,edge};float turn=R(rng,0,Mathf.PI*2);
   // One dominant shard and six smaller crystals emerge from the same compact root.
   Shard(faces,centre,R(rng,2.7f,3.05f)*scale,.29f*scale,Rad(turn,.17f*scale),turn,rng);
   for(int i=0;i<6;i++){
    float angle=turn+i*Mathf.PI/3+R(rng,-.15f,.15f),radius=R(rng,.31f,.44f)*scale;
    Vector3 p=centre+Rad(angle,radius);float h=R(rng,1.05f,2.25f)*scale,w=R(rng,.15f,.235f)*scale;
    Shard(faces,p,h,w,Rad(angle,R(rng,.22f,.39f)*scale),R(rng,0,Mathf.PI*2),rng);
   }
   for(int i=0;i<3;i++){float angle=turn+i*2.1f;Shard(faces,centre+Rad(angle,.45f*scale),R(rng,.3f,.55f)*scale,.17f*scale,Rad(angle,.18f*scale),angle,rng);}
  }
 }
}
