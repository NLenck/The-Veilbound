using System;using System.Collections.Generic;using UnityEngine;using UnityEngine.InputSystem;
namespace Veilbound {public partial class VeilGame {
 string queuePicker;int queuePickerFocus,queueExpandedJob;Vector2 queuePickerScroll;float queuePickerCooldown;bool queueFollowFocus;
 struct QueuePick {public string title;public int value;public QueuePick(string title,int value){this.title=title;this.value=value;}}
 List<QueuePick> QueuePickerOptions(){
  var choices=new List<QueuePick>();
  if(queuePicker=="category"){for(int i=0;i<QueueKinds.Length;i++)choices.Add(new QueuePick(QueueKinds[i],i));}
  else if(queuePicker=="slot"){for(int i=0;i<8;i++)choices.Add(new QueuePick(Gear.Slots[i],i));}
  else if(queuePicker=="tier"){for(int i=1;i<=10;i++)choices.Add(new QueuePick("Tier "+i,i));}
  else if(queuePicker=="filter"){choices.Add(new QueuePick("All resources",-1));var filters=city.PriorityResourceFilters();for(int i=0;i<filters.Count;i++)choices.Add(new QueuePick(city.PriorityResourceName(filters[i].family,filters[i].index),filters[i].family*100+filters[i].index));}
  else {var items=QueueItems();for(int i=0;i<items.Length;i++)choices.Add(new QueuePick(QueueItemTitle(items[i]),i));}
  return choices;
 }
 void OpenQueuePicker(string kind){queuePicker=kind;queuePickerScroll=Vector2.zero;var choices=QueuePickerOptions();int current=kind=="category"?queueKind:kind=="slot"?queueSlot:kind=="tier"?queueGearTier:kind=="filter"?priorityResourceFamily<0?-1:priorityResourceFamily*100+priorityResourceIndex:queueItem;queuePickerFocus=Math.Max(0,choices.FindIndex(p=>p.value==current));KeepQueuePickerVisible();}
 void KeepQueuePickerVisible(){float y=(queuePickerFocus/2)*53;if(y<queuePickerScroll.y)queuePickerScroll.y=y;else if(y+49>queuePickerScroll.y+466)queuePickerScroll.y=y+49-466;}
 void ChooseQueueOption(int at){var choices=QueuePickerOptions();if(at<0||at>=choices.Count)return;int value=choices[at].value;
  if(queuePicker=="category"){queueKind=value;queueItem=0;}
  else if(queuePicker=="slot")queueSlot=value;
  else if(queuePicker=="tier")queueGearTier=value;
  else if(queuePicker=="filter"){priorityResourceFamily=value<0?-1:value/100;priorityResourceIndex=value<0?0:value%100;settlementQueueScroll=Vector2.zero;}
  else queueItem=value;
  queuePicker=null;selection=0;
 }
 bool ReadQueuePickerInput(){
  if(queuePicker==null)return false;var key=Keyboard.current;var pad=PreferredPad;
  if(key?.escapeKey.wasPressedThisFrame==true||pad?.buttonEast.wasPressedThisFrame==true){queuePicker=null;return true;}
  int dx=key?.rightArrowKey.wasPressedThisFrame==true||pad?.dpad.right.wasPressedThisFrame==true?1:key?.leftArrowKey.wasPressedThisFrame==true||pad?.dpad.left.wasPressedThisFrame==true?-1:0;
  int dy=key?.downArrowKey.wasPressedThisFrame==true||pad?.dpad.down.wasPressedThisFrame==true?1:key?.upArrowKey.wasPressedThisFrame==true||pad?.dpad.up.wasPressedThisFrame==true?-1:0;
  queuePickerCooldown-=Time.unscaledDeltaTime;if(pad!=null&&queuePickerCooldown<=0){Vector2 stick=pad.leftStick.ReadValue();if(Math.Abs(stick.y)>.6f){dy=stick.y<0?1:-1;queuePickerCooldown=.2f;}else if(Math.Abs(stick.x)>.6f){dx=stick.x>0?1:-1;queuePickerCooldown=.2f;}}
  var choices=QueuePickerOptions();if(dx!=0||dy!=0){queuePickerFocus=Mathf.Clamp(queuePickerFocus+dx+dy*2,0,choices.Count-1);KeepQueuePickerVisible();}
  if(key?.enterKey.wasPressedThisFrame==true||pad?.buttonSouth.wasPressedThisFrame==true)ChooseQueueOption(queuePickerFocus);return true;
 }
 void DrawQueuePicker(){
  if(queuePicker==null)return;Box(new Rect(0,0,1600,900),new Color(0,0,0,.72f));ArchivePlate(new Rect(440,128,720,652),0,18);
  string title=queuePicker=="category"?"CHOOSE A JOB TYPE":queuePicker=="slot"?"CHOOSE AN ARMOUR SLOT":queuePicker=="tier"?"CHOOSE A GEAR TIER":queuePicker=="filter"?"FILTER BY RESOURCE":queueKind==0?"CHOOSE A BUILDING":queueKind==1?"CHOOSE RESEARCH":queueKind==3?"CHOOSE A PLATE MATERIAL":queueKind==4?"CHOOSE A WEAPON":queueKind==5||queueKind==6?"CHOOSE AN ARMOUR SLOT":"CHOOSE A MATERIAL";GUI.Label(new Rect(468,151,540,41),title,ArchiveType(23,archiveIvory,true));ArchiveButton(new Rect(1020,151,110,32),"Back",()=>queuePicker=null);
  GUI.Label(new Rect(468,204,660,27),"Select directly · Arrows / D-pad to move · Enter / A to choose",ArchiveType(13,archiveMuted));var choices=QueuePickerOptions();
  queuePickerScroll=GUI.BeginScrollView(new Rect(464,247,672,466),queuePickerScroll,new Rect(0,0,648,Math.Max(462,((choices.Count+1)/2)*53)),false,false);
  for(int i=0;i<choices.Count;i++){int at=i;Rect r=new Rect((i%2)*324,(i/2)*53,313,45);ArchiveButton(r,choices[i].title,()=>ChooseQueueOption(at),true,queuePickerFocus==i);if(queuePickerFocus==i)DrawMenuNavigationFocus(r,choices[i].title);}
  GUI.EndScrollView();GUI.Label(new Rect(468,733,660,24),"B / Escape returns without changing the selection.",ArchiveType(13,archiveMuted));
 }
 void QueueButton(Rect rect,string title,Action action,bool enabled=true,bool active=false,float listY=-1){int index=buttons.Count;bool available=enabled&&queuePicker==null;ArchiveButton(rect,title,action,available,active);if(available&&selection==index){if(queueFollowFocus&&listY>=0){if(listY<settlementQueueScroll.y)settlementQueueScroll.y=listY;else if(listY+70>settlementQueueScroll.y+416)settlementQueueScroll.y=listY+70-416;}}}
 SettlementJob SelectedQueuePreview(){
  var items=QueueItems();queueItem=Mathf.Clamp(queueItem,0,items.Length-1);int kind=queueKind>=7?queueKind-1:queueKind>=5?5:queueKind;string branch=kind<=1?items[queueItem]:"";int target=queueKind==5||queueKind==6?queueGearTier:0;
  if(kind<=1){target=kind==0?city.BuildingLevel(branch):city.ResearchRank(branch);if(kind==0&&city.activeConstruction==branch)target=Math.Max(target,city.constructionTarget);if(kind==1&&city.activeResearch==branch)target=Math.Max(target,city.researchTarget);foreach(var job in city.settlementQueue)if(job.kind==kind&&job.branch==branch)target=Math.Max(target,job.target);target++;}
  return new SettlementJob{kind=kind,branch=branch,index=queueKind>=7?queueItem:queueKind>=5?queueKind-4:queueItem,slot=queueKind>=5?queueItem:queueSlot,quantity=kind<=1?1:queueAmount,target=target};
 }
 void DrawQueueGoalBuilder(){
  ArchivePlate(new Rect(1110,210,420,550),0,14);GUI.Label(new Rect(1132,230,374,30),"ADD A GOAL",ArchiveType(19,archiveGold,true));
  QueueButton(new Rect(1130,280,378,42),QueueKinds[queueKind]+"  ▾",()=>OpenQueuePicker("category"));var items=QueueItems();queueItem=Mathf.Clamp(queueItem,0,items.Length-1);float y=337;
  QueueButton(new Rect(1130,y,378,44),QueueItemTitle(items[queueItem])+"  ▾",()=>OpenQueuePicker("item"));y+=57;
  if(queueKind==3){QueueButton(new Rect(1130,y,378,39),Gear.Slots[queueSlot]+"  ▾",()=>OpenQueuePicker("slot"));y+=52;}
  if(queueKind==5||queueKind==6){QueueButton(new Rect(1130,y,378,39),"Tier "+queueGearTier+"  ▾",()=>OpenQueuePicker("tier"));y+=52;}
  if(queueKind>=2){GUI.Label(new Rect(1133,y,374,26),"QUANTITY / "+queueAmount,ArchiveType(15,archiveGold,true));y+=33;QueueButton(new Rect(1130,y,86,32),"−10",()=>queueAmount=Math.Max(1,queueAmount-10));QueueButton(new Rect(1225,y,86,32),"−",()=>queueAmount=Math.Max(1,queueAmount-1));QueueButton(new Rect(1320,y,86,32),"+",()=>queueAmount=Math.Min(999,queueAmount+1));QueueButton(new Rect(1415,y,93,32),"+10",()=>queueAmount=Math.Min(999,queueAmount+10));y+=47;}
  var preview=SelectedQueuePreview();string reason=city.GoalRequirementPreview(preview);if(preview.kind==0&&preview.target>city.BuildingLimit(preview.branch)||preview.kind==1&&preview.target>LibraryResearch.Limit(preview.branch))reason="All levels are already built, learned or queued.";
  GUI.Label(new Rect(1133,y,374,26),"REQUIRED MATERIALS",ArchiveType(12,archiveGold,true));y+=29;
  if(reason.Contains("All levels"))GUI.Label(new Rect(1133,y,374,50),reason,ArchiveType(13,archiveMuted));else {DrawWorkshopCostIcons(new Rect(1132,y,374,64),city.GoalIngredientsPreview(preview));GUI.Label(new Rect(1133,y+72,374,60),reason==""?"Missing supplies will be gathered and processed automatically.":"Will wait: "+reason,ArchiveType(13,archiveMuted));}
  QueueButton(new Rect(1130,701,378,35),"Add to queue",AddSelectedQueueGoal,city.settlementQueue.Count<Settlement.SettlementQueueLimit&&!reason.Contains("All levels"));
 }
}}
