using System;
using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  string SmithBarName(int metal)=>metal==9?"Veilcrystal / Titanium base":metal==10?"Veilcrystal / Tungsten base":Metals.All[metal].name+" bars";
  bool smithScheduleView;
  void AddSmithRepeat(){if(city.AddRepeatStep(true,smithBarRecipe,SmithQuantity,out smithFeedback)){smithScheduleView=true;Save();}}
  string SmithAutoStatus=>city.barSchedule.Count==0?"Repeat schedule empty":!city.autoBarsEnabled?"Repeat schedule paused":city.smithOrders.Count>0?"Repeat schedule resumes after queued orders":city.AutoBarsReady?"Repeat "+(city.barScheduleIndex+1)+"/"+city.barSchedule.Count+": "+SmithBarName(city.autoBarMetal)+" / "+city.barStepCompleted+" of "+city.BarStep.quantity+" / next in "+ProductionTime.Duration(city.NextBarHours)+" city time":"Repeat "+(city.barScheduleIndex+1)+"/"+city.barSchedule.Count+": "+SmithBarName(city.autoBarMetal)+" / waiting for ingredients or research";
  
  
 }
}
