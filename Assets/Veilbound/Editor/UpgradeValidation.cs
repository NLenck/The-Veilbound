using System;
using Veilbound;
using UnityEngine;

namespace Veilbound.Editor
{
    public static class UpgradeValidation
    {
        static void Check(bool condition,string message){if(!condition)throw new Exception("Upgrade check failed: "+message);}
        public static void Run()
        {
            double[] baseline={.5,.25,.15,.08,.02};var zero=Rarities.Chances(0);
            for(int i=0;i<5;i++)Check(Math.Abs(zero[i]-baseline[i])<1e-10,"Zero luck matches specified base odds");
            double previousCommon=1,previousUncommon=1,previousLegendary=0;
            foreach(int luck in new[]{0,10,50,100,200,500,1000,1000000})
            {
                var odds=Rarities.Chances(luck);double total=0;
                foreach(double p in odds){Check(p>=0&&!double.IsNaN(p)&&!double.IsInfinity(p),"Finite nonnegative probabilities");total+=p;}
                Check(Math.Abs(total-1)<1e-10,"Probabilities sum to one");
                Check(odds[0]<=previousCommon&&odds[1]<=previousUncommon&&odds[4]>=previousLegendary,"Luck reduces both lower tiers and raises Legendary");
                previousCommon=odds[0];previousUncommon=odds[1];previousLegendary=odds[4];
            }
            foreach(int luck in new[]{0,100,500})
            {
                var rng=new System.Random(481+luck);var expected=Rarities.Chances(luck);var counts=new int[5];
                for(int i=0;i<100000;i++)counts[(int)Rarities.Roll(rng,luck)]++;
                for(int i=0;i<5;i++)Check(Math.Abs(counts[i]/100000d-expected[i])<.006,"Sampled rolls match their distribution");
            }
            var random=new System.Random(37);var build=new RunBuild();
            Check(build.WeaponSlots==1&&build.VirtueSlots==0&&build.Slots==1&&build.Rank("rage")==1,"Basic weapon occupies one slot; active Fury is separate");
            for(int level=2;level<=22;level++) {
                bool acquisition=level%2==0&&Array.Exists(RunBuild.Pool,c=>c.ability&&build.Rank(c.id)==0&&build.Eligible(c));
                build.Roll(random,level);Check(build.offers.Count==3,"Three distinct choices remain available");
                Check(build.offers[0].Key!=build.offers[1].Key&&build.offers[1].Key!=build.offers[2].Key&&build.offers[0].Key!=build.offers[2].Key,"Offers have unique ability/attribute keys");
                if(!acquisition)foreach(var c in build.offers)Check(!c.ability||build.Rank(c.id)>0,"Upgrade rounds do not unlock another ability");
                var chosen=acquisition?build.offers.Find(c=>c.ability&&build.Rank(c.id)==0):build.offers[0];Check(chosen!=null,"Acquisition rounds offer a new ability");build.Pick(chosen);
                Check(build.Slots<=5,"Weapon and Virtue abilities share five equipped slots");
            }
            Check(build.Slots==5,"Acquisitions stop at five equipped abilities; active Fury remains separate");
            var heldBuild=new RunBuild();heldBuild.Roll(random,2);var held=heldBuild.offers[0];heldBuild.Lock(held);
            heldBuild.Roll(random);Check(heldBuild.offers.Contains(held),"Reroll preserves exact locked rarity");
            heldBuild.Pick(heldBuild.offers.Find(c=>c.id!=held.id));heldBuild.Roll(random,3);
            Check(heldBuild.locked==held.id&&!heldBuild.offers.Contains(held),"Off-cycle acquisition lock is deferred");
            heldBuild.Roll(random,4);Check(heldBuild.offers.Contains(held),"Lock returns on next eligible round");
            var common=new RunBuild();var legendary=new RunBuild();
            common.Pick(new Choice("power","",""));legendary.Pick(new Choice("power","",""){rarity=UpgradeRarity.Legendary});
            Check(common.Amount("power")==1&&legendary.Amount("power")==2.5f,"Rarity changes applied strength, not just colour");
            legendary.Pick(new Choice("luck","",""){rarity=UpgradeRarity.Rare});Check(legendary.Luck==15,"Luck upgrade applies rarity strength");
            Check(RunBuild.Pool[0].rarity==UpgradeRarity.Common,"Rolling never mutates shared card definitions");
            Debug.Log("VEILBOUND_UPGRADE_CHECKS_PASS");
        }
    }
}
