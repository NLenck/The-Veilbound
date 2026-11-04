using System.Collections.Generic;
using UnityEngine;

namespace Veilbound
{
    public static class ArmourVisuals
    {
        static Mesh helmet;
        public static Transform Build(Transform[] bones,Settlement city,Color cloth)
        {
            Transform cape=null;
            for(int slot=0;slot<9;slot++)
            {
                int item=city.Item(slot);if(item==0)continue;
                int material=Gear.Material(item),tier=Gear.Tier(item);
                Color colour=material==1?cloth:material==2?new Color(.24f,.14f,.085f):new Color(.38f,.43f,.49f);
                if(slot==0&&material!=3){Helmet(bones[4],colour,material);continue;}
                if(slot==8)
                {
                    cape=RoundedArt.Cloak(bones[2],new Vector3(0,-.43f,-.13f),new Vector3(.73f,1.12f,.82f),cloth);
                    cape.name="Equipped removable cape";continue;
                }
                var anchors=new Dictionary<int,Transform>();
                foreach(var piece in HeroArt.Data.pieces)
                {
                    if(piece.slot!=slot)continue;
                    bool plate=material==3;
                    if(plate!=(piece.family=="plate"))continue;
                    if(piece.category!="gear"&&piece.category!="trim"&&piece.category!="strap"&&piece.category!="inlay"&&piece.category!="rune")continue;
                    if(!anchors.TryGetValue(piece.bone,out Transform anchor))
                    {
                        if(slot==1)
                        {
                            anchor=WorldArt.Group("Shoulder anchor / "+HeroBodyMesh.Names[piece.bone],bones[0].parent);
                            var follow=anchor.gameObject.AddComponent<HeroGearAnchor>();follow.joint=bones[piece.bone];follow.chest=bones[2];follow.Sync();
                        }
                        else anchor=WorldArt.Group(Gear.Slots[slot]+" anchor / "+HeroBodyMesh.Names[piece.bone],bones[piece.bone]);
                        WorldArt.Group("Detail socket / "+Gear.Slots[slot]+" / "+HeroBodyMesh.Names[piece.bone],anchor);
                        anchors[piece.bone]=anchor;
                    }
                    Color c=piece.category=="strap"?new Color(.14f,.10f,.075f):piece.category=="trim"?WorldArt.Gold:colour;
                    var part=HeroArt.Attach(anchor,piece,plate?(Color?)null:c);
                    if(!plate&&tier==2 && piece.category=="trim")part.GetComponent<MeshRenderer>().sharedMaterial=WorldArt.Mat(WorldArt.Gold*1.12f);
                }
            }
            return cape;
        }
        public static void Sync(Transform root)
        {foreach(var anchor in root.GetComponentsInChildren<HeroGearAnchor>())anchor.Sync();}
        static Transform Helmet(Transform parent,Color colour,int material)
        {
            if(!helmet)
            {
                const int sides=24;var vertices=new List<Vector3>();var triangles=new List<int>();
                float[] heights={-.09f,.055f,.17f,.225f},radius={.123f,.142f,.111f,.027f};
                for(int ring=0;ring<4;ring++)for(int x=0;x<=sides;x++)
                {
                    float a=x*Mathf.PI*2/sides,y=heights[ring];
                    if(ring==0)y+=Mathf.Max(0,Mathf.Cos(a))*.16f;
                    vertices.Add(new Vector3(Mathf.Sin(a)*radius[ring],y,Mathf.Cos(a)*radius[ring]*.94f));
                }
                for(int y=0;y<3;y++)for(int x=0;x<sides;x++)
                {int a=y*(sides+1)+x;triangles.AddRange(new[]{a,a+1,a+sides+1,a+1,a+sides+2,a+sides+1});}
                int cap=vertices.Count;vertices.Add(Vector3.up*.23f);
                for(int x=0;x<sides;x++)triangles.AddRange(new[]{cap,3*(sides+1)+x,3*(sides+1)+x+1});
                helmet=new Mesh{name="Fitted open-face helmet"};helmet.SetVertices(vertices);helmet.SetTriangles(triangles,0);helmet.RecalculateNormals();
            }
            var root=WorldArt.Group(material==1?"Separate fitted hood":"Separate fitted helmet",parent);
            root.gameObject.AddComponent<MeshFilter>().sharedMesh=helmet;
            root.gameObject.AddComponent<MeshRenderer>().sharedMaterial=WorldArt.Mat(colour);return root;
        }
    }
}
