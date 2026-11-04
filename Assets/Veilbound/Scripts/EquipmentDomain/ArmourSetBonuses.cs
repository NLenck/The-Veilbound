namespace Veilbound
{
    // Counts body slots, not item copies. Material tiers may be mixed within a type.
    public readonly struct ArmourSetBonuses
    {
        public int Heavy { get; }
        public int Leather { get; }
        public int Cloth { get; }

        public ArmourSetBonuses(int heavy, int leather, int cloth)
        {
            Heavy = heavy;
            Leather = leather;
            Cloth = cloth;
        }

        public int Count(ArmourType type) => type == ArmourType.Heavy ? Heavy : type == ArmourType.Leather ? Leather : type == ArmourType.Cloth ? Cloth : 0;

        public float Bonus(string stat)
        {
            switch (stat)
            {
                case "regen": return Heavy >= 4 ? .3f : 0;
                case "vitality": return Heavy >= 8 ? 100 : 0;
                case "speed": return Leather >= 4 ? 5 : 0;
                case "attackspeed": return Leather >= 8 ? 10 : 0;
                case "area": return Cloth >= 4 ? 15 : 0;
                case "wisdom": return Cloth >= 8 ? 50 : 0;
                default: return 0;
            }
        }

        public static string Name(ArmourType type) => type == ArmourType.Heavy ? "Heavy" : type == ArmourType.Leather ? "Leather" : type == ArmourType.Cloth ? "Cloth" : "None";
        public static string Description(ArmourType type)
        {
            switch (type)
            {
                case ArmourType.Heavy: return "4 pieces: +0.3 health/sec | 8 pieces: +100 health";
                case ArmourType.Leather: return "4 pieces: +5% movement | 8 pieces: +10% attack speed";
                case ArmourType.Cloth: return "4 pieces: +15% area | 8 pieces: +50% XP";
                default: return "";
            }
        }
    }
}
