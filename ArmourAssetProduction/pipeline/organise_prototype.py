"""Classify only disconnected objects. Unmatched or duplicate slots fail the gate."""
import bpy,json,math,csv
from pathlib import Path
from mathutils import Vector
R=Path(r'C:\The Veilbound\ArmourAssetProduction')
bpy.ops.wm.open_mainfile(filepath=str(R/'blender/Prototype_Grouped_Inspection.blend'))
audit=json.loads((R/'qa/prototype_components.json').read_text());rows=audit['components']
heads=sorted([r for r in rows if r['centre'][2]>.28 and r['dimensions'][2]<.2],key=lambda r:r['centre'][1])
assert len(heads)==2,'Two distinct helmets required; inspect manually instead of inventing tier assignments.'
centres=[r['centre'][1] for r in heads];classified=[];unknown=[]
slot_map={'Head':0,'Shoulder':1,'Chest':2,'Hand':3,'Belt':4,'Bracer':5,'Leg':6,'Foot':7,'Cape':8}
for row in rows:
    o=bpy.data.objects[row['name']];x,y,z=row['centre'];dx,dy,dz=row['dimensions']
    tier_i=min(range(2),key=lambda i:abs(y-centres[i]));tier=['Tin','Copper'][tier_i]
    side='L' if y>centres[tier_i] else 'R'
    if dz>.4:slot='Cape'
    elif z>.28:slot='Head'
    elif z>.18:slot='Shoulder'
    elif z>.10 and dy>.12:slot='Chest'
    elif z>.015:slot='Bracer'
    elif z<-.30:slot='Foot'
    elif z<-.10:slot='Leg'
    elif dy>.12:slot='Belt'
    elif -.08<z<.015:slot='Hand'
    else:unknown.append(row);continue
    name=tier+'_'+slot+('_'+side if slot in ['Shoulder','Bracer','Hand','Leg','Foot'] else '')
    assert not any(r['name']==name for r in classified),'Duplicate inferred slot: '+name
    col=bpy.data.collections.get('Plate_'+tier) or bpy.data.collections.new('Plate_'+tier)
    if not col.name in bpy.context.scene.collection.children:bpy.context.scene.collection.children.link(col)
    for previous in list(o.users_collection):previous.objects.unlink(o)
    col.objects.link(o);o.name=name;o['equipment_slot']=slot_map[slot];o['tier']=tier;o['side']=side if slot in ['Shoulder','Bracer','Hand','Leg','Foot'] else ''
    o['classification']='spatial rule validated against two-view inspection; no semantic mesh cutting'
    classified.append(dict(candidate=row['name'],name=name,tier=tier,slot=slot,slot_index=slot_map[slot],side=o['side'],triangles=row['triangles'],source=row['source'],centre=row['centre'],dimensions=row['dimensions']))
expected={'Head':1,'Shoulder':2,'Chest':1,'Bracer':2,'Hand':2,'Belt':1,'Leg':2,'Foot':2,'Cape':1}
completeness=[]
for tier in ['Tin','Copper']:
    for slot,n in expected.items():
        found=[r for r in classified if r['tier']==tier and r['slot']==slot]
        completeness.append(dict(tier=tier,slot=slot,expected_objects=n,actual_objects=len(found),triangles=sum(r['triangles'] for r in found),separable=len(found)==n,coverage_note='Knee/shin only; upper-thigh coverage missing from reference/generated legs.' if slot=='Leg' else 'Independent component; fitting and motion acceptance separate.'))
assert not unknown and all(r['separable'] for r in completeness),'Do not pass incomplete component extraction.'
# Material atlas: all tiers and cloth share one material; flat UV swatches avoid textures per piece.
mat=bpy.data.materials.new('Veilbound_Plate_Atlas');mat.use_nodes=True
bs=mat.node_tree.nodes.get('Principled BSDF');bs.inputs['Roughness'].default_value=.65
for channel in ['BaseColor','ORM']:
    im=bpy.data.images.load(str(R/f'exports/atlas/Plate_{channel}.png'),check_existing=True)
    tex=mat.node_tree.nodes.new('ShaderNodeTexImage');tex.image=im;tex.interpolation='Closest'
    if channel=='BaseColor':mat.node_tree.links.new(tex.outputs['Color'],bs.inputs['Base Color'])
    else:
        im.colorspace_settings.name='Non-Color';sep=mat.node_tree.nodes.new('ShaderNodeSeparateColor');mat.node_tree.links.new(tex.outputs['Color'],sep.inputs[0]);mat.node_tree.links.new(sep.outputs['Green'],bs.inputs['Roughness']);mat.node_tree.links.new(sep.outputs['Blue'],bs.inputs['Metallic'])
for row in classified:
    o=bpy.data.objects[row['name']];o.data.materials.clear();o.data.materials.append(mat)
    uv=o.data.uv_layers.active or o.data.uv_layers.new(name='AtlasUV')
    lo=min(v.co.z for v in o.data.vertices);hi=max(v.co.z for v in o.data.vertices)
    centre_y=centres[['Tin','Copper'].index(row['tier'])]
    for poly in o.data.polygons:
        c=poly.center;tile=0 if row['tier']=='Tin' else 1
        if row['slot']=='Cape' or (row['slot']=='Belt' and c.z<lo+(hi-lo)*.65 and abs(c.y-centre_y)<.028):tile=10
        for k in poly.loop_indices:uv.data[k].uv=((tile+.5)/16,.5)
        poly.use_smooth=False
    if row['slot']=='Cape':
        original=o.data.copy();mod=o.modifiers.new('Silhouette-preserving cape reduction','DECIMATE');mod.ratio=.30
        bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=mod.name)
        from mathutils.bvhtree import BVHTree
        o.data.calc_loop_triangles();tree=BVHTree.FromPolygons([v.co for v in o.data.vertices],[t.vertices[:] for t in o.data.loop_triangles],all_triangles=True)
        distances=[tree.find_nearest(v.co)[3] for v in original.vertices]
        error=max(distances);row['cape_max_surface_deviation_raw_units']=error
        # Do not accept a reduction that moves the cloth silhouette by >1% of scene height.
        if error>.008:o.data=original;row['cape_reduction_accepted']=False
        else:row['cape_reduction_accepted']=True
    o.data.calc_loop_triangles();row['export_triangles']=len(o.data.loop_triangles)
    # Exploded outputs preserve authored transforms; fitted outputs get rig attachment pivots later.
    bpy.ops.object.select_all(action='DESELECT');o.select_set(True);bpy.context.view_layer.objects.active=o
    target=R/f'exports/exploded/{row["tier"]}';target.mkdir(parents=True,exist_ok=True)
    bpy.ops.export_scene.gltf(filepath=str(target/(o.name+'.glb')),export_format='GLB',use_selection=True,export_animations=False)
for tier in ['Tin','Copper']:
    bpy.ops.object.select_all(action='DESELECT')
    for o in bpy.data.collections['Plate_'+tier].objects:o.select_set(True)
    bpy.ops.export_scene.gltf(filepath=str(R/f'exports/exploded/{tier}/{tier}_Exploded.glb'),export_format='GLB',use_selection=True,export_animations=False)
(R/'qa/classification.json').write_text(json.dumps(dict(objects=classified,slots=completeness,unknown=unknown,set_centres_raw_y=centres,complete_slots=18,total_slots=18,bulk_gate='two-set separation passed; five-set scalability and fitting acceptance unproven'),indent=2))
with (R/'qa/slot_completeness.csv').open('w',newline='') as f:
    w=csv.DictWriter(f,fieldnames=completeness[0].keys());w.writeheader();w.writerows(completeness)
bpy.ops.wm.save_as_mainfile(filepath=str(R/'blender/Prototype_Organised_Exploded.blend'))
print('CLASSIFICATION_PASS',len(classified),'objects; 18 separable equipment slots')
