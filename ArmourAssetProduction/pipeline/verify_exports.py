import bpy,json,math
from pathlib import Path
R=Path(r'C:\The Veilbound\ArmourAssetProduction');c=json.loads((R/'qa/classification.json').read_text());results=[]
for path in sorted(list((R/'exports').rglob('*.fbx'))+list((R/'exports').rglob('*.glb'))):
    bpy.ops.wm.read_factory_settings(use_empty=True)
    if path.suffix=='.fbx':bpy.ops.import_scene.fbx(filepath=str(path))
    else:bpy.ops.import_scene.gltf(filepath=str(path))
    meshes=[o for o in bpy.context.scene.objects if o.type=='MESH' and not (o.name.startswith('Icosphere') and not o.data.materials)]
    triangles=0;bad=0;unweighted=0;uv=True
    fitted='fitted' in path.parts
    for o in meshes:
        o.data.calc_loop_triangles();triangles+=len(o.data.loop_triangles)
        bad+=sum(not all(math.isfinite(x) for x in v.co) for v in o.data.vertices)
        unweighted+=sum(not v.groups for v in o.data.vertices) if fitted else 0
        uv &= bool(o.data.uv_layers)
    bones=sum(len(o.data.bones) for o in bpy.context.scene.objects if o.type=='ARMATURE')
    tier=path.parent.name
    if fitted:
        variant=path.parent.parent.name;a=json.loads((R/f'qa/fitting_{variant}.json').read_text())
        slot=path.stem.split('_')[-1]
        if slot=='AllSlots':expected=sum(r['triangles'] for r in a['objects'] if r['tier']==tier)
        else:
            index=['Head','Shoulders','Chest','Hands','Belt','Bracers','Legs','Feet','Cape'].index(slot)
            expected=sum(r['triangles'] for r in a['objects'] if r['tier']==tier and r['slot_index']==index)
    else:
        expected=sum(r['export_triangles'] for r in c['objects'] if r['tier']==tier) if path.stem.endswith('_Exploded') else next(r['export_triangles'] for r in c['objects'] if r['name']==path.stem)
    record=dict(file=str(path),triangles=triangles,expected_triangles=expected,bones=bones,nonfinite_vertices=bad,unweighted_vertices=unweighted,uv_present=uv)
    record['passed']=triangles==expected and bad==0 and uv and unweighted==0 and (bones==52 if fitted else bones==0)
    results.append(record)
(R/'qa/export_verification.json').write_text(json.dumps(results,indent=2));print('EXPORT_VERIFICATION',len(results),'files',sum(r['passed'] for r in results),'passed');assert all(r['passed'] for r in results)
