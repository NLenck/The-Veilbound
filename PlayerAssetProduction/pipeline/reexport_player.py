"""Consolidate the skinned details and use FBX unit metadata for Unity-safe scaling."""
import bpy,json
from pathlib import Path
from mathutils import Matrix
ROOT=Path(r'C:\The Veilbound\PlayerAssetProduction')
for variant in ['Male','Female']:
    bpy.ops.wm.open_mainfile(filepath=str(ROOT/f'models_blender/{variant}/{variant}.blend'))
    arm=next(o for o in bpy.context.scene.objects if o.type=='ARMATURE')
    meshes=[o for o in bpy.context.scene.objects if o.type=='MESH' and any(m.type=='ARMATURE' for m in o.modifiers)]
    body=max(meshes,key=lambda o:len(o.data.vertices))
    root=bpy.data.objects['Veilborne_'+variant]
    arm.animation_data.action=None
    for pb in arm.pose.bones:pb.matrix_basis=Matrix.Identity(4)
    bpy.context.view_layer.update();bpy.ops.object.select_all(action='DESELECT')
    for o in meshes:o.select_set(True)
    bpy.context.view_layer.objects.active=body
    if len(meshes)>1:bpy.ops.object.join()
    audit_path=ROOT/f'qa/{variant}/player_audit.json'
    previous=json.loads(audit_path.read_text())
    if not previous.get('unit_transforms_baked',False):
        # Bake the shared uniform normalization into mesh and rest bones.
        # Local bone rotation keys are invariant under a shared rigid transform;
        # local translation keys need the same uniform scale as the rest bones.
        bpy.context.view_layer.update()
        arm_world=arm.matrix_world.copy();mesh_world=body.matrix_world.copy()
        unit_scale=arm_world.to_scale().x
        body.data.transform(mesh_world);arm.data.transform(arm_world)
        root.matrix_world=Matrix.Identity(4)
        arm.matrix_parent_inverse=Matrix.Identity(4);arm.matrix_world=Matrix.Identity(4)
        body.matrix_parent_inverse=Matrix.Identity(4);body.matrix_world=Matrix.Identity(4)
        for action in bpy.data.actions:
            for layer in action.layers:
                for strip in layer.strips:
                    for bag in strip.channelbags:
                        for curve in bag.fcurves:
                            if curve.data_path.endswith('.location'):
                                for key in curve.keyframe_points:
                                    key.co.y*=unit_scale;key.handle_left.y*=unit_scale;key.handle_right.y*=unit_scale
        bpy.context.view_layer.update()
    bpy.context.scene.unit_settings.system='METRIC';bpy.context.scene.unit_settings.scale_length=1.0
    actions={name:bpy.data.actions[name] for name in ['Idle','Walk','Run']}
    def select():
        bpy.ops.object.select_all(action='DESELECT')
        for o in [root,arm,body]:o.select_set(True)
        bpy.context.view_layer.objects.active=arm
    def activate(name):
        action=actions[name];arm.animation_data.action=action
        if action.slots:arm.animation_data.action_slot=action.slots[0]
        a,b=action.frame_range;bpy.context.scene.frame_start=round(a);bpy.context.scene.frame_end=round(b);bpy.context.scene.frame_set(round(a))
    def export(name,animated,all_actions=False):
        select();bpy.ops.export_scene.fbx(filepath=str(ROOT/f'models_unity/{variant}/{name}.fbx'),use_selection=True,
            object_types={'EMPTY','ARMATURE','MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,
            apply_scale_options='FBX_SCALE_UNITS',add_leaf_bones=False,mesh_smooth_type='FACE',
            bake_anim=animated,bake_anim_use_all_actions=all_actions,bake_anim_use_nla_strips=False,
            bake_anim_simplify_factor=0,path_mode='COPY',embed_textures=True)
    export(variant+'_Rigged',False)
    for name in actions:activate(name);export(name,True)
    activate('Idle');export(variant+'_Locomotion',True,True)
    select();bpy.ops.export_scene.gltf(filepath=str(ROOT/f'models_unity/{variant}/{variant}_Locomotion.glb'),
        export_format='GLB',use_selection=True,export_animations=True,export_animation_mode='ACTIONS',export_anim_slide_to_zero=True)
    select();bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/f'models_blender/{variant}/{variant}.blend'))
    p=ROOT/f'qa/{variant}/player_audit.json';audit=json.loads(p.read_text())
    audit['skinned_mesh_count']=1;audit['fbx_scale_mode']='FBX_SCALE_UNITS';audit['skin_details_consolidated']=True
    audit['unit_transforms_baked']=True
    p.write_text(json.dumps(audit,indent=2))
    print('REEXPORT_COMPLETE',variant)
