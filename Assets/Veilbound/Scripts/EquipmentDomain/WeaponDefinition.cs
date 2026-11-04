using System;
using System.Collections.Generic;

namespace Veilbound
{
    public enum WeaponType { Greatsword }
    public enum WeaponVariant { Standard, TitaniumBase, TungstenBase }

    // Shared weapon data; each owned copy keeps its own EquipmentInstance and enchantments.
    public sealed class WeaponDefinition
    {
        public string Id { get; }
        public int ItemId { get; }
        public string Name { get; }
        public WeaponType Type => WeaponType.Greatsword;
        public WeaponVariant Variant { get; }
        public int Slot => 9;
        public int HandsRequired => 2;
        public int MaterialIndex { get; }
        public int Tier => MaterialIndex < 0 ? 0 : MaterialIndex + 1;
        public UpgradeRarity Grade { get; }
        public float Damage { get; }
        public float AttackInterval { get; }
        public float DamagePerSecond => Damage / AttackInterval;
        public float Weight { get; }
        public bool CanCraft => ItemId >= 0;
        public string PrefabResource => ItemId < 0 ? "StarterEquipment/WoodenGreatsword" : null;
        public string VisualLibraryResource => ItemId < 0 ? null : "WeaponVisuals/GreatswordLibrary";
        public int VisualId => ItemId;
        public string BarName { get; }
        WeaponCraftingRecipe recipe;
        public WeaponCraftingRecipe Recipe => CanCraft ? recipe ?? (recipe = new WeaponCraftingRecipe(ItemId, MaterialIndex)) : null;

        internal WeaponDefinition(int itemId, string name, float damage, float interval, WeaponVariant variant = WeaponVariant.Standard)
        {
            ItemId = itemId;
            Id = "weapon:" + itemId;
            Name = name;
            Damage = damage;
            AttackInterval = interval;
            Variant = variant;
            MaterialIndex = itemId < 0 ? -1 : WeaponCatalog.MaterialIndexFor(itemId);
            Grade = itemId < 0 ? UpgradeRarity.Common : Metals.All[MaterialIndex].rarity;
            Weight = itemId < 0 ? 1.8f : 4.5f * Metals.All[MaterialIndex].weight;
            BarName = itemId < 0 ? null : itemId == 9 ? "Veilcrystal (Titanium base) bar" : itemId == 10 ? "Veilcrystal (Tungsten base) bar" : Metals.All[MaterialIndex].name + " bar";
        }
    }

    public sealed class WeaponCraftingRecipe
    {
        readonly ResourceCost[] resources;
        public IReadOnlyList<ResourceCost> Resources { get; }

        internal WeaponCraftingRecipe(int item, int material)
        {
            int tier = material + 1;
            int grip = Metals.GripGrade(material);
            int lining = GearCraftCosts.Lining(9, tier);
            resources = new[]
            {
                new ResourceCost(3, item, GearCraftCosts.Main(9, 3, tier)),
                new ResourceCost(4, grip, lining),
                new ResourceCost(5, grip, lining)
            };
            Resources = Array.AsReadOnly(resources);
        }

        public ResourceCost[] CopyResources() => (ResourceCost[])resources.Clone();
    }
}
