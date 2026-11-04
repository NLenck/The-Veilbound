import bpy,sys,shutil
from pathlib import Path
R=Path(r'C:\The Veilbound\ArmourAssetProduction\Mastered');variant=sys.argv[sys.argv.index('--')+1]
backup=Path(r'C:\The Veilbound\StarterGearUpdate\Backup\Armour');backup.mkdir(parents=True,exist_ok=True)
source=R/'pipeline/render_masters.py';text=source.read_text()
old="material.node_tree.links.new(orm.outputs['Alpha'],inverse.inputs[1])"
new="multiply=material.node_tree.nodes.new('ShaderNodeMath');multiply.operation='MULTIPLY';multiply.inputs[1].default_value=.45;material.node_tree.links.new(orm.outputs['Alpha'],multiply.inputs[0]);material.node_tree.links.new(multiply.outputs[0],inverse.inputs[1])"
if old in text:
 if not (backup/'render_masters.py').exists():shutil.copy2(source,backup/'render_masters.py')
 text=text.replace(old,new);source.write_text(text)
target=R/f'blender/Master_{variant}.blend'
if not (backup/target.name).exists():shutil.copy2(target,backup/target.name)
# Re-export the existing ten tiers without repeating unchanged preview renders.
text=text.replace('bpy.ops.render.render(write_still=True)','None')
exec(compile(text,str(source),'exec'))
print('MATTE_ARMOUR_EXPORTS_UPDATED',variant)
