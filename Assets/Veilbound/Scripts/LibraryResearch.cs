using System;
using UnityEngine;
namespace Veilbound {
 public static class LibraryResearch {
  public static readonly string[] Ids={"timber","quarry","mine","hunters","weaver","blacksmith","weapons","plate","clothier","housing","mine-slots","hunters-slots","weaver-slots","blacksmith-slots","leather-slots","cloth-slots","enchanting"};
  public static readonly string[] Names={"Lumber processing","Stone quarrying","Mining","Hunting","Textiles","Metal refining & smithing","Weapon forging","Plate armour","Cloth & leather workshop","Housing","Mine production slots","Hunter production slots","Textile production slots","Bar refining slots","Leather processing slots","Cloth processing slots","Veil enchanting"};
  public static int Index(string id)=>Array.IndexOf(Ids,id);
  public static bool IsSlotResearch(string id)=>Settlement.SlotResearchSource(id)>=0;
  public static int Limit(string id)=>id=="enchanting"?4:IsSlotResearch(id)?3:id=="housing"?5:10;
  public static int Baseline(string id)=>IsSlotResearch(id)||id=="housing"||id=="enchanting"?0:id=="timber"?2:1;
  public static double Duration(string id,int rank)=>Duration(IsSlotResearch(id)?rank*3:rank);
  public static double Duration(int rank)=>24+Math.Min(24,Math.Max(0,rank-2)*3);
  // Provisional tuning: wood, stone and bars from an already accessible material.
  public static TownUpgradeCost Cost(string id,int rank){if(IsSlotResearch(id))rank*=3;double scale=Math.Pow(1.8,Math.Max(0,rank-2));int material=Mathf.Clamp(rank-2,0,8);return new TownUpgradeCost(Math.Ceiling(8*scale),Math.Ceiling(4*scale),new ResourceCost(3,material,rank<=2?2:rank));}
 }
 public partial class Settlement {
  public int researchVersion;public int[] researchRanks;
  public string activeResearch="",activeConstruction="";public int researchTarget,constructionTarget;
  public double researchRemaining,constructionRemaining,researchTotal,constructionTotal;
  public double researchPaidWood,researchPaidStone,constructionPaidWood,constructionPaidStone;
  public ResourceCost[] researchPaidMaterials,constructionPaidMaterials;
  public void EnsureResearch(){
   if(researchRanks==null||researchRanks.Length!=LibraryResearch.Ids.Length){var old=researchRanks;researchRanks=new int[LibraryResearch.Ids.Length];if(old!=null)Array.Copy(old,researchRanks,Math.Min(old.Length,researchRanks.Length));}
   if(researchVersion==0){
    for(int i=0;i<researchRanks.Length;i++){string id=LibraryResearch.Ids[i];int rank=id=="enchanting"?mysticLevel:LibraryResearch.IsSlotResearch(id)?0:id=="weapons"||id=="plate"?1:BuildingLevel(id);researchRanks[i]=Math.Max(LibraryResearch.Baseline(id),Math.Min(rank,LibraryResearch.Limit(id)));}
    int knownMetal=1;for(int i=0;i<Metals.All.Length;i++)if(blacksmith>=Metals.All[i].unlock)knownMetal=Math.Max(knownMetal,i+1);
    foreach(string id in new[]{"blacksmith","weapons","plate"})researchRanks[LibraryResearch.Index(id)]=Math.Max(researchRanks[LibraryResearch.Index(id)],knownMetal);
    researchVersion=1;
   }
   activeResearch=activeResearch??"";activeConstruction=activeConstruction??"";
  }
  public int ResearchRank(string id){EnsureResearch();int i=LibraryResearch.Index(id);return i<0?0:researchRanks[i];}
  public string ResearchRequirement(string id){EnsureResearch();int i=LibraryResearch.Index(id);if(i<0)return "Unknown research.";if(library<1)return "Build the Library first.";if(activeResearch!="")return "Another research project is active.";if(researchRanks[i]>=LibraryResearch.Limit(id))return "Research complete.";if(LibraryResearch.IsSlotResearch(id)){int source=SlotResearchSource(id),tier=(researchRanks[i]+1)*3;string parent=source==0?"mine":source==1?"hunters":source==2?"weaver":source==3?"blacksmith":"clothier";if(ResearchRank(parent)<tier)return "Requires "+LibraryResearch.Names[LibraryResearch.Index(parent)]+" research tier "+tier+".";}return "";}
  bool CanPayTechnology(TownUpgradeCost c){EnsureIndustry();EnsureGear();if(wood+1e-9<c.wood||stone+1e-9<c.stone)return false;foreach(var m in c.materials)if(ResourceAmount(m.family,m.index)+1e-9<m.amount)return false;return true;}
  void PayTechnology(TownUpgradeCost c){wood=Math.Max(0,wood-c.wood);stone=Math.Max(0,stone-c.stone);foreach(var m in c.materials)AddResource(m.family,m.index,-m.amount);}
  public bool CanResearch(string id){if(ResearchRequirement(id)!="")return false;return CanPayTechnology(LibraryResearch.Cost(id,ResearchRank(id)+1));}
  public bool StartResearch(string id,out string message){message=ResearchRequirement(id);if(message!="")return false;int rank=ResearchRank(id)+1;var cost=LibraryResearch.Cost(id,rank);if(!CanPayTechnology(cost)){message="Requires "+TechnologyCostText(cost);return false;}PayTechnology(cost);activeResearch=id;researchTarget=rank;researchTotal=researchRemaining=LibraryResearch.Duration(id,rank);researchPaidWood=cost.wood;researchPaidStone=cost.stone;researchPaidMaterials=cost.materials;message=LibraryResearch.Names[LibraryResearch.Index(id)]+" research started.";return true;}
  public string TechnologyCostText(TownUpgradeCost c)=>c.wood+" wood · "+c.stone+" stone · "+RecipeText(c.materials);
  public string ResearchBuildingRequirement(string id){EnsureResearch();if(activeConstruction!="")return "Construction in progress: "+activeConstruction+".";int target=BuildingLevel(id)+1;string branch=id=="enchanter"?"enchanting":id=="tailor"||id=="leatherworker"?"clothier":id;if(LibraryResearch.Index(branch)<0)return "";int baseline=LibraryResearch.Baseline(branch);if(target<=baseline)return "";return ResearchRank(branch)>=target?"":"Research "+LibraryResearch.Names[LibraryResearch.Index(branch)]+" "+target+" at the Library first.";}
  public string CraftResearchRequirement(int metal,string branch){if(metal<0||metal>=WeaponRules.Count)return "Invalid material.";int rank=WeaponRules.Material(metal)+1;if(ResearchRank(branch)<rank)return "Research "+LibraryResearch.Names[LibraryResearch.Index(branch)]+" "+rank+" at the Library first.";return "";}
  public double ConstructionDuration(string id,int target)=>id=="enchanter"?24+(target-1)*6:id=="housing"?24+Math.Min(24,(target-1)*6):target<=LibraryResearch.Baseline(id)?0:24+Math.Min(24,(target-2)*3);
  void RefundTechnology(double w,double s,ResourceCost[] m){wood+=w;stone+=s;if(m!=null)foreach(var part in m)AddResource(part.family,part.index,part.amount);}
  public bool CancelResearch(){EnsureResearch();if(activeResearch=="")return false;RefundTechnology(researchPaidWood,researchPaidStone,researchPaidMaterials);activeResearch="";researchRemaining=researchTotal=0;researchPaidMaterials=null;return true;}
  public bool CancelConstruction(){EnsureResearch();if(activeConstruction=="")return false;RefundTechnology(constructionPaidWood,constructionPaidStone,constructionPaidMaterials);activeConstruction="";constructionRemaining=constructionTotal=0;constructionPaidMaterials=null;return true;}
  void FinishTechnologyJobs(){
   if(activeResearch!=""&&researchRemaining<=1e-9){int i=LibraryResearch.Index(activeResearch);if(i>=0)researchRanks[i]=Math.Max(researchRanks[i],researchTarget);activeResearch="";researchRemaining=researchTotal=0;researchPaidMaterials=null;}
   if(activeConstruction!=""&&constructionRemaining<=1e-9){string id=activeConstruction;activeConstruction="";constructionRemaining=constructionTotal=0;constructionPaidMaterials=null;ApplyTownUpgrade(id);}
  }
  void TickTechnology(double h){EnsureResearch();FinishTechnologyJobs();while(h>1e-9){RefreshSettlementPlan(true);BeginDependencyBar();double step=PlannerBoundary(h);if(activeResearch!="")step=Math.Min(step,researchRemaining);if(activeConstruction!="")step=Math.Min(step,constructionRemaining);if(autoBarsEnabled)step=Math.Min(step,NextMineHours);if(step<=1e-9){FinishTechnologyJobs();continue;}wood+=WoodRate*step;stone+=StoneRate*step;TickSmithOrders(step);TickMaterialProcessing(step);TickIndustry(step);hours+=step;if(activeResearch!="")researchRemaining=Math.Max(0,researchRemaining-step);if(activeConstruction!="")constructionRemaining=Math.Max(0,constructionRemaining-step);h-=step;FinishTechnologyJobs();}RefreshSettlementPlan(true);}
 }
}
