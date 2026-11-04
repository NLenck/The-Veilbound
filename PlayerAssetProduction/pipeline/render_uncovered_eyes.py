import bpy,sys
from pathlib import Path
from mathutils import Vector
ROOT=Path(r'C:\The Veilbound\PlayerAssetProduction')
variant=sys.argv[sys.argv.index('--')+1]
bpy.ops.wm.open_mainfile(filepath=str(ROOT/f'models_blender/{variant}/{variant}.blend'))
for mat in bpy.data.materials:
    if mat.name in ['Veil_Red_Emission','Iris_Dark_Centre']:
        mat.node_tree.nodes.clear();n=mat.node_tree.nodes.new('ShaderNodeBsdfTransparent');out=mat.node_tree.nodes.new('ShaderNodeOutputMaterial');mat.node_tree.links.new(n.outputs[0],out.inputs['Surface'])
arm=next(o for o in bpy.context.scene.objects if o.type=='ARMATURE');arm.animation_data.action=bpy.data.actions['Idle']
if arm.animation_data.action.slots:arm.animation_data.action_slot=arm.animation_data.action.slots[0]
bpy.context.scene.frame_set(1)
H=1.85 if variant=='Male' else 1.73
scene=bpy.context.scene;camera=scene.camera
camera.location=Vector((0,-2,H*.89));target=Vector((0,0,H*.89));camera.rotation_euler=(target-camera.location).to_track_quat('-Z','Y').to_euler()
camera.data.type='ORTHO';camera.data.ortho_scale=H*.32
scene.render.resolution_x=1200;scene.render.resolution_y=1200;scene.render.resolution_percentage=100
scene.render.engine='CYCLES';scene.cycles.samples=24
scene.render.filepath=str(ROOT/f'qa/{variant}/uncovered_eyes.png');bpy.ops.render.render(write_still=True)
