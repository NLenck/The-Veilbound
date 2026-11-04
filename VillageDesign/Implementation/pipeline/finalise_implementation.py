from pathlib import Path
import json,hashlib,shutil
R=Path(r'C:\The Veilbound');O=R/'VillageDesign/Implementation';W=Path(__file__).parent
data=json.loads((O/'validation.json').read_text());assert data['passed'] and data['buildings']==8 and data['openingsChecked']==32
assert 'VEILBOUND_PRODUCTION_BUILD_PASS' in (O/'build.log').read_text()
log=(O/'runtime.log').read_text();assert 'VEILBOUND_EXPANDED_VILLAGE_RUNTIME_PASS' in log
assert 'Exception:' not in log and 'Check failed:' not in log
runtime=R/'Builds/Integrated/Smoke/ExpandedVillage'
for name in ['actual-player-overview.png','gameplay-crossroads.png','PASS.txt']:shutil.copy2(runtime/name,O/name)
pipeline=O/'pipeline';pipeline.mkdir(exist_ok=True)
for name in ['RuinedTownArena.cs','VillageMeshOwner.cs','VillageRuntimeReview.cs']:
 shutil.copy2(R/'Assets/Veilbound/Scripts'/name,pipeline/name);shutil.copy2(R/'Assets/Veilbound/Scripts'/name,W/name)
shutil.copy2(R/'Assets/Veilbound/Editor/VillageValidation.cs',pipeline/'VillageValidation.cs')
for name in ['install_village.py','finalise_implementation.py']:shutil.copy2(W/name,pipeline/name)
changed=['ArenaCollision.cs','ArenaNavigation.cs','VeilGame.cs','EnemyCombat.cs','ExpeditionMaps.cs','VeilFogEffect.cs','VeilBoundary.cs','RuinedTownSmoke.cs','RandomMapSmoke.cs']
for name in changed:shutil.copy2(R/'Assets/Veilbound/Scripts'/name,pipeline/name)
for name in ['TownSurface.shader','VeilVolume.shader']:shutil.copy2(R/'Assets/Veilbound/Resources'/name,pipeline/name)
files=list((R/'Assets/Veilbound/Scripts').glob('Village*.cs'))+[R/'Assets/Veilbound/Scripts/RuinedTownArena.cs']
manifest=[dict(path=str(p.relative_to(R)),sha256=hashlib.sha256(p.read_bytes()).hexdigest()) for p in files]
(O/'source_manifest.json').write_text(json.dumps(manifest,indent=2))
(O/'IMPLEMENTATION_REPORT.md').write_text(f'''# Expanded low-poly ruined village

Implemented and tested 28 October 2026 in `Builds/Integrated/TheVeilbound.exe`.

The new layout replaces the Fallen Borough expedition map. It remains in the existing random destination pool alongside Sanctuary Ruins; normal expeditions can select either. No manual destination selector or progression/balance change was added. Existing character saves and equipment are retained. `Builds/Windows` was left unchanged.

## Implemented scene

Eight roofless ruined buildings follow the approved sketch: two northwest houses, northeast church and house, southwest hall and tavern, two southeast houses. Broad north-south paving crosses a narrower east-west street. The church has broken Gothic window outlines, a bell-gable, side pews and altar; the hall has civic furniture/dais and torn banners; the tavern has a bar, tables and barrels; the houses have domestic furnishings and damaged hearths. Charred roof timbers, low rubble, angular dead trees and red crystal growths provide low-poly silhouettes. This is a simplified procedural game-art interpretation of the concept, not a reproduction of its detailed painted surfaces.

The playable radius is **52.5 m**, 25% above the prior 42 m; the spawn radius is **62.5 m**, up from 50 m. The 25% radius increase gives 56.25% more playable area. Geometry and placement use local meshes generated in Unity. **No Tripo credits or paid generation were used.**

The outer Veil uses the existing volumetric fog system, with map-specific bounds. The central village remains clear and roads fade into the outer cloud. Sanctuary keeps its 42/50 m bounds. Its layout and map selection weighting are preserved. Fog also runs in editor cameras so actual-scene preview renders show the effect; the shader's ray distance cap supports the pulled-back overview.

## Collision and navigation

Every building has four floor-level wall breaks, 4.8 m wide. Rubble flanks them, and furnishings were moved out of the cross-building routes after clearance tests. Wall collision remains present on intact masonry, including attack/projectile obstruction. Interior visual cutaways lower walls near the player without changing their physical footprints. Navigation grid dimensions now derive from each collision map's radius; the larger map retains the same 0.75 m cell spacing.

Editor checks passed for all **32 openings**, swept movement through each gap using 1.12 m body clearance, intact-wall blocking, reachable building centres, largest-enemy walking into each room and cutaway restore. **{data['reachableSpawnSamples']}/96** uniformly sampled perimeter points admit the largest body and connect to the village; blocked points are skipped by the existing spawn search.

The actual rebuilt player passed the town movement/line-of-sight tests, normal enemy-family spawns, Elder pursuit into a building, Elder perimeter spawns while targeting each room, random destination switching and restoration of Sanctuary collision and fog bounds. The player captured `actual-player-overview.png` and `gameplay-crossroads.png`; these are real Unity renders, not generated artwork. `runtime.log` and `PASS.txt` retain the result.

## Static scene budget

- {data['triangles']:,} triangles, {data['renderers']} mesh renderers, {data['materials']} shared materials (excluding characters/enemies, fog postprocessing and boundary lights).
- Scenery is batched by material; masonry is batched per building for cutaways.
- Flat faceted geometry, matte surfaces and simple palette materials.
- Procedural meshes are owned by each village instance and destroyed on map unload. Shared material instances are reused.
- Build logic and production asset checks passed. Crowd performance was not benchmarked in this task.

## Files and repeatability

Runtime layout: `Assets/Veilbound/Scripts/RuinedTownArena.cs`. Map bounds and switching: `ExpeditionMaps.cs`, `ArenaCollision.cs`, `ArenaNavigation.cs`. Actual-game test: `VillageRuntimeReview.cs`; editor validator/render tool: `Assets/Veilbound/Editor/VillageValidation.cs`.

To validate and rebuild locally, run Unity 6000.3.9f1 with `-batchmode -quit -projectPath "C:\\The Veilbound" -executeMethod Veilbound.Editor.VillageValidation.Build -logFile <path>`. Graphics are required for renders. Then run the Integrated executable with `-batchmode -veilSmoke -villageImplementationReview -logFile <path>` for isolated runtime tests. These tests use smoke state rather than real character saves.

Implementation sources and dependent files are copied under `pipeline`; pre-change scripts are under `Backup`. The one-time installer is a record, not an idempotent deployment script: do not rerun it on already modified sources. Concept v1/v2 and the original sketch remain unchanged.
''',encoding='utf-8')
print('VILLAGE_DELIVERY_VERIFIED',data['triangles'],'triangles; 32 gaps; actual player pass')
