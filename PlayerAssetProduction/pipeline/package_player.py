"""Assemble GIFs and a standalone Unity import kit/validation project. No API calls."""
import csv,json,hashlib,shutil
from pathlib import Path
from PIL import Image,ImageDraw
ROOT=Path(r'C:\The Veilbound\PlayerAssetProduction')
BASE=Path(__file__).resolve().parent
KIT=ROOT/'unity_staging/VeilbornePlayer'
for variant in ['Male','Female']:
    audit=json.loads((ROOT/f'qa/{variant}/player_audit.json').read_text())
    for kind,data in audit['motion_frames'].items():
        frames=[Image.open(p).convert('RGB') for p in data['paths']]
        frames[0].save(ROOT/f'previews/{variant}/{kind}.gif',save_all=True,append_images=frames[1:],duration=round(data['frame_duration_ms']),loop=0)
    folder=KIT/f'Models/{variant}';folder.mkdir(parents=True,exist_ok=True)
    for name in [variant+'_Rigged.fbx',variant+'_Locomotion.fbx']:
        shutil.copy2(ROOT/f'models_unity/{variant}/{name}',folder/name)
    shutil.copy2(ROOT/f'models_unity/{variant}/{variant}_Rigged.fbx',folder/f'{variant}_Humanoid.fbx')
    (folder/'textures').mkdir(exist_ok=True)
    for texture in audit['textures']:shutil.copy2(texture['path'],folder/'textures'/Path(texture['path']).name)
for sub in ['Runtime','Editor']:shutil.copytree(BASE/'unity_staging'/sub,KIT/sub,dirs_exist_ok=True)
for kind in ['Walk','Run']:
    audits=[json.loads((ROOT/f'qa/{v}/player_audit.json').read_text()) for v in ['Male','Female']]
    paths=[a['motion_frames'][kind]['paths'] for a in audits];count=max(map(len,paths));frames=[]
    for i in range(count):
        canvas=Image.new('RGB',(960,632),'#22262b');draw=ImageDraw.Draw(canvas)
        for j,v in enumerate(['Male','Female']):
            with Image.open(paths[j][i%len(paths[j])]) as im:canvas.paste(im.convert('RGB'),(j*480,32))
            draw.text((j*480+12,10),f'{v} - {kind} prototype',fill='white')
        frames.append(canvas)
    duration=round(audits[0]['motion_frames'][kind]['duration_seconds']/count*1000)
    frames[0].save(ROOT/f'previews/PLAYER_{kind.upper()}_REVIEW.gif',save_all=True,append_images=frames[1:],duration=duration,loop=0)
canvas=Image.new('RGB',(1200,790),'#22262b');draw=ImageDraw.Draw(canvas)
for i,v in enumerate(['Male','Female']):
    a=json.loads((ROOT/f'qa/{v}/player_audit.json').read_text())
    with Image.open(ROOT/f'previews/{v}/three_quarter.png') as im:canvas.paste(im.convert('RGB').resize((600,750)),(i*600,35))
    draw.text((i*600+14,10),f"{v} - {a['final_triangles']:,} triangles - {a['bone_count']} bones",fill='white')
canvas.save(ROOT/'previews/PLAYER_VARIANTS_OVERVIEW.jpg',quality=94)
heads=Image.new('RGB',(1200,635),'#22262b');draw=ImageDraw.Draw(heads)
for i,v in enumerate(['Male','Female']):
    with Image.open(ROOT/f'previews/{v}/head_detail.png') as im:heads.paste(im.convert('RGB').resize((600,600)),(i*600,35))
    draw.text((i*600+14,10),v+' - head detail',fill='white')
heads.save(ROOT/'previews/PLAYER_HEAD_DETAILS.jpg',quality=94)
validation=ROOT/'UnityValidation'
(validation/'Assets').mkdir(parents=True,exist_ok=True)
shutil.copytree(KIT,validation/'Assets/VeilbornePlayer',dirs_exist_ok=True)
(validation/'Packages').mkdir(exist_ok=True);(validation/'ProjectSettings').mkdir(exist_ok=True)
(validation/'Packages/manifest.json').write_text(json.dumps({'dependencies':{'com.unity.modules.animation':'1.0.0','com.unity.modules.imageconversion':'1.0.0','com.unity.modules.imgui':'1.0.0','com.unity.modules.jsonserialize':'1.0.0','com.unity.modules.physics':'1.0.0'}},indent=2))
(validation/'ProjectSettings/ProjectVersion.txt').write_text('m_EditorVersion: 6000.3.9f1\nm_EditorVersionWithRevision: 6000.3.9f1 (7a9955a4f2fa)\n')
print(json.dumps(dict(kit=str(KIT),validation_project=str(validation),variants=2,motion_review_gifs=2)))
