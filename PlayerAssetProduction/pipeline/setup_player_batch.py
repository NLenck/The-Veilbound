"""Create a separate two-player production workspace; never modifies Unity Assets."""
import hashlib,json,shutil
from pathlib import Path
BASE=Path(__file__).resolve().parent
ROOT=Path(r'C:\The Veilbound\PlayerAssetProduction')
inventory=json.loads((BASE/'reference_inventory.json').read_text())
assert hashlib.sha256(Path(inventory['source']).read_bytes()).hexdigest()==inventory['source_sha256']
assert not (ROOT/'pipeline/player_batch.json').exists(),'Player batch exists; do not reset.'
for d in ['pipeline','references','inputs','models_raw','rigs_raw','animations_raw','models_blender','models_unity','previews','qa','logs','unity_staging']:
    (ROOT/d).mkdir(parents=True,exist_ok=True)
shutil.copy2(inventory['source'],ROOT/'references/Veilborne_Male_Female_Sheet.jpg')
shutil.copy2(BASE/'reference_inventory.json',ROOT/'references/reference_inventory_original.json')
for v in ['Male','Female']:
    shutil.copytree(BASE/f'reference_crops/{v}',ROOT/f'inputs/{v}',dirs_exist_ok=True)
cfg=dict(initial_balance=450,hard_credit_cap=210,automatic_retries=0,max_generation_attempts=2,
    source_sha256=inventory['source_sha256'],generation_model='v3.1-20260211',rig_model='v2.5-20260210',
    stages=dict(Generation=50,Rig=25,Animation=30),
    variants=[dict(name='Male',target_triangles=20000,height_m=1.85),dict(name='Female',target_triangles=20000,height_m=1.73)],
    animation_presets=['preset:idle','preset:walk','preset:run'],animation_in_place=True,
    rig_spec='mixamo',live_unity_project_modified=False)
(ROOT/'pipeline/player_batch.json').write_text(json.dumps(cfg,indent=2))
print(json.dumps(dict(output_root=str(ROOT),variants=2,hard_cap=210,expected_remaining=240)))
