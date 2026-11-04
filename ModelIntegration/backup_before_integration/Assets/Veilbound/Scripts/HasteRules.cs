using UnityEngine;
namespace Veilbound {
 public static class HasteRules {
  static readonly float[] Spans={40,20,10,10,10};
  static readonly float[] Costs={1,3,6,12,24};
  // Input is percent rating. Output is the effective fraction removed from the base interval.
  public static float Reduction(float rating) {
   if(float.IsNaN(rating)||rating<=0)return 0;
   float result=0;
   for(int i=0;i<Spans.Length;i++){float spent=Mathf.Min(rating,Spans[i]*Costs[i]);result+=spent/Costs[i];rating-=spent;if(rating<=0)return result*.01f;}
   // Above 90%, cost is 96:1. A safety ceiling prevents a zero or negative interval.
   return Mathf.Min(.999f,(result+rating/96)*.01f);
  }
 }
}
