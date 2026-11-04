import bpy,json,math
from pathlib import Path
from mathutils import Vector,Matrix
from mathutils.bvhtree import BVHTree
R=Path(r'C:\The Veilbound\ArmourAssetProduction\Mastered');checks=[]
for variant in ['Male','Female']:
    bpy.ops.wm.open_mainfile(filepath=str(R/f'blender/Master_{variant}.blend'))
    arm=next(o for o in bpy.context.scene.objects if o.type=='ARMATURE');scale=1.73/1.85 if variant=='Female' else 1
    arm.animation_data.action=None
    for bone in arm.pose.bones:bone.matrix_basis=Matrix.Identity(4)
    bpy.context.view_layer.update()
    def head(side,part):return arm.matrix_world@arm.data.bones['mixamorig:'+side+part].head_local
    for family in ['Tin','Copper']:
        for side,suffix in [('Left','L'),('Right','R')]:
            obj=bpy.data.objects[family+'_Bracer_'+suffix];wrist=head(side,'Hand');elbow=head(side,'ForeArm');up=(elbow-wrist).normalized()
            points=[obj.matrix_world@v.co for v in obj.data.vertices];values=[(p-wrist).dot(up) for p in points]
            check=dict(variant=variant,family=family,side=side,wrist_extension_m=-min(values),elbow_extension_m=max(values)-(elbow-wrist).length)
            check['length_coverage_pass']=check['wrist_extension_m']>.01*scale and check['elbow_extension_m']>.01*scale
            checks.append(check)
    for clip,fractions in [('Idle',[0]),('Walk',[0,.25,.5,.75]),('Run',[0,.25,.5,.75])]:
        action=bpy.data.actions[clip];arm.animation_data.action=action;arm.animation_data.action_slot=action.slots[0];start,end=action.frame_range
        for fraction in fractions:
            frame=start+(end-start)*fraction;bpy.context.scene.frame_set(math.floor(frame),subframe=frame-math.floor(frame));deps=bpy.context.evaluated_depsgraph_get()
            for family in ['Tin','Copper']:
                for side,suffix in [('Left','L'),('Right','R')]:
                    wrist=arm.matrix_world@arm.pose.bones['mixamorig:'+side+'Hand'].head;elbow=arm.matrix_world@arm.pose.bones['mixamorig:'+side+'ForeArm'].head
                    def geometry(name):
                        obj=bpy.data.objects[family+'_'+name+'_'+suffix];ev=obj.evaluated_get(deps);mesh=ev.to_mesh();mesh.calc_loop_triangles();points=[obj.matrix_world@v.co for v in mesh.vertices];tris=[tuple(t.vertices) for t in mesh.loop_triangles];ev.to_mesh_clear();return points,BVHTree.FromPolygons(points,tris,all_triangles=True)
                    bracer_points,bracer_tree=geometry('Bracer');glove_points,glove_tree=geometry('Hand');upper_points,upper_tree=geometry('UpperArm');cop_points,cop_tree=geometry('Elbow')
                    cuff=[p for p in glove_points if (p-wrist).length<.09*scale]
                    wrist_gap=min(bracer_tree.find_nearest(p)[3] for p in cuff)
                    cop_to_bracer=min(bracer_tree.find_nearest(p)[3] for p in cop_points)
                    cop_to_upper=min(upper_tree.find_nearest(p)[3] for p in cop_points)
                    checks.append(dict(variant=variant,family=family,side=side,clip=clip,fraction=fraction,wrist_surface_gap_m=wrist_gap,elbow_bracer_surface_gap_m=cop_to_bracer,elbow_bicep_surface_gap_m=cop_to_upper,wrist_overlap_triangles=len(bracer_tree.overlap(glove_tree)),elbow_bracer_overlap_triangles=len(bracer_tree.overlap(cop_tree)),elbow_upper_overlap_triangles=len(upper_tree.overlap(cop_tree)),
                        joint_proximity_pass=max(wrist_gap,cop_to_bracer,cop_to_upper)<.025*scale))
(R/'qa/arm_connection_verification.json').write_text(json.dumps(checks,indent=2))
passed=all(c.get('length_coverage_pass',c.get('joint_proximity_pass',False)) for c in checks)
print('ARM_CONNECTION_CHECKS',len(checks),'PASS' if passed else 'FAIL');assert passed
