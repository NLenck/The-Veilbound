"""Pixel-exact working crops from the supplied player sheet. No API calls."""
import hashlib,json
from pathlib import Path
from PIL import Image
BASE=Path(__file__).resolve().parent
SOURCE=Path(r'C:\Users\nicho\.codex\.chatgpt-projects\g-p-69858d55a73881919e10e84ceee7ffb4\.codex-remote-attachments\01a17bd7-3920-7270-b092-0875ce03218d\b65cb1f7-95f9-4531-a821-f7a0dde715f8\1-2956.jpg')
BOXES={
 'Male':{'front':(176,34,335,423),'left':(350,34,451,423),'back':(467,34,605,423),'right':(626,34,735,423),'head_detail':(795,58,1062,372),'eye_detail':(1077,56,1256,182),'skin_detail':(1077,214,1256,390)},
 'Female':{'front':(170,441,329,815),'left':(352,439,448,815),'back':(466,439,602,815),'right':(630,439,740,815),'head_detail':(796,448,1063,774),'eye_detail':(1077,448,1256,610),'skin_detail':(1077,617,1256,804)}}
inventory={'source':str(SOURCE),'source_sha256':hashlib.sha256(SOURCE.read_bytes()).hexdigest(),'crops':{},'api_submissions':0}
with Image.open(SOURCE) as im:
    assert im.size==(1280,853)
    for variant,views in BOXES.items():
        folder=BASE/'reference_crops'/variant;folder.mkdir(parents=True,exist_ok=True)
        inventory['crops'][variant]={}
        for view,box in views.items():
            p=folder/f'{variant}_{view}.png';im.crop(box).save(p)
            inventory['crops'][variant][view]={'path':str(p),'crop':box,'generation_view':view in ['front','left','back','right']}
(BASE/'reference_inventory.json').write_text(json.dumps(inventory,indent=2))
print(json.dumps({'variants_prepared':2,'crops':14,'api_submissions':0,'source_unchanged':hashlib.sha256(SOURCE.read_bytes()).hexdigest()==inventory['source_sha256']}))
