import bpy,bmesh,json
from pathlib import Path
r=Path(r'C:\The Veilbound\ArmourAssetProduction\Mastered');rows=[]
for variant in ['Male','Female']:
 bpy.ops.wm.open_mainfile(filepath=str(r/f'blender/Master_{variant}.blend'))
 for o in bpy.context.scene.objects:
  if o.type!='MESH' or not any(k in o.name for k in ['_UpperArm_','_Elbow_']):continue
  bm=bmesh.new();bm.from_mesh(o.data);volume=bm.calc_volume(signed=True);boundaries=sum(e.is_boundary for e in bm.edges);bm.free();rows.append(dict(variant=variant,name=o.name,volume=volume,boundary_edges=boundaries,passed=volume>0 and boundaries==0))
(r/'qa/arm_mesh_integrity.json').write_text(json.dumps(rows,indent=2));print('ARM_INTEGRITY',len(rows),all(row['passed'] for row in rows));assert all(row['passed'] for row in rows)
