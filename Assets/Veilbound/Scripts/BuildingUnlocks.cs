using System;using System.Collections.Generic;using UnityEngine;
namespace Veilbound {
 public partial class Settlement {
  public bool BaseBuildingsBuilt=>timber>0&&quarry>0&&mine>0&&huntersLodge>0&&weaver>0&&blacksmith>0&&clothier>0;
  public string InitialBuildingRequirement(string id){
   if(id!="housing"&&Array.IndexOf(Buildings,id)<0)return "Unknown building.";
   if(id=="herbalist")return "Requires Library research (system in development).";
   if(BuildingLevel(id)>0)return "";
   EnsureResearch();string branch=id=="enchanter"?"enchanting":id=="tailor"||id=="leatherworker"?"clothier":id;
   if(LibraryResearch.Index(branch)>=0&&LibraryResearch.Baseline(branch)<1&&ResearchRank(branch)<1)return "Research "+LibraryResearch.Names[LibraryResearch.Index(branch)]+" 1 at the Library first.";
   if(id=="library"&&!BaseBuildingsBuilt)return "Build all base production and crafting buildings first.";
   if(id=="quarry"&&timber<2)return "Upgrade the logger's tent to a lumbermill first.";
   if(id!="timber"&&id!="quarry"&&id!="housing"&&quarry<1)return "Build a quarry first.";
   if(id=="blacksmith"&&mine<1)return "Build a mine first.";
   if(id=="tailor"&&weaver<1)return "Build the textile tavern first.";
   if(id=="leatherworker"&&huntersLodge<1)return "Build the hunter's cabin first.";
   if(id=="clothier"&&weaver<1&&huntersLodge<1)return "Build textile supply or a hunter's cabin first.";
   return "";
  }
  public bool BuildingVisible(string id)=>id=="housing"||BuildingLevel(id)>0||activeConstruction==id||InitialBuildingRequirement(id)=="";
 }
 public partial class VeilGame {
  string[] VisibleSidebarBuildings(){var visible=new List<string>();foreach(string id in CitySidebarBuildings)if(city.BuildingVisible(id))visible.Add(id);return visible.ToArray();}
  int SidebarButtonIndex(string id)=>5+Mathf.Max(0,Array.IndexOf(VisibleSidebarBuildings(),id));
 }
}
