import bpy,json
from pathlib import Path
from mathutils import Matrix,Vector
R=Path(r'C:\The Veilbound\ArmourAssetProduction\Mastered');checks=[]
for variant in ['Male','Female']:
    bpy.ops.wm.open_mainfile(filepath=str(R/f'blender/Master_{variant}.blend'))
    arm=next(o for o in bpy.context.scene.objects if o.type=='ARMATURE');arm.animation_data.action=None
    for bone in arm.pose.bones:bone.matrix_basis=Matrix.Identity(4)
    bpy.context.view_layer.update();scale=1.73/1.85 if variant=='Female' else 1
    for obj in bpy.context.scene.objects:
        if obj.type!='MESH' or '_Shoulder_' not in obj.name:continue
        points=[obj.matrix_world@v.co for v in obj.data.vertices]
        size=Vector(tuple(max(p[k] for p in points)-min(p[k] for p in points) for k in range(3)))
        expected=Vector((.245,.39,.36))*scale
        result=dict(variant=variant,name=obj.name,dimensions_m=list(size),expected_m=list(expected),passed=(size-expected).length<.0001)
        checks.append(result)
(R/'qa/pauldron_size_verification.json').write_text(json.dumps(checks,indent=2));assert len(checks)==8 and all(r['passed'] for r in checks)
print('PAULDRON_SIZE_CHECKS_PASS',len(checks))
