using System;
using UnityEngine;
namespace Veilbound.Editor {
 public static class ExpeditionFlowValidation {
  static void Check(bool ok,string message){if(!ok)throw new Exception("Expedition flow: "+message);}
  public static void Run(){
   Check(ExpeditionSpawns.Stage(59.99f,-1,-1)==0&&ExpeditionSpawns.Stage(60,-1,-1)==1&&ExpeditionSpawns.Stage(120,-1,-1)==2,"early introduction boundaries");
   Check(ExpeditionSpawns.Stage(900,-1,-1)==2,"late enemies wait for a successful boss spawn");
   Check(ExpeditionSpawns.Stage(349.99f,320,-1)==2&&ExpeditionSpawns.Stage(350,320,-1)==3,"first boss unlock uses actual arrival plus thirty seconds");
   Check(ExpeditionSpawns.Stage(649.99f,300,620)==3&&ExpeditionSpawns.Stage(650,300,620)==4,"second boss unlock boundary");
   for(int stage=0;stage<5;stage++){
    int heavy=0,bulwarks=0;
    for(int roll=0;roll<100;roll++)for(int v=0;v<100;v++){
     ExpeditionSpawns.Pick(1,stage,roll,v,0,0,out int kind,out int variant);
     if(stage==0)Check(kind==0&&variant==0,"opening contains only Husks");
     if(stage<2)Check(kind!=4,"no early bloater");if(stage<3)Check(kind!=2&&variant==0,"no early heavy or advanced visual variant");
     if(kind==2){heavy++;if(variant==1)bulwarks++;}
     string id=ProductionEnemySelection.Pick(1,kind,0,variant);Check(!id.StartsWith("Forest/"),"village pool excludes forest wolf");
    }
    Check(heavy==(stage==4?700:stage==3?500:0)&&bulwarks==(stage==4?200:0),"heavy weights are five percent Brute and two percent Bulwark");
   }
   for(int roll=0;roll<100;roll++){
    ExpeditionSpawns.Pick(1,4,roll,0,2,1,out int kind,out int variant);Check(kind!=2,"combined cap of three");
    ExpeditionSpawns.Pick(1,4,99,0,0,1,out kind,out variant);Check(kind==0&&variant==0,"second Bulwark becomes a Husk");
   }
   Debug.Log("VEILBOUND_EXPEDITION_FLOW_POLICY_PASS");
  }
 }
}
