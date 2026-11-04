using UnityEngine;
namespace Veilbound {
 public static class ExpeditionSpawns {
  public static int Stage(float seconds,float firstBoss,float secondBoss) {
   if(secondBoss>=0&&seconds>=secondBoss+30)return 4;
   if(firstBoss>=0&&seconds>=firstBoss+30)return 3;
   return seconds>=120?2:seconds>=60?1:0;
  }
  // Each row totals 100. Heavy enemies replace normal spawns rather than adding to them.
  static readonly int[][] Weights={new[]{100,0,0,0,0,0},new[]{75,15,10,0,0,0},new[]{65,15,12,8,0,0},new[]{60,15,12,8,5,0},new[]{53,15,14,11,5,2}};
  public static void Pick(int map,int stage,float roll,int variation,int brutes,int bulwarks,out int kind,out int variant,float eliteFrequency=1) {
   int[] w=Weights[Mathf.Clamp(stage,0,4)];float heavy=w[4]+w[5];
   float heavyScale=map==1?Mathf.Clamp(eliteFrequency,1,100/Mathf.Max(1,heavy)):1;
   float normalScale=(100-heavy*heavyScale)/(100-heavy);int group=0;
   for(;group<5;group++){float weight=w[group]*(group>=4?heavyScale:normalScale);if(roll<weight)break;roll-=weight;}
   kind=group==0?0:group==1?1:group==2?3:group==3?4:2;variant=0;
   if(group>=4){bool bulwark=group==5;if(brutes+bulwarks>=3||(!bulwark&&brutes>=3)||(bulwark&&bulwarks>=1)){kind=0;return;}variant=bulwark?1:0;return;}
   if(map==1){if(stage>=4&&kind==0&&variation<10)variant=1;else if(stage>=4&&kind==1&&variation<20)variant=1;else if(stage>=3&&kind==3)variant=variation<70?0:variation<85?1:2;}
   else if(stage>=3&&(kind==0||kind==1||kind==3))variant=variation<80?0:1;
  }
 }
}
