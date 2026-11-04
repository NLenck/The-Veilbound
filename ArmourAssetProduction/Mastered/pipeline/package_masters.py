from pathlib import Path
from PIL import Image,ImageDraw
import json,shutil
S=Path(r'C:\The Veilbound\ArmourAssetProduction');R=S/'Mastered';BASE=Path(__file__).resolve().parent
names=['Tin','Copper','Bronze','Brass','Iron','Steel','HardenedSteel','Tungsten','Titanium','Veilcrystal'];ids=[100,101,102,103,104,105,106,108,107,109];families=[0,1,1,1,0,0,0,0,1,1]
colors=[(.52,.58,.63),(.58,.23,.10),(.33,.19,.075),(.65,.49,.16),(.24,.28,.29),(.44,.49,.54),(.15,.19,.23),(.24,.23,.27),(.61,.66,.72),(.50,.035,.055)]
accents=[(.32,.36,.39),(.68,.32,.14),(.56,.38,.14),(.75,.59,.22),(.15,.17,.19),(.60,.65,.7),(.34,.39,.44),(.4,.4,.43),(.65,.49,.16),(.24,.012,.02)]
cloth=json.loads((BASE/'tier_cloth_palettes.json').read_text())
shutil.copy2(BASE/'tier_cloth_palettes.json',R/'tier_cloth_palettes.json')
def rgb(h):return tuple(int(h[i:i+2],16)/255 for i in (1,3,5))
kit=R/'unity_staging/VeilboundPlateMasters'
for folder in ['Editor','Runtime','Models','Atlas']:(kit/folder).mkdir(parents=True,exist_ok=True)
for name,color,accent in zip(names,colors,accents):
    image=Image.new('RGB',(1024,64));draw=ImageDraw.Draw(image)
    for tile in range(16):
        c=color if tile<10 else (rgb(cloth[name]['cape']) if tile==10 else (.12,.07,.04) if tile==11 else accent if tile==12 else (.9,.01,.015) if tile==13 else rgb(cloth[name]['belt_cloth']) if tile==15 else (.018,.018,.02))
        draw.rectangle((tile*64,0,(tile+1)*64-1,63),fill=tuple(round(v*255) for v in c))
    image.save(R/f'atlas/{name}_BaseColor.png');shutil.copy2(R/f'atlas/{name}_BaseColor.png',kit/f'Atlas/{name}_BaseColor.png')
metal=Image.new('RGBA',(1024,64));draw=ImageDraw.Draw(metal)
for tile in range(16):draw.rectangle((tile*64,0,(tile+1)*64-1,63),fill=(204 if tile<10 or tile==12 else 0,0,0,125 if tile<10 or tile==12 else 25))
metal.save(R/'atlas/Plate_MetallicSmoothness.png');shutil.copy2(R/'atlas/Plate_MetallicSmoothness.png',kit/'Atlas/Plate_MetallicSmoothness.png')
for variant in ['Male','Female']:
    (kit/f'Models/{variant}').mkdir(exist_ok=True)
    shutil.copy2(R/f'exports/fitted/{variant}/{variant}_BodyMaskSource.fbx',kit/f'Models/{variant}/{variant}_BodyMaskSource.fbx')
    for family in ['Tin','Copper']:shutil.copy2(R/f'exports/fitted/{variant}/{family}/{family}_{variant}_AllSlots.fbx',kit/f'Models/{variant}/{family}_{variant}_AllSlots.fbx')
shutil.copy2(BASE/'MasterPlateEquipment.cs',kit/'Runtime/MasterPlateEquipment.cs');shutil.copy2(BASE/'BuildPlateMasters.cs',kit/'Editor/BuildPlateMasters.cs')
project=R/'UnityValidation';(project/'Assets').mkdir(parents=True,exist_ok=True)
shutil.copytree(S.parent/'PlayerAssetProduction/unity_staging/VeilbornePlayer',project/'Assets/VeilbornePlayer',dirs_exist_ok=True)
shutil.copytree(kit,project/'Assets/VeilboundPlateMasters',dirs_exist_ok=True)
(project/'Packages').mkdir(exist_ok=True);(project/'ProjectSettings').mkdir(exist_ok=True)
(project/'Packages/manifest.json').write_text(json.dumps({'dependencies':{'com.unity.modules.animation':'1.0.0','com.unity.modules.imageconversion':'1.0.0','com.unity.modules.imgui':'1.0.0','com.unity.modules.jsonserialize':'1.0.0','com.unity.modules.physics':'1.0.0'}},indent=2))
(project/'ProjectSettings/ProjectVersion.txt').write_text('m_EditorVersion: 6000.3.9f1\nm_EditorVersionWithRevision: 6000.3.9f1 (7a9955a4f2fa)\n')
(R/'tier_mapping.json').write_text(json.dumps([dict(tier=n,item_id=i,mesh_family=['Tin','Copper'][f],male_prefab=n+'_Male_Master',female_prefab=n+'_Female_Master',metal_color=list(c),cape_color=cloth[n]['cape'],belt_cloth_color=cloth[n]['belt_cloth'],cloth_description=cloth[n]['cloth_name']) for n,i,f,c in zip(names,ids,families,colors)],indent=2))
print('MASTER_REVIEW_PROJECT_READY',project)
