using System;
using System.IO;
using System.Collections;
using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  IEnumerator LibraryReview(){yield return null;combatReviewFreeze=true;city=Settlement.NewCamp();city.EnsureResearch();city.timber=2;city.quarry=city.mine=city.weaver=city.huntersLodge=city.blacksmith=city.library=1;city.wood=city.stone=10000;city.EnsureIndustry();for(int i=0;i<WeaponRules.Count;i++)city.metalBars[i]=1000;
   ReturnHome();tab=2;SelectCityBuilding("library");libraryResearchOpen=true;SelectResearchTab(0);
   if(Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-libraryPreview")){city.StartResearch("mine",out _);selectedResearch="mine";selectedResearchTier=2;city.wood=32;city.stone=18;city.metalBars[0]=12;for(int m=1;m<city.metalBars.Length;m++)city.metalBars[m]=0;city.AddResource(0,0,24);city.AddResource(2,0,8);city.AddResource(1,0,6);while(true)yield return null;}
   yield return null;Require(selectedResearch=="timber"&&selectedResearchTier==3,"Resources tab selects the next timber technology");Require(ResearchIcon("mine")&&ResearchIcon("weapons")&&ResearchIcon("housing"),"Tree icons load");
   InitArchiveArt();Require(archiveBackdrop&&archiveBook,"Library background and low-poly book emblem are available");for(int skin=0;skin<7;skin++)Require(ArchiveMaterial(skin),"Painted Library material loads: "+skin);ToggleResourceCategory(0);Require(resourceCategory==0&&HandleResourcePointer(ResourceDropdownRect(0).center),"Library resource dropdown opens and consumes clicks over the tree");ToggleResourceCategory(0);Require(resourceCategory==-1,"Library resource dropdown closes");
   string folder=Path.Combine(Application.dataPath,"..","Smoke","Library");Directory.CreateDirectory(folder);
   city.StartResearch("mine",out _);city.Tick(600,2);Require(city.ResearchRank("mine")==1&&Math.Abs(city.researchRemaining-4)<1e-8,"Expedition dilation advances saved Library timer");Save();var saved=JsonUtility.FromJson<Settlement>(File.ReadAllText(savePath));saved.EnsureResearch();Require(saved.activeResearch=="mine"&&Math.Abs(saved.researchRemaining-4)<1e-8,"Actual game save retains active Library research");city=saved;city.Tick(240,1);Require(city.ResearchRank("mine")==2,"Reloaded research completes");
   Require(city.UpgradeBuilding("mine",out notice)&&city.mine==1,"Actual town upgrade queues construction");city.Tick(360,2);Save();city=JsonUtility.FromJson<Settlement>(File.ReadAllText(savePath));city.EnsureResearch();Require(city.activeConstruction=="mine"&&Math.Abs(city.constructionRemaining-12)<1e-8,"Construction survives actual save/reload");city.Tick(360,2);Require(city.mine==2,"Construction uses expedition clock");
   SelectResearchTab(1);yield return null;Require(selectedResearch=="blacksmith"&&selectedResearchTier==2,"Crafting opens at the next refining tier");SelectResearchTab(2);yield return null;Require(selectedResearch=="housing"&&selectedResearchTier==1,"Housing opens at first unlearned tier");SelectResearchTab(3);yield return null;Require(selectedResearch=="alchemy"&&selectedResearchTier==1,"Mysticism selects its planned alchemy path");
   double time=city.hours;SetState(ScreenState.Paused);Require(!SimulationActive,"Pause disables city simulation");yield return new WaitForSecondsRealtime(.15f);Require(city.hours==time,"Pause does not advance research city clock");ReturnHome();libraryResearchOpen=false;StartRun();Require(!libraryResearchOpen,"Research overlay does not leak into expedition");
   File.WriteAllText(Path.Combine(folder,"PASS.txt"),"PASS: node tree tabs and icons; independent research and construction queues; actual saved timer roundtrip; expedition dilation; paused clock; research overlay isolation. Editor validation covers costs, bootstrap, atomic transactions, refunds, legacy unlock migration and exact mid-tick production changes.");Debug.Log("VEILBOUND_LIBRARY_RUNTIME_PASS");Application.Quit(0);
  }
 }
}
