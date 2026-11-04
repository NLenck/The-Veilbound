using System;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Veilbound {
 public partial class VeilGame {
  int smithPage,smithMetal=2,smithSlot=2,smithBarRecipe=2;bool smithHeavyVeil;string smithAmountText="10",smithFeedback="";bool smithEditingQuantity;Vector2 smithCatalogScroll,smithQueueScroll;
  bool SmithScreenOpen=>state==ScreenState.Settlement&&tab==2&&sanctuaryWorkshopOpen&&industryPage==1&&gearMaterial==3&&!gearBagOpen&&!libraryResearchOpen;
  int SmithWeapon=>smithMetal==9&&smithHeavyVeil?10:smithMetal;
  int SmithQuantity=>int.TryParse(smithAmountText,out int q)?q:0;
  void SetSmithPage(int page){smithPage=Mathf.Clamp(page,0,ProductionBuildingCatalog.Blacksmith.pages.Length-1);smithCatalogScroll=Vector2.zero;smithEditingQuantity=false;selection=0;}
  void CloseSmith(){sanctuaryWorkshopOpen=false;LeaveResourceBar();selection=SidebarButtonIndex(cityBuilding);}
  bool ReadSmithInput(){return SmithScreenOpen&&ReadCraftingBuildingInput(city.Crafter("blacksmith"));}
  void QueueSmithSelection(){if(city.QueueBars(smithBarRecipe,SmithQuantity,out smithFeedback))Save();}
  void CraftSmithSelection(){CraftEquipment(city.Crafter("blacksmith"));}
  void SmithScreen(){DrawCraftingBuilding(city.Crafter("blacksmith"));}
  void SmithEquipmentGrid(){DrawEquipmentGrid(city.EquipmentRecipes(smithPage==2?EquipmentRecipeKind.Weapon:EquipmentRecipeKind.Plate));}
  void SmithEquipmentDetails(){DrawEquipmentDetails(city.Crafter("blacksmith"),city.EquipmentRecipes(smithPage==2?EquipmentRecipeKind.Weapon:EquipmentRecipeKind.Plate));}
  void SmithStat(float y,string label,string value){GUI.Label(new Rect(1165,y,225,25),label,ArchiveType(13,archiveMuted));GUI.Label(new Rect(1390,y,115,25),value,ArchiveType(15,archiveIvory,true,TextAnchor.UpperRight));}
 }
}
