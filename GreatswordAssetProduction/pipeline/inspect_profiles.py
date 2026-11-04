import bpy,json,numpy as np
from pathlib import Path
r=Path(r'C:\The Veilbound\GreatswordAssetProduction');bpy.ops.wm.open_mainfile(filepath=str(r/'raw/Imported_Original.blend'))
for o in sorted([o for o in bpy.context.scene.objects if o.type=='MESH'],key=lambda o:o.location.y):
 a=np.array([list(o.matrix_world@v.co) for v in o.data.vertices]);lo=a.min(0);hi=a.max(0);h=hi[2]-lo[2];samples=[]
 for frac in [.1,.25,.4,.55,.65,.72,.8,.87,.93]:
  selected=a[np.abs((a[:,2]-lo[2])/h-frac)<.025]
  if len(selected):samples.append([frac,round(np.ptp(selected[:,1]),4),round(np.ptp(selected[:,0]),4)])
 print('PROFILE',o.name,'centreY',round((lo[1]+hi[1])/2,3),samples)
