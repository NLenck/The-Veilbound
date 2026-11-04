using System;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Veilbound {public partial class VeilGame {
 bool materialProcessingOpen,materialProcessingEditing;int materialProcessingSource,materialProcessingIndex;string materialProcessingAmount="10",materialProcessingFeedback="";
 readonly Texture2D[] materialProcessingBackgrounds=new Texture2D[2];
 bool MaterialProcessingScreenOpen=>materialProcessingOpen&&state==ScreenState.Settlement&&tab==2&&!gearBagOpen&&!libraryResearchOpen&&!sanctuaryWorkshopOpen;
 int MaterialProcessingQuantity=>int.TryParse(materialProcessingAmount,out int amount)?amount:0;
 bool TryMaterialProcessingActivity(out string material,out double hours,out float progress){material="";hours=double.PositiveInfinity;progress=0;for(int source=0;source<2;source++)for(int i=0;i<city.MaterialProcessingSlotCount(source);i++){var slot=city.ProcessingSlots(source)[i];if(!slot.paid&&slot.mode!=2)continue;double time=slot.remaining/city.MaterialProcessingSpeed;if(time<hours){material=Settlement.ProcessedNames[source][slot.index];hours=time;progress=1-(float)(slot.remaining/Settlement.ProcessUnitWork[slot.index]);}}return material!="";}
 void OpenMaterialProcessing(int source){clothierPage=source==1?0:1;SelectCityBuilding("clothier");materialProcessingSource=source;materialProcessingIndex=0;materialProcessingOpen=true;materialProcessingEditing=false;libraryResearchOpen=sanctuaryWorkshopOpen=false;producerOpen=false;tab=2;selection=0;LeaveResourceBar();}
 void CloseMaterialProcessing(){materialProcessingOpen=materialProcessingEditing=false;SelectCityBuilding("clothier");}
 void CityMaterialServiceDetails(){
  GUI.Label(new Rect(1100,385,450,70),"Process hides into finished leather and fibres into finished cloth, then use them to craft equipment.",CityType(16,cityIvory));
  CityButton(1100,475,450,43,"Process fibres into cloth",()=>OpenMaterialProcessing(1),city.clothier>0);CityButton(1100,527,450,43,"Process hides into leather",()=>OpenMaterialProcessing(0),city.clothier>0);
  CityButton(1100,579,450,43,"Craft cloth equipment & capes",()=>OpenCityWorkshop(true,1),city.clothier>0);CityButton(1100,631,450,43,"Craft leather equipment",()=>OpenCityWorkshop(true,2),city.clothier>0);
  CityButton(1100,683,450,35,"Manage raw fibres",()=>SelectCityBuilding("weaver"));CityButton(1100,727,450,35,"Manage raw hides",()=>SelectCityBuilding("hunters"));
 }
 void OpenMaterialSlotResearch(int source){materialProcessingOpen=false;producerOpen=sanctuaryWorkshopOpen=false;libraryResearchOpen=true;SelectResearchTab(1);selectedResearch=source==0?"leather-slots":"cloth-slots";selectedResearchTier=Math.Min(3,city.ResearchRank(selectedResearch)+1);researchScroll.y=Mathf.Clamp((selectedResearchTier*3-3)*88,0,510);selection=0;}
 void DrawMaterialSlotResearchNodes(int rank,float x,float y){
  for(int source=0;source<2;source++){int line=source;string id=source==0?"leather-slots":"cloth-slots";bool known=city.ResearchRank(id)>=rank,chosen=selectedResearch==id&&selectedResearchTier==rank;float sy=y+source*30;Box(new Rect(x+29,sy+15,28,2),known?archiveGold:archiveMuted);ArchiveButton(new Rect(x+58,sy+3,45,26),source==0?"L +1":"C +1",()=>{selectedResearch=line==0?"leather-slots":"cloth-slots";selectedResearchTier=rank;},true,chosen,known?4:3);if(chosen&&!resourceFocus&&resourceCategory<0)DrawMenuNavigationFocus(new Rect(x+58,sy+3,45,26),"Processing slot research");}
 }
 void QueueManualMaterialProcessing(){if(city.QueueMaterialProcessing(materialProcessingSource,materialProcessingIndex,MaterialProcessingQuantity,out materialProcessingFeedback)){city.RefreshSettlementPlan();Save();}}
 bool ReadMaterialProcessingInput(){return ReadClothierInput();}
 void MaterialProcessingScreen(){ClothierScreen();}
 void ClothierProcessingPanels(){DrawProcessingPage(city.ProcessingLine(materialProcessingSource+4),false);}
}}
