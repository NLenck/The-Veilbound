using System;using UnityEngine;using UnityEngine.InputSystem;
namespace Veilbound {public partial class VeilGame {
 bool workshopCostInspect;int workshopCostFocus;string workshopCostHoverName;double workshopCostHoverOwned,workshopCostHoverRequired;Rect workshopCostHoverRect;
 void ResetWorkshopCostInspection(){workshopCostInspect=false;workshopCostFocus=0;workshopCostHoverName=null;}
 bool ReadWorkshopCostInspection(int count){
  var key=Keyboard.current;var pad=PreferredPad;
  if(key?.tabKey.wasPressedThisFrame==true||pad?.rightStickButton.wasPressedThisFrame==true){workshopCostInspect=!workshopCostInspect;workshopCostFocus=0;return true;}
  if(!workshopCostInspect)return false;
  if(key?.escapeKey.wasPressedThisFrame==true||pad?.buttonEast.wasPressedThisFrame==true){ResetWorkshopCostInspection();return true;}
  int move=key?.rightArrowKey.wasPressedThisFrame==true||key?.downArrowKey.wasPressedThisFrame==true||pad?.dpad.right.wasPressedThisFrame==true||pad?.dpad.down.wasPressedThisFrame==true?1:key?.leftArrowKey.wasPressedThisFrame==true||key?.upArrowKey.wasPressedThisFrame==true||pad?.dpad.left.wasPressedThisFrame==true||pad?.dpad.up.wasPressedThisFrame==true?-1:0;
  workshopCostFocus=(workshopCostFocus+move+Math.Max(1,count))%Math.Max(1,count);return true;
 }
 void WorkshopIngredientIcon(Rect rect,int family,int index){
  if(family==4||family==5){DrawProcessedMaterial(rect,family-4,index);return;}
  string name=family==7?"essence":family==6?index==0?"timber":"stone":family==3?"group-bars":ResourceIconNames[family>=4?family-3:family][index];var icon=MaterialIcon(name);if(!icon)return;
  Color previous=GUI.color;GUI.color=family==3?SmithTint(index):Color.white;GUI.DrawTexture(rect,icon,ScaleMode.ScaleToFit);GUI.color=previous;
 }
 void DrawWorkshopCostIcons(Rect area,ResourceCost[] recipe,double essence=0,Vector2 tooltipOffset=default(Vector2)){
  int count=recipe.Length+(essence>0?1:0);float step=Mathf.Min(80,area.width/Math.Max(1,count));
  for(int i=0;i<count;i++){
   var part=i<recipe.Length?recipe[i]:new ResourceCost(7,0,essence);double owned=city.PriorityResourceStock(part.family,part.index);Rect rect=new Rect(area.x+i*step,area.y,step-8,64);
   bool hover=resourceCategory<0&&!resourceFocus&&(rect.Contains(Event.current.mousePosition)||workshopCostInspect&&workshopCostFocus==i);
   if(hover)CardOutline(rect,archiveGold);if(workshopCostInspect&&workshopCostFocus==i&&resourceCategory<0&&!resourceFocus)DrawMenuNavigationFocus(rect,"Ingredient");
   WorkshopIngredientIcon(new Rect(rect.center.x-22,rect.y+3,44,38),part.family,part.index);GUI.Label(new Rect(rect.x,rect.y+43,rect.width,20),part.amount.ToString("0.##"),ArchiveType(14,owned+1e-9>=part.amount?archiveIvory:archiveRed,true,TextAnchor.MiddleCenter));
   if(hover){workshopCostHoverName=city.PriorityResourceName(part.family,part.index);workshopCostHoverOwned=owned;workshopCostHoverRequired=part.amount;workshopCostHoverRect=new Rect(rect.position+tooltipOffset,rect.size);}
  }
 }
 void DrawWorkshopCostTooltip(){
  if(string.IsNullOrEmpty(workshopCostHoverName)||resourceCategory>=0||resourceFocus)return;
  Rect rect=new Rect(Mathf.Clamp(workshopCostHoverRect.center.x-142,30,1286),workshopCostHoverRect.y-110,284,100);ArchivePlate(rect,0,12);
  GUI.Label(new Rect(rect.x+14,rect.y+9,rect.width-28,37),workshopCostHoverName,ArchiveType(15,archiveIvory,true));
  GUI.Label(new Rect(rect.x+14,rect.y+48,rect.width-28,20),"Required: "+workshopCostHoverRequired.ToString("N0"),ArchiveType(12,archiveMuted));
  GUI.Label(new Rect(rect.x+14,rect.y+71,rect.width-28,20),"Owned: "+workshopCostHoverOwned.ToString("N0"),ArchiveType(13,workshopCostHoverOwned+1e-9>=workshopCostHoverRequired?archiveIvory:archiveRed));
 }
}}
