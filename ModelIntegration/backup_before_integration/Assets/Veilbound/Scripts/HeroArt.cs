using System;
using System.Collections.Generic;
using UnityEngine;

namespace Veilbound
{
    // Every piece is authored relative to a named skeletal anchor.
    public static class HeroArt
    {
        [Serializable] public sealed class Piece
        {
            public string key,name,color,category,family,finish;
            public int bone,slot;
            public float[] v;
            public int[] triangles;
        }
        [Serializable] public sealed class Asset
        {
            public float[] body;
            public int[] triangles;
            public Piece[] pieces;
        }
        static Asset asset;
        static Material emberEyeMaterial;
        static readonly Dictionary<string,Material> plateMaterials=new Dictionary<string,Material>();
        static readonly Dictionary<string,Mesh> meshes=new Dictionary<string,Mesh>();
        public static Asset Data
        {
            get
            {
                if(asset==null)
                {
                    var text=Resources.Load<TextAsset>("HeroAnatomyData");
                    if(text)asset=JsonUtility.FromJson<Asset>(text.text);
                }
                return asset;
            }
        }
        static Mesh Create(string name,float[] points,int[] triangles)
        {
            var vertices=new Vector3[points.Length/3];
            for(int i=0;i<vertices.Length;i++)vertices[i]=new Vector3(points[i*3],points[i*3+1],points[i*3+2]);
            var mesh=new Mesh{name=name,indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};
            mesh.vertices=vertices;mesh.triangles=triangles;mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
        }
        public static Mesh Body()
        {
            if(Data==null)throw new InvalidOperationException("Missing authored HeroAnatomyData resource");
            return Create("Human Veilborne / continuous skinned anatomy",Data.body,Data.triangles);
        }
        public static Transform Attach(Transform parent,Piece piece,Color? colour=null)
        {
            if(!meshes.TryGetValue(piece.key,out Mesh mesh))
            {
                mesh=Create(piece.name,piece.v,piece.triangles);
                if(piece.category=="eyes")
                {
                    Bounds bounds=mesh.bounds;var uv=new Vector2[mesh.vertexCount];Vector3[] points=mesh.vertices;
                    for(int i=0;i<uv.Length;i++)uv[i]=new Vector2(Mathf.InverseLerp(bounds.min.x,bounds.max.x,points[i].x),Mathf.InverseLerp(bounds.min.y,bounds.max.y,points[i].y));
                    mesh.uv=uv;
                }
                meshes[piece.key]=mesh;
            }
            var root=WorldArt.Group(piece.name,parent);
            root.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
            ColorUtility.TryParseHtmlString(piece.color,out Color c);
            var renderer=root.gameObject.AddComponent<MeshRenderer>();renderer.sharedMaterial=WorldArt.Mat(colour??c);
            if(piece.family=="plate")
            {
                string materialKey=piece.finish+piece.color;
                if(!plateMaterials.TryGetValue(materialKey,out Material fittedMaterial))
                {
                    fittedMaterial=new Material(WorldArt.Mat(c)){name="Plate / "+piece.finish+" / "+piece.color};
                    float metallic=piece.finish=="leather"?0:piece.finish=="ember"?.15f:.7f;
                    fittedMaterial.SetFloat("_Metallic",metallic);fittedMaterial.SetFloat("_Glossiness",piece.finish=="leather"?.13f:.37f);
                    if(piece.finish=="ember"){fittedMaterial.EnableKeyword("_EMISSION");fittedMaterial.SetColor("_EmissionColor",new Color(.38f,.02f,.008f));}
                    plateMaterials[materialKey]=fittedMaterial;
                }
                renderer.sharedMaterial=fittedMaterial;
            }
            if(piece.category=="hair")renderer.sharedMaterial.SetFloat("_Glossiness",.08f);
            if(piece.category=="eyes")
            {
                if(!emberEyeMaterial)emberEyeMaterial=new Material(Resources.Load<Shader>("HeroEmberEye")){name="Veilborne ember irises"};
                renderer.sharedMaterial=emberEyeMaterial;
            }
            return root;
        }
        public static Renderer[] BaseOutfit(Transform[] bones,Settlement city,Color cloth)
        {
            var eyes=new List<Renderer>();
            foreach(var piece in Data.pieces)
            {
                if(piece.category=="eyes")eyes.Add(Attach(bones[piece.bone],piece).GetComponent<Renderer>());
                if(piece.category=="hair" && city.Item(0)==0)Attach(bones[piece.bone],piece);
                if(piece.category=="basevest" && city.Item(2)==0)
                    Attach(bones[piece.bone],piece,piece.key.Contains("trim")||piece.key.Contains("fastener")?(Color?)null:cloth);
                if(piece.category=="sash" && city.Item(4)==0)Attach(bones[piece.bone],piece);
                if(piece.category=="buckle"&&Gear.Material(city.Item(4))!=3)Attach(bones[piece.bone],piece);
            }
            return eyes.ToArray();
        }
    }
    public sealed class HeroGearAnchor : MonoBehaviour
    {
        public Transform joint,chest;
        public float armFollow=.82f;
        public void Sync()
        {
            transform.position=joint.position;
            transform.rotation=Quaternion.Slerp(chest.rotation,joint.rotation,armFollow);
        }
        void LateUpdate(){Sync();}
    }
}
