using System;

namespace Veilbound
{
    public static class EnchantmentRules
    {
        public const int MaximumSlots = 2;
        public static int SlotLimit(int mysticLevel) => mysticLevel < 1 ? 0 : mysticLevel == 1 ? 1 : MaximumSlots;

        // Material progression controls new enchantment power; armour rarity is not used.
        public static int PowerBand(CraftedGear item)
        {
            if (item == null) return 0;
            if (item.slot == 9) return (int)GearRollRules.MaterialGrade(item.slot, item.item);
            int tier = item.ArmourDefinition?.Tier ?? 1;
            return tier <= 4 ? 0 : tier <= 6 ? 1 : tier == 7 ? 2 : tier <= 9 ? 3 : 4;
        }

        public static void Normalize(CraftedGear item)
        {
            if (item.enchantments != null && item.enchantments.Length > MaximumSlots)
                Array.Resize(ref item.enchantments, MaximumSlots);
        }
    }
}
