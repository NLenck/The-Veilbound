using UnityEngine;

namespace Veilbound
{
    [RequireComponent(typeof(Camera))]
    public sealed class VeilFogEffect : MonoBehaviour
    {
        Camera view;Material material;
        void OnEnable(){view=GetComponent<Camera>();view.depthTextureMode|=DepthTextureMode.Depth;}
        void OnRenderImage(RenderTexture source,RenderTexture destination)
        {
            if(!material){var template=Resources.Load<Material>("VeilVolume");if(!template){Graphics.Blit(source,destination);return;}material=new Material(template);}
            material.SetVector("_ArenaEdges",new Vector4(ArenaCollision.PlayerBoundary+.6f,ArenaCollision.Boundary+.5f,0,0));
            Transform t=view.transform;material.SetVector("_Origin",t.position);material.SetVector("_Right",t.right);material.SetVector("_Up",t.up);material.SetVector("_Forward",t.forward);
            material.SetVector("_View",new Vector4(view.orthographicSize*2*view.aspect,view.orthographicSize*2,view.nearClipPlane,view.farClipPlane));
            RenderTexture fog=RenderTexture.GetTemporary(Mathf.Max(1,source.width/2),Mathf.Max(1,source.height/2),0,RenderTextureFormat.ARGBHalf);
            fog.filterMode=FilterMode.Bilinear;Graphics.Blit(source,fog,material,0);material.SetTexture("_FogTex",fog);Graphics.Blit(source,destination,material,1);RenderTexture.ReleaseTemporary(fog);
        }
        void OnDestroy(){if(material)Destroy(material);}
    }
}
