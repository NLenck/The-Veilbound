"""Reconcile actual task bills, verify immutable references, update production manifest."""
import csv,hashlib,json
from pathlib import Path
from PIL import Image,ImageDraw,ImageOps
ROOT=Path(r'C:\The Veilbound\EnemyAssetProduction')
REF=Path(r'C:\The Veilbound\Veilbound_40_Enemy_Reference_Pack')
cfg=json.loads((ROOT/'pipeline/batch_config.json').read_text())
rows=list(csv.DictReader((ROOT/'manifest.csv').open(newline='',encoding='utf-8')))
verification=json.loads((ROOT/'qa/export_verification.json').read_text())
assert len(verification)==6 and all(r['passed'] for r in verification)
balance=json.loads((ROOT/'logs/final_balance.json').read_text(encoding='utf-8-sig'))
notes={
'Husk':'Static review candidate. Lean humanoid, ragged skirt, red eyes and asymmetrical crystal shoulder/forearm/shin growths retained. Shoulder crystal base is rounder than reference; mouth and hands simplified. Open cloth/panels remain; repair deformation topology before rigging.',
'Wolf':'Static review candidate. Quadruped wolf proportions, paws, tail, red eyes and dorsal crystal ridge retained. Crystal bases and fur volumes are softer than reference. Check crystal density and muzzle against side reference; small disconnected panels remain.',
'Gargoyle':'Static review candidate. Crouched winged stone creature, claw tips, horns and red crystal crest retained. Wing membranes/hand proportions differ; rear feet do not convincingly contact the ground in generated pose. Back/tail crystals are simplified compared with the secondary view. Manual silhouette, wing and contact-pose cleanup recommended before acceptance.'}
ledger=[];audits={}
for row in rows:
    p=REF/row['reference_image']
    assert hashlib.sha256(p.read_bytes()).hexdigest()==row['reference_sha256'],f'Reference changed: {p}'
    row['reference_absolute_path']=str(p)
for job in cfg['jobs']:
    b,u=job['biome'],job['unit']
    result=json.loads((ROOT/f'logs/{u}_result.json').read_text(encoding='utf-8-sig').strip().splitlines()[-1])
    task=json.loads((ROOT/f'logs/{u}_task.json').read_text(encoding='utf-8-sig'))
    assert result['status']==task['status']=='success'
    assert result['credits_consumed']==task['credits_consumed']
    assert hashlib.sha256(Path(result['model_file']).read_bytes()).digest()==hashlib.sha256((ROOT/f'models_raw/{b}/{u}.glb').read_bytes()).digest()
    # Persist provenance without short-lived signed download URLs or upload tokens.
    record={k:task[k] for k in ['task_id','type','status','created_at','completed_at','credits_consumed'] if k in task}
    record['input']={k:v for k,v in task.get('input',{}).items() if k!='file'}
    record['hero_crop']=str(ROOT/f'inputs/{b}/{u}_hero.png')
    record['local_original_glb']=str(ROOT/f'models_raw/{b}/{u}.glb')
    record['model_sha256']=hashlib.sha256((ROOT/f'models_raw/{b}/{u}.glb').read_bytes()).hexdigest()
    (ROOT/f'logs/{u}_task.json').write_text(json.dumps(record,indent=2))
    (ROOT/f'models_raw/{b}/{u}_task.json').write_text(json.dumps(record,indent=2))
    ledger.append(dict(biome=b,unit=u,step='image_to_model',task_id=task['task_id'],credits=task['credits_consumed']))
    audit=json.loads((ROOT/f'qa/{b}/{u}_mesh_audit.json').read_text());audits[u]=audit
    final=audit['final']
    row=next(r for r in rows if r['biome']==b and r['unit']==u)
    row.update(generation_status='generated_review_required',task_id=task['task_id'],credits_used=task['credits_consumed'],
        raw_triangles=audit['raw']['triangles'],actual_triangles=final['triangles'],material_slots=final['material_slots'],
        dimensions_m=' x '.join(str(x) for x in final['dimensions_m']),raw_glb=record['local_original_glb'],
        blend_file=str(ROOT/f'models_blender/{b}/{u}.blend'),unity_fbx=str(ROOT/f'models_unity/{b}/{u}.fbx'),
        unity_glb=str(ROOT/f'models_unity/{b}/{u}.glb'),review_notes=notes[u],
        boundary_edges=final['boundary_edges'],nonmanifold_edges=final['nonmanifold_edges'],connected_components=final['connected_components'],
        missing_uv_faces=final['missing_uv_faces'],zero_uv_area_triangles=final['zero_uv_area_triangles'],
        export_roundtrip_pass=True,preview_front=audit['renders']['front'],preview_side=audit['renders']['side'],preview_three_quarter=audit['renders']['three_quarter'])
total=sum(x['credits'] for x in ledger)
assert total<=cfg['hard_credit_cap'] and len(ledger)==3
assert cfg['initial_balance']-total==balance['balance'] and balance['frozen']==0
keys=list(dict.fromkeys(k for r in rows for k in r))
with (ROOT/'manifest.csv').open('w',newline='',encoding='utf-8') as f:
    w=csv.DictWriter(f,fieldnames=keys);w.writeheader();w.writerows(rows)
(ROOT/'qa/credit_ledger.json').write_text(json.dumps(dict(initial_balance=1500,hard_cap=300,actual_total=total,
    remaining_balance=balance['balance'],frozen=balance['frozen'],tasks=ledger,reference_hashes_verified=40),indent=2))

# Compact visual review artifact, generated entirely from the Blender renders.
sheet=Image.new('RGB',(1200,1305),'#22262b');draw=ImageDraw.Draw(sheet)
for i,job in enumerate(cfg['jobs']):
    b,u=job['biome'],job['unit']
    for j,view in enumerate(['front','side','three_quarter']):
        with Image.open(ROOT/f'previews/{b}/{u}_{view}.png') as im:
            sheet.paste(im.resize((400,400)),(j*400,i*435+30))
        draw.text((j*400+10,i*435+8),f"{b}/{u} - {view} - {audits[u]['final']['triangles']:,} triangles",fill='white')
sheet.save(ROOT/'previews/FIRST_BATCH_CONTACT_SHEET.jpg',quality=92)
overview=Image.new('RGB',(1500,535),'#22262b');overview_draw=ImageDraw.Draw(overview)
for i,job in enumerate(cfg['jobs']):
    b,u=job['biome'],job['unit']
    with Image.open(ROOT/f'previews/{b}/{u}_three_quarter.png') as im:
        overview.paste(im.resize((500,500)),(i*500,35))
    overview_draw.text((i*500+12,12),f"{b}/{u} - {audits[u]['final']['triangles']:,} triangles",fill='white')
overview.save(ROOT/'previews/FIRST_BATCH_OVERVIEW.jpg',quality=93)
table='\n'.join(f"| {x['biome']}/{x['unit']} | {audits[x['unit']]['raw']['triangles']:,} | {audits[x['unit']]['final']['triangles']:,} | 1 | {x['credits']} |" for x in ledger)
costs='\n'.join(f"- image-to-model, `{x['task_id']}`, {x['biome']}/{x['unit']}: **{x['credits']} credits**." for x in ledger)
integrity='\n'.join(f"| {u} | {a['final']['boundary_edges']} | {a['final']['connected_components']} | {a['final']['nonmanifold_edges']} | {a['final']['missing_uv_faces']} | {a['final']['zero_uv_area_triangles']} |" for u,a in audits.items())
report=f'''# Veilbound — first enemy asset batch

Date: 27 October 2026 (Australia/Sydney). Status: **three static review candidates delivered; bulk generation stopped**.

Output root: `C:\\The Veilbound\\EnemyAssetProduction`.

## Results

| Enemy | Original triangles | Exported triangles | Materials | API credits |
|---|---:|---:|---:|---:|
{table}

Husk and Wolf fit the common range (1,500–3,500); Gargoyle fits the heavy/rare range (4,000–8,000). These are production assumptions, not newly assigned gameplay rarities. Actual triangle counts are measured in Blender and independently checked after FBX and GLB reimport. The service returned Wolf slightly above its requested 3,000 target, still within the intended range.

## Connection and spending

Official Codex Tripo skills with official `tripo-cli 0.7.2`; Node 24.19.0; international `ov` Developer API at `openapi.tripo3d.ai`. Device authorization succeeded. `tripo doctor` passed authentication and API reachability. This uses Developer API credits, separate from Studio; no Blender extension was needed for generation.

All three paid calls used `POST /v3/generation/image-to-model`, CLI alias `tripo-p1`, wire model `P1-20260311`, texture=true, pbr=false, and requested face limits 3,000 / 3,000 / 7,000. CLI dry runs passed. No conversion, texture refresh, AI retopology, rigging or animation API steps were added. No failed generations, repeated attempts, purchases or top-ups.

{costs}

**Total billed: {total} Developer API credits. Initial balance: 1,500. Final verified balance: {balance['balance']}; frozen: 0. Unused first-run allowance: {300-total}.** Three generations only; all other 37 remain `not_generated` in the production manifest. Free task reads and local Blender runs cost no credits.

## Reference inventory and cropping

Read the game README and pack README, CODEX_START_HERE and manifest. Verified all 40 manifest entries resolve to valid PNGs: ten each in Town, Forest, Shrine and Cave; no missing or extra PNGs. Recorded dimensions, file sizes and SHA-256 hashes. Viewed biome contact sheets and compared each test model to the full supplied reference sheet.

Hero crops are pixel-exact rectangular extractions, with no repainting, synthetic views or redesign. Coordinates are `[left, top, right, bottom]` in source pixels:

- Husk: [0, 0, 755, 1360].
- Wolf: [0, 300, 720, 1100].
- Gargoyle: [0, 0, 1122, 845]. Its existing title is retained to avoid cropping the highest wing tip.

Other sheet views were used for comparison, not supplied as unrelated extra objects. All 40 original image hashes were rechecked unchanged at completion. The reference-pack manifest was left untouched; production results are in the separate output manifest. Existing Unity Assets, Packages, ProjectSettings and scenes were not edited or opened for import.

## Blender pipeline

Blender 5.2.3 LTS, background Python. The reusable `pipeline/rebuild_local.ps1` rebuilds only the allowlisted three existing GLBs and performs export verification. It makes no Tripo calls, queues no bulk jobs and spends no credits. `prepare.py` refuses to reset an existing production manifest.

Processing imports a copy of the original GLB, preserves the UV map and base-colour atlas, welds coincident geometry at a 0.000001 m tolerance, removes loose/degenerate geometry if found, recalculates face normals and separates over-shared edges without deleting visible faces. Flat face shading preserves an angular presentation. No aggressive decimation or remeshing was used: these were generated directly at low-poly budgets, so no topology-changing reduction or texture rebake was necessary. A multi-material colour-atlas bake branch exists for later assets, but was not needed or exercised by this batch.

Each enemy has one mesh and one nonmetallic rough material, with its existing 2K colour texture, double-sided rendering for open cloth/wing panels, and packed textures in the .blend and GLB. FBX contains embedded texture media, with explicit PNG sidecars supplied too. Studio lights, camera and ground remain in a separate preview collection in the .blend and are excluded from engine exports. No armatures or animations exist in the exports.

Transforms are applied with bottom-centred pivots and a consistent forward direction. Blender dimensions are X width, Y depth, Z height; provisional heights are Husk 2.0 m, Wolf 1.25 m (including crystals), Gargoyle 2.4 m (including wings). These do not measure or modify existing gameplay prefabs. FBX is exported for Y-up engine import. Reimported dimensions match within 0.002 m.

## Integrity and export verification

All six FBX/GLB round trips passed triangle-count, material-count, UV-presence, texture-availability, no-armature and dimension checks. All three final meshes have zero degenerate faces, loose vertices, nonfinite coordinates, inconsistent-normal manifold edges, or UV coordinates outside 0–1.

| Enemy | Boundary edges | Connected components | Over-shared/loose edges | Faces missing UVs | Zero-area UV triangles |
|---|---:|---:|---:|---:|---:|
{integrity}

**Open boundaries and disconnected panels remain.** The generated geometry is suitable for static visual review, but is not a proven watertight or deformation-ready character mesh. Welding reduces the artificial splits from GLB normal/UV seams; separating over-shared edges avoids invalid edge sharing but does not create anatomical edge loops. Boundary counts include cloth, mouth/wing panels and crystal attachments. Do not infer animation readiness from a zero over-shared-edge count. Full self-intersection and UV-overlap proofs were not performed; UV area/presence/range checks and rendered texture review passed. The Unity editor was not used, so material appearance in the project's render pipeline and actual in-game scale still need a later authorized import review.

## Visual comparison and remaining work

### Town/Husk

{notes['Husk']}

### Forest/Wolf

{notes['Wolf']}

### Shrine/Gargoyle

{notes['Gargoyle']}

All keep the red-and-ashen palette and have no added weapons. Flat shading improves the faceted presentation, but cannot reproduce all of the concept's intentional planar crystal shapes. Crystal glow remains painted colour rather than a separately authored emission mask. These are review candidates, not an assertion of exact reconstruction or final art approval.

## Files

- `manifest.csv`: all 40 entries, three task IDs, actual credits/triangles, paths, hashes, QA and review notes.
- `models_raw/<Biome>/<Enemy>.glb`: byte-identical original generated model; task provenance alongside it. The CLI's original artifact directory is also retained.
- `models_blender/<Biome>/<Enemy>.blend`: cleaned static mesh, packed texture and preview studio.
- `models_unity/<Biome>/<Enemy>.fbx` and `.glb`, with texture sidecars.
- `previews/<Biome>/<Enemy>_front.png`, `_side.png`, `_three_quarter.png`, `_back.png`.
- `previews/FIRST_BATCH_CONTACT_SHEET.jpg`: nine-angle review sheet.
- `qa/`: per-enemy raw/final audits, six export checks, credit ledger and four inventory contact sheets.
- `logs/`: CLI result/progress records, sanitized task provenance, balance and Blender logs. Expiring download URLs are not used for rebuilding.
- `pipeline/`: source, crop configuration, local rebuild and verification scripts.

## Recommendations for the other 37 — not authorized or started

1. Review these three silhouettes first. Resolve Husk shoulder crystal faceting, Wolf crystal/fur softness, and Gargoyle wing/back design and grounded contact pose before scaling this approach. Do not use bulk generation to hide these issues.
2. Make controlled local edits to the existing meshes for sharper crystal planes, grounded contacts and clean deformation topology. Re-unwrap/bake only the edited regions when UVs must change; retain the untouched source GLB. Reserve more geometry around thin wings, fingers, antlers and weapons rather than reducing every surface equally.
3. At the observed 50 credits per textured P1 model, the other 37 would cost **1,850 credits**, exceeding the remaining 1,350 by **500**. The balance covers at most **27** further generations at this price before any retries or paid processing. Do not begin a 37-job batch with the current budget. No credits were purchased.
4. After explicit approval, use small reviewed batches with an actual-cost ledger and a fresh hard cap. Keep zero automatic retries and stop on ambiguous network submission rather than risk duplicate charges. Any change of model or texture strategy needs a separate comparison test and fresh estimate.
5. Assign production classes deliberately: common 1.5–3.5k, uncommon 3–5k, heavy/rare 4–8k, elite at most 12k triangles. The inventory does not establish gameplay rarity for all 40. Hand-check all crop boxes; sheets have different layouts and should not share one global crop.
6. Preserve species and props: Town Ghoul is a humanoid on all fours; Shrine Gargoyle replaces Stone Colossus; staff/axe/shield units must retain their supplied weapons. Treat gargoyle observation behavior as later gameplay scripting. Delay rigging/animation until silhouette and topology are approved.

**Stopped after three models. No remaining generation is queued.**
'''
(ROOT/'FIRST_BATCH_REPORT.md').write_text(report,encoding='utf-8')
print(json.dumps({'generated':3,'not_generated':37,'triangles':{u:a['final']['triangles'] for u,a in audits.items()},
                  'credits':total,'remaining':balance['balance'],'reference_hashes_verified':40,'export_checks_passed':6}))
