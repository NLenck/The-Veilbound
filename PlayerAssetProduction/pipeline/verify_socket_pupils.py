import bpy,bmesh,json,math
from pathlib import Path
from mathutils import Vector
ROOT=Path(r'C:\The Veilbound\PlayerAssetProduction');results=[]
for variant in ['Male','Female']:
    bpy.ops.wm.open_mainfile(filepath=str(ROOT/f'models_blender/{variant}/{variant}.blend'))
    a=json.loads((ROOT/f'qa/{variant}/player_audit.json').read_text());H=a['height_m']
    arm=next(o for o in bpy.context.scene.objects if o.type=='ARMATURE');arm.animation_data.action=bpy.data.actions['Idle']
    arm.animation_data.action_slot=arm.animation_data.action.slots[0];bpy.context.scene.frame_set(1);bpy.context.view_layer.update()
    obj=next(o for o in bpy.context.scene.objects if o.type=='MESH' and any(m.type=='ARMATURE' for m in o.modifiers))
    mat=next(i for i,m in enumerate(obj.data.materials) if m.name=='Socket_Pupil_Emission')
    ids=sorted({i for p in obj.data.polygons if p.material_index==mat for i in p.vertices})
    ev=obj.evaluated_get(bpy.context.evaluated_depsgraph_get());mesh=ev.to_mesh();errors=[]
    for i,landmark in zip([ids[0],ids[21]],a['socket_pupils']):
        p=obj.matrix_world@mesh.vertices[i].co
        px=1200*(p.x/(H*.32)+.5);py=1200*(.5-(p.z-H*.89)/(H*.32))
        errors.append(math.dist((px,py),landmark['render_pixel']))
    ev.to_mesh_clear()
    bm=bmesh.new();bm.from_mesh(obj.data)
    bmesh.ops.delete(bm,geom=[f for f in bm.faces if f.material_index!=0],context='FACES')
    bmesh.ops.remove_doubles(bm,verts=list(bm.verts),dist=1e-6)
    boundary=sum(e.is_boundary for e in bm.edges);nonmanifold=sum(not e.is_manifold for e in bm.edges if e.link_faces)
    degenerate=sum(f.calc_area()<1e-12 for f in bm.faces);bm.free()
    result=dict(variant=variant,pupil_count=len(ids)//21,landmark_error_pixels=errors,
        old_overlay_materials_present=any(m.name in ['Veil_Red_Emission','Iris_Dark_Centre'] for m in obj.data.materials),
        pupil_front_normals=all(p.normal.y<0 for p in obj.data.polygons if p.material_index==mat),
        base_boundary_edges_after_weld=boundary,base_nonmanifold_edges_after_weld=nonmanifold,base_degenerate_faces=degenerate)
    result['passed']=len(ids)==42 and max(errors)<1 and not result['old_overlay_materials_present'] and result['pupil_front_normals']
    results.append(result)
(ROOT/'qa/socket_pupil_verification.json').write_text(json.dumps(results,indent=2))
print('SOCKET_QA',json.dumps(results));assert all(r['passed'] for r in results)
