using System.IO;
using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  void CheckAreaWarningTiming(string folder) {
   ReturnHome();StartRun();invulnerability=0;position=Vector3.zero;
   float initial=health;Telegraph(Vector3.zero,3,1,9);var hazard=hazards[hazards.Count-1];
   var outer=hazard.t.Find("Maximum damage radius").GetComponent<LineRenderer>();
   var inner=hazard.t.Find("Time to impact").GetComponent<LineRenderer>();
   Require(Mathf.Abs(outer.GetPosition(0).x-3)<.001f&&inner.GetPosition(0).x==0,"Warning starts with a fixed outer boundary and an empty timer");
   float[] steps={.25f,.25f,.45f};float elapsedWarning=0;
   for(int i=0;i<steps.Length;i++) {
    elapsedWarning+=steps[i];UpdateHazards(steps[i]);
    Require(health==initial&&hazards.Contains(hazard),"Area attack waits for the full warning duration before damage");
    Require(Mathf.Abs(inner.GetPosition(0).x-3*elapsedWarning)<.001f&&Mathf.Abs(outer.GetPosition(0).x-3)<.001f,"Inner ring grows linearly while the full damage radius stays fixed");
    foreach(var t in hazard.t.GetComponentsInChildren<Transform>())t.gameObject.layer=30;
    FamilyFrame(Path.Combine(folder,"area-warning-"+(i+1)+".png"),new Vector3(0,7,-4),Vector3.zero,3.6f,1400,1000);
    foreach(var t in hazard.t.GetComponentsInChildren<Transform>())t.gameObject.layer=0;
   }
   UpdateHazards(.06f);Require(health<initial&&!hazards.Contains(hazard),"Area attack deals damage when the growing ring reaches the boundary");
   ReturnHome();StartRun();invulnerability=0;position=Vector3.zero;initial=health;
   LobArea(new Vector3(0,1,7),Vector3.zero,14);var lob=lobs[lobs.Count-1];UpdateLobs(.7f);
   Require(health==initial&&Mathf.Abs(lob.area.Progress-.5f)<.001f,"Lob warning reaches half radius halfway through its flight without early damage");
   UpdateLobs(.69f);Require(health==initial,"Lob does not damage before impact");UpdateLobs(.02f);Require(health<initial&&lobs.Count==0,"Lob damage waits until its growing timer reaches the outer ring");
   ReturnHome();StartRun();position=Vector3.forward*10;var bomb=SpawnFoe(0,4);bomb.t.position=Vector3.zero;StartFuse(bomb,false);UpdateFoes(1.5f);
   Require(Mathf.Abs(bomb.fuseWarning.Progress-.5f)<.001f&&Mathf.Abs(bomb.fuseWarning.Radius-3.4f)<.001f,"Volatile fuse has the same growing timer and a fixed blast boundary");
   ReturnHome();Debug.Log("VEILBOUND_AREA_WARNING_TIMING_PASS");
  }
 }
}
