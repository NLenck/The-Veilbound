from mathutils import Vector

def apply_visor(obj):
    points=[obj.matrix_world@v.co for v in obj.data.vertices]
    low=Vector(tuple(min(p[k] for p in points) for k in range(3)));high=Vector(tuple(max(p[k] for p in points) for k in range(3)));centre=(low+high)/2;size=high-low
    uv=obj.data.uv_layers.active;selected=[]
    for face in obj.data.polygons:
        p=sum((points[i] for i in face.vertices),Vector())/len(face.vertices);q=Vector(tuple((p[k]-centre[k])/size[k] for k in range(3)));normal=obj.matrix_world.to_3x3()@face.normal
        line=-.126+.30*abs(q.x)
        eye_slot= -.355<q.y<-.18 and abs(q.z-line)<.033 and normal.y<-.25 and normal.z<.45
        centre_channel=abs(q.x)<.105 and -.435<q.y<-.20 and -.47<q.z<-.10 and (normal.y<-.05 or abs(normal.x)>.85)
        if eye_slot or centre_channel:
            for loop in face.loop_indices:uv.data[loop].uv=(14.5/16,.5)
            selected.append(face.index)
    obj['black_visor_faces']=len(selected)
    return selected
