from pathlib import Path
import json,hashlib,shutil
from PIL import Image,ImageDraw
R=Path(r'C:\The Veilbound\GreatswordAssetProduction');P=Path(r'C:\The Veilbound\Veilborne_Greatsword_Tripo_Pack');records=[]
for p in P.rglob('*'):
 if p.is_file():records.append(dict(path=str(p.relative_to(P)),bytes=p.stat().st_size,sha256=hashlib.sha256(p.read_bytes()).hexdigest()))
(R/'qa/reference_inventory.json').write_text(json.dumps(records,indent=2))
names=['Tin','Copper','Bronze','Brass','Iron','Steel','HardenedSteel','Tungsten','Titanium','Veilcrystal']
main=['#777F86','#AD5934','#88603A','#BB8D39','#45474B','#BDC4CD','#62666F','#34383D','#D3D6DC','#170E13']
trim=['#888D91','#C67543','#B8873F','#D4AD5B','#55585D','#D5D8DB','#8A8C91','#565A60','#BD9349','#2C1118']
wrap=['#392D25','#41241E','#39241C','#4B1F24','#201E20','#23344E','#25242A','#1D1C1F','#1F4767','#201017']
base=Image.new('RGB',(1024,640));metal=Image.new('RGBA',base.size);emission=Image.new('RGB',base.size);bd=ImageDraw.Draw(base);md=ImageDraw.Draw(metal);ed=ImageDraw.Draw(emission);rows=[]
def rgb(h):return tuple(bytes.fromhex(h[1:]))
for i,name in enumerate(names):
 c=rgb(main[i]);tr=rgb(trim[i]);wr=rgb(wrap[i]);pal=[c,tuple(round(v*.78) for v in c),tuple(min(255,round(v*1.12)) for v in c),tuple(round(v*.50) for v in c),tr,wr,(31,106,177) if name=='Titanium' else (185,8,26),(9,8,11),tuple(round(v*.6) for v in wr),(115,4,14)]+[c]*6
 for tile,color in enumerate(pal):
  rect=(tile*64,i*64,tile*64+63,i*64+63);bd.rectangle(rect,fill=color);cloth=tile in [5,8];gem=tile in [6,9];md.rectangle(rect,fill=(0 if cloth or gem else 205,0,0,40 if cloth else 140 if gem else 115));ed.rectangle(rect,fill=(175,3,12) if name=='Veilcrystal' and gem else (0,0,0))
 rows.append(dict(name=name,metal_index=8 if name=='Tungsten' else 7 if name=='Titanium' else i,atlas_row=i,main_color=main[i],trim_color=trim[i],wrap_color=wrap[i]))
(R/'atlas').mkdir(exist_ok=True);base.save(R/'atlas/Greatswords_BaseColor.png');metal.save(R/'atlas/Greatswords_MetallicSmoothness.png');emission.save(R/'atlas/Greatswords_Emission.png');(R/'sword_palette.json').write_text(json.dumps(rows,indent=2))
print('REFERENCES_INVENTORIED_AND_SHARED_ATLAS_PREPARED',len(records))
