import bpy,bmesh,json,math,csv,hashlib,sys
import numpy as np
from pathlib import Path
from mathutils import Vector,Matrix
R=Path(r'C:\The Veilbound\GreatswordAssetProduction');P=Path(r'C:\The Veilbound\Veilborne_Greatsword_Tripo_Pack');bpy.ops.wm.open_mainfile(filepath=str(R/'raw/Imported_Original.blend'))
raw=[o for o in bpy.context.scene.objects if o.type=='MESH'];raw.sort(key=lambda o:sum((o.matrix_world@v.co).y for v in o.data.vertices)/len(o.data.vertices));pal=json.loads((R/'sword_palette.json').read_text());assert len(raw)==10
raw_collection=bpy.data.collections.new('Original_Generated_Group_DO_NOT_EDIT');bpy.context.scene.collection.children.link(raw_collection)
for o in raw:
 for c in list(o.users_collection):c.objects.unlink(o)
 raw_collection.objects.link(o);o.hide_render=True;o.hide_set(True)
material=bpy.data.materials.new('Greatswords_Shared_Atlas');material.use_nodes=True;bs=material.node_tree.nodes.get('Principled BSDF');tree=material.node_tree
base=tree.nodes.new('ShaderNodeTexImage');base.image=bpy.data.images.load(str(R/'atlas/Greatswords_BaseColor.png'));base.interpolation='Closest';tree.links.new(base.outputs['Color'],bs.inputs['Base Color'])
orm=tree.nodes.new('ShaderNodeTexImage');orm.image=bpy.data.images.load(str(R/'atlas/Greatswords_MetallicSmoothness.png'));orm.image.colorspace_settings.name='Non-Color';orm.interpolation='Closest';sep=tree.nodes.new('ShaderNodeSeparateColor');tree.links.new(orm.outputs['Color'],sep.inputs[0]);tree.links.new(sep.outputs['Red'],bs.inputs['Metallic']);inv=tree.nodes.new('ShaderNodeMath');inv.operation='SUBTRACT';inv.inputs[0].default_value=1;mul=tree.nodes.new('ShaderNodeMath');mul.operation='MULTIPLY';mul.inputs[1].default_value=.45;tree.links.new(orm.outputs['Alpha'],mul.inputs[0]);tree.links.new(mul.outputs[0],inv.inputs[1]);tree.links.new(inv.outputs[0],bs.inputs['Roughness'])
em=tree.nodes.new('ShaderNodeTexImage');em.image=bpy.data.images.load(str(R/'atlas/Greatswords_Emission.png'));em.interpolation='Closest';tree.links.new(em.outputs['Color'],bs.inputs['Emission Color']);bs.inputs['Emission Strength'].default_value=.60
rows=[];processed=[]
for index,(source,tier) in enumerate(zip(raw,pal)):
 name=tier['name'];col=bpy.data.collections.new('Greatsword_'+name);bpy.context.scene.collection.children.link(col);obj=source.copy();obj.data=source.data.copy();obj.name='GS_'+name;col.objects.link(obj);obj.hide_set(False);obj.hide_render=False;obj.matrix_world=Matrix.Identity(4)
 for vertex,old in zip(obj.data.vertices,source.data.vertices):vertex.co=source.matrix_world@old.co
 bm=bmesh.new();bm.from_mesh(obj.data);before_faces=len(bm.faces);bmesh.ops.remove_doubles(bm,verts=list(bm.verts),dist=1e-7);bmesh.ops.dissolve_degenerate(bm,dist=1e-10,edges=list(bm.edges));bm.verts.ensure_lookup_table();seen=set();duplicates=[]
 for face in bm.faces:
  key=tuple(sorted(v.index for v in face.verts))
  if key in seen:duplicates.append(face)
  seen.add(key)
 if duplicates:bmesh.ops.delete(bm,geom=duplicates,context='FACES_ONLY')
 # Repair only small local boundary loops. Reject any cap that would replace a significant surface.
 original_area=sum(f.calc_area() for f in bm.faces);cap_count=0
 boundaries=[e for e in bm.edges if e.is_boundary]
 if boundaries:
  new_faces=bmesh.ops.holes_fill(bm,edges=boundaries,sides=32).get('faces',[]);cap_area=sum(f.calc_area() for f in new_faces)
  if cap_area>original_area*.008:raise RuntimeError(name+' has a substantial open surface; requires manual repair')
  cap_count=len(new_faces)
 if any(len(e.link_faces)>2 for e in bm.edges):
  bmesh.ops.split_edges(bm,edges=[e for e in bm.edges if len(e.link_faces)>2])
  boundary=[e for e in bm.edges if e.is_boundary]
  if boundary:
   new_faces=bmesh.ops.holes_fill(bm,edges=boundary,sides=16).get('faces',[])
   if sum(f.calc_area() for f in new_faces)>original_area*.008:raise RuntimeError(name+' nonmanifold seam cannot be safely capped')
   cap_count+=len(new_faces)
 # Split repair can expose two-triangle collapsed seam sheets. Remove only negligible shells.
 bm.faces.ensure_lookup_table();seen_faces=set();debris=[]
 for face in bm.faces:
  if face.index in seen_faces:continue
  pending=[face];seen_faces.add(face.index);shell=[]
  while pending:
   current=pending.pop();shell.append(current)
   for edge in current.edges:
    for other in edge.link_faces:
     if other.index not in seen_faces:seen_faces.add(other.index);pending.append(other)
  if len(shell)<8 and sum(f.calc_area() for f in shell)<original_area*.0001:debris.extend(shell)
 removed_seam_faces=len(debris)
 if debris:bmesh.ops.delete(bm,geom=debris,context='FACES')
 collapsed_edges=0;max_collapse=0
 for iteration in range(3):
  for face in [f for f in bm.faces if f.calc_area()<1e-12]:
   if face.is_valid:
    edge=min(face.edges,key=lambda e:e.calc_length());max_collapse=max(max_collapse,edge.calc_length());bmesh.ops.collapse(bm,edges=[edge],uvs=False);collapsed_edges+=1
  boundary_edges=[e for e in bm.edges if e.is_boundary]
  if boundary_edges:bmesh.ops.holes_fill(bm,edges=boundary_edges,sides=32)
 bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces))
 if bm.calc_volume(signed=True)<0:bmesh.ops.reverse_faces(bm,faces=list(bm.faces))
 bm.to_mesh(obj.data);bm.free();obj.data.update()
 points=np.array([tuple(v.co) for v in obj.data.vertices]);lo=points.min(0);hi=points.max(0);height=hi[2]-lo[2];u=(points[:,2]-lo[2])/height;grip=points[(u>.79)&(u<.90)];assert len(grip)>20
 centre_y=float(np.median(grip[:,1]));centre_x=float(np.median(grip[:,0]));guard_z=float(lo[2]+height*.70);scale=1.42/(guard_z-lo[2]);depth_scale=.55 if name=='Tungsten' else .35
 half_width=max(abs(grip[:,1]-centre_y))*scale;half_depth=max(abs(grip[:,0]-centre_x))*scale*depth_scale;grip_x_factor=min(1,.0275/half_width);grip_z_factor=min(1,.0275/half_depth)
 for vertex,p,amount in zip(obj.data.vertices,points,u):
  influence=min(1,max(0,(amount-.745)/.035),max(0,(.935-amount)/.035));x=(p[1]-centre_y)*scale*(1-influence*(1-grip_x_factor));z=(p[0]-centre_x)*scale*depth_scale*(1-influence*(1-grip_z_factor));length=.30+(guard_z-p[2])*scale;vertex.co=(x,-length,z)
 obj.data.materials.clear();obj.data.materials.append(material);uv=obj.data.uv_layers.active or obj.data.uv_layers.new(name='SharedAtlas');obj.data.update()
 ref_path=sorted((P/'references/individual_fallback').glob(f'{index+1:02d}_*.png'))[0];ref=bpy.data.images.load(str(ref_path));w,h=ref.size;pixels=np.array(ref.pixels[:]).reshape(h,w,4)
 for face in obj.data.polygons:
  original=np.mean(points[list(face.vertices)],axis=0);fraction=(original[2]-lo[2])/height;horizontal=(original[1]-lo[1])/(hi[1]-lo[1]);px=min(w-1,max(0,round(horizontal*(w-1))));py=min(h-1,max(0,round(fraction*(h-1))));color=pixels[py,px,:3];r,g,b=color;tile=0
  if .77<fraction<.905:tile=8 if int(fraction*90)%3==0 else 5
  elif name=='Veilcrystal':
   offset=abs((original[1]-centre_y)/(hi[1]-lo[1]))
   tile=6 if (offset>.17 and fraction<.76) or (.25<fraction<.52 and offset<.07) or fraction>.935 else 0
  elif name=='Titanium':
   offset=abs((original[1]-centre_y)/(hi[1]-lo[1]))
   if .65<fraction<.725 and offset<.10:tile=6
   elif .17<fraction<.63 and offset<.028:tile=6
   elif (.17<fraction<.63 and .028<offset<.055) or (.64<fraction<.75 and offset>.25) or .905<fraction<.93:tile=4
   else:tile=0
  elif fraction>.91 or .63<fraction<.76:tile=4
  else:tile=1 if .12<max(color)<.37 else 2 if max(color)>.78 else 0
  face.use_smooth=False
  for loop in face.loop_indices:uv.data[loop].uv=((tile+.5)/16,1-(index+.5)/10)
 obj.data.update();obj.data.calc_loop_triangles();mesh=obj.data;bm=bmesh.new();bm.from_mesh(mesh);boundary=sum(e.is_boundary for e in bm.edges);nonmanifold=sum(not e.is_manifold and not e.is_boundary for e in bm.edges);volume=bm.calc_volume(signed=True);degenerate=sum(f.calc_area()<1e-12 for f in bm.faces);bm.free()
 final=np.array([tuple(v.co) for v in mesh.vertices]);length=-final[:,1];blade=final[(length>.55)&(length<1.40)];blade_width=float(np.ptp(blade[:,0]));blade_thickness=float(np.ptp(blade[:,2]));tip=float(length.max());pommel=float(length.min())
 complete=tip>1.70 and pommel<-.10 and blade_width>.08 and blade_thickness>.018 and volume>1e-5 and boundary==0 and nonmanifold==0 and degenerate==0
 row=dict(name=obj.name,material=name,metal_index=tier['metal_index'],raw_object=source.name,triangles=len(mesh.loop_triangles),raw_triangles=source.data.loop_triangles.__len__() if source.data.loop_triangles else len(source.data.polygons),components_grouped=1,boundary_edges=boundary,nonmanifold_edges=nonmanifold,degenerate_faces=degenerate,volume_m3=volume,blade_width_m=blade_width,blade_thickness_m=blade_thickness,tip_z_m=tip,pommel_z_m=pommel,guard_z_m=.30,grip_main=[0,.035,.015],grip_off=[0,.035,.17],caps_added=cap_count,seam_fragment_faces_removed=removed_seam_faces,collapsed_degenerate_edges=collapsed_edges,max_collapsed_edge_raw=max_collapse,duplicate_faces_removed=len(duplicates),decimated=False,passed=complete,glb=str(R/f'exports/{obj.name}.glb'),fbx=str(R/f'exports/{obj.name}.fbx'))
 rows.append(row);processed.append(obj)
 if not complete:print('GEOMETRY_REVIEW_REQUIRED',row);continue
 for label,offset in [('MainHand_Grip',(0,-.015,.035)),('OffHand_Grip',(0,-.17,.035)),('Blade_Tip',(0,-1.72,0))]:
  marker=bpy.data.objects.new(label,None);col.objects.link(marker);marker.parent=obj;marker.location=offset;marker.empty_display_size=.025;marker.empty_display_type='SPHERE'
 bpy.ops.object.select_all(action='DESELECT');obj.select_set(True);bpy.context.view_layer.objects.active=obj
 bpy.ops.export_scene.gltf(filepath=row['glb'],export_format='GLB',use_selection=True,export_animations=False)
 bpy.ops.export_scene.fbx(filepath=row['fbx'],use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',mesh_smooth_type='FACE',bake_anim=False,path_mode='COPY',embed_textures=True)
# Keep the fitted objects in their socket space; the preview gallery uses instances.
for obj in processed:obj.hide_render=True;obj.hide_set(True)
gallery=bpy.data.collections.new('Preview_Lineup');bpy.context.scene.collection.children.link(gallery)
for i,obj in enumerate(processed):
 copy=obj.copy();copy.data=obj.data;gallery.objects.link(copy);copy.hide_set(False);copy.hide_render=False;copy.location.x=(i-4.5)*.83;copy['preview_instance']=True
scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=16;scene.render.resolution_x=1800;scene.render.resolution_y=600;scene.render.resolution_percentage=100;scene.world=scene.world or bpy.data.worlds.new('Review_World');scene.world.color=(.16,.16,.16)
world=scene.world;world.use_nodes=True;world.node_tree.nodes['Background'].inputs[0].default_value=(.13,.15,.18,1);world.node_tree.nodes['Background'].inputs[1].default_value=.6
camera=bpy.data.objects.new('Review_Camera',bpy.data.cameras.new('Review_Camera'));scene.collection.objects.link(camera);scene.camera=camera;camera.data.type='ORTHO'
def look(at,position):
 camera.location=position;back=(camera.location-at).normalized();right=Vector((0,1,0)).cross(back).normalized();up=back.cross(right).normalized();camera.rotation_euler=Matrix((right,up,back)).transposed().to_euler()
def area(name,loc,power,size):
 light=bpy.data.objects.new(name,bpy.data.lights.new(name,'AREA'));scene.collection.objects.link(light);light.location=loc;light.data.energy=power;light.data.shape='DISK';light.data.size=size;light.rotation_euler=(Vector((0,-.6,0))-light.location).to_track_quat('-Z','Y').to_euler()
area('Key',(-3,1,5),650,5);area('Fill',(4,-1,3),400,5);area('Rim',(0,-1,-3),450,4)
camera.data.ortho_scale=9;look(Vector((0,-.68,0)),Vector((0,-.68,6)));scene.render.filepath=str(R/'previews/ALL_TEN_FRONT.png');bpy.ops.render.render(write_still=True)
for o in gallery.objects:o.hide_render=True
scene.render.resolution_x=600;scene.render.resolution_y=1000
for obj in processed:
 if '--focus' in sys.argv and obj.name not in ['GS_Titanium','GS_Veilcrystal']:continue
 obj.hide_render=False;camera.data.ortho_scale=1.48;at=Vector((0,-.68,0))
 for view,position in [('front',Vector((0,-.68,6))),('side',Vector((6,-.68,0))),('three_quarter',Vector((3,-.68,6)))]:
  look(at,position);scene.render.filepath=str(R/f'previews/{obj.name}_{view}.png');bpy.ops.render.render(write_still=True)
 obj.hide_render=True
for o in gallery.objects:o.hide_render=False
scene.render.resolution_x=1800;scene.render.resolution_y=600;camera.data.ortho_scale=9;look(Vector((0,-.68,0)),Vector((0,-.68,6)))
(R/'qa/processed_swords.json').write_text(json.dumps(rows,indent=2));
with (R/'SWORD_MANIFEST.csv').open('w',newline='') as file:
 writer=csv.DictWriter(file,fieldnames=rows[0].keys());writer.writeheader();writer.writerows(rows)
bpy.ops.wm.save_as_mainfile(filepath=str(R/'Greatswords_Processed.blend'));print('PROCESSED_SWORDS',len(rows),sum(r['passed'] for r in rows),'TRIANGLES',sum(r['triangles'] for r in rows))
