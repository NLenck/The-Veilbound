using System;
using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  void ProducerStockNotes(){
   GUI.Label(new Rect(91,674,680,22),"MATERIAL STOCKPILE",ArchiveType(13,archiveGold,true));GUI.Label(new Rect(94,709,964,26),"Total stored: "+Math.Floor(city.ResourceTotal(producerSource)).ToString("N0")+" units / select a material to inspect its stock and production time.",ArchiveType(14,archiveIvory));GUI.Label(new Rect(94,753,964,37),producerSource==0?"Ore arrives in whole units. Queued batches run first; the repeat schedule resumes afterward.":"All assigned materials produce together. Fractional progress is saved; the stock bar shows completed whole units.",ArchiveType(12,archiveMuted));
  }
  void ProducerMineOrders(){
   ArchivePlate(new Rect(77,660,1028,145),0,10);if(producerPage==2){ProducerStockNotes();return;}
   if(producerPage==1){ProducerMineSchedule();return;}GUI.Label(new Rect(91,674,680,22),"WORK ORDERS / "+city.mineOrders.Count+" OF 5",ArchiveType(13,archiveGold,true));
   if(city.mineOrders.Count==0){GUI.Label(new Rect(94,712,964,58),city.AutoMineReady?"Repeat schedule active: "+Settlement.ResourceNames[0][city.autoMineResource]+" / next unit in "+ProductionTime.Duration(city.NextMineHours)+" city time.":"No queued work orders. Choose a material and quantity, then queue it or add it to the repeating schedule.",ArchiveType(14,archiveMuted));return;}
   KeepMenuFocusVisible(ref producerScroll,buttons.Count,city.mineOrders.Count,31,93);producerScroll=GUI.BeginScrollView(new Rect(89,703,1002,93),producerScroll,new Rect(0,0,982,city.mineOrders.Count*31),false,false,GUIStyle.none,GUIStyle.none);
   for(int i=0;i<city.mineOrders.Count;i++){var order=city.mineOrders[i];int id=order.id;float y=i*31;GUI.Label(new Rect(7,y+8,69,20),i==0?"ACTIVE":"QUEUED",ArchiveType(10,i==0?archiveGold:archiveMuted,true));var icon=MaterialIcon(ResourceIconNames[0][order.resource]);if(icon)GUI.DrawTexture(new Rect(75,y+3,36,29),icon,ScaleMode.ScaleToFit);GUI.Label(new Rect(120,y+4,390,27),Settlement.ResourceNames[0][order.resource]+" / "+order.completed+" of "+order.quantity,ArchiveType(12,archiveIvory));
    if(i==0){ArchivePlate(new Rect(532,y+10,210,7),3,2);Box(new Rect(534,y+12,206*Mathf.Clamp01(1-(float)(order.remainingWork/order.workPerUnit)),3),archiveRed);GUI.Label(new Rect(753,y+6,120,22),"Next "+ProductionTime.Duration(city.NextMineHours),ArchiveType(11,archiveMuted));}else GUI.Label(new Rect(532,y+6,330,22),"Waiting for previous order",ArchiveType(11,archiveMuted));ProducerButton(new Rect(879,y+1,90,29),"Cancel",()=>{city.CancelMining(id);Save();producerFeedback="Order cancelled; delivered ore kept.";});
   }GUI.EndScrollView();
  }
  void ProducerMineSchedule(){
   var list=city.mineSchedule;GUI.Label(new Rect(91,674,680,22),"REPEAT SCHEDULE / "+list.Count+" STEPS",ArchiveType(13,archiveGold,true));ProducerButton(new Rect(981,669,108,28),city.autoMineEnabled?"Pause":"Run",()=>{city.PauseRepeat(false);Save();},list.Count>0);
   ProducerButton(new Rect(658,669,310,28),city.mineScheduleManualPriority?"Mode: Manual priority":"Mode: Fallback",()=>{city.mineScheduleManualPriority=!city.mineScheduleManualPriority;Save();});
   if(list.Count==0){GUI.Label(new Rect(94,713,964,55),"Choose materials and quantities, then Add to repeat schedule. Steps run in order and loop after the last step.",ArchiveType(14,archiveMuted));return;}
   KeepReorderFocusVisible(ref producerScroll,buttons.Count,list.Count,31,93);producerScroll=GUI.BeginScrollView(new Rect(89,703,1002,93),producerScroll,new Rect(0,0,982,list.Count*31),false,false,GUIStyle.none,GUIStyle.none);
   for(int i=0;i<list.Count;i++){int index=i;var step=list[i];bool current=i==city.mineScheduleIndex;float y=i*31;GUI.Label(new Rect(7,y+7,65,22),(current?"> ":"")+(i+1),ArchiveType(12,current?archiveGold:archiveMuted));var icon=MaterialIcon(ResourceIconNames[0][step.resource]);if(icon)GUI.DrawTexture(new Rect(75,y+3,36,29),icon,ScaleMode.ScaleToFit);GUI.Label(new Rect(120,y+3,440,28),Settlement.ResourceNames[0][step.resource]+" x"+step.quantity,ArchiveType(12,archiveIvory));
    GUI.Label(new Rect(562,y+7,246,22),current?city.mineStepCompleted+" done / "+(city.mineOrders.Count>0?"queued orders first":!city.autoMineEnabled?"paused":city.AutoMineReady?"next "+ProductionTime.Duration(city.NextMineHours):"research locked"):"Next in sequence",ArchiveType(11,archiveMuted));
    ProducerButton(new Rect(812,y+1,28,29),"↑",()=>{city.MoveRepeatStep(false,index,-1);Save();},i>0);ProducerButton(new Rect(844,y+1,28,29),"↓",()=>{city.MoveRepeatStep(false,index,1);Save();},i<list.Count-1);ProducerButton(new Rect(879,y+1,90,29),"Remove",()=>{city.RemoveRepeatStep(false,index);Save();});
   }GUI.EndScrollView();
  }
  void ProducerAllocations(){if(producerPage==0){DrawGatherProductionSlots();return;}
   ArchivePlate(new Rect(77,660,1028,145),0,10);if(producerPage==1){ProducerStockNotes();return;}
   GUI.Label(new Rect(91,674,700,22),"ACTIVE PRODUCTION / "+city.UsedCapacity(producerSource).ToString("0.##")+" OF "+city.Capacity(producerSource).ToString("0.##")+" CAPACITY",ArchiveType(13,archiveGold,true));ProducerButton(new Rect(943,669,146,28),"Clear allocation",()=>{Array.Clear(city.Allocation(producerSource),0,city.Allocation(producerSource).Length);producerAmount="0";Save();},city.UsedCapacity(producerSource)>0);
   int row=0;producerScroll=GUI.BeginScrollView(new Rect(89,703,1002,93),producerScroll,new Rect(0,0,982,Settlement.ResourceNames[producerSource].Length*34),false,false,GUIStyle.none,GUIStyle.none);
   for(int i=0;i<Settlement.ResourceNames[producerSource].Length;i++){double assigned=city.Allocation(producerSource)[i];if(assigned<=0)continue;int index=i;float y=row++*34;double stock=city.ResourceAmount(producerSource,i),rate=city.ProductionRate(producerSource,i);GUI.Label(new Rect(8,y+5,341,24),Settlement.ResourceNames[producerSource][i],ArchiveType(13,archiveIvory));GUI.Label(new Rect(355,y+5,223,24),"Capacity "+assigned.ToString("0.##"),ArchiveType(12,archiveMuted));GUI.Label(new Rect(579,y+5,285,24),"Next "+ProductionTime.Duration(ProductionTime.NextUnit(stock,rate)),ArchiveType(12,archiveIvory));ProducerButton(new Rect(879,y+1,90,28),"Inspect",()=>ProducerSelect(index));}
   if(row==0)GUI.Label(new Rect(8,12,900,44),"No manual capacity assigned. Queued goals use spare capacity automatically.",ArchiveType(14,archiveMuted));GUI.EndScrollView();
  }
  void CityProducerOverview(int source){
   string activity=BuildingActivity(cityBuilding,out string detail);GUI.Label(new Rect(1100,383,450,30),source==0?"MINING WORK ORDERS":"RESOURCE PRODUCTION",CityType(18,cityIvory,true));GUI.Label(new Rect(1100,425,450,73),detail!=""?detail.Split('\n')[0]:"No active production. Open this building to choose materials and production settings.",CityType(16,muted));CityButton(1100,521,450,45,"Open "+(source==0?"Mine":source==1?"Hunter's Cabin":"Textile Tavern"),()=>OpenProducer(source));CityButton(1100,582,450,45,source==0?"Go to Blacksmith":"Go to Clothier",()=>SelectCityBuilding(source==0?"blacksmith":"clothier"));GUI.Label(new Rect(1100,657,450,104),source==0?"Manage quantity work orders, repeating schedules, ore stocks, research locks and time per unit in the Mine.":"Manage material stocks, production allocations, research locks and time per unit. Your existing assignments continue producing during expeditions.",CityType(15,muted));
  }
 }
}
