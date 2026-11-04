using System;
using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  void DrawAbilityLoadout() {
   string hover=null;int slot=0;
   foreach(var ability in RunBuild.Pool)if(ability.ability&&ability.id!="rage"&&build.Rank(ability.id)>0){string id=ability.id;Rect r=new Rect(624+slot++*72,758,64,64);AbilityTile(r,id);if(r.Contains(Event.current.mousePosition))hover=id;}
   for(;slot<5;slot++)EmptyAbilityTile(new Rect(624+slot*72,758,64,64));
   GUI.Label(new Rect(630,730,340,18),build.Slots+" / 5 EQUIPPED ABILITIES",CardStyle(10,hudGold,TextAnchor.MiddleCenter,true));
   UtilityActionSlots();
   if(smoke&&reviewAbilityHover!=null)hover=reviewAbilityHover;
   if(hover!=null)AbilityTooltip(hover);
  }
  void EmptyAbilityTile(Rect r){Box(r,new Color(.025f,.03f,.033f,.8f));CardOutline(r,new Color(.15f,.17f,.16f));GUI.Label(r,"+",CardStyle(23,new Color(.24f,.25f,.22f),TextAnchor.MiddleCenter));}
  void AbilityTile(Rect r,string id) {
   Box(r,new Color(.025f,.03f,.033f,.96f));var icon=UpgradeIcon(id);if(icon)GUI.DrawTexture(new Rect(r.x+4,r.y+4,r.width-8,r.height-8),icon);CardOutline(r,AbilitySpecs.IsVirtue(id)?new Color(.61f,.25f,.16f):hudGold);
   Rect badge=new Rect(r.center.x-13,r.y-10,26,21);Box(badge,new Color(.10f,.09f,.07f));CardOutline(badge,hudGold);GUI.Label(badge,build.Rank(id).ToString(),CardStyle(12,parchment,TextAnchor.MiddleCenter,true));
   float remaining=id=="brand"?brandClock:id=="warcry"?warCryClock:id=="bloodthirst"?bloodClock:0;
   if(remaining>0){Box(new Rect(r.x+4,r.yMax-8,r.width-8,4),new Color(.07f,.05f,.04f));Box(new Rect(r.x+4,r.yMax-8,(r.width-8)*(1-Mathf.Clamp01(remaining/SkillBaseCooldown(id))),4),new Color(.85f,.31f,.12f));}
  }
  void AbilityTooltip(string id) {
   var ability=Array.Find(RunBuild.Pool,c=>c.id==id);if(ability==null)return;
   Rect r=new Rect(530,424,540,212);HudFrame(r,true);GUI.Label(new Rect(r.x+18,r.y+14,504,28),ability.name+" / LEVEL "+build.Rank(id),CardStyle(21,parchment,bold:true));
   GUI.Label(new Rect(r.x+18,r.y+53,504,79),AbilityEffectText(id),CardStyle(15,parchment));
   string extra=AbilitySpecs.IsVirtue(id)?(id=="rage"?"Full-Rage activation":id=="vengeance"?"Reactive counterattack":id=="rend"?"Weapon-hit passive / ticks every 0.5s":$"Automatic / {AbilityCooldown(id):0.##}s interval"):$"{CritChance:P1} critical chance / {CritMultiplier:P1} critical damage";
   if(id!="rend"&&id!="rage")extra=$"{SkillHasteRating(id):0.#}% Haste rating / {HasteRules.Reduction(SkillHasteRating(id))*100:0.#}% effective reduction / {AbilityCooldown(id):0.##}s interval";
   GUI.Label(new Rect(r.x+18,r.y+142,504,48),extra+"\nLevel counts investments. Only the chosen attribute improves.",CardStyle(12,muted));
  }
  Texture2D RageKitIcon(string id) {
   string key="virtue-"+id;if(upgradeIcons.TryGetValue(key,out var cached))return cached;
   return upgradeIcons[key]=HudTexture(key,256,(u,v)=>{
    float x=(u-.5f)*2,y=(v-.5f)*2;bool glyph=false;
    if(id=="brand"){float d=Mathf.Abs(x)/.74f+Mathf.Abs(y)/.43f;glyph=d>.78f&&d<1.05f||x*x+y*y<.14f*.14f||Mathf.Abs(x)<.035f&&Mathf.Abs(y)>.52f&&Mathf.Abs(y)<.76f;}
    else if(id=="warcry"){float d=x*x/.48f+y*y/.44f;bool rim=d>.72f&&d<1;float teeth=Mathf.Abs((x+.60f)%.24f-.12f);glyph=rim||Mathf.Abs(x)<.6f&&((y>.20f&&y<.45f-teeth*1.5f)||(y<-.20f&&y>-.45f+teeth*1.5f));}
    else if(id=="bloodthirst"){bool drop=(x*x+(y+.20f)*(y+.20f)<.37f*.37f)||(y>=-.1f&&y<.68f&&Mathf.Abs(x)<(.68f-y)*.48f);bool cut=Mathf.Abs(y-x*.80f)<.065f&&Mathf.Abs(x)<.50f;glyph=drop&&!cut;}
    else if(id=="rend"){for(int i=-1;i<=1;i++){float slash=x-y*.42f-i*.34f;glyph|=Mathf.Abs(slash)<.065f&&Mathf.Abs(y)<.65f-Mathf.Abs(i)*.09f;}}
    else if(id=="vengeance"){float edge=.62f-Mathf.Max(0,-y)*.5f;bool shield=y<.65f&&y>-.7f&&Mathf.Abs(x)<edge&&Mathf.Abs(x)>edge-.09f;bool blade=Mathf.Abs(x-y*.45f)<.065f&&y>-.63f&&y<.72f;bool cross=Mathf.Abs(y+.33f)<.06f&&Mathf.Abs(x+.15f)<.26f;glyph=shield||blade||cross;}
    bool border=Mathf.Abs(x)>.94f||Mathf.Abs(y)>.94f;
    if(border)return new Color(.43f,.13f,.07f);if(glyph)return Color.Lerp(new Color(.60f,.035f,.012f),new Color(1,.35f,.12f),Mathf.Clamp01((x+y+1)*.5f));
    return new Color(.035f+.018f*(1-u),.014f,.012f,1);
   });
  }
 }
}
