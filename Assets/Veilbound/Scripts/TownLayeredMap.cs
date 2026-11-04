using System;using System.Collections.Generic;using UnityEngine;
namespace Veilbound {public partial class VeilGame {
 // Pixel coordinates refer to the untouched 1536 x 1024 terrain. Ground anchors,
 // rather than texture centres, keep upgrades planted in the same location.
 struct TownLayer {public string id,sheet;public int stage;public Vector2 ground;public float width,maxHeight;public bool atlas;public TownLayer(string id,string sheet,int stage,float x,float y,float width,float maxHeight=0,bool atlas=false){this.id=id;this.sheet=sheet;this.stage=stage;ground=new Vector2(x,y);this.width=width;this.maxHeight=maxHeight;this.atlas=atlas;}}
 static readonly Vector2[] LayerHomeGrounds={
 new Vector2(568,377),new Vector2(735,445),new Vector2(510,514),new Vector2(609,626),new Vector2(1019,438),new Vector2(1138,522),new Vector2(752,684),new Vector2(966,685),
 new Vector2(653,377),new Vector2(593,502),new Vector2(1101,438),new Vector2(1219,559),new Vector2(1057,657),new Vector2(640,645),new Vector2(798,680),new Vector2(1012,680)};
 static readonly int[] LayerHomeCounts={8,10,11,13,14,17};
 readonly List<TownLayer> townLayers=new List<TownLayer>();
 void AddTownLayer(string id,string sheet,int stage,float x,float y,float width,float maxHeight=0,bool atlas=false){townLayers.Add(new TownLayer(id,sheet,stage,x,y,width,maxHeight,atlas));}
 void AddPine(string owner,float x,float y,float width=27){AddTownLayer(owner+" tree","decor",0,x,y,width,0,true);}
 void AddRocks(string owner,float x,float y,float width=28){AddTownLayer(owner+" rocks","decor",1,x,y,width,0,true);}
 void CollectTownLayers(){
  townLayers.Clear();int tier=Mathf.Clamp(city.housingTier,0,5);
  CollectReferenceHousing(tier);
  if(city.timber>0){AddTownLayer("timber","Recovery/timber",0,402,405,135,80,true);foreach(var p in new[]{new Vector2(297,415),new Vector2(326,358),new Vector2(468,351),new Vector2(494,389),new Vector2(516,412),new Vector2(480,430)})AddPine("timber",p.x,p.y,24);}
  if(city.blacksmith>0){AddTownLayer("blacksmith","Recovery/blacksmith",0,355,586,105,82,true);foreach(var p in new[]{new Vector2(250,575),new Vector2(278,533),new Vector2(307,507),new Vector2(335,497),new Vector2(379,509),new Vector2(425,555),new Vector2(447,571)})AddPine("blacksmith",p.x,p.y,24);}
  if(city.mine>0){AddTownLayer("mine","Recovery/mine",0,452,816,114,80,true);AddRocks("mine",371,789,28);AddRocks("mine",387,810,25);AddRocks("mine",520,801,27);AddRocks("mine",535,822,25);AddRocks("mine",500,747,23);}
  if(city.quarry>0){AddTownLayer("quarry","Recovery/quarry",0,1240,845,204,106,true);AddRocks("quarry",1128,820,30);AddRocks("quarry",1150,846,25);AddRocks("quarry",1355,843,32);AddRocks("quarry",1330,785,25);AddRocks("quarry",1201,754,24);}
  if(city.huntersLodge>0){AddTownLayer("hunters","Recovery/hunters",0,1310,420,148,78,true);foreach(var p in new[]{new Vector2(1207,408),new Vector2(1243,371),new Vector2(1339,368),new Vector2(1391,401)})AddPine("hunters",p.x,p.y,25);}
  if(city.weaver>0){
   AddTownLayer("weaver","Recovery/weaver",0,1370,620,103,80,true);
   AddTownLayer("flax","textile-props",0,1255,638,86,49,true);
   if(city.weaver>=2)AddTownLayer("wool","textile-props",1,1460,640,74,42,true);
   if(city.weaver>=3)AddTownLayer("silk","textile-props",2,1404,615,26,28,true);
   if(city.weaver>=4)AddTownLayer("ash silk","textile-props",3,1295,590,34,39,true);
   if(city.weaver>=5)AddTownLayer("veil fibre","textile-props",4,1325,650,28,33,true);
  }
  if(city.library>0)AddTownLayer("library","Recovery/library",0,700,820,175,96,true);
  if(city.clothier>0)AddTownLayer("clothier","clothier",Mathf.Clamp(city.clothier-1,0,9),993,850,122,115);
  // Reserved service plots remain empty until the corresponding building exists.
  if(city.BuildingLevel("herbalist")>0)AddTownLayer("herbalist","services",1,1180,320,105,100);
  if(city.BuildingLevel("enchanter")>0)AddTownLayer("enchanter","services",3,630,320,105,90);
  townLayers.Sort((a,b)=>a.ground.y.CompareTo(b.ground.y));
 }
 Rect LayerUV(TownLayer item)=>item.sheet.StartsWith("Recovery/HousingRefs/")?HousingReferenceUV[item.sheet+":"+item.stage]:item.sheet.StartsWith("Recovery/")?TownRecoveryUV[item.sheet+":"+item.stage]:TownLayerUV[(item.atlas?"":"original/")+item.sheet+":"+item.stage];
 Texture2D LayerTexture(TownLayer item)=>TownSheet((item.atlas?"Layers/":"")+item.sheet);
 static readonly float[] LayerHomeHeights={65,69,73,82,95,109};
 Rect LayerPixelRect(TownLayer item){var t=LayerTexture(item);var uv=LayerUV(item);float w=item.width,h=w*(t.height*uv.height)/(t.width*uv.width);if(item.sheet.StartsWith("Recovery/")&&item.maxHeight>0)return new Rect(item.ground.x-w/2,item.ground.y-item.maxHeight,w,item.maxHeight);if(item.atlas&&item.sheet=="housing")h=LayerHomeHeights[item.stage];if(item.maxHeight>0&&h>item.maxHeight){w*=item.maxHeight/h;h=item.maxHeight;}return new Rect(item.ground.x-w/2,item.ground.y-h,w,h);}
 void DrawLayeredTown(Rect map){
  map=TownAspectRect(map);InitCityArt();if(sanctuaryMap){if(townOffscreenCapture)Graphics.DrawTexture(map,sanctuaryMap);else GUI.DrawTexture(map,sanctuaryMap,ScaleMode.StretchToFill);}
  if(!townOffscreenCapture)GUI.BeginGroup(map);CollectTownLayers();
  foreach(var item in townLayers){var p=LayerPixelRect(item);var r=new Rect(p.x/1536*map.width,p.y/1024*map.height,p.width/1536*map.width,p.height/1024*map.height);if(townOffscreenCapture){r.x+=map.x;r.y+=map.y;Graphics.DrawTexture(r,LayerTexture(item),LayerUV(item),0,0,0,0,new Color(.5f,.5f,.5f,.5f));}else GUI.DrawTextureWithTexCoords(r,LayerTexture(item),LayerUV(item),true);}
  if(!townOffscreenCapture)GUI.EndGroup();DrawTownVeilOverlay(map);
 }
 bool DrawLayerPortrait(Rect r,string id){
  if(id=="housing"){DrawReferenceHousingPortrait(r);return true;}
  if(id=="timber"||id=="mine"||id=="quarry"||id=="blacksmith"||id=="hunters"||id=="weaver"||id=="library"){DrawRecoveryPortrait(r,id,0);return true;}return false;
 }
 void DrawRecoveryPortrait(Rect r,string name,int stage){var item=new TownLayer(name,"Recovery/"+name,stage,0,0,100,0,true);var t=LayerTexture(item);if(!t)return;var uv=LayerUV(item);float aspect=t.width*uv.width/(t.height*uv.height),w=Mathf.Min(r.width,r.height*aspect),h=w/aspect;GUI.DrawTextureWithTexCoords(new Rect(r.center.x-w/2,r.center.y-h/2,w,h),t,uv,true);}
}}
