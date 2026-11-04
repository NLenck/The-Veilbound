using System;
using System.Collections.Generic;
namespace Veilbound {public partial class Settlement {
 // Resource filters are views of one order, never independent reservation queues.
 public string PriorityResourceName(int family,int index)=>family==6?(index==0?"Timber":"Stone"):family==7?"Essence":ResourceName(family,index);
 public double PriorityResourceStock(int family,int index)=>family==6?(index==0?wood:stone):family==7?essence:ResourceAmount(family,index);
 double RecipeResourceAmount(ResourceCost part,int family,int index,HashSet<int> path){
  if(part.amount<=0)return 0;if(part.family==family&&part.index==index)return part.amount;
  int key=part.family*100+part.index;if(part.family<3||!path.Add(key))return 0;
  double amount=0;foreach(var child in part.family==3?BarRecipe(part.index):MaterialProcessRecipe(part.family-4,part.index))amount+=part.amount*RecipeResourceAmount(child,family,index,path);path.Remove(key);return amount;
 }
 public double PriorityResourceAmount(SettlementJob job,int family,int index){
  var cost=JobCost(job);double amount=family==6?(index==0?cost.wood:cost.stone):0;
  if(family==7&&job.kind==5){GearCost(job.slot,job.index,LightJobTier(job),out _,out _,out _,out amount);}
  foreach(var part in cost.materials)amount+=RecipeResourceAmount(part,family,index,new HashSet<int>());
  return amount*(job.kind>=3&&job.kind<=5?job.quantity:1);
 }
 public List<ResourceCost> PriorityResourceFilters(){
  EnsureSettlementQueue();var result=new List<ResourceCost>();
  for(int family=0;family<8;family++){int count=family<3?ResourceNames[family].Length:family==3?WeaponRules.Count:family<6?5:family==6?2:1;
   for(int index=0;index<count;index++)foreach(var job in settlementQueue)if(PriorityResourceAmount(job,family,index)>1e-9){result.Add(new ResourceCost(family,index,0));break;}
  }return result;
 }
 public List<SettlementJob> FilterSettlementPriorities(int family,int index){EnsureSettlementQueue();var result=new List<SettlementJob>();foreach(var job in settlementQueue)if(family<0||PriorityResourceAmount(job,family,index)>1e-9)result.Add(job);return result;}
 public bool MoveFilteredPriority(int id,int delta,int family,int index){
  if(delta!=-1&&delta!=1)return false;var matches=FilterSettlementPriorities(family,index);int at=matches.FindIndex(j=>j.id==id),other=at+delta;
  if(at<0||other<0||other>=matches.Count)return false;
  int a=settlementQueue.IndexOf(matches[at]),b=settlementQueue.IndexOf(matches[other]);var first=settlementQueue[a];settlementQueue[a]=settlementQueue[b];settlementQueue[b]=first;return true;
 }
 // Validate the whole recipe graph. A shared input is not a cycle; a node on
 // the current recursion path is. No data or materials are changed by this check.
 public static bool RecipeGraphHasCycle(Func<int,ResourceCost[]> recipe,int count){
  var complete=new HashSet<int>();var path=new HashSet<int>();
  for(int i=0;i<count;i++)if(RecipeCycleVisit(i,recipe,count,complete,path))return true;return false;
 }
 static bool RecipeCycleVisit(int node,Func<int,ResourceCost[]> recipe,int count,HashSet<int> complete,HashSet<int> path){
  if(node<0||node>=count)return true;if(complete.Contains(node))return false;if(!path.Add(node))return true;
  foreach(var child in recipe(node))if(child.family==3&&RecipeCycleVisit(child.index,recipe,count,complete,path))return true;
  path.Remove(node);complete.Add(node);return false;
 }
 string MaterialSupplyBlock(ResourceCost part,double[][] stock,HashSet<int> path){
  if(part.amount<=1e-9)return "";double used=Math.Min(stock[part.family][part.index],part.amount);stock[part.family][part.index]-=used;double missing=part.amount-used;if(missing<=1e-9)return "";
  if(part.family>=4){int source=part.family-4,key=part.family*100+part.index;if(!path.Add(key))return "Circular material processing dependency";string requirement=MaterialProcessingRequirement(source,part.index);if(requirement!="")return requirement;if(!ProcessingCanSupply(source,part.index))return "Waiting for an automatic processing slot (manual assignments first)";double units=Math.Ceiling(missing-1e-9);stock[part.family][part.index]+=units-missing;foreach(var child in MaterialProcessRecipe(source,part.index)){string block=MaterialSupplyBlock(new ResourceCost(child.family,child.index,child.amount*units),stock,path);if(block!="")return block;}path.Remove(key);return "";}
  if(part.family==3){
   if(!path.Add(part.index))return "Circular recipe dependency: "+PriorityResourceName(3,part.index);
   string requirement=SmithRequirement(part.index,"blacksmith");if(requirement!="")return requirement;
   double units=Math.Ceiling(missing-1e-9);stock[3][part.index]+=units-missing;
   foreach(var child in BarRecipe(part.index)){string block=MaterialSupplyBlock(new ResourceCost(child.family,child.index,child.amount*units),stock,path);if(block!="")return block;}
   path.Remove(part.index);return "";
  }
  if(part.family==0)return MiningRequirement(part.index);
  string branch=part.family==1?"hunters":"weaver";int rank=ResourceUnlocks[part.family][part.index];
  if(ProducerLevel(part.family)<rank||ResearchRank(branch)<rank)return "Requires "+branch+" building / research tier "+rank+" for "+ResourceName(part.family,part.index);
  if(AutomaticGatherCapacity(part.family)<=1e-9&&ManualGatherRate(part.family,part.index)<=1e-9)return "Waiting for free "+(part.family==1?"hunter":"textile")+" capacity (manual allocations first)";
  return "";
 }
 string JobSupplyBlock(SettlementJob job,TownUpgradeCost cost,double[][] available,bool cyclicRecipes){
  if(cost.wood>wood+1e-9&&WoodRate<=0)return "Requires timber production";
  if(cost.stone>stone+1e-9&&StoneRate<=0)return "Requires stone production";
  if(job.kind==5){GearCost(job.slot,job.index,LightJobTier(job),out _,out _,out _,out double e);if(essence+1e-9<e)return "Requires "+e+" essence from expeditions";}
  var trial=new double[available.Length][];for(int f=0;f<trial.Length;f++)trial[f]=(double[])available[f].Clone();
  foreach(var part in cost.materials){if(cyclicRecipes&&part.family==3)return "Circular production dependency; other priorities can continue";string block=MaterialSupplyBlock(part,trial,new HashSet<int>());if(block!="")return block;}
  return "";
 }
}}
