using UnityEngine;
namespace Veilbound {
 [System.Serializable] public sealed class EnemyMotionCycle {
  public string source;public float duration,strideDistance;public int frameCount;
  public Quaternion[] rotations;public Vector3[] hips;
  public float NaturalSpeed=>strideDistance/Mathf.Max(.01f,duration);
 }
 public sealed class EnemyMotionSamples:ScriptableObject {
  public string assetId;public EnemyMotionCycle movement,fastMovement;
  public bool HasFastMovement=>fastMovement!=null&&fastMovement.frameCount>1;
 }
}
