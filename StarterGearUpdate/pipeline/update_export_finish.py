from pathlib import Path
import bpy, shutil
R=Path(r'C:\The Veilbound\GreatswordAssetProduction');backup=Path(r'C:\The Veilbound\StarterGearUpdate\Backup')
p=R/'Greatswords_Processed.blend';target=backup/'Greatswords_Processed.blend'
if not target.exists():shutil.copy2(p,target)
bpy.ops.wm.open_mainfile(filepath=str(p))
for mat in bpy.data.materials:
 if not mat.use_nodes:continue
 tree=mat.node_tree
 for node in list(tree.nodes):
  if node.type=='MATH' and node.operation=='SUBTRACT' and node.inputs[1].is_linked:
   link=node.inputs[1].links[0]
   if link.from_socket.name!='Alpha':continue
   multiply=tree.nodes.new('ShaderNodeMath');multiply.name='Equipment matte finish';multiply.operation='MULTIPLY';multiply.inputs[1].default_value=.45
   tree.links.new(link.from_socket,multiply.inputs[0]);tree.links.new(multiply.outputs[0],node.inputs[1])
for obj in bpy.data.objects:
 if obj.type!='MESH' or obj.name not in ['GS_'+n for n in ['Tin','Copper','Bronze','Brass','Iron','Steel','HardenedSteel','Tungsten','Titanium','Veilcrystal']]:continue
 bpy.ops.object.select_all(action='DESELECT');obj.hide_set(False);obj.select_set(True);bpy.context.view_layer.objects.active=obj
 bpy.ops.export_scene.gltf(filepath=str(R/'exports'/f'{obj.name}.glb'),export_format='GLB',use_selection=True)
 bpy.ops.export_scene.fbx(filepath=str(R/'exports'/f'{obj.name}.fbx'),use_selection=True,axis_forward='-Z',axis_up='Y',bake_anim=False,add_leaf_bones=False)
bpy.ops.file.pack_all();bpy.ops.wm.save_as_mainfile(filepath=str(p));print('MATTE_EXPORTS_SAVED')
