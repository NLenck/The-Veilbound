import bpy,bmesh,json,math
from pathlib import Path
from mathutils import Vector
R=Path(r'C:\The Veilbound\GreatswordAssetProduction');result=json.loads((R/'logs/prototype_result.json').read_text(encoding='utf-8-sig').strip());bpy.ops.wm.read_factory_settings(use_empty=True);bpy.ops.import_scene.gltf(filepath=result['model_file']);rows=[]
for obj in [o for o in bpy.context.scene.objects if o.type=='MESH']:
 mesh=obj.data;mesh.calc_loop_triangles();p=[obj.matrix_world@v.co for v in mesh.vertices];bm=bmesh.new();bm.from_mesh(mesh);bmesh.ops.remove_doubles(bm,verts=list(bm.verts),dist=1e-6);bm.verts.ensure_lookup_table();seen=set();components=[]
 for v in bm.verts:
  if v.index in seen:continue
  stack=[v];seen.add(v.index);verts=[];faces=set()
  while stack:
   cur=stack.pop();verts.append(cur);faces.update(f.index for f in cur.link_faces)
   for edge in cur.link_edges:
    n=edge.other_vert(cur)
    if n.index not in seen:seen.add(n.index);stack.append(n)
  xyz=[obj.matrix_world@v.co for v in verts];lo=[min(v[k] for v in xyz) for k in range(3)];hi=[max(v[k] for v in xyz) for k in range(3)];components.append(dict(vertices=len(verts),faces=len(faces),lo=lo,hi=hi,centre=[(a+b)/2 for a,b in zip(lo,hi)]))
 rows.append(dict(name=obj.name,vertices=len(mesh.vertices),triangles=len(mesh.loop_triangles),bounds=[[min(v[k] for v in p),max(v[k] for v in p)] for k in range(3)],matrix=[list(r) for r in obj.matrix_world],materials=[m.name for m in mesh.materials],components=components,boundary_edges=sum(e.is_boundary for e in bm.edges),nonmanifold_edges=sum(not e.is_manifold and not e.is_boundary for e in bm.edges),volume=bm.calc_volume(signed=True)));bm.free()
(R/'qa/raw_geometry.json').write_text(json.dumps(rows,indent=2));print('RAW_MESHES',len(rows),'TRIANGLES',sum(o['triangles'] for o in rows));print('BOUNDS',[(o['name'],o['triangles'],o['bounds'],len(o['components'])) for o in rows]);bpy.ops.wm.save_as_mainfile(filepath=str(R/'raw/Imported_Original.blend'))
