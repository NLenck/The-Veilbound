using System;
using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace Veilbound {
 public partial class VeilGame {
  IEnumerator ProducerReview(){
   yield return null;combatReviewFreeze=true;ReturnHome();city=Settlement.NewCamp();FundTownTest(city);city.mine=city.huntersLodge=city.weaver=3;city.blacksmith=4;city.library=1;city.timber=2;city.quarry=1;city.wood=60;city.stone=40;city.EnsureMineOrders();city.mineOrders.Clear();
   foreach(string branch in new[]{"mine","hunters","weaver"})city.researchRanks[LibraryResearch.Index(branch)]=3;
   for(int f=0;f<3;f++){Array.Clear(city.Allocation(f),0,city.Allocation(f).Length);for(int i=0;i<Settlement.ResourceNames[f].Length;i++)city.AddResource(f,i,24-city.ResourceAmount(f,i));}for(int i=0;i<WeaponRules.Count;i++)city.metalBars[i]=i<4?12:0;
   tab=2;SetState(ScreenState.Settlement);OpenProducer(0);Require(ProducerScreenOpen&&cityBuilding=="mine","Mine management opens in real building context");producerItem=0;producerAmount="4";ProducerQueue();Require(city.mineOrders.Count==1&&city.mineOrders[0].quantity==4,"quantity action queues real mining order");producerItem=1;producerAmount="2";ProducerRepeat();producerItem=0;producerAmount="1";ProducerRepeat();city.Tick(75,1);
   string output=Path.Combine(Application.dataPath,"..","Smoke","ProducerInterfaces");Directory.CreateDirectory(output);producerPage=0;ProducerSelect(0);yield return null;yield return null;Require(producerBackdrops[0],"Mine backdrop included in player");yield return CardScreenshot(Path.Combine(output,"01-mine-orders.png"));producerPage=1;yield return CardScreenshot(Path.Combine(output,"02-mine-repeat.png"));
   ProducerSelect(7);string before=JsonUtility.ToJson(city);ProducerQueue();ProducerRepeat();Require(JsonUtility.ToJson(city)==before&&ProducerRequirement(7)!="","locked ore actions cannot change queue or repeat schedule");
   OpenProducer(1);ProducerSelect(1);producerAmount="4";ProducerAssign();Require(city.huntEffort[1]==4,"Hunter allocation action updates actual production");ProducerSelect(0);ProducerAll();Require(city.huntEffort[0]==city.Capacity(1)&&city.UsedCapacity(1)==city.Capacity(1),"Assign all preserves shared capacity model");double allocation=city.huntEffort[0];ProducerSelect(4);producerAmount="10";ProducerAssign();Require(city.huntEffort[0]==allocation&&city.huntEffort[4]==0&&ProducerRequirement(4)!="","unresearched hide cannot change production");ProducerSelect(0);yield return CardScreenshot(Path.Combine(output,"03-hunters-production.png"));Require(producerBackdrops[1],"Hunter backdrop included");
   OpenProducer(2);ProducerSelect(2);producerAmount="6";ProducerAssign();Require(city.textileEffort[2]==6,"Textile allocation action updates actual silk production");producerAmount="999";ProducerAssign();Require(city.textileEffort[2]==6,"excess allocation rejected without spending or changes");producerAmount="6";producerFeedback="";yield return CardScreenshot(Path.Combine(output,"04-textile-production.png"));Require(producerBackdrops[2],"Textile backdrop included");producerPage=1;yield return CardScreenshot(Path.Combine(output,"05-textile-stockpile.png"));
   double silk=city.ResourceAmount(2,2),expected=city.ProductionRate(2,2);city.Tick(60,1);Require(Math.Abs(city.ResourceAmount(2,2)-silk-expected)<1e-8,"existing production remains live behind overlay");
   ToggleResourceCategory(3);Require(HandleResourcePointer(ResourceDropdownRect(3).center)&&resourceCategory==3,"resource dropdown blocks underlying actions");LeaveResourceBar();
   var pad=InputSystem.AddDevice<Gamepad>();InputSystem.QueueStateEvent(pad,new GamepadState());InputSystem.Update();InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.East));InputSystem.Update();Require(ReadProducerInput()&&!ProducerScreenOpen,"controller B closes producer overlay");InputSystem.RemoveDevice(pad);
   OpenProducer(0);StartRun();Require(!producerOpen&&!ProducerScreenOpen,"producer overlay cannot leak into expedition");ReturnHome();OpenProducer(1);OpenCityWorkshop(false);Require(!producerOpen&&SmithScreenOpen,"opening Blacksmith closes producer overlay");CloseSmith();
   CheckEconomyBalance();CheckMetalProgression();CheckExpeditionSetup();File.WriteAllText(Path.Combine(output,"PASS.txt"),"PASS: all producer backdrops, real building entry, queue/repeat actions, locks, assignment/capacity limits, active production, stockpile, resource popup blocking, controller return, and expedition/workshop isolation. Existing economy and projections unchanged.");Debug.Log("VEILBOUND_PRODUCER_INTERFACES_RUNTIME_PASS");Application.Quit(0);
  }
 }
}
