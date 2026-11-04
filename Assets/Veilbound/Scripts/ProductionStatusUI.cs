using System;using UnityEngine;
namespace Veilbound {public partial class VeilGame {
 Color ProductionStateColour(ProductionActivity state)=>state.tag=="BLOCKED"||state.tag=="NEEDS INPUTS"?new Color(.96f,.57f,.30f):state.tag=="PAUSED"?new Color(.83f,.64f,.36f):state.working||state.tag=="GATHERING"||state.tag=="SUPPLYING"?new Color(.60f,.79f,.56f):archiveMuted;
 void DrawProductionState(Rect rect,ProductionActivity state){
  float badge=Mathf.Min(114,rect.width*.28f);GUI.Label(new Rect(rect.x,rect.y,badge,21),state.tag=="NEEDS INPUTS"?"INPUTS":state.tag,ArchiveType(11,ProductionStateColour(state),true));GUI.Label(new Rect(rect.x+badge+7,rect.y,rect.width-badge-7,rect.height),state.detail,ArchiveType(12,archiveIvory));
 }
 void DrawQueueWorkflowSummary(){
  string[] ids={"mine","blacksmith","hunters","weaver","library","construction"},names={"MINE","BLACKSMITH","HUNTERS","TEXTILES","RESEARCH","CONSTRUCTION"};
  for(int i=0;i<ids.Length;i++){float x=68+(i%3)*489,y=138+(i/3)*36;ProductionActivity a;
   if(i<4)a=city.ProductionOverview(ids[i]);else if(i==4)a=new ProductionActivity(city.activeResearch==""?"READY":"WORKING",city.activeResearch==""?"Waiting for a research task":LibraryResearch.Names[LibraryResearch.Index(city.activeResearch)]+" · "+ProductionTime.Duration(city.researchRemaining));else a=new ProductionActivity(city.activeConstruction==""?"READY":"WORKING",city.activeConstruction==""?"Waiting for a construction task":CityBuildingName(city.activeConstruction)+" · "+ProductionTime.Duration(city.constructionRemaining));
   GUI.Label(new Rect(x,y,470,17),names[i]+" / "+a.tag,ArchiveType(10,ProductionStateColour(a),true));GUI.Label(new Rect(x,y+16,470,20),a.detail,ArchiveType(12,archiveIvory));
  }
 }
}}
