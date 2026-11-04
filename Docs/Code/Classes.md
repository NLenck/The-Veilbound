# Class reference

[Back to the code guide](README.md) · [Worked flows](Flows.md) · [Source index](SourceIndex.md)

This reference explains the principal classes. The source index lists every C# source file under the live `Assets/` tree, including smaller helpers, nested types, partial files and test tools.

## Runtime coordination and state

| Class / type | Responsibility and ownership | How it is used |
| --- | --- | --- |
| [`VeilGame`](../../Assets/Veilbound/Scripts/VeilGame.cs) | Main `MonoBehaviour`: screen state, live player/run state, actor collections, input routing, simulation, UI and save orchestration. | Unity calls `Awake`, `Update`, `OnGUI` and shutdown callbacks. Other files declaring `partial class VeilGame` extend this same component. |
| `VeilGame.ScreenState` | Names the current screen/mode. | Simulation and input routing test this state; `SetState` also performs UI and combat-resume housekeeping. |
| [`Settlement`](../../Assets/Veilbound/Scripts/Progression.cs) | Serializable persistent gameplay state and operations, extended across many partial files. | `VeilGame.city` is the active instance. Public operations mutate this state; save adapters split it into character inventory/progression and town data. |
| [`RunBuild`](../../Assets/Veilbound/Scripts/Progression.cs) | Current expedition's ranks, ability attributes, passive potency/pick history, offered choices, banishes and lock/reroll budgets. | `StartRun` creates a new one. `Roll`, `Pick`, `Eligible`, `Amount` and `Rank` support level-up/relic choices and combat calculations. |
| [`Choice`](../../Assets/Veilbound/Scripts/Progression.cs) | One ability/passive/upgrade offer, including its ID, attribute, ascension and card rarity. | `RunBuild` offers/picks choices; card UI reads them. Card rarity remains active and is distinct from item grade. |
| `Foe`, `Shard`, `Hazard`, `Bolt`, `Flash` | Nested runtime records in `VeilGame`: enemy state, XP pickups, delayed danger, projectiles and transient effects. | Stored in coordinator-owned collections and advanced by gameplay methods. A `Foe` is not currently an independently updated enemy component. |
| [`Relic`](../../Assets/Veilbound/Scripts/ExpeditionFlow.cs) | Nested relic position and scene-reference record. | Spawn/collect methods update the collection; `RelicPointers.cs` derives guidance from it. |

## Shared equipment definitions

Definitions describe item types; they do not represent owned copies or visible GameObjects.

| Class | What it contains | Main callers / intended use |
| --- | --- | --- |
| [`ArmourDefinition`](../../Assets/Veilbound/Scripts/EquipmentDomain/ArmourDefinition.cs) | Stable catalogue identity, name, typed slot/type, canonical saved item ID, material/tier, armour, weight and recipe. No armour rarity. | Obtain through `ArmourCatalog.Find(slot, itemId)`. Crafting adapters, item metrics and loadout calculations read it. |
| `ArmourCatalog` | Cached lookup/building of definitions from current material and light-gear rules. Resolves legacy plate IDs to canonical definitions. | Repeated lookup of the same slot/item returns the shared object. It returns `null` for invalid combinations. |
| `ArmourCraftingRecipe` | Resource costs and Essence cost for one armour definition. | Read `Resources`; use `CopyResources()` when passing an array into existing APIs. Definitions calculate current base values from the material-rule classes rather than duplicating every item row. |
| `ArmourType`, `ArmourSlot` | Typed armour-family and slot names. | Use these when writing new equipment-domain code; existing save/UI adapters still use integer slot IDs. |
| [`WeaponDefinition`](../../Assets/Veilbound/Scripts/EquipmentDomain/WeaponDefinition.cs) | Stable identity, name/type/variant, slot, hands required, material/tier, weapon grade, damage, attack interval, DPS, weight, recipe and visual references. | Inventory, crafting, combat stat adapters and the visual factory read the shared profile. Current definitions are two-handed greatswords. |
| [`WeaponCatalog`](../../Assets/Veilbound/Scripts/EquipmentDomain/WeaponCatalog.cs) | Wooden starter and eleven craftable weapon profiles. | `Find` is strict. `ResolveLegacy` preserves negative-wooden/clamped helper compatibility. Use `All` for supported profiles. |
| `WeaponCraftingRecipe` | Bar, grip and wrap resource costs. | Each craftable definition has a recipe. The wooden starter has no recipe. Returned cost arrays can be changed by callers without changing shared recipe data. |
| `WeaponType`, `WeaponVariant` | Weapon archetype and standard/Titanium-base/Tungsten-base variant names. | Different Veilcrystal variants use different item, bar and visual IDs. These enums do not by themselves implement new weapon animations or combat styles. |

## Owned equipment, calculations and enchanting

| Class | Responsibility | How it is used |
| --- | --- | --- |
| [`EquipmentInstance`](../../Assets/Veilbound/Scripts/EquipmentDomain/EquipmentInstance.cs) | Owned ID, saved slot/item reference, inherited flag and per-copy enchantments. Resolves the appropriate definition. | Base class for the saved inventory object. Two copies can resolve the same definition while owning different enchantment arrays. |
| [`CraftedGear`](../../Assets/Veilbound/Scripts/GearRolls.cs) | Compatibility subclass used by existing inventory APIs and save JSON. | Stored in `Settlement.craftedGear`. Its retained `rarity` field is for old JSON/weapon grade compatibility; armour does not use it for enchanting or display. |
| `GearAffix` | A stat key/value modifier record. | Active modifiers are in `EquipmentInstance.enchantments`. The old `affixes` field is read for migration, cleared and excluded from gameplay. |
| [`EquipmentLoadout`](../../Assets/Veilbound/Scripts/EquipmentDomain/EquipmentLoadout.cs) | Read-only calculation over bound equipment arrays and owned items. | Read `Armour`, `Weight`, `Equipped(slot)`, `Sets()` and `Bonus(stat)`. Pass a candidate to `Bonus(stat, candidate)` or `Sets(candidate)` to preview replacing its slot. |
| [`ArmourSetBonuses`](../../Assets/Veilbound/Scripts/EquipmentDomain/ArmourSetBonuses.cs) | Value result containing Heavy/Leather/Cloth counts and cumulative 4/8-piece effects. | Calculated from body slots 0–7. Capes, weapons and extra backpack copies never count. These effects do not occupy enchantment slots. |
| [`EnchantmentRules`](../../Assets/Veilbound/Scripts/EquipmentDomain/EnchantmentRules.cs) | One slot at Mystic level 1, two from level 2, permanent cap of two; material power bands and saved-item normalization. | Called by Mystic operations and inventory normalization. Existing saved enchantment values are not rerolled. |
| [`MysticRules`](../../Assets/Veilbound/Scripts/MysticEnchanting.cs) | Enchantment stat pools, strength, cost and text. | `Settlement.EnchantmentRequirement` validates an action; `EnchantGear` performs a paid add/replacement. Mystic UI calls those operations. |
| [`GearRollRules`](../../Assets/Veilbound/Scripts/GearRolls.cs) | Compatibility helpers for instance creation, names, weapon grades and enchantment stat lookup/formatting. | `Value` reads active enchantments, not retired crafting affixes. The historical name does not imply that crafting still rolls random item quality. |
| [`GearItemMetrics`](../../Assets/Veilbound/Scripts/GearComparison.cs) | Per-item armour/mass or weapon damage/interval/DPS values. | `From(item)` reads definition data for UI. It does not include all the other equipped items. |
| `GearStatChange` | Before/after values, delta, unit and whether a lower value is better. | Comparison screens display its `Transition`, `DeltaText` and `Better` result. |
| `GearComparisonData` | Item primary-stat changes and projected bonus changes. | Supply the current `EquipmentLoadout` to include effects gained/lost at set thresholds. Item-only construction cannot account for the rest of the outfit. |

`Settlement` exposes adapters including `Loadout`, `EquippedRoll`, `FindRoll`, `EquipRoll`, `GearBonus`, `EquipmentArmour`, `EquipmentMass` and `EquippedWeaponDefinition`. Use the operations rather than independently modifying the item-code and equipped-instance arrays.

### Existing balance and compatibility helpers

| Type | Purpose |
| --- | --- |
| [`Gear`](../../Assets/Veilbound/Scripts/Equipment.cs) | Existing slot/material names, item-code decoding and slot size/cost scaling. |
| [`MetalDefinition`, `Metals`](../../Assets/Veilbound/Scripts/MetalProgression.cs) | Metal progression, colours, armour/weapon material multipliers, canonical/legacy plate IDs and material helpers. |
| [`LightGear`](../../Assets/Veilbound/Scripts/LightGear.cs) | Cloth/leather item IDs, base armour/weight formulas, material naming and type-set description adapter. The old intrinsic per-piece utility bonus was removed. |
| [`GearCraftCosts`](../../Assets/Veilbound/Scripts/GearCraftCosts.cs) | Shared armour cost policy and compatibility entry point to weapon recipes. |
| [`WeaponRules`](../../Assets/Veilbound/Scripts/WeaponRules.cs) | Compatibility name/material/damage/interval accessors backed by `WeaponCatalog`; also contains ability coefficients and base cooldown rules. |

## Crafting, production and town progression

| Class / family | Responsibility | How it is used |
| --- | --- | --- |
| [`ProductionBuildingDefinition`](../../Assets/Veilbound/Scripts/ProductionBuildings.cs) and subclasses | Shared building configuration, presentation identifiers, family and workshop pages. | `GatheringBuildingDefinition` and `CraftingBuildingDefinition` describe configured buildings. |
| `ProductionBuildingCatalog` | Configurations for Mine, Hunter's Cabin, Textile Tavern, Blacksmith and Clothier. | New shared UIs obtain settings and behavior through these definitions and runtime wrappers. |
| `WorkshopPageDefinition` | Processing/equipment/slot page configuration. | `WorkshopPageKind` distinguishes the page role; `EquipmentRecipeKind` selects plate, weapon, cloth or leather rows. |
| `ProductionBuilding` | Base runtime facade over a `Settlement` and one building definition. | Reads current building level and provides slot-unlock logic. It does not hold a second serialized copy of the town. |
| `GatheringBuilding` | Raw-resource gathering, capacities, assignments and status. | Obtain with `town.Gatherer(family)` for raw families 0–2. |
| `CraftingBuilding` | Workshop behavior and ownership checks for processing lines / equipment pages. | Obtain with `town.Crafter("blacksmith")` or `town.Crafter("clothier")`. |
| [`CraftingRecipeSet`](../../Assets/Veilbound/Scripts/CraftingRecipes.cs) | Adapter exposing rows, columns, a selected entry, affordability, direct crafting and queue actions. | `town.EquipmentRecipes(kind)` is used by shared workshop UI. Armour/weapon entry values come from their definitions. |
| `EquipmentRecipeEntry`, `WorkshopStat` | UI-facing recipe row data, requirements, ingredients, displayed stats and definition references. | Useful for building the display; not a separate saved owned item. |
| [`ProcessingLine`](../../Assets/Veilbound/Scripts/ProcessingLine.cs) | Shared bar/leather/cloth processing operations: assign slots, pay inputs, advance work, queue, cancel and report status. | Obtain with `town.ProcessingLine(family)` for processed families 3–5. |
| `ProcessingSlotAccess`, `BarSlotAccess`, `MaterialSlotAccess` | Live adapters to different saved slot shapes. | Their setters change the underlying `RefiningSlot` or `MaterialProcessSlot`; they do not copy timers or ingredients into independent state. |
| `ProcessingOrderAccess` | Common live view over bar and material orders. | Used by processing logic/UI to work with their IDs, quantities, completed units and recipes. |
| `ProcessingSlotStatus`, [`ProductionActivity`](../../Assets/Veilbound/Scripts/ProductionStatus.cs) | Display/reporting results for running, waiting or blocked work. | UI reads these results instead of guessing production state. |
| [`ProducerSlot`](../../Assets/Veilbound/Scripts/ProductionSlots.cs), [`RefiningSlot`](../../Assets/Veilbound/Scripts/RefiningSlots.cs), [`MaterialProcessSlot`](../../Assets/Veilbound/Scripts/MaterialProcessing.cs) | Serializable per-slot assignment/work state. | `Settlement` owns the saved arrays; wrapper services operate on them. |
| [`MineWorkOrder`](../../Assets/Veilbound/Scripts/MineWorkOrders.cs), [`SmithWorkOrder`](../../Assets/Veilbound/Scripts/SmithWorkOrders.cs), `MaterialProcessOrder` | Serializable manual batch progress. | Queue/cancellation operations preserve or refund the appropriate reserved inputs. |
| [`ProductionStep`](../../Assets/Veilbound/Scripts/AutoProduction.cs) | Repeat-schedule step. | Repeat scheduling combines these with manual-priority and automatic-demand rules. |
| [`SettlementJob`](../../Assets/Veilbound/Scripts/SettlementPlanner.cs) | Saved high-level goal: construction, research, bars, gear or processed materials. | The planner finds missing inputs, reserves incoming work and supplies dependencies. A goal is distinct from a manual work order. |
| [`ResourceCost`](../../Assets/Veilbound/Scripts/MetalProgression.cs) | Resource family, index and quantity tuple. | Recipes and resource transactions use these identifiers. See the encoding table in [Flows.md](Flows.md). |
| [`TownUpgradeCost`](../../Assets/Veilbound/Scripts/TownProgression.cs) | Wood/stone plus material costs. | Building and research operations validate/pay this aggregate. |
| [`LibraryResearch`](../../Assets/Veilbound/Scripts/LibraryResearch.cs) | Research definitions, limits, duration and costs. | `Settlement.StartResearch`, construction/research requirements and time advancement consume it. Active research/construction state remains in `Settlement`. |

## Combat, progression and navigation

| Class / code location | Responsibility and relationship |
| --- | --- |
| [`AbilitySpecs`, `AbilityUpgrade`](../../Assets/Veilbound/Scripts/AbilityUpgrades.cs) | Ability attribute definitions and upgrade policy used by `RunBuild`, card descriptions and combat formulas. |
| [`PassiveRules`](../../Assets/Veilbound/Scripts/PassiveRules.cs) | Passive percentage conversion, caps and related `RunBuild` behavior. |
| [`AbilityAscensions`, `AscensionSpec`](../../Assets/Veilbound/Scripts/AbilityAscensions.cs) | Ascension definitions; `AscensionCombat.cs` executes their live effects through `VeilGame`. |
| [`Rarities`, `UpgradeRarity`](../../Assets/Veilbound/Scripts/UpgradeRarity.cs) | Card rarity strengths/odds/colours; the enum also remains part of legacy item/weapon-grade representation. |
| [`HasteRules`](../../Assets/Veilbound/Scripts/HasteRules.cs) | Haste rating to cooldown-reduction conversion. Leather's 8-piece attack-rate multiplier is applied separately. |
| [`EnemyTypes`, `HuskKind`](../../Assets/Veilbound/Scripts/EnemyTypes.cs) | Enemy-kind health, speed, damage, XP and radius scaling. |
| [`ExpeditionSpawns`](../../Assets/Veilbound/Scripts/ExpeditionSpawns.cs) | Stage/encounter selection rules. The runtime spawner creates/records `Foe` instances. |
| [`ExpeditionDifficulty`](../../Assets/Veilbound/Scripts/ExpeditionDifficulty.cs), [`ExpeditionModifiers`](../../Assets/Veilbound/Scripts/ExpeditionModifiers.cs) | Difficulty and modifier rules consumed by expedition setup, combat and production-time scaling. |
| [`EnemySpawnPlacement`](../../Assets/Veilbound/Scripts/EnemySpawnPlacement.cs) | Measures visual bounds and finds entry placements outside the gameplay view with collision/navigation constraints. |
| [`ArenaCollision`](../../Assets/Veilbound/Scripts/ArenaCollision.cs) | Obstacle/wall data, swept movement, clearance and line-of-sight operations. Used by player movement, AI and direct attacks. |
| [`ArenaNavigation`](../../Assets/Veilbound/Scripts/ArenaNavigation.cs) | Shared Dijkstra flow fields with different body clearances. Steers enemy crowds around obstacles instead of giving every enemy an independent route search. |
| [`EnemyPursuit`](../../Assets/Veilbound/Scripts/EnemyPursuit.cs) | Pursuit-related policy and coordinator helpers. |
| [`RelicPlacement`](../../Assets/Veilbound/Scripts/RelicPlacement.cs) | Relic placement history/coverage decisions. `ExpeditionFlow.cs` owns the live relic records. |
| [`VeilBoundary`](../../Assets/Veilbound/Scripts/VeilBoundary.cs) | Boundary presentation/runtime helper created with the arena's player radius. |

Attack execution remains primarily in `VeilGame` partial files: `RageAbilityKit.cs`, `EnemyCombat.cs`, `EnemyCharge.cs`, `EnemyGroundSlam.cs`, `AscensionCombat.cs`, `SpinningWeapons.cs` and `CombatReadouts.cs`. Those filenames describe code groups, not separately constructed combat-system classes.

## Presentation classes

| Class | Responsibility and relationship |
| --- | --- |
| [`HeroRig`](../../Assets/Veilbound/Scripts/HeroRig.cs) | Ordinary C# facade for a player's root, joints, sword grip and presentation calls. Creates production visuals when available and retains a legacy fallback. |
| [`ProductionHeroVisual`](../../Assets/Veilbound/Scripts/ProductionHeroVisual.cs) | Unity component selecting production player data, equipping meshes/materials and evaluating motion/hand grips. |
| [`BronzeHeroVisual`](../../Assets/Veilbound/Scripts/BronzeHeroVisual.cs) | Legacy visual implementation and compatibility facade for existing joint/part callers. Its presence does not necessarily mean the old model path is active. |
| [`MasterPlateEquipment`](../../Assets/VeilboundPlateMasters/Runtime/MasterPlateEquipment.cs) | Unity component controlling armour slot meshes, palettes and uncovered body regions. Supports original masking and current replacement mode. Belt tuck is restricted to the appropriate waist/trouser regions. |
| `PlateTierBinding` | Palette/family/item binding used by the equipment component. |
| [`StarterEquipment`](../../Assets/Veilbound/Scripts/StarterEquipment.cs) | Equipped weapon visual factory using weapon definitions, prefabs or the greatsword library. |
| [`GreatswordVisualLibrary`, `GreatswordEntry`](../../Assets/VeilboundGreatswords/Runtime/GreatswordVisualLibrary.cs) | ScriptableObject and entries mapping visual IDs to prefabs. Visual IDs are not owned instance IDs. |
| [`GreatswordMotionSet`](../../Assets/Veilbound/Scripts/GreatswordMotionSet.cs), [`GreatswordMotionPlayer`](../../Assets/Veilbound/Scripts/GreatswordMotionPlayer.cs) | Shared motion clips and runtime animation evaluation. Combat stats do not come from animation clip names. |
| [`ProductionModelCatalog`, `ProductionPlayerEntry`, `ProductionEnemyEntry`](../../Assets/Veilbound/Scripts/ProductionModelCatalog.cs) | Resource-loaded ScriptableObject and records selecting player/enemy prefabs and player locomotion clips. |
| [`HuskRig`](../../Assets/Veilbound/Scripts/HuskRig.cs) | Ordinary enemy-presentation facade used by live `Foe` records. |
| [`MonsterVisual`](../../Assets/Veilbound/Scripts/MonsterVisual.cs) | Enemy visual creation and pose/fuse/hit behavior. |
| [`ProductionEnemyMotion`](../../Assets/Veilbound/Scripts/ProductionEnemyMotion.cs), [`EnemyMixamoMotion`](../../Assets/Veilbound/Scripts/EnemyMixamoMotion.cs) | Enemy motion selection/evaluation, including imported animation where available. |
| [`AreaWarningVisual`](../../Assets/Veilbound/Scripts/AreaWarningVisual.cs), [`ChargeWarningVisual`](../../Assets/Veilbound/Scripts/ChargeWarningVisual.cs) | Visible danger/charge telegraphs; actual damage timing is handled by combat state. |
| [`WorldArt`](../../Assets/Veilbound/Scripts/WorldArt.cs), [`GraveyardArena`](../../Assets/Veilbound/Scripts/GraveyardArena.cs), [`RuinedTownArena`](../../Assets/Veilbound/Scripts/RuinedTownArena.cs) | Scene/model factories and arena construction. |

## Saving and preferences

| Class | What it does |
| --- | --- |
| [`CharacterInventory`](../../Assets/Veilbound/Scripts/CharacterSaves.cs) | Captures/applies character-owned inventory fields and strips them from the serialized town copy. `Capture` transfers references; it is not a deep-clone API. |
| `SavedCharacter` | Character identity, appearance, virtue, progression, revision and inventory record. |
| `SavedTown` | Matching character ID/revision and the town-state portion of the save. |
| `CharacterSummary`, `CharacterRoster` | Lightweight selection/index information and active character ID. |
| `LostKnowledgeEntry`, `LostKnowledgeSave` | Account-level knowledge/discovery records. |
| `CharacterSaveStore` | Validates IDs, reads/writes save records, maintains backups and journals and recovers interrupted save/deletion operations. Call through the established profile flow for real gameplay. |
| `CharacterSaveJournal`, `CharacterDeleteJournal` | Internal transaction/recovery records, not gameplay entities. |
| [`VeilPreferences`, `ControlAction`](../../Assets/Veilbound/Scripts/GamePreferences.cs) | Local display/audio/control preferences and control-action identifiers. Their persistence is separate from character progression. |

## UI and editor tooling

`InventoryScreen.cs`, `SharedCraftingUI.cs`, `MysticUI.cs`, `CharacterSheet.cs`, `ExpeditionSetup.cs`, `SanctuaryCityUI.cs`, `ResourceTopBar.cs` and other screen files extend `VeilGame`. Input handlers, drawing methods and UI selection fields currently share that component. `SharedWorkshopLayout` supplies common equipment-grid geometry, while recipe/status objects supply the displayed data.

Classes under [`Assets/Veilbound/Editor`](../../Assets/Veilbound/Editor) are editor entry points, not player gameplay components. Important examples are `PrototypeBuild`, `ProductionModelImport`, `RawGearBuild`, `ArmourFeatureBuild` and `WeaponFeatureBuild`. `*Review.cs`, `*Smoke.cs` and validation helpers form the existing test/review harness; many are also `VeilGame` partial files selected by command-line flags.
