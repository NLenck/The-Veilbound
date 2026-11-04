using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  Texture2D expeditionBackdrop,expeditionIconAtlas;Font expeditionSerif;
  void InitExpeditionArt(){if(!expeditionBackdrop)expeditionBackdrop=Resources.Load<Texture2D>("ExpeditionUI/ExpeditionBackdrop");if(!expeditionIconAtlas)expeditionIconAtlas=Resources.Load<Texture2D>("ExpeditionUI/ExpeditionIcons");if(!expeditionSerif)expeditionSerif=Font.CreateDynamicFontFromOSFont("Georgia",28);}
  void ExpeditionIcon(Rect rect,int index,Color tint){
   if(!expeditionIconAtlas)return;var old=GUI.color;GUI.color=tint;
   GUI.DrawTextureWithTexCoords(rect,expeditionIconAtlas,new Rect((index%5)/5f,1-(index/5+1)/2f,.2f,.5f),true);GUI.color=old;
  }
  void ExpeditionBevel(Rect r,Color colour,float edge=9){
   Vector2[] points={new Vector2(r.x+edge,r.y),new Vector2(r.xMax-edge,r.y),new Vector2(r.xMax,r.y+edge),new Vector2(r.xMax,r.yMax-edge),new Vector2(r.xMax-edge,r.yMax),new Vector2(r.x+edge,r.yMax),new Vector2(r.x,r.yMax-edge),new Vector2(r.x,r.y+edge)};
   for(int i=0;i<8;i++)HudLine(points[i],points[(i+1)%8],colour,1.5f);
  }
  void ExpeditionActionButton(Rect r,string title,System.Action action,int fontSize=18){
   int actionIndex=buttons.Count;menuFocusSuppression++;Button(r.x,r.y,r.width,r.height,"",action);menuFocusSuppression--;
   bool focused=selection==actionIndex;
   Box(r,new Color(.24f,.065f,.075f));
   ExpeditionBevel(r,focused?new Color(.98f,.8f,.46f):new Color(.77f,.58f,.31f),7);
   ExpeditionBevel(new Rect(r.x+5,r.y+5,r.width-10,r.height-10),new Color(.47f,.33f,.19f),5);
   ExpeditionText(r.x+8,r.y+4,r.width-16,r.height-8,title,fontSize,new Color(.96f,.86f,.64f),TextAnchor.MiddleCenter,true);RegisterMenuFocus(r,actionIndex,title,true);
  }
  void ExpeditionSeal(Rect r){
   var saved=GUI.matrix;GUI.matrix=saved*Matrix4x4.TRS(new Vector3(r.center.x,r.center.y,0),Quaternion.Euler(0,0,45),Vector3.one);
   float size=r.width*.62f;Box(new Rect(-size/2,-size/2,size,size),new Color(.22f,.025f,.035f));CardOutline(new Rect(-size/2,-size/2,size,size),new Color(.65f,.43f,.22f),3);CardOutline(new Rect(-size/2+5,-size/2+5,size-10,size-10),new Color(.45f,.045f,.065f),2);GUI.matrix=saved;
   HudLine(new Vector2(r.x-8,r.center.y+19),new Vector2(r.x+21,r.center.y+9),new Color(.65f,.06f,.09f),5);HudLine(new Vector2(r.xMax-21,r.center.y+9),new Vector2(r.xMax+8,r.center.y+19),new Color(.65f,.06f,.09f),5);
  }
 }
}
