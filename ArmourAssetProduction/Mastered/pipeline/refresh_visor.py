import bpy,sys,json
from pathlib import Path
from mathutils import Matrix,Vector
sys.path.insert(0,str(Path(__file__).resolve().parent));from visor_finish import apply_visor
from cloth_finish import separate_belt_cloth
R=Path(r'C:\The Veilbound\ArmourAssetProduction\Mastered');results=[]
for variant in ['Male','Female']:
 bpy.ops.wm.open_mainfile(filepath=str(R/f'blender/Master_{variant}.blend'))
 arm=next(o for o in bpy.context.scene.objects if o.type=='ARMATURE');arm.animation_data.action=None
 for b in arm.pose.bones:b.matrix_basis=Matrix.Identity(4)
 bpy.context.view_layer.update()
 gear=[o for o in bpy.context.scene.objects if o.type=='MESH' and o.get('tier') in ['Tin','Copper']]
 def select(objects):
  bpy.ops.object.select_all(action='DESELECT');arm.select_set(True)
  root=bpy.data.objects.get('Veilborne_'+variant)
  if root:root.select_set(True)
  for o in objects:o.select_set(True)
  bpy.context.view_layer.objects.active=arm
 def export(path,objects):
  select(objects);bpy.ops.export_scene.fbx(filepath=str(path),use_selection=True,object_types={'EMPTY','ARMATURE','MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',add_leaf_bones=False,mesh_smooth_type='FACE',bake_anim=False,path_mode='COPY',embed_textures=True)
 for family in ['Tin','Copper']:
  head=bpy.data.objects[family+'_Head'];faces=apply_visor(head);assert faces
  cloth_faces=separate_belt_cloth(bpy.data.objects[family+'_Belt']);assert cloth_faces
  export(R/f'exports/fitted/{variant}/{family}/{family}_{variant}_Belt.fbx',[bpy.data.objects[family+'_Belt']])
  results.append(dict(variant=variant,family=family,visor_faces=faces,material_slots=len(head.data.materials)))
  objects=[o for o in gear if o['tier']==family];folder=R/f'exports/fitted/{variant}/{family}'
  export(folder/f'{family}_{variant}_Head.fbx',[head]);export(folder/f'{family}_{variant}_AllSlots.fbx',objects)
  select(objects);bpy.ops.export_scene.gltf(filepath=str(folder/f'{family}_{variant}_AllSlots.glb'),export_format='GLB',use_selection=True,export_animations=False)
 bpy.ops.wm.save_as_mainfile(filepath=str(R/f'blender/Master_{variant}.blend'))
(R/'qa/black_visor.json').write_text(json.dumps(results,indent=2));print('BLACK_VISOR_UPDATED',results)
