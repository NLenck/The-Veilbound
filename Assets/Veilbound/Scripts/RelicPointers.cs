using System.Collections.Generic;
using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  const float RelicPointerRadius=125f;
  Texture2D relicPointerIcon;
  struct RelicPointer {public Vector2 point,direction;public float radius;}
  readonly List<RelicPointer> relicPointers=new List<RelicPointer>();
  Vector2 WorldToHudPoint(Vector3 world){
   var screen=cameraRig.WorldToScreenPoint(world);float scale=Mathf.Min(Screen.width/1600f,Screen.height/900f);
   return new Vector2((screen.x-(Screen.width-1600*scale)*.5f)/scale,(Screen.height-screen.y-(Screen.height-900*scale)*.5f)/scale);
  }
  void BuildRelicPointers(){
   relicPointers.Clear();Vector2 centre=WorldToHudPoint(position+Vector3.up*.65f);
   foreach(var relic in relics){
    Vector2 direction=WorldToHudPoint(relic.point+Vector3.up*.65f)-centre;
    direction=direction.sqrMagnitude>.0001f?direction.normalized:Vector2.up;
    float angle=Mathf.Atan2(direction.y,direction.x);Vector2 anchor=direction;
    // Spread matching bearings along the same invisible circle, never away from it.
    for(int attempt=0;attempt<64;attempt++){
     float offset=attempt==0?0:((attempt+1)/2)*.22f*(attempt%2==1?1:-1);
     anchor=new Vector2(Mathf.Cos(angle+offset),Mathf.Sin(angle+offset));bool occupied=false;
     foreach(var previous in relicPointers)if(Vector2.Distance(centre+anchor*RelicPointerRadius,previous.point)<26){occupied=true;break;}
     if(!occupied)break;
    }
    relicPointers.Add(new RelicPointer{point=centre+anchor*RelicPointerRadius,direction=direction,radius=RelicPointerRadius});
   }
  }

  Texture2D RelicPointerIcon(){
   if(relicPointerIcon)return relicPointerIcon;
   return relicPointerIcon=HudTexture("Relic direction pointer",64,(u,v)=>{
    float x=(u-.5f)*2,y=(v-.5f)*2;float edge=.40f-.70f*Mathf.Abs(y);
    bool outer=Mathf.Abs(y)<.88f&&Mathf.Abs(x-edge)<.14f;
    bool inner=Mathf.Abs(y)<.81f&&Mathf.Abs(x-edge)<.075f;
    return inner?new Color(1,.86f,.12f):outer?new Color(.20f,.12f,.015f,.95f):Color.clear;
   });
  }
  void DrawRelicGuidance(){
   if(state!=ScreenState.Expedition&&state!=ScreenState.Paused)return;
   if(elapsed<relicMessageUntil)GUI.Label(new Rect(420,290,760,54),relicMessage,CardStyle(16,WorldArt.Gold,TextAnchor.UpperCenter));
   BuildRelicPointers();
   foreach(var pointer in relicPointers){
    var matrix=GUI.matrix;float angle=Mathf.Atan2(pointer.direction.y,pointer.direction.x)*Mathf.Rad2Deg;
    // Compose in HUD space before applying the screen scale and letterbox offset.
    // RotateAroundPivot pre-multiplies a screen-space rotation around a HUD-space
    // pivot, displacing arrows whenever the HUD scale is not one.
    GUI.matrix=matrix*Matrix4x4.TRS(pointer.point,Quaternion.Euler(0,0,angle),Vector3.one);
    GUI.DrawTexture(new Rect(-12,-12,24,24),RelicPointerIcon());GUI.matrix=matrix;
   }
  }
 }
}
