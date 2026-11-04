"""Read-only reference inventory and pixel-exact hero crops; never touches Unity assets."""
import csv, hashlib, json
from pathlib import Path
from PIL import Image, ImageOps, ImageDraw

ROOT = Path(r'C:\The Veilbound\EnemyAssetProduction')
REF = Path(r'C:\The Veilbound\Veilbound_40_Enemy_Reference_Pack')
TESTS = [('Town', 'Husk', (0, 0, 755, 1360), 3000, 2.0),
         ('Forest', 'Wolf', (0, 300, 720, 1100), 3000, 1.25),
         ('Shrine', 'Gargoyle', (0, 0, 1122, 845), 7000, 2.4)]

def main():
    if (ROOT/'manifest.csv').exists():
        raise SystemExit('Inventory already exists. Refusing to reset production status; use rebuild_local.ps1 for existing models.')
    ROOT.mkdir(parents=True, exist_ok=True)
    for d in ['pipeline', 'inputs', 'models_raw', 'models_blender', 'models_unity', 'previews', 'qa', 'logs']:
        (ROOT/d).mkdir(exist_ok=True)
    rows = list(csv.DictReader((REF/'manifest.csv').open(newline='', encoding='utf-8-sig')))
    assert len(rows) == 40
    actual = set((REF/'references').rglob('*.png'))
    assert actual == {REF/r['reference_image'] for r in rows}
    for row in rows:
        path = REF/row['reference_image']
        with Image.open(path) as im:
            im.verify()
        with Image.open(path) as im:
            row.update(width=im.width, height=im.height, image_mode=im.mode)
        row.update(reference_sha256=hashlib.sha256(path.read_bytes()).hexdigest(), reference_bytes=path.stat().st_size,
                   task_id='', credits_used='', raw_triangles='', actual_triangles='', material_slots='',
                   dimensions_m='', raw_glb='', blend_file='', unity_fbx='', unity_glb='', review_notes='Not generated; bulk approval required')
    for biome, unit, box, budget, height in TESTS:
        path = REF/f'references/{biome}/{unit}.png'
        with Image.open(path) as im:
            assert im.size == (1122,1402) if unit != 'Wolf' else im.size == (1086,1448)
            dst = ROOT/f'inputs/{biome}'
            dst.mkdir(exist_ok=True)
            im.crop(box).save(dst/f'{unit}_hero.png')
        row = next(r for r in rows if r['biome']==biome and r['unit']==unit)
        row['review_notes'] = f'Hero pixel crop {box}; target {budget} triangles; provisional height {height} m; awaiting generation'
    with (ROOT/'manifest.csv').open('w',newline='',encoding='utf-8') as f:
        w=csv.DictWriter(f,fieldnames=list(rows[0]));w.writeheader();w.writerows(rows)
    config = {'reference_root':str(REF),'output_root':str(ROOT),'initial_balance':1500,'hard_credit_cap':300,
              'max_generations':3,'auto_retries':0,'rigging':False,'animation':False,
              'jobs':[dict(biome=b,unit=u,crop=list(c),target_triangles=t,height_m=h) for b,u,c,t,h in TESTS]}
    (ROOT/'pipeline/batch_config.json').write_text(json.dumps(config,indent=2))
    for biome in ['Town','Forest','Shrine','Cave']:
        canvas=Image.new('RGB',(1000,650),'#242424');draw=ImageDraw.Draw(canvas)
        for i,row in enumerate(r for r in rows if r['biome']==biome):
            with Image.open(REF/row['reference_image']) as im:
                thumb=ImageOps.contain(im.convert('RGB'),(190,285))
                x=(i%5)*200;y=(i//5)*325
                canvas.paste(thumb,(x,y+25));draw.text((x+5,y+5),row['unit'],fill='white')
        canvas.save(ROOT/f'qa/{biome}_inventory.jpg')
    print(json.dumps({'inventoried':len(rows),'crops':3,'output':str(ROOT)}))

if __name__=='__main__': main()
