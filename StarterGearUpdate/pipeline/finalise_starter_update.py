from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
import shutil,json
R=Path(r'C:\The Veilbound');U=R/'StarterGearUpdate';P=Path(__file__).parent
assert json.loads((U/'validation.json').read_text())['passed']
assert 'VEILBOUND_STARTER_RUNTIME_PASS' in (U/'runtime_starter.log').read_text()
assert 'VEILBOUND_PRODUCTION_BUILD_PASS' in (U/'build.log').read_text()
assert all(x['passed'] for x in json.loads((R/'GreatswordAssetProduction/qa/export_verification.json').read_text()))
for name in ['process_swords.py']:
 p=P/name;s=p.read_text();old="tree.links.new(orm.outputs['Alpha'],inv.inputs[1])";new="mul=tree.nodes.new('ShaderNodeMath');mul.operation='MULTIPLY';mul.inputs[1].default_value=.45;tree.links.new(orm.outputs['Alpha'],mul.inputs[0]);tree.links.new(mul.outputs[0],inv.inputs[1])";s=s.replace(old,new);p.write_text(s);shutil.copy2(p,R/'GreatswordAssetProduction/pipeline'/name)
shutil.copy2(R/'Assets/Veilbound/Editor/GreatswordImport.cs',R/'GreatswordAssetProduction/pipeline/GreatswordImport.cs')
shutil.copy2(R/'Assets/VeilboundPlateMasters/Editor/BuildPlateMasters.cs',R/'ArmourAssetProduction/Mastered/pipeline/BuildPlateMasters.cs')
pipeline=U/'pipeline';pipeline.mkdir(exist_ok=True)
for name in ['update_starter_and_finish.py','build_wooden_sword.py','update_export_finish.py','update_armour_exports.py','finalise_starter_update.py']:
 shutil.copy2(P/name,pipeline/name)
for name,rel in [('StarterEquipment.cs','Scripts'),('StarterGearImport.cs','Editor'),('StarterGearRuntimeCheck.cs','Scripts')]:shutil.copy2(R/'Assets/Veilbound'/rel/name,pipeline/name)
canvas=Image.new('RGB',(1600,1060),(28,33,38));d=ImageDraw.Draw(canvas);font=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',23)
for x,gender in [(0,'Male'),(800,'Female')]:canvas.paste(Image.open(U/f'{gender}_Starter.png').convert('RGB'),(x,0));d.text((x+400,1020),gender+' - no armour, wooden starter sword',font=font,fill='white',anchor='mm')
canvas.save(U/'STARTER_PREVIEW.jpg',quality=92)
image=Image.open(R/'GreatswordAssetProduction/previews/ALL_TEN_UNITY.png').convert('RGB');image.save(U/'SWORDS_MATTE_PREVIEW.jpg',quality=94)
readme='''# Starter gear and matte equipment update

Completed 28 October 2026. Run `Builds/Integrated/TheVeilbound.exe` for the updated playable build. `Builds/Windows` is unchanged.

- All ten plate materials and the shared ten-sword review material use smoothness-map scale 0.45 instead of 1. This reduces highlights without changing tier colours, cloth colours, metallic masks or crystal emission. Runtime legacy sword tint smoothness is 0.20 (Veilcrystal 0.24). Updated import scripts preserve the finish on regeneration.
- Updated both armour Blender masters, 20 tier GLBs, sword Blender source and 20 sword GLB/FBX exports. The new Unity review sword package also contains the reduced-shine finish. Historical preview images and reports describe the prior prototype; use the new Unity images here for the current finish.
- A locally modelled wooden two-handed practice sword uses a single dry-wood material, no metallic reflectivity and 0.12 smoothness. It is 340 Blender triangles / 304 triangles after Unity's FBX import. The origin, +Z blade axis, 1.72 m tip and two-hand markers match the existing weapon sockets. Editable Blender source, GLB and FBX are in this folder.
- Fresh characters start with no equipped armour or cape, no free metal equipment and no free Blacksmith upgrade. Their original tattered base clothing remains. They own the wooden greatsword, which appears in the bag and can be re-equipped after a metal upgrade. It retains the prior starter's 18 base damage; wood mass is 1.8 kg. Metal-tier stats, crafting and recipes are unchanged.
- Wooden weapon state uses an explicit saved equipped-weapon sentinel and an owned inventory entry; it is not a new metal tier. Existing saved characters and gear are preserved. The initializer runs only for fresh characters; legacy whole-suit/weapon migration remains supported.
- Both male and female native rigs passed attachment, sampled movement/swing, no-armour masks and isolated on-disk save/reload checks in the actual rebuilt player. Editor checks also verified metal upgrade/re-equipping wood and legacy equipment preservation. All 20 sword exports passed geometry/UV/material round-trip checks after the finish update. Unity logic, production validation and Integrated build passed.
- An initial batch-mode roster test reached its screenshot stage but that screen-capture API returned null in batch mode. It was stopped and replaced with a dedicated graphics-independent runtime check, which passed. Real character saves were not used for these checks.

No Tripo requests or paid generation were made: **0 additional credits spent**. The last verified Developer API balance from the sword generation remains 220; it was not queried again for this local-only task.

Preview: `STARTER_PREVIEW.jpg`. Current sword finish: `SWORDS_MATTE_PREVIEW.jpg`. Actual character-with-armour previews were refreshed under `GreatswordAssetProduction/previews`. Validation receipts and build/runtime logs are in this folder. Before-change scripts and master Blender files are in `Backup`.

The ten generated metal swords remain a review library; their live weapon-factory activation was not part of this finish/starter update. The new wooden starter sword is connected to both held and sweep weapon paths in the playable game.
'''
(U/'README.md').write_text(readme,encoding='utf-8')
print('STARTER_UPDATE_DELIVERY_COMPLETE')
