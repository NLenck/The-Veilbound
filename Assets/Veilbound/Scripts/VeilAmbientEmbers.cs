using System;using UnityEngine;using UnityEngine.Rendering;
namespace Veilbound {
 public sealed class VeilAmbientEmbers:MonoBehaviour {
  public int Count{get;private set;}public float Radius{get;private set;}public Mesh Mesh=>mesh;
  Mesh mesh;Vector3[] vertices,anchors;Color[] colors;float[] lifetimes,phases;bool crimsonBoundary;
  public void Initialize(float radius,bool villageBoundary=false){
   crimsonBoundary=villageBoundary;
   Radius=radius;Count=Mathf.Clamp(Mathf.RoundToInt(192*radius*radius/(42*42)),144,300);
   var material=Resources.Load<Material>("VeilAmbientEmbers");if(!material)throw new InvalidOperationException("Missing VeilAmbientEmbers material");
   mesh=new Mesh{name="Sparse Veil embers - one dynamic batch"};mesh.MarkDynamic();vertices=new Vector3[Count*4];colors=new Color[Count*4];anchors=new Vector3[Count];lifetimes=new float[Count];phases=new float[Count];var uv=new Vector2[Count*4];var sizes=new Vector2[Count*4];var triangles=new int[Count*6];
   for(int i=0;i<Count;i++){
    float angle=i*2.399963f,r=radius*.94f*Mathf.Sqrt((i+.5f)/Count);anchors[i]=new Vector3(Mathf.Cos(angle)*r,0,Mathf.Sin(angle)*r);lifetimes[i]=11+(i%7);phases[i]=Mathf.Repeat(i*.618034f+.13f,1);float size=.10f+(i%5)*.018f;
    if(crimsonBoundary&&i%4!=0){r=radius-.6f+Mathf.Repeat(i*.381966f,1)*6.5f;anchors[i]=new Vector3(Mathf.Cos(angle)*r,0,Mathf.Sin(angle)*r);}
    for(int j=0;j<4;j++){uv[i*4+j]=new Vector2(j==1||j==2?1:0,j>=2?1:0);sizes[i*4+j]=new Vector2(size,size*(i%4==0?1.8f:1.2f));}
    int a=i*4,b=i*6;triangles[b]=a;triangles[b+1]=a+1;triangles[b+2]=a+2;triangles[b+3]=a;triangles[b+4]=a+2;triangles[b+5]=a+3;
   }
   mesh.vertices=vertices;mesh.uv=uv;mesh.uv2=sizes;mesh.triangles=triangles;mesh.bounds=new Bounds(Vector3.up*1.5f,new Vector3((radius+(crimsonBoundary?8:2))*2,6,(radius+(crimsonBoundary?8:2))*2));
   gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;var renderer=gameObject.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;renderer.lightProbeUsage=LightProbeUsage.Off;renderer.reflectionProbeUsage=ReflectionProbeUsage.Off;Sample(0);
  }
  public void Sample(float time){if(!mesh)return;for(int i=0;i<Count;i++){
   float phase=Mathf.Repeat(phases[i]+time/lifetimes[i],1);float fade=Mathf.Sin(phase*Mathf.PI);fade=.25f+.75f*fade*fade;
   Vector3 point=anchors[i]+new Vector3(Mathf.Sin(i*1.3f+phase*4)*.45f+(phase-.5f)*1.2f,.25f+phase*2.5f,Mathf.Cos(i*.7f+phase*3)*.35f);
   Color color=i%5==0?new Color(.65f,.36f,.24f,fade*.60f):new Color(1,.72f,.45f,fade*.80f);
   if(crimsonBoundary)color=i%5==0?new Color(1,.30f,.075f,fade*.85f):new Color(.95f,.055f,.018f,fade*.85f);
   for(int j=0;j<4;j++){vertices[i*4+j]=point;colors[i*4+j]=color;}
  }mesh.vertices=vertices;mesh.colors=colors;}
  void OnDestroy(){if(mesh){if(Application.isPlaying)Destroy(mesh);else DestroyImmediate(mesh);}}
 }
}
