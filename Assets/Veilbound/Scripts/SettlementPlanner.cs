using System;
using System.Collections.Generic;
namespace Veilbound {
 [Serializable] public sealed class SettlementJob {
  public int id,kind,index,slot,quantity=1,target;public string branch="";
 }
 // Goal kinds: construction, research, bars, plate, weapon, cloth/leather gear.
 public partial class Settlement {
  public const int SettlementQueueLimit=32;
  public List<SettlementJob> settlementQueue=new List<SettlementJob>();
  public int nextSettlementJobId=1;
  public int dependencyMineResource=-1;public double dependencyMineWork;
  public SmithWorkOrder dependencyBarOrder;
  [NonSerialized] double[][] plannerDemand;
  [NonSerialized] List<int> plannerBars;
  [NonSerialized] List<int> plannerReadyBars;
  [NonSerialized] Dictionary<int,string> plannerStatus;
  [NonSerialized] int[] plannerGather={-1,-1,-1};
  [NonSerialized] string plannerBlock;
  [NonSerialized] double[][] plannerSupplyStock;
  public void EnsureSettlementQueue(){
   EnsureGear();EnsureIndustry();EnsureResearch();EnsureMineOrders();EnsureSmithOrders();
   if(settlementQueue==null)settlementQueue=new List<SettlementJob>();
   if(plannerGather==null)plannerGather=new[]{-1,-1,-1};
   if(dependencyBarOrder!=null&&(dependencyBarOrder.quantity!=1||dependencyBarOrder.workPerBar<=0||dependencyBarOrder.ingredients==null||dependencyBarOrder.ingredients.Length==0))dependencyBarOrder=null;
   foreach(var job in settlementQueue)nextSettlementJobId=Math.Max(nextSettlementJobId,job.id+1);
  }
  public bool QueueSettlementJob(int kind,string branch,int index,int slot,int quantity,int target,out string message){
   EnsureSettlementQueue();message="";
   if(settlementQueue.Count>=SettlementQueueLimit){message="Settlement queue is full (32 jobs).";return false;}
   if(quantity<1||quantity>999||kind<0||kind>7){message="Invalid queued job.";return false;}
   if(kind==0){
    if((branch!="housing"&&Array.IndexOf(Buildings,branch)<0)||branch=="herbalist"){message="This building is not available.";return false;}
    target=Math.Max(BuildingLevel(branch),activeConstruction==branch?constructionTarget:0);
    foreach(var j in settlementQueue)if(j.kind==0&&j.branch==branch)target=Math.Max(target,j.target);
    if(++target>BuildingLimit(branch)){message="All building levels are already built or queued.";return false;}
   }
   if(kind==1){
    if(LibraryResearch.Index(branch)<0){message="This research is not available.";return false;}
    int next=ResearchRank(branch);if(activeResearch==branch)next=Math.Max(next,researchTarget);
    foreach(var j in settlementQueue)if(j.kind==1&&j.branch==branch)next=Math.Max(next,j.target);
    if(target<=0)target=next+1;
    if(target!=next+1||target>LibraryResearch.Limit(branch)){message="Queue the previous research tier first.";return false;}
   }
   if(kind>=2&&kind<=4&&(index<0||index>=(kind==3?10:WeaponRules.Count))){message="Invalid material.";return false;}
   if(kind==3&&(slot<0||slot>=8)){message="Invalid plate slot.";return false;}
   if(kind>=6&&(index<0||index>4)){message="Invalid processing material.";return false;}
   if(kind==5){if(slot<0||slot>8||index<1||index>2||slot==8&&index!=1){message="Invalid cloth or leather recipe.";return false;}if(target<=0)target=NextLightTier(slot,index);if(target<1||target>10){message="Choose gear tier 1–10.";return false;}}
   var job=new SettlementJob{id=nextSettlementJobId++,kind=kind,branch=branch??"",index=index,slot=slot,quantity=quantity,target=target};
   settlementQueue.Add(job);message="Queued "+SettlementJobName(job)+". Missing materials will be supplied automatically.";return true;
  }
  public string SettlementJobName(SettlementJob j){
   if(j.kind==0)return j.branch+" / build level "+j.target;
   if(j.kind==1)return LibraryResearch.Names[LibraryResearch.Index(j.branch)]+" / research "+j.target;
   if(j.kind==2)return WeaponRules.BarName(j.index)+" x"+j.quantity;
   if(j.kind==3)return Metals.All[j.index].name+" "+Gear.Slots[j.slot]+" x"+j.quantity;
   if(j.kind>=6)return ProcessedNames[j.kind-6][j.index]+" x"+j.quantity;
   if(j.kind==4)return WeaponRules.Name(j.index)+" x"+j.quantity;
   return LightGear.Name(j.index,LightJobTier(j))+" "+Gear.Slots[j.slot]+" x"+j.quantity;
  }
  public bool RemoveSettlementJob(int id){EnsureSettlementQueue();int i=settlementQueue.FindIndex(j=>j.id==id);if(i<0)return false;settlementQueue.RemoveAt(i);if(settlementQueue.Count==0){RefundDependencyBar();RefundAutomaticRefiningSlots();RefundAutomaticMaterialProcessing();}return true;}
  public bool MoveSettlementJob(int id,int delta){EnsureSettlementQueue();int i=settlementQueue.FindIndex(j=>j.id==id),to=i+delta;if(i<0||to<0||to>=settlementQueue.Count)return false;var j=settlementQueue[i];settlementQueue.RemoveAt(i);settlementQueue.Insert(to,j);return true;}
  public bool MoveMiningOrder(int id,int delta){EnsureMineOrders();int i=mineOrders.FindIndex(j=>j.id==id),to=i+delta;if(i<0||to<0||to>=mineOrders.Count)return false;var j=mineOrders[i];mineOrders.RemoveAt(i);mineOrders.Insert(to,j);return true;}
  string JobRequirement(SettlementJob j){
   if(j.kind==0){if(BuildingLevel(j.branch)+1!=j.target)return "Waiting for previous building level";return ConstructionRequirement(j.branch);}
   if(j.kind==1){if(ResearchRank(j.branch)+1!=j.target)return "Waiting for previous research tier";return ResearchRequirement(j.branch);}
   if(j.kind==2)return SmithRequirement(j.index,"blacksmith");
   if(j.kind==3)return SmithRequirement(j.index,"plate");
   if(j.kind==4)return SmithRequirement(j.index,"weapons");
   if(j.kind>=6)return MaterialProcessingRequirement(j.kind-6,j.index);
   return LightGearRequirement(j.slot,j.index,LightJobTier(j));
  }
  TownUpgradeCost JobCost(SettlementJob j){
   if(j.kind==0)return UpgradeCost(j.branch,j.target);
   if(j.kind==1)return LibraryResearch.Cost(j.branch,j.target);
   if(j.kind==2)return new TownUpgradeCost(0,0,ScaleRecipe(BarRecipe(j.index),j.quantity));
   if(j.kind==3)return new TownUpgradeCost(0,0,PlateRecipe(j.slot,j.index));
   if(j.kind==4)return new TownUpgradeCost(0,0,WeaponRecipe(j.index));
   if(j.kind>=6)return new TownUpgradeCost(0,0,ScaleRecipe(MaterialProcessRecipe(j.kind-6,j.index),j.quantity));
   return new TownUpgradeCost(0,0,LightGearRecipe(j.slot,j.index,LightJobTier(j)));
  }
  static ResourceCost[] ScaleRecipe(ResourceCost[] recipe,int quantity){var result=new ResourceCost[recipe.Length];for(int i=0;i<result.Length;i++)result[i]=new ResourceCost(recipe[i].family,recipe[i].index,recipe[i].amount*quantity);return result;}
  double[][] PlannerStock(bool incoming){
   var stock=new double[6][];for(int f=0;f<6;f++){stock[f]=new double[f>=4?5:f==3?WeaponRules.Count:ResourceNames[f].Length];for(int i=0;i<stock[f].Length;i++)stock[f][i]=ResourceAmount(f,i);}
   if(incoming){AddIncomingMaterialProcessing(stock);AddIncomingRefiningSlots(stock);var slots=ProducerSlots(0);for(int i=0;i<ProductionSlotCount(0);i++)if(slots[i].resource>=0)stock[0][slots[i].resource]++;foreach(var o in mineOrders)stock[0][o.resource]+=o.quantity-o.completed;foreach(var o in smithOrders)stock[3][o.metal]+=o.quantity-o.completed;if(autoBarsEnabled&&autoBarOrder!=null)stock[3][autoBarOrder.metal]++;if(dependencyBarOrder!=null)stock[3][dependencyBarOrder.metal]++;}
   return stock;
  }
  void PlanMaterial(double[][] stock,ResourceCost part,List<ResourceCost> leaves,int depth=0){
   if(part.amount<=1e-9||depth>12)return;
   double realUsed=Math.Min(plannerSupplyStock[part.family][part.index],part.amount);plannerSupplyStock[part.family][part.index]-=realUsed;double used=Math.Min(stock[part.family][part.index],part.amount);stock[part.family][part.index]-=used;double missing=part.amount-used;
   if(missing<=1e-9)return;
   if(part.family>=4){int source=part.family-4;string requirement=MaterialProcessingRequirement(source,part.index);if(requirement!=""){plannerBlock=requirement;return;}int units=(int)Math.Ceiling(missing-1e-9);plannerProcessDemand[source][part.index]+=units;plannerProcessReadyUnits[source][part.index]+=RecipeBudgetUnits(MaterialProcessRecipe(source,part.index),plannerSupplyStock,units);stock[part.family][part.index]+=units-missing;foreach(var child in MaterialProcessRecipe(source,part.index))PlanMaterial(stock,new ResourceCost(child.family,child.index,child.amount*units),leaves,depth+1);if(!plannerProcessPriority.Exists(p=>p.family==part.family&&p.index==part.index))plannerProcessPriority.Add(new ResourceCost(part.family,part.index,0));return;}
   if(part.family==3){
    if(SmithRequirement(part.index,"blacksmith")!=""){plannerBlock=SmithRequirement(part.index,"blacksmith");return;}
    int count=(int)Math.Ceiling(missing-1e-9);plannerBarDemand[part.index]+=count;stock[3][part.index]+=count-missing;
    int readyUnits=RecipeBudgetUnits(BarRecipe(part.index),plannerSupplyStock,count);plannerBarReadyUnits[part.index]+=readyUnits;bool ready=readyUnits>0;
    if(ready&&!plannerReadyBars.Contains(part.index))plannerReadyBars.Add(part.index);
    foreach(var ingredient in BarRecipe(part.index))PlanMaterial(stock,new ResourceCost(ingredient.family,ingredient.index,ingredient.amount*count),leaves,depth+1);
    if(!plannerBars.Contains(part.index))plannerBars.Add(part.index);
   }else leaves.Add(new ResourceCost(part.family,part.index,missing));
  }
  bool ActualAvailable(TownUpgradeCost cost,double[][] stock,double w,double s){if(w+1e-9<cost.wood||s+1e-9<cost.stone)return false;foreach(var p in cost.materials)if(stock[p.family][p.index]+1e-9<p.amount)return false;return true;}
  void ReserveActual(TownUpgradeCost cost,double[][] stock){foreach(var p in cost.materials)stock[p.family][p.index]=Math.Max(0,stock[p.family][p.index]-p.amount);}
  void ReserveTree(double[][] stock,ResourceCost p,int depth=0){
   if(depth>12||p.amount<=1e-9)return;double used=Math.Min(stock[p.family][p.index],p.amount);stock[p.family][p.index]-=used;double missing=p.amount-used;
   if(p.family>=3&&missing>1e-9)foreach(var child in p.family==3?BarRecipe(p.index):MaterialProcessRecipe(p.family-4,p.index))ReserveTree(stock,new ResourceCost(child.family,child.index,child.amount*Math.Ceiling(missing-1e-9)),depth+1);
  }
  bool DispatchJob(SettlementJob j,out string message){
   if(j.kind==0)return TryTownUpgrade(j.branch,out message);
   if(j.kind==1)return StartResearch(j.branch,out message);
   if(j.kind==2)return QueueBars(j.index,j.quantity,out message);
   if(j.kind==3)return CraftPlate(j.slot,j.index,out message);
   if(j.kind==4)return CraftMetalWeapon(j.index,out message);
   if(j.kind>=6)return QueueMaterialProcessing(j.kind-6,j.index,j.quantity,out message);
   return CraftLightGear(j.slot,j.index,LightJobTier(j),out message);
  }
  public string SettlementJobStatus(int id){if(plannerStatus==null||!plannerStatus.ContainsKey(id))return "Waiting for next city tick";return plannerStatus[id];}
  public void RefreshSettlementPlan(bool dispatch=false){int remaining=SettlementQueueLimit*1000+2;while(BuildSettlementPlan(dispatch)&&--remaining>0){} PrepareProductionSlots();PrepareBarProductionSlots();PrepareMaterialProcessing(); }
  bool BuildSettlementPlan(bool dispatch){
   EnsureSettlementQueue();plannerDemand=new[]{new double[8],new double[5],new double[5]};plannerProcessDemand=new[]{new double[5],new double[5]};plannerProcessReadyUnits=new[]{new double[5],new double[5]};plannerBarReadyUnits=new double[WeaponRules.Count];plannerProcessPriority=new List<ResourceCost>();plannerBarDemand=new double[WeaponRules.Count];plannerBars=new List<int>();plannerReadyBars=new List<int>();plannerStatus=new Dictionary<int,string>();for(int f=0;f<3;f++)plannerGather[f]=-1;
   if(settlementQueue.Count==0){RefundAutomaticMaterialProcessing();RefundAutomaticRefiningSlots();RefundDependencyBar();dependencyMineResource=-1;dependencyMineWork=0;return false;}
   var claimedWorkflows=new HashSet<string>();var stock=PlannerStock(true);var actual=PlannerStock(false);plannerSupplyStock=PlannerStock(false);double w=wood,s=stone;bool cyclicRecipes=RecipeGraphHasCycle(BarRecipe,WeaponRules.Count);
   for(int at=0;at<settlementQueue.Count;at++){
    var j=settlementQueue[at];
    if((j.kind==0&&BuildingLevel(j.branch)>=j.target)||(j.kind==1&&ResearchRank(j.branch)>=j.target)){if(dispatch){settlementQueue.RemoveAt(at--);continue;}}
    string workflow=j.kind==0?"Construction":j.kind==1?"Library":j.kind<=4?"Blacksmith":j.kind==5?"Clothier":j.kind==6?"Leather processing":"Cloth processing";
    if(claimedWorkflows.Contains(workflow)){plannerStatus[j.id]="Waiting for an earlier "+workflow+" job";continue;}
    var cost=JobCost(j);string reason=JobRequirement(j);
    bool manualForgePause=workflow=="Blacksmith"&&(ManualBarScheduleReady||j.kind>=3&&smithOrders.Count>0);if(manualForgePause)reason="Manual Blacksmith production has priority";
    if(dispatch&&reason==""&&ActualAvailable(cost,actual,w,s)&&DispatchJob(j,out _)){
     if(j.kind>=3&&j.kind<=5&&--j.quantity>0)return true;
     settlementQueue.RemoveAt(at);return true;
    }
    // A blocked goal must not hold materials or producer time ahead of work that can run.
    if(reason!=""&&!manualForgePause){plannerStatus[j.id]=reason+" / checking lower priorities";continue;}
    string supplyBlock=JobSupplyBlock(j,cost,stock,cyclicRecipes);
    if(supplyBlock!=""){plannerStatus[j.id]=supplyBlock+" / checking lower priorities";continue;}
    claimedWorkflows.Add(workflow);
    foreach(var p in cost.materials)ReserveTree(actual,p);w=Math.Max(0,w-cost.wood);s=Math.Max(0,s-cost.stone);
    plannerBlock="";var leaves=new List<ResourceCost>();foreach(var p in cost.materials)PlanMaterial(stock,p,leaves);
    string supply="";
    foreach(var p in leaves){
     plannerDemand[p.family][p.index]+=p.amount;
     bool unlocked=p.family==0?MiningRequirement(p.index)=="":ProducerLevel(p.family)>=ResourceUnlocks[p.family][p.index]&&ResearchRank(p.family==1?"hunters":"weaver")>=ResourceUnlocks[p.family][p.index];
     if(unlocked&&plannerGather[p.family]<0)plannerGather[p.family]=p.index;
     if(supply=="")supply=unlocked?"Supplying "+ResourceName(p.family,p.index):"Requires producer upgrade: "+ResourceName(p.family,p.index);
    }
    if(reason!="")plannerStatus[j.id]=reason;
    else if(wood+1e-9<cost.wood||stone+1e-9<cost.stone)plannerStatus[j.id]="Waiting for timber / stone";
    else if(supply!="")plannerStatus[j.id]=supply;
    else if(plannerBlock!="")plannerStatus[j.id]=plannerBlock;
    else plannerStatus[j.id]="Waiting for earlier reservations, incoming materials or an available workshop";
   }
   // Keep partially mined units when still needed, avoiding loss on unrelated queue changes.
   if(dependencyMineResource<0||plannerDemand[0][dependencyMineResource]<=1e-9||MiningRequirement(dependencyMineResource)!=""){
    dependencyMineResource=plannerGather[0];dependencyMineWork=dependencyMineResource<0?0:MineUnitWork(dependencyMineResource);
   }
   if(RefundUnusedMaterialProcessing(stock))return true;
   if(RefundUnneededRefiningSlots(stock))return true;
   if(dependencyBarOrder!=null&&stock[3][dependencyBarOrder.metal]>=1-1e-9){RefundDependencyBar();return true;}
   return false;
  }
  public int PlannedGatherResource(int family)=>plannerGather==null?-1:plannerGather[family];
  public double PlannedGatherRate(int family,int index){
   if(family<1||family>2||PlannedGatherResource(family)!=index||plannerDemand==null)return 0;
   return Math.Max(0,Capacity(family)-UsedCapacity(family))*ResourceYields[family][index]*PassiveProductionMultiplier*Productivity/CityHoursPerDay+ExtraAutomaticGatherRate(family,index);
  }
  public double PlannedMineHours=>dependencyMineResource<0||MiningSpeed<=0?double.PositiveInfinity:dependencyMineWork/MiningSpeed;
  public int DependencyBarMetal=>dependencyBarOrder==null?-1:dependencyBarOrder.metal;
  public void BeginDependencyBar(){
   if(smithOrders.Count>0||PrimaryAssignedBar>=0||ManualBarScheduleReady||dependencyBarOrder!=null||plannerBars==null)return;
   foreach(int metal in plannerReadyBars){var recipe=BarRecipe(metal);if(!HasRecipe(recipe))continue;
    // Inputs are needed by an earlier job before a later refinement request.
    if(!PayRecipe(recipe,1,out _))continue;double unit=SmithRefining.WorkHours[metal];dependencyBarOrder=new SmithWorkOrder{metal=metal,quantity=1,workPerBar=unit,remainingWork=unit,ingredients=recipe};return;
   }
  }
  void RefundDependencyBar(){if(dependencyBarOrder==null)return;foreach(var p in dependencyBarOrder.ingredients)AddResource(p.family,p.index,p.amount);dependencyBarOrder=null;}
  public bool HasDependencyBars=>dependencyBarOrder!=null||plannerBars!=null&&plannerBars.Count>0;
  public void TickDependencyBars(double work){if(dependencyBarOrder==null)return;var o=dependencyBarOrder;o.remainingWork=Math.Max(0,o.remainingWork-work);if(o.remainingWork>1e-9)return;AddResource(3,o.metal,1);dependencyBarOrder=null;}
  public void TickDependencyMining(double work){if(dependencyMineResource<0)return;dependencyMineWork=Math.Max(0,dependencyMineWork-work);if(dependencyMineWork<=1e-9){AddResource(0,dependencyMineResource,1);totalMinedUnits++;dependencyMineResource=-1;}}
  public void TickDependencyGather(double h){if(plannerDemand==null)return;for(int f=1;f<3;f++){int i=PlannedGatherResource(f);if(i<0)continue;double demand=plannerDemand[f][i];double manual=ManualGatherRate(f,i)*h;double amount=Math.Min(Math.Max(0,demand-manual),PlannedGatherRate(f,i)*h);AddResource(f,i,amount);}}
  public double PlannerBoundary(double max){
   max=Math.Min(max,NextMineHours);
   max=Math.Min(max,NextBarHours);max=Math.Min(max,NextMaterialProcessingHours);max=Math.Min(max,NextManualMaterialInputHours);
   if(plannerDemand!=null)for(int f=1;f<3;f++)for(int i=0;i<plannerDemand[f].Length;i++)if(plannerDemand[f][i]>1e-9){double rate=ProductionRate(f,i);if(rate>0)max=Math.Min(max,Math.Min(plannerDemand[f][i]/rate,ProductionTime.NextUnit(ResourceAmount(f,i),rate)));}
   foreach(var j in settlementQueue){var c=JobCost(j);if(c.wood>wood+1e-9&&WoodRate>0)max=Math.Min(max,(c.wood-wood)/WoodRate);if(c.stone>stone+1e-9&&StoneRate>0)max=Math.Min(max,(c.stone-stone)/StoneRate);}
   return max;
  }
 }
}
