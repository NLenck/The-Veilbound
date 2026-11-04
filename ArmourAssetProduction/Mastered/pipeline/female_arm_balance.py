from mathutils import Vector,Matrix

def balance_female_arms(objects,arm):
    def B(name):return arm.matrix_world@arm.data.bones['mixamorig:'+name].head_local
    changes=[];height_scale=1.73/1.85
    for obj in objects:
        if not any('_'+part+'_' in obj.name for part in ['Hand','Bracer','UpperArm','Elbow']):continue
        side='Left' if obj.name.endswith('_L') else 'Right';part=next(p for p in ['Hand','Bracer','UpperArm','Elbow'] if '_'+p+'_' in obj.name)
        wrist=B(side+'Hand');elbow=B(side+'ForeArm');shoulder=B(side+'Arm')
        origin=wrist if part in ['Hand','Bracer'] else shoulder if part=='UpperArm' else elbow
        up=(wrist-B(side+'HandMiddle1')).normalized() if part=='Hand' else (elbow-wrist).normalized() if part=='Bracer' else (shoulder-elbow).normalized() if part=='UpperArm' else ((shoulder-elbow).normalized()+(elbow-wrist).normalized()).normalized()
        across=B(side+'HandPinky1')-B(side+'HandIndex1');x=(across-up*across.dot(up)).normalized();y=up.cross(x).normalized();basis=Matrix((x,y,up)).transposed();inverse=basis.transposed()
        points=[inverse@(obj.matrix_world@v.co-origin) for v in obj.data.vertices];lo=Vector(tuple(min(p[k] for p in points) for k in range(3)));hi=Vector(tuple(max(p[k] for p in points) for k in range(3)));size=hi-lo
        factors=Vector((1,1,1));target_length=size.z
        if part=='Hand':
            # Male mean gauntlet size relative to forearm length, fitted to this rig.
            ratio=(elbow-wrist).length/.2537
            factors.x=.15425*ratio/size.x;factors.y=.12772*ratio/size.y;target_length=.22629*ratio
        elif part=='Bracer':factors.x=.13590*height_scale/size.x;factors.y=.11220*height_scale/size.y
        elif part=='UpperArm':factors.x=1.20;factors.y=1.20
        else:factors.x=1.07;factors.y=1.07
        invworld=obj.matrix_world.inverted()
        for vertex,p in zip(obj.data.vertices,points):
            q=Vector((p.x*factors.x,p.y*factors.y,hi.z-(hi.z-p.z)*target_length/size.z if part=='Hand' else p.z));vertex.co=invworld@(origin+basis@q)
        obj.data.update();obj['female_arm_balance']='male proportions, rig-length adapted'
        changes.append(dict(name=obj.name,part=part,before_dimensions=list(size),after_dimensions=[size.x*factors.x,size.y*factors.y,target_length]))
    return changes
