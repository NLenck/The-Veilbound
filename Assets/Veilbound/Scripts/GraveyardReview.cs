using System;using System.IO;using System.Collections;using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  IEnumerator GraveyardReview(){
   yield return null;combatReviewFreeze=true;ReturnHome();SelectExpeditionMap(0);StartRun();Require(ExpeditionMapName(activeMap)=="Graveyard"&&arena.name=="Graveyard","Graveyard display name and map are active");Require(GraveyardArena.GravePositions.Count==42&&GraveyardArena.Rooms.Count==2,"clean graveyard layout is built");Require(collision.Free(Vector3.zero,1.12f),"safe starting space at shrine");
   foreach(int kind in new[]{0,1,2,3,4}){var foe=SpawnFoe(0,kind);Require(foe!=null&&navigation.Reachable(foe.t.position,foe.radius),"all existing shrine enemy families can enter Graveyard");}
   foreach(int boss in new[]{1,2,3}){var foe=SpawnFoe(boss);Require(foe!=null&&navigation.Reachable(foe.t.position,foe.radius)&&foe.rig.visual.production.assetId==ProductionEnemySelection.Pick(0,0,boss),"doubled bosses retain their correct Graveyard selection and route");}
   ClearRunObjects();var room=GraveyardArena.Rooms[0];position=room.transform.position;navigation.Tick(position,1,true);var elder=SpawnFoe(1);Require(elder!=null,"boss can target cathedral interior");elder.t.position=position-Vector3.forward*(room.depth/2+1.8f);for(int i=0;i<250;i++){elder.t.position=navigation.Move(elder.t.position,.09f,elder.radius);if(Vector3.Distance(elder.t.position,position)<.2f)break;}Require(Vector3.Distance(elder.t.position,position)<.3f,"boss physically traverses cathedral breach");
   hero.root.position=position;TickTownInteriors(1);var block=new MaterialPropertyBlock();room.walls[0].GetPropertyBlock(block);Require(block.GetFloat("_WallHeightScale")<.4f,"cathedral cutaway works during actual play");ClearRunObjects();
   CheckRandomExpeditionMaps();ReturnHome();SelectExpeditionMap(0);StartRun();Require(collision.Radius==58&&collision.PlayerRadius==48&&cameraRig.GetComponent<VeilFogEffect>().playerRadius==48,"Graveyard restores its own fog and collision bounds after village visits");
   string folder=Path.Combine(Application.dataPath,"..","Smoke","Graveyard");Directory.CreateDirectory(folder);position=Vector3.zero;hero.root.position=position;TownCamera(Vector3.zero,12);TickTownInteriors(1);yield return null;yield return null;CaptureVillageCamera(Path.Combine(folder,"gameplay.png"),1600,1000);
   cameraRig.transform.position=new Vector3(0,70,-20);cameraRig.transform.LookAt(Vector3.zero);cameraRig.orthographicSize=66;yield return null;CaptureVillageCamera(Path.Combine(folder,"overview.png"),1800,1600);
   position=GraveyardArena.CathedralCentre;hero.root.position=position;TownCamera(position,12);TickTownInteriors(1);yield return null;CaptureVillageCamera(Path.Combine(folder,"cathedral-gameplay.png"),1600,1000);
   File.WriteAllText(Path.Combine(folder,"PASS.txt"),"PASS: Graveyard naming and default map; 42 grave markers and keeper house; player spawn; all shrine enemies and doubled bosses reachable; actual boss cathedral traversal; indoor cutaway; random village/Graveyard switching; 48/58m bounds and matching Veil; actual-game renders.");Debug.Log("VEILBOUND_GRAVEYARD_RUNTIME_PASS");Application.Quit(0);
  }
 }
}
