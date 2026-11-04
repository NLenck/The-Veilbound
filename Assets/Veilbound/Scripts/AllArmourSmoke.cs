using System;
using System.Collections;
using System.IO;
using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  IEnumerator AllArmourReview() {
   string folder=Path.Combine(Application.dataPath,"..","Smoke","AllArmour");Directory.CreateDirectory(folder);
   string pass=Path.Combine(folder,"PASS.txt");if(File.Exists(pass))File.Delete(pass);
   yield return new WaitForSecondsRealtime(.4f);ReturnHome();combatReviewFreeze=true;
   city=new Settlement{blacksmith=9,tailor=2,leatherworker=2,bronzeHeroVersion=1};city.EnsureGear();
   for(int s=0;s<8;s++){for(int m=0;m<10;m++)city.ownedPlate[s*10+m]=true;city.ownedGear[s*3]=2;city.ownedGear[s*3+1]=2;}
   city.ownedGear[24]=2;city.equippedGear[8]=12;for(int m=0;m<10;m++)city.ownedMetalWeapons[m]=true;
   var prefab=Resources.Load<GameObject>("BronzeKnight/BronzeKnight").GetComponent<BronzeHeroVisual>();
   for(int variant=0;variant<15;variant++) {
    int item=variant<10?Metals.Item(variant):variant<12?11+(variant-10):21+(variant-12);
    for(int s=0;s<8;s++)city.equippedGear[s]=variant==14?(s%3==0?109:s%3==1?22:12):item;
    city.equippedMetalWeapon=variant<10?variant:2;city.EnsureGearRolls();RebuildHero();
    var visual=hero.root.GetComponent<BronzeHeroVisual>();Require(visual&&visual.parts.Length==prefab.parts.Length,"Every material uses the complete fitted set");
    for(int p=0;p<visual.parts.Length;p++) {
     var part=visual.parts[p];Require(part.sharedMesh==prefab.parts[p].sharedMesh,"Material changes preserve fitted mesh geometry");
     int slot=visual.slots[p];if(slot<0)continue;Require(part.gameObject.activeSelf,"Each equipped component remains visible");
     var mats=part.sharedMaterials;var original=prefab.parts[p].sharedMaterials;
     for(int k=0;k<mats.Length;k++)if(original[k].name.ToLowerInvariant().Contains("trim"))Require(Vector4.Distance(mats[k].color,original[k].color)<.001f,"Silver edging is retained for every armour family");
     if(part.name=="ARMOUR - sanctuary cuirass front") {
      Color expected=variant<10?(variant==2?original[0].color:Metals.All[variant].color):variant<12?new Color(.14f,.17f,.18f):variant<14?new Color(.17f,.095f,.045f):new Color(.14f,.17f,.18f);
      Require(Vector4.Distance(mats[0].color,expected)<.001f,"Chest finish matches its equipped material");
      Require(variant<10?mats[0].GetFloat("_Metallic")>=.4f:mats[0].GetFloat("_Metallic")==0,"Cloth/leather are matte; plate remains metallic");
     }
    }
    string name=variant<10?Metals.All[variant].name.Replace(" ","-"):variant<12?"Cloth-T"+(variant-9):variant<14?"Leather-T"+(variant-11):"Mixed-loadout";
    BronzePortrait(Path.Combine(folder,name+".png"),0,0,0,new Vector3(2.8f,2.1f,5),1.38f,1.14f);
    hero.Animate(.47f,1,.65f);foreach(var b in visual.bones)Require(!float.IsNaN(b.position.x),"Every material animates with the existing rig");
   }
   city.equippedGear[0]=0;RebuildHero();var toggled=hero.root.GetComponent<BronzeHeroVisual>();for(int p=0;p<toggled.parts.Length;p++)if(toggled.slots[p]==0)Require(!toggled.parts[p].gameObject.activeSelf,"Mixed set helmet removes independently");
   File.WriteAllText(pass,"PASS: all ten plate materials, both cloth tiers, both leather tiers, mixed loadout; same fitted meshes; correct material finishes; silver trim preserved; independent helmet toggle; posed rig. Isolated test save.");
   Debug.Log("VEILBOUND_ALL_ARMOUR_PASS");Application.Quit(0);
  }
 }
}
