using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  Texture2D smithBackdrop,smithIconAtlas;
  // Per-icon UV bounds keep each generated glyph isolated, including the diagonal sword.
  static readonly Rect[] SmithIconBounds={new Rect(.04f,.03f,.20f,.31f),new Rect(.26f,.07f,.25f,.25f),new Rect(.54f,.04f,.20f,.30f),new Rect(.79f,.04f,.18f,.30f),new Rect(.028f,.43f,.25f,.18f),new Rect(.30f,.34f,.21f,.30f),new Rect(.54f,.35f,.20f,.30f),new Rect(.76f,.36f,.22f,.29f),new Rect(.02f,.635f,.28f,.33f),new Rect(.28f,.72f,.23f,.23f),new Rect(.52f,.70f,.25f,.265f),new Rect(.80f,.66f,.17f,.305f)};
  void InitSmithArt(){InitArchiveArt();if(!smithBackdrop)smithBackdrop=Resources.Load<Texture2D>("BlacksmithUI/BlacksmithBackdrop");if(!smithIconAtlas)smithIconAtlas=Resources.Load<Texture2D>("BlacksmithUI/SmithIcons");}
  Color SmithTint(int metal,bool locked=false){Color baseColor=Metals.All[WeaponRules.Material(metal)].color;float peak=Mathf.Max(baseColor.r,Mathf.Max(baseColor.g,baseColor.b));Color c=Color.Lerp(baseColor*(.9f/Mathf.Max(.1f,peak)),Color.white,.08f);return locked?new Color(c.r*.42f,c.g*.42f,c.b*.42f,1):c;}
  void SmithIcon(Rect rect,int icon,Color color){if(!smithIconAtlas)return;Rect b=SmithIconBounds[Mathf.Clamp(icon,0,11)];float aspect=b.width*smithIconAtlas.width/(b.height*smithIconAtlas.height),w=Mathf.Min(rect.width,rect.height*aspect),h=w/aspect;Rect fit=new Rect(rect.center.x-w/2,rect.center.y-h/2,w,h);Color old=GUI.color;GUI.color=color;GUI.DrawTextureWithTexCoords(fit,smithIconAtlas,new Rect(b.x,1-b.yMax,b.width,b.height),true);GUI.color=old;}
  void SmithLock(Rect r){SmithIcon(r,11,new Color(.65f,.64f,.58f));}
 }
}
