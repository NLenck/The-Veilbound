using System;
using UnityEngine;
namespace Veilbound {public partial class VeilGame {
 void OpenSlotResearch(int source){producerOpen=false;libraryResearchOpen=true;SelectResearchTab(0);selectedResearch=Settlement.SlotResearchBranch(source);selectedResearchTier=Math.Min(3,city.ResearchRank(selectedResearch)+1);researchScroll.y=Mathf.Clamp((selectedResearchTier*3-3)*88,0,510);selection=0;}
 void DrawSlotResearchNode(string parent,int rank,float x,float y){if(parent=="clothier"){DrawMaterialSlotResearchNodes(rank,x,y);return;}
  int source=parent=="mine"?0:parent=="hunters"?1:parent=="weaver"?2:3;string id=Settlement.SlotResearchBranch(source);bool known=city.ResearchRank(id)>=rank,active=city.activeResearch==id&&city.researchTarget==rank,chosen=selectedResearch==id&&selectedResearchTier==rank;
  Box(new Rect(x+29,y+26,29,2),known?archiveGold:archiveMuted);ArchiveButton(new Rect(x+59,y+10,36,36),"+1",()=>{selectedResearch=id;selectedResearchTier=rank;},true,chosen,active?5:known?4:3);
  if(chosen&&!resourceFocus&&resourceCategory<0)DrawMenuNavigationFocus(new Rect(x+59,y+10,36,36),"Production slot research");
  GUI.Label(new Rect(x+46,y+49,70,29),"Slot "+(rank+1),ArchiveType(10,known?archiveGold:archiveMuted,false,TextAnchor.UpperCenter));
 }
 void DrawSlotResearchDetails(){
  string id=selectedResearch;int source=Settlement.SlotResearchSource(id),rank=Mathf.Clamp(selectedResearchTier,1,3),milestone=rank*3;selectedResearchTier=rank;
  ArchivePlate(new Rect(1140,360,390,451),0,17);GUI.Label(new Rect(1165,385,340,69),ResearchShortName(id).ToUpper(),ArchiveType(22,archiveIvory,true));ArchiveRule(1164,460,342);
  GUI.Label(new Rect(1165,478,340,60),"UNLOCK PRODUCTION SLOT "+(rank+1)+"\nOne additional parallel worker",ArchiveType(18,archiveGold,true));
  var cost=LibraryResearch.Cost(id,rank);GUI.Label(new Rect(1165,550,340,75),city.TechnologyCostText(cost),ArchiveType(14,archiveIvory));
  GUI.Label(new Rect(1165,641,340,62),"Requires "+ResearchShortName(source==0?"mine":source==1?"hunters":source==2?"weaver":source==3?"blacksmith":"clothier")+" research tier "+milestone+" and the previous slot unlock.\nTime: "+LibraryResearch.Duration(id,rank)+" city hours",ArchiveType(13,archiveMuted));
  bool known=city.ResearchRank(id)>=rank;string reason=known?"Learned. The slot is available in this building.":city.ResearchRequirement(id);GUI.Label(new Rect(1165,710,340,34),reason,ArchiveType(12,archiveMuted));
  ArchiveButton(new Rect(1161,755,348,39),known?"Learned":"Queue slot research",()=>QueueGoal(1,id,target:rank),!known);
 }
 string SlotMode(int source,int index){
  var state=city.ProducerSlots(source)[index];if(source==0&&index==0){if(city.mineOrders.Count>0)return "Manual work order";if(city.PrimaryAssignedMineResource>=0)return "Manual assignment";if(city.ManualMineScheduleReady)return "Manual repeat";return city.dependencyMineResource>=0?"Automatic city demand":city.AutoMineReady?"Fallback repeat":"Automatic / idle";}
  if(index==0&&source>0)return city.UsedCapacity(source)>0?"Manual capacity allocation":"Automatic city demand";
  return state.resource>=0?"Manual assignment":state.mode==2?"Manual work order":state.mode==3?"Automatic city demand":"Automatic / idle";
 }
 int SlotResource(int source,int index){
  if(index>0)return city.ProducerSlots(source)[index].activeResource;
  if(source==0)return city.ActiveMineResource;int first=city.PlannedGatherResource(source);if(city.UsedCapacity(source)>0){for(int i=0;i<Settlement.ResourceNames[source].Length;i++)if(city.Allocation(source)[i]>0)return i;}return first;
 }
 void DrawMineProductionSlots(){
  int count=city.ProductionSlotCount(0);GUI.Label(new Rect(427,364,406,28),"PRODUCTION SLOTS / "+count+" OF 4",ArchiveType(17,archiveGold,true));ProducerButton(new Rect(865,363,203,26),"Research slots",()=>OpenSlotResearch(0));
  for(int i=0;i<count;i++){
   int index=i,res=SlotResource(0,i);float y=405+i*42;double work=0;
   if(res>=0)work=i>0?city.ProducerSlots(0)[i].remainingWork:city.mineOrders.Count>0?city.mineOrders[0].remainingWork:city.PrimaryAssignedMineResource>=0?city.PrimaryAssignedMineWork:city.ManualMineScheduleReady?city.AutoMineWork:city.dependencyMineResource>=0?city.dependencyMineWork:city.AutoMineWork;
   GUI.Label(new Rect(432,y+1,315,23),"Slot "+(i+1)+" / "+(res<0?"Idle":Settlement.ResourceNames[0][res]),ArchiveType(14,archiveIvory,true));GUI.Label(new Rect(433,y+23,304,18),SlotMode(0,i),ArchiveType(10,archiveMuted));
   ArchivePlate(new Rect(755,y+8,153,9),3,2);if(res>=0)Box(new Rect(758,y+11,147*Mathf.Clamp01(1-(float)(work/Settlement.MineUnitWork(res))),3),archiveRed);
   GUI.Label(new Rect(754,y+22,154,20),res<0?"":ProductionTime.Duration(work/city.MiningSpeed)+" to next unit",ArchiveType(10,archiveMuted));
   ProducerButton(new Rect(918,y+5,73,27),"Set ore",()=>{city.AssignProductionSlot(0,index,producerItem,out producerFeedback);city.RefreshSettlementPlan();Save();},city.MiningRequirement(producerItem)=="");
   ProducerButton(new Rect(1000,y+5,68,27),"Auto",()=>{city.AssignProductionSlot(0,index,-1,out producerFeedback);city.RefreshSettlementPlan();Save();});
  }
 }
 void DrawGatherProductionSlots(){
  int source=producerSource,count=city.ProductionSlotCount(source);ArchivePlate(new Rect(77,660,1028,145),0,10);GUI.Label(new Rect(91,672,692,25),"PRODUCTION SLOTS / "+count+" OF 4",ArchiveType(14,archiveGold,true));ProducerButton(new Rect(867,669,220,26),"Research extra slots",()=>OpenSlotResearch(source));
  for(int i=0;i<count;i++){
   int index=i,res=SlotResource(source,i);float y=702+i*24;GUI.Label(new Rect(95,y,420,23),"Slot "+(i+1)+" / "+(res<0?"Idle":Settlement.ResourceNames[source][res]),ArchiveType(12,archiveIvory));GUI.Label(new Rect(519,y+2,277,21),SlotMode(source,i),ArchiveType(10,archiveMuted));
   ProducerButton(new Rect(811,y,155,22),"Set selected material",()=>{city.AssignProductionSlot(source,index,producerItem,out producerFeedback);city.RefreshSettlementPlan();Save();},ProducerRequirement(producerItem)=="");ProducerButton(new Rect(974,y,113,22),"Auto",()=>{city.AssignProductionSlot(source,index,-1,out producerFeedback);city.RefreshSettlementPlan();Save();});
  }
 }
 void DrawGatherSlotDetails(string requirement){
  GUI.Label(new Rect(1164,584,342,25),"SLOT ASSIGNMENT",ArchiveType(15,archiveGold,true));
  GUI.Label(new Rect(1164,612,342,98),requirement!=""?requirement:"Select a material, then use Set selected material on a slot below the catalog.\n\nAuto returns that slot to city demand. Manual assignments keep producing for your stockpile.",ArchiveType(14,archiveIvory));
  ProducerButton(new Rect(1161,716,348,29),producerSource==1?"Process hides into leather":"Process fibres into cloth",()=>{int source=producerSource==1?0:1;OpenMaterialProcessing(source);materialProcessingIndex=producerItem;});
  ProducerButton(new Rect(1161,755,348,35),"Research production slots",()=>OpenSlotResearch(producerSource),city.library>0);
 }
}}
