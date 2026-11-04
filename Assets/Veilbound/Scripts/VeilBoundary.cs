using UnityEngine;

namespace Veilbound
{
    public sealed class VeilBoundary
    {
        public VeilAmbientEmbers Ambient{get;private set;}
        float clock;
        public VeilBoundary(Transform parent,float playerRadius=ArenaCollision.PlayerBoundary)
        {
            var root=WorldArt.Group("The surrounding Veil",parent);
            for(int i=0;i<12;i++)
            {
                float a=i*Mathf.PI/6;var light=WorldArt.Group("Veil glow",root).gameObject.AddComponent<Light>();
                light.type=LightType.Point;light.color=new Color(.9f,.16f,.055f);light.intensity=.65f;light.range=6;light.transform.localPosition=new Vector3(Mathf.Cos(a)*(playerRadius+5),1.4f,Mathf.Sin(a)*(playerRadius+5));
            }
            var ambient=WorldArt.Group("Sparse drifting Veil embers",root);Ambient=ambient.gameObject.AddComponent<VeilAmbientEmbers>();Ambient.Initialize(playerRadius,Mathf.Abs(playerRadius-RuinedTownArena.VillagePlayerRadius)<.01f);
            Tick(0);
        }
        public void Tick(float dt)
        {
            clock+=dt;Shader.SetGlobalFloat("_VeilClock",clock);
            if(Ambient)Ambient.Sample(clock);
        }
    }
}
