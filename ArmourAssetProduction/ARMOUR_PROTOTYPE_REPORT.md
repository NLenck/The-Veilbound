# Plate armour prototype report

## Decision

**Grouped parts extraction works for this two-set Tin/Copper prototype. Final character fitting does not yet pass visual acceptance. Stop paid generation here.** The prototype does not demonstrate that a five-set composite will work, and none of the remaining eight tiers has been generated.

The official CLI 0.7.2 authenticated to the existing Developer account. Actual balance was 300, not a newly funded 1,500. One multiview task `b634e884-59b5-4a00-a755-5a5c8a7f74ef` succeeded for **40 credits**, leaving **260 available / 0 frozen**. No Studio credits, purchases, paid retries, API texturing, segmentation, rigging or conversion were used.

## What was produced and checked

The two-tier input produced exactly **28 meshes / 28 welded connected components / 15,104 raw triangles**, with no extra body/mannequin component. Fourteen objects belong to each tier: helmet, two pauldrons, chest, two bracers, two gauntlets, belt/tassets, two knee/shin guards, two boots and cape. They were classified by the measured scene regions and height bands, checked against front/back inspection, then named and placed in `Plate_Tin` and `Plate_Copper` collections. The paired leg objects share the Legs slot. No fused geometry was cut apart or silently relabelled.

The original grouped GLB remains unchanged under `raw/`. `blender/Prototype_Organised_Exploded.blend` preserves both organised sets. `exports/exploded/` contains individual named GLBs and an assembled exploded GLB per tier. `exports/fitted/` contains male/female full-slot FBX+GLB drafts and nine slot FBXs per tier/variant. Fitted pivots use the relevant rig joints rather than the arbitrary exploded centres.

| Tier | Variant | Raw tris | Export tris | Separable slots | Worst sampled body penetration |
|---|---|---:|---:|---:|---:|
| Tin | Male | 7,432 | 7,430 | 9/9 | 16.3% |
| Tin | Female | 7,432 | 7,430 | 9/9 | 12.7% |
| Copper | Male | 7,672 | 6,588 | 9/9 | 20.0% |
| Copper | Female | 7,672 | 6,588 | 9/9 | 15.0% |

Triangle counts exclude the player body. Male/female copies use the same generated tier geometry; fitting/weights differ locally without duplicate paid generations. A shared 16-swatch atlas supplies metal, cloth and accent colours. All armour slots use one shared Unity material. Runtime paired parts are combined within their equipment slot, reducing fourteen source renderers to nine independently toggled slot renderers. This is nine potential armour draws, not one draw. The body adds its own renderers/materials. The source left/right objects remain independent and reversible.

Welding removed six negligible/duplicate source triangles before optimisation; the unmodified raw count remains 15,104. The Copper cape reduced to 462 triangles with a measured maximum source-space surface deviation of about 0.00587. A comparable Tin reduction exceeded the 0.008 source-unit acceptance limit and was reverted; its cape remains 1,772 triangles. No aggressive global decimation or subdivisions were applied. Flat shading retains faceting, although the generated metal silhouettes remain rounded and less crisp than the concept art.

All **74 exported GLB/FBX files** passed Blender round trips: expected triangles, UV presence, finite vertices, expected rig count and skin-weight coverage. The four fitted combinations carry the existing 52-bone skeleton with no unweighted vertices or weight-sum errors. The isolated Unity **6000.3.9f1** project compiled the kit, built four draft prefabs and verified nine independent toggles, triangle counts, finite baked skinning and actual motion in the existing Idle/Walk/Run clips. These are format/skinning tests, **not** clipping or art-quality approval.

## Fit and coverage failures

`previews/PROTOTYPE_FITTING_ISSUES.jpg` records the unresolved fit. Hair and the head protrude through helmets; base clothing intersects chest/belt plates; hands and boots show gaps. Nearest-surface weight transfer and a limited outward push were insufficient. The penetration figures above are sampled nearest-surface signed tests on the unmasked body, useful as warnings rather than exact collision volumes or image percentages. Nine animation phases were sampled per tier/variant. Full-body occlusion masks, retopology around joints, size corrections and hand/boot fitting are still required. Chest and legs are skinned, not parented wholesale to one bone. Cape weights use spine/hips without cloth simulation and do not supply a finished cape animation.

The reference and reconstruction omit substantial upper-thigh plates. Therefore **9/9 separate slot containers does not mean complete anatomical armour coverage**. Tin/Copper silhouettes differ only modestly; stronger tier-specific structure is needed before broad production. No slot should be promoted into the live game on the basis of these technical checks alone.

## Existing Unity integration findings

`Assets/Veilbound/Scripts/Equipment.cs` already defines nine slots in the order head, shoulders, chest, hands, belt, bracers, legs, feet, cape. `ArmourVisuals.cs` attaches procedural armour to the older 17-bone `HeroBodyMesh` rig, with shoulders using `HeroGearAnchor` chest/arm interpolation. The new Veilborne review bodies use a separate 52-bone rig. The prototype binds to those review rigs and does not replace the old character or its anchors.

`MetalProgression.cs` already defines ten materials. Its Titanium/Tungsten runtime order is opposite the requested visual list: Tungsten is item **108**, Titanium **107**. This explicit mapping is recorded in `configs/armour_pipeline.json`. The current crafting path treats capes as cloth-only; a tier cape needs a deliberate game-data decision rather than silently changing that rule. The staging runtime `ModularPlateSlots` exposes numeric slot toggling and does not alter settlement saves or live crafting.

## Reference preparation

Both supplied concept JPGs were copied into `references/original/`; originals were not edited. The built-in image generator used Group A as the design reference to create a two-row exploded sheet. Front/back rows were cropped non-destructively; the complete back scene was mirrored to reverse world order and cape side, so multiview inputs describe the same spatial arrangement. Group A's five-set front/back images and the cropped two-set prototype inputs are saved with hashes in `references/prepared/`. They are **draft references**, with the thigh omission and approximate front/back consistency noted above. Group B's original concept and a disabled request proposal are saved; its exploded inputs are deferred while lower-tier fitting fails acceptance. In that future reference, Tungsten must have substantially wider/thicker shoulders and torso than Titanium, not simply a darker finish.

## Current official API findings

- H3.1 `v3.1-20260211` supports multiview and `generate_parts`. Parts requires `texture=false`, `pbr=false`; smart low-poly suppresses parts, so it was deliberately omitted.
- P1 is the clean low-poly option up to 20,000 triangles but does not support parts generation. P2 Preview extends budget/quad support but also lacks parts; its listed 100-credit base is poor value for this limited prototype budget.
- Multiview documentation asks for consistent views of the same object. It does not guarantee independent reconstruction of five armour assemblies. The successful two-set experiment is evidence only at that scale.
- Semantic segmentation v2 (`v2.0-20260430`, Beta) offers semantic labels plus geometry and connectivity splitting for 40 credits. Smart model segmentation is 55. Neither was needed because parts generation already returned the required components. A segmentation call cannot recover a missing thigh plate or fix fitting.
- Texturing after parts is supported as a separate task, with references re-supplied; standard texture is 10 credits. The local atlas costs zero API credits and is appropriate for the simple materials, but omits richer concept-specific surface detailing.

Sources checked: [H3.1 model and costs](https://developers.tripo3d.ai/en/models/v3-1), [multiview H series](https://developers.tripo3d.ai/en/docs/generation-multiview-to-model/standard), [P series parameters](https://developers.tripo3d.ai/en/docs/generation-multiview-to-model/p), [semantic segmentation](https://developers.tripo3d.ai/en/docs/mesh-segment), [texture after generation](https://developers.tripo3d.ai/en/docs/models-texture), [Developer pricing](https://developers.tripo3d.ai/en/pricing).

## Proposed next spend — not authorised or submitted

First fix the modular body masks/fit and upper-thigh reference coverage locally, then review these two tiers. If that succeeds, a three-set Bronze/Brass/Iron request at about 40 credits and a five-set upper-tier request at about 40 would add **80 credits**, bringing total prototype+generation spend to **120** and leaving about **180** from the original 300. Those are conditional estimates, not a guarantee of successful five-set output or finished assets. Two fresh five-set batches would cost the same estimated 80 but would regenerate Tin/Copper unnecessarily.

A more conservative continuation at the tested two-set size would use four additional pairs (Bronze/Brass, Iron/Steel, Hardened Steel/Tungsten, Titanium/Veilcrystal), about **160 additional credits / 200 total**, leaving about **100**. This processes lower tiers first and avoids assuming five-set scalability, but is still gated on repaired references, fitting and approval.

Eight individual H3.1 parts generations would cost about **320**, exceeding the remaining 260 by 60. This is the conservative parts-preserving fallback estimate. Smaller multi-set batches could reduce that count, but need approval and preserved detail. API standard textures would add 10 per request; semantic segmentation would add 40 per rescue attempt. No rescue, retry or bulk job is automatic. Cost savings demonstrated so far: two sets in one 40-credit parts request versus two equivalent individual 40-credit parts requests, saving 40 generation credits, before art/fitting work.

## Deliverables and scope

See `README.md` for repeat steps, `ARMOUR_MANIFEST.csv` for actual geometry/fit status, `qa/slot_completeness.csv` for each equipment slot, `qa/export_verification.json` and `UnityValidation/ARMOUR_UNITY_VALIDATION.json` for test results, and `CREDIT_LEDGER.json` for the paid task. The optional Unity package is explicitly labelled **FITTING_DRAFT**. The live Assets/Packages/ProjectSettings, existing player masters and reference originals were not changed. Production files are confined to this separate folder.
