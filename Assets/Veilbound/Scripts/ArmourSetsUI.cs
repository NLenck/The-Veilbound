using UnityEngine;

namespace Veilbound
{
    public partial class VeilGame
    {
        static int EquipmentTier(CraftedGear item) => item.slot == 9 ? WeaponRules.Material(item.item) + 1 : item.ArmourDefinition?.Tier ?? 0;
        static string EquipmentTypeLabel(CraftedGear item) => item.slot == 9 ? item.rarity.ToString() : ArmourSetBonuses.Name(item.ArmourDefinition?.Type ?? ArmourType.None);

        Color EquipmentColour(CraftedGear item)
        {
            if (item.slot == 9) return Rarities.Colour(item.rarity);
            var definition = item.ArmourDefinition;
            if (definition?.Type == ArmourType.Heavy) return SmithTint(Metals.Index(item.item));
            return definition?.Type == ArmourType.Leather ? new Color(.72f, .48f, .28f) : new Color(.55f, .72f, .79f);
        }

        void DrawArmourSets(Rect area)
        {
            var sets = city.Loadout.Sets();
            var types = new[] { ArmourType.Heavy, ArmourType.Leather, ArmourType.Cloth };
            string[] first = { "+0.3 HP/sec", "+5% movement", "+15% area" };
            string[] second = { "+100 health", "+10% attack speed", "+50% XP" };
            float width = (area.width - 16) / 3;
            for (int i = 0; i < types.Length; i++)
            {
                Rect cell = new Rect(area.x + i * (width + 8), area.y, width, area.height);
                Box(cell, new Color(.045f, .042f, .034f));
                int count = sets.Count(types[i]);
                GUI.Label(new Rect(cell.x + 10, cell.y + 2, width - 20, 20), ArmourSetBonuses.Name(types[i]).ToUpper() + " SET / " + count + " OF 8", ArchiveType(12, archiveIvory, true));
                GUI.Label(new Rect(cell.x + 10, cell.y + 25, width * .45f, 22), "4: " + first[i] + (count >= 4 ? " / Active" : ""), ArchiveType(12, count >= 4 ? archiveGold : archiveMuted));
                GUI.Label(new Rect(cell.x + width * .47f, cell.y + 25, width * .52f - 10, 22), "8: " + second[i] + (count >= 8 ? " / Active" : ""), ArchiveType(12, count >= 8 ? archiveGold : archiveMuted));
            }
        }
    }
}
