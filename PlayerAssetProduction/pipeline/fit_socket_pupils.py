"""Fit small glowing pupils to measured eye openings, on the skinned surface."""
import bpy,bmesh,json,math,sys
from pathlib import Path
from mathutils import Vector,geometry
from mathutils.bvhtree import BVHTree
ROOT=Path(r'C:\The Veilbound\PlayerAssetProduction')
variant=sys.argv[sys.argv.index('--')+1]
path=ROOT/f'models_blender/{variant}/{variant}.blend'
bpy.ops.wm.open_mainfile(filepath=str(path))
arm=next(o for o in bpy.context.scene.objects if o.type=='ARMATURE')
body=next(o for o in bpy.context.scene.objects if o.type=='MESH' and any(m.type=='ARMATURE' for m in o.modifiers))
audit_path=ROOT/f'qa/{variant}/player_audit.json';audit=json.loads(audit_path.read_text());H=audit['height_m']
arm.animation_data.action=bpy.data.actions['Idle']
if arm.animation_data.action.slots:arm.animation_data.action_slot=arm.animation_data.action.slots[0]
bpy.context.scene.frame_set(1);bpy.context.view_layer.update()
# Landmarks measured in the uncovered 1200x1200 orthographic render.
landmarks={'Male':[(596,532),(726,531)],'Female':[(590,576),(737,575)]}[variant]
old_slots={i for i,m in enumerate(body.data.materials) if m.name in ['Veil_Red_Emission','Iris_Dark_Centre','Socket_Pupil_Emission']}
bm=bmesh.new();bm.from_mesh(body.data)
bmesh.ops.delete(bm,geom=[f for f in bm.faces if f.material_index in old_slots],context='FACES')
loose=[v for v in bm.verts if not v.link_faces]
if loose:bmesh.ops.delete(bm,geom=loose,context='VERTS')
bm.to_mesh(body.data);bm.free();body.data.update();bpy.context.view_layer.update()
deps=bpy.context.evaluated_depsgraph_get();ev=body.evaluated_get(deps);posed=ev.to_mesh();posed.calc_loop_triangles()
coords=[body.matrix_world@v.co for v in posed.vertices]
tris=[tuple(t.vertices) for t in posed.loop_triangles if posed.polygons[t.polygon_index].material_index==0]
bvh=BVHTree.FromPolygons(coords,tris,all_triangles=True)
def sample(px,py):
    x=(px/1200-.5)*H*.32;z=H*.89+(.5-py/1200)*H*.32
    at,n,idx,d=bvh.ray_cast(Vector((x,-2,z)),Vector((0,1,0)),4)
    if at is None:raise RuntimeError('No eye surface at '+str((px,py)))
    tri=tris[idx];a,b,c=[coords[i] for i in tri]
    weights=geometry.barycentric_transform(at,a,b,c,Vector((1,0,0)),Vector((0,1,0)),Vector((0,0,1)))
    rest=sum((body.data.vertices[i].co*w for i,w in zip(tri,weights)),Vector())
    groups={}
    for i,w in zip(tri,weights):
        for g in body.data.vertices[i].groups:groups[g.group]=groups.get(g.group,0)+g.weight*w
    # A 0.12 mm lift prevents z fighting while keeping the pupil within the socket.
    rest.y-=.00012
    return rest,groups
verts=[];faces=[];skin=[];measurements=[]
for px,py in landmarks:
    centre,groups=sample(px,py);measurements.append(dict(render_pixel=[px,py],rest_position=list(centre)))
    offset=len(verts);verts.append(centre);skin.append(groups)
    # 2.8 mm pupil; the shallow conforming patch follows the actual eyeball surface.
    radius=.0028/(H*.32)*1200
    for j in range(20):
        angle=2*math.pi*j/20;p,g=sample(px+radius*math.cos(angle),py+radius*.88*math.sin(angle))
        verts.append(p);skin.append(g)
    for j in range(20):faces.append((offset,offset+1+(j+1)%20,offset+1+j))
ev.to_mesh_clear()
mesh=bpy.data.meshes.new('SocketPupils');mesh.from_pydata(verts,[],faces);mesh.update()
obj=bpy.data.objects.new('SocketPupils',mesh);bpy.context.collection.objects.link(obj)
obj.matrix_world=body.matrix_world.copy()
for group in body.vertex_groups:obj.vertex_groups.new(name=group.name)
for i,weights in enumerate(skin):
    total=sum(weights.values())
    for g,w in weights.items():
        if w>1e-8:obj.vertex_groups[g].add([i],max(0,w/total),'REPLACE')
mat=bpy.data.materials.get('Socket_Pupil_Emission') or bpy.data.materials.new('Socket_Pupil_Emission');mat.use_nodes=True
bs=mat.node_tree.nodes.get('Principled BSDF');bs.inputs['Base Color'].default_value=(.35,.006,.002,1)
bs.inputs['Emission Color'].default_value=(1,.012,.003,1);bs.inputs['Emission Strength'].default_value=.65
bs.inputs['Roughness'].default_value=.65;bs.inputs['Specular IOR Level'].default_value=.1
obj.data.materials.append(mat)
bpy.ops.object.select_all(action='DESELECT');body.select_set(True);obj.select_set(True);bpy.context.view_layer.objects.active=body;bpy.ops.object.join()
# Remove the unused overlay material slots; retain the textured body, fissures and pupils.
for i in reversed(range(len(body.data.materials))):
    if not any(p.material_index==i for p in body.data.polygons):body.data.materials.pop(index=i)
body.data.calc_loop_triangles();audit['final_triangles']=len(body.data.loop_triangles)
audit['materials']=[m.name for m in body.data.materials]
audit['art_revision']=3;audit['socket_pupils']=measurements
audit['art_corrections']['eye_treatment']='Old overlay rings removed; two surface-fitted 2.8 mm glowing pupils, measured from eye openings.'
audit['art_corrections']['eye_emission_strength']=.65
audit['unweighted_vertices']=sum(not v.groups for v in body.data.vertices)
audit['bad_weight_sums']=sum(abs(sum(g.weight for g in v.groups)-1)>.01 for v in body.data.vertices)
audit_path.write_text(json.dumps(audit,indent=2))
bpy.ops.wm.save_as_mainfile(filepath=str(path))
print('SOCKET_PUPILS_FITTED',variant,json.dumps(measurements))


