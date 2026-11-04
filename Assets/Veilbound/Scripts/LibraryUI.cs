using System;
using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  bool libraryResearchOpen;int researchTab;string selectedResearch="mine";int selectedResearchTier=2;Vector2 researchScroll;
  static readonly string[] ResearchTabs={"Resources","Crafting","Housing","Mysticism"};
  static readonly string[][] ResearchBranches={new[]{"timber","quarry","mine","hunters","weaver"},new[]{"blacksmith","weapons","plate","clothier"},new[]{"housing"},new string[0]};
  void CityLibraryDetails(){city.EnsureResearch();GUI.Label(new Rect(1100,380,450,96),"Independent technology paths unlock new building ranks and crafting materials. Research costs materials and active city time.",CityType(17,cityIvory));
   CityButton(1100,492,450,46,"Open Library research",()=>{libraryResearchOpen=true;SelectResearchTab(researchTab);selection=0;},city.library>0);
   string text=city.library==0?"Build the Library to begin. Starter buildings and Tin refining remain available.":city.activeResearch==""?"No active research. Choose your next technology.":LibraryResearch.Names[LibraryResearch.Index(city.activeResearch)]+" "+city.researchTarget+"\n"+city.researchRemaining.ToString("0.0")+" city hours remaining";
   GUI.Label(new Rect(1100,565,450,100),text,CityType(17,cityIvory));GUI.Label(new Rect(1100,685,450,90),"One research and one construction project can run together. Expeditions speed both up. Pausing or closing the game stops progress.",CityType(14,muted));
  }
  void ResearchScreen(){
   city.EnsureResearch();InitArchiveArt();ResourceBarInput();
   if(archiveBackdrop)GUI.DrawTexture(new Rect(0,0,1600,900),archiveBackdrop,ScaleMode.ScaleAndCrop);else Box(new Rect(0,0,1600,900),new Color(.025f,.023f,.022f));
   ArchivePlate(new Rect(46,132,1508,720),0,18);
   GUI.DrawTexture(new Rect(64,145,100,86),archiveBook,ScaleMode.ScaleToFit);
   GUI.Label(new Rect(184,147,900,46),"THE LIBRARY",ArchiveType(35,archiveIvory,true));
   GUI.Label(new Rect(186,193,800,25),"SETTLEMENT RESEARCH  /  THE ASHEN ARCHIVE",ArchiveType(13,archiveGold,true));
   ArchiveButton(new Rect(1038,155,240,38),"Priorities ("+(city.settlementQueue==null?0:city.settlementQueue.Count)+")",OpenSettlementQueue);
   ArchiveButton(new Rect(1290,155,240,38),"Back to settlement",()=>{libraryResearchOpen=false;LeaveResourceBar();SelectCityBuilding("library");});
   string active=city.activeResearch==""?"RESEARCH / Choose a technology":LibraryResearch.Names[LibraryResearch.Index(city.activeResearch)]+" "+city.researchTarget+" / "+city.researchRemaining.ToString("0.0")+" city hours remaining";
   GUI.Label(new Rect(186,228,880,23),active,ArchiveType(15,archiveIvory));
   ArchivePlate(new Rect(186,257,880,9),3,3);if(city.activeResearch!=""){float progress=1-(float)(city.researchRemaining/city.researchTotal);Box(new Rect(190,260,872*Mathf.Clamp01(progress),3),archiveRed);ArchiveButton(new Rect(1160,224,370,34),"Cancel research & refund",()=>{city.CancelResearch();Save();});}
   string construction=city.activeConstruction==""?"CONSTRUCTION / Idle":"CONSTRUCTION / "+CityBuildingName(city.activeConstruction)+" "+city.constructionTarget+" / "+city.constructionRemaining.ToString("0.0")+" city hours left";
   GUI.Label(new Rect(186,278,900,23),construction,ArchiveType(13,archiveMuted));if(city.activeConstruction!="")ArchiveButton(new Rect(1160,268,370,32),"Cancel construction & refund",()=>{city.CancelConstruction();Save();});
   for(int i=0;i<4;i++){int tree=i;ArchiveButton(new Rect(64+i*368,311,357,38),ResearchTabs[i],()=>SelectResearchTab(tree),true,researchTab==i);}
   DrawResearchTree();
   GUI.Label(new Rect(68,826,1440,20),"24 city hours = 1 city day. Expeditions accelerate research and construction. Cancelling refunds materials and discards elapsed work.",ArchiveType(12,archiveMuted));
   GUI.Label(new Rect(68,859,1460,18),"Wheel: scroll · Arrows / D-pad: select · Enter / A: research · LB / RB: change tree · Resources: open resource bar · Escape / B: back",ArchiveType(11,archiveIvory));
   ArchiveResourceHeader();
  }
 }
}