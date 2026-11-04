using System;using System.IO;using System.Collections;using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  IEnumerator VillageImplementationReview(){
   yield return null;ReturnHome();combatReviewFreeze=true;CheckRuinedTown();CheckRandomExpeditionMaps();ReturnHome();SelectExpeditionMap(1);StartRun();
   Require(collision.PlayerRadius==52.5f&&collision.Radius==62.5f,"Expanded village bounds active in actual game");var fog=cameraRig.GetComponent<VeilFogEffect>();Require(fog&&fog.playerRadius==52.5f&&fog.spawnRadius==62.5f,"Main gameplay camera uses village Veil perimeter");
   foreach(var room in RuinedTownArena.Rooms){position=room.transform.position;hero.root.position=position;TickTownInteriors(1);navigation.Tick(position,1,true);var elder=SpawnFoe(1);Require(elder!=null&&elder.t.position.magnitude>60,"Actual Elder enters enlarged map through Veil");}ClearRunObjects();
   position=Vector3.zero;hero.root.position=position;TownCamera(Vector3.zero,13);TickTownInteriors(1);yield return null;yield return null;
   string folder=Path.Combine(Application.dataPath,"..","Smoke","ExpandedVillage");Directory.CreateDirectory(folder);CaptureVillageCamera(Path.Combine(folder,"gameplay-crossroads.png"),1600,1000);
   cameraRig.transform.position=new Vector3(0,78,-17);cameraRig.transform.LookAt(Vector3.zero);cameraRig.orthographicSize=72;yield return null;CaptureVillageCamera(Path.Combine(folder,"actual-player-overview.png"),1920,1600);
   ReturnHome();SelectExpeditionMap(0);StartRun();Require(collision.PlayerRadius==42&&collision.Radius==50&&fog.playerRadius==42&&fog.spawnRadius==50,"Map switching restores Sanctuary collision, spawn and fog bounds");
   File.WriteAllText(Path.Combine(folder,"PASS.txt"),"PASS: eight enterable low-poly ruins; player/enemy collision and traversal; all enemy families; actual Elder indoor pursuit and perimeter spawns; random destination selection; enlarged village-specific 52.5/62.5m bounds and matching Veil fog; Sanctuary restored to 42/50m; actual game camera renders.");Debug.Log("VEILBOUND_EXPANDED_VILLAGE_RUNTIME_PASS");Application.Quit(0);
  }
  void CaptureVillageCamera(string path,int width,int height){var priorTarget=cameraRig.targetTexture;float aspect=cameraRig.aspect;var rt=new RenderTexture(width,height,24);cameraRig.targetTexture=rt;cameraRig.aspect=(float)width/height;cameraRig.Render();var prior=RenderTexture.active;RenderTexture.active=rt;var image=new Texture2D(width,height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());RenderTexture.active=prior;cameraRig.targetTexture=priorTarget;cameraRig.aspect=aspect;rt.Release();Destroy(rt);Destroy(image);}
 }
}
