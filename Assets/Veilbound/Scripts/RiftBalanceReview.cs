using System;using System.IO;using System.Collections;using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  int riftChecks;
  void RiftCheck(bool ok,string message){Require(ok,message);riftChecks++;}
  void RiftFixture(int branch=0){
   PressureStart(1,0);position=Vector3.zero;hero.root.position=position;facing=Vector3.forward;criticalRandom=new NonCriticalReviewRandom();
   if(branch>0){build.ranks["lance"]=7;Require(build.BeginAscension(),"Rift ascension available");build.Pick(build.offers[branch-1]);}
   collision.walls.Clear();collision.obstacles.Clear();foreach(var f in foes)Destroy(f.t.gameObject);foes.Clear();ResetCombatReadouts();
  }
  void RiftStack(int count,Vector3 point){for(int i=0;i<count;i++){var rig=new HuskRig(actors,0,0,1,0);var root=rig.root;root.position=point;foes.Add(new Foe{t=root,rig=rig,hp=100000,maxHp=100000,radius=.43f});}}
  IEnumerator RiftBalanceReview(){
   yield return null;combatReviewFreeze=true;city=FreshCharacterTown();city.InitializeWoodenStarter();RebuildHero();
   RiftFixture();RiftStack(40,Vector3.forward*2);FireBolt(Vector3.up,Vector3.forward,10,false);UpdateBolts(2f/14);
   RiftCheck(abilityDamageRecords["lance"].hits==5&&Mathf.Abs(abilityDamageRecords["lance"].total-50)<.001f&&bolts.Count==0,"Forty stacked enemies receive at most five full-damage hits per blade");
   RiftFixture();RiftStack(40,Vector3.forward*2);for(int i=0;i<4;i++)FireBolt(Vector3.up,Vector3.forward,10,false);UpdateBolts(2f/14);
   RiftCheck(abilityDamageRecords["lance"].hits==20&&Mathf.Abs(abilityDamageRecords["lance"].total-200)<.001f,"Four projectiles each retain their own five-hit budget");
   RiftFixture();RiftStack(1,Vector3.forward*2);FireBolt(Vector3.up,Vector3.forward,10,false);UpdateBolts(2f/14);UpdateBolts(0);
   RiftCheck(abilityDamageRecords["lance"].hits==1&&bolts.Count==1,"A surviving blade never damages the same enemy twice");
   RiftFixture();RiftStack(1,Vector3.forward*13.1f);FireBolt(Vector3.up,Vector3.forward,10,false);var blade=bolts[0];UpdateBolts(2);
   RiftCheck(!abilityDamageRecords.ContainsKey("lance")&&bolts.Count==0,"Large frame steps cannot overshoot twelve-metre range to hit distant enemies");
   RiftFixture();RiftStack(1,Vector3.forward*11.8f);FireBolt(Vector3.up,Vector3.forward,10,false);UpdateBolts(1);
   RiftCheck(abilityDamageRecords["lance"].hits==1&&bolts.Count==0,"Targets at the end of the shortened range remain hittable");
   RiftFixture();collision.AddWall(new Vector3(-3,0,1),new Vector3(3,0,1),.2f);RiftStack(1,Vector3.forward*2);FireBolt(Vector3.up,Vector3.forward,10,false);UpdateBolts(2f/14);
   RiftCheck(bolts.Count==0&&!abilityDamageRecords.ContainsKey("lance"),"Daggers still stop at walls");
   foreach(int branch in new[]{1,2}){
    RiftFixture(branch);RiftStack(40,Vector3.forward*2);FireBolt(Vector3.up,Vector3.forward,10,false);var parent=bolts[0];UpdateBolts(2f/14);
    RiftCheck(parent.impacts==5&&!bolts.Contains(parent),"Both ascensions respect parent blade hit cap");
    if(branch==1)RiftCheck(abilityDamageRecords["lance"].total==60,"Veilbreaker retains its twenty percent per-hit bonus");
    else {RiftCheck(bolts.Count==2&&bolts.TrueForAll(b=>b.fragment),"Shattered Edge creates exactly two non-recursive fragments");UpdateBolts(0);RiftCheck(bolts.Count==0&&abilityDamageRecords["lance"].hits==15,"Fragments have five fresh hits each despite excluding the original target");}
   }
   RiftFixture();FireBolt(Vector3.up,Vector3.forward,10,true);RiftCheck(bolts[0].life==3,"Hostile projectile lifetime stays unchanged");
   RiftCheck(AbilityEffectText("lance").Contains("5 targets / 12m range"),"Ability description exposes the new limits");
   string folder=Path.Combine(Application.dataPath,"..","Smoke","RiftBalance");Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"validation.json"),"{\"passed\":true,\"checks\":"+riftChecks+",\"maxHits\":5,\"rangeMetres\":12}");Debug.Log("VEILBOUND_RIFT_BALANCE_PASS checks="+riftChecks);Application.Quit(0);
  }
 }
}
