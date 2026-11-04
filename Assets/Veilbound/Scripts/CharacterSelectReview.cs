using System;
using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace Veilbound {
 public partial class VeilGame {
  void CheckCharacterDeletionStorage(string folder){
   var store=new CharacterSaveStore(Path.Combine(folder,"delete-"+Guid.NewGuid().ToString("N")));var list=store.LoadRoster();var town=FreshCharacterTown();town.wood=321;var first=SavedCharacter.From("Delete me",town);store.Save(first,town,list);town.wood=432;store.Save(first,town,list);
   var otherTown=FreshCharacterTown();otherTown.wood=999;var other=SavedCharacter.From("Keep me",otherTown);store.Save(other,otherTown,list);
   var knowledge=new LostKnowledgeSave();knowledge.discoveries.Add(new LostKnowledgeEntry{id="retained",rank=3});store.SaveKnowledges(knowledge);string knowledgeBefore=File.ReadAllText(Path.Combine(store.root,"lost-knowledges.json"));
   var after=store.Delete(first.id);Require(after.characters.Count==1&&after.characters[0].id==other.id,"Deletion removes only its target from the roster");
   foreach(string name in new[]{"character.json","settlement.json"})foreach(string suffix in new[]{"",".bak",".tmp"})Require(!File.Exists(store.SlotPath(first.id,name)+suffix),"Deleted character and town data plus their backups are removed");
   store.Load(other.id,out _,out var kept);Require(kept.wood==999&&File.ReadAllText(Path.Combine(store.root,"lost-knowledges.json"))==knowledgeBefore,"Other character progress and account knowledge are unchanged");
   File.WriteAllText(Path.Combine(store.root,"index.json"),"bad index");Require(store.LoadRoster().characters.Count==1,"Roster backup cannot resurrect a deleted character");
   bool staleBlocked=false;try{store.Save(first,town,list);}catch(InvalidDataException){staleBlocked=true;}Require(staleBlocked,"A stale autosave reference cannot revive a deleted slot");
   var empty=store.Delete(other.id);Require(empty.characters.Count==0&&empty.activeId==""&&store.HasRoster&&store.LoadKnowledges().discoveries.Count==1,"Deleting the last character leaves a valid empty account and keeps knowledge");
   foreach(string boundary in new[]{"delete-marker","delete-index","delete-files"}){
    string root=Path.Combine(folder,"interrupted-delete-"+Guid.NewGuid().ToString("N"));var clean=new CharacterSaveStore(root);var rosterTest=clean.LoadRoster();var a=SavedCharacter.From("Target",FreshCharacterTown());clean.Save(a,FreshCharacterTown(),rosterTest);var b=SavedCharacter.From("Survivor",FreshCharacterTown());clean.Save(b,FreshCharacterTown(),rosterTest);
    bool interrupted=false;var failing=new CharacterSaveStore(root,step=>{if(step==boundary){interrupted=true;throw new IOException("Injected delete interruption");}});try{failing.Delete(a.id);}catch(IOException){}
    var reopened=new CharacterSaveStore(root);var recovered=reopened.LoadRoster();reopened.Load(b.id,out _,out _);
    Require(interrupted&&recovered.characters.Count==1&&recovered.characters[0].id==b.id&&!File.Exists(Path.Combine(root,"pending-delete.json")),"Interrupted deletion recovers consistently at "+boundary);
   }
   bool invalidBlocked=false;try{store.Delete("../outside");}catch(InvalidDataException){invalidBlocked=true;}Require(invalidBlocked,"Deletion rejects identities outside the save directory");
   Debug.Log("VEILBOUND_CHARACTER_DELETE_STORAGE_PASS");
  }
  IEnumerator CharacterSelectReview(){
   yield return null;combatReviewFreeze=true;
   string folder=Path.Combine(Application.dataPath,"..","Smoke","CharacterSelectUpdate");Directory.CreateDirectory(folder);
   CheckCharacterStore(folder);CheckCharacterSwitching(folder);CheckCharacterDeletionStorage(folder);
   activeCharacter=null;city=FreshCharacterTown();InitializeCharacterProfiles(Path.Combine(folder,"preview-"+Guid.NewGuid().ToString("N")));
   newCharacterFemale=false;Require(CreateSavedCharacter("Aldren",Virtue.Rage),"Create preview character with real assets");string aldren=activeCharacter.id;
   for(int slot=0;slot<8;slot++){city.ownedPlate[slot*10]=true;city.equippedGear[slot]=Metals.Item(0);}city.ownedMetalWeapons[0]=true;city.equippedMetalWeapon=0;city.hours=264;city.victories=8;city.expeditionModifierVersion=1;city.expeditionBestDepth=15;Save();
   newCharacterFemale=true;Require(CreateSavedCharacter("Elara",Virtue.Rage),"Create female preview character");city.expeditionBestDepth=8;Save();string elara=activeCharacter.id;
   newCharacterFemale=false;Require(CreateSavedCharacter("Dorn",Virtue.Rage),"Create fresh preview character");string dorn=activeCharacter.id;
   Require(SelectSavedCharacter(aldren),"Select an existing equipped hero");SetState(ScreenState.CharacterSelect);characterPage=0;for(int i=0;i<50;i++)PositionCamera(.1f);
   yield return new WaitForSecondsRealtime(.6f);yield return CardScreenshot(Path.Combine(folder,"01-character-select.png"));
   OpenDeleteCharacter();Require(selection==0&&!CanConfirmCharacterDeletion,"Delete opens with cancel selected and destructive action disabled");deleteCharacterTyped="wrong name";ConfirmDeleteCharacter();Require(activeCharacter.id==aldren&&roster.characters.Count==3,"Incorrect confirmation cannot remove a character");deleteCharacterTyped="";
   yield return CardScreenshot(Path.Combine(folder,"02-delete-confirmation.png"));var pad=InputSystem.AddDevice<Gamepad>();InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.South));InputSystem.Update();ReadDeleteCharacterInput();InputSystem.RemoveDevice(pad);Require(!deleteCharacterOpen&&roster.characters.Count==3,"Controller confirmation defaults to Cancel and preserves every character");
   Require(SelectSavedCharacter(dorn),"Select disposable character");OpenDeleteCharacter();deleteCharacterTyped="Dorn";ConfirmDeleteCharacter();Require(roster.characters.Count==2&&activeCharacter!=null&&activeCharacter.id!=dorn,"Confirmed deletion updates active selection");Save();Require(characterStore.LoadRoster().characters.Count==2,"Autosave does not recreate deleted character");
   Require(SelectSavedCharacter(elara),"Remaining female character can be loaded");yield return new WaitForSecondsRealtime(.2f);yield return CardScreenshot(Path.Combine(folder,"03-female-character.png"));
   BeginCharacterCreation();yield return CardScreenshot(Path.Combine(folder,"04-create-character.png"));creatingCharacter=false;
   OpenDeleteCharacter();deleteCharacterTyped=activeCharacter.name;ConfirmDeleteCharacter();OpenDeleteCharacter();deleteCharacterTyped=activeCharacter.name;ConfirmDeleteCharacter();Require(activeCharacter==null&&roster.characters.Count==0&&!CanEnterCharacterRun(),"Last deletion returns to empty roster and blocks settlement entry");
   yield return CardScreenshot(Path.Combine(folder,"05-empty-roster.png"));Require(CreateSavedCharacter("New beginning",Virtue.Rage),"Creation works after the last character is deleted");EnterSelectedCharacter();Require(state==ScreenState.Settlement,"New character enters settlement normally");
   File.WriteAllText(Path.Combine(folder,"PASS.txt"),"PASS: existing character storage/switching; target-only deletion; account knowledge retention; backup and stale-autosave protection; interruption recovery at marker/index/files boundaries; path validation; named confirmation and cancel; selected/last deletion; real equipped male and female model previews; creation after empty account; settlement entry. All data isolated in test accounts.");
   Debug.Log("VEILBOUND_CHARACTER_SELECT_UPDATE_PASS");Application.Quit(0);
  }
 }
}
