"""Blender 5.2 player authoring: preserve service rig/UVs, map Humanoid bones,
author in-place locomotion locally, add restrained Veil glow, and export/review.
No network calls and no writes to the existing Unity project.
"""
import bpy,bmesh,json,math,sys
import numpy as np
from pathlib import Path
from mathutils import Vector,Matrix,Quaternion
from mathutils.bvhtree import BVHTree
from mathutils.kdtree import KDTree
ROOT=Path(r'C:\The Veilbound\PlayerAssetProduction')
variant=sys.argv[sys.argv.index('--')+1]
cfg=json.loads((ROOT/'pipeline/player_batch.json').read_text())
job=next(v for v in cfg['variants'] if v['name']==variant)
for d in ['models_blender','models_unity','previews','qa']:(ROOT/d/variant).mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=str(ROOT/f'rigs_raw/{variant}/{variant}_rigged.glb'))
arm=next(o for o in bpy.context.scene.objects if o.type=='ARMATURE')
removed_helpers=[]
for o in list(bpy.context.scene.objects):
    if o.type=='MESH' and o.name.startswith('Icosphere') and not o.data.materials and len(o.data.polygons)==80:
        removed_helpers.append(o.name);bpy.data.objects.remove(o,do_unlink=True)
meshes=[o for o in bpy.context.scene.objects if o.type=='MESH']
assert len(meshes)==1,'Inspect unexpected source mesh structure before processing.'
body=meshes[0]
body.data.calc_loop_triangles();original_triangles=len(body.data.loop_triangles)
to_arm=arm.matrix_world.inverted()@body.matrix_world
points=[to_arm@v.co for v in body.data.vertices]
lo=Vector(tuple(min(p[i] for p in points) for i in range(3)));hi=Vector(tuple(max(p[i] for p in points) for i in range(3)))
H=hi.z-lo.z;scale=job['height_m']/H;up=Vector((0,0,1))

# Infer anatomy from hierarchy/positions rather than trusting generic service labels.
root=next(b for b in arm.data.bones if b.parent is None)
root_children=list(root.children)
hips=max(root_children,key=lambda b:b.head_local.z)
spine=hips.children[0];chest=spine.children[0]
neck=max(chest.children,key=lambda b:b.head_local.z+.4*b.tail_local.z)
# The true central neck has near-zero lateral displacement and an upward child.
neck=min(chest.children,key=lambda b:abs(b.head_local.y)+abs(b.head_local.x))
head=neck.children[0]
shoulders=[b for b in chest.children if b!=neck]
assert len(shoulders)==2 and len(root_children)==3,'Unexpected biped hierarchy.'
left_shoulder=max(shoulders,key=lambda b:b.head_local.y)
right_shoulder=min(shoulders,key=lambda b:b.head_local.y)
legs=[b for b in root_children if b!=hips]
left_leg=max(legs,key=lambda b:b.head_local.y);right_leg=min(legs,key=lambda b:b.head_local.y)
mapping={root.name:'Root',hips.name:'Hips',spine.name:'Spine',chest.name:'Spine1',neck.name:'Neck',head.name:'Head'}
for side,shoulder,leg in [('Left',left_shoulder,left_leg),('Right',right_shoulder,right_leg)]:
    chain=[shoulder]
    for _ in range(3):chain.append(chain[-1].children[0])
    for bone,name in zip(chain,[side+'Shoulder',side+'Arm',side+'ForeArm',side+'Hand']):mapping[bone.name]=name
    hand=chain[-1];fingers=list(hand.children)
    if len(fingers)==5:
        thumb=min(fingers,key=lambda b:(b.head_local-hand.head_local).length)
        remaining=sorted([b for b in fingers if b!=thumb],key=lambda b:b.head_local.x,reverse=True)
        for first,finger in [(thumb,'Thumb')]+list(zip(remaining,['Index','Middle','Ring','Pinky'])):
            bone=first
            for index in range(1,4):
                mapping[bone.name]=side+'Hand'+finger+str(index)
                if bone.children:bone=bone.children[0]
    bone=leg
    for name in [side+'UpLeg',side+'Leg',side+'Foot',side+'ToeBase']:
        mapping[bone.name]=name
        if bone.children:bone=bone.children[0]
# Explicitly rename the deform groups too; mesh ownership is separate from bones.
for mesh in meshes:
    for group in mesh.vertex_groups:
        if group.name in mapping:group.name='mixamorig:'+mapping[group.name]
for old,new in mapping.items():arm.data.bones[old].name='mixamorig:'+new
arm.name=variant+'_Humanoid'
bpy.context.view_layer.objects.active=arm
bpy.ops.object.mode_set(mode='EDIT')
for side in ['Left','Right']:
    arm.data.edit_bones['mixamorig:'+side+'UpLeg'].parent=arm.data.edit_bones['mixamorig:Hips']
bpy.ops.object.mode_set(mode='OBJECT')
names={n:arm.pose.bones['mixamorig:'+n] for n in mapping.values()}
left=arm.data.bones['mixamorig:LeftArm'].head_local
right=arm.data.bones['mixamorig:RightArm'].head_local
side_axis=(left-right).normalized()
forward=side_axis.cross(up).normalized()  # +X in the service's local rig.
right_axis=-side_axis

# Base surface: retain its existing UV/PBR maps; use flat face shading.
for p in body.data.polygons:p.use_smooth=False
for material in body.data.materials:
    material.use_backface_culling=False
    for node in material.node_tree.nodes:
        if node.type=='BSDF_PRINCIPLED':
            for link in list(node.inputs['Metallic'].links):material.node_tree.links.remove(link)
            node.inputs['Metallic'].default_value=0
            node.inputs['Specular IOR Level'].default_value=.18
            # Preserve texture-driven roughness and normal detail when supplied.
            if not node.inputs['Roughness'].is_linked:node.inputs['Roughness'].default_value=.82

# Add the reference's subtle red iris/skin fissure motifs locally on the surface.
# Their weights are copied from the nearest existing body vertex, never guessed bones.
bvh=BVHTree.FromPolygons(points,[tuple(p.vertices) for p in body.data.polygons],all_triangles=False)
kd=KDTree(len(points))
for i,p in enumerate(points):kd.insert(p,i)
kd.balance()
glow=bpy.data.materials.new('Veil_Red_Emission');glow.use_nodes=True
bsdf=glow.node_tree.nodes.get('Principled BSDF');bsdf.inputs['Base Color'].default_value=(.7,.008,.002,1)
bsdf.inputs['Emission Color'].default_value=(1,.018,.004,1);bsdf.inputs['Emission Strength'].default_value=2.2
bsdf.inputs['Roughness'].default_value=.5
pupil=bpy.data.materials.new('Iris_Dark_Centre');pupil.use_nodes=True
pupil.node_tree.nodes.get('Principled BSDF').inputs['Base Color'].default_value=(.003,.001,.001,1)
skin_mid=(lo+hi)/2
def project(lateral,zratio):
    origin=side_axis*(lateral*H)+up*(lo.z+zratio*H)+forward*(H*2)
    hit,normal,_,_=bvh.ray_cast(origin,-forward,H*4)
    return hit+forward*(H*.0007) if hit is not None else None
def weighted_mesh(name,vertices,faces,mat):
    mesh=bpy.data.meshes.new(name);mesh.from_pydata(vertices,[],faces);mesh.update()
    obj=bpy.data.objects.new(name,mesh);bpy.context.scene.collection.objects.link(obj)
    obj.matrix_world=arm.matrix_world.copy();obj.data.materials.append(mat)
    for group in body.vertex_groups:obj.vertex_groups.new(name=group.name)
    for v in mesh.vertices:
        _,i,_=kd.find(v.co)
        for g in body.data.vertices[i].groups:obj.vertex_groups[g.group].add([v.index],g.weight,'REPLACE')
    mod=obj.modifiers.new('Humanoid skin','ARMATURE');mod.object=arm
    return obj
def ellipsoid(name,at,rx,ry,rz,mat):
    verts=[];faces=[];segments=12;rings=6
    for j in range(1,rings):
        theta=math.pi*j/rings
        for i in range(segments):
            phi=2*math.pi*i/segments
            p=at+side_axis*(math.sin(theta)*math.cos(phi)*rx)+forward*(math.sin(theta)*math.sin(phi)*ry)+up*(math.cos(theta)*rz)
            verts.append(p)
    for j in range(rings-2):
        for i in range(segments):
            a=j*segments+i;b=j*segments+(i+1)%segments
            faces.append((a,b,b+segments,a+segments))
    top=len(verts);verts.append(at+up*rz)
    bottom=len(verts);verts.append(at-up*rz)
    for i in range(segments):
        faces.append((top,i,(i+1)%segments))
        start=(rings-2)*segments
        faces.append((bottom,start+(i+1)%segments,start+i))
    return weighted_mesh(name,verts,faces,mat)
extras=[];eye_hits=[]
for sign in [-1,1]:
    at=project(job.get('eye_center_lateral',0)+sign*job.get('eye_half_spacing',.0145),job.get('eye_z_ratio',.9045))
    if at is None:continue
    eye_hits.append(list(at))
    extras.append(ellipsoid('Veil iris',at+forward*(H*.0006),H*.0032,H*.0011,H*.0027,glow))
    extras.append(ellipsoid('Iris pupil',at+forward*(H*.0018),H*.00115,H*.0003,H*.0017,pupil))
paths=[[(.044,.893),(.051,.884),(.047,.872),(.058,.859)],[(.051,.884),(.064,.883),(.070,.875)],
       [(-.040,.885),(-.049,.875),(-.046,.863)],[(-.020,.815),(-.026,.803),(-.021,.790),(-.030,.777)],
       [(-.026,.803),(-.040,.799),(-.044,.788)]]
vein_verts=[];vein_faces=[]
for path in paths:
    projected=[project(x*.6,z) for x,z in path]
    for a,b in zip(projected,projected[1:]):
        if a is None or b is None:continue
        if abs((b-a).dot(forward))>H*.012 or (b-a).length>H*.035:continue
        tangent=(b-a).normalized();cross=tangent.cross(forward).normalized();radius=H*.00034
        start=len(vein_verts)
        for p in [a,b]:
            for i in range(4):vein_verts.append(p+cross*(math.cos(i*math.pi/2)*radius)+forward*(math.sin(i*math.pi/2)*radius))
        for i in range(4):vein_faces.append((start+i,start+(i+1)%4,start+4+(i+1)%4,start+4+i))
if vein_verts:
    fissure=glow.copy();fissure.name='Veil_Fissure_Emission'
    fissure.node_tree.nodes.get('Principled BSDF').inputs['Emission Strength'].default_value=.7
    extras.append(weighted_mesh('Veil cheek and neck fissures',vein_verts,vein_faces,fissure))
meshes+=extras

# Uniform parent transform preserves the service bind pose and weight data.
bpy.context.view_layer.update()
base_objects=[o for o in bpy.context.scene.objects if o.parent is None]
model_root=bpy.data.objects.new('Veilborne_'+variant,None);bpy.context.scene.collection.objects.link(model_root)
angle=-math.pi/2-math.atan2(forward.y,forward.x)
rotation=Matrix.Rotation(angle,4,'Z')
world_points=[o.matrix_world@v.co for o in meshes for v in o.data.vertices]
rotated=[rotation@p for p in world_points]
base=Vector(((min(p.x for p in rotated)+max(p.x for p in rotated))/2,(min(p.y for p in rotated)+max(p.y for p in rotated))/2,min(p.z for p in rotated)))
normalization=Matrix.Translation(-base*scale)@Matrix.Scale(scale,4)@rotation
for o in base_objects:
    old=o.matrix_world.copy();o.parent=model_root;o.matrix_world=old
model_root.matrix_world=normalization
bpy.context.view_layer.update()
model_objects=[model_root,arm]+meshes

# Author in-place clips with analytic two-bone leg placement and planted stance phases.
# Pose matrices are solved in armature space, with foot rotation independent of the shin.
scene=bpy.context.scene;scene.render.fps=30
def pose_rotation(name,axis,angle):
    pb=names[name];pb.rotation_mode='QUATERNION'
    world_q=Quaternion(axis,angle)
    rest=pb.bone.matrix_local.to_quaternion()
    pb.rotation_quaternion=rest.inverted()@world_q@rest
def orient(name,head_at,tail_at):
    pb=names[name];bone=pb.bone
    q=(bone.tail_local-bone.head_local).rotation_difference(tail_at-head_at)@bone.matrix_local.to_quaternion()
    pb.matrix=Matrix.LocRotScale(head_at,q,Vector((1,1,1)))
def leg_pose(side,t,kind):
    thigh=names[side+'UpLeg'];shin=names[side+'Leg'];foot=names[side+'Foot']
    hip=thigh.head.copy();rest=foot.bone.head_local.copy()
    if kind=='Idle':goal=rest;pitch=0
    else:
        stance=.60 if kind=='Walk' else .40
        half=(.32 if kind=='Walk' else .46)/scale
        if t<stance:
            u=t/stance;travel=half*(1-2*u);lift=0
            pitch=math.radians(10)*(max(0,1-u*5))-math.radians(16)*max(0,(u-.72)/.28)
        else:
            u=(t-stance)/(1-stance);ease=u*u*(3-2*u)
            travel=-half+2*half*ease
            lift=(.10 if kind=='Walk' else .22)/scale*math.sin(math.pi*u)
            pitch=math.radians(-8)*math.sin(math.pi*u)
        goal=rest+forward*travel+up*lift
    upper=(thigh.bone.tail_local-thigh.bone.head_local).length
    lower=(shin.bone.tail_local-shin.bone.head_local).length
    delta=goal-hip;distance=min(delta.length,upper+lower-.0005);direction=delta.normalized()
    goal=hip+direction*distance
    pole=forward-direction*forward.dot(direction);pole.normalize()
    along=(upper*upper-lower*lower+distance*distance)/(2*distance)
    knee=hip+direction*along+pole*math.sqrt(max(0,upper*upper-along*along))
    orient(side+'UpLeg',hip,knee);bpy.context.view_layer.update()
    orient(side+'Leg',knee,goal);bpy.context.view_layer.update()
    q=Quaternion(side_axis,pitch)@foot.bone.matrix_local.to_quaternion()
    foot.matrix=Matrix.LocRotScale(goal,q,Vector((1,1,1)))
    # Keep the toe segment aligned with the foot; no extra toe stretch.
def arm_pose(side,phase,kind):
    upper=names[side+'Arm'];lower=names[side+'ForeArm'];hand=names[side+'Hand']
    sign=1 if side=='Left' else -1
    amplitude=0 if kind=='Idle' else math.radians(16 if kind=='Walk' else 27)
    swing=-sign*amplitude*math.cos(phase)
    elbow_flex=math.radians(6 if kind=='Idle' else 12 if kind=='Walk' else 58)
    origin=upper.head.copy()
    direction=Quaternion(side_axis,swing)@(upper.bone.tail_local-upper.bone.head_local).normalized()
    elbow=origin+direction*upper.bone.length
    lower_dir=Quaternion(side_axis,-elbow_flex)@direction
    wrist=elbow+lower_dir*lower.bone.length
    orient(side+'Arm',origin,elbow);bpy.context.view_layer.update()
    orient(side+'ForeArm',elbow,wrist);bpy.context.view_layer.update()
    hand.matrix=Matrix.LocRotScale(wrist,lower.matrix.to_quaternion()@lower.bone.matrix_local.to_quaternion().inverted()@hand.bone.matrix_local.to_quaternion(),Vector((1,1,1)))
actions={};motion_audits={}
foot_vertices={s:[i for i,p in enumerate(points) if p.z<lo.z+H*.14 and (p.dot(side_axis)>0)==(s=='Left')] for s in ['Left','Right']}
for kind,cycle in [('Idle',3.0),('Walk',.9),('Run',.64)]:
    action=bpy.data.actions.new(kind);action.use_fake_user=True
    arm.animation_data_create();arm.animation_data.action=action
    total=round(cycle*30);scene.frame_start=1;scene.frame_end=total+1
    foot_samples=[];bounds_samples=[]
    for frame in range(1,total+2):
        scene.frame_set(frame)
        for pb in arm.pose.bones:pb.matrix_basis=Matrix.Identity(4);pb.rotation_mode='QUATERNION'
        t=(frame-1)/total;phase=2*math.pi*t
        moving=kind!='Idle'
        hip=names['Hips']
        bob=(.009 if kind=='Walk' else .025 if kind=='Run' else .003)*(1-math.cos(phase*2))/scale
        drop=(.05 if kind=='Walk' else .13 if kind=='Run' else 0)/scale
        hip.location=hip.bone.matrix_local.to_3x3().inverted()@(up*(bob-drop)+side_axis*((.009 if kind=='Walk' else .005 if kind=='Run' else .0015)/scale*math.sin(phase)))
        pose_rotation('Hips',up,math.radians(3 if moving else .3)*math.sin(phase))
        pose_rotation('Spine',side_axis,math.radians(-3 if kind=='Walk' else -8 if kind=='Run' else .5)*(.85+.15*math.cos(phase*2)))
        pose_rotation('Spine1',up,math.radians(-4 if moving else .5)*math.sin(phase))
        bpy.context.view_layer.update()
        leg_pose('Left',t%1,kind);leg_pose('Right',(t+.5)%1,kind)
        arm_pose('Left',phase,kind);arm_pose('Right',phase,kind)
        bpy.context.view_layer.update()
        # Contact correction uses the deformed foot surface rather than bone height.
        stance=.60 if kind=='Walk' else .40
        grounded=[s for s,p in [('Left',t%1),('Right',(t+.5)%1)] if kind=='Idle' or p<stance]
        if grounded:
            evaluated=body.evaluated_get(bpy.context.evaluated_depsgraph_get());deformed=evaluated.to_mesh()
            minimum=min((evaluated.matrix_world@deformed.vertices[i].co).z for s in grounded for i in foot_vertices[s])
            evaluated.to_mesh_clear()
            names['Root'].location=names['Root'].bone.matrix_local.to_3x3().inverted()@(up*((.001-minimum)/scale))
            bpy.context.view_layer.update()
        for pb in arm.pose.bones:
            pb.keyframe_insert('location',frame=frame,group=pb.name)
            pb.keyframe_insert('rotation_quaternion',frame=frame,group=pb.name)
            pb.keyframe_insert('scale',frame=frame,group=pb.name)
        foot_samples.append([float(names[s+'Foot'].head.z) for s in ['Left','Right']])
    actions[kind]=action
    motion_audits[kind]=dict(frames=total+1,duration_seconds=total/30,authored_locally=True,in_place=True,
                            intended_speed_mps=0 if kind=='Idle' else 1.2 if kind=='Walk' else 3.6,
                            ankle_height_range_armature_units=[min(min(v) for v in foot_samples),max(max(v) for v in foot_samples)])

# Pack the source textures and save explicit sidecars for FBX material reconstruction.
texture_dir=ROOT/f'models_unity/{variant}/textures';texture_dir.mkdir(exist_ok=True)
textures=[]
for i,im in enumerate(list(bpy.data.images)):
    if im.type!='IMAGE' or not im.size[0]:continue
    _=im.pixels[0]
    role='BaseColor' if im.name.startswith('Color_') else 'Normal' if im.name.startswith('NormalGL_') else 'ORM' if im.name.startswith('ORM_') else f'Image_{i:02d}'
    path=texture_dir/f'{variant}_{role}.png'
    im.filepath_raw=str(path);im.file_format='PNG';im.save();im.pack();textures.append(dict(path=str(path),size=list(im.size),role=role))
    if role=='ORM':
        data=np.empty(im.size[0]*im.size[1]*4,dtype=np.float32);im.pixels.foreach_get(data);pixels=data.reshape((-1,4))
        for packed_role in ['MetallicSmoothness','Occlusion']:
            packed=np.ones_like(pixels)
            if packed_role=='MetallicSmoothness':packed[:,:3]=0;packed[:,3]=1-pixels[:,1]
            else:packed[:,:3]=pixels[:,0:1]
            target_image=bpy.data.images.new(variant+'_'+packed_role,width=im.size[0],height=im.size[1],alpha=True)
            target_image.colorspace_settings.name='Non-Color';target_image.pixels.foreach_set(packed.ravel())
            derived_path=texture_dir/f'{variant}_{packed_role}.png';target_image.filepath_raw=str(derived_path);target_image.file_format='PNG';target_image.save();target_image.pack()
            textures.append(dict(path=str(derived_path),size=list(target_image.size),role=packed_role))
def select_model():
    bpy.ops.object.select_all(action='DESELECT')
    for o in model_objects:o.select_set(True)
    bpy.context.view_layer.objects.active=arm
def activate(kind):
    arm.animation_data.action=actions[kind]
    if actions[kind].slots:arm.animation_data.action_slot=actions[kind].slots[0]
    a,b=actions[kind].frame_range;scene.frame_start=round(a);scene.frame_end=round(b);scene.frame_set(round(a))
def fbx(path,animated,all_actions=False):
    select_model()
    bpy.ops.export_scene.fbx(filepath=str(path),use_selection=True,object_types={'EMPTY','ARMATURE','MESH'},
        axis_forward='-Z',axis_up='Y',apply_unit_scale=True,add_leaf_bones=False,mesh_smooth_type='FACE',
        bake_anim=animated,bake_anim_use_all_actions=all_actions,bake_anim_use_nla_strips=False,
        bake_anim_simplify_factor=0,path_mode='COPY',embed_textures=True)
activate('Idle');select_model()
saved_action=arm.animation_data.action;arm.animation_data.action=None
for pb in arm.pose.bones:pb.matrix_basis=Matrix.Identity(4)
bpy.context.view_layer.update()
fbx(ROOT/f'models_unity/{variant}/{variant}_Rigged.fbx',False)
for kind in actions:
    activate(kind);fbx(ROOT/f'models_unity/{variant}/{kind}.fbx',True,False)
activate('Idle');fbx(ROOT/f'models_unity/{variant}/{variant}_Locomotion.fbx',True,True)
select_model()
bpy.ops.export_scene.gltf(filepath=str(ROOT/f'models_unity/{variant}/{variant}_Locomotion.glb'),export_format='GLB',
    use_selection=True,export_animations=True,export_animation_mode='ACTIONS',export_anim_slide_to_zero=True)

# Static review studio excluded from model-only exports.
studio=bpy.data.collections.new('PreviewStudio');scene.collection.children.link(studio)
def studio_object(o):
    for c in list(o.users_collection):c.objects.unlink(o)
    studio.objects.link(o)
bpy.ops.mesh.primitive_plane_add(size=200,location=(0,0,-.008));ground=bpy.context.object;studio_object(ground)
mat=bpy.data.materials.new('Studio charcoal');mat.diffuse_color=(.045,.05,.055,1);ground.data.materials.append(mat)
scene.world=bpy.data.worlds.new('World');scene.world.use_nodes=True
scene.world.node_tree.nodes['Background'].inputs[0].default_value=(.15,.17,.21,1)
scene.world.node_tree.nodes['Background'].inputs[1].default_value=.4
height=job['height_m'];target=Vector((0,0,height*.51))
def aim(o,at):o.rotation_euler=(Vector(at)-o.location).to_track_quat('-Z','Y').to_euler()
for name,loc,power,size in [('Key',(-3,-4,5),500,3),('Fill',(3,-2,3),280,3),('Rim',(2,3,5),650,3)]:
    data=bpy.data.lights.new(name,'AREA');data.energy=power;data.size=size
    o=bpy.data.objects.new(name,data);studio.objects.link(o);o.location=Vector(loc)*height/2;aim(o,target)
data=bpy.data.cameras.new('ReviewCamera');camera=bpy.data.objects.new('ReviewCamera',data);studio.objects.link(camera)
scene.camera=camera;data.type='ORTHO';data.ortho_scale=height*1.16
scene.render.engine='CYCLES';scene.cycles.samples=20;scene.cycles.device='CPU'
scene.render.resolution_x=800;scene.render.resolution_y=1000;scene.render.resolution_percentage=100
scene.render.image_settings.file_format='PNG';scene.view_settings.view_transform='AgX';scene.view_settings.exposure=-.55
renders={};activate('Idle')
for name,direction in [('front',(0,-1,.08)),('side',(1,0,.08)),('three_quarter',(.65,-1,.12)),('back',(0,1,.08))]:
    camera.location=target+Vector(direction).normalized()*height*3;aim(camera,target)
    path=ROOT/f'previews/{variant}/{name}.png';scene.render.filepath=str(path);bpy.ops.render.render(write_still=True);renders[name]=str(path)
camera.location=Vector((0,-2,height*.89));aim(camera,Vector((0,0,height*.89)));data.ortho_scale=height*.32
scene.render.resolution_x=800;scene.render.resolution_y=800
path=ROOT/f'previews/{variant}/head_detail.png';scene.render.filepath=str(path);bpy.ops.render.render(write_still=True);renders['head_detail']=str(path)

# Short review sequences at the clips' real duration. GIF assembly runs outside Blender.
motion_frames={}
scene.cycles.samples=6;scene.render.resolution_x=480;scene.render.resolution_y=600
data.ortho_scale=height*1.18
camera.location=target+Vector((.55,-1,.10)).normalized()*height*3;aim(camera,target)
for kind in ['Walk','Run']:
    activate(kind);a,b=actions[kind].frame_range;duration=(b-a)/30
    count=max(12,round(duration*20));folder=ROOT/f'previews/{variant}/{kind}_frames';folder.mkdir(exist_ok=True)
    paths=[]
    for i in range(count):
        at=a+(b-a)*i/count;scene.frame_set(math.floor(at),subframe=at-math.floor(at))
        path=folder/f'{i:03d}.png';scene.render.filepath=str(path);bpy.ops.render.render(write_still=True);paths.append(str(path))
    motion_frames[kind]=dict(paths=paths,duration_seconds=duration,frame_duration_ms=duration/count*1000)

activate('Idle');scene.cycles.samples=24;scene.render.resolution_x=800;scene.render.resolution_y=1000
camera.location=target+Vector((.65,-1,.12)).normalized()*height*3;aim(camera,target)
select_model();bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/f'models_blender/{variant}/{variant}.blend'))
total_triangles=0;unweighted=0;bad_weight_sums=0
for o in meshes:
    o.data.calc_loop_triangles();total_triangles+=len(o.data.loop_triangles)
    for v in o.data.vertices:
        total=sum(g.weight for g in v.groups)
        if total<1e-5:unweighted+=1
        if abs(total-1)>.01:bad_weight_sums+=1
report=dict(variant=variant,original_triangles=original_triangles,final_triangles=total_triangles,
    target_triangles=20000,nominal_target_exceeded=original_triangles>20000,preserved_source_detail=True,
    height_m=height,bone_count=len(arm.data.bones),bone_mapping=mapping,removed_unmaterialed_helpers=removed_helpers,
    reparented_upper_legs_to_hips=True,unweighted_vertices=unweighted,bad_weight_sums=bad_weight_sums,
    materials=sorted({m.name for o in meshes for m in o.data.materials if m}),textures=textures,
    local_veil_iris_positions=eye_hits,local_veil_fissure_segments=len(vein_faces)//4,
    animations=motion_audits,motion_frames=motion_frames,renders=renders,
    limitations=['Locomotion is an authored prototype; final in-game foot-speed, equipment fitting and attack poses need review.',
                 'Base clothing is part of the generated surface, not a complete interchangeable equipment system.',
                 'No live Unity character, saves, input or combat code was changed.'])
(ROOT/f'qa/{variant}/player_audit.json').write_text(json.dumps(report,indent=2))
print('PLAYER_AUDIT '+json.dumps({k:report[k] for k in ['variant','original_triangles','final_triangles','bone_count','unweighted_vertices','bad_weight_sums']}))
