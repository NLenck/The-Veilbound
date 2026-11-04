using System;
using System.Collections.Generic;
using UnityEngine;

namespace Veilbound
{
    [Serializable] public partial class Settlement
    {
        public int version = 1;
        public int bronzeHeroVersion;
        public double wood = 110, iron = 35, essence, hours;
        public int timber = 1, mine = 1, forge, armour, sword, equippedArmour, equippedSword, colour;
        public int clearedTier, victories;
        public const double CityHoursPerDay = 24;
        public const double PassiveProductionMultiplier = 1;
        // Public producer rates remain per city hour; configured yields are now per day.
        public double WoodRate => TimberDaily[Mathf.Clamp(timber,0,10)] * PassiveProductionMultiplier * Productivity / CityHoursPerDay;
        public double IronRate => ProductionRate(0,3);
        // One real minute = one city hour. A day takes 24 active minutes at x1. Fractional stock stays in saved doubles. All production uses the same expedition multiplier.
        public void Tick(double seconds, double multiplier)
        {
            double h = Math.Max(0, seconds) * multiplier / 60;
            TickTechnology(h);
        }
        public bool Spend(double w, double i, double e = 0)
        {
            if (wood + 0.0001 < w || iron + 0.0001 < i || essence + 0.0001 < e) return false;
            wood -= w; iron -= i; essence -= e; return true;
        }
    }

    public sealed class Choice
    {
        public string id, name, description;
        public string attribute;public int ascension;
        public string Key=>ascension>0?id+"/ascension/"+ascension:string.IsNullOrEmpty(attribute)?id:id+"/"+attribute;
        public bool virtue=>AbilitySpecs.IsVirtue(id);
        public bool ability;
        public UpgradeRarity rarity;
        public float Strength=>Rarities.Strength(rarity);
        public Choice(string id, string name, string description, bool ability = false)
        { this.id = id; this.name = name; this.description = description; this.ability = ability; }
    }

    public sealed partial class RunBuild
    {
        public readonly Dictionary<string, int> ranks = new Dictionary<string, int> { { "cleave", 1 }, { "rage", 1 } };
        public readonly HashSet<string> banished = new HashSet<string>();
        public readonly List<Choice> offers = new List<Choice>();
        public readonly Dictionary<string,int[]> passivePicks=new Dictionary<string,int[]>();
        readonly Dictionary<string,float> potency=new Dictionary<string,float>();
        Choice lockedOffer;
        Choice abilityLockedOffer,passiveLockedOffer;
        public bool PassiveRound {get;private set;}
        public int offerLevel=2;
        public float Amount(string id)=>Array.Exists(Pool,c=>c.id==id&&c.ability)?(Rank(id)>0?1+Attribute(id,"damage"):0):(potency.TryGetValue(id,out float value)?value:Rank(id));
        public float Luck=>Amount("luck")*10;
        public string locked;
        public int rerolls = 3, banishes = 2, locks = 1;
        public static readonly Choice[] Pool = {
            new Choice("cleave", "Sundering steel", "A directional greatsword swing." , true),
            new Choice("whirl", "Reaping circle", "Your equipped greatsword spins around you for three sweeping passes.", true),
            new Choice("lance", "Rift edge", "Send a piercing blade in your facing direction.", true),
            new Choice("orbit", "Oathblades", "A copy of your equipped weapon spins as it orbits you.", true),
            new Choice("smite", "Hollow judgement", "Automatically strike the nearest enemy.", true),
            new Choice("rage", "Berserker's Fury", "At 100 Rage, explode for 100% weapon damage and gain +25% damage and +30% Haste rating for 6 seconds. Rage drains; no gain while active.",true),
            new Choice("brand", "Rage Brand", "Mark nearby enemies for 6 seconds: they take +15% damage from your attacks. 5s cooldown.",true),
            new Choice("bloodthirst", "Bloodthirst", "Damage nearby enemies and heal for 10% of actual damage dealt, capped at 15% maximum health per cast. 8s cooldown.",true),
            new Choice("vengeance", "Vengeance", "When hit, a chance to counter with your fully modified standard weapon attack. Protection applies to the triggering hit only.",true),
            new Choice("warcry", "War Cry", "Weaken nearby enemies: they deal 20% less damage for 6 seconds. 15s cooldown.",true),
            new Choice("rend", "Rend", "Weapon hits have a 20% chance to inflict bleeding for 35% weapon damage per second over 3 seconds. Reapplications refresh without stacking.",true),
            new Choice("power", "Tempered resolve", "+4% damage to all abilities."),
            new Choice("haste", "Relentless", "+2% Haste rating to all abilities. Diminishing returns apply after 40%."),
            new Choice("vitality", "Second breath", "+20 maximum health; restore 20 health."),
            new Choice("guard", "Iron oath", "+2% damage reduction (maximum 56%)."),
            new Choice("regen", "Mending thread", "Recover 0.3 health each second."),
            new Choice("speed", "Lightfoot", "+3% movement speed, multiplied against your gear baseline."),
            new Choice("wisdom", "Veil insight", "+15% experience gained. The level threshold stays constant."),
            new Choice("reach", "Gathering dark", "+25% experience collection Area."),
            new Choice("crit", "Killing instinct", "+1% critical hit chance (maximum 100%)."),
            new Choice("critdamage", "Ruinous strikes", "+10% critical damage."),
            new Choice("area", "Widening ruin", "+10% Area (ability coverage and projectile size)."),
            new Choice("luck", "Fortune in ashes", "+10 luck. Future offers favour higher rarities.")
        };
        public int Rank(string id) => ranks.TryGetValue(id, out int n) ? n : 0;
        public int Slots { get { int n = 0; foreach (Choice c in Pool) if (c.ability && c.id!="rage" && Rank(c.id) > 0) n++; return n; } }
        public bool Eligible(Choice c) => !banished.Contains(c.id)&&!banished.Contains(c.Key)&&(!c.ability||Rank(c.id)>0||Slots<5)&&CanUpgrade(c)&&(c.id!="guard"||PassivePercent(c.id)<56)&&(c.id!="crit"||CritBasePercent+PassivePercent(c.id)<100);
        public bool AbilityRound=>!PassiveRound&&offerLevel%2==0&&Array.Exists(Pool,c=>c.ability&&Rank(c.id)==0&&Eligible(c));
        void SetOfferMode(bool passive){
            if(PassiveRound==passive)return;
            if(PassiveRound)passiveLockedOffer=lockedOffer;else abilityLockedOffer=lockedOffer;
            PassiveRound=passive;lockedOffer=passive?passiveLockedOffer:abilityLockedOffer;locked=lockedOffer?.Key;
        }
        public void RollPassives(System.Random random){SetOfferMode(true);Roll(random);}
        public void Roll(System.Random random,int level=-1) {
            if(AscensionRound)return;
            if(level>=2){offerLevel=level;SetOfferMode(false);}offers.Clear();bool acquisition=AbilityRound;var candidates=new List<Choice>(PassiveRound?PassiveCandidates():Candidates(acquisition));
            if(lockedOffer!=null&&!Eligible(lockedOffer)){locked=null;lockedOffer=null;}
            Choice held=lockedOffer==null?null:candidates.Find(c=>c.Key==locked);
            if(held!=null){offers.Add(lockedOffer);candidates.RemoveAll(c=>c.Key==locked);}
            Draw(random,candidates);
            if(acquisition&&offers.Count<3){candidates=new List<Choice>(Candidates(false));candidates.RemoveAll(c=>offers.Exists(o=>o.Key==c.Key));Draw(random,candidates);}
            if(!PassiveRound&&offers.Count==0)Draw(random,new List<Choice>(Candidates(true)));
        }
        void Draw(System.Random random,List<Choice> candidates) {
            while(offers.Count<3&&candidates.Count>0){
                // Roll rarity first: count bonuses are only candidates at their authored rarities.
                var weights=Rarities.Chances(Luck);double total=0;
                for(int r=0;r<5;r++){if(!candidates.Exists(c=>AbilitySpecs.SupportsRarity(c,(UpgradeRarity)r)))weights[r]=0;total+=weights[r];}
                if(total<=0)break;double roll=random.NextDouble()*total;int selected=4;
                for(int r=0;r<5;r++){roll-=weights[r];if(roll<0){selected=r;break;}}
                var rarity=(UpgradeRarity)selected;var available=candidates.FindAll(c=>AbilitySpecs.SupportsRarity(c,rarity));var c=available[random.Next(available.Count)];
                offers.Add(new Choice(c.id,c.name,c.description,c.ability){attribute=c.attribute,rarity=rarity});candidates.RemoveAll(candidate=>candidate.Key==c.Key);
            }
        }
        public void Pick(Choice c) {
            if(c.ascension>0){PickAscension(c);return;}
            if(c.ability)PickAbility(c);else{if(!passivePicks.TryGetValue(c.id,out var counts)){counts=new int[5];passivePicks[c.id]=counts;}counts[(int)c.rarity]++;potency[c.id]=Amount(c.id)+c.Strength;ranks[c.id]=Rank(c.id)+1;}
            if(locked==c.Key){locked=null;lockedOffer=null;}
        }
        public string Describe(Choice c)
        {
            float s=c.Strength;
            if(c.ascension>0)return c.description;
            if(c.ability)return DescribeAbility(c);
            if(PassiveRules.Has(c.id))return DescribePassive(c);
            switch(c.id)
            {
                case "vitality":return $"+{20*s:0.#} maximum health; restore {20*s:0.#} health.";
                case "regen":return $"Recover {0.3f*s:0.###} health each second.";
                case "area":return $"+{10*s:0.#}% Area (ability coverage and projectile size).";
                case "luck":return $"+{10*s:0.#} luck. Future offers favour higher rarities.";
                default:return $"+{25*s:0.#}% experience collection Area.";
            }
        }
        public bool Banish(Choice c)
        {
            if (AscensionRound||banishes <= 0) return false;
            banishes--; banished.Add(c.Key); if (locked == c.Key){locked = null;lockedOffer=null;} return true;
        }
        public bool Lock(Choice c)
        {
            if(AscensionRound)return false;
            if (locked == c.Key) { locked = null;lockedOffer=null; return true; }
            if (locks <= 0) return false;
            locks--; locked = c.Key;lockedOffer=c; return true;
        }
    }
}
