import bpy,json,math,hashlib
from pathlib import Path
from mathutils import Matrix,Vector
from mathutils.bvhtree import BVHTree
R=Path(r'C:\The Veilbound\ArmourAssetProduction\Mastered');checks=[]
def fingerprint(o):
    geometry=([[round(v,6) for v in vertex.co] for vertex in o.data.vertices],[list(p.vertices) for p in o.data.polygons])
    return hashlib.sha256(json.dumps(geometry).encode()).hexdigest()
for variant in ['Male','Female']:
    bpy.ops.wm.open_mainfile(filepath=str(R/f'revisions/before_thigh_sleeve_removal/Master_{variant}.blend'))
    old={name:fingerprint(bpy.data.objects[name]) for name in ['Tin_Belt','Copper_Belt']}
    old_thigh={n:[bpy.data.objects[n].matrix_world@v.co for v in bpy.data.objects[n].data.vertices] for n in ['Tin_Thigh_L','Tin_Thigh_R','Copper_Thigh_L','Copper_Thigh_R']}
    bpy.ops.wm.open_mainfile(filepath=str(R/f'blender/Master_{variant}.blend'))
    assert len([o for o in bpy.context.scene.objects if '_Thigh_' in o.name])==4,'Expected restored thigh plates'
    for family in ['Tin','Copper']:
        details=[]
        for suffix,side in [('L','Left'),('R','Right')]:
            obj=bpy.data.objects[family+'_Thigh_'+suffix];current=[obj.matrix_world@v.co for v in obj.data.vertices];previous=old_thigh[obj.name]
            armature=next(o for o in bpy.context.scene.objects if o.type=='ARMATURE');hip=armature.matrix_world@armature.data.bones['mixamorig:'+side+'UpLeg'].head_local;knee=armature.matrix_world@armature.data.bones['mixamorig:'+side+'Leg'].head_local;axis=(knee-hip).normalized()
            radius=lambda p:((p-hip)-axis*(p-hip).dot(axis)).length
            ratio=max(radius(p) for p in current[:12])/max(radius(p) for p in previous[:12]);lower_delta=max((a-b).length for a,b in zip(current[36:48],previous[36:48]))
            assert .65<ratio<.67 and lower_delta<.00001
            details.append(dict(side=side,upper_cuff_radius_ratio=ratio,lower_ring_change_m=lower_delta))
        checks.append(dict(variant=variant,family=family,belt_geometry_unchanged=fingerprint(bpy.data.objects[family+'_Belt'])==old[family+'_Belt'],thigh_fit=details))
        assert len([o for o in bpy.context.scene.objects if o.get('tier')==family and o.get('equipment_slot')==6])==4
    arm=next(o for o in bpy.context.scene.objects if o.type=='ARMATURE')
    for clip,fractions in [('Idle',[0]),('Walk',[0,.25,.5,.75]),('Run',[0,.25,.5,.75])]:
        action=bpy.data.actions[clip];arm.animation_data.action=action;arm.animation_data.action_slot=action.slots[0];a,b=action.frame_range
        for fraction in fractions:
            t=a+(b-a)*fraction;bpy.context.scene.frame_set(math.floor(t),subframe=t-math.floor(t));deps=bpy.context.evaluated_depsgraph_get()
            def geometry(obj):
                ev=obj.evaluated_get(deps);mesh=ev.to_mesh();mesh.calc_loop_triangles();points=[obj.matrix_world@v.co for v in mesh.vertices];tris=[tuple(t.vertices) for t in mesh.loop_triangles];ev.to_mesh_clear();return points,BVHTree.FromPolygons(points,tris,all_triangles=True)
            for family in ['Tin','Copper']:
                points,belt=geometry(bpy.data.objects[family+'_Belt'])
                for side in ['L','R']:
                    points,leg=geometry(bpy.data.objects[family+'_Leg_'+side]);overlaps=len(belt.overlap(leg));nearest=min(belt.find_nearest(p)[3] for p in points)
                    checks.append(dict(variant=variant,family=family,side=side,clip=clip,fraction=fraction,waist_leg_intersection_pairs=overlaps,nearest_leg_vertex_to_waist_m=nearest))
(R/'qa/leg_clearance_verification.json').write_text(json.dumps(checks,indent=2))
passed=all(c.get('belt_geometry_unchanged',True) and c.get('waist_leg_intersection_pairs',0)==0 for c in checks)
print('LEG_CLEARANCE_CHECKS',len(checks),'PASS' if passed else 'FAIL');assert passed
