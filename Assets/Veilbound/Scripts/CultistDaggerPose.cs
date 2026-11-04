using UnityEngine;
namespace Veilbound {
 public sealed class CultistDaggerPose:MonoBehaviour {
  public Transform hand,elbow,blade;public Vector3 gripOffset;
  public void Sync(){if(!hand||!blade||!elbow)return;blade.position=hand.TransformPoint(gripOffset);Vector3 direction=hand.position-elbow.position;if(direction.sqrMagnitude<.0001f)direction=transform.up;Vector3 up=Vector3.ProjectOnPlane(transform.forward,direction.normalized);if(up.sqrMagnitude<.001f)up=transform.up;blade.rotation=Quaternion.LookRotation(direction,up);}
  void LateUpdate(){Sync();}
 }
}
