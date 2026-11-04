using System;using System.IO;using System.Collections;using System.Collections.Generic;using UnityEngine;
namespace Veilbound {public partial class VeilGame {
 bool pointerScaleReviewOnly;
 IEnumerator PointerScaleReview(){
  yield return new WaitForSecondsRealtime(.5f);combatReviewFreeze=true;city=FreshCharacterTown();city.InitializeWoodenStarter();RebuildHero();PressureStart(1,0);Require(SpawnRelic()&&SpawnRelic(),"Real relic spawns");
  while(relics.Count<8){var model=Instantiate(relics[0].root.gameObject);relics.Add(new Relic{root=model.transform,point=Vector3.zero});}
  position=Vector3.zero;hero.root.position=position;relicMessageUntil=0;
  string folder=Path.Combine(Application.dataPath,"..","Smoke","PointerScale");Directory.CreateDirectory(folder);int checkedPixels=0;var records=new List<string>();
  foreach(var size in new[]{new Vector2Int(1600,900),new Vector2Int(2520,1176),new Vector2Int(1280,960),new Vector2Int(2560,1080)}){
   Screen.SetResolution(size.x,size.y,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.7f);
   foreach(Vector3 player in new[]{Vector3.zero,new Vector3(-24,0,24),new Vector3(-24,0,-24),new Vector3(24,0,24),new Vector3(24,0,-24)}){
   position=player;hero.root.position=player;
   foreach(float yaw in new[]{0f,90f}){
    cameraRig.aspect=Screen.width/(float)Screen.height;cameraRig.orthographicSize=12;cameraRig.transform.SetPositionAndRotation(player+Quaternion.Euler(0,yaw,0)*new Vector3(0,20,-16),Quaternion.Euler(51,yaw,0));
    for(int i=0;i<8;i++){float angle=i*Mathf.PI/4;relics[i].point=player+new Vector3(Mathf.Cos(angle)*20,0,Mathf.Sin(angle)*20);relics[i].root.position=relics[i].point;}
    int oldMask=cameraRig.cullingMask;var oldFlags=cameraRig.clearFlags;var oldColor=cameraRig.backgroundColor;cameraRig.cullingMask=0;cameraRig.clearFlags=CameraClearFlags.SolidColor;cameraRig.backgroundColor=Color.black;pointerScaleReviewOnly=true;
    yield return null;yield return new WaitForEndOfFrame();var tex=ScreenCapture.CaptureScreenshotAsTexture();BuildRelicPointers();
    float scale=Mathf.Min(Screen.width/1600f,Screen.height/900f);Vector2 offset=new Vector2((Screen.width-1600*scale)/2,(Screen.height-900*scale)/2);Vector2 centre=WorldToHudPoint(position+Vector3.up*.65f);
    var counts=new int[8];var pixels=tex.GetPixels32();
    for(int y=0;y<tex.height;y++)for(int x=0;x<tex.width;x++){
     var c=pixels[y*tex.width+x];if(c.r<180||c.g<140||c.b>90)continue;
     Vector2 hud=(new Vector2(x+.5f,tex.height-y-.5f)-offset)/scale;int nearest=-1;float distance=9999;
     for(int i=0;i<8;i++){float d=Vector2.Distance(hud,relicPointers[i].point);if(d<distance){distance=d;nearest=i;}}
     Require(distance<18,"Rendered yellow arrow pixels stay at their player-centred anchor at "+Screen.width+"x"+Screen.height);counts[nearest]++;checkedPixels++;
    }
    for(int i=0;i<8;i++)Require(counts[i]>8,"All eight rendered bearings are present at "+Screen.width+"x"+Screen.height);
    string label=Screen.width+"x"+Screen.height+"-player"+player.x+"_"+player.z+"-yaw"+yaw;File.WriteAllBytes(Path.Combine(folder,label+".png"),tex.EncodeToPNG());records.Add(label);Destroy(tex);
    pointerScaleReviewOnly=false;cameraRig.cullingMask=oldMask;cameraRig.clearFlags=oldFlags;cameraRig.backgroundColor=oldColor;
   }
  }
  }
  position=Vector3.zero;hero.root.position=position;
  Screen.SetResolution(2520,1176,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.7f);cameraRig.aspect=Screen.width/(float)Screen.height;cameraRig.transform.SetPositionAndRotation(new Vector3(0,20,-16),Quaternion.Euler(51,0,0));
  for(int i=relics.Count-1;i>=2;i--){Destroy(relics[i].root.gameObject);relics.RemoveAt(i);}relics[0].point=Vector3.left*20;relics[1].point=Vector3.forward*20;foreach(var relic in relics)relic.root.position=relic.point;
  yield return null;yield return CardScreenshot(Path.Combine(folder,"gameplay-2520x1176.png"));
  Require(CollectRelic(0),"Relic pickup remains functional");BuildRelicPointers();Require(relicPointers.Count==1,"Collected pointer disappears");
  File.WriteAllText(Path.Combine(folder,"PASS.txt"),"PASS: actual rendered arrow pixels stay on player-centred anchors at the centre and all four map corners for eight bearings, two camera rotations, four resolutions (including 2520x1176); pickup cleanup; images: "+string.Join(", ",records)+"; yellow pixels checked: "+checkedPixels);Debug.Log("VEILBOUND_POINTER_SCALE_PASS pixels="+checkedPixels);Application.Quit(0);
 }
}}
