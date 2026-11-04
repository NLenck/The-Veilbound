using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  Rect ExperienceBarRect {get{float scale=Mathf.Min(Screen.width/1600f,Screen.height/900f);float left=(Screen.width-1600*scale)*.5f/scale,extraBottom=(Screen.height-900*scale)*.5f/scale;return new Rect(-left,900+extraBottom-22,Screen.width/scale,22);}}
  void ExperienceBar() {
   Rect r=ExperienceBarRect;HudFrame(r);Rect inside=new Rect(r.x+4,r.y+4,r.width-8,r.height-8);Box(inside,new Color(.025f,.018f,.045f));
   float width=inside.width*ExperienceBarFill;Box(new Rect(inside.x,inside.y,width,inside.height),new Color(.31f,.17f,.62f));Box(new Rect(inside.x,inside.y,width,2),new Color(.71f,.53f,.96f));
   for(int i=1;i<20;i++){float x=inside.x+inside.width*i/20;Box(new Rect(x,inside.y,1,inside.height),new Color(.08f,.05f,.12f,.55f));}
   HudLine(new Vector2(inside.x,inside.yMax),new Vector2(inside.xMax,inside.yMax),new Color(.10f,.07f,.15f));
   string text=$"XP   {Mathf.Max(0,xp):0.#} / {Threshold:0.#}";var label=new Rect(r.center.x-180,r.y+1,360,20);GUI.Label(new Rect(label.x+1,label.y+1,label.width,label.height),text,CardStyle(12,Color.black,TextAnchor.MiddleCenter,true));GUI.Label(label,text,CardStyle(12,parchment,TextAnchor.MiddleCenter,true));
  }
  void RageGlobeControl() {
   Color ember=new Color(1,.50f,.12f);
   if(RageActive)GUI.Label(new Rect(580,620,440,23),$"FURY   +{RageDamageBonus*100:0.#}% DAMAGE / +{RageAttackBonus*100:0.#}% ATTACK SPEED",CardStyle(11,ember,TextAnchor.MiddleCenter,true));
   if(new Rect(1110,700,140,140).Contains(Event.current.mousePosition)||VirtueActionRect.Contains(Event.current.mousePosition)||(smoke&&reviewRageTooltip)){
    Rect tip=new Rect(931,469,395,197);HudFrame(tip,true);var icon=UpgradeIcon("rage");if(icon)GUI.DrawTexture(new Rect(tip.x+17,tip.y+12,35,35),icon);GUI.Label(new Rect(tip.x+65,tip.y+14,313,27),"BERSERKER'S FURY",CardStyle(19,ember,bold:true));
    GUI.Label(new Rect(tip.x+17,tip.y+49,360,27),$"{RageBlastDamage:0} damage  /  {SkillRadius("rage"):0.#} m explosion",CardStyle(16,parchment));
    GUI.Label(new Rect(tip.x+17,tip.y+83,360,92),$"+{RageDamageBonus*100:0.#}% damage / +{RageAttackBonus*100:0.#}% Haste rating for {RageDuration:0.#}s.\nRage drains to zero; no gain while active.\n\nGain: 2% of damage dealt + 10% taken.\nRequires 100 Rage. Xbox: {PadLabel(ControlAction.Virtue)} / key: {KeyLabel(ControlAction.Virtue)}.",CardStyle(12,muted));
   }
  }
 }
}
