using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  bool IsEtherealEnemy(Foe foe){
   var model=foe.rig!=null?foe.rig.visual.production:null;
   return model&&(model.assetId=="Shrine/Ash_Wraith"||model.assetId=="Shrine/Lamenter");
  }
  Vector3 MoveEnemyTerrain(Foe foe,Vector3 start,Vector3 delta,bool pursue=false){
   delta.y=0;
   if(IsEtherealEnemy(foe))return Vector3.ClampMagnitude(start+delta,collision.Radius-foe.radius-.005f);
   return pursue?navigation.Move(start,delta.magnitude,foe.radius):collision.Move(start,delta,foe.radius);
  }
 }
}
