using System;
using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  bool veilTungsten;int SelectedWeapon=>selectedMetal==9&&veilTungsten?10:selectedMetal;
  int industryPage,industrySource,selectedMetal;Vector2 industryScroll;
  string[] ProducerNames={"Mine","Hunter's Lodge","Textile Supply"};
  void RefreshMetalGear(){RebuildHero();cachedEquipmentHero=null;Save();}
  void CraftSelectedPlate(){if(city.CraftPlate(gearSlot,selectedMetal,out notice)){RefreshMetalGear();OpenGearBag(gearSlot,city.lastCraftedRollId);}}
  void ForgeSelectedWeapon(){if(city.CraftMetalWeapon(SelectedWeapon,out notice)){RefreshMetalGear();OpenGearBag(9,city.lastCraftedRollId);}}
  void EquipMetalWeapon(int metal){city.EnsureGear();if(!city.ownedMetalWeapons[metal])return;city.EquipKnownRoll(9,metal);city.equippedMetalWeapon=metal;city.equippedSword=0;RefreshMetalGear();}
  void MetalPalette(float x,float y,float width=672) {
   for(int i=0;i<10;i++){int index=i;float bx=x+(i%5)*(width/5),by=y+(i/5)*35;Rect r=new Rect(bx,by,width/5-7,29);
    CardUtilityButton(r,Metals.All[i].name,()=>selectedMetal=index,true);if(selectedMetal==i)CardOutline(r,hudLight,2);
    Box(new Rect(r.x+3,r.y+3,3,r.height-6),Metals.All[i].color);
   }
  }
  void FamilyButtons(){for(int m=1;m<=3;m++){int material=m;CardUtilityButton(new Rect(54+(m-1)*115,836,105,29),Gear.Materials[m],()=>gearMaterial=material,true);}}
  void MetalPlateDetails() {
   city.EnsureGear();selectedMetal=Mathf.Clamp(selectedMetal,0,9);var def=Metals.All[selectedMetal];int current=city.Item(gearSlot);bool owned=city.OwnsPlate(gearSlot,selectedMetal),equipped=Metals.Index(current)==selectedMetal;
   GUI.Label(new Rect(54,731,365,29),Gear.Slots[gearSlot].ToUpper()+" / "+Gear.Name(current),CardStyle(19,parchment,bold:true));
   GUI.Label(new Rect(54,773,365,54),$"Preview: {Metals.Armour(gearSlot,selectedMetal):0.#} armour\n{Metals.Mass(gearSlot,selectedMetal):0.##} kg  /  30 kg total allowance",CardStyle(13,muted));FamilyButtons();
   MetalPalette(435,732);GUI.Label(new Rect(435,811,672,48),def.name+" / Material tier: "+def.rarity+" / Smith "+def.unlock+"\n"+city.RecipeText(city.PlateRecipe(gearSlot,selectedMetal))+"  Â·  Have "+Math.Floor(city.metalBars[selectedMetal])+" bars",CardStyle(12,hudLight));
   Button(1135,733,405,35,"CRAFT NEW ROLL",CraftSelectedPlate,city.blacksmith>=def.unlock&&city.CraftResearchRequirement(selectedMetal,"plate")=="");
   Button(1135,780,405,35,"BAG / COMPARE GEAR",()=>OpenGearBag(gearSlot),true);
   Button(1135,827,195,35,"REMOVE",()=>RemoveEquipmentPiece(gearSlot),current>0);Button(1342,827,198,35,"BLACKSMITH",()=>{tab=2;industryPage=1;sanctuaryWorkshopOpen=true;selection=0;});
  }
  void VeilWeaponButtons(float x,float y,float width){if(selectedMetal!=9)return;Button(x,y,width*.49f,27,(veilTungsten?"":"Selected / ")+"Titanium base",()=>veilTungsten=false);Button(x+width*.51f,y,width*.49f,27,(veilTungsten?"Selected / ":"")+"Tungsten base",()=>veilTungsten=true);}
  void MetalWeaponDetails() {
   city.EnsureGear();if(gearSlot==10){GUI.Label(new Rect(54,735,1000,35),"OFF HAND / TWO-HANDED GREATSWORD",CardStyle(22,parchment,bold:true));Button(1135,770,405,42,"VIEW MAIN HAND",()=>SelectPaperSlot(9));return;}
   int weapon=SelectedWeapon;var def=Metals.All[WeaponRules.Material(weapon)];
   GUI.Label(new Rect(54,731,365,43),"MAIN HAND / "+city.WeaponName,CardStyle(16,parchment,bold:true));GUI.Label(new Rect(54,785,365,72),$"Preview: {WeaponRules.Damage(weapon):0.#} damage / {WeaponRules.Interval(weapon):0.##}s interval\n{WeaponRules.Damage(weapon)/WeaponRules.Interval(weapon):0.##} base DPS / Two-handed",CardStyle(13,muted));
   MetalPalette(435,732);if(selectedMetal==9)VeilWeaponButtons(435,811,672);else GUI.Label(new Rect(435,811,672,48),WeaponRules.Name(weapon)+" / Smith "+def.unlock+"\n"+city.RecipeText(city.WeaponRecipe(weapon)),CardStyle(12,hudLight));
   Button(1135,733,405,35,"FORGE WEAPON",ForgeSelectedWeapon,city.blacksmith>=def.unlock&&city.CraftResearchRequirement(SelectedWeapon,"weapons")=="");Button(1135,780,405,35,"BAG / COMPARE GEAR",()=>OpenGearBag(9),true);Button(1135,827,405,35,"OPEN BLACKSMITH",()=>{tab=2;industryPage=1;sanctuaryWorkshopOpen=true;selection=0;});
  }
  void LegacyIndustryScreen() {
   city.EnsureGear();city.EnsureIndustry();InitHudArt();Box(new Rect(0,0,1600,900),new Color(.018f,.025f,.027f));HudFrame(new Rect(30,18,1540,96));
   GUI.Label(new Rect(54,35,850,38),"SETTLEMENT / INDUSTRY & CRAFT",CardStyle(27,parchment,bold:true));
   Button(54,137,312,42,"EXPEDITION",()=>{tab=0;selection=0;});Button(644,137,312,42,"LOADOUT",()=>{tab=1;selection=0;});Button(1234,137,312,42,"PROCESS MATERIALS",()=>OpenMaterialProcessing(gearMaterial==2?0:1));
   for(int row=0;row<8;row++){
    string id=Settlement.Buildings[row];int n=city.BuildingLevel(id);float y=211+row*76;HudFrame(new Rect(30,y,370,68));
    GUI.Label(new Rect(43,y+8,233,23),Settlement.BuildingNames[row]+" / "+n,CardStyle(15,parchment,bold:true));
    bool max=n>=city.BuildingLimit(id);var cost=max?new TownUpgradeCost(0,0):city.UpgradeCost(id);double woodCost=cost.wood,stoneCost=cost.stone;
    GUI.Label(new Rect(43,y+38,233,23),max?"All material unlocks complete":$"{woodCost:0} timber / {stoneCost:0} stone",CardStyle(11,muted));Button(282,y+15,103,37,max?"MAX":n==0?"BUILD":"UPGRADE",()=>Buy(id),!max&&city.CanUpgrade(id));
   }
   Button(430,211,355,39,"PRODUCTION",()=>{industryPage=0;selection=0;});Button(798,211,355,39,"BLACKSMITH",()=>{industryPage=1;selection=0;});Button(1166,211,374,39,"STOCKPILE",()=>{industryPage=2;selection=0;});
   HudFrame(new Rect(430,266,1110,596));if(industryPage==0)ProductionPanel();else if(industryPage==1)BlacksmithPanel();else StockpilePanel();
   GUI.Label(new Rect(435,876,1105,22),notice,CardStyle(12,muted));
  }
  void ProductionPanel() {
   for(int i=0;i<3;i++){int source=i;Button(450+i*355,281,340,34,(industrySource==i?"â€¢ ":"")+ProducerNames[i],()=>industrySource=source);}
   int src=industrySource;if(src==0){GUI.Label(new Rect(453,340,1030,50),"The Mine uses work orders. Choose an ore and queue how many units to mine.",CardStyle(18,hudLight));Button(453,420,500,40,"OPEN MINE WORK ORDERS",()=>{SelectCityBuilding("mine");});return;}double used=city.UsedCapacity(src),cap=city.Capacity(src);GUI.Label(new Rect(453,330,1050,28),$"{ProducerNames[src]}  /  {used:0.#} of {cap:0.#} capacity allocated  /  {cap-used:0.#} free",CardStyle(18,hudLight,bold:true));
   GUI.Label(new Rect(453,363,1060,25),"Time per unit uses city time. Expeditions accelerate deliveries. Fractions are retained.",CardStyle(12,muted));
   for(int i=0;i<Settlement.ResourceNames[src].Length;i++){
    int index=i;float y=402+i*51;bool unlocked=city.ProducerLevel(src)>=Settlement.ResourceUnlocks[src][i];double effort=city.Allocation(src)[i];
    GUI.Label(new Rect(453,y+3,225,25),Settlement.ResourceNames[src][i],CardStyle(16,unlocked?parchment:muted));GUI.Label(new Rect(685,y+5,145,25),"Stock "+Math.Floor(city.ResourceAmount(src,i)),CardStyle(13,muted));
    GUI.Label(new Rect(830,y+5,290,26),unlocked?ProductionTime.PerUnit(city.ProductionRate(src,i))+" city time":"Requires level "+Settlement.ResourceUnlocks[src][i],CardStyle(13,hudLight));
    Button(1130,y,52,32,"âˆ’",()=>{if(city.Allocate(src,index,effort-1))Save();},unlocked&&effort>=1);GUI.Label(new Rect(1187,y+5,75,25),effort.ToString("0.#"),CardStyle(15,parchment,TextAnchor.UpperCenter));
    Button(1265,y,52,32,"+",()=>{if(city.Allocate(src,index,effort+1))Save();},unlocked&&used<cap);Button(1330,y,185,32,"ALL CAPACITY",()=>{Array.Clear(city.Allocation(src),0,city.Allocation(src).Length);city.Allocate(src,index,city.Capacity(src));Save();},unlocked);
    HudLine(new Vector2(453,y+42),new Vector2(1515,y+42),new Color(.13f,.15f,.14f));
   }
   Button(1170,817,345,30,"CLEAR ALLOCATION",()=>{Array.Clear(city.Allocation(src),0,city.Allocation(src).Length);Save();});
  }
  void BlacksmithPanel() {
   if(smithArmourPage){ArmourCraftPanel();return;}
   MetalPalette(452,286,1060);VeilWeaponButtons(453,360,1060);int weapon=SelectedWeapon;var def=Metals.All[WeaponRules.Material(weapon)];
   GUI.Label(new Rect(453,403,1050,30),WeaponRules.Name(weapon)+" / Blacksmith "+def.unlock,CardStyle(21,hudLight,bold:true));
   GUI.Label(new Rect(453,445,1050,49),$"Weapon: {WeaponRules.Damage(weapon):0.#} damage / {WeaponRules.Interval(weapon):0.##}s attack interval / {WeaponRules.Damage(weapon)/WeaponRules.Interval(weapon):0.##} base DPS\nAll ability cooldowns scale by {WeaponRules.CooldownMultiplier(WeaponRules.Interval(weapon))*100:0.#}% before Haste.",CardStyle(16,parchment));
   GUI.Label(new Rect(453,505,1050,28),"BARS OWNED  "+Math.Floor(city.metalBars[weapon])+" / "+WeaponRules.BarName(weapon),CardStyle(17,hudLight,bold:true));GUI.Label(new Rect(453,545,1050,40),"1 bar: "+city.RecipeText(city.BarRecipe(weapon)),CardStyle(15,muted));
   Button(1137,597,378,40,"CRAFT ARMOUR",()=>{smithArmourPage=true;selection=0;});
   Button(453,597,325,40,"QUEUE 1 BAR",()=>{city.MakeBars(SelectedWeapon,1,out notice);Save();},city.blacksmith>=def.unlock&&city.CraftResearchRequirement(SelectedWeapon,"blacksmith")=="");Button(795,597,325,40,"QUEUE 5 BARS",()=>{city.MakeBars(SelectedWeapon,5,out notice);Save();},city.blacksmith>=def.unlock&&city.CraftResearchRequirement(SelectedWeapon,"blacksmith")=="");
   GUI.Label(new Rect(453,673,1050,44),"GREATSWORD: "+city.RecipeText(city.WeaponRecipe(weapon)),CardStyle(17,parchment));
   GUI.Label(new Rect(453,717,1050,27),city.CraftResearchRequirement(weapon,"weapons")!=""?city.CraftResearchRequirement(weapon,"weapons"):city.CraftResearchRequirement(weapon,"blacksmith")!=""?city.CraftResearchRequirement(weapon,"blacksmith"):"Both Veilcrystal variants retain separate bars and weapon profiles.",CardStyle(12,muted));
   Button(453,773,325,42,"FORGE WEAPON",ForgeSelectedWeapon,city.blacksmith>=def.unlock&&city.CraftResearchRequirement(SelectedWeapon,"weapons")=="");Button(795,773,325,42,"COMPARE WEAPONS",()=>OpenGearBag(9),true);Button(1137,773,378,42,"LOADOUT",()=>{tab=1;selection=0;});
  }
  void StockpilePanel() {
   GUI.Label(new Rect(453,285,1050,30),"MATERIAL STOCKPILE",CardStyle(22,hudLight,bold:true));int slot=0;
   foreach(int family in new[]{0,3,1,2}){int length=family==3?WeaponRules.Count:Settlement.ResourceNames[family].Length;for(int i=0;i<length;i++){
    float x=453+(slot%4)*265,y=341+(slot/4)*64;GUI.Label(new Rect(x,y,245,24),city.ResourceName(family,i),CardStyle(14,parchment));GUI.Label(new Rect(x,y+25,245,25),Math.Floor(city.ResourceAmount(family,i)).ToString("0"),CardStyle(18,hudLight,bold:true));slot++;
   }}
  }
 }
}
