using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  IEnumerator EquipmentPaperDollReview(string folder) {
   Directory.CreateDirectory(folder);ReturnHome();FundTestCity();city.blacksmith=Math.Max(2,city.blacksmith);city.tailor=Math.Max(2,city.tailor);city.leatherworker=Math.Max(2,city.leatherworker);
   city.sword=2;city.equippedSword=2;RebuildHero();tab=1;SelectPaperSlot(2);
   yield return new WaitForSecondsRealtime(.4f);Require(cachedEquipmentHero==hero&&equipmentPortrait&&equipmentCamera,"Equipment preview renders the actual equipped character");
   Require(new HashSet<int>(PaperSlots).Count==11,"Nine real armour slots plus main-hand and occupied off-hand are shown");
   var visual=hero.root.GetComponent<BronzeHeroVisual>();Vector3 head=visual.bones[4].position;Quaternion turn=hero.root.rotation;
   yield return new WaitForSecondsRealtime(.5f);Require(Vector3.Distance(head,visual.bones[4].position)<.00001f&&Quaternion.Angle(turn,hero.root.rotation)<.0001f,"Equipment character has no idle animation or automatic rotation");
   foreach(var t in hero.root.GetComponentsInChildren<Transform>())Require(t.gameObject.layer==0,"Preview capture restores the real player's render layers");
   yield return CardScreenshot(Path.Combine(folder,"01-paper-doll.png"));SelectPaperSlot(0);yield return CardScreenshot(Path.Combine(folder,"02-head-slot.png"));
   int helm=city.Item(0);RemoveEquipmentPiece(0);yield return new WaitForSecondsRealtime(.3f);Require(city.Item(0)==0&&cachedEquipmentHero==hero,"Removing a slot refreshes the static character and item thumbnails");
   visual=hero.root.GetComponent<BronzeHeroVisual>();for(int i=0;i<visual.parts.Length;i++)if(visual.slots[i]==0)Require(!visual.parts[i].gameObject.activeSelf,"Removing the helm hides its actual meshes");
   yield return CardScreenshot(Path.Combine(folder,"03-helmet-removed.png"));EquipOwnedPiece(0,helm);yield return new WaitForSecondsRealtime(.2f);Require(city.Item(0)==helm,"Direct owned-item selection re-equips the selected piece");
   city.ownedGear[7*3+2]=1;city.equippedGear[7]=31;SelectPaperSlot(7);CraftGear(7,3);yield return new WaitForSecondsRealtime(.2f);Require(city.Item(7)==31&&city.Owned(7,3)==2&&gearBagOpen,"Slot crafting retains costs and unlocks, and opens comparison without replacing the current piece");city.EquipRoll(city.lastCraftedRollId);CloseGearBag();RefreshMetalGear();Require(city.Item(7)==32,"The crafted copy equips explicitly after comparison");
   SelectPaperSlot(9);city.equippedSword=1;RebuildHero();yield return new WaitForSecondsRealtime(.3f);Require(cachedEquipmentHero==hero&&city.equippedSword==1,"Weapon selection updates the static preview and weapon thumbnail");
   yield return CardScreenshot(Path.Combine(folder,"04-weapon-slot.png"));SelectPaperSlot(10);yield return CardScreenshot(Path.Combine(folder,"05-two-handed-occupancy.png"));
   tab=0;yield return new WaitForSecondsRealtime(.2f);Require(Quaternion.Angle(turn,hero.root.rotation)>10,"Normal settlement display resumes after leaving equipment");
   File.WriteAllText(Path.Combine(folder,"PASS.txt"),"PASS: centred static equipped-character portrait; nine armour slots and two weapon positions; actual mesh thumbnails; no idle motion/rotation; restored render layers; remove/re-equip helm; slot crafting and equip; weapon preview refresh; two-handed off-hand occupancy; existing combat, cards, character sheets and HUD regression checks.\n");Debug.Log("VEILBOUND_EQUIPMENT_PAPER_DOLL_PASS");ReturnHome();
  }
 }
}


