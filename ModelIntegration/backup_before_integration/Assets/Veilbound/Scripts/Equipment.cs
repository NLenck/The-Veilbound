using System;
using UnityEngine;

namespace Veilbound
{
    public static class Gear
    {
        public static readonly string[] Slots={"Helm","Shoulders","Chest","Gloves","Belt","Bracers","Legs","Boots","Cape"};
        public static readonly string[] Materials={"None","Cloth","Leather","Plate"};
        public static int Material(int item)=>Metals.IsMetal(item)?3:item/10;
        public static int Tier(int item)=>Metals.IsMetal(item)?Metals.All[Metals.Index(item)].unlock:item%10;
        public static string Name(int item)=>item==0?"Unequipped":Metals.IsMetal(item)?Metals.All[Metals.Index(item)].name+" plate":Materials[Material(item)]+" T"+Tier(item);
        public static int CraftUnits(int slot)=>slot==2||slot==9?8:slot==0||slot==1||slot==6?6:4;
        public static float Size(int slot)=>slot==2?1.7f:slot==6?1.4f:slot==8?1.2f:1;
    }
    public partial class Settlement
    {
        public double stone=80,cloth,leather;
        public int quarry=1,weaver,blacksmith,tailor,leatherworker;
        public int[] ownedGear,equippedGear;
        public void EnsureGear()
        {
            if(ownedGear!=null&&ownedGear.Length==27&&equippedGear!=null&&equippedGear.Length==9){EnsureMetals();return;}
            ownedGear=new int[27];equippedGear=new int[9];
            // Convert legacy whole-suit unlocks. Capes are now an optional independent item.
            int material=armour==2?3:2;
            if(armour>0)for(int slot=0;slot<8;slot++)
            {ownedGear[slot*3+material-1]=Math.Min(2,armour);if(equippedArmour>0)equippedGear[slot]=material*10+Math.Min(2,equippedArmour);}
            blacksmith=Math.Max(blacksmith,forge);
            if(sword>0)blacksmith=Math.Max(blacksmith,sword);EnsureMetals();
        }
        public int Owned(int slot,int material){EnsureGear();return ownedGear[slot*3+material-1];}
        public int Item(int slot){EnsureGear();return equippedGear[slot];}
        public int Workshop(int material)=>material==1?tailor:material==2?leatherworker:blacksmith;
        public double StoneRate=>4+quarry*4;
        public double ClothRate=>ProductionRate(2,0);
        public double LeatherRate=>ProductionRate(1,0);
        public float GearHealth=>EquipmentArmour*.15f;
        public float Protection=>EquipmentArmour/(EquipmentArmour+1200);
        public float GearSpeed=>1-Mathf.Min(.75f,EquipmentMass*.0025f);
        public float GearExperience
        {get{EnsureGear();float n=0;foreach(int item in equippedGear)if(Gear.Material(item)==1)n+=Gear.Tier(item)*.01f;return 1+n;}}
        public static void GearCost(int slot,int material,int tier,out double ironCost,out double clothCost,out double leatherCost,out double essenceCost)
        {
            double amount=Gear.CraftUnits(slot);
            ironCost=material==3?amount:0;clothCost=material==1?amount:material==2?Math.Ceiling(amount*.25):0;
            leatherCost=material==2?amount:0;essenceCost=tier==2?Math.Ceiling(Gear.Size(slot)*2):0;
        }
        public bool Craft(int slot,int material,out string message)
        {
            EnsureGearRolls();int next=Math.Min(2,Owned(slot,material)+1);
            if(slot==8&&material!=1){message="Capes are crafted by the tailor from cloth.";return false;}
            if(next>2){message="This piece is fully upgraded.";return false;}
            if(Workshop(material)<next){message="Requires "+(material==1?"Tailor":material==2?"Leatherworker":"Blacksmith")+" level "+next+".";return false;}
            GearCost(slot,material,next,out double i,out double c,out double l,out double e);
            if(iron<i||cloth<c||leather<l||essence<e){message="Not enough crafting materials.";return false;}
            iron-=i;cloth-=c;leather-=l;essence-=e;ownedGear[slot*3+material-1]=next;var roll=RecordCraft(slot,material*10+next);
            message=roll.rarity+" "+Gear.Slots[slot]+" #"+roll.id+" crafted. Compare it in your bag.";return true;
        }
        public void Cycle(int slot)
        {
            EnsureGear();int current=Item(slot);var items=new System.Collections.Generic.List<int>{0};
            for(int m=1;m<=3;m++)for(int t=1;t<=Owned(slot,m);t++)items.Add(m*10+t);
            equippedGear[slot]=items[(items.IndexOf(current)+1)%items.Count];
        }
        public int BuildingLevel(string id)=>id=="timber"?timber:id=="quarry"?quarry:id=="mine"?mine:id=="weaver"?weaver:id=="hunters"?huntersLodge:id=="blacksmith"?blacksmith:id=="tailor"?tailor:leatherworker;
        public static readonly string[] Buildings={"timber","quarry","mine","weaver","hunters","blacksmith","tailor","leatherworker"};
        public static readonly string[] BuildingNames={"Timber Yard","Stone Quarry","Mine","Textile Supply","Hunter's Lodge","Blacksmith","Tailor","Leatherworker"};
        public static void BuildingCost(int level,out double woodCost,out double stoneCost){woodCost=40*(level+1);stoneCost=25*(level+1);}
        public bool UpgradeBuilding(string id,out string message)
        {
            if((id=="tailor"&&tailor>=2)||(id=="leatherworker"&&leatherworker>=2)){message="This workshop has unlocked both current gear tiers.";return false;}
            int n=BuildingLevel(id);if((id=="mine"&&n>=7)||((id=="weaver"||id=="hunters")&&n>=5)||(id=="blacksmith"&&n>=9)){message="All materials are unlocked.";return false;}BuildingCost(n,out double w,out double s);
            if(wood<w||stone<s){message="Buildings require more timber and stone.";return false;}
            wood-=w;stone-=s;
            if(id=="timber")timber++;else if(id=="quarry")quarry++;else if(id=="mine")mine++;else if(id=="weaver"){weaver++;EnsureIndustry();if(n==0)textileEffort[0]=Capacity(2)/2;}else if(id=="hunters"){huntersLodge++;EnsureIndustry();if(n==0)huntEffort[0]=Capacity(1)/2;}
            else if(id=="blacksmith"){blacksmith++;forge=blacksmith;}else if(id=="tailor")tailor++;else leatherworker++;
            message="Construction complete.";return true;
        }
    }
}
