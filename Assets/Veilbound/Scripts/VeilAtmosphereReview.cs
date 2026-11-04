using System;using System.IO;using System.Linq;using System.Collections;using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  [Serializable]class AtmosphereAudit{public bool passed;public int maps,graveyardEmbers,villageEmbers;public bool mistCapped,drifting,stableMesh,noColliders;}
  IEnumerator VeilAtmosphereReview(){
   yield return null;combatReviewFreeze=true;var audit=new AtmosphereAudit();string output=Path.Combine(Application.dataPath,"..","Smoke","VeilAtmosphere");Directory.CreateDirectory(output);
   // Check the live threshold, XP carry-over and the actual level-up path.
   ReturnHome();SelectExpeditionMap(1);StartRun();Require(level==1&&Threshold==25,"first level costs 25 XP");
   xp=63;spawnClock=100;Step(0);Require(level==2&&xp==38&&Threshold==35&&state==ScreenState.Choice,"first level subtracts 25 and retains surplus");
   SetState(ScreenState.Expedition);Step(0);Require(level==3&&xp==3&&Threshold==45&&state==ScreenState.Choice,"surplus buys next level using increasing cost");
   for(int i=1;i<=100;i++)Require(ExperienceRequired(i)==25+10*(i-1),"XP curve grows consistently");
   foreach(int map in new[]{0,1}){
    ReturnHome();SelectExpeditionMap(map);StartRun();var mist=cameraRig.GetComponent<VeilFogEffect>();var ambient=veil.Ambient;Require(ambient&&ambient.Count==(map==0?192:300),"sparse emitter uses map-size budget");Require(mist.interiorMistOpacity<=.35f&&mist.interiorMistHeight==2.2f&&mist.playerRadius==collision.PlayerRadius,"mist remains low and capped with correct map bounds");audit.mistCapped=true;
    var crystals=arena.GetComponentsInChildren<MeshRenderer>().Where(r=>r.sharedMaterial&&r.sharedMaterial.HasProperty("_Crystal")&&r.sharedMaterial.GetFloat("_Crystal")>.5f).ToArray();Require(crystals.Length>0,"environment dark ruby crystals present");foreach(var crystal in crystals)Require(crystal.sharedMaterial.color.r<.2f&&crystal.sharedMaterial.GetColor("_Emission").r>.3f,"dark crystal body and crimson emission");
    int mesh=ambient.Mesh.GetInstanceID(),obstacles=collision.obstacles.Count,walls=collision.walls.Count;Vector3 before=ambient.Mesh.vertices[0];veil.Tick(3.5f);Require(Vector3.Distance(before,ambient.Mesh.vertices[0])>.1f,"embers drift with Veil time");audit.drifting=true;Require(ambient.Mesh.GetInstanceID()==mesh&&ambient.Mesh.vertexCount==ambient.Count*4,"one reusable emitter mesh");audit.stableMesh=true;Require(ambient.GetComponentsInChildren<Collider>().Length==0&&collision.obstacles.Count==obstacles&&collision.walls.Count==walls,"ambient effects cannot block navigation");audit.noColliders=true;
    Require(ambient.Mesh.vertices.All(v=>float.IsFinite(v.x)&&float.IsFinite(v.y)&&float.IsFinite(v.z)),"all mote positions finite");Require(ambient.GetComponent<MeshRenderer>().sharedMaterial.shader.name=="Veilbound/AmbientEmbers","intended ember shader included in player");
    position=Vector3.zero;hero.root.position=position;TownCamera(Vector3.zero,12);TickTownInteriors(1);yield return null;yield return null;
    var renderer=ambient.GetComponent<MeshRenderer>();mist.interiorMistDensity=0;renderer.enabled=false;CaptureVillageCamera(Path.Combine(output,map==0?"Graveyard-before.png":"Village-before.png"),1600,1000);
    mist.interiorMistDensity=.28f;renderer.enabled=true;CaptureVillageCamera(Path.Combine(output,map==0?"Graveyard-mist-embers.png":"Village-mist-embers.png"),1600,1000);
    TownCamera(map==0?new Vector3(0,0,3.7f):new Vector3(6.8f,0,17),5);CaptureVillageCamera(Path.Combine(output,map==0?"Graveyard-crystal.png":"Village-crystal.png"),1200,1000);
    if(map==0)audit.graveyardEmbers=ambient.Count;else audit.villageEmbers=ambient.Count;audit.maps++;
   }
   ReturnHome();SelectExpeditionMap(0);StartRun();Require(arena.GetComponentsInChildren<VeilAmbientEmbers>().Length==1&&veil.Ambient.Count==192,"map switching never duplicates emitters");audit.passed=true;File.WriteAllText(Path.Combine(output,"validation.json"),JsonUtility.ToJson(audit,true));Debug.Log("VEILBOUND_AMBIENT_ATMOSPHERE_RUNTIME_PASS "+JsonUtility.ToJson(audit));Application.Quit(0);
  }
 }
}
