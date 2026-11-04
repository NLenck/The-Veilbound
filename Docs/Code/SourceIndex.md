# Complete C# source index

[Back to the code guide](README.md) · [Class reference](Classes.md)

Generated from the live `Assets/` C# files by `Tools/Documentation/update_source_index.py`.
Root art-production copies, Packages and local archives are excluded. Nested types are listed by their declared short names.
Repeated `VeilGame`, `Settlement` or `RunBuild` entries refer to partial declarations of the same class.
Category grouping is based on path/filename; a review helper can still be compiled into the player unless its source is editor-only.

**313 C# files indexed.**

## Equipment domain

| Source file | Declared types |
| --- | --- |
| [Assets/Veilbound/Scripts/EquipmentDomain/ArmourDefinition.cs](../../Assets/Veilbound/Scripts/EquipmentDomain/ArmourDefinition.cs) | `ArmourType` (enum), `ArmourSlot` (enum), `ArmourDefinition` (class), `ArmourCraftingRecipe` (class), `ArmourCatalog` (class) |
| [Assets/Veilbound/Scripts/EquipmentDomain/ArmourSetBonuses.cs](../../Assets/Veilbound/Scripts/EquipmentDomain/ArmourSetBonuses.cs) | `ArmourSetBonuses` (struct) |
| [Assets/Veilbound/Scripts/EquipmentDomain/EnchantmentRules.cs](../../Assets/Veilbound/Scripts/EquipmentDomain/EnchantmentRules.cs) | `EnchantmentRules` (class) |
| [Assets/Veilbound/Scripts/EquipmentDomain/EquipmentInstance.cs](../../Assets/Veilbound/Scripts/EquipmentDomain/EquipmentInstance.cs) | `EquipmentInstance` (class) |
| [Assets/Veilbound/Scripts/EquipmentDomain/EquipmentLoadout.cs](../../Assets/Veilbound/Scripts/EquipmentDomain/EquipmentLoadout.cs) | `EquipmentLoadout` (class) |
| [Assets/Veilbound/Scripts/EquipmentDomain/WeaponCatalog.cs](../../Assets/Veilbound/Scripts/EquipmentDomain/WeaponCatalog.cs) | `WeaponCatalog` (class) |
| [Assets/Veilbound/Scripts/EquipmentDomain/WeaponDefinition.cs](../../Assets/Veilbound/Scripts/EquipmentDomain/WeaponDefinition.cs) | `WeaponType` (enum), `WeaponVariant` (enum), `WeaponDefinition` (class), `WeaponCraftingRecipe` (class) |

## Main runtime gameplay and UI

| Source file | Declared types |
| --- | --- |
| [Assets/Veilbound/Scripts/AbilityAscensions.cs](../../Assets/Veilbound/Scripts/AbilityAscensions.cs) | `AscensionSpec` (class), `AbilityAscensions` (class), `RunBuild` (partial class) |
| [Assets/Veilbound/Scripts/AbilityFeedback.cs](../../Assets/Veilbound/Scripts/AbilityFeedback.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/AbilityUpgrades.cs](../../Assets/Veilbound/Scripts/AbilityUpgrades.cs) | `AbilityUpgrade` (class), `AbilitySpecs` (class), `RunBuild` (partial class) |
| [Assets/Veilbound/Scripts/AreaWarningVisual.cs](../../Assets/Veilbound/Scripts/AreaWarningVisual.cs) | `AreaWarningVisual` (class) |
| [Assets/Veilbound/Scripts/ArenaCollision.cs](../../Assets/Veilbound/Scripts/ArenaCollision.cs) | `ArenaCollision` (class), `Obstacle` (struct), `Wall` (struct) |
| [Assets/Veilbound/Scripts/ArenaNavigation.cs](../../Assets/Veilbound/Scripts/ArenaNavigation.cs) | `ArenaNavigation` (class) |
| [Assets/Veilbound/Scripts/ArmourSetsUI.cs](../../Assets/Veilbound/Scripts/ArmourSetsUI.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/ArmourVisuals.cs](../../Assets/Veilbound/Scripts/ArmourVisuals.cs) | `ArmourVisuals` (class) |
| [Assets/Veilbound/Scripts/AscensionCombat.cs](../../Assets/Veilbound/Scripts/AscensionCombat.cs) | `VeilGame` (partial class), `AscendedFoe` (class), `AscensionZone` (class), `AscensionEvent` (class), `AscensionFlight` (class), `BloodTether` (class) |
| [Assets/Veilbound/Scripts/AutoProduction.cs](../../Assets/Veilbound/Scripts/AutoProduction.cs) | `ProductionStep` (class), `Settlement` (partial class) |
| [Assets/Veilbound/Scripts/BronzeHeroVisual.cs](../../Assets/Veilbound/Scripts/BronzeHeroVisual.cs) | `BronzeHeroVisual` (class) |
| [Assets/Veilbound/Scripts/BuildingActivity.cs](../../Assets/Veilbound/Scripts/BuildingActivity.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/BuildingUnlocks.cs](../../Assets/Veilbound/Scripts/BuildingUnlocks.cs) | `Settlement` (partial class), `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/BuildOverview.cs](../../Assets/Veilbound/Scripts/BuildOverview.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/CharacterDeletion.cs](../../Assets/Veilbound/Scripts/CharacterDeletion.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/CharacterRosterUI.cs](../../Assets/Veilbound/Scripts/CharacterRosterUI.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/CharacterSaves.cs](../../Assets/Veilbound/Scripts/CharacterSaves.cs) | `CharacterInventory` (class), `SavedCharacter` (class), `CharacterSummary` (class), `CharacterRoster` (class), `LostKnowledgeEntry` (class), `LostKnowledgeSave` (class), `SavedTown` (class), `CharacterSaveJournal` (class), `CharacterDeleteJournal` (class), `CharacterSaveStore` (class) |
| [Assets/Veilbound/Scripts/CharacterSelectScreen.cs](../../Assets/Veilbound/Scripts/CharacterSelectScreen.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/CharacterSheet.cs](../../Assets/Veilbound/Scripts/CharacterSheet.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/ChargeWarningVisual.cs](../../Assets/Veilbound/Scripts/ChargeWarningVisual.cs) | `ChargeWarningVisual` (class), `EnemyAttackGeometry` (class) |
| [Assets/Veilbound/Scripts/ChoiceGridNavigation.cs](../../Assets/Veilbound/Scripts/ChoiceGridNavigation.cs) | `VeilGame` (partial class), `ChoiceNode` (struct) |
| [Assets/Veilbound/Scripts/CitySidebar.cs](../../Assets/Veilbound/Scripts/CitySidebar.cs) | `VeilGame` (partial class), `SidebarActivity` (struct) |
| [Assets/Veilbound/Scripts/CleanHud.cs](../../Assets/Veilbound/Scripts/CleanHud.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/ClothierUI.cs](../../Assets/Veilbound/Scripts/ClothierUI.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/CombatIndicatorColors.cs](../../Assets/Veilbound/Scripts/CombatIndicatorColors.cs) | `CombatIndicatorColors` (class) |
| [Assets/Veilbound/Scripts/CombatReadouts.cs](../../Assets/Veilbound/Scripts/CombatReadouts.cs) | `VeilGame` (partial class), `AbilityDamageRecord` (class), `DamageSample` (struct), `DamageNumber` (class) |
| [Assets/Veilbound/Scripts/CraftingRecipes.cs](../../Assets/Veilbound/Scripts/CraftingRecipes.cs) | `WorkshopStat` (struct), `EquipmentRecipeEntry` (class), `CraftingRecipeSet` (class), `Settlement` (partial class) |
| [Assets/Veilbound/Scripts/CrystalClouds.cs](../../Assets/Veilbound/Scripts/CrystalClouds.cs) | `CrystalClouds` (class) |
| [Assets/Veilbound/Scripts/CrystalClusters.cs](../../Assets/Veilbound/Scripts/CrystalClusters.cs) | `CrystalClusters` (class) |
| [Assets/Veilbound/Scripts/CultistDaggerPose.cs](../../Assets/Veilbound/Scripts/CultistDaggerPose.cs) | `CultistDaggerPose` (class) |
| [Assets/Veilbound/Scripts/EnemyArrivalFog.cs](../../Assets/Veilbound/Scripts/EnemyArrivalFog.cs) | `EnemyArrivalFog` (class) |
| [Assets/Veilbound/Scripts/EnemyBodyCollision.cs](../../Assets/Veilbound/Scripts/EnemyBodyCollision.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/EnemyCharge.cs](../../Assets/Veilbound/Scripts/EnemyCharge.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/EnemyCombat.cs](../../Assets/Veilbound/Scripts/EnemyCombat.cs) | `VeilGame` (partial class), `Lob` (class) |
| [Assets/Veilbound/Scripts/EnemyGroundSlam.cs](../../Assets/Veilbound/Scripts/EnemyGroundSlam.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/EnemyMixamoMotion.cs](../../Assets/Veilbound/Scripts/EnemyMixamoMotion.cs) | `EnemyMixamoMotion` (class) |
| [Assets/Veilbound/Scripts/EnemyMotionSamples.cs](../../Assets/Veilbound/Scripts/EnemyMotionSamples.cs) | `EnemyMotionCycle` (class), `EnemyMotionSamples` (class) |
| [Assets/Veilbound/Scripts/EnemyPursuit.cs](../../Assets/Veilbound/Scripts/EnemyPursuit.cs) | `EnemyPursuit` (class), `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/EnemySpawnPlacement.cs](../../Assets/Veilbound/Scripts/EnemySpawnPlacement.cs) | `EnemySpawnPlacement` (class) |
| [Assets/Veilbound/Scripts/EnemyTypes.cs](../../Assets/Veilbound/Scripts/EnemyTypes.cs) | `HuskKind` (enum), `EnemyTypes` (class) |
| [Assets/Veilbound/Scripts/EnvironmentCrystalGlow.cs](../../Assets/Veilbound/Scripts/EnvironmentCrystalGlow.cs) | `EnvironmentCrystalGlow` (class) |
| [Assets/Veilbound/Scripts/Equipment.cs](../../Assets/Veilbound/Scripts/Equipment.cs) | `Gear` (class), `Settlement` (partial class) |
| [Assets/Veilbound/Scripts/EquipmentPaperDoll.cs](../../Assets/Veilbound/Scripts/EquipmentPaperDoll.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/EquipmentUI.cs](../../Assets/Veilbound/Scripts/EquipmentUI.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/EtherealMovement.cs](../../Assets/Veilbound/Scripts/EtherealMovement.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/ExpeditionArt.cs](../../Assets/Veilbound/Scripts/ExpeditionArt.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/ExpeditionDifficulty.cs](../../Assets/Veilbound/Scripts/ExpeditionDifficulty.cs) | `ExpeditionDifficulty` (class) |
| [Assets/Veilbound/Scripts/ExpeditionFlow.cs](../../Assets/Veilbound/Scripts/ExpeditionFlow.cs) | `RunBuild` (partial class), `VeilGame` (partial class), `Relic` (class) |
| [Assets/Veilbound/Scripts/ExpeditionMaps.cs](../../Assets/Veilbound/Scripts/ExpeditionMaps.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/ExpeditionModifiers.cs](../../Assets/Veilbound/Scripts/ExpeditionModifiers.cs) | `Settlement` (partial class), `ExpeditionModifiers` (class), `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/ExpeditionSetup.cs](../../Assets/Veilbound/Scripts/ExpeditionSetup.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/ExpeditionSpawns.cs](../../Assets/Veilbound/Scripts/ExpeditionSpawns.cs) | `ExpeditionSpawns` (class) |
| [Assets/Veilbound/Scripts/GameMusic.cs](../../Assets/Veilbound/Scripts/GameMusic.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/GamePreferences.cs](../../Assets/Veilbound/Scripts/GamePreferences.cs) | `ControlAction` (enum), `VeilPreferences` (class), `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/GameSettingsUI.cs](../../Assets/Veilbound/Scripts/GameSettingsUI.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/GameUI.cs](../../Assets/Veilbound/Scripts/GameUI.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/GearComparison.cs](../../Assets/Veilbound/Scripts/GearComparison.cs) | `GearItemMetrics` (struct), `GearStatChange` (struct), `GearComparisonData` (class) |
| [Assets/Veilbound/Scripts/GearCraftCosts.cs](../../Assets/Veilbound/Scripts/GearCraftCosts.cs) | `GearCraftCosts` (class) |
| [Assets/Veilbound/Scripts/GearRolls.cs](../../Assets/Veilbound/Scripts/GearRolls.cs) | `GearAffix` (class), `CraftedGear` (class), `GearRollRules` (class), `Settlement` (partial class) |
| [Assets/Veilbound/Scripts/GearRollUI.cs](../../Assets/Veilbound/Scripts/GearRollUI.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/GhoulRig.cs](../../Assets/Veilbound/Scripts/GhoulRig.cs) | `GhoulRig` (class) |
| [Assets/Veilbound/Scripts/GraveyardArena.cs](../../Assets/Veilbound/Scripts/GraveyardArena.cs) | `GraveyardArena` (class) |
| [Assets/Veilbound/Scripts/GraveyardBrazierFlame.cs](../../Assets/Veilbound/Scripts/GraveyardBrazierFlame.cs) | `GraveyardBrazierFlame` (class) |
| [Assets/Veilbound/Scripts/GraveyardDressing.cs](../../Assets/Veilbound/Scripts/GraveyardDressing.cs) | `GraveyardDressing` (class) |
| [Assets/Veilbound/Scripts/GreatswordMotionPlayer.cs](../../Assets/Veilbound/Scripts/GreatswordMotionPlayer.cs) | `GreatswordMotionPlayer` (class) |
| [Assets/Veilbound/Scripts/GreatswordMotionSet.cs](../../Assets/Veilbound/Scripts/GreatswordMotionSet.cs) | `GreatswordMotionSet` (class) |
| [Assets/Veilbound/Scripts/HasteRules.cs](../../Assets/Veilbound/Scripts/HasteRules.cs) | `HasteRules` (class) |
| [Assets/Veilbound/Scripts/HeroArt.cs](../../Assets/Veilbound/Scripts/HeroArt.cs) | `HeroArt` (class), `Piece` (class), `Asset` (class), `HeroGearAnchor` (class) |
| [Assets/Veilbound/Scripts/HeroBodyMesh.cs](../../Assets/Veilbound/Scripts/HeroBodyMesh.cs) | `HeroBodyMesh` (class), `Form` (struct) |
| [Assets/Veilbound/Scripts/HeroRig.cs](../../Assets/Veilbound/Scripts/HeroRig.cs) | `HeroRig` (class) |
| [Assets/Veilbound/Scripts/HudStyle.cs](../../Assets/Veilbound/Scripts/HudStyle.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/HuskRig.cs](../../Assets/Veilbound/Scripts/HuskRig.cs) | `HuskRig` (class) |
| [Assets/Veilbound/Scripts/Industry.cs](../../Assets/Veilbound/Scripts/Industry.cs) | `Settlement` (partial class) |
| [Assets/Veilbound/Scripts/IndustryUI.cs](../../Assets/Veilbound/Scripts/IndustryUI.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/InventoryArtwork.cs](../../Assets/Veilbound/Scripts/InventoryArtwork.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/InventoryScreen.cs](../../Assets/Veilbound/Scripts/InventoryScreen.cs) | `Settlement` (partial class), `VeilGame` (partial class), `InventoryControl` (struct) |
| [Assets/Veilbound/Scripts/LandscapeDressing.cs](../../Assets/Veilbound/Scripts/LandscapeDressing.cs) | `LandscapeDressing` (class) |
| [Assets/Veilbound/Scripts/LandscapeWeathering.cs](../../Assets/Veilbound/Scripts/LandscapeWeathering.cs) | `LandscapeWeathering` (class) |
| [Assets/Veilbound/Scripts/LegacyExpeditionSetup.cs](../../Assets/Veilbound/Scripts/LegacyExpeditionSetup.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/LibraryArt.cs](../../Assets/Veilbound/Scripts/LibraryArt.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/LibraryResearch.cs](../../Assets/Veilbound/Scripts/LibraryResearch.cs) | `LibraryResearch` (class), `Settlement` (partial class) |
| [Assets/Veilbound/Scripts/LibraryTreeUI.cs](../../Assets/Veilbound/Scripts/LibraryTreeUI.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/LibraryUI.cs](../../Assets/Veilbound/Scripts/LibraryUI.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/LightGear.cs](../../Assets/Veilbound/Scripts/LightGear.cs) | `LightGear` (class), `Settlement` (partial class) |
| [Assets/Veilbound/Scripts/LightGearUI.cs](../../Assets/Veilbound/Scripts/LightGearUI.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/LoadoutInventory.cs](../../Assets/Veilbound/Scripts/LoadoutInventory.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/MaterialProcessing.cs](../../Assets/Veilbound/Scripts/MaterialProcessing.cs) | `MaterialProcessOrder` (class), `MaterialProcessSlot` (class), `Settlement` (partial class) |
| [Assets/Veilbound/Scripts/MaterialProcessingUI.cs](../../Assets/Veilbound/Scripts/MaterialProcessingUI.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/MenuNavigationFocus.cs](../../Assets/Veilbound/Scripts/MenuNavigationFocus.cs) | `VeilGame` (partial class), `MenuFocusControl` (struct) |
| [Assets/Veilbound/Scripts/MetalAppearance.cs](../../Assets/Veilbound/Scripts/MetalAppearance.cs) | `MetalAppearance` (class) |
| [Assets/Veilbound/Scripts/MetalProgression.cs](../../Assets/Veilbound/Scripts/MetalProgression.cs) | `MetalDefinition` (class), `Metals` (class), `ResourceCost` (struct), `Settlement` (partial class) |
| [Assets/Veilbound/Scripts/MineDemandUI.cs](../../Assets/Veilbound/Scripts/MineDemandUI.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/MineUI.cs](../../Assets/Veilbound/Scripts/MineUI.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/MineWorkOrders.cs](../../Assets/Veilbound/Scripts/MineWorkOrders.cs) | `MineWorkOrder` (class), `Settlement` (partial class), `ProductionTime` (class) |
| [Assets/Veilbound/Scripts/MonsterVisual.cs](../../Assets/Veilbound/Scripts/MonsterVisual.cs) | `MonsterVisual` (class) |
| [Assets/Veilbound/Scripts/MysticEnchanting.cs](../../Assets/Veilbound/Scripts/MysticEnchanting.cs) | `MysticRules` (class), `Settlement` (partial class) |
| [Assets/Veilbound/Scripts/MysticUI.cs](../../Assets/Veilbound/Scripts/MysticUI.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/PassiveRules.cs](../../Assets/Veilbound/Scripts/PassiveRules.cs) | `PassiveRules` (class), `RunBuild` (partial class) |
| [Assets/Veilbound/Scripts/PlayerAbilityPresentation.cs](../../Assets/Veilbound/Scripts/PlayerAbilityPresentation.cs) | `VeilGame` (partial class), `PlayerFx` (class) |
| [Assets/Veilbound/Scripts/ProcessingLine.cs](../../Assets/Veilbound/Scripts/ProcessingLine.cs) | `ProcessingSlotAccess` (class), `BarSlotAccess` (class), `MaterialSlotAccess` (class), `ProcessingOrderAccess` (class), `ProcessingSlotStatus` (struct), `ProcessingLine` (class) |
| [Assets/Veilbound/Scripts/ProducerProductionUI.cs](../../Assets/Veilbound/Scripts/ProducerProductionUI.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/ProducerScreen.cs](../../Assets/Veilbound/Scripts/ProducerScreen.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/ProductionBuildings.cs](../../Assets/Veilbound/Scripts/ProductionBuildings.cs) | `WorkshopPageKind` (enum), `EquipmentRecipeKind` (enum), `WorkshopPageDefinition` (class), `ProductionBuildingDefinition` (class), `GatheringBuildingDefinition` (class), `CraftingBuildingDefinition` (class), `ProductionBuildingCatalog` (class), `ProductionBuilding` (class), `GatheringBuilding` (class), `CraftingBuilding` (class), `Settlement` (partial class) |
| [Assets/Veilbound/Scripts/ProductionEnemyMotion.cs](../../Assets/Veilbound/Scripts/ProductionEnemyMotion.cs) | `ProductionEnemyMotion` (class), `ProductionEnemySelection` (class) |
| [Assets/Veilbound/Scripts/ProductionHeroVisual.cs](../../Assets/Veilbound/Scripts/ProductionHeroVisual.cs) | `ProductionHeroVisual` (class) |
| [Assets/Veilbound/Scripts/ProductionModelCatalog.cs](../../Assets/Veilbound/Scripts/ProductionModelCatalog.cs) | `ProductionPlayerEntry` (class), `ProductionEnemyEntry` (class), `ProductionModelCatalog` (class), `Settlement` (partial class) |
| [Assets/Veilbound/Scripts/ProductionSlots.cs](../../Assets/Veilbound/Scripts/ProductionSlots.cs) | `ProducerSlot` (class), `Settlement` (partial class) |
| [Assets/Veilbound/Scripts/ProductionSlotUI.cs](../../Assets/Veilbound/Scripts/ProductionSlotUI.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/ProductionStatus.cs](../../Assets/Veilbound/Scripts/ProductionStatus.cs) | `ProductionActivity` (struct), `Settlement` (partial class) |
| [Assets/Veilbound/Scripts/ProductionStatusUI.cs](../../Assets/Veilbound/Scripts/ProductionStatusUI.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/Progression.cs](../../Assets/Veilbound/Scripts/Progression.cs) | `Settlement` (partial class), `Choice` (class), `RunBuild` (partial class) |
| [Assets/Veilbound/Scripts/QueueDirectPicker.cs](../../Assets/Veilbound/Scripts/QueueDirectPicker.cs) | `VeilGame` (partial class), `QueuePick` (struct) |
| [Assets/Veilbound/Scripts/RageAbilityKit.cs](../../Assets/Veilbound/Scripts/RageAbilityKit.cs) | `VeilGame` (partial class), `ExtraSwing` (class) |
| [Assets/Veilbound/Scripts/RageHud.cs](../../Assets/Veilbound/Scripts/RageHud.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/RageKitHud.cs](../../Assets/Veilbound/Scripts/RageKitHud.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/RageVirtue.cs](../../Assets/Veilbound/Scripts/RageVirtue.cs) | `Virtue` (enum), `Settlement` (partial class), `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/RefiningSlots.cs](../../Assets/Veilbound/Scripts/RefiningSlots.cs) | `RefiningSlot` (class), `Settlement` (partial class) |
| [Assets/Veilbound/Scripts/RefiningSlotUI.cs](../../Assets/Veilbound/Scripts/RefiningSlotUI.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/RelicPlacement.cs](../../Assets/Veilbound/Scripts/RelicPlacement.cs) | `RelicPlacement` (class) |
| [Assets/Veilbound/Scripts/RelicPointers.cs](../../Assets/Veilbound/Scripts/RelicPointers.cs) | `VeilGame` (partial class), `RelicPointer` (struct) |
| [Assets/Veilbound/Scripts/RepeatScheduleUI.cs](../../Assets/Veilbound/Scripts/RepeatScheduleUI.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/ResourceControllerNavigation.cs](../../Assets/Veilbound/Scripts/ResourceControllerNavigation.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/ResourcePriorityRules.cs](../../Assets/Veilbound/Scripts/ResourcePriorityRules.cs) | `Settlement` (partial class) |
| [Assets/Veilbound/Scripts/ResourceTopBar.cs](../../Assets/Veilbound/Scripts/ResourceTopBar.cs) | `VeilGame` (partial class), `ResourceDisplay` (struct) |
| [Assets/Veilbound/Scripts/RoundedArt.cs](../../Assets/Veilbound/Scripts/RoundedArt.cs) | `RoundedArt` (class) |
| [Assets/Veilbound/Scripts/RuinedTownArena.cs](../../Assets/Veilbound/Scripts/RuinedTownArena.cs) | `RuinedTownArena` (class), `Meshes` (class) |
| [Assets/Veilbound/Scripts/RuinMasonry.cs](../../Assets/Veilbound/Scripts/RuinMasonry.cs) | `RuinMasonry` (class) |
| [Assets/Veilbound/Scripts/SanctuaryArena.cs](../../Assets/Veilbound/Scripts/SanctuaryArena.cs) | `SanctuaryArena` (class), `Batch` (class) |
| [Assets/Veilbound/Scripts/SanctuaryCityDetails.cs](../../Assets/Veilbound/Scripts/SanctuaryCityDetails.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/SanctuaryCityUI.cs](../../Assets/Veilbound/Scripts/SanctuaryCityUI.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/SettingsNavigation.cs](../../Assets/Veilbound/Scripts/SettingsNavigation.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/SettlementPlanner.cs](../../Assets/Veilbound/Scripts/SettlementPlanner.cs) | `SettlementJob` (class), `Settlement` (partial class) |
| [Assets/Veilbound/Scripts/SettlementQueueUI.cs](../../Assets/Veilbound/Scripts/SettlementQueueUI.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/SharedCraftingUI.cs](../../Assets/Veilbound/Scripts/SharedCraftingUI.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/SharedGatheringUI.cs](../../Assets/Veilbound/Scripts/SharedGatheringUI.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/SharedWorkshopLayout.cs](../../Assets/Veilbound/Scripts/SharedWorkshopLayout.cs) | `SharedWorkshopLayout` (class) |
| [Assets/Veilbound/Scripts/SmithArt.cs](../../Assets/Veilbound/Scripts/SmithArt.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/SmithBarsUI.cs](../../Assets/Veilbound/Scripts/SmithBarsUI.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/SmithUI.cs](../../Assets/Veilbound/Scripts/SmithUI.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/SmithWorkOrders.cs](../../Assets/Veilbound/Scripts/SmithWorkOrders.cs) | `SmithWorkOrder` (class), `SmithRefining` (class), `Settlement` (partial class) |
| [Assets/Veilbound/Scripts/SpinningWeapons.cs](../../Assets/Veilbound/Scripts/SpinningWeapons.cs) | `VeilGame` (partial class), `WeaponSweep` (class) |
| [Assets/Veilbound/Scripts/StarterEquipment.cs](../../Assets/Veilbound/Scripts/StarterEquipment.cs) | `Settlement` (partial class), `StarterEquipment` (class) |
| [Assets/Veilbound/Scripts/TownApprovedLayout.cs](../../Assets/Veilbound/Scripts/TownApprovedLayout.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/TownHousingReference.cs](../../Assets/Veilbound/Scripts/TownHousingReference.cs) | `VeilGame` (partial class), `HousingRefPlot` (struct) |
| [Assets/Veilbound/Scripts/TownInterior.cs](../../Assets/Veilbound/Scripts/TownInterior.cs) | `TownInterior` (class) |
| [Assets/Veilbound/Scripts/TownLayerBounds.cs](../../Assets/Veilbound/Scripts/TownLayerBounds.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/TownLayeredMap.cs](../../Assets/Veilbound/Scripts/TownLayeredMap.cs) | `VeilGame` (partial class), `TownLayer` (struct) |
| [Assets/Veilbound/Scripts/TownLayout.cs](../../Assets/Veilbound/Scripts/TownLayout.cs) | `VeilGame` (partial class), `TownSpriteBounds` (struct) |
| [Assets/Veilbound/Scripts/TownProgression.cs](../../Assets/Veilbound/Scripts/TownProgression.cs) | `TownUpgradeCost` (class), `Settlement` (partial class) |
| [Assets/Veilbound/Scripts/TownProgressionArt.cs](../../Assets/Veilbound/Scripts/TownProgressionArt.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/TownRadialCity.cs](../../Assets/Veilbound/Scripts/TownRadialCity.cs) | `VeilGame` (partial class), `RadialPart` (struct), `RadialRoad` (struct) |
| [Assets/Veilbound/Scripts/TownRadialData.cs](../../Assets/Veilbound/Scripts/TownRadialData.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/TownRecoveryBounds.cs](../../Assets/Veilbound/Scripts/TownRecoveryBounds.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/TownServices.cs](../../Assets/Veilbound/Scripts/TownServices.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/UpgradeCards.cs](../../Assets/Veilbound/Scripts/UpgradeCards.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/UpgradeRarity.cs](../../Assets/Veilbound/Scripts/UpgradeRarity.cs) | `UpgradeRarity` (enum), `Rarities` (class) |
| [Assets/Veilbound/Scripts/VeilAmbientEmbers.cs](../../Assets/Veilbound/Scripts/VeilAmbientEmbers.cs) | `VeilAmbientEmbers` (class) |
| [Assets/Veilbound/Scripts/VeilBoundary.cs](../../Assets/Veilbound/Scripts/VeilBoundary.cs) | `VeilBoundary` (class) |
| [Assets/Veilbound/Scripts/VeilFogEffect.cs](../../Assets/Veilbound/Scripts/VeilFogEffect.cs) | `VeilFogEffect` (class) |
| [Assets/Veilbound/Scripts/VeilGame.cs](../../Assets/Veilbound/Scripts/VeilGame.cs) | `VeilGame` (partial class), `ScreenState` (enum), `Foe` (class), `Shard` (class), `Hazard` (class), `Bolt` (class), `Flash` (class) |
| [Assets/Veilbound/Scripts/VillageMeshOwner.cs](../../Assets/Veilbound/Scripts/VillageMeshOwner.cs) | `VillageMeshOwner` (class) |
| [Assets/Veilbound/Scripts/VillageTextureCatalog.cs](../../Assets/Veilbound/Scripts/VillageTextureCatalog.cs) | `VillageTextureCatalog` (class), `Part` (class) |
| [Assets/Veilbound/Scripts/WeaponRules.cs](../../Assets/Veilbound/Scripts/WeaponRules.cs) | `WeaponRules` (class), `Settlement` (partial class) |
| [Assets/Veilbound/Scripts/WorkshopCostIcons.cs](../../Assets/Veilbound/Scripts/WorkshopCostIcons.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/WorldArt.cs](../../Assets/Veilbound/Scripts/WorldArt.cs) | `WorldArt` (class) |

## Runtime smoke and review harness

| Source file | Declared types |
| --- | --- |
| [Assets/Veilbound/Scripts/AbilityFeedbackReview.cs](../../Assets/Veilbound/Scripts/AbilityFeedbackReview.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/AbilityPresentationSmoke.cs](../../Assets/Veilbound/Scripts/AbilityPresentationSmoke.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/AllArmourSmoke.cs](../../Assets/Veilbound/Scripts/AllArmourSmoke.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/AreaWarningSmoke.cs](../../Assets/Veilbound/Scripts/AreaWarningSmoke.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/ArenaExpansionReview.cs](../../Assets/Veilbound/Scripts/ArenaExpansionReview.cs) | `VeilGame` (partial class), `RelicCoverage` (class) |
| [Assets/Veilbound/Scripts/ArmIconsReview.cs](../../Assets/Veilbound/Scripts/ArmIconsReview.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/ArmourFeatureReview.cs](../../Assets/Veilbound/Scripts/ArmourFeatureReview.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/AscensionReview.cs](../../Assets/Veilbound/Scripts/AscensionReview.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/BodyCollisionReview.cs](../../Assets/Veilbound/Scripts/BodyCollisionReview.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/BossScaleReview.cs](../../Assets/Veilbound/Scripts/BossScaleReview.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/BronzeHeroSmoke.cs](../../Assets/Veilbound/Scripts/BronzeHeroSmoke.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/BuildOverviewReview.cs](../../Assets/Veilbound/Scripts/BuildOverviewReview.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/CharacterRosterSmoke.cs](../../Assets/Veilbound/Scripts/CharacterRosterSmoke.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/CharacterSelectReview.cs](../../Assets/Veilbound/Scripts/CharacterSelectReview.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/CharacterValidation.cs](../../Assets/Veilbound/Scripts/CharacterValidation.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/ChoiceGridReview.cs](../../Assets/Veilbound/Scripts/ChoiceGridReview.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/CitySidebarReview.cs](../../Assets/Veilbound/Scripts/CitySidebarReview.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/CleanHudSmoke.cs](../../Assets/Veilbound/Scripts/CleanHudSmoke.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/ClothierWorkshopReview.cs](../../Assets/Veilbound/Scripts/ClothierWorkshopReview.cs) | `VeilGame` (partial class), `NoCraftRandom` (class) |
| [Assets/Veilbound/Scripts/CombatFlowReview.cs](../../Assets/Veilbound/Scripts/CombatFlowReview.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/CombatInterfaceSmoke.cs](../../Assets/Veilbound/Scripts/CombatInterfaceSmoke.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/ControllerFocusReview.cs](../../Assets/Veilbound/Scripts/ControllerFocusReview.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/CountUpgradeReview.cs](../../Assets/Veilbound/Scripts/CountUpgradeReview.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/CrowdMeleeReview.cs](../../Assets/Veilbound/Scripts/CrowdMeleeReview.cs) | `VeilGame` (partial class), `CrowdMeleeReport` (class) |
| [Assets/Veilbound/Scripts/EconomyBalanceSmoke.cs](../../Assets/Veilbound/Scripts/EconomyBalanceSmoke.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/EnemyAttackReview.cs](../../Assets/Veilbound/Scripts/EnemyAttackReview.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/EnemyLocomotionReview.cs](../../Assets/Veilbound/Scripts/EnemyLocomotionReview.cs) | `VeilGame` (partial class), `EnemyMotionAudit` (class) |
| [Assets/Veilbound/Scripts/EnemyPressureReview.cs](../../Assets/Veilbound/Scripts/EnemyPressureReview.cs) | `VeilGame` (partial class), `PressureSample` (class), `PressureReport` (class) |
| [Assets/Veilbound/Scripts/EnemyPursuitReview.cs](../../Assets/Veilbound/Scripts/EnemyPursuitReview.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/EnemyStyleSmoke.cs](../../Assets/Veilbound/Scripts/EnemyStyleSmoke.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/EquipmentPaperDollSmoke.cs](../../Assets/Veilbound/Scripts/EquipmentPaperDollSmoke.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/ExpeditionArtReview.cs](../../Assets/Veilbound/Scripts/ExpeditionArtReview.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/ExpeditionFlowReview.cs](../../Assets/Veilbound/Scripts/ExpeditionFlowReview.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/ExpeditionLoadoutSmoke.cs](../../Assets/Veilbound/Scripts/ExpeditionLoadoutSmoke.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/ExpeditionModifierReview.cs](../../Assets/Veilbound/Scripts/ExpeditionModifierReview.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/GameSettingsSmoke.cs](../../Assets/Veilbound/Scripts/GameSettingsSmoke.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/GearComparisonReview.cs](../../Assets/Veilbound/Scripts/GearComparisonReview.cs) | `VeilGame` (partial class), `ComparisonInventory` (class) |
| [Assets/Veilbound/Scripts/GearCostReview.cs](../../Assets/Veilbound/Scripts/GearCostReview.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/GearRollSmoke.cs](../../Assets/Veilbound/Scripts/GearRollSmoke.cs) | `VeilGame` (partial class), `FixedGearRandom` (class) |
| [Assets/Veilbound/Scripts/GraveyardReview.cs](../../Assets/Veilbound/Scripts/GraveyardReview.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/GreatswordRuntimeReview.cs](../../Assets/Veilbound/Scripts/GreatswordRuntimeReview.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/GroundSlamSmoke.cs](../../Assets/Veilbound/Scripts/GroundSlamSmoke.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/HudStyleSmoke.cs](../../Assets/Veilbound/Scripts/HudStyleSmoke.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/InterfaceCompletionReview.cs](../../Assets/Veilbound/Scripts/InterfaceCompletionReview.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/LibraryReview.cs](../../Assets/Veilbound/Scripts/LibraryReview.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/LightGearReview.cs](../../Assets/Veilbound/Scripts/LightGearReview.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/ManualStockpileReview.cs](../../Assets/Veilbound/Scripts/ManualStockpileReview.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/MaterialProcessingReview.cs](../../Assets/Veilbound/Scripts/MaterialProcessingReview.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/MenuCombatInputSmoke.cs](../../Assets/Veilbound/Scripts/MenuCombatInputSmoke.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/MetalProgressionSmoke.cs](../../Assets/Veilbound/Scripts/MetalProgressionSmoke.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/MineReview.cs](../../Assets/Veilbound/Scripts/MineReview.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/MonsterFamilySmoke.cs](../../Assets/Veilbound/Scripts/MonsterFamilySmoke.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/MusicReview.cs](../../Assets/Veilbound/Scripts/MusicReview.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/MysticReview.cs](../../Assets/Veilbound/Scripts/MysticReview.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/PassiveSummaryReview.cs](../../Assets/Veilbound/Scripts/PassiveSummaryReview.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/PointerScaleReview.cs](../../Assets/Veilbound/Scripts/PointerScaleReview.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/ProducerReview.cs](../../Assets/Veilbound/Scripts/ProducerReview.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/ProductionModelSmoke.cs](../../Assets/Veilbound/Scripts/ProductionModelSmoke.cs) | `VeilGame` (partial class), `ProductionRuntimeAudit` (class) |
| [Assets/Veilbound/Scripts/ProductionQueueReview.cs](../../Assets/Veilbound/Scripts/ProductionQueueReview.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/ProductionSlotReview.cs](../../Assets/Veilbound/Scripts/ProductionSlotReview.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/RageKitSmoke.cs](../../Assets/Veilbound/Scripts/RageKitSmoke.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/RageVirtueSmoke.cs](../../Assets/Veilbound/Scripts/RageVirtueSmoke.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/RandomMapSmoke.cs](../../Assets/Veilbound/Scripts/RandomMapSmoke.cs) | `VeilGame` (partial class), `SequenceMapRandom` (class) |
| [Assets/Veilbound/Scripts/RawGearReview.cs](../../Assets/Veilbound/Scripts/RawGearReview.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/RefiningSlotReview.cs](../../Assets/Veilbound/Scripts/RefiningSlotReview.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/RelicPointerReview.cs](../../Assets/Veilbound/Scripts/RelicPointerReview.cs) | `VeilGame` (partial class), `NonCriticalReviewRandom` (class) |
| [Assets/Veilbound/Scripts/RepeatReview.cs](../../Assets/Veilbound/Scripts/RepeatReview.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/ResourcePriorityReview.cs](../../Assets/Veilbound/Scripts/ResourcePriorityReview.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/ResourceTopBarSmoke.cs](../../Assets/Veilbound/Scripts/ResourceTopBarSmoke.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/RiftBalanceReview.cs](../../Assets/Veilbound/Scripts/RiftBalanceReview.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/RuinedTownSmoke.cs](../../Assets/Veilbound/Scripts/RuinedTownSmoke.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/SanctuaryArenaSmoke.cs](../../Assets/Veilbound/Scripts/SanctuaryArenaSmoke.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/SanctuaryCitySmoke.cs](../../Assets/Veilbound/Scripts/SanctuaryCitySmoke.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/SanctuaryKnightSmoke.cs](../../Assets/Veilbound/Scripts/SanctuaryKnightSmoke.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/ScreenSpawnReview.cs](../../Assets/Veilbound/Scripts/ScreenSpawnReview.cs) | `VeilGame` (partial class), `ZeroSpawnRandom` (class), `ScreenSpawnReport` (class) |
| [Assets/Veilbound/Scripts/SettingsNavigationSmoke.cs](../../Assets/Veilbound/Scripts/SettingsNavigationSmoke.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/SettlementQueueReview.cs](../../Assets/Veilbound/Scripts/SettlementQueueReview.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/SharedProductionReview.cs](../../Assets/Veilbound/Scripts/SharedProductionReview.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/SmithReview.cs](../../Assets/Veilbound/Scripts/SmithReview.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/SmokeTest.cs](../../Assets/Veilbound/Scripts/SmokeTest.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/StarterBuildingReview.cs](../../Assets/Veilbound/Scripts/StarterBuildingReview.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/StarterGearRuntimeCheck.cs](../../Assets/Veilbound/Scripts/StarterGearRuntimeCheck.cs) | `StarterGearRuntimeCheck` (class) |
| [Assets/Veilbound/Scripts/TownDailyRatesReview.cs](../../Assets/Veilbound/Scripts/TownDailyRatesReview.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/TownLayersReview.cs](../../Assets/Veilbound/Scripts/TownLayersReview.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/TownProgressionReview.cs](../../Assets/Veilbound/Scripts/TownProgressionReview.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/UpgradeCardSmoke.cs](../../Assets/Veilbound/Scripts/UpgradeCardSmoke.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/VeilAtmosphereReview.cs](../../Assets/Veilbound/Scripts/VeilAtmosphereReview.cs) | `VeilGame` (partial class), `AtmosphereAudit` (class) |
| [Assets/Veilbound/Scripts/VillageRuntimeReview.cs](../../Assets/Veilbound/Scripts/VillageRuntimeReview.cs) | `VeilGame` (partial class) |
| [Assets/Veilbound/Scripts/WeaponScalingReview.cs](../../Assets/Veilbound/Scripts/WeaponScalingReview.cs) | `VeilGame` (partial class), `WeaponAbilityAudit` (class) |

## Editor tooling

| Source file | Declared types |
| --- | --- |
| [Assets/VeilbornePlayer/Editor/PlayerValidation.cs](../../Assets/VeilbornePlayer/Editor/PlayerValidation.cs) | `PlayerValidation` (class), `ClipCheck` (class), `RendererCheck` (class), `VariantCheck` (class), `Results` (class) |
| [Assets/VeilbornePlayer/Editor/VeilbornePlayerImport.cs](../../Assets/VeilbornePlayer/Editor/VeilbornePlayerImport.cs) | `VeilbornePlayerImport` (class), `VeilborneTextureImport` (class) |
| [Assets/Veilbound/Editor/ArmIconsBuild.cs](../../Assets/Veilbound/Editor/ArmIconsBuild.cs) | `ArmIconsBuild` (class) |
| [Assets/Veilbound/Editor/ArmourFeatureBuild.cs](../../Assets/Veilbound/Editor/ArmourFeatureBuild.cs) | `ArmourFeatureBuild` (class) |
| [Assets/Veilbound/Editor/BodyIntegrationDiagnostic.cs](../../Assets/Veilbound/Editor/BodyIntegrationDiagnostic.cs) | `BodyIntegrationDiagnostic` (class) |
| [Assets/Veilbound/Editor/BossScaleValidation.cs](../../Assets/Veilbound/Editor/BossScaleValidation.cs) | `BossScaleValidation` (class), `Row` (class), `Audit` (class) |
| [Assets/Veilbound/Editor/BronzeKnightImport.cs](../../Assets/Veilbound/Editor/BronzeKnightImport.cs) | `BronzeKnightImport` (class), `Asset` (class), `Surface` (class), `Indices` (class), `Part` (class) |
| [Assets/Veilbound/Editor/CharacterSelectBuild.cs](../../Assets/Veilbound/Editor/CharacterSelectBuild.cs) | `CharacterSelectBuild` (class) |
| [Assets/Veilbound/Editor/CombatFlowBuild.cs](../../Assets/Veilbound/Editor/CombatFlowBuild.cs) | `CombatFlowBuild` (class) |
| [Assets/Veilbound/Editor/CultistModelSetup.cs](../../Assets/Veilbound/Editor/CultistModelSetup.cs) | `CultistModelSetup` (class) |
| [Assets/Veilbound/Editor/EnemyAttackSetup.cs](../../Assets/Veilbound/Editor/EnemyAttackSetup.cs) | `EnemyAttackSetup` (class) |
| [Assets/Veilbound/Editor/EnemyLocomotionSetup.cs](../../Assets/Veilbound/Editor/EnemyLocomotionSetup.cs) | `EnemyLocomotionSetup` (class), `SourceFrame` (class), `SourceClip` (class), `SourceLibrary` (class), `Audit` (class) |
| [Assets/Veilbound/Editor/EnemyPressureBuild.cs](../../Assets/Veilbound/Editor/EnemyPressureBuild.cs) | `EnemyPressureBuild` (class) |
| [Assets/Veilbound/Editor/ExpeditionAnimationDiagnostic.cs](../../Assets/Veilbound/Editor/ExpeditionAnimationDiagnostic.cs) | `ExpeditionAnimationDiagnostic` (class) |
| [Assets/Veilbound/Editor/ExpeditionFlowValidation.cs](../../Assets/Veilbound/Editor/ExpeditionFlowValidation.cs) | `ExpeditionFlowValidation` (class) |
| [Assets/Veilbound/Editor/ExpeditionModifierBuild.cs](../../Assets/Veilbound/Editor/ExpeditionModifierBuild.cs) | `ExpeditionModifierBuild` (class) |
| [Assets/Veilbound/Editor/GraveyardValidation.cs](../../Assets/Veilbound/Editor/GraveyardValidation.cs) | `GraveyardValidation` (class), `Result` (class) |
| [Assets/Veilbound/Editor/GreatswordAnimationSetup.cs](../../Assets/Veilbound/Editor/GreatswordAnimationSetup.cs) | `GreatswordAnimationSetup` (class), `Audit` (class) |
| [Assets/Veilbound/Editor/GreatswordImport.cs](../../Assets/Veilbound/Editor/GreatswordImport.cs) | `GreatswordImport` (class), `SwordCheck` (class), `Validation` (class) |
| [Assets/Veilbound/Editor/HousingReferenceImport.cs](../../Assets/Veilbound/Editor/HousingReferenceImport.cs) | `HousingReferenceImport` (class) |
| [Assets/Veilbound/Editor/LandscapePolishValidation.cs](../../Assets/Veilbound/Editor/LandscapePolishValidation.cs) | `LandscapePolishValidation` (class) |
| [Assets/Veilbound/Editor/LibraryValidation.cs](../../Assets/Veilbound/Editor/LibraryValidation.cs) | `LibraryValidation` (class) |
| [Assets/Veilbound/Editor/MineValidation.cs](../../Assets/Veilbound/Editor/MineValidation.cs) | `MineValidation` (class) |
| [Assets/Veilbound/Editor/MonsterFamilyImport.cs](../../Assets/Veilbound/Editor/MonsterFamilyImport.cs) | `MonsterFamilyImport` (class), `Family` (class), `Surface` (class), `Monster` (class), `Part` (class), `Indices` (class) |
| [Assets/Veilbound/Editor/MusicImport.cs](../../Assets/Veilbound/Editor/MusicImport.cs) | `MusicImport` (class) |
| [Assets/Veilbound/Editor/PointerScaleBuild.cs](../../Assets/Veilbound/Editor/PointerScaleBuild.cs) | `PointerScaleBuild` (class) |
| [Assets/Veilbound/Editor/ProducerArtImport.cs](../../Assets/Veilbound/Editor/ProducerArtImport.cs) | `ProducerArtImport` (class) |
| [Assets/Veilbound/Editor/ProductionModelImport.cs](../../Assets/Veilbound/Editor/ProductionModelImport.cs) | `ProductionModelImport` (class), `Check` (class), `Audit` (class) |
| [Assets/Veilbound/Editor/PrototypeBuild.cs](../../Assets/Veilbound/Editor/PrototypeBuild.cs) | `PrototypeBuild` (class) |
| [Assets/Veilbound/Editor/RawGearBuild.cs](../../Assets/Veilbound/Editor/RawGearBuild.cs) | `RawGearBuild` (class) |
| [Assets/Veilbound/Editor/RepeatValidation.cs](../../Assets/Veilbound/Editor/RepeatValidation.cs) | `RepeatValidation` (class) |
| [Assets/Veilbound/Editor/SanctuaryCityImport.cs](../../Assets/Veilbound/Editor/SanctuaryCityImport.cs) | `SanctuaryCityImport` (class) |
| [Assets/Veilbound/Editor/SmithValidation.cs](../../Assets/Veilbound/Editor/SmithValidation.cs) | `SmithValidation` (class) |
| [Assets/Veilbound/Editor/StarterGearImport.cs](../../Assets/Veilbound/Editor/StarterGearImport.cs) | `StarterGearImport` (class), `Result` (class) |
| [Assets/Veilbound/Editor/TownLayerImport.cs](../../Assets/Veilbound/Editor/TownLayerImport.cs) | `TownLayerImport` (class) |
| [Assets/Veilbound/Editor/TownProgressionSetup.cs](../../Assets/Veilbound/Editor/TownProgressionSetup.cs) | `TownProgressionSetup` (class) |
| [Assets/Veilbound/Editor/TownRadialImport.cs](../../Assets/Veilbound/Editor/TownRadialImport.cs) | `TownRadialImport` (class) |
| [Assets/Veilbound/Editor/TownRecoveryImport.cs](../../Assets/Veilbound/Editor/TownRecoveryImport.cs) | `TownRecoveryImport` (class) |
| [Assets/Veilbound/Editor/UpgradeCardImport.cs](../../Assets/Veilbound/Editor/UpgradeCardImport.cs) | `UpgradeCardImport` (class) |
| [Assets/Veilbound/Editor/UpgradeValidation.cs](../../Assets/Veilbound/Editor/UpgradeValidation.cs) | `UpgradeValidation` (class) |
| [Assets/Veilbound/Editor/VeilAtmosphereSetup.cs](../../Assets/Veilbound/Editor/VeilAtmosphereSetup.cs) | `VeilAtmosphereSetup` (class), `Result` (class) |
| [Assets/Veilbound/Editor/VillageTextureBuild.cs](../../Assets/Veilbound/Editor/VillageTextureBuild.cs) | `VillageTextureBuild` (class), `Mapping` (class), `Part` (class) |
| [Assets/Veilbound/Editor/VillageTripoExport.cs](../../Assets/Veilbound/Editor/VillageTripoExport.cs) | `VillageTripoExport` (class), `Part` (class), `Manifest` (class) |
| [Assets/Veilbound/Editor/VillageValidation.cs](../../Assets/Veilbound/Editor/VillageValidation.cs) | `VillageValidation` (class), `Result` (class) |
| [Assets/Veilbound/Editor/WeaponFeatureBuild.cs](../../Assets/Veilbound/Editor/WeaponFeatureBuild.cs) | `WeaponFeatureBuild` (class) |
| [Assets/Veilbound/Editor/WeaponScalingSetup.cs](../../Assets/Veilbound/Editor/WeaponScalingSetup.cs) | `WeaponScalingSetup` (class), `Row` (class), `Result` (class) |
| [Assets/Veilbound/Editor/WeightBalanceBuild.cs](../../Assets/Veilbound/Editor/WeightBalanceBuild.cs) | `WeightBalanceBuild` (class) |
| [Assets/VeilboundPlateMasters/Editor/BuildPlateMasters.cs](../../Assets/VeilboundPlateMasters/Editor/BuildPlateMasters.cs) | `BuildPlateMasters` (class), `VariantCheck` (class), `MotionCheck` (class), `Results` (class) |

## Model and presentation support

| Source file | Declared types |
| --- | --- |
| [Assets/VeilbornePlayer/Runtime/VeilborneAnimationDriver.cs](../../Assets/VeilbornePlayer/Runtime/VeilborneAnimationDriver.cs) | `VeilborneAnimationDriver` (class) |
| [Assets/VeilboundGreatswords/Runtime/GreatswordVariant.cs](../../Assets/VeilboundGreatswords/Runtime/GreatswordVariant.cs) | `GreatswordVariant` (class) |
| [Assets/VeilboundGreatswords/Runtime/GreatswordVisualLibrary.cs](../../Assets/VeilboundGreatswords/Runtime/GreatswordVisualLibrary.cs) | `GreatswordEntry` (class), `GreatswordVisualLibrary` (class) |
| [Assets/VeilboundPlateMasters/Runtime/MasterPlateEquipment.cs](../../Assets/VeilboundPlateMasters/Runtime/MasterPlateEquipment.cs) | `PlateTierBinding` (class), `MasterPlateEquipment` (class) |
