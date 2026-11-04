using System;
using System.IO;
using System.Collections;
using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  void CheckCharacterStore(string folder){
   var store=new CharacterSaveStore(Path.Combine(folder,"storage-"+Guid.NewGuid().ToString("N")));var list=store.LoadRoster();var a=FreshCharacterTown();a.wood=9000;a.blacksmith=7;a.clearedTier=4;a.victories=3;a.ownedMetalWeapons[9]=true;a.equippedMetalWeapon=9;var first=SavedCharacter.From("First",a);store.Save(first,a,list);
   var b=FreshCharacterTown();var second=SavedCharacter.From("Second",b);store.Save(second,b,list);store.Load(second.id,out var secondLoaded,out var bLoaded);
   Require(bLoaded.wood==0&&bLoaded.stone==0&&bLoaded.mine==0&&bLoaded.quarry==0&&bLoaded.blacksmith==0&&bLoaded.IsWoodenSword&&Array.TrueForAll(bLoaded.equippedGear,x=>x==0)&&bLoaded.clearedTier==0&&!bLoaded.ownedMetalWeapons[9],"A new character has fresh town resources, buildings, inventory and tier progress");
   store.Load(first.id,out var firstLoaded,out var aLoaded);Require(aLoaded.wood==9000&&aLoaded.blacksmith==7&&aLoaded.clearedTier==4&&aLoaded.equippedMetalWeapon==9,"The original character keeps its own wealth, gear and progression");
   var townOnDisk=JsonUtility.FromJson<SavedTown>(File.ReadAllText(store.SlotPath(first.id,"settlement.json")));Require((townOnDisk.town.equippedGear==null||townOnDisk.town.equippedGear.Length==0)&&(townOnDisk.town.craftedGear==null||townOnDisk.town.craftedGear.Count==0)&&townOnDisk.town.virtue==Virtue.None,"Character equipment and identity are physically separate from the town file");
   var knowledge=new LostKnowledgeSave();knowledge.discoveries.Add(new LostKnowledgeEntry{id="test-only-global-memory",rank=2});store.SaveKnowledges(knowledge);bLoaded.wood=222;store.Save(secondLoaded,bLoaded,list);Require(new CharacterSaveStore(store.root).LoadKnowledges().discoveries[0].rank==2,"Lost Knowledge storage survives independent character/town saves");
   aLoaded.wood=10000;aLoaded.clearedTier=5;store.Save(firstLoaded,aLoaded,list);File.WriteAllText(store.SlotPath(first.id,"settlement.json"),"broken JSON");store.Load(first.id,out var recovered,out var recoveredTown);Require(recoveredTown.wood==9000&&recovered.clearedTier==4,"Backup recovery restores a matching character/town generation rather than mixing revisions");
   var interruptedRoot=Path.Combine(folder,"interrupted-"+Guid.NewGuid().ToString("N"));var clean=new CharacterSaveStore(interruptedRoot);var index=clean.LoadRoster();var c=FreshCharacterTown();var character=SavedCharacter.From("Recovery",c);clean.Save(character,c,index);c.wood=1444;c.clearedTier=6;
   bool injected=false;var failing=new CharacterSaveStore(interruptedRoot,step=>{if(step=="character"){injected=true;throw new IOException("Injected interruption");}});try{failing.Save(character,c,index);}catch(IOException){}Require(injected,"Save-interruption test reached the character/town commit boundary");var reopened=new CharacterSaveStore(interruptedRoot);var restoredIndex=reopened.LoadRoster();reopened.Load(character.id,out var resumed,out var resumedTown);Require(resumedTown.wood==1444&&resumed.clearedTier==6&&restoredIndex.activeId==character.id&&!File.Exists(Path.Combine(interruptedRoot,"pending-slot.json")),"Interrupted writes recover character, settlement and roster together");
   var many=new CharacterSaveStore(Path.Combine(folder,"many-"+Guid.NewGuid().ToString("N")));var all=many.LoadRoster();for(int i=0;i<40;i++){var town=FreshCharacterTown();many.Save(SavedCharacter.From("Veilborne "+i,town),town,all);}Require(many.LoadRoster().characters.Count==40,"Character storage has no fixed slot limit and supports multiple UI pages");
   bool blocked=false;try{many.SlotPath("../other","character.json");}catch(InvalidDataException){blocked=true;}Require(blocked,"Character identities cannot escape their save directory");Debug.Log("VEILBOUND_CHARACTER_STORAGE_PASS");
  }
  void CheckCharacterSwitching(string folder){
   city=FreshCharacterTown();city.wood=6000;city.mine=5;city.clearedTier=3;city.victories=2;InitializeCharacterProfiles(Path.Combine(folder,"account-"+Guid.NewGuid().ToString("N")),true);ResetForCharacter();string original=activeCharacter.id;
   Require(roster.characters.Count==1&&city.wood==6000&&city.mine==5&&city.clearedTier==3,"Legacy progress imports intact into an independent character/town pair");
   Require(CreateSavedCharacter("Fresh Rage",Virtue.Rage)&&city.wood==0&&city.mine==0&&city.clearedTier==0,"Creating through the actual UI action starts new progression");string fresh=activeCharacter.id;city.wood=222;city.mine=3;Save();
   Require(SelectSavedCharacter(original)&&city.wood==6000&&city.mine==5&&city.clearedTier==3,"Switching back restores the first character's own settlement");Require(SelectSavedCharacter(fresh)&&city.wood==222&&city.mine==3&&city.clearedTier==0,"The new character's changes stay in its own settlement");
   int count=roster.characters.Count;Require(!CreateSavedCharacter("Unimplemented",Virtue.Devotion)&&roster.characters.Count==count,"Unimplemented Virtues cannot silently play as Rage");
   EnterSelectedCharacter();Require(state==ScreenState.Settlement,"Character selection enters the selected character's settlement");OpenCharacterSelection();Require(state==ScreenState.CharacterSelect&&!SimulationActive,"Character selection pauses production rather than ticking every inactive town");
   OpenSettings();Require(settingsReturn==ScreenState.CharacterSelect,"Settings remember character selection as their return screen");SettingsReturnToSettlement();Require(state==ScreenState.CharacterSelect,"Settings cannot bypass selection into a characterless settlement");
   Require(AudioListener.volume==0,"Character/save tests remain silent");Debug.Log("VEILBOUND_CHARACTER_SWITCHING_PASS");
  }
  IEnumerator CharacterRosterReview(){
   string folder=Path.Combine(Application.dataPath,"..","Smoke","CharacterRoster");Directory.CreateDirectory(folder);string pass=Path.Combine(folder,"PASS.txt");if(File.Exists(pass))File.Delete(pass);
   yield return new WaitForSecondsRealtime(.5f);ReturnHome();combatReviewFreeze=true;CheckCharacterStore(folder);CheckCharacterSwitching(folder);
   for(int i=0;i<10;i++){Require(CreateSavedCharacter("Rage Wanderer "+(i+1),Virtue.Rage),"Creation remains available beyond the first page");yield return null;}
   SetState(ScreenState.CharacterSelect);characterPage=0;for(int i=0;i<40;i++)PositionCamera(.1f);yield return new WaitForSecondsRealtime(.2f);yield return CardScreenshot(Path.Combine(folder,"01-character-selection.png"));characterPage=1;yield return CardScreenshot(Path.Combine(folder,"02-more-characters.png"));
   BeginCharacterCreation();newCharacterName="New Veilborne";yield return CardScreenshot(Path.Combine(folder,"03-create-character.png"));creatingCharacter=false;EnterSelectedCharacter();tab=0;yield return new WaitForSecondsRealtime(.2f);yield return CardScreenshot(Path.Combine(folder,"04-switch-from-settlement.png"));
   File.WriteAllText(pass,"PASS: separate character and per-character settlement files; legacy migration; fresh resources/buildings/inventory/tier for new characters; independent switching and save reload; forty slots and paginated creation beyond eight; global Lost Knowledge data preserved without invented bonuses; matching backup generations; recovery after interrupted writes; safe save identities; planned Virtues cannot play as Rage; character-selection production pause; safe settings return; silent test run and isolated saves.");Debug.Log("VEILBOUND_CHARACTER_ROSTER_PASS");Application.Quit(0);
  }
 }
}
