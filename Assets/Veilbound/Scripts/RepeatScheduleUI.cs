using System;
using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  void CityMineSchedule(){
   var list=city.mineSchedule;mineQueuePage=Mathf.Clamp(mineQueuePage,0,Math.Max(0,list.Count-2));
   if(list.Count==0)GUI.Label(new Rect(1098,697,454,65),"Choose a material and quantity, then Add to repeat. Steps run in order and repeat after the last step.",CityType(13,cityIvory));
   for(int row=0;row<2;row++){int at=mineQueuePage+row;if(at>=list.Count)break;int index=at;var step=list[at];float y=690+row*38;bool current=at==city.mineScheduleIndex;
    GUI.Label(new Rect(1098,y,285,20),(current?"> ":"")+(at+1)+". "+Settlement.ResourceNames[0][step.resource]+" ×"+step.quantity,CityType(12,current?cityIvory:muted));
    GUI.Label(new Rect(1098,y+18,285,18),current?city.mineStepCompleted+" complete / "+(city.mineOrders.Count>0?"queued orders first":!city.autoMineEnabled?"paused":city.AutoMineReady?"next in "+ProductionTime.Duration(city.NextMineHours):"research locked"):"Then next step; repeat after last",CityType(10,muted));
    CityButton(1385,y+3,27,26,"↑",()=>{city.MoveRepeatStep(false,index,-1);Save();},at>0);CityButton(1417,y+3,27,26,"↓",()=>{city.MoveRepeatStep(false,index,1);Save();},at<list.Count-1);CityButton(1450,y+3,102,26,"Remove",()=>{city.RemoveRepeatStep(false,index);Save();});
   }
   if(list.Count>2){CityButton(1098,770,66,23,"Earlier",()=>mineQueuePage--,mineQueuePage>0);CityButton(1173,770,66,23,"Later",()=>mineQueuePage++,mineQueuePage<list.Count-2);}
   GUI.Label(new Rect(1251,772,301,21),list.Count==0?"Schedule empty":(city.autoMineEnabled?"Repeating":"Paused")+" / step "+(city.mineScheduleIndex+1)+" of "+list.Count,CityType(11,muted,false,TextAnchor.UpperRight));
  }
  
 }
}
