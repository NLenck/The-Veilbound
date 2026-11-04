using System;using UnityEngine;
namespace Veilbound {public partial class VeilGame {
 int lightGearTier=1;
 void LightTierPalette(float x,float y,float width){
  lightGearTier=Mathf.Clamp(lightGearTier,1,10);
  for(int i=0;i<10;i++){int tier=i+1;string requirement=city.LightGearRequirement(gearSlot,gearMaterial,tier);var rect=new Rect(x+i%5*width/5,y+i/5*35,width/5-7,29);CardUtilityButton(rect,"T"+tier+(requirement==""?"":" / locked"),()=>lightGearTier=tier,true);if(lightGearTier==tier)CardOutline(rect,hudLight,2);}
 }
 void CraftSelectedLightGear(){if(city.CraftLightGear(gearSlot,gearMaterial,lightGearTier,out notice)){RefreshMetalGear();OpenGearBag(gearSlot,city.lastCraftedRollId);}}
 void QueueSelectedLightGear(){if(city.QueueSettlementJob(5,"",gearMaterial,gearSlot,1,lightGearTier,out notice)){city.RefreshSettlementPlan();Save();}}
}}
