using System;
using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace Veilbound {
 public partial class VeilGame {
  IEnumerator ExpeditionModifierReview(){
   yield return null;combatReviewFreeze=true;city=Settlement.NewCamp();SelectHighestExpeditionTier();ReturnHome();tab=0;
   string folder=Path.Combine(Application.dataPath,"..","Smoke","ExpeditionModifiers");Directory.CreateDirectory(folder);
   Require(city.ExpeditionColumns==1&&VeilDepth==0,"Fresh characters begin at depth zero with ten first-column choices");
   if(Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-modifierPreview"))yield return new WaitForSecondsRealtime(30);
   shortRun=true;Require(Duration==900,"Expeditions always use the standard fifteen-minute duration");shortRun=false;
   yield return new WaitForEndOfFrame();var pad=InputSystem.AddDevice<Gamepad>();selection=5;
   InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.South));InputSystem.Update();MenuInput();
   Require(city.expeditionRanks[0]==1&&VeilDepth==1,"Controller confirmation selects the first modifier cell through the actual menu binding");
   InputSystem.RemoveDevice(pad);SetModifierRank(0,0);
   SetModifierRank(0,10);Require(ModifierRank(0)==1,"Locked ranks cannot be selected");SetModifierRank(0,0);
   for(int i=0;i<10;i++)SetModifierRank(i,1);Require(VeilDepth==10&&Math.Abs(Multiplier-CityTimeMultiplier(10))<.00001,"All ten first-column modifiers can be selected immediately");
   yield return CardScreenshot(Path.Combine(folder,"01-first-column.png"));
   StartRun();city.expeditionRanks[0]=0;Require(ModifierRank(0)==1,"Run modifiers are a fixed snapshot");EndRun(false);ReturnHome();Require(city.ExpeditionColumns==1,"Defeat never unlocks a column");
   for(int i=0;i<10;i++)SetModifierRank(i,i<5?1:0);StartRun();EndRun(true);ReturnHome();Require(city.ExpeditionColumns==2&&city.expeditionBestDepth==5,"Depth five victory permanently unlocks column two");
   for(int i=0;i<10;i++)SetModifierRank(i,i<5?2:1);Require(VeilDepth==15&&Math.Abs(Multiplier-CityTimeMultiplier(15))<.00001,"Depth fifteen keeps the existing tier fifteen time dilation");
   modifierFocus=9;
   if(Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-modifierPreview"))yield return new WaitForSecondsRealtime(30);
   yield return CardScreenshot(Path.Combine(folder,"02-depth-fifteen.png"));
   StartRun();EndRun(true);ReturnHome();Require(city.ExpeditionColumns==3,"Depth fifteen victory unlocks column three");
   var saved=JsonUtility.FromJson<Settlement>(JsonUtility.ToJson(city));saved.EnsureExpeditionModifiers();Require(saved.ExpeditionColumns==3&&ExpeditionModifiers.Depth(saved.expeditionRanks)==15,"Ranks and permanent unlocks survive serialization");
   for(int depth=25;depth<=85;depth+=10){city.expeditionBestDepth=depth;Require(city.ExpeditionColumns==2+(depth-5)/10,"Every ten-depth milestone unlocks the next column");}
   city.expeditionBestDepth=100;city.EnsureExpeditionModifiers();Require(city.ExpeditionColumns==10,"Columns cap at ten");
   for(int i=0;i<10;i++)SetModifierRank(i,10);Require(VeilDepth==100&&ModifierFactor(0)==2&&Math.Abs(ModifierFactor(2)-2.5)<.0001,"Maximum ranks and independent health factors");
   yield return CardScreenshot(Path.Combine(folder,"03-all-columns.png"));
   for(int i=0;i<10;i++)SetModifierRank(i,0);SetModifierRank(0,10);StartRun();var regular=SpawnFoe(0,0);var boss=SpawnFoe(1);Require(regular!=null&&boss!=null,"Both combat categories spawn");
   Require(Math.Abs(regular.maxHp-EnemyTypes.RegularHealth(1,0,0)*2)<.001&&boss.maxHp==480,"Enemy health selection changes normal health without increasing boss health");
   Require(ExpeditionDamageFactor(regular)==1&&ExpeditionDamageFactor(boss)==1&&ExpeditionCooldownRate(boss)==1,"Unselected damage and cooldown remain at baseline");
   EndRun(false);ReturnHome();for(int i=0;i<10;i++)SetModifierRank(i,0);SetModifierRank(3,3);SetModifierRank(4,10);SetModifierRank(8,4);StartRun();boss=SpawnFoe(1);regular=SpawnFoe(0,0);
   Require(Math.Abs(ExpeditionDamageFactor(boss)-1.3)<.0001&&ExpeditionDamageFactor(regular)==1&&ExpeditionCooldownRate(boss)==2&&Math.Abs(ExpeditionCooldownRate(regular)-1.2)<.0001,"Boss damage, boss cooldown and enemy attack speed operate independently");
   EndRun(false);ReturnHome();for(int i=0;i<10;i++)SetModifierRank(i,0);SetModifierRank(9,2);SelectExpeditionMap(1);StartRun();var heavy=SpawnFoe(0,2);regular=SpawnFoe(0,0);boss=SpawnFoe(1);
   Require(heavy!=null&&regular!=null&&boss!=null&&Math.Abs(heavy.maxHp-EnemyTypes.RegularHealth(1,0,2)*1.3f)<.001&&regular.maxHp==EnemyTypes.RegularHealth(1,0,0)&&boss.maxHp==480,"Elite Health increases Brute health without affecting ordinary enemies or bosses");
   EndRun(false);ReturnHome();SelectExpeditionMap(0);StartRun();heavy=SpawnFoe(0,2);Require(heavy!=null&&heavy.maxHp==EnemyTypes.RegularHealth(1,0,2),"Shrine Guardian remains outside the confirmed elite pool");
   EndRun(false);ReturnHome();CheckExpeditionSetup();
   File.WriteAllText(Path.Combine(folder,"PASS.txt"),"PASS: fresh availability; rank limits; freely selecting all ten first ranks; fixed run snapshot; defeat protection; clear milestones 5/15/25/35/45/55/65/75/85; persistence; unchanged per-tier time dilation; depth 100 cap; independent normal/boss health, damage and cooldown; existing production projections. Isolated smoke profile.");
   Debug.Log("VEILBOUND_EXPEDITION_MODIFIERS_PASS");Application.Quit(0);
  }
 }
}
