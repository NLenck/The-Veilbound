import bpy,json,sys
from pathlib import Path
ROOT=Path(r'C:\The Veilbound\PlayerAssetProduction')
v=sys.argv[sys.argv.index('--')+1]
bpy.ops.wm.open_mainfile(filepath=str(ROOT/f'models_blender/{v}/{v}.blend'))
arm=next(o for o in bpy.context.scene.objects if o.type=='ARMATURE');arm.animation_data.action=None
from mathutils import Matrix
for b in arm.pose.bones:b.matrix_basis=Matrix.Identity(4)
body=next(o for o in bpy.context.scene.objects if o.type=='MESH' and any(m.type=='ARMATURE' for m in o.modifiers))
print('MATERIALS',[(i,m.name) for i,m in enumerate(body.data.materials)])
for z in [1.25,1.3,1.35,1.4,1.45,1.5,1.55,1.6,1.65,1.7]:
    pts=[p.co for p in body.data.vertices if abs(p.co.x)<.12 and abs(p.co.z-z)<.025]
    if pts:print('SLICE',z,'Y',sorted(round(p.y,3) for p in pts)[::max(1,len(pts)//20)],'COUNT',len(pts))
for i,m in enumerate(body.data.materials):
    if 'Emission' in m.name or 'Iris' in m.name:
        ids={vi for p in body.data.polygons if p.material_index==i for vi in p.vertices}
        pts=[body.data.vertices[vi].co for vi in ids]
        print('DETAIL',m.name,len(ids),[sum(p[k] for p in pts)/len(pts) for k in range(3)])
