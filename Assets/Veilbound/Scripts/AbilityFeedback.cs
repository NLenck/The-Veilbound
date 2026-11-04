using System;
using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  readonly Texture2D[] cooldownMasks=new Texture2D[129];
  Texture2D bleedingStatusIcon,brandStatusIcon,weakenStatusIcon;
  float CooldownSeconds(string id){
   if(id=="vengeance")return Mathf.Max(0,vengeanceReadyAt-elapsed);
   float clock=id=="brand"?brandClock:id=="warcry"?warCryClock:id=="bloodthirst"?bloodClock:0;
   int index=Array.IndexOf(AbilitySpecs.Weapons,id);if(index>=0)clock=attacks[index];
   return Mathf.Max(0,clock)/SkillAttackRate(id);
  }
  static float CooldownMaskAlpha(float u,float v,float remaining){
   float angle=Mathf.Repeat(Mathf.Atan2(u-.5f,v-.5f)/(Mathf.PI*2),1);
   return remaining>0&&angle>=1-Mathf.Clamp01(remaining)?.76f:0;
  }
  void CooldownSweep(Rect rect,float fraction,float seconds){
   if(Event.current.type!=EventType.Repaint||fraction<=0)return;
   int frame=Mathf.Clamp(Mathf.CeilToInt(fraction*128),1,128);
   if(!cooldownMasks[frame])cooldownMasks[frame]=HudTexture("Cooldown sweep "+frame,64,(u,v)=>new Color(.005f,.009f,.014f,CooldownMaskAlpha(u,v,frame/128f)));
   GUI.DrawTexture(rect,cooldownMasks[frame]);
   string text=seconds>=10?Mathf.CeilToInt(seconds).ToString():seconds.ToString("0.0");
   GUI.Label(new Rect(rect.x+1,rect.y+1,rect.width,rect.height),text,CardStyle(19,Color.black,TextAnchor.MiddleCenter,true));
   GUI.Label(rect,text,CardStyle(19,parchment,TextAnchor.MiddleCenter,true));
  }
  bool DebuffActive(Foe foe,string id)=>!foe.dead&&foe.hp>0&&(id=="brand"?foe.brandUntil>elapsed:id=="warcry"?foe.weakUntil>elapsed:foe.bleedRemaining>0);
  Vector3 DebuffAnchor(Foe foe){
   var visual=foe.rig.visual;
   if(visual.bones!=null&&visual.bones.Length>4&&visual.bones[4])return visual.bones[4].position+Vector3.up*(.65f*foe.t.lossyScale.y);
   return foe.t.position+Vector3.up*(2.5f*foe.t.lossyScale.y);
  }
  Texture2D BleedingStatusIcon(){
   if(bleedingStatusIcon)return bleedingStatusIcon;
   return bleedingStatusIcon=HudTexture("Bleeding debuff",64,(u,v)=>{
    float x=(u-.5f)*2,y=(v-.5f)*2;
    bool Drop(float dx,float dy,float size){float px=(x-dx)/size,py=(y-dy)/size;return px*px+(py+.20f)*(py+.20f)<.34f*.34f||py>=-.20f&&py<.66f&&Mathf.Abs(px)<(.66f-py)*.40f;}
    bool drop=Drop(-.28f,.20f,1)||Drop(.32f,-.27f,.65f);
    return drop?new Color(1,.12f,.16f):Color.clear;
   });
  }
  Texture2D BrandStatusIcon(){
   if(brandStatusIcon)return brandStatusIcon;
   return brandStatusIcon=HudTexture("Floating Brand rune",64,(u,v)=>{
    float x=(u-.5f)*2,y=(v-.5f)*2,d=Mathf.Abs(x)/.80f+Mathf.Abs(y)/.47f;
    bool rune=d>.76f&&d<1.06f||x*x+y*y<.15f*.15f||Mathf.Abs(x)<.045f&&Mathf.Abs(y)>.56f&&Mathf.Abs(y)<.84f;
    return rune?new Color(1,.43f,.12f):Color.clear;
   });
  }
  Texture2D WeakenStatusIcon(){
   if(weakenStatusIcon)return weakenStatusIcon;
   return weakenStatusIcon=HudTexture("Floating weakness rune",64,(u,v)=>{
    float x=(u-.5f)*2,y=(v-.5f)*2;
    bool rune=Mathf.Abs(x)<.09f&&y>-.45f&&y<.72f||Mathf.Abs(y-(Mathf.Abs(x)-.67f))<.10f&&Mathf.Abs(x)<.57f||Mathf.Abs(y-.48f)<.06f&&Mathf.Abs(x)>.22f&&Mathf.Abs(x)<.66f;
    return rune?new Color(.82f,.66f,1):Color.clear;
   });
  }
  void DrawEnemyDebuffs(){
   float scale=Mathf.Min(Screen.width/1600f,Screen.height/900f);
   foreach(var foe in foes){
    bool brand=DebuffActive(foe,"brand"),bleed=DebuffActive(foe,"rend"),weaken=DebuffActive(foe,"warcry");if(!brand&&!bleed&&!weaken)continue;
    Vector3 screen=cameraRig.WorldToScreenPoint(DebuffAnchor(foe));if(screen.z<=0||screen.x<0||screen.x>Screen.width||screen.y<0||screen.y>Screen.height)continue;
    float x=(screen.x-(Screen.width-1600*scale)/2)/scale,y=(Screen.height-screen.y-(Screen.height-900*scale)/2)/scale;
    int count=(brand?1:0)+(bleed?1:0)+(weaken?1:0);float left=x-(count*27-3)*.5f;
    if(brand){DrawDebuffIcon(new Rect(left,y-30,24,24),BrandStatusIcon());left+=27;}
    if(bleed){DrawDebuffIcon(new Rect(left,y-30,24,24),BleedingStatusIcon());left+=27;}
    if(weaken)DrawDebuffIcon(new Rect(left,y-30,24,24),WeakenStatusIcon());
   }
  }
  void DrawDebuffIcon(Rect rect,Texture2D icon){GUI.DrawTexture(rect,icon);}
 }
}
