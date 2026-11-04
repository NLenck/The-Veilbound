using System;
using UnityEngine;
namespace Veilbound {
 public partial class Settlement {
  public int expeditionModifierVersion, expeditionBestDepth;
  public int[] expeditionRanks;
  public void EnsureExpeditionModifiers(){
   if(expeditionModifierVersion==0){expeditionBestDepth=Mathf.Clamp(clearedTier,0,100);expeditionModifierVersion=1;}
   expeditionBestDepth=Mathf.Clamp(expeditionBestDepth,0,100);
   if(expeditionRanks==null||expeditionRanks.Length!=10)expeditionRanks=new int[10];
   for(int i=0;i<10;i++)expeditionRanks[i]=Mathf.Clamp(expeditionRanks[i],0,ExpeditionColumns);
  }
  public int ExpeditionColumns=>Mathf.Clamp(1+(expeditionBestDepth>=5?1+(expeditionBestDepth-5)/10:0),1,10);
 }
 public static class ExpeditionModifiers {
  public static readonly string[] Names={"Enemy Health","Enemy Damage","Boss Health","Boss Damage","Boss Cooldown Reduction","Enemy Spawn Rate","Elite Spawn Frequency","Enemy Movement Speed","Enemy Attack / Cast Speed","Elite Health"};
  public static readonly int[] Percent={10,5,15,10,5,8,15,5,5,15};
  public static int Depth(int[] ranks){int n=0;foreach(int r in ranks)n+=r;return n;}
 }
 public partial class VeilGame {
  int modifierFocus;int[] activeExpeditionRanks;bool expeditionModifiersActive;
  int ModifierRank(int i){city.EnsureExpeditionModifiers();return expeditionModifiersActive&&activeExpeditionRanks!=null?activeExpeditionRanks[i]:city.expeditionRanks[i];}
  float ModifierFactor(int i)=>1+ModifierRank(i)*ExpeditionModifiers.Percent[i]*.01f;
  int VeilDepth {get{int n=0;for(int i=0;i<10;i++)n+=ModifierRank(i);return n;}}
  void SetModifierRank(int row,int rank){city.EnsureExpeditionModifiers();if(expeditionModifiersActive)return;city.expeditionRanks[row]=Mathf.Clamp(rank,0,city.ExpeditionColumns);modifierFocus=row;tier=Math.Max(1,VeilDepth);Save();}
  void BeginExpeditionModifiers(){city.EnsureExpeditionModifiers();activeExpeditionRanks=(int[])city.expeditionRanks.Clone();expeditionModifiersActive=true;tier=Math.Max(1,VeilDepth);}
  float RunModifierFactor(int row){
   float factor=ModifierFactor(row);
   if(row==0||row==2)factor=ExpeditionDifficulty.Health(VeilDepth)*factor;
   else if(row==1||row==3)factor=ExpeditionDifficulty.Damage(VeilDepth)*factor;
   else if(row==5)factor=ExpeditionDifficulty.SpawnRate(VeilDepth)*factor;
   else if(row==9)factor=ExpeditionDifficulty.Health(VeilDepth)*ModifierFactor(0)*factor;
   else if(row==4)factor=1-ModifierRank(4)*.05f;
   return factor;
  }
  string RunModifierSummary(int row){float percent=(RunModifierFactor(row)-1)*100;return (percent>0?"+":"")+percent.ToString("0.#")+"%";}
  float ExpeditionDamageFactor(Foe f)=>f==null?ExpeditionDifficulty.Damage(VeilDepth):RunModifierFactor(f.boss>0?3:1)*f.spawnDamageFactor*AberrationAgeFactor(f);
  float ExpeditionCooldownRate(Foe f)=>f.boss>0?1/(1-ModifierRank(4)*.05f):ModifierFactor(8);
 }
}
