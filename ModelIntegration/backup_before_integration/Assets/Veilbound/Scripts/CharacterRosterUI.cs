using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Veilbound {
 public partial class VeilGame {
  CharacterSaveStore characterStore;CharacterRoster roster;SavedCharacter activeCharacter;LostKnowledgeSave lostKnowledges;
  bool characterProfilesEnabled,creatingCharacter,characterNameFocused,focusCharacterName;int characterPage;const int CharactersPerPage=8;string profileError="",profileNotice="",newCharacterName="";
  static bool PlayableVirtue(Virtue virtue)=>virtue==Virtue.Rage;
  bool CharacterAccountExists(){string path=Path.Combine(Application.persistentDataPath,"characters-v1","index.json");return File.Exists(path)||File.Exists(path+".bak");}
  static Settlement FreshCharacterTown(){var c=new Settlement();c.EnsureGear();BronzeHeroVisual.EquipStarterSet(c);c.EnsureGearRolls();c.EnsureIndustry();c.EnsureVirtue();return c;}
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
   try {var fresh=FreshCharacterTown();fresh.virtue=virtue;var record=SavedCharacter.From(name,fresh);characterStore.Save(record,fresh,roster);activeCharacter=record;city=fresh;characterPage=(roster.characters.Count-1)/CharactersPerPage;creatingCharacter=false;profileNotice="New character, new settlement. Lost Knowledges remain account-wide.";ResetForCharacter();SetState(ScreenState.CharacterSelect);return true;}catch(Exception e){profileNotice="Could not create character: "+e.Message;return false;}
  }
  void OpenCharacterSelection(){if(!characterProfilesEnabled||state!=ScreenState.Settlement||!SaveActiveCharacter())return;creatingCharacter=false;CloseGearBag();LeaveResourceBar();ClearRunObjects();SetState(ScreenState.CharacterSelect);}
  void EnterSelectedCharacter(){if(!CanEnterCharacterRun())return;creatingCharacter=false;tab=0;SetState(ScreenState.Settlement);SelectHighestExpeditionTier();Save();}
  void BeginCharacterCreation(){creatingCharacter=true;newCharacterName="Veilborne "+((roster?.characters.Count??0)+1);profileNotice="";selection=0;focusCharacterName=true;}
  void CharacterSelectionUpdate(float dt){
   if(creatingCharacter&&characterNameFocused&&Keyboard.current?.enterKey.wasPressedThisFrame==true){CreateSavedCharacter(newCharacterName,Virtue.Rage);return;}
   if(creatingCharacter&&(Keyboard.current?.escapeKey.wasPressedThisFrame==true||PreferredPad?.buttonEast.wasPressedThisFrame==true)){creatingCharacter=false;return;}
   MenuInput();animClock+=dt;if(hero!=null){hero.root.rotation=Quaternion.Euler(0,155+Mathf.Sin(animClock*.25f)*12,0);hero.Animate(animClock,0,0);}PositionCamera(dt);
   saveClock+=dt;if(saveClock>15){saveClock=0;Save();}
  }
  void CharacterSelectionScreen(){
   Box(new Rect(0,0,1600,900),new Color(.018f,.025f,.022f,.62f));GUI.Label(new Rect(65,40,1400,56),creatingCharacter?"CREATE YOUR VEILBORNE":"CHOOSE YOUR VEILBORNE",CardStyle(35,parchment,bold:true));
   GUI.Label(new Rect(68,99,1400,25),"Each character has its own settlement and progression. Only Lost Knowledges carry across characters.",CardStyle(15,muted));
   if(!string.IsNullOrEmpty(profileError)){HudFrame(new Rect(65,155,1465,560));GUI.Label(new Rect(95,190,1400,400),profileError,CardStyle(20,parchment));Button(95,770,300,55,"Exit Game",ExitGame);return;}
   if(creatingCharacter){CreateCharacterScreen();return;}
   HudFrame(new Rect(65,145,760,605));int count=roster?.characters.Count??0,pages=Mathf.Max(1,Mathf.CeilToInt(count/(float)CharactersPerPage));characterPage=Mathf.Clamp(characterPage,0,pages-1);
   if(count==0)GUI.Label(new Rect(110,235,665,270),"Your story starts here.\nCreate a character to begin a new settlement.",CardStyle(24,parchment,TextAnchor.MiddleCenter));
   for(int i=0;i<CharactersPerPage;i++){int index=characterPage*CharactersPerPage+i;if(index>=count)break;var row=roster.characters[index];Rect r=new Rect(85,165+i*64,720,57);Button(r.x,r.y,r.width,r.height,row.name+"  /  "+row.virtue+"\nHighest expedition: Tier "+row.highestTier,()=>SelectSavedCharacter(row.id));if(row.id==roster.activeId)CardOutline(r,hudLight,2);}
   Button(85,695,105,35,"Previous",()=>characterPage--,characterPage>0);GUI.Label(new Rect(220,702,440,24),count+" CHARACTERS  /  PAGE "+(characterPage+1)+" OF "+pages,CardStyle(13,muted,TextAnchor.MiddleCenter));Button(700,695,105,35,"Next",()=>characterPage++,characterPage+1<pages);
   Button(85,770,340,54,"Create Character",BeginCharacterCreation);Button(445,770,165,54,"Settings",OpenSettings);Button(630,770,175,54,"Exit Game",ExitGame);
   CardOutline(new Rect(865,145,670,590),new Color(.36f,.31f,.22f));GUI.Label(new Rect(900,165,600,48),activeCharacter?.name??"A new beginning",CardStyle(31,parchment,TextAnchor.MiddleCenter,true));GUI.Label(new Rect(900,216,600,28),activeCharacter==null?"CREATE YOUR FIRST CHARACTER":activeCharacter.virtue.ToString().ToUpper(),CardStyle(19,hudLight,TextAnchor.MiddleCenter,true));
   GUI.Label(new Rect(900,605,600,38),activeCharacter==null?"Fresh character / Fresh settlement":$"Tier {activeCharacter.clearedTier+1}  /  {activeCharacter.victories} victories  /  Settlement day {(int)city.hours/24+1}",CardStyle(17,parchment,TextAnchor.MiddleCenter));
   GUI.Label(new Rect(900,655,600,58),"Private progression: resources, buildings, gear and expedition tiers.\nNo materials or inventory are transferred when creating a character.",CardStyle(14,muted,TextAnchor.MiddleCenter));
   HudFrame(new Rect(865,755,670,66));GUI.Label(new Rect(887,763,620,24),"LOST KNOWLEDGES  /  ACCOUNT-WIDE",CardStyle(15,hudLight,bold:true));GUI.Label(new Rect(887,791,620,20),(lostKnowledges?.discoveries.Count??0)+" discoveries. Resource-generation bonuses will be defined later.",CardStyle(12,muted));
   Button(925,837,550,45,"Enter Settlement",EnterSelectedCharacter,activeCharacter!=null&&PlayableVirtue(activeCharacter.virtue));
   if(!string.IsNullOrEmpty(profileNotice))GUI.Label(new Rect(85,837,720,52),profileNotice,CardStyle(13,hudLight));
  }
  void CreateCharacterScreen(){
   Box(new Rect(65,145,1470,680),new Color(.025f,.031f,.027f,.98f));GUI.Label(new Rect(105,180,1300,33),"CHARACTER NAME",CardStyle(17,hudLight,bold:true));var style=new GUIStyle(GUI.skin.textField){fontSize=24};GUI.SetNextControlName("CharacterName");newCharacterName=GUI.TextField(new Rect(105,225,1325,49),newCharacterName,32,style);if(focusCharacterName){GUI.FocusControl("CharacterName");if(Event.current.type==EventType.Repaint)focusCharacterName=false;}characterNameFocused=GUI.GetNameOfFocusedControl()=="CharacterName";
   GUI.Label(new Rect(105,310,1325,30),"CHOOSE A VIRTUE",CardStyle(18,hudLight,bold:true));string[] names={"Rage","Devotion","Academic","Discipline"},descriptions={"Controlled fury / Available now","Faith and sacrifice / Planned","Study and spellcraft / Planned","Precision and mastery / Planned"};
   for(int i=0;i<4;i++){Rect r=new Rect(105+i*337,360,317,144);Button(r.x,r.y,r.width,r.height,"",()=>{},i==0);GUI.Label(new Rect(r.x+18,r.y+20,r.width-36,38),names[i],CardStyle(25,i==0?hudLight:muted,bold:true));GUI.Label(new Rect(r.x+18,r.y+74,r.width-36,54),descriptions[i],CardStyle(15,muted));if(i==0)CardOutline(r,hudLight,2);}
   GUI.Label(new Rect(105,545,1320,82),"A new character starts from the beginning with its own settlement, resources and equipment.\nLost Knowledges are stored globally. Their resource-generation bonuses will be added once defined.\nRage is playable now; the other Virtues are reserved for their future combat kits.",CardStyle(16,parchment));
   Button(105,700,640,62,"Create Character",()=>CreateSavedCharacter(newCharacterName,Virtue.Rage),ProfileName(newCharacterName).Length>0);Button(775,700,655,62,"Cancel",()=>creatingCharacter=false);if(!string.IsNullOrEmpty(profileNotice))GUI.Label(new Rect(105,783,1320,45),profileNotice,CardStyle(13,hudLight));
  }
 }
}
