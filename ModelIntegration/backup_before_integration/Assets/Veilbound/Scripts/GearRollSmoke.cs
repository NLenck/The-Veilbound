using System;
using System.IO;
using System.Collections;
using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  sealed class FixedGearRandom:System.Random {readonly double roll;public FixedGearRandom(double n){roll=n;}public override double NextDouble()=>roll;public override int Next(int maxValue)=>0;}
  void CheckGearRolls() {
   double[] thresholds={.1,.6,.8,.93,.99};float[] crit={1,1.3f,1.6f,1.9f,2};
   for(int r=0;r<5;r++){
    var major=GearRollRules.Roll(2,102,1,new FixedGearRandom(thresholds[r]));var minor=GearRollRules.Roll(3,102,2,new FixedGearRandom(thresholds[r]));
    Require((int)major.rarity==r&&major.affixes.Length==GearRollRules.Count((UpgradeRarity)r),"Each craft rarity has its intended affix count");Require(Math.Abs(major.affixes[0].value-crit[r])<.00001f&&Math.Abs(minor.affixes[0].value-major.affixes[0].value*.5f)<.00001f,"Exact requested crit values and minor scaling");
    var unique=new System.Collections.Generic.HashSet<string>();foreach(var a in major.affixes)Require(unique.Add(a.stat),"A crafted item cannot repeat the same stat");
   }
   var legendary=GearRollRules.Roll(2,100,1,new FixedGearRandom(.99));Require(GearRollRules.Value(legendary,"crit")==2&&Math.Abs(GearRollRules.Value(legendary,"speed")-4.2f)<.00001f&&GearRollRules.Value(legendary,"critdamage")==20,"Legendary major matches the example when those stats are drawn");
   var rng=new System.Random(8192);int[] counts=new int[5],stats=new int[8];for(int n=0;n<50000;n++){var item=GearRollRules.Roll(2,102,n,rng);counts[(int)item.rarity]++;if(item.rarity==UpgradeRarity.Common)stats[Array.IndexOf(GearRollRules.Stats,item.affixes[0].stat)]++;}
   var odds=Rarities.Chances(0);for(int i=0;i<5;i++)Require(Math.Abs(counts[i]/50000d-odds[i])<.009,"Craft distribution matches ability odds before luck");for(int i=0;i<8;i++)Require(Math.Abs(stats[i]/(double)counts[0]-GearRollRules.Weights[i]/100d)<.014,"Weighted stat selection matches initial tuning");
   var sample=new Settlement{blacksmith=9};sample.EnsureGearRolls();sample.metalBars[2]=100;
   Require(sample.CraftPlate(2,2,out _),"First plate roll crafts");int first=sample.lastCraftedRollId;sample.EquipRoll(first);float stock=(float)sample.metalBars[2];Require(sample.CraftPlate(2,2,out _)&&sample.lastCraftedRollId!=first&&sample.EquippedRoll(2).id==first&&sample.metalBars[2]==stock-8,"Repeated crafts cost materials, retain both copies and preserve equipped gear");
   int second=sample.lastCraftedRollId;Require(sample.FindRoll(first)!=null&&sample.FindRoll(second)!=null,"Copies have stable distinct identities");int before=sample.craftedGear.Count;sample.metalBars[2]=0;Require(!sample.CraftPlate(2,2,out _)&&sample.craftedGear.Count==before&&sample.EquippedRoll(2).id==first,"Unfunded crafts create no roll or equipment change");
   Require(Metals.PlateBars(0)==6&&Metals.PlateBars(2)==8&&Metals.PlateBars(6)==6&&Metals.PlateBars(3)==4,"Major pieces cost more than half-strength minor rolls");
   sample.EquipRoll(second);var save=JsonUtility.FromJson<Settlement>(JsonUtility.ToJson(sample));save.EnsureGearRolls();int savedCount=save.craftedGear.Count;save.EnsureGearRolls();Require(save.craftedGear.Count==savedCount&&save.EquippedRoll(2).id==second&&save.FindRoll(first)!=null&&save.FindRoll(first).affixes[0].value==sample.FindRoll(first).affixes[0].value,"Reload preserves exact affixes, equipment identity and every owned copy without rerolling");
   var old=new Settlement{armour=2,equippedArmour=2,sword=2};old.EnsureGearRolls();int oldCount=old.craftedGear.Count;old.EnsureGearRolls();Require(old.EquippedRoll(2).inherited&&old.GearBonus("crit")==0&&old.Item(2)==32&&old.craftedGear.Count==oldCount,"Legacy gear is imported once and keeps its base without free affixes");
   // Material progression does not force a crafting rarity.
   Require(GearRollRules.Roll(2,109,4,new FixedGearRandom(.1)).rarity==UpgradeRarity.Common&&GearRollRules.Roll(2,100,5,new FixedGearRandom(.99)).rarity==UpgradeRarity.Legendary,"Any metal can roll any craft rarity");
   Debug.Log("VEILBOUND_GEAR_ROLLS_LOGIC_PASS");
  }
  CraftedGear ReviewRoll(int slot,int item,double tier){var roll=GearRollRules.Roll(slot,item,city.nextGearRollId++,new FixedGearRandom(tier));city.craftedGear.Add(roll);return roll;}
  IEnumerator GearRollReview(string folder) {
   Directory.CreateDirectory(folder);CheckGearRolls();ReturnHome();var previous=city;city=new Settlement{blacksmith=9,mine=7,huntersLodge=5,weaver=5,bronzeHeroVersion=1};city.EnsureGear();for(int s=0;s<8;s++){city.ownedPlate[s*10+2]=true;city.equippedGear[s]=102;}city.equippedGear[8]=11;city.ownedGear[24]=1;city.equippedMetalWeapon=2;city.ownedMetalWeapons[2]=true;city.EnsureGearRolls();
   var rare=ReviewRoll(2,102,.8);city.EquipRoll(rare.id);var best=ReviewRoll(2,102,.99);ReviewRoll(2,102,.1);ReviewRoll(2,102,.6);ReviewRoll(2,102,.93);RebuildHero();
   OpenGearBag(2,best.id);yield return CardScreenshot(Path.Combine(folder,"01-chest-comparison.png"));gearBagRules=true;yield return CardScreenshot(Path.Combine(folder,"02-major-roll-values.png"));
   var glove=ReviewRoll(3,102,.99);OpenGearBag(3,glove.id);yield return CardScreenshot(Path.Combine(folder,"03-minor-comparison.png"));gearBagRules=true;yield return CardScreenshot(Path.Combine(folder,"04-minor-roll-values.png"));
   city.EquipRoll(best.id);city.EquipRoll(glove.id);var sword=ReviewRoll(9,2,.93);OpenGearBag(9,sword.id);yield return CardScreenshot(Path.Combine(folder,"05-weapon-rolls.png"));city.EquipRoll(sword.id);CloseGearBag();
   var originalBuild=build;build=new RunBuild();float plainCrit=CritChance,plainSpeed=Speed;Require(Math.Abs(plainCrit-.049f)<.00001f&&Math.Abs(CritMultiplier-1.8f)<.00001f,"Equipped armour and weapon roll bonuses reach critical chance and damage");
   float bonus=city.GearBonus("crit");city.equippedRollIds[2]=0;Require(Math.Abs(CritChance-(bonus-2)*.01f)<.00001f&&Speed<plainSpeed,"Removing a rolled item removes its bonuses immediately");city.EquipRoll(best.id);
   // Verify the remaining affix pool through actual gameplay properties, not only aggregation.
   var extras=new CraftedGear{id=city.nextGearRollId++,slot=0,item=102,rarity=UpgradeRarity.Legendary,affixes=new[]{new GearAffix{stat="power",value=4.2f},new GearAffix{stat="vitality",value=20},new GearAffix{stat="area",value=4.2f}}};city.craftedGear.Add(extras);float beforeDamage=Damage,beforeHealth=MaxHealth,beforeArea=AreaMultiplier;city.EquipRoll(extras.id);Require(Math.Abs(Damage/beforeDamage-1.042f)<.0001f&&Math.Abs(MaxHealth-beforeHealth-20)<.0001f&&Math.Abs(AreaMultiplier-beforeArea-.042f)<.0001f,"Gear damage, health and area bonuses reach gameplay");
   StartRun();Require(city.EquippedRoll(2).id==best.id&&Math.Abs(CritChance-.049f)<.00001f,"Permanent equipment rolls survive starting a new expedition");combatReviewFreeze=true;OpenCharacterSheet(0);yield return CardScreenshot(Path.Combine(folder,"06-equipped-character-stats.png"));CloseCharacterSheet();ReturnHome();tab=1;gearSlot=2;gearMaterial=3;selectedMetal=2;RebuildHero();yield return CardScreenshot(Path.Combine(folder,"07-equipped-paperdoll.png"));
   city=previous;build=originalBuild;gearBagOpen=false;RebuildHero();File.WriteAllText(Path.Combine(folder,"PASS.txt"),"PASS: ability rarity odds before luck; weighted unique stats; exact major/minor values; repeat crafting and atomic costs; retained copies and explicit equip; JSON save identity; legacy migration; material/craft rarity independence; combat stats and new-run persistence.\n");Debug.Log("VEILBOUND_GEAR_ROLLS_PASS");
  }
 }
}


