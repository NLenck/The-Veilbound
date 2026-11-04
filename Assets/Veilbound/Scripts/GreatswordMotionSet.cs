using System;
using UnityEngine;
namespace Veilbound {
 public sealed class GreatswordMotionSet:ScriptableObject {
  public AnimationClip ready;
  // Forward, backward, left, right, then the same four running directions.
  public AnimationClip[] movement;
  public AnimationClip[] attacks;
  public AnimationClip blockEnter,blockHold,blockExit,blockImpact;
  public static GreatswordMotionSet Load(bool female)=>Resources.Load<GreatswordMotionSet>("GreatswordAnimations/"+(female?"Female":"Male"));
  public bool Complete=>ready&&movement!=null&&movement.Length==8&&Array.TrueForAll(movement,c=>c)&&attacks!=null&&attacks.Length==4&&Array.TrueForAll(attacks,c=>c);
 }
}
