using UnityEngine;
namespace Veilbound {
 public sealed class GraveyardBrazierFlame:MonoBehaviour {
  public Transform flame;public Light lamp;public float phase;public float brightness=1.45f;
  public void Sample(float time){float pulse=Mathf.Sin(time*4.1f+phase)*.06f+Mathf.Sin(time*7.3f+phase*.7f)*.035f;if(flame)flame.localScale=new Vector3(1+pulse*.45f,1+pulse,1-pulse*.3f);if(lamp)lamp.intensity=brightness*(.96f+pulse*.4f);}
  void Update(){Sample(Time.time);}
 }
}
