# Working on the code

[Back to the code guide](README.md) · [Class reference](Classes.md) · [Worked flows](Flows.md)

## Choose the place that owns the rule

| Change | Start here | Check the related consumers |
| --- | --- | --- |
| Weapon base damage, interval or profile identity | `EquipmentDomain/WeaponCatalog.cs` and `WeaponDefinition.cs`. | Combat regression, crafting entries, inventory comparisons and save IDs. |
| Armour base defence/weight by material | `MetalProgression.cs` / `LightGear.cs`; exposed through `ArmourCatalog`. | `EquipmentLoadout`, weight penalties, comparison metrics and recipe/UI adapters. |
| Four/eight-piece set effects | `EquipmentDomain/ArmourSetBonuses.cs`. | Set display in `ArmourSetsUI.cs`, `GearComparisonData`, `MaxHealth`, `SkillAttackRate`, XP and regeneration consumers. |
| Mystic slot limits/material power | `EquipmentDomain/EnchantmentRules.cs`. | `MysticRules`, requirement checks, UI messages, save normalization and exact saved enchantments. |
| Mystic cost/stat selection/strength | `MysticEnchanting.cs` and its current stat-table helpers. | Random/category/exact modes and atomic failed actions. |
| Crafting costs | `GearCraftCosts.cs` / `WeaponCraftingRecipe`. | Direct crafting, queued goals, workshop cost displays and automatic supply. |
| Building configuration/pages | `ProductionBuildings.cs`. | Shared workshop/gathering UI and building ownership checks. |
| Production behavior/timers | `ProcessingLine.cs` and the relevant saved-order/slot adapters. | Refunds, manual priority, queued demand and large-step versus frame-step simulation. |
| Research/construction | `LibraryResearch.cs` / `TownProgression.cs`. | Queue planner, resource payment, cancellation/refunds and completion timing. |
| Ability attributes or passives | `AbilityUpgrades.cs`, `PassiveRules.cs`, `AbilityAscensions.cs`. | Offers, descriptions, eligibility/caps and actual combat formulas. |
| Enemy balance/selection/behavior | `EnemyTypes.cs`, `ExpeditionSpawns.cs`, `EnemyCombat.cs` and specialized attack files. | Visual bounds, pursuit, telegraphs, hit timing and rewards. |
| Character meshes/materials/animation | Production model catalogue, `ProductionHeroVisual`, `MasterPlateEquipment` and motion/visual libraries. | Both appearances, mixed equipment, exposed body regions and grip/collider behavior. |
| Screen layout/input | The active screen's `VeilGame` partial and shared layout/focus helpers. | Controller and mouse paths, screen transitions, GUI transform restoration and simulation pause state. |
| Save fields | `CharacterSaves.cs` plus the owning state class and normalization methods. | Inventory capture/apply/strip, migration, journals, separate copies and roundtrip tests. |

Paths above are under `Assets/Veilbound/Scripts/` unless the class reference links to another asset package.

## Add a weapon profile carefully

A new `WeaponDefinition` row is only one part of adding supported content. Currently the game has a wooden starter, eleven craftable profiles and a UI/material progression designed around those IDs.

1. Choose a new stable saved ID; never reuse an old ID for a different weapon.
2. Add its definition and determine type, stats, material/grade, handedness, recipe and visual route.
3. Review `WeaponCatalog.CraftableCount`, selection/ownership arrays, bar resources, recipe/research bounds and loops that assume the current number of materials.
4. Add the visual entry and required prefab/imported assets with their `.meta` files. A new `WeaponType` enum value alone does not create new attacks, motion clips, off-hand rules or animation support.
5. Update crafting selection and variant handling, inventory displays and relevant save normalization.
6. Add a golden balance case, valid/invalid crafting cases, owned-copy independence and real combat/save coverage.

For a balance-only change to an existing weapon, change its authoritative definition and expected balance fixture together. Do not add a second damage table to UI code.

## Add or change armour content

Armour definitions are built from slot, material and tier rules. Use `ArmourCatalog.Find` and the existing encoding helpers. An additional material/tier can affect ownership array sizes, recipe tables, research gates, mesh families/palettes, thumbnails and save migration; adding a name alone is insufficient.

For a new set effect, define its count rule independently from enchantments. Confirm 3/4 and 7/8 boundaries, mixed outfits, mixed material tiers, unequipping, capes/weapons and comparison previews. Ensure the chosen stat is actually consumed in combat or production; a new string key without a consumer has no gameplay effect.

## Preserve model boundaries

- Keep shared definition data separate from owned instance identity and enchantments.
- Use model operations such as `CraftMetalWeapon`, `EquipRoll` and `EnchantGear` instead of editing both saved arrays by hand.
- Keep set counts in `ArmourSetBonuses`/`EquipmentLoadout`, not in enchantment arrays or per-item copies of the whole-outfit bonus.
- Return calculated/display results to UI. Avoid independently reimplementing the recipe, weight, damage or set formula inside a drawing method.
- Add a focused class when it owns independent behavior/state. Another partial file improves navigation but still joins the same shared `VeilGame`/`Settlement` class.
- Existing wrappers still have compatibility responsibilities. Removing `WeaponRules`, old item codes or serialized fields requires migrating their callers/saves first.

## Validate changes

The project has editor validation methods and a built-player smoke/review harness. Many reviews are coroutines within the `VeilGame` partial class; they are not conventional NUnit tests. Keep that distinction in mind when selecting an entry point.

| Entry point | Purpose |
| --- | --- |
| `Veilbound.Editor.PrototypeBuild.Checks` | Existing broad economy, gear migration, offer and collision/navigation logic checks. |
| `Veilbound.Editor.ArmourFeatureBuild.Validate` | Current armour-domain checks, set thresholds, independent enchantments, definitions, recipes and save compatibility. |
| `Veilbound.Editor.WeaponFeatureBuild.Validate` | Current weapon profile, recipe, visual-reference, owned-copy and save checks. |
| `Veilbound.Editor.RawGearBuild.Validate` | Current equipment visibility/mesh configuration checks. |
| `Veilbound.Editor.WeaponFeatureBuild.Build` | Runs weapon/armour/prototype logic checks and builds `Builds/WeaponClassFeature`. |
| `Veilbound.Editor.ArmourFeatureBuild.Build` | Armour/prototype/equipment validation and `Builds/ArmourSetsFeature`. |
| `Veilbound.Editor.PrototypeBuild.Build` | Legacy scene/art preparation plus a Windows build in `Builds/Windows`; it is not simply a synonym for the current Integrated executable. |

Example PowerShell validation command:

```powershell
$unityEditor = 'C:/Program Files/Unity/Hub/Editor/6000.3.9f1/Editor/Unity.exe'
& $unityEditor -batchmode -quit -projectPath 'C:/The Veilbound' `
  -executeMethod Veilbound.Editor.WeaponFeatureBuild.Validate `
  -logFile 'C:/The Veilbound/Logs/weapon-domain-validation.log'
```

Check the log for the pass marker and for exceptions/compile errors. On this Windows setup the process launch can return before the editor has finished; an empty initial tool/terminal response is not proof of a completed validation.

For actual player behavior, build the intended executable and run an appropriate flag:

| Flags | Coverage |
| --- | --- |
| `-veilSmoke -weaponClassReview` | Twelve weapon profiles, damage, cooldowns/timers, Rage/kit/resource regressions and save reload. |
| `-veilSmoke -armourSetsReview` | Actual set effects, XP pickup, attack speed, regeneration, Mystic behavior and UI captures. |
| `-veilSmoke -lightGearReview` | Cloth/leather crafting, processing/queue behavior, stats and saves. |
| `-veilSmoke -mysticReview` | Mystic levels, paid actions, failure atomicity, per-copy enchantments and UI. |
| `-veilSmoke -pointerScaleReview` | Rendered relic pointers across resolutions, directions, camera rotations and map positions. |

For example:

```powershell
& 'C:/The Veilbound/Builds/WeaponClassFeature/TheVeilbound.exe' `
  -veilSmoke -weaponClassReview -screen-fullscreen 0 `
  -logFile 'C:/The Veilbound/Logs/weapon-runtime-review.log'
```

Use one review at a time when it shares smoke paths. These are executable test scenarios; they can close the player automatically. Screenshot reviews need a visible game window on this machine. Review fixtures should use isolated saves, supply required recipe inputs and test gameplay operations rather than bypassing them. A screenshot alone does not prove that damage, payment, save migration or a set threshold works.

Some older reports describe previous versions. The current `Docs/Validation/` results and feature-specific guides identify the checks run for the recent equipment changes. A documentation-only change needs source/reference/link verification, not a fresh gameplay build merely to reproduce unchanged behavior.

## Work through Git and GitHub

1. Start from the appropriate integration or feature base with a clean working tree.
2. Create a focused `feature/...`, `fix/...` or `docs/...` branch.
3. Review the diff, make the smallest coherent change, and run relevant checks.
4. Commit source, documentation and Unity `.meta` files together. Git LFS handles configured binary assets.
5. Push and open a PR with the behavior change, validation and any compatibility implications.
6. For stacked work, target the dependency branch while its PR is open; retarget the next PR after the dependency is integrated. State the dependency in the PR.

Do not commit builds, caches, personal saves, credentials or local archives. A feature build can exist locally without changing `Builds/Integrated`; state which executable contains the change when handing it over for testing.

## Keep documentation current

Update the class reference or worked flow when changing ownership, a public operation or a save boundary. Update the detailed equipment/weapon guide when changing those contracts. Regenerate the source index from the repository root:

```powershell
python Tools/Documentation/update_source_index.py
```

Then verify all local Markdown links and inspect the documentation diff. The source index lists declarations; it does not infer behavioral independence merely because a type or filename exists.
