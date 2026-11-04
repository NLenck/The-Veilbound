using UnityEngine;

namespace Veilbound
{
    // Low, human-like crawler: long arms, folded hind legs and alternating planted contacts.
    public sealed class GhoulRig
    {
        readonly Transform body;
        readonly Transform[] upper=new Transform[4],lower=new Transform[4],joints=new Transform[4],contacts=new Transform[4];
        readonly Color skin=new Color(.34f,.38f,.36f);
        readonly Color corruption=new Color(.30f,.12f,.095f);
        public GhoulRig(Transform root)
        {
            body=WorldArt.Group("Ghoul hunched body",root);
            RoundedArt.Shape(body,"Arched ribcage",new Vector3(0,.66f,.03f),new Vector3(.39f,.35f,.70f),skin);
            RoundedArt.Shape(body,"Wasted waist",new Vector3(0,.54f,-.32f),new Vector3(.26f,.27f,.40f),corruption);
            RoundedArt.Shape(body,"Bony pelvis",new Vector3(0,.51f,-.48f),new Vector3(.38f,.28f,.30f),skin);
            RoundedArt.Shape(body,"Forward neck",new Vector3(0,.61f,.35f),new Vector3(.22f,.23f,.29f),skin);
            var head=RoundedArt.Shape(body,"Ghoul skull",new Vector3(0,.59f,.52f),new Vector3(.30f,.33f,.35f),skin);
            head.localRotation=Quaternion.Euler(20,0,0);
            RoundedArt.Shape(body,"Open jaw",new Vector3(0,.49f,.66f),new Vector3(.14f,.075f,.025f),new Color(.08f,.04f,.035f));
            for(int t=0;t<4;t++)RoundedArt.Shape(body,"Broken tooth",new Vector3((t-1.5f)*.028f,.515f,.681f),new Vector3(.017f,.025f,.018f),new Color(.57f,.54f,.44f));
            for(int s=-1;s<=1;s+=2)
            {
                RoundedArt.Shape(body,"Sunken eye",new Vector3(s*.073f,.635f,.65f),new Vector3(.08f,.045f,.024f),corruption);
                var eye=RoundedArt.Shape(body,"Veil eye",new Vector3(s*.073f,.635f,.665f),new Vector3(.028f,.021f,.012f),WorldArt.VeilRed);
                eye.GetComponent<Renderer>().sharedMaterial=WorldArt.Glow(WorldArt.VeilRed);
                for(int r=0;r<3;r++)RoundedArt.Shape(body,"Raised ribs",new Vector3(s*.168f,.71f,.21f-r*.16f),new Vector3(.035f,.12f,.04f),new Color(.49f,.47f,.41f));
            }
            for(int i=0;i<3;i++)WorldArt.Crystal(body,new Vector3(0,.81f,.14f-i*.17f),.13f,WorldArt.VeilRed);
            for(int i=0;i<4;i++)
            {
                bool front=i<2;
                upper[i]=RoundedArt.Shape(root,"Ghoul upper limb "+i,Vector3.zero,Vector3.one,skin);
                lower[i]=RoundedArt.Shape(root,"Ghoul lower limb "+i,Vector3.zero,Vector3.one,front?corruption:skin);
                joints[i]=RoundedArt.Shape(root,"Ghoul joint "+i,Vector3.zero,Vector3.one*(front?.14f:.16f),skin);
                contacts[i]=WorldArt.Group(front?"Planted claw hand":"Planted hind foot",root);
                RoundedArt.Shape(contacts[i],"Knuckles",Vector3.zero,new Vector3(front?.18f:.13f,.10f,.20f),skin);
                for(int c=0;c<3;c++)RoundedArt.Shape(contacts[i],"Claw",new Vector3((c-1)*.05f,-.015f,.13f),new Vector3(.025f,.035f,.14f),new Color(.54f,.50f,.43f));
            }
            Animate(0,0);
        }
        static void Segment(Transform shape,Vector3 from,Vector3 to,float width)
        {
            shape.localPosition=(from+to)*.5f;
            shape.localRotation=Quaternion.FromToRotation(Vector3.up,to-from);
            shape.localScale=new Vector3(width,Vector3.Distance(from,to)+width*.35f,width);
        }
        public void Animate(float time,float moving)
        {
            float cycle=time*2.8f;
            float bob=Mathf.Sin(cycle*Mathf.PI*4)*.035f*moving;
            body.localPosition=Vector3.up*bob;
            body.localRotation=Quaternion.Euler(0,Mathf.Sin(cycle*Mathf.PI*2)*3*moving,0);
            for(int i=0;i<4;i++)
            {
                bool front=i<2;float side=i%2==0?-1:1;
                float phase=Mathf.Repeat(cycle+((i==0||i==3)?0:.5f),1);
                // Contact sweeps backwards during stance; the return stroke lifts clear of the floor.
                float stride=phase<.6f?Mathf.Lerp(.21f,-.21f,phase/.6f):Mathf.Lerp(-.21f,.21f,(phase-.6f)/.4f);
                float lift=phase<.6f?0:Mathf.Sin((phase-.6f)/.4f*Mathf.PI)*.20f;
                Vector3 end=new Vector3(side*(front?.32f:.25f),.065f+lift*moving,(front?.43f:-.53f)+stride*moving);
                Vector3 start=new Vector3(side*(front?.23f:.18f),(front?.69f:.53f)+bob,front?.23f:-.45f);
                Vector3 joint=(start+end)*.5f+new Vector3(side*.055f,.025f,front?-.19f:.22f);
                Segment(upper[i],start,joint,front?.125f:.16f);
                Segment(lower[i],joint,end,front?.105f:.105f);
                joints[i].localPosition=joint;contacts[i].localPosition=end;
                contacts[i].localRotation=Quaternion.Euler(-lift*100*moving,side*-8,0);
            }
        }
    }
}
