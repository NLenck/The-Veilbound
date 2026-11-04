using System;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Veilbound {
 public partial class VeilGame {
  void SelectResearchTab(int index){researchTab=(index+4)%4;researchScroll=Vector2.zero;selectedResearch=researchTab==3?"alchemy":ResearchBranches[researchTab][0];selectedResearchTier=researchTab==3?1:Math.Min(LibraryResearch.Limit(selectedResearch),city.ResearchRank(selectedResearch)+1);researchScroll.y=Mathf.Clamp((selectedResearchTier-3)*88,0,(researchTab>=2?5:10)*88-320);selection=0;}
  bool ReadLibraryInput(){if(state!=ScreenState.Settlement||!libraryResearchOpen)return false;if(resourceFocus||ActionPressed(ControlAction.Resources))return ReadResourceController();var pad=PreferredPad;var k=Keyboard.current;
   if(k?.escapeKey.wasPressedThisFrame==true||pad?.buttonEast.wasPressedThisFrame==true){if(resourceCategory>=0){resourceCategory=-1;return true;}libraryResearchOpen=false;selection=0;return true;}
   if(resourceCategory>=0)return true;
   if(pad?.rightShoulder.wasPressedThisFrame==true)SelectResearchTab(researchTab+1);if(pad?.leftShoulder.wasPressedThisFrame==true)SelectResearchTab(researchTab-1);
   int horizontal=k?.rightArrowKey.wasPressedThisFrame==true||pad?.dpad.right.wasPressedThisFrame==true?1:k?.leftArrowKey.wasPressedThisFrame==true||pad?.dpad.left.wasPressedThisFrame==true?-1:0;
   string[] paths=researchTab==3?new[]{"alchemy","enchanting"}:ResearchBranches[researchTab];if(horizontal!=0){int i=Array.IndexOf(paths,selectedResearch);selectedResearch=paths[(Math.Max(0,i)+horizontal+paths.Length)%paths.Length];}
   int vertical=k?.downArrowKey.wasPressedThisFrame==true||pad?.dpad.down.wasPressedThisFrame==true?1:k?.upArrowKey.wasPressedThisFrame==true||pad?.dpad.up.wasPressedThisFrame==true?-1:0;
   selectedResearchTier=Mathf.Clamp(selectedResearchTier+vertical,1,LibraryResearch.IsSlotResearch(selectedResearch)?3:selectedResearch=="enchanting"?4:researchTab>=2?5:10);if(vertical!=0)researchScroll.y=Mathf.Clamp((selectedResearchTier-1)*88-130,0,(researchTab>=2?5:10)*88-320);
   if(k?.enterKey.wasPressedThisFrame==true||pad?.buttonSouth.wasPressedThisFrame==true){if(LibraryResearch.Index(selectedResearch)>=0&&selectedResearchTier==city.ResearchRank(selectedResearch)+1){QueueGoal(1,selectedResearch,target:selectedResearchTier);}}return true;
  }
  Texture2D ResearchIcon(string id)=>LibraryResearch.IsSlotResearch(id)?MaterialIcon(Settlement.SlotResearchSource(id)==0?"group-ore":Settlement.SlotResearchSource(id)==1?"rawhide":Settlement.SlotResearchSource(id)==2?"flax":"group-bars"):id=="weapons"?UpgradeIcon("cleave"):id=="plate"?UpgradeIcon("guard"):id=="housing"?UpgradeIcon("vitality"):id=="clothier"?MaterialIcon("flax"):id=="hunters"?MaterialIcon("rawhide"):id=="weaver"?MaterialIcon("flax"):id=="blacksmith"?MaterialIcon("tin-ore"):id=="quarry"?MaterialIcon("stone"):id=="mine"?MaterialIcon("iron-ore"):id=="alchemy"?UpgradeIcon("regen"):id=="enchanting"?UpgradeIcon("luck"):MaterialIcon("timber");
  string ResearchShortName(string id)=>LibraryResearch.IsSlotResearch(id)?LibraryResearch.Names[LibraryResearch.Index(id)]:id=="blacksmith"?"Refining":id=="plate"?"Plate armour":id=="weapons"?"Weapons":id=="clothier"?"Clothier":id=="housing"?"Housing":id=="alchemy"?"Alchemy":id=="enchanting"?"Enchanting":id=="timber"?"Timber":id=="quarry"?"Stone":id=="mine"?"Mining":id=="hunters"?"Hunting":"Textiles";
  void DrawResearchTree(){
   string[] paths=researchTab==3?new[]{"alchemy","enchanting"}:ResearchBranches[researchTab];int tiers=researchTab>=2?5:10;float width=1020f/paths.Length;
   ArchivePlate(new Rect(62,360,1064,451),0,15);
   for(int col=0;col<paths.Length;col++){if(paths[col]==selectedResearch)Box(new Rect(76+col*width,365,width-4,40),new Color(.42f,.32f,.12f,.22f));}
   for(int col=0;col<paths.Length;col++)GUI.Label(new Rect(76+col*width,374,width,27),ResearchShortName(paths[col]).ToUpper(),ArchiveType(14,archiveGold,true,TextAnchor.UpperCenter));
   ArchiveRule(83,403,1014);
   researchScroll=GUI.BeginScrollView(new Rect(76,413,1032,382),researchScroll,new Rect(0,0,1020,tiers*88+12),false,false,GUIStyle.none,GUIStyle.none);
   for(int col=0;col<paths.Length;col++){
    string id=paths[col];int rank=city.ResearchRank(id);float x=col*width+width/2-(id=="mine"||id=="hunters"||id=="weaver"||id=="blacksmith"||id=="clothier"?20:0);
    int nodeCount=id=="enchanting"?4:tiers;for(int t=1;t<=nodeCount;t++){
     int nodeTier=t;float y=13+(t-1)*88;bool known=t<=rank,active=city.activeResearch==id&&city.researchTarget==t,available=LibraryResearch.Index(id)>=0&&t==rank+1&&city.library>0,chosen=selectedResearch==id&&selectedResearchTier==t;
     Color color=known?archiveGold:active?archiveRed:available?new Color(.66f,.53f,.36f):new Color(.38f,.37f,.33f);
     if(t<nodeCount){Box(new Rect(x-1,y+56,2,32),t<rank?archiveGold:new Color(.25f,.23f,.19f));var matrix=GUI.matrix;GUIUtility.RotateAroundPivot(45,new Vector2(x,y+75));Box(new Rect(x-2,y+73,4,4),t<rank?archiveGold:new Color(.34f,.31f,.25f));GUI.matrix=matrix;}
     if(chosen)ArchivePlate(new Rect(x-34,y-5,68,66),active?5:2,11);
     ArchiveButton(new Rect(x-29,y,58,56),"",()=>{selectedResearch=id;selectedResearchTier=nodeTier;},true,false,active?5:known?4:available?6:3);
     var icon=ResearchIcon(id);GUI.color=known||available||active?Color.white:new Color(.40f,.40f,.38f);if(icon)GUI.DrawTexture(new Rect(x-23,y+5,46,46),icon,ScaleMode.ScaleToFit);GUI.color=Color.white;
     if(known)GUI.Label(new Rect(x+12,y+1,20,19),"✓",ArchiveType(13,archiveGold,true));else if(active)GUI.Label(new Rect(x+12,y+1,20,19),"…",ArchiveType(13,archiveRed,true));else if(available)GUI.Label(new Rect(x+12,y+1,20,19),"+",ArchiveType(13,archiveIvory,true));else SmithLock(new Rect(x+16,y+2,12,15));
     if(chosen&&!resourceFocus&&resourceCategory<0)DrawMenuNavigationFocus(new Rect(x-29,y,58,56),ResearchShortName(id));
     bool producer=id=="mine"||id=="hunters"||id=="weaver"||id=="blacksmith"||id=="clothier";
     GUI.Label(new Rect(x-(producer?39:91),y+58,producer?78:182,23),producer?"T"+t+(known?" / learned":active?" / active":""):known?"Tier "+t+" · Learned":active?"Tier "+t+" · Studying":"Tier "+t+(available?" · Next":" · Locked"),ArchiveType(producer?10:12,color,false,TextAnchor.UpperCenter));
     if(producer&&t%3==0)DrawSlotResearchNode(id,t/3,x,y);
    }
   }
   GUI.EndScrollView();
   float max=tiers*88+12-382;ArchivePlate(new Rect(1112,416,5,373),3,2);ArchivePlate(new Rect(1111,416+(max<=0?0:researchScroll.y/max*305),7,68),6,3);
   DrawResearchNodeDetails();
  }
  void DrawResearchNodeDetails(){if(LibraryResearch.IsSlotResearch(selectedResearch)){DrawSlotResearchDetails();return;}
   ArchivePlate(new Rect(1140,360,390,451),0,17);string id=selectedResearch;bool planned=researchTab==3&&id!="enchanting";int i=LibraryResearch.Index(id);if(!planned&&i<0){SelectResearchTab(researchTab);id=selectedResearch;i=LibraryResearch.Index(id);}
   ArchivePlate(new Rect(1160,380,64,64),6,11);var icon=ResearchIcon(id);if(icon)GUI.DrawTexture(new Rect(1167,387,50,50),icon,ScaleMode.ScaleToFit);
   GUI.Label(new Rect(1243,385,266,58),ResearchShortName(id).ToUpper()+"\nTIER "+selectedResearchTier,ArchiveType(20,archiveIvory,true));ArchiveRule(1164,460,342);
   if(planned){GUI.Label(new Rect(1165,486,340,223),"PLANNED TECHNOLOGY\n\nAlchemy is planned. Veil enchanting is available in the adjacent research path. No materials are charged.",ArchiveType(16,archiveMuted));return;}
   int rank=city.ResearchRank(id),target=selectedResearchTier;bool known=target<=rank,next=target==rank+1;var cost=LibraryResearch.Cost(id,target);
   string unlock=id=="enchanting"?"Mystic level "+target+" / "+MysticRules.Feature(target):id=="weapons"||id=="plate"||id=="blacksmith"?Metals.All[Mathf.Clamp(target-1,0,9)].name+" "+ResearchShortName(id).ToLower():ResearchShortName(id)+" building level "+target;
   GUI.Label(new Rect(1165,476,340,23),"UNLOCKS",ArchiveType(13,archiveGold,true));GUI.Label(new Rect(1165,504,340,42),unlock,ArchiveType(16,archiveIvory));ArchiveRule(1164,548,342);
   GUI.Label(new Rect(1165,562,340,23),"COST",ArchiveType(13,archiveGold,true));
   Texture2D[] icons={MaterialIcon("timber"),MaterialIcon("stone"),MaterialIcon("group-bars")};string[] amounts={cost.wood+" wood",cost.stone+" stone",cost.materials[0].amount+" "+WeaponRules.BarName(cost.materials[0].index)};
   for(int part=0;part<3;part++){float x=1165+part*113;if(icons[part])GUI.DrawTexture(new Rect(x,593,27,27),icons[part]);GUI.Label(new Rect(x+31,592,80,45),amounts[part],ArchiveType(12,archiveIvory));}
   ArchiveRule(1164,643,342);GUI.Label(new Rect(1165,657,340,24),"TIME / "+LibraryResearch.Duration(target)+" city hours",ArchiveType(14,archiveGold,true));
   string reason=known?"Learned. Build or craft the unlock separately.":!next?"Requires previous tier":city.activeResearch==id&&city.researchTarget==target?"Research in progress.":city.ResearchRequirement(id);if(next&&reason==""&&!city.CanResearch(id))reason="Not enough materials";
   GUI.Label(new Rect(1165,696,340,41),reason,ArchiveType(13,archiveMuted));ArchiveButton(new Rect(1161,751,348,40),known?"Learned":city.activeResearch==id&&city.researchTarget==target?"Researching":next?"Queue next tier":"Queue future tier",()=>QueueGoal(1,id,target:target),!known&&!(city.activeResearch==id&&city.researchTarget==target));
  }
 }
}