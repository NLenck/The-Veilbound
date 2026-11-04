using System;using System.IO;using System.Collections;using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  static Settlement DailyRateSample(){var c=new Settlement{mine=7,weaver=5,huntersLodge=5};c.EnsureGear();c.EnsureIndustry();for(int f=0;f<3;f++){Array.Clear(c.Allocation(f),0,c.Allocation(f).Length);for(int i=0;i<c.Allocation(f).Length;i++)c.Allocate(f,i,1);}return c;}
  static double[] DailyStocks(Settlement c){var values=new double[20];values[0]=c.wood;values[1]=c.stone;int n=10;for(int f=1;f<3;f++)for(int i=0;i<Settlement.ResourceNames[f].Length;i++)values[n++]=c.ResourceAmount(f,i);return values;}
  IEnumerator TownDailyRatesReview(){
   yield return null;combatReviewFreeze=true;
   var c=DailyRateSample();var before=DailyStocks(c);c.Tick(60,1);var after=DailyStocks(c);
   Require(Math.Abs(c.hours-1)<1e-9,"one active minute still advances one city hour");Require(Math.Abs(after[0]-before[0]-10d*Settlement.PassiveProductionMultiplier/24)<1e-9&&Math.Abs(after[1]-before[1]-7d*Settlement.PassiveProductionMultiplier/24)<1e-9,"wood and stone now use daily yields");
   int n=10;for(int f=1;f<3;f++)for(int i=0;i<Settlement.ResourceNames[f].Length;i++)Require(Math.Abs(after[n]-before[n++]-Settlement.ResourceYields[f][i]*Settlement.PassiveProductionMultiplier/24)<1e-9,"every gathered material retains its fractional hourly progress");
   var daily=DailyRateSample();before=DailyStocks(daily);daily.Tick(1440,1);after=DailyStocks(daily);Require(daily.hours==24&&Math.Abs(after[0]-before[0]-10*Settlement.PassiveProductionMultiplier)<1e-9&&Math.Abs(after[1]-before[1]-7*Settlement.PassiveProductionMultiplier)<1e-9,"24 active minutes produce one day's wood and stone");n=10;for(int f=1;f<3;f++)for(int i=0;i<Settlement.ResourceNames[f].Length;i++)Require(Math.Abs(after[n]-before[n++]-Settlement.ResourceYields[f][i]*Settlement.PassiveProductionMultiplier)<1e-9,"one day produces the reduced industry amount");
   var stepped=DailyRateSample();for(int i=0;i<14400;i++){if(i==7200){stepped=JsonUtility.FromJson<Settlement>(JsonUtility.ToJson(stepped));stepped.EnsureGear();stepped.EnsureIndustry();}stepped.Tick(.1,1);}var steppedStocks=DailyStocks(stepped);for(int i=0;i<after.Length;i++)Require(Math.Abs(after[i]-steppedStocks[i])<1e-7,"small frame ticks and save/reload preserve fractional stock");
   var accelerated=DailyRateSample();accelerated.Tick(720,2);var acceleratedStocks=DailyStocks(accelerated);for(int i=0;i<after.Length;i++)Require(Math.Abs(after[i]-acceleratedStocks[i])<1e-8,"expedition multiplier still accelerates production consistently");
   CheckEconomyBalance();CheckMetalProgression();CheckExpeditionSetup();
   ReturnHome();city=DailyRateSample();city.EnsureGearRolls();CheckResourceTopBar();
   double clock=city.hours,stock=city.wood;SetState(ScreenState.CharacterSelect);Require(!SimulationActive,"character select does not advance settlement");yield return new WaitForSecondsRealtime(.2f);Require(city.hours==clock&&city.wood==stock,"inactive town has no time or resource progress");
   string output=Path.Combine(Application.dataPath,"..","Smoke","TownDailyRates");Directory.CreateDirectory(output);File.WriteAllText(Path.Combine(output,"PASS.txt"),"PASS: 20 passive materials at original daily output; 24 active minutes per baseline day; fractional per-frame accumulation and mid-day save/reload; expedition multipliers and projected yields; crafting, resource UI and independent idle-state pause regressions. No offline progress added. Existing balances preserved.");Debug.Log("VEILBOUND_TOWN_DAILY_RATES_RUNTIME_PASS");Application.Quit(0);
  }
 }
}
