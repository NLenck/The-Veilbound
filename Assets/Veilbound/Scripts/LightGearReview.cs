using System;using System.IO;using System.Collections;using UnityEngine;
namespace Veilbound {public partial class VeilGame {
 int lightChecks;
 void LightCheck(bool ok,string message){Require(ok,message);lightChecks++;}
 static Settlement LightSample(){var c=QueueSample();c.clothier=c.huntersLodge=c.weaver=10;c.essence=100000;return c;}
 void LightSuit(Settlement c,int material,int tier){c.EnsureGearRolls();for(int slot=0;slot<(material==1?9:8);slot++){int item=LightGear.Item(material,tier);c.ownedGear[slot*3+material-1]=tier;var g=c.RecordCraft(slot,item);c.EquipRoll(g.id);}}
 IEnumerator LightGearReview(){
  int runtimeExceptions=0;Application.LogCallback errors=(message,stack,type)=>{if(type==LogType.Exception)runtimeExceptions++;};Application.logMessageReceived+=errors;
  yield return null;combatReviewFreeze=true;city=FreshCharacterTown();city.InitializeWoodenStarter();RebuildHero();
  foreach(int material in new[]{1,2})for(int tier=1;tier<=10;tier++){
   int item=LightGear.Item(material,tier);LightCheck(Gear.Material(item)==material&&Gear.Tier(item)==tier&&!Metals.IsMetal(item),"All twenty light gear tiers have distinct valid IDs without colliding with metals");
   var c=LightSample();var recipe=c.LightGearRecipe(2,material,tier);foreach(var part in recipe)c.AddResource(part.family,part.index,part.amount);
   LightCheck(c.CraftLightGear(2,material,tier,out _)&&c.Item(2)==item&&c.Owned(2,material)==tier,"Each tier crafts from its exact finished-material recipe");
   foreach(var part in recipe)LightCheck(c.ResourceAmount(part.family,part.index)<1e-7&&part.index==(part.family==3?GearCraftCosts.FittingMetal(tier):(tier-1)/2),"Each recipe consumes its own material grade exactly");
   c.EquippedRoll(2).enchantments=new[]{new GearAffix{stat="area",value=1}};var save=JsonUtility.FromJson<Settlement>(JsonUtility.ToJson(c));LightCheck(save.EquippedRoll(2).item==item&&save.EquippedRoll(2).enchantments[0].value==1,"Every gear tier and enchantment survives save/load");
   c.clothier=tier-1;c.tailor=c.leatherworker=0;LightCheck(!c.CraftLightGear(2,material,tier,out _),"Building level gates every tier");c.clothier=tier;c.researchRanks[LibraryResearch.Index("clothier")]=tier-1;LightCheck(!c.CraftLightGear(2,material,tier,out _),"Research independently gates every tier");
   if(material==2)LightCheck(c.LightGearRequirement(8,2,tier)!="","Leather cannot be crafted into the cloth cape slot");
  }
  LightCheck(LightGear.Item(1,1)==11&&LightGear.Item(1,2)==12&&LightGear.Item(2,1)==21&&LightGear.Item(2,2)==22,"Existing light gear IDs retain their meanings");
  foreach(int material in new[]{1,2})foreach(int tier in new[]{1,10}){
   var c=LightSample();float bareMass=c.EquipmentMass;LightSuit(c,material,tier);float p=(tier-1)/9f;
   LightCheck(Mathf.Abs(c.EquipmentMass-bareMass-(material==2?Mathf.Lerp(2,4,p):Mathf.Lerp(.5f,1,p)))<.001f,"Complete light armour has the stated low weight");
   LightCheck(Mathf.Abs(c.GearBonus(material==2?"speed":"area")-Mathf.Lerp(2,5,p))<.001f,"Complete set utility bonus stays within the low balance budget");
   LightCheck(Mathf.Abs(material==2?c.GearBonus("haste")-Mathf.Lerp(2,5,p):(c.GearExperience-1)*100-Mathf.Lerp(5,10,p))<.001f,"Haste and experience use exact whole-set budget");
   LightCheck(c.EquipmentArmour>0&&c.Protection<(material==2?.23f:.09f),"Armour protection remains well below plate");
  }
  city=LightSample();LightSuit(city,2,10);build=new RunBuild();float haste=GlobalHasteRating,rate=AbilityCooldown("cleave"),speed=Speed;
  LightCheck(Mathf.Abs(haste-5)<.001f&&speed>6.4f&&rate<2,"Leather movement and Haste actually apply in combat");
  city=LightSample();LightSuit(city,1,10);build=new RunBuild();LightCheck(Mathf.Abs(AreaMultiplier-1.05f)<.001f&&Mathf.Abs(city.GearExperience-1.1f)<.001f,"Cloth area and XP actually apply in combat");
  var enchant=city.EquippedRoll(2);enchant.enchantments=new[]{new GearAffix{stat="area",value=2}};LightCheck(Mathf.Abs(AreaMultiplier-1.07f)<.001f,"Material bonuses add to real enchantments without replacing them");
  city.EquipRoll(city.RecordCraft(2,LightGear.Item(2,10)).id);LightCheck(city.GearBonus("haste")>0&&city.GearBonus("area")<5&&city.GearExperience<1.1f,"Mixed sets contribute per equipped piece");
  foreach(int material in new[]{1,2}){
   var c=LightSample();LightCheck(c.QueueSettlementJob(5,"",material,3,2,10,out _),"Late-tier recipe can be queued explicitly");c.mine=c.blacksmith=10;c.Tick(60*2000,1);LightCheck(c.settlementQueue.Count==0&&c.craftedGear.FindAll(g=>g.slot==3&&g.item==LightGear.Item(material,10)).Count==2,"Queue gathers/processes the correct high-tier materials and crafts an exact-tier batch");
   LightCheck(c.ResourceTotal(4)+c.ResourceTotal(5)<1e-6,"Automatic late-tier production avoids unused finished-material surplus");
  }
  city=FreshCharacterTown();RebuildHero();PressureStart(1,20);float[] seconds={0,59.99f,60,119.99f,120,300,600,900};Foe old=null;
  foreach(float time in seconds){elapsed=time;var foe=SpawnFoe(0,0);LightCheck(foe!=null&&Mathf.Abs(foe.spawnDamageFactor-(1+.1f*Mathf.Floor(time/60)))<.0001f,"Actual spawns snapshot additive ten-percent minute bonuses at exact boundaries");if(time==0)old=foe;}
  LightCheck(Mathf.Abs(ExpeditionDamageFactor(old)-RunModifierFactor(1))<.0001f,"Old enemies never retroactively gain the spawn-time damage bonus");
  elapsed=300;var attacker=SpawnFoe(0,0);attacker.t.position=Vector3.forward*3;elapsed=600;health=MaxHealth;invulnerability=0;float hp=health;Hurt(2,attacker);LightCheck(Mathf.Abs(hp-health-2*RunModifierFactor(1)*1.5f)<.001f,"Direct damage uses spawn-time bonus after depth and modifiers");
  health=MaxHealth;invulnerability=0;hp=health;Telegraph(position,2,.1f,2,owner:attacker);UpdateHazards(.11f);LightCheck(Mathf.Abs(hp-health-2*RunModifierFactor(1)*1.5f)<.001f,"Owned delayed attacks retain their spawn-time damage factor");
  elapsed=300;var boss=SpawnFoe(1);elapsed=420;LightCheck(Mathf.Abs(ExpeditionDamageFactor(boss)-RunModifierFactor(3)*1.5f*1.15f)<.001f,"Aberration birth-time difficulty stacks with its independent aging bonus");
  string folder=Path.Combine(Application.dataPath,"..","Smoke","LightGear");Directory.CreateDirectory(folder);
  ReturnHome();city=LightSample();RebuildHero();tab=2;sanctuaryWorkshopOpen=true;industryPage=1;smithArmourPage=true;gearMaterial=2;gearSlot=2;lightGearTier=10;SetState(ScreenState.Settlement);yield return CardScreenshot(Path.Combine(folder,"01-leather-crafting.png"));
  foreach(bool female in new[]{false,true}){city.female=female;foreach(int material in new[]{1,2}){Array.Clear(city.equippedGear,0,9);LightSuit(city,material,10);RebuildHero();LightCheck(hero.root.GetComponent<BronzeHeroVisual>()!=null,"Both character models render high-tier cloth and leather");}}
  gearMaterial=1;Array.Clear(city.equippedGear,0,9);LightSuit(city,1,10);RebuildHero();yield return CardScreenshot(Path.Combine(folder,"02-cloth-crafting.png"));OpenGearBag(2,city.EquippedRoll(2).id);yield return CardScreenshot(Path.Combine(folder,"03-material-bonuses.png"));
  LightCheck(runtimeExceptions==0,"Crafting and comparison screens render without runtime exceptions at maximum building levels");Application.logMessageReceived-=errors;
  File.WriteAllText(Path.Combine(folder,"validation.json"),"{\"passed\":true,\"checks\":"+lightChecks+",\"lightGearTiers\":10,\"spawnDamagePerMinutePercent\":10}");Debug.Log("VEILBOUND_LIGHT_GEAR_PASS checks="+lightChecks);Application.Quit(0);
 }
}}
