using System;
using System.Collections.Generic;
using UnityEngine;
namespace Veilbound {
 public sealed class MetalDefinition {
  public string name;public int unlock;public UpgradeRarity rarity;public float armour,weight,armourWeight,weapon;public Color color;
  public MetalDefinition(string n,int u,UpgradeRarity r,float a,float w,float d,Color c,float plateWeight=-1){name=n;unlock=u;rarity=r;armour=a;weight=w;armourWeight=plateWeight<0?w:plateWeight;weapon=d;color=c;}
 }
 public static class Metals {
  public static readonly MetalDefinition[] All={
   new MetalDefinition("Tin",1,UpgradeRarity.Common,16,.93f,18,new Color(.52f,.58f,.63f),1.05f),
   new MetalDefinition("Copper",2,UpgradeRarity.Common,40,1.14f,24,new Color(.58f,.23f,.10f),1.30f),
   new MetalDefinition("Bronze",3,UpgradeRarity.Common,55,1.12f,32,new Color(.33f,.19f,.075f),1.25f),
   new MetalDefinition("Brass",4,UpgradeRarity.Common,62,1.08f,36,new Color(.65f,.49f,.16f),1.20f),
   new MetalDefinition("Iron",5,UpgradeRarity.Uncommon,75,.99f,44,new Color(.24f,.28f,.29f),1.15f),
   new MetalDefinition("Steel",6,UpgradeRarity.Uncommon,100,1,54,new Color(.44f,.49f,.54f),1.20f),
   new MetalDefinition("Hardened steel",7,UpgradeRarity.Rare,140,1.05f,66,new Color(.15f,.19f,.23f),1.35f),
   new MetalDefinition("Titanium",8,UpgradeRarity.Epic,160,.56f,78,new Color(.61f,.66f,.72f),.85f),
   new MetalDefinition("Tungsten",8,UpgradeRarity.Epic,210,2.26f,90,new Color(.24f,.23f,.27f),1.751373626f),
   new MetalDefinition("VeilCrystal",9,UpgradeRarity.Legendary,280,.70f,112,new Color(.55f,.055f,.04f),1f)
  };
  public static int Item(int metal)=>100+metal;
  public static bool IsMetal(int item)=>item>=100&&item<110||item==31||item==32;
  public static int Index(int item)=>item>=100&&item<110?item-100:item==32?5:item==31?2:-1;
  public static float SlotScale(int slot)=>Gear.Size(slot)/1.7f;
  public static float Armour(int slot,int metal)=>All[metal].armour*SlotScale(slot);
  public static float Mass(int slot,int metal)=>8*All[metal].armourWeight*SlotScale(slot);
  public static int GripGrade(int metal)=>Math.Max(0,(int)All[metal].rarity-1);
  public static int PlateBars(int slot)=>Gear.CraftUnits(slot);
 }
 [Serializable] public struct ResourceCost {
  public int family,index;public double amount;
  public ResourceCost(int f,int i,double a){family=f;index=i;amount=a;}
 }
 public partial class Settlement {
  public int metalVersion;public bool[] ownedPlate,ownedMetalWeapons;public int equippedMetalWeapon=-1;
  public double[] metalBars;
  public void EnsureMetals() {
   if(ownedPlate==null||ownedPlate.Length!=90)ownedPlate=new bool[90];
   if(ownedMetalWeapons==null)ownedMetalWeapons=new bool[WeaponRules.Count];else if(ownedMetalWeapons.Length<WeaponRules.Count)Array.Resize(ref ownedMetalWeapons,WeaponRules.Count);
   if(metalBars==null)metalBars=new double[WeaponRules.Count];else if(metalBars.Length<WeaponRules.Count)Array.Resize(ref metalBars,WeaponRules.Count);
   if(metalVersion==0){equippedMetalWeapon=-1;ownedMetalWeapons[0]=true;if(sword>=1)ownedMetalWeapons[4]=true;if(sword>=2)ownedMetalWeapons[5]=true;
    int legacy=0;for(int s=0;s<8;s++)legacy=Math.Max(legacy,ownedGear[s*3+2]);if(legacy>0)blacksmith=Math.Max(blacksmith,legacy==2?6:3);if(sword>0)blacksmith=Math.Max(blacksmith,sword==2?6:5);metalVersion=1;
   }
   for(int s=0;s<8;s++){if(ownedGear[s*3+2]>=1)ownedPlate[s*10+2]=true;if(ownedGear[s*3+2]>=2)ownedPlate[s*10+5]=true;}
  }
  public bool OwnsPlate(int slot,int metal){EnsureGear();return slot>=0&&slot<8&&metal>=0&&metal<10&&ownedPlate[slot*10+metal];}
  public int WeaponMetalIndex {get{EnsureGear();return equippedMetalWeapon>=0&&equippedMetalWeapon<WeaponRules.Count?WeaponRules.Material(equippedMetalWeapon):equippedSword>=2?5:equippedSword==1?4:0;}}
  public bool IsWoodenSword=>equippedMetalWeapon==-2;
  public int WeaponItemIndex=>IsWoodenSword?-1:equippedMetalWeapon>=0&&equippedMetalWeapon<WeaponRules.Count?equippedMetalWeapon:WeaponMetalIndex;
  public float WeaponDamage=>WeaponRules.Damage(WeaponItemIndex);
  public string WeaponName=>WeaponRules.Name(WeaponItemIndex);
  public float EquipmentArmour {get{EnsureGear();float n=0;for(int s=0;s<9;s++){int item=equippedGear[s],m=Metals.Index(item);n+=m>=0?Metals.Armour(s,m):LightGear.Armour(s,Gear.Material(item),Gear.Tier(item));}return n;}}
  public float EquipmentMass {get{EnsureGear();float n=IsWoodenSword?1.8f:4.5f*Metals.All[WeaponMetalIndex].weight;for(int s=0;s<9;s++){int item=equippedGear[s],m=Metals.Index(item);if(m>=0)n+=Metals.Mass(s,m);else if(item>0)n+=LightGear.Mass(s,Gear.Material(item),Gear.Tier(item));}return n;}}
  public ResourceCost[] BarRecipe(int metal) {
   switch(metal){case 0:return new[]{new ResourceCost(0,0,1)};case 1:return new[]{new ResourceCost(0,1,1)};
    case 2:return new[]{new ResourceCost(3,1,2),new ResourceCost(3,0,1)};case 3:return new[]{new ResourceCost(3,1,2),new ResourceCost(0,2,1)};
    case 4:return new[]{new ResourceCost(0,3,1)};case 5:return new[]{new ResourceCost(0,3,2),new ResourceCost(0,4,1)};
    case 6:return new[]{new ResourceCost(3,5,2),new ResourceCost(0,4,1)};case 7:return new[]{new ResourceCost(0,5,2),new ResourceCost(0,4,1)};
    case 8:return new[]{new ResourceCost(0,6,2),new ResourceCost(0,4,2)};case 9:return new[]{new ResourceCost(0,7,2),new ResourceCost(3,7,1),new ResourceCost(0,4,2)};default:return new[]{new ResourceCost(0,7,2),new ResourceCost(3,8,1),new ResourceCost(0,4,2)};}
  }
  public ResourceCost[] PlateRecipe(int slot,int metal)=>GearCraftCosts.Armour(slot,3,metal+1,metal);
  public ResourceCost[] WeaponRecipe(int metal)=>GearCraftCosts.Weapon(metal);
  public bool PayRecipe(ResourceCost[] costs,int count,out string message) {
   EnsureIndustry();EnsureGear();if(count<1){message="Invalid batch size.";return false;}
   foreach(var c in costs)if(ResourceAmount(c.family,c.index)+1e-9<c.amount*count){message="Requires "+RecipeText(costs,count)+".";return false;}
   foreach(var c in costs)AddResource(c.family,c.index,-c.amount*count);message="";return true;
  }
  public string RecipeText(ResourceCost[] costs,int count=1){var names=new List<string>();foreach(var c in costs)names.Add((c.amount*count).ToString("0.##")+" "+ResourceName(c.family,c.index));return string.Join(" + ",names);}
  public bool MakeBars(int metal,int count,out string message)=>QueueBars(metal,count,out message);
  public bool CraftPlate(int slot,int metal,out string message) {
   EnsureGearRolls();if(slot<0||slot>=8||metal<0||metal>=10){message="This slot does not accept plate.";return false;}
   if(blacksmith<Metals.All[metal].unlock){message="Requires Blacksmith level "+Metals.All[metal].unlock+".";return false;}
   message=CraftResearchRequirement(metal,"plate");if(message!="")return false;
   if(!PayRecipe(PlateRecipe(slot,metal),1,out message))return false;
   ownedPlate[slot*10+metal]=true;var roll=RecordCraft(slot,Metals.Item(metal));message=roll.rarity+" "+Metals.All[metal].name+" "+Gear.Slots[slot]+" #"+roll.id+" crafted. Compare it in your bag.";return true;
  }
  public bool CraftMetalWeapon(int metal,out string message) {
   EnsureGearRolls();if(metal<0||metal>=WeaponRules.Count){message="Invalid material.";return false;}
   if(blacksmith<Metals.All[WeaponRules.Material(metal)].unlock){message="Requires Blacksmith level "+Metals.All[WeaponRules.Material(metal)].unlock+".";return false;}
   message=CraftResearchRequirement(metal,"weapons");if(message!="")return false;
   if(!PayRecipe(WeaponRecipe(metal),1,out message))return false;ownedMetalWeapons[metal]=true;var roll=RecordCraft(9,metal);message=roll.rarity+" "+WeaponRules.Name(metal)+" #"+roll.id+" crafted. Compare it in your bag.";return true;
  }
 }
}
