import csv,hashlib,json,shutil
from pathlib import Path
from PIL import Image,ImageOps,ImageDraw
ROOT=Path(r'C:\The Veilbound\EnemyAssetProduction')
REF=Path(r'C:\The Veilbound\Veilbound_40_Enemy_Reference_Pack')
# Individually inspected pixel crops; generation classes are provisional production budgets.
data=[
('Town','Lobber',(0,0,735,1340),4000,5000,2.1,'uncommon','Raised crystal projectile, lean humanoid, shoulder growths and ragged skirt.'),
('Town','Brute',(0,0,1122,895),6500,8000,2.6,'heavy','Huge asymmetrical crystal forearm, hunched shoulders, muscular humanoid.'),
('Town','Ghoul',(0,0,1122,860),3200,3500,1.4,'common','Human face and long human arms on all fours; not a dog; dorsal crystals.'),
('Town','Burster',(0,0,780,1380),4500,5000,2.3,'uncommon','Bloated humanoid with exposed red crystal belly and crystal hands.'),
('Town','Shrieker',(0,0,725,1380),3200,3500,2.2,'common','Long neck, open screaming mouth, thin humanoid and crystal forearms.'),
('Town','Bulwark',(0,0,765,1380),6500,8000,2.6,'heavy','Broad hunched torso and heavy asymmetrical red crystal arm; no added shield.'),
('Town','Impaler',(0,0,735,1380),4500,5000,2.3,'uncommon','Very long red crystal blades growing from both forearms, spiked head.'),
('Town','Ravager',(0,0,750,1380),4500,5000,2.2,'uncommon','Long red crystal claws and head/back crest on lean humanoid.'),
('Town','Conduit',(0,0,760,1380),6500,8000,2.7,'heavy','Branching crystal staff, tall robed humanoid and oversized crystal arm.'),
('Shrine','Acolyte',(0,50,750,1240),3200,3500,2.0,'common','Hooded faceless armoured figure, crystal crown, eyes and single red sword.'),
('Shrine','Cinder_Priest',(0,0,1122,805),4500,5000,2.5,'uncommon','Hood, long angular robes and a tall three-pronged red crystal staff.'),
('Shrine','Ash_Wraith',(0,0,710,1220),4500,5000,2.3,'uncommon','Floating shredded robe, hollow hood, red crystal shoulder shards and long claws.'),
('Shrine','Shrine_Guardian',(0,0,1122,879),7000,8000,2.7,'heavy','Armoured guardian carrying a long axe and large red-inlaid shield.'),
('Shrine','Blade_Sentinel',(0,0,1122,790),6500,8000,2.6,'heavy','Slender armoured humanoid with two long red crystal swords and crystal crest.'),
('Shrine','Ritual_Orb',(0,0,1122,850),4500,5000,1.8,'uncommon','Open floating stone ring, central star and many detached red crystals; no humanoid.'),
('Shrine','Lamenter',(0,0,740,1250),4500,5000,2.5,'uncommon','Elongated floating hooded wraith, red eyes, long claws and ragged robe.'),
('Shrine','Veil_Angel',(0,0,1122,890),10500,12000,3.0,'elite','Wide layered feather-like stone wings, crystal crown, two red forearm blades and pointed floating lower body.'),
('Shrine','High_Priest',(0,0,740,1285),8500,12000,3.0,'elite','Tall crowned masked priest, ornate robes, cape and red crystal staff.')]
config_path=ROOT/'pipeline/batch_config_town_shrine.json'
assert not config_path.exists(),'Batch already prepared; do not reset.'
rows=list(csv.DictReader((ROOT/'manifest.csv').open(newline='',encoding='utf-8')))
for r in rows:
    p=REF/r['reference_image']
    assert hashlib.sha256(p.read_bytes()).hexdigest()==r['reference_sha256']
shutil.copy2(ROOT/'manifest.csv',ROOT/'qa/manifest_before_town_shrine.csv')
jobs=[]
for b,u,c,t,m,h,category,features in data:
    row=next(r for r in rows if r['biome']==b and r['unit']==u)
    assert row['generation_status']=='not_generated'
    crop=ROOT/f'inputs/{b}/{u}_hero.png';crop.parent.mkdir(exist_ok=True)
    with Image.open(REF/row['reference_image']) as im:
        assert im.size==(1122,1402)
        im.crop(c).save(crop)
    jobs.append(dict(biome=b,unit=u,crop=list(c),target_triangles=t,max_triangles=m,height_m=h,
        production_class=category,features=features,estimated_credits=50,input=str(crop),yaw_deg=-180 if u=='Shrieker' else -90,
        remove_flat_x_boundary_debris=u=='Ritual_Orb'))
cfg=dict(batch='Town_Shrine',initial_balance=1350,hard_credit_cap=900,max_generations=18,
    auto_retries=0,rigging=False,animation=False,reference_root=str(REF),output_root=str(ROOT),jobs=jobs)
config_path.write_text(json.dumps(cfg,indent=2))
for b in ['Town','Shrine']:
    sheet=Image.new('RGB',(1200,1200),'#25282d');draw=ImageDraw.Draw(sheet)
    for i,j in enumerate(j for j in jobs if j['biome']==b):
        with Image.open(j['input']) as im:
            thumb=ImageOps.contain(im.convert('RGB'),(390,365))
        x=i%3*400;y=i//3*400
        sheet.paste(thumb,(x+(400-thumb.width)//2,y+30));draw.text((x+8,y+8),j['unit'],fill='white')
    sheet.save(ROOT/f'qa/{b}_batch2_hero_crops.jpg',quality=94)
print(json.dumps(dict(prepared=len(jobs),estimated_credits=900,remaining_after_estimate=450)))
