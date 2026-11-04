import bpy,json,sys
from pathlib import Path
args=sys.argv[sys.argv.index('--')+1:]
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=args[0])
objects=[]
for o in bpy.context.scene.objects:
    r=dict(name=o.name,type=o.type,location=list(o.location),scale=list(o.scale))
    if o.type=='MESH':
        o.data.calc_loop_triangles()
        r.update(vertices=len(o.data.vertices),triangles=len(o.data.loop_triangles),materials=[m.name if m else None for m in o.data.materials],uv_layers=[u.name for u in o.data.uv_layers])
    if o.type=='ARMATURE':
        r['bones']=[dict(name=b.name,head=list(b.head_local),tail=list(b.tail_local),parent=b.parent.name if b.parent else None) for b in o.data.bones]
    if o.animation_data and o.animation_data.action:r['action']=o.animation_data.action.name
    objects.append(r)
result=dict(source=args[0],objects=objects,fps=bpy.context.scene.render.fps,
    actions=[dict(name=a.name,frame_range=list(a.frame_range),slots=[s.identifier for s in a.slots]) for a in bpy.data.actions],
    images=[dict(name=i.name,size=list(i.size),packed=bool(i.packed_file),type=i.type) for i in bpy.data.images])
Path(args[1]).write_text(json.dumps(result,indent=2))
print(json.dumps(result))
