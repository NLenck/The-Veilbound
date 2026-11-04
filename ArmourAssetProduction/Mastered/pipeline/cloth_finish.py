def separate_belt_cloth(obj):
    uv=obj.data.uv_layers.active;count=0
    for face in obj.data.polygons:
        tile=int(sum(uv.data[i].uv.x for i in face.loop_indices)/len(face.loop_indices)*16)
        if tile in [10,15]:
            for loop in face.loop_indices:uv.data[loop].uv=(15.5/16,.5)
            count+=1
    obj['independent_belt_cloth_faces']=count
    return count
