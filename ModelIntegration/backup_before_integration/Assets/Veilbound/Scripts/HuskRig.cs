using UnityEngine;
namespace Veilbound {
 public sealed class HuskRig {
  public readonly Transform root;
  public readonly MonsterVisual visual;
  public HuskRig(Transform parent,int kind,int boss){visual=MonsterVisual.Create(parent,kind,boss);root=visual.transform;}
  public void Hit(){visual.Hit();}
  public void FuseGlow(float progress){visual.FuseGlow(progress);}
  public void Animate(float time,float moving,float dt){visual.Animate(time,moving,dt);}
 }
}
