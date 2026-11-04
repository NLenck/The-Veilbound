"""Final batch report and visual sheets. Requires complete local audits and export checks."""
import csv,hashlib,json
from pathlib import Path
from PIL import Image,ImageDraw
ROOT=Path(r'C:\The Veilbound\EnemyAssetProduction')
REF=Path(r'C:\The Veilbound\Veilbound_40_Enemy_Reference_Pack')
cfg=json.loads((ROOT/'pipeline/batch_config_town_shrine.json').read_text())
ledger=json.loads((ROOT/'qa/credit_ledger_town_shrine.json').read_text())
checks=json.loads((ROOT/'qa/export_verification_town_shrine.json').read_text())
balance=json.loads((ROOT/'logs/town_shrine_final_balance.json').read_text(encoding='utf-8-sig'))
reviews=json.loads((ROOT/'qa/visual_review_town_shrine.json').read_text())
assert len(ledger['tasks'])==18 and ledger['actual_total']<=900
assert len(checks)==36 and all(c['passed'] for c in checks)
assert balance['balance']==1350-ledger['actual_total'] and balance['frozen']==0
rows=list(csv.DictReader((ROOT/'manifest.csv').open(newline='',encoding='utf-8')))
for r in rows:
    assert hashlib.sha256((REF/r['reference_image']).read_bytes()).hexdigest()==r['reference_sha256']
    if r['biome'] in ['Town','Shrine']:
        assert r['generation_status']=='generated_review_required'
        if r['unit'] in reviews:r['review_notes']=reviews[r['unit']]
        r['export_roundtrip_pass']=True
with (ROOT/'manifest.csv').open('w',newline='',encoding='utf-8') as f:
    w=csv.DictWriter(f,fieldnames=list(rows[0]));w.writeheader();w.writerows(rows)
audits={r['unit']:json.loads((ROOT/f"qa/{r['biome']}/{r['unit']}_mesh_audit.json").read_text()) for r in rows if r['biome'] in ['Town','Shrine']}
for b in ['Town','Shrine']:
    units=[r for r in rows if r['biome']==b]
    sheet=Image.new('RGB',(1800,800),'#24282d');draw=ImageDraw.Draw(sheet)
    for i,r in enumerate(units):
        x=i%5*360;y=i//5*400
        with Image.open(ROOT/f"previews/{b}/{r['unit']}_three_quarter.png") as im:sheet.paste(im.resize((360,360)),(x,y+35))
        draw.text((x+10,y+10),f"{r['unit']} - {int(r['actual_triangles']):,} triangles",fill='white')
    sheet.save(ROOT/f'previews/{b}_COMPLETE_CONTACT_SHEET.jpg',quality=94)
    # One full-size row per enemy: front, side, and three-quarter.
    detailed=Image.new('RGB',(1200,4300),'#24282d');dd=ImageDraw.Draw(detailed)
    for i,r in enumerate(units):
        for k,view in enumerate(['front','side','three_quarter']):
            with Image.open(ROOT/f"previews/{b}/{r['unit']}_{view}.png") as im:detailed.paste(im.resize((400,400)),(k*400,i*430+28))
            dd.text((k*400+8,i*430+8),f"{r['unit']} / {view}",fill='white')
    detailed.save(ROOT/f'previews/{b}_FULL_REVIEW_SHEET.jpg',quality=92)
table='\n'.join(f"| {r['biome']}/{r['unit']} | {r['actual_triangles']} | {r['material_slots']} | {'reused sample' if r['unit'] in ['Husk','Gargoyle'] else r['credits_used']} |" for r in rows if r['biome'] in ['Town','Shrine'])
costs='\n'.join(f"| {t['biome']}/{t['unit']} | image_to_model | `{t['task_id']}` | {t['credits']} |" for t in ledger['tasks'])
qa='\n'.join(f"| {u} | {a['final']['boundary_edges']} | {a['final']['connected_components']} | {a['final']['nonmanifold_edges']} | {a['final']['inconsistent_normal_edges']} | {a['final']['zero_uv_area_triangles']} |" for u,a in audits.items())
visual='\n\n'.join(f"**{u}:** {reviews[u]}" for u in reviews)
over=[u for u,a in audits.items() if not a['final']['budget_pass']]
report=f'''# Veilbound — Town and Shrine production batch

Date: 27 October 2026 (Australia/Sydney).

**All 20 Town and Shrine static enemy assets are present:** 18 newly generated models plus reused Husk and Gargoyle. Forest Wolf remains from the test batch; the other 19 Forest/Cave enemies were not generated. No rigging, animation or Unity integration was performed.

## Scope and cost

The user approved proceeding with village/Town and Shrine after reviewing the three-model test. This is a separate continuation batch; the original first-run 300-credit cap and report remain historical records. New batch hard cap: 900 Developer API credits. Beginning balance: 1,350. No purchases, top-ups, duplicate sample generations or automatic retries.

All 18 CLI dry runs passed with no warnings. One paid `POST /v3/generation/image-to-model` call per new enemy: CLI alias `tripo-p1`, actual model `P1-20260311`, texture=true, pbr=false. Each used the individually cropped hero view with its original reference artwork. Other sheet views guided visual checking. Requested triangle caps and provisional height/class choices are in `pipeline/batch_config_town_shrine.json`. Classification guides art budgets and does not assign gameplay rarity.

**New batch actual spend: {ledger['actual_total']} credits. Final API balance: {balance['balance']}; frozen: 0. Lifetime pipeline spend: {150+ledger['actual_total']} of the original 1,500 Developer API credits.** Local Blender work and free balance reads used no credits.

| Enemy | Actual exported triangles | Materials | New batch credits |
|---|---:|---:|---:|
{table}

## Itemized paid-task ledger

| Enemy | Step | Task ID | Actual credits |
|---|---|---|---:|
{costs}

Total for the above tasks: **{ledger['actual_total']} credits**. Reused Husk and Gargoyle cost zero in this batch; their original 50-credit task records remain in `FIRST_BATCH_REPORT.md` and the original ledger.

## Blender processing and saved files

Each new enemy includes `models_raw/<Biome>/<Unit>.glb`, a byte-identical original generated GLB with SHA-256 provenance; `models_blender/<Biome>/<Unit>.blend`; `models_unity/<Biome>/<Unit>.fbx` and `.glb`; PNG colour-texture sidecars; and front, side, three-quarter and back 1000-pixel preview renders. All original CLI artifacts are retained too.

Blender 5.2.3 LTS applies bottom-centred metric transforms and forward-axis normalization, micrometre welding of coincident GLB vertices while retaining UV corner data, normal recalculation and flat shading. It separates over-shared edges without deleting silhouette faces. Geometry was generated directly within low-poly production budgets; no aggressive decimation was used. Textures and UVs were preserved; no paid retopology or retexture steps were submitted. One rough, nonmetallic material per enemy is used where delivered; actual counts are in the table. No multi-material atlas bake was required if all counts are one.

Ritual Orb received a targeted local debris cleanup after inspecting connected components: {audits['Ritual_Orb']['final'].get('removed_debris_components',0)} tiny flat boundary components / {audits['Ritual_Orb']['final'].get('removed_debris_faces',0)} faces removed. The open ring and volumetric crystal motif were preserved. Shrieker received an extra axis correction. Later preview framing uses per-view projected mesh bounds to keep long weapons and wing tips within the image.

Saved .blend files include a separate preview studio excluded from mesh-only engine exports. Flat shading adds faceting, but is not a substitute for an artist rebuilding softened crystal shapes. Dimensions are provisional asset scales including crowns, staffs, wings or floating shards. Final in-game sizes, floating pivots and material/shader treatment should be reviewed on a later import.

## Validation

All 40 source image SHA-256 hashes match the initial inventory. Reference pack images and its manifest were left unchanged. Existing Unity Assets, scenes, Packages and ProjectSettings were not modified. The original first-batch report and credit ledger were preserved.

All **36 new FBX/GLB round trips** passed actual triangle count, material count, UV presence, texture availability, no armatures and dimensions within 0.002 m. The four existing Husk/Gargoyle export checks also passed in the first batch. Triangle-range exceptions: **{', '.join(over) if over else 'none'}**.

| Enemy | Boundary edges | Components | Over-shared/loose edges | Inconsistent-normal edges | Zero-area UV triangles |
|---|---:|---:|---:|---:|---:|
{qa}

Open cloth/wing panels, mouth openings and detached crystals remain. Disconnected geometry is expected for floating crystal motifs but needs examination before skinning. Zero over-shared edges does not prove a watertight or deformation-ready mesh. Full UV-overlap and self-intersection proof is outside these checks. These are **static review candidates**; no animation-ready topology or final art approval is claimed. Unity render-pipeline appearance and gameplay silhouettes have not been tested inside the untouched game project.

## Visual review against the supplied references

{visual}

The sample-specific limitations for Husk/Gargoyle remain documented in `FIRST_BATCH_REPORT.md`. Do not silently substitute these new meshes into gameplay before reviewing their silhouettes and later deformation requirements.

## Review artifacts and reusable local commands

- `previews/Town_COMPLETE_CONTACT_SHEET.jpg` and `Shrine_COMPLETE_CONTACT_SHEET.jpg`: all ten enemies in each biome.
- `previews/Town_FULL_REVIEW_SHEET.jpg` and `Shrine_FULL_REVIEW_SHEET.jpg`: front, side and three-quarter comparisons for all 20.
- `manifest.csv`: all 40 enemies with actual costs, triangles, dimensions, paths and per-enemy notes. Nineteen Forest/Cave entries remain ungenerated.
- `qa/credit_ledger_town_shrine.json`, `qa/export_verification_town_shrine.json` and per-unit mesh audits.
- `qa/visual_review_town_shrine.json`: explicit visual findings for each new model.
- `pipeline/process_batch2_one.ps1 -Unit <Name>`: local cleanup/export/render for one existing raw model; costs no API credits.
- `pipeline/run_batch2_one.ps1`: per-submission balance/cap reconciliation and submission markers. It refuses any previously attempted unit; do not remove markers to retry. Nothing is queued automatically.

## Next production work

Review weapons, thin fingers, cloth panels and detached crystals at gameplay camera distance, then correct the flagged silhouette details locally. Preserve every raw source. When topology changes are required for deformation, retopologize around joints and bake the existing colour texture onto the changed UVs. Keep red-crystal emission masks as a separate authoring decision; painted red colour currently carries most glow information.

The remaining {balance['balance']} credits cover at most {int(balance['balance']//50)} further textured P1 generations at the observed 50-credit price, with no reserve for paid retries or mesh operations. No Forest/Cave generation or additional paid processing is authorized by this batch and none is queued.
'''
(ROOT/'TOWN_SHRINE_BATCH_REPORT.md').write_text(report,encoding='utf-8')
print(json.dumps(dict(town_models=10,shrine_models=10,new_models=18,new_credits=ledger['actual_total'],remaining=balance['balance'],export_checks=36,unchanged_references=40,triangle_exceptions=over)))
