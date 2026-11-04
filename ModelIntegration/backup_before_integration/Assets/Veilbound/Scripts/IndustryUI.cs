using System;
using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  int industryPage,industrySource,selectedMetal;Vector2 industryScroll;
  string[] ProducerNames={"Mine","Hunter's Lodge","Textile Supply"};
  void RefreshMetalGear(){RebuildHero();cachedEquipmentHero=null;Save();}
  void CraftSelectedPlate(){if(city.CraftPlate(gearSlot,selectedMetal,out notice)){RefreshMetalGear();OpenGearBag(gearSlot,city.lastCraftedRollId);}}
  void ForgeSelectedWeapon(){if(city.CraftMetalWeapon(selectedMetal,out notice)){RefreshMetalGear();OpenGearBag(9,city.lastCraftedRollId);}}
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
   GUI.Label(new Rect(54,773,365,54),$"Preview: {Metals.Armour(gearSlot,selectedMetal):0.#} armour\n{Metals.Mass(gearSlot,selectedMetal):0.##} kg  /  -{Metals.Mass(gearSlot,selectedMetal)*.25f:0.##}% movement",CardStyle(13,muted));FamilyButtons();
   MetalPalette(435,732);GUI.Label(new Rect(435,811,672,48),def.name+" / Material tier: "+def.rarity+" / Smith "+def.unlock+"\n"+city.RecipeText(city.PlateRecipe(gearSlot,selectedMetal))+"  ·  Have "+Math.Floor(city.metalBars[selectedMetal])+" bars",CardStyle(12,hudLight));
   Button(1135,733,405,35,"CRAFT NEW ROLL",CraftSelectedPlate,city.blacksmith>=def.unlock);
   Button(1135,780,405,35,"BAG / COMPARE ROLLS",()=>OpenGearBag(gearSlot),true);
   Button(1135,827,195,35,"REMOVE",()=>RemoveEquipmentPiece(gearSlot),current>0);Button(1342,827,198,35,"BLACKSMITH",()=>{tab=2;industryPage=1;sanctuaryWorkshopOpen=true;selection=0;});
  }
  void MetalWeaponDetails() {
   city.EnsureGear();if(gearSlot==10){GUI.Label(new Rect(54,735,1000,35),"OFF HAND / TWO-HANDED GREATSWORD",CardStyle(22,parchment,bold:true));GUI.Label(new Rect(54,785,900,40),"The equipped greatsword occupies both hands.",CardStyle(15,muted));Button(1135,770,405,42,"VIEW MAIN HAND",()=>SelectPaperSlot(9));return;}
   var def=Metals.All[selectedMetal];bool owned=city.ownedMetalWeapons[selectedMetal],equipped=city.WeaponMetalIndex==selectedMetal;
   GUI.Label(new Rect(54,731,365,35),"MAIN HAND / "+city.WeaponName,CardStyle(19,parchment,bold:true));GUI.Label(new Rect(54,780,365,75),$"Preview: {def.weapon:0.#} weapon damage\n{4.5f*def.weight:0.##} kg / -{4.5f*def.weight*.25f:0.##}% movement\nTwo-handed · Full-strength rolls",CardStyle(13,muted));
   MetalPalette(435,732);GUI.Label(new Rect(435,811,672,48),def.name+" / Material tier: "+def.rarity+" / Smith "+def.unlock+"\n"+city.RecipeText(city.WeaponRecipe(selectedMetal)),CardStyle(12,hudLight));
   Button(1135,733,405,35,"FORGE NEW ROLL",ForgeSelectedWeapon,city.blacksmith>=def.unlock);Button(1135,780,405,35,"BAG / COMPARE ROLLS",()=>OpenGearBag(9),true);Button(1135,827,405,35,"OPEN BLACKSMITH",()=>{tab=2;industryPage=1;sanctuaryWorkshopOpen=true;selection=0;});
  }
  void LegacyIndustryScreen() {
   city.EnsureGear();city.EnsureIndustry();InitHudArt();Box(new Rect(0,0,1600,900),new Color(.018f,.025f,.027f));HudFrame(new Rect(30,18,1540,96));
   GUI.Label(new Rect(54,35,850,38),"SETTLEMENT / INDUSTRY & CRAFT",CardStyle(27,parchment,bold:true));
   Button(54,137,312,42,"EXPEDITION",()=>{tab=0;selection=0;});Button(644,137,312,42,"LOADOUT",()=>{tab=1;selection=0;});Button(1234,137,312,42,"SETTLEMENT",()=>{});
   for(int row=0;row<Settlement.Buildings.Length;row++){
    string id=Settlement.Buildings[row];int n=city.BuildingLevel(id);float y=211+row*76;HudFrame(new Rect(30,y,370,68));
    GUI.Label(new Rect(43,y+8,233,23),Settlement.BuildingNames[row]+" / "+n,CardStyle(15,parchment,bold:true));
    Settlement.BuildingCost(n,out double woodCost,out double stoneCost);bool max=(id=="mine"&&n>=7)||((id=="hunters"||id=="weaver")&&n>=5)||(id=="blacksmith"&&n>=9)||((id=="tailor"||id=="leatherworker")&&n>=2);
    GUI.Label(new Rect(43,y+38,233,23),max?"All material unlocks complete":$"{woodCost:0} timber / {stoneCost:0} stone",CardStyle(11,muted));Button(282,y+15,103,37,max?"MAX":n==0?"BUILD":"UPGRADE",()=>Buy(id),!max);
   }
   Button(430,211,355,39,"PRODUCTION",()=>{industryPage=0;selection=0;});Button(798,211,355,39,"BLACKSMITH",()=>{industryPage=1;selection=0;});Button(1166,211,374,39,"STOCKPILE",()=>{industryPage=2;selection=0;});
   HudFrame(new Rect(430,266,1110,596));if(industryPage==0)ProductionPanel();else if(industryPage==1)BlacksmithPanel();else StockpilePanel();
   GUI.Label(new Rect(435,876,1105,22),notice,CardStyle(12,muted));
  }
  void ProductionPanel() {
   for(int i=0;i<3;i++){int source=i;Button(450+i*355,281,340,34,(industrySource==i?"• ":"")+ProducerNames[i],()=>industrySource=source);}
   int src=industrySource;double used=city.UsedCapacity(src),cap=city.Capacity(src);GUI.Label(new Rect(453,330,1050,28),$"{ProducerNames[src]}  /  {used:0.#} of {cap:0.#} capacity allocated  /  {cap-used:0.#} free",CardStyle(18,hudLight,bold:true));
   GUI.Label(new Rect(453,363,1060,25),"Yields are per in-game hour (1 real minute), before expedition bonuses. Fractions are retained.",CardStyle(12,muted));
   for(int i=0;i<Settlement.ResourceNames[src].Length;i++){
    int index=i;float y=402+i*51;bool unlocked=city.ProducerLevel(src)>=Settlement.ResourceUnlocks[src][i];double effort=city.Allocation(src)[i];
    GUI.Label(new Rect(453,y+3,225,25),Settlement.ResourceNames[src][i],CardStyle(16,unlocked?parchment:muted));GUI.Label(new Rect(685,y+5,145,25),"Stock "+Math.Floor(city.ResourceAmount(src,i)),CardStyle(13,muted));
    GUI.Label(new Rect(830,y+5,290,26),unlocked?$"{Settlement.ResourceYields[src][i]:0.###}/capacity  →  {city.ProductionRate(src,i):0.###}/h":"Requires level "+Settlement.ResourceUnlocks[src][i],CardStyle(13,hudLight));
    Button(1130,y,52,32,"−",()=>{if(city.Allocate(src,index,effort-1))Save();},unlocked&&effort>=1);GUI.Label(new Rect(1187,y+5,75,25),effort.ToString("0.#"),CardStyle(15,parchment,TextAnchor.UpperCenter));
    Button(1265,y,52,32,"+",()=>{if(city.Allocate(src,index,effort+1))Save();},unlocked&&used<cap);Button(1330,y,185,32,"ALL CAPACITY",()=>{Array.Clear(city.Allocation(src),0,city.Allocation(src).Length);city.Allocate(src,index,city.Capacity(src));Save();},unlocked);
    HudLine(new Vector2(453,y+42),new Vector2(1515,y+42),new Color(.13f,.15f,.14f));
   }
   Button(1170,817,345,30,"CLEAR ALLOCATION",()=>{Array.Clear(city.Allocation(src),0,city.Allocation(src).Length);Save();});
  }
  void BlacksmithPanel() {
   if(smithArmourPage){ArmourCraftPanel();return;}
   MetalPalette(452,286,1060);var def=Metals.All[selectedMetal];
   GUI.Label(new Rect(453,377,1050,35),def.name.ToUpper()+" / Material tier: "+def.rarity+" / Requires Blacksmith "+def.unlock,CardStyle(22,hudLight,bold:true));
   GUI.Label(new Rect(453,424,1045,57),$"Chest: {def.armour:0} armour / {8*def.weight:0.##} kg / -{2*def.weight:0.##}% movement\nWeapon: {def.weapon:0} damage / {4.5f*def.weight:0.##} kg  ·  Titanium and Tungsten share the Epic tier.",CardStyle(16,parchment));
   GUI.Label(new Rect(453,505,1050,28),"BARS OWNED  "+Math.Floor(city.metalBars[selectedMetal]),CardStyle(19,hudLight,bold:true));GUI.Label(new Rect(453,545,1050,40),"1 bar: "+city.RecipeText(city.BarRecipe(selectedMetal)),CardStyle(16,muted));
   Button(1137,597,378,40,"CRAFT ARMOUR",()=>{smithArmourPage=true;selection=0;});
   Button(453,597,325,40,"MAKE 1 BAR",()=>{city.MakeBars(selectedMetal,1,out notice);Save();},city.blacksmith>=def.unlock);Button(795,597,325,40,"MAKE 5 BARS",()=>{city.MakeBars(selectedMetal,5,out notice);Save();},city.blacksmith>=def.unlock);
   GUI.Label(new Rect(453,673,1050,44),"GREATSWORD: "+city.RecipeText(city.WeaponRecipe(selectedMetal)),CardStyle(17,parchment));
   GUI.Label(new Rect(453,717,1050,27),"Grip leather follows the material tier, not rolled rarity. Craft odds: 50% / 25% / 15% / 8% / 2%, before luck.",CardStyle(12,muted));
   Button(453,773,325,42,"FORGE NEW ROLL",ForgeSelectedWeapon,city.blacksmith>=def.unlock);Button(795,773,325,42,"COMPARE WEAPON ROLLS",()=>OpenGearBag(9),true);Button(1137,773,378,42,"LOADOUT",()=>{tab=1;selection=0;});
  }
  void StockpilePanel() {
   GUI.Label(new Rect(453,285,1050,30),"MATERIAL STOCKPILE",CardStyle(22,hudLight,bold:true));int slot=0;
   foreach(int family in new[]{0,3,1,2}){int length=family==3?10:Settlement.ResourceNames[family].Length;for(int i=0;i<length;i++){
    float x=453+(slot%4)*265,y=341+(slot/4)*69;GUI.Label(new Rect(x,y,245,24),city.ResourceName(family,i),CardStyle(14,parchment));GUI.Label(new Rect(x,y+25,245,25),Math.Floor(city.ResourceAmount(family,i)).ToString("0"),CardStyle(18,hudLight,bold:true));slot++;
   }}
  }
 }
}
