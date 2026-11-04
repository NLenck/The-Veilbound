#if UNITY_EDITOR
using System;using System.IO;using System.Linq;using UnityEngine;using UnityEditor.SceneManagement;
namespace Veilbound.Editor {
 public static class GraveyardValidation {
  const string Output="GraveyardDesign/Implementation";
  [Serializable]class Result{public bool passed;public int braziers,headstones,plots=9,cathedrals,cathedralBreaches,renderers,triangles,reachableSpawnSamples;public float playerRadius,spawnRadius;}
  static void Need(bool ok,string why){if(!ok)throw new Exception("Graveyard: "+why);}
  public static void Build(){Run();ProductionModelImport.Build();}
  public static void Run(){
   Directory.CreateDirectory(Output);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);var root=WorldArt.Group("Graveyard");var c=new ArenaCollision();GraveyardArena.Build(root,c);var veil=new VeilBoundary(root,c.PlayerRadius);var nav=new ArenaNavigation(c);var result=new Result{headstones=GraveyardArena.GravePositions.Count,cathedrals=GraveyardArena.Rooms.Count,playerRadius=c.PlayerRadius,spawnRadius=c.Radius};
   result.braziers=root.GetComponentsInChildren<GraveyardBrazierFlame>().Length;Need(result.braziers==6,"six braziers");foreach(var p in GraveyardDressing.BrazierPositions)Need(!c.Free(p,.42f),"brazier collision");Need(result.headstones==27&&result.cathedrals==1,"quiet grave plots and exactly one cathedral");Need(c.Free(Vector3.zero,1.12f),"clear player spawn");
   foreach(var grave in GraveyardArena.GravePositions){Need(!c.Free(grave,.42f),"headstone has collision");var lane=grave+Vector3.right*1.9f;Need(c.Free(lane,1.12f)&&nav.Reachable(lane,1.12f),"large-body passage beside grave markers");}
   foreach(var p in new[]{new Vector3(0,0,13),new Vector3(0,0,-13),new Vector3(13,0,0),new Vector3(-13,0,0)})Need(nav.Reachable(p,1.12f),"open cardinal shrine approach");
   foreach(int side in new[]{-1,1})Need(c.ClearLine(new Vector3(side*2.7f,0,-2),new Vector3(side*2.7f,0,12),1.12f),"wide lanes around central altar");
   var room=GraveyardArena.Rooms[0];Vector3 centre=room.transform.position;Need(centre.x<0&&centre.z<0&&nav.Reachable(centre,1.12f),"cathedral southwest centre reachable");
   Vector3[] approach={centre+new Vector3(0,0,room.depth/2+1.6f),centre-new Vector3(0,0,room.depth/2+1.6f),centre+new Vector3(room.width/2+1.6f,0,-4),centre+new Vector3(-room.width/2-1.6f,0,-4)};
   for(int i=0;i<approach.Length;i++){Vector3 goal=centre+(i>=2?Vector3.back*4:Vector3.zero);Need(c.ClearLine(approach[i],goal,1.12f),"cathedral ground-level breach "+i);Need(Vector3.Distance(c.Move(approach[i],goal-approach[i],1.12f,c.PlayerRadius),goal)<.05f,"swept body crosses cathedral entrance");result.cathedralBreaches++;}
   for(int i=0;i<96;i++){float a=i*Mathf.PI/48;Vector3 p=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*(c.Radius-1.3f);if(c.Free(p,1.12f)&&nav.Reachable(p,1.12f))result.reachableSpawnSamples++;}Need(result.reachableSpawnSamples>80,"broad connected enemy perimeter");
   result.renderers=root.GetComponentsInChildren<MeshRenderer>().Length;result.triangles=root.GetComponentsInChildren<MeshFilter>().Sum(f=>f.sharedMesh.triangles.Length/3);Need(result.renderers<70&&result.triangles<50000,"clean low-poly scene budget");
   RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;RenderSettings.ambientLight=new Color(.39f,.41f,.41f);RenderSettings.fog=true;RenderSettings.fogColor=new Color(.075f,.083f,.075f);RenderSettings.fogStartDistance=90;RenderSettings.fogEndDistance=180;var sun=new GameObject("Graveyard soft key").AddComponent<Light>();sun.type=LightType.Directional;sun.intensity=1.1f;sun.color=new Color(.95f,.97f,1);sun.transform.rotation=Quaternion.Euler(48,-28,0);sun.shadows=LightShadows.Soft;
   Render(c,"01-graveyard-overview",new Vector3(0,70,-20),Vector3.zero,59,1800,1600);
   Render(c,"02-shrine-gameplay",new Vector3(8,17,-18),new Vector3(0,.5f,1),12,1600,1000);
   Render(c,"03-cathedral",centre+new Vector3(15,19,-21),centre+Vector3.up,17,1500,1100);
   room.Tick(centre,1);var block=new MaterialPropertyBlock();room.walls[0].GetPropertyBlock(block);Need(block.GetFloat("_WallHeightScale")<.4f,"cathedral cutaway");Render(c,"04-cathedral-cutaway",centre+new Vector3(9,15,-16),centre+Vector3.up,12,1500,1100);room.Tick(centre+Vector3.right*100,1);
   result.passed=true;File.WriteAllText(Output+"/validation.json",JsonUtility.ToJson(result,true));Debug.Log("VEILBOUND_GRAVEYARD_VALIDATION_PASS "+JsonUtility.ToJson(result));
  }
  static void Render(ArenaCollision c,string name,Vector3 position,Vector3 target,float size,int width,int height){var camera=new GameObject("Actual Unity Graveyard preview").AddComponent<Camera>();camera.enabled=false;camera.orthographic=true;camera.orthographicSize=size;camera.aspect=(float)width/height;camera.nearClipPlane=.1f;camera.farClipPlane=200;camera.transform.position=position;camera.transform.LookAt(target);camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.06f,.07f,.07f);camera.gameObject.AddComponent<VeilFogEffect>().ConfigureBounds(c);var rt=new RenderTexture(width,height,24);camera.targetTexture=rt;camera.Render();var previous=RenderTexture.active;RenderTexture.active=rt;var image=new Texture2D(width,height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();File.WriteAllBytes(Output+"/"+name+".png",image.EncodeToPNG());RenderTexture.active=previous;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(camera.gameObject);}
 }
}
#endif

