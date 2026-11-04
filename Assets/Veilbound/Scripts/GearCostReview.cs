using System;using System.IO;using System.Collections;using UnityEngine;
namespace Veilbound {public partial class VeilGame {
 int gearCostChecks;
 void CostCheck(bool pass,string message){Require(pass,"Gear costs: "+message);gearCostChecks++;}
 static Settlement CostSample(){var c=SharedProductionSample();for(int f=0;f<6;f++){int n=f==3?WeaponRules.Count:f>=4?5:Settlement.ResourceNames[f].Length;for(int i=0;i<n;i++)c.AddResource(f,i,-c.ResourceAmount(f,i));}for(int f=0;f<3;f++)Array.Clear(c.Allocation(f),0,c.Allocation(f).Length);c.essence=100000;c.mineOrders.Clear();return c;}
 static string CostStock(Settlement c){string s=c.essence+"/";for(int f=0;f<6;f++){int n=f==3?WeaponRules.Count:f>=4?5:Settlement.ResourceNames[f].Length;for(int i=0;i<n;i++)s+=c.ResourceAmount(f,i)+",";}return s;}
 static bool CostStocksEqual(Settlement a,Settlement b){if(Math.Abs(a.essence-b.essence)>1e-6)return false;for(int f=0;f<6;f++){int n=f==3?WeaponRules.Count:f>=4?5:Settlement.ResourceNames[f].Length;for(int i=0;i<n;i++)if(Math.Abs(a.ResourceAmount(f,i)-b.ResourceAmount(f,i))>1e-6)return false;}return true;}
 void CheckCostEntry(CraftingRecipeSet set,EquipmentRecipeEntry entry,Settlement c){
  CostCheck(entry.ingredients.Length==3,"Every recipe has its primary material plus two supports");
  foreach(var part in entry.ingredients)CostCheck(part.family>=3&&part.amount>=1&&part.amount==Math.Floor(part.amount),"Only whole finished materials are charged");
  foreach(var missing in entry.ingredients){
   foreach(var part in entry.ingredients)c.AddResource(part.family,part.index,part.amount);c.AddResource(missing.family,missing.index,-1);string before=CostStock(c);
   CostCheck(!set.Craft(entry,out _)&&CostStock(c)==before,"Missing any ingredient rejects the craft without consuming other stock or Essence");
   foreach(var part in entry.ingredients)c.AddResource(part.family,part.index,-c.ResourceAmount(part.family,part.index));
  }
  foreach(var part in entry.ingredients)c.AddResource(part.family,part.index,part.amount);double essence=c.essence;
  CostCheck(set.Craft(entry,out _)&&c.FindRoll(c.lastCraftedRollId).item==entry.item,"Exact mixed recipe crafts the selected output");
  foreach(var part in entry.ingredients)CostCheck(c.ResourceAmount(part.family,part.index)==0,"Every ingredient is charged exactly once");
  CostCheck(Math.Abs(c.essence-essence+entry.essence)<1e-8,"Existing Essence rule is preserved");
 }
 IEnumerator GearCostReview(){
  yield return null;combatReviewFreeze=true;int exceptions=0;Application.LogCallback errors=(m,s,t)=>{if(t==LogType.Exception)exceptions++;};Application.logMessageReceived+=errors;
  CheckEconomyBalance();CheckMetalProgression();
  int[] plateChest={8,10,11,12,13,14,16,17,18,19},lightChest={8,12,16,20,24,28,32,36,40,44};
  foreach(EquipmentRecipeKind kind in Enum.GetValues(typeof(EquipmentRecipeKind))){var c=CostSample();var set=c.EquipmentRecipes(kind);for(int row=0;row<set.Rows;row++)for(int column=0;column<set.Columns;column++){
   var entry=set.Entry(row,column);CheckCostEntry(set,entry,c);
   if(column==2)CostCheck(entry.ingredients[0].amount==(kind==EquipmentRecipeKind.Plate?plateChest[row]:lightChest[row]),"Chest progression matches the selected cost policy");
   if(row>0)CostCheck(entry.ingredients[0].amount>=set.Entry(row-1,column).ingredients[0].amount,"Primary material quantity never decreases at a higher tier");
  }if(kind==EquipmentRecipeKind.Weapon)CheckCostEntry(set,set.Entry(9,0,true),c);}
  // Real supply from empty stocks must traverse all three material families.
  foreach(int material in new[]{1,2,3})foreach(int tier in new[]{1,3,10}){
   var c=CostSample();int kind=material==3?3:5,index=material==3?tier-1:material;
   CostCheck(c.QueueSettlementJob(kind,"",index,3,2,material==3?0:tier,out _),"Mixed recipe batch queues successfully");c.Tick(7,1);var saved=JsonUtility.FromJson<Settlement>(JsonUtility.ToJson(c));
   c.Tick(60*4000,1);saved.Tick(60*4000,1);int item=material==3?Metals.Item(tier-1):LightGear.Item(material,tier);
   CostCheck(c.settlementQueue.Count==0&&c.craftedGear.FindAll(g=>g.slot==3&&g.item==item).Count==2,"Automatic supply gathers, processes and crafts exactly two selected items");
   CostCheck(CostStocksEqual(c,saved)&&saved.settlementQueue.Count==0,"Saving during mixed supply preserves paid work and final stocks");
   for(int f=0;f<6;f++)CostCheck(c.ResourceTotal(f)<1e-6,"Automatic batch leaves no excess gear materials");
  }
  var stepped=CostSample();stepped.QueueSettlementJob(5,"",2,3,1,1,out _);var bulk=JsonUtility.FromJson<Settlement>(JsonUtility.ToJson(stepped));bulk.Tick(60*100,1);for(int i=0;i<6000;i++)stepped.Tick(1,1);CostCheck(bulk.settlementQueue.Count==0&&stepped.settlementQueue.Count==0&&CostStocksEqual(bulk,stepped),"Frame and bulk time produce identical mixed-material gear stocks");
  var cancel=CostSample();cancel.EnsureGearRolls();int previousItems=cancel.craftedGear.Count;cancel.AddResource(0,0,1);cancel.QueueSettlementJob(5,"",1,3,1,1,out _);cancel.Tick(6,1);CostCheck(cancel.RemoveSettlementJob(cancel.settlementQueue[0].id),"Mixed supply goal can be cancelled");cancel.Tick(600,1);CostCheck(cancel.craftedGear.Count==previousItems,"Cancelled goal cannot later deliver gear");
  city=SharedProductionSample();RebuildHero();ReturnHome();tab=2;SetState(ScreenState.Settlement);string folder=Path.Combine(Application.dataPath,"..","Smoke","GearCosts");Directory.CreateDirectory(folder);
  OpenCityWorkshop(true);SetSmithPage(1);smithMetal=2;smithSlot=2;yield return CardScreenshot(Path.Combine(folder,"01-bronze-chest.png"));CloseSmith();OpenClothier(3);lightGearTier=10;gearSlot=2;yield return CardScreenshot(Path.Combine(folder,"02-leather-chest.png"));SetClothierPage(2);yield return CardScreenshot(Path.Combine(folder,"03-cloth-chest.png"));CloseClothier();
  CostCheck(exceptions==0,"Crafting panels render mixed ingredients without exceptions");Application.logMessageReceived-=errors;File.WriteAllText(Path.Combine(folder,"validation.json"),"{\"passed\":true,\"checks\":"+gearCostChecks+"}");Debug.Log("VEILBOUND_GEAR_COST_PASS checks="+gearCostChecks);Application.Quit(0);
 }
}}
