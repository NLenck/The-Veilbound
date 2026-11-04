using System;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Veilbound {
 public partial class VeilGame {
  bool deleteCharacterOpen,drawingDeleteCharacterDialog,deleteCharacterNameFocused,focusDeleteCancel;
  string deleteCharacterId="",deleteCharacterName="",deleteCharacterTyped="",deleteCharacterNotice="";
  bool CanConfirmCharacterDeletion=>deleteCharacterOpen&&activeCharacter!=null&&activeCharacter.id==deleteCharacterId&&string.Equals(deleteCharacterTyped.Trim(),deleteCharacterName,StringComparison.OrdinalIgnoreCase);
  void OpenDeleteCharacter(){if(activeCharacter==null||!string.IsNullOrEmpty(profileError))return;deleteCharacterId=activeCharacter.id;deleteCharacterName=activeCharacter.name;deleteCharacterTyped=deleteCharacterNotice="";deleteCharacterNameFocused=false;deleteCharacterOpen=true;focusDeleteCancel=true;selection=0;}
  void CancelDeleteCharacter(){deleteCharacterOpen=false;deleteCharacterId=deleteCharacterName=deleteCharacterTyped=deleteCharacterNotice="";deleteCharacterNameFocused=false;selection=0;}
  void ConfirmDeleteCharacter(){
   if(!CanConfirmCharacterDeletion||!SaveActiveCharacter())return;
   try{
    roster=characterStore.Delete(deleteCharacterId);activeCharacter=null;city=FreshCharacterTown();
    if(roster.characters.Count>0){characterStore.Load(roster.activeId,out activeCharacter,out city);characterPage=roster.characters.FindIndex(c=>c.id==roster.activeId)/CharactersPerPage;}else characterPage=0;
    CancelDeleteCharacter();ResetForCharacter();profileNotice="Character deleted. Lost Knowledges are retained.";
   }catch(Exception e){
    // Reload authoritative state before any autosave can reuse the removed slot.
    activeCharacter=null;city=FreshCharacterTown();
    try{roster=characterStore.LoadRoster();if(roster.characters.Count>0)characterStore.Load(roster.activeId,out activeCharacter,out city);CancelDeleteCharacter();ResetForCharacter();profileNotice="Deletion recovery completed. "+e.Message;}
    catch(Exception recovery){profileError="Character changes could not be recovered: "+recovery.Message;CancelDeleteCharacter();}
   }
  }
  void ReadDeleteCharacterInput(){
   if(Keyboard.current?.escapeKey.wasPressedThisFrame==true||PreferredPad?.buttonEast.wasPressedThisFrame==true){CancelDeleteCharacter();return;}
   if(deleteCharacterNameFocused){if(Keyboard.current?.enterKey.wasPressedThisFrame==true&&CanConfirmCharacterDeletion)ConfirmDeleteCharacter();return;}
   MenuInput();
  }
  void DrawDeleteCharacterDialog(){
   drawingDeleteCharacterDialog=true;
   if(focusDeleteCancel){GUI.FocusControl(null);focusDeleteCancel=false;}
   Box(new Rect(0,0,1600,900),new Color(0,0,0,.72f));ExpeditionFrame(new Rect(430,180,740,550));
   ExpeditionText(465,207,670,52,"DELETE "+deleteCharacterName.ToUpper()+"?",27,parchment,TextAnchor.MiddleCenter,true);
   ExpeditionText(485,286,630,40,deleteCharacterName,24,hudLight,TextAnchor.MiddleCenter,true);
   ExpeditionText(485,344,630,77,"This permanently deletes this character and their settlement, equipment, resources and expedition progress.",18,parchment,TextAnchor.MiddleCenter);
   Rule(475,438,650);ExpeditionText(485,456,630,50,"Lost Knowledges and your other characters are kept.",17,hudLight,TextAnchor.MiddleCenter);
   ExpeditionText(485,521,630,30,"Type "+deleteCharacterName+" to confirm",17,parchment,TextAnchor.MiddleCenter);
   GUI.SetNextControlName("DeleteCharacterName");deleteCharacterTyped=GUI.TextField(new Rect(485,565,630,44),deleteCharacterTyped,32,new GUIStyle(GUI.skin.textField){fontSize=22,padding=new RectOffset(12,12,9,9)});deleteCharacterNameFocused=GUI.GetNameOfFocusedControl()=="DeleteCharacterName";
   CharacterAction(new Rect(475,650,290,49),"CANCEL",CancelDeleteCharacter);
   CharacterAction(new Rect(785,650,340,49),"DELETE CHARACTER",ConfirmDeleteCharacter,CanConfirmCharacterDeletion);
   Button(1113,197,37,36,"×",CancelDeleteCharacter);
   drawingDeleteCharacterDialog=false;
  }
 }
}
