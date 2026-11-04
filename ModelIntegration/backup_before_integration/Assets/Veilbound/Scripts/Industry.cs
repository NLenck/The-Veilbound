using System;
using UnityEngine;
namespace Veilbound {
 public partial class Settlement {
  public int industryVersion,huntersLodge;
  public double[] ores,hides,textiles,mineEffort,huntEffort,textileEffort;
  public static readonly string[][] ResourceNames={new[]{"Tin ore","Copper ore","Zinc ore","Iron ore","Coal","Titanium ore","Tungsten ore","VeilCrystal"},new[]{"Rawhide","Cured hide","Thick hide","Primal hide","Veil hide"},new[]{"Flax","Wool","Silk","Ash silk","Veil fibre"}};
  public static readonly double[][] ResourceYields={new[]{.5d,.5,.375,.25,.5,.125,.1,.05},new[]{.5d,.375,.25,.125,.0625},new[]{.5d,.375,.25,.125,.0625}};
  public static readonly int[][] ResourceUnlocks={new[]{1,1,2,3,4,5,6,7},new[]{1,2,3,4,5},new[]{1,2,3,4,5}};
  public int ProducerLevel(int source)=>source==0?mine:source==1?huntersLodge:weaver;
  public double Capacity(int source)=>ProducerLevel(source)<=0?0:8+8*ProducerLevel(source);
  public void EnsureIndustry() {
   bool fresh=industryVersion==0;
   if(ores==null||ores.Length!=8)ores=new double[8];if(hides==null||hides.Length!=5)hides=new double[5];if(textiles==null||textiles.Length!=5)textiles=new double[5];
   if(mineEffort==null||mineEffort.Length!=8)mineEffort=new double[8];if(huntEffort==null||huntEffort.Length!=5)huntEffort=new double[5];if(textileEffort==null||textileEffort.Length!=5)textileEffort=new double[5];
   if(fresh){huntersLodge=Math.Max(huntersLodge,Math.Min(5,leatherworker));if(mine>0){mineEffort[0]=Capacity(0)/2;mineEffort[1]=Capacity(0)/2;}if(weaver>0)textileEffort[0]=Capacity(2)/2;if(huntersLodge>0)huntEffort[0]=Capacity(1)/2;industryVersion=1;}
   for(int source=0;source<3;source++){var a=Allocation(source);double left=Capacity(source);for(int i=0;i<a.Length;i++){if(double.IsNaN(a[i])||double.IsInfinity(a[i])||a[i]<0||ProducerLevel(source)<ResourceUnlocks[source][i])a[i]=0;a[i]=Math.Min(a[i],left);left-=a[i];}}
  }
  public double[] Allocation(int source)=>source==0?mineEffort:source==1?huntEffort:textileEffort;
  public double UsedCapacity(int source){EnsureIndustry();double sum=0;foreach(double n in Allocation(source))sum+=n;return sum;}
  public bool Allocate(int source,int index,double amount) {
   EnsureIndustry();if(source<0||source>2||index<0||index>=ResourceNames[source].Length||amount<0||double.IsNaN(amount)||double.IsInfinity(amount)||ProducerLevel(source)<ResourceUnlocks[source][index])return false;
   var a=Allocation(source);double used=0;for(int i=0;i<a.Length;i++)if(i!=index)used+=a[i];if(used+amount>Capacity(source)+1e-9)return false;a[index]=amount;return true;
  }
  public double ProductionRate(int source,int index){EnsureIndustry();return Allocation(source)[index]*ResourceYields[source][index];}
  public double TotalProduction(int source){EnsureIndustry();double n=0;for(int i=0;i<ResourceNames[source].Length;i++)n+=Allocation(source)[i]*ResourceYields[source][i];return n;}
  public string ResourceName(int family,int index)=>family==3?Metals.All[index].name+" bar":ResourceNames[family][index];
  public double ResourceAmount(int family,int index){EnsureIndustry();if(family==3){EnsureGear();return metalBars[index];}if(family==0)return index==3?iron:ores[index];if(family==1)return index==0?leather:hides[index];return index==0?cloth:textiles[index];}
  public void AddResource(int family,int index,double delta){if(family==3){metalBars[index]=Math.Max(0,metalBars[index]+delta);return;}if(family==0){if(index==3)iron=Math.Max(0,iron+delta);else ores[index]=Math.Max(0,ores[index]+delta);}else if(family==1){if(index==0)leather=Math.Max(0,leather+delta);else hides[index]=Math.Max(0,hides[index]+delta);}else{if(index==0)cloth=Math.Max(0,cloth+delta);else textiles[index]=Math.Max(0,textiles[index]+delta);}}
  public double ResourceTotal(int family){EnsureIndustry();double n=0;int count=family==3?10:ResourceNames[family].Length;for(int i=0;i<count;i++)n+=ResourceAmount(family,i);return n;}
  public void TickIndustry(double hours){EnsureIndustry();for(int source=0;source<3;source++)for(int i=0;i<ResourceNames[source].Length;i++)AddResource(source,i,hours*Allocation(source)[i]*ResourceYields[source][i]);}
 }
}
