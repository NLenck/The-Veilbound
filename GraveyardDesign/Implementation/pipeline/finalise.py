from pathlib import Path
import shutil,json,hashlib
R=Path(r'C:\The Veilbound');O=R/'GraveyardDesign/Implementation';W=Path(__file__).parent
data=json.loads((O/'validation.json').read_text());assert data['passed'] and data['headstones']==27 and data['cathedralBreaches']==4
assert 'VEILBOUND_PRODUCTION_BUILD_PASS' in (O/'build.log').read_text()
runtime=(O/'runtime.log').read_text();regression=(O/'weapon-regression.log').read_text();assert 'VEILBOUND_GRAVEYARD_RUNTIME_PASS' in runtime and 'Exception:' not in runtime
assert 'VEILBOUND_WEAPON_SCALING_RUNTIME_PASS' in regression and 'Exception:' not in regression
for name in ['overview.png','gameplay.png','cathedral-gameplay.png','PASS.txt']:shutil.copy2(R/'Builds/Integrated/Smoke/Graveyard'/name,O/name)
shutil.copy2(R/'Builds/Integrated/Smoke/WeaponScaling/validation.json',O/'weapon-regression.json')
pipeline=O/'pipeline';pipeline.mkdir(exist_ok=True)
for name in ['GraveyardArena.cs','GraveyardReview.cs','ExpeditionMaps.cs','WorldArt.cs','VeilGame.cs','RandomMapSmoke.cs','RuinedTownSmoke.cs']:
 p=R/'Assets/Veilbound/Scripts'/name;shutil.copy2(p,pipeline/name)
 if name in ['GraveyardArena.cs','GraveyardReview.cs']:shutil.copy2(p,W/name)
shutil.copy2(R/'Assets/Veilbound/Editor/GraveyardValidation.cs',pipeline/'GraveyardValidation.cs')
for name in ['install.py','finalise.py']:shutil.copy2(W/name,pipeline/name)
(O/'source_manifest.json').write_text(json.dumps([dict(path=str(p.relative_to(R)),sha256=hashlib.sha256(p.read_bytes()).hexdigest()) for p in pipeline.glob('*.cs')],indent=2))
(O/'IMPLEMENTATION_REPORT.md').write_text(f'''# Graveyard arena

Implemented and tested 28 October 2026. Run `Builds/Integrated/TheVeilbound.exe`. The former Sanctuary/shrine arena at map ID 0 is now displayed as **Graveyard**. The Fallen Borough remains the other equally weighted random expedition destination. Existing shrine enemy selections, doubled boss visuals, player equipment and the recent weapon/ability rules are preserved. Saved map IDs and progression were not changed. `Builds/Windows` is unchanged.

## Clean arena interpretation

- Central circular paved shrine with four open cardinal approaches, short diagonal colonnade remnants and one small altar set north of the clear player spawn.
- {data['headstones']} simple low-poly headstones in nine separated plots across the northwest, northeast and southeast. Markers are low enough to keep enemies visible; space between adjacent markers admits the largest current navigation body.
- One roofless cathedral in the southwest, with four broad breaches, low collapsed east steps, a few pointed window outlines, a broken gable and four side pews. The central floor remains open. Interior wall cutaways keep the player visible.
- Two dead trees inside the grounds, with a few silhouettes outside in the Veil. Most crimson colour is confined to the shrine and three small rim accents.
- Broad muted ground colours and large paving shapes. No carpet of loose rubble, repeated glowing graves, grave fences, dense ground dressing or painted micro-detail.
- Existing volumetric Veil fog surrounds the playable circle. It is visible at the edges and the four outgoing roads disappear into it. The circular ground boundary is not a physical wall.

This is a simplified low-poly gameplay interpretation of the sketch and concept. The dense decorative detail of the generated concept was deliberately reduced after the user's readability feedback. Original sketches, concept files and the old `SanctuaryArena` source remain intact. No paid generation or Tripo calls were used.

## Dimensions and budget

Player radius: {data['playerRadius']:g} m. Enemy spawn radius: {data['spawnRadius']:g} m. Central plaza radius: 9.4 m. Cathedral footprint: 19 × 20 m at world centre (-20, -21). Its breaches are 5.6 m wide. Grave spacing within plots is 3.8 m.

Static scene: **{data['triangles']:,} triangles and {data['renderers']} mesh renderers**, excluding characters, boundary lights and the fog post-process. Scenery is batched by material; cathedral walls are separate batches for cutaways. Shared materials are reused and generated meshes are destroyed on map unload. No crowd-frame-rate benchmark was performed in this task.

## Validation

Editor checks passed for all 27 grave obstacles and adjacent large-body lanes, the safe origin spawn, the four shrine approaches and both routes around the altar, cathedral placement, all four cathedral breaches, swept movement, cutaway visibility and **{data['reachableSpawnSamples']}/96** sampled connected perimeter spawn positions.

The actual rebuilt player passed Graveyard naming and construction, all existing shrine enemy families and three doubled boss selections, actual boss navigation through a cathedral entrance, indoor cutaway, randomized map selection and repeated village/Graveyard switching with the appropriate 42/50 m or 52.5/62.5 m bounds and fog settings. Captured `overview.png`, `gameplay.png` and `cathedral-gameplay.png` are actual Unity player renders, not generated artwork.

The existing weapon-scaling regression also passed with Graveyard as the default arena: all twelve profiles, 96 damage checks, 108 cooldown checks, 36 actual timer checks, Rage/Virtue/bleed/Haste checks, resource display and save/load checks. Build logic, upgrade validation and production asset validation passed.

Receipts: `validation.json`, `validation.log`, `build.log`, `runtime.log`, `weapon-regression.log`, `weapon-regression.json`, `PASS.txt`.

## Implementation and repeatability

Layout: `Assets/Veilbound/Scripts/GraveyardArena.cs`. The `WorldArt.Arena` factory now builds this layout. `ExpeditionMaps.cs` supplies the display name, random-destination notice and cathedral cutaway updates. The internal shrine enemy asset IDs are retained; they identify the existing models rather than the map's visible name.

`Assets/Veilbound/Editor/GraveyardValidation.cs` creates editor previews and validates collision/navigation. Run Unity 6000.3.9f1 with `-batchmode -quit -projectPath "C:\\The Veilbound" -executeMethod Veilbound.Editor.GraveyardValidation.Build -logFile <path>` to validate and rebuild. Graphics are needed for rendering.

Run the Integrated player with `-batchmode -veilSmoke -graveyardReview -logFile <path>` for isolated gameplay checks and camera captures. `-weaponScalingReview` runs the preserved combat regression. Smoke runs use separate test state and QA saves rather than real character profiles.

Source snapshots are in `pipeline`; pre-change files are in `Backup`. The installer is a one-time patch record, not an idempotent script to rerun blindly. The old shrine layout remains in its source class and can be restored through the backed-up factory wiring.
''',encoding='utf-8')
print('GRAVEYARD_DELIVERY_VERIFIED',data)
