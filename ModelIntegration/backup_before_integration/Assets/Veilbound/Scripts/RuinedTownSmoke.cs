using System;
using System.IO;
using System.Collections;
using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  void CheckTownWallGeometry(){
   var c=new ArenaCollision();c.AddWall(new Vector3(-5,0,0),new Vector3(-1.95f,0,0),.31f);c.AddWall(new Vector3(1.95f,0,0),new Vector3(5,0,0),.31f);
   Require(!c.Free(new Vector3(3,0,0),.42f)&&c.Free(Vector3.zero,1.12f),"A wall blocks bodies while its wide doorway admits even an Elder");
   Vector3 stopped=c.Move(new Vector3(3,0,-3),Vector3.forward*6,.42f);Require(stopped.z<-.72f,"A fast dodge cannot tunnel through masonry");
   Vector3 entered=c.Move(new Vector3(0,0,-3),Vector3.forward*6,.42f);Require(entered.z>2.9f,"Player movement passes freely through the doorway");
   Require(!c.ClearLine(new Vector3(3,0,-3),new Vector3(3,0,3),.12f)&&c.ClearLine(new Vector3(0,0,-3),new Vector3(0,0,3),.12f),"Walls stop projectiles and attacks; open doorways preserve line of sight");
   c.AddWall(new Vector3(5,0,0),new Vector3(5,0,5),.31f);Require(!c.ClearLine(new Vector3(4,0,-1),new Vector3(6,0,1)),"Diagonal segments cannot cut through wall corners");
  }
  void CheckRuinedTown(){
   Require(selectedMap==0&&activeMap==0,"The sanctuary remains the default starting map");int oldProps=collision.obstacles.Count,oldRenderers=arena.GetComponentsInChildren<Renderer>().Length;CheckTownWallGeometry();
   ReturnHome();SelectExpeditionMap(1);StartRun();Require(activeMap==1&&RuinedTownArena.Rooms.Count==12&&collision.walls.Count>=72,"The second map has twelve enterable buildings with real wall footprints");
   Require(collision.Free(Vector3.zero,1.12f),"The town starting crossroads is clear");navigation.Tick(Vector3.zero,1,true);
   foreach(var room in RuinedTownArena.Rooms){Vector3 centre=room.transform.position;Require(collision.Free(centre,1.12f)&&navigation.Reachable(centre,1.12f),"Each building centre is reachable by the largest enemy clearance");
    Vector3 a=room.transform.TransformPoint(new Vector3(0,0,-room.depth*.5f-1.6f)),b=room.transform.TransformPoint(new Vector3(0,0,room.depth*.5f+1.6f));Require(collision.ClearLine(a,b,1.12f),"Each roofless building has two aligned, passable entrances");
    var inside=cascadeTestMove(a,b-a,.42f);Require(Vector3.Distance(inside,b)<.05f,"Player can enter, cross and leave each building");
    room.Tick(centre,1);var block=new MaterialPropertyBlock();room.walls[0].GetPropertyBlock(block);Require(block.GetFloat("_WallHeightScale")<.4f,"Indoor cutaway lowers the visual wall height");room.Tick(centre+Vector3.right*100,1);room.walls[0].GetPropertyBlock(block);Require(block.GetFloat("_WallHeightScale")>.99f,"Walls return to full visual height after leaving");
   }
   foreach(int kind in new[]{0,1,2,3,4}){var foe=SpawnFoe(0,kind);Require(foe!=null&&navigation.Reachable(foe.t.position,foe.radius),"Every enemy family can enter the town from the Veil");}
   var targetRoom=RuinedTownArena.Rooms[0];position=targetRoom.transform.position;navigation.Tick(position,1,true);var elder=SpawnFoe(1);Require(elder!=null&&navigation.Reachable(elder.t.position,elder.radius),"Flow fields lead an Elder from the perimeter to a player inside a building");
   Vector3 approach=targetRoom.transform.TransformPoint(new Vector3(0,0,-targetRoom.depth*.5f-2));for(int i=0;i<220;i++)approach=navigation.Move(approach,.05f,1.12f);Require(Vector3.Distance(approach,position)<.3f,"An Elder actually walks through the door toward an indoor player");
   ReturnHome();SelectExpeditionMap(0);StartRun();Require(activeMap==0&&collision.walls.Count==0&&collision.obstacles.Count==oldProps&&arena.GetComponentsInChildren<Renderer>().Length==oldRenderers,"Returning to the sanctuary restores the original layout without town collision or duplicate geometry");
   Require(AudioListener.volume==0,"Map switching tests remain muted");Debug.Log("VEILBOUND_RUINED_TOWN_LOGIC_PASS");
  }
  Vector3 cascadeTestMove(Vector3 at,Vector3 delta,float radius)=>collision.Move(at,delta,radius,ArenaCollision.PlayerBoundary);
  void TownCamera(Vector3 player,float size=9){position=player;hero.root.position=player;facing=Vector3.forward;hero.root.rotation=Quaternion.identity;hero.Animate(0,0,0);cameraRig.transform.position=player+new Vector3(7,15,-16);cameraRig.transform.LookAt(player+Vector3.up*.5f);cameraRig.orthographicSize=size;}
  IEnumerator RuinedTownReview(){
   string folder=Path.Combine(Application.dataPath,"..","Smoke","RuinedTown");Directory.CreateDirectory(folder);string pass=Path.Combine(folder,"PASS.txt");if(File.Exists(pass))File.Delete(pass);
   yield return new WaitForSecondsRealtime(.5f);ReturnHome();combatReviewFreeze=true;CheckRuinedTown();ReturnHome();tab=0;SelectExpeditionMap(1);yield return new WaitForSecondsRealtime(.2f);yield return CardScreenshot(Path.Combine(folder,"01-map-selection.png"));StartRun();
   TownCamera(Vector3.zero,11);yield return CardScreenshot(Path.Combine(folder,"02-town-crossroads.png"));
   var room=RuinedTownArena.Rooms[3];TownCamera(room.transform.TransformPoint(new Vector3(0,0,-room.depth*.5f-1.6f)),8);TickTownInteriors(1);yield return CardScreenshot(Path.Combine(folder,"03-open-entrance.png"));
   TownCamera(room.transform.position,7);TickTownInteriors(1);ReadoutTarget(room.transform.position+room.transform.forward*2);yield return CardScreenshot(Path.Combine(folder,"04-enterable-interior.png"));
   ClearRunObjects();TownCamera(new Vector3(20,0,5),10);TickTownInteriors(1);yield return CardScreenshot(Path.Combine(folder,"05-streets-and-walls.png"));
   TownCamera(Vector3.zero,48);cameraRig.transform.position=new Vector3(0,85,-65);cameraRig.transform.LookAt(Vector3.zero);var fog=cameraRig.GetComponent<VeilFogEffect>();fog.enabled=false;yield return CardScreenshot(Path.Combine(folder,"06-town-layout.png"));fog.enabled=true;
   ReturnHome();SelectExpeditionMap(0);StartRun();TownCamera(Vector3.zero,12);yield return CardScreenshot(Path.Combine(folder,"07-sanctuary-preserved.png"));
   File.WriteAllText(pass,"PASS: separate selectable town and default sanctuary; twelve roofless through-buildings; real wall collision, swept movement and attack blocking; both entrances traversable; large-body flow fields into every room; perimeter spawning; actual Elder movement through a doorway; indoor visual cutaways and restore; clean map switching without duplicate geometry or stale colliders; starting sanctuary renderer/obstacle counts preserved; silent capture.");Debug.Log("VEILBOUND_RUINED_TOWN_PASS");Application.Quit(0);
  }
 }
}
