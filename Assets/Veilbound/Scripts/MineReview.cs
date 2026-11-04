using System;
using System.IO;
using System.Collections;
using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  IEnumerator MineReview(){
   yield return null;combatReviewFreeze=true;ReturnHome();city=Settlement.NewCamp();FundTownTest(city);city.mine=city.blacksmith=city.huntersLodge=city.weaver=3;city.quarry=city.timber=3;city.library=1;city.EnsureMineOrders();city.mineOrders.Clear();
   mineSelection=0;mineAmountText="10";QueueMineSelection();Require(city.mineOrders.Count==1&&city.mineOrders[0].quantity==10,"mine quantity UI action queues real order");city.QueueMining(1,5,out _);city.QueueMining(3,3,out _);city.Tick(90,1);
   city.researchRanks[LibraryResearch.Index("mine")]=3;city.QueueBars(2,5,out _);city.StartResearch("mine",out _);
   Require(BuildingActivity("mine",out var mineTip).StartsWith("Next ")&&mineTip.Contains("Tin ore"),"mine map badge shows next ore");
   Require(BuildingActivity("blacksmith",out var barTip).StartsWith("Next ")&&barTip.Contains(WeaponRules.BarName(2)),"smith map badge shows next bar");
   Require(BuildingActivity("library",out _).StartsWith("Study "),"library map badge shows research countdown");
   Require(BuildingActivity("timber",out var woodTip).StartsWith("Next ")&&woodTip.Contains("Timber"),"passive producer shows next whole unit");
   city.activeConstruction="quarry";city.constructionTarget=4;city.constructionRemaining=12;Require(BuildingActivity("quarry",out _).StartsWith("Build "),"construction takes badge priority");city.activeConstruction="";
   tab=2;sanctuaryWorkshopOpen=false;SetState(ScreenState.Settlement);SelectCityBuilding("mine");notice="";
   string folder=Path.Combine(Application.dataPath,"..","Smoke","MineOrders");Directory.CreateDirectory(folder);yield return null;yield return null;yield return CardScreenshot(Path.Combine(folder,"01-mine-work-orders.png"));
   cityResourcePage=1;yield return CardScreenshot(Path.Combine(folder,"02-locked-ores.png"));SelectCityBuilding("timber");yield return CardScreenshot(Path.Combine(folder,"03-unit-duration.png"));
   SelectCityBuilding("blacksmith");OpenCityWorkshop(false);yield return CardScreenshot(Path.Combine(folder,"04-bars-unit-duration.png"));
   CheckEconomyBalance();CheckMetalProgression();CheckExpeditionSetup();
   File.WriteAllText(Path.Combine(folder,"PASS.txt"),"PASS: mining queue UI action, whole deliveries, locked ores, unit-time display, mine/smith/research/construction/passive map badges, economy and expedition projection regressions.");Debug.Log("VEILBOUND_MINE_RUNTIME_PASS");Application.Quit(0);
  }
 }
}
