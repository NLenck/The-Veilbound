from pathlib import Path
import json,csv,shutil,hashlib
R=Path(r'C:\The Veilbound\ArmourAssetProduction');BASE=Path(__file__).resolve().parent
result=json.loads((R/'logs/prototype_result.json').read_text(encoding='utf-8-sig').strip().splitlines()[-1]);before=json.loads((R/'logs/balance_before.json').read_text(encoding='utf-8-sig'));after=json.loads((R/'logs/balance_after.json').read_text(encoding='utf-8-sig'))
c=json.loads((R/'qa/classification.json').read_text());exports=json.loads((R/'qa/export_verification.json').read_text());unity=json.loads((R/'UnityValidation/ARMOUR_UNITY_VALIDATION.json').read_text());raw=json.loads((R/'qa/prototype_components.json').read_text())
assert result['status']=='success' and result['credits_consumed']==40 and before['balance']-after['balance']==40 and after['frozen']==0
assert len(exports)==74 and all(x['passed'] for x in exports) and unity['passed']
fits={v:json.loads((R/f'qa/fitting_{v}.json').read_text()) for v in ['Male','Female']}
ledger=dict(task_id=result['task_id'],type=result['type'],model='v3.1-20260211',actual_credits=40,starting_balance=before['balance'],remaining_balance=after['balance'],frozen=after['frozen'],paid_calls=1,bulk_paid_calls=0,retries=0,local_material_and_export_credits=0)
(R/'CREDIT_LEDGER.json').write_text(json.dumps(ledger,indent=2))
rows=[]
for tier in ['Tin','Copper']:
    objects=[o for o in c['objects'] if o['tier']==tier]
    for variant in ['Male','Female']:
        a=[o for o in fits[variant]['objects'] if o['tier']==tier];m=[x for x in fits[variant]['motion'] if x['tier']==tier]
        rows.append(dict(tier=tier,variant=variant,status='SEPARABLE_FITTING_DRAFT_NOT_GAME_READY',independent_slots=9,source_objects=14,raw_triangles=sum(o['triangles'] for o in raw['raw_meshes'] if o['name'] in {p['source'] for p in objects}),export_triangles=sum(o['triangles'] for o in a),runtime_armour_renderers=9,shared_armour_materials=1,rig_bones=52,unweighted_vertices=sum(o['unweighted_vertices'] for o in a),max_motion_body_penetration_fraction=max(x['penetration_fraction'] for x in m),missing_coverage='upper-thigh plates',credits_for_shared_two_tier_geometry=40))
with (R/'ARMOUR_MANIFEST.csv').open('w',newline='') as f:
    w=csv.DictWriter(f,fieldnames=rows[0].keys());w.writeheader();w.writerows(rows)
table='\n'.join(f"| {r['tier']} | {r['variant']} | {r['raw_triangles']:,} | {r['export_triangles']:,} | {r['independent_slots']}/9 | {r['max_motion_body_penetration_fraction']:.1%} |" for r in rows)
report=f'''# Plate armour prototype report

## Decision

**Grouped parts extraction works for this two-set Tin/Copper prototype. Final character fitting does not yet pass visual acceptance. Stop paid generation here.** The prototype does not demonstrate that a five-set composite will work, and none of the remaining eight tiers has been generated.

The official CLI 0.7.2 authenticated to the existing Developer account. Actual balance was 300, not a newly funded 1,500. One multiview task `{result['task_id']}` succeeded for **40 credits**, leaving **260 available / 0 frozen**. No Studio credits, purchases, paid retries, API texturing, segmentation, rigging or conversion were used.

## What was produced and checked

The two-tier input produced exactly **28 meshes / 28 welded connected components / {raw['raw_triangles']:,} raw triangles**, with no extra body/mannequin component. Fourteen objects belong to each tier: helmet, two pauldrons, chest, two bracers, two gauntlets, belt/tassets, two knee/shin guards, two boots and cape. They were classified by the measured scene regions and height bands, checked against front/back inspection, then named and placed in `Plate_Tin` and `Plate_Copper` collections. The paired leg objects share the Legs slot. No fused geometry was cut apart or silently relabelled.

The original grouped GLB remains unchanged under `raw/`. `blender/Prototype_Organised_Exploded.blend` preserves both organised sets. `exports/exploded/` contains individual named GLBs and an assembled exploded GLB per tier. `exports/fitted/` contains male/female full-slot FBX+GLB drafts and nine slot FBXs per tier/variant. Fitted pivots use the relevant rig joints rather than the arbitrary exploded centres.

| Tier | Variant | Raw tris | Export tris | Separable slots | Worst sampled body penetration |
|---|---|---:|---:|---:|---:|
{table}

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
'''
(R/'ARMOUR_PROTOTYPE_REPORT.md').write_text(report,encoding='utf-8')
readme='''# Veilbound plate armour production prototype

Start with ARMOUR_PROTOTYPE_REPORT.md. These are separable but visually failing fitting drafts, not finished game assets. Bulk generation is disabled. No API retries or paid rescue steps are automatic.

## Repeat locally without spending

Use Blender 5.2 and Python with Pillow. Scripts use explicit C:/The Veilbound/ArmourAssetProduction paths. Run prepare_pipeline.py (atlas/configs), inspect_rigs.py in Blender, inspect_prototype.py in Blender against the saved successful raw result, organise_prototype.py in Blender, then fit_prototype.py -- Male and -- Female in Blender. Run verify_exports.py in Blender. This rebuilds drafts from saved raw data and costs zero Tripo credits.

package_prototype.py creates the isolated UnityValidation project and source kit. Open only that validation project with Unity 6000.3.9f1. ArmourPrototypeReview.Run builds four draft prefabs and tests slot toggles plus actual animation. It creates Generated assets and should be run only with that disposable Generated folder absent; it never targets the live game's Generated or Assets directories. ArmourPrototypeReview.Export packages a passed technical test. finalise_prototype.py writes counts/report/ledger and copies validated staging files. Never treat a technical pass as visual acceptance.

The anatomy bands in organise_prototype.py are calibrated to this inspected two-set layout, not a universal five-set classifier. It fails if expected helmets/slots are absent or duplicated. For a different layout, inspect raw axes/components first, set explicit tier regions, then review the assignments. Never assign names to fused slots just to fill the manifest.

## Paid generation gate

run_prototype.ps1 defaults to a dry run. Its SubmitPrototype switch refuses to run when an attempt/result already exists. The present prototype has already been used and must not be resubmitted. Logs preserve result, progress, task ID, starting/remaining balance and validated payload. Group_A/Group_B_PROPOSAL_ONLY.json have submit=false and require a new user decision after fit/coverage review. Every CLI invocation sets TRIPO_CLIENT_APP=codex and TRIPO_DISTRIBUTION_CHANNEL=codex_plugin; credentials remain owned by the official CLI. Do not inspect/copy them.

## Unity review and future integration

The draft package adds ArmourPrototype and the required VeilbornePlayer review dependencies. It does not replace the live hero. Four Generated/*FITTING_DRAFT.prefab assets have nine armour slot renderers sharing one material; source left/right objects remain separate in Blender and in source FBXs. ModularPlateSlots.SetSlotEnabled uses the game's numeric nine-slot order. Rebind by exact bone names to the new 52-bone review rig; the old procedural 17-bone player is incompatible. Chest, belt and legs are weighted across relevant body bones. Cloth/cape uses spine/hips weights without simulation. Runtime body masks and correct complete leg/hand/boot coverage remain required before gameplay integration.

Renderer count can be reduced further by rebuilding a combined equipped mesh only when loadout changes. Prune unused bone palettes, build LODs after silhouette review, and budget cape geometry explicitly. Do not add real-time cloth simulation for every enemy by default.
'''
(R/'README.md').write_text(readme,encoding='utf-8')
for p in BASE.iterdir():
    if p.is_file() and p.suffix in ['.py','.ps1','.cs','.txt']:shutil.copy2(p,R/'pipeline'/p.name)
kit=R/'UnityValidation/Assets/ArmourPrototype';shutil.copytree(kit,R/'unity_staging/ArmourPrototype',dirs_exist_ok=True)
package=R/'UnityValidation/Plate_Prototype_FITTING_DRAFT.unitypackage'
if package.exists():shutil.copy2(package,R/package.name)
print('PROTOTYPE_REPORT_COMPLETE',json.dumps(dict(credits=40,remaining=260,slots=18,verified_exports=74,unity=unity['passed'],bulk=False)))
