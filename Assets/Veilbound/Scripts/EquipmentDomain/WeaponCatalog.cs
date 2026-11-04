using System;
using System.Collections.Generic;

namespace Veilbound
{
    public static class WeaponCatalog
    {
        public const int CraftableCount = 11;
        static readonly WeaponDefinition[] definitions =
        {
            new WeaponDefinition(-1, "Wooden greatsword", 10, 2),
            new WeaponDefinition(0, "Tin greatsword", 18, 2.24f),
            new WeaponDefinition(1, "Copper greatsword", 24, 2.4f),
            new WeaponDefinition(2, "Bronze greatsword", 35, 2.32f),
            new WeaponDefinition(3, "Brass greatsword", 34, 2.08f),
            new WeaponDefinition(4, "Iron greatsword", 39, 1.92f),
            new WeaponDefinition(5, "Steel greatsword", 48, 1.92f),
            new WeaponDefinition(6, "Hardened steel greatsword", 61, 2.24f),
            new WeaponDefinition(7, "Titanium greatsword", 64, 1.6f),
            new WeaponDefinition(8, "Tungsten greatsword", 109, 2.72f),
            new WeaponDefinition(9, "Veilcrystal (Titanium base) greatsword", 88, 1.6f, WeaponVariant.TitaniumBase),
            new WeaponDefinition(10, "Veilcrystal (Tungsten base) greatsword", 150, 2.72f, WeaponVariant.TungstenBase)
        };
        public static IReadOnlyList<WeaponDefinition> All { get; } = Array.AsReadOnly(definitions);
        public static int MaterialIndexFor(int item) => item == 10 ? 9 : Math.Max(0, Math.Min(item, 9));
        public static WeaponDefinition Find(int item) => item < -1 || item >= CraftableCount ? null : definitions[item + 1];

        // Preserve the existing negative wooden-weapon and clamped legacy helper behavior.
        public static WeaponDefinition ResolveLegacy(int item) => definitions[item < 0 ? 0 : Math.Min(item, CraftableCount - 1) + 1];
    }
}
