# Veilbound player model and locomotion review

Date: 27 October 2026 (Australia/Sydney). Output: `C:\The Veilbound\PlayerAssetProduction`.

Both male and female reference-based player variants are delivered with a fresh mapped 52-bone rig, 4K PBR texture set, locally authored idle/walk/run prototypes, Blender sources, FBX/GLB exports and a tested Unity review package. **The live game has not been changed.**

## Models

| Variant | Generated triangles | Final triangles | Bones | Provisional height | Credits |
|---|---:|---:|---:|---|---:|
| Male | 26,800 | 30,352 | 52 | 1.85 m | 75 |
| Female | 26,975 | 27,084 | 52 | 1.73 m | 75 |

The requested nominal target was about 20,000 triangles. Tripo returned approximately 27,000 despite that setting. That detail was retained for a single on-screen player rather than forcibly reducing the face, fingers and clothing. Local pupil and fissure geometry adds a small amount of detail; the male rear hair was reduced locally. Each final variant uses one skinned mesh and three material slots. Imported Unity GPU vertex counts are higher than authoring vertex counts because flat normals, UVs and material boundaries split vertices; triangle counts above remain the relevant geometry count.

## Visual source and review

The supplied 1280x853 male/female concept sheet is preserved byte-for-byte, SHA-256 `1d2c6928a90623b9ee1c072532f9af8a176f9ff01df6ad7f946765259be1317b`. Front/left/back/right views were cropped separately for each variant and used in one multiview generation per model. Portrait, eye and skin crops were kept as validation references, not fed as extra body views.

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
| Male/Idle | 3.000 | True | 0.002 | 0.0010 to 1.851 |
| Male/Walk | 0.900 | True | 0.533 | 0.0010 to 1.828 |
| Male/Run | 0.633 | True | 0.857 | -0.0006 to 1.773 |
| Female/Idle | 3.000 | True | 0.003 | 0.0004 to 1.734 |
| Female/Walk | 0.900 | True | 0.533 | 0.0010 to 1.702 |
| Female/Run | 0.633 | True | 0.858 | -0.0005 to 1.643 |

The sampled foot travel is relative in-place motion, not a foot-slide proof inside the live game. The tested bounds include the authored walk/run crouch. Tests sample several phases, not every possible blended pose. Body/cloth self-intersections and full UV-overlap proofs were not exhaustively tested. The animation GIFs show the local Blender prototypes at their actual clip durations.

## Actual Developer API spend

Generation used CLI `tripo-v3.1`, wire `v3.1-20260211`, `face_limit=20000`, `smart_low_poly=true`, `texture_quality=detailed`, texture/PBR enabled. Rigging used `v2.5-20260210`. Exact dry-run plans and submission markers are saved in `logs/`. No Studio credits, purchases or top-ups were used.

| Variant | Step | Task ID | Billed credits |
|---|---|---|---:|
| Male | Generation | `2fd9bd72-d0b2-48cf-bb13-d07270b646bc` | 50 |
| Male | Rig | `c2031a9d-0a02-4f6e-afe2-43aa3620ba11` | 25 |
| Female | Generation | `a29ef39f-0dc2-473c-89d1-b13f758c3e17` | 50 |
| Female | Rig | `5d522963-d7d5-474e-a51c-ad3404576208` | 25 |
| Male | Failed idle retarget | `2c991b47-a10c-4f79-bb80-f56cc75e21b3` | 0 |

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
