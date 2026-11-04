using System.Collections.Generic;

namespace Veilbound
{
    // Read-only equipment calculations. This service never crafts, equips, saves or changes inventory.
    public sealed class EquipmentLoadout
    {
        int[] armourItems, equippedIds;
        List<CraftedGear> inventory;
        int weaponItem;
        float weaponWeight;

        public void Bind(int[] items, int[] ids, List<CraftedGear> owned, int weapon, float mass)
        {
            armourItems = items;
            equippedIds = ids;
            inventory = owned;
            weaponItem = weapon;
            weaponWeight = mass;
        }

        public CraftedGear Equipped(int slot)
        {
            if (slot < 0 || slot > 9 || equippedIds == null || inventory == null) return null;
            int item = slot == 9 ? weaponItem : armourItems[slot];
            foreach (var owned in inventory)
            {
                if (owned.id != equippedIds[slot] || owned.slot != slot) continue;
                if (owned.item == item || slot < 9 && Metals.IsMetal(owned.item) && Metals.IsMetal(item) && Metals.Index(owned.item) == Metals.Index(item)) return owned;
            }
            return null;
        }

        public float Armour
        {
            get
            {
                float total = 0;
                for (int slot = 0; slot < 9; slot++) total += ArmourCatalog.Find(slot, armourItems[slot])?.Armour ?? 0;
                return total;
            }
        }

        public float Weight
        {
            get
            {
                float total = weaponWeight;
                for (int slot = 0; slot < 9; slot++) total += ArmourCatalog.Find(slot, armourItems[slot])?.Weight ?? 0;
                return total;
            }
        }

        public ArmourSetBonuses Sets(CraftedGear replacement = null)
        {
            int heavy = 0, leather = 0, cloth = 0;
            for (int slot = 0; slot < 8; slot++)
            {
                int item = replacement != null && replacement.slot == slot ? replacement.item : armourItems[slot];
                var definition = ArmourCatalog.Find(slot, item);
                if (definition == null) continue;
                if (definition.Type == ArmourType.Heavy) heavy++;
                else if (definition.Type == ArmourType.Leather) leather++;
                else if (definition.Type == ArmourType.Cloth) cloth++;
            }
            return new ArmourSetBonuses(heavy, leather, cloth);
        }

        public float Bonus(string stat, CraftedGear replacement = null)
        {
            float total = Sets(replacement).Bonus(stat);
            for (int slot = 0; slot < 10; slot++)
            {
                var instance = replacement != null && replacement.slot == slot ? replacement : Equipped(slot);
                total += GearRollRules.Value(instance, stat);
            }
            return total;
        }
    }
}
