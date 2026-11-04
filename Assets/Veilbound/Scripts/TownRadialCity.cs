using System;using System.Collections.Generic;using UnityEngine;
namespace Veilbound {public partial class VeilGame {
 // Rendering only. Save data, building levels, research and production rules are unchanged.
 struct RadialPart {
  public string id,owner,asset;public Rect rect,uv;public float ground;
  public RadialPart(string id,string owner,string asset,Rect rect,Rect uv,float ground){this.id=id;this.owner=owner;this.asset=asset;this.rect=rect;this.uv=uv;this.ground=ground;}
 }
 readonly Dictionary<string,Texture2D> radialTextures=new Dictionary<string,Texture2D>();
 Texture2D radialAccessTexture;int radialAccessKey=-1;Color[] radialPaving;
 Texture2D RadialTexture(string asset){if(!radialTextures.TryGetValue(asset,out var t)){t=Resources.Load<Texture2D>("Sanctuary/TownProgression/Radial/"+asset);radialTextures[asset]=t;}return t;}
 bool RadialOwnerVisible(string id)=>id=="herbalist"||id=="enchanter"||city.BuildingLevel(id)>0;
 static Rect RadialMapRect(Rect map,Rect pixels)=>new Rect(map.x+pixels.x/1536*map.width,map.y+pixels.y/1024*map.height,pixels.width/1536*map.width,pixels.height/1024*map.height);
 void RadialPaint(Rect map,Rect pixels,Texture2D texture,Rect uv){if(!texture)return;Rect r=RadialMapRect(map,pixels);if(townOffscreenCapture)Graphics.DrawTexture(r,texture,uv,0,0,0,0,new Color(.5f,.5f,.5f,.5f));else GUI.DrawTextureWithTexCoords(r,texture,uv,true);}
 void DrawRadialTown(Rect area){
  if(!townOffscreenCapture&&Event.current.type!=EventType.Repaint)return;
  Rect map=TownAspectRect(area);InitCityArt();var terrain=RadialTexture("terrain");
  if(!terrain){DrawLayeredTown(area);return;}
  RadialPaint(map,new Rect(0,0,1536,1024),terrain,new Rect(0,0,1,1));
  EnsureRadialAccess();RadialPaint(map,new Rect(0,0,1536,1024),radialAccessTexture,new Rect(0,0,1,1));
  int tier=Mathf.Clamp(city.housingTier,0,5);RadialPaint(map,RadialHousingRects[tier],RadialTexture("housing-"+tier),new Rect(0,0,1,1));
  bool towerDrawn=false;
  foreach(var p in RadialWorkshopParts){if(!RadialOwnerVisible(p.owner))continue;if(!towerDrawn&&p.ground>610){RadialPaint(map,RadialTowerRect,RadialTexture("tower"),new Rect(0,0,1,1));towerDrawn=true;}RadialPaint(map,p.rect,RadialTexture(p.asset),p.uv);}
  if(!towerDrawn)RadialPaint(map,RadialTowerRect,RadialTexture("tower"),new Rect(0,0,1,1));
  DrawTownVeilOverlay(map);
 }
 string RadialPortraitAsset(string id){switch(id){case "housing":return "housing-"+Mathf.Clamp(city.housingTier,0,5);case "timber":return "lumbermill";case "weaver":return "textiles";case "library":return "library-v2";case "enchanter":return "mystic";case "tailor":case "leatherworker":return "clothier";case "mine":case "quarry":case "blacksmith":case "hunters":case "clothier":case "herbalist":return id;default:return null;}}
 bool DrawRadialPortrait(Rect rect,string id){string asset=RadialPortraitAsset(id);if(asset==null)return false;var texture=RadialTexture(asset);if(!texture)return false;var uv=RadialPortraitUV[asset];float aspect=texture.width*uv.width/(texture.height*uv.height),w=Mathf.Min(rect.width,rect.height*aspect),h=w/aspect;GUI.DrawTextureWithTexCoords(new Rect(rect.center.x-w/2,rect.center.y-h/2,w,h),texture,uv,true);return true;}

 struct RadialRoad {public string owner;public Vector2 a,b,c,d;public bool fromRing;public RadialRoad(string owner,bool fromRing,float ax,float ay,float bx,float by,float cx,float cy,float dx,float dy){this.owner=owner;this.fromRing=fromRing;a=new Vector2(ax,ay);b=new Vector2(bx,by);c=new Vector2(cx,cy);d=new Vector2(dx,dy);}}
 static readonly RadialRoad[] RadialRoads={
  new RadialRoad("",true,850,760,850,815,850,920,850,1010),
  new RadialRoad("library",false,850,870,800,885,720,890,660,890),
  new RadialRoad("clothier",false,850,890,880,905,920,910,950,910),
  new RadialRoad("timber",true,530,405,480,425,440,427,402,405),
  new RadialRoad("blacksmith",true,450,545,412,565,380,585,340,586),
  new RadialRoad("mine",true,600,735,540,770,500,808,452,816),
  new RadialRoad("hunters",true,1175,430,1240,450,1290,435,1330,420),
  new RadialRoad("weaver",true,1235,590,1290,610,1330,635,1370,640),
  new RadialRoad("quarry",true,1100,740,1160,785,1200,825,1240,845),
  new RadialRoad("herbalist",true,1175,430,1220,365,1170,310,1115,300),
  new RadialRoad("enchanter",true,530,405,500,360,530,315,610,300),
 };
 static Vector2 RadialBezier(Vector2 a,Vector2 b,Vector2 c,Vector2 d,float t){float u=1-t;return a*(u*u*u)+b*(3*u*u*t)+c*(3*u*t*t)+d*(t*t*t);}
 void EnsureRadialAccess(){
  int tier=Mathf.Clamp(city.housingTier,0,5),key=tier;for(int i=1;i<RadialRoads.Length;i++)if(RadialOwnerVisible(RadialRoads[i].owner))key|=1<<(i+3);
  if(radialAccessTexture&&radialAccessKey==key)return;radialAccessKey=key;
  if(!radialAccessTexture)radialAccessTexture=new Texture2D(1536,1024,TextureFormat.RGBA32,true){name="Settlement access paths",filterMode=FilterMode.Trilinear,wrapMode=TextureWrapMode.Clamp};
  if(radialPaving==null){var source=RadialTexture("housing-5");radialPaving=source.GetPixels(750,source.height-180-36,36,36);}
  var pixels=new Color[1536*1024];bool stone=tier>=4;float[] ringScale={.55f,.6f,.67f,.85f,.9f,1};Vector2 centre=new Vector2(850,540);
  for(int pass=0;pass<2;pass++)foreach(var road in RadialRoads){
   if(road.owner!=""&&!RadialOwnerVisible(road.owner))continue;Vector2 a=road.a,b=road.b;if(road.fromRing){a=centre+(a-centre)*ringScale[tier];b=centre+(b-centre)*(.5f+.5f*ringScale[tier]);}
   Vector2 previous=a;for(int step=1;step<=80;step++){Vector2 next=RadialBezier(a,b,road.c,road.d,step/80f);RasterRadialRoad(pixels,previous,next,pass==0?(stone?6.5f:5.5f):(stone?4.5f:3.5f),pass==1,stone);previous=next;}
  }
  radialAccessTexture.SetPixels(pixels);radialAccessTexture.Apply(true,false);
 }
 void RasterRadialRoad(Color[] pixels,Vector2 a,Vector2 b,float radius,bool inside,bool stone){
  Vector2 delta=b-a;float length=delta.sqrMagnitude;int left=Mathf.Max(0,Mathf.FloorToInt(Mathf.Min(a.x,b.x)-radius-1)),right=Mathf.Min(1535,Mathf.CeilToInt(Mathf.Max(a.x,b.x)+radius+1)),top=Mathf.Max(0,Mathf.FloorToInt(Mathf.Min(a.y,b.y)-radius-1)),bottom=Mathf.Min(1023,Mathf.CeilToInt(Mathf.Max(a.y,b.y)+radius+1));
  for(int y=top;y<=bottom;y++)for(int x=left;x<=right;x++){Vector2 point=new Vector2(x+.5f,y+.5f);float t=length>.0001f?Mathf.Clamp01(Vector2.Dot(point-a,delta)/length):0;float coverage=Mathf.Clamp01(radius+.5f-Vector2.Distance(point,a+t*delta));if(coverage<=0)continue;Color color;
   if(inside&&stone){int px=Mathf.FloorToInt(x/.55f)%36,py=35-Mathf.FloorToInt(y/.55f)%36;color=radialPaving[py*36+px];color.a=1;}else color=inside?new Color(.702f,.592f,.427f,1):stone?new Color(.341f,.345f,.286f,.85f):new Color(.478f,.404f,.278f,.75f);
   color.a*=coverage;int index=(1023-y)*1536+x;Color old=pixels[index];float alpha=color.a+old.a*(1-color.a);if(alpha>0){float oldWeight=old.a*(1-color.a);pixels[index]=new Color((color.r*color.a+old.r*oldWeight)/alpha,(color.g*color.a+old.g*oldWeight)/alpha,(color.b*color.a+old.b*oldWeight)/alpha,alpha);}
  }
 }
}}
