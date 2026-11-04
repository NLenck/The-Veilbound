using System;using System.IO;using System.Collections;using UnityEngine;
namespace Veilbound {public partial class VeilGame {
 [Serializable] sealed class ComparisonInventory {public CraftedGear[] items;public int[] equipped;public double essence;}
 string ComparisonSnapshot()=>JsonUtility.ToJson(new ComparisonInventory{items=city.craftedGear.ToArray(),equipped=city.equippedRollIds,essence=city.essence});
 int gearComparisonChecks;
 void ComparisonCheck(bool pass,string message){Require(pass,"Gear comparison: "+message);gearComparisonChecks++;}
 void ComparisonFixture(){
  city=SharedProductionSample();city.mysticLevel=4;city.EnsureGearRolls();var current=city.RecordCraft(2,101);city.EnchantGear(current.id,2,0,"power",0,out _);city.EnchantGear(current.id,2,0,"crit",1,out _);city.EquipRoll(current.id);
  var candidate=city.RecordCraft(2,LightGear.Item(2,5));city.EnchantGear(candidate.id,2,0,"power",0,out _);city.EnchantGear(candidate.id,2,0,"regen",1,out _);city.RecordCraft(2,LightGear.Item(1,5));RebuildHero();ReturnHome();tab=1;SetState(ScreenState.Settlement);OpenGearBag(2,candidate.id);menuNavigationActive=false;notice="";
 }
 partial void CheckGearComparison();
 IEnumerator GearComparisonReview(){
  yield return null;combatReviewFreeze=true;int exceptions=0;Application.LogCallback errors=(m,s,t)=>{if(t==LogType.Exception)exceptions++;};Application.logMessageReceived+=errors;
  bool before=Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-gearComparisonBefore");string folder=Path.Combine(Application.dataPath,"..","Smoke","GearComparison");Directory.CreateDirectory(folder);ComparisonFixture();
  string fixture=ComparisonSnapshot();File.WriteAllText(Path.Combine(folder,before?"before-fixture.json":"after-fixture.json"),fixture);
  yield return CardScreenshot(Path.Combine(folder,before?"before.png":"after.png"));ComparisonCheck(ComparisonSnapshot()==fixture,"Rendering the comparison does not alter inventory, equipment or enchantments");
  if(!before)CheckGearComparison();ComparisonCheck(exceptions==0,"Comparison screens render without exceptions");Application.logMessageReceived-=errors;Debug.Log(before?"VEILBOUND_GEAR_COMPARISON_BEFORE_CAPTURED":"VEILBOUND_GEAR_COMPARISON_PASS checks="+gearComparisonChecks);Application.Quit(0);
 }
}}
