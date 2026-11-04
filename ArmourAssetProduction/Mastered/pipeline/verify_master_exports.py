import bpy,json,math
from pathlib import Path
R=Path(r'C:\The Veilbound\ArmourAssetProduction\Mastered');checks=[]
for path in sorted(list((R/'exports/fitted').rglob('*.fbx'))+list((R/'exports/fitted').rglob('*.glb'))):
    bpy.ops.wm.read_factory_settings(use_empty=True)
    if path.suffix=='.fbx':bpy.ops.import_scene.fbx(filepath=str(path))
    else:bpy.ops.import_scene.gltf(filepath=str(path))
    meshes=[o for o in bpy.context.scene.objects if o.type=='MESH' and not (o.name.startswith('Icosphere') and not o.data.materials)]
    is_body=path.stem.endswith('_BodyMaskSource');variant=path.parent.name if is_body else path.parent.parent.name
    audit=json.loads((R/f'qa/fitting_{variant}.json').read_text());triangles=0;invalid=0;unweighted=0;uvs=True;mask_ok=True;all_mask_faces=0
    for o in meshes:
        o.data.calc_loop_triangles();triangles+=len(o.data.loop_triangles);invalid+=sum(not all(math.isfinite(v) for v in vertex.co) for vertex in o.data.vertices);unweighted+=sum(not v.groups for v in o.data.vertices);uvs &= bool(o.data.uv_layers)
        if is_body:
            mask_ok &= len(o.data.uv_layers)>=2
            if len(o.data.uv_layers)>=2:
                uv=o.data.uv_layers[1].data
                for face in o.data.polygons:
                    any_bits,all_bits=[uv[face.loop_indices[0]].uv[k]*512 for k in [0,1]]
                    mask_ok &= abs(any_bits-round(any_bits))<.001 and abs(all_bits-round(all_bits))<.001
                    all_mask_faces+=int(all_bits>.5)
    if is_body:
        mask_audit=json.loads((R/f'qa/body_masks_{variant}.json').read_text());expected=mask_audit['faces'];mask_ok &= all_mask_faces==mask_audit.get('all_required_mask_faces',all_mask_faces)
    else:
        tier=path.parent.name;slot=path.stem.split('_')[-1]
        if slot=='AllSlots':expected=sum(o['triangles'] for o in audit['objects'] if o['tier']==tier)
        else:
            index=['Head','Shoulders','Chest','Hands','Belt','Bracers','Legs','Feet','Cape'].index(slot);expected=sum(o['triangles'] for o in audit['objects'] if o['tier']==tier and o['slot_index']==index)
    bones=sum(len(o.data.bones) for o in bpy.context.scene.objects if o.type=='ARMATURE')
    check=dict(file=str(path),triangles=triangles,expected=expected,bones=bones,nonfinite=invalid,unweighted=unweighted,uvs=uvs,mask_data_ok=mask_ok,all_required_mask_faces=all_mask_faces)
    check['passed']=triangles==expected and bones==52 and not invalid and not unweighted and uvs and mask_ok;checks.append(check)
(R/'qa/master_export_verification.json').write_text(json.dumps(checks,indent=2));print('MASTER_EXPORT_CHECKS',len(checks),sum(c['passed'] for c in checks));assert all(c['passed'] for c in checks)
