import bpy,json
from pathlib import Path
from mathutils import Vector
R=Path(r'C:\The Veilbound\ArmourAssetProduction')
bpy.ops.wm.open_mainfile(filepath=str(R/'blender/Prototype_Organised_Exploded.blend'))
a=json.loads((R/'qa/prototype_components.json').read_text());lo,hi=map(Vector,a['bounds']);centre=(lo+hi)/2
camera=bpy.context.scene.camera;camera.location=centre+Vector((4,0,.05));camera.rotation_euler=(centre-camera.location).to_track_quat('-Z','Y').to_euler()
bpy.context.scene.render.filepath=str(R/'previews/PROTOTYPE_EXPLODED_COLOURED.png');bpy.ops.render.render(write_still=True)
