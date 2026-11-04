using System;
using System.IO;
using System.Collections;
using UnityEngine;
namespace Veilbound {public partial class VeilGame {
 sealed class FixedGearRandom:System.Random {readonly double roll;public FixedGearRandom(double n){roll=n;}public override double NextDouble()=>roll;public override int Next(int maxValue)=>0;}
 void CheckGearRolls(){
  var c=QueueSample();c.metalBars[0]=100;FundPlateSupports(c,0,2,2);c.EnsureGearRolls();
  Require(c.CraftPlate(2,0,out _),"Craft armour");int first=c.lastCraftedRollId;c.EquipRoll(first);
  Require(c.CraftPlate(2,0,out _)&&c.lastCraftedRollId!=first&&c.EquippedRoll(2).id==first,"Repeated crafts retain copies and explicit equipment selection");
  foreach(var item in c.craftedGear){Require(item.affixes==null||item.affixes.Length==0,"Crafting has no random stat bonuses");foreach(string stat in GearRollRules.Stats)Require(GearRollRules.Value(item,stat)==0,"Unenchanted craft has zero bonus stats");}
  var legacy=c.FindRoll(first);legacy.affixes=new[]{new GearAffix{stat="power",value=999}};Require(GearRollRules.Value(legacy,"power")==0,"Retired crafting bonus cannot affect gameplay before migration");c.EnsureGearRolls();Require(legacy.affixes.Length==0&&c.EquippedRoll(2).id==first,"Retired affixes removed without replacing gear");
  c.metalBars[0]=0;int count=c.craftedGear.Count;Require(!c.CraftPlate(2,0,out _)&&c.craftedGear.Count==count,"Failed craft creates no item");
  var loaded=JsonUtility.FromJson<Settlement>(JsonUtility.ToJson(c));loaded.EnsureGearRolls();Require(loaded.EquippedRoll(2).id==first&&loaded.craftedGear.Count==count,"Craft copies and selection survive saving");
  Debug.Log("VEILBOUND_GEAR_ROLLS_LOGIC_PASS");
 }
 CraftedGear ReviewRoll(int slot,int item,double tier){var roll=GearRollRules.Roll(slot,item,city.nextGearRollId++,new FixedGearRandom(tier));city.craftedGear.Add(roll);return roll;}
 IEnumerator GearRollReview(string folder){Directory.CreateDirectory(folder);CheckGearRolls();OpenGearBag(2);yield return CardScreenshot(Path.Combine(folder,"01-gear.png"));CloseGearBag();Debug.Log("VEILBOUND_GEAR_ROLLS_PASS");}
}}
