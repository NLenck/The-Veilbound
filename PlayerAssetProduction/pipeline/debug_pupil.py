import bpy,json
from pathlib import Path
from mathutils import Vector
r=Path(r'C:\The Veilbound\PlayerAssetProduction')
for v in ['Male','Female']:
 bpy.ops.wm.open_mainfile(filepath=str(r/f'models_blender/{v}/{v}.blend'))
 a=next(o for o in bpy.context.scene.objects if o.type=='ARMATURE');a.animation_data.action=bpy.data.actions['Idle'];a.animation_data.action_slot=a.animation_data.action.slots[0];bpy.context.scene.frame_set(1);bpy.context.view_layer.update()
 o=next(o for o in bpy.context.scene.objects if o.type=='MESH' and any(m.type=='ARMATURE' for m in o.modifiers));ev=o.evaluated_get(bpy.context.evaluated_depsgraph_get());m=ev.to_mesh();H=1.85 if v=='Male' else 1.73
 idx=next(i for i,mt in enumerate(m.materials) if mt.name=='Socket_Pupil_Emission');ids=sorted(set(i for p in m.polygons if p.material_index==idx for i in p.vertices))
 for i in [ids[0],ids[21]]:
  p=o.matrix_world@m.vertices[i].co
  print('POINT',v,i,list(p),'PIX',1200*(p.x/(H*.32)+.5),1200*(.5-(p.z-H*.89)/(H*.32)),[(o.vertex_groups[g.group].name,g.weight) for g in o.data.vertices[i].groups])
