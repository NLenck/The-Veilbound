# Equipment model and armour sets

Armour definitions are shared catalogue data. Individual owned items keep their own identity and Mystic enchantments. Armour type set bonuses are calculated from equipped body pieces and are never stored in enchantment slots.

| Armour type | Four body pieces | Eight body pieces |
| --- | --- | --- |
| Heavy / plate | +0.3 health per second | +100 maximum health |
| Leather | +5% movement speed | +10% attack speed |
| Cloth | +15% ability area | +50% experience gained |

Thresholds are cumulative: eight pieces grant both bonuses. The eight body slots count once each; capes, weapons, backpack copies and empty slots do not count. Materials and tiers can be mixed within one armour type. A 4-heavy/4-cloth outfit receives both four-piece bonuses. An unarmoured body does not count as cloth.

Each armour piece provides armour value and weight. The previous per-piece light-armour bonuses and armour-to-health conversion are replaced by the type sets. Existing crafting recipes, costs, unlock requirements and item IDs remain unchanged.

## Mystic enchantments

Mystic level 1 unlocks one enchantment slot per item. Level 2 unlocks the second; two is the permanent maximum. Levels 3 and 4 retain category and specific-stat targeting. Adding or replacing enchantments consumes Veil Essence and does not affect type counts. Set bonuses consume no enchantment slots. Their stat effects add to enchantment effects, while leather attack speed is a direct 1.10 attack-rate multiplier at eight pieces, independent of diminishing-return Haste rating.

Armour has no gameplay rarity. New enchantment strength and cost use deterministic material progression bands, retaining the previous material-based values. Existing enchantment values survive save/load without rerolling. Weapon grades and ability/passive card rarity remain unchanged.

## Code boundaries

- `EquipmentDomain/ArmourDefinition.cs`: immutable shared definitions, typed armour slots/types, catalogue lookup and crafting recipe data. Canonical IDs resolve legacy item IDs.
- `EquipmentDomain/EquipmentInstance.cs`: owned identity and enchantments, with existing serialized field names retained.
- `EquipmentDomain/ArmourSetBonuses.cs`: independent cumulative 4/8-piece rules.
- `EquipmentDomain/EquipmentLoadout.cs`: read-only equipped stats, set counts and projected replacement effects.
- `EquipmentDomain/EnchantmentRules.cs`: Mystic slot limits, material power bands and compatibility normalization.
- `CraftedGear`: compatibility wrapper used by existing save files and inventory APIs. Its old rarity field is retained for old JSON and weapon grades; armour normalizes to Common and never reads that field for enchanting or display. Retired crafting affixes provide no effects.
- `Settlement.Loadout`: adapter from existing save arrays into the calculation service.

Comparison previews use the full equipped loadout, so crossing a set threshold shows all resulting bonus changes without temporarily equipping or mutating the candidate item. Inventory displays set progress separately from the Mystic's per-item enchantment slots.

## Validation and feature build

Run `Veilbound.Editor.ArmourFeatureBuild.Build` with Unity 6000.3.9f1. This runs 216 domain checks, the existing 1,024 equipment-visibility combinations and prototype logic checks before building `Builds/ArmourSetsFeature/TheVeilbound.exe`.

Run that executable with `-veilSmoke -armourSetsReview` for actual XP pickup, exact attack-speed and movement effects, combat regeneration, independent enchantments, save compatibility and inventory/Mystic/comparison captures. Use `-veilSmoke -lightGearReview` for cloth/leather crafting, production and save regression coverage.
