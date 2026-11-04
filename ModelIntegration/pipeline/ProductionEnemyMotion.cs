using UnityEngine;
namespace Veilbound {
 public sealed class ProductionEnemyMotion:MonoBehaviour {
  public string assetId,motion;public MonsterVisual visual;
  public void Animate(float time,float moving) {
   var b=visual.bones;var rest=visual.rest;
   for(int i=0;i<b.Length;i++){b[i].localRotation=Quaternion.identity;b[i].localPosition=rest[i]-(i==0?Vector3.zero:rest[MonsterVisual.Parents[i]]);}
   float phase=time*(visual.kind==2||visual.kind==5?3.8f:5.8f),height=visual.skin.localBounds.size.y;
   if(motion=="floating") {b[0].localPosition+=Vector3.up*(Mathf.Sin(time*1.8f)*.045f+ .10f);b[0].localRotation=Quaternion.Euler(Mathf.Sin(time)*2,assetId.EndsWith("Ritual_Orb")?time*18:Mathf.Sin(time*.7f)*3,Mathf.Sin(time*1.4f)*2);return;}
   float angle=motion=="quadruped"?9:12;
   b[0].localPosition+=Vector3.up*(Mathf.Abs(Mathf.Sin(phase))*.012f*moving);
   b[1].localRotation=Quaternion.Euler(Mathf.Sin(time*1.4f)*.8f,Mathf.Sin(phase)*1.5f*moving,0);
   b[4].localRotation=Quaternion.Euler(0,Mathf.Sin(time*.8f)*2,0);
   for(int side=0;side<2;side++) {
    float stride=Mathf.Sin(phase+side*Mathf.PI)*angle*moving;int leg=side==0?11:14,arm=side==0?5:8;
    b[leg].localRotation=Quaternion.Euler(stride,0,0);b[leg+1].localRotation=Quaternion.Euler(Mathf.Max(0,-stride)*.8f,0,0);b[leg+2].localRotation=Quaternion.Euler(-stride*.35f,0,0);
    b[arm].localRotation=Quaternion.Euler(-stride*(motion=="quadruped"?1:.30f),0,0);b[arm+1].localRotation=Quaternion.Euler(Mathf.Max(0,stride)*.25f,0,0);
   }
  }
  void Aim(int start,int child,Vector3 target){var b=visual.bones;b[start].rotation=Quaternion.FromToRotation(b[child].position-b[start].position,target-b[start].position)*b[start].rotation;}
  public void GroundSlamPose(float progress) {
   if(motion=="floating") {visual.bones[0].localPosition+=Vector3.up*Mathf.Sin(Mathf.Clamp01(progress)*Mathf.PI)*.35f;return;}
   float p=Mathf.Clamp01(progress),raise=p<.72f?Mathf.SmoothStep(0,1,p/.72f):1-Mathf.SmoothStep(0,1,(p-.72f)/.28f);float recovery=progress>1?Mathf.Clamp01(2-progress):1;
   for(int arm=5;arm<=8;arm+=3){var target=transform.TransformPoint(visual.rest[arm+2]+Vector3.up*(raise*.85f*recovery));Aim(arm,arm+1,Vector3.Lerp(visual.bones[arm+1].position,target,.65f));Aim(arm+1,arm+2,target);}
   visual.bones[1].localRotation*=Quaternion.Euler((18*(1-raise)-6*raise)*recovery,0,0);
  }
 }
 public static class ProductionEnemySelection {
  static readonly string[][] Town={new[]{"Town/Husk","Town/Shrieker"},new[]{"Town/Lobber","Town/Conduit"},new[]{"Town/Brute","Town/Bulwark"},new[]{"Town/Ghoul","Town/Impaler","Town/Ravager","Forest/Wolf"},new[]{"Town/Burster"}};
  static readonly string[][] Shrine={new[]{"Shrine/Acolyte","Shrine/Blade_Sentinel"},new[]{"Shrine/Cinder_Priest","Shrine/Lamenter"},new[]{"Shrine/Shrine_Guardian"},new[]{"Shrine/Gargoyle","Shrine/Ash_Wraith"},new[]{"Shrine/Ritual_Orb"}};
  static readonly string[] TownBoss={"Town/Brute","Town/Conduit","Town/Bulwark"},ShrineBoss={"Shrine/Shrine_Guardian","Shrine/Veil_Angel","Shrine/High_Priest"};
  public static string Pick(int map,int kind,int boss,int variant=0){if(boss>0)return (map==1?TownBoss:ShrineBoss)[Mathf.Clamp(boss-1,0,2)];var pool=(map==1?Town:Shrine)[Mathf.Clamp(kind,0,4)];return pool[(variant&int.MaxValue)%pool.Length];}
  public static MonsterVisual Create(Transform parent,int kind,int boss,int map,int variant=0) {
   var catalog=ProductionModelCatalog.Current;if(!catalog)return null;
   string id=Pick(map,kind,boss,variant);var entry=catalog.Enemy(id);if(entry==null||!entry.prefab)throw new System.InvalidOperationException("Missing production enemy "+id);
   var go=Object.Instantiate(entry.prefab,parent,false);var v=go.GetComponent<MonsterVisual>();v.kind=boss>0?5:kind;
   if(boss>0)go.transform.localScale*=1+(boss-1)*.07f;
   v.Animate(0,0,0);return v;
  }
 }
}
