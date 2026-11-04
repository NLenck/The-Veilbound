using System;
using System.IO;
using System.Collections;
using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  void CheckResourceTopBar() {
   ReturnHome();int[] counts={2,8,11,10,10,1};for(int i=0;i<6;i++){var rows=ResourceRows(i);Require(rows.Count==counts[i],"Every resource category includes all its materials");foreach(var row in rows)Require(MaterialIcon(row.icon),"Dropdown material artwork loads: "+row.name);Require(MaterialIcon(ResourceCategoryIcons[i]),"Each resource group has an icon");}
   city.EnsureMineOrders();city.mineOrders.Clear();Require(city.QueueMining(0,3,out _)&&city.QueueMining(1,3,out _),"resource header uses real mining queue");var ore=ResourceRows(1);Require(ore[0].rate==city.MiningSpeed/Settlement.MineUnitWork(0)&&ore[1].rate==0&&ore[3].rate==0,"only the active mining order has a unit interval");
   Require(ResourceRows(0)[0].rate==city.WoodRate&&ResourceRows(0)[1].rate==city.StoneRate,"Building materials use actual producer rates");city.EnsureSmithOrders();var bars=ResourceRows(2);for(int i=0;i<bars.Count;i++)Require(bars[i].rate==ActiveBarRate(i),"bar rate uses active order");Require(ResourceRows(5)[0].rate==0,"Essence has no invented production");
   double tin=city.ResourceAmount(0,0);city.Tick(Settlement.MineUnitWork(0)/city.MiningSpeed*60,1);Require(Math.Abs(ResourceRows(1)[0].stock-tin-1)<.000001,"Open resource rows reflect whole-unit deliveries");
   resourceCategory=-1;Require(HandleResourcePointer(ResourceCategoryRect(1).center)&&resourceCategory==1,"Clicking an ore category opens its dropdown");int chosen=resourceCategory;Require(HandleResourcePointer(ResourceDropdownRect(1).center)&&resourceCategory==chosen,"Dropdown rows consume clicks instead of activating controls underneath");Require(HandleResourcePointer(ResourceCategoryRect(1).center)&&resourceCategory==-1,"Clicking the selected category closes it");ToggleResourceCategory(4);Require(!HandleResourcePointer(new Vector2(10,700))&&resourceCategory==-1,"Clicking outside dismisses the resource menu");
   for(int g=0;g<6;g++){Rect r=ResourceDropdownRect(g);Require(r.x>=30&&r.xMax<=1570&&r.yMax<=900,"Every dropdown, including all eleven bars, fits inside the window");}
   Require(ResourceCategoryRect(5).x>ResourceCategoryRect(4).xMax+50,"Essence is separated on the far right");
   Require(Metals.All[9].color.r>Metals.All[9].color.b*3,"VeilCrystal equipment palette is red");StartRun();Require(resourceCategory==-1,"The settlement resource dropdown never carries into an expedition");ReturnHome();Debug.Log("VEILBOUND_RESOURCE_TOPBAR_LOGIC_PASS");
  }
  IEnumerator ResourceTopBarReview(string folder) {
   Directory.CreateDirectory(folder);ReturnHome();var previous=city;city=new Settlement{wood=165,stone=1801,essence=455,mine=7,weaver=5,huntersLodge=5,blacksmith=9,bronzeHeroVersion=1};city.EnsureGearRolls();city.EnsureIndustry();city.EnsureVirtue();
   for(int f=0;f<3;f++)for(int i=0;i<Settlement.ResourceNames[f].Length;i++)city.AddResource(f,i,35+i*19.25);for(int i=0;i<10;i++)city.metalBars[i]=12+i*3.5;for(int s=0;s<8;s++){city.equippedGear[s]=109;city.ownedPlate[s*10+9]=true;}city.equippedMetalWeapon=9;city.ownedMetalWeapons[9]=true;RebuildHero();CheckResourceTopBar();combatReviewFreeze=true;tab=0;resourceCategory=-1;yield return CardScreenshot(Path.Combine(folder,"01-settlement-resource-bar.png"));
   resourceCategory=0;yield return CardScreenshot(Path.Combine(folder,"02-building-materials.png"));resourceCategory=1;yield return CardScreenshot(Path.Combine(folder,"03-ore-rates.png"));resourceCategory=2;yield return CardScreenshot(Path.Combine(folder,"04-bars.png"));resourceCategory=3;yield return CardScreenshot(Path.Combine(folder,"05-textiles-red-veil-fibre.png"));resourceCategory=4;yield return CardScreenshot(Path.Combine(folder,"06-hides-red-veil-hide.png"));resourceCategory=5;yield return CardScreenshot(Path.Combine(folder,"07-essence.png"));
   resourceCategory=-1;tab=1;gearSlot=2;gearMaterial=3;selectedMetal=9;RebuildHero();yield return CardScreenshot(Path.Combine(folder,"08-equipment-red-veilcrystal.png"));tab=2;industryPage=0;resourceCategory=1;yield return CardScreenshot(Path.Combine(folder,"09-industry-shared-bar.png"));resourceCategory=-1;OpenGearBag(2);yield return CardScreenshot(Path.Combine(folder,"10-gear-bag-shared-bar.png"));CloseGearBag();resourceCategory=-1;city=previous;RebuildHero();combatReviewFreeze=false;ReturnHome();
   File.WriteAllText(Path.Combine(folder,"PASS.txt"),"PASS: six ordered groups; all material icons; allocation-derived rates; manual bar/essence zero passive rate; live stock updates; dropdown open/toggle/outside dismissal and click-through blocking; ten-bar popup fit; essence separation; shared settlement/equipment/industry/gear-bag bar; red VeilCrystal gear; expedition isolation.\n");Debug.Log("VEILBOUND_RESOURCE_TOPBAR_PASS");
  }
 }
}

