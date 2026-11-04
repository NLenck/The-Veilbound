"""Update the existing manifest and ledger incrementally; never submit API jobs."""
import csv,hashlib,json
from pathlib import Path
ROOT=Path(r'C:\The Veilbound\EnemyAssetProduction')
cfg=json.loads((ROOT/'pipeline/batch_config_town_shrine.json').read_text())
rows=list(csv.DictReader((ROOT/'manifest.csv').open(newline='',encoding='utf-8')))
ledger=[]
for j in cfg['jobs']:
    b,u=j['biome'],j['unit'];p=ROOT/f'logs/{u}_result.json'
    if not p.exists() or not p.read_text(encoding='utf-8-sig').strip():continue
    r=json.loads(p.read_text(encoding='utf-8-sig').strip().splitlines()[-1])
    row=next(x for x in rows if x['biome']==b and x['unit']==u)
    if r.get('status')!='success':
        row['generation_status']='attempt_failed_or_ambiguous'
        row['review_notes']=f"No retries. Inspect logs/{u}_result.json and submission marker before any further paid action."
        continue
    ledger.append(dict(biome=b,unit=u,step='image_to_model',task_id=r['task_id'],credits=r['credits_consumed']))
    row.update(generation_status='generated_pending_blender',task_id=r['task_id'],credits_used=r['credits_consumed'],
        raw_glb=str(ROOT/f'models_raw/{b}/{u}.glb'),requested_triangles=j['target_triangles'],
        production_class=j['production_class'],max_triangles=j['max_triangles'],review_notes=j['features']+' Static art review required.')
    source=ROOT/f'models_raw/{b}/{u}.glb'
    if source.exists():
        assert source.read_bytes()==Path(r['model_file']).read_bytes(),'Raw model changed'
        plan=json.loads((ROOT/f'logs/{u}_dry_run.json').read_text(encoding='utf-8-sig'))
        record=dict(task_id=r['task_id'],type=r['type'],status=r['status'],credits_consumed=r['credits_consumed'],
            model=plan['model'],parameters=plan['steps'][0]['payload'],local_original_glb=str(source),
            model_sha256=hashlib.sha256(source.read_bytes()).hexdigest(),hero_crop=j['input'])
        (ROOT/f'models_raw/{b}/{u}_task.json').write_text(json.dumps(record,indent=2))
    audit_path=ROOT/f'qa/{b}/{u}_mesh_audit.json'
    if audit_path.exists():
        a=json.loads(audit_path.read_text());f=a['final']
        row.update(generation_status='generated_review_required',raw_triangles=a['raw']['triangles'],
            actual_triangles=f['triangles'],material_slots=f['material_slots'],dimensions_m=' x '.join(map(str,f['dimensions_m'])),
            blend_file=str(ROOT/f'models_blender/{b}/{u}.blend'),unity_fbx=str(ROOT/f'models_unity/{b}/{u}.fbx'),
            unity_glb=str(ROOT/f'models_unity/{b}/{u}.glb'),boundary_edges=f['boundary_edges'],
            nonmanifold_edges=f['nonmanifold_edges'],connected_components=f['connected_components'],
            missing_uv_faces=f['missing_uv_faces'],zero_uv_area_triangles=f['zero_uv_area_triangles'],
            budget_pass=f['budget_pass'],preview_front=a['renders']['front'],preview_side=a['renders']['side'],preview_three_quarter=a['renders']['three_quarter'])
        if not f['budget_pass']:row['review_notes']+=' OVER TRIANGLE RANGE: local optimisation required.'
total=sum(x['credits'] for x in ledger)
assert total<=900 and len(ledger)<=18
keys=list(dict.fromkeys(k for r in rows for k in r))
with (ROOT/'manifest.csv').open('w',newline='',encoding='utf-8') as file:
    w=csv.DictWriter(file,fieldnames=keys);w.writeheader();w.writerows(rows)
(ROOT/'qa/credit_ledger_town_shrine.json').write_text(json.dumps(dict(initial_balance=1350,hard_cap=900,
    actual_total=total,expected_remaining_balance=1350-total,tasks=ledger),indent=2))
print(json.dumps(dict(generated=len(ledger),processed=sum(1 for j in cfg['jobs'] if (ROOT/f"qa/{j['biome']}/{j['unit']}_mesh_audit.json").exists()),credits=total,expected_remaining=1350-total)))
