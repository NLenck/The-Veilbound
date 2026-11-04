# Veilborne Greatsword Prototype Report

Completed 28 October 2026. The single composite generation worked: ten complete swords were reconstructed as ten independent meshes. All ten have individual GLB and FBX exports, an editable packed Blender scene, and tested Unity review prefabs. No fallback generation was needed.

## Actual cost

**One paid generation: 40 Developer API credits. Balance: 260 before, 220 after; frozen: 0.** No extra texturing, segmentation, conversion or retries were purchased. The original approximately 1,500-credit purchase was not treated as a new balance.

Task `4876aff4-f009-48f9-869b-1f8d4656a9f3` used H Series `v3.1-20260211` (CLI alias `tripo-v3.1`), image-to-model, 30,000 face limit, `generate_parts=true`, `texture=false`, `pbr=false`. The exact supplied clean composite PNG was the input. The saved dry run, task receipt, balance snapshots and `CREDIT_LEDGER.json` record this transaction.

The installed official CLI 0.7.2 worked with the Developer API. Each CLI command identified the client as Codex. Parts output requires disabling texture and PBR in this request. Smart low-poly would suppress parts, so it was omitted. No paid semantic segmentation was necessary. Tripo's Batch Images feature is a queue of independently charged generations; it was not used. A single composite is feasible in this particular tested result, not a guarantee for future composites. See [generation parameters](https://developers.tripo3d.ai/en/docs/generation-image-to-model/standard) and [Batch Images documentation](https://www.tripo3d.ai/product-doc/model-generation/batch-image-to-3d).

## Extraction and geometry

The output contained ten native objects, each with one connected complete blade, guard, grip and pommel. Sorting their centres along the source scene's Y axis recovered the reference's left-to-right material order. No sword was split into separate equipment items. No flat board or fused group was accepted.

| Material | Generated triangles | Final triangles | Components |
|---|---:|---:|---|
| Tin | 2,910 | 2,910 | Complete |
| Copper | 2,338 | 2,350 | Complete |
| Bronze | 2,594 | 2,598 | Complete |
| Brass | 2,770 | 2,770 | Complete |
| Iron | 2,358 | 2,358 | Complete |
| Steel | 2,622 | 2,614 | Complete |
| HardenedSteel | 3,206 | 3,206 | Complete |
| Tungsten | 3,606 | 3,626 | Complete |
| Titanium | 3,208 | 3,228 | Complete |
| Veilcrystal | 3,112 | 3,112 | Complete |

Total: 28,724 generated triangles; **28,772 final triangles**. Local repairs capped small holes, removed tiny disconnected seam fragments and collapsed zero-area seam faces. No aggressive decimation was needed. All ten have positive volume, consistent face winding, finite geometry, UVs, zero boundary edges, zero nonmanifold edges and zero degenerate faces. Each export reimported as one complete mesh with one material: **20/20 round-trip checks passed**.

Depth was reduced locally to avoid excessively chunky blades while retaining 3D volume. Tungsten retains **1.51 times Titanium's blade width and 1.65 times its thickness**. One shared palette atlas material serves all ten models; colours were applied locally, with no paid AI texturing. UVs use palette swatches, suitable for these simple materials rather than detailed baked surface artwork.

## Visual limitations

These are usable geometry prototypes for art review, not exact replicas of the concept sheet. Some guards and pommels are rounded or irregular, Titanium's open ornamental shapes are simplified, and Veilcrystal's crystalline facets and spikes are less precise than the reference. Engravings and surface wear were not reconstructed faithfully. The shared local palette approximates the reference colours. Veilcrystal uses black metal with crimson accents; Titanium uses silver, gold and blue. Review the reference comparison and individual side/three-quarter renders before approving final art.

## Scale, grip and Unity review

Unity blade direction is **+Z**. The local origin matches the existing weapon socket; guard Z=0.30 m, blade tip Z=1.72 m, pommel approximately Z=-0.309 m. Attachment markers are MainHand `(0, 0.035, 0.015)` and OffHand `(0, 0.035, 0.17)`. Blender authoring uses -Y as the long axis and exports to Unity +Z.

New review assets are under `Assets/VeilboundGreatswords`. Ten prefabs contain one mesh renderer each, the shared material, grip markers and blade-tip marker. `GreatswordVisualLibrary` maps the existing material IDs, including Titanium=7 and Tungsten=8, and offers `CreateVisual(socket, metalIndex)` without changing weapon stats.

The existing male rig holding Tungsten and female rig holding Veilcrystal were tested in temporary Unity review scenes. Hand-to-target errors were approximately 0.000002 m and 0.00594 m respectively. These checks validate attachment and the sampled pose, not every attack animation or finger contact. Actual Unity renders are saved in `previews`.

The exported review unitypackage was unpacked into a clean Unity 6000.3.9f1 project and validated: ten prefabs, one shared material, all atlas dependencies, correct triangle counts and grip markers, no missing scripts. The package does not require the live game's character scripts.

**The live weapon factory, gameplay and existing weapon assets remain unchanged.** Six relevant runtime source hashes and all 16 reference files were verified unchanged. No updated playable build was produced. These are review assets ready for a deliberate visual-library hookup after art review.

## Remaining work and unspent fallback estimates

No swords are missing. The lowest-cost next step is local art polish of these ten meshes, followed by attack-pose/clipping review. Further paid generations are not needed for separation or completeness and have not been authorised or submitted.

Current published rates below are estimates for new work, not additional charges. They exclude optional steps and can change. [Official pricing](https://developers.tripo3d.ai/en/pricing), [model pricing](https://docs.tripo3d.ai/get-started/pricing.html).

| Alternative | Estimated additional credits |
|---|---:|
| One H geometry composite without parts (untested) | 20 |
| Two five-sword H composites with parts | 80 |
| Ten individual H geometry generations | 200 |
| Ten individual H textured generations | 300 |
| Ten individual P1 geometry generations | 400 |
| Ten individual P1 textured generations | 500 |
| Standard segmentation of an existing model | 40 |
| Smart segmentation model step | 55 |
| Separate standard H texture task | 10 per task |
| P2 Preview geometry / textured generation | 100 / 110 per task |

The selected one-call parts workflow cost 40 versus 200 credits for ten bare H generations: **160 credits saved against that baseline**. Geometry and texturing quality are not guaranteed equivalent across those alternatives. Only 220 credits remain, so some fallback plans exceed the balance. No additional purchases or paid tasks were performed.

## Deliverables

- `Greatswords_Processed.blend`: packed editable source, processed collections and preserved hidden original group.
- `raw/Generated_Ten_Greatswords.glb`: byte-identical original GLB; source task files retained under `raw/tripo-out`.
- `exports/`: ten named GLBs and ten FBXs.
- `atlas/`: shared base colour, metallic/smoothness and emission atlases.
- `previews/`: actual Unity lineup, male/female grip demos, reference comparison and individual front/side/three-quarter Blender renders.
- `Veilborne_Greatswords_Review.unitypackage`: tested review package.
- `SWORD_MANIFEST.csv`, `CREDIT_LEDGER.json`, `qa/`, `logs/`, `pipeline/`: measured counts, receipts, tests and reusable scripts.

See `WORKFLOW.md` for repeatable local processing. The paid-prototype guard must remain in place to prevent accidental duplicate spending.
