using System;
using UnityEngine;
namespace Veilbound {
 [Serializable] public sealed class ProductionPlayerEntry {
  public string gender;public GameObject prefab;public AnimationClip idle,walk,run;
 }
 [Serializable] public sealed class ProductionEnemyEntry {
  public string id,biome,displayName,motion;public int kind;public GameObject prefab;public int triangles;
 }
 public sealed class ProductionModelCatalog:ScriptableObject {
  public ProductionPlayerEntry[] players;public ProductionEnemyEntry[] enemies;public GameObject[] tierPrefabs;
  static ProductionModelCatalog cached;
  public static ProductionModelCatalog Current=>cached?cached:(cached=Resources.Load<ProductionModelCatalog>("ProductionModels/Catalog"));
  public ProductionPlayerEntry Player(bool female)=>Array.Find(players,p=>p.gender==(female?"Female":"Male"));
  public ProductionEnemyEntry Enemy(string id)=>Array.Find(enemies,e=>e.id==id);
 }
 public partial class Settlement {public bool female;}
}
