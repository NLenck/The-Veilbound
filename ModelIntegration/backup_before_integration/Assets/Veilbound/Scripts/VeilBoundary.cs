using UnityEngine;

namespace Veilbound
{
    public sealed class VeilBoundary
    {
        readonly Transform[] sparks=new Transform[0];
        float clock;
        public VeilBoundary(Transform parent)
        {
            var root=WorldArt.Group("The surrounding Veil",parent);
            for(int i=0;i<12;i++)
            {
                float a=i*Mathf.PI/6;var light=WorldArt.Group("Veil glow",root).gameObject.AddComponent<Light>();
                light.type=LightType.Point;light.color=new Color(.9f,.16f,.055f);light.intensity=.65f;light.range=6;light.transform.localPosition=new Vector3(Mathf.Cos(a)*(ArenaCollision.PlayerBoundary+5),1.4f,Mathf.Sin(a)*(ArenaCollision.PlayerBoundary+5));
            }
            for(int i=0;i<sparks.Length;i++)
            {
                sparks[i]=WorldArt.Part(root,"Veil ember",PrimitiveType.Cube,Vector3.zero,Vector3.one*.045f,WorldArt.VeilRed);
                sparks[i].GetComponent<Renderer>().sharedMaterial=WorldArt.Glow(i%4==0?new Color(.95f,.34f,.07f):new Color(.65f,.10f,.025f));
            }
            Tick(0);
        }
        public void Tick(float dt)
        {
            clock+=dt;Shader.SetGlobalFloat("_VeilClock",clock);
            for(int i=0;i<sparks.Length;i++)
            {
                float a=i*2.39996f+clock*.008f,r=35.8f+Mathf.Sin(i*12.7f+clock*.3f)*2.5f;
                float phase=Mathf.Repeat(i*.617f+clock*.10f,1);
                sparks[i].localPosition=new Vector3(Mathf.Cos(a)*r,.2f+phase*8,Mathf.Sin(a)*r);
                sparks[i].localScale=new Vector3(1,i%3==0?2:1,1)*(.018f+.032f*Mathf.Sin(phase*Mathf.PI));
            }
        }
    }
}
