using UnityEngine;
namespace Veilbound {
 public sealed class TownInterior:MonoBehaviour {
  public float width,depth;public Renderer[] walls;float heightScale=1;MaterialPropertyBlock block;
  public bool Contains(Vector3 world,float margin=0){Vector3 p=transform.InverseTransformPoint(world);return Mathf.Abs(p.x)<width*.5f+margin&&Mathf.Abs(p.z)<depth*.5f+margin;}
  public void Tick(Vector3 player,float dt){float desired=Contains(player,1.4f)?.38f:1;heightScale=Mathf.Lerp(heightScale,desired,1-Mathf.Exp(-Mathf.Max(0,dt)*9));if(block==null)block=new MaterialPropertyBlock();block.SetFloat("_WallHeightScale",heightScale);foreach(var wall in walls)if(wall)wall.SetPropertyBlock(block);}
 }
}
