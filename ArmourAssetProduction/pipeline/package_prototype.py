from pathlib import Path
from PIL import Image,ImageDraw
import shutil,json
R=Path(r'C:\The Veilbound\ArmourAssetProduction');BASE=Path(__file__).resolve().parent
KIT=R/'unity_staging/ArmourPrototype'
for folder in ['Editor','Runtime','Models','Atlas']:(KIT/folder).mkdir(parents=True,exist_ok=True)
shutil.copy2(BASE/'ArmourPrototypeReview.cs',KIT/'Editor/ArmourPrototypeReview.cs');shutil.copy2(BASE/'ModularPlateSlots.cs',KIT/'Runtime/ModularPlateSlots.cs')
for p in (R/'exports/atlas').glob('*.png'):shutil.copy2(p,KIT/'Atlas'/p.name)
orm=Image.open(R/'exports/atlas/Plate_ORM.png').convert('RGB');metal=Image.new('RGBA',orm.size)
metal.putdata([(b,0,0,255-g) for r,g,b in orm.getdata()]);metal.save(KIT/'Atlas/Plate_MetallicSmoothness.png')
for variant in ['Male','Female']:
    (KIT/'Models'/variant).mkdir(exist_ok=True)
    for tier in ['Tin','Copper']:shutil.copy2(R/f'exports/fitted/{variant}/{tier}/{tier}_{variant}_AllSlots.fbx',KIT/f'Models/{variant}/{tier}_{variant}_AllSlots.fbx')
project=R/'UnityValidation';(project/'Assets').mkdir(parents=True,exist_ok=True)
shutil.copytree(R.parent/'PlayerAssetProduction/unity_staging/VeilbornePlayer',project/'Assets/VeilbornePlayer',dirs_exist_ok=True)
shutil.copytree(KIT,project/'Assets/ArmourPrototype',dirs_exist_ok=True)
(project/'Packages').mkdir(exist_ok=True);(project/'ProjectSettings').mkdir(exist_ok=True)
(project/'Packages/manifest.json').write_text(json.dumps({'dependencies':{'com.unity.modules.animation':'1.0.0','com.unity.modules.imageconversion':'1.0.0','com.unity.modules.imgui':'1.0.0','com.unity.modules.jsonserialize':'1.0.0','com.unity.modules.physics':'1.0.0'}},indent=2))
(project/'ProjectSettings/ProjectVersion.txt').write_text('m_EditorVersion: 6000.3.9f1\nm_EditorVersionWithRevision: 6000.3.9f1 (7a9955a4f2fa)\n')
canvas=Image.new('RGB',(1400,980),'#242830');draw=ImageDraw.Draw(canvas)
for i,(variant,tier) in enumerate([('Male','Tin'),('Female','Tin'),('Male','Copper'),('Female','Copper')]):
    with Image.open(R/f'previews/{tier}_{variant}_front.png') as im:canvas.paste(im.convert('RGB').resize((350,450)),((i%2)*700+175,(i//2)*490+30))
    draw.text(((i%2)*700+175,(i//2)*490+10),f'{tier} / {variant} - fitting draft: clipping unresolved',fill='white')
canvas.save(R/'previews/PROTOTYPE_FITTING_ISSUES.jpg',quality=94)
print('ISOLATED_UNITY_PROJECT_READY',project)
