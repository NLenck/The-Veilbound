using System;
using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  void PositionCharacterSelectCamera(float dt){
   if(hero==null)return;
   cameraRig.orthographicSize=Mathf.Lerp(cameraRig.orthographicSize,2.05f,1-Mathf.Exp(-dt*6));
   Vector3 center=hero.root.position+Vector3.up*.95f;
   cameraRig.transform.position=Vector3.Lerp(cameraRig.transform.position,hero.root.position+new Vector3(-.08f,2.4f,-7),1-Mathf.Exp(-dt*8));
   cameraRig.transform.LookAt(center);
  }
  void CharacterAction(Rect rect,string title,Action action,bool enabled=true,int size=18){
   enabled=enabled&&(!deleteCharacterOpen||drawingDeleteCharacterDialog);
   if(enabled)ExpeditionActionButton(rect,title,action,size);
   else {Button(rect.x,rect.y,rect.width,rect.height,"",()=>{},false);ExpeditionText(rect.x+8,rect.y+4,rect.width-16,rect.height-8,title,size,muted,TextAnchor.MiddleCenter,true);}
  }
  int CharacterBestDepth(CharacterSummary row)=>row.progressVersion==1?row.bestDepth:Math.Max(0,row.highestTier-1);
  void DrawCharacterRosterScreen(){
   InitExpeditionArt();Box(new Rect(0,0,1600,900),new Color(.012f,.02f,.023f,.18f));
   Box(new Rect(0,0,1600,148),new Color(.012f,.02f,.023f,.88f));
   ExpeditionText(40,20,750,28,"V E I L B O U N D",18,hudLight,bold:true);
   ExpeditionText(40,58,1100,48,creatingCharacter?"CREATE YOUR VEILBORNE":"CHOOSE YOUR VEILBORNE",32,parchment,bold:true);
   ExpeditionText(42,113,1060,24,"Your character. Your settlement. Your journey.",15,muted);
   CharacterAction(new Rect(1220,34,166,42),"SETTINGS",OpenSettings);CharacterAction(new Rect(1400,34,172,42),"EXIT",ExitGame);
   if(!string.IsNullOrEmpty(profileError)){ExpeditionFrame(new Rect(28,164,1544,610));ExpeditionText(55,200,1480,500,profileError,19,parchment);return;}
   if(creatingCharacter){CreateCharacterScreen();return;}
   ExpeditionFrame(new Rect(28,164,420,610));ExpeditionText(48,180,380,32,"YOUR CHARACTERS",20,hudLight,bold:true);Rule(48,220,380);
   int count=roster?.characters.Count??0,pages=Mathf.Max(1,Mathf.CeilToInt(count/(float)CharactersPerPage));characterPage=Mathf.Clamp(characterPage,0,pages-1);
   if(count==0)ExpeditionText(62,300,350,230,"Your story starts here.\n\nCreate your first Veilborne to begin a new settlement.",22,parchment,TextAnchor.MiddleCenter);
   for(int i=0;i<CharactersPerPage;i++){
    int index=characterPage*CharactersPerPage+i;if(index>=count)break;var row=roster.characters[index];float y=232+i*57;bool selected=row.id==roster.activeId;
    Button(48,y,380,51,"",()=>SelectSavedCharacter(row.id),!deleteCharacterOpen);
    if(selected){Box(new Rect(50,y+2,376,47),new Color(.14f,.105f,.055f));ExpeditionBevel(new Rect(48,y,380,51),hudLight,5);}
    var icon=UpgradeIcon(row.virtue==Virtue.Rage?"rage":"fortune");if(icon)GUI.DrawTexture(new Rect(58,y+9,32,32),icon);
    ExpeditionText(103,y+5,312,24,row.name,18,selected?parchment:muted,bold:true);
    int best=CharacterBestDepth(row);ExpeditionText(103,y+29,312,19,row.virtue+" · "+(best>0?"Best Depth "+best:"New journey"),12,muted);
   }
   Button(48,708,85,32,"Previous",()=>characterPage--,characterPage>0&&!deleteCharacterOpen);
   ExpeditionText(139,713,195,24,count+" characters · "+(characterPage+1)+" / "+pages,12,muted,TextAnchor.MiddleCenter);
   Button(341,708,87,32,"Next",()=>characterPage++,characterPage+1<pages&&!deleteCharacterOpen);
   CharacterAction(new Rect(28,790,420,48),"+ CREATE CHARACTER",BeginCharacterCreation);
   CharacterAction(new Rect(68,850,340,34),"DELETE CHARACTER",OpenDeleteCharacter,activeCharacter!=null,15);
   ExpeditionFrame(new Rect(1150,164,422,440));
   ExpeditionText(1174,188,374,56,activeCharacter?.name??"A NEW BEGINNING",28,parchment,bold:true);
   var virtue=UpgradeIcon("rage");if(activeCharacter!=null&&virtue)GUI.DrawTexture(new Rect(1174,253,31,31),virtue);
   if(activeCharacter!=null)ExpeditionText(1216,253,330,31,activeCharacter.virtue.ToString().ToUpper(),18,hudLight,bold:true);
   else ExpeditionText(1174,253,374,31,"NO CHARACTER SELECTED",16,hudLight,bold:true);Rule(1174,298,374);
   if(activeCharacter!=null){
    city.EnsureExpeditionModifiers();CharacterStat(320,"Best Veil Depth",city.expeditionBestDepth.ToString());CharacterStat(363,"Victories",city.victories.ToString());CharacterStat(406,"Settlement Day",((int)city.hours/24+1).ToString());
    Rule(1174,452,374);ExpeditionText(1174,470,374,25,"APPEARANCE",15,hudLight,bold:true);
    Button(1174,511,180,39,(city.female?"":"• ")+"Male",()=>SetCharacterAppearance(false),!deleteCharacterOpen);Button(1368,511,180,39,(city.female?"• ":"")+"Female",()=>SetCharacterAppearance(true),!deleteCharacterOpen);
   }else ExpeditionText(1174,332,374,180,"Create a character to begin your journey.",19,muted,TextAnchor.MiddleCenter);
   ExpeditionFrame(new Rect(1150,622,422,110));ExpeditionText(1174,640,374,27,"LOST KNOWLEDGES",20,hudLight,bold:true);
   ExpeditionText(1174,679,374,35,(lostKnowledges?.discoveries.Count??0)+" discoveries · Shared across characters",13,muted);
   CharacterAction(new Rect(1150,790,422,58),"ENTER SETTLEMENT",EnterSelectedCharacter,CanEnterCharacterRun(),20);
   if(activeCharacter!=null)ExpeditionText(485,745,640,33,activeCharacter.name,22,parchment,TextAnchor.MiddleCenter,true);
   ExpeditionText(485,847,640,45,"Each character has its own settlement, equipment\nand expedition progress.",13,parchment,TextAnchor.MiddleCenter);
   if(!string.IsNullOrEmpty(profileNotice))ExpeditionText(485,797,640,45,profileNotice,13,hudLight,TextAnchor.MiddleCenter);
   if(deleteCharacterOpen){buttons.Clear();DrawDeleteCharacterDialog();}
  }
  void CharacterStat(float y,string title,string value){ExpeditionText(1174,y,285,28,title,17,parchment);ExpeditionText(1465,y,83,28,value,20,hudLight,TextAnchor.UpperRight,true);}
 }
}
