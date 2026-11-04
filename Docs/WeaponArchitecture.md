# Weapon definitions and owned copies

`WeaponDefinition` is the shared class for a weapon profile. `WeaponCatalog` contains the wooden starter and all eleven craftable greatswords, including both Veilcrystal variants. Definitions expose stable identity, item ID, name, weapon type, variant, equipment slot, handedness, material, tier, grade, damage, attack interval, DPS, weight, crafting recipe and visual resource/variant references.

An owned weapon remains an `EquipmentInstance` through the existing `CraftedGear` save wrapper. It references its definition using the saved slot/item IDs and keeps its own instance ID and Mystic enchantments. Two copies share a definition but never share their enchantment arrays. There is no new save schema or duplicate weapon inventory.

`WeaponCraftingRecipe` holds resource costs and returns independent arrays to existing crafting/queue APIs. The wooden starter has no crafting recipe. Both Veilcrystal variants retain different item IDs, bars and visual IDs. Current damage, timing, weight, costs, grades and unlock requirements are preserved exactly, including the existing equal weight of the two Veilcrystal variants.

Combat stat adapters, comparison metrics, crafting entries, equipped mass, inventory names/grades and the visual factory consume the definitions. `WeaponRules` retains compatibility helpers for existing callers; it no longer owns duplicate damage/interval tables. Ability coefficients and base ability cooldowns remain ability rules. `GearCraftCosts.Weapon` delegates to the definition's recipe.

Weapons do not contribute to armour type set counts. Their Mystic enchantments remain independent from armour set bonuses, following the shared one-slot-at-level-1 and two-slots-at-level-2 rule. Existing weapon grades remain unchanged; the earlier removal of armour rarity does not alter weapon grades.

## Validation

Use Unity 6000.3.9f1 with `Veilbound.Editor.WeaponFeatureBuild.Build`. This runs 193 weapon checks, 216 armour-domain regression checks and prototype logic checks, then produces `Builds/WeaponClassFeature/TheVeilbound.exe`.

Run the feature executable with `-veilSmoke -weaponClassReview` for the twelve-profile combat regression: 96 damage checks, 108 cooldown checks, 36 actual timer checks, basic hits/spin budgets, Haste composition, Rage/ability-kit regressions, resource displays and real isolated save/load of both Veilcrystal variants.

Validation results are in `Docs/Validation/weapon-domain.json` and `Docs/Validation/weapon-runtime.json`. Executables remain local and are excluded from Git. This branch is based on the armour model/set-bonus feature.
