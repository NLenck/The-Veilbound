from pathlib import Path
from PIL import Image,ImageDraw
import json,hashlib
R=Path(r'C:\The Veilbound\ArmourAssetProduction')
tiers=['Tin','Copper','Bronze','Brass','Iron','Steel','Hardened Steel','Tungsten','Titanium','Veilcrystal']
# Explicit runtime indices: existing game defines Titanium before Tungsten.
runtime_ids=[100,101,102,103,104,105,106,108,107,109]
colors=[(.52,.58,.63),(.58,.23,.10),(.33,.19,.075),(.65,.49,.16),(.24,.28,.29),(.44,.49,.54),(.15,.19,.23),(.24,.23,.27),(.61,.66,.72),(.06,.025,.035),(.23,.025,.035),(.12,.07,.04),(.65,.49,.16),(1,.015,.005),(.2,.2,.2),(.5,.5,.5)]
folder=R/'exports/atlas';folder.mkdir(parents=True,exist_ok=True)
base=Image.new('RGB',(1024,64));orm=Image.new('RGB',(1024,64));b=ImageDraw.Draw(base);m=ImageDraw.Draw(orm)
for i,c in enumerate(colors):
    b.rectangle((i*64,0,(i+1)*64-1,63),fill=tuple(round(x*255) for x in c))
    rough=.8 if i==0 else .52 if i<10 else .9
    m.rectangle((i*64,0,(i+1)*64-1,63),fill=(255,round(rough*255),round((.8 if i<10 or i==12 else 0)*255)))
base.save(folder/'Plate_BaseColor.png');orm.save(folder/'Plate_ORM.png')
config=dict(root=str(R),model_alias='tripo-v3.1',wire_model='v3.1-20260211',starting_credits=300,
    authorised_paid_calls=1,prototype_estimated_credits=40,bulk_authorised=False,slots={'Head':0,'Shoulders':1,'Chest':2,'Hands':3,'Belt':4,'Bracers':5,'Legs':6,'Feet':7,'Cape':8},
    tiers=[dict(name=t,runtime_item_id=i,group='A' if n<5 else 'B',status='prototype_generated' if n<2 else 'not_generated') for n,(t,i) in enumerate(zip(tiers,runtime_ids))],
    model_parameters=dict(face_limit=16000,generate_parts=True,texture=False,pbr=False,model_seed=296059),
    atlas=dict(tiles=16,base_color=str(folder/'Plate_BaseColor.png'),orm=str(folder/'Plate_ORM.png'),cloth_tile=10,leather_tile=11,trim_tile=12,crystal_tile=13),
    fitting=dict(targets=['Male','Female'],source='PlayerAssetProduction/models_blender',status='draft_needs_motion_and_clipping_review'),
    cost_estimates=dict(parts_multiview=40,standard_texture_after_parts=10,semantic_segmentation=40,smart_segment_model=55,pbr_convert=5,blender_exports=0))
(R/'configs/armour_pipeline.json').write_text(json.dumps(config,indent=2))
for group in ['A','B']:
    tiers_in_group=tiers[:5] if group=='A' else tiers[5:]
    (R/f'configs/Group_{group}_PROPOSAL_ONLY.json').write_text(json.dumps(dict(submit=False,tiers=tiers_in_group,parameters=dict(face_limit=40000,generate_parts=True,texture=False,pbr=False),estimated_credits=40,reference_ready=group=='A',gate='Requires user approval after prototype report; five-set generation untested.'),indent=2))
inventory=[]
for p in (R/'references').rglob('*'):
    if p.is_file():inventory.append(dict(path=str(p),bytes=p.stat().st_size,sha256=hashlib.sha256(p.read_bytes()).hexdigest()))
(R/'qa/reference_inventory.json').write_text(json.dumps(inventory,indent=2))
print('PIPELINE_CONFIG_AND_SHARED_ATLAS_READY')
