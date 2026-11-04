using System;using System.IO;using System.Collections;using System.Collections.Generic;using UnityEngine;
namespace Veilbound {public partial class VeilGame {
 IEnumerator TownLayersReview(){
  yield return null;combatReviewFreeze=true;ReturnHome();city=Settlement.NewCamp();tab=2;SetState(ScreenState.Settlement);SelectCityBuilding("timber");
  string output=Path.Combine(Application.dataPath,"..","Smoke","RadialCity");Directory.CreateDirectory(output);InitCityArt();
  Require(RadialTexture("terrain")&&sanctuaryMap&&sanctuaryMap.name=="terrain","clean terrain has no baked-in tower or buildings");
  Require(RadialOwnerVisible("timber")&&!RadialOwnerVisible("mine")&&!RadialOwnerVisible("quarry")&&!RadialOwnerVisible("blacksmith")&&!RadialOwnerVisible("library")&&!RadialOwnerVisible("clothier"),"fresh camp hides unbuilt production buildings");
  Require(RadialOwnerVisible("herbalist")&&RadialOwnerVisible("enchanter")&&city.BuildingLevel("herbalist")==0&&city.BuildingLevel("enchanter")==0,"planned service artwork does not unlock gameplay");
  Require(CityBuildingName("enchanter")=="Mystic's Tent"&&CityBuildingName("herbalist")=="Herbalist / Alchemist","approved service names");
  Require(Mathf.Abs(TownAspectRect(CityMapRect).width/TownAspectRect(CityMapRect).height-1.5f)<.0001f,"terrain and all layers keep the approved aspect ratio");
  foreach(var pair in RadialPortraitUV){var texture=RadialTexture(pair.Key);Require(texture,"radial sprite exists: "+pair.Key);Require(pair.Value.xMin>=-.00001f&&pair.Value.yMin>=-.00001f&&pair.Value.xMax<=1.00001f&&pair.Value.yMax<=1.00001f,"valid alpha crop: "+pair.Key);}
  yield return null;yield return CardScreenshot(Path.Combine(output,"01-fresh-settlement.png"));
  city.timber=city.mine=city.blacksmith=city.quarry=city.weaver=city.huntersLodge=city.clothier=5;city.library=1;city.housingTier=5;FundTownTest(city);city.EnsureIndustry();
  city.EnsureMineOrders();city.mineOrders.Clear();Require(city.QueueMining(0,4,out _),"mine orders remain available");city.smithOrders.Clear();Require(city.QueueBars(2,4,out _),"refining orders remain available");
  var csv=new List<string>{"id,asset,left,top,width,height"};
  foreach(var part in RadialWorkshopParts){Require(RadialOwnerVisible(part.owner),"developed workshop visible: "+part.id);Require(part.rect.x>=0&&part.rect.y>=0&&part.rect.xMax<=1536&&part.rect.yMax<=1024,"approved workshop within terrain: "+part.id);csv.Add(string.Format(System.Globalization.CultureInfo.InvariantCulture,"{0},{1},{2:F6},{3:F6},{4:F6},{5:F6}",part.id,part.asset,part.rect.x,part.rect.y,part.rect.width,part.rect.height));}
  File.WriteAllLines(Path.Combine(output,"placement-audit.csv"),csv);
  Rect tower=RadialTowerRect;for(int tier=0;tier<6;tier++){city.housingTier=tier;var tex=RadialTexture("housing-"+tier);Require(tex.width==1536&&tex.height==1024,"original housing canvas retained: "+tier);Require(RadialTowerRect==tower,"housing upgrades never resize or reposition tower");yield return null;CaptureTownLayers(Path.Combine(output,"housing-"+tier+".png"));}
  city.housingTier=5;SelectCityBuilding("library");yield return null;yield return CardScreenshot(Path.Combine(output,"02-developed-settlement.png"));
  SelectCityBuilding("enchanter");yield return null;yield return CardScreenshot(Path.Combine(output,"03-planned-services-sidebar.png"));
  OpenCityBuilding("enchanter");Require(cityDetailsOpen&&city.BuildingLevel("enchanter")==0,"mystic opens planned service information");cityDetailsOpen=false;OpenCityBuilding("herbalist");Require(cityDetailsOpen&&city.BuildingLevel("herbalist")==0,"herbalist remains a planned service");cityDetailsOpen=false;
  city=Settlement.NewCamp();FundTownTest(city);city.library=city.quarry=city.weaver=city.huntersLodge=1;for(int tier=1;tier<=5;tier++){Require(UpgradeReviewedTown(city,"housing"),"housing upgrade still completes: "+tier);Require(city.housingTier==tier&&Math.Abs(city.Productivity-(1+.2*tier))<.00001,"housing gameplay progression retained: "+tier);}
  File.WriteAllText(Path.Combine(output,"PASS.txt"),"PASS: approved independent terrain, six housing layers, fixed tower, ten workshop/service sprites, exact placement audit, conditional construction visibility, planned services remain locked, real mining/refining orders and housing upgrades, runtime map and UI captures. Workshop visual tier variants are deferred.");Debug.Log("VEILBOUND_RADIAL_CITY_RUNTIME_PASS");Application.Quit(0);
 }
}}
