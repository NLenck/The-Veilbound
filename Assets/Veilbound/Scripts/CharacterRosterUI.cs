using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Veilbound {
 public partial class VeilGame {
  CharacterSaveStore characterStore;CharacterRoster roster;SavedCharacter activeCharacter;LostKnowledgeSave lostKnowledges;
  bool characterProfilesEnabled,creatingCharacter,characterNameFocused,focusCharacterName,newCharacterFemale;int characterPage;const int CharactersPerPage=8;string profileError="",profileNotice="",newCharacterName="";
  static bool PlayableVirtue(Virtue virtue)=>virtue==Virtue.Rage;
  bool CharacterAccountExists(){string path=Path.Combine(Application.persistentDataPath,"characters-v1","index.json");return File.Exists(path)||File.Exists(path+".bak");}
  static Settlement FreshCharacterTown(){return Settlement.NewCamp();}
  void InitializeCharacterProfiles(string root=null,bool importLegacy=false){
   characterProfilesEnabled=true;profileError="";
   try {characterStore=new CharacterSaveStore(root??Path.Combine(Application.persistentDataPath,"characters-v1"));roster=characterStore.LoadRoster();lostKnowledges=characterStore.LoadKnowledges();
    if(!characterStore.HasRoster&&importLegacy){var imported=SavedCharacter.From("Veilborne",city);characterStore.Save(imported,city,roster);profileNotice="Your existing character and settlement have been preserved in their own save slot.";}
    if(roster.characters.Count>0){string id=roster.characters.Exists(c=>c.id==roster.activeId)?roster.activeId:roster.characters[0].id;characterStore.Load(id,out activeCharacter,out city);roster.activeId=id;characterPage=roster.characters.FindIndex(c=>c.id==id)/CharactersPerPage;}
   }catch(Exception e){profileError="Character saves could not be loaded. Existing files were left intact. "+e.Message;Debug.LogWarning(profileError);}
  }
  bool SaveActiveCharacter(){
   if(!characterProfilesEnabled)return true;if(!string.IsNullOrEmpty(profileError))return false;
   try {if(activeCharacter!=null)characterStore.Save(activeCharacter,city,roster);if(lostKnowledges!=null)characterStore.SaveKnowledges(lostKnowledges);return true;}catch(Exception e){profileNotice="Could not save character progress: "+e.Message;Debug.LogWarning(profileNotice);return false;}
  }
  bool CanEnterCharacterRun()=>!characterProfilesEnabled||(activeCharacter!=null&&PlayableVirtue(activeCharacter.virtue)&&string.IsNullOrEmpty(profileError));
  void ResetForCharacter(){ClearRunObjects();build=new RunBuild();ResetCombatReadouts();ResetRage();position=Vector3.zero;level=1;xp=elapsed=0;health=MaxHealth;SelectHighestExpeditionTier();if(actors)RebuildHero();}
  bool SelectSavedCharacter(string id){
   if(!SaveActiveCharacter())return false;
   try {characterStore.Load(id,out var next,out var nextTown);activeCharacter=next;city=nextTown;roster.activeId=id;profileNotice="";ResetForCharacter();return SaveActiveCharacter();}catch(Exception e){profileNotice="Could not load this character: "+e.Message;return false;}
  }
  static string ProfileName(string input){var b=new StringBuilder();foreach(char c in (input??"").Trim())if(!char.IsControl(c)&&b.Length<32)b.Append(c);return b.ToString();}
  bool CreateSavedCharacter(string name,Virtue virtue){
   if(!PlayableVirtue(virtue)||!string.IsNullOrEmpty(profileError))return false;name=ProfileName(name);if(name.Length==0){profileNotice="Give your character a name.";return false;}if(!SaveActiveCharacter())return false;
   try {var fresh=FreshCharacterTown();fresh.female=newCharacterFemale;fresh.virtue=virtue;var record=SavedCharacter.From(name,fresh);characterStore.Save(record,fresh,roster);activeCharacter=record;city=fresh;characterPage=(roster.characters.Count-1)/CharactersPerPage;creatingCharacter=false;profileNotice="New character, new settlement. Lost Knowledges remain account-wide.";ResetForCharacter();SetState(ScreenState.CharacterSelect);return true;}catch(Exception e){profileNotice="Could not create character: "+e.Message;return false;}
  }
  void OpenCharacterSelection(){if(!characterProfilesEnabled||state!=ScreenState.Settlement||!SaveActiveCharacter())return;creatingCharacter=false;CloseGearBag();LeaveResourceBar();ClearRunObjects();SetState(ScreenState.CharacterSelect);}
  void EnterSelectedCharacter(){if(!CanEnterCharacterRun())return;creatingCharacter=false;tab=0;SetState(ScreenState.Settlement);SelectHighestExpeditionTier();Save();}
  void BeginCharacterCreation(){newCharacterFemale=city.female;creatingCharacter=true;newCharacterName="Veilborne "+((roster?.characters.Count??0)+1);profileNotice="";selection=0;focusCharacterName=true;}
  void SetCharacterAppearance(bool female) {if(activeCharacter==null)return;city.female=female;activeCharacter.female=female;ReleaseInventoryIcons();RebuildHero();Save();}
  void CharacterSelectionUpdate(float dt){
   if(creatingCharacter&&characterNameFocused&&Keyboard.current?.enterKey.wasPressedThisFrame==true){CreateSavedCharacter(newCharacterName,Virtue.Rage);return;}
   if(creatingCharacter&&(Keyboard.current?.escapeKey.wasPressedThisFrame==true||PreferredPad?.buttonEast.wasPressedThisFrame==true)){creatingCharacter=false;return;}
   if(deleteCharacterOpen)ReadDeleteCharacterInput();else MenuInput();animClock+=dt;if(hero!=null){hero.SetMotion(Vector3.zero);hero.root.gameObject.SetActive(activeCharacter!=null||creatingCharacter);hero.root.rotation=Quaternion.Euler(0,155+Mathf.Sin(animClock*.25f)*5,0);hero.Animate(animClock,0,0);}PositionCamera(dt);
   saveClock+=dt;if(saveClock>15){saveClock=0;Save();}
  }
  void CharacterSelectionScreen(){DrawCharacterRosterScreen();}
  void CreateCharacterScreen(){
   Box(new Rect(65,145,1470,680),new Color(.025f,.031f,.027f,.98f));GUI.Label(new Rect(105,180,1300,33),"CHARACTER NAME",CardStyle(17,hudLight,bold:true));var style=new GUIStyle(GUI.skin.textField){fontSize=24};GUI.SetNextControlName("CharacterName");newCharacterName=GUI.TextField(new Rect(105,225,1325,49),newCharacterName,32,style);if(focusCharacterName){GUI.FocusControl("CharacterName");if(Event.current.type==EventType.Repaint)focusCharacterName=false;}characterNameFocused=GUI.GetNameOfFocusedControl()=="CharacterName";
   GUI.Label(new Rect(105,310,1325,30),"CHOOSE A VIRTUE",CardStyle(18,hudLight,bold:true));string[] names={"Rage","Devotion","Academic","Discipline"},descriptions={"Controlled fury / Available now","Faith and sacrifice / Planned","Study and spellcraft / Planned","Precision and mastery / Planned"};
   for(int i=0;i<4;i++){Rect r=new Rect(105+i*337,360,317,144);Button(r.x,r.y,r.width,r.height,"",()=>{},i==0);GUI.Label(new Rect(r.x+18,r.y+20,r.width-36,38),names[i],CardStyle(25,i==0?hudLight:muted,bold:true));GUI.Label(new Rect(r.x+18,r.y+74,r.width-36,54),descriptions[i],CardStyle(15,muted));if(i==0)CardOutline(r,hudLight,2);}
   GUI.Label(new Rect(105,545,1320,82),"A new character starts from the beginning with its own settlement, resources and equipment.\nLost Knowledges are stored globally. Their resource-generation bonuses will be added once defined.\nRage is playable now; the other Virtues are reserved for their future combat kits.",CardStyle(16,parchment));
   Button(105,638,640,38,(newCharacterFemale?"":"Selected / ")+"Male appearance",()=>newCharacterFemale=false);Button(775,638,655,38,(newCharacterFemale?"Selected / ":"")+"Female appearance",()=>newCharacterFemale=true);
   Button(105,700,640,62,"Create Character",()=>CreateSavedCharacter(newCharacterName,Virtue.Rage),ProfileName(newCharacterName).Length>0);Button(775,700,655,62,"Cancel",()=>creatingCharacter=false);if(!string.IsNullOrEmpty(profileNotice))GUI.Label(new Rect(105,783,1320,45),profileNotice,CardStyle(13,hudLight));
  }
 }
}
