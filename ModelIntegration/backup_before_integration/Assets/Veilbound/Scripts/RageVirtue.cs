using System;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Veilbound {
 public enum Virtue { None, Rage, Devotion, Academic, Discipline }
 public partial class Settlement { public Virtue virtue=Virtue.Rage;public void EnsureVirtue(){if(virtue==Virtue.None)virtue=Virtue.Rage;} }
 public partial class VeilGame {
  const float RageMaximum=100,RageBlastRadius=4;
  float RageDuration=>6+build.Attribute("rage","duration");
  float RageBlastDamage=>100*(1+.20f*build.Attribute("rage","damage"))*(1+build.Amount("power")*.12f+city.GearBonus("power")*.01f);
  float rageWindowDuration=6;
  float RageDamageBonus=>.25f+.05f*build.Attribute("rage","power");
  float RageAttackBonus=>.30f+.05f*build.Attribute("rage","haste");
  float rage,rageRemaining;bool gamepadCombat;
  bool RageActive=>rageRemaining>0;
  bool RageReady=>city.virtue==Virtue.Rage&&!RageActive&&rage>=RageMaximum-.0001f;
  float RageFill=>Mathf.Clamp01(rage/RageMaximum);
  float RageDamageMultiplier=>RageActive?1+RageDamageBonus:1;
  float GlobalHasteRating=>build.Amount("haste")*8+city.GearBonus("haste")+(RageActive?RageAttackBonus*100:0);
  float AttackRate=>1/(1-HasteRules.Reduction(GlobalHasteRating));
  float SkillHasteRating(string id)=>GlobalHasteRating+(id=="rage"?0:build.Attribute(id,"haste")*UpgradeStep(id,"haste"));
  float SkillAttackRate(string id)=>1/(1-HasteRules.Reduction(SkillHasteRating(id)));
  string RageBinding=>gamepadCombat?PadLabel(ControlAction.Virtue):KeyLabel(ControlAction.Virtue);
  void ResetRage(){rage=0;rageRemaining=0;rageWindowDuration=6;ResetVirtueAbilities();}
  void GainRage(float actualDamage,bool dealt){if(city.virtue!=Virtue.Rage||RageActive||state!=ScreenState.Expedition||actualDamage<=0)return;rage=Mathf.Clamp(rage+actualDamage*(dealt?.02f:.10f),0,RageMaximum);}
  bool ActivateRage() {
   if(state!=ScreenState.Expedition||!RageReady)return false;
   // Set active before the blast, so the activation itself cannot refill the resource.
   rage=RageMaximum;rageWindowDuration=RageDuration;rageRemaining=rageWindowDuration;PlayerCast("rage",position,SkillRadius("rage"));
   AreaDamage(position,SkillRadius("rage"),RageBlastDamage,false,"rage");
   Burst(position,SkillRadius("rage"),new Color(1,.32f,.035f),.55f);Burst(position,1.2f,new Color(1,.65f,.16f),.35f);
   return true;
  }
  void TickRage(float dt){if(!RageActive)return;rageRemaining=Mathf.Max(0,rageRemaining-Mathf.Max(0,dt));if(rageRemaining<.0001f)rageRemaining=0;rage=RageMaximum*rageRemaining/rageWindowDuration;}
  bool RagePressed()=>ActionPressed(ControlAction.Virtue);
  void ReadRageInput(){if(RagePressed())ActivateRage();}
  void ReadCombatControls() {
   var key=KeyboardGameplay?Keyboard.current:null;var pad=ControllerGameplay?PreferredPad:null;var mouse=KeyboardGameplay?Mouse.current:null;Vector2 keyboard=Vector2.zero;
   if(key!=null)keyboard=new Vector2((Held(ControlAction.Right)?1:0)-(Held(ControlAction.Left)?1:0),(Held(ControlAction.Up)?1:0)-(Held(ControlAction.Down)?1:0));
   Vector2 left=pad?.leftStick.ReadValue()??Vector2.zero,right=pad?.rightStick.ReadValue()??Vector2.zero;
   bool keyboardIntent=keyboard.sqrMagnitude>.01f||KeyPressed(ControlAction.Virtue)||KeyPressed(ControlAction.Dodge)||KeyPressed(ControlAction.DodgeAlt);
   bool mouseIntent=mouse!=null&&(mouse.delta.ReadValue().sqrMagnitude>.1f||mouse.leftButton.wasPressedThisFrame||mouse.rightButton.wasPressedThisFrame);
   if(keyboardIntent||mouseIntent)gamepadCombat=false;
   else if(left.sqrMagnitude>.03f||right.sqrMagnitude>.12f||PadPressed(ControlAction.Virtue)||PadPressed(ControlAction.Dodge)||PadPressed(ControlAction.DodgeAlt))gamepadCombat=true;
   if(preferences.inputMode==1)gamepadCombat=false;else if(preferences.inputMode==2)gamepadCombat=true;
   movement=Vector2.ClampMagnitude(gamepadCombat?(left.sqrMagnitude>.03f?left:Vector2.zero):keyboard,1);
   if(gamepadCombat){if(right.sqrMagnitude>.12f)facing=new Vector3(right.x,0,right.y).normalized;else if(movement.sqrMagnitude>.01f)facing=new Vector3(movement.x,0,movement.y).normalized;}
   else if(mouse!=null)AimAtPointer(mouse.position.ReadValue());
   // Keyboard movement never becomes a fallback aiming direction.
  }
  void AimAtPointer(Vector2 pointer){var ray=cameraRig.ScreenPointToRay(pointer);if(new Plane(Vector3.up,Vector3.zero).Raycast(ray,out float distance)){Vector3 delta=ray.GetPoint(distance)-position;delta.y=0;if(delta.sqrMagnitude>.1f)facing=delta.normalized;}}
  Choice RageDamageChoice=>Array.Find(RunBuild.Pool,c=>c.id=="rage");
  Choice DamageSource(string id)=>id=="rage"?RageDamageChoice:Array.Find(RunBuild.Pool,c=>c.id==id);
 }
}
