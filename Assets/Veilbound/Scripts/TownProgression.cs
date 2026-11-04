using System;using System.Collections.Generic;using UnityEngine;
namespace Veilbound {
 public sealed class TownUpgradeCost {
  public double wood,stone;public ResourceCost[] materials;
  public TownUpgradeCost(double w,double s,params ResourceCost[] m){wood=w;stone=s;materials=m;}
 }
 public partial class Settlement {
  public int townProgressionVersion,housingTier,clothier,library;
  public static readonly string[] HousingNames={"Tents","Basic","Practical","Comfortable","Advanced","Expanded"};
  public static readonly double[] TimberDaily={0,10,12,14.4,17.28,20.736,24.8832,29.85984,35.831808,42.9981696,51.59780352};
  public static readonly double[] StoneDaily={0,7,8.4,10.08,12.096,14.5152,17.41824,20.901888,25.0822656,30.09871872,36.118462464};
  public double Productivity=>1+.2*Math.Max(0,Math.Min(5,housingTier));
  public void EnsureTownProgression(){housingTier=Math.Max(0,Math.Min(5,housingTier));clothier=Math.Max(clothier,Math.Max(tailor,leatherworker));townProgressionVersion=2;}
  public static Settlement NewCamp(){var c=new Settlement{townProgressionVersion=1,wood=0,stone=0,iron=0,timber=1,quarry=0,mine=0,weaver=0,huntersLodge=0};c.InitializeWoodenStarter();c.EnsureGearRolls();c.EnsureIndustry();c.EnsureVirtue();return c;}
  public int BuildingLimit(string id)=>id=="housing"?5:id=="enchanter"?4:id=="library"||id=="herbalist"?1:id=="tailor"||id=="leatherworker"?2:10;
  public string ConstructionRequirement(string id){
   if(id!="housing"&&Array.IndexOf(Buildings,id)<0)return "Unknown building.";
   if(id=="herbalist")return "Requires Library research (system in development).";
   int n=BuildingLevel(id);if(n>=BuildingLimit(id))return "Fully upgraded.";
   string initial=InitialBuildingRequirement(id);if(initial!="")return initial;
   string technology=ResearchBuildingRequirement(id);if(technology!="")return technology;
   if(n==0&&id=="quarry"&&timber<2)return "Upgrade the logger's tent to a lumbermill first.";
   if(n==0&&id!="timber"&&id!="quarry"&&id!="housing"&&quarry<1)return "Build a quarry first.";
   if(n==0&&id=="blacksmith"&&mine<1)return "Build a mine first.";
   if(n==0&&id=="tailor"&&weaver<1)return "Build the textile tavern first.";
   if(n==0&&id=="leatherworker"&&huntersLodge<1)return "Build the hunter's cabin first.";
   if(n==0&&id=="clothier"&&weaver<1&&huntersLodge<1)return "Build textile supply or a hunter's cabin first.";
   return "";
  }
  public TownUpgradeCost UpgradeCost(string id,int plannedTarget=0){
   int target=plannedTarget>0?plannedTarget:BuildingLevel(id)+1;
   if(id=="enchanter"){int level=Mathf.Clamp(target,1,4);double[] w={40,100,300,900},s={30,70,220,650};int[] metal={0,1,2,4};return new TownUpgradeCost(w[level-1],s[level-1],new ResourceCost(3,metal[level-1],level*2+2));}
   if(id=="library")return new TownUpgradeCost(10,5,new ResourceCost(2,0,4),new ResourceCost(1,0,4),new ResourceCost(3,0,4));
   if(id=="housing"){
    int t=Mathf.Clamp(target,1,5);double[] w={10,60,300,2000,15000},s={5,30,180,1400,10000};int[] qty={4,12,30,60,100};var m=new List<ResourceCost>{new ResourceCost(2,t-1,qty[t-1]),new ResourceCost(1,t-1,qty[t-1])};
    if(t==3)m.Add(new ResourceCost(3,2,8));if(t==4)m.Add(new ResourceCost(3,5,20));if(t==5){m.Add(new ResourceCost(3,7,30));m.Add(new ResourceCost(3,8,30));}return new TownUpgradeCost(w[t-1],s[t-1],m.ToArray());
   }
   if(target==1)return id=="timber"?new TownUpgradeCost(0,0):id=="quarry"?new TownUpgradeCost(10,0):new TownUpgradeCost(10,5);
   if(id=="timber"&&target==2)return new TownUpgradeCost(4,0);
   int i=Mathf.Clamp(target,2,10)-2;double[] wood={16,40,100,260,700,2000,6000,18000,60000},stone={8,24,65,180,500,1500,4500,14000,45000};var parts=new List<ResourceCost>();
   if(target>=3){int grade=Mathf.Clamp(target-3,0,4);int[] amount={4,8,12,20,30,50,80,120};int q=amount[target-3];parts.Add(new ResourceCost(2,grade,q));parts.Add(new ResourceCost(1,grade,q));}
   if(target==4)parts.Add(new ResourceCost(3,1,4));if(target==5)parts.Add(new ResourceCost(3,2,8));if(target==6)parts.Add(new ResourceCost(3,4,12));if(target==7)parts.Add(new ResourceCost(3,5,20));if(target==8)parts.Add(new ResourceCost(3,6,32));if(target==9){parts.Add(new ResourceCost(3,7,40));parts.Add(new ResourceCost(3,8,40));}if(target==10){parts.Add(new ResourceCost(3,9,30));parts.Add(new ResourceCost(3,10,30));}
   return new TownUpgradeCost(wood[i],stone[i],parts.ToArray());
  }
  public bool CanUpgrade(string id){if(ConstructionRequirement(id)!="")return false;EnsureGear();EnsureIndustry();var c=UpgradeCost(id);if(wood+1e-9<c.wood||stone+1e-9<c.stone)return false;foreach(var m in c.materials)if(ResourceAmount(m.family,m.index)+1e-9<m.amount)return false;return true;}
  public string UpgradeCostText(string id){var c=UpgradeCost(id);string text=c.wood+" wood · "+c.stone+" stone";if(c.materials.Length>0)text+="\n"+RecipeText(c.materials);return text;}
  public bool TryTownUpgrade(string id,out string message){
   message=ConstructionRequirement(id);if(message!="")return false;
   if(!CanUpgrade(id)){message="Requires "+UpgradeCostText(id).Replace("\n"," · ");return false;}
   var c=UpgradeCost(id);wood=Math.Max(0,wood-c.wood);stone=Math.Max(0,stone-c.stone);foreach(var m in c.materials)AddResource(m.family,m.index,-m.amount);
   int target=BuildingLevel(id)+1;double duration=ConstructionDuration(id,target);
   if(duration>0){activeConstruction=id;constructionTarget=target;constructionRemaining=constructionTotal=duration;constructionPaidWood=c.wood;constructionPaidStone=c.stone;constructionPaidMaterials=c.materials;message="Construction started: "+duration+" city hours.";return true;}
   ApplyTownUpgrade(id);message="Construction complete. Level "+BuildingLevel(id)+".";return true;
  }
  void ApplyTownUpgrade(string id){
   int old=BuildingLevel(id);
   switch(id){case "enchanter":mysticLevel++;break;case "library":library++;break;case "clothier":clothier++;tailor=Math.Max(tailor,Math.Min(2,clothier));leatherworker=Math.Max(leatherworker,Math.Min(2,clothier));break;case "housing":housingTier++;break;case "timber":timber++;break;case "quarry":quarry++;break;case "mine":mine++;break;case "weaver":weaver++;break;case "hunters":huntersLodge++;break;case "blacksmith":blacksmith++;forge=blacksmith;break;case "tailor":tailor++;break;case "leatherworker":leatherworker++;break;}
   EnsureTownProgression();EnsureIndustry();if(old==0&&(id=="mine"||id=="weaver"||id=="hunters")){int f=id=="mine"?0:id=="hunters"?1:2;Array.Clear(Allocation(f),0,Allocation(f).Length);Allocation(f)[0]=Capacity(f);}

  }
 }
}
