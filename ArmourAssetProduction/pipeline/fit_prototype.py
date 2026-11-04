"""Local fitting drafts, not an assertion of final art or motion acceptance."""
import bpy,json,math,sys
from pathlib import Path
from mathutils import Vector,Matrix,geometry
from mathutils.bvhtree import BVHTree
R=Path(r'C:\The Veilbound\ArmourAssetProduction');P=R.parent/'PlayerAssetProduction'
classification=json.loads((R/'qa/classification.json').read_text())
variant=sys.argv[sys.argv.index('--')+1];female=variant=='Female';scale=1.73/1.85 if female else 1
bpy.ops.wm.open_mainfile(filepath=str(P/f'models_blender/{variant}/{variant}.blend'))
arm=next(o for o in bpy.context.scene.objects if o.type=='ARMATURE');arm.animation_data.action=None
for b in arm.pose.bones:b.matrix_basis=Matrix.Identity(4)
bpy.context.view_layer.update()
body=max([o for o in bpy.context.scene.objects if o.type=='MESH' and any(m.type=='ARMATURE' for m in o.modifiers)],key=lambda o:len(o.data.vertices))
body.data.calc_loop_triangles();points=[body.matrix_world@v.co for v in body.data.vertices]
tris=[tuple(t.vertices) for t in body.data.loop_triangles if body.data.polygons[t.polygon_index].material_index==0]
bvh=BVHTree.FromPolygons(points,tris,all_triangles=True)
weights=[{body.vertex_groups[g.group].name:g.weight for g in v.groups} for v in body.data.vertices]
def B(name):return arm.matrix_world@arm.data.bones['mixamorig:'+name].head_local
def T(name):return arm.matrix_world@arm.data.bones['mixamorig:'+name].tail_local
def sample(p,allowed,fallback):
    at,normal,idx,d=bvh.find_nearest(p);tri=tris[idx]
    bary=geometry.barycentric_transform(at,*[points[i] for i in tri],Vector((1,0,0)),Vector((0,1,0)),Vector((0,0,1)))
    out={}
    for i,w in zip(tri,bary):
        for name,value in weights[i].items():
            if allowed(name):out[name]=out.get(name,0)+max(0,w)*value
    total=sum(out.values())
    return ({name:value/total for name,value in out.items() if value/total>.0001} if total>.00001 else {'mixamorig:'+fallback:1}),at,normal,d
rows=classification['objects'];names=[r['name'] for r in rows]
with bpy.data.libraries.load(str(R/'blender/Prototype_Organised_Exploded.blend'),link=False) as (source,target):target.objects=names
loaded={o.name:o for o in target.objects}
fitted=[];audit=[]
for row in rows:
    o=loaded[row['name']];slot=row['slot'];side='Left' if row['side']=='L' else 'Right';f=scale
    col=bpy.data.collections.get(f'Plate_{row["tier"]}_{variant}')
    if not col:col=bpy.data.collections.new(f'Plate_{row["tier"]}_{variant}');bpy.context.scene.collection.children.link(col)
    col.objects.link(o)
    centre=Vector(row['centre']);source_size=Vector((row['dimensions'][1],row['dimensions'][0],row['dimensions'][2]))
    rotation=Matrix.Identity(3);allowed=lambda n:True
    if slot=='Head':
        pivot=B('Head');target=pivot+Vector((.025*f,-.005*f,.125*f));size=Vector((.31,.32,.35))*f;fallback='Head';allowed=lambda n:n.endswith('Head')
    elif slot=='Shoulder':
        pivot=B(side+'Arm');target=pivot+Vector(((.025 if side=='Left' else -.025)*f,0,.025*f));size=Vector((.225,.23,.23))*f;fallback=side+'Arm'
    elif slot=='Chest':
        pivot=B('Spine1');target=(B('Spine')+T('Spine1'))/2+Vector((0,0,.025*f));size=Vector((.43 if not female else .38,.31 if not female else .29,.39))*f;fallback='Spine1';allowed=lambda n:any(n.endswith(k) for k in ['Hips','Spine','Spine1','Neck'])
    elif slot in ['Bracer','Hand','Leg']:
        bone=side+{'Bracer':'ForeArm','Hand':'Hand','Leg':'Leg'}[slot];pivot=B(bone);target=(B(bone)+T(bone))/2
        if slot=='Hand':target=B(bone)+(T(bone)-B(bone))*.9
        length=(B(bone)-T(bone)).length
        size=Vector((.125,.14,length*.84)) if slot=='Bracer' else Vector((.115,.10,.155))*f if slot=='Hand' else Vector((.17,.20,length*1.04))
        rotation=Vector((0,0,1)).rotation_difference((B(bone)-T(bone)).normalized()).to_matrix();fallback=bone
        if slot=='Bracer':allowed=lambda n,s=side:any(n.endswith(s+k) for k in ['ForeArm','Hand'])
        elif slot=='Leg':allowed=lambda n,s=side:any(n.endswith(s+k) for k in ['UpLeg','Leg','Foot'])
        else:allowed=lambda n,s=side:s+'Hand' in n
    elif slot=='Foot':
        pivot=B(side+'Foot');target=pivot+Vector((0,-.045*f,-.035*f));size=Vector((.19,.32,.19))*f;fallback=side+'Foot';allowed=lambda n,s=side:any(n.endswith(s+k) for k in ['Foot','ToeBase'])
    elif slot=='Belt':
        pivot=B('Hips');target=pivot+Vector((0,0,-.105*f));size=Vector((.46 if not female else .43,.34,.40))*f;fallback='Hips';allowed=lambda n:any(n.endswith(k) for k in ['Hips','Spine','LeftUpLeg','RightUpLeg'])
    elif slot=='Cape':
        pivot=T('Spine1');target=pivot+Vector((0,.19*f,-.53*f));size=Vector((.43,.075,1.14))*f;fallback='Spine1'
    else:raise ValueError(slot)
    positions=[];skin=[];push_count=0;max_push=0
    for vertex in o.data.vertices:
        p=vertex.co-centre;local=Vector((p.y,-p.x,p.z));local=Vector(tuple(local[k]/source_size[k]*size[k] for k in range(3)))
        point=target+rotation@local
        if slot=='Cape':
            top=max(0,min(1,(point.z-(target.z-size.z/2))/size.z));skin.append({'mixamorig:Spine1':.2+.8*top,'mixamorig:Hips':.8*(1-top)})
        elif slot=='Shoulder':skin.append({'mixamorig:'+side+'Arm':.65,'mixamorig:Spine1':.35})
        elif slot in ['Head','Foot']:skin.append({'mixamorig:'+fallback:1})
        else:
            w,at,normal,d=sample(point,allowed,fallback);skin.append(w)
            signed=(point-at).dot(normal)
            if signed<.006:
                push=min(.06,.006-signed);point+=normal*push;push_count+=1;max_push=max(max_push,push)
        positions.append(point)
    # Each object pivot is at the relevant attachment joint, not its exploded centroid.
    for vertex,p in zip(o.data.vertices,positions):vertex.co=p-pivot
    o.parent=arm;o.matrix_parent_inverse=Matrix.Identity(4);o.matrix_world=Matrix.Translation(pivot)
    for name in arm.data.bones:o.vertex_groups.new(name=name.name)
    for i,w in enumerate(skin):
        total=sum(w.values())
        for name,value in w.items():
            if value>0:o.vertex_groups[name].add([i],value/total,'REPLACE')
    mod=o.modifiers.new('Bound to existing Veilborne skeleton','ARMATURE');mod.object=arm
    o['variant']=variant;o['fit_status']='prototype_draft';o['equipment_slot']=row['slot_index'];o['tier']=row['tier'];o['attachment_bone']='mixamorig:'+fallback
    o.data.update();o.data.calc_loop_triangles();fitted.append(o)
    audit.append(dict(name=o.name,tier=row['tier'],slot=row['slot'],slot_index=row['slot_index'],variant=variant,triangles=len(o.data.loop_triangles),pivot=list(pivot),attachment_bone='mixamorig:'+fallback,
        unweighted_vertices=sum(not v.groups for v in o.data.vertices),weight_sum_errors=sum(abs(sum(g.weight for g in v.groups)-1)>.001 for v in o.data.vertices),pushed_vertices=push_count,max_rest_surface_push_m=max_push))
root=bpy.data.objects.get('Veilborne_'+variant)
def select(objects):
    bpy.ops.object.select_all(action='DESELECT');arm.select_set(True)
    if root:root.select_set(True)
    for o in objects:o.select_set(True)
    bpy.context.view_layer.objects.active=arm
def export(path,objects):
    path.parent.mkdir(parents=True,exist_ok=True);select(objects)
    bpy.ops.export_scene.fbx(filepath=str(path),use_selection=True,object_types={'EMPTY','ARMATURE','MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',add_leaf_bones=False,mesh_smooth_type='FACE',bake_anim=False,path_mode='COPY',embed_textures=True)
for tier in ['Tin','Copper']:
    objects=[o for o in fitted if o['tier']==tier];folder=R/f'exports/fitted/{variant}/{tier}'
    export(folder/f'{tier}_{variant}_AllSlots.fbx',objects)
    select(objects);bpy.ops.export_scene.gltf(filepath=str(folder/f'{tier}_{variant}_AllSlots.glb'),export_format='GLB',use_selection=True,export_animations=False)
    for slot in range(9):
        pieces=[o for o in objects if o['equipment_slot']==slot];slot_name=['Head','Shoulders','Chest','Hands','Belt','Bracers','Legs','Feet','Cape'][slot]
        export(folder/f'{tier}_{variant}_{slot_name}.fbx',pieces)
    for o in fitted:o.hide_render=o not in objects
    scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=16;scene.render.resolution_x=700;scene.render.resolution_y=900;scene.render.resolution_percentage=100
    camera=scene.camera;camera.data.type='ORTHO';camera.data.ortho_scale=(1.73 if female else 1.85)*1.22;at=Vector((0,0,(1.73 if female else 1.85)*.52))
    for view,direction in [('front',(0,-1,.06)),('back',(0,1,.06)),('three_quarter',(.65,-1,.1))]:
        camera.location=at+Vector(direction).normalized()*5;camera.rotation_euler=(at-camera.location).to_track_quat('-Z','Y').to_euler();scene.render.filepath=str(R/f'previews/{tier}_{variant}_{view}.png');bpy.ops.render.render(write_still=True)
for o in fitted:o.hide_render=o['tier']!='Tin'
# Motion QA measures finite skinning and penetrations. It does not silently certify fit.
motion=[]
for clip,frames in [('Idle',[1]),('Walk',[1,7,14,21]),('Run',[1,5,10,15])]:
    action=bpy.data.actions.get(clip)
    if not action:continue
    arm.animation_data.action=action
    if action.slots:arm.animation_data.action_slot=action.slots[0]
    for frame in frames:
        bpy.context.scene.frame_set(frame);deps=bpy.context.evaluated_depsgraph_get();bev=body.evaluated_get(deps);bm=bev.to_mesh();bm.calc_loop_triangles()
        verts=[body.matrix_world@v.co for v in bm.vertices];bodytree=BVHTree.FromPolygons(verts,[tuple(t.vertices) for t in bm.loop_triangles if bm.polygons[t.polygon_index].material_index==0],all_triangles=True)
        for tier in ['Tin','Copper']:
            inside=0;sampled=0;finite=True;lo=1e9;hi=-1e9
            for o in [obj for obj in fitted if obj['tier']==tier]:
                ev=o.evaluated_get(deps);mesh=ev.to_mesh()
                for vertex in mesh.vertices:
                    p=o.matrix_world@vertex.co;finite &= all(math.isfinite(v) for v in p);lo=min(lo,p.z);hi=max(hi,p.z)
                    at,n,idx,d=bodytree.find_nearest(p);inside+=int((p-at).dot(n)<-.004);sampled+=1
                ev.to_mesh_clear()
            motion.append(dict(tier=tier,variant=variant,clip=clip,frame=frame,finite=finite,min_height=lo,max_height=hi,body_penetration_samples=inside,samples=sampled,penetration_fraction=inside/max(1,sampled)))
        bev.to_mesh_clear()
arm.animation_data.action=bpy.data.actions['Idle'];arm.animation_data.action_slot=arm.animation_data.action.slots[0];bpy.context.scene.frame_set(1)
(R/f'qa/fitting_{variant}.json').write_text(json.dumps(dict(variant=variant,objects=audit,motion=motion,status='draft; rest and motion clipping require review; upper-thigh armour absent'),indent=2))
bpy.ops.wm.save_as_mainfile(filepath=str(R/f'blender/Prototype_Fitted_{variant}.blend'))
print('FITTING_DRAFT_COMPLETE',variant,len(audit),'pieces; 18 slots exported')
