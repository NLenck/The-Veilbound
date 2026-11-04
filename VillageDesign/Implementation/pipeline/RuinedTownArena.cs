using System;using System.Collections.Generic;using UnityEngine;
namespace Veilbound {
 public static class RuinedTownArena {
  public static readonly List<TownInterior> Rooms=new List<TownInterior>();
  public const float VillagePlayerRadius=52.5f,VillageSpawnRadius=62.5f,Gap=4.8f;
  static readonly Color Stone=new Color(.32f,.32f,.29f),Wood=new Color(.22f,.135f,.075f),Dark=new Color(.10f,.085f,.07f),Red=new Color(.30f,.045f,.035f);
  static readonly Dictionary<Color,Material> mats=new Dictionary<Color,Material>();
  static readonly Dictionary<Color,Meshes> scenery=new Dictionary<Color,Meshes>();
  static Material Mat(Color c){if(mats.TryGetValue(c,out var m)&&m)return m;m=new Material(Shader.Find("Veilbound/TownSurface")){color=c,name="Village matte "+c};if(c.r>.16f&&c.g<.04f)m.SetColor("_Emission",new Color(c.r*.65f,c.g*.65f,c.b*.65f));m.enableInstancing=true;return mats[c]=m;}
  public sealed class Meshes {
   readonly List<Vector3> v=new List<Vector3>();readonly List<int> t=new List<int>();
   public void Face(params Vector3[] p){int n=v.Count;v.AddRange(p);for(int i=1;i<p.Length-1;i++)t.AddRange(new[]{n,n+i,n+i+1});}
   public void Box(Vector3 c,Vector3 size){Vector3 a=c-size*.5f,b=c+size*.5f;Face(new Vector3(a.x,a.y,a.z),new Vector3(a.x,b.y,a.z),new Vector3(b.x,b.y,a.z),new Vector3(b.x,a.y,a.z));Face(new Vector3(b.x,a.y,b.z),new Vector3(b.x,b.y,b.z),new Vector3(a.x,b.y,b.z),new Vector3(a.x,a.y,b.z));Face(new Vector3(a.x,a.y,b.z),new Vector3(a.x,b.y,b.z),new Vector3(a.x,b.y,a.z),new Vector3(a.x,a.y,a.z));Face(new Vector3(b.x,a.y,a.z),new Vector3(b.x,b.y,a.z),new Vector3(b.x,b.y,b.z),new Vector3(b.x,a.y,b.z));Face(new Vector3(a.x,b.y,a.z),new Vector3(a.x,b.y,b.z),new Vector3(b.x,b.y,b.z),new Vector3(b.x,b.y,a.z));Face(new Vector3(a.x,a.y,b.z),new Vector3(a.x,a.y,a.z),new Vector3(b.x,a.y,a.z),new Vector3(b.x,a.y,b.z));}
   public void Branch(Vector3 a,Vector3 b,float ra,float rb,int sides=6){Vector3 d=(b-a).normalized,u=Vector3.Cross(d,Mathf.Abs(d.y)>.9f?Vector3.right:Vector3.up).normalized,w=Vector3.Cross(d,u);for(int i=0;i<sides;i++){float p=i*Mathf.PI*2/sides,q=(i+1)*Mathf.PI*2/sides;Vector3 x=u*Mathf.Cos(p)+w*Mathf.Sin(p),y=u*Mathf.Cos(q)+w*Mathf.Sin(q);Face(a+x*ra,a+y*ra,b+y*rb,b+x*rb);Face(a,a+y*ra,a+x*ra);Face(b,b+x*rb,b+y*rb);}}
   public Renderer Finish(Transform parent,string name,Color c){var mesh=new Mesh{name=name};if(v.Count>65000)mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;mesh.SetVertices(v);mesh.SetTriangles(t,0);mesh.RecalculateNormals();mesh.RecalculateBounds();var owner=parent.GetComponentInParent<VillageMeshOwner>();if(owner)owner.meshes.Add(mesh);var go=new GameObject(name);go.transform.SetParent(parent,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;var r=go.AddComponent<MeshRenderer>();r.sharedMaterial=Mat(c);return r;}
  }
  static Meshes Batch(Color c){if(!scenery.TryGetValue(c,out var m))scenery[c]=m=new Meshes();return m;}
  static float Rand(System.Random r,float a,float b)=>Mathf.Lerp(a,b,(float)r.NextDouble());
  static Renderer Box(Transform root,string name,Vector3 at,Vector3 size,Color c){var m=new Meshes();m.Box(at,size);return m.Finish(root,name,c);}
  static void Ground(Transform root){
   var rng=new System.Random(2806);var earth=new Meshes[5];for(int i=0;i<5;i++)earth[i]=new Meshes();
   for(int x=-78;x<78;x+=4)for(int z=-78;z<78;z+=4){int i=rng.Next(5);float y=-.10f;Vector3 Point(int a,int b){float n=Mathf.Sin(a*17.1f+b*41.7f);return new Vector3(a+n*.7f,y,b+Mathf.Sin(a*31.3f+b*11.2f)*.7f);}var a0=Point(x,z);var b0=Point(x,z+4);var c0=Point(x+4,z+4);var d0=Point(x+4,z);earth[i].Face(a0,b0,c0);earth[rng.Next(5)].Face(a0,c0,d0);}
   for(int i=0;i<5;i++)earth[i].Finish(root,"Faceted ash and sparse moss "+i,new Color(.175f+i*.004f,.172f+i*.004f,.135f+i*.003f));
   for(float x=-65;x<65;x+=1.25f)for(float z=-65;z<65;z+=1.1f){if(Mathf.Abs(x)>5.9f&&Mathf.Abs(z)>2.1f)continue;if(x*x+z*z>67*67)continue;Batch(Stone*(.68f+rng.Next(4)*.07f)).Box(new Vector3(x,.005f,z),new Vector3(1.18f,.09f,1.02f));}
  }
  static void Wall(Transform root,Meshes[] walls,ArenaCollision c,Vector3 a,Vector3 b,float height,System.Random rng){
   c.AddWall(root.TransformPoint(a),root.TransformPoint(b),.32f);float length=(b-a).magnitude;int cols=Mathf.CeilToInt(length/1.05f);bool alongX=Mathf.Abs(b.x-a.x)>Mathf.Abs(b.z-a.z);
   for(int k=0;k<cols;k++){int courses=Mathf.Max(2,Mathf.RoundToInt((height+Rand(rng,-.42f,.32f))/.4f));for(int row=0;row<courses;row++){
    Vector3 p=Vector3.Lerp(a,b,(k+.5f)/cols)+Vector3.up*(.2f+row*.4f);walls[rng.Next(walls.Length)].Box(p,alongX?new Vector3(length/cols-.035f,.365f,.64f):new Vector3(.64f,.365f,length/cols-.035f));
   }}
  }
  static void BuildRoom(Transform root,ArenaCollision c,string name,Vector3 at,float width,float depth,int type,int seed){
   var g=WorldArt.Group(name,root);g.localPosition=at;var room=g.gameObject.AddComponent<TownInterior>();room.width=width;room.depth=depth;Rooms.Add(room);var rng=new System.Random(seed);float x=width/2,z=depth/2,gap=Gap/2;
   var floor=new Meshes();for(float a=-x+.48f;a<x;a+=.95f)for(float b=-z+.5f;b<z;b+=1.0f)floor.Box(new Vector3(a,-.015f,b),type==1?new Vector3(.91f,.08f,.96f):new Vector3(.91f,.08f,.98f));floor.Finish(g,type==1?"Church stone nave floor":"Broken low-poly floor",type==1?Stone*.63f:Wood*1.25f);
   var masonry=new Meshes[4];for(int i=0;i<4;i++)masonry[i]=new Meshes();
   // Four wide breaches: north/south form an uninterrupted cross-building route.
   foreach(int side in new[]{-1,1}){
    float zz=side*z;Wall(g,masonry,c,new Vector3(-x,0,zz),new Vector3(-gap,0,zz),2.5f,rng);Wall(g,masonry,c,new Vector3(gap,0,zz),new Vector3(x,0,zz),2.7f,rng);
    float xx=side*x;Wall(g,masonry,c,new Vector3(xx,0,-z),new Vector3(xx,0,-gap),2.5f,rng);Wall(g,masonry,c,new Vector3(xx,0,gap),new Vector3(xx,0,z),2.8f,rng);
   }
   var cutaway=new List<Renderer>();for(int i=0;i<4;i++)cutaway.Add(masonry[i].Finish(g,"Crumbled cutaway stone "+i,Stone*(.80f+i*.09f)));
   // Rubble flanks the openings; no threshold, lintel, collider or debris crosses them.
   var rubble=new Meshes();foreach(int side in new[]{-1,1})foreach(int flank in new[]{-1,1}){rubble.Box(new Vector3(flank*(gap+.55f),.16f,side*(z+.35f)),new Vector3(.55f,.32f,.65f));rubble.Box(new Vector3(side*(x+.35f),.13f,flank*(gap+.55f)),new Vector3(.7f,.26f,.5f));}rubble.Finish(g,"Low rubble at breach sides",Stone*.75f);
   var props=new Meshes();var trim=new Meshes();
   var timbers=new Meshes();foreach(int sx in new[]{-1,1})foreach(int sz in new[]{-1,1}){Vector3 p=new Vector3(sx*(x-.25f),0,sz*(z-.25f));timbers.Box(p+Vector3.up*1.7f,new Vector3(.19f,3.4f,.19f));timbers.Branch(p+Vector3.up*3.2f,p+new Vector3(-sx*1.8f,4.3f,-sz*.25f),.12f,.07f,4);}cutaway.Add(timbers.Finish(g,"Charred surviving roof timbers",Wood*.6f));
   void Prop(Vector3 p,Vector3 size,Color color,bool solid=true){(color==Wood?props:trim).Box(p,size);if(solid)c.Add(g.TransformPoint(new Vector3(p.x,0,p.z)),Mathf.Max(size.x,size.z)*.5f+.10f);}
   if(type==1){
    // Side pews leave the broad central nave clear for the largest enemy body.
    for(float zz=-z+4;zz<z-4;zz+=3.1f)if(Mathf.Abs(zz)>3.6f)foreach(int side in new[]{-1,1}){float xx=side*(x-1.8f);Prop(new Vector3(xx,.48f,zz),new Vector3(1.65f,.28f,.65f),Wood);props.Box(new Vector3(xx,.95f,zz+.3f),new Vector3(1.65f,.75f,.12f));}
    Prop(new Vector3(x-1.8f,.85f,z-2.3f),new Vector3(1.6f,1.5f,2.1f),Stone);
    var arches=new Meshes();foreach(int side in new[]{-1,1}){float xx=side*x;arches.Branch(new Vector3(xx,2.5f,z-3.8f),new Vector3(xx,5.1f,z-2.5f),.18f,.16f);arches.Branch(new Vector3(xx,5.1f,z-2.5f),new Vector3(xx,2.7f,z-1.2f),.16f,.18f);}
    float bx=-x+1.4f,bz=z-.3f;arches.Box(new Vector3(bx-1.0f,4.0f,bz),new Vector3(.65f,4.1f,.9f));arches.Box(new Vector3(bx+1.0f,3.9f,bz),new Vector3(.65f,3.9f,.9f));arches.Branch(new Vector3(bx-1.2f,6,bz),new Vector3(bx,7.4f,bz),.40f,.30f);arches.Branch(new Vector3(bx,7.4f,bz),new Vector3(bx+1.2f,6,bz),.30f,.4f);cutaway.Add(arches.Finish(g,"Broken gothic windows and bell gable",Stone*.86f));
    trim.Branch(new Vector3(bx,5.4f,bz),new Vector3(bx,5.9f,bz),.45f,.24f,6);
   }else if(type==2){
    // Raised civic dais sits to one side; the north/south axis remains ground-level.
    Prop(new Vector3(-x+2,.20f,z-2),new Vector3(3.1f,.4f,2.5f),Wood);Prop(new Vector3(-x+2,.90f,z-2),new Vector3(.8f,1.4f,.8f),Wood);
    foreach(int side in new[]{-1,1})for(int i=0;i<2;i++)Prop(new Vector3(side*(x-1.5f),.55f,-z+3+i*(depth-6)),new Vector3(1.6f,.4f,1.6f),Wood);
    var banners=new Meshes();foreach(int side in new[]{-1,1})banners.Face(new Vector3(side*(x-.3f),3.9f,z-1.5f),new Vector3(side*(x-.3f),3.9f,z-3.1f),new Vector3(side*(x-.3f),1.7f,z-2.9f),new Vector3(side*(x-.3f),2.1f,z-2.4f));cutaway.Add(banners.Finish(g,"Torn civic banners",Red));
   }else if(type==3){
    Prop(new Vector3(-x+1.7f,.55f,z-3.2f),new Vector3(1.35f,1.1f,4.1f),Wood);
    foreach(int side in new[]{-1,1}){Prop(new Vector3(side*(x-2.0f),.65f,-z+2.4f),new Vector3(2.1f,.3f,1.5f),Wood);}
    for(int i=0;i<3;i++)Barrel(g,c,new Vector3(x-1.4f,0,z-1.4f-i*1.3f),false);
   }else{
    Prop(new Vector3(-x+1.3f,.35f,z-1.8f),new Vector3(1.4f,.7f,2.2f),Wood);trim.Box(new Vector3(-x+1.3f,.73f,z-1.8f),new Vector3(1.3f,.08f,1.6f));
    Prop(new Vector3(x-1.3f,.6f,-z+1.5f),new Vector3(1.2f,1.2f,1.25f),Wood);
   }
   props.Finish(g,type==3?"Ruined tavern bar and tables":type==2?"Civic benches and dais":type==1?"Broken pews":"Ruined domestic furniture",Wood);trim.Finish(g,"Weathered interior accents",type==1?Dark:type==0?Red:Stone*.7f);
   // A recessed hearth never blocks the central corridor.
   var hearth=new Meshes();hearth.Box(new Vector3(x-1.1f,.25f,z-1.4f),new Vector3(1.25f,.50f,1.1f));hearth.Box(new Vector3(x-.55f,1.45f,z-1.4f),new Vector3(.65f,2.9f,1.1f));cutaway.Add(hearth.Finish(g,"Broken soot-dark hearth",Stone*.55f));c.Add(g.TransformPoint(new Vector3(x-.8f,0,z-1.4f)),.9f);
   room.walls=cutaway.ToArray();
  }
  static void Barrel(Transform root,ArenaCollision c,Vector3 p,bool global){
   var wood=global?Batch(Wood*.8f):new Meshes();var bands=global?Batch(Dark):new Meshes();wood.Branch(p+Vector3.up*.08f,p+Vector3.up*.55f,.42f,.52f,8);wood.Branch(p+Vector3.up*.55f,p+Vector3.up*1.05f,.52f,.42f,8);foreach(float y in new[]{.22f,.82f})bands.Branch(p+Vector3.up*y,p+Vector3.up*(y+.09f),.48f,.49f,8);if(!global){wood.Finish(root,"Tavern oak barrel",Wood*.8f);bands.Finish(root,"Barrel dark hoops",Dark);}c.Add(root.TransformPoint(p),.56f);
  }
  static void Tree(Transform root,ArenaCollision c,Vector3 p,float scale){
   var mesh=Batch(Wood*.64f);mesh.Branch(p,p+new Vector3(.25f,4.3f,0)*scale,.55f*scale,.22f*scale);mesh.Branch(p+Vector3.up*2.1f*scale,p+new Vector3(-2.8f,4.3f,.6f)*scale,.3f*scale,.07f*scale);mesh.Branch(p+new Vector3(.1f,3.3f,0)*scale,p+new Vector3(2.7f,5.5f,.1f)*scale,.24f*scale,.045f*scale);mesh.Branch(p+new Vector3(-1.7f,3.6f,.4f)*scale,p+new Vector3(-1.8f,5.3f,1.1f)*scale,.15f*scale,.035f*scale);mesh.Branch(p+new Vector3(1.7f,4.8f,0)*scale,p+new Vector3(3,5.0f,-1.2f)*scale,.13f*scale,.035f*scale);c.Add(p,.75f*scale);
  }
  static void Crystal(ArenaCollision c,Vector3 p,int seed){var rng=new System.Random(seed);for(int i=0;i<5;i++){Vector3 q=p+new Vector3(Rand(rng,-.65f,.65f),.1f,Rand(rng,-.65f,.65f));float h=Rand(rng,1.6f,3.6f);Vector3 tip=q+new Vector3(Rand(rng,-.4f,.4f),h,Rand(rng,-.4f,.4f));Batch(i%2==0?new Color(.48f,.025f,.04f):new Color(.22f,.018f,.025f)).Branch(q,tip,.32f,0,5);}c.Add(p,1.1f);}
  public static void Build(Transform root,ArenaCollision c){
   c.Radius=VillageSpawnRadius;c.PlayerRadius=VillagePlayerRadius;root.gameObject.AddComponent<VillageMeshOwner>();Rooms.Clear();scenery.Clear();Ground(root);
   BuildRoom(root,c,"House northwest upper",new Vector3(-15.5f,0,29),10,10,0,101);
   BuildRoom(root,c,"House northwest lower",new Vector3(-15.5f,0,13),10,10,0,102);
   BuildRoom(root,c,"Ruined Church",new Vector3(14,0,21),12,27,1,103);
   BuildRoom(root,c,"House northeast",new Vector3(36,0,12),11,10,0,104);
   BuildRoom(root,c,"Village Hall",new Vector3(-21,0,-12),23,16,2,105);
   BuildRoom(root,c,"Abandoned Tavern",new Vector3(-21,0,-33),23,14,3,106);
   BuildRoom(root,c,"House southeast upper",new Vector3(14,0,-12),12,16,0,107);
   BuildRoom(root,c,"House southeast lower",new Vector3(15,0,-33),13,13,0,108);
   Tree(root,c,new Vector3(-37,0,23),1.35f);Tree(root,c,new Vector3(30,0,27),1.0f);Tree(root,c,new Vector3(34,0,-26),1.35f);
   for(int i=0;i<20;i++){float a=i*Mathf.PI*2/20;Vector3 p=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*(57.5f+(i%3));if(Mathf.Abs(p.x)<7||Mathf.Abs(p.z)<4)continue;Tree(root,c,p,.8f+(i%3)*.13f);}
   foreach(var p in new[]{new Vector3(-39,0,20),new Vector3(8,0,35),new Vector3(6.8f,0,17),new Vector3(43,0,7),new Vector3(-35,0,-5),new Vector3(-29,0,-22),new Vector3(-14,0,-22),new Vector3(7,0,-21),new Vector3(35,0,-29),new Vector3(45,0,-15),new Vector3(-46,0,-10)})Crystal(c,p,300+(int)p.x);
   foreach(var p in new[]{new Vector3(-10,0,21),new Vector3(-11.3f,0,21),new Vector3(-34.7f,0,-29),new Vector3(-35,0,-31),new Vector3(-8,0,-39)})Barrel(root,c,p,true);
   foreach(var pair in scenery)pair.Value.Finish(root,"Batched low-poly village scenery "+pair.Key,pair.Key);scenery.Clear();
  }
 }
}



