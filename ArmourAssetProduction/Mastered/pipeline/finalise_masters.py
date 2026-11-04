from pathlib import Path
from PIL import Image,ImageDraw
import json,csv,shutil,hashlib
S=Path(r'C:\The Veilbound\ArmourAssetProduction');R=S/'Mastered';BASE=Path(__file__).resolve().parent
tiers=json.loads((R/'tier_mapping.json').read_text());unity=json.loads((R/'UnityValidation/PLATE_MASTER_VALIDATION.json').read_text());exports=json.loads((R/'qa/master_export_verification.json').read_text());balance=json.loads((R/'qa/balance_final.json').read_text(encoding='utf-8-sig'))
assert unity['passed'] and len(exports)==46 and all(e['passed'] for e in exports)
leg_checks=json.loads((R/'qa/leg_clearance_verification.json').read_text())
assert len(leg_checks)==76 and all(c.get('belt_geometry_unchanged',True) and c.get('waist_leg_intersection_pairs',0)==0 for c in leg_checks)
assert all(v['waistTuck'] for v in unity['variants'])
palette_checks=json.loads((R/'qa/tier_palette_verification.json').read_text())
assert len(palette_checks)==20 and all(v['passed'] for v in palette_checks)
fits={v:json.loads((R/f'qa/fitting_{v}.json').read_text()) for v in ['Male','Female']}
manifest=[]
for tier in tiers:
    for variant in ['Male','Female']:
        objects=[o for o in fits[variant]['objects'] if o['tier']==tier['mesh_family']]
        manifest.append(dict(tier=tier['tier'],gender=variant,runtime_item_id=tier['item_id'],mesh_family=tier['mesh_family'],triangles=sum(o['triangles'] for o in objects),equipment_slots=9,source_objects=len(objects),runtime_slot_renderers=9,shared_tier_materials=1,bones=52,body_masks=True,cape_color=tier['cape_color'],belt_cloth_color=tier['belt_cloth_color'],local_added_thigh_plates=2,local_added_upper_arm_plates=2,local_added_elbow_couters=2,additional_tripo_credits=0,prefab='Assets/VeilboundPlateMasters/Generated/'+tier[variant.lower()+'_prefab']+'.prefab'))
with (R/'MASTER_MANIFEST.csv').open('w',newline='') as f:
    w=csv.DictWriter(f,fieldnames=manifest[0].keys());w.writeheader();w.writerows(manifest)
for variant in ['Male','Female']:
    canvas=Image.new('RGB',(1500,860),'#242830');draw=ImageDraw.Draw(canvas)
    for index,tier in enumerate(tiers):
        path=R/f'previews/{tier["tier"]}_{variant}_Master.png'
        with Image.open(path) as im:canvas.paste(im.convert('RGB').resize((300,400)),((index%5)*300,(index//5)*430+25))
        name=tier['tier'].replace('HardenedSteel','Hardened Steel');draw.text(((index%5)*300+10,(index//5)*430+8),name,fill='white')
    canvas.save(R/f'previews/ALL_TEN_TIERS_{variant.upper()}.jpg',quality=94)
    back=Image.new('RGB',(1500,860),'#242830');back_draw=ImageDraw.Draw(back)
    for index,tier in enumerate(tiers):
        with Image.open(R/f'previews/{tier["tier"]}_{variant}_Back.png') as im:back.paste(im.convert('RGB').resize((300,400)),((index%5)*300,(index//5)*430+25))
        back_draw.text(((index%5)*300+10,(index//5)*430+8),tier['tier'].replace('HardenedSteel','Hardened Steel'),fill='white')
    back.save(R/f'previews/ALL_TEN_TIERS_BACK_{variant.upper()}.jpg',quality=94)

    for family in ['Tin','Copper']:
        for clip,total_ms in [('Walk',900),('Run',633)]:
            paths=sorted((R/f'previews/motion/{family}_{variant}_{clip}').glob('*.png'));assert len(paths)==12
            frames=[Image.open(p).convert('RGB') for p in paths]
            boundaries=[round(total_ms*i/12/10)*10 for i in range(13)];durations=[b-a for a,b in zip(boundaries,boundaries[1:])]
            frames[0].save(R/f'previews/{family}_{variant}_{clip}.gif',save_all=True,append_images=frames[1:],duration=durations,loop=0)
full_models=Image.new('RGB',(1200,825),'#242830');full_draw=ImageDraw.Draw(full_models)
for column,gender in enumerate(['Male','Female']):
    with Image.open(R/f'previews/Tin_{gender}_Master.png') as im:full_models.paste(im.convert('RGB'),(column*600,25))
    full_draw.text((column*600+15,8),gender+(' - balanced arm armour' if gender=='Female' else ' - approved proportions'),fill='white')
full_models.save(R/'previews/FULL_MODELS_BLACK_VISOR.jpg',quality=96)
comparison=Image.new('RGB',(1200,825),'#242830');draw=ImageDraw.Draw(comparison)
for x,(title,path) in enumerate([('Prototype fitting',S/'previews/Tin_Male_front.png'),('Local master',R/'previews/Tin_Male_Master.png')]):
    with Image.open(path) as im:comparison.paste(im.convert('RGB').resize((600,800)),(x*600,25))
    draw.text((x*600+12,8),title,fill='white')
comparison.save(R/'previews/FITTING_BEFORE_AFTER.jpg',quality=94)
for variant in ['Male','Female']:
    old=R/f'revisions/before_thigh_sleeve_removal/Tin_{variant}.png';new=R/f'previews/Tin_{variant}_Master.png'
    if old.exists():
        shoulder_comparison=Image.new('RGB',(1080,505),'#242830');draw=ImageDraw.Draw(shoulder_comparison)
        for i,(title,path) in enumerate([('Previous thigh fit',old),('Thighs tucked under waist',new)]):
            with Image.open(path) as im:shoulder_comparison.paste(im.convert('RGB').crop((75,70,525,465)).resize((540,474)),(i*540,28))
            draw.text((i*540+12,9),variant+' - '+title,fill='white')
        shoulder_comparison.save(R/f'previews/PAULDRON_COMPARISON_{variant.upper()}.jpg',quality=94)
        shoulder_comparison.save(R/f'previews/PAULDRON_ARM_HUGGING_{variant.upper()}.jpg',quality=94)
        arm_comparison=Image.new('RGB',(960,700),'#242830');draw=ImageDraw.Draw(arm_comparison)
        for i,(title,path) in enumerate([('Previous thigh fit',old),('Thighs tucked under waist',new)]):
            with Image.open(path) as im:arm_comparison.paste(im.convert('RGB').crop((100,60,500,620)).resize((480,672)),(i*480,28))
            draw.text((i*480+10,9),variant+' - '+title,fill='white')
        arm_comparison.save(R/f'previews/ARM_CONNECTION_COMPARISON_{variant.upper()}.jpg',quality=94)
        legs=Image.new('RGB',(960,570),'#242830');draw=ImageDraw.Draw(legs)
        for i,(title,path) in enumerate([('Previous thigh fit',old),('Thighs tucked under waist',new)]):
            with Image.open(path) as im:legs.paste(im.convert('RGB').crop((120,290,480,690)).resize((480,533)),(i*480,28))
            draw.text((i*480+10,9),variant+' - '+title,fill='white')
        legs.save(R/f'previews/LEG_CLEARANCE_COMPARISON_{variant.upper()}.jpg',quality=94)



ledger=dict(additional_generation_calls=0,additional_paid_processing_calls=0,additional_tripo_credits=0,total_armour_prototype_credits=40,verified_remaining=balance['balance'],frozen=balance['frozen'],workflow='Local reusable masters and ten material variants; no further Tripo generation')
(R/'CREDIT_LEDGER.json').write_text(json.dumps(ledger,indent=2))
rows='\n'.join(f"| {t['tier'].replace('HardenedSteel','Hardened Steel')} | {t['mesh_family']} | {next(m['triangles'] for m in manifest if m['tier']==t['tier']):,} | {t['item_id']} |" for t in tiers)
report=f'''# Reusable plate armour masters

## Completed scope

The approved Tin/Copper generation now supplies **two reusable mesh families**, fitted separately to the male/female Veilborne review rigs. All ten tiers are available as temporary material variants, with **20 Unity prefabs**. This follows the request to reuse the designs and conserve credits; unique high-tier silhouettes and crystal growth geometry are deferred. Tungsten shares the Tin-family shape, Titanium shares Copper, and Veilcrystal uses blood-red metal with black cape and black belt cloth rather than newly generated crystals.

**Additional Tripo spend: 0.** No generation, texturing, rigging, segmentation or conversion API was submitted. The existing prototype cost remains 40. Final verified Developer balance is **{balance['balance']} available / {balance['frozen']} frozen**.

## Geometry and materials

| Tier | Shared mesh family | Armour triangles | Game item ID |
|---|---|---:|---:|
{rows}

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
{chr(10).join('| '+t['tier']+' | '+t['cape_color']+' | '+t['belt_cloth_color']+' |' for t in tiers)}

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
'''
(R/'MASTER_REPORT.md').write_text(report,encoding='utf-8')
(R/'README.md').write_text('# Veilbound reusable plate masters\n\nStart with [MASTER_REPORT.md](MASTER_REPORT.md). Ten temporary tier palettes use two approved mesh families with male/female fits. Import Veilbound_Plate_Masters.unitypackage into a review project and use Generated/*_Master.prefab. Keep the current live player until the new review rig is integrated. Runtime API: SetSlotTier(slot,itemId), SetSlotEnabled(slot,bool), EquipWholeTier(itemId). Body masks restore covered source geometry on unequip.\n\nAll processing is local; no further Tripo generation is required. Original prototype files remain in the parent folder.\n',encoding='utf-8')
for path in BASE.iterdir():
    if path.is_file() and path.name in ['master_fit.py','MasterPlateEquipment.cs','BuildPlateMasters.cs','package_masters.py','verify_master_exports.py','render_masters.py','finalise_masters.py','verify_pauldrons.py','verify_arm_connections.py','verify_arm_integrity.py','verify_leg_clearance.py','visor_finish.py','refresh_visor.py','female_arm_balance.py','apply_female_arm_balance.py','cloth_finish.py','tier_cloth_palettes.json','verify_tier_palettes.py']:shutil.copy2(path,R/'pipeline'/path.name)
shutil.copytree(R/'UnityValidation/Assets/VeilboundPlateMasters',R/'unity_staging/VeilboundPlateMasters',dirs_exist_ok=True)
package=R/'UnityValidation/Veilbound_Plate_Masters.unitypackage';assert package.exists();shutil.copy2(package,R/package.name)
parent_readme=S/'README.md';text=parent_readme.read_text(encoding='utf-8');notice='> Current version: **Mastered/MASTER_REPORT.md** — ten local colour variants, corrected masters and reversible body masks. The prototype notes below are historical.\n\n'
if notice not in text:parent_readme.write_text(notice+text,encoding='utf-8')
print('REUSABLE_MASTERS_COMPLETE',json.dumps(dict(tiers=10,prefabs=20,shared_slot_meshes=36,export_checks=46,additional_credits=0,remaining=balance['balance'],package_bytes=package.stat().st_size)))
