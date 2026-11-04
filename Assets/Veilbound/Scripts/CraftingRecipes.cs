using System;using UnityEngine;
namespace Veilbound {
 public struct WorkshopStat {public string label,value;public WorkshopStat(string label,string value){this.label=label;this.value=value;}}
 public sealed class EquipmentRecipeEntry {
  public ArmourDefinition armourDefinition;public string name,requirement,traits;public int row,column,slot,item,material,tier,queueKind,queueIndex;public double essence;public ResourceCost[] ingredients;public WorkshopStat[] stats;public Color tint;public bool hasVariant;
 }
 public sealed class CraftingRecipeSet {
  readonly Settlement city;public readonly EquipmentRecipeKind Kind;
  public CraftingRecipeSet(Settlement city,EquipmentRecipeKind kind){this.city=city;Kind=kind;}
  public int Rows=>Light?LightGear.MaxTier:Metals.All.Length;public int Columns=>Kind==EquipmentRecipeKind.Weapon?1:Kind==EquipmentRecipeKind.Cloth?9:8;
  public string Title=>Kind==EquipmentRecipeKind.Weapon?"WEAPON CRAFTING":Kind==EquipmentRecipeKind.Plate?"PLATE ARMOUR":Kind==EquipmentRecipeKind.Cloth?"CLOTH ARMOUR & CAPES":"LEATHER ARMOUR";
  bool Light=>Kind==EquipmentRecipeKind.Cloth||Kind==EquipmentRecipeKind.Leather;
  int Material=>Kind==EquipmentRecipeKind.Cloth?1:2;
  public string ColumnName(int column)=>Kind==EquipmentRecipeKind.Weapon?"GREATSWORD":Gear.Slots[column].ToUpper();
  public string RowName(int row)=>Light?"T"+(row+1)+" / "+Settlement.ProcessedNames[Material==1?1:0][LightGear.FabricIndex(row+1)].Replace(" cloth","").Replace(" leather","")+(row%2==1?" +":""):Metals.All[row].name;
  public EquipmentRecipeEntry Entry(int row,int column,bool heavy=false){
   row=Mathf.Clamp(row,0,Rows-1);column=Mathf.Clamp(column,0,Columns-1);var entry=new EquipmentRecipeEntry{row=row,column=column,slot=Kind==EquipmentRecipeKind.Weapon?9:column,tier=row+1,material=Light?Material:3,traits=""};
   if(Light||Kind==EquipmentRecipeKind.Plate){
    entry.item=Light?LightGear.Item(Material,row+1):Metals.Item(row);
    var definition=ArmourCatalog.Find(column,entry.item);entry.armourDefinition=definition;
    entry.name=definition.Name;entry.requirement=Light?city.LightGearRequirement(column,Material,row+1):city.SmithRequirement(row,"plate");
    entry.ingredients=definition.Recipe.CopyResources();entry.essence=definition.Recipe.Essence;
    entry.traits=definition.CountsForSet?ArmourSetBonuses.Description(definition.Type):"Cape: does not count toward armour sets";
    entry.stats=new[]{new WorkshopStat("ARMOUR",definition.Armour.ToString("0.##")),new WorkshopStat("WEIGHT",definition.Weight.ToString("0.##")+" kg")};
    entry.tint=Light?LightGear.Colour(Material,row+1):Metals.All[row].color;entry.queueKind=Light?5:3;entry.queueIndex=Light?Material:row;
   }
   else {int item=row==9&&heavy?10:row;entry.item=item;entry.name=WeaponRules.Name(item);entry.requirement=city.SmithRequirement(item,"weapons");entry.ingredients=city.WeaponRecipe(item);entry.stats=new[]{new WorkshopStat("DAMAGE",WeaponRules.Damage(item).ToString("0.##")),new WorkshopStat("ATTACK INTERVAL",WeaponRules.Interval(item).ToString("0.00")+" s"),new WorkshopStat("BASE DPS",(WeaponRules.Damage(item)/WeaponRules.Interval(item)).ToString("0.0")),new WorkshopStat("WEIGHT",(4.5f*Metals.All[WeaponRules.Material(item)].weight).ToString("0.##")+" kg")};entry.tint=Metals.All[row].color;entry.hasVariant=row==9;entry.queueKind=4;entry.queueIndex=item;}
   return entry;
  }
  public bool Affordable(EquipmentRecipeEntry entry)=>city.HasRecipe(entry.ingredients)&&city.essence+1e-9>=entry.essence;
  public bool Craft(EquipmentRecipeEntry entry,out string message)=>Light?city.CraftLightGear(entry.slot,entry.material,entry.tier,out message):Kind==EquipmentRecipeKind.Plate?city.CraftPlate(entry.slot,entry.row,out message):city.CraftMetalWeapon(entry.item,out message);
  public bool Queue(EquipmentRecipeEntry entry,out string message)=>city.QueueSettlementJob(entry.queueKind,"",entry.queueIndex,entry.slot,1,Light?entry.tier:0,out message);
 }
 public partial class Settlement {
  [NonSerialized] CraftingRecipeSet[] equipmentRecipeSets;
  public CraftingRecipeSet EquipmentRecipes(EquipmentRecipeKind kind){if(equipmentRecipeSets==null)equipmentRecipeSets=new CraftingRecipeSet[4];int index=(int)kind;return equipmentRecipeSets[index]??(equipmentRecipeSets[index]=new CraftingRecipeSet(this,kind));}
 }
}
