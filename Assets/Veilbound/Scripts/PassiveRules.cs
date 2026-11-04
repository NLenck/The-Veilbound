using System.Collections.Generic;
using UnityEngine;
namespace Veilbound {
 public static class PassiveRules {
  static readonly Dictionary<string,float[]> values=new Dictionary<string,float[]>{
   {"wisdom",new float[]{15,25,40,55,80}},
   {"crit",new float[]{1,2,3,4,5}},
   {"critdamage",new float[]{10,20,30,40,50}},
   {"haste",new float[]{2,4,6,8,10}},
   {"power",new float[]{4,8,12,16,20}},
   {"guard",new float[]{2,3,4,5,8}},
   {"speed",new float[]{3,6,9,12,15}}
  };
  public static bool Has(string id)=>values.ContainsKey(id);
  public static float Percent(string id,UpgradeRarity rarity)=>values.TryGetValue(id,out var row)?row[Mathf.Clamp((int)rarity,0,4)]:0;
 }
 public sealed partial class RunBuild {
  public float CritBasePercent=5;
  public float PassivePercent(string id){float total=0;if(passivePicks.TryGetValue(id,out var counts))for(int i=0;i<5;i++)total+=counts[i]*PassiveRules.Percent(id,(UpgradeRarity)i);return total;}
  string DescribePassive(Choice choice){
   float amount=PassiveRules.Percent(choice.id,choice.rarity);
   switch(choice.id){
    case "wisdom":return $"+{amount:0.#}% experience gained.";
    case "crit":return $"+{Mathf.Min(amount,Mathf.Max(0,100-CritBasePercent-PassivePercent("crit"))):0.#}% critical hit chance (maximum 100%).";
    case "critdamage":return $"+{amount:0.#}% critical damage.";
    case "haste":return $"+{amount:0.#}% Haste rating to all abilities. Diminishing returns apply after 40%.";
    case "power":return $"+{amount:0.#}% damage to all abilities.";
    case "guard":return $"+{Mathf.Min(amount,Mathf.Max(0,56-PassivePercent("guard"))):0.#}% damage reduction (maximum 56%).";
    case "speed":return $"+{amount:0.#}% movement speed, multiplied against your gear baseline.";
    default:return choice.description;
   }
  }
 }
}
