import bpy,json,math,sys
from pathlib import Path
from mathutils import Vector
ROOT=Path(r'C:\The Veilbound\PlayerAssetProduction')
variant=sys.argv[sys.argv.index('--')+1]
bpy.ops.wm.open_mainfile(filepath=str(ROOT/f'models_blender/{variant}/{variant}.blend'))
audit=json.loads((ROOT/f'qa/{variant}/player_audit.json').read_text());H=audit['height_m']
scene=bpy.context.scene;camera=scene.camera;arm=next(o for o in scene.objects if o.type=='ARMATURE')
def activate(name):
    action=bpy.data.actions[name];arm.animation_data.action=action
    if action.slots:arm.animation_data.action_slot=action.slots[0]
    scene.frame_set(1)
def aim(at):camera.rotation_euler=(Vector(at)-camera.location).to_track_quat('-Z','Y').to_euler()
target=Vector((0,0,H*.51));activate('Idle')
scene.render.engine='CYCLES';scene.cycles.samples=20;scene.render.resolution_percentage=100
scene.render.resolution_x=800;scene.render.resolution_y=1000;camera.data.ortho_scale=H*1.16
for view,direction in [('front',(0,-1,.08)),('side',(1,0,.08)),('three_quarter',(.65,-1,.12)),('back',(0,1,.08))]:
    camera.location=target+Vector(direction).normalized()*H*3;aim(target)
    scene.render.filepath=str(ROOT/f'previews/{variant}/{view}.png');bpy.ops.render.render(write_still=True)
camera.location=Vector((0,-2,H*.89));aim(Vector((0,0,H*.89)));camera.data.ortho_scale=H*.32
scene.render.resolution_x=800;scene.render.resolution_y=800
scene.render.filepath=str(ROOT/f'previews/{variant}/head_detail.png');bpy.ops.render.render(write_still=True)
scene.cycles.samples=6;scene.render.resolution_x=480;scene.render.resolution_y=600;camera.data.ortho_scale=H*1.18
camera.location=target+Vector((.55,-1,.10)).normalized()*H*3;aim(target)
for kind,data in audit['motion_frames'].items():
    activate(kind);a,b=bpy.data.actions[kind].frame_range
    for i,path in enumerate(data['paths']):
        t=a+(b-a)*i/len(data['paths']);scene.frame_set(math.floor(t),subframe=t-math.floor(t))
        scene.render.filepath=path;bpy.ops.render.render(write_still=True)
print('REVISION_PREVIEWS_COMPLETE',variant)
