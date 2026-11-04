using System;
using System.Collections.Generic;
using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  readonly Dictionary<string,Texture2D> upgradeIcons=new Dictionary<string,Texture2D>();
  readonly float[] upgradeLift=new float[3];
  float choiceOpenedAt; bool upgradePointerFocus=true; Vector2 lastUpgradePointer;
  static string UpgradeIconName(string id) {
   switch(id){
    case "dodge":return "dodge";case "rage":return "rage";case "cleave":return "basic-swing";case "whirl":return "whirl";case "orbit":return "whirling-blade";
    case "smite":return "earth-hammer";case "lance":return "dagger-toss";case "power":return "damage";
    case "haste":return "attack-speed";case "vitality":return "health";case "guard":return "damage-reduction";
    case "regen":return "health-regen";case "speed":return "move-speed";case "wisdom":return "xp-gain";
    case "reach":return "xp-pull";case "crit":return "crit-chance";case "critdamage":return "crit-damage";case "area":return "area";default:return "fortune";
   }
  }
  Texture2D UpgradeIcon(string id) {
   if(AbilitySpecs.IsVirtue(id)&&id!="rage")return RageKitIcon(id);
   string name=UpgradeIconName(id);
   if(!upgradeIcons.TryGetValue(name,out var texture)){texture=Resources.Load<Texture2D>("UpgradeIcons/"+name);upgradeIcons[name]=texture;}
   return texture;
  }
  GUIStyle CardStyle(int size,Color color,TextAnchor alignment=TextAnchor.UpperLeft,bool bold=false) {
   return new GUIStyle(textStyle){fontSize=Mathf.Max(12,size)+(preferences.largeText&&size<18?2:0),alignment=alignment,fontStyle=bold?FontStyle.Bold:FontStyle.Normal,normal={textColor=color},padding=new RectOffset(0,0,0,0)};
  }
  void CardOutline(Rect r,Color color,float width=1) {
   Box(new Rect(r.x,r.y,r.width,width),color);Box(new Rect(r.x,r.yMax-width,r.width,width),color);
   Box(new Rect(r.x,r.y,width,r.height),color);Box(new Rect(r.xMax-width,r.y,width,r.height),color);
  }
  Rect UpgradeCardRect(int index) => new Rect((build.AscensionRound?399:190)+index*418,231-upgradeLift[index],384,478);
  void UpgradeCard(int index,Choice choice,bool pointerOnCards) {
   var initial=UpgradeCardRect(index);bool hover=upgradePointerFocus&&initial.Contains(Event.current.mousePosition);
   if(hover)selection=index;bool focused=upgradePointerFocus?hover:selection==index;
   if(Event.current.type==EventType.Repaint)upgradeLift[index]=Mathf.MoveTowards(upgradeLift[index],focused?9:0,Time.unscaledDeltaTime*65);
   Rect r=UpgradeCardRect(index);float x=r.x,y=r.y;
   float enter=Mathf.Clamp01((Time.unscaledTime-choiceOpenedAt-index*.055f)/.24f);y+=(1-enter)*(1-enter)*24;r.y=y;
   Color rarity=Rarities.Colour(choice.rarity),gold=new Color(.83f,.67f,.41f),light=new Color(.94f,.90f,.82f);
   Box(new Rect(x+7,y+10,r.width,r.height),new Color(0,0,0,.36f));
   Box(r,focused?new Color(.105f,.100f,.086f):new Color(.058f,.063f,.063f));
   CardOutline(r,focused?rarity:Color.Lerp(new Color(.22f,.23f,.22f),rarity,.45f),focused?2:1);
   CardOutline(new Rect(x+7,y+7,r.width-14,r.height-14),new Color(.24f,.23f,.19f));
   Box(new Rect(x+24,y,r.width-48,3),rarity);
   foreach(float cx in new[]{x,x+r.width-17})foreach(float cy in new[]{y,y+r.height-3})Box(new Rect(cx,cy,17,3),rarity);
   GUI.Label(new Rect(x+24,y+19,270,24),choice.ascension>0?"ASCENSION "+(choice.ascension==1?"I":"II"):choice.rarity.ToString().ToUpperInvariant(),CardStyle(14,rarity,bold:true));
   Box(new Rect(x+r.width-48,y+17,25,25),new Color(.14f,.14f,.12f));
   GUI.Label(new Rect(x+r.width-48,y+17,25,25),(index+1).ToString(),CardStyle(14,gold,TextAnchor.MiddleCenter,true));
   Rect art=new Rect(x+96,y+54,192,192);var icon=UpgradeIcon(choice.id);
   Box(new Rect(art.x-5,art.y-5,202,202),new Color(.035f,.040f,.039f));
   if(icon)GUI.DrawTexture(art,icon,ScaleMode.ScaleToFit);
   CardOutline(new Rect(art.x-5,art.y-5,202,202),new Color(.25f,.24f,.19f));
   string type=choice.ascension>0?"ABILITY LEVEL 7 / CHOOSE A PATH":choice.ability?(build.Rank(choice.id)>0?(choice.virtue?"RAGE VIRTUE":"WEAPON")+"  /  ATTRIBUTE UPGRADE":(choice.virtue?"RAGE VIRTUE":"WEAPON")+"  /  NEW ABILITY"):"PASSIVE  /  CHARACTER UPGRADE";
   GUI.Label(new Rect(x+23,y+263,338,22),type,CardStyle(11,gold,TextAnchor.UpperCenter,true));
   GUI.Label(new Rect(x+20,y+291,344,44),(choice.ascension==0&&choice.ability?AbilityDisplayName(choice.id):choice.name),CardStyle(27,light,TextAnchor.UpperCenter,true));
   int rank=build.Rank(choice.id);
   GUI.Label(new Rect(x+24,y+332,336,20),choice.ascension>0?"UPGRADES KEPT / SAME ABILITY SLOT":rank==0&&choice.ability?(choice.Strength>1?"UNLOCK / ENHANCED "+AbilitySpecs.Upgrades(choice.id)[0].label.ToUpperInvariant():"UNLOCK / BASE VALUES"):"LEVEL "+rank+"  >  "+(rank+1),CardStyle(12,muted,TextAnchor.UpperCenter));
   Box(new Rect(x+37,y+361,310,1),new Color(.26f,.25f,.21f));
   if(choice.ascension>0)GUI.Label(new Rect(x+27,y+372,330,66),choice.description,CardStyle(13,light,TextAnchor.UpperCenter));else if(choice.ability){GUI.Label(new Rect(x+23,y+375,338,23),AbilityCardDamage(choice),CardStyle(17,gold,TextAnchor.UpperCenter,true));GUI.Label(new Rect(x+27,y+401,330,31),AbilityCardSummary(choice),CardStyle(13,light,TextAnchor.UpperCenter));}else GUI.Label(new Rect(x+27,y+376,330,58),build.Describe(choice),CardStyle(build.Describe(choice).Length>110?14:16,light,TextAnchor.UpperCenter));
   Rect action=new Rect(x+17,y+437,350,27);
   Box(action,focused?gold:new Color(.14f,.14f,.12f));
   if(focused)DrawMenuNavigationFocus(r,choice.name);
   GUI.Label(action,choice.ascension>0?"CHOOSE THIS ASCENSION":build.locked==choice.Key?"SELECT UPGRADE  /  LOCKED":"SELECT UPGRADE",CardStyle(12,focused?new Color(.08f,.075f,.06f):gold,TextAnchor.MiddleCenter,true));
   int picked=index;RegisterChoiceNode(buttons.Count,0,index);buttons.Add(()=>Choose(picked));
   if(Event.current.type==EventType.MouseDown&&Event.current.button==0&&r.Contains(Event.current.mousePosition)){selection=index;Choose(index);Event.current.Use();}
  }
  void ChoiceScreen() {
   choiceNodes.Clear();
   Box(new Rect(0,0,1600,900),new Color(.018f,.023f,.025f,.95f));
   Color gold=new Color(.83f,.67f,.41f);
   GUI.Label(new Rect(190,48,1220,25),build.AscensionRound?"LEVEL 7   /   ABILITY ASCENSION":build.PassiveRound?"RELIC DISCOVERED   /   THE VEIL OFFERS A CHOICE":"LEVEL "+level.ToString("00")+"   /   THE VEIL OFFERS A CHOICE",CardStyle(13,gold,TextAnchor.MiddleCenter,true));
   GUI.Label(new Rect(190,86,1220,55),build.AscensionRound?"CHOOSE YOUR ASCENSION":build.PassiveRound?"CHOOSE YOUR PASSIVE":"CHOOSE YOUR ASCENT",CardStyle(40,parchment,TextAnchor.MiddleCenter,true));
   GUI.Label(new Rect(190,153,1220,30),build.AscensionRound?"Choose one permanent form for "+Array.Find(RunBuild.Pool,c=>c.id==build.AscendingAbility).name+".":build.PassiveRound?"Choose one passive for this expedition.":build.AbilityRound?"Choose a weapon or Virtue ability. Five equipped slots in total.":"Upgrade one ability attribute. Find relics for passive upgrades.",CardStyle(18,muted,TextAnchor.MiddleCenter));
   if(build.offers.Count==0){GUI.Label(new Rect(190,320,1220,55),"No eligible upgrades remain.",CardStyle(22,muted,TextAnchor.MiddleCenter));CardUtilityButton(new Rect(633,430,334,40),"CONTINUE",()=>SetState(ScreenState.Expedition),true,2,0);return;}
   Box(new Rect(685,200,230,1),new Color(.40f,.33f,.22f));
   if((Event.current.mousePosition-lastUpgradePointer).sqrMagnitude>1){upgradePointerFocus=true;lastUpgradePointer=Event.current.mousePosition;}bool pointer=false;for(int i=0;i<build.offers.Count;i++)if(upgradePointerFocus&&UpgradeCardRect(i).Contains(Event.current.mousePosition))pointer=true;
   for(int i=0;i<build.offers.Count;i++)UpgradeCard(i,build.offers[i],pointer);
   if(build.AscensionRound){GUI.Label(new Rect(190,800,1220,45),"LEFT / RIGHT TO CHOOSE  /  ENTER OR A TO ASCEND\nAll existing attribute upgrades are retained. This choice lasts for this expedition.",CardStyle(13,muted,TextAnchor.MiddleCenter));return;}
   // Register enabled controls by their visual row and column.
   for(int i=0;i<build.offers.Count;i++){
    Choice offer=build.offers[i];float x=190+i*418;
    CardUtilityButton(new Rect(x,729,184,34),"BANISH",()=>{if(build.Banish(offer)){build.Roll(random);selection=0;choiceOpenedAt=Time.unscaledTime;}},build.banishes>0,1,i*2);
    CardUtilityButton(new Rect(x+200,729,184,34),build.locked==offer.Key?"UNLOCK":"LOCK",()=>build.Lock(offer),build.locks>0||build.locked==offer.Key,1,i*2+1);
   }
   CardUtilityButton(new Rect(633,791,334,40),"REROLL  /  "+build.rerolls+" LEFT",()=>{if(build.rerolls>0){build.rerolls--;build.Roll(random);selection=0;choiceOpenedAt=Time.unscaledTime;}},build.rerolls>0,2,0);
   GUI.Label(new Rect(190,796,420,34),build.Slots+" / 5 EQUIPPED ABILITIES   |   "+build.Luck.ToString("0.#")+" LUCK",CardStyle(12,muted,TextAnchor.MiddleLeft));
   GUI.Label(new Rect(993,796,417,34),build.banishes+" BANISHES   |   "+build.locks+" LOCKS",CardStyle(12,muted,TextAnchor.MiddleRight));
   GUI.Label(new Rect(190,851,1220,20),"CLICK A CARD / 1 - 2 - 3    |    ARROWS / D-PAD / STICK + ENTER / A    |    BOTH CLOCKS PAUSED",CardStyle(11,muted,TextAnchor.MiddleCenter));
   var odds=Rarities.Chances(build.Luck);
   GUI.Label(new Rect(190,877,1220,18),$"OFFER ODDS   COMMON {odds[0]:P0}   /   UNCOMMON {odds[1]:P0}   /   RARE {odds[2]:P0}   /   EPIC {odds[3]:P0}   /   LEGENDARY {odds[4]:P0}",CardStyle(10,new Color(.43f,.45f,.43f),TextAnchor.MiddleCenter));
  }
  void CardUtilityButton(Rect r,string title,Action action,bool enabled,int row=-1,int column=0) {
   int id=buttons.Count;if(enabled){if(state==ScreenState.Choice&&row>=0)RegisterChoiceNode(id,row,column);buttons.Add(action);}
   if(state==ScreenState.Choice&&enabled&&upgradePointerFocus&&r.Contains(Event.current.mousePosition))selection=id;
   bool focused=enabled&&(state==ScreenState.Choice?(upgradePointerFocus?r.Contains(Event.current.mousePosition):selection==id):(selection==id||(upgradePointerFocus&&r.Contains(Event.current.mousePosition))));
   Color border=focused?new Color(.83f,.67f,.41f):new Color(.25f,.27f,.25f);
   Box(r,new Color(.06f,.07f,.07f));CardOutline(r,border);
   GUI.Label(r,title,CardStyle(12,enabled?(focused?parchment:muted):new Color(.29f,.31f,.30f),TextAnchor.MiddleCenter,true));if(focused&&(state==ScreenState.Choice||GenericMenuFocusAllowed))DrawMenuNavigationFocus(r,title);
   if(enabled&&Event.current.type==EventType.MouseDown&&Event.current.button==0&&r.Contains(Event.current.mousePosition)){selection=id;action();Event.current.Use();}
  }
 }
}


