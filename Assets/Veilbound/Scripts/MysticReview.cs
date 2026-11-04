using System;
using System.Collections;
using System.IO;
using UnityEngine;
namespace Veilbound {public partial class VeilGame {
 void MysticCheck(bool ok,string text){Require(ok,"Mystic: "+text);Debug.Log("MYSTIC_CHECK_PASS: "+text);}
 Settlement MysticSample(){var c=QueueSample();c.wood=c.stone=100000;for(int i=0;i<c.metalBars.Length;i++)c.metalBars[i]=1000;c.essence=10000;c.researchRanks[LibraryResearch.Index("enchanting")]=0;return c;}
 void CheckMystic(){
  CheckGearRolls();var c=MysticSample();var item=c.RecordCraft(2,100);var other=c.RecordCraft(2,100);int id=item.id;
  MysticCheck(!c.EnchantGear(id,0,0,"crit",0,out _),"unbuilt Mystic cannot enchant");
  MysticCheck(!c.TryTownUpgrade("enchanter",out _),"Mystic construction requires research");
  for(int level=1;level<=4;level++){
   MysticCheck(c.StartResearch("enchanting",out _),"start enchanting research "+level);c.Tick(60*60,1);MysticCheck(c.ResearchRank("enchanting")==level,"finish research "+level);
   MysticCheck(c.TryTownUpgrade("enchanter",out _),"start Mystic construction "+level);c.Tick(60*60,1);MysticCheck(c.mysticLevel==level,"finish Mystic level "+level);
   if(level==1){double before=c.essence;MysticCheck(c.EnchantGear(id,0,0,"crit",0,out _)&&c.essence==before-MysticRules.Cost(item,0,0),"first enchantment pays exact cost");MysticCheck(!c.EnchantGear(id,0,0,"crit",1,out _)&&!c.EnchantGear(id,1,0,"crit",0,out _)&&!c.EnchantGear(id,2,0,"crit",0,out _),"level 1 locks second slot and targeting");}
   if(level==2){MysticCheck(c.EnchantGear(id,0,0,"crit",1,out _)&&item.enchantments.Length==2&&item.enchantments[0].stat!=item.enchantments[1].stat,"level 2 unlocks distinct second stat");double balance=c.essence;MysticCheck(!c.EnchantGear(id,0,0,"crit",2,out _)&&c.essence==balance,"third slot rejected atomically");}
   if(level==3){MysticCheck(c.EnchantGear(id,1,0,"crit",0,out _)&&Array.IndexOf(MysticRules.CategoryStats[0],item.enchantments[0].stat)>=0,"level 3 targets offense");MysticCheck(!c.EnchantGear(id,2,0,"crit",0,out _),"specific stat stays locked at level 3");}
  }
  // Set exact stats while respecting the occupied second slot.
  string second=item.enchantments[1].stat;string chosen=Array.Find(GearRollRules.Stats,s=>s!=second&&s!=item.enchantments[0].stat);
  MysticCheck(c.EnchantGear(id,2,0,chosen,0,out _)&&item.enchantments[0].stat==chosen&&item.enchantments[1].stat==second,"level 4 targets exact stat and preserves other slot");
  double paid=c.essence;string snapshot=JsonUtility.ToJson(item);MysticCheck(!c.EnchantGear(id,2,0,second,0,out _)&&!c.EnchantGear(id,2,0,chosen,0,out _)&&!c.EnchantGear(999999,0,0,"crit",0,out _)&&c.essence==paid&&JsonUtility.ToJson(item)==snapshot,"duplicates, no-op replacements and invalid ownership charge nothing");
  c.essence=0;MysticCheck(!c.EnchantGear(other.id,0,0,"crit",0,out _)&&c.EnchantmentCount(other)==0,"insufficient essence leaves item unchanged");c.essence=paid;c.EquipRoll(id);
  var loaded=JsonUtility.FromJson<Settlement>(JsonUtility.ToJson(c));loaded.EnsureGearRolls();MysticCheck(loaded.mysticLevel==4&&loaded.ResearchRank("enchanting")==4&&loaded.EquippedRoll(2).id==id&&JsonUtility.ToJson(loaded.FindRoll(id))==snapshot&&loaded.EnchantmentCount(loaded.FindRoll(other.id))==0,"save retains upgrades, exact enchantments, equipment and separate copies");
  var inventory=JsonUtility.FromJson<CharacterInventory>(JsonUtility.ToJson(CharacterInventory.Capture(c)));var target=MysticSample();inventory.Apply(target);target.EnsureGearRolls();MysticCheck(JsonUtility.ToJson(target.FindRoll(id))==snapshot,"character inventory preserves enchantments");
  var previous=city;var previousBuild=build;city=c;build=new RunBuild();c.equippedRollIds[2]=0;
  var test=c.RecordCraft(0,100);c.EquipRoll(test.id);float hp=MaxHealth,damage=Damage;test.enchantments=new[]{new GearAffix{stat="vitality",value=10},new GearAffix{stat="power",value=2}};MysticCheck(Math.Abs(MaxHealth-hp-10)<.001&&Math.Abs(Damage/damage-1.02f)<.0001,"enchantments reach combat health and damage");test.affixes=new[]{new GearAffix{stat="vitality",value=999}};MysticCheck(Math.Abs(MaxHealth-hp-10)<.001,"retired craft affixes do not stack with enchantments");c.equippedRollIds[0]=0;MysticCheck(Math.Abs(MaxHealth-hp)<.001&&Math.Abs(Damage-damage)<.001,"unequipping removes enchantment effects");city=previous;build=previousBuild;
  c=MysticSample();c.researchRanks[LibraryResearch.Index("enchanting")]=1;MysticCheck(c.QueueSettlementJob(0,"enchanter",0,0,1,0,out _),"Mystic joins construction priority queue");c.Tick(60*60,1);MysticCheck(c.mysticLevel==1&&c.settlementQueue.Count==0,"queued Mystic build completes");
  Debug.Log("VEILBOUND_MYSTIC_LOGIC_PASS");
 }
 IEnumerator MysticReview(){
  yield return null;combatReviewFreeze=true;CheckMystic();ReturnHome();city=MysticSample();city.mysticLevel=1;city.researchRanks[LibraryResearch.Index("enchanting")]=1;var a=city.RecordCraft(2,102);city.EquipRoll(a.id);city.RecordCraft(9,2);city.RecordCraft(3,102);tab=2;SetState(ScreenState.Settlement);OpenMystic(a.id);
  string folder=Path.Combine(Application.dataPath,"..","Smoke","Mystic");Directory.CreateDirectory(folder);yield return CardScreenshot(Path.Combine(folder,"01-level-one.png"));ApplyMysticEnchantment();yield return CardScreenshot(Path.Combine(folder,"02-first-enchantment.png"));
  city.mysticLevel=4;city.researchRanks[LibraryResearch.Index("enchanting")]=4;mysticMode=2;mysticSlot=1;mysticStat=Array.IndexOf(GearRollRules.Stats,a.enchantments[0].stat=="vitality"?"power":"vitality");ApplyMysticEnchantment();yield return CardScreenshot(Path.Combine(folder,"03-targeted-enchantment.png"));
  CloseMystic();tab=1;OpenGearBag(2,a.id);OpenMystic(a.id,true);CloseMystic();MysticCheck(gearBagOpen&&tab==1&&gearBagSelected==a.id,"Mystic returns to original loadout selection");OpenGearBag(2,a.id);yield return CardScreenshot(Path.Combine(folder,"04-inventory.png"));CloseGearBag();OpenMysticResearch();yield return CardScreenshot(Path.Combine(folder,"05-research.png"));
  File.WriteAllText(Path.Combine(folder,"PASS.txt"),"PASS: Mystic research/construction levels 1-4; slot and targeting gates; exact Essence costs; atomic failures; distinct stats; targeted replacement; save/character identity; combat stats; retired crafting bonuses removed; queued construction; UI previews.");Debug.Log("VEILBOUND_MYSTIC_RUNTIME_PASS");Application.Quit(0);
 }
}}
