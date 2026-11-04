import bpy,bmesh,json,numpy as np
from pathlib import Path
R=Path(r'C:\The Veilbound\GreatswordAssetProduction');bpy.ops.wm.open_mainfile(filepath=str(R/'Greatswords_Processed.blend'));rows=json.loads((R/'qa/processed_swords.json').read_text());checks=[]
for row in rows:
 o=bpy.data.objects[row['name']];bm=bmesh.new();bm.from_mesh(o.data);bad_normals=sum(e.is_manifold and not e.is_contiguous for e in bm.edges);bm.free();assert bad_normals==0;
 a=np.array([list(v.co) for v in o.data.vertices]);length=-a[:,1];guard=a[np.abs(length-.30)<.12];grip=a[np.abs(length-.015)<.025];pommel=a[length<-.20];blade=a[(length>.55)&(length<1.4)]
 width=lambda p:float(np.ptp(p[:,0]));depth=lambda p:float(np.ptp(p[:,2]));complete=width(guard)>width(blade)*1.4 and .025<width(grip)<.10 and len(pommel)>15 and depth(blade)>.018
 checks.append(dict(sword=row['name'],blade=True,guard_width_m=width(guard),grip_width_m=width(grip),pommel_width_m=width(pommel),blade_width_m=width(blade),blade_thickness_m=depth(blade),complete=complete,normal_winding_errors=bad_normals));assert complete,row['name']
tung=next(x for x in checks if x['sword']=='GS_Tungsten');titan=next(x for x in checks if x['sword']=='GS_Titanium');weight=dict(tungsten_to_titanium_blade_width=tung['blade_width_m']/titan['blade_width_m'],tungsten_to_titanium_blade_thickness=tung['blade_thickness_m']/titan['blade_thickness_m']);assert weight['tungsten_to_titanium_blade_width']>1.15 and weight['tungsten_to_titanium_blade_thickness']>1.2
(R/'qa/component_completeness.json').write_text(json.dumps(dict(swords=checks,heavy_silhouette=weight,passed=True),indent=2));bpy.ops.file.pack_all();bpy.ops.wm.save_as_mainfile(filepath=str(R/'Greatswords_Processed.blend'));print('ALL_TEN_COMPLETE',weight)
