#if UNITY_EDITOR
using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
namespace Veilbound.Editor {
 public static class BossScaleValidation {
  const string Output="BossScaleUpdate";
  [Serializable]class Row{public int map,boss;public string asset;public float factor,height;}
  [Serializable]class Audit{public bool passed;public List<Row> bosses=new List<Row>();}
  public static void Build(){Run();ProductionModelImport.Build();}
  public static void Run(){
   Directory.CreateDirectory(Output);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);var audit=new Audit();
   foreach(int map in new[]{0,1})foreach(int boss in new[]{1,2,3}){
    var id=ProductionEnemySelection.Pick(map,0,boss);var prefab=ProductionModelCatalog.Current.Enemy(id).prefab;var rig=new HuskRig(null,0,boss,map);
    try{float expected=2*(1+(boss-1)*.07f);if(Vector3.Distance(rig.root.localScale,prefab.transform.localScale*expected)>.0001f)throw new Exception("Boss scale mismatch "+id);rig.Animate(0,0,0);var mesh=new Mesh();rig.visual.skin.BakeMesh(mesh);float height=rig.visual.skin.bounds.size.y;if(height<2||!float.IsFinite(height))throw new Exception("Invalid boss geometry");UnityEngine.Object.DestroyImmediate(mesh);rig.Animate(.3f,1,.05f);rig.visual.GroundSlamPose(.5f);if(rig.visual.bones.Any(b=>!float.IsFinite(b.position.y)))throw new Exception("Invalid scaled pose");audit.bosses.Add(new Row{map=map,boss=boss,asset=id,factor=expected,height=height});}
    finally{UnityEngine.Object.DestroyImmediate(rig.root.gameObject);}
   }
   foreach(int map in new[]{0,1}){var rig=new HuskRig(null,0,0,map);var prefab=ProductionModelCatalog.Current.Enemy(ProductionEnemySelection.Pick(map,0,0)).prefab;if(Vector3.Distance(rig.root.localScale,prefab.transform.localScale)>.0001f)throw new Exception("Normal enemy was resized");UnityEngine.Object.DestroyImmediate(rig.root.gameObject);}
   audit.passed=true;File.WriteAllText(Output+"/validation.json",JsonUtility.ToJson(audit,true));Debug.Log("VEILBOUND_BOSS_SCALE_VALIDATION_PASS");
  }
 }
}
#endif

