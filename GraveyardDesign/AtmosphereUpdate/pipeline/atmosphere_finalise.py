from pathlib import Path
import json,shutil
R=Path(r'C:\The Veilbound');O=R/'GraveyardDesign/AtmosphereUpdate';W=Path(__file__).parent
data=json.loads((R/'GraveyardDesign/Implementation/validation.json').read_text());assert data['passed'] and data['braziers']==6 and data['cathedralBreaches']==4
assert 'VEILBOUND_PRODUCTION_BUILD_PASS' in (O/'build.log').read_text()
log=(O/'runtime.log').read_text();assert 'VEILBOUND_GRAVEYARD_RUNTIME_PASS' in log and 'Exception:' not in log
shutil.copy2(R/'GraveyardDesign/Implementation/validation.json',O/'validation.json')
for name in ['gameplay.png','overview.png','cathedral-gameplay.png','PASS.txt']:shutil.copy2(R/'Builds/Integrated/Smoke/Graveyard'/name,O/name)
P=O/'pipeline';P.mkdir(exist_ok=True)
for name in ['GraveyardArena.cs','GraveyardDressing.cs','GraveyardBrazierFlame.cs']:
 p=R/'Assets/Veilbound/Scripts'/name;shutil.copy2(p,P/name);shutil.copy2(p,W/name)
shutil.copy2(R/'Assets/Veilbound/Editor/GraveyardValidation.cs',P/'GraveyardValidation.cs');shutil.copy2(W/'atmosphere_finalise.py',P/'atmosphere_finalise.py')
(O/'README.md').write_text(f'''# Graveyard atmosphere update

Completed 28 October 2026. Run `Builds/Integrated/TheVeilbound.exe`.

Added six forged-iron braziers: four around the shrine approaches and two flanking the cathedral's north entrance. They have low-poly orange/gold flames, gentle scale/light flicker, compact warm light pools and solid stone feet. The six local point lights use no shadows. Their small ranges and limited number keep the lighting local; frame-rate performance was not benchmarked.

The shrine now has a broken stone halo behind its crystal. Two faded burgundy funeral banners on supported spars add identity to the cathedral and participate in the existing interior cutaway. These additions are decorative; the fires do not cause damage.

The grave plots, wide main lanes and combat floor remain clear. Brazier bases and shrine-frame feet have collision and were included in the existing large-body navigation checks. All four cathedral breaches and all 96 sampled perimeter spawns passed again. The scene is {data['triangles']:,} static triangles and {data['renderers']} mesh renderers, excluding characters, lights and the fog post-process. The previous pass was 14,846 triangles / 28 renderers.

Editor geometry/route checks, build logic and production asset validation passed. The actual rebuilt game passed enemy/boss entry, actual boss cathedral traversal, cutaway and random village/Graveyard switching checks. Actual-game previews are `gameplay.png`, `cathedral-gameplay.png` and `overview.png`. Evidence is in `validation.json`, `validation.log`, `build.log`, `runtime.log` and `PASS.txt`.

No Tripo requests or paid generation were used. Weapon stats, abilities, character saves, the village layout and arena bounds are unchanged by this pass. The Integrated executable was rebuilt; Builds/Windows remains unchanged.

Updated source snapshots are in `pipeline` and pre-change arena/validator sources are in `Backup`. Repeat validation and building with Unity 6000.3.9f1: `-batchmode -quit -projectPath "C:\\The Veilbound" -executeMethod Veilbound.Editor.GraveyardValidation.Build -logFile <path>`. Run the player with `-batchmode -veilSmoke -graveyardReview -logFile <path>` for isolated gameplay checks and captures.
''',encoding='utf-8')
print('GRAVEYARD_ATMOSPHERE_DELIVERY_VERIFIED',data)
