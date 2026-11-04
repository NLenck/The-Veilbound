using System;
using System.Collections.Generic;
using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  bool gearBagOpen;int gearBagSlot=2,gearBagSelected,gearBagPage;bool gearBagRules;
  void OpenGearBag(int slot,int selected=0){city.EnsureGearRolls();gearBagOpen=true;gearBagSlot=slot;gearBagSelected=selected!=0?selected:city.EquippedRoll(slot)?.id??0;gearBagPage=0;gearBagRules=false;selection=0;}
  void CloseGearBag(){gearBagOpen=false;selection=0;}
  List<CraftedGear> BagEntries(){var items=city.craftedGear.FindAll(g=>g.slot==gearBagSlot);items.Sort((a,b)=>b.id.CompareTo(a.id));return items;}
  void GearBagScreen() {
   city.EnsureGearRolls();InitHudArt();Box(new Rect(0,0,1600,900),new Color(.018f,.025f,.027f));HudFrame(new Rect(30,18,1540,103));
   GUI.Label(new Rect(54,132,1000,34),"CRAFTED GEAR / "+PaperSlotName(gearBagSlot).ToUpper(),CardStyle(28,parchment,bold:true));
   Button(1300,132,240,36,tab==1?"BACK TO LOADOUT":"BACK TO CRAFTING",CloseGearBag);
   GUI.Label(new Rect(54,171,1490,22),"CRAFT ODDS   Common 50%  /  Uncommon 25%  /  Rare 15%  /  Epic 8%  /  Legendary 2%   ·   Luck does not change crafting",CardStyle(14,hudLight));
   Button(54,199,330,27,gearBagRules?"VIEW OWNED ITEMS":"ROLL VALUES & WEIGHTS",()=>{gearBagRules=!gearBagRules;selection=0;});
   GUI.Label(new Rect(410,204,1120,22),(GearRollRules.Major(gearBagSlot)?"Full bonus strength":"50% bonus strength")+"  /  Base material cost: "+Gear.CraftUnits(gearBagSlot),CardStyle(13,muted));
   if(gearBagRules){GearRollBalancePanel();return;}
   var items=BagEntries();int pages=Math.Max(1,(items.Count+7)/8);gearBagPage=Mathf.Clamp(gearBagPage,0,pages-1);
   if(gearBagSelected==0&&items.Count>0)gearBagSelected=items[0].id;
   var chosen=city.FindRoll(gearBagSelected);if(chosen!=null&&chosen.slot!=gearBagSlot)chosen=null;var equipped=city.EquippedRoll(gearBagSlot);
   HudFrame(new Rect(30,230,655,574));GUI.Label(new Rect(54,244,590,24),items.Count+" OWNED  /  PAGE "+(gearBagPage+1)+" OF "+pages,CardStyle(13,hudLight,bold:true));
   for(int row=0;row<8;row++){int index=gearBagPage*8+row;if(index>=items.Count)break;var item=items[index];float y=281+row*62;Rect r=new Rect(49,y,617,55);bool selected=gearBagSelected==item.id;
    Box(r,selected?new Color(.11f,.105f,.085f):new Color(.035f,.043f,.044f));CardOutline(r,selected?hudLight:Rarities.Colour(item.rarity));
    string title=(equipped?.id==item.id?"[E] ":"")+item.rarity+" "+GearRollRules.BaseName(item)+"  #"+item.id;
    GUI.Label(new Rect(r.x+12,r.y+6,r.width-24,24),title,CardStyle(15,Rarities.Colour(item.rarity),bold:true));GUI.Label(new Rect(r.x+12,r.y+32,r.width-24,18),GearRollRules.Summary(item),CardStyle(11,muted));
    int id=item.id;buttons.Add(()=>gearBagSelected=id);if(Event.current.type==EventType.MouseDown&&Event.current.button==0&&r.Contains(Event.current.mousePosition)){gearBagSelected=id;Event.current.Use();}
   }
   Button(54,751,150,34,"PREVIOUS",()=>{gearBagPage--;selection=0;},gearBagPage>0);Button(510,751,150,34,"NEXT",()=>{gearBagPage++;selection=0;},gearBagPage<pages-1);
   if(Event.current.type==EventType.ScrollWheel&&new Rect(30,230,655,574).Contains(Event.current.mousePosition)){gearBagPage=Mathf.Clamp(gearBagPage+(Event.current.delta.y>0?1:-1),0,pages-1);Event.current.Use();}
   HudFrame(new Rect(710,230,860,574));GearComparisonCard(new Rect(733,246,392,244),chosen,"SELECTED ITEM");GearComparisonCard(new Rect(1150,246,392,244),equipped,"CURRENTLY EQUIPPED");
   GUI.Label(new Rect(739,514,780,24),"BONUS CHANGE IF EQUIPPED",CardStyle(15,hudLight,bold:true));
   for(int i=0;i<GearRollRules.Stats.Length;i++){
    string stat=GearRollRules.Stats[i];float delta=GearRollRules.Value(chosen,stat)-GearRollRules.Value(equipped,stat);float x=739+(i%2)*410,y=553+(i/2)*36;
    GUI.Label(new Rect(x,y,245,25),GearRollRules.Labels[i],CardStyle(14,muted));GUI.Label(new Rect(x+235,y,145,25),GearRollRules.Amount(stat,delta),CardStyle(15,Mathf.Abs(delta)<.00001f?muted:delta>0?new Color(.47f,.83f,.54f):new Color(.96f,.40f,.33f),TextAnchor.UpperRight));
   }
   Button(739,725,386,49,equipped?.id==chosen?.id?"EQUIPPED":"EQUIP SELECTED",()=>{if(chosen!=null&&city.EquipRoll(chosen.id)){notice=chosen.rarity+" "+GearRollRules.BaseName(chosen)+" #"+chosen.id+" equipped.";RefreshMetalGear();}},chosen!=null&&equipped?.id!=chosen.id);
   Button(1150,725,392,49,"KEEP CURRENT GEAR",CloseGearBag);
   GUI.Label(new Rect(54,829,1490,25),"Craft rarity changes bonuses only. Material still determines base armour, weapon damage, weight and recipe.",CardStyle(14,muted));GUI.Label(new Rect(54,869,1490,22),notice,CardStyle(12,hudLight));
  }
  void GearComparisonCard(Rect r,CraftedGear item,string heading) {
   GUI.Label(new Rect(r.x,r.y,r.width,23),heading,CardStyle(12,hudLight,bold:true));if(item==null){GUI.Label(new Rect(r.x,r.y+40,r.width,28),"No item selected",CardStyle(18,muted));return;}
   Color colour=Rarities.Colour(item.rarity);GUI.Label(new Rect(r.x,r.y+30,r.width,28),item.rarity+"  /  #"+item.id,CardStyle(18,colour,bold:true));GUI.Label(new Rect(r.x,r.y+61,r.width,27),GearRollRules.BaseName(item),CardStyle(18,parchment,bold:true));
   int metal=item.slot==9?item.item:Metals.Index(item.item);float armour=metal>=0&&item.slot!=9?Metals.Armour(item.slot,metal):item.slot==9?0:Gear.Tier(item.item)*(Gear.Material(item.item)==2?7:3)*Gear.Size(item.slot);
   float mass=metal>=0?(item.slot==9?4.5f*Metals.All[metal].weight:Metals.Mass(item.slot,metal)):0;
   string line=item.slot==9?Metals.All[item.item].weapon.ToString("0.#")+" base damage":armour.ToString("0.#")+" armour";
   GUI.Label(new Rect(r.x,r.y+94,r.width,23),line+"  /  "+mass.ToString("0.##")+" kg",CardStyle(14,muted));
   if(item.inherited)GUI.Label(new Rect(r.x,r.y+136,r.width,63),"Existing item preserved.\nNew crafts roll random bonuses.",CardStyle(14,muted));
   else for(int i=0;i<item.affixes.Length;i++){var a=item.affixes[i];GUI.Label(new Rect(r.x,r.y+132+i*29,r.width,25),GearRollRules.Amount(a.stat,a.value)+" "+(a.stat=="vitality"||a.stat=="regen"?"":GearRollRules.Label(a.stat)),CardStyle(16,colour));}
  }
  void GearRollBalancePanel() {
   HudFrame(new Rect(30,230,1540,574));GUI.Label(new Rect(54,248,1490,29),"BONUS VALUES / "+(GearRollRules.Major(gearBagSlot)?"FULL STRENGTH":"HALF STRENGTH"),CardStyle(21,parchment,bold:true));
   GUI.Label(new Rect(54,291,1490,25),"One bonus on Common / Uncommon, two on Rare / Epic, three on Legendary. Stats are drawn by weight without repeats.",CardStyle(14,muted));
   GUI.Label(new Rect(54,338,300,25),"STAT / DRAW WEIGHT",CardStyle(13,hudLight,bold:true));for(int r=0;r<5;r++)GUI.Label(new Rect(405+r*225,338,205,25),((UpgradeRarity)r).ToString(),CardStyle(15,Rarities.Colour((UpgradeRarity)r),bold:true));
   for(int i=0;i<GearRollRules.Stats.Length;i++){float y=389+i*43;string stat=GearRollRules.Stats[i];GUI.Label(new Rect(54,y,320,26),GearRollRules.Labels[i]+"  /  "+GearRollRules.Weights[i],CardStyle(15,parchment));for(int r=0;r<5;r++)GUI.Label(new Rect(405+r*225,y,205,26),GearRollRules.Amount(stat,GearRollRules.Values[i][r]*GearRollRules.Scale(gearBagSlot)),CardStyle(16,muted));}
   GUI.Label(new Rect(54,829,1490,26),"Weights are initial tuning values, renormalised after each stat is drawn. Material tier and crafting luck do not affect these rolls.",CardStyle(14,muted));
  }
 }
}
