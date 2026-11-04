import bpy,bmesh,json
from mathutils import Vector
o=next(o for o in bpy.context.scene.objects if o.type=='MESH' and o.name.startswith('Veilbound_'))
bm=bmesh.new();bm.from_mesh(o.data);bm.verts.ensure_lookup_table()
unseen=set(bm.verts);out=[]
while unseen:
    seed=min(unseen,key=lambda v:v.index);unseen.remove(seed);todo=[seed];verts={seed}
    while todo:
        v=todo.pop()
        for e in v.link_edges:
            w=e.other_vert(v)
            if w in unseen:unseen.remove(w);verts.add(w);todo.append(w)
    faces=set(f for v in verts for f in v.link_faces)
    pts=[v.co for v in verts]
    lo=Vector(tuple(min(p[i] for p in pts) for i in range(3)))
    hi=Vector(tuple(max(p[i] for p in pts) for i in range(3)))
    out.append(dict(seed=seed.index,vertices=len(verts),faces=len(faces),area=sum(f.calc_area() for f in faces),
                    center=list((lo+hi)/2),dimensions=list(hi-lo)))
print('COMPONENTS '+json.dumps(out))
bm.free()
