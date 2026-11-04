"""Run in Blender 5.2: conservative cleanup, audit, atlas if needed, export, renders.
No paid services. Never opens or edits the Unity project. No automatic decimation.
"""
import bpy, bmesh, json, math, sys
from pathlib import Path
from mathutils import Vector, Matrix

ROOT=Path(r'C:\The Veilbound\EnemyAssetProduction')
args=sys.argv[sys.argv.index('--')+1:]
biome,unit=args[:2]
cfg=json.loads((ROOT/'pipeline'/ (args[2] if len(args)>2 else 'batch_config.json')).read_text())
job=next(j for j in cfg['jobs'] if j['biome']==biome and j['unit']==unit)
source=ROOT/f'models_raw/{biome}/{unit}.glb'
for folder in ['models_blender','models_unity','previews','qa']:
    (ROOT/folder/biome).mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=str(source))
meshes=[o for o in bpy.context.scene.objects if o.type=='MESH']
assert meshes, 'No mesh imported'

def select(objects):
    bpy.ops.object.select_all(action='DESELECT')
    for o in objects:o.select_set(True)
    bpy.context.view_layer.objects.active=objects[0]

def audit(objects):
    result={'mesh_count':len(objects),'vertices':0,'triangles':0,'material_slots':0,
            'boundary_edges':0,'nonmanifold_edges':0,'loose_vertices':0,'degenerate_faces':0,
            'uv_layers':{},'missing_uv_faces':0,'zero_uv_area_triangles':0,'nonfinite_vertices':0,
            'uv_out_of_range_loops':0,'connected_components':0,'inconsistent_normal_edges':0}
    points=[]
    for o in objects:
        m=o.data;m.calc_loop_triangles()
        result['vertices']+=len(m.vertices);result['triangles']+=len(m.loop_triangles)
        result['material_slots']+=len(m.materials)
        result['uv_layers'][o.name]=[u.name for u in m.uv_layers]
        if not m.uv_layers:result['missing_uv_faces']+=len(m.polygons)
        else:
            uv=m.uv_layers.active.data
            result['uv_out_of_range_loops']+=sum(1 for v in uv if min(v.uv)<-1e-5 or max(v.uv)>1.00001)
            for t in m.loop_triangles:
                a,b,c=[uv[i].uv for i in t.loops]
                if abs((b.x-a.x)*(c.y-a.y)-(b.y-a.y)*(c.x-a.x))<1e-12:result['zero_uv_area_triangles']+=1
        for v in m.vertices:
            points.append(o.matrix_world@v.co)
            if not all(math.isfinite(x) for x in v.co):result['nonfinite_vertices']+=1
        bm=bmesh.new();bm.from_mesh(m)
        result['boundary_edges']+=sum(e.is_boundary for e in bm.edges)
        result['nonmanifold_edges']+=sum(not e.is_manifold and not e.is_boundary for e in bm.edges)
        result['inconsistent_normal_edges']+=sum(e.is_manifold and not e.is_contiguous for e in bm.edges)
        result['loose_vertices']+=sum(not v.link_edges for v in bm.verts)
        result['degenerate_faces']+=sum(f.calc_area()<1e-12 for f in bm.faces)
        unseen=set(bm.verts)
        while unseen:
            result['connected_components']+=1;todo=[unseen.pop()]
            while todo:
                v=todo.pop()
                for e in v.link_edges:
                    w=e.other_vert(v)
                    if w in unseen:unseen.remove(w);todo.append(w)
        bm.free()
    lo=Vector(tuple(min(p[i] for p in points) for i in range(3)))
    hi=Vector(tuple(max(p[i] for p in points) for i in range(3)))
    result['dimensions_m']=[round(x,5) for x in hi-lo]
    result['bounds_min']=[round(x,5) for x in lo];result['bounds_max']=[round(x,5) for x in hi]
    return result

raw=audit(meshes)
# Detach importer empties while preserving transforms, then join static geometry.
for o in meshes:
    mat=o.matrix_world.copy();o.parent=None;o.matrix_world=mat
select(meshes)
bpy.ops.object.convert(target='MESH')
bpy.ops.object.join();obj=bpy.context.object;obj.name=f'Veilbound_{biome}_{unit}'
bpy.context.view_layer.update()
# Bake the evaluated import transform explicitly, avoiding stale operator matrices.
# P1 +X forward becomes Blender -Y forward; FBX export handles engine axes.
world=Matrix.Rotation(math.radians(float(job.get('yaw_deg',-90))),4,'Z') @ obj.matrix_world.copy()
coords=[world@v.co for v in obj.data.vertices]
lo=Vector(tuple(min(p[i] for p in coords) for i in range(3)))
hi=Vector(tuple(max(p[i] for p in coords) for i in range(3)))
scale=job['height_m']/(hi.z-lo.z)
center=Vector(((hi.x+lo.x)/2,(hi.y+lo.y)/2,lo.z))
for v,p in zip(obj.data.vertices,coords):v.co=(p-center)*scale
obj.matrix_world=Matrix.Identity(4)
bpy.context.view_layer.update()
bm=bmesh.new();bm.from_mesh(obj.data)
# Micrometre-scale weld only; loop UV data stays on face corners.
bmesh.ops.remove_doubles(bm,verts=list(bm.verts),dist=1e-6)
bmesh.ops.dissolve_degenerate(bm,edges=list(bm.edges),dist=1e-7)
loose=[v for v in bm.verts if not v.link_edges]
if loose:bmesh.ops.delete(bm,geom=loose,context='VERTS')
removed_debris_faces=0
removed_debris_components=0
if job.get('remove_flat_x_boundary_debris',False):
    # Ritual Orb only: inspected extraction debris lies exactly on the left X bound.
    # Keep volumetric floating diamonds and the open ring; remove tiny flat panels.
    unseen=set(bm.verts);left=min(v.co.x for v in bm.verts);remove=[]
    while unseen:
        seed=unseen.pop();group={seed};todo=[seed]
        while todo:
            v=todo.pop()
            for e in v.link_edges:
                w=e.other_vert(v)
                if w in unseen:unseen.remove(w);group.add(w);todo.append(w)
        faces=set(f for v in group for f in v.link_faces)
        xs=[v.co.x for v in group]
        area=sum(f.calc_area() for f in faces)
        if max(xs)-min(xs)<1e-6 and abs(min(xs)-left)<1e-6 and area<job['height_m']**2*.01 and len(faces)<250:
            remove.extend(group);removed_debris_faces+=len(faces);removed_debris_components+=1
    if remove:bmesh.ops.delete(bm,geom=remove,context='VERTS')
bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces))
# Separate coincident panels at over-shared edges without removing any faces.
overshared=[e for e in bm.edges if len(e.link_faces)>2]
split_nonmanifold_edges=len(overshared)
if overshared:bmesh.ops.split_edges(bm,edges=overshared)
bm.to_mesh(obj.data);bm.free();obj.data.update()
for p in obj.data.polygons:p.use_smooth=False
for mat in obj.data.materials:
    if mat:
        mat.use_backface_culling=False
        if mat.use_nodes:
            for node in mat.node_tree.nodes:
                if node.type=='BSDF_PRINCIPLED':
                    node.inputs['Roughness'].default_value=.85
                    node.inputs['Metallic'].default_value=0

scene=bpy.context.scene
scene.render.engine='CYCLES';scene.cycles.device='CPU';scene.cycles.samples=16
atlas_baked=False
if len(obj.data.materials)>1:
    # Keep source sampling on the original UV, bake colour to a new single atlas.
    old_uv=obj.data.uv_layers.active.name
    atlas=bpy.data.images.new(unit+'_BaseColor',width=2048,height=2048,alpha=False)
    for mat in obj.data.materials:
        mat.use_nodes=True;nodes=mat.node_tree.nodes;links=mat.node_tree.links
        uv=nodes.new('ShaderNodeUVMap');uv.uv_map=old_uv
        for n in list(nodes):
            if n.type=='TEX_IMAGE':links.new(uv.outputs['UV'],n.inputs['Vector'])
        target=nodes.new('ShaderNodeTexImage');target.image=atlas;nodes.active=target
    new_uv=obj.data.uv_layers.new(name='GameAtlas');obj.data.uv_layers.active=new_uv
    select([obj]);bpy.ops.object.mode_set(mode='EDIT');bpy.ops.mesh.select_all(action='SELECT')
    bpy.ops.uv.smart_project(angle_limit=math.radians(66),island_margin=.02)
    bpy.ops.object.mode_set(mode='OBJECT');new_uv.active_render=True
    scene.render.bake.use_pass_direct=False;scene.render.bake.use_pass_indirect=False
    scene.render.bake.use_pass_color=True;scene.render.bake.margin=8
    bpy.ops.object.bake(type='DIFFUSE')
    mat=bpy.data.materials.new(unit+'_AshenAtlas');mat.use_nodes=True
    tex=mat.node_tree.nodes.new('ShaderNodeTexImage');tex.image=atlas
    bsdf=mat.node_tree.nodes.get('Principled BSDF');bsdf.inputs['Roughness'].default_value=.85
    mat.node_tree.links.new(tex.outputs['Color'],bsdf.inputs['Base Color'])
    obj.data.materials.clear();obj.data.materials.append(mat)
    for p in obj.data.polygons:p.material_index=0
    atlas_baked=True

# Store unpacked texture files alongside FBX as well as packing the .blend.
texture_dir=ROOT/f'models_unity/{biome}/textures/{unit}';texture_dir.mkdir(parents=True,exist_ok=True)
textures=[]
for i,im in enumerate(list(bpy.data.images)):
    if im.type!='IMAGE' or im.size[0]==0:continue
    _=im.pixels[0]  # Force lazy-loaded embedded GLB images into memory.
    path=texture_dir/f'{unit}_{i:02d}.png'
    im.filepath_raw=str(path);im.file_format='PNG';im.save();im.pack();textures.append(str(path))
final=audit([obj])
final['target_triangles']=job['target_triangles']
final['budget_pass']=final['triangles']<=job.get('max_triangles',job['target_triangles']*1.1)
final['atlas_baked']=atlas_baked
final['faceted_shading']=True
final['provisional_height_m']=job['height_m']
final['decimation_used']=False
final['rigged']=False
final['split_overshared_edges']=split_nonmanifold_edges
final['removed_debris_faces']=removed_debris_faces
final['removed_debris_components']=removed_debris_components
final['textures']=textures
select([obj])
fbx=ROOT/f'models_unity/{biome}/{unit}.fbx'
glb=ROOT/f'models_unity/{biome}/{unit}.glb'
bpy.ops.export_scene.fbx(filepath=str(fbx),use_selection=True,object_types={'MESH'},
    axis_forward='-Z',axis_up='Y',apply_unit_scale=True,bake_anim=False,
    use_mesh_modifiers=True,mesh_smooth_type='FACE',path_mode='COPY',embed_textures=True)
bpy.ops.export_scene.gltf(filepath=str(glb),export_format='GLB',use_selection=True,
    export_normals=True,export_materials='EXPORT',export_animations=False)

# Separate studio collection: never included in engine exports.
studio=bpy.data.collections.new('PreviewStudio');scene.collection.children.link(studio)
def to_studio(o):
    for c in list(o.users_collection):c.objects.unlink(o)
    studio.objects.link(o)
bpy.ops.mesh.primitive_plane_add(size=max(final['dimensions_m'])*200,location=(0,0,-.008))
ground=bpy.context.object;ground.name='PreviewGround';to_studio(ground)
gmat=bpy.data.materials.new('StudioCharcoal');gmat.diffuse_color=(.035,.04,.045,1)
ground.data.materials.append(gmat)
scene.world=bpy.data.worlds.new('StudioWorld');scene.world.use_nodes=True
scene.world.node_tree.nodes['Background'].inputs[0].default_value=(.12,.14,.18,1)
scene.world.node_tree.nodes['Background'].inputs[1].default_value=.45
height=job['height_m'];span=max(final['dimensions_m']);target=Vector((0,0,height*.48))
def aim(o,at):o.rotation_euler=(Vector(at)-o.location).to_track_quat('-Z','Y').to_euler()
for name,loc,power,size in [('Key',(-3,-4,6),650,4),('Fill',(4,-2,3),400,3),('Rim',(2,3,5),800,3)]:
    data=bpy.data.lights.new(name,'AREA');data.energy=power*(height/2)**2;data.shape='DISK';data.size=size*height/2
    o=bpy.data.objects.new(name,data);studio.objects.link(o);o.location=Vector(loc)*height/2;aim(o,target)
camdata=bpy.data.cameras.new('ReviewCamera');camera=bpy.data.objects.new('ReviewCamera',camdata)
studio.objects.link(camera);scene.camera=camera;camdata.type='ORTHO';camdata.ortho_scale=span*1.25
scene.render.engine='CYCLES';scene.cycles.samples=24
scene.render.resolution_x=1000;scene.render.resolution_y=1000;scene.render.resolution_percentage=100
scene.render.image_settings.file_format='PNG';scene.view_settings.view_transform='AgX'
scene.view_settings.look='AgX - Medium High Contrast'
scene.view_settings.exposure=-.7
renders={}
def frame_camera(direction):
    direction=Vector(direction).normalized()
    at=Vector((0,0,height*.5))
    camera.location=at+direction*span*3;aim(camera,at)
    basis=camera.rotation_euler.to_matrix()
    right=basis@Vector((1,0,0));up=basis@Vector((0,1,0))
    points=[obj.matrix_world@v.co-at for v in obj.data.vertices]
    xs=[p.dot(right) for p in points];ys=[p.dot(up) for p in points]
    at+=right*((min(xs)+max(xs))*.5)+up*((min(ys)+max(ys))*.5)
    camera.location=at+direction*span*3;aim(camera,at)
    camdata.ortho_scale=max(max(xs)-min(xs),max(ys)-min(ys))*1.15
for name,direction in [('front',(0,-1,.10)),('side',(1,0,.10)),('three_quarter',(.75,-1,.25)),('back',(0,1,.10))]:
    frame_camera(direction)
    path=ROOT/f'previews/{biome}/{unit}_{name}.png';scene.render.filepath=str(path)
    bpy.ops.render.render(write_still=True);renders[name]=str(path)
frame_camera((.75,-1,.25))
select([obj])
bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/f'models_blender/{biome}/{unit}.blend'))
report={'unit':unit,'biome':biome,'source':str(source),'raw':raw,'final':final,'renders':renders,
        'limitations':['No automatic self-intersection or full UV-overlap proof; visual review required.',
                        'Height is a provisional production scale, not an existing Unity prefab measurement.',
                        'No retopology or decimation was applied; P1 generated directly at the requested low-poly budget.']}
(ROOT/f'qa/{biome}/{unit}_mesh_audit.json').write_text(json.dumps(report,indent=2))
print('VEILBOUND_AUDIT '+json.dumps(report))
