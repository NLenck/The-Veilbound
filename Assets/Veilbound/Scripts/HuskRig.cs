using UnityEngine;
namespace Veilbound {
 public sealed class HuskRig {
  public readonly Transform root;
  public readonly MonsterVisual visual;
  public HuskRig(Transform parent,int kind,int boss,int map=0,int variant=0){visual=MonsterVisual.Create(parent,kind,boss,map,variant);root=visual.transform;}
  public void SetMotion(Vector3 velocity){if(visual.production)visual.production.SetMotion(velocity);}
  public void Hit(){visual.Hit();}
  public void FuseGlow(float progress){visual.FuseGlow(progress);}
  public void Animate(float time,float moving,float dt){visual.Animate(time,moving,dt);}
 }
}
