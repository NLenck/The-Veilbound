using System;
using System.Collections.Generic;
using UnityEngine;

namespace Veilbound
{
    // A single welded surface is extracted from a sculpted distance field, then weighted to a skeleton.
    public static class HeroBodyMesh
    {
        public static readonly string[] Names={"Pelvis","Spine","Chest","Neck","Head","Left shoulder","Left elbow","Left wrist","Right shoulder","Right elbow","Right wrist","Left hip","Left knee","Left ankle","Right hip","Right knee","Right ankle"};
        public static readonly int[] Parents={-1,0,1,2,3,2,5,6,2,8,9,0,11,12,0,14,15};
        public static readonly Vector3[] Rest={
            new Vector3(0,.95f,0),new Vector3(0,1.18f,0),new Vector3(0,1.46f,0),new Vector3(0,1.69f,0),new Vector3(0,1.87f,.015f),
            new Vector3(-.30f,1.57f,0),new Vector3(-.43f,1.20f,.03f),new Vector3(-.43f,.91f,.13f),
            new Vector3(.30f,1.57f,0),new Vector3(.43f,1.20f,.03f),new Vector3(.43f,.91f,.13f),
            new Vector3(-.135f,.95f,0),new Vector3(-.17f,.54f,.015f),new Vector3(-.17f,.16f,0),
            new Vector3(.135f,.95f,0),new Vector3(.17f,.54f,.015f),new Vector3(.17f,.16f,0)};
        struct Form { public Vector3 a,b,r;public float endRadius;public bool capsule; }
        static List<Form> Sculpt(int armour)
        {
            var forms=new List<Form>();
            void Ell(Vector3 p,Vector3 r){forms.Add(new Form{a=p,r=r});}
            void Tube(Vector3 a,Vector3 b,float ra,float rb){forms.Add(new Form{a=a,b=b,r=Vector3.one*ra,endRadius=rb,capsule=true});}
            Ell(new Vector3(0,.96f,0),new Vector3(.205f,.18f,.15f));
            Ell(new Vector3(0,1.17f,0),new Vector3(.18f,.25f,.145f));
            Ell(new Vector3(0,1.43f,0),new Vector3(.27f+armour*.018f,.245f,.175f+armour*.01f));
            Tube(new Vector3(0,1.60f,0),new Vector3(0,1.79f,.01f),.084f,.075f);
            Ell(new Vector3(0,1.93f,.015f),new Vector3(.14f+armour*.005f,.205f,.14f));
            Ell(new Vector3(0,1.835f,.045f),new Vector3(.115f,.095f,.115f));
            Ell(new Vector3(0,1.925f,.143f),new Vector3(.032f,.054f,.052f));
            Ell(new Vector3(0,2.06f,-.01f),new Vector3(.147f+armour*.007f,.105f,.148f));
            for(int side=-1;side<=1;side+=2)
            {
                int a=side<0?5:8,leg=side<0?11:14;
                Tube(Rest[a],Rest[a+1],.105f+armour*.009f,.078f);
                Tube(Rest[a+1],Rest[a+2],.083f,.062f+armour*.006f);
                Ell(Rest[a],new Vector3(.116f+armour*.014f,.125f,.118f+armour*.008f));
                Ell(Rest[a+2]+new Vector3(0,-.045f,.022f),new Vector3(.064f,.088f,.055f));
                Tube(Rest[leg],Rest[leg+1],.12f,.087f);
                Tube(Rest[leg+1],Rest[leg+2],.085f,.067f);
                Ell(new Vector3(side*.17f,.105f,.072f),new Vector3(.098f,.106f,.165f));
            }
            return forms;
        }
        static float Field(Vector3 p,List<Form> forms)
        {
            float d=100;
            foreach(var f in forms)
            {
                float next;
                if(f.capsule)
                {
                    Vector3 axis=f.b-f.a;float t=Mathf.Clamp01(Vector3.Dot(p-f.a,axis)/axis.sqrMagnitude);
                    next=(p-f.a-axis*t).magnitude-Mathf.Lerp(f.r.x,f.endRadius,t);
                }
                else
                {
                    Vector3 q=p-f.a;
                    float k0=new Vector3(q.x/f.r.x,q.y/f.r.y,q.z/f.r.z).magnitude;
                    float k1=new Vector3(q.x/(f.r.x*f.r.x),q.y/(f.r.y*f.r.y),q.z/(f.r.z*f.r.z)).magnitude;
                    next=k1>.000001f?k0*(k0-1)/k1:-Mathf.Min(f.r.x,Mathf.Min(f.r.y,f.r.z));
                }
                float h=Mathf.Max(.042f-Mathf.Abs(d-next),0)/.042f;
                d=Mathf.Min(d,next)-h*h*.042f*.25f;
            }
            return Mathf.Max(d,.0127f-p.y);
        }
        static int MaterialAt(Vector3 p,int armour)
        {
            if(p.y>1.76f)
            {
                if(p.y>2.035f || p.z<-.03f && p.y>1.94f)return armour==2?3:2;
                if(p.z>.10f && Mathf.Abs(p.y-1.969f)<.019f && Mathf.Abs(Mathf.Abs(p.x)-.059f)<.029f)return 4;
                if(p.z>.11f && Mathf.Abs(p.y-1.995f)<.013f)return 2;
                if(p.z>.13f && Mathf.Abs(p.y-1.865f)<.012f && Mathf.Abs(p.x)<.047f)return 2;
                if(armour==2 && (p.y>1.99f || Mathf.Abs(p.x)>.105f || p.z<.015f))return 3;
                return 1;
            }
            if(Mathf.Abs(p.y-1.105f)<.032f && Mathf.Abs(p.x)<.235f)return 2;
            if(Mathf.Abs(p.x)>.36f && p.y<.995f && p.y>.79f)return armour==0?1:3;
            if(p.y<.35f)return 2;
            if(p.y<1.0f)return 5;
            return armour>0?3:0;
        }
        static Vector3 SegmentEnd(int bone)
        {
            for(int i=0;i<Parents.Length;i++)if(Parents[i]==bone)return Rest[i];
            if(bone==7||bone==10)return Rest[bone]+new Vector3(0,-.10f,.025f);
            if(bone==13||bone==16)return Rest[bone]+new Vector3(0,-.04f,.13f);
            return Rest[bone]+Vector3.up*.19f;
        }
        static BoneWeight Weight(Vector3 p)
        {
            float[] best=new float[4];int[] ids=new int[4];
            for(int i=0;i<Rest.Length;i++)
            {
                Vector3 a=Rest[i],axis=SegmentEnd(i)-a;float t=Mathf.Clamp01(Vector3.Dot(p-a,axis)/axis.sqrMagnitude);
                float distance=(p-a-axis*t).magnitude;float value=1/Mathf.Pow(distance+.025f,4);
                for(int j=0;j<4;j++)if(value>best[j]){for(int k=3;k>j;k--){best[k]=best[k-1];ids[k]=ids[k-1];}best[j]=value;ids[j]=i;break;}
            }
            float sum=best[0]+best[1]+best[2]+best[3];
            return new BoneWeight{boneIndex0=ids[0],boneIndex1=ids[1],boneIndex2=ids[2],boneIndex3=ids[3],weight0=best[0]/sum,weight1=best[1]/sum,weight2=best[2]/sum,weight3=best[3]/sum};
        }
        public static Mesh Generate(int armour)
        {
            if(HeroArt.Data!=null)
            {
                Mesh authored=HeroArt.Body();Vector3[] authoredPoints=authored.vertices;
                var authoredWeights=new BoneWeight[authoredPoints.Length];
                for(int i=0;i<authoredPoints.Length;i++)authoredWeights[i]=Weight(authoredPoints[i]);
                authored.boneWeights=authoredWeights;authored.SetUVs(1,new List<Vector3>(authoredPoints));
                var authoredBind=new Matrix4x4[Rest.Length];
                for(int i=0;i<Rest.Length;i++)authoredBind[i]=Matrix4x4.TRS(Rest[i],Quaternion.identity,Vector3.one).inverse;
                authored.bindposes=authoredBind;return authored;
            }
            var forms=Sculpt(armour);const float step=.025f;
            Vector3 origin=new Vector3(-.65f,-.025f,-.25f);const int nx=53,ny=92,nz=25;
            int Id(int x,int y,int z)=>(x*ny+y)*nz+z;
            var points=new Vector3[nx*ny*nz];var field=new float[points.Length];
            for(int x=0;x<nx;x++)for(int y=0;y<ny;y++)for(int z=0;z<nz;z++)
            {int i=Id(x,y,z);points[i]=origin+new Vector3(x,y,z)*step;field[i]=Field(points[i],forms);}
            var vertices=new List<Vector3>();var edges=new Dictionary<ulong,int>();var sub=new List<int>[6];for(int i=0;i<6;i++)sub[i]=new List<int>();
            int Edge(int a,int b)
            {
                uint lo=(uint)Mathf.Min(a,b),hi=(uint)Mathf.Max(a,b);ulong key=((ulong)lo<<32)|hi;
                if(edges.TryGetValue(key,out int n))return n;
                float t=field[a]/(field[a]-field[b]);Vector3 p=Vector3.LerpUnclamped(points[a],points[b],t);
                n=vertices.Count;vertices.Add(p);edges[key]=n;return n;
            }
            int[,] tetra={{0,5,1,6},{0,1,2,6},{0,2,3,6},{0,3,7,6},{0,7,4,6},{0,4,5,6}};
            int[,] pairs={{0,1},{0,2},{0,3},{1,2},{1,3},{2,3}};
            int[] cube=new int[8],ids=new int[4],poly=new int[4];
            for(int x=0;x<nx-1;x++)for(int y=0;y<ny-1;y++)for(int z=0;z<nz-1;z++)
            {
                cube[0]=Id(x,y,z);cube[1]=Id(x+1,y,z);cube[2]=Id(x+1,y+1,z);cube[3]=Id(x,y+1,z);
                cube[4]=Id(x,y,z+1);cube[5]=Id(x+1,y,z+1);cube[6]=Id(x+1,y+1,z+1);cube[7]=Id(x,y+1,z+1);
                for(int tet=0;tet<6;tet++)
                {
                    Vector3 inside=Vector3.zero,outside=Vector3.zero;int ni=0,no=0;
                    for(int j=0;j<4;j++){ids[j]=cube[tetra[tet,j]];if(field[ids[j]]<0){inside+=points[ids[j]];ni++;}else{outside+=points[ids[j]];no++;}}
                    if(ni==0||no==0)continue;
                    int count=0;for(int j=0;j<6;j++){int a=ids[pairs[j,0]],b=ids[pairs[j,1]];if((field[a]<0)!=(field[b]<0))poly[count++]=Edge(a,b);}
                    Vector3 centre=Vector3.zero;for(int j=0;j<count;j++)centre+=vertices[poly[j]];centre/=count;
                    Vector3 normal=(outside/no-inside/ni).normalized,u=(vertices[poly[0]]-centre).normalized,v=Vector3.Cross(normal,u);
                    // Sort the 3/4 edge intersections around their outward normal.
                    for(int a=0;a<count-1;a++)for(int b=a+1;b<count;b++)
                    {
                        Vector3 pa=vertices[poly[a]]-centre,pb=vertices[poly[b]]-centre;
                        if(Mathf.Atan2(Vector3.Dot(pa,v),Vector3.Dot(pa,u))>Mathf.Atan2(Vector3.Dot(pb,v),Vector3.Dot(pb,u))){int swap=poly[a];poly[a]=poly[b];poly[b]=swap;}
                    }
                    var triangles=sub[MaterialAt(centre,armour)];for(int j=1;j<count-1;j++){triangles.Add(poly[0]);triangles.Add(poly[j]);triangles.Add(poly[j+1]);}
                }
            }
            var mesh=new Mesh{name="Connected Veilborne body T"+armour};mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.SetVertices(vertices);mesh.SetUVs(1,vertices);var allTriangles=new List<int>();for(int i=0;i<6;i++)allTriangles.AddRange(sub[i]);mesh.SetTriangles(allTriangles,0);
            var weights=new BoneWeight[vertices.Count];for(int i=0;i<weights.Length;i++)weights[i]=Weight(vertices[i]);mesh.boneWeights=weights;
            var bind=new Matrix4x4[Rest.Length];for(int i=0;i<bind.Length;i++)bind[i]=Matrix4x4.TRS(Rest[i],Quaternion.identity,Vector3.one).inverse;mesh.bindposes=bind;
            mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
        }
    }
}

