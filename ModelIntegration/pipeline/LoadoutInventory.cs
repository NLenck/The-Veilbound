using System;
using System.Collections.Generic;
using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  int loadoutInventoryPage;bool smithArmourPage;
  readonly Dictionary<string,RenderTexture> loadoutItemIcons=new Dictionary<string,RenderTexture>();
  List<CraftedGear> LoadoutItems(int slot){city.EnsureGearRolls();var list=city.craftedGear.FindAll(g=>g.slot==slot);list.Sort((a,b)=>a.rarity!=b.rarity?b.rarity.CompareTo(a.rarity):b.id.CompareTo(a.id));return list;}
  void OpenSlotCrafting(){tab=2;sanctuaryWorkshopOpen=true;industryPage=1;smithArmourPage=gearSlot<9;if(gearSlot==10)gearSlot=9;if(gearSlot==8)gearMaterial=1;selection=0;}
  void LoadoutInventoryStrip() {
   Rect panel=new Rect(30,718,1540,160);HudFrame(panel);var list=LoadoutItems(gearSlot);int pages=Math.Max(1,(list.Count+15)/16);loadoutInventoryPage=Mathf.Clamp(loadoutInventoryPage,0,pages-1);
   GUI.Label(new Rect(54,734,580,29),PaperSlotName(gearSlot).ToUpper()+" / "+list.Count+" OWNED",CardStyle(19,parchment,bold:true));
   Button(650,731,145,31,"Unequip",()=>RemoveEquipmentPiece(gearSlot),gearSlot<9&&city.Item(gearSlot)>0);Button(810,731,155,31,"Craft gear",OpenSlotCrafting);Button(980,731,225,31,"Compare rolls",()=>OpenGearBag(gearSlot),list.Count>0);
   Button(1260,731,105,31,"Previous",()=>loadoutInventoryPage--,loadoutInventoryPage>0);GUI.Label(new Rect(1371,737,55,22),(loadoutInventoryPage+1)+" / "+pages,CardStyle(12,muted,TextAnchor.MiddleCenter));Button(1435,731,105,31,"Next",()=>loadoutInventoryPage++,loadoutInventoryPage<pages-1);
   if(list.Count==0){GUI.Label(new Rect(54,786,1460,55),gearSlot==10?"The greatsword occupies both hands. Select Main Hand to change it.":"No owned gear for this slot yet. Craft a piece at its workshop.",CardStyle(17,muted));return;}
   CraftedGear hover=null;Rect hoverRect=new Rect();var equipped=city.EquippedRoll(gearSlot);
   for(int i=0;i<16;i++){int index=loadoutInventoryPage*16+i;if(index>=list.Count)break;var item=list[index];Rect r=new Rect(54+i*92,777,72,72);Color rarity=Rarities.Colour(item.rarity);Box(r,new Color(.023f,.029f,.031f));var icon=InventoryItemIcon(item);if(icon)GUI.DrawTexture(new Rect(r.x+4,r.y+4,64,64),icon);CardOutline(r,rarity,2);
    if(equipped?.id==item.id){Box(new Rect(r.xMax-21,r.y-2,23,20),new Color(.06f,.07f,.06f));CardOutline(new Rect(r.xMax-21,r.y-2,23,20),rarity);GUI.Label(new Rect(r.xMax-21,r.y-2,23,20),"E",CardStyle(12,parchment,TextAnchor.MiddleCenter,true));}
    GUI.Label(new Rect(r.x-5,r.yMax+3,r.width+10,19),"#"+item.id,CardStyle(10,muted,TextAnchor.UpperCenter));int id=item.id;int button=buttons.Count;buttons.Add(()=>OpenGearBag(item.slot,id));if(selection==button)CardOutline(new Rect(r.x-3,r.y-3,r.width+6,r.height+6),parchment);
    if(r.Contains(Event.current.mousePosition)){hover=item;hoverRect=r;if(Event.current.type==EventType.MouseDown&&Event.current.button==0){OpenGearBag(item.slot,item.id);Event.current.Use();}}
   }
   if(hover!=null){Rect tip=new Rect(Mathf.Clamp(hoverRect.center.x-200,35,1160),475,405,232);HudFrame(tip,true);GearComparisonCard(new Rect(tip.x+17,tip.y+12,tip.width-34,202),hover,"SELECT TO COMPARE");}
  }
  RenderTexture InventoryItemIcon(CraftedGear item) {
   string key=item.slot+":"+item.item+":"+city.colour+":"+city.female;if(loadoutItemIcons.TryGetValue(key,out var cached))return cached;if(Event.current.type!=EventType.Repaint)return null;InitEquipmentStudio();
   var snapshot=new Settlement{bronzeHeroVersion=1,colour=city.colour,female=city.female};snapshot.EnsureGear();if(item.slot==9)snapshot.equippedMetalWeapon=item.item;else snapshot.equippedGear[item.slot]=item.item;
   var preview=WorldArt.Hero(equipmentStudio,snapshot);preview.root.position=new Vector3(500,0,0);preview.root.rotation=Quaternion.identity;preview.Animate(0,0,0);var visual=preview.root.GetComponent<BronzeHeroVisual>();foreach(var t in preview.root.GetComponentsInChildren<Transform>(true))t.gameObject.layer=29;
   var baked=new List<GameObject>();var meshes=new List<Mesh>();Bounds bounds=new Bounds();bool found=false;
   foreach(var renderer in preview.root.GetComponentsInChildren<Renderer>()){
    int slot=-1;for(int i=0;i<visual.parts.Length;i++)if(visual.parts[i]==renderer){slot=visual.slots[i];break;}if(renderer.transform.IsChildOf(preview.swordPivot))slot=9;bool show=renderer.enabled&&renderer.gameObject.activeInHierarchy&&slot==item.slot&&!(renderer is TrailRenderer);renderer.enabled=show;if(!show)continue;Renderer visible=renderer;
    if(renderer is SkinnedMeshRenderer skin){var mesh=new Mesh();skin.BakeMesh(mesh);var obj=new GameObject("Inventory thumbnail baked pose");obj.layer=29;obj.transform.SetParent(skin.transform,false);obj.AddComponent<MeshFilter>().sharedMesh=mesh;var replacement=obj.AddComponent<MeshRenderer>();replacement.sharedMaterials=skin.sharedMaterials;skin.enabled=false;visible=replacement;baked.Add(obj);meshes.Add(mesh);}
    if(!found){bounds=visible.bounds;found=true;}else bounds.Encapsulate(visible.bounds);
   }
   var texture=new RenderTexture(80,80,24){name="Inventory "+key};var oldTarget=equipmentCamera.targetTexture;bool fog=RenderSettings.fog;RenderSettings.fog=false;equipmentKey.enabled=true;equipmentKey.transform.rotation=Quaternion.LookRotation(new Vector3(.35f,-.5f,-1));equipmentCamera.targetTexture=texture;equipmentCamera.aspect=1;Vector3 center=found?bounds.center:preview.root.position;equipmentCamera.transform.position=center+new Vector3(0,.04f,4);equipmentCamera.transform.LookAt(center);equipmentCamera.orthographicSize=found?Mathf.Max(.08f,Mathf.Max(bounds.size.x,bounds.size.y)*.64f):1;equipmentCamera.Render();equipmentCamera.targetTexture=oldTarget;equipmentKey.enabled=false;RenderSettings.fog=fog;
   foreach(var obj in baked)DestroyImmediate(obj);foreach(var mesh in meshes)DestroyImmediate(mesh);preview.Release();DestroyImmediate(preview.root.gameObject);loadoutItemIcons[key]=texture;return texture;
  }
  void ReleaseInventoryIcons(){foreach(var texture in loadoutItemIcons.Values)if(texture){texture.Release();Destroy(texture);}loadoutItemIcons.Clear();}
  void ArmourCraftPanel() {
   city.EnsureGearRolls();if(gearSlot>8)gearSlot=2;if(gearSlot==8)gearMaterial=1;GUI.Label(new Rect(453,284,700,30),"CRAFT ARMOUR",CardStyle(22,hudLight,bold:true));Button(1215,282,300,34,"Bars & weapons",()=>{smithArmourPage=false;selection=0;});
   for(int m=1;m<=3;m++){int family=m;Button(453+(m-1)*355,329,340,32,(gearMaterial==m?"• ":"")+Gear.Materials[m],()=>gearMaterial=family,gearSlot!=8||m==1);}
   if(gearMaterial==3)MetalPalette(453,375,1060);else GUI.Label(new Rect(453,385,1050,44),Gear.Materials[gearMaterial]+" workshop level "+city.Workshop(gearMaterial)+" / Current recipes retain their two-tier progression.",CardStyle(16,muted));
   for(int slot=0;slot<9;slot++){int selected=slot;Button(453+(slot%5)*213,467+(slot/5)*39,201,31,(gearSlot==slot?"• ":"")+Gear.Slots[slot],()=>{gearSlot=selected;if(slot==8)gearMaterial=1;},slot!=8||gearMaterial==1);}
   GUI.Label(new Rect(453,568,1050,30),Gear.Slots[gearSlot].ToUpper()+" / "+(gearMaterial==3?Metals.All[selectedMetal].name+" plate":Gear.Materials[gearMaterial]),CardStyle(21,parchment,bold:true));
   bool unlocked;string cost;
   if(gearMaterial==3){var def=Metals.All[selectedMetal];unlocked=city.blacksmith>=def.unlock;cost=city.RecipeText(city.PlateRecipe(gearSlot,selectedMetal))+"  /  Have "+Math.Floor(city.metalBars[selectedMetal])+" bars";GUI.Label(new Rect(453,650,1050,27),$"{Metals.Armour(gearSlot,selectedMetal):0.#} armour / {Metals.Mass(gearSlot,selectedMetal):0.##} kg / Blacksmith {def.unlock}",CardStyle(16,hudLight));}
   else {int next=Math.Min(2,city.Owned(gearSlot,gearMaterial)+1);Settlement.GearCost(gearSlot,gearMaterial,next,out double iron,out double cloth,out double hide,out double essence);cost=$"T{next}: {iron:0} iron / {cloth:0} cloth / {hide:0} leather / {essence:0} essence";unlocked=city.Workshop(gearMaterial)>=next;}
   GUI.Label(new Rect(453,612,1050,33),cost,CardStyle(16,muted));GUI.Label(new Rect(453,690,1050,28),"Each craft rolls new bonuses; your currently equipped piece is retained.",CardStyle(13,muted));Button(453,752,325,47,"Craft new roll",()=>{if(gearMaterial==3)CraftSelectedPlate();else CraftGear(gearSlot,gearMaterial);},unlocked);Button(795,752,325,47,"Compare owned",()=>OpenGearBag(gearSlot));Button(1137,752,378,47,"Loadout",()=>{tab=1;selection=0;});
  }
 }
}
