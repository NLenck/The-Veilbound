using System.Collections.Generic;using System.Linq;using UnityEngine;
using M=Veilbound.RuinedTownArena.Meshes;
namespace Veilbound {
 public static class GraveyardDressing {
  public static readonly Vector3[] BrazierPositions={new Vector3(-7.8f,0,11.8f),new Vector3(7.8f,0,11.8f),new Vector3(-7.8f,0,-11.8f),new Vector3(7.8f,0,-11.8f),new Vector3(-24,0,-8.4f),new Vector3(-16,0,-8.4f)};
  static readonly Color Iron=new Color(.10f,.115f,.12f),Stone=new Color(.28f,.30f,.29f),Orange=new Color(1,.31f,.055f),Gold=new Color(1,.65f,.18f);
  public static void Build(Transform root,ArenaCollision collision){
   var feet=new M();var iron=new M();var coals=new M();
   for(int i=0;i<BrazierPositions.Length;i++){
    Vector3 p=BrazierPositions[i];feet.Box(p+Vector3.up*.10f,new Vector3(.86f,.20f,.86f));iron.Branch(p+Vector3.up*.20f,p+Vector3.up*1.05f,.13f,.13f,6);iron.Branch(p+Vector3.up*1.01f,p+Vector3.up*1.31f,.19f,.47f,8);coals.Box(p+Vector3.up*1.32f,new Vector3(.54f,.07f,.54f));
    for(int arm=0;arm<6;arm++){float a=arm*Mathf.PI/3;Vector3 edge=new Vector3(Mathf.Cos(a)*.48f,0,Mathf.Sin(a)*.48f);iron.Branch(p+Vector3.up*1.08f,p+edge+Vector3.up*1.56f,.035f,.035f,4);}
    collision.Add(p,.52f);var holder=WorldArt.Group("Brazier "+(i+1)+" - warm flame",root);holder.localPosition=p+Vector3.up*1.38f;
    var outer=new M();outer.Branch(Vector3.zero,new Vector3(.14f,.86f,-.04f),.29f,.018f,5);var renderer=outer.Finish(holder,"Faceted amber flame",Orange);renderer.sharedMaterial=WorldArt.Glow(Orange);renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
    var inner=new M();inner.Branch(new Vector3(0,.02f,0),new Vector3(-.07f,.56f,.06f),.17f,.012f,5);renderer=inner.Finish(holder,"Warm flame heart",Gold);renderer.sharedMaterial=WorldArt.Glow(Gold);renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
    var light=WorldArt.Group("Local brazier warmth",root).gameObject.AddComponent<Light>();light.transform.localPosition=p+Vector3.up*1.85f;light.type=LightType.Point;light.range=7;light.renderMode=LightRenderMode.ForcePixel;light.color=new Color(1,.48f,.20f);light.shadows=LightShadows.None;light.intensity=1.45f;
    var flicker=holder.gameObject.AddComponent<GraveyardBrazierFlame>();flicker.flame=holder;flicker.lamp=light;flicker.phase=i*1.37f;flicker.Sample(0);
   }
   feet.Finish(root,"Six brazier stone feet - batched",Stone);iron.Finish(root,"Forged iron brazier baskets - batched",Iron);coals.Finish(root,"Banked coals - batched",Orange).sharedMaterial=WorldArt.Glow(Orange);
   ShrineHalo(root,collision);CathedralBanners();
  }
  static void ShrineHalo(Transform root,ArenaCollision collision){
   var frame=new M();const float z=4.22f;float radius=1.25f;
   Vector3 Point(float degrees){float a=degrees*Mathf.Deg2Rad;return new Vector3(Mathf.Cos(a)*radius,1.7f+Mathf.Sin(a)*radius,z);}
   foreach(float a in new[]{20f,160f}){var top=Point(a);frame.Branch(new Vector3(top.x,0,z),top,.18f,.17f,5);collision.Add(new Vector3(top.x,0,z),.18f);}
   for(int i=0;i<7;i++){if(i==3)continue;frame.Branch(Point(20+i*20),Point(40+i*20),.17f,.15f,5);}
   frame.Finish(root,"Broken ritual halo behind the crystal",Stone*.92f);
  }
  static void CathedralBanners(){
   var room=GraveyardArena.Rooms[0];var cloth=new M();var rods=new M();float z=room.depth*.5f-.36f;
   foreach(int side in new[]{-1,1}){float x=side*4.5f;cloth.Face(new Vector3(x-.62f,3.65f,z),new Vector3(x+.62f,3.65f,z),new Vector3(x+.62f,1.65f,z),new Vector3(x+.18f,1.93f,z),new Vector3(x-.15f,1.53f,z),new Vector3(x-.62f,1.80f,z));rods.Box(new Vector3(x,3.78f,z),new Vector3(1.48f,.11f,.14f));rods.Box(new Vector3(x,1.90f,z),new Vector3(.10f,3.80f,.14f));}
   var banner=cloth.Finish(room.transform,"Two faded cathedral funeral banners",new Color(.24f,.09f,.08f));banner.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.TwoSided;
   var rod=rods.Finish(room.transform,"Old banner spars",new Color(.14f,.105f,.075f));room.walls=room.walls.Concat(new[]{banner,rod}).ToArray();
  }
 }
}

