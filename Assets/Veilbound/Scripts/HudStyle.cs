using System;
using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  Texture2D globeWell,globeLife,globeRage,globeGlass,globeRim,metalGrain,bossGlyph;
  readonly Color hudGold=new Color(.70f,.53f,.30f),hudLight=new Color(.91f,.78f,.52f);
  float HealthGlobeFill=>Mathf.Clamp01(health/Mathf.Max(1,MaxHealth));
  float ExperienceBarFill=>Mathf.Clamp01(xp/Mathf.Max(1,Threshold));
  float ExpeditionFill=>Mathf.Clamp01(elapsed/Duration);
  float BossMilestone(int rank)=>Duration*rank/3;
  Rect ExpeditionTrack=>new Rect(380,54,840,9);
  static Texture2D HudTexture(string name,int size,Func<float,float,Color> pixel) {
   var t=new Texture2D(size,size,TextureFormat.RGBA32,false){name=name,filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};var colors=new Color[size*size];
   for(int y=0;y<size;y++)for(int x=0;x<size;x++)colors[y*size+x]=pixel((x+.5f)/size,(y+.5f)/size);
   t.SetPixels(colors);t.Apply(false,true);return t;
  }
  void InitHudArt() {
   if(globeWell)return;
   globeWell=HudTexture("Empty obsidian vessel",256,(u,v)=>{float x=(u-.5f)*2,y=(v-.5f)*2,r=Mathf.Sqrt(x*x+y*y);if(r>.835f)return Color.clear;float light=Mathf.Pow(Mathf.Max(0,1-r*r),.5f);return new Color(.012f+light*.017f,.016f+light*.018f,.019f+light*.019f,Mathf.Clamp01((.835f-r)*150));});
   Func<Color,Texture2D> liquid=color=>HudTexture("Globe liquid",256,(u,v)=>{float x=(u-.5f)*2,y=(v-.5f)*2,r=Mathf.Sqrt(x*x+y*y);if(r>.823f)return Color.clear;float edge=Mathf.Pow(Mathf.Max(0,1-r*r),.7f),light=.25f+.69f*edge+.18f*v;float mist=Mathf.Sin(u*31+Mathf.Sin(v*17)*2)*Mathf.Sin(v*39+u*11)*.035f;Color c=color*(light+mist);c.a=Mathf.Clamp01((.823f-r)*160);return c;});
   globeLife=liquid(new Color(.86f,.025f,.045f));globeRage=liquid(new Color(1,.20f,.018f));
   globeGlass=HudTexture("Crystal vessel highlights",256,(u,v)=>{float x=(u-.5f)*2,y=(v-.5f)*2,r=Mathf.Sqrt(x*x+y*y);if(r>.83f)return Color.clear;float shine=Mathf.Exp(-Mathf.Pow((x+.31f)/.24f,2)-Mathf.Pow((y-.43f)/.12f,2))*.22f;float crescent=Mathf.Exp(-Mathf.Pow((r-.78f)/.018f,2))*Mathf.Max(0,(y-x)*.35f);return new Color(.80f,.88f,.94f,shine+crescent*.27f);});
   globeRim=HudTexture("Aged bronze globe housing",256,(u,v)=>{float x=(u-.5f)*2,y=(v-.5f)*2,r=Mathf.Sqrt(x*x+y*y);if(r>.99f||r<.825f)return Color.clear;float light=.52f+y*.29f-x*.16f;float band=(r>.96f||r<.85f)?.9f:r>.92f?.28f:.64f;float grooves=Mathf.Cos(Mathf.Atan2(y,x)*48)*.035f;Color c=new Color(.60f,.43f,.25f)*(light*band+grooves);if(r>.975f||r<.835f)c=new Color(.07f,.065f,.055f);c.a=Mathf.Clamp01((.99f-r)*180);return c;});
   metalGrain=HudTexture("Brushed dark iron",96,(u,v)=>{float n=Mathf.Sin(u*1281+v*71)*Mathf.Cos(v*933-u*139)*.009f;return new Color(.038f+n,.046f+n,.046f+n,1);});
   bossGlyph=HudTexture("Boss skull milestone",64,(u,v)=>{float x=(u-.5f)*2,y=(v-.5f)*2;bool skull=x*x/.46f+y*y/.51f<1&&y>-.35f;bool jaw=Mathf.Abs(x)<.36f&&y<-.20f&&y>-.76f;bool horn=Mathf.Abs(x)>.44f&&Mathf.Abs(x)<.9f&&y>.17f&&y<.66f&&Mathf.Abs(x)<.9f-(y-.17f)*.5f;if(!skull&&!jaw&&!horn)return Color.clear;bool eye=(Mathf.Pow((Mathf.Abs(x)-.29f)/.16f,2)+Mathf.Pow((y-.06f)/.17f,2)<1);bool nose=Mathf.Abs(x)<.08f&&y<-.15f&&y>-.37f;bool teeth=y<-.47f&&Mathf.Abs(Mathf.Sin(x*19))>.79f;return eye||nose||teeth?new Color(.03f,.025f,.02f):Color.white;});
  }
  void HudLine(Vector2 a,Vector2 b,Color color,float width=1) {
   var matrix=GUI.matrix;float angle=Mathf.Atan2(b.y-a.y,b.x-a.x)*Mathf.Rad2Deg;GUI.matrix=matrix*Matrix4x4.TRS(new Vector3(a.x,a.y,0),Quaternion.Euler(0,0,angle),Vector3.one);Box(new Rect(0,-width*.5f,Vector2.Distance(a,b),width),color);GUI.matrix=matrix;
  }
  void HudDiamond(Vector2 center,float size,Color color) {
   HudLine(center+Vector2.up*size,center+Vector2.right*size,color);HudLine(center+Vector2.right*size,center+Vector2.down*size,color);HudLine(center+Vector2.down*size,center+Vector2.left*size,color);HudLine(center+Vector2.left*size,center+Vector2.up*size,color);
  }
  void HudFrame(Rect r,bool lit=false) {
   InitHudArt();Box(new Rect(r.x+4,r.y+5,r.width,r.height),new Color(0,0,0,.32f));GUI.DrawTexture(r,metalGrain);Box(r,new Color(.015f,.019f,.019f,.20f));
   CardOutline(r,lit?hudGold:new Color(.27f,.25f,.20f));CardOutline(new Rect(r.x+3,r.y+3,r.width-6,r.height-6),new Color(.095f,.10f,.09f));
   float n=11;foreach(float x in new[]{r.x,r.xMax-n}){Box(new Rect(x,r.y,n,2),hudGold);Box(new Rect(x,r.yMax-2,n,2),hudGold);}foreach(float y in new[]{r.y,r.yMax-n}){Box(new Rect(r.x,y,2,n),hudGold);Box(new Rect(r.xMax-2,y,2,n),hudGold);}
  }
  void DrawGlobe(Rect r,float fill,bool life) {
   InitHudArt();GUI.color=new Color(0,0,0,.5f);GUI.DrawTexture(new Rect(r.x+3,r.y+5,r.width,r.height),globeWell);GUI.color=Color.white;
   GUI.DrawTexture(r,globeWell);fill=Mathf.Clamp01(fill);
   // Clip the tinted sphere vertically, retaining a circular silhouette at every fill level.
   float inset=r.width*.0875f,inside=r.height*.825f,fluidHeight=inside*fill,top=r.y+inset+inside-fluidHeight;
   if(fill>0){GUI.BeginGroup(new Rect(r.x,top,r.width,fluidHeight));GUI.DrawTexture(new Rect(0,r.y-top,r.width,r.height),life?globeLife:globeRage);GUI.EndGroup();
    if(fill<.995f){float radius=r.width*.4115f,dy=(.5f-fill)*inside,chord=Mathf.Sqrt(Mathf.Max(0,radius*radius-dy*dy));float center=r.x+r.width*.5f;Color surface=life?new Color(.93f,.22f,.23f,.8f):new Color(1,.57f,.12f,.8f);HudLine(new Vector2(center-chord+2,top+1),new Vector2(center+chord-2,top+1),surface,1.3f);}
   }
   GUI.DrawTexture(r,globeGlass);GUI.DrawTexture(r,globeRim);
   Vector2 c=r.center;HudDiamond(new Vector2(c.x,r.y+2),5,hudLight);HudDiamond(new Vector2(c.x,r.yMax-2),5,hudGold);
   HudLine(new Vector2(r.x-13,c.y),new Vector2(r.x+2,c.y),hudGold,2);HudLine(new Vector2(r.xMax-2,c.y),new Vector2(r.xMax+13,c.y),hudGold,2);
  }
  void GlobeDock() {
   DrawGlobe(new Rect(350,700,140,140),HealthGlobeFill,true);DrawGlobe(new Rect(1110,700,140,140),RageFill,false);
   GUI.Label(new Rect(325,679,190,20),"LIFE",CardStyle(11,hudLight,TextAnchor.MiddleCenter,true));
   GUI.Label(new Rect(1085,679,190,20),RageActive?"RAGE / "+rageRemaining.ToString("0.0")+"s":RageReady?"RAGE READY":"RAGE",CardStyle(11,RageReady||RageActive?new Color(1,.50f,.12f):hudLight,TextAnchor.MiddleCenter,true));
   GUI.Label(new Rect(320,845,200,25),$"{Mathf.Max(0,health):0.#} / {MaxHealth:0.#}",CardStyle(17,parchment,TextAnchor.MiddleCenter,true));
   GUI.Label(new Rect(1080,845,200,25),$"{Mathf.Max(0,rage):0.#} / {RageMaximum:0}",CardStyle(17,parchment,TextAnchor.MiddleCenter,true));
   RageGlobeControl();
  }
  void ExpeditionTimeline() {
   HudFrame(new Rect(344,18,912,80));var track=ExpeditionTrack;
   GUI.Label(new Rect(365,27,270,24),ExpeditionMapName(activeMap).ToUpper()+" / DEPTH "+VeilDepth,CardStyle(12,hudLight,bold:true));
   GUI.Label(new Rect(650,24,300,28),Clock(elapsed)+"  /  "+Clock(Duration),CardStyle(20,parchment,TextAnchor.MiddleCenter,true));
   GUI.Label(new Rect(993,27,242,24),bossesDead+" / 3 ABERRATIONS",CardStyle(12,muted,TextAnchor.UpperRight));
   Box(new Rect(track.x-2,track.y-2,track.width+4,track.height+4),new Color(.012f,.015f,.015f));Box(track,new Color(.12f,.13f,.12f));Box(new Rect(track.x,track.y,track.width*ExpeditionFill,track.height),new Color(.58f,.35f,.14f));Box(new Rect(track.x,track.y,track.width*ExpeditionFill,2),hudLight);
   for(int i=1;i<15;i++){float x=track.x+track.width*i/15;HudLine(new Vector2(x,track.yMax+3),new Vector2(x,track.yMax+(i%5==0?10:6)),new Color(.30f,.28f,.21f));}
   for(int rank=1;rank<=3;rank++){
    float time=BossMilestone(rank),x=track.x+track.width*time/Duration;bool spawned=bossesSpawned>=rank;Color color=spawned?new Color(.76f,.25f,.17f):hudLight;
    Box(new Rect(x-13,track.y-10,26,28),new Color(.045f,.04f,.032f));HudDiamond(new Vector2(x,track.y+4),17,color);GUI.color=color;GUI.DrawTexture(new Rect(x-10,track.y-7,20,22),bossGlyph);GUI.color=Color.white;
    GUI.Label(new Rect(x-40,75,80,18),Clock(time),CardStyle(11,spawned?new Color(.74f,.43f,.30f):muted,TextAnchor.UpperCenter));
    if(new Rect(x-18,track.y-15,36,40).Contains(Event.current.mousePosition)){
     Rect tip=new Rect(Mathf.Clamp(x-135,344,980),115,270,24);HudFrame(tip);GUI.Label(new Rect(tip.x+8,tip.y+3,254,18),BossName(rank)+"  /  "+Clock(time),CardStyle(11,parchment,TextAnchor.MiddleCenter,true));
    }
   }
  }
  void ExpeditionIdentity() {
   HudFrame(new Rect(24,20,265,65));var icon=UpgradeIcon("rage");if(icon)GUI.DrawTexture(new Rect(38,32,36,36),icon);
   GUI.Label(new Rect(85,26,190,27),city.virtue.ToString().ToUpper(),CardStyle(22,hudLight,bold:true));
   GUI.Label(new Rect(85,56,190,22),"LEVEL "+level+"  /  "+kills+" SLAIN",CardStyle(11,muted));
  }
  void SettlementLedger() {
   HudFrame(new Rect(1300,20,276,112));GUI.Label(new Rect(1317,32,244,23),$"SETTLEMENT   ×{Multiplier:0.0}",CardStyle(13,hudLight,bold:true));
   HudLine(new Vector2(1317,59),new Vector2(1557,59),new Color(.25f,.23f,.18f));
   GUI.Label(new Rect(1317,68,125,48),$"TIMBER  +{city.wood-startWood:0}\nORES  +{Math.Floor(city.ResourceTotal(0)-startOreUnits):0}\nHIDES  +{Math.Floor(city.ResourceTotal(1)-startHideUnits):0}",CardStyle(10,muted));
   GUI.Label(new Rect(1446,68,114,48),$"STONE  +{city.stone-startStone:0}\nTEXTILES  +{Math.Floor(city.ResourceTotal(2)-startTextileUnits):0}",CardStyle(10,muted));
  }
  void HudSheetControls() {CompactSheetLauncher();}
 }
}

