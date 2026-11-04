using System;
using System.IO;
using System.Collections;
using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  void CheckMetalProgression() {
   var sample=new Settlement{mine=3,weaver=3,huntersLodge=3};sample.EnsureGear();sample.EnsureIndustry();Array.Clear(sample.mineEffort,0,8);sample.iron=0;
   Require(sample.Allocate(0,0,3)&&sample.Allocate(0,1,3)&&sample.Allocate(0,3,10),"Mine accepts mixed allocations within shared capacity");sample.Tick(60,1);
   Require(Math.Abs(sample.ResourceAmount(0,0)-1.5)<1e-8&&Math.Abs(sample.ResourceAmount(0,1)-1.5)<1e-8&&Math.Abs(sample.iron-2.5)<1e-8&&sample.UsedCapacity(0)==16,"Sixteen effort yields 1.5 tin, 1.5 copper and 2.5 iron per city hour");
   double before=sample.UsedCapacity(0);Require(!sample.Allocate(0,0,999)&&sample.UsedCapacity(0)==before,"Over-allocation is rejected atomically");Require(!sample.Allocate(0,7,1),"Locked ores cannot receive effort");
   Array.Clear(sample.mineEffort,0,8);sample.Allocate(0,3,1);sample.iron=0;sample.Tick(60,1);Require(Math.Abs(sample.iron-.25)<1e-8,"A quarter iron unit is retained internally");
   sample=JsonUtility.FromJson<Settlement>(JsonUtility.ToJson(sample));sample.EnsureGear();sample.EnsureIndustry();sample.Tick(60,1);Require(Math.Abs(sample.iron-.5)<1e-8,"Fractional production survives saving and accumulates correctly");
   Array.Clear(sample.huntEffort,0,5);Array.Clear(sample.textileEffort,0,5);sample.Allocate(1,2,1);sample.Allocate(2,2,1);double hides=sample.ResourceAmount(1,2),silk=sample.ResourceAmount(2,2);sample.Tick(60,1);Require(Math.Abs(sample.ResourceAmount(1,2)-hides-.25)<1e-8&&Math.Abs(sample.ResourceAmount(2,2)-silk-.25)<1e-8,"Hunting and textiles use the same fractional allocation model");
   var crafter=new Settlement{blacksmith=9};crafter.EnsureGear();crafter.EnsureIndustry();double tin=crafter.ResourceAmount(0,0),copper=crafter.ResourceAmount(0,1);Require(!crafter.MakeBars(2,1,out _)&&crafter.ResourceAmount(0,0)==tin&&crafter.ResourceAmount(0,1)==copper,"Unfunded alloy recipes consume nothing");
   crafter.AddResource(0,0,1);crafter.AddResource(0,1,2);Require(crafter.MakeBars(2,1,out _)&&crafter.metalBars[2]==1&&crafter.ResourceAmount(0,0)==0&&crafter.ResourceAmount(0,1)==0,"Bronze consumes copper and tin");
   for(int f=0;f<3;f++)for(int i=0;i<Settlement.ResourceNames[f].Length;i++)crafter.AddResource(f,i,10000);for(int i=0;i<10;i++)crafter.metalBars[i]+=10000;
   Array.Clear(crafter.ownedMetalWeapons,0,10);for(int metal=0;metal<10;metal++){
    Require(crafter.MakeBars(metal,1,out _),"Every metal bar/alloy recipe can be made: "+Metals.All[metal].name);
    for(int slot=0;slot<8;slot++)Require(crafter.CraftPlate(slot,metal,out _)&&crafter.EquipRoll(crafter.lastCraftedRollId)&&crafter.OwnsPlate(slot,metal)&&crafter.Item(slot)==Metals.Item(metal),"Each material crafts into each real plate slot");
    Require(crafter.CraftMetalWeapon(metal,out _)&&crafter.EquipRoll(crafter.lastCraftedRollId)&&crafter.WeaponMetalIndex==metal,"Each metal has a forgeable greatsword");
    Require(crafter.WeaponRecipe(metal)[1].index==Math.Max(0,(int)Metals.All[metal].rarity-1),"Weapon grip leather is a rarity lower than its metal");
   }
   Require(Metals.All[7].rarity==Metals.All[8].rarity&&Metals.All[7].armour<Metals.All[8].armour&&Metals.All[7].weight<Metals.All[8].weight,"Titanium and tungsten share rarity but trade protection against mass");
   for(int s=0;s<8;s++)crafter.equippedGear[s]=107;crafter.equippedMetalWeapon=7;float titaniumSpeed=crafter.GearSpeed;for(int s=0;s<8;s++)crafter.equippedGear[s]=108;crafter.equippedMetalWeapon=8;Require(titaniumSpeed>crafter.GearSpeed,"The mass trade-off reaches actual movement speed");
   Array.Clear(crafter.equippedGear,0,9);float unarmouredMass=crafter.EquipmentMass;for(int s=0;s<8;s++)crafter.equippedGear[s]=21;Require(crafter.EquipmentMass==unarmouredMass,"Leather clothing adds no movement penalty");
   var legacy=new Settlement{armour=2,equippedArmour=2,sword=2,iron=35.5};legacy.EnsureGear();Require(legacy.Item(2)==32&&legacy.OwnsPlate(2,2)&&legacy.OwnsPlate(2,5)&&legacy.iron==35.5,"Legacy plate ownership and fractional stock survive migration");
   var saved=JsonUtility.FromJson<Settlement>(JsonUtility.ToJson(crafter));saved.EnsureGear();Require(saved.ownedMetalWeapons[9]&&saved.OwnsPlate(2,9)&&saved.metalBars[2]==crafter.metalBars[2],"New material ownership and bars roundtrip through the real save format");
   Debug.Log("VEILBOUND_METAL_PROGRESSION_LOGIC_PASS");
  }
  IEnumerator MetalProgressionReview(string folder) {
   Directory.CreateDirectory(folder);CheckMetalProgression();ReturnHome();FundTestCity();city.blacksmith=9;city.mine=7;city.huntersLodge=5;city.weaver=5;city.tailor=2;city.leatherworker=2;city.EnsureGear();city.EnsureIndustry();
   for(int family=0;family<3;family++)for(int i=0;i<Settlement.ResourceNames[family].Length;i++)city.AddResource(family,i,48);for(int i=0;i<10;i++){city.metalBars[i]=80;city.ownedMetalWeapons[i]=true;}
   tab=1;gearMaterial=3;gearSlot=2;
   for(int metal=0;metal<10;metal++){
    selectedMetal=metal;for(int slot=0;slot<8;slot++){city.ownedPlate[slot*10+metal]=true;city.equippedGear[slot]=Metals.Item(metal);}city.equippedMetalWeapon=metal;city.equippedSword=0;RebuildHero();yield return new WaitForSecondsRealtime(.18f);
    Require(cachedEquipmentHero==hero,"Paper doll and item thumbnails update for each metal");
    var visual=hero.root.GetComponent<BronzeHeroVisual>();foreach(var part in visual.parts)if(part.name.StartsWith("TRIM -"))Require(part.sharedMaterials[0].color.b>=part.sharedMaterials[0].color.r,"Silver trim stays silver across material recolours");
    yield return CardScreenshot(Path.Combine(folder,(metal+1).ToString("00")+"-"+Metals.All[metal].name.ToLowerInvariant().Replace(' ','-')+".png"));
   }
   tab=2;industryPage=0;industrySource=0;Array.Clear(city.mineEffort,0,8);city.Allocate(0,0,3);city.Allocate(0,1,3);city.Allocate(0,3,10);yield return new WaitForSecondsRealtime(.2f);yield return CardScreenshot(Path.Combine(folder,"11-mine-allocation.png"));
   industrySource=1;Array.Clear(city.huntEffort,0,5);city.Allocate(1,0,6);city.Allocate(1,2,8);yield return CardScreenshot(Path.Combine(folder,"12-hunters-lodge.png"));industrySource=2;yield return CardScreenshot(Path.Combine(folder,"13-textile-supply.png"));
   industryPage=1;selectedMetal=3;yield return CardScreenshot(Path.Combine(folder,"14-brass-blacksmith.png"));industryPage=2;yield return CardScreenshot(Path.Combine(folder,"15-stockpile.png"));
   File.WriteAllText(Path.Combine(folder,"PASS.txt"),"PASS: mixed capacity allocation; exact fractional rates and save roundtrip; locked ores and capacity limits; all three producers; atomic alloy costs; all ten metal recipes; eighty slot/material plate combinations; ten weapon recipes; lower-rarity leather grips; titanium/tungsten armour-mass tradeoff and actual speed; no leather garment movement penalty; legacy ownership migration; static paper-doll recolours and silver trim.\n");Debug.Log("VEILBOUND_METAL_PROGRESSION_PASS");tab=0;ReturnHome();
  }
 }
}


