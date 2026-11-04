using System;using System.Linq;using System.IO;using System.Collections;using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  IEnumerator VillageImplementationReview(){
   yield return null;ReturnHome();combatReviewFreeze=true;CheckRuinedTown();ReturnHome();SelectExpeditionMap(1);StartRun();Require(!shortRun&&tier==Mathf.Max(1,VeilDepth),"Village uses the current standard modifier difficulty");
   Require(collision.PlayerRadius==60f&&collision.Radius==71f,"Expanded village bounds active in actual game");var fog=cameraRig.GetComponent<VeilFogEffect>();Require(fog&&fog.playerRadius==60f&&fog.spawnRadius==71f,"Main gameplay camera uses village Veil perimeter");
   foreach(var room in RuinedTownArena.Rooms){position=room.transform.position;hero.root.position=position;TickTownInteriors(1);navigation.Tick(position,1,true);var elder=SpawnFoe(1);Require(elder!=null&&!EnemySpawnPlacement.IsVisible(cameraRig,elder.t),"Actual Elder enters nearby through the Veil");}ClearRunObjects();
   position=Vector3.zero;hero.root.position=position;TownCamera(Vector3.zero,13);TickTownInteriors(1);yield return null;yield return null;
   string folder=Path.Combine(Application.dataPath,"..","Smoke","ExpandedVillage");Directory.CreateDirectory(folder);CaptureVillageCamera(Path.Combine(folder,"gameplay-crossroads.png"),1600,1000);
   cameraRig.transform.position=new Vector3(0,78,-17);cameraRig.transform.LookAt(Vector3.zero);cameraRig.orthographicSize=82;yield return null;CaptureVillageCamera(Path.Combine(folder,"actual-player-overview.png"),1920,1600);
   Require(arena.GetComponentsInChildren<MeshRenderer>().Count(r=>r.sharedMaterial&&r.sharedMaterial.shader.name=="Veilbound/VillageTextured")==136,"All solid village parts use the approved texture pass");
   Require(arena.Find("Low drifting dark clouds around crystal clusters").GetComponent<MeshRenderer>().sharedMaterial.shader.name=="Veilbound/CrystalCloud","Crystal clouds keep their original transparent shader");Require(arena.Find("Dark crimson crystal ground glow").GetComponent<MeshRenderer>().sharedMaterial.shader.name=="Veilbound/EnvironmentCrystalGlow","Crystal ground glow stays transparent");
   position=new Vector3(14,0,21);hero.root.position=position;TownCamera(position,12);TickTownInteriors(1);yield return null;CaptureVillageCamera(Path.Combine(folder,"textured-church.png"),1600,1000);
   position=new Vector3(52,0,0);hero.root.position=position;TownCamera(position,15);TickTownInteriors(1);veil.Tick(8);yield return null;CaptureVillageCamera(Path.Combine(folder,"crimson-fog-boundary.png"),1600,1000);
   ReturnHome();SelectExpeditionMap(0);StartRun();Require(collision.PlayerRadius==48&&collision.Radius==58&&fog.playerRadius==48&&fog.spawnRadius==58,"Map switching restores Sanctuary collision, spawn and fog bounds");
   File.WriteAllText(Path.Combine(folder,"PASS.txt"),"PASS: ten enterable low-poly ruins; player/enemy collision and traversal; all enemy families; actual Elder indoor pursuit and perimeter spawns; random destination selection; enlarged village-specific 60/71m bounds and matching Veil fog; Sanctuary restored to 48/58m; actual game camera renders.");Debug.Log("VEILBOUND_EXPANDED_VILLAGE_RUNTIME_PASS");Application.Quit(0);
  }
  void CaptureVillageCamera(string path,int width,int height){var priorTarget=cameraRig.targetTexture;float aspect=cameraRig.aspect;var rt=new RenderTexture(width,height,24);cameraRig.targetTexture=rt;cameraRig.aspect=(float)width/height;cameraRig.Render();var prior=RenderTexture.active;RenderTexture.active=rt;var image=new Texture2D(width,height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());RenderTexture.active=prior;cameraRig.targetTexture=priorTarget;cameraRig.aspect=aspect;rt.Release();Destroy(rt);Destroy(image);}
 }
}



