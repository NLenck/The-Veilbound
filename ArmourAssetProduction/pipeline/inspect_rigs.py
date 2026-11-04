import bpy,json
from pathlib import Path
R=Path(r'C:\The Veilbound');out={}
for variant in ['Male','Female']:
    bpy.ops.wm.open_mainfile(filepath=str(R/f'PlayerAssetProduction/models_blender/{variant}/{variant}.blend'))
    arm=next(o for o in bpy.context.scene.objects if o.type=='ARMATURE')
    out[variant]={b.name:{'head':list(arm.matrix_world@b.head_local),'tail':list(arm.matrix_world@b.tail_local),'parent':b.parent.name if b.parent else None} for b in arm.data.bones}
(R/'ArmourAssetProduction/qa/character_rig_map.json').write_text(json.dumps(out,indent=2))
