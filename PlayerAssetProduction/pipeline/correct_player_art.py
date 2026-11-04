"""Revision 2: remove male ponytail, neutralize expressions, soften red iris treatment.
Edits only the processed models; raw Tripo outputs and the live game are untouched.
"""
import bpy,bmesh,json,math,sys,shutil
from pathlib import Path
from mathutils import Vector,Matrix,geometry
from mathutils.bvhtree import BVHTree
ROOT=Path(r'C:\The Veilbound\PlayerAssetProduction')
variant=sys.argv[sys.argv.index('--')+1]
backup=ROOT/'revisions/01_before_art_corrections';backup.mkdir(parents=True,exist_ok=True)
source=ROOT/f'models_blender/{variant}/{variant}.blend'
if not (backup/f'{variant}.blend').exists():shutil.copy2(source,backup/f'{variant}.blend')
audit_path=ROOT/f'qa/{variant}/player_audit.json'
if not (backup/f'{variant}_audit.json').exists():shutil.copy2(audit_path,backup/f'{variant}_audit.json')
for view in ['head_detail','side','back']:
    src=ROOT/f'previews/{variant}/{view}.png';dst=backup/f'{variant}_{view}.png'
    if src.exists() and not dst.exists():shutil.copy2(src,dst)
bpy.ops.wm.open_mainfile(filepath=str(backup/f'{variant}.blend'))
arm=next(o for o in bpy.context.scene.objects if o.type=='ARMATURE')
arm.animation_data.action=None
for pb in arm.pose.bones:pb.matrix_basis=Matrix.Identity(4)
bpy.context.view_layer.update()
body=next(o for o in bpy.context.scene.objects if o.type=='MESH' and any(m.type=='ARMATURE' for m in o.modifiers))
audit=json.loads((backup/f'{variant}_audit.json').read_text());H=audit['height_m']
material_indices={m.name:i for i,m in enumerate(body.data.materials)}
iris_index=material_indices['Veil_Red_Emission'];pupil_index=material_indices['Iris_Dark_Centre']
body_index=next(i for name,i in material_indices.items() if name.startswith('tripo_material'))

def material_components(index):
    faces=[p for p in body.data.polygons if p.material_index==index]
    ids={i for p in faces for i in p.vertices};neighbors={i:set() for i in ids}
    for p in faces:
        for i in p.vertices:neighbors[i].update(p.vertices)
    result=[]
    while ids:
        seed=ids.pop();group={seed};todo=[seed]
        while todo:
            for other in neighbors[todo.pop()]:
                if other in ids:ids.remove(other);group.add(other);todo.append(other)
        result.append(group)
    return result
eye_components=material_components(iris_index)
centres=[]
for group in eye_components:
    points=[body.data.vertices[i].co.copy() for i in group]
    centre=Vector(tuple((min(p[k] for p in points)+max(p[k] for p in points))*.5 for k in range(3)))
    centres.append(centre)
    for i in group:
        p=body.data.vertices[i].co;delta=p-centre
        p.x=centre.x+delta.x*.82;p.z=centre.z+delta.z*.75
        p.y=centre.y+H*.00105+delta.y*.23
for group in material_components(pupil_index):
    points=[body.data.vertices[i].co.copy() for i in group]
    centre=Vector(tuple((min(p[k] for p in points)+max(p[k] for p in points))*.5 for k in range(3)))
    for i in group:
        p=body.data.vertices[i].co;delta=p-centre
        p.x=centre.x+delta.x*.82;p.z=centre.z+delta.z*.8
        p.y=centre.y+H*.00197+delta.y*.23
iris=body.data.materials[iris_index].node_tree.nodes.get('Principled BSDF')
iris.inputs['Base Color'].default_value=(.16,.008,.002,1)
iris.inputs['Emission Color'].default_value=(.3,.004,.001,1)
iris.inputs['Emission Strength'].default_value=.25
iris.inputs['Specular IOR Level'].default_value=.08
iris.inputs['Roughness'].default_value=.72

# Neutral, alert rather than smiling: lift downturned lip corners and heavy outer lids.
eye_x=sum(p.x for p in centres)/len(centres);eye_z=sum(p.z for p in centres)/len(centres)
eye_y=sum(p.y for p in centres)/len(centres)
mouth_z=eye_z-H*.05;mouth_half=H*.017
sculpted=0
for v in body.data.vertices:
    p=v.co
    if p.y>eye_y+H*.055 or abs(p.x-eye_x)>H*.09 or p.z<eye_z-H*.11 or p.z>eye_z+H*.045:continue
    delta=0
    for sign in [-1,1]:
        dx=(p.x-(eye_x+sign*mouth_half))/(H*.009)
        dz=(p.z-mouth_z)/(H*.008)
        delta+=H*(.0030 if variant=='Male' else .0038)*math.exp(-(dx*dx+dz*dz)*1.4)
        dx=(p.x-(eye_x+sign*H*.027))/(H*.012)
        dz=(p.z-(eye_z+H*.010))/(H*.007)
        delta+=H*.0015*math.exp(-(dx*dx+dz*dz)*1.4)
    if eye_z+H*.0008<p.z<eye_z+H*.008:
        for c in centres:
            dx=(p.x-c.x)/(H*.011);dz=(p.z-(eye_z+H*.004))/(H*.004)
            delta+=H*.0008*math.exp(-(dx*dx+dz*dz)*1.4)
    if delta>1e-7:p.z+=delta;sculpted+=1

removed_hair_faces=0;filled_hair_faces=0;reprojected_faces=0
if variant=='Male':
    # Sample the clean opposite upper back, including its UVs, as the patch guide.
    body.data.calc_loop_triangles();uv_data=body.data.uv_layers.active.data
    source_points=[v.co.copy() for v in body.data.vertices]
    source_triangles=[];source_uvs=[]
    for tri in body.data.loop_triangles:
        if body.data.polygons[tri.polygon_index].material_index!=body_index:continue
        points=[source_points[i] for i in tri.vertices]
        if sum(p.x for p in points)/3>eye_x:continue
        source_triangles.append(tuple(tri.vertices))
        source_uvs.append([uv_data[i].uv.copy() for i in tri.loops])
    bvh=BVHTree.FromPolygons(source_points,source_triangles,all_triangles=True)
    def clean_back(x,z):
        at,normal,index,distance=bvh.ray_cast(Vector((2*eye_x-x,1,z)),Vector((0,-1,0)),2)
        if at is None:return None
        tri=source_triangles[index];points=[source_points[i] for i in tri]
        uvs=[Vector((uv.x,uv.y,0)) for uv in source_uvs[index]]
        uv=geometry.barycentric_transform(at,*points,*uvs)
        return at.y,uv.xy
    # Replace the asymmetric posterior quarter with the clean opposite surface.
    # Plane cuts give exact region boundaries; mirror source UVs and skin weights.
    bm=bmesh.new();bm.from_mesh(body.data)
    bmesh.ops.remove_doubles(bm,verts=list(bm.verts),dist=1e-6)
    for origin,normal in [((0,0,0),(1,0,0)),((0,.035,0),(0,1,0)),((0,0,1.24),(0,0,1)),((0,0,1.76),(0,0,1))]:
        base_faces=[f for f in bm.faces if f.material_index==body_index]
        base_edges=set(e for f in base_faces for e in f.edges)
        base_verts=set(v for f in base_faces for v in f.verts)
        bmesh.ops.bisect_plane(bm,geom=base_faces+list(base_edges)+list(base_verts),dist=1e-7,plane_co=origin,plane_no=normal)
    def region(f,sign):
        c=f.calc_center_median()
        return f.material_index==body_index and c.x*sign>1e-6 and c.y>.035+1e-6 and 1.240001<c.z<1.759999
    donor=[f for f in bm.faces if region(f,-1)]
    cut=[f for f in bm.faces if region(f,1)];removed_hair_faces=len(cut)
    duplicate=bmesh.ops.duplicate(bm,geom=donor)
    new_faces=[f for f in duplicate['geom'] if isinstance(f,bmesh.types.BMFace)]
    new_verts=[v for v in duplicate['geom'] if isinstance(v,bmesh.types.BMVert)]
    deform=bm.verts.layers.deform.active
    group_names={g.index:g.name for g in body.vertex_groups};group_ids={g.name:g.index for g in body.vertex_groups}
    for v in new_verts:
        v.co.x=2*eye_x*max(0,min(1,(v.co.z-1.50)/.16))-v.co.x
        if deform:
            weights=dict(v[deform]);v[deform].clear()
            for group,weight in weights.items():
                name=group_names[group]
                swap=name.replace('Left','SWAP').replace('Right','Left').replace('SWAP','Right')
                v[deform][group_ids.get(swap,group)]=weight
    for f in new_faces:f.normal_flip()
    bmesh.ops.delete(bm,geom=cut,context='FACES')
    def edge_loops(edges):
        todo=set(edges);loops=[]
        while todo:
            seed=todo.pop();group={seed};stack=[seed]
            while stack:
                edge=stack.pop()
                for v in edge.verts:
                    for other in v.link_edges:
                        if other in todo:todo.remove(other);group.add(other);stack.append(other)
            loops.append(group)
        return loops
    new_set=set(new_verts)
    new_boundaries=edge_loops([e for e in bm.edges if e.is_boundary and all(v in new_set for v in e.verts)])
    old_boundaries=edge_loops([e for e in bm.edges if e.is_boundary and not any(v in new_set for v in e.verts) and e.link_faces[0].material_index==body_index])
    def centre(edges):
        points=set(v for e in edges for v in e.verts)
        return sum((v.co for v in points),Vector())/len(points)
    seam_faces=[]
    for new_loop in new_boundaries:
        if not old_boundaries:break
        c=centre(new_loop);old=min(old_boundaries,key=lambda edges:(centre(edges)-c).length)
        old_boundaries.remove(old)
        def ordered(edges):
            neighbors={v:[] for e in edges for v in e.verts}
            for e in edges:
                a,b=e.verts;neighbors[a].append(b);neighbors[b].append(a)
            start=next(iter(neighbors));out=[start];previous=None;current=start
            for _ in range(len(neighbors)-1):
                nxt=next((v for v in neighbors[current] if v!=previous and v not in out),None)
                if nxt is None:break
                out.append(nxt);previous,current=current,nxt
            return out
        A=ordered(old);B=ordered(new_loop)
        ia,ib=min(((i,j) for i in range(len(A)) for j in range(len(B))),key=lambda ij:(A[ij[0]].co-B[ij[1]].co).length_squared)
        A=A[ia:]+A[:ia];B=B[ib:]+B[:ib]
        if (A[1].co-B[-1].co).length<(A[1].co-B[1].co).length:B=[B[0]]+list(reversed(B[1:]))
        n,m=len(A),len(B);A.append(A[0]);B.append(B[0]);i=j=0
        while i<n or j<m:
            advance_a=i<n and (j==m or (A[i+1].co-B[j].co).length<=(A[i].co-B[j+1].co).length)
            points=(A[i],A[i+1],B[j]) if advance_a else (A[i],B[j+1],B[j])
            if len(set(points))==3:
                try:seam_faces.append(bm.faces.new(points))
                except ValueError:pass
            if advance_a:i+=1
            else:j+=1
    filled_hair_faces=len(seam_faces);reprojected_faces=len(new_faces)
    uv_layer=bm.loops.layers.uv.active
    for face in seam_faces:
        face.material_index=body_index
        c=face.calc_center_median();sample=clean_back(c.x,c.z)
        if sample:
            face.normal_update();axes=sorted(range(3),key=lambda k:abs(face.normal[k]))[:2]
            for loop in face.loops:
                delta=loop.vert.co-c
                loop[uv_layer].uv=sample[1]+Vector((delta[axes[0]],delta[axes[1]]))*.005
    bmesh.ops.remove_doubles(bm,verts=list(bm.verts),dist=.00015)
    bmesh.ops.dissolve_degenerate(bm,edges=list(bm.edges),dist=.000001)
    bmesh.ops.triangulate(bm,faces=[f for f in bm.faces if len(f.verts)>3])
    loose=[v for v in bm.verts if not v.link_faces]
    if loose:bmesh.ops.delete(bm,geom=loose,context='VERTS')
    bmesh.ops.recalc_face_normals(bm,faces=[f for f in bm.faces if f.material_index==body_index])
    bm.to_mesh(body.data);bm.free()

body.data.update();body.data.calc_loop_triangles()
audit['final_triangles']=len(body.data.loop_triangles)
audit['art_revision']=2
audit['art_corrections']=dict(male_hair_faces_removed=removed_hair_faces,hair_patch_faces=filled_hair_faces,
    back_faces_uv_reprojected=reprojected_faces,expression_vertices_adjusted=sculpted,
    eye_emission_strength=.25,iris_size_factor=.82,flattened_eye_depth=True,additional_api_credits=0)
audit['unweighted_vertices']=sum(not v.groups or sum(g.weight for g in v.groups)<1e-6 for v in body.data.vertices)
audit['bad_weight_sums']=sum(abs(sum(g.weight for g in v.groups)-1)>.01 for v in body.data.vertices)
audit_path.write_text(json.dumps(audit,indent=2))
arm.animation_data.action=bpy.data.actions['Idle']
if arm.animation_data.action.slots:arm.animation_data.action_slot=arm.animation_data.action.slots[0]
bpy.context.scene.frame_set(1)
bpy.ops.wm.save_as_mainfile(filepath=str(source))
print('ART_CORRECTION',variant,json.dumps(audit['art_corrections']))
