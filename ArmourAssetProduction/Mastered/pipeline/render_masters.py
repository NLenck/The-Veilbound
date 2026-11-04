import bpy,json,math,sys
from pathlib import Path
from mathutils import Vector
R=Path(r'C:\The Veilbound\ArmourAssetProduction\Mastered');variant=sys.argv[sys.argv.index('--')+1]
bpy.ops.wm.open_mainfile(filepath=str(R/f'blender/Master_{variant}.blend'))
tiers=json.loads((R/'tier_mapping.json').read_text());materials={}
for tier in tiers:
    material=bpy.data.materials.new('Plate_'+tier['tier']);material.use_nodes=True;material.use_fake_user=True
    bs=material.node_tree.nodes.get('Principled BSDF');base=material.node_tree.nodes.new('ShaderNodeTexImage');base.image=bpy.data.images.load(str(R/f'atlas/{tier["tier"]}_BaseColor.png'),check_existing=True);base.image.reload();base.interpolation='Closest';material.node_tree.links.new(base.outputs['Color'],bs.inputs['Base Color'])
    orm=material.node_tree.nodes.new('ShaderNodeTexImage');orm.image=bpy.data.images.load(str(R/'atlas/Plate_MetallicSmoothness.png'),check_existing=True);orm.image.reload();orm.image.colorspace_settings.name='Non-Color';orm.interpolation='Closest'
    separate=material.node_tree.nodes.new('ShaderNodeSeparateColor');material.node_tree.links.new(orm.outputs['Color'],separate.inputs[0]);material.node_tree.links.new(separate.outputs['Red'],bs.inputs['Metallic'])
    inverse=material.node_tree.nodes.new('ShaderNodeMath');inverse.operation='SUBTRACT';inverse.inputs[0].default_value=1;multiply=material.node_tree.nodes.new('ShaderNodeMath');multiply.operation='MULTIPLY';multiply.inputs[1].default_value=.45;material.node_tree.links.new(orm.outputs['Alpha'],multiply.inputs[0]);material.node_tree.links.new(multiply.outputs[0],inverse.inputs[1]);material.node_tree.links.new(inverse.outputs[0],bs.inputs['Roughness']);materials[tier['tier']]=material
gear=[o for o in bpy.context.scene.objects if o.type=='MESH' and o.get('tier') in ['Tin','Copper']]
arm=next(o for o in bpy.context.scene.objects if o.type=='ARMATURE');arm.animation_data.action=None
from mathutils import Matrix
for b in arm.pose.bones:b.matrix_basis=Matrix.Identity(4)
bpy.context.view_layer.update()
scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=12;scene.render.resolution_x=600;scene.render.resolution_y=800;scene.render.resolution_percentage=100
height=1.73 if variant=='Female' else 1.85;at=Vector((0,0,height*.52));camera=scene.camera;camera.data.type='ORTHO';camera.data.ortho_scale=height*1.22
camera.location=at+Vector((.3,-1,.07)).normalized()*5;camera.rotation_euler=(at-camera.location).to_track_quat('-Z','Y').to_euler()
for tier in tiers:
    if '--tier' in sys.argv and tier['tier']!=sys.argv[sys.argv.index('--tier')+1]:continue
    for o in gear:
        o.hide_render=o['tier']!=tier['mesh_family']
        if not o.hide_render:o.data.materials.clear();o.data.materials.append(materials[tier['tier']])
    camera.location=at+Vector((.3,-1,.07)).normalized()*5;camera.rotation_euler=(at-camera.location).to_track_quat('-Z','Y').to_euler()
    scene.render.filepath=str(R/f'previews/{tier["tier"]}_{variant}_Master.png');bpy.ops.render.render(write_still=True)
    camera.location=at+Vector((.3,1,.07)).normalized()*5;camera.rotation_euler=(at-camera.location).to_track_quat('-Z','Y').to_euler()
    scene.render.filepath=str(R/f'previews/{tier["tier"]}_{variant}_Back.png');bpy.ops.render.render(write_still=True)
    bpy.ops.object.select_all(action='DESELECT');arm.select_set(True)
    root=bpy.data.objects.get('Veilborne_'+variant)
    if root:root.select_set(True)
    for obj in gear:obj.select_set(obj['tier']==tier['mesh_family'])
    bpy.context.view_layer.objects.active=arm
    output=R/f'exports/tiers/{variant}/{tier["tier"]}_{variant}.glb';output.parent.mkdir(parents=True,exist_ok=True)
    bpy.ops.export_scene.gltf(filepath=str(output),export_format='GLB',use_selection=True,export_animations=False)

camera.location=at+Vector((.3,-1,.07)).normalized()*5;camera.rotation_euler=(at-camera.location).to_track_quat('-Z','Y').to_euler()
# Motion previews test the same masked body and shared mesh bases.
scene.cycles.samples=4;scene.render.resolution_x=320;scene.render.resolution_y=440
for family in ([] if '--no-motion' in sys.argv else ['Tin','Copper']):
    for o in gear:
        o.hide_render=o['tier']!=family
        if not o.hide_render:o.data.materials.clear();o.data.materials.append(materials[family])
    for clip in ['Walk','Run']:
        action=bpy.data.actions[clip];arm.animation_data.action=action;arm.animation_data.action_slot=action.slots[0];a,b=action.frame_range
        for i in range(12):
            t=a+(b-a)*i/12;scene.frame_set(math.floor(t),subframe=t-math.floor(t));scene.render.filepath=str(R/f'previews/motion/{family}_{variant}_{clip}/{i:03d}.png');bpy.ops.render.render(write_still=True)
arm.animation_data.action=bpy.data.actions['Idle'];arm.animation_data.action_slot=arm.animation_data.action.slots[0];scene.frame_set(1)
for o in gear:
    o.hide_render=o['tier']!='Tin';o.data.materials.clear();o.data.materials.append(materials[o['tier']])
bpy.ops.wm.save_as_mainfile(filepath=str(R/f'blender/Master_{variant}.blend'))
print('MASTER_PALETTE_AND_MOTION_PREVIEWS',variant)
