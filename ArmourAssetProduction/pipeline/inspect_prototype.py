"""Preserve raw provenance; split welded connectivity, audit and render all axes."""
import bpy,bmesh,json,math,sys
from pathlib import Path
from mathutils import Vector
R=Path(r'C:\The Veilbound\ArmourAssetProduction')
result=json.loads((R/'logs/prototype_result.json').read_text(encoding='utf-8-sig').strip().splitlines()[-1])
assert result['status']=='success'
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=result['model_file'])
original=[o for o in bpy.context.scene.objects if o.type=='MESH'];raw=[]
def bounds(o):
    pts=[o.matrix_world@v.co for v in o.data.vertices]
    return [min(p[k] for p in pts) for k in range(3)],[max(p[k] for p in pts) for k in range(3)]
def count(o):o.data.calc_loop_triangles();return len(o.data.loop_triangles)
for o in original:
    raw.append(dict(name=o.name,vertices=len(o.data.vertices),triangles=count(o),bounds=bounds(o)))
    o['tripo_part']=o.name
    bpy.ops.object.select_all(action='DESELECT');o.select_set(True);bpy.context.view_layer.objects.active=o
    bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
    bm=bmesh.new();bm.from_mesh(o.data);bmesh.ops.remove_doubles(bm,verts=list(bm.verts),dist=1e-6)
    bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(o.data);bm.free()
    bpy.ops.object.mode_set(mode='EDIT');bpy.ops.mesh.select_all(action='SELECT');bpy.ops.mesh.separate(type='LOOSE');bpy.ops.object.mode_set(mode='OBJECT')
objects=[o for o in bpy.context.scene.objects if o.type=='MESH' and o.data.polygons]
objects.sort(key=lambda o:count(o),reverse=True)
rows=[]
for i,o in enumerate(objects):
    lo,hi=bounds(o);o.name=f'Candidate_{i:03d}'
    bm=bmesh.new();bm.from_mesh(o.data)
    rows.append(dict(name=o.name,source=o.get('tripo_part'),triangles=count(o),vertices=len(o.data.vertices),lo=lo,hi=hi,
        centre=[(a+b)/2 for a,b in zip(lo,hi)],dimensions=[b-a for a,b in zip(lo,hi)],
        boundary_edges=sum(e.is_boundary for e in bm.edges),nonmanifold_edges=sum(not e.is_manifold for e in bm.edges),
        degenerate_faces=sum(f.calc_area()<1e-12 for f in bm.faces)))
    bm.free()
    mat=bpy.data.materials.new('Diagnostic_'+o.name);mat.diffuse_color=(*[.25+.55*((i*prime)%19)/19 for prime in [7,11,13]],1);mat.use_nodes=True
    mat.node_tree.nodes.get('Principled BSDF').inputs['Base Color'].default_value=mat.diffuse_color
    mat.node_tree.nodes.get('Principled BSDF').inputs['Roughness'].default_value=.65
    o.data.materials.clear();o.data.materials.append(mat)
lo=Vector(tuple(min(r['lo'][k] for r in rows) for k in range(3)));hi=Vector(tuple(max(r['hi'][k] for r in rows) for k in range(3)));centre=(lo+hi)/2;size=hi-lo
scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=16
scene.world=bpy.data.worlds.new('ArmourWorld');scene.world.use_nodes=True;scene.world.node_tree.nodes['Background'].inputs[0].default_value=(.25,.25,.25,1)
bpy.ops.object.camera_add();camera=bpy.context.object;scene.camera=camera;camera.data.type='ORTHO';camera.data.ortho_scale=max(size)*1.2
scene.render.resolution_x=1200;scene.render.resolution_y=1000;scene.render.resolution_percentage=100
for loc,power,scale in [(centre+Vector((3,-4,6))*max(size),1600,5),(centre+Vector((-4,2,3))*max(size),1200,4)]:
    bpy.ops.object.light_add(type='AREA',location=loc);light=bpy.context.object;light.data.energy=power*max(size)**2;light.data.shape='DISK';light.data.size=scale*max(size);light.rotation_euler=(centre-light.location).to_track_quat('-Z','Y').to_euler()
for name,direction in [('posX',(1,0,.03)),('negX',(-1,0,.03)),('posY',(0,1,.03)),('negY',(0,-1,.03)),('three_quarter',(1,-1,.35))]:
    camera.location=centre+Vector(direction).normalized()*max(size)*4;camera.rotation_euler=(centre-camera.location).to_track_quat('-Z','Y').to_euler()
    scene.render.filepath=str(R/f'previews/prototype_{name}.png');bpy.ops.render.render(write_still=True)
audit=dict(task_id=result['task_id'],raw_meshes=raw,raw_mesh_count=len(raw),raw_triangles=sum(r['triangles'] for r in raw),
    connected_component_count=len(rows),components=rows,bounds=[list(lo),list(hi)],dimensions=list(size))
(R/'qa/prototype_components.json').write_text(json.dumps(audit,indent=2))
bpy.ops.wm.save_as_mainfile(filepath=str(R/'blender/Prototype_Grouped_Inspection.blend'))
print('PROTOTYPE_INSPECTION',json.dumps({k:v for k,v in audit.items() if k not in ['components','raw_meshes']}))
