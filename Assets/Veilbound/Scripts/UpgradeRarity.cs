using System;
using UnityEngine;

namespace Veilbound
{
    public enum UpgradeRarity { Common, Uncommon, Rare, Epic, Legendary }
    public static class Rarities
    {
        static readonly double[] baseline={.50,.25,.15,.08,.02};
        public static double[] Chances(double luck)
        {
            // Both lower tiers keep their relative weight; luck boosts each upper tier progressively.
            double boost=1+Math.Max(0,Math.Min(1000000,luck))/100;
            var weights=new double[5];double total=0;
            for(int i=0;i<5;i++){weights[i]=baseline[i]*Math.Pow(boost,Math.Max(0,i-1));total+=weights[i];}
            for(int i=0;i<5;i++)weights[i]/=total;
            return weights;
        }
        public static UpgradeRarity Roll(System.Random random,float luck)
        {
            double roll=random.NextDouble();var chances=Chances(luck);
            for(int i=0;i<4;i++){roll-=chances[i];if(roll<0)return (UpgradeRarity)i;}
            return UpgradeRarity.Legendary;
        }
        public static float Strength(UpgradeRarity rarity)=>rarity==UpgradeRarity.Common?1:rarity==UpgradeRarity.Uncommon?1.25f:rarity==UpgradeRarity.Rare?1.5f:rarity==UpgradeRarity.Epic?1.85f:2.5f;
        public static Color Colour(UpgradeRarity rarity)=>rarity==UpgradeRarity.Common?new Color(.75f,.76f,.76f):rarity==UpgradeRarity.Uncommon?new Color(.30f,.72f,.39f):rarity==UpgradeRarity.Rare?new Color(.25f,.55f,.95f):rarity==UpgradeRarity.Epic?new Color(.92f,.20f,.24f):new Color(1,.67f,.13f);
    }
}
