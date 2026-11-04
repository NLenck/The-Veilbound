"""Reimport standalone FBX and GLB exports and check the deliverables without Unity."""
import bpy,json,math,sys
from pathlib import Path
from mathutils import Vector
ROOT=Path(r'C:\The Veilbound\EnemyAssetProduction')
args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
config_name=args[0] if args else 'batch_config.json'
cfg=json.loads((ROOT/'pipeline'/config_name).read_text())
results=[]
for job in cfg['jobs']:
    b,u=job['biome'],job['unit']
    expected=json.loads((ROOT/f'qa/{b}/{u}_mesh_audit.json').read_text())['final']
    for ext in ['fbx','glb']:
        path=ROOT/f'models_unity/{b}/{u}.{ext}'
        bpy.ops.wm.read_factory_settings(use_empty=True)
        if ext=='fbx':bpy.ops.import_scene.fbx(filepath=str(path))
        else:bpy.ops.import_scene.gltf(filepath=str(path))
        objects=[o for o in bpy.context.scene.objects if o.type=='MESH']
        triangles=0;points=[];materials=set();missing_uv=0
        for o in objects:
            o.data.calc_loop_triangles();triangles+=len(o.data.loop_triangles)
            points += [o.matrix_world@v.co for v in o.data.vertices]
            materials.update(m.name for m in o.data.materials if m)
            if not o.data.uv_layers:missing_uv+=1
        dims=[max(p[i] for p in points)-min(p[i] for p in points) for i in range(3)]
        images=[i for i in bpy.data.images if i.type=='IMAGE']
        textures_ok=bool(images) and all(i.packed_file or Path(bpy.path.abspath(i.filepath)).is_file() for i in images)
        dims_ok=all(abs(a-b)<.002 for a,b in zip(dims,expected['dimensions_m']))
        rig_count=sum(o.type=='ARMATURE' for o in bpy.context.scene.objects)
        passed=triangles==expected['triangles'] and dims_ok and len(materials)==1 and missing_uv==0 and textures_ok and rig_count==0
        results.append(dict(biome=b,unit=u,format=ext,triangles=triangles,dimensions_m=dims,
                            materials=len(materials),missing_uv_meshes=missing_uv,textures_resolve=textures_ok,
                            armatures=rig_count,passed=passed,path=str(path)))
report_name='export_verification_town_shrine.json' if config_name=='batch_config_town_shrine.json' else 'export_verification.json'
(ROOT/'qa'/report_name).write_text(json.dumps(results,indent=2))
print('EXPORT_VERIFICATION '+json.dumps(results))
assert all(r['passed'] for r in results),'Export verification failed; inspect qa/export_verification.json'
