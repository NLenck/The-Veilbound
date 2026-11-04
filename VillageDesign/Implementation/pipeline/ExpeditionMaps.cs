using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  int selectedMap,activeMap;
  System.Random expeditionMapRandom=new System.Random();bool testRandomExpeditionMaps;
  void RollExpeditionMap(){if(!smoke||testRandomExpeditionMaps)selectedMap=expeditionMapRandom.Next(2);}
  static string ExpeditionMapName(int id)=>id==1?"The Fallen Borough":"Sanctuary Ruins";
  void SelectExpeditionMap(int id){if(smoke&&state==ScreenState.Settlement)selectedMap=Mathf.Clamp(id,0,1);}
  void EnsureExpeditionMap(){
   if(activeMap==selectedMap)return;
   if(arena){arena.gameObject.SetActive(false);Destroy(arena.gameObject);}
   collision.obstacles.Clear();collision.walls.Clear();collision.Radius=ArenaCollision.Boundary;collision.PlayerRadius=ArenaCollision.PlayerBoundary;
   arena=WorldArt.Group(selectedMap==0?"The Hollow Court":"The Fallen Borough");
   if(selectedMap==0)WorldArt.Arena(arena,collision);else RuinedTownArena.Build(arena,collision);
   veil=new VeilBoundary(arena,collision.PlayerRadius);cameraRig.GetComponent<VeilFogEffect>().ConfigureBounds(collision);navigation=new ArenaNavigation(collision);activeMap=selectedMap;
  }
  void TickTownInteriors(float dt){if(activeMap!=1)return;foreach(var room in RuinedTownArena.Rooms)if(room)room.Tick(position,dt);}
  void RandomDestinationNotice(){Rect r=new Rect(52,302,832,53);HudFrame(r);GUI.Label(new Rect(66,309,44,36),"?",CardStyle(27,hudLight,TextAnchor.MiddleCenter,true));GUI.Label(new Rect(122,307,740,24),"Random destination",CardStyle(18,parchment,bold:true));GUI.Label(new Rect(122,333,740,18),"Sanctuary Ruins or The Fallen Borough / revealed when you enter",CardStyle(11,muted));}

 }
}
