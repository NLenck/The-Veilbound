using System;
using UnityEngine;
namespace Veilbound.Greatswords {
 [Serializable] public sealed class GreatswordEntry {public string tier;public int metalIndex;public GameObject prefab;public int triangles;}
 public sealed class GreatswordVisualLibrary:ScriptableObject {
  public GreatswordEntry[] swords;
  // Optional visual factory. Does not change combat stats, reach, inputs or the live weapon factory.
  public Transform CreateVisual(Transform socket,int metalIndex) {
   var entry=Array.Find(swords,s=>s.metalIndex==metalIndex);if(entry==null||!entry.prefab)return null;
   var instance=UnityEngine.Object.Instantiate(entry.prefab,socket,false);instance.name="Greatsword - "+entry.tier;return instance.transform;
  }
 }
}
