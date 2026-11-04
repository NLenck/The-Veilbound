using System;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Veilbound {public partial class VeilGame {
 bool settlementQueueOpen;Vector2 settlementQueueScroll;int queueKind,queueItem,queueSlot,queueAmount=1;int priorityResourceFamily=-1,priorityResourceIndex,queueGearTier=1;
 readonly string[] QueueKinds={"Construction","Research","Bars","Plate armour","Weapons","Cloth gear","Leather gear","Process leather","Process cloth"};
 void OpenSettlementQueue(){settlementQueueOpen=true;notice="";queuePicker=null;ResetWorkshopCostInspection();producerEditing=smithEditingQuantity=false;selection=0;city.RefreshSettlementPlan();}
 void CloseSettlementQueue(){settlementQueueOpen=false;queuePicker=null;selection=0;}
 void QueueGoal(int kind,string branch="",int item=0,int slot=0,int amount=1,int target=0){if(city.QueueSettlementJob(kind,branch,item,slot,amount,target,out notice))Save();}
 string[] QueueItems(){if(queueKind==0)return new[]{"timber","quarry","mine","weaver","hunters","blacksmith","clothier","housing","library","enchanter"};if(queueKind==1)return LibraryResearch.Ids;if(queueKind>=7)return Settlement.ProcessedNames[queueKind-7];if(queueKind==6){var slots=new string[8];Array.Copy(Gear.Slots,slots,8);return slots;}if(queueKind==5)return Gear.Slots;var names=new string[queueKind==3?10:WeaponRules.Count];for(int i=0;i<names.Length;i++)names[i]=queueKind==3?Metals.All[i].name:queueKind==4?WeaponRules.Name(i):WeaponRules.BarName(i);return names;}
 string QueueItemTitle(string value)=>queueKind==0?CityBuildingName(value):queueKind==1?LibraryResearch.Names[LibraryResearch.Index(value)]:value;
 void AddSelectedQueueGoal(){var items=QueueItems();queueItem=Mathf.Clamp(queueItem,0,items.Length-1);int kind=queueKind>=7?queueKind-1:queueKind>=5?5:queueKind;QueueGoal(kind,kind<=1?items[queueItem]:"",queueKind>=7?queueItem:queueKind>=5?queueKind-4:queueItem,queueKind>=5?queueItem:queueSlot,kind<=1?1:queueAmount,queueKind==5||queueKind==6?queueGearTier:0);}
 bool ReadSettlementQueueInput(){if(state!=ScreenState.Settlement||!settlementQueueOpen)return false;if(ReadQueuePickerInput())return true;int previous=selection;MenuInput();queueFollowFocus=selection!=previous;if(Keyboard.current?.escapeKey.wasPressedThisFrame==true||PreferredPad?.buttonEast.wasPressedThisFrame==true)CloseSettlementQueue();return true;}
 string PriorityProductionSummary(){
  int mine=city.ActiveMineResource,bar=city.ActiveBarMetal,hunt=city.PlannedGatherResource(1),textile=city.PlannedGatherResource(2);
  return "MINE: "+(mine<0?"Idle":Settlement.ResourceNames[0][mine])+"   FORGE: "+(bar<0?"Idle":WeaponRules.BarName(bar))+"   HUNTERS: "+(hunt<0?"Manual / idle":Settlement.ResourceNames[1][hunt])+"   TEXTILES: "+(textile<0?"Manual / idle":Settlement.ResourceNames[2][textile]);
 }
 void CyclePriorityResource(int direction){
  var options=city.PriorityResourceFilters();int at=options.FindIndex(p=>p.family==priorityResourceFamily&&p.index==priorityResourceIndex)+1;at=(at+direction+options.Count+1)%(options.Count+1);
  priorityResourceFamily=at==0?-1:options[at-1].family;priorityResourceIndex=at==0?0:options[at-1].index;settlementQueueScroll=Vector2.zero;selection=0;
 }
 void SettlementQueueScreen(){
  InitSmithArt();city.EnsureSettlementQueue();workshopCostHoverName=null;Box(new Rect(0,0,1600,900),new Color(.018f,.022f,.024f));
  ArchivePlate(new Rect(34,28,1532,828),0,18);GUI.Label(new Rect(65,48,1040,48),"SETTLEMENT PRIORITIES",ArchiveType(32,archiveIvory,true));QueueButton(new Rect(1290,49,240,38),"Back",CloseSettlementQueue);
  GUI.Label(new Rect(67,104,1455,27),"Auto slots supply queued tasks. Manual orders and stockpile assignments keep their priority.",ArchiveType(15,archiveMuted));DrawQueueWorkflowSummary();
  GUI.Label(new Rect(68,232,920,25),"PRIORITY ORDER / "+city.settlementQueue.Count+" OF "+Settlement.SettlementQueueLimit,ArchiveType(16,archiveGold,true));
  QueueButton(new Rect(72,271,117,30),"All resources",()=>{priorityResourceFamily=-1;priorityResourceIndex=0;settlementQueueScroll=Vector2.zero;},true,priorityResourceFamily<0);
  QueueButton(new Rect(201,271,454,30),priorityResourceFamily<0?"Choose a resource filter  ▾":city.PriorityResourceName(priorityResourceFamily,priorityResourceIndex)+"  ▾",()=>OpenQueuePicker("filter"));
  var jobs=city.FilterSettlementPriorities(priorityResourceFamily,priorityResourceIndex).ToArray();GUI.Label(new Rect(674,276,404,24),priorityResourceFamily<0?"One order shared by all buildings":"Stock: "+city.PriorityResourceStock(priorityResourceFamily,priorityResourceIndex).ToString("0.##")+" / "+jobs.Length+" matching tasks",ArchiveType(12,archiveMuted,false,TextAnchor.UpperRight));
  ArchivePlate(new Rect(59,313,1034,447),0,12);float height=0;foreach(var job in jobs)height+=job.id==queueExpandedJob?212:116;
  settlementQueueScroll=GUI.BeginScrollView(new Rect(72,327,1008,418),settlementQueueScroll,new Rect(0,0,982,Math.Max(408,height)),false,false);float y=0;
  for(int i=0;i<jobs.Length;i++){
   var j=jobs[i];int id=j.id;bool expanded=queueExpandedJob==id;var activity=city.GoalActivity(j);
   GUI.Label(new Rect(10,y+7,53,26),(city.settlementQueue.IndexOf(j)+1).ToString("00"),ArchiveType(19,archiveGold,true));string title=j.kind==0?CityBuildingName(j.branch)+" / build level "+j.target:city.SettlementJobName(j);
   GUI.Label(new Rect(67,y+7,631,29),title,ArchiveType(17,archiveIvory,true));DrawProductionState(new Rect(68,y+40,630,48),activity);
   QueueButton(new Rect(715,y+10,54,31),"Up",()=>{city.MoveFilteredPriority(id,-1,priorityResourceFamily,priorityResourceIndex);city.RefreshSettlementPlan();Save();},i>0,false,y);
   QueueButton(new Rect(776,y+10,65,31),"Down",()=>{city.MoveFilteredPriority(id,1,priorityResourceFamily,priorityResourceIndex);city.RefreshSettlementPlan();Save();},i<jobs.Length-1,false,y);
   QueueButton(new Rect(848,y+10,117,31),"Cancel",()=>{city.RemoveSettlementJob(id);city.RefreshSettlementPlan();Save();},true,false,y);
   QueueButton(new Rect(715,y+53,250,29),expanded?"Hide materials":"View materials",()=>queueExpandedJob=queueExpandedJob==id?0:id,true,expanded,y);
   if(expanded){GUI.Label(new Rect(68,y+99,880,25),"REQUIRED FOR REMAINING TASK / hover an icon for stock",ArchiveType(12,archiveGold,true));DrawWorkshopCostIcons(new Rect(68,y+129,888,64),city.GoalIngredientsPreview(j),0,new Vector2(72,327)-settlementQueueScroll);}
   float step=expanded?212:116;ArchiveRule(13,y+step-5,949);y+=step;
  }
  if(jobs.Length==0)GUI.Label(new Rect(25,38,912,90),priorityResourceFamily<0?"No queued tasks. Choose a job type on the right to get started.":"No queued tasks use this resource. Choose All resources or another filter.",ArchiveType(18,archiveMuted));GUI.EndScrollView();queueFollowFocus=false;
  DrawQueueGoalBuilder();GUI.Label(new Rect(70,785,1450,46),notice,ArchiveType(14,archiveIvory));GUI.Label(new Rect(67,864,1470,24),"Up / Down reorders tasks. Resource filters keep the same shared order. View materials to inspect a task's ingredients.",ArchiveType(12,archiveMuted));DrawWorkshopCostTooltip();DrawQueuePicker();
 }
}}
