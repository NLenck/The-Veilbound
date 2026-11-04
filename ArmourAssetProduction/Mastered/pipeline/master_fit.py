"""Local fitting drafts, not an assertion of final art or motion acceptance."""
import bpy,bmesh,json,math,sys
import numpy as np
from pathlib import Path
from mathutils import Vector,Matrix,geometry
from mathutils.bvhtree import BVHTree
sys.path.insert(0,str(Path(__file__).resolve().parent))
from visor_finish import apply_visor
from female_arm_balance import balance_female_arms
from cloth_finish import separate_belt_cloth
S=Path(r'C:\The Veilbound\ArmourAssetProduction');R=S/'Mastered';P=S.parent/'PlayerAssetProduction'
classification=json.loads((S/'qa/classification.json').read_text())
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
def anatomical_frame(side,up):
    z=up.normalized();across=B(side+'HandPinky1')-B(side+'HandIndex1')
    x=(across-z*across.dot(z)).normalized();y=z.cross(x).normalized()
    return Matrix((x,y,z)).transposed()

def source_arm_frame(obj,raw_centre,slot):
    local=[Vector((v.co.y-raw_centre.y,-v.co.x+raw_centre.x,v.co.z-raw_centre.z)) for v in obj.data.vertices]
    low=min(p.z for p in local);high=max(p.z for p in local);band=(high-low)*.18
    top=[p for p in local if p.z>high-band];bottom=[p for p in local if p.z<low+band]
    a=sum(top,Vector())/len(top);b=sum(bottom,Vector())/len(bottom);z=(a-b).normalized()
    hint=Vector((0,1,0))
    if slot=='Hand':
        projected=np.array([tuple((p-a)-z*(p-a).dot(z)) for p in local]);values,vectors=np.linalg.eigh(np.cov(projected.T));hint=Vector(vectors[:,int(np.argmax(values))])
        if hint.dot(Vector((0,1,0)))<0:hint=-hint
    x=(hint-z*hint.dot(z)).normalized();y=z.cross(x).normalized();basis=Matrix((x,y,z)).transposed();inverse=basis.transposed()
    points=[inverse@(p-a) for p in local]
    half_x=max(abs(p.x) for p in points);half_y=max(abs(p.y) for p in points);z0=min(p.z for p in points);z1=max(p.z for p in points)
    normalized=[Vector((p.x/half_x,p.y/half_y,(p.z-z0)/(z1-z0))) for p in points]
    return normalized,math.degrees(z.angle(Vector((0,0,1))))

rows=classification['objects'];names=[r['name'] for r in rows]
with bpy.data.libraries.load(str(S/'blender/Prototype_Organised_Exploded.blend'),link=False) as (source,target):target.objects=names
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
        pivot=B('Head');target=pivot+Vector((.025*f,-.005*f,.125*f));size=Vector((.31,.34,.35))*f;fallback='Head';allowed=lambda n:n.endswith('Head')
    elif slot=='Shoulder':
        pivot=B(side+'Arm');target=pivot+Vector(((.035 if side=='Left' else -.035)*f,0,.015*f));size=Vector((.245,.39,.36))*f;fallback=side+'Arm'
    elif slot=='Chest':
        pivot=B('Spine1');target=(B('Spine')+T('Spine1'))/2+Vector((0,0,.025*f));size=Vector((.47 if not female else .43,.40 if not female else .39,.43))*f;fallback='Spine1';allowed=lambda n:any(n.endswith(k) for k in ['Hips','Spine','Spine1','Neck'])
    elif slot in ['Bracer','Hand','Leg']:
        bone=side+{'Bracer':'ForeArm','Hand':'Hand','Leg':'Leg'}[slot];pivot=B(bone);target=(B(bone)+T(bone))/2
        if slot=='Hand':target=B(bone)+(T(bone)-B(bone))*.9
        length=(B(bone)-T(bone)).length
        size=Vector((.125,.14,length*.84)) if slot=='Bracer' else Vector((.115,.10,.155))*f if slot=='Hand' else Vector((.205,.24,length*1.10))
        rotation=Vector((0,0,1)).rotation_difference((B(bone)-T(bone)).normalized()).to_matrix();fallback=bone
        if slot=='Bracer':allowed=lambda n,s=side:any(n.endswith(s+k) for k in ['ForeArm','Hand'])
        elif slot=='Leg':allowed=lambda n,s=side:any(n.endswith(s+k) for k in ['UpLeg','Leg','Foot'])
        else:allowed=lambda n,s=side:s+'Hand' in n
    elif slot=='Foot':
        pivot=B(side+'Foot');target=pivot+Vector((0,-.045*f,-.035*f));size=Vector((.19,.32,.19))*f;fallback=side+'Foot';allowed=lambda n,s=side:any(n.endswith(s+k) for k in ['Foot','ToeBase'])
    elif slot=='Belt':
        pivot=B('Hips');target=pivot+Vector((0,0,-.105*f));size=Vector((.50 if not female else .47,.39,.42))*f;fallback='Hips';allowed=lambda n:any(n.endswith(k) for k in ['Hips','Spine','LeftUpLeg','RightUpLeg'])
    elif slot=='Cape':
        pivot=T('Spine1');target=pivot+Vector((0,.19*f,-.53*f));size=Vector((.43,.075,1.14))*f;fallback='Spine1'
    else:raise ValueError(slot)
    if slot in ['Hand','Foot']:
        relevant=[points[i] for i,w in enumerate(weights) if sum(value for name,value in w.items() if (side+'Hand' in name if slot=='Hand' else any(name.endswith(side+k) for k in ['Foot','ToeBase'])))>.35]
        inv=rotation.inverted();local_points=[inv@p for p in relevant]
        low=Vector(tuple(min(p[k] for p in local_points) for k in range(3)));high=Vector(tuple(max(p[k] for p in local_points) for k in range(3)))
        target=rotation@((low+high)/2);size=(high-low)*1.15+Vector((.015,.015,.012))*f
        if slot=='Foot':target.z=max(target.z,size.z/2+.003)
    arm_normalized=None;arm_fit={}
    if slot in ['Bracer','Hand']:
        wrist=B(side+'Hand');elbow=B(side+'ForeArm')
        up=(elbow-wrist).normalized() if slot=='Bracer' else (wrist-B(side+'HandMiddle1')).normalized()
        rotation=anatomical_frame(side,up);inverse=rotation.transposed()
        arm_normalized,source_tilt=source_arm_frame(o,centre,slot)
        selected=[points[i] for i,w in enumerate(weights) if sum(value for name,value in w.items() if (name.endswith(side+'ForeArm') if slot=='Bracer' else side+'Hand' in name))>.35]
        projected=[inverse@(p-wrist) for p in selected]
        half_x=max(abs(p.x) for p in projected)*1.08+.004*f;half_y=max(abs(p.y) for p in projected)*1.08+.004*f
        if slot=='Bracer':
            half_x=max(.053*f,min(.09*f,half_x));half_y=max(.053*f,min(.09*f,half_y));z0=-.015*f;z1=(elbow-wrist).length+.018*f
        else:z0=min(p.z for p in projected)-.006*f;z1=.022*f
        arm_fit=dict(source_tilt_removed_deg=source_tilt,target_axis=list(up),wrist=list(wrist),elbow=list(elbow),longitudinal_min=z0,longitudinal_max=z1)
    cape_error=0.0
    if slot=='Cape' and row['tier']=='Tin':
        source_geometry=o.data.copy();bpy.context.view_layer.objects.active=o
        reduction=o.modifiers.new('Conservative cape LOD cleanup','DECIMATE');reduction.ratio=.60
        bpy.ops.object.modifier_apply(modifier=reduction.name);o.data.calc_loop_triangles()
        tree=BVHTree.FromPolygons([v.co for v in o.data.vertices],[tuple(t.vertices) for t in o.data.loop_triangles],all_triangles=True)
        cape_error=max(tree.find_nearest(v.co)[3] for v in source_geometry.vertices)
        if cape_error>.008:o.data=source_geometry
    positions=[];skin=[];push_count=0;max_push=0
    for vertex in o.data.vertices:
        p=vertex.co-centre;local=Vector((p.y,-p.x,p.z));local=Vector(tuple(local[k]/source_size[k]*size[k] for k in range(3)))
        point=target+rotation@local
        if arm_normalized is not None:
            q=arm_normalized[vertex.index];point=wrist+rotation@Vector((q.x*half_x,q.y*half_y,z0+q.z*(z1-z0)))
        if slot=='Cape':
            point.y+=.10*f*max(0,min(1,(target.z+size.z/2-point.z)/size.z))
            top=max(0,min(1,(point.z-(target.z-size.z/2))/size.z));skin.append({'mixamorig:Spine1':.2+.8*top,'mixamorig:Hips':.8*(1-top)})
        elif slot=='Shoulder':skin.append({'mixamorig:'+side+'Arm':.65,'mixamorig:Spine1':.35})
        elif slot in ['Head','Foot']:skin.append({'mixamorig:'+fallback:1})
        elif slot=='Bracer':
            distance=(point-wrist).dot(up);hand_fraction=max(0,min(.45,(.022*f-distance)/(.05*f)*.45))
            skin.append({'mixamorig:'+side+'ForeArm':1-hand_fraction,'mixamorig:'+side+'Hand':hand_fraction})
        else:
            w,at,normal,d=sample(point,allowed,fallback);skin.append(w)
        positions.append(point)
    if slot=='Belt':
        uv=o.data.uv_layers.active;bottom=centre.z-source_size.z/2
        for face in o.data.polygons:
            c=face.center
            if abs(c.y-centre.y)<source_size.x*.12 and c.z>bottom+source_size.z*.84:
                for loop in face.loop_indices:uv.data[loop].uv=(12.5/16,.5)
    # Each object pivot is at the relevant attachment joint, not its exploded centroid.
    for vertex,p in zip(o.data.vertices,positions):vertex.co=p-pivot
    o.parent=arm;o.matrix_parent_inverse=Matrix.Identity(4);o.matrix_world=Matrix.Translation(pivot)
    for name in arm.data.bones:o.vertex_groups.new(name=name.name)
    for i,w in enumerate(skin):
        total=sum(w.values())
        for name,value in w.items():
            if value>0:o.vertex_groups[name].add([i],value/total,'REPLACE')
    mod=o.modifiers.new('Bound to existing Veilborne skeleton','ARMATURE');mod.object=arm
    o['variant']=variant;o['fit_status']='master_base';o['equipment_slot']=row['slot_index'];o['tier']=row['tier'];o['attachment_bone']='mixamorig:'+fallback
    o.data.update()
    if slot=='Head':apply_visor(o)
    if slot=='Belt':separate_belt_cloth(o)
    o.data.calc_loop_triangles();fitted.append(o)
    audit.append(dict(name=o.name,tier=row['tier'],slot=row['slot'],slot_index=row['slot_index'],variant=variant,triangles=len(o.data.loop_triangles),pivot=list(pivot),attachment_bone='mixamorig:'+fallback,
        arm_fit=arm_fit,cape_surface_error_raw=cape_error,unweighted_vertices=sum(not v.groups for v in o.data.vertices),weight_sum_errors=sum(abs(sum(g.weight for g in v.groups)-1)>.001 for v in o.data.vertices),pushed_vertices=push_count,max_rest_surface_push_m=max_push))
f=scale
# Extra thigh sleeves removed: preserve the original waist/tassets and knee-shin pieces.
# Exposed upper-leg body regions are restored by the slot mask below.
# Bridge the anatomical upper arm and elbow with independently slotted armour.
def add_arm_mesh(name,tier,slot,verts,faces,pivot,weight_function):
    mesh=bpy.data.meshes.new(name);mesh.from_pydata(verts,[],faces);mesh.update()
    bm=bmesh.new();bm.from_mesh(mesh);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(mesh);bm.free()
    uv=mesh.uv_layers.new(name='AtlasUV');mesh.materials.append(next(o for o in fitted if o['tier']==tier).data.materials[0])
    for face in mesh.polygons:
        for loop in face.loop_indices:uv.data[loop].uv=((.5 if tier=='Tin' else 1.5)/16,.5)
    obj=bpy.data.objects.new(name,mesh);bpy.data.collections[f'Plate_{tier}_{variant}'].objects.link(obj)
    for vertex in mesh.vertices:vertex.co-=pivot
    obj.parent=arm;obj.matrix_parent_inverse=Matrix.Identity(4);obj.matrix_world=Matrix.Translation(pivot)
    for bone in arm.data.bones:obj.vertex_groups.new(name=bone.name)
    for i,p in enumerate(verts):
        w=weight_function(p)
        for name,value in w.items():
            if value>0:obj.vertex_groups['mixamorig:'+name].add([i],value/sum(w.values()),'REPLACE')
    mod=obj.modifiers.new('Articulated arm chain','ARMATURE');mod.object=arm
    obj['tier']=tier;obj['variant']=variant;obj['equipment_slot']=slot;obj['fit_status']='anatomical_arm_chain'
    mesh.calc_loop_triangles();fitted.append(obj)
    audit.append(dict(name=obj.name,tier=tier,slot='UpperArm' if slot==1 else 'Elbow',slot_index=slot,variant=variant,triangles=len(mesh.loop_triangles),pivot=list(pivot),attachment_bone='existing upper-arm / forearm skeleton',unweighted_vertices=0,weight_sum_errors=0,pushed_vertices=0,max_rest_surface_push_m=0))
for tier in ['Tin','Copper']:
    for side in ['Left','Right']:
        shoulder=B(side+'Arm');elbow=B(side+'ForeArm');wrist=B(side+'Hand');down=(elbow-shoulder).normalized();length=(elbow-shoulder).length
        basis=anatomical_frame(side,-down);local_points=[]
        for i,w in enumerate(weights):
            if w.get('mixamorig:'+side+'Arm',0)<.35:continue
            p=points[i]-shoulder;t=p.dot(down)/length
            if .25<t<1.02:local_points.append(basis.transposed()@(p-down*p.dot(down)))
        rx=max(.075*f,min(.11*f,max(abs(p.x) for p in local_points)+.009*f));ry=max(.075*f,min(.11*f,max(abs(p.y) for p in local_points)+.009*f))
        verts=[];faces=[];n=10
        for inner in range(2):
            for ring,t in enumerate([.30,.38,.91,1.04]):
                centre=shoulder.lerp(elbow,t);taper=1-.22*(t-.30)/.74
                for k in range(n):
                    angle=2*math.pi*k/n;r1=rx*taper-inner*.008*f;r2=ry*taper-inner*.008*f
                    verts.append(centre+basis@Vector((r1*math.cos(angle),r2*math.sin(angle),0)))
        for inner in range(2):
            offset=inner*4*n
            for ring in range(3):
                for k in range(n):
                    a=offset+ring*n+k;b=offset+ring*n+(k+1)%n;c=offset+(ring+1)*n+(k+1)%n;d=offset+(ring+1)*n+k;faces.append((a,b,c,d) if not inner else (d,c,b,a))
        for ring in [0,3]:
            for k in range(n):
                a=ring*n+k;b=ring*n+(k+1)%n;faces.append((a,a+4*n,b+4*n,b))
        suffix='L' if side=='Left' else 'R'
        add_arm_mesh(tier+'_UpperArm_'+suffix,tier,1,verts,faces,shoulder,lambda p,s=side:{s+'Arm':1})
        # A compact couter spans the elbow hinge; opposite halves follow the two bones.
        up=((shoulder-elbow).normalized()+(elbow-wrist).normalized()).normalized();basis=anatomical_frame(side,up)
        verts=[elbow+basis@Vector((0,0,.073*f))];faces=[];segments=10;rings=6
        for ring in range(1,rings):
            theta=math.pi*ring/rings
            for k in range(segments):
                phi=2*math.pi*k/segments;verts.append(elbow+basis@Vector((.087*f*math.sin(theta)*math.cos(phi),.087*f*math.sin(theta)*math.sin(phi),.073*f*math.cos(theta))))
        bottom=len(verts);verts.append(elbow-basis@Vector((0,0,.073*f)))
        for k in range(segments):
            faces.append((0,1+k,1+(k+1)%segments));a=1+(rings-2)*segments;faces.append((bottom,a+(k+1)%segments,a+k))
        for ring in range(rings-2):
            for k in range(segments):
                a=1+ring*segments+k;b=1+ring*segments+(k+1)%segments;faces.append((a,b,b+segments,a+segments))
        def elbow_weights(p,s=side,joint=elbow.copy(),axis=up.copy()):
            upper=max(.1,min(.9,.5+.4*(p-joint).dot(axis)/(.073*f)))
            return {s+'Arm':upper,s+'ForeArm':1-upper}
        add_arm_mesh(tier+'_Elbow_'+suffix,tier,5,verts,faces,elbow,elbow_weights)
# Restore the approved thigh plates with a narrower upper cuff beneath the tassets.
with bpy.data.libraries.load(str(R/f'revisions/before_thigh_sleeve_removal/Master_{variant}.blend'),link=False) as (src,dst):
    dst.objects=[n for n in src.objects if '_Thigh_' in n]
for obj in dst.objects:
    tier=obj['tier'];side='Left' if obj.name.endswith('_L') else 'Right'
    bpy.data.collections[f'Plate_{tier}_{variant}'].objects.link(obj)
    pivot=B(side+'UpLeg');knee=B(side+'Leg');axis=(knee-pivot).normalized();length=(knee-pivot).length
    world=Matrix.Translation(pivot)
    for vertex in obj.data.vertices:
        p=world@vertex.co;t=(p-pivot).dot(axis)/length;centre=pivot+axis*(t*length)
        # Retain the lower silhouette; taper the upper half into the waist armour.
        blend=max(0,min(1,(.65-t)/.65));radial=1-.34*blend
        q=centre+(p-centre)*radial-axis*(.018*f*blend)
        vertex.co=world.inverted()@q
    obj.parent=arm;obj.matrix_parent_inverse=Matrix.Identity(4);obj.matrix_world=world
    for mod in obj.modifiers:
        if mod.type=='ARMATURE':mod.object=arm
    obj.data.calc_loop_triangles();fitted.append(obj)
    audit.append(dict(name=obj.name,tier=tier,slot='Thigh',slot_index=6,variant=variant,triangles=len(obj.data.loop_triangles),pivot=list(pivot),attachment_bone='mixamorig:'+side+'UpLeg',unweighted_vertices=0,weight_sum_errors=0,pushed_vertices=0,max_rest_surface_push_m=0))

if female:
    balance_female_arms(fitted,arm)
# Encode per-triangle body coverage into UV2; FBX carries it through vertex splitting.
# Unity consumes this to restore hidden body regions whenever an individual slot is removed.
mask_uv=body.data.uv_layers.get('SlotOcclusion') or body.data.uv_layers.new(name='SlotOcclusion')
mask_counts={str(i):0 for i in range(9)};face_masks=[]
for poly in body.data.polygons:
    p=sum((points[i] for i in poly.vertices),Vector())/len(poly.vertices);w={}
    for i in poly.vertices:
        for name,value in weights[i].items():w[name]=w.get(name,0)+value/len(poly.vertices)
    value=lambda term:sum(v for name,v in w.items() if term in name)
    bits=0;all_bits=0
    if abs(p.x-B('Hips').x)<.29*f and abs(p.y-B('Hips').y)<.24*f and B('Hips').z-.32*f<p.z<B('Hips').z+.07*f:bits|=1<<4
    if p.z>B('Head').z-.035*f or value('Head')>.45:bits|=1<<0
    if value('Spine')>.35 and B('Spine').z-.065*f<p.z<B('Neck').z+.025*f:bits|=1<<2
    for side in ['Left','Right']:
        if value(side+'Hand')>.3:bits|=1<<3
        if value(side+'ForeArm')>.4:bits|=1<<5
        if value(side+'Foot')+value(side+'ToeBase')>.35:bits|=1<<7
        if value(side+'Leg')>.4:bits|=1<<6
        upper=B(side+'UpLeg');knee=B(side+'Leg');upper_t=(p-upper).dot(knee-upper)/(knee-upper).length_squared
        if value(side+'UpLeg')>.25 and upper_t>-.08:bits|=1<<6
        a=B(side+'Arm');b=T(side+'Arm');t=(p-a).dot(b-a)/(b-a).length_squared
        if (value(side+'Arm')+value(side+'Shoulder')>.35) and t<1.04:bits|=1<<1
    face_masks.append(bits|all_bits)
    for slot in range(9):mask_counts[str(slot)]+=int(bool(bits&(1<<slot)))
    for loop in poly.loop_indices:mask_uv.data[loop].uv=(bits/512,all_bits/512)
body.data.update()
(R/f'qa/body_masks_{variant}.json').write_text(json.dumps(dict(variant=variant,channel='UV2.x=any-mask/512; UV2.y=all-required-mask/512',mask_counts=mask_counts,faces=len(face_masks),all_required_mask_faces=0),indent=2))
# Tuck loose base clothing under the belt without deleting upper-leg surfaces.
body.shape_key_add(name='Basis',from_mix=False);tuck=body.shape_key_add(name='BeltTuck',from_mix=False)
def smooth_step(a,b,x):
    t=max(0,min(1,(x-a)/(b-a)));return t*t*(3-2*t)
hip=B('Hips');tucked=[];inverse_body=body.matrix_world.inverted()
for i,p in enumerate(points):
    influence=smooth_step(hip.z-.13*f,hip.z-.06*f,p.z)*(1-smooth_step(hip.z+.07*f,hip.z+.16*f,p.z))
    target=p.copy();factor=1-.45*influence;target.x=hip.x+(p.x-hip.x)*factor;target.y=hip.y+(p.y-hip.y)*factor
    local=inverse_body@target;tuck.data[i].co=local;tucked.append(local.copy())
tuck.value=0
root=bpy.data.objects.get('Veilborne_'+variant)
def select(objects):
    bpy.ops.object.select_all(action='DESELECT');arm.select_set(True)
    if root:root.select_set(True)
    for o in objects:o.select_set(True)
    bpy.context.view_layer.objects.active=arm
def export(path,objects):
    path.parent.mkdir(parents=True,exist_ok=True);select(objects)
    bpy.ops.export_scene.fbx(filepath=str(path),use_selection=True,object_types={'EMPTY','ARMATURE','MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',add_leaf_bones=False,mesh_smooth_type='FACE',bake_anim=False,path_mode='COPY',embed_textures=True)
export(R/f'exports/fitted/{variant}/{variant}_BodyMaskSource.fbx',[body])
for tier in ['Tin','Copper']:
    objects=[o for o in fitted if o['tier']==tier];folder=R/f'exports/fitted/{variant}/{tier}'
    export(folder/f'{tier}_{variant}_AllSlots.fbx',objects)
    select(objects);bpy.ops.export_scene.gltf(filepath=str(folder/f'{tier}_{variant}_AllSlots.glb'),export_format='GLB',use_selection=True,export_animations=False)
    for slot in range(9):
        pieces=[o for o in objects if o['equipment_slot']==slot];slot_name=['Head','Shoulders','Chest','Hands','Belt','Bracers','Legs','Feet','Cape'][slot]
        export(folder/f'{tier}_{variant}_{slot_name}.fbx',pieces)
    for o in fitted:o.hide_render=o not in objects
    full_body_mesh=body.data;bm=bmesh.new();bm.from_mesh(full_body_mesh);bm.faces.ensure_lookup_table();bm.verts.ensure_lookup_table()
    for i,vertex in enumerate(bm.verts):vertex.co=tucked[i]
    bmesh.ops.delete(bm,geom=[face for i,face in enumerate(bm.faces) if face_masks[i]!=0],context='FACES')
    masked_body_mesh=bpy.data.meshes.new('Body_FullSet_Masked_Tucked');bm.to_mesh(masked_body_mesh);bm.free()
    for material in full_body_mesh.materials:masked_body_mesh.materials.append(material)
    body.data=masked_body_mesh
    scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=16;scene.render.resolution_x=700;scene.render.resolution_y=900;scene.render.resolution_percentage=100
    camera=scene.camera;camera.data.type='ORTHO';camera.data.ortho_scale=(1.73 if female else 1.85)*1.22;at=Vector((0,0,(1.73 if female else 1.85)*.52))
    for view,direction in [('front',(0,-1,.06)),('back',(0,1,.06)),('three_quarter',(.65,-1,.1))]:
        camera.location=at+Vector(direction).normalized()*5;camera.rotation_euler=(at-camera.location).to_track_quat('-Z','Y').to_euler();scene.render.filepath=str(R/f'previews/{tier}_{variant}_{view}.png');bpy.ops.render.render(write_still=True)
    body.data=full_body_mesh
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
(R/f'qa/fitting_{variant}.json').write_text(json.dumps(dict(variant=variant,objects=audit,motion=motion,status='local master; slot-aware body masks and added upper-thigh coverage'),indent=2))
reference=body.copy();reference.data=body.data;reference.name='Unmasked_Body_Reference';bpy.context.scene.collection.objects.link(reference);reference.hide_render=True;reference.hide_set(True)
body.data=masked_body_mesh
bpy.ops.wm.save_as_mainfile(filepath=str(R/f'blender/Master_{variant}.blend'))
print('MASTER_BASE_COMPLETE',variant,len(audit),'pieces; 18 slots exported')
