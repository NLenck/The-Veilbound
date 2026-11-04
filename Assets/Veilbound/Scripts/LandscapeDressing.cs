using System;using System.Collections.Generic;using UnityEngine;
using M=Veilbound.RuinedTownArena.Meshes;
namespace Veilbound {
 // Seeded, material-batched ground cover. Tiny soft props deliberately have no collision.
 public static class LandscapeDressing {
  static readonly Color[] Palette={new Color(.205f,.218f,.19f),new Color(.26f,.275f,.25f),new Color(.18f,.185f,.105f),new Color(.24f,.225f,.125f),new Color(.29f,.255f,.14f),new Color(.165f,.155f,.105f),new Color(.19f,.185f,.12f)};
  static float R(System.Random r,float a,float b)=>Mathf.Lerp(a,b,(float)r.NextDouble());
  static Vector3 Rad(float a,float r)=>new Vector3(Mathf.Cos(a)*r,0,Mathf.Sin(a)*r);
  static void Pebble(M m,Vector3 p,float width,float height,float angle,System.Random rng){
   var q=Quaternion.Euler(0,angle,0);var bottom=new Vector3[6];var shoulder=new Vector3[6];
   for(int i=0;i<6;i++){float a=i*Mathf.PI/3;var v=Rad(a,width*R(rng,.78f,1));v.z*=.72f;bottom[i]=p+q*v;shoulder[i]=p+q*(v*.7f+Vector3.up*height*.65f);}
   Vector3 tip=p+q*new Vector3(width*.12f,height,-width*.08f);
   for(int i=0;i<6;i++){int j=(i+1)%6;m.Face(bottom[i],shoulder[i],shoulder[j],bottom[j]);m.Face(shoulder[i],tip,shoulder[j]);}
  }
  static void Patch(M m,Vector3 p,float radius,System.Random rng){
   var ring=new Vector3[9];for(int i=0;i<9;i++)ring[i]=p+Rad(i*Mathf.PI*2/9,radius*R(rng,.65f,1));
   for(int i=0;i<9;i++)m.Face(p,ring[(i+1)%9],ring[i]);
  }
  static void Grass(M m,Vector3 p,System.Random rng){
   for(int i=0;i<5;i++){float a=R(rng,0,Mathf.PI*2),h=R(rng,.15f,.34f);Vector3 side=Rad(a,.035f),tip=p+Vector3.up*h+Rad(a+.6f,h*.4f);m.Face(p-side,tip,p+side);m.Face(p+side,tip,p-side);}
  }
  public static void Build(Transform root,ArenaCollision collision,bool village){
   var group=WorldArt.Group(village?"Village landscape - rocks and scrub":"Shrine landscape - moss and heath",root);
   var rng=new System.Random(village?51026:61026);var mesh=new M[Palette.Length];for(int i=0;i<mesh.Length;i++)mesh[i]=new M();
   var rooms=village?RuinedTownArena.Rooms:GraveyardArena.Rooms;var placed=new List<Vector3>();
   bool Allowed(Vector3 p,float margin){
    if(p.magnitude< (village?8:11.5f)||p.magnitude>collision.PlayerRadius-1.5f)return false;
    if(Mathf.Abs(p.x)<(village?7.4f:4.4f)||Mathf.Abs(p.z)<(village?3.6f:4.4f))return false;
    foreach(var room in rooms){Vector3 local=room.transform.InverseTransformPoint(root.TransformPoint(p));if(Mathf.Abs(local.x)<room.width*.5f+margin&&Mathf.Abs(local.z)<room.depth*.5f+margin)return false;}
    return collision.Free(root.TransformPoint(p),margin);
   }
   int target=village?76:58;
   for(int attempt=0;attempt<2400&&placed.Count<target;attempt++){
    Vector3 p=Rad(R(rng,0,Mathf.PI*2),Mathf.Sqrt(R(rng,0,1))*collision.PlayerRadius);
    if(!Allowed(p,1.3f)||placed.Exists(v=>(v-p).sqrMagnitude<10))continue;placed.Add(p);
    // Broad, irregular muted soil/moss shapes tie each small cluster into the ground.
    Patch(mesh[5+placed.Count%2],p+Vector3.up*(-.074f+placed.Count*.00008f),R(rng,.85f,1.5f),rng);
    for(int i=0;i<3;i++){
     Vector3 at=p+Rad(R(rng,0,Mathf.PI*2),R(rng,.15f,.85f));
     if(!Allowed(at,.55f))continue;
     if(i==0||placed.Count%3==0){float w=R(rng,.16f,.42f);Pebble(mesh[placed.Count%2],at+Vector3.down*.065f,w,R(rng,.12f,.27f),R(rng,0,360),rng);}
     else Grass(mesh[2+placed.Count%3],at+Vector3.down*.065f,rng);
    }
    if(placed.Count%3==1){
     // Three angular leaf masses form one low woody shrub, rather than smooth spheres.
     for(int leaf=0;leaf<3;leaf++)Pebble(mesh[2+leaf%2],p+Rad(leaf*Mathf.PI*2/3,.21f)+Vector3.up*.01f,R(rng,.25f,.38f),R(rng,.32f,.52f),leaf*71,rng);
    }
   }
   // Grounded faceted banks sit beyond the player boundary and fade into the Veil.
   for(int i=0;i<18;i++){
    float a=(i+.35f)*Mathf.PI*2/18;Vector3 p=Rad(a,collision.PlayerRadius+R(rng,5,9));
    if(Mathf.Abs(p.x)<10||Mathf.Abs(p.z)<7)continue;
    Pebble(mesh[6],p+Vector3.down*.15f,R(rng,3,5.5f),R(rng,.6f,1.3f),R(rng,0,360),rng);
    Pebble(mesh[0],p+Rad(a+.8f,2),R(rng,.7f,1.4f),R(rng,.35f,.75f),i*37,rng);
   }
   for(int i=0;i<mesh.Length;i++)mesh[i].Finish(group,"Batched faceted landscape "+i,Palette[i]);
   LandscapeWeathering.Build(root,collision,village);
  }
 }
}
