#if UNITY_EDITOR
using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
namespace Veilbound.Editor {
 public static class VillageValidation {
  const string Output="VillageDesign/Implementation";
  [Serializable]class Result{public bool passed;public int buildings,openingsChecked,renderers,triangles,materials,reachableSpawnSamples;public float playerRadius,spawnRadius;public string[] buildingNames;}
  static void Need(bool ok,string label){if(!ok)throw new Exception("Village validation: "+label);}
  public static void Build(){Run();ProductionModelImport.Build();}
  public static void Run(){
   Directory.CreateDirectory(Output);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
   var root=WorldArt.Group("Approved low-poly Fallen Borough");var collision=new ArenaCollision();RuinedTownArena.Build(root,collision);var veil=new VeilBoundary(root,collision.PlayerRadius);var nav=new ArenaNavigation(collision);var result=new Result{buildings=RuinedTownArena.Rooms.Count,playerRadius=collision.PlayerRadius,spawnRadius=collision.Radius};
   Need(result.buildings==8&&result.playerRadius==52.5f&&result.spawnRadius==62.5f,"eight buildings and 25 percent larger radius");Need(collision.Free(Vector3.zero,1.12f),"clear central road junction");
   foreach(var room in RuinedTownArena.Rooms){
    var centre=room.transform.position;Need(collision.Free(centre,1.12f)&&nav.Reachable(centre,1.12f),room.name+" centre reachable");
    foreach(var direction in new[]{Vector3.forward,Vector3.back,Vector3.left,Vector3.right}){
     float extent=Mathf.Abs(direction.x)>.5f?room.width*.5f:room.depth*.5f;Vector3 a=centre+direction*(extent+1.6f),b=centre-direction*.5f;
     Need(collision.ClearLine(a,b,1.12f),room.name+" open floor-level breach "+direction);var moved=collision.Move(a,b-a,1.12f,collision.PlayerRadius);Need(Vector3.Distance(moved,b)<.04f,room.name+" swept body passes breach");result.openingsChecked++;
    }
    var wall=centre+new Vector3(room.width*.5f,0,room.depth*.5f*.78f);Need(!collision.Free(wall,.42f),room.name+" intact wall collision");
    nav.Tick(centre,1,true);var walker=centre-Vector3.forward*(room.depth/2+1.6f);for(int i=0;i<250&&Vector3.Distance(walker,centre)>.15f;i++)walker=nav.Move(walker,.12f,1.12f);Need(Vector3.Distance(walker,centre)<.2f,room.name+" largest enemy walks inside");nav.Tick(Vector3.zero,1,true);
    room.Tick(centre,1);var block=new MaterialPropertyBlock();room.walls[0].GetPropertyBlock(block);Need(block.GetFloat("_WallHeightScale")<.4f,"interior camera cutaway");room.Tick(centre+Vector3.right*100,1);
   }
   for(int i=0;i<96;i++){float a=i*Mathf.PI*2/96;Vector3 p=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*(collision.Radius-1.3f);if(collision.Free(p,1.12f)&&nav.Reachable(p,1.12f))result.reachableSpawnSamples++;}Need(result.reachableSpawnSamples>65,"perimeter enemy routes through Veil");
   var renderers=root.GetComponentsInChildren<MeshRenderer>();result.renderers=renderers.Length;result.triangles=root.GetComponentsInChildren<MeshFilter>().Sum(f=>f.sharedMesh.triangles.Length/3);result.materials=renderers.Select(r=>r.sharedMaterial).Distinct().Count();Need(result.renderers<180&&result.triangles<250000,"reasonable combined static geometry budget");result.buildingNames=RuinedTownArena.Rooms.Select(r=>r.name).ToArray();
   RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;RenderSettings.ambientLight=new Color(.43f,.44f,.45f);RenderSettings.fog=true;RenderSettings.fogColor=new Color(.075f,.083f,.075f);RenderSettings.fogStartDistance=100;RenderSettings.fogEndDistance=200;
   var sun=new GameObject("Village preview sun").AddComponent<Light>();sun.type=LightType.Directional;sun.intensity=1.25f;sun.color=new Color(1,.90f,.78f);sun.transform.rotation=Quaternion.Euler(48,-25,0);sun.shadows=LightShadows.Soft;
   Render(collision,"01-village-overview",new Vector3(0,78,-17),new Vector3(0,0,0),72,true,1920,1600);
   Render(collision,"02-layout-without-fog",new Vector3(0,78,-.01f),Vector3.zero,66,false,1800,1800);
   Render(collision,"03-church",new Vector3(28,22,-4),new Vector3(14,1,21),19,true,1400,1100);
   Render(collision,"04-hall-and-tavern",new Vector3(-3,25,-54),new Vector3(-21,1,-22),25,true,1400,1100);
   Render(collision,"05-crossroads-gameplay",new Vector3(9,18,-19),new Vector3(0,.5f,3),13,true,1600,1000);
   // Independent map data defaults ensure Sanctuary keeps its existing bounds.
   var sanctuary=new ArenaCollision();Need(sanctuary.Radius==50&&sanctuary.PlayerRadius==42,"Sanctuary default radii unchanged");
   result.passed=true;File.WriteAllText(Output+"/validation.json",JsonUtility.ToJson(result,true));Debug.Log("VEILBOUND_VILLAGE_VALIDATION_PASS "+JsonUtility.ToJson(result));
  }
  static void Render(ArenaCollision c,string name,Vector3 position,Vector3 target,float size,bool fog,int width,int height){
   var camera=new GameObject("Actual Unity village camera").AddComponent<Camera>();camera.enabled=false;camera.orthographic=true;camera.orthographicSize=size;camera.aspect=(float)width/height;camera.nearClipPlane=.1f;camera.farClipPlane=230;camera.transform.position=position;camera.transform.LookAt(target);camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.06f,.07f,.075f);camera.depthTextureMode=DepthTextureMode.Depth;if(fog)camera.gameObject.AddComponent<VeilFogEffect>().ConfigureBounds(c);
   var rt=new RenderTexture(width,height,24);camera.targetTexture=rt;camera.Render();var prior=RenderTexture.active;RenderTexture.active=rt;var image=new Texture2D(width,height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();File.WriteAllBytes(Output+"/"+name+".png",image.EncodeToPNG());RenderTexture.active=prior;rt.Release();UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(camera.gameObject);
  }
 }
}
#endif


