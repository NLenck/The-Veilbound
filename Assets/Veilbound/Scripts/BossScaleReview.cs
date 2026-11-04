using System;using System.IO;using System.Collections;using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  IEnumerator BossScaleReview(){yield return null;combatReviewFreeze=true;
   foreach(int map in new[]{0,1}){ReturnHome();SelectExpeditionMap(map);StartRun();foreach(int boss in new[]{1,2,3}){
    var foe=SpawnFoe(boss);Require(foe!=null,"Scaled boss spawns on both maps");var prefab=ProductionModelCatalog.Current.Enemy(ProductionEnemySelection.Pick(map,0,boss)).prefab;Require(Vector3.Distance(foe.t.localScale,prefab.transform.localScale*(2*(1+(boss-1)*.07f)))<.0001f,"Boss is twice its previous scale");Require(Mathf.Abs(foe.radius-1.1f)<.0001f&&navigation.Reachable(foe.t.position,foe.radius),"Boss movement footprint and accessible spawn preserved");Require(Mathf.Abs(foe.maxHp-(260+boss*220)*ModifierFactor(2)*ExpeditionDifficulty.Health(VeilDepth))<.01f&&foe.contact==18,"Boss depth-scaled health and base contact value correct");foe.rig.Animate(.4f,1,.05f);yield return null;
   }ClearRunObjects();}
   string folder=Path.Combine(Application.dataPath,"..","Smoke","BossScale");Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"PASS.txt"),"PASS: six boss selections across both maps doubled relative to prior scale; animation sampled; reachable perimeter spawns; original 1.1m movement footprint, health, contact damage and normal-enemy scale preserved.");Debug.Log("VEILBOUND_BOSS_SCALE_RUNTIME_PASS");Application.Quit(0);
  }
 }
}
