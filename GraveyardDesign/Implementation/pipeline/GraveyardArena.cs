using System;using System.Collections.Generic;using System.Linq;using UnityEngine;
using M=Veilbound.RuinedTownArena.Meshes;
namespace Veilbound {
 public static class GraveyardArena {
  public static readonly List<TownInterior> Rooms=new List<TownInterior>();
  public static readonly List<Vector3> GravePositions=new List<Vector3>();
  public static readonly Vector3 CathedralCentre=new Vector3(-20,0,-21);
  public const float ShrineRadius=9.4f,CathedralWidth=19,CathedralDepth=20,BreachWidth=5.6f;
  static readonly Color Stone=new Color(.32f,.34f,.33f),Paving=new Color(.26f,.28f,.27f),Ash=new Color(.19f,.20f,.17f),Wood=new Color(.16f,.11f,.075f),Dark=new Color(.105f,.12f,.115f);
  static readonly Dictionary<Color,M> batches=new Dictionary<Color,M>();
  static M Batch(Color color){if(!batches.TryGetValue(color,out var mesh))batches[color]=mesh=new M();return mesh;}
  static Vector3 Polar(float a,float radius)=>new Vector3(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius);
  static void Floor(){
   Vector3 Point(int x,int z)=>new Vector3(x+Mathf.Sin(x*2.7f+z*.71f)*.7f,-.10f,z+Mathf.Sin(z*2.1f+x*.31f)*.7f);
   for(int x=-72;x<72;x+=6)for(int z=-72;z<72;z+=6){int tint=Math.Abs((x*13+z*7)/6)%3;var a=Point(x,z);var b=Point(x,z+6);var c=Point(x+6,z+6);var d=Point(x+6,z);Batch(Ash*(.97f+tint*.025f)).Face(a,b,c);Batch(Ash*(.98f+tint*.025f)).Face(a,c,d);}
   // Large paving pieces and only three quiet tones; no scattered gravel or busy textures.
   for(int row=-29;row<=29;row++)for(int column=-2;column<=2;column++){
    float along=row*2.1f,across=column*1.18f;var colour=Paving*(.97f+(Math.Abs(row+column)%3)*.03f);Vector3 ns=new Vector3(across,0,along),ew=new Vector3(along,0,across);
    if(ns.magnitude>ShrineRadius)Batch(colour).Box(ns,new Vector3(1.14f,.06f,2.02f));
    if(ew.magnitude>ShrineRadius&&Mathf.Abs(along)>3.1f)Batch(colour).Box(ew,new Vector3(2.02f,.06f,1.14f));
   }
   for(int ring=0;ring<3;ring++)for(int i=0;i<20;i++){
    float a=i*Mathf.PI/10+.004f,b=(i+1)*Mathf.PI/10-.004f,r0=ring*ShrineRadius/3,r1=(ring+1)*ShrineRadius/3-.04f;
    if(ring==0)Batch(Paving).Face(Vector3.up*.032f,Polar(b,r1)+Vector3.up*.032f,Polar(a,r1)+Vector3.up*.032f);else Batch(Paving*(1+ring*.035f)).Face(Polar(a,r0)+Vector3.up*.032f,Polar(b,r0)+Vector3.up*.032f,Polar(b,r1)+Vector3.up*.032f,Polar(a,r1)+Vector3.up*.032f);
   }
  }
  static void Gravestone(Vector3 p,int type,int seed,ArenaCollision collision){
   Vector2[] outline=type%3==0?new[]{new Vector2(-.43f,0),new Vector2(.43f,0),new Vector2(.43f,.75f),new Vector2(0,1.23f),new Vector2(-.43f,.75f)}:type%3==1?new[]{new Vector2(-.44f,0),new Vector2(.44f,0),new Vector2(.44f,.88f),new Vector2(.23f,1.14f),new Vector2(-.23f,1.14f),new Vector2(-.44f,.88f)}:new[]{new Vector2(-.40f,0),new Vector2(.40f,0),new Vector2(.40f,1.0f),new Vector2(.12f,1.07f),new Vector2(-.40f,.97f)};
   Quaternion q=Quaternion.Euler(0,(seed%3-1)*5,(seed%5-2)*2);var front=outline.Select(v=>p+q*new Vector3(v.x,v.y,-.17f)).ToArray();var back=outline.Select(v=>p+q*new Vector3(v.x,v.y,.17f)).ToArray();var mesh=Batch(Stone*(.94f+(type%3)*.045f));mesh.Face(front.Reverse().ToArray());mesh.Face(back);for(int i=0;i<front.Length;i++){int j=(i+1)%front.Length;mesh.Face(front[i],front[j],back[j],back[i]);}mesh.Box(p+Vector3.up*.055f,new Vector3(.99f,.11f,.58f));collision.Add(p,.52f);GravePositions.Add(p);
  }
  static void Plots(ArenaCollision collision){
   Vector3[] centres={new Vector3(-20,0,14),new Vector3(-22,0,23),new Vector3(-13,0,31),new Vector3(20,0,14),new Vector3(23,0,23),new Vector3(13,0,31),new Vector3(20,0,-13),new Vector3(26,0,-22),new Vector3(13,0,-31)};
   int seed=0;foreach(var centre in centres){Batch(Ash*.87f).Face(centre+new Vector3(-5,-.065f,-1.1f),centre+new Vector3(-5,-.065f,1.0f),centre+new Vector3(5,-.065f,1.0f),centre+new Vector3(5,-.065f,-1.1f));for(int i=-1;i<=1;i++)Gravestone(centre+Vector3.right*(i*3.8f),seed++%3,seed,collision);}
  }
  static void Tree(ArenaCollision collision,Vector3 p,float scale){var mesh=Batch(Wood*.7f);mesh.Branch(p,p+new Vector3(.2f,4.1f,0)*scale,.42f*scale,.16f*scale);mesh.Branch(p+Vector3.up*2*scale,p+new Vector3(-2.5f,4.2f,.3f)*scale,.23f*scale,.035f*scale);mesh.Branch(p+Vector3.up*3.0f*scale,p+new Vector3(2.2f,5.2f,-.3f)*scale,.18f*scale,.03f*scale);mesh.Branch(p+new Vector3(-1.5f,3.4f,.2f)*scale,p+new Vector3(-1.8f,4.8f,1.1f)*scale,.10f*scale,.025f*scale);collision.Add(p,.62f*scale);}
  static void Shrine(ArenaCollision collision){
   foreach(int quadrant in new[]{0,1,2,3}){
    float a=quadrant*Mathf.PI/2+Mathf.PI/4;Vector3 left=Polar(a-.15f,8.7f),right=Polar(a+.15f,8.7f);var stone=Batch(Stone*.88f);
    foreach(var p in new[]{left,right}){stone.Box(p+Vector3.up*.14f,new Vector3(.95f,.28f,.95f));stone.Branch(p+Vector3.up*.28f,p+Vector3.up*2.8f,.27f,.24f,6);stone.Box(p+Vector3.up*2.9f,new Vector3(.7f,.22f,.7f));collision.Add(p,.50f);}
    // Short broken lintels sit between diagonal pillars, never across the four road mouths.
    stone.Branch(left+Vector3.up*3.03f,right+Vector3.up*3.03f,.18f,.18f,4);
   }
   Vector3 altar=new Vector3(0,0,3.7f);Batch(Stone*.72f).Box(altar+Vector3.up*.25f,new Vector3(1.85f,.50f,1.5f));collision.Add(altar,1.1f);
   var red=Batch(new Color(.65f,.025f,.04f));red.Branch(altar+Vector3.up*.5f,altar+Vector3.up*2.3f,.38f,.018f,6);red.Branch(altar+new Vector3(.45f,.5f,.12f),altar+new Vector3(.8f,1.65f,.1f),.20f,.015f,5);
   // Three restrained rim accents, all off the cardinal travel lanes.
   foreach(var p in new[]{new Vector3(-7,0,6),new Vector3(-7,0,-6),new Vector3(7,0,-6)}){red.Branch(p,p+new Vector3(.15f,1.25f,.1f),.24f,.015f,5);collision.Add(p,.45f);}
  }
  static void Cathedral(Transform root,ArenaCollision collision){
   var g=WorldArt.Group("Ruined Cathedral - southwest",root);g.localPosition=CathedralCentre;var room=g.gameObject.AddComponent<TownInterior>();room.width=CathedralWidth;room.depth=CathedralDepth;Rooms.Add(room);float x=CathedralWidth/2,z=CathedralDepth/2,gap=BreachWidth/2;
   var walls=new[]{new M(),new M(),new M()};var rng=new System.Random(281028);
   void Wall(Vector3 a,Vector3 b){collision.AddWall(g.TransformPoint(a),g.TransformPoint(b),.32f);int columns=Mathf.CeilToInt((b-a).magnitude/1.15f);bool horizontal=Mathf.Abs(b.x-a.x)>Mathf.Abs(b.z-a.z);for(int i=0;i<columns;i++){int courses=5+rng.Next(3);for(int row=0;row<courses;row++)walls[rng.Next(3)].Box(Vector3.Lerp(a,b,(i+.5f)/columns)+Vector3.up*(.22f+row*.44f),horizontal?new Vector3((b-a).magnitude/columns-.025f,.415f,.64f):new Vector3(.64f,.415f,(b-a).magnitude/columns-.025f));}}
   foreach(int side in new[]{-1,1}){float zz=side*z;Wall(new Vector3(-x,0,zz),new Vector3(-gap,0,zz));Wall(new Vector3(gap,0,zz),new Vector3(x,0,zz));float xx=side*x;Wall(new Vector3(xx,0,-z),new Vector3(xx,0,-4-gap));Wall(new Vector3(xx,0,-4+gap),new Vector3(xx,0,z));}
   var cutaway=new List<Renderer>();for(int i=0;i<3;i++)cutaway.Add(walls[i].Finish(g,"Broad broken cathedral masonry "+i,Stone*(.80f+i*.08f)));
   var floor=new M();for(int a=0;a<9;a++)for(int b=0;b<10;b++)floor.Box(new Vector3(-x+(a+.5f)*CathedralWidth/9,0,-z+(b+.5f)*2),new Vector3(CathedralWidth/9-.035f,.065f,1.96f));floor.Finish(g,"Quiet cathedral flagstone floor",Paving*.93f);
   var details=new M();
   foreach(int side in new[]{-1,1})foreach(float zz in new[]{3.4f,7.1f}){float xx=side*x;details.Box(new Vector3(xx,2.4f,zz-1.05f),new Vector3(.40f,4.8f,.40f));details.Box(new Vector3(xx,2.4f,zz+1.05f),new Vector3(.40f,4.8f,.40f));details.Branch(new Vector3(xx,4.8f,zz-1.05f),new Vector3(xx,5.8f,zz),.20f,.16f,4);details.Branch(new Vector3(xx,5.8f,zz),new Vector3(xx,4.8f,zz+1.05f),.16f,.20f,4);}
   // A single surviving gable defines the cathedral; no roof covers the combat floor.
   details.Box(new Vector3(-x+1.1f,3,z-.25f),new Vector3(.75f,6,.75f));details.Box(new Vector3(-x+3.1f,2.8f,z-.25f),new Vector3(.65f,5.6f,.75f));details.Branch(new Vector3(-x+.9f,6,z-.25f),new Vector3(-x+2.1f,7,z-.25f),.28f,.22f,4);details.Branch(new Vector3(-x+2.1f,7,z-.25f),new Vector3(-x+3.3f,5.6f,z-.25f),.22f,.28f,4);cutaway.Add(details.Finish(g,"Simplified pointed windows and ruined bell gable",Stone*.91f));
   var furniture=new M();foreach(int side in new[]{-1,1})foreach(float zz in new[]{2.2f,5.8f}){Vector3 p=new Vector3(side*5.8f,.50f,zz);furniture.Box(p,new Vector3(2.2f,.28f,.7f));furniture.Box(p+new Vector3(0,.40f,.3f),new Vector3(2.2f,.8f,.13f));collision.Add(g.TransformPoint(p),1.25f);}furniture.Finish(g,"Four broken pews at the nave sides",Wood);
   var altar=new M();altar.Box(new Vector3(-5.8f,.6f,8.2f),new Vector3(2.1f,1.2f,1.25f));altar.Finish(g,"Ruined side altar",Stone*.73f);collision.Add(g.TransformPoint(new Vector3(-5.8f,0,8.2f)),1.18f);
   var steps=new M();for(int i=0;i<3;i++)steps.Box(new Vector3(x+.35f+i*.55f,-.025f-i*.01f,-4),new Vector3(.52f,.09f,BreachWidth-.2f));steps.Finish(g,"Low collapsed east entrance steps",Stone*.82f);
   // A few broad rubble blocks flank the entrances, never carpeting or closing the floor.
   var rubble=new M();foreach(int side in new[]{-1,1})foreach(int flank in new[]{-1,1})rubble.Box(new Vector3(flank*(gap+.5f),.12f,side*(z+.25f)),new Vector3(.6f,.24f,.6f));rubble.Finish(g,"Entrance side rubble only",Stone*.79f);room.walls=cutaway.ToArray();
  }
  public static void Build(Transform root,ArenaCollision collision){
   collision.Radius=50;collision.PlayerRadius=42;root.gameObject.AddComponent<VillageMeshOwner>();Rooms.Clear();GravePositions.Clear();batches.Clear();Floor();Plots(collision);Shrine(collision);Cathedral(root,collision);Tree(collision,new Vector3(-32,0,19),1.1f);Tree(collision,new Vector3(9,0,36),.95f);
   for(int i=0;i<8;i++){float a=(i+.5f)*Mathf.PI/4;Tree(collision,Polar(a,46),.85f);}
   foreach(var pair in batches)pair.Value.Finish(root,"Batched Graveyard scenery "+pair.Key,pair.Key);batches.Clear();
  }
 }
}

