using UnityEngine;
using UnityEngine.Rendering;
namespace Veilbound {
 public sealed class EnemyArrivalFog:MonoBehaviour {
  static Mesh mesh;static Material material;MeshRenderer draw;float age;MaterialPropertyBlock block;
  public static EnemyArrivalFog Create(Transform parent,Vector3 position,bool boss){
   if(!material)material=new Material(Resources.Load<Shader>("EnemyArrivalFog")){name="Charcoal and crimson enemy arrival"};
   if(!mesh){var v=new Vector3[12];var uv=new Vector2[12];var triangles=new int[18];for(int i=0;i<3;i++){Vector3 across=Quaternion.Euler(0,i*60,0)*Vector3.right*1.6f;int n=i*4,t=i*6;v[n]=-across;v[n+1]=across;v[n+2]=-across+Vector3.up*3;v[n+3]=across+Vector3.up*3;uv[n]=Vector2.zero;uv[n+1]=Vector2.right;uv[n+2]=Vector2.up;uv[n+3]=Vector2.one;triangles[t]=n;triangles[t+1]=n+2;triangles[t+2]=n+1;triangles[t+3]=n+1;triangles[t+4]=n+2;triangles[t+5]=n+3;}mesh=new Mesh{name="Three crossed enemy arrival wisps",vertices=v,uv=uv,triangles=triangles};mesh.RecalculateBounds();}
   var obj=new GameObject("Enemy emerging from local Veil mist");obj.transform.SetParent(parent,false);obj.transform.position=position;obj.transform.localScale=Vector3.one*(boss?1.4f:1);obj.AddComponent<MeshFilter>().sharedMesh=mesh;var fog=obj.AddComponent<EnemyArrivalFog>();fog.draw=obj.AddComponent<MeshRenderer>();fog.block=new MaterialPropertyBlock();fog.draw.sharedMaterial=material;fog.draw.shadowCastingMode=ShadowCastingMode.Off;fog.draw.receiveShadows=false;fog.Advance(0);return fog;
  }
  public bool Advance(float dt){age+=Mathf.Max(0,dt);if(age>=1.8f){Destroy(gameObject);return false;}block.SetFloat("_Progress",age/1.8f);draw.SetPropertyBlock(block);return true;}
 }
}

