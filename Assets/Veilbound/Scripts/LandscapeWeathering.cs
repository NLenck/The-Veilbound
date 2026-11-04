using System;using UnityEngine;
using M=Veilbound.RuinedTownArena.Meshes;
namespace Veilbound {
 public static class LandscapeWeathering {
  static float R(System.Random r,float a,float b)=>Mathf.Lerp(a,b,(float)r.NextDouble());
  static Vector3 Rad(float a,float r)=>new Vector3(Mathf.Cos(a)*r,0,Mathf.Sin(a)*r);
  static void Line(M m,Vector3 a,Vector3 b,float width){Vector3 d=b-a,n=new Vector3(-d.z,0,d.x).normalized*width;m.Face(a-n,a+n,b+n*.4f,b-n*.4f);}
  static void Crack(M m,Vector3 p,float length,float angle,bool wood,System.Random r){
   var q=Quaternion.Euler(0,angle,0);Vector3 a=p+q*new Vector3(0,0,-length*.5f),b=p+q*new Vector3(R(r,-.09f,.09f),0,-length*.12f),c=p+q*new Vector3(R(r,-.08f,.08f),0,length*.2f),d=p+q*new Vector3(.025f,0,length*.5f);
   float w=wood?.015f:.022f;Line(m,a,b,w);Line(m,b,c,w*.85f);Line(m,c,d,w*.65f);
   if(!wood)Line(m,c,c+q*new Vector3(length*.26f,0,length*.2f),w*.65f);
  }
  static void Rock(M m,Vector3 p,float w,float h,System.Random r){
   var q=Quaternion.Euler(0,R(r,0,360),0);Vector3 top=p+new Vector3(w*.12f,h,0);
   for(int i=0;i<5;i++){float a=i*Mathf.PI*2/5,b=(i+1)*Mathf.PI*2/5;Vector3 x=p+q*Rad(a,w),y=p+q*Rad(b,w);m.Face(x,top,y);}
  }
  static void Weed(M m,Vector3 p,System.Random r){
   // Small angular leaves rooted in the joints, below character silhouettes.
   for(int i=0;i<4;i++){float a=R(r,0,Mathf.PI*2),h=R(r,.13f,.28f);Vector3 side=Rad(a,.025f),bend=Rad(a+.4f,h*.5f),tip=p+Vector3.up*h+bend;
    m.Face(p-side,tip,p+side);m.Face(p+side,tip,p-side);
   }
  }
  public static void Build(Transform root,ArenaCollision collision,bool village){
   var rng=new System.Random(village?1186:2286);var rooms=village?RuinedTownArena.Rooms:GraveyardArena.Rooms;
   var straw=new M();var faded=new M();var soil=new M();var grass=new M();var rubble=new M();var cracks=new M();var splits=new M();var weeds=new M();
   bool Earth(Vector3 p){
    if(p.magnitude>collision.PlayerRadius+4)return false;
    if(village?(Mathf.Abs(p.x)<6.6f||Mathf.Abs(p.z)<2.8f):(p.magnitude<9.8f||Mathf.Abs(p.x)<3.35f||Mathf.Abs(p.z)<3.35f))return false;
    foreach(var room in rooms){Vector3 a=p-room.transform.localPosition;if(Mathf.Abs(a.x)<room.width*.5f+.2f&&Mathf.Abs(a.z)<room.depth*.5f+.2f)return false;}
    return true;
   }
   // An irregular, connected field of dead grass rather than isolated green islands.
   float limit=collision.PlayerRadius+4;
   Vector3 GroundPoint(float x,float z)=>new Vector3(x+Mathf.Sin(x*1.7f+z*.8f)*.4f,-.086f,z+Mathf.Sin(z*1.3f-x*.6f)*.4f);
   for(float x=-limit;x<limit;x+=2.3f)for(float z=-limit;z<limit;z+=2.3f){
    Vector3 p=new Vector3(x+R(rng,-.45f,.45f),-.086f,z+R(rng,-.45f,.45f));if(!Earth(p))continue;
    M batch=rng.Next(5)==0?soil:rng.Next(2)==0?straw:faded;
    Vector3 a=GroundPoint(x,z),b=GroundPoint(x,z+2.3f),c=GroundPoint(x+2.3f,z+2.3f),d=GroundPoint(x+2.3f,z);
    if(Earth(a)&&Earth(b)&&Earth(c))batch.Face(a,b,c);if(Earth(a)&&Earth(c)&&Earth(d))batch.Face(a,c,d);
    for(int t=0;t<2;t++){Vector3 at=p+Rad(R(rng,0,6.28f),R(rng,0,.9f));if(!Earth(at))continue;
     for(int blade=0;blade<3;blade++){float angle=R(rng,0,6.28f),h=R(rng,.16f,.32f);Vector3 side=Rad(angle,.035f),tip=at+Vector3.up*h+Rad(angle+.4f,h*.6f);grass.Face(at-side,tip,at+side);grass.Face(at+side,tip,at-side);}
    }
   }
   foreach(var room in rooms){
    Vector3 centre=room.transform.localPosition;float hx=room.width*.5f,hz=room.depth*.5f;
    foreach(int side in new[]{-1,1}){
     for(float along=-hx+.5f;along<hx;along+=.62f){if(Mathf.Abs(along)<3.5f||rng.Next(5)==0)continue;for(int k=0;k<2;k++){Vector3 p=centre+new Vector3(along+R(rng,-.22f,.22f),-.045f,side*(hz+R(rng,.4f,1.0f)));Rock(rubble,p,R(rng,.16f,.38f),R(rng,.13f,.34f),rng);}}
     for(float along=-hz+.5f;along<hz;along+=.65f){float door=village?0:-4;if(Mathf.Abs(along-door)<3.7f||rng.Next(5)==0)continue;for(int k=0;k<2;k++){Vector3 p=centre+new Vector3(side*(hx+R(rng,.4f,1)), -.045f,along+R(rng,-.2f,.2f));Rock(rubble,p,R(rng,.16f,.36f),R(rng,.14f,.32f),rng);}}
    }
    bool wood=(village&&!room.name.Contains("Church"))||room.name.Contains("keeper");
    for(float x=-hx+.6f;x<hx-.4f;x+=.95f)for(float z=-hz+.55f;z<hz-.4f;z+=1.0f){if(rng.Next(4)!=0)continue;Vector3 p=centre+new Vector3(x+R(rng,-.13f,.13f),wood?.0265f:village?.0265f:.034f,z);Crack(wood?splits:cracks,p,R(rng,.35f,.78f),wood?R(rng,-7,7):R(rng,-75,75),wood,rng);}
   }
   if(village){
    for(float x=-58.75f;x<60;x+=1.25f)for(float z=-58.4f;z<60;z+=1.1f){if((Mathf.Abs(x)>5.9f&&Mathf.Abs(z)>2.1f)||x*x+z*z>3600)continue;if(rng.Next(5)==0)Crack(cracks,new Vector3(x,.0515f,z),R(rng,.45f,.85f),R(rng,-90,90),false,rng);}
   }else{
    for(int row=-22;row<=22;row++)for(int col=-2;col<=2;col++){float a=row*2.1f,b=col*1.18f;if(Mathf.Abs(a)<10)continue;if(rng.Next(5)==0)Crack(cracks,new Vector3(b,.032f,a),R(rng,.7f,1.4f),R(rng,-15,15),false,rng);if(rng.Next(5)==0)Crack(cracks,new Vector3(a,.032f,b),R(rng,.7f,1.4f),R(rng,75,105),false,rng);}
    for(int i=0;i<30;i++){Vector3 p=Rad(R(rng,0,6.28f),R(rng,1,8.9f))+Vector3.up*.034f;Crack(cracks,p,R(rng,.4f,1.1f),R(rng,0,360),false,rng);}
   }
   // Independent seed keeps the sparse joint vegetation stable when cracks are adjusted.
   var weedRng=new System.Random(village?716:817);
   if(village){
    for(float x=-58.75f;x<60;x+=1.25f)for(float z=-58.4f;z<60;z+=1.1f){
     if((Mathf.Abs(x)>5.9f&&Mathf.Abs(z)>2.1f)||x*x+z*z>3600||weedRng.Next(22)!=0)continue;
     Vector3 p=weedRng.Next(2)==0?new Vector3(x+.625f,.045f,z+R(weedRng,-.3f,.3f)):new Vector3(x+R(weedRng,-.35f,.35f),.045f,z+.55f);if(p.magnitude>3&&collision.Free(p,.18f))Weed(weeds,p,weedRng);
    }
   }else{
    for(int row=-22;row<=22;row++)for(int col=-2;col<=2;col++){float a=row*2.1f,b=col*1.18f;if(Mathf.Abs(a)<10)continue;
     foreach(var p in new[]{new Vector3(b+.59f,.028f,a+R(weedRng,-.6f,.6f)),new Vector3(a+R(weedRng,-.6f,.6f),.028f,b+.59f)})if(weedRng.Next(12)==0&&collision.Free(p,.18f))Weed(weeds,p,weedRng);
    }
    for(int i=0;i<20;i++){float a=i*Mathf.PI/10;Vector3 p=Rad(a,R(weedRng,3.5f,9.1f))+Vector3.up*.03f;if(weedRng.Next(3)!=0&&collision.Free(p,.2f))Weed(weeds,p,weedRng);}
   }
   straw.Finish(root,"Dying grass - ochre earth cover",new Color(.235f,.213f,.137f));faded.Finish(root,"Dying grass - faded olive cover",new Color(.205f,.207f,.135f));soil.Finish(root,"Dying grass - bare soil breaks",new Color(.16f,.145f,.11f));grass.Finish(root,"Dry bent grass tufts",new Color(.34f,.29f,.16f));
   rubble.Finish(root,"Collapsed masonry along building edges",new Color(.29f,.285f,.25f));cracks.Finish(root,"Jagged fractures in old paving",new Color(.105f,.112f,.095f));splits.Finish(root,"Splits and worn grain in timber floors",new Color(.105f,.065f,.033f));
   weeds.Finish(root,"Sparse weeds between paving joints",new Color(.24f,.265f,.13f));
  }
 }
}
