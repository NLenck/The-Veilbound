#if UNITY_EDITOR
using System;
using UnityEngine;
namespace Veilbound.Editor {
 public static class WeightBalanceBuild {
  public static void Build(){
   float[] oldWeaponWeights={.93f,1.14f,1.12f,1.08f,.99f,1f,1.05f,.56f,2.26f,.70f};
   float[] suitWeights={44.964706f,55.670588f,53.529412f,51.388235f,49.247059f,51.388235f,57.811765f,36.4f,75f,42.823529f};
   var sample=new Settlement();sample.EnsureGear();sample.equippedMetalWeapon=-2;Array.Clear(sample.equippedGear,0,9);
   if(Mathf.Abs(sample.GearSpeed-1)>.00001f)throw new Exception("Below allowance must have no slowdown");
   for(int m=0;m<10;m++){
    if(Mathf.Abs(Metals.All[m].weight-oldWeaponWeights[m])>.00001f)throw new Exception("Weapon weight changed: "+m);
    for(int s=0;s<8;s++)sample.equippedGear[s]=Metals.Item(m);
    float expectedMass=suitWeights[m]+1.8f;
    if(Mathf.Abs(sample.EquipmentMass-expectedMass)>.001f)throw new Exception("Equipped suit mass mismatch: "+m);
    if(sample.GearSpeed>=1||Mathf.Abs(sample.GearSpeed-(1-(expectedMass-30)*.0025f))>.00001f)throw new Exception("Suit weight does not reach movement: "+m);
    sample.equippedMetalWeapon=m;
    if(Mathf.Abs(sample.EquipmentMass-(suitWeights[m]+4.5f*oldWeaponWeights[m]))>.001f)throw new Exception("Weapon mass mismatch: "+m);
    sample.equippedMetalWeapon=-2;
   }
   Debug.Log("VEILBOUND_WEIGHT_BALANCE_PASS: ten armour suits, unchanged weapon weights, equipment totals and movement penalties");
   CombatFlowBuild.Build();
  }
 }
}
#endif
