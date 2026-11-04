using System;using System.Collections.Generic;using UnityEngine;using UnityEngine.InputSystem;
namespace Veilbound {
 public partial class VeilGame {
  readonly List<Choice> overviewAbilities=new List<Choice>(),overviewPassives=new List<Choice>();
  int overviewFocus;string expandedPassive;bool overviewPointer;Vector2 overviewMouse;Vector2Int overviewHeld;float overviewRepeat;
  void ResetBuildOverview(){overviewFocus=0;expandedPassive=null;overviewPointer=false;overviewHeld=Vector2Int.zero;}
  int PassivePickCount(string id){int n=0;if(build.passivePicks.TryGetValue(id,out var counts))foreach(int count in counts)n+=count;return n;}
  void RefreshBuildOverview(){overviewAbilities.Clear();overviewPassives.Clear();foreach(var choice in RunBuild.Pool)if(choice.ability){if(build.Rank(choice.id)>0)overviewAbilities.Add(choice);}else if(PassivePickCount(choice.id)>0)overviewPassives.Add(choice);overviewFocus=Mathf.Clamp(overviewFocus,0,Mathf.Max(0,overviewAbilities.Count+overviewPassives.Count-1));}
  Choice OverviewChoice(int index)=>index<overviewAbilities.Count?overviewAbilities[index]:overviewPassives[index-overviewAbilities.Count];
  Vector2Int OverviewCell(int index)=>index<overviewAbilities.Count?new Vector2Int(index,0):new Vector2Int((index-overviewAbilities.Count)%6,1+(index-overviewAbilities.Count)/6);
  void FocusOverview(int index){if(index!=overviewFocus)expandedPassive=null;overviewFocus=index;}
  void MoveBuildOverview(Vector2Int direction){RefreshBuildOverview();int count=overviewAbilities.Count+overviewPassives.Count;if(count==0)return;Vector2Int from=OverviewCell(overviewFocus);int chosen=-1;float best=float.MaxValue;
   for(int i=0;i<count;i++){Vector2Int cell=OverviewCell(i);if(direction.x!=0){if(cell.y!=from.y||(cell.x-from.x)*direction.x<=0)continue;float score=Mathf.Abs(cell.x-from.x);if(score<best){best=score;chosen=i;}}
    else{if((cell.y-from.y)*direction.y>=0)continue;float score=Mathf.Abs(cell.y-from.y)*100+Mathf.Abs(cell.x-from.x);if(score<best){best=score;chosen=i;}}}
   if(chosen>=0)FocusOverview(chosen);
  }
  void ToggleOverviewEntry(){RefreshBuildOverview();if(overviewAbilities.Count+overviewPassives.Count==0)return;var choice=OverviewChoice(overviewFocus);if(!choice.ability)expandedPassive=expandedPassive==choice.id?null:choice.id;}
  void ReadBuildOverviewInput(){RefreshBuildOverview();var key=Keyboard.current;var pad=PreferredPad;
   if(key?.escapeKey.wasPressedThisFrame==true||pad?.buttonEast.wasPressedThisFrame==true){if(expandedPassive!=null)expandedPassive=null;else CloseCharacterSheet();return;}
   Vector2 input=new Vector2((key?.rightArrowKey.isPressed==true?1:0)-(key?.leftArrowKey.isPressed==true?1:0),(key?.upArrowKey.isPressed==true?1:0)-(key?.downArrowKey.isPressed==true?1:0));
   if(pad!=null){Vector2 dpad=pad.dpad.ReadValue(),stick=pad.leftStick.ReadValue();if(dpad.sqrMagnitude>.1f)input=dpad;else if(stick.sqrMagnitude>.36f)input=stick;}
   Vector2Int direction=input.sqrMagnitude<.25f?Vector2Int.zero:Mathf.Abs(input.x)>Mathf.Abs(input.y)?new Vector2Int(input.x>0?1:-1,0):new Vector2Int(0,input.y>0?1:-1);
   if(direction!=Vector2Int.zero&&(direction!=overviewHeld||Time.unscaledTime>=overviewRepeat)){overviewPointer=false;MoveBuildOverview(direction);overviewRepeat=Time.unscaledTime+(direction!=overviewHeld?.3f:.12f);}overviewHeld=direction;
   if(key?.tabKey.wasPressedThisFrame==true){int count=overviewAbilities.Count+overviewPassives.Count;if(count>0)FocusOverview((overviewFocus+(key.shiftKey.isPressed?count-1:1))%count);overviewPointer=false;}
   if(key?.enterKey.wasPressedThisFrame==true||pad?.buttonSouth.wasPressedThisFrame==true){overviewPointer=false;ToggleOverviewEntry();}
  }
  float PassiveCollectedValue(string id){if(PassiveRules.Has(id))return build.PassivePercent(id);float factor=id=="vitality"?20:id=="regen"?.3f:id=="reach"?25:10;return build.Amount(id)*factor;}
  string PassiveValueUnit(string id)=>id=="vitality"?" health":id=="regen"?" health/sec":id=="luck"?" Luck":"%";
  string PassiveTotal(string id){float value=PassiveCollectedValue(id);if(id=="guard")value=Mathf.Min(56,value);if(id=="crit")value=Mathf.Min(Mathf.Max(0,100-build.CritBasePercent),value);return "+"+value.ToString("0.##")+PassiveValueUnit(id);}
  string PassiveEffectLabel(string id){switch(id){case "power":return "All ability damage";case "haste":return "Haste rating";case "vitality":return "Maximum health";case "guard":return "Passive damage reduction";case "regen":return "Health regeneration";case "speed":return "Movement speed";case "wisdom":return "Experience gained";case "reach":return "Experience collection area";case "crit":return "Critical hit chance";case "critdamage":return "Critical damage";case "area":return "Ability area";default:return "Higher-rarity offer chance";}}
  float PassiveRarityValue(string id,int rarity)=>PassiveRules.Has(id)?PassiveRules.Percent(id,(UpgradeRarity)rarity):Rarities.Strength((UpgradeRarity)rarity)*(id=="vitality"?20:id=="regen"?.3f:id=="reach"?25:10);
  bool PassiveAffectsAbility(string passive,string ability){
   if(passive=="power")return ability!="brand"&&ability!="warcry";
   if(passive=="haste")return ability!="rend"&&ability!="rage";
   if(passive=="area")return ability!="rend";
   if(passive=="crit"||passive=="critdamage")return ability!="rend"&&ability!="rage"&&ability!="warcry"&&(ability!="brand"||Ascended("brand",1));
   return passive=="vitality"&&(ability=="bloodthirst"||ability=="vengeance"&&Ascended("vengeance",2));
  }
  string AppliedAbilityPassives(string id){var lines=new List<string>();foreach(var passive in overviewPassives)if(PassiveAffectsAbility(passive.id,id))lines.Add(passive.name+" "+PassiveTotal(passive.id));return lines.Count==0?"Applied passives: none.":"Applied passives: "+string.Join(" · ",lines);}
  string AbilityDamageSummary(string id){float damage=AbilityDamage(id);switch(id){
   case "brand":return "+"+(BrandBonus*100).ToString("0.#")+"% damage taken";
   case "warcry":return (WarCryReduction*100).ToString("0.#")+"% less enemy damage";
   case "rend":return RendDps.ToString("0.#")+" / sec"+(Ascended("rend",1)?" / stack (max 5)":"")+" · "+RendDuration.ToString("0.#")+"s";
   case "cleave":return damage.ToString("0.#")+" / swing"+(Ascended(id,1)?" + "+(damage*.65f).ToString("0.#")+" fissure":Ascended(id,2)?" + "+(damage*1.2f).ToString("0.#")+" finisher":"");
   case "whirl":return Ascended(id,1)?(damage*.3f).ToString("0.#")+" / pass · "+(damage*3).ToString("0.#")+" total":Ascended(id,2)?(damage*.9f).ToString("0.#")+" / journey":damage.ToString("0.#")+" / cast (3 passes)";
   case "lance":return (damage*(Ascended(id,1)?1.2f:1)).ToString("0.#")+" / blade"+(Ascended(id,2)?" · "+(damage*.65f).ToString("0.#")+" / fragment":"");
   case "orbit":return damage.ToString("0.#")+(Ascended(id,1)?" / sword · "+OathBladeCount+" swords":Ascended(id,2)?" base · 0.9–1.3× / journey":" / hit · "+OathBladeCount+" blade(s)");
   case "smite":return Ascended(id,1)?damage.ToString("0.#")+" → "+(damage*2.2f).ToString("0.#")+" / judgement":(damage*(Ascended(id,2)?.85f:1)).ToString("0.#")+" / strike";
   case "bloodthirst":return (damage*(Ascended(id,1)?1.2f:Ascended(id,2)?1.5f:1)).ToString("0.#")+" / strike";
   case "vengeance":return (damage+(Ascended(id,2)?bloodDebt*2:0)).ToString("0.#")+" / counter swing";
   case "rage":return (damage*(Ascended(id,2)?1.75f:1)).ToString("0.#")+" / eruption";
   default:return damage.ToString("0.#");
  }}
  void PassiveHistory(){
   RefreshBuildOverview();if((Event.current.mousePosition-overviewMouse).sqrMagnitude>1){overviewPointer=true;overviewMouse=Event.current.mousePosition;}
   GUI.Label(new Rect(175,246,770,24),"ABILITIES",CardStyle(14,hudLight,bold:true));
   for(int i=0;i<overviewAbilities.Count;i++)DrawOverviewTile(i,new Rect(175+i*130,279,118,117),overviewAbilities[i],true);
   int picks=0;foreach(var passive in overviewPassives)picks+=PassivePickCount(passive.id);
   GUI.Label(new Rect(175,411,770,24),"PASSIVES  /  "+overviewPassives.Count+" TYPES  /  "+picks+" PICKS",CardStyle(14,hudLight,bold:true));
   for(int i=0;i<overviewPassives.Count;i++)DrawOverviewTile(overviewAbilities.Count+i,new Rect(175+i%6*130,447+i/6*137,118,119),overviewPassives[i],false);
   if(overviewPassives.Count==0)GUI.Label(new Rect(185,469,740,80),"Find relics to choose passives. Each passive appears once here; open it to inspect its rarity picks.",CardStyle(17,muted));
   if(overviewAbilities.Count+overviewPassives.Count>0)DrawOverviewDetails(OverviewChoice(overviewFocus));
   GUI.Label(new Rect(175,737,770,19),"Hover or focus to inspect · Click / Enter / A for rarity picks · B / Esc to go back",CardStyle(11,muted));
  }
  void DrawOverviewTile(int index,Rect rect,Choice choice,bool ability){
   bool hover=overviewPointer&&rect.Contains(Event.current.mousePosition);if(hover)FocusOverview(index);bool focus=overviewFocus==index;
   Box(rect,focus?new Color(.13f,.115f,.075f):new Color(.029f,.036f,.037f));CardOutline(rect,focus?hudLight:new Color(.19f,.23f,.22f),focus?2:1);
   Rect icon=new Rect(rect.center.x-29,rect.y+7,58,58);var texture=UpgradeIcon(choice.id);if(texture)GUI.DrawTexture(icon,texture);if(ability)DrawAscensionBadge(icon,choice.id);
   string count=ability?build.Rank(choice.id).ToString():PassivePickCount(choice.id).ToString();Rect badge=new Rect(rect.xMax-27,rect.y+3,24,21);Box(badge,new Color(.04f,.045f,.045f));GUI.Label(badge,count,CardStyle(12,hudLight,TextAnchor.MiddleCenter,true));
   GUI.Label(new Rect(rect.x+3,rect.y+71,rect.width-6,42),ability?AbilityDisplayName(choice.id):choice.name,CardStyle(12,parchment,TextAnchor.UpperCenter,true));
   if(focus)DrawMenuNavigationFocus(rect,choice.name);
   if(Event.current.type==EventType.MouseDown&&Event.current.button==0&&rect.Contains(Event.current.mousePosition)){FocusOverview(index);ToggleOverviewEntry();Event.current.Use();}
  }
  void DetailLine(float y,string title,string value){GUI.Label(new Rect(1004,y,396,18),title,CardStyle(11,muted));GUI.Label(new Rect(1004,y+20,396,36),value,CardStyle(16,parchment,bold:true));}
  void DrawOverviewDetails(Choice choice){
   Rect panel=new Rect(982,250,452,488);Box(panel,new Color(.023f,.030f,.032f));CardOutline(panel,new Color(.32f,.29f,.21f));
   GUI.Label(new Rect(1004,267,408,55),choice.ability?AbilityDisplayName(choice.id):choice.name,CardStyle(24,hudLight,bold:true));
   GUI.Label(new Rect(1004,324,408,21),choice.ability?"ABILITY LEVEL "+build.Rank(choice.id)+(build.Ascension(choice.id)>0?" / ASCENSION "+(build.Ascension(choice.id)==1?"I":"II"):""):PassivePickCount(choice.id)+" PICKS / COMBINED BONUS",CardStyle(12,muted));
   if(choice.ability){
    DetailLine(360,choice.id=="brand"||choice.id=="warcry"?"EFFECT":"DAMAGE",AbilityDamageSummary(choice.id));
    float cd=AbilityCooldown(choice.id);DetailLine(422,"ATTACK RATE / COOLDOWN",cd>0?$"{1/cd:0.##} casts/sec · {cd:0.##}s cooldown":choice.id=="rage"?$"100 Rage · {RageDuration:0.#}s duration":$"{RendChance*100:0.#}% proc chance · 0.5s bleed ticks");
    string extra=choice.id=="brand"?$"{BrandDuration:0.#}s mark · {SkillRadius(choice.id):0.#}m radius":choice.id=="warcry"?$"{WarCryDuration:0.#}s weaken · {SkillRadius(choice.id):0.#}m radius":choice.id=="bloodthirst"?$"{BloodHealing*100:0.#}% healing · {BloodHealCap:0.#} health cap":choice.id=="vengeance"?$"{VengeanceChance*100:0.#}% retaliation · {VengeanceProtection*100:0.#}% triggering-hit protection":choice.id=="rage"?$"+{RageDamageBonus*100:0.#}% damage · +{RageAttackBonus*100:0.#}% Haste during Fury":choice.id=="rend"?"Bleeds cannot critically hit":$"{CritChance*100:0.#}% crit · {CritMultiplier*100:0.#}% crit damage · {SkillArea(choice.id):0.##}× area";
    GUI.Label(new Rect(1004,486,408,49),extra,CardStyle(13,parchment));
    var spec=AbilityAscensions.Get(choice.id,build.Ascension(choice.id));GUI.Label(new Rect(1004,541,408,90),(spec!=null?spec.description:Array.Find(RunBuild.Pool,c=>c.id==choice.id).description)+(choice.id=="lance"?$" Each blade: {RiftBladeMaxHits} targets maximum / {RiftBladeRange:0}m range.":""),CardStyle(12,muted));
    GUI.Label(new Rect(1004,648,408,76),AppliedAbilityPassives(choice.id),CardStyle(11,new Color(.50f,.53f,.52f)));
   }else{
    DetailLine(360,PassiveEffectLabel(choice.id).ToUpperInvariant(),PassiveTotal(choice.id));
    if(expandedPassive==choice.id&&build.passivePicks.TryGetValue(choice.id,out var counts)){float y=432;for(int rarity=0;rarity<5;rarity++)if(counts[rarity]>0){var color=Rarities.Colour((UpgradeRarity)rarity);GUI.Label(new Rect(1004,y,190,23),((UpgradeRarity)rarity)+" × "+counts[rarity],CardStyle(14,color,TextAnchor.MiddleLeft));GUI.Label(new Rect(1200,y,208,23),"+"+(counts[rarity]*PassiveRarityValue(choice.id,rarity)).ToString("0.##")+PassiveValueUnit(choice.id),CardStyle(14,parchment,TextAnchor.MiddleRight));y+=38;}
     GUI.Label(new Rect(1004,659,408,49),"Each row totals the picks of that rarity. Click / Enter / A again to collapse.",CardStyle(12,muted));
    }else GUI.Label(new Rect(1004,439,408,75),"Click / Enter / A to see the rarity and number of picks contributing to this total.",CardStyle(15,parchment));
    string note=choice.id=="haste"?"Haste combines with gear, ability upgrades and Fury before diminishing returns.":choice.id=="guard"?"Passive reduction is capped at 56% and combines multiplicatively with gear.":choice.id=="crit"?"Adds to your 5% base chance and gear; total critical chance is capped at 100%.":choice.id=="speed"?"Applied to your movement speed after equipment weight.":choice.id=="wisdom"?"Stacks with other XP picks, then multiplies your gear XP bonus.":"All picks of this passive are combined. Bonuses last for this expedition.";
    GUI.Label(new Rect(1004,710,408,23),note,CardStyle(10,muted));
   }
  }
 }
}

