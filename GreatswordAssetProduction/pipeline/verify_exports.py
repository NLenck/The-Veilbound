import bpy,bmesh,json,math
from pathlib import Path
R=Path(r'C:\The Veilbound\GreatswordAssetProduction');audit=json.loads((R/'qa/processed_swords.json').read_text());results=[]
for row in audit:
 for extension in ['glb','fbx']:
  bpy.ops.wm.read_factory_settings(use_empty=True);path=Path(row[extension]);assert path.exists()
  if extension=='glb':bpy.ops.import_scene.gltf(filepath=str(path))
  else:bpy.ops.import_scene.fbx(filepath=str(path))
  objects=[o for o in bpy.context.scene.objects if o.type=='MESH'];count=0;finite=True;uv=True;materials=0;boundary=0;nonmanifold=0
  for obj in objects:
   obj.data.calc_loop_triangles();count+=len(obj.data.loop_triangles);finite &= all(all(math.isfinite(c) for c in v.co) for v in obj.data.vertices);uv &= bool(obj.data.uv_layers);materials+=len(obj.data.materials)
   bm=bmesh.new();bm.from_mesh(obj.data);bmesh.ops.remove_doubles(bm,verts=list(bm.verts),dist=1e-7);boundary+=sum(e.is_boundary for e in bm.edges);nonmanifold+=sum(not e.is_manifold and not e.is_boundary for e in bm.edges);bm.free()
  passed=count==row['triangles'] and finite and uv and len(objects)==1 and materials==1 and boundary==0 and nonmanifold==0;results.append(dict(sword=row['name'],format=extension,triangles=count,expected=row['triangles'],finite=finite,uv=uv,mesh_objects=len(objects),materials=materials,boundary_edges=boundary,nonmanifold_edges=nonmanifold,passed=passed))
(R/'qa/export_verification.json').write_text(json.dumps(results,indent=2));print('GREATSWORD_EXPORT_CHECKS',len(results),sum(r['passed'] for r in results));assert all(r['passed'] for r in results)
