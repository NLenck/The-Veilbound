using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Veilbound {
 public partial class VeilGame {
  ScreenState characterReturn=ScreenState.Expedition;int characterTab;Vector2 passiveScroll,damageSheetScroll;
  void OpenCharacterSheet(int page) {
   if(state==ScreenState.Choice||state==ScreenState.Result)return;
   if(state!=ScreenState.Character)characterReturn=state;characterTab=page;SetState(ScreenState.Character);if(page==1)ResetBuildOverview();
  }
  void CloseCharacterSheet(){SetState(characterReturn);}
  void ReadCharacterSheetInput() {
   var key=Keyboard.current;int page=KeyPressed(ControlAction.Character)?0:KeyPressed(ControlAction.Passives)?1:KeyPressed(ControlAction.Damage)?2:-1;
   if(page>=0)gamepadCombat=false;
   var pad=PreferredPad;
   if(state==ScreenState.Character&&pad?.buttonEast.wasPressedThisFrame==true){if(characterTab==1)return;CloseCharacterSheet();return;}
   if(state==ScreenState.Character){
    if(pad?.leftShoulder.wasPressedThisFrame==true){gamepadCombat=true;CycleCharacterPage(-1);}
    else if(pad?.rightShoulder.wasPressedThisFrame==true){gamepadCombat=true;CycleCharacterPage(1);}
   }
   if(PadPressed(ControlAction.Character)){gamepadCombat=true;if(state==ScreenState.Character){CloseCharacterSheet();return;}page=0;}
   if(state==ScreenState.Expedition){if(PadPressed(ControlAction.Passives)){gamepadCombat=true;page=1;}else if(PadPressed(ControlAction.Damage)){gamepadCombat=true;page=2;}}
   if(page>=0){if(state==ScreenState.Character&&characterTab==page)CloseCharacterSheet();else OpenCharacterSheet(page);}
  }
  void CharacterSheet() {
   Box(new Rect(0,0,1600,900),new Color(.018f,.023f,.025f,.98f));
   GUI.Label(new Rect(140,43,1320,28),city.virtue.ToString().ToUpper()+" VIRTUE  /  LEVEL "+level+"  /  EXPEDITION RECORD",CardStyle(14,new Color(.83f,.67f,.41f),TextAnchor.MiddleCenter,true));
   GUI.Label(new Rect(140,89,1320,55),characterTab==0?"CHARACTER":characterTab==1?"ABILITIES & PASSIVES":"DAMAGE BREAKDOWN",CardStyle(38,parchment,TextAnchor.MiddleCenter,true));
   string[] tabs={"Character","Passives","Damage"};
   for(int i=0;i<3;i++){int page=i;Button(350+i*300,163,286,42,tabs[i],()=>{characterTab=page;selection=page;});if(characterTab==i)Box(new Rect(350+i*300,203,286,2),WorldArt.Gold);}
   Box(new Rect(140,230,1320,540),new Color(.044f,.051f,.052f));CardOutline(new Rect(140,230,1320,540),new Color(.28f,.27f,.23f));
   if(characterTab==0)CharacterStats();else if(characterTab==1)PassiveHistory();else DamageBreakdown();
   Button(633,799,334,43,"Return",CloseCharacterSheet);
   GUI.Label(new Rect(140,855,1320,24),"GEAR ENCHANTMENTS PERSIST   /   EXPEDITION PASSIVES RESET ON A NEW RUN   /   BOTH CLOCKS PAUSED",CardStyle(11,muted,TextAnchor.MiddleCenter));
  }
  void StatRow(float x,float y,string name,string value,string detail) {
   GUI.Label(new Rect(x,y,310,25),name,CardStyle(17,parchment));
   GUI.Label(new Rect(x+300,y,245,25),value,CardStyle(18,new Color(.91f,.76f,.45f),TextAnchor.UpperRight,true));
   GUI.Label(new Rect(x,y+26,540,21),detail,CardStyle(12,muted));Box(new Rect(x,y+53,545,1),new Color(.15f,.17f,.16f));
  }
  void CharacterStats() {
   float protection=1-(1-Mathf.Min(.56f,build.PassivePercent("guard")*.01f))*(1-city.Protection);
   StatRow(185,258,"All ability damage",$"+{(Damage/Mathf.Max(1,city.WeaponDamage)-1)*100:0.#}%",$"Weapon base {city.WeaponDamage:0.#}   /   Scaled base {Damage:0.#}");
   StatRow(185,327,"Critical chance",CritChance.ToString("P1"),$"5% base / +{build.PassivePercent("crit"):0.#}% passive / +{city.GearBonus("crit"):0.#}% gear / Capped at 100%");
   StatRow(185,396,"Critical damage",CritMultiplier.ToString("P1"),$"{CritMultiplier:0.##}x normal damage on a critical hit");
   StatRow(185,465,"Haste",$"{GlobalHasteRating:0.#}% rating",$"{HasteRules.Reduction(GlobalHasteRating)*100:0.#}% effective cooldown reduction / ability Haste adds before diminishing returns"+(RageActive?" / FURY":""));
   StatRow(185,534,"Area",$"+{(AreaMultiplier-1)*100:0.#}%",$"{AreaMultiplier:0.##}x radius and projectile size");
   StatRow(185,603,"Movement speed",$"{Speed:0.##} m/s",$"{city.EquipmentMass:0.#} kg gear   /   30 kg allowance   /   -{(1-city.GearSpeed)*100:0.#}% weight penalty   /   +{build.PassivePercent("speed"):0.#}% passive   /   +{city.GearBonus("speed"):0.#}% gear");
   StatRow(185,672,"Luck",build.Luck.ToString("0.#"),"Increases the odds of higher-rarity offers");
   StatRow(870,258,"Maximum health",MaxHealth.ToString("0.#"),$"Base 120  +  Gear {city.GearHealth+city.GearBonus("vitality"):0.#}  +  Passives {build.Amount("vitality")*20:0.#}   /   Armour {city.EquipmentArmour:0.#}");
   StatRow(870,327,"Current health",Mathf.Max(0,health).ToString("0.#"),"Current expedition health");
   StatRow(870,396,"Health regeneration",$"{build.Amount("regen")*.3f+city.GearBonus("regen"):0.###} / sec","Restores health during active combat");
   StatRow(870,465,"Damage reduction",protection.ToString("P1"),$"Gear {city.Protection:P1}   /   Passives {Mathf.Min(.56f,build.PassivePercent("guard")*.01f):P0}   /   Combined multiplicatively");
   StatRow(870,534,"Experience gain",$"+{(city.GearExperience*(1+build.PassivePercent("wisdom")*.01f)-1)*100:0.#}%","Includes gear and selected passives");
   StatRow(870,603,"Experience collection area",$"{3.1f*(1+build.Amount("reach")*.25f):0.##} m",$"+{build.Amount("reach")*25:0.#}% pickup radius");
   StatRow(870,672,"Abilities",build.Slots+" / 5 equipped",$"Run time {Clock(elapsed)}   /   {kills} enemies defeated");
  }

  void DamageBreakdown() {
   float total=TotalPlayerDamage,seconds=Mathf.Max(.1f,elapsed);
   GUI.Label(new Rect(185,253,1220,34),$"{RecentDps():0.#} RECENT DPS     /     {total/seconds:0.#} RUN DPS     /     {total:0.#} TOTAL DAMAGE",CardStyle(23,new Color(.91f,.76f,.45f),bold:true));
   GUI.Label(new Rect(185,297,1220,25),"Recent DPS uses a rolling 5 seconds of combat time. Run DPS uses the entire active expedition.",CardStyle(13,muted));
   string[] headers={"ABILITY","TOTAL","SHARE","5s DPS","RUN DPS","HITS","CRITS"};float[] columns={185,655,805,930,1070,1220,1325};
   for(int i=0;i<headers.Length;i++)GUI.Label(new Rect(columns[i],344,i==0?370:120,24),headers[i],CardStyle(12,muted,bold:true));
   var entries=new List<Choice>();foreach(var ability in RunBuild.Pool)if(ability.ability&&(build.Rank(ability.id)>0||abilityDamageRecords.ContainsKey(ability.id)))entries.Add(ability);
   entries.Sort((a,b)=>(abilityDamageRecords.TryGetValue(b.id,out var br)?br.total:0).CompareTo(abilityDamageRecords.TryGetValue(a.id,out var ar)?ar.total:0));
   damageSheetScroll=GUI.BeginScrollView(new Rect(170,375,1270,335),damageSheetScroll,new Rect(0,0,1240,Mathf.Max(330,entries.Count*62+10)));
   for(int row=0;row<entries.Count;row++){
    var ability=entries[row];float y=9+row*62;abilityDamageRecords.TryGetValue(ability.id,out var record);float damage=record?.total??0,share=total>0?damage/total:0;
    GUI.DrawTexture(new Rect(15,y,45,45),UpgradeIcon(ability.id));GUI.Label(new Rect(77,y+1,365,25),ability.name,CardStyle(18,parchment));
    Bar(77,y+34,350,4,share,new Color(.75f,.57f,.27f));
    string[] values={damage.ToString("0.#"),share.ToString("P0"),RecentDps(ability.id).ToString("0.#"),(damage/seconds).ToString("0.#"),(record?.hits??0).ToString(),(record?.crits??0).ToString()};
    for(int i=0;i<values.Length;i++)GUI.Label(new Rect(columns[i+1]-170,y+8,110,26),values[i],CardStyle(17,parchment));
   }
   GUI.EndScrollView();
   GUI.Label(new Rect(185,722,1230,25),"Effective player damage only: overkill and enemy-on-enemy explosions are excluded. The meter resets each new run.",CardStyle(12,muted));
  }
 }
}


