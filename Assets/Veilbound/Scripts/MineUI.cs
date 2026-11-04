using System;
using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  int mineSelection,mineQueuePage;string mineAmountText="10";bool mineEditingQuantity;
  int MineQuantity=>int.TryParse(mineAmountText,out int n)?n:0;
  void QueueMineSelection(){if(city.QueueMining(mineSelection,MineQuantity,out notice))Save();}
  bool mineScheduleView;
  void AddMineRepeat(){if(city.AddRepeatStep(false,mineSelection,MineQuantity,out notice)){mineScheduleView=true;Save();}}
  void CityMineDetails(){
   city.EnsureMineOrders();int pages=2;cityResourcePage=Mathf.Clamp(cityResourcePage,0,pages-1);
   GUI.Label(new Rect(1098,400,454,22),"MINING / ONE UNIT AT A TIME",CityType(16,cityIvory,true));
   GUI.Label(new Rect(1098,428,454,22),"City time / Mine ×"+city.MiningSpeed.ToString("0.##")+" / Orders "+city.mineOrders.Count+" of 5",CityType(12,muted));
   for(int row=0;row<4;row++){
    int i=cityResourcePage*4+row;float y=459+row*42;string requirement=city.MiningRequirement(i);bool unlocked=requirement=="";var icon=MaterialIcon(ResourceIconNames[0][i]);
    if(icon){GUI.color=unlocked?Color.white:new Color(.4f,.4f,.4f);GUI.DrawTexture(new Rect(1100,y,31,31),icon);GUI.color=Color.white;}
    CityButton(1139,y,166,30,Settlement.ResourceNames[0][i],()=>mineSelection=i,true,mineSelection==i);
    GUI.Label(new Rect(1312,y+3,70,25),Math.Floor(city.ResourceAmount(0,i)).ToString("N0"),CityType(12,cityIvory,false,TextAnchor.MiddleRight));
    GUI.Label(new Rect(1385,y+3,167,25),unlocked?"1 / "+ProductionTime.Duration(Settlement.MineUnitWork(i)/city.MiningSpeed):"Locked / tier "+Settlement.ResourceUnlocks[0][i],CityType(12,unlocked?cityIvory:muted,false,TextAnchor.MiddleRight));
   }
   CityButton(1098,630,66,24,"Previous",()=>{cityResourcePage--;mineSelection=cityResourcePage*4;},cityResourcePage>0);CityButton(1173,630,66,24,"Next",()=>{cityResourcePage++;mineSelection=cityResourcePage*4;},cityResourcePage<pages-1);
   if(cityDetailTab==1){GUI.Label(new Rect(1098,677,454,68),"Mining orders deliver whole units. Stored ore stays available for crafting. Select Work orders to choose what to mine next.",CityType(14,muted));return;}
   CityButton(1260,630,28,24,"−",()=>mineAmountText=Math.Max(1,MineQuantity-1).ToString());
   Rect input=new Rect(1291,630,52,24);if(Event.current.type==EventType.MouseDown)mineEditingQuantity=input.Contains(Event.current.mousePosition);GUI.SetNextControlName("Mine order quantity");mineAmountText=GUI.TextField(input,mineAmountText,3,CityType(14,cityIvory,false,TextAnchor.MiddleCenter));
   CityButton(1347,630,28,24,"+",()=>mineAmountText=Math.Min(999,MineQuantity+1).ToString());
   CityButton(1385,630,167,24,"Queue ×"+MineQuantity,QueueMineSelection,city.MiningRequirement(mineSelection)==""&&city.mineOrders.Count<Settlement.MineQueueLimit&&MineQuantity>0&&MineQuantity<=999);
   string locked=city.MiningRequirement(mineSelection);CityButton(1098,660,178,24,"Add ×"+MineQuantity+" to repeat",AddMineRepeat,locked==""&&MineQuantity>0&&MineQuantity<=999&&city.mineSchedule.Count<Settlement.RepeatStepLimit);CityButton(1284,660,72,24,city.autoMineEnabled?"Pause":"Run",()=>{city.PauseRepeat(false);Save();},city.mineSchedule.Count>0);CityButton(1364,660,188,24,mineScheduleView?"Show queued orders":"Repeat schedule ("+city.mineSchedule.Count+")",()=>{mineScheduleView=!mineScheduleView;mineQueuePage=0;});
   if(mineScheduleView){CityMineSchedule();return;}
   mineQueuePage=Mathf.Clamp(mineQueuePage,0,Math.Max(0,city.mineOrders.Count-2));
   if(city.mineOrders.Count==0){string status=city.AutoMineReady?"Auto: "+Settlement.ResourceNames[0][city.autoMineResource]+" / next in "+ProductionTime.Duration(city.NextMineHours):city.autoMineEnabled?city.MiningRequirement(city.autoMineResource):city.mineSchedule.Count>0?"Repeat schedule paused.":"Idle. Queue an amount or create a repeating schedule.";GUI.Label(new Rect(1098,694,454,40),status,CityType(14,cityIvory));} 
   for(int row=0;row<2;row++){
    int at=mineQueuePage+row;if(at>=city.mineOrders.Count)break;var o=city.mineOrders[at];int id=o.id;float y=689+row*39;
    GUI.Label(new Rect(1098,y,351,20),(at==0?"Active: ":"Queued: ")+Settlement.ResourceNames[0][o.resource]+" / "+o.completed+" of "+o.quantity,CityType(12,cityIvory));
    GUI.Label(new Rect(1098,y+18,351,19),at==0?"Next unit in "+ProductionTime.Duration(o.remainingWork/city.MiningSpeed):"Waiting for previous orders",CityType(11,muted));
    if(at==0){Box(new Rect(1385,y+22,67,4),new Color(.12f,.13f,.12f));Box(new Rect(1385,y+22,67*Mathf.Clamp01(1-(float)(o.remainingWork/o.workPerUnit)),4),cityRed);}
    CityButton(1468,y+2,84,28,"Cancel",()=>{city.CancelMining(id);Save();notice="Mining order cancelled. Delivered ore kept.";});
   }
   if(city.mineOrders.Count>2){CityButton(1098,770,66,23,"Earlier",()=>mineQueuePage--,mineQueuePage>0);CityButton(1173,770,66,23,"Later",()=>mineQueuePage++,mineQueuePage<city.mineOrders.Count-2);}
   GUI.Label(new Rect(1251,772,301,21),city.mineOrders.Count==0?(city.autoMineEnabled?"Repeat: step "+(city.mineScheduleIndex+1)+" / "+city.mineSchedule.Count:"Queue empty"):"Queue: "+ProductionTime.Duration(city.MineQueueHours())+" city time",CityType(11,muted,false,TextAnchor.UpperRight));
  }
 }
}
