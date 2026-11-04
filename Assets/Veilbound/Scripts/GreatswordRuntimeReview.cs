using System;using System.IO;using System.Collections;using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  IEnumerator GreatswordRuntimeReview(){
   yield return null;combatReviewFreeze=true;
   string folder=Path.Combine(Application.dataPath,"..","Smoke","GreatswordAnimations");Directory.CreateDirectory(folder);int checks=0;
   foreach(bool female in new[]{false,true}){
    city=FreshCharacterTown();city.female=female;RebuildHero();StartRun();combatReviewFreeze=true;position=Vector3.zero;hero.root.position=position;
    cameraRig.transform.position=new Vector3(7,15,-16);cameraRig.transform.LookAt(Vector3.up*.5f);cameraRig.orthographicSize=10;
    AimAtPointer(cameraRig.WorldToScreenPoint(position+Vector3.forward*4));Require(Vector3.Dot(facing,Vector3.forward)>.999f,"mouse aim remains forward");hero.root.rotation=Quaternion.LookRotation(facing);checks++;
    var v=hero.root.GetComponent<ProductionHeroVisual>();Require(v&&v.greatsword!=null,"built player loads greatsword resources");
    string[] labels={"forward","backward","left","right"};Vector3[] dirs={Vector3.forward,Vector3.back,Vector3.left,Vector3.right};float t=0;
    for(int i=0;i<4;i++){
     hero.SetMotion(dirs[i]*3.6f);for(int f=0;f<30;f++){t+=1/30f;hero.Animate(t,1,0);}
     Require(v.greatsword.Weight(5+i)>.99f,"actual hero selects "+labels[i]+" while aiming forward");Require(Vector3.Dot(hero.root.forward,facing)>.999f,"movement does not turn aiming");checks++;
     yield return null;yield return null;
     GreatswordCapture(Path.Combine(folder,(female?"Female":"Male")+"_"+labels[i]+".png"));
    }
    int before=v.greatsword.AttackSerial;float damage=AbilityDamage("cleave"),cooldown=AbilityCooldown("cleave");
    ExecuteBasicAttack();Require(v.greatsword.AttackSerial==before+1,"real gameplay attack triggers authored animation");Require(AbilityDamage("cleave")==damage&&AbilityCooldown("cleave")==cooldown,"visual animation preserves damage and cooldown");checks+=2;
    for(int f=0;f<18;f++){t+=1/60f;hero.Animate(t,1,0);}
    Require(v.greatsword.AttackWeight>.1f&&v.greatsword.Weight(8)>.99f,"actual attack and side-running play together");Require(v.lastGripError<.035f,"equipped game sword stays in hands");checks+=2;
    yield return null;yield return null;
    GreatswordCapture(Path.Combine(folder,(female?"Female":"Male")+"_attack.png"));
    yield return null;
   }
   CheckRageAbilityKit();CheckRageVirtue();checks+=2;
   File.WriteAllText(Path.Combine(folder,"validation.json"),"{\"passed\":true,\"checks\":"+checks+",\"variants\":2,\"rageRegressions\":true}");Debug.Log("VEILBOUND_GREATSWORD_RUNTIME_PASS "+checks);Application.Quit(0);
  }
  void GreatswordCapture(string path){
   var cameraObject=new GameObject("Greatsword QA camera");var cam=cameraObject.AddComponent<Camera>();cam.CopyFrom(cameraRig);cam.enabled=false;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.045f,.055f,.07f);cam.cullingMask=1<<30;cam.orthographicSize=1.9f;
   foreach(var tr in hero.root.GetComponentsInChildren<Transform>(true))tr.gameObject.layer=30;
   cam.transform.position=hero.root.position+new Vector3(3,2.8f,4.5f);cam.transform.LookAt(hero.root.position+Vector3.up*.95f+hero.root.forward*.25f);
   var rt=new RenderTexture(1000,800,24);cam.targetTexture=rt;var previous=RenderTexture.active;cam.Render();RenderTexture.active=rt;var tex=new Texture2D(1000,800,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1000,800),0,0);tex.Apply();File.WriteAllBytes(path,tex.EncodeToPNG());RenderTexture.active=previous;cam.targetTexture=null;rt.Release();Destroy(rt);Destroy(tex);Destroy(cameraObject);
  }
 }
}
