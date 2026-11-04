import csv,sys
from pathlib import Path
from PIL import Image,ImageDraw
ROOT=Path(r'C:\The Veilbound\EnemyAssetProduction')
rows=list(csv.DictReader((ROOT/'manifest.csv').open(newline='',encoding='utf-8')))
for b in ['Town','Shrine']:
    sheet=Image.new('RGB',(1800,800),'#24282d');draw=ImageDraw.Draw(sheet)
    for i,r in enumerate(x for x in rows if x['biome']==b):
        x=i%5*360;y=i//5*400
        path=ROOT/f"previews/{b}/{r['unit']}_front.png"
        if path.exists():
            with Image.open(path) as im:sheet.paste(im.resize((360,360)),(x,y+35))
        draw.text((x+10,y+10),r['unit'],fill='white')
    sheet.save(ROOT/f'qa/{b}_front_alignment.jpg',quality=94)
print('Alignment contact sheets saved.')
