using System;using UnityEngine;using UnityEngine.InputSystem;
namespace Veilbound {public partial class VeilGame {
 int clothierPage;
 readonly string[] ClothierTabs=Array.ConvertAll(ProductionBuildingCatalog.Clothier.pages,p=>p.title);
 bool ClothierScreenOpen=>state==ScreenState.Settlement&&tab==2&&!gearBagOpen&&!libraryResearchOpen&&(materialProcessingOpen||sanctuaryWorkshopOpen&&industryPage==1&&gearMaterial>=1&&gearMaterial<=2);
 void OpenClothier(int page){SelectCityBuilding("clothier");libraryResearchOpen=producerOpen=gearBagOpen=false;tab=2;SetClothierPage(page);LeaveResourceBar();}
 void SetClothierPage(int page){
  clothierPage=(page+4)%4;materialProcessingEditing=false;selection=0;
  if(clothierPage<2){materialProcessingOpen=true;sanctuaryWorkshopOpen=false;materialProcessingSource=clothierPage==0?1:0;}
  else {materialProcessingOpen=false;sanctuaryWorkshopOpen=true;industryPage=1;smithArmourPage=true;gearMaterial=clothierPage-1;gearSlot=Mathf.Clamp(gearSlot,0,gearMaterial==1?8:7);}
 }
 void CloseClothier(){materialProcessingOpen=sanctuaryWorkshopOpen=materialProcessingEditing=false;LeaveResourceBar();SelectCityBuilding("clothier");selection=SidebarButtonIndex("clothier");}
 bool ReadClothierInput(){return ClothierScreenOpen&&ReadCraftingBuildingInput(city.Crafter("clothier"));}
 void CraftClothierSelection(){CraftEquipment(city.Crafter("clothier"));}
 void QueueClothierSelection(){QueueEquipment(city.Crafter("clothier"));}
 void ClothierScreen(){DrawCraftingBuilding(city.Crafter("clothier"));}
 
 void ClothierEquipmentGrid(){DrawEquipmentGrid(city.EquipmentRecipes(gearMaterial==1?EquipmentRecipeKind.Cloth:EquipmentRecipeKind.Leather));}
 void ClothierEquipmentDetails(){DrawEquipmentDetails(city.Crafter("clothier"),city.EquipmentRecipes(gearMaterial==1?EquipmentRecipeKind.Cloth:EquipmentRecipeKind.Leather));}
}}
