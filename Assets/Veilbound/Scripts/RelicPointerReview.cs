using System;using System.IO;using System.Collections;using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  // Exact-damage reviews still exercise guaranteed crits at 100%, without random baseline crits.
  sealed class NonCriticalReviewRandom:System.Random {public override double NextDouble()=>.99999999;}
  IEnumerator RelicPointerReview(){
   yield return null;combatReviewFreeze=true;city=FreshCharacterTown();city.InitializeWoodenStarter();RebuildHero();PressureStart(1,0);int checks=0;
   Require(Mathf.Abs(CritChance-.05f)<.0001f&&Mathf.Abs(build.CritBasePercent-5)<.001f,"Starter has five percent base crit");checks++;
   var target=ReadoutTarget(Vector3.forward*3);var savedRandom=criticalRandom;criticalRandom=new System.Random(7401);var expectedRandom=new System.Random(7401);int expectedCrits=0;
   for(int i=0;i<200;i++){if(expectedRandom.NextDouble()<.05)expectedCrits++;Hit(target,10,"cleave");}
   Require(abilityDamageRecords["cleave"].crits==expectedCrits&&expectedCrits>0&&Mathf.Abs(abilityDamageRecords["cleave"].total-(2000+expectedCrits*5))<.001f,"Actual unupgraded hits can crit at base five percent and deal correct damage");criticalRandom=savedRandom;checks++;
   var weapon=city.EquippedRoll(9);var oldEnchantments=weapon.enchantments;weapon.enchantments=new[]{new GearAffix{stat="crit",value=7}};ReturnHome();StartRun();Require(Mathf.Abs(CritChance-.12f)<.001f&&Mathf.Abs(build.CritBasePercent-12)<.001f,"Base and gear crit add together");checks++;
   for(int i=0;i<17;i++)build.Pick(TestCard("crit",UpgradeRarity.Legendary));Require(Mathf.Abs(CritChance-.97f)<.001f&&build.Describe(TestCard("crit",UpgradeRarity.Legendary)).StartsWith("+3%"),"Crit card preview accounts for base, gear and passive cap");build.Pick(TestCard("crit",UpgradeRarity.Legendary));Require(CritChance==1&&!build.Eligible(TestCard("crit",UpgradeRarity.Common)),"Combined crit caps at 100 and removes capped offers");checks+=2;weapon.enchantments=oldEnchantments;
   PressureStart(1,0);Require(SpawnRelic()&&SpawnRelic(),"Two real relics spawn");
   foreach(float aspect in new[]{16f/9,4f/3,21f/9})foreach(float yaw in new[]{0f,90f})foreach(float size in new[]{8f,12f,18f})foreach(Vector3 player in new[]{Vector3.zero,new Vector3(4,0,-3)}){
    position=player;cameraRig.aspect=aspect;cameraRig.orthographicSize=size;cameraRig.transform.SetPositionAndRotation(position+Quaternion.Euler(0,yaw,0)*new Vector3(0,20,-16),Quaternion.Euler(51,yaw,0));
    relics[0].point=position+Vector3.right*20;relics[1].point=position+Vector3.back*20;BuildRelicPointers();Require(relicPointers.Count==2,"All relics have simultaneous pointers");
    Vector2 centre=WorldToHudPoint(position+Vector3.up*.65f);
    for(int i=0;i<2;i++){Vector3 world= cameraRig.transform.InverseTransformDirection(relics[i].point-position);Vector2 direction=new Vector2(world.x,-world.y).normalized;Require(Vector2.Dot(direction,relicPointers[i].direction)>.999f&&Mathf.Abs(Vector2.Distance(centre,relicPointers[i].point)-RelicPointerRadius)<.01f,"Pointers use player-centred circle and correct projected bearing across camera/aspect/player changes");checks++;}
   }
   position=Vector3.zero;cameraRig.aspect=16f/9;TownCamera(position,12);cameraRig.transform.SetPositionAndRotation(new Vector3(0,20,-16),Quaternion.Euler(51,0,0));relics[0].point=Vector3.forward*14;relics[1].point=Vector3.forward*28;BuildRelicPointers();Require(relicPointers.Count==2&&Mathf.Abs(relicPointers[1].radius-relicPointers[0].radius)<.001f&&Vector2.Distance(relicPointers[0].point,relicPointers[1].point)>=26&&Vector2.Dot(relicPointers[0].direction,relicPointers[1].direction)>.999f,"Co-directional relic pointers separate along the same circle without changing bearing");checks++;
   string folder=Path.Combine(Application.dataPath,"..","Smoke","RelicPointers");Directory.CreateDirectory(folder);
   relics[0].point=Vector3.right*14;relics[1].point=Vector3.back*18;foreach(var r in relics)r.root.position=r.point;hero.root.position=position;damageNumbers.Clear();UpdateFlashes(10);yield return CardScreenshot(Path.Combine(folder,"01-player-relic-pointers.png"));
   for(int i=0;i<2;i++){var model=Instantiate(relics[0].root.gameObject);relics.Add(new Relic{root=model.transform,point=Vector3.zero});}
   relics[0].point=new Vector3(-4,0,22);relics[1].point=new Vector3(0,0,25);relics[2].point=new Vector3(4,0,22);relics[3].point=Vector3.left*20;
   foreach(var relic in relics)relic.root.position=relic.point;BuildRelicPointers();Require(relicPointers.Count==4,"Every relic has its own radial pointer");checks++;
   foreach(var pointer in relicPointers)Require(Mathf.Abs(Vector2.Distance(WorldToHudPoint(position+Vector3.up*.65f),pointer.point)-125)<.01f,"All four pointers remain on the same circular radial");
   yield return CardScreenshot(Path.Combine(folder,"03-reference-radial-layout.png"));
   for(int i=relics.Count-1;i>=2;i--){Destroy(relics[i].root.gameObject);relics.RemoveAt(i);}
   relics[1].point=Vector3.right*28;relics[1].root.position=relics[1].point;yield return CardScreenshot(Path.Combine(folder,"02-same-bearing-pointers.png"));
   Require(CollectRelic(0),"Real relic can still be collected");BuildRelicPointers();Require(relicPointers.Count==1,"Collected relic pointer disappears");Choose(0);Destroy(relics[0].root.gameObject);relics.Clear();BuildRelicPointers();Require(relicPointers.Count==0,"No pointers remain without relics");checks+=2;
   File.WriteAllText(Path.Combine(folder,"validation.json"),"{\"passed\":true,\"checks\":"+checks+",\"baseCritPercent\":5,\"actualHitsChecked\":200,\"actualCriticalHits\":"+expectedCrits+"}");Debug.Log("VEILBOUND_RELIC_POINTER_PASS checks="+checks);Application.Quit(0);
  }
 }
}
