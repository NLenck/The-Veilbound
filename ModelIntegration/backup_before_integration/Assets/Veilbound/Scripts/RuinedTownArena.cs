using System;
using System.Collections.Generic;
using UnityEngine;
namespace Veilbound {
 public static class RuinedTownArena {
  public static readonly List<TownInterior> Rooms=new List<TownInterior>();
  static readonly Color Stone=new Color(.29f,.285f,.25f),Wood=new Color(.20f,.13f,.075f),Iron=new Color(.14f,.15f,.135f);
  static readonly Dictionary<Color,Material> mats=new Dictionary<Color,Material>();
  static Material Mat(Color c,bool ground=false){var key=new Color(c.r,c.g,c.b,ground?0:1);if(mats.TryGetValue(key,out var m)&&m)return m;m=new Material(Shader.Find("Veilbound/TownSurface"));m.color=c;m.SetFloat("_Terrain",ground?1:0);mats[key]=m;return m;}
  static float R(System.Random rng,float a,float b)=>Mathf.Lerp(a,b,(float)rng.NextDouble());
  public sealed class Meshes {
   readonly List<Vector3> v=new List<Vector3>();readonly List<int> t=new List<int>();
   public void Face(params Vector3[] p){int n=v.Count;v.AddRange(p);for(int i=1;i<p.Length-1;i++)t.AddRange(new[]{n,n+i,n+i+1});}
   public void Box(Vector3 centre,Vector3 size){Vector3 a=centre-size*.5f,b=centre+size*.5f;Face(new Vector3(a.x,a.y,a.z),new Vector3(a.x,b.y,a.z),new Vector3(b.x,b.y,a.z),new Vector3(b.x,a.y,a.z));Face(new Vector3(b.x,a.y,b.z),new Vector3(b.x,b.y,b.z),new Vector3(a.x,b.y,b.z),new Vector3(a.x,a.y,b.z));Face(new Vector3(a.x,a.y,b.z),new Vector3(a.x,b.y,b.z),new Vector3(a.x,b.y,a.z),new Vector3(a.x,a.y,a.z));Face(new Vector3(b.x,a.y,a.z),new Vector3(b.x,b.y,a.z),new Vector3(b.x,b.y,b.z),new Vector3(b.x,a.y,b.z));Face(new Vector3(a.x,b.y,a.z),new Vector3(a.x,b.y,b.z),new Vector3(b.x,b.y,b.z),new Vector3(b.x,b.y,a.z));Face(new Vector3(a.x,a.y,b.z),new Vector3(a.x,a.y,a.z),new Vector3(b.x,a.y,a.z),new Vector3(b.x,a.y,b.z));}
   public Renderer Finish(Transform parent,string name,Color color,bool terrain=false){var m=new Mesh{name=name};if(v.Count>65000)m.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;m.SetVertices(v);m.SetTriangles(t,0);m.RecalculateNormals();m.RecalculateBounds();var g=WorldArt.Group(name,parent);g.gameObject.AddComponent<MeshFilter>().sharedMesh=m;var renderer=g.gameObject.AddComponent<MeshRenderer>();renderer.sharedMaterial=Mat(color,terrain);return renderer;}
  }
  static Transform Part(Transform p,string name,Vector3 at,Vector3 size,Color color){var b=new Meshes();b.Box(at,size);return b.Finish(p,name,color).transform;}
  static void Ground(Transform root){
   var earth=new Meshes();earth.Box(new Vector3(0,-.19f,0),new Vector3(120,.20f,120));earth.Finish(root,"Borough earth and moss",new Color(.17f,.16f,.10f),true);
   var rng=new System.Random(833);var streets=new Meshes[5];for(int i=0;i<5;i++)streets[i]=new Meshes();
   for(float z=-52;z<52;z+=1.1f)for(float x=-52;x<52;x+=1.25f){if(x*x+z*z>54*54)continue;float dx=Mathf.Min(Mathf.Abs(x),Mathf.Abs(Mathf.Abs(x)-20)),dz=Mathf.Min(Mathf.Abs(z),Mathf.Abs(Mathf.Abs(z)-20));if(dx>3.2f&&dz>3.2f)continue;if(rng.NextDouble()<.065)continue;streets[rng.Next(5)].Box(new Vector3(x+R(rng,-.035f,.035f),-.025f,z+R(rng,-.035f,.035f)),new Vector3(1.19f,.07f,1.04f));}
   for(int i=0;i<5;i++)streets[i].Finish(root,"Worn borough street paving "+i,Stone*(.84f+i*.055f));
  }
  static void Wall(Transform house,Meshes[] masonry,ArenaCollision collision,Vector3 a,Vector3 b,float height,System.Random rng){
   collision.AddWall(house.TransformPoint(a),house.TransformPoint(b),.31f);Vector3 d=b-a;float length=d.magnitude;bool alongX=Mathf.Abs(d.x)>Mathf.Abs(d.z);int columns=Mathf.CeilToInt(length/.95f);float step=length/columns;
   for(int i=0;i<columns;i++){float top=height+R(rng,-.3f,.22f);int courses=Mathf.Max(2,Mathf.RoundToInt(top/.32f));for(int j=0;j<courses;j++){Vector3 p=Vector3.Lerp(a,b,(i+.5f)/columns)+Vector3.up*(.16f+j*.32f);Vector3 size=alongX?new Vector3(step-.025f,.295f,.58f):new Vector3(.58f,.295f,step-.025f);masonry[rng.Next(masonry.Length)].Box(p,size);}}
  }
  static void Building(Transform root,ArenaCollision collision,Vector3 at,float width,float depth,float rotation,int index){
   var g=WorldArt.Group("Enterable ruin "+(index+1),root);g.localPosition=at;g.localRotation=Quaternion.Euler(0,rotation,0);var room=g.gameObject.AddComponent<TownInterior>();room.width=width;room.depth=depth;Rooms.Add(room);
   var floor=new Meshes();for(float x=-width*.5f+.45f;x<width*.5f;x+=.9f)for(float z=-depth*.5f+.45f;z<depth*.5f;z+=.9f)floor.Box(new Vector3(x,-.015f,z),new Vector3(.87f,.07f,.87f));floor.Finish(g,"Accessible roofless interior floor",new Color(.32f,.29f,.225f));
   var rng=new System.Random(1021+index*43);var masonry=new Meshes[4];for(int i=0;i<4;i++)masonry[i]=new Meshes();float xh=width*.5f,zh=depth*.5f,door=1.95f;
   if(index%3==2){Wall(g,masonry,collision,new Vector3(-xh,0,-zh),new Vector3(-xh,0,-2.05f),1.5f,rng);Wall(g,masonry,collision,new Vector3(-xh,0,2.05f),new Vector3(-xh,0,zh),1.8f,rng);}else Wall(g,masonry,collision,new Vector3(-xh,0,-zh),new Vector3(-xh,0,zh),1.8f,rng);
   Wall(g,masonry,collision,new Vector3(xh,0,-zh),new Vector3(xh,0,zh),1.65f,rng);
   foreach(int side in new[]{-1,1}){float z=side*zh;Wall(g,masonry,collision,new Vector3(-xh,0,z),new Vector3(-door,0,z),side<0?1.45f:2.1f,rng);Wall(g,masonry,collision,new Vector3(door,0,z),new Vector3(xh,0,z),side<0?1.65f:2.25f,rng);
    Part(g,"Open doorway wooden lintel",new Vector3(0,2.65f,z),new Vector3(4.15f,.23f,.70f),Wood);Part(g,"Door frame left",new Vector3(-2.05f,1.32f,z),new Vector3(.15f,2.64f,.66f),Wood);Part(g,"Door frame right",new Vector3(2.05f,1.32f,z),new Vector3(.15f,2.64f,.66f),Wood);
    var step=Part(g,"Door threshold",new Vector3(0,.035f,z),new Vector3(3.8f,.07f,.9f),Stone*.85f);
   }
   var wallRenderers=new List<Renderer>();for(int i=0;i<4;i++)wallRenderers.Add(masonry[i].Finish(g,"Cutaway broken masonry "+i,Stone*(.82f+i*.09f)));room.walls=wallRenderers.ToArray();
   // Remaining timber corners and plaster make these domestic ruins rather than arena props.
   Part(g,"Surviving corner timber",new Vector3(-xh+.12f,1.5f,zh-.1f),new Vector3(.19f,3,.19f),Wood);Part(g,"Surviving corner timber",new Vector3(xh-.12f,1.5f,zh-.1f),new Vector3(.19f,3,.19f),Wood);
   var plaster=new Meshes();if(index%3!=2)plaster.Box(new Vector3(-xh-.305f,.85f,zh*.35f),new Vector3(.025f,1.10f,depth*.36f));plaster.Box(new Vector3(xh+.305f,.9f,-zh*.25f),new Vector3(.025f,1.2f,depth*.3f));var pr=plaster.Finish(g,"Chipped old lime plaster",new Color(.44f,.415f,.35f));wallRenderers.Add(pr);room.walls=wallRenderers.ToArray();
   Vector3 corner=new Vector3(-xh+1.0f,0,zh-1.2f);Vector3 worldCorner=g.TransformPoint(corner);collision.Add(worldCorner,.95f);
   if(index%3==0){Part(g,"Old workshop bench",corner+Vector3.up*.7f,new Vector3(1.55f,.18f,.80f),Wood);for(int s=-1;s<=1;s+=2)Part(g,"Bench leg",corner+new Vector3(s*.6f,.34f,0),new Vector3(.13f,.68f,.65f),Wood);Part(g,"Abandoned anvil",corner+Vector3.up*.93f,new Vector3(.7f,.3f,.38f),Iron);}
   else if(index%3==1){Part(g,"Broken sleeping frame",corner+Vector3.up*.25f,new Vector3(1.4f,.5f,1.3f),Wood);Part(g,"Torn burgundy blanket",corner+Vector3.up*.52f,new Vector3(1.24f,.08f,1.15f),new Color(.25f,.045f,.032f));}
   else {Part(g,"Abandoned crate",corner+Vector3.up*.4f,Vector3.one*.8f,Wood);Part(g,"Crate iron band",corner+Vector3.up*.4f,new Vector3(.83f,.10f,.83f),Iron);}
   if(index%3==1){Vector3 chimney=new Vector3(xh-1.1f,0,-zh+1.1f);wallRenderers.Add(Part(g,"Surviving soot-dark chimney",chimney+Vector3.up*1.65f,new Vector3(.90f,3.30f,.90f),Stone*.75f).GetComponent<Renderer>());wallRenderers.Add(Part(g,"Broken chimney crown",chimney+Vector3.up*3.35f,new Vector3(1.06f,.22f,1.06f),Stone*.88f).GetComponent<Renderer>());collision.Add(g.TransformPoint(chimney),.72f);}
   // A broad torn cloth panel echoes the hero's cape; no tiny repetitive prop dots.
   var banner=new Meshes();banner.Face(new Vector3(xh+.315f,2.25f,-1.0f),new Vector3(xh+.315f,2.25f,.3f),new Vector3(xh+.315f,1.10f,.14f),new Vector3(xh+.315f,1.25f,-.4f),new Vector3(xh+.315f,1.0f,-1.0f));wallRenderers.Add(banner.Finish(g,"Torn household cloth",new Color(.24f,.047f,.033f)));room.walls=wallRenderers.ToArray();
  }
  static void StreetLamp(Transform root,ArenaCollision collision,Vector3 p){Part(root,"Iron street fire basket",p+Vector3.up*.8f,new Vector3(.65f,.23f,.65f),Iron);Part(root,"Street lamp stone foot",p+Vector3.up*.33f,new Vector3(.60f,.66f,.60f),Stone);var flame=WorldArt.Crystal(root,p+Vector3.up*1.1f,.32f,new Color(1,.36f,.07f));flame.name="Small street fire";collision.Add(p,.46f);var light=WorldArt.Group("Street firelight",root).gameObject.AddComponent<Light>();light.transform.position=p+Vector3.up*1.3f;light.type=LightType.Point;light.range=6;light.intensity=1.2f;light.color=new Color(1,.53f,.26f);}
  public static void Build(Transform root,ArenaCollision collision){
   Rooms.Clear();Ground(root);int index=0;
   foreach(int x in new[]{-1,1})foreach(int z in new[]{-1,1})Building(root,collision,new Vector3(x*10,0,z*10),index%2==0?10:9,index%3==0?10:11,index%2*90,index++);
   foreach(int x in new[]{-1,1})foreach(int z in new[]{-1,1})Building(root,collision,new Vector3(x*29,0,z*10),index%2==0?9:10,index%3==0?10:11,90,index++);
   foreach(int x in new[]{-1,1})foreach(int z in new[]{-1,1})Building(root,collision,new Vector3(x*10,0,z*29),index%2==0?10:9,index%3==0?10:11,0,index++);
   foreach(int x in new[]{-1,1})foreach(int z in new[]{-1,1})StreetLamp(root,collision,new Vector3(x*3.6f,0,z*17));
  }
 }
}

