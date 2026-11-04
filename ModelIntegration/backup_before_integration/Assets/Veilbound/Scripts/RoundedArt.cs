using System.Collections.Generic;
using UnityEngine;

namespace Veilbound
{
    public static class RoundedArt
    {
        static Mesh sphere,cloth,blade;
        static readonly Dictionary<int,Mesh> profiles=new Dictionary<int,Mesh>();
        public static Transform Shape(Transform parent,string name,Vector3 p,Vector3 size,Color colour)
        {
            if(!sphere)
            {
                const int sides=12,rings=8;
                var v=new List<Vector3>();var normals=new List<Vector3>();var t=new List<int>();
                for(int y=0;y<=rings;y++)for(int x=0;x<=sides;x++)
                {
                    float a=x*Mathf.PI*2/sides,b=y*Mathf.PI/rings;
                    Vector3 n=new Vector3(Mathf.Sin(b)*Mathf.Cos(a),Mathf.Cos(b),Mathf.Sin(b)*Mathf.Sin(a));v.Add(n*.5f);normals.Add(n);
                }
                for(int y=0;y<rings;y++)for(int x=0;x<sides;x++)
                {int i=y*(sides+1)+x;t.AddRange(new[]{i,i+1,i+sides+1,i+1,i+sides+2,i+sides+1});}
                sphere=new Mesh{name="Smooth stylised ellipsoid"};sphere.SetVertices(v);sphere.SetTriangles(t,0);sphere.SetNormals(normals);sphere.RecalculateBounds();
            }
            return MeshPart(parent,name,p,size,colour,sphere);
        }
        static Transform MeshPart(Transform parent,string name,Vector3 p,Vector3 size,Color colour,Mesh mesh)
        {
            var t=WorldArt.Group(name,parent);t.localPosition=p;t.localScale=size;
            t.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;t.gameObject.AddComponent<MeshRenderer>().sharedMaterial=WorldArt.Mat(colour);return t;
        }
        public static Transform Coat(Transform parent,string name,Vector3 p,Vector3 size,Color colour,bool flare=false)
        {
            int key=flare?1:0;
            if(!profiles.TryGetValue(key,out Mesh mesh))
            {
                float[] radius=flare?new[]{.51f,.48f,.36f,.30f}:new[]{.32f,.43f,.49f,.47f,.32f};
                const int sides=16;var v=new List<Vector3>();var tri=new List<int>();
                for(int y=0;y<radius.Length;y++)for(int x=0;x<=sides;x++)
                {float a=x*Mathf.PI*2/sides;v.Add(new Vector3(Mathf.Cos(a)*radius[y],y/(float)(radius.Length-1)-.5f,Mathf.Sin(a)*radius[y]));}
                for(int y=0;y<radius.Length-1;y++)for(int x=0;x<sides;x++)
                {int i=y*(sides+1)+x;tri.AddRange(new[]{i,i+sides+1,i+1,i+1,i+sides+1,i+sides+2});}
                int bottom=v.Count;v.Add(Vector3.down*.5f);int top=v.Count;v.Add(Vector3.up*.5f);
                for(int x=0;x<sides;x++){tri.AddRange(new[]{bottom,x,x+1,top,(radius.Length-1)*(sides+1)+x+1,(radius.Length-1)*(sides+1)+x});}
                mesh=new Mesh{name=flare?"Flared cloth hem":"Curved fitted cuirass"};mesh.SetVertices(v);mesh.SetTriangles(tri,0);mesh.RecalculateNormals();profiles[key]=mesh;
            }
            return MeshPart(parent,name,p,size,colour,mesh);
        }
        public static Transform Cloak(Transform parent,Vector3 p,Vector3 size,Color colour)
        {
            if(!cloth)
            {
                const int columns=8,rows=5;var v=new List<Vector3>();var tri=new List<int>();
                for(int y=0;y<=rows;y++)for(int x=0;x<=columns;x++)
                {
                    float u=x/(float)columns-.5f,h=y/(float)rows;
                    v.Add(new Vector3(u*(.6f+h*.4f),.5f-h-(y==rows?(x%2)*.055f:0),-.18f-Mathf.Cos(u*Mathf.PI)*.12f-h*h*.18f));
                }
                int offset=v.Count;v.AddRange(v.ToArray());
                for(int y=0;y<rows;y++)for(int x=0;x<columns;x++)
                {int i=y*(columns+1)+x;tri.AddRange(new[]{i,i+1,i+columns+1,i+1,i+columns+2,i+columns+1,i+offset,i+columns+1+offset,i+1+offset,i+1+offset,i+columns+1+offset,i+columns+2+offset});}
                cloth=new Mesh{name="Curved flowing cloak"};cloth.SetVertices(v);cloth.SetTriangles(tri,0);cloth.RecalculateNormals();
            }
            return MeshPart(parent,"Cloak",p,size,colour,cloth);
        }
        public static Transform Greatsword(Transform parent,int tier)
        {
            var authored=BronzeHeroVisual.MakeSword(parent,tier);if(authored)return authored;
            var root=WorldArt.Group("Equipped greatsword T"+tier,parent);
            Shape(root,"Leather grip",new Vector3(0,0,.16f),new Vector3(.13f,.13f,.48f),new Color(.10f,.09f,.12f));
            Shape(root,"Pommel",new Vector3(0,0,-.1f),Vector3.one*.18f,WorldArt.Gold);
            var guard=Shape(root,"Curved crossguard",new Vector3(0,0,.43f),new Vector3(.74f,.13f,.2f),WorldArt.Gold);guard.localRotation=Quaternion.Euler(0,0,-5);
            if(!blade)
            {
                // A raised central ridge gives the blade a sharp edge and a real pointed tip.
                Vector3[] v={new Vector3(-.5f,0,0),new Vector3(.5f,0,0),new Vector3(-.46f,0,.77f),new Vector3(.46f,0,.77f),new Vector3(0,0,1),new Vector3(0,.5f,0),new Vector3(0,.5f,.77f),new Vector3(0,-.5f,0),new Vector3(0,-.5f,.77f)};
                int[] t={0,5,2,2,5,6,5,1,6,1,3,6,2,6,4,6,3,4,0,2,7,2,8,7,7,8,1,1,8,3,2,4,8,8,4,3,0,7,5,5,7,1};
                for(int i=0;i<t.Length;i+=3){int swap=t[i+1];t[i+1]=t[i+2];t[i+2]=swap;}
                blade=new Mesh{name="Ridged greatsword blade"};blade.vertices=v;blade.triangles=t;blade.RecalculateNormals();
            }
            var edge=MeshPart(root,"Blade",new Vector3(0,0,.46f),new Vector3(.26f+tier*.07f,.11f,1.55f+tier*.23f),tier==2?new Color(.72f,.40f,.33f):new Color(.65f,.70f,.75f),blade);
            if(tier>0)
            {
                var fuller=WorldArt.Part(root,"Fuller",PrimitiveType.Cube,new Vector3(0,.06f,1.12f),new Vector3(.032f,.012f,1.25f),tier==2?WorldArt.VeilRed:WorldArt.Gold);
                if(tier==2)fuller.GetComponent<Renderer>().sharedMaterial=WorldArt.Glow(WorldArt.VeilRed);
            }
            return root;
        }
    }
}
