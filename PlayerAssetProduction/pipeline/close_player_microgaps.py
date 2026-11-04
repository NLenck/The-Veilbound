import bpy,bmesh,json
from pathlib import Path
from mathutils import Vector
ROOT=Path(r'C:\The Veilbound\PlayerAssetProduction')
for variant in ['Male','Female']:
    path=ROOT/f'models_blender/{variant}/{variant}.blend';bpy.ops.wm.open_mainfile(filepath=str(path))
    obj=next(o for o in bpy.context.scene.objects if o.type=='MESH' and any(m.type=='ARMATURE' for m in o.modifiers))
    bm=bmesh.new();bm.from_mesh(obj.data);bmesh.ops.remove_doubles(bm,verts=list(bm.verts),dist=1e-6)
    edges=[e for e in bm.edges if e.is_boundary and e.link_faces[0].material_index==0]
    # Close only the short residual loops, keeping the pupil/fissure surfaces open.
    faces=bmesh.ops.holes_fill(bm,edges=edges,sides=24)['faces'] if edges else []
    uv=bm.loops.layers.uv.active
    for f in faces:
        f.material_index=0;c=f.calc_center_median();samples=[]
        for e in f.edges:
            for neighbor in e.link_faces:
                if neighbor!=f and neighbor.material_index==0:
                    samples.extend(l[uv].uv.copy() for l in neighbor.loops)
        centre=sum(samples,Vector((0,0)))/len(samples) if samples else Vector((.5,.5))
        for l in f.loops:l[uv].uv=centre+Vector((l.vert.co.x-c.x,l.vert.co.z-c.z))*.005
    bmesh.ops.triangulate(bm,faces=[f for f in bm.faces if len(f.verts)>3])
    bmesh.ops.recalc_face_normals(bm,faces=[f for f in bm.faces if f.material_index==0])
    bm.to_mesh(obj.data);bm.free();obj.data.calc_loop_triangles()
    audit_path=ROOT/f'qa/{variant}/player_audit.json';audit=json.loads(audit_path.read_text())
    audit['final_triangles']=len(obj.data.loop_triangles)
    audit['art_corrections']['microgap_faces_filled']=len(faces)
    audit['unweighted_vertices']=sum(not v.groups for v in obj.data.vertices)
    audit['bad_weight_sums']=sum(abs(sum(g.weight for g in v.groups)-1)>.01 for v in obj.data.vertices)
    audit_path.write_text(json.dumps(audit,indent=2));bpy.ops.wm.save_as_mainfile(filepath=str(path))
    print('MICROGAPS_CLOSED',variant,len(faces))
