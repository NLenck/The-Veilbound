# Reusable plate armour masters

## Completed scope

The approved Tin/Copper generation now supplies **two reusable mesh families**, fitted separately to the male/female Veilborne review rigs. All ten tiers are available as temporary material variants, with **20 Unity prefabs**. This follows the request to reuse the designs and conserve credits; unique high-tier silhouettes and crystal growth geometry are deferred. Tungsten shares the Tin-family shape, Titanium shares Copper, and Veilcrystal uses blood-red metal with black cape and black belt cloth rather than newly generated crystals.

**Additional Tripo spend: 0.** No generation, texturing, rigging, segmentation or conversion API was submitted. The existing prototype cost remains 40. Final verified Developer balance is **260 available / 0 frozen**.

## Geometry and materials

| Tier | Shared mesh family | Armour triangles | Game item ID |
|---|---|---:|---:|
| Tin | Tin | 7,624 | 100 |
| Copper | Copper | 7,492 | 101 |
| Bronze | Copper | 7,492 | 102 |
| Brass | Copper | 7,492 | 103 |
| Iron | Tin | 7,624 | 104 |
| Steel | Tin | 7,624 | 105 |
| Hardened Steel | Tin | 7,624 | 106 |
| Tungsten | Tin | 7,624 | 108 |
| Titanium | Copper | 7,492 | 107 |
| Veilcrystal | Copper | 7,492 | 109 |

Counts exclude the underlying player body and are the same for both genders. Source authoring preserves **20 objects per family/gender**: the original fourteen independent components plus left/right thigh plates, upper-arm plates and elbow couters. The thigh plates retain their lower silhouette and taper into narrower upper cuffs beneath the original waist and tasset details. These form nine equipment slots. The paired pieces and thigh/shin pieces combine only within their runtime slot, producing **nine armour renderers**, each with one shared tier material. The runtime library contains **36 shared slot meshes total** (two families × two genders × nine slots), not twenty duplicate complete-suit mesh libraries. Twenty prefabs reference those shared assets. Ten tier materials/colour atlases are shared across genders and slots; one metallic/smoothness atlas is shared globally. Buckle accents distinguish the palettes, including Titanium gold and Veilcrystal crimson.

The Tin cape was conservatively reduced from 1,772 to 1,062 triangles, with maximum source-surface deviation 0.00536 in the original normalised scene. The Copper cape remains 462 triangles from the earlier accepted reduction. No additional subdivisions or API retopology were used. Capes are weighted to spine/hips, with additional rear clearance; there is no runtime cloth simulation.

## Pauldron size revision

The tall pauldron profile is preserved, with a small width increase from 0.225 to 0.245 m (about 9%) and a 10 mm outward adjustment on the male fit, proportionally scaled for female. Height remains 0.36 m and depth 0.39 m. The overall shoulder span grows only about 6% from the narrow revision. All eight parts pass measured dimensions.

The arm chain passed 80 coverage/proximity checks across rest and locomotion poses, with actual cuff/couter mesh overlap in all 216 sampled linkage observations; all sixteen new upper-arm/elbow meshes have closed surfaces and outward normals. The arm chain was also refitted: source bracer/gauntlet lean is removed before fitting, the forearm frame comes from the elbow/wrist joints, and palm roll comes from the actual index/pinky/middle finger joint positions. Bracers now cover the full elbow-to-wrist length with deliberate cuff overlap. Gauntlets are anchored to the wrist and shaped in the palm frame. Slim rerebraces connect the biceps under the pauldrons, and weighted elbow couters bridge the hinge. Upper-arm plates belong to Shoulders; elbow couters belong to Bracers, preserving all nine independent equipment slots. `qa/arm_connection_verification.json` records extension coverage and near-surface joint distances in nine locomotion phases per family/gender. These checks address visible joins, not watertight union of independently equippable parts.

## Fitting and visibility fixes

- Enlarged/repositioned cuirasses and tassets for the actual body regions instead of pushing isolated vertices into an irregular shell.
- Fitted gauntlets from complete weighted hand/finger bounds, and boots from complete foot/toe bounds. The short hand bone alone was insufficient in the prototype.
- Rebalanced female gauntlets to the male hand-to-forearm proportions, increased forearm thickness to the male proportions scaled by character height, and increased upper-arm cross sections by 20%. Wrist/elbow positions, cuff lengths, skin weights and the male master are preserved.
- Coloured the recessed helmet eye/visor slots and recessed central face channel black in both mesh families and both genders using atlas tile 14, preserving one shared material per tier.
- Restored the thigh plates and tapered their upper cuffs by up to 34%, extending the hidden cuff 18 mm upward beneath the belt/tassets. Original knee/shin armour and belt/tasset geometry are preserved.
- Added reversible body occlusion masks, authored per triangle in FBX UV2. Covered head/hair, torso, limb and foot faces are removed from the rendered body when the matching slot is equipped. The original full source body remains available and restores on unequip. The Legs mask hides covered trousers beneath the thigh plates, and the Belt mask hides loose waist cloth beneath the tassets. A reversible `BeltTuck` blendshape draws loose waist clothing under the belt when equipped, restoring its original shape when the belt is removed.
- Preserved actual skinning to the existing 52-bone rigs. Chest, waist and legs carry suitable multi-bone weights; they are not parented wholesale to one joint. Fitted source pivots are at the attachment joints.

The Blender masters show the full-set masked preview. A hidden `Unmasked_Body_Reference` object preserves the complete source surface. Runtime restoration is handled by `MasterPlateEquipment`; hiding an armour object manually in Blender does not automatically run the Unity mask logic. The original PlayerAssetProduction masters and generated prototype files were preserved.

## Validation

All **twenty tier GLBs** passed embedded cape/belt/visor palette, twenty-component geometry, expected triangle count, one-material and 52-bone checks.

All **46 FBX/GLB master exports** round-tripped with expected triangle counts, 52 bones, finite coordinates, UVs and complete skin weights. Body-source exports preserve the active per-slot mask codes through FBX vertex splitting; the reserved all-required channel remains available in the runtime but is unused in this revision.

The leg-clearance audit passed 76 checks: four unchanged belt/tasset geometry comparisons and 72 waist-versus-knee/shin intersection checks across Idle, Walk and Run. Four restored thigh plates are present, with narrowed upper cuffs. The approved eight pauldron dimensions and 80 arm-connection checks still pass. Unity also verifies that `BeltTuck` activates with the belt and returns to zero when the belt is removed on both characters and all ten palettes.

The isolated Unity **6000.3.9f1** project compiled the runtime/editor code and passed tests for all twenty prefab configurations: nine independent slot toggles, restoration of the complete body when all armour is removed, mixed-tier changes without changing other slots, shared geometry and readable UV2 source data for runtime masking. The full-body source triangle counts restore exactly: 30,352 male and 27,084 female. With all armour equipped, only the uncovered body faces are rendered. Runtime mask work runs on equipment changes, not every frame; it does not alter the original source mesh asset.

All four family/gender combinations passed finite baked-skinning bounds and actual motion tests in Idle, Walk and Run. Eight motion GIFs are included, along with twenty colour previews and male/female ten-tier contact sheets. The before/after sheet records the removal of the major prototype clipping. These checks cover the supplied locomotion, not every possible attack, dodge or extreme pose. Greatsword grips, combat-specific intersections and final in-game lighting still require a gameplay fitting pass. Ordinary shoulder/torso plate overlaps are intentional lapped armour; this is not a certification of collision-free physical cloth/metal simulation.

## Cape and belt cloth palettes

All ten tiers have independent cape and belt-cloth colours, using atlas tile 10 for capes and tile 15 for waist cloth. Both are non-metallic. The visor remains black in tile 14. No extra material slots or renderers were added. `tier_cloth_palettes.json` is the editable palette source; `tier_mapping.json` and `MASTER_MANIFEST.csv` record the chosen colours. Re-run `package_masters.py` and the render/build steps after palette edits.

| Tier | Cape | Belt cloth |
|---|---|---|
| Tin | #48505B | #6C727A |
| Copper | #244D4A | #3C6A60 |
| Bronze | #61362C | #89543A |
| Brass | #4D305F | #79547D |
| Iron | #344338 | #66704D |
| Steel | #263C64 | #526C8A |
| HardenedSteel | #512330 | #813B46 |
| Tungsten | #29282E | #5A5358 |
| Titanium | #C9C4AE | #344D88 |
| Veilcrystal | #1B171D | #1B171D |

`exports/tiers/Male/` and `exports/tiers/Female/` contain twenty GLBs with the selected metal, cape and belt-cloth appearance embedded. Each preserves the twenty authoring components, rig and one shared material. The Unity kit uses the shared family mesh library and twenty palette-specific prefabs. Front/back ten-tier sheets are under `previews/`.

## Files and use

- `Veilbound_Plate_Masters.unitypackage`: tested staged kit, twenty prefabs, shared meshes/materials, body-mask runtime, source FBXs and required player-review dependencies.
- `unity_staging/VeilboundPlateMasters/Generated/*_Master.prefab`: loose equivalents. Use the package or the loose kit with its player dependencies.
- `blender/Master_Male.blend` / `Master_Female.blend`: two source families and the ten-material palette library.
- `exports/fitted/`: four full-family FBX+GLB exports, thirty-six per-slot FBXs, and two body-mask source FBXs.
- `MASTER_MANIFEST.csv`, `tier_mapping.json`, `qa/master_export_verification.json`, `UnityValidation/PLATE_MASTER_VALIDATION.json`, and `CREDIT_LEDGER.json`: counts, mappings, tests and spend.
- `previews/ALL_TEN_TIERS_MALE.jpg`, female equivalent, `FITTING_BEFORE_AFTER.jpg`, and the family/gender Walk/Run GIFs: visual review.

## Runtime API and integration boundary

`MasterPlateEquipment.SetSlotTier(slot, itemId)` changes that one slot's mesh family/material and enables it. `SetSlotEnabled(slot, false)` removes it and restores body visibility. `EquipWholeTier(itemId)` applies a complete tier. Slots match the existing game order: Head 0, Shoulders 1, Chest 2, Hands 3, Belt 4, Bracers 5, Legs 6, Feet 7, Cape 8. Item IDs preserve the game's Titanium/Tungsten ordering: Titanium 107, Tungsten 108.

This kit targets the new Veilborne **52-bone review rigs**. The live game's existing procedural 17-bone hero is not interchangeable with it. No live Assets, scenes, saves, crafting rules or current character were replaced. Existing cloth-only cape crafting remains unchanged; the kit supplies coloured cape appearances for later game integration. No purchase or paid bulk generation is queued.

## Repeat

Run `master_fit.py -- Male/Female` in Blender to rebuild from preserved approved geometry, `package_masters.py` with Python/Pillow to prepare materials and the isolated Unity project, and `verify_master_exports.py` in Blender. Run `BuildPlateMasters.Run` in the isolated Mastered/UnityValidation project with its disposable Generated folder absent. `BuildPlateMasters.Export` packages a passed build. `render_masters.py -- Male/Female` in Blender prepares the palette library and review frames; `finalise_masters.py` composes sheets/GIFs and writes this report. All steps are local and make no paid Tripo calls.
