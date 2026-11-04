using System;
using System.Collections.Generic;
using UnityEngine;
namespace Veilbound {
 public static class SanctuaryArena {
  public const float FloorRadius=54;
  static readonly Color Ash=new Color(.245f,.255f,.235f),Iron=new Color(.135f,.15f,.14f),Bronze=new Color(.38f,.27f,.15f);
  static readonly Dictionary<Color,Material> painted=new Dictionary<Color,Material>();
  static Material Painted(Color color,bool terrain=false){Color key=new Color(color.r,color.g,color.b,terrain?0:1);if(painted.TryGetValue(key,out var m)&&m)return m;var shader=Shader.Find("Veilbound/EnvironmentSurface");m=shader?new Material(shader):new Material(WorldArt.Mat(color));m.color=color;if(shader)m.SetFloat("_Terrain",terrain?1:0);m.enableInstancing=true;painted[key]=m;return m;}
  sealed class Batch {
   public readonly List<Vector3> v=new List<Vector3>();public readonly List<int> t=new List<int>();
   public void Face(Vector3[] p){int n=v.Count;v.AddRange(p);for(int i=1;i<p.Length-1;i++)t.AddRange(new[]{n,n+i,n+i+1});}
   public Transform Finish(Transform root,string name,Color color){var mesh=new Mesh{name=name};mesh.SetVertices(v);mesh.SetTriangles(t,0);mesh.RecalculateNormals();mesh.RecalculateBounds();var g=WorldArt.Group(name,root);g.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;g.gameObject.AddComponent<MeshRenderer>().sharedMaterial=WorldArt.Mat(color);return g;}
  }
  static float R(System.Random rng,float a,float b)=>Mathf.Lerp(a,b,(float)rng.NextDouble());
  static Vector3 At(float angle,float radius)=>new Vector3(Mathf.Cos(angle)*radius,0,Mathf.Sin(angle)*radius);
  static void Slab(Batch b,Vector3 p,float hx,float hz,float cut,float rotation,System.Random rng){
   Vector2[] outline={new Vector2(-hx+cut,-hz),new Vector2(hx-cut,-hz),new Vector2(hx,-hz+cut),new Vector2(hx,hz-cut),new Vector2(hx-cut,hz),new Vector2(-hx+cut,hz),new Vector2(-hx,hz-cut),new Vector2(-hx,-hz+cut)};
   var q=Quaternion.Euler(0,rotation,0);Vector3[] top=new Vector3[8],edge=new Vector3[8];
   for(int i=0;i<8;i++){var point=outline[i]+new Vector2(R(rng,-.028f,.028f),R(rng,-.028f,.028f));top[i]=p+q*new Vector3(point.x*.97f,.008f,point.y*.97f);edge[i]=p+q*new Vector3(point.x,-.065f,point.y);}
   for(int i=0;i<8;i++){int j=(i+1)%8;b.Face(new[]{p+Vector3.up*.008f,top[j],top[i]});b.Face(new[]{top[i],top[j],edge[j],edge[i]});}
  }
  static void Floor(Transform root){
   WorldArt.Part(root,"Dark earth beneath the sanctuary ruins",PrimitiveType.Cylinder,new Vector3(0,-.20f,0),new Vector3(160,.12f,160),new Color(.14f,.145f,.105f));
   var rng=new System.Random(4187);var batches=new Batch[7];for(int i=0;i<batches.Length;i++)batches[i]=new Batch();
   var patches=new Batch[5];for(int i=0;i<patches.Length;i++)patches[i]=new Batch();
   for(int patch=0;patch<45;patch++){Vector3 c=At(R(rng,0,Mathf.PI*2),R(rng,7,51))+Vector3.up*(-.078f+patch*.0006f);float radius=R(rng,4,10);var ring=new Vector3[13];for(int i=0;i<ring.Length;i++)ring[i]=c+At(i*Mathf.PI*2/ring.Length,radius*R(rng,.65f,1));for(int i=0;i<ring.Length;i++)patches[patch%5].Face(new[]{c,ring[(i+1)%ring.Length],ring[i]});}
   Color[] soils={new Color(.145f,.17f,.095f),new Color(.18f,.18f,.12f),new Color(.12f,.15f,.08f),new Color(.205f,.185f,.125f),new Color(.16f,.165f,.115f)};
   for(int i=0;i<5;i++)patches[i].Finish(root,"Broad earth and moss field "+i,soils[i]);
   for(float z=-54;z<54;z+=1.12f){float x=-54+R(rng,-.7f,.7f);while(x<54){float width=R(rng,.78f,1.7f);Vector3 p=new Vector3(x+width*.5f,-.018f,z);x+=width+.045f;float radius=p.magnitude;if(radius>FloorRadius)continue;
    bool centre=radius<9.8f,road=Mathf.Min(Mathf.Abs(p.x),Mathf.Abs(p.z))<2.5f,ring=radius>21&&radius<23.6f;
    if(!centre&&!road&&!ring)continue;if(!centre&&rng.NextDouble()<.12)continue;
    Slab(batches[rng.Next(batches.Length)],p,width*.49f,.535f,R(rng,.055f,.15f),R(rng,-1.2f,1.2f),rng);
   }
   }
   for(int i=0;i<batches.Length;i++){float f=.86f+i*.045f;batches[i].Finish(root,"Batched hand-cut flagstones "+i,Ash*f);}
   // Inlaid iron and bronze, never glowing circles that could be mistaken for attack warnings.
   var inlay=new Batch();for(int i=0;i<96;i++){float a=i*Mathf.PI*2/96,b=(i+1)*Mathf.PI*2/96;inlay.Face(new[]{At(a,5.85f)+Vector3.up*.008f,At(a,5.80f)+Vector3.up*.008f,At(b,5.80f)+Vector3.up*.008f,At(b,5.85f)+Vector3.up*.008f});}
   for(int i=0;i<8;i++){float a=i*Mathf.PI/4;var q=Quaternion.Euler(0,-a*Mathf.Rad2Deg,0);Vector3[] diamond={new Vector3(0,.009f,1.4f),new Vector3(-.13f,.009f,2.9f),new Vector3(0,.009f,i%2==0?4.6f:3.7f),new Vector3(.13f,.009f,2.9f)};for(int j=0;j<diamond.Length;j++)diamond[j]=q*diamond[j];inlay.Face(diamond);}
   inlay.Finish(root,"Worn sanctuary compass inlay",Bronze*.78f);
  }
  static Transform Block(Transform parent,string name,Vector3 p,Vector3 scale,Color c,float angle=0){var t=WorldArt.Part(parent,name,PrimitiveType.Cube,p,scale,c);t.localRotation=Quaternion.Euler(0,angle,0);return t;}
  static Transform Column(Transform root,Vector3 p,float height,float radius,int seed){
   var rng=new System.Random(seed);var group=WorldArt.Group("Weathered octagonal column",root);group.localPosition=p;
   Block(group,"Chipped square footing",new Vector3(0,.16f,0),new Vector3(radius*3,.32f,radius*3),Ash*.82f,4);
   Block(group,"Worn foot moulding",new Vector3(0,.38f,0),new Vector3(radius*2.5f,.14f,radius*2.5f),Ash*.96f);
   var b=new Batch();for(int i=0;i<8;i++){float a=i*Mathf.PI/4,c=(i+1)*Mathf.PI/4;float topA=height+R(rng,-.16f,.16f),topB=height+R(rng,-.16f,.16f);b.Face(new[]{At(a,radius)+Vector3.up*.43f,At(a,radius*.86f)+Vector3.up*topA,At(c,radius*.86f)+Vector3.up*topB,At(c,radius)+Vector3.up*.43f});b.Face(new[]{Vector3.up*(height-.04f),At(c,radius*.86f)+Vector3.up*topB,At(a,radius*.86f)+Vector3.up*topA});}
   b.Finish(group,"Fractured stone shaft",Ash*1.08f);
   if(height>2.2f){Block(group,"Iron retaining collar",new Vector3(0,.7f,0),new Vector3(radius*2.2f,.12f,radius*2.2f),Iron);Block(group,"Silver collar lip",new Vector3(0,.77f,0),new Vector3(radius*2.22f,.025f,radius*2.22f),new Color(.38f,.40f,.36f));}
   return group;
  }
  static void Brazier(Transform root,Vector3 p,int seed){
   var g=WorldArt.Group("Forged sanctuary brazier",root);g.localPosition=p;
   Block(g,"Stone hearth footing",new Vector3(0,.10f,0),new Vector3(.92f,.20f,.92f),Ash*.8f,12);
   for(int i=0;i<4;i++){float a=i*Mathf.PI*.5f+Mathf.PI*.25f;var leg=Block(g,"Forged splayed leg",At(a,.25f)+Vector3.up*.47f,new Vector3(.09f,.65f,.09f),Iron);leg.localRotation=Quaternion.Euler(Mathf.Sin(a)*13,0,Mathf.Cos(a)*13);}
   WorldArt.Part(g,"Hammered bronze fire bowl",PrimitiveType.Cylinder,new Vector3(0,.83f,0),new Vector3(.85f,.12f,.85f),Bronze);
   WorldArt.Part(g,"Blackened coals",PrimitiveType.Cylinder,new Vector3(0,.96f,0),new Vector3(.72f,.025f,.72f),new Color(.22f,.045f,.018f));
   for(int i=0;i<6;i++){float a=i*Mathf.PI/3;Block(g,"Iron basket rib",At(a,.37f)+Vector3.up*1.0f,new Vector3(.045f,.30f,.045f),Iron);}
   // Broad flame tongues rather than floating crystal primitives or speckles.
   var flames=new Batch();for(int i=0;i<3;i++){float x=(i-1)*.15f;float h=i==1?.62f:.40f;flames.Face(new[]{new Vector3(x-.15f,.96f,-.04f),new Vector3(x+.05f,.98f,.12f),new Vector3(x+.08f,1.02f+h,.01f)});flames.Face(new[]{new Vector3(x-.15f,.96f,-.04f),new Vector3(x+.08f,1.02f+h,.01f),new Vector3(x+.16f,.97f,-.10f)});}
   var fire=flames.Finish(g,"Stylized fire tongues",new Color(.95f,.28f,.035f));fire.GetComponent<Renderer>().sharedMaterial=WorldArt.Glow(new Color(1,.37f,.055f));
   var lamp=WorldArt.Group("Warm brazier pool",g).gameObject.AddComponent<Light>();lamp.transform.localPosition=Vector3.up*1.25f;lamp.type=LightType.Point;lamp.color=new Color(1,.51f,.21f);lamp.intensity=1.15f;lamp.range=5;lamp.shadows=LightShadows.None;
  }
  static void Arch(Transform root,Vector3 p,float angle,ArenaCollision collision,int seed){
   var group=WorldArt.Group("Broken sanctuary arch",root);group.localPosition=p;group.localRotation=Quaternion.Euler(0,angle,0);
   for(int side=-1;side<=1;side+=2){Column(group,new Vector3(side*1.45f,0,0),2.45f,.39f,seed+side);collision.Add(group.TransformPoint(new Vector3(side*1.45f,0,0)),.84f);}
   for(int i=0;i<10;i++){if(i==6||i==7)continue;float a=(i+.5f)*Mathf.PI/10;var stone=Block(group,"Chipped arch voussoir",new Vector3(Mathf.Cos(a)*1.45f,2.4f+Mathf.Sin(a)*1.45f,0),new Vector3(.48f,.50f,.62f),Ash*(.91f+i*.014f));stone.localRotation=Quaternion.Euler(0,0,a*Mathf.Rad2Deg-90);}
   Banner(group,new Vector3(2.8f,0,.2f),seed);collision.Add(group.TransformPoint(new Vector3(2.8f,0,.2f)),.17f);
  }
  static void Banner(Transform root,Vector3 p,int seed){
   var g=WorldArt.Group("Tattered burgundy sanctuary standard",root);g.localPosition=p;
   Block(g,"Iron banner staff",new Vector3(0,1.65f,0),new Vector3(.09f,3.3f,.09f),Iron);Block(g,"Bronze banner crossbar",new Vector3(.43f,3.05f,0),new Vector3(1.05f,.075f,.075f),Bronze);
   var cloth=new Batch();for(int y=0;y<5;y++){float a=y/5f,b=(y+1)/5f;float bottom=y==4?.12f:0;Vector3 l0=new Vector3(.04f,2.99f-a*1.8f,Mathf.Sin(a*4+seed)*.08f),r0=new Vector3(.93f,2.99f-a*1.8f,Mathf.Sin(a*4+seed+.6f)*.12f),l1=new Vector3(.04f,2.99f-b*1.8f+bottom,Mathf.Sin(b*4+seed)*.08f),r1=new Vector3(.93f,2.99f-b*1.8f-bottom,Mathf.Sin(b*4+seed+.6f)*.12f);cloth.Face(new[]{l0,r0,r1,l1});cloth.Face(new[]{l1,r1,r0,l0});}
   cloth.Finish(g,"Worn red cloth",new Color(.235f,.041f,.028f));
   var emblem=new Batch();emblem.Face(new[]{new Vector3(.49f,2.65f,-.10f),new Vector3(.29f,2.31f,-.10f),new Vector3(.49f,1.97f,-.10f),new Vector3(.69f,2.31f,-.10f)});emblem.Face(new[]{new Vector3(.69f,2.31f,.14f),new Vector3(.49f,1.97f,.14f),new Vector3(.29f,2.31f,.14f),new Vector3(.49f,2.65f,.14f)});emblem.Finish(g,"Faded bronze sanctuary device",Bronze*.9f);
  }
  static void Moss(Transform root,Vector3 p,float radius,int seed){var rng=new System.Random(seed);var b=new Batch();Vector3 centre=p+Vector3.up*.012f;var ring=new Vector3[9];for(int i=0;i<9;i++)ring[i]=centre+At(i*Mathf.PI*2/9,radius*R(rng,.6f,1));for(int i=0;i<9;i++)b.Face(new[]{centre,ring[(i+1)%9],ring[i]});b.Finish(root,"Broad muted moss at ruin footing",new Color(.115f,.145f,.075f));
   var grass=new Batch();for(int clump=0;clump<4;clump++){Vector3 at=p+At(R(rng,0,Mathf.PI*2),R(rng,.85f,1.4f));for(int leaf=0;leaf<6;leaf++){float a=R(rng,0,Mathf.PI*2),h=R(rng,.16f,.36f);Vector3 side=At(a,.045f),bend=At(a+.7f,.14f);Vector3[] blade={at-side,at+side,at+Vector3.up*h+bend*.5f,at+Vector3.up*(h*1.2f)+bend};grass.Face(blade);Array.Reverse(blade);grass.Face(blade);}}grass.Finish(root,"Grouped olive grass blades",new Color(.14f,.19f,.07f));
  }
  public static void Build(Transform root,ArenaCollision collision){
   Floor(root);
   for(int i=0;i<8;i++){Vector3 p=At((i+.5f)*Mathf.PI/4,13.5f);Brazier(root,p,81+i);collision.Add(p,.68f);}
   for(int i=0;i<12;i++){float a=i*Mathf.PI/6+.18f,r=23+(i%3)*3.6f;Vector3 p=At(a,r);Column(root,p,i%3==0?2.7f:1.0f+(i%4)*.3f,.48f,90+i);collision.Add(p,1.04f);Moss(root,p,1.6f,310+i);
    var fallen=Block(root,"Fallen dressed stone",p+At(a+.9f,2.1f)+Vector3.up*.27f,new Vector3(1.2f,.54f,.85f),Ash*.87f,i*31);collision.Add(fallen.position,.77f);
   }
   for(int i=0;i<8;i++){float a=(i+.25f)*Mathf.PI/4;Arch(root,At(a,i%2==0?36:44),-a*Mathf.Rad2Deg+90,collision,420+i*7);}
   for(int i=0;i<6;i++){float a=(i+.6f)*Mathf.PI/3;Vector3 start=At(a,42+i%2*3);Vector3[] path={start,start+At(a+.4f,1.1f),start+At(a-.15f,2),start+At(a+.25f,3.1f)};
    var dark=new Batch();var ember=new Batch();for(int j=0;j<path.Length-1;j++){Vector3 d=(path[j+1]-path[j]).normalized,n=new Vector3(-d.z,0,d.x);dark.Face(new[]{path[j]-n*.065f+Vector3.up*.014f,path[j]+n*.065f+Vector3.up*.014f,path[j+1]+n*.065f+Vector3.up*.014f,path[j+1]-n*.065f+Vector3.up*.014f});ember.Face(new[]{path[j]-n*.016f+Vector3.up*.015f,path[j]+n*.016f+Vector3.up*.015f,path[j+1]+n*.016f+Vector3.up*.015f,path[j+1]-n*.016f+Vector3.up*.015f});}
    dark.Finish(root,"Inset fracture shadow",new Color(.045f,.038f,.028f));var glow=ember.Finish(root,"Restrained Veil seam",new Color(.35f,.04f,.012f));glow.GetComponent<Renderer>().sharedMaterial=WorldArt.Glow(new Color(.28f,.025f,.008f));
   }
   foreach(var renderer in root.GetComponentsInChildren<MeshRenderer>()){var old=renderer.sharedMaterial;bool terrain=renderer.name.Contains("earth")||renderer.name.Contains("moss");if(old&&old.GetColor("_EmissionColor").maxColorComponent<.01f)renderer.sharedMaterial=Painted(old.color,terrain);}
   StaticBatchingUtility.Combine(root.gameObject);
  }
 }
}

