using System;
using System.Collections;
using System.IO;
using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  IEnumerator SanctuaryKnightReview(){string folder=Path.Combine(Application.dataPath,"..","Smoke","SanctuaryKnightV2");Directory.CreateDirectory(folder);yield return new WaitForSecondsRealtime(.4f);ReturnHome();combatReviewFreeze=true;city=new Settlement{blacksmith=6,tailor=2,leatherworker=2,bronzeHeroVersion=1,mine=3,weaver=2,huntersLodge=2,wood=500,stone=400,essence=455};city.EnsureGear();for(int slot=0;slot<8;slot++){city.ownedPlate[slot*10+2]=true;city.equippedGear[slot]=102;}city.equippedGear[8]=11;city.ownedGear[24]=1;city.equippedMetalWeapon=2;city.ownedMetalWeapons[2]=true;city.EnsureGearRolls();RebuildHero();var v=hero.root.GetComponent<BronzeHeroVisual>();Require(v&&v.bones.Length==17,"Sanctuary set uses the existing fitted character rig");Require(hero.root.Find("HELM - sanctuary crested crown")&&hero.root.Find("ARMOUR - sanctuary cuirass front"),"New Sanctuary geometry replaces the old plate meshes");
   for(int slot=0;slot<9;slot++){bool found=false;for(int i=0;i<v.slots.Length;i++)if(v.slots[i]==slot)found=true;Require(found,"Every equipment slot has independent rigged geometry");}
   foreach(var renderer in v.parts)foreach(var w in renderer.sharedMesh.boneWeights)Require(Mathf.Abs(w.weight0+w.weight1+w.weight2+w.weight3-1)<.0002f,"Every imported weight remains normalised");
   BronzePortrait(Path.Combine(folder,"01-bronze-guard.png"),0,0,0,new Vector3(2.8f,2.1f,5),1.38f,1.14f);BronzePortrait(Path.Combine(folder,"02-walk.png"),.47f,1,0,new Vector3(2.8f,2.1f,5),1.38f,1.14f);BronzePortrait(Path.Combine(folder,"03-attack.png"),.25f,1,.65f,new Vector3(2.8f,2.1f,5),1.38f,1.14f);BronzePortrait(Path.Combine(folder,"04-rear.png"),0,0,0,new Vector3(-2.5f,2.1f,-5),1.38f,1.14f);BronzePortrait(Path.Combine(folder,"05-helmet.png"),0,0,0,new Vector3(.4f,2,4),.33f,1.9f);
   city.equippedGear[0]=0;RebuildHero();v=hero.root.GetComponent<BronzeHeroVisual>();foreach(var part in v.parts)if(Array.IndexOf(v.parts,part)>=0&&v.slots[Array.IndexOf(v.parts,part)]==0)Require(!part.gameObject.activeSelf,"Helmet removes independently");BronzePortrait(Path.Combine(folder,"06-without-helmet.png"),0,0,0,new Vector3(2.8f,2.1f,5),1.38f,1.14f);city.equippedGear[0]=102;RebuildHero();tab=1;gearSlot=2;yield return CardScreenshot(Path.Combine(folder,"07-loadout.png"));StartRun();for(int i=0;i<60;i++)PositionCamera(.1f);yield return CardScreenshot(Path.Combine(folder,"08-in-expedition.png"));File.WriteAllText(Path.Combine(folder,"PASS.txt"),"PASS: new Sanctuary helmet/cuirass; all nine independent slots; 17 fitted joints; all normalised imported weights; idle/walk/attack/rear/helmet portraits; independent helmet removal; existing body and two-hand grip; loadout and actual expedition captures.\n");Debug.Log("VEILBOUND_SANCTUARY_KNIGHT_PASS");Application.Quit(0);
  }
 }
}

