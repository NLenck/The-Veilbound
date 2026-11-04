import bpy,json,sys,math
from pathlib import Path
ROOT=Path(r'C:\The Veilbound\PlayerAssetProduction')
results=[]
for variant in ['Male','Female']:
    audit=json.loads((ROOT/f'qa/{variant}/player_audit.json').read_text())
    for name in [variant+'_Rigged.fbx',variant+'_Locomotion.fbx',variant+'_Locomotion.glb']:
        bpy.ops.wm.read_factory_settings(use_empty=True)
        path=ROOT/f'models_unity/{variant}/{name}'
        if path.suffix=='.fbx':bpy.ops.import_scene.fbx(filepath=str(path))
        else:bpy.ops.import_scene.gltf(filepath=str(path))
        # Blender's glTF importer creates an unmaterialed 80-triangle bone display widget.
        # It is not a mesh encoded in the GLB and is excluded from character validation.
        meshes=[o for o in bpy.context.scene.objects if o.type=='MESH' and not (o.name.startswith('Icosphere') and not o.data.materials and len(o.data.polygons)==80)]
        armatures=[o for o in bpy.context.scene.objects if o.type=='ARMATURE']
        triangles=0;unweighted=0;nonfinite=0
        for obj in meshes:
            obj.data.calc_loop_triangles();triangles+=len(obj.data.loop_triangles)
            unweighted+=sum(not v.groups or sum(g.weight for g in v.groups)<1e-6 for v in obj.data.vertices)
            nonfinite+=sum(not all(math.isfinite(x) for x in v.co) for v in obj.data.vertices)
        action_names=[a.name for a in bpy.data.actions]
        animations_ok=name.endswith('_Rigged.fbx') or all(any(clip.lower() in a.lower() for a in action_names) for clip in ['Idle','Walk','Run'])
        textures=[i for i in bpy.data.images if i.type=='IMAGE']
        textures_ok=bool(textures) and all(i.packed_file or Path(bpy.path.abspath(i.filepath)).exists() for i in textures)
        bone_count=sum(len(a.data.bones) for a in armatures)
        result=dict(variant=variant,file=str(path),triangles=triangles,bone_count=bone_count,actions=action_names,
                    animations_ok=animations_ok,unweighted_vertices=unweighted,nonfinite_vertices=nonfinite,textures_resolve=textures_ok)
        result['passed']=triangles==audit['final_triangles'] and bone_count==audit['bone_count'] and animations_ok and unweighted==0 and nonfinite==0 and textures_ok
        results.append(result)
(ROOT/'qa/player_export_verification.json').write_text(json.dumps(results,indent=2))
print('PLAYER_EXPORTS '+json.dumps(results))
assert all(r['passed'] for r in results),'A player export check failed.'
