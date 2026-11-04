using System;using System.IO;using System.Collections;using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  int enemyAttackChecks;
  void AttackCheck(bool value,string text){Require(value,text);enemyAttackChecks++;}
  Foe AttackTestFoe(int kind,int map,int variant,int boss=0){
   foreach(var old in foes){CancelEnemyCharge(old);Destroy(old.t.gameObject);}foes.Clear();foreach(var h in hazards)Destroy(h.t.gameObject);hazards.Clear();collision.obstacles.Clear();collision.walls.Clear();collision.Radius=ArenaCollision.Boundary;collision.PlayerRadius=ArenaCollision.PlayerBoundary;navigation=new ArenaNavigation(collision);
   var rig=new HuskRig(actors,kind,boss,map,variant);rig.root.SetPositionAndRotation(Vector3.zero,Quaternion.identity);var f=new Foe{rig=rig,t=rig.root,kind=kind,boss=boss,variant=variant,hp=10000,maxHp=10000,radius=boss>0?1.1f:EnemyTypes.Radius(kind),contact=9};foes.Add(f);return f;
  }
  IEnumerator EnemyAttackReview(){
   yield return null;combatReviewFreeze=true;city=FreshCharacterTown();RebuildHero();StartRun();combatReviewFreeze=true;string folder=Path.Combine(Application.dataPath,"..","Smoke","EnemyAttacks");Directory.CreateDirectory(folder);
   foreach(bool boss in new[]{false,true}){
    var f=AttackTestFoe(2,1,0,boss?1:0);position=Vector3.forward*2;hero.root.position=position;invulnerability=0;float hp=health;
    AttackCheck(HandleGroundSlam(f,0)&&f.slamHazard.directional,"Brute uses a front sector including boss form");var hazard=f.slamHazard;Vector3 facing=hazard.direction;
    for(int frame=0;frame<24;frame++){UpdateHazards(1/30f);if(frame%4==0){yield return null;AttackCapture(f,Path.Combine(folder,(boss?"BossBrute":"Brute")+"_"+frame+".png"));}}
    position=Vector3.back*1.2f;UpdateHazards(.3f);AttackCheck(health==hp,"standing behind committed Brute slam avoids damage");AttackCheck(Vector3.Dot(facing,Vector3.forward)>.99f,"Brute locks its tell direction");
   }
   var front=AttackTestFoe(2,1,0);position=Vector3.forward*2;invulnerability=0;float before=health;HandleGroundSlam(front,0);UpdateHazards(1.01f);AttackCheck(health<before,"Brute front sector damages at impact");float hitHealth=health;UpdateHazards(2);AttackCheck(health==hitHealth,"Brute slam hits only once");
   foreach(int map in new[]{0,1}){var heavy=AttackTestFoe(2,map,map==1?1:0);position=Vector3.forward*2;HandleGroundSlam(heavy,0);AttackCheck(!heavy.slamHazard.directional&&Mathf.Abs(heavy.slamHazard.radius-2.82f)<.001f,"Guardian and Bulwark retain circular slam");}
   AttackCheck(!EnemyAttackGeometry.SectorContains(Vector3.back,Vector3.zero,Vector3.forward,3,55,.35f),"frontal hit geometry excludes rear");AttackCheck(EnemyAttackGeometry.SectorContains(new Vector3(1,0,2),Vector3.zero,Vector3.forward,3,55,.35f),"frontal hit geometry includes front flank");
   foreach(bool sentinel in new[]{false,true}){
    var f=AttackTestFoe(sentinel?0:3,sentinel?0:1,1);position=Vector3.forward*6;hero.root.position=position;invulnerability=0;health=MaxHealth;before=health;
    AttackCheck(HandleCharge(f,0)&&f.chargeWarning,"selected unit starts its charge warning");Vector3 direction=f.chargeDirection;float length=f.chargeLength;
    for(int frame=0;frame<80;frame++){animClock+=1/30f;invulnerability=Mathf.Max(0,invulnerability-1/30f);HandleCharge(f,1/30f);if(frame%2==0){yield return null;yield return null;AttackCapture(f,Path.Combine(folder,(sentinel?"Sentinel":"Impaler")+"_"+(frame/2).ToString("D2")+".png"));}}
    AttackCheck(f.chargeHit&&health<before&&before-health<=18.01f,"charge sweeps hit once without tunnelling");AttackCheck(f.t.position.z<6&&Vector3.Distance(f.t.position,position)>=f.radius+PlayerBodyRadius&&Vector3.Dot(direction,f.t.forward)>.999f,"charge stops on player body contact");
    position=f.t.position+f.t.forward*.75f;f.attack=0;AttackCheck(f.chargeCooldown>0&&!HandleCharge(f,0)&&HandleMelee(f,0),"melee remains usable during the separate charge cooldown");
    f=AttackTestFoe(sentinel?0:3,sentinel?0:1,1);position=Vector3.forward*6;HandleCharge(f,0);float progress=f.chargeWarning.Progress;HandleCharge(f,0);AttackCheck(f.chargeWarning.Progress==progress,"zero-time update does not advance warning");position=new Vector3(4,0,6);before=health;
    for(int frame=0;frame<80;frame++){animClock+=1/30f;HandleCharge(f,1/30f);}AttackCheck(health==before&&Mathf.Abs(f.t.position.x)<.001f,"sidestep evades the locked straight charge");
   }
   var blocked=AttackTestFoe(3,1,1);position=Vector3.forward*4;collision.AddWall(new Vector3(-3,0,6),new Vector3(3,0,6),.1f);HandleCharge(blocked,0);AttackCheck(blocked.chargeLength<6&&blocked.chargeLength>3,"warning endpoint stops before wall");float marked=blocked.chargeLength;for(int frame=0;frame<80;frame++)HandleCharge(blocked,1/30f);AttackCheck(blocked.t.position.z<=marked+.01f,"charge cannot pass wall");
   var dead=AttackTestFoe(0,0,1);position=Vector3.forward*6;HandleCharge(dead,0);Hit(dead,20000,"cleave",false);AttackCheck(!dead.chargeWarning&&dead.chargeRemaining==0,"death cancels a charge and warning");
   var cultist=AttackTestFoe(0,0,0);position=Vector3.forward*6;var pose=cultist.t.GetComponent<CultistDaggerPose>();AttackCheck(pose&&pose.blade&&cultist.rig.visual.production.UsesImportedMotion,"cultist loads walk and separate dagger");
   float raisedGap=100;for(int frame=0;frame<60;frame++){animClock+=1/30f;cultist.rig.SetMotion(Vector3.forward*2.1f);cultist.rig.Animate(animClock,1,1/30f);pose.Sync();raisedGap=Mathf.Min(raisedGap,pose.hand.position.y-cultist.rig.visual.bones[7].position.y);if(frame%2==0){yield return null;yield return null;AttackCapture(cultist,Path.Combine(folder,"Cultist_"+(frame/2).ToString("D2")+".png"),true);}}
   AttackCheck(raisedGap>.15f,"dagger remains in the higher hand throughout the walk");
   AttackCheck(Vector3.Distance(pose.blade.position,pose.hand.TransformPoint(pose.gripOffset))<.001f,"dagger remains attached to raised hand");
   File.WriteAllText(Path.Combine(folder,"validation.json"),"{\"passed\":true,\"checks\":"+enemyAttackChecks+"}");Debug.Log("VEILBOUND_ENEMY_ATTACK_RUNTIME_PASS "+enemyAttackChecks);Application.Quit(0);
  }
  void AttackCapture(Foe f,string path,bool portrait=false){
   var cameraObject=new GameObject("Enemy attack QA camera");var cam=cameraObject.AddComponent<Camera>();cam.enabled=false;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.04f,.05f,.065f);cam.cullingMask=1<<28;cam.orthographic=true;cam.orthographicSize=portrait?2:6;
   foreach(var t in f.t.GetComponentsInChildren<Transform>(true))t.gameObject.layer=28;foreach(var t in hero.root.GetComponentsInChildren<Transform>(true))t.gameObject.layer=28;
   if(f.chargeWarning)foreach(var t in f.chargeWarning.GetComponentsInChildren<Transform>(true))t.gameObject.layer=28;if(f.slamHazard!=null)foreach(var t in f.slamHazard.t.GetComponentsInChildren<Transform>(true))t.gameObject.layer=28;
   hero.root.position=position;var floor=GameObject.CreatePrimitive(PrimitiveType.Plane);floor.layer=28;floor.transform.localScale=Vector3.one*3;floor.GetComponent<Collider>().enabled=false;var mat=new Material(Shader.Find("Standard"));mat.color=new Color(.13f,.14f,.16f);floor.GetComponent<Renderer>().sharedMaterial=mat;
   cam.transform.position=portrait?new Vector3(3,2.7f,4):new Vector3(9,13,-9);cam.transform.LookAt(portrait?Vector3.up:Vector3.forward*3);
   var rt=new RenderTexture(900,700,24);cam.targetTexture=rt;var previous=RenderTexture.active;cam.Render();RenderTexture.active=rt;var tex=new Texture2D(900,700,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,900,700),0,0);tex.Apply();File.WriteAllBytes(path,tex.EncodeToPNG());RenderTexture.active=previous;cam.targetTexture=null;rt.Release();Destroy(rt);Destroy(tex);Destroy(cameraObject);Destroy(floor);Destroy(mat);
  }
 }
}
