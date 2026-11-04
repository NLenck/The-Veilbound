using System;
using System.Collections.Generic;

namespace Veilbound
{
    public enum ArmourType { None, Cloth, Leather, Heavy }
    public enum ArmourSlot { Helm, Shoulders, Chest, Gloves, Belt, Bracers, Legs, Boots, Cape }

    // Shared catalogue data. Owned copies reference this definition through their existing slot/item IDs.
    public sealed class ArmourDefinition
    {
        public string Id { get; }
        public string Name { get; }
        public ArmourSlot Slot { get; }
        public ArmourType Type { get; }
        public int ItemId { get; }
        public int Material { get; }
        public int Tier { get; }
        public float Armour { get; }
        public float Weight { get; }
        public bool CountsForSet => Slot != ArmourSlot.Cape;
        public ArmourCraftingRecipe Recipe { get; }

        internal ArmourDefinition(int slot, int item, int material, int tier, float armour, float weight)
        {
            Slot = (ArmourSlot)slot;
            ItemId = item;
            Material = material;
            Tier = tier;
            Type = material == 3 ? ArmourType.Heavy : material == 2 ? ArmourType.Leather : ArmourType.Cloth;
            Id = "armour:" + slot + ":" + item;
            Name = Gear.Name(item) + " " + Gear.Slots[slot];
            Armour = armour;
            Weight = weight;
            Recipe = new ArmourCraftingRecipe(slot, material, tier, material == 3 ? Metals.Index(item) : -1);
        }
    }

    public sealed class ArmourCraftingRecipe
    {
        readonly ResourceCost[] resources;
        public IReadOnlyList<ResourceCost> Resources { get; }
        public double Essence { get; }

        internal ArmourCraftingRecipe(int slot, int material, int tier, int metal)
        {
            resources = GearCraftCosts.Armour(slot, material, tier, metal);
            Resources = Array.AsReadOnly(resources);
            Essence = material == 3 || tier < 2 ? 0 : Math.Ceiling(Gear.Size(slot) * tier);
        }

        public ResourceCost[] CopyResources() => (ResourceCost[])resources.Clone();
    }

    public static class ArmourCatalog
    {
        static readonly Dictionary<int, ArmourDefinition> definitions = new Dictionary<int, ArmourDefinition>();

        public static ArmourDefinition Find(int slot, int item)
        {
            if (slot < 0 || slot > 8 || item <= 0) return null;
            int metal = Metals.Index(item);
            int material = metal >= 0 ? 3 : Gear.Material(item);
            int tier = metal >= 0 ? metal + 1 : Gear.Tier(item);
            if (material < 1 || material > 3 || tier < 1 || tier > 10 || slot == 8 && material != 1) return null;
            int canonical = metal >= 0 ? Metals.Item(metal) : LightGear.Item(material, tier);
            int key = slot * 1000 + canonical;
            if (!definitions.TryGetValue(key, out var definition))
            {
                float armour = metal >= 0 ? Metals.Armour(slot, metal) : LightGear.Armour(slot, material, tier);
                float weight = metal >= 0 ? Metals.Mass(slot, metal) : LightGear.Mass(slot, material, tier);
                definition = new ArmourDefinition(slot, canonical, material, tier, armour, weight);
                definitions.Add(key, definition);
            }
            return definition;
        }
    }
}
