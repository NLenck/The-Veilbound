using System;

namespace Veilbound
{
    [Serializable]
    public class EquipmentInstance
    {
        // Field names are retained to preserve existing character JSON and item identities.
        public int id, slot, item;
        public bool inherited;
        public GearAffix[] enchantments;
        // Read legacy crafting affixes for migration only; they never grant stats.
        public GearAffix[] affixes;

        public ArmourDefinition ArmourDefinition => ArmourCatalog.Find(slot, item);
    }
}
