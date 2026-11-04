using System;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Veilbound {public partial class VeilGame {
 bool mysticOpen,mysticReturnBag;int mysticReturnTab,mysticItem,mysticPage,mysticMode,mysticCategory,mysticStat,mysticSlot;string mysticFeedback="";
 bool MysticScreenOpen=>mysticOpen&&state==ScreenState.Settlement&&tab==2;
 void OpenMystic(int itemId=0,bool returnToBag=false){city.EnsureGearRolls();mysticReturnBag=returnToBag;mysticReturnTab=tab;gearBagOpen=false;SelectCityBuilding("enchanter");materialProcessingOpen=producerOpen=libraryResearchOpen=sanctuaryWorkshopOpen=false;mysticOpen=true;tab=2;mysticItem=itemId!=0?itemId:city.craftedGear.Count>0?city.craftedGear[0].id:0;mysticPage=mysticMode=mysticSlot=0;mysticFeedback="";selection=0;LeaveResourceBar();}
 void CloseMystic(){mysticOpen=false;if(mysticReturnBag){var item=city.FindRoll(mysticItem);if(item!=null){tab=mysticReturnTab;OpenGearBag(item.slot,item.id);}}else SelectCityBuilding("enchanter");selection=0;}
 void OpenMysticResearch(){tab=2;gearBagOpen=false;mysticOpen=false;materialProcessingOpen=producerOpen=sanctuaryWorkshopOpen=false;libraryResearchOpen=true;SelectResearchTab(3);selectedResearch="enchanting";selectedResearchTier=Math.Min(4,city.ResearchRank("enchanting")+1);selection=0;}
 void CityMysticDetails(){
  GUI.Label(new Rect(1100,385,450,72),"Enchant owned gear with Veil Essence. Crafting itself adds no random stat bonuses.",CityType(16,cityIvory));
  for(int i=1;i<=4;i++)GUI.Label(new Rect(1100,475+(i-1)*48,450,44),"Level "+i+" / "+MysticRules.Feature(i),CityType(15,city.mysticLevel>=i?cityIvory:muted));
  CityButton(1100,690,450,43,"Enchant equipment",()=>OpenMystic(),city.mysticLevel>0);CityButton(1100,738,450,30,"Research Veil enchanting",OpenMysticResearch,city.library>0);
 }
 bool ReadMysticInput(){if(!MysticScreenOpen)return false;if(resourceFocus||ActionPressed(ControlAction.Resources))return ReadResourceController();if(Keyboard.current?.escapeKey.wasPressedThisFrame==true||PreferredPad?.buttonEast.wasPressedThisFrame==true){if(resourceCategory>=0)LeaveResourceBar();else CloseMystic();return true;}if(resourceCategory>=0)return true;MenuInput();return true;}
 void ApplyMysticEnchantment(){if(city.EnchantGear(mysticItem,mysticMode,mysticCategory,GearRollRules.Stats[mysticStat],mysticSlot,out mysticFeedback)){health=Mathf.Min(health,MaxHealth);Save();}}
 void MysticScreen(){
  InitSmithArt();ResourceBarInput();Box(new Rect(0,0,1600,900),new Color(.025f,.025f,.035f));ArchivePlate(new Rect(46,132,1508,720),0,18);
  GUI.Label(new Rect(80,154,850,43),"THE MYSTIC / VEIL ENCHANTING",ArchiveType(28,archiveIvory,true));
  GUI.Label(new Rect(82,208,1100,25),"MYSTIC LEVEL "+city.mysticLevel+" / "+city.ItemEnchantmentLimit+" ENCHANTMENT SLOTS PER ITEM",ArchiveType(15,archiveGold));
  ArchiveButton(new Rect(1270,155,258,38),"Back",CloseMystic);ArchiveButton(new Rect(1270,207,258,32),"Research upgrades",OpenMysticResearch,city.library>0);
  GUI.Label(new Rect(82,265,1420,24),"VEIL ESSENCE / "+city.essence.ToString("0.##")+"     Choose an owned item, an enchantment slot and a method.",ArchiveType(15,archiveIvory));
  ArchivePlate(new Rect(64,310,470,498),0,14);ArchivePlate(new Rect(550,310,492,498),0,14);ArchivePlate(new Rect(1058,310,470,498),0,14);
  GUI.Label(new Rect(82,327,430,27),"OWNED EQUIPMENT",ArchiveType(18,archiveGold,true));var entries=city.craftedGear;int pages=Math.Max(1,(entries.Count+5)/6);mysticPage=Mathf.Clamp(mysticPage,0,pages-1);
  for(int i=0;i<6;i++){int index=mysticPage*6+i;if(index>=entries.Count)break;var entry=entries[index];int id=entry.id;float y=370+i*58;ArchiveButton(new Rect(80,y,438,51),"",()=>{mysticItem=id;mysticSlot=0;mysticFeedback="";},true,mysticItem==id);GUI.Label(new Rect(92,y+5,414,22),GearRollRules.BaseName(entry)+" #"+id+(city.EquippedRoll(entry.slot)?.id==id?" / Equipped":""),ArchiveType(14,archiveIvory,true));GUI.Label(new Rect(92,y+29,414,18),entry.rarity+" / "+city.EnchantmentCount(entry)+" enchantments",ArchiveType(11,archiveMuted));}
  ArchiveButton(new Rect(82,750,125,32),"Previous",()=>mysticPage--,mysticPage>0);GUI.Label(new Rect(217,755,160,23),(mysticPage+1)+" / "+pages,ArchiveType(13,archiveMuted,false,TextAnchor.UpperCenter));ArchiveButton(new Rect(387,750,125,32),"Next",()=>mysticPage++,mysticPage<pages-1);
  var item=city.FindRoll(mysticItem);if(item==null){GUI.Label(new Rect(575,370,440,100),"Craft equipment to add it to your collection.",ArchiveType(18,archiveIvory));ArchiveResourceHeader();return;}
  GUI.Label(new Rect(570,327,450,28),"SELECTED ITEM",ArchiveType(18,archiveGold,true));GUI.Label(new Rect(573,375,443,62),GearRollRules.BaseName(item)+" #"+item.id+"\n"+item.rarity+" / "+PaperSlotName(item.slot),ArchiveType(20,archiveIvory,true));
  GUI.Label(new Rect(573,454,443,50),"Material determines base stats.\nNo crafting bonuses.",ArchiveType(14,archiveMuted));
  int count=city.EnchantmentCount(item);mysticSlot=Mathf.Clamp(mysticSlot,0,Math.Min(count,Math.Max(0,city.ItemEnchantmentLimit-1)));
  for(int i=0;i<2;i++){int target=i;float y=533+i*88;bool unlocked=i<city.ItemEnchantmentLimit,exists=i<count;ArchiveButton(new Rect(571,y,451,75),"",()=>{mysticSlot=target;mysticFeedback="";},unlocked,mysticSlot==i&&unlocked);GUI.Label(new Rect(585,y+8,422,24),"SLOT "+(i+1)+(exists?" / Replace selected enchantment":unlocked?" / Add enchantment":" / Requires Mystic level 2"),ArchiveType(13,unlocked?archiveGold:archiveMuted,true));GUI.Label(new Rect(585,y+37,422,24),exists?MysticRules.StatText(item.enchantments[i].stat,item.enchantments[i].value):"Empty",ArchiveType(14,archiveIvory));}
  GUI.Label(new Rect(575,730,442,50),"Replacing costs Veil Essence and changes only the selected slot. Other enchantments stay.",ArchiveType(13,archiveMuted));
  GUI.Label(new Rect(1078,327,430,27),"ENCHANTMENT METHOD",ArchiveType(18,archiveGold,true));string[] modes={"Random stat","Choose category / Level 3","Choose stat / Level 4"};
  for(int i=0;i<3;i++){int mode=i;ArchiveButton(new Rect(1078,371+i*40,430,33),modes[i],()=>{mysticMode=mode;mysticFeedback="";},i==0||city.mysticLevel>=(i==1?3:4),mysticMode==i);}
  if(mysticMode==1)for(int i=0;i<3;i++){int category=i;ArchiveButton(new Rect(1078,506+i*35,430,29),MysticRules.Categories[i],()=>mysticCategory=category,true,mysticCategory==i);}
  if(mysticMode==2)for(int i=0;i<8;i++){int stat=i;ArchiveButton(new Rect(1078+(i%2)*219,505+(i/2)*32,211,27),GearRollRules.Labels[i],()=>mysticStat=stat,true,mysticStat==i);}
  string reason=city.EnchantmentRequirement(item.id,mysticMode,mysticCategory,GearRollRules.Stats[mysticStat],mysticSlot);double cost=MysticRules.Cost(item,mysticMode,mysticSlot);
  GUI.Label(new Rect(1080,648,426,24),"COST / "+cost+" VEIL ESSENCE",ArchiveType(17,archiveGold,true));GUI.Label(new Rect(1080,682,426,50),reason!=""?reason:"A new stat is drawn without duplicating the item's other enchantment.",ArchiveType(13,archiveMuted));
  ArchiveButton(new Rect(1078,749,430,38),mysticSlot<count?"Replace slot "+(mysticSlot+1)+" / "+cost+" essence":"Add enchantment / "+cost+" essence",ApplyMysticEnchantment,reason=="");
  GUI.Label(new Rect(80,824,1440,24),mysticFeedback,ArchiveType(13,archiveGold));ArchiveResourceHeader();
 }
}}
