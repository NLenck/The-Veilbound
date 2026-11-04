from pathlib import Path
import json,csv,hashlib,shutil,math
from PIL import Image,ImageDraw
root=Path(r'C:\The Veilbound');out=root/'ModelIntegration';shots=root/'Builds/Integrated/Smoke/ProductionModels';binary=root/'Builds/Integrated/TheVeilbound.exe'
editor=json.loads((out/'EDITOR_VALIDATION.json').read_text());runtime=json.loads((shots/'RUNTIME_VALIDATION.json').read_text());assert editor['passed'] and runtime['passed'];assert 'VEILBOUND_PRODUCTION_RUNTIME_PASS' in (out/'logs/runtime.log').read_text(errors='replace');assert 'VEILBOUND_PRODUCTION_BUILD_PASS' in (out/'logs/build.log').read_text(errors='replace');assert (shots/'RUNTIME_VALIDATION.json').stat().st_mtime>=binary.stat().st_mtime
assert 'VEILBOUND_CHARACTER_ROSTER_PASS' in (out/'logs/character_regression.log').read_text(errors='replace')
(out/'screenshots').mkdir(exist_ok=True);(out/'pipeline').mkdir(exist_ok=True);(out/'qa').mkdir(exist_ok=True)
shutil.copy2(shots/'RUNTIME_VALIDATION.json',out/'RUNTIME_VALIDATION.json');shutil.copy2(shots/'PASS.txt',out/'RUNTIME_PASS.txt')
for p in shots.glob('*.png'):shutil.copy2(p,out/'screenshots'/p.name)
source_rows=list(csv.DictReader((root/'EnemyAssetProduction/manifest.csv').open(encoding='utf-8-sig')));source={r['biome']+'/'+r['unit']:r for r in source_rows}
manifest=[]
for check in editor['checks']:
 if '/' in check['id']:
  r=source[check['id']];assert check['triangles']==int(r['actual_triangles']);biome,name=check['id'].split('/');manifest.append(dict(type='enemy',id=check['id'],gender='',biome=biome,triangles=check['triangles'],bones=17,equipment_slots='',unity_asset=f'Assets/Veilbound/Resources/ProductionModels/Enemies/{biome}_{name}.prefab',source=r['unity_fbx'],motion=check['motion']))
 else:
  gender=check['id'];manifest.append(dict(type='player',id=gender,gender=gender,biome='',triangles=check['triangles'],bones=52,equipment_slots=9,unity_asset=f'Assets/VeilboundPlateMasters/Generated/Tin_{gender}_Master.prefab',source=f'PlayerAssetProduction/models_unity/{gender}',motion='Generic idle/walk/run + combat IK'))
armour=list(csv.DictReader((root/'ArmourAssetProduction/Mastered/MASTER_MANIFEST.csv').open(encoding='utf-8-sig')))
for row in armour:manifest.append(dict(type='armour',id=row['tier']+'_'+row['gender'],gender=row['gender'],biome='',triangles=row['triangles'],bones=52,equipment_slots=9,unity_asset=row['prefab'],source='ArmourAssetProduction/Mastered',motion='Skinned to selected player rig'))
with (out/'INTEGRATED_MODELS.csv').open('w',newline='',encoding='utf-8') as f:w=csv.DictWriter(f,fieldnames=manifest[0].keys());w.writeheader();w.writerows(manifest)
assert len(manifest)==43
mapping=json.loads((root/'ArmourAssetProduction/Mastered/tier_mapping.json').read_text())
for gender in ['Male','Female']:
 canvas=Image.new('RGB',(1200,660),'#171b20');draw=ImageDraw.Draw(canvas)
 for i,tier in enumerate(mapping):
  label=tier['tier'].replace('HardenedSteel','Hardened steel');file_tier=tier['tier'].replace('HardenedSteel','Hardened_steel').replace('Veilcrystal','VeilCrystal')
  with Image.open(shots/f'{gender}_{file_tier}.png') as im:canvas.paste(im.convert('RGB').resize((240,300)),((i%5)*240,(i//5)*330+25))
  draw.text(((i%5)*240+8,(i//5)*330+8),label,fill='white')
 canvas.save(out/f'screenshots/ALL_TIERS_{gender.upper()}_IN_GAME.jpg',quality=95)
for biome,names in [('Town',['Husk','Lobber','Brute','Ghoul','Burster','Shrieker','Bulwark','Impaler','Ravager','Conduit']),('Shrine',['Acolyte','Cinder_Priest','Ash_Wraith','Shrine_Guardian','Blade_Sentinel','Ritual_Orb','Lamenter','Gargoyle','Veil_Angel','High_Priest'])]:
 canvas=Image.new('RGB',(1200,660),'#171b20');draw=ImageDraw.Draw(canvas)
 for i,name in enumerate(names):
  with Image.open(shots/f'{biome}_{name}.png') as im:canvas.paste(im.convert('RGB').resize((240,300)),((i%5)*240,(i//5)*330+25))
  draw.text(((i%5)*240+8,(i//5)*330+8),name.replace('_',' '),fill='white')
 canvas.save(out/f'screenshots/{biome.upper()}_ENEMIES_IN_GAME.jpg',quality=95)
canvas=Image.new('RGB',(1280,950),'#171b20');draw=ImageDraw.Draw(canvas)
for i,(label,name) in enumerate([('Male / original body','Male_Unarmoured.png'),('Male / Tin armour','Male_Tin.png'),('Female / original body','Female_Unarmoured.png'),('Female / Veilcrystal','Female_VeilCrystal.png')]):
 with Image.open(shots/name) as im:canvas.paste(im.convert('RGB').resize((320,400)),(i*320,30))
 draw.text((i*320+10,10),label,fill='white')
for i,(label,name) in enumerate([('Sanctuary Ruins / generated enemies','Shrine_Gameplay.png'),('The Fallen Borough / generated enemies','Town_Gameplay.png')]):
 with Image.open(shots/name) as im:canvas.paste(im.convert('RGB').resize((640,450)),(i*640,500))
 draw.text((i*640+10,477),label,fill='white')
canvas.save(out/'screenshots/INTEGRATED_MODELS_OVERVIEW.jpg',quality=95)
original=json.loads((out/'backup_before_integration/original_file_hashes.json').read_text());changed=[]
for row in original:
 p=root/row['path']
 if p.exists() and hashlib.sha256(p.read_bytes()).hexdigest()!=row['sha256']:changed.append(row['path'])
(out/'CHANGED_EXISTING_FILES.json').write_text(json.dumps(changed,indent=2))
for p in (root/'Assets/Veilbound/Scripts').glob('Production*.cs'):shutil.copy2(p,out/'pipeline'/p.name)
for name in ['ProductionModelImport.cs','BodyIntegrationDiagnostic.cs']:shutil.copy2(root/'Assets/Veilbound/Editor'/name,out/'pipeline'/name)
ledger=dict(paid_api_calls=0,credits_spent=0,original_production_assets_preserved=True,previous_build_preserved=True,player_models=2,armour_appearances=20,enemy_models=21,build=str(binary),build_sha256=hashlib.sha256(binary.read_bytes()).hexdigest())
(out/'INTEGRATION_MANIFEST.json').write_text(json.dumps(ledger,indent=2))
rows='\n'.join('| '+c['id']+' | '+str(c['triangles'])+' | '+c['motion']+' |' for c in editor['checks'] if '/' in c['id'])
report=f'''# Models integrated into The Veilbound

28 October 2026 (Australia/Sydney).

The live Unity project now uses the created male/female Veilborne characters, all ten plate materials on both rigs, and all twenty Town/Shrine enemy models plus the Forest Wolf. **No paid API calls or Tripo credits were used.**

## Launch

Run [TheVeilbound.exe](../Builds/Integrated/TheVeilbound.exe) with its data folder and DLLs beside it. The prior `Builds/Windows` build remains intact. In Unity, use the existing `Assets/Veilbound/Scenes/HollowCourt.unity` scene.

Choose **Male appearance** or **Female appearance** on the character selection screen, or choose the appearance while creating a character. Appearance is saved per character. Existing characters default to male until changed; resources, workshop levels, item ownership, rolls and expedition progression are preserved.

## Player and equipment

The new 52-bone skeletons are used directly with the supplied Generic idle/walk/run clips. Collision-resolved velocity drives movement blending, with continuous stride phases as speed changes. Root motion is off. Additive arm IK reaches the existing greatsword grips, and the existing swing/cast gesture calls remain connected. The legacy character system is retained as a fallback if the new catalog is unavailable.

Nine equipment slots remain independently equippable. Existing item IDs choose the approved Tin/Copper mesh families and their ten material palettes. Capes use the chest material palette, falling back to the weapon material when there is no metal chest piece; cape ownership, crafting costs and the ability to unequip it are unchanged. **Veilcrystal is blood-red metal with black cape and black belt cloth.** Cloth/leather items remain functional using fitted shared geometry with matte material treatment. No new cloth/leather designs were generated.

The body mask and reversible waist tuck are connected to actual equipment state. All removed armour restores 30,352 male / 27,084 female body triangles. Matched body vertices retain the original bone weights and idle pose (within 0.04 mm in the sampled comparison). Review captures bake the current skin pose, avoiding a previous-frame GPU skinning artifact during immediate screenshots.

The compatibility facade maps the new joints for existing inventory portraits, slot anchors, thumbnail cameras and combat callers. Character saves now carry appearance independently of the settlement and preserve old saves without requiring a reset.

## Enemies and biome mapping

Town uses Husk/Shrieker as standard variants, Lobber/Conduit as ranged variants, Brute/Bulwark as heavy variants, Ghoul/Impaler/Ravager/Wolf as fast variants and Burster as the volatile. The wolf is a Town wildlife variant because there is currently no Forest landscape. Town bosses use Brute, Conduit and Bulwark.

Shrine uses Acolyte/Blade Sentinel as standard variants, Cinder Priest/Lamenter as ranged variants, Shrine Guardian as heavy, Gargoyle/Ash Wraith as fast and Ritual Orb as volatile. Bosses use Shrine Guardian, Veil Angel and High Priest. Existing timings, health, damage, rewards, pathfinding, radius rules, projectiles and warning/fuse mechanics are preserved; boss display names match the new models.

All twenty-one enemy source triangle counts match the production manifest. UVs and colour textures are retained. New local 17-joint rigs and inexpensive procedural biped/quadruped/floating motion make the models usable without paid rigging. Red-dominant crystal texture regions supply restrained emission, with the existing hit and fuse property-block effects preserved. Enemies share their saved mesh/material assets. Textures are capped at 1024 pixels for these crowd assets; source textures and production files are unchanged.

| Model | Triangles | Local motion |
|---|---:|---|
{rows}

## Verified in the game

- Unity { '6000.3.9f1' } compiled and built `Builds/Integrated/TheVeilbound.exe`; existing economy/progression logic checks passed.
- Editor validation passed for two player rigs, twenty armour prefabs and twenty-one enemy meshes. Player foot-bone movement, two-handed grip reach, finite skinned vertices and full body restoration were checked.
- The built game passed all twenty gender/material appearances, all nine active single-material slots, mixed cloth/leather equipment, helmet removal and body restoration. Equipment portraits and thumbnails rendered using the new rigs.
- All twenty-one enemies retained their triangle counts and animated with finite, bounded geometry. Actual Town/Shrine edge spawns, biome selection, boss death/combat and crowded waves passed.
- Female appearance survived a character/settlement save round trip. The existing character creation, migration, interrupted-write recovery and independent character-switching regression passed with the new models.
- A two-second 110-enemy sample measured approximately **{runtime['crowdFps']:.1f} FPS**, near the game's 60 FPS cap on this machine. This is a short functional sample, not a full performance benchmark.
- All tests used isolated smoke/profile saves. No existing player character saves were rewritten by the test runs.

Evidence: `EDITOR_VALIDATION.json`, `RUNTIME_VALIDATION.json`, `RUNTIME_PASS.txt`, `logs/`, and `Builds/Integrated/Smoke/CharacterRoster/PASS.txt`. Actual Unity captures are under `screenshots/`; `INTEGRATED_MODELS_OVERVIEW.jpg` shows both player models and both maps.

## Animation limits

Enemy motion rigs are **local procedural prototypes**, not hand-authored final animation. Quadruped ground contacts, large weapons, wings, cloth and extreme combat poses still need an art/animation fitting pass. The player uses the supplied locomotion prototypes plus procedural grips/gestures; this integration does not claim a finished dodge/attack animation library or a full-run balance certification. All created models are present and usable in the game now.

## Source, backup and repeat

`INTEGRATED_MODELS.csv` lists all 43 character/armour/enemy entries and their source and Unity paths. Runtime loading is configured by `Assets/Veilbound/Resources/ProductionModels/Catalog.asset`. New source enemies live under `Assets/Veilbound/ProductionModels/Enemies`, with generated skinned meshes, materials and prefabs under `Assets/Veilbound/Resources/ProductionModels/Enemies`. Approved staged player/armour files are imported under `Assets/VeilbornePlayer` and `Assets/VeilboundPlateMasters`.

`backup_before_integration/` retains the previous scripts, scenes, editor code, package/project settings and README; `CHANGED_EXISTING_FILES.json` lists changed pre-existing files. Original reference art, raw generations, Blender masters, production exports and the previous executable were preserved. No new purchases, API jobs or credit expenditure occurred.

To rebuild the catalog/enemy rigs, run `Veilbound.Editor.ProductionModelImport.Run` in Unity batch mode; `Validate` checks them and `Build` produces the separate integrated Windows executable. Run that executable with `-batchmode -veilSmoke -productionModelsReview` for the isolated runtime suite, and `-characterRosterReview` with `-veilSmoke` for the character-save regression. The tested integration C# sources are copied into `pipeline/`.
'''
(out/'INTEGRATION_REPORT.md').write_text(report,encoding='utf-8')
p=root/'README.md';text=p.read_text(encoding='utf-8');text=text.replace('Launch `Builds/Windows/TheVeilbound.exe`','Launch `Builds/Integrated/TheVeilbound.exe`',1)
notice='''## Integrated character, armour and enemy models

The latest build uses both Veilborne character appearances, all ten approved plate palettes, and the twenty Town/Shrine enemies plus the wolf. Choose the appearance on character selection or during creation; equipment and progression remain saved per character. Veilcrystal uses blood-red metal and black cloth. The previous `Builds/Windows` executable is retained.

See [Model integration report](ModelIntegration/INTEGRATION_REPORT.md) for the complete inventory, actual in-game screenshots, verification and the provisional enemy-animation limits.

'''
if '## Integrated character, armour and enemy models' not in text:text=text.replace('## Start here',notice+'## Start here',1)
p.write_text(text,encoding='utf-8')
p=root/'Docs/Validation.md';text=p.read_text(encoding='utf-8');section='''

## Created-model integration — 28 October 2026

The live project and `Builds/Integrated` build now use the created male/female 52-bone players, twenty material/gender plate appearances and all twenty-one generated enemies. Editor and built-game tests pass equipment/body-mask restoration, finite skinning, direct-skeleton locomotion, two-hand grips, appearances/saves, thumbnails, biome spawns, bosses/combat and a 110-enemy crowd sample. Existing character roster/save regressions also pass. Enemy procedural rigs and extreme combat/cloth poses remain provisional. See `ModelIntegration/INTEGRATION_REPORT.md` and its JSON/log evidence. The old Windows build and pre-integration scripts/settings are preserved.
'''
if '## Created-model integration' not in text:p.write_text(text+section,encoding='utf-8')
print('MODEL_INTEGRATION_COMPLETE',json.dumps(dict(players=2,armour_looks=20,enemies=21,manifest_rows=len(manifest),runtime_pass=True,character_regression_pass=True,credits_spent=0,changed_existing_files=len(changed))))
