import bpy,bmesh,json
from pathlib import Path
from mathutils import Matrix
p=Path(r'C:\The Veilbound\PlayerAssetProduction')
bpy.ops.wm.open_mainfile(filepath=str(p/'models_blender/Male/Male.blend'))
arm=next(o for o in bpy.context.scene.objects if o.type=='ARMATURE');arm.animation_data.action=None
for b in arm.pose.bones:b.matrix_basis=Matrix.Identity(4)
o=next(o for o in bpy.context.scene.objects if o.type=='MESH' and any(m.type=='ARMATURE' for m in o.modifiers))
bm=bmesh.new();bm.from_mesh(o.data)
bmesh.ops.delete(bm,geom=[f for f in bm.faces if f.material_index!=0],context='FACES')
bmesh.ops.remove_doubles(bm,verts=list(bm.verts),dist=1e-5)
unseen=set(v for v in bm.verts if v.link_faces);out=[]
while unseen:
    seed=unseen.pop();group={seed};todo=[seed]
    while todo:
        v=todo.pop()
        for e in v.link_edges:
            w=e.other_vert(v)
            if w in unseen:unseen.remove(w);group.add(w);todo.append(w)
    fs=set(f for v in group for f in v.link_faces)
    out.append(dict(vertices=len(group),faces=len(fs),lo=[min(v.co[i] for v in group) for i in range(3)],hi=[max(v.co[i] for v in group) for i in range(3)]))
print('COMPONENTS',json.dumps(sorted(out,key=lambda r:r['faces'],reverse=True)[:25]))
for z in [1.3,1.4,1.5,1.6,1.65]:
    pts=[v.co for v in o.data.vertices if abs(v.co.z-z)<.025 and v.co.y>.14]
    print('BACK',z,len(pts),[[round(x,3) for x in v] for v in sorted(pts,key=lambda v:v.y,reverse=True)[:8]])
