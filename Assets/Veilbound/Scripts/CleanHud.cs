using System;
using System.Collections.Generic;
using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  Rect DodgeActionRect=>new Rect(520,758,64,64);
  Rect VirtueActionRect=>new Rect(1016,758,64,64);
  readonly Texture2D[] sheetGlyphs=new Texture2D[3];
  void CycleCharacterPage(int direction){if(state!=ScreenState.Character)return;characterTab=(characterTab+direction+3)%3;selection=characterTab;}
  bool TryDodge(){if(state!=ScreenState.Expedition||dodgeCooldown>0)return false;dodgeVector=dodgeFacing||movement.sqrMagnitude<.01f?facing:new Vector3(movement.x,0,movement.y).normalized;dodgeTime=.23f;invulnerability=.35f;dodgeCooldown=2.8f;PlayerCast("dodge",position,1.1f);Burst(position,1.1f,CombatIndicatorColors.Player,.25f);return true;}
  void UtilityActionSlots() {
   bool playing=state==ScreenState.Expedition;DrawUtilityTile(DodgeActionRect,"dodge",gamepadCombat?PadLabel(ControlAction.Dodge)+" / "+PadLabel(ControlAction.DodgeAlt):KeyLabel(ControlAction.Dodge),dodgeCooldown<=0,dodgeCooldown/2.8f,dodgeCooldown>0?dodgeCooldown.ToString("0.0"):null);
   DrawUtilityTile(VirtueActionRect,"rage",RageBinding,RageReady||RageActive,RageActive?rageRemaining/rageWindowDuration:0,RageActive?rageRemaining.ToString("0.0"):null);
   if(playing&&Event.current.type==EventType.MouseDown&&Event.current.button==0){if(DodgeActionRect.Contains(Event.current.mousePosition)){TryDodge();Event.current.Use();}else if(VirtueActionRect.Contains(Event.current.mousePosition)){ActivateRage();Event.current.Use();}}
   if(DodgeActionRect.Contains(Event.current.mousePosition)){
    Rect tip=new Rect(421,579,382,141);HudFrame(tip,true);GUI.Label(new Rect(tip.x+17,tip.y+14,350,27),"DODGE",CardStyle(19,parchment,bold:true));GUI.Label(new Rect(tip.x+17,tip.y+49,350,72),"A short evasive dash. 2.8 second cooldown.\n"+KeyLabel(ControlAction.Dodge)+" / Xbox "+PadLabel(ControlAction.Dodge)+" or "+PadLabel(ControlAction.DodgeAlt)+".\nDash direction follows your direction setting.",CardStyle(13,muted));
   }
  }
  void DrawUtilityTile(Rect r,string id,string binding,bool ready,float cooling,string status) {
   Box(r,new Color(.025f,.031f,.033f,.96f));var icon=UpgradeIcon(id);GUI.color=ready?Color.white:new Color(.48f,.48f,.48f);if(icon)GUI.DrawTexture(new Rect(r.x+4,r.y+4,r.width-8,r.height-8),icon);GUI.color=Color.white;
   if(cooling>0)CooldownSweep(new Rect(r.x+4,r.y+4,r.width-8,r.height-8),cooling,id=="dodge"?dodgeCooldown:rageRemaining);CardOutline(r,id=="rage"&&ready?new Color(1,.50f,.12f):hudGold);
   DrawAscensionBadge(r,id);Rect badge=new Rect(r.center.x-26,r.y-10,52,22);Box(badge,new Color(.10f,.09f,.07f));CardOutline(badge,hudGold);GUI.Label(badge,binding,CardStyle(11,parchment,TextAnchor.MiddleCenter,true));
   if(status!=null&&cooling<=0){var label=new Rect(r.x,r.y+25,r.width,27);GUI.Label(new Rect(label.x+1,label.y+1,label.width,label.height),status,CardStyle(20,Color.black,TextAnchor.MiddleCenter,true));GUI.Label(label,status,CardStyle(20,parchment,TextAnchor.MiddleCenter,true));}
  }
  Texture2D SheetGlyph(int page) {
   if(sheetGlyphs[page])return sheetGlyphs[page];
   return sheetGlyphs[page]=HudTexture("Sheet launcher glyph "+page,64,(u,v)=>{
    float x=(u-.5f)*2,y=(v-.5f)*2;bool ink=false;
    if(page==0){bool head=x*x+(y-.38f)*(y-.38f)<.23f*.23f;bool shoulders=y<.02f&&y>-.68f&&Mathf.Abs(x)<.66f*(1+(y+.1f)*.32f);ink=head||shoulders;}
    else if(page==1){float a=Mathf.Abs(x)+Mathf.Abs(y);ink=a<.79f&&a>.56f||Mathf.Abs(x)<.12f&&Mathf.Abs(y)<.30f||Mathf.Abs(y)<.12f&&Mathf.Abs(x)<.30f;}
    else{ink=(x>-.70f&&x<-.38f&&y>-.65f&&y<-.15f)||(x>-.16f&&x<.16f&&y>-.65f&&y<.27f)||(x>.38f&&x<.70f&&y>-.65f&&y<.70f);}
    return ink?new Color(.88f,.74f,.48f):Color.clear;
   });
  }
  void CompactSheetLauncher() {
   HudFrame(new Rect(1370,789,206,65));if(state!=ScreenState.Expedition)return;string[] names={"Character","Passives","Damage"};string[] keys=gamepadCombat?new[]{PadLabel(ControlAction.Character),PadLabel(ControlAction.Passives),PadLabel(ControlAction.Damage)}:new[]{KeyLabel(ControlAction.Character),KeyLabel(ControlAction.Passives),KeyLabel(ControlAction.Damage)};
   for(int i=0;i<3;i++){int page=i;Rect r=new Rect(1382+i*64,797,44,44);Box(r,new Color(.03f,.038f,.04f));CardOutline(r,hudGold);GUI.DrawTexture(new Rect(r.x+7,r.y+5,30,30),SheetGlyph(i));
    Rect key=new Rect(r.x+18,r.y+31,35,20);Box(key,new Color(.075f,.072f,.060f));CardOutline(key,hudGold);GUI.Label(key,keys[i],CardStyle(gamepadCombat?8:11,parchment,TextAnchor.MiddleCenter,true));
    if(r.Contains(Event.current.mousePosition)||key.Contains(Event.current.mousePosition)){var tip=new Rect(r.x-34,757,116,25);HudFrame(tip);GUI.Label(tip,names[i],CardStyle(12,parchment,TextAnchor.MiddleCenter));if(Event.current.type==EventType.MouseDown&&Event.current.button==0){OpenCharacterSheet(page);Event.current.Use();}}
   }
  }
  void TopDamageMeter() {
   bool expanded=preferences.damageMeterExpanded;Rect toggle=new Rect(1300,20,176,42);ArchiveButton(toggle,"DPS  "+RecentDps().ToString("0.#")+(expanded?"  −":"  +"),()=>{preferences.damageMeterExpanded=!preferences.damageMeterExpanded;SavePreferences();});if(!expanded)return;
   Rect r=new Rect(1300,72,276,128);HudFrame(r);GUI.Label(new Rect(r.x+16,r.y+14,130,24),"DAMAGE / 5s",CardStyle(11,hudLight,bold:true));GUI.Label(new Rect(r.x+129,r.y+7,138,32),RecentDps().ToString("0.#"),CardStyle(24,parchment,TextAnchor.UpperRight,true));HudLine(new Vector2(r.x+16,r.y+44),new Vector2(r.x+269,r.y+44),new Color(.24f,.23f,.18f));
   var entries=new List<KeyValuePair<string,AbilityDamageRecord>>(abilityDamageRecords);entries.Sort((a,b)=>RecentDps(b.Key).CompareTo(RecentDps(a.Key)));entries.RemoveAll(item=>RecentDps(item.Key)<=0);
   if(entries.Count==0)GUI.Label(new Rect(r.x+16,r.y+57,252,30),"No damage in the last 5 seconds.",CardStyle(11,muted));
   for(int i=0;i<Mathf.Min(2,entries.Count);i++){var ability=DamageSource(entries[i].Key);GUI.Label(new Rect(r.x+16,r.y+55+i*22,190,22),ability.name,CardStyle(11,muted));GUI.Label(new Rect(r.x+206,r.y+55+i*22,62,22),RecentDps(ability.id).ToString("0.#"),CardStyle(11,hudLight,TextAnchor.UpperRight));}
   GUI.Label(new Rect(r.x+16,r.y+106,252,16),"Damage breakdown",CardStyle(9,hudGold));if(state==ScreenState.Expedition&&Event.current.type==EventType.MouseDown&&r.Contains(Event.current.mousePosition)){OpenCharacterSheet(2);Event.current.Use();}
  }
 }
}
