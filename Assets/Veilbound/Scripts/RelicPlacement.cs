using System;using System.Collections.Generic;using UnityEngine;
namespace Veilbound {
 // A shuffled sweep of eight regions spreads objectives across the whole playable disk.
 public sealed class RelicPlacement {
  readonly int[] sectors={0,1,2,3,4,5,6,7};int cursor=8;
  public void Reset(){cursor=8;}
  public bool TryFind(System.Random random,ArenaCollision collision,ArenaNavigation navigation,Vector3 player,List<Vector3> occupied,out Vector3 point){
   if(cursor==8){for(int i=0;i<8;i++)sectors[i]=i;for(int i=7;i>0;i--){int j=random.Next(i+1),swap=sectors[i];sectors[i]=sectors[j];sectors[j]=swap;}cursor=0;}
   float radius=collision.PlayerRadius-3;
   for(int region=0;region<8;region++)for(int attempt=0;attempt<48;attempt++){
    int sector=sectors[(cursor+region)%8];float angle=(sector+(float)random.NextDouble())*Mathf.PI/4;
    float distance=radius*Mathf.Sqrt((float)random.NextDouble());
    Vector3 p=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle))*distance;
    if(Vector3.Distance(p,player)<12||occupied.Exists(q=>Vector3.Distance(q,p)<10)||!collision.Free(p,.65f)||!navigation.Reachable(p,.65f))continue;
    point=p;cursor++;return true;
   }
   point=Vector3.zero;return false;
  }
 }
}
