using UnityEngine;
using System.Collections.Generic;

namespace Veilbound
{
    public static class WorldArt
    {
        static readonly Dictionary<Color, Material> materials = new Dictionary<Color, Material>();
        static readonly Dictionary<Color, Material> glowing = new Dictionary<Color, Material>();
        static Mesh bevelMesh, crystalMesh;
        public static readonly Color Gold = new Color(.76f,.53f,.25f), Stone = new Color(.18f,.21f,.28f), VeilRed = new Color(.78f,.095f,.045f);
        public static Material Mat(Color c)
        {
            if (materials.TryGetValue(c, out Material m) && m) return m;
            var template = Resources.Load<Material>("VeilMaterial");
            m = template ? new Material(template) : new Material(Shader.Find("Standard")); m.color = c; m.SetFloat("_Glossiness", .24f);m.enableInstancing=true;
            materials[c] = m; return m;
        }
        public static Transform Part(Transform parent, string name, PrimitiveType shape, Vector3 pos, Vector3 scale, Color colour)
        {
            GameObject g;
            if(shape==PrimitiveType.Cube)
            {
                g=new GameObject(name);g.AddComponent<MeshFilter>().sharedMesh=BevelMesh();g.AddComponent<MeshRenderer>();
            }
            else { g=GameObject.CreatePrimitive(shape); Object.Destroy(g.GetComponent<Collider>()); }
            g.name = name; g.transform.SetParent(parent, false);
            g.transform.localPosition = pos; g.transform.localScale = scale;
            g.GetComponent<Renderer>().sharedMaterial = Mat(colour); return g.transform;
        }
        public static Transform Group(string name, Transform parent = null)
        { var t = new GameObject(name).transform; t.SetParent(parent, false); return t; }
        public static Transform Crystal(Transform parent, Vector3 pos, float scale, Color c)
        {
            var t = Part(parent, "Veil crystal", PrimitiveType.Cube, pos, new Vector3(.5f,1.4f,.5f)*scale,c);
            if(!crystalMesh)
            {
                var v=new List<Vector3>(); var triangles=new List<int>();
                for(int i=0;i<6;i++)
                {
                    float a=i*Mathf.PI/3,b=(i+1)*Mathf.PI/3;
                    Vector3 p=new Vector3(Mathf.Cos(a)*.5f,-.25f,Mathf.Sin(a)*.5f), q=new Vector3(Mathf.Cos(b)*.5f,-.25f,Mathf.Sin(b)*.5f);
                    AddFace(v,triangles,new[]{p,Vector3.up*.65f,q},(p+q+Vector3.up*.3f).normalized);
                    AddFace(v,triangles,new[]{p,q,Vector3.down*.5f},(p+q+Vector3.down).normalized);
                }
                crystalMesh=new Mesh{name="Faceted crystal"};crystalMesh.SetVertices(v);crystalMesh.SetTriangles(triangles,0);crystalMesh.RecalculateNormals();
            }
            t.GetComponent<MeshFilter>().sharedMesh=crystalMesh;
            t.GetComponent<Renderer>().sharedMaterial=Glow(c);
            t.localRotation = Quaternion.Euler(10,45,18); return t;
        }
        public static Material Glow(Color c)
        {
            if(glowing.TryGetValue(c,out Material m)&&m) return m;
            var template=Resources.Load<Material>("VeilGlowMaterial");
            m=template?new Material(template):new Material(Mat(c));m.color=c;m.enableInstancing=true;
            m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",c*.65f);glowing[c]=m;return m;
        }
        static void AddFace(List<Vector3> vertices,List<int> indices,Vector3[] points,Vector3 normal)
        {
            Vector3 centre=Vector3.zero;foreach(var p in points)centre+=p;centre/=points.Length;
            Vector3 u=(points[0]-centre).normalized,v=Vector3.Cross(normal,u).normalized;
            System.Array.Sort(points,(a,b)=>Mathf.Atan2(Vector3.Dot(a-centre,v),Vector3.Dot(a-centre,u)).CompareTo(Mathf.Atan2(Vector3.Dot(b-centre,v),Vector3.Dot(b-centre,u))));
            int offset=vertices.Count;vertices.AddRange(points);
            for(int i=1;i<points.Length-1;i++){indices.Add(offset);indices.Add(offset+i);indices.Add(offset+i+1);}
        }
        static Mesh BevelMesh()
        {
            if(bevelMesh)return bevelMesh;
            var vertices=new List<Vector3>();var indices=new List<int>();const float inner=.38f,outer=.5f;
            Vector3[] axes={Vector3.right,Vector3.up,Vector3.forward};
            for(int a=0;a<3;a++)for(int sign=-1;sign<=1;sign+=2)
            {
                Vector3 n=axes[a]*sign,u=axes[(a+1)%3]*inner,v=axes[(a+2)%3]*inner,c=n*outer;
                AddFace(vertices,indices,new[]{c+u+v,c+u-v,c-u-v,c-u+v},n);
            }
            for(int a=0;a<3;a++)for(int b=a+1;b<3;b++)for(int sa=-1;sa<=1;sa+=2)for(int sb=-1;sb<=1;sb+=2)
            {
                Vector3 aa=axes[a]*sa,bb=axes[b]*sb,along=axes[3-a-b]*inner;
                Vector3 p=aa*outer+bb*inner,q=aa*inner+bb*outer;
                AddFace(vertices,indices,new[]{p+along,p-along,q-along,q+along},(aa+bb).normalized);
            }
            for(int x=-1;x<=1;x+=2)for(int y=-1;y<=1;y+=2)for(int z=-1;z<=1;z+=2)
                AddFace(vertices,indices,new[]{new Vector3(x*outer,y*inner,z*inner),new Vector3(x*inner,y*outer,z*inner),new Vector3(x*inner,y*inner,z*outer)},new Vector3(x,y,z).normalized);
            bevelMesh=new Mesh{name="Bevelled stone and steel"};bevelMesh.SetVertices(vertices);bevelMesh.SetTriangles(indices,0);bevelMesh.RecalculateNormals();return bevelMesh;
        }
        public static void Ring(Transform parent, Vector3 centre, float radius, Color c, float width = .08f, int points = 64)
        {
            var go = new GameObject("Rune ring"); go.transform.SetParent(parent, false);
            var line = go.AddComponent<LineRenderer>(); line.useWorldSpace = false; line.loop = true;
            line.positionCount = points; line.widthMultiplier = width;
            line.sharedMaterial = Glow(c); line.startColor = line.endColor = Color.white;
            for(int i=0;i<points;i++) { float a = i*Mathf.PI*2/points; line.SetPosition(i,centre+new Vector3(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius)); }
        }
        public static void Arena(Transform root,ArenaCollision collision){SanctuaryArena.Build(root,collision);}
        public static HeroRig Hero(Transform parent, Settlement city)
        { return new HeroRig(parent,city); }
    }

}
