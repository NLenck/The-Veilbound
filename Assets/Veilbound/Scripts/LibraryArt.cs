using System;
using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  Texture2D archiveBackdrop,archiveBook;
  readonly Texture2D[] archiveMaterials=new Texture2D[7];
  readonly Color archiveGold=new Color(.78f,.57f,.30f),archiveIvory=new Color(.94f,.88f,.75f),archiveMuted=new Color(.73f,.69f,.61f),archiveRed=new Color(.86f,.25f,.12f);
  GUIStyle ArchiveType(int size,Color color,bool title=false,TextAnchor align=TextAnchor.UpperLeft){InitCityArt();return CityType(size,color,title,align);}
  void InitArchiveArt(){if(!archiveBackdrop)archiveBackdrop=Resources.Load<Texture2D>("LibraryUI/LibraryBackdrop");if(!archiveBook)archiveBook=PaintArchiveBook();}
  Texture2D ArchiveMaterial(int kind){
   if(archiveMaterials[kind])return archiveMaterials[kind];
   Color[] body={new Color(.061f,.054f,.047f),new Color(.12f,.093f,.063f),new Color(.22f,.105f,.046f),new Color(.055f,.053f,.048f),new Color(.095f,.073f,.045f),new Color(.13f,.052f,.032f),new Color(.075f,.065f,.049f)};
   Color[] trim={new Color(.30f,.25f,.19f),new Color(.53f,.37f,.20f),new Color(.80f,.51f,.23f),new Color(.25f,.25f,.22f),new Color(.83f,.61f,.30f),new Color(.86f,.25f,.12f),new Color(.48f,.36f,.21f)};
   const int w=256,h=256,cut=20;var tex=new Texture2D(w,h,TextureFormat.RGBA32,false){name="Library painted bevel "+kind,wrapMode=TextureWrapMode.Clamp,filterMode=FilterMode.Bilinear};var pixels=new Color[w*h];
   for(int y=0;y<h;y++)for(int x=0;x<w;x++){
    int corner=Math.Min(Math.Min(x+y,w-1-x+y),Math.Min(x+h-1-y,w+h-2-x-y));if(corner<cut){pixels[y*w+x]=Color.clear;continue;}
    int edge=Math.Min(Math.Min(x,w-1-x),Math.Min(y,h-1-y));edge=Math.Min(edge,(corner-cut)*2/3);
    int cx=x/32,cy=y/32,triangle=(x%32+y%32)>32?1:0;int hash=(cx*97+cy*43+triangle*71+kind*19)%29;
    float paint=.93f+hash/200f;Color c=body[kind]*paint;
    if(edge<2)c=trim[kind]*.28f;else if(edge<5)c=trim[kind]*(y>h/2?1.13f:.62f);else if(edge<11)c=Color.Lerp(trim[kind],body[kind],(edge-5)/6f)*(y>h/2?1.1f:.8f);
    else if(edge<14)c=body[kind]*.58f;
    // Broad hand-painted scuffs and a few planar facets, not photographic surface grain.
    if(edge>=14&&edge<22&&((x/43+y/37+kind)%7==0))c=Color.Lerp(c,trim[kind],.10f);
    c.a=1;pixels[y*w+x]=c;
   }tex.SetPixels(pixels);tex.Apply(false,true);archiveMaterials[kind]=tex;return tex;
  }
  void ArchivePlate(Rect r,int kind=0,float edge=14){
   var t=ArchiveMaterial(kind);float b=Mathf.Min(edge,Mathf.Min(r.width,r.height)*.32f);float[] dx={r.x,r.x+b,r.xMax-b,r.xMax},dy={r.y,r.y+b,r.yMax-b,r.yMax};float[] uv={0,.125f,.875f,1};
   for(int y=0;y<3;y++)for(int x=0;x<3;x++)GUI.DrawTextureWithTexCoords(new Rect(dx[x],dy[y],dx[x+1]-dx[x],dy[y+1]-dy[y]),t,new Rect(uv[x],1-uv[y+1],uv[x+1]-uv[x],uv[y+1]-uv[y]),true);
  }
  void ArchiveButton(Rect r,string title,Action action,bool enabled=true,bool active=false,int skin=-1){
   int index=buttons.Count;if(enabled)buttons.Add(action);bool hover=r.Contains(Event.current.mousePosition)&&resourceCategory<0;
   Color prior=GUI.color;GUI.color=enabled?Color.white:new Color(.65f,.65f,.65f);ArchivePlate(r,skin>=0?skin:active?2:1,11);GUI.color=prior;
   if(enabled&&hover)Box(new Rect(r.x+12,r.y+3,r.width-24,1),archiveGold);
   GUI.Label(new Rect(r.x+7,r.y+2,r.width-14,r.height-4),title,ArchiveType(r.height<36?13:16,enabled?archiveIvory:archiveMuted,true,TextAnchor.MiddleCenter));RegisterMenuFocus(r,index,title,enabled);
   if(enabled&&Event.current.type==EventType.MouseDown&&Event.current.button==0&&r.Contains(Event.current.mousePosition)){selection=index;action();Event.current.Use();}
  }
  void ArchiveRule(float x,float y,float width){Box(new Rect(x,y,width,1),new Color(.30f,.24f,.16f));Box(new Rect(x+width*.5f-2,y-1,4,3),archiveGold);}
  Texture2D PaintArchiveBook(){
   const int size=192;var t=new Texture2D(size,size,TextureFormat.RGBA32,false){name="Library open folio emblem",filterMode=FilterMode.Bilinear};var pixels=new Color[size*size];
   Vector2[][] shapes={new[]{new Vector2(.03f,.23f),new Vector2(.44f,.08f),new Vector2(.51f,.17f),new Vector2(.62f,.06f),new Vector2(.98f,.23f),new Vector2(.92f,.86f),new Vector2(.52f,.82f),new Vector2(.08f,.90f)},new[]{new Vector2(.10f,.24f),new Vector2(.43f,.13f),new Vector2(.50f,.29f),new Vector2(.50f,.77f),new Vector2(.14f,.73f)},new[]{new Vector2(.50f,.29f),new Vector2(.62f,.12f),new Vector2(.91f,.25f),new Vector2(.85f,.74f),new Vector2(.50f,.77f)},new[]{new Vector2(.11f,.24f),new Vector2(.31f,.26f),new Vector2(.49f,.72f),new Vector2(.16f,.67f)},new[]{new Vector2(.61f,.14f),new Vector2(.63f,.66f),new Vector2(.51f,.77f),new Vector2(.51f,.30f)},new[]{new Vector2(.12f,.74f),new Vector2(.25f,.76f),new Vector2(.23f,.99f),new Vector2(.18f,.93f),new Vector2(.12f,1)}};
   Color[] colors={new Color(.19f,.12f,.065f),new Color(.68f,.48f,.25f),new Color(.79f,.60f,.34f),new Color(.81f,.62f,.35f),new Color(.60f,.40f,.20f),new Color(.45f,.08f,.055f)};
   for(int y=0;y<size;y++)for(int x=0;x<size;x++){Vector2 p=new Vector2(x/(float)size,1-y/(float)size);Color c=Color.clear;for(int s=0;s<shapes.Length;s++){bool inside=false;var poly=shapes[s];for(int a=0,b=poly.Length-1;a<poly.Length;b=a++)if((poly[a].y>p.y)!=(poly[b].y>p.y)&&p.x<(poly[b].x-poly[a].x)*(p.y-poly[a].y)/(poly[b].y-poly[a].y)+poly[a].x)inside=!inside;if(inside)c=colors[s];}pixels[y*size+x]=c;}
   t.SetPixels(pixels);t.Apply(false,true);return t;
  }
  void ArchiveResourceHeader(){
   ArchivePlate(new Rect(30,18,1540,96),0,13);GUI.Label(new Rect(52,35,155,29),"SANCTUARY",ArchiveType(19,archiveIvory,true));GUI.Label(new Rect(52,75,250,23),CityClock+" / ×1",ArchiveType(12,archiveMuted));
   ArchiveButton(new Rect(213,36,93,30),"Settings",OpenSettings);
   for(int i=0;i<6;i++){int group=i;Rect r=ResourceCategoryRect(i);ArchivePlate(r,resourceCategory==i||resourceFocus&&resourceFocusCategory==i?2:6,10);var icon=MaterialIcon(ResourceCategoryIcons[i]);if(icon)GUI.DrawTexture(new Rect(r.x+8,r.y+11,44,44),icon);GUI.Label(new Rect(r.x+61,r.y+9,r.width-66,28),ResourceCategories[i],ArchiveType(12,archiveIvory));GUI.Label(new Rect(r.x+61,r.y+37,r.width-66,24),Math.Floor(Math.Max(0,ResourceCategoryStock(i))).ToString("N0"),ArchiveType(17,archiveIvory,true));int index=buttons.Count;buttons.Add(()=>ToggleResourceCategory(group));RegisterMenuFocus(r,index,ResourceCategories[i],true);if(resourceFocus&&resourceFocusCategory==i&&resourceCategory<0)DrawMenuNavigationFocus(r,ResourceCategories[i]);}
   if(resourceCategory>=0)ResourceDropdown();
  }
 }
}
