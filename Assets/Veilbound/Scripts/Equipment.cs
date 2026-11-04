using System;
using UnityEngine;

namespace Veilbound
{
    public static class Gear
    {
        public static readonly string[] Slots={"Helm","Shoulders","Chest","Gloves","Belt","Bracers","Legs","Boots","Cape"};
        public static readonly string[] Materials={"None","Cloth","Leather","Plate"};
        public static int Material(int item)=>Metals.IsMetal(item)?3:LightGear.Extended(item)?LightGear.Material(item):item/10;
        public static int Tier(int item)=>Metals.IsMetal(item)?Metals.All[Metals.Index(item)].unlock:LightGear.Extended(item)?LightGear.Tier(item):item%10;
        public static string Name(int item)=>item==0?"Unequipped":Metals.IsMetal(item)?Metals.All[Metals.Index(item)].name+" plate":LightGear.Name(Material(item),Tier(item));
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
        public int Workshop(int material)=>material==1?Math.Max(tailor,clothier):material==2?Math.Max(leatherworker,clothier):blacksmith;
        public double StoneRate=>StoneDaily[Mathf.Clamp(quarry,0,10)]*PassiveProductionMultiplier*Productivity/CityHoursPerDay;
        public double ClothRate=>ProductionRate(2,0);
        public double LeatherRate=>ProductionRate(1,0);
        public float GearHealth=>0; // Armour grants protection; maximum health comes from sets and enchantments.
        public float Protection=>EquipmentArmour/(EquipmentArmour+1200);
        public float GearSpeed=>1-Mathf.Min(.75f,Mathf.Max(0,EquipmentMass-30f)*.0025f);
        public float GearExperience=>1+GearBonus("wisdom")*.01f;
        public static void GearCost(int slot,int material,int tier,out double ironCost,out double clothCost,out double leatherCost,out double essenceCost)
        {
            double amount=GearCraftCosts.Main(slot,material,tier),lining=GearCraftCosts.Lining(slot,tier);
            ironCost=material==3?amount:GearCraftCosts.Fittings(slot,tier);clothCost=material==1?amount:lining;
            leatherCost=material==2?amount:lining;essenceCost=tier>=2?Math.Ceiling(Gear.Size(slot)*tier):0;
        }
        public bool Craft(int slot,int material,out string message)
        {
            if(material==1||material==2)return CraftLightGear(slot,material,NextLightTier(slot,material),out message);
            EnsureGearRolls();int next=Math.Min(2,Owned(slot,material)+1);
            if(slot==8&&material!=1){message="Capes are crafted by the tailor from cloth.";return false;}
            if(next>2){message="This piece is fully upgraded.";return false;}
            if(Workshop(material)<next){message="Requires "+(material==1?"Tailor":material==2?"Leatherworker":"Blacksmith")+" level "+next+".";return false;}
            GearCost(slot,material,next,out double i,out double c,out double l,out double e);
            if(iron<i||ResourceAmount(5,0)<c||ResourceAmount(4,0)<l||essence<e){message="Requires finished cloth / leather. Process fibres and hides at the Clothier.";return false;}
            iron-=i;AddResource(5,0,-c);AddResource(4,0,-l);essence-=e;ownedGear[slot*3+material-1]=next;var roll=RecordCraft(slot,material*10+next);
            message=Gear.Slots[slot]+" #"+roll.id+" crafted. Compare it in your bag.";return true;
        }
        public void Cycle(int slot)
        {
            EnsureGear();int current=Item(slot);var items=new System.Collections.Generic.List<int>{0};
            for(int m=1;m<=3;m++)for(int t=1;t<=Owned(slot,m);t++)items.Add(m<=2?LightGear.Item(m,t):m*10+t);
            equippedGear[slot]=items[(items.IndexOf(current)+1)%items.Count];
        }
        public int BuildingLevel(string id)=>id=="clothier"?clothier:id=="library"?library:id=="enchanter"?mysticLevel:id=="herbalist"?0:id=="housing"?housingTier:id=="timber"?timber:id=="quarry"?quarry:id=="mine"?mine:id=="weaver"?weaver:id=="hunters"?huntersLodge:id=="blacksmith"?blacksmith:id=="tailor"?tailor:leatherworker;
        public static readonly string[] Buildings={"timber","quarry","mine","weaver","hunters","blacksmith","tailor","leatherworker","clothier","library","herbalist","enchanter"};
        public static readonly string[] BuildingNames={"Logging","Stone Quarry","Mine","Textile Tavern","Hunter's Cabin","Blacksmith","Tailor","Leatherworker","Clothier","Library","Herbalist","Enchanting Tent"};
        public static void BuildingCost(int level,out double woodCost,out double stoneCost){woodCost=40*(level+1);stoneCost=25*(level+1);}
        public bool UpgradeBuilding(string id,out string message)=>TryTownUpgrade(id,out message);
    }
}
