# Veilbound — Town and Shrine production batch

Date: 27 October 2026 (Australia/Sydney).

**All 20 Town and Shrine static enemy assets are present:** 18 newly generated models plus reused Husk and Gargoyle. Forest Wolf remains from the test batch; the other 19 Forest/Cave enemies were not generated. No rigging, animation or Unity integration was performed.

## Scope and cost

The user approved proceeding with village/Town and Shrine after reviewing the three-model test. This is a separate continuation batch; the original first-run 300-credit cap and report remain historical records. New batch hard cap: 900 Developer API credits. Beginning balance: 1,350. No purchases, top-ups, duplicate sample generations or automatic retries.

All 18 CLI dry runs passed with no warnings. One paid `POST /v3/generation/image-to-model` call per new enemy: CLI alias `tripo-p1`, actual model `P1-20260311`, texture=true, pbr=false. Each used the individually cropped hero view with its original reference artwork. Other sheet views guided visual checking. Requested triangle caps and provisional height/class choices are in `pipeline/batch_config_town_shrine.json`. Classification guides art budgets and does not assign gameplay rarity.

**New batch actual spend: 900 credits. Final API balance: 450; frozen: 0. Lifetime pipeline spend: 1050 of the original 1,500 Developer API credits.** Local Blender work and free balance reads used no credits.

| Enemy | Actual exported triangles | Materials | New batch credits |
|---|---:|---:|---:|
| Town/Husk | 2762 | 1 | reused sample |
| Town/Lobber | 3902 | 1 | 50 |
| Town/Brute | 6310 | 1 | 50 |
| Town/Ghoul | 3185 | 1 | 50 |
| Town/Burster | 4351 | 1 | 50 |
| Town/Shrieker | 3074 | 1 | 50 |
| Town/Bulwark | 6456 | 1 | 50 |
| Town/Impaler | 4520 | 1 | 50 |
| Town/Ravager | 4562 | 1 | 50 |
| Town/Conduit | 6219 | 1 | 50 |
| Shrine/Acolyte | 3337 | 1 | 50 |
| Shrine/Cinder_Priest | 4464 | 1 | 50 |
| Shrine/Ash_Wraith | 4460 | 1 | 50 |
| Shrine/Shrine_Guardian | 6931 | 1 | 50 |
| Shrine/Blade_Sentinel | 6429 | 1 | 50 |
| Shrine/Ritual_Orb | 3836 | 1 | 50 |
| Shrine/Lamenter | 4578 | 1 | 50 |
| Shrine/Gargoyle | 6702 | 1 | reused sample |
| Shrine/Veil_Angel | 10087 | 1 | 50 |
| Shrine/High_Priest | 8049 | 1 | 50 |

## Itemized paid-task ledger

| Enemy | Step | Task ID | Actual credits |
|---|---|---|---:|
| Town/Lobber | image_to_model | `8354ca02-5e95-4f30-970a-23dd81d4b87e` | 50 |
| Town/Brute | image_to_model | `538760b1-cd87-4d5c-b66c-5144295c9125` | 50 |
| Town/Ghoul | image_to_model | `b8838043-9b5b-4660-8c57-e5c9bb8ce39d` | 50 |
| Town/Burster | image_to_model | `b9d7bf4e-728c-4c3c-b1f8-6c8e2cd17bcd` | 50 |
| Town/Shrieker | image_to_model | `747ce409-6f43-4045-96fb-41732ac8e189` | 50 |
| Town/Bulwark | image_to_model | `8a3696cd-b938-4e6e-b261-1ab40a7bd906` | 50 |
| Town/Impaler | image_to_model | `c929f68b-92ec-454c-b181-94d8f6e83c55` | 50 |
| Town/Ravager | image_to_model | `47fbbece-a308-4d75-9004-2ebf869b9f3a` | 50 |
| Town/Conduit | image_to_model | `f7b30c88-8bcf-4ec7-b293-08604f88a253` | 50 |
| Shrine/Acolyte | image_to_model | `f2ad3fd9-6c8d-41e8-82cc-882f80b1fc3c` | 50 |
| Shrine/Cinder_Priest | image_to_model | `d9752468-ff8c-425f-8963-46c5ea70876b` | 50 |
| Shrine/Ash_Wraith | image_to_model | `ff2255eb-3537-4d98-9b32-04c899f0cd98` | 50 |
| Shrine/Shrine_Guardian | image_to_model | `be7c8d1b-a74b-4a4e-99d7-962fe8435a45` | 50 |
| Shrine/Blade_Sentinel | image_to_model | `0b5bdd1e-76c7-4700-b0ee-585a5fb24076` | 50 |
| Shrine/Ritual_Orb | image_to_model | `b8d06921-d510-4885-a7b7-d38649f19409` | 50 |
| Shrine/Lamenter | image_to_model | `a586bea8-ff22-4c60-800a-a032dbc0fcc7` | 50 |
| Shrine/Veil_Angel | image_to_model | `bcf559c7-8a46-402d-b9e2-df20e95b08d9` | 50 |
| Shrine/High_Priest | image_to_model | `f0e53e5f-0998-471c-97a6-34116fa0852a` | 50 |

Total for the above tasks: **900 credits**. Reused Husk and Gargoyle cost zero in this batch; their original 50-credit task records remain in `FIRST_BATCH_REPORT.md` and the original ledger.

## Blender processing and saved files

Each new enemy includes `models_raw/<Biome>/<Unit>.glb`, a byte-identical original generated GLB with SHA-256 provenance; `models_blender/<Biome>/<Unit>.blend`; `models_unity/<Biome>/<Unit>.fbx` and `.glb`; PNG colour-texture sidecars; and front, side, three-quarter and back 1000-pixel preview renders. All original CLI artifacts are retained too.

Blender 5.2.3 LTS applies bottom-centred metric transforms and forward-axis normalization, micrometre welding of coincident GLB vertices while retaining UV corner data, normal recalculation and flat shading. It separates over-shared edges without deleting silhouette faces. Geometry was generated directly within low-poly production budgets; no aggressive decimation was used. Textures and UVs were preserved; no paid retopology or retexture steps were submitted. One rough, nonmetallic material per enemy is used where delivered; actual counts are in the table. No multi-material atlas bake was required if all counts are one.

Ritual Orb received a targeted local debris cleanup after inspecting connected components: 2 tiny flat boundary components / 161 faces removed. The open ring and volumetric crystal motif were preserved. Shrieker received an extra axis correction. Later preview framing uses per-view projected mesh bounds to keep long weapons and wing tips within the image.

Saved .blend files include a separate preview studio excluded from mesh-only engine exports. Flat shading adds faceting, but is not a substitute for an artist rebuilding softened crystal shapes. Dimensions are provisional asset scales including crowns, staffs, wings or floating shards. Final in-game sizes, floating pivots and material/shader treatment should be reviewed on a later import.

## Validation

All 40 source image SHA-256 hashes match the initial inventory. Reference pack images and its manifest were left unchanged. Existing Unity Assets, scenes, Packages and ProjectSettings were not modified. The original first-batch report and credit ledger were preserved.

All **36 new FBX/GLB round trips** passed actual triangle count, material count, UV presence, texture availability, no armatures and dimensions within 0.002 m. The four existing Husk/Gargoyle export checks also passed in the first batch. Triangle-range exceptions: **none**.

| Enemy | Boundary edges | Components | Over-shared/loose edges | Inconsistent-normal edges | Zero-area UV triangles |
|---|---:|---:|---:|---:|---:|
| Husk | 178 | 36 | 0 | 0 | 0 |
| Lobber | 256 | 21 | 0 | 0 | 0 |
| Brute | 132 | 15 | 0 | 0 | 0 |
| Ghoul | 75 | 7 | 0 | 0 | 0 |
| Burster | 593 | 103 | 0 | 0 | 0 |
| Shrieker | 132 | 3 | 0 | 0 | 0 |
| Bulwark | 106 | 14 | 0 | 0 | 0 |
| Impaler | 116 | 24 | 0 | 0 | 0 |
| Ravager | 40 | 8 | 0 | 0 | 0 |
| Conduit | 501 | 58 | 0 | 0 | 0 |
| Acolyte | 113 | 16 | 0 | 0 | 0 |
| Cinder_Priest | 28 | 9 | 0 | 0 | 0 |
| Ash_Wraith | 146 | 30 | 0 | 0 | 0 |
| Shrine_Guardian | 71 | 35 | 0 | 0 | 0 |
| Blade_Sentinel | 193 | 53 | 0 | 0 | 0 |
| Ritual_Orb | 306 | 48 | 0 | 0 | 0 |
| Lamenter | 10 | 13 | 0 | 0 | 0 |
| Gargoyle | 90 | 59 | 0 | 0 | 0 |
| Veil_Angel | 27 | 18 | 0 | 0 | 0 |
| High_Priest | 403 | 20 | 0 | 0 | 0 |

Open cloth/wing panels, mouth openings and detached crystals remain. Disconnected geometry is expected for floating crystal motifs but needs examination before skinning. Zero over-shared edges does not prove a watertight or deformation-ready mesh. Full UV-overlap and self-intersection proof is outside these checks. These are **static review candidates**; no animation-ready topology or final art approval is claimed. Unity render-pipeline appearance and gameplay silhouettes have not been tested inside the untouched game project.

## Visual review against the supplied references

**Lobber:** Raised crystal projectile, second crystal hand, lean humanoid and ragged skirt retained. Crystal projectile and shoulder planes are simplified; fingers need review before rigging. Static review candidate.

**Brute:** Broad hunched humanoid and oversized asymmetrical red-crystal forearm retained. The service softened the shoulder and torso planes and simplified some crystal facets. Inspect crystal attachment and ragged cloth edges before acceptance.

**Ghoul:** Confirmed humanlike face, long human arms/hands and humanoid hind limbs on all fours; no canine reconstruction. Dorsal and forearm crystals retained. Mouth crystals and finger separation differ from the concept; a hind foot is lifted and contact pose needs correction before use. Review the long hands and limb intersections in the side/back renders.

**Burster:** Bloated humanoid silhouette, open red crystal belly, shoulder shards and red crystal hands retained. The service rounded some belly-edge shards and simplified the face and skirt. The red centre is painted colour, not a separately authored emissive material.

**Shrieker:** Very thin humanoid, elongated neck, open screaming mouth, crystal head spines and forearm growths retained. Face and mouth interior are simplified; inspect the narrow silhouette and cloth panels from gameplay distance. Its generated axis needed an additional local 90-degree correction for the front view.

**Bulwark:** Broad hunched humanoid and long asymmetrical crystal forearm retained, without adding a separate shield. Shoulder and crystal planes are softer and less angular than the reference, and some small back growths are simplified. Inspect hands and cloak/skirt openings before skinning.

**Impaler:** Paired long red forearm blades, lean humanoid legs and tall head/shoulder spikes retained. Spike placement and torso details differ from the concept. Check thin blade tips and hand/blade transitions before rigging.

**Ravager:** Long red crystal claws, back/head crest and forward-leaning humanoid pose retained. Claws and head planes are simplified and do not exactly reproduce every reference shard; review the hand/crystal silhouette from both sides.

**Conduit:** Tall humanoid, branching crystal staff, long ragged robe and red crystalline forearm/claw retained. The generated staff and robe are recognizable but some thin branch/shard and finger shapes are simplified. Keep staff clearance and thin cloth panels under review.

**Acolyte:** Faceless hood with red eyes, red crown/shoulder crystals, angular light armour and the single red sword retained. Fingers, knee plates and robe cuts are simplified; check sword thickness and hand grip before rigging.

**Cinder_Priest:** Hooded faceless priest, long light-and-red robes, shoulder crystals and a three-pronged crystal staff retained. Staff-tip and robe planes are simplified, and the hand grip should be checked before rigging.

**Ash_Wraith:** Floating hooded silhouette, hollow face, long claws, shredded robe and red shoulder/tail crystals retained. Detached shard counts and robe tears differ from the sheet; verify the lower floating silhouette and open cloth panels before final use.

**Shrine_Guardian:** Armoured guardian, long axe, large shield with red crystal inset, helmet crest and shoulder growths retained. Plate edges and crystal shapes are simplified; inspect the axe grip, shield thickness and thin protruding cape strip behind the shield before acceptance. Preview framing was fitted to the silhouette to retain the entire axe.

**Blade_Sentinel:** Slender armoured figure, tall crystal helmet crest and both long red swords retained. Hand grips, armour planes and sword serrations are simplified; inspect blade thickness and crest proportions against the sheet.

**Ritual_Orb:** Open stone ring, central star/core and detached red crystals retained without inventing a humanoid. Floating-crystal placement and count differ from the reference and several shards are softer/elongated. Inspected flat extraction-boundary debris was removed locally from the optimized copy; raw GLB retained unchanged. Flat shading does not restore every original crystal facet.

**Lamenter:** Elongated floating hooded figure, dark face, red eyes, shoulder shards, long claws and ragged robe retained. Cloth tears, hand shapes and crystal placement are simplified; check thin claws and open cloth panels before rigging.

**Veil_Angel:** Wide layered feather-like wings, red crystal crown/shoulders, masked humanoid torso, red forearm blades and pointed floating lower body retained. Feather planes are rounder than the original and red blade/shard placements differ. Inspect layered-wing intersections and thin blade tips before deformation work.

**High_Priest:** Tall masked priest, red crystal crown, shoulder spikes, ornate light/red robes, cape and crystal staff retained. The cape hem and staff ornament are simplified. The left hand is more open than the supplied grip and should be corrected against the staff before rigging.

The sample-specific limitations for Husk/Gargoyle remain documented in `FIRST_BATCH_REPORT.md`. Do not silently substitute these new meshes into gameplay before reviewing their silhouettes and later deformation requirements.

## Review artifacts and reusable local commands

- `previews/Town_COMPLETE_CONTACT_SHEET.jpg` and `Shrine_COMPLETE_CONTACT_SHEET.jpg`: all ten enemies in each biome.
- `previews/Town_FULL_REVIEW_SHEET.jpg` and `Shrine_FULL_REVIEW_SHEET.jpg`: front, side and three-quarter comparisons for all 20.
- `manifest.csv`: all 40 enemies with actual costs, triangles, dimensions, paths and per-enemy notes. Nineteen Forest/Cave entries remain ungenerated.
- `qa/credit_ledger_town_shrine.json`, `qa/export_verification_town_shrine.json` and per-unit mesh audits.
- `qa/visual_review_town_shrine.json`: explicit visual findings for each new model.
- `pipeline/process_batch2_one.ps1 -Unit <Name>`: local cleanup/export/render for one existing raw model; costs no API credits.
- `pipeline/run_batch2_one.ps1`: per-submission balance/cap reconciliation and submission markers. It refuses any previously attempted unit; do not remove markers to retry. Nothing is queued automatically.

## Next production work

Review weapons, thin fingers, cloth panels and detached crystals at gameplay camera distance, then correct the flagged silhouette details locally. Preserve every raw source. When topology changes are required for deformation, retopologize around joints and bake the existing colour texture onto the changed UVs. Keep red-crystal emission masks as a separate authoring decision; painted red colour currently carries most glow information.

The remaining 450 credits cover at most 9 further textured P1 generations at the observed 50-credit price, with no reserve for paid retries or mesh operations. No Forest/Cave generation or additional paid processing is authorized by this batch and none is queued.
