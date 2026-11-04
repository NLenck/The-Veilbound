using System;
using System.IO;
using System.Collections;
using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  IEnumerator SmithReview(){
   yield return null;combatReviewFreeze=true;city=Settlement.NewCamp();city.EnsureResearch();city.blacksmith=10;city.library=1;city.timber=2;city.quarry=city.mine=city.weaver=city.huntersLodge=1;city.wood=32;city.stone=18;city.EnsureGearRolls();city.EnsureIndustry();foreach(string path in new[]{"blacksmith","weapons"})city.researchRanks[LibraryResearch.Index(path)]=10;city.researchRanks[LibraryResearch.Index("plate")]=3;
   for(int f=0;f<3;f++)for(int i=0;i<Settlement.ResourceNames[f].Length;i++)city.AddResource(f,i,100);for(int i=0;i<5;i++){city.AddResource(4,i,100);city.AddResource(5,i,100);}double[] stock={48,22,12,12,38,16,4,8,3,30,30};for(int i=0;i<stock.Length;i++)city.metalBars[i]=stock[i];
   city.QueueBars(2,10,out _);city.TickSmithOrders((3*1.5+.4)/city.RefiningSpeed);if(Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-smithPreview")){city.QueueBars(1,5,out _);city.QueueBars(0,2,out _);foreach(string path in new[]{"blacksmith","weapons","plate"})city.researchRanks[LibraryResearch.Index(path)]=3;}else{city.QueueBars(5,5,out _);city.QueueBars(9,2,out _);}
   ReturnHome();tab=2;OpenCityWorkshop(true);smithMetal=3;smithSlot=2;InitSmithArt();
   Require(smithBackdrop&&smithIconAtlas&&SmithScreenOpen,"Blacksmith resources and entry route load");
   if(Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-smithPreview")){
    if(Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-smithPreviewBars")){SetSmithPage(0);smithBarRecipe=2;}
    if(Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-smithPreviewWeapons")){SetSmithPage(2);smithMetal=9;}
    while(true)yield return null;
   }
   int records=city.craftedGear.Count;double bars=city.metalBars[3];CraftSmithSelection();Require(city.craftedGear.Count==records&&city.metalBars[3]==bars&&city.SmithRequirement(3,"plate")!="","UI craft action rejects unresearched Brass even with sufficient bars");
   city.researchRanks[LibraryResearch.Index("plate")]=4;CraftSmithSelection();Require(city.craftedGear.Count==records+1&&city.metalBars[3]==bars-city.PlateRecipe(2,3)[0].amount,"UI craft action uses completed research and exact recipe");var made=city.FindRoll(city.lastCraftedRollId);Require(made.slot==2&&made.item==103&&city.EquipRoll(made.id),"crafted Brass chest enters real inventory and equips");
   SetSmithPage(2);smithMetal=9;smithHeavyVeil=false;CraftSmithSelection();int light=city.lastCraftedRollId;Require(city.FindRoll(light).item==9,"Titanium-base Veilcrystal weapon uses stable item ID");smithHeavyVeil=true;CraftSmithSelection();Require(city.FindRoll(city.lastCraftedRollId).item==10&&city.lastCraftedRollId!=light,"Tungsten-base weapon remains distinct inventory item");
   Save();var saved=JsonUtility.FromJson<Settlement>(File.ReadAllText(savePath));saved.EnsureGearRolls();saved.EnsureSmithOrders();Require(saved.smithOrders.Count==3&&saved.smithOrders[0].completed==3&&Math.Abs(saved.smithOrders[0].remainingWork-1.1)<1e-7,"actual game save preserves active and queued orders plus partial bar work");Require(saved.FindRoll(light)!=null&&saved.FindRoll(made.id)!=null,"crafted equipment persists with work orders");city=saved;
   SetSmithPage(0);smithBarRecipe=0;smithAmountText="2";int count=city.smithOrders.Count;double tin=city.ResourceAmount(0,0);QueueSmithSelection();Require(city.smithOrders.Count==count+1&&city.ResourceAmount(0,0)==tin-2,"UI queues a desired amount and reserves ingredients");int id=city.smithOrders[city.smithOrders.Count-1].id;Require(city.CancelSmithOrder(id)&&city.ResourceAmount(0,0)==tin,"queued-order cancellation refunds exact reserved inputs");
   int active=city.smithOrders[0].id;double bronze=city.metalBars[2],copper=city.ResourceAmount(3,1);Require(city.CancelSmithOrder(active)&&city.metalBars[2]==bronze&&city.ResourceAmount(3,1)==copper+14,"active cancellation retains completed bars and refunds unfinished alloy ingredients");
   ToggleResourceCategory(2);Require(resourceCategory==2&&HandleResourcePointer(ResourceDropdownRect(2).center),"resource dropdown available and blocks click-through in Blacksmith");LeaveResourceBar();CloseSmith();Require(!SmithScreenOpen&&!sanctuaryWorkshopOpen,"back action returns to settlement");
   string output=Path.Combine(Application.dataPath,"..","Smoke","Blacksmith");Directory.CreateDirectory(output);File.WriteAllText(Path.Combine(output,"PASS.txt"),"PASS: Blacksmith assets, workshop entry/back, research-gated UI craft, exact costs, inventory/equip/save, two Veilcrystal variants, quantity work orders, actual save/reload of partial work and queue, active and pending refunds, and resource dropdown interaction. Editor suite covers timer boundaries, dilation, queue limits, upgrades and legacy saves.");Debug.Log("VEILBOUND_SMITH_RUNTIME_PASS");Application.Quit(0);
  }
 }
}
