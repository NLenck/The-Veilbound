using System;using System.Collections;using System.IO;using System.Linq;using UnityEngine;
namespace Veilbound {
 public sealed class StarterGearRuntimeCheck:MonoBehaviour {
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Begin(){if(Array.IndexOf(Environment.GetCommandLineArgs(),"-starterGearReview")>=0)new GameObject("Starter gear isolated check").AddComponent<StarterGearRuntimeCheck>();}
  IEnumerator Start(){yield return null;yield return null;
   foreach(bool female in new[]{false,true}){
    var city=new Settlement{female=female};city.InitializeWoodenStarter();
    if(!city.IsWoodenSword||city.craftedGear.Count!=1||city.equippedGear.Any(x=>x!=0))throw new Exception("Invalid starter loadout");
    var rig=new HeroRig(null,city);for(int i=0;i<20;i++){rig.SetMotion(new Vector3(0,0,2));rig.Animate(i*.05f,1,Mathf.Sin(i*.2f)*.5f+.5f);yield return null;}
    if(rig.root.GetComponent<ProductionHeroVisual>().equipment.activeSlots!=0||rig.swordPivot.GetComponentsInChildren<MeshRenderer>().Length!=1)throw new Exception("Invalid starter visuals");
    var store=new CharacterSaveStore(Path.Combine(Application.persistentDataPath,"starter-qa",Guid.NewGuid().ToString("N")));var roster=store.LoadRoster();var character=SavedCharacter.From("Starter QA",city);store.Save(character,city,roster);store.Load(character.id,out _,out var loaded);
    if(!loaded.IsWoodenSword||loaded.EquippedRoll(9)?.item!=-1||loaded.EquipmentArmour!=0)throw new Exception("Invalid starter save reload");
    rig.Release();Destroy(rig.root.gameObject);yield return null;
   }
   File.WriteAllText(Path.Combine(Application.dataPath,"..","starter-runtime-PASS.txt"),"PASS: both native character rigs, wooden sword attachment, sampled walk/swing poses, no armour, independent isolated save/load.");Debug.Log("VEILBOUND_STARTER_RUNTIME_PASS");Application.Quit(0);
  }
 }
}
