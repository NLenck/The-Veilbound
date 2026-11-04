import csv,hashlib,json,shutil
from pathlib import Path
ROOT=Path(r'C:\The Veilbound\PlayerAssetProduction')
cfg=json.loads((ROOT/'pipeline/player_batch.json').read_text())
balance=json.loads((ROOT/'logs/final_balance.json').read_text(encoding='utf-8-sig'))
checks=json.loads((ROOT/'qa/player_export_verification.json').read_text())
unity=json.loads((ROOT/'UnityValidation/PLAYER_UNITY_VALIDATION.json').read_text())
assert unity['passed'] and len(checks)==6 and all(c['passed'] for c in checks)
assert balance=={'balance':300,'frozen':0}
assert hashlib.sha256((ROOT/'references/Veilborne_Male_Female_Sheet.jpg').read_bytes()).hexdigest()==cfg['source_sha256']
audits={v:json.loads((ROOT/f'qa/{v}/player_audit.json').read_text()) for v in ['Male','Female']}
ledger=[];rows=[]
for v,a in audits.items():
    for stage in ['Generation','Rig']:
        result=json.loads((ROOT/f'logs/{v}_{stage}_result.json').read_text(encoding='utf-8-sig'))
        assert result['status']=='success'
        ledger.append(dict(variant=v,stage=stage,task_id=result['task_id'],credits=result['credits_consumed']))
    raw=ROOT/f'models_raw/{v}/{v}.glb';rig=ROOT/f'rigs_raw/{v}/{v}_rigged.glb'
    rows.append(dict(variant=v,status='rigged_locally_animated_review',raw_triangles=a['original_triangles'],
        actual_triangles=a['final_triangles'],bones=a['bone_count'],height_m=a['height_m'],skinned_meshes=1,
        material_slots=len(a['materials']),generation_credits=50,rig_credits=25,local_animation_credits=0,
        raw_sha256=hashlib.sha256(raw.read_bytes()).hexdigest(),rig_sha256=hashlib.sha256(rig.read_bytes()).hexdigest(),
        raw_glb=str(raw),raw_rig_glb=str(rig),blend_file=str(ROOT/f'models_blender/{v}/{v}.blend'),
        rigged_fbx=str(ROOT/f'models_unity/{v}/{v}_Rigged.fbx'),animated_fbx=str(ROOT/f'models_unity/{v}/{v}_Locomotion.fbx'),
        animated_glb=str(ROOT/f'models_unity/{v}/{v}_Locomotion.glb'),unity_playback='Generic/direct skeleton',humanoid_rest_avatar_valid=True,
        animations='Idle;Walk;Run',review_notes='Movement prototypes; not integrated with live combat/equipment. Detailed identity and clothing fidelity need art review.'))
total=sum(t['credits'] for t in ledger);assert total==150 and 450-total==balance['balance']
ledger.append(dict(variant='Male',stage='Failed idle retarget',task_id='2c991b47-a10c-4f79-bb80-f56cc75e21b3',credits=0,status='failed; hold refunded; no retry'))
(ROOT/'qa/credit_ledger.json').write_text(json.dumps(dict(initial_balance=450,initial_estimate=210,revised_cap=150,actual_total=150,remaining=300,frozen=0,tasks=ledger),indent=2))
with (ROOT/'PLAYER_MANIFEST.csv').open('w',newline='',encoding='utf-8') as f:
    w=csv.DictWriter(f,fieldnames=list(rows[0]));w.writeheader();w.writerows(rows)
shutil.copy2(ROOT/'UnityValidation/PLAYER_UNITY_VALIDATION.json',ROOT/'qa/unity_validation.json')
package=ROOT/'UnityValidation/Veilborne_Player_Review.unitypackage'
assert package.is_file() and package.stat().st_size>100000
shutil.copy2(package,ROOT/package.name)
shutil.copytree(ROOT/'UnityValidation/Assets/VeilbornePlayer',ROOT/'unity_staging/VeilbornePlayer',dirs_exist_ok=True)
table='\n'.join(f"| {v} | {a['original_triangles']:,} | {a['final_triangles']:,} | {a['bone_count']} | {a['height_m']} m | 75 |" for v,a in audits.items())
tasks='\n'.join(f"| {t['variant']} | {t['stage']} | `{t['task_id']}` | {t['credits']} |" for t in ledger)
motion='\n'.join(f"| {c['variant']}/{c['clip']} | {c['duration']:.3f} | {c['loop']} | {c['footMotion']:.3f} | {c['minHeight']:.4f} to {c['maxHeight']:.3f} |" for c in unity['clips'])
report=f'''# Veilbound player model and locomotion review

Date: 27 October 2026 (Australia/Sydney). Output: `C:\\The Veilbound\\PlayerAssetProduction`.

Both male and female reference-based player variants are delivered with a fresh mapped 52-bone rig, 4K PBR texture set, locally authored idle/walk/run prototypes, Blender sources, FBX/GLB exports and a tested Unity review package. **The live game has not been changed.**

## Models

| Variant | Generated triangles | Final triangles | Bones | Provisional height | Credits |
|---|---:|---:|---:|---|---:|
{table}

The requested nominal target was about 20,000 triangles. Tripo returned approximately 27,000 despite that setting. That detail was retained for a single on-screen player rather than forcibly reducing the face, fingers and clothing. Local pupil and fissure geometry adds a small amount of detail; the male rear hair was reduced locally. Each final variant uses one skinned mesh and three material slots. Imported Unity GPU vertex counts are higher than authoring vertex counts because flat normals, UVs and material boundaries split vertices; triangle counts above remain the relevant geometry count.

## Visual source and review

The supplied 1280x853 male/female concept sheet is preserved byte-for-byte, SHA-256 `{cfg['source_sha256']}`. Front/left/back/right views were cropped separately for each variant and used in one multiview generation per model. Portrait, eye and skin crops were kept as validation references, not fed as extra body views.

Both models retain worn brown clothing, waist ties, cropped trousers, lower-leg wraps, bare feet, dark hair and the ashen human aesthetic. The male has the beard and loose hair; the female has the tied-back/bun silhouette. Reconstruction is approximate: facial structure and hair shapes differ, and some cloth tears/folds and wraps were simplified. The unwanted male rear tail has been replaced with the clean mirrored upper-back surface, with a local UV reprojection. A small lift to mouth corners and outer lids softens the downcast expression; the faces remain stern. The full-body source crops are only about 150x380 pixels, so the larger portrait details are not faithfully reconstructed by geometry alone.

The generated red-eye and fissure details were weak. The original misplaced iris rings and black centres have been completely removed. Two 2.8 mm red glowing pupils per face were fitted to manually measured eye-opening landmarks in uncovered orthographic renders. Pupil vertices conform to the actual evaluated surface and inherit barycentrically interpolated skin weights. The shallow 0.12 mm surface lift avoids z fighting. Blender and Unity use the same 0.65 emission multiplier. Restrained angular cheek/neck fissures were authored locally against the surface. Discontinuous surface hits were excluded to prevent glowing lines bridging through air. The emission material and eye placement remain art-review choices, not an exact copy of the close-up portraits. The body atlas is preserved; UV edits are confined to the male reconstructed back region. Eye alignment and front-facing pupil normals passed the separate socket audit (less than 0.1 pixel error in the measured orthographic view). Short residual base-mesh boundary loops were closed after the hair edit. The separate socket audit records the final welded boundary/nonmanifold measurements; each welded base now has zero boundary edges, but one nonmanifold edge and one negligible-area face remain per variant. These models are review assets rather than certified watertight production characters. The male reconstructed rear hair/neck transition remains visibly faceted and needs a manual art refinement pass before final acceptance. This correction used zero additional API credits.

## Rig and animation approach

The existing game uses a custom procedural 17-bone rig and independent armour meshes. The user approved both variants and a movement redo. A new service rig was therefore created rather than forcing the new body into the old rig.

Although the rig request used `spec=mixamo`, the returned rig had generic service bone labels. Anatomy was inferred from hierarchy and joint positions, then mapped to Mixamo-style names, including fingers. Upper-leg parents were corrected to the hips while preserving rest positions and skin weights. All 52 bones are retained; there are no unweighted vertices or invalid weight sums in either authored model. A Blender-generated bone-display widget was excluded from validation rather than counted as character geometry.

The first paid idle-retarget task failed. It was not retried and its hold was refunded. No walk/run retarget tasks were submitted. All six final clips are **locally authored Blender prototypes**, not successful Tripo retarget outputs. The gait uses stance/swing phases, analytic leg placement, pelvis settling/bob, shoulder counter-motion, elbow bend, and heel/toe motion. Deformed foot surfaces guide vertical contact correction. The walk/run calibration is approximately 1.2 / 3.6 metres per second; final gameplay speeds and stride/turn feel still need playtesting.

Unity Humanoid rest avatars validate for both variants, but Humanoid animation conversion altered proportions during playback in the isolated test. The working review prefabs deliberately use **Generic/direct skeleton clips**, which preserve the authored proportions and planted-foot motion. Separate `<Variant>_Humanoid.fbx` rest rigs are included for future retarget calibration. Do not switch the review locomotion clips to Humanoid and assume they have been validated that way.

The Unity controller contains an idle/walk/run blend tree driven by `PlanarSpeed`, with damped parameter updates and optional exponential facing interpolation. The runtime driver takes actual collision-resolved velocity and aim direction through `SubmitMotion`; it does not replace input, collision, saves or combat. Root motion is disabled.

## Verification

All six rest/animated FBX and animated GLB round trips passed measured triangles, 52-bone count, animation names, resolved textures, finite coordinates and skin-weight coverage. The shared normalization was baked into rest bones/mesh and local translation keys so exported metric scale remains stable.

An isolated Unity **6000.3.9f1** project compiled the import/runtime scripts and built both review prefabs. Both Generic playback avatars and the separate Humanoid rest avatars are valid. All six direct-skeleton clips import as loops, animate foot bones, bake finite skin geometry and stay within plausible metric bounds.

| Variant/clip | Duration (s) | Loop | Sampled foot motion (m) | Sampled mesh Y range (m) |
|---|---:|---|---:|---|
{motion}

The sampled foot travel is relative in-place motion, not a foot-slide proof inside the live game. The tested bounds include the authored walk/run crouch. Tests sample several phases, not every possible blended pose. Body/cloth self-intersections and full UV-overlap proofs were not exhaustively tested. The animation GIFs show the local Blender prototypes at their actual clip durations.

## Actual Developer API spend

Generation used CLI `tripo-v3.1`, wire `v3.1-20260211`, `face_limit=20000`, `smart_low_poly=true`, `texture_quality=detailed`, texture/PBR enabled. Rigging used `v2.5-20260210`. Exact dry-run plans and submission markers are saved in `logs/`. No Studio credits, purchases or top-ups were used.

| Variant | Step | Task ID | Billed credits |
|---|---|---|---:|
{tasks}

**Total: 150 Developer API credits. Starting balance: 450. Final verified balance: 300; frozen: 0.** The initial 210-credit plan was reduced to a 150-credit cap when paid retargeting failed; subsequent animation work was local and free. No further tasks are queued.

## Deliverables

- `Veilborne_Player_Review.unitypackage`: tested isolated review assets, prefabs, controllers, materials and import settings under a new `Assets/VeilbornePlayer` folder. Importing this package is a later user action; it has not been imported into the live game.
- `unity_staging/VeilbornePlayer/`: the same loose asset/source folder with `.meta` files preserved. Use either this folder or the Unity package, not duplicate copies of both.
- `models_raw/<Variant>/<Variant>.glb` and `rigs_raw/<Variant>/<Variant>_rigged.glb`: untouched service outputs plus CLI artifact directories.
- `models_blender/<Variant>/<Variant>.blend`: final single-mesh rig, all three actions, textures and preview studio.
- `models_unity/<Variant>/`: rigged rest FBX, combined locomotion FBX/GLB, per-clip FBX files and texture sidecars. Texture roles include BaseColor, Normal, ORM, Unity MetallicSmoothness and Occlusion.
- `previews/PLAYER_VARIANTS_OVERVIEW.jpg`, `PLAYER_HEAD_DETAILS.jpg`, `PLAYER_WALK_REVIEW.gif`, `PLAYER_RUN_REVIEW.gif`, and per-variant front/side/back/three-quarter/head renders.
- `PLAYER_MANIFEST.csv`, `qa/credit_ledger.json`, per-variant audits, export checks and `qa/unity_validation.json`.
- `pipeline/`: generation safeguards, local authoring, final scale-safe reexport, verification and packaging sources.
- `UnityValidation/`: isolated test project. Its generated files are validation evidence, not changes to the game project.

## Work before replacing the live player

These are review models and locomotion prototypes, not a completed combat/equipment integration. Existing armour parts were not refitted or rebound. The base clothing is part of the generated body surface, not an interchangeable wardrobe. Greatsword grip, attack/cast/dodge poses, equipment occlusion/clipping and sex selection/save plumbing still need an integration pass. Keep the current character active until these models and motion are accepted.

Check the face/hair likeness and emission subtlety, then review walk/run foot cadence and upper-body feel at the game's camera distance. Humanoid retargeting beyond the provided rest avatar requires further calibration; the validated direct-skeleton clips are the supported movement path in this package.
'''
(ROOT/'PLAYER_MODEL_REPORT.md').write_text(report,encoding='utf-8')
readme='''# Veilborne player review kit

The Unity package is the easiest way to review. It adds Assets/VeilbornePlayer only; it does not replace the existing player or scene.

Both Generated/*_Review.prefab assets use Generic/direct-skeleton playback. Do not change the locomotion FBX files to Humanoid: that conversion is not the validated playback path. Separate *_Humanoid.fbx rest rigs are supplied for later retarget work.

Open Idle, Walk and Run in the Unity animation preview. PlanarSpeed blend thresholds are 0, 1.2 and 3.6 m/s. The driver uses damped blending and exponential turning; feed actual resolved movement and aim via SubmitMotion. The clips are in-place. Existing input/combat/equipment integration is intentionally separate.

The loose folder already includes generated review assets. The Build review prefabs menu is for a fresh kit without Generated assets and refuses to overwrite existing controllers. See PLAYER_MODEL_REPORT.md for measured counts, credit records, validation and known limits.
'''
(ROOT/'unity_staging/README.md').write_text(readme,encoding='utf-8')
print(json.dumps(dict(variants=2,triangles={v:a['final_triangles'] for v,a in audits.items()},bones=52,credits=150,remaining=300,blender_export_checks=6,unity_motion_checks=6,package_bytes=(ROOT/package.name).stat().st_size)))
