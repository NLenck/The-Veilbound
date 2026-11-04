using System;
using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace Veilbound {
 public partial class VeilGame {
  bool reviewRageTooltip;
  void CheckRageVirtue() {
   var reviewRandom=criticalRandom;criticalRandom=new NonCriticalReviewRandom();try{
   StartRun();Require(city.virtue==Virtue.Rage&&rage==0&&!RageReady&&!RageActive,"Testing character starts with Rage virtue and an empty resource");Require(!ActivateRage(),"Rage cannot activate before full");
   var target=ReadoutTarget(Vector3.forward*2);Hit(target,100,"cleave");Require(Mathf.Abs(rage-2)<.0001f,"Dealing 100 real damage grants two Rage");
   target.hp=3;Hit(target,100,"cleave");Require(Mathf.Abs(rage-2.06f)<.0001f,"Overkill grants Rage only for actual damage applied");
   float before=rage;target=ReadoutTarget(Vector3.right*2);Hit(target,20,"volatile",false);Require(rage==before,"Enemy-on-enemy damage grants no player Rage");
   float hp=health;invulnerability=0;Hurt(20);Require(Mathf.Abs(rage-before-(hp-health)*.1f)<.0001f,"Rage from damage taken uses health actually lost after protection");before=rage;Hurt(20);Require(rage==before,"Invulnerable hits cannot generate Rage");
   GainRage(10000,true);Require(rage==100&&RageReady,"Rage caps at one hundred and becomes ready");
   float normalDamage=Damage,normalRate=AttackRate,normalCooldown=AbilityCooldown("cleave");float targetHp=target.hp,blast=RageBlastDamage;
   Require(ActivateRage()&&RageActive&&rage==100&&rageRemaining==6,"A full resource activates a six second Rage window");Require(Mathf.Abs(targetHp-target.hp-blast)<.001f&&Mathf.Abs(abilityDamageRecords["rage"].total-blast)<.001f,"Fury explosion matches the scaled damage preview and enters the damage tracker");
   Require(Mathf.Abs(Damage/normalDamage-1.25f)<.0001f&&Mathf.Abs(AttackRate-1/(1-HasteRules.Reduction(GlobalHasteRating)))<.0001f&&Mathf.Abs(AbilityCooldown("cleave")-SkillBaseCooldown("cleave")*WeaponCooldownMultiplier*(1-HasteRules.Reduction(GlobalHasteRating)))<.0001f,"Fury boosts damage by 25% and attack cadence by 30%");
   Require(!ActivateRage(),"Active Rage cannot be activated again");TickRage(3);Require(Mathf.Abs(rage-50)<.0001f,"Active resource drains linearly to half after three seconds");before=rage;Hit(target,10,"cleave");invulnerability=0;Hurt(10);Require(rage==before,"Neither outgoing nor incoming damage refills active Rage");
   SetState(ScreenState.Paused);float remaining=rageRemaining;ReadRageInput();Require(rageRemaining==remaining,"Paused input cannot activate Rage");SetState(ScreenState.Expedition);TickRage(3);Require(rage==0&&!RageActive&&Mathf.Abs(Damage-normalDamage)<.0001f&&Mathf.Abs(AttackRate-normalRate)<.0001f,"Rage empties exactly at six seconds and restores normal combat values");GainRage(50,true);Require(rage==1,"Rage can charge again after the active window");
   StartRun();Require(rage==0&&rageRemaining==0&&xp==0,"Starting a new run resets Rage and XP independently");xp=Threshold*.37f;GainRage(5000,true);Require(ExperienceBarFill==.37f&&RageFill==1,"XP bar and Rage globe have independent resources");
   var saved=JsonUtility.FromJson<Settlement>(JsonUtility.ToJson(city));saved.EnsureVirtue();Require(saved.virtue==Virtue.Rage,"Virtue assignment persists in the settlement save");
   CheckRageInputAndFacing();Debug.Log("VEILBOUND_RAGE_VIRTUE_LOGIC_PASS");
  
   }finally{criticalRandom=reviewRandom;}
  }
  void CheckRageInputAndFacing() {
   var keyboard=InputSystem.AddDevice<Keyboard>();var mouse=InputSystem.AddDevice<Mouse>();var pad=InputSystem.AddDevice<Gamepad>();
   StartRun();combatReviewFreeze=true;cameraRig.transform.position=new Vector3(0,20,-16);cameraRig.transform.LookAt(Vector3.zero);cameraRig.orthographicSize=12;var aimPoint=new Vector3(-4,0,4);var pointer=(Vector2)cameraRig.WorldToScreenPoint(aimPoint);
   InputSystem.QueueStateEvent(mouse,new MouseState{position=pointer});InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.D));InputSystem.QueueStateEvent(pad,new GamepadState{leftStick=new Vector2(.05f,0)});InputSystem.Update();
   for(int i=0;i<40;i++){InputSystem.Update();ReadCombatControls();Require(!gamepadCombat&&movement.x==1&&Vector3.Dot(facing,(aimPoint-position).normalized)>.999f,"Stationary mouse keeps aiming across keyboard movement and connected-controller drift");position+=Vector3.right*.025f;}
   InputSystem.QueueStateEvent(keyboard,new KeyboardState());InputSystem.Update();ReadCombatControls();Require(Vector3.Dot(facing,(aimPoint-position).normalized)>.999f,"Releasing movement keeps facing the stationary mouse");
   ResetRage();rage=100;InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Q));InputSystem.Update();ReadCombatControls();ReadRageInput();Require(RageActive&&!gamepadCombat,"Keyboard Q activates ready Rage");
   InputSystem.QueueStateEvent(keyboard,new KeyboardState());InputSystem.Update();ResetRage();rage=100;InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.South));InputSystem.Update();ReadRageInput();Require(!RageActive,"Xbox A remains dodge and does not activate Rage");
   InputSystem.QueueStateEvent(pad,new GamepadState());InputSystem.Update();InputSystem.QueueStateEvent(pad,new GamepadState{leftStick=Vector2.right,rightStick=Vector2.up}.WithButton(GamepadButton.West));InputSystem.Update();ReadCombatControls();ReadRageInput();Require(RageActive&&gamepadCombat&&facing.z>.99f&&movement.x>.99f,"Xbox X activates Rage while independent stick movement and aim stay intact");
   ResetRage();rage=100;SetState(ScreenState.Paused);Require(!ActivateRage()&&rage==100,"Menus cannot consume ready Rage");SetState(ScreenState.Expedition);InputSystem.RemoveDevice(keyboard);InputSystem.RemoveDevice(mouse);InputSystem.RemoveDevice(pad);ResetRage();position=Vector3.zero;gamepadCombat=false;
   Debug.Log("VEILBOUND_MOUSE_FACING_AND_RAGE_BINDINGS_PASS");
  }
  IEnumerator RageVirtueReview(string folder) {
   Directory.CreateDirectory(folder);ReturnHome();var previous=city;city=new Settlement{blacksmith=3,bronzeHeroVersion=1};city.EnsureGear();for(int s=0;s<8;s++){city.equippedGear[s]=102;city.ownedPlate[s*10+2]=true;}city.equippedGear[8]=11;city.ownedGear[24]=1;city.equippedMetalWeapon=2;city.ownedMetalWeapons[2]=true;city.EnsureGearRolls();city.EnsureVirtue();RebuildHero();CheckRageVirtue();
   StartRun();combatReviewFreeze=true;foreach(var id in new[]{"whirl","orbit","smite"})build.Pick(TestCard(id,UpgradeRarity.Common));level=7;elapsed=173;health=MaxHealth*.81f;xp=Threshold*.63f;
   ReadoutTarget(new Vector3(-2,0,2));ReadoutTarget(new Vector3(2,0,2));ReadoutTarget(new Vector3(0,0,3));hero.root.position=position;hero.root.rotation=Quaternion.identity;hero.Animate(0,0,0);cameraRig.transform.position=new Vector3(7,13,-13);cameraRig.transform.LookAt(new Vector3(0,0,2));cameraRig.orthographicSize=9;
   rage=43;yield return CardScreenshot(Path.Combine(folder,"01-rage-building-xp-bar.png"));rage=100;reviewRageTooltip=true;yield return CardScreenshot(Path.Combine(folder,"02-rage-ready-tooltip.png"));reviewRageTooltip=false;
   ActivateRage();yield return CardScreenshot(Path.Combine(folder,"03-rage-eruption.png"));TickRage(3);damageNumbers.Clear();yield return CardScreenshot(Path.Combine(folder,"04-rage-half-drained.png"));OpenCharacterSheet(0);yield return CardScreenshot(Path.Combine(folder,"05-active-buff-stats.png"));characterTab=2;yield return CardScreenshot(Path.Combine(folder,"06-rage-damage-tracker.png"));CloseCharacterSheet();TickRage(3);yield return CardScreenshot(Path.Combine(folder,"07-rage-empty.png"));
   xp=Threshold*.94f;rage=100;gamepadCombat=true;yield return CardScreenshot(Path.Combine(folder,"08-controller-ready.png"));gamepadCombat=false;reviewRageTooltip=false;combatReviewFreeze=false;ReturnHome();city=previous;RebuildHero();
   File.WriteAllText(Path.Combine(folder,"PASS.txt"),"PASS: default and saved Rage virtue; actual dealt/taken gain; capped resource; blocked overkill/friendly/immune gain; ready-only Q and Xbox X; independent controller aim and keyboard/mouse persistence; weapon-scaled AoE and damage attribution; 25% damage and 30% attack rate; linear six-second drain; no active recharge; expiry/reset; independent full-width bordered XP rail.\n");Debug.Log("VEILBOUND_RAGE_VIRTUE_PASS");
  }
 }
}

