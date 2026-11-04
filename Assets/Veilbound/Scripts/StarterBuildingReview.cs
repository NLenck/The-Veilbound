using System;using System.IO;using System.Collections;using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  int starterChecks;
  void StarterCheck(bool ok,string message){Require(ok,message);starterChecks++;}
  IEnumerator StarterBuildingReview(){
   yield return null;combatReviewFreeze=true;ReturnHome();city=FreshCharacterTown();tab=2;SetState(ScreenState.Settlement);SelectCityBuilding("timber");
   string folder=Path.Combine(Application.dataPath,"..","Smoke","StarterBuildings");Directory.CreateDirectory(folder);
   StarterCheck(VisibleSidebarBuildings().Length==2&&city.BuildingVisible("timber")&&city.BuildingVisible("housing"),"Fresh camp shows logger and tents without locked plots");
   foreach(string id in new[]{"mine","weaver","hunters","blacksmith","clothier","tailor","leatherworker","library","housing"}){var cost=city.UpgradeCost(id,1);StarterCheck(cost.wood==10&&cost.stone==5,"Initial construction uses ten wood and five stone: "+id);}
   StarterCheck(city.UpgradeCost("quarry",1).wood==10&&city.UpgradeCost("quarry",1).stone==0&&city.UpgradeCost("timber",2).wood==4,"Bootstrap retains cheap lumbermill and wood-only quarry");
   StarterCheck(city.UpgradeCost("mine",2).wood==16&&city.UpgradeCost("mine",2).stone==8&&city.UpgradeCost("enchanter",1).wood==40&&city.UpgradeCost("library",1).materials.Length==3,"Later upgrades, advanced workshop and processed materials are retained");
   yield return CardScreenshot(Path.Combine(folder,"01-fresh-camp-visible-buildings.png"));
   city.Tick(4/city.WoodRate*60,1);StarterCheck(city.UpgradeBuilding("timber",out _)&&city.timber==2&&city.BuildingVisible("quarry"),"Natural wood production unlocks quarry without starting stone");
   city.Tick(10/city.WoodRate*60,1);StarterCheck(city.UpgradeBuilding("quarry",out _)&&city.quarry==1&&city.StoneRate>0,"Ten wood buys first quarry and starts stone production");
   StarterCheck(city.BuildingVisible("mine")&&city.BuildingVisible("hunters")&&city.BuildingVisible("weaver")&&!city.BuildingVisible("blacksmith")&&!city.BuildingVisible("library"),"Quarry exposes base producers while downstream prerequisites stay hidden");
   double wait=Math.Max(Math.Max(0,10-city.wood)/city.WoodRate,Math.Max(0,5-city.stone)/city.StoneRate);city.Tick(wait*60,1);double wood=city.wood,stone=city.stone;
   StarterCheck(city.UpgradeBuilding("mine",out _)&&city.mine==1&&Math.Abs(wood-city.wood-10)<.000001&&Math.Abs(stone-city.stone-5)<.000001,"First mine is reachable from zero resources and charges exact new price");
   StarterCheck(city.BuildingVisible("blacksmith")&&!city.BuildingVisible("library"),"Completed mine exposes blacksmith but not Library");city.wood=city.stone=0;StarterCheck(city.BuildingVisible("blacksmith")&&!city.CanUpgrade("blacksmith"),"Affordability does not hide an unlocked construction goal");
   yield return CardScreenshot(Path.Combine(folder,"02-mine-unlocks-blacksmith.png"));
   city.wood=city.stone=1000;foreach(string id in new[]{"hunters","weaver","blacksmith"})StarterCheck(city.UpgradeBuilding(id,out _),"Core building can be constructed before Library: "+id);
   StarterCheck(!city.BuildingVisible("library")&&!city.BaseBuildingsBuilt,"Missing clothier still locks Library");StarterCheck(city.UpgradeBuilding("clothier",out _)&&city.BaseBuildingsBuilt&&city.housingTier==0&&city.BuildingVisible("library"),"All seven production/crafting buildings unlock Library without Basic Housing");
   foreach(string missing in new[]{"timber","quarry","mine","hunters","weaver","blacksmith","clothier"}){var sample=JsonUtility.FromJson<Settlement>(JsonUtility.ToJson(city));switch(missing){case "timber":sample.timber=0;break;case "quarry":sample.quarry=0;break;case "mine":sample.mine=0;break;case "hunters":sample.huntersLodge=0;break;case "weaver":sample.weaver=0;break;case "blacksmith":sample.blacksmith=sample.forge=0;break;case "clothier":sample.clothier=sample.tailor=sample.leatherworker=0;break;}StarterCheck(!sample.BaseBuildingsBuilt&&!sample.BuildingVisible("library")&&sample.ConstructionRequirement("library")!="","Each of the seven foundations is enforced for Library: "+missing);}
   city.AddResource(1,0,4);city.AddResource(2,0,4);city.metalBars[0]+=4;StarterCheck(city.UpgradeBuilding("library",out _)&&city.library==1,"Library builds using existing obtainable material requirements");
   string visible=string.Join(",",VisibleSidebarBuildings());city.activeConstruction="mine";StarterCheck(string.Join(",",VisibleSidebarBuildings())==visible,"A busy construction queue does not make unlocked buildings disappear");city.activeConstruction="";
   SelectCityBuilding("mine");for(int i=0;i<VisibleSidebarBuildings().Length*2;i++){CycleSidebarBuilding(1);StarterCheck(city.BuildingVisible(cityBuilding)&&cityBuilding!="herbalist"&&cityBuilding!="enchanter","Controller cycle skips prerequisite-locked and reserved buildings");}
   cityDetailsOpen=false;OpenCityBuilding("herbalist");StarterCheck(!cityDetailsOpen,"Hidden reserved building cannot be opened through sidebar route");
   SelectCityBuilding("mine");OpenCityBuilding("mine");CloseProducer();StarterCheck(selection==SidebarButtonIndex("mine"),"Return focus uses actual visible row index");
   citySidebarScroll=Vector2.zero;SelectCityBuilding("library");yield return CardScreenshot(Path.Combine(folder,"03-core-buildings-unlock-library.png"));
   var legacy=Settlement.NewCamp();legacy.library=1;StarterCheck(legacy.BuildingVisible("library"),"Existing built libraries stay visible in older saves");
   File.WriteAllText(Path.Combine(folder,"validation.json"),"{\"passed\":true,\"checks\":"+starterChecks+",\"regularFirstWood\":10,\"regularFirstStone\":5,\"quarryWood\":10,\"quarryStone\":0}");Debug.Log("VEILBOUND_STARTER_BUILDING_PASS checks="+starterChecks);Application.Quit(0);
  }
 }
}
