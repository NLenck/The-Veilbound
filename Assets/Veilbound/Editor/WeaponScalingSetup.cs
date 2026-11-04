#if UNITY_EDITOR
using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using Veilbound.Greatswords;
namespace Veilbound.Editor {
 public static class WeaponScalingSetup {
  const string Output="WeaponScalingUpdate";
  static void Need(bool value,string why){if(!value)throw new Exception("Weapon scaling: "+why);}
  [Serializable]class Row{public int id;public string name;public float damage,interval,cooldownPercent;}
  [Serializable]class Result{public bool passed;public List<Row> weapons=new List<Row>();public bool sharedVeilModel,coalRecipes,saveRoundtrip,legacyMigration;}
  static bool RefineForCheck(Settlement c,int metal){if(!c.MakeBars(metal,1,out _))return false;c.TickSmithOrders(c.SmithQueueHours());return true;}
  public static void Build(){Run();ProductionModelImport.Build();}
  public static void Run(){
   Directory.CreateDirectory(Output);Directory.CreateDirectory("Assets/Veilbound/Resources/WeaponVisuals");AssetDatabase.Refresh();
   var source=AssetDatabase.LoadAssetAtPath<GreatswordVisualLibrary>("Assets/VeilboundGreatswords/Generated/GreatswordVisualLibrary.asset");Need(source&&source.swords.Length==10,"generated sword models available");
   var library=ScriptableObject.CreateInstance<GreatswordVisualLibrary>();var entries=source.swords.Select(e=>new GreatswordEntry{tier=e.metalIndex==9?"Veilcrystal_Titanium":e.tier,metalIndex=e.metalIndex,prefab=e.prefab,triangles=e.triangles}).ToList();var veil=entries.First(e=>e.metalIndex==9);entries.Add(new GreatswordEntry{tier="Veilcrystal_Tungsten",metalIndex=10,prefab=veil.prefab,triangles=veil.triangles});library.swords=entries.ToArray();
   const string path="Assets/Veilbound/Resources/WeaponVisuals/GreatswordLibrary.asset";var existing=AssetDatabase.LoadAssetAtPath<GreatswordVisualLibrary>(path);if(existing){EditorUtility.CopySerialized(library,existing);UnityEngine.Object.DestroyImmediate(library);library=existing;}else AssetDatabase.CreateAsset(library,path);AssetDatabase.SaveAssets();
   var result=new Result();float[] damage={10,18,24,35,34,39,48,61,64,109,88,150},interval={2,2.24f,2.4f,2.32f,2.08f,1.92f,1.92f,2.24f,1.6f,2.72f,1.6f,2.72f};
   for(int id=-1;id<WeaponRules.Count;id++){Need(WeaponRules.Damage(id)==damage[id+1]&&WeaponRules.Interval(id)==interval[id+1],"workbook profile "+id);result.weapons.Add(new Row{id=id,name=WeaponRules.Name(id),damage=WeaponRules.Damage(id),interval=WeaponRules.Interval(id),cooldownPercent=WeaponRules.CooldownMultiplier(WeaponRules.Interval(id))*100});}
   var c=new Settlement{blacksmith=9};c.EnsureGearRolls();c.EnsureIndustry();c.ores[7]=20;c.ores[4]=20;c.metalBars[7]=2;c.metalBars[8]=2;
   Need(RefineForCheck(c,9)&&c.ores[7]==18&&c.ores[4]==18&&c.metalBars[7]==1&&c.metalBars[8]==2&&c.metalBars[9]==1,"Titanium Veil bar exact recipe");Need(RefineForCheck(c,10)&&c.ores[7]==16&&c.ores[4]==16&&c.metalBars[8]==1&&c.metalBars[10]==1,"Tungsten Veil bar exact recipe");
   c.ores[4]=0;double raw=c.ores[7],titanium=c.metalBars[7];Need(!c.MakeBars(9,1,out _)&&c.ores[7]==raw&&c.metalBars[7]==titanium,"missing coal rejects atomically");
   for(int id=5;id<WeaponRules.Count;id++)Need(c.BarRecipe(id).Any(r=>r.family==0&&r.index==4&&r.amount>0),"coal in every Steel+ bar recipe");result.coalRecipes=true;
   c.metalBars[9]=c.metalBars[10]=8;for(int i=0;i<5;i++)c.AddResource(1,i,100);Need(c.CraftMetalWeapon(9,out _),"craft light Veil weapon");int light=c.lastCraftedRollId;Need(c.CraftMetalWeapon(10,out _),"craft heavy Veil weapon");int heavy=c.lastCraftedRollId;Need(c.EquipRoll(heavy)&&c.WeaponItemIndex==10&&c.WeaponMetalIndex==9&&c.WeaponDamage==150&&c.WeaponInterval==2.72f,"equip heavy Veil profile and shared material");
   var store=new CharacterSaveStore(Output+"/SaveQA-"+Guid.NewGuid().ToString("N"));var roster=store.LoadRoster();var character=SavedCharacter.From("Weapon QA",c);store.Save(character,c,roster);store.Load(character.id,out _,out var loaded);Need(loaded.WeaponItemIndex==10&&loaded.craftedGear.Any(g=>g.id==light&&g.item==9)&&loaded.craftedGear.Any(g=>g.id==heavy&&g.item==10),"separate variants survive on-disk saves");Need(loaded.EquipRoll(light)&&loaded.WeaponDamage==88&&loaded.WeaponInterval==1.6f,"light variant re-equips correctly");result.saveRoundtrip=true;
   var old=new Settlement{metalVersion=1,ownedMetalWeapons=new bool[10],metalBars=new double[10]};old.ownedMetalWeapons[9]=true;old.metalBars[9]=12.5;old.EnsureGear();Need(old.ownedMetalWeapons.Length==11&&old.metalBars.Length==11&&old.ownedMetalWeapons[9]&&old.metalBars[9]==12.5&&!old.ownedMetalWeapons[10]&&old.metalBars[10]==0,"old inventory growth preserves all entries");result.legacyMigration=true;
   Need(library.swords.First(e=>e.metalIndex==9).prefab==library.swords.First(e=>e.metalIndex==10).prefab,"both variants share the exact Veil model");result.sharedVeilModel=true;
   foreach(int id in new[]{9,10}){c.equippedMetalWeapon=id;var root=new GameObject("Weapon visual QA");try{var visual=StarterEquipment.CreateEquippedSword(root.transform,c);Need(visual.GetComponentsInChildren<MeshFilter>().Length==1&&visual.GetComponentsInChildren<MeshRenderer>().All(r=>r.sharedMaterial),"shared model and material load in real factory");}finally{UnityEngine.Object.DestroyImmediate(root);}}
   result.passed=true;File.WriteAllText(Output+"/validation.json",JsonUtility.ToJson(result,true));Debug.Log("VEILBOUND_WEAPON_SCALING_SETUP_PASS");
  }
 }
}
#endif
