# Codex task — Veilborne greatswords, ten materials from one Tripo run if feasible

We are developing **Veilbound** in `C:\The Veilbound`. The project already uses low-poly 3D art, modular gear and a two-handed greatsword player. Inspect the existing Unity project, greatsword handling, material conventions, and Tripo extension/integration before changing files. Do not overwrite working assets. There is a reference pack accompanying this prompt; copy the `references` folder into a sensible project location and use its exact image files.

## Desired result

Build **10 independent, optimized, Unity-ready greatswords**, in ascending material order:

1. Tin — dull weathered silver-grey, plain guard and pommel, crude basic blade.
2. Copper — reddish-copper, minimally improved guard and geometric blade.
3. Bronze — deeper bronze, stronger angular central motif.
4. Brass — yellow-gold, polished, decorative but not excessively ornate.
5. Iron — dark rough iron, stocky industrial silhouette.
6. Steel — bright refined silver, clean symmetrical geometry.
7. Hardened Steel — reinforced steel, more aggressive guard and subtle spikes.
8. Tungsten — **dark gunmetal, exceptionally thick/wide and heavy**, brutal guard and solid blade; visually heavier than Titanium.
9. Titanium — refined lightweight-looking pale silver and gold with a blue inlay, distinctive open swept guard.
10. Veilcrystal — black faceted blade and guard with glowing **red** crystal elements, never purple, low-poly shapes rather than excessive particle geometry.

Preserve the designs from `references/original_8_swords_user_reference.png` as the primary visual guide for its eight depicted materials; Tin is supplied separately and Tungsten appears in `references/all_10_swords_concept_sheet.png`.

## Important optimisation idea to test first

I suspect Tripo may charge a flat **per-generation** amount regardless of the number of distinct objects shown. I want to **test one single Image-to-3D run for all ten swords** rather than immediately paying for ten separate swords. The proposed input is `references/all_10_swords_tripo_input.png`, which contains ten separate, front-facing swords in one image, evenly spaced, with no text or extra close-ups.

**Do not assume this will succeed or save credits**. Tripo officially recommends one subject per image, and its "Batch Images to 3D" queues separate jobs rather than creating one discounted ten-object mesh. Investigate the actual current costs in our installed extension/account before choosing a workflow. Determine which API (Studio vs OpenAPI v2/v3) the extension actually uses; do not conflate their balances or pricing.

Test this hierarchy:

1. **Preferred:** one paid generation from the ten-sword composite, yielding ten distinct disconnected sword meshes after import.
2. If that fails, investigate mesh separation/segmentation or 2 groups of 5 swords, but calculate the extra charge first.
3. Fallback: make each greatsword from its own image under `references/individual_fallback`. Batch submission may save time but **not necessarily money**.

Only make **one** low-cost paid prototype after checking the fee and available credits; report the results and total proposed further cost before running additional charged tasks. No repeated paid retries, no bulk paid generation without approval. Prefer the least expensive mode that can produce practical geometry; don't turn on paid post-processing options without costing them.

## About Tripo settings and model generation

Read current official documentation and inspect actual supported parameters. Relevant docs: https://developers.tripo3d.ai/en/docs/generation-image-to-model/standard and https://www.tripo3d.ai/product-doc/model-generation/batch-image-to-3d .

Important API behaviour for H-series image-to-3D: `generate_parts=true` requires `texture=false` and `pbr=false`. In v3 it is also **incompatible with `smart_low_poly=true`** (smart-low-poly takes precedence and parts may not be created). Do not select mutually incompatible flags. Try standard geometry with editable parts first if the price is reasonable; alternatively generate without texture and separate connected components in Blender, then create optimized low-poly geometry and materials locally. Be aware that arbitrary semantic "parts" do not necessarily correspond to exactly one sword each.

Treat this as an image-to-3D workflow. A long text prompt supplied to Codex does not imply the image-to-3D endpoint itself accepts detailed text conditioning. Use real input images and supported API parameters.

## Scene / splitting requirements

The ten swords are **objects**, not a single decorative display plaque or flat picture. The final asset may be generated as one grouped scene to minimise paid calls, but **the delivered objects must be separate**. Absolutely no visible text, stands, backing planes, borders, podiums, labels or characters in the resulting mesh.

Each greatsword must be complete: pommel, grip, crossguard and two-edged blade. Maintain consistent overall sword proportions; Tungsten is visibly wider/heavier. Orient all swords upright identically. Blades must be appropriately thick in 3D, not paper-flat. The material styles should match the existing references.

Create a Blender Python post-processing workflow that:

- Imports the generated GLB/FBX (or supported Tripo output), preserves the raw original, and evaluates actual geometry.
- Detects disconnected mesh islands; splits by loose parts when possible; when components of one sword are separate, group them by proximity along the horizontal lineup and not merely one island per item.
- Assigns ten sword collections left-to-right in the above material order. Validate that **each sword** has blade, hilt, guard, tip and nonzero volume.
- Detects fusions between adjacent swords, unintentional background geometry, missing blades, implausible proportions, or fewer than ten valid items and **reports failure** instead of pretending it succeeded.
- Applies only deliberate cleanup; do not silently destroy geometries or texture references.
- Brings every extracted sword into a consistent local coordinate system, with a grip-aligned origin for attaching to the existing Unity 2H weapon socket. Match the real socket transform from the project; do not invent one.
- Creates optimized game-ready versions, balancing silhouette and polygons. Prefer roughly 1–3k triangles for simpler designs and up to 5–6k for complex upper tiers when justified; report actual counts and adjust for the player's existing game rendering budget.
- Applies shared/reusable materials where practical, keeping copper/bronze/brass distinguishable and Veilcrystal red highlights emissive without expensive particle effects.
- Exports independent named assets (`GS_Tin`, `GS_Copper`, `GS_Bronze`, `GS_Brass`, `GS_Iron`, `GS_Steel`, `GS_HardenedSteel`, `GS_Tungsten`, `GS_Titanium`, `GS_Veilcrystal`) and a source `.blend` scene containing the original grouped asset and processed collections.

Use a consistent unit scale, normals and mesh orientation. Set up appropriate LODs only if they materially improve our current Unity use case.

## Unity integration

Inspect existing weapon mesh replacement, inventory/item definitions, equipment sockets, prefabs and animations. Add the resulting greatsword meshes as **visual variants of the existing two-handed greatsword weapon type**, not new combat mechanics. Avoid changing existing balance/gameplay without being asked.

If the model generation is successful and within the authorised prototype, demonstrate at least one correct imported sword attached to the player. For additional models that are not yet generated, prepare import infrastructure and naming without fabricating assets.

## Deliverables and reporting

1. A cost/credit preflight: Tripo integration path and model, actual available balance (if accessible), charge per run, and whether a 10-object single run is technically supported or just experimental.
2. Exactly one approved low-cost prototype attempt using `all_10_swords_tripo_input.png` if the account/integration allows it.
3. Inspectable 3D output plus a script/report on the actual number of complete separable swords produced.
4. Blender extraction/optimization/export scripts and instructions to reproduce.
5. Unity-side imports, naming and prefabs for any successfully produced sword assets.
6. A brief next-steps recommendation: continue single-run, split into 5+5, or use one-per-sword, with actual cost estimates and quality tradeoffs.

**Do not claim 10 completed assets if the model output is fused, missing swords, a flat board, or materially distorted.** The outcome may be a proof-of-concept and scripts rather than all ten finished models, and that is acceptable if the test reveals a Tripo limitation.
