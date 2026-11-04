using System;
using System.IO;
using System.Collections;
using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  void CheckSanctuaryArena() {
   Require(AudioListener.volume==0,"Arena tests stay silent");StartRun();
   Require(ArenaCollision.PlayerBoundary==42&&ArenaCollision.Boundary==50,"Playable radius grows from 30m to 42m, with a matching 50m enemy perimeter");
   Require(arena.Find("Batched hand-cut flagstones 0")&&arena.Find("Worn sanctuary compass inlay"),"New flagstone floor and matte bronze inlay are present");
   for(int x=-8;x<=8;x+=2)for(int z=-8;z<=8;z+=2)Require(collision.Free(new Vector3(x,0,z),1.12f),"Central fighting space stays clear even for Elder bodies");
   navigation.Tick(Vector3.zero,1,true);int free=0;
   for(int i=0;i<32;i++){float a=i*Mathf.PI/16;Vector3 p=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*40;if(collision.Free(p,1.2f)){free++;Require(navigation.Reachable(p,1.12f),"Large-enemy navigation reaches the expanded playable edge");}}
   Require(free>=20,"Expanded outer lanes retain broad accessible routes");
   foreach(int kind in new[]{0,1,2,3,4}){var f=SpawnFoe(0,kind);Require(f!=null&&f.t.position.magnitude>48.5f&&navigation.Reachable(f.t.position,f.radius),"Enemy family spawns from the new reachable perimeter");}
   var elder=SpawnFoe(1);Require(elder!=null&&navigation.Reachable(elder.t.position,elder.radius),"Elder can also enter the enlarged arena");
   for(int i=0;i<12;i++){float a=i*Mathf.PI/6;Vector3 dir=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));Vector3 stop=collision.Move(Vector3.zero,dir*80,.42f,ArenaCollision.PlayerBoundary);Require(stop.magnitude+.42f<=ArenaCollision.PlayerBoundary+.001f,"Dodge movement respects the larger boundary");}
   Require(arena.GetComponentsInChildren<Renderer>().Length<650,"Arena geometry stays within the presentation renderer budget and uses static batching");
   StartRun();Debug.Log("VEILBOUND_SANCTUARY_ARENA_LOGIC_PASS");
  }
  void ArenaCamera(Vector3 at,Vector3 look,float size){cameraRig.transform.position=at;cameraRig.transform.LookAt(look);cameraRig.orthographicSize=size;hero.root.position=position;hero.root.rotation=Quaternion.LookRotation(facing);hero.Animate(0,0,0);}
  IEnumerator SanctuaryArenaReview() {
   string folder=Path.Combine(Application.dataPath,"..","Smoke","SanctuaryArena");Directory.CreateDirectory(folder);string pass=Path.Combine(folder,"PASS.txt");if(File.Exists(pass))File.Delete(pass);
   yield return new WaitForSecondsRealtime(.5f);ReturnHome();combatReviewFreeze=true;CheckSanctuaryArena();StartRun();
   foreach(var p in new[]{new Vector3(-3,0,4),new Vector3(3,0,5)})ReadoutTarget(p);level=8;health=MaxHealth*.85f;
   ArenaCamera(new Vector3(0,20,-16),Vector3.zero,12);yield return CardScreenshot(Path.Combine(folder,"01-central-combat.png"));
   ClearRunObjects();position=new Vector3(8,0,11);ArenaCamera(position+new Vector3(5,12,-12),position+Vector3.up,8);yield return CardScreenshot(Path.Combine(folder,"02-braziers-and-stone.png"));
   position=new Vector3(22,0,14);ArenaCamera(position+new Vector3(7,13,-13),position+Vector3.up,9);yield return CardScreenshot(Path.Combine(folder,"03-ruined-columns.png"));
   position=collision.Resolve(new Vector3(30,0,6),.42f,ArenaCollision.PlayerBoundary);ArenaCamera(position+new Vector3(8,15,-15),position+new Vector3(1,0,2),10);yield return CardScreenshot(Path.Combine(folder,"04-arches-and-veil.png"));
   position=Vector3.zero;ArenaCamera(new Vector3(0,90,-65),Vector3.zero,52);var fog=cameraRig.GetComponent<VeilFogEffect>();fog.enabled=false;yield return CardScreenshot(Path.Combine(folder,"05-layout-overview.png"));fog.enabled=true;
   StartRun();position=new Vector3(0,0,40);hero.root.position=position;navigation.Tick(position,1,true);for(int i=0;i<12;i++)Require(SpawnFoe(0)!=null,"Spawning remains reliable near the expanded player boundary");
   Require(AudioListener.volume==0,"Arena capture stayed muted");File.WriteAllText(pass,"PASS: 42m player radius and 50m enemy perimeter; clear central combat space; expanded flow fields and Elder-sized paths; all enemy families spawn from reachable edges; reliable spawning near outer player positions; swept movement boundary; batched flagstones and bounded renderer count; screenshots of combat floor, braziers, ruins, arches, boundary and layout; silent testing.");Debug.Log("VEILBOUND_SANCTUARY_ARENA_PASS");Application.Quit(0);
  }
 }
}
