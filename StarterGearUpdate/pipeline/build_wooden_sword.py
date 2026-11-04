import bpy, math, json
from pathlib import Path
R=Path(r'C:\The Veilbound\StarterGearUpdate');R.mkdir(exist_ok=True)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
image=bpy.data.images.new('Wooden sword palette',width=128,height=32)
colors=[(.26,.125,.048,1),(.37,.21,.095,1),(.16,.075,.027,1),(.44,.28,.13,1)]
image.pixels=[c for y in range(32) for x in range(128) for c in colors[x//32]]
image.filepath_raw=str(R/'WoodenSword_Palette.png');image.file_format='PNG';image.save()
mat=bpy.data.materials.new('Dry worn wood');mat.use_nodes=True;bs=mat.node_tree.nodes.get('Principled BSDF');bs.inputs['Metallic'].default_value=0;bs.inputs['Roughness'].default_value=.88
node=mat.node_tree.nodes.new('ShaderNodeTexImage');node.image=image;node.interpolation='Closest';mat.node_tree.links.new(node.outputs['Color'],bs.inputs['Base Color'])
parts=[]
def prism(name,outline,depth,tile):
 n=len(outline);v=[(x,-y,z) for z in [-depth/2,depth/2] for x,y in outline]
 faces=[tuple(range(n-1,-1,-1)),tuple(range(n,n*2))]+[(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)]
 mesh=bpy.data.meshes.new(name);mesh.from_pydata(v,[],faces);mesh.update();o=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(o);bpy.context.view_layer.objects.active=o;o.select_set(True)
 bevel=o.modifiers.new('Soft worn plank edges','BEVEL');bevel.width=.006;bevel.segments=1;bpy.ops.object.modifier_apply(modifier=bevel.name)
 o.data.materials.append(mat);uv=o.data.uv_layers.new(name='WoodPalette')
 for poly in o.data.polygons:
  t=(tile+(1 if abs(poly.normal.z)<.5 else 0))%4
  for loop in poly.loop_indices:uv.data[loop].uv=((t+.5)/4,.5)
 parts.append(o);o.select_set(False)
prism('Solid carved wooden blade',[(-.095,.33),(.095,.33),(.10,1.48),(.045,1.66),(0,1.72),(-.045,1.66),(-.10,1.48)],.055,1)
prism('Wooden crossguard',[(-.285,.265),(.285,.265),(.27,.34),(-.27,.34)],.075,0)
prism('Long two handed wood grip',[(-.028,-.24),(.028,-.24),(.029,.265),(-.029,.265)],.055,0)
prism('Wooden pommel',[(-.045,-.309),(.045,-.309),(.05,-.25),(-.05,-.25)],.075,2)
# A few inset-colour grain strips preserve the simple plank appearance without texture baking.
for x,start,end in [(-.041,.43,1.37),(.028,.59,1.51)]:
 for side in [-1,1]:
  prism('Subtle wood grain',[(x-.0015,start),(x+.0015,start+.05),(x+.002,end),(x-.001,end-.04)],.001,2);parts[-1].location.z=side*.028
for o in parts:o.select_set(True)
bpy.context.view_layer.objects.active=parts[0];bpy.ops.object.join();o=bpy.context.object;o.name='GS_Wooden'
bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
bpy.ops.object.mode_set(mode='EDIT');bpy.ops.mesh.select_all(action='SELECT');bpy.ops.mesh.normals_make_consistent(inside=False);bpy.ops.object.mode_set(mode='OBJECT')
bpy.ops.wm.save_as_mainfile(filepath=str(R/'WoodenSword.blend'));bpy.ops.export_scene.fbx(filepath=str(R/'GS_Wooden.fbx'),use_selection=True,axis_forward='-Z',axis_up='Y',bake_anim=False,add_leaf_bones=False)
bpy.ops.export_scene.gltf(filepath=str(R/'GS_Wooden.glb'),export_format='GLB',use_selection=True)
o.data.calc_loop_triangles();(R/'wood_geometry.json').write_text(json.dumps(dict(triangles=len(o.data.loop_triangles),material_slots=len(o.data.materials),paid_credits=0),indent=2))
print('WOODEN_SWORD_BUILT',len(o.data.loop_triangles))
