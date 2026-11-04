using UnityEngine;
using M=Veilbound.RuinedTownArena.Meshes;
namespace Veilbound {
 public static class RuinMasonry {
  static float R(System.Random r,float a,float b)=>Mathf.Lerp(a,b,(float)r.NextDouble());
  static void Block(M mesh,Vector3 p,Vector3 size,float yaw,System.Random r){
   float x=size.x*.5f,z=size.z*.5f,cut=Mathf.Min(x,z)*R(r,.2f,.45f);var q=Quaternion.Euler(0,yaw,0);
   Vector2[] shape={new Vector2(-x+cut,-z),new Vector2(x-cut,-z),new Vector2(x,-z+cut),new Vector2(x,z-cut),new Vector2(x-cut,z),new Vector2(-x+cut,z),new Vector2(-x,z-cut),new Vector2(-x,-z+cut)};
   var top=new Vector3[8];var bottom=new Vector3[8];
   for(int i=0;i<8;i++){var v=shape[i];bottom[i]=p+q*new Vector3(v.x,-size.y*.5f,v.y);top[i]=p+q*new Vector3(v.x*R(r,.86f,1),size.y*.5f-R(r,0,size.y*.22f),v.y*R(r,.86f,1));}
   for(int i=0;i<8;i++){int j=(i+1)%8;mesh.Face(bottom[i],top[i],top[j],bottom[j]);mesh.Face(p+Vector3.up*size.y*.32f,top[j],top[i]);}
  }
  public static void Wall(Transform root,M[] walls,ArenaCollision collision,Vector3 a,Vector3 b,float height,System.Random rng){
   collision.AddWall(root.TransformPoint(a),root.TransformPoint(b),.32f);
   float length=(b-a).magnitude;int cols=Mathf.CeilToInt(length/1.05f);bool alongX=Mathf.Abs(b.x-a.x)>Mathf.Abs(b.z-a.z);
   float notch=R(rng,.28f,.72f);Vector3 outward=alongX?new Vector3(0,0,Mathf.Sign(a.z)):new Vector3(Mathf.Sign(a.x),0,0);
   for(int k=0;k<cols;k++){
    float t=(k+.5f)/cols,collapse=Mathf.Max(0,1-Mathf.Abs(t-notch)/.3f);
    int courses=Mathf.Clamp(Mathf.RoundToInt(height/.4f-collapse*4.8f+R(rng,-1.2f,.8f)),2,8);
    for(int row=0;row<courses;row++){
     Vector3 p=Vector3.Lerp(a,b,t)+Vector3.up*(.2f+row*.4f)+outward*R(rng,-.045f,.045f);
     Vector3 size=alongX?new Vector3(length/cols-.045f,.37f,.64f):new Vector3(.64f,.37f,length/cols-.045f);
     Block(walls[rng.Next(walls.Length)],p,size,row==courses-1?R(rng,-9,9):R(rng,-2.5f,2.5f),rng);
    }
    // Fallen courses gather beneath the collapsed section, close to its solid footing.
    if(collapse>.25f||k%3==0){
     Vector3 at=Vector3.Lerp(a,b,t);for(int rock=0;rock<3;rock++){
      Vector3 p=at+outward*R(rng,.22f,.64f)+Vector3.up*R(rng,.09f,.18f);
      p+= (b-a).normalized*R(rng,-.22f,.22f);
      Block(walls[rng.Next(walls.Length)],p,new Vector3(R(rng,.35f,.65f),R(rng,.18f,.36f),R(rng,.32f,.6f)),R(rng,-45,45),rng);
     }
     if(collapse>.6f)Block(walls[rng.Next(walls.Length)],at+outward*.18f+Vector3.up*.36f,new Vector3(.6f,.65f,.6f),R(rng,-25,25),rng);
    }
   }
  }
 }
}
