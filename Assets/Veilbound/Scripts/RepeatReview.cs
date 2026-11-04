using System;
using System.IO;
using System.Collections;
using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  IEnumerator RepeatReview(){
   yield return null;combatReviewFreeze=true;ReturnHome();city=Settlement.NewCamp();FundTownTest(city);city.mine=3;city.blacksmith=4;city.library=1;city.timber=2;city.quarry=1;city.wood=60;city.stone=40;city.EnsureMineOrders();city.mineOrders.Clear();
   for(int f=0;f<3;f++)for(int i=0;i<Settlement.ResourceNames[f].Length;i++)city.AddResource(f,i,24-city.ResourceAmount(f,i));for(int i=0;i<WeaponRules.Count;i++)city.metalBars[i]=i<4?12:0;
   mineSelection=1;mineAmountText="2";AddMineRepeat();mineSelection=0;mineAmountText="1";AddMineRepeat();mineSelection=2;AddMineRepeat();
   smithBarRecipe=1;smithAmountText="2";AddSmithRepeat();smithBarRecipe=0;smithAmountText="1";AddSmithRepeat();smithBarRecipe=2;AddSmithRepeat();city.Tick(75,1);
   Require(city.mineSchedule.Count==3&&city.barSchedule.Count==3&&city.autoMineEnabled&&city.autoBarsEnabled,"UI actions build active repeating schedules");
   Require(BuildingActivity("mine",out var ore).StartsWith("Next ")&&ore.Contains("Copper ore"),"mine badge follows current repeating material");Require(BuildingActivity("blacksmith",out _).StartsWith("Next "),"smith badge follows repeat production");
   var saved=JsonUtility.FromJson<Settlement>(File.ReadAllText(savePath));Require(saved.mineSchedule.Count==3&&saved.barSchedule.Count==3,"game save retains schedule entries");
   tab=2;sanctuaryWorkshopOpen=false;SetState(ScreenState.Settlement);SelectCityBuilding("mine");mineScheduleView=true;notice="";
   string folder=Path.Combine(Application.dataPath,"..","Smoke","RepeatProduction");Directory.CreateDirectory(folder);yield return null;yield return null;yield return CardScreenshot(Path.Combine(folder,"01-mine-schedule.png"));
   SelectCityBuilding("blacksmith");OpenCityWorkshop(false);SetSmithPage(0);smithScheduleView=true;smithBarRecipe=2;smithFeedback="";yield return CardScreenshot(Path.Combine(folder,"02-bronze-schedule.png"));smithBarRecipe=3;yield return CardScreenshot(Path.Combine(folder,"03-brass-recipe.png"));
   city.QueueBars(0,2,out _);Require(SmithAutoStatus.Contains("queued orders"),"manual orders take priority in UI status");city.smithOrders.Clear();city.barScheduleIndex=2;city.autoBarOrder=null;city.metalBars[0]=city.metalBars[1]=0;Require(BuildingActivity("blacksmith",out var wait)=="Waiting"&&wait.Contains("ingredients"),"map badge reports missing repeat inputs");
   CheckEconomyBalance();CheckMetalProgression();CheckExpeditionSetup();File.WriteAllText(Path.Combine(folder,"PASS.txt"),"PASS: UI-built repeating schedules, saved entries, current-step map badges, alloy recipes, queued-order priority, waiting status, economy and expedition projection regressions.");Debug.Log("VEILBOUND_REPEAT_RUNTIME_PASS");Application.Quit(0);
  }
 }
}
