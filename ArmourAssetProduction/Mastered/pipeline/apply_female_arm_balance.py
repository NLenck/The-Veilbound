import bpy,sys,json,shutil,hashlib
from pathlib import Path
from mathutils import Matrix
sys.path.insert(0,str(Path(__file__).resolve().parent));from female_arm_balance import balance_female_arms
R=Path(r'C:\The Veilbound\ArmourAssetProduction\Mastered');backup=R/'revisions/before_female_arm_balance';backup.mkdir(exist_ok=True)
for source in [R/'blender/Master_Female.blend',R/'previews/Tin_Female_Master.png',R/'previews/Copper_Female_Master.png']:
 if not (backup/source.name).exists():shutil.copy2(source,backup/source.name)
male=R/'blender/Master_Male.blend';male_hash=hashlib.sha256(male.read_bytes()).hexdigest()
bpy.ops.wm.open_mainfile(filepath=str(R/'blender/Master_Female.blend'));arm=next(o for o in bpy.context.scene.objects if o.type=='ARMATURE');arm.animation_data.action=None
for bone in arm.pose.bones:bone.matrix_basis=Matrix.Identity(4)
bpy.context.view_layer.update();gear=[o for o in bpy.context.scene.objects if o.type=='MESH' and o.get('tier') in ['Tin','Copper']];changes=balance_female_arms(gear,arm)
def select(objects):
 bpy.ops.object.select_all(action='DESELECT');arm.select_set(True);root=bpy.data.objects.get('Veilborne_Female')
 if root:root.select_set(True)
 for o in objects:o.select_set(True)
 bpy.context.view_layer.objects.active=arm
def export(path,objects):
 select(objects);bpy.ops.export_scene.fbx(filepath=str(path),use_selection=True,object_types={'EMPTY','ARMATURE','MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',add_leaf_bones=False,mesh_smooth_type='FACE',bake_anim=False,path_mode='COPY',embed_textures=True)
for family in ['Tin','Copper']:
 objects=[o for o in gear if o['tier']==family];folder=R/f'exports/fitted/Female/{family}';export(folder/f'{family}_Female_AllSlots.fbx',objects);select(objects);bpy.ops.export_scene.gltf(filepath=str(folder/f'{family}_Female_AllSlots.glb'),export_format='GLB',use_selection=True,export_animations=False)
 for slot,label in [(1,'Shoulders'),(3,'Hands'),(5,'Bracers')]:export(folder/f'{family}_Female_{label}.fbx',[o for o in objects if o['equipment_slot']==slot])
bpy.ops.wm.save_as_mainfile(filepath=str(R/'blender/Master_Female.blend'));assert hashlib.sha256(male.read_bytes()).hexdigest()==male_hash
(R/'qa/female_arm_balance.json').write_text(json.dumps(dict(male_master_unchanged=True,male_sha256=male_hash,changes=changes),indent=2));print('FEMALE_ARM_BALANCE',len(changes))
