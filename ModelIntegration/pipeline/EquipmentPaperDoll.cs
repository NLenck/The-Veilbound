using System;
using System.Collections.Generic;
using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  Camera equipmentCamera;RenderTexture equipmentPortrait;readonly RenderTexture[] equipmentIcons=new RenderTexture[10];
  Transform equipmentStudio,equipmentPlinth;Light equipmentKey;HeroRig cachedEquipmentHero;
  static readonly int[] PaperSlots={0,1,2,5,9,8,3,4,6,7,10};
  Rect PortraitRect=>new Rect(458,205,684,494);
  Rect PaperSlotRect(int slot) {
   bool left=slot==0||slot==1||slot==2||slot==5||slot==9;
   int row=slot==0||slot==8?0:slot==1||slot==3?1:slot==2||slot==4?2:slot==5||slot==6?3:slot==7?4:5;
   return new Rect(left?54:1234,225+row*78,312,66);
  }
  string PaperSlotName(int slot)=>slot<9?Gear.Slots[slot]:slot==9?"Main hand":"Off hand";
  void SelectPaperSlot(int slot) {gearSlot=slot;if(slot<9){int item=city.Item(slot);if(item>0)gearMaterial=Gear.Material(item);if(slot==8)gearMaterial=1;if(Metals.Index(item)>=0)selectedMetal=Metals.Index(item);}if(slot==9)selectedMetal=city.WeaponMetalIndex;loadoutInventoryPage=0;selection=Array.IndexOf(PaperSlots,slot)+3;}
  void StaticEquipmentPose() {hero.SetMotion(Vector3.zero);hero.root.rotation=Quaternion.Euler(0,-6,0);hero.Animate(0,0,0);}
  void InitEquipmentStudio() {
   if(equipmentCamera)return;
   equipmentStudio=WorldArt.Group("Equipment inspection studio",transform);
   var obj=WorldArt.Group("Static equipment camera",equipmentStudio);equipmentCamera=obj.gameObject.AddComponent<Camera>();equipmentCamera.enabled=false;equipmentCamera.orthographic=true;equipmentCamera.nearClipPlane=.01f;equipmentCamera.farClipPlane=20;equipmentCamera.cullingMask=1<<29;equipmentCamera.clearFlags=CameraClearFlags.SolidColor;equipmentCamera.backgroundColor=new Color(.027f,.034f,.035f);
   equipmentPortrait=new RenderTexture(684,494,24){name="Static equipped character",antiAliasing=4};
   for(int i=0;i<10;i++)equipmentIcons[i]=new RenderTexture(112,112,24){name="Equipment slot "+i,antiAliasing=4};
   var lamp=WorldArt.Group("Equipment portrait key",equipmentStudio);equipmentKey=lamp.gameObject.AddComponent<Light>();equipmentKey.type=LightType.Directional;equipmentKey.color=new Color(1,.93f,.84f);equipmentKey.intensity=.9f;equipmentKey.cullingMask=1<<29;equipmentKey.enabled=false;
   equipmentPlinth=WorldArt.Part(equipmentStudio,"Inspection plinth",PrimitiveType.Cylinder,Vector3.zero,new Vector3(1.05f,.025f,1.05f),new Color(.085f,.090f,.080f));equipmentPlinth.gameObject.layer=29;equipmentPlinth.gameObject.SetActive(false);
  }
  void RefreshEquipmentPortrait() {
   InitEquipmentStudio();StaticEquipmentPose();if(cachedEquipmentHero==hero)return;
   var layers=new Dictionary<Transform,int>();foreach(var t in hero.root.GetComponentsInChildren<Transform>(true)){layers[t]=t.gameObject.layer;t.gameObject.layer=29;}
   var renderers=new List<Renderer>();var slots=new Dictionary<Renderer,int>();var bakedObjects=new List<GameObject>();var meshes=new List<Mesh>();var originalSkins=new List<SkinnedMeshRenderer>();
   var visual=hero.root.GetComponent<BronzeHeroVisual>();
   foreach(var renderer in hero.root.GetComponentsInChildren<Renderer>()) {
    if(!renderer.enabled||!renderer.gameObject.activeInHierarchy||renderer is TrailRenderer)continue;
    int slot=-1;for(int i=0;i<visual.parts.Length;i++)if(visual.parts[i]==renderer){slot=visual.slots[i];break;}
    if(renderer.transform.IsChildOf(hero.swordPivot))slot=9;
    if(renderer is SkinnedMeshRenderer skin){var mesh=new Mesh();skin.BakeMesh(mesh);var obj=new GameObject("Equipment baked current pose");obj.layer=29;obj.transform.SetParent(skin.transform,false);obj.AddComponent<MeshFilter>().sharedMesh=mesh;var replacement=obj.AddComponent<MeshRenderer>();replacement.sharedMaterials=skin.sharedMaterials;renderers.Add(replacement);slots[replacement]=slot;skin.enabled=false;originalSkins.Add(skin);meshes.Add(mesh);bakedObjects.Add(obj);}
    else {renderers.Add(renderer);slots[renderer]=slot;}
   }
   bool fog=RenderSettings.fog;RenderSettings.fog=false;equipmentKey.enabled=true;equipmentKey.transform.rotation=Quaternion.LookRotation(new Vector3(.35f,-.5f,-1));
   for(int slot=0;slot<10;slot++) {
    Bounds bounds=new Bounds();bool found=false;foreach(var renderer in renderers){renderer.enabled=slots[renderer]==slot;if(renderer.enabled){if(!found){bounds=renderer.bounds;found=true;}else bounds.Encapsulate(renderer.bounds);}}
    equipmentCamera.targetTexture=equipmentIcons[slot];equipmentCamera.aspect=1;
    equipmentCamera.transform.position=(found?bounds.center:hero.root.position)+new Vector3(0,.04f,4);equipmentCamera.transform.LookAt(found?bounds.center:hero.root.position);equipmentCamera.orthographicSize=found?Mathf.Max(.08f,Mathf.Max(bounds.size.x,bounds.size.y)*.64f):1;
    equipmentCamera.Render();
   }
   foreach(var renderer in renderers)renderer.enabled=true;
   equipmentPlinth.position=hero.root.position+Vector3.down*.035f;equipmentPlinth.gameObject.SetActive(true);
   equipmentCamera.targetTexture=equipmentPortrait;equipmentCamera.aspect=684f/494;
   equipmentCamera.transform.position=hero.root.position+new Vector3(0,1.25f,5);equipmentCamera.transform.LookAt(hero.root.position+Vector3.up*1.21f);equipmentCamera.orthographicSize=1.40f;equipmentCamera.Render();
   equipmentPlinth.gameObject.SetActive(false);equipmentKey.enabled=false;RenderSettings.fog=fog;
   foreach(var skin in originalSkins)skin.enabled=true;foreach(var obj in bakedObjects)DestroyImmediate(obj);foreach(var mesh in meshes)DestroyImmediate(mesh);foreach(var item in layers)item.Key.gameObject.layer=item.Value;
   cachedEquipmentHero=hero;
  }
  Vector3 PaperAnchorWorld(int slot) {
   var b=hero.root.GetComponent<BronzeHeroVisual>().bones;
   switch(slot){case 0:return b[4].position+hero.root.up*.14f;case 1:return b[5].position+hero.root.up*.07f;case 2:return b[2].position+hero.root.forward*.12f;case 3:return b[10].position;case 4:return b[0].position+hero.root.up*.025f;case 5:return Vector3.Lerp(b[6].position,b[7].position,.65f);case 6:return Vector3.Lerp(b[14].position,b[15].position,.45f);case 7:return b[16].position+hero.root.forward*.06f;case 8:return b[2].position+hero.root.up*.19f-hero.root.forward*.11f;case 9:return hero.swordPivot.TransformPoint(new Vector3(0,0,.025f));default:return b[7].position;}
  }
  Vector2 PaperAnchorScreen(int slot) {var p=equipmentCamera.WorldToViewportPoint(PaperAnchorWorld(slot));var r=PortraitRect;return new Vector2(r.x+p.x*r.width,r.y+(1-p.y)*r.height);}
  void DrawPaperSlot(int slot) {
   Rect r=PaperSlotRect(slot);bool active=gearSlot==slot;bool left=r.x<800;int item=slot<9?city.Item(slot):1;var rolled=slot<10?city.EquippedRoll(slot):null;Color accent=rolled!=null?Rarities.Colour(rolled.rarity):active?hudLight:item>0?hudGold:new Color(.27f,.29f,.27f);
   HudFrame(r,active);CardOutline(r,accent,active?2:1);Rect icon=new Rect(left?r.x+7:r.xMax-59,r.y+7,52,52);Box(icon,new Color(.018f,.022f,.022f));
   if(slot!=10&&item>0&&equipmentIcons[slot])GUI.DrawTexture(icon,equipmentIcons[slot]);else GUI.Label(icon,slot==10?"2H":"—",CardStyle(19,muted,TextAnchor.MiddleCenter,true));CardOutline(icon,accent);
   float textX=left?r.x+73:r.x+13;GUI.Label(new Rect(textX,r.y+10,225,23),PaperSlotName(slot).ToUpperInvariant(),CardStyle(13,active?hudLight:parchment,bold:true));
   string detail=slot<9?Gear.Name(item):slot==9?SwordName(city.equippedSword):"Occupied by greatsword";if(rolled!=null&&!rolled.inherited)detail=rolled.rarity+" · "+detail;GUI.Label(new Rect(textX,r.y+35,225,23),detail,CardStyle(12,muted));
   int index=buttons.Count;buttons.Add(()=>SelectPaperSlot(slot));if(selection==index)CardOutline(new Rect(r.x+3,r.y+3,r.width-6,r.height-6),parchment);
   if(Event.current.type==EventType.MouseDown&&Event.current.button==0&&r.Contains(Event.current.mousePosition)){SelectPaperSlot(slot);Event.current.Use();}

  }
  void EquipmentPaperDoll() {
   city.EnsureGear();InitHudArt();Box(new Rect(0,0,1600,900),new Color(.018f,.025f,.027f));
   HudFrame(new Rect(30,18,1540,96));GUI.Label(new Rect(54,34,550,38),"THE VEILBORNE  /  LOADOUT",CardStyle(27,parchment,bold:true));
   GUI.Label(new Rect(56,82,650,22),"Inspect a slot to equip, remove or craft a piece.",CardStyle(13,muted));
   Button(54,137,312,42,"EXPEDITION",()=>{tab=0;selection=0;});Button(644,137,312,42,"LOADOUT",()=>{tab=1;selection=0;});Button(1234,137,312,42,"SETTLEMENT",()=>{tab=2;selection=0;});
   if(Event.current.type==EventType.Repaint)RefreshEquipmentPortrait();
   HudFrame(new Rect(452,199,696,506));if(equipmentPortrait)GUI.DrawTexture(PortraitRect,equipmentPortrait);
   GUI.Label(new Rect(510,211,580,24),"VEILBORNE  /  EQUIPPED PREVIEW",CardStyle(11,hudLight,TextAnchor.MiddleCenter));
   if(equipmentCamera)foreach(int slot in PaperSlots)DrawPaperSlot(slot);
   GUI.Label(new Rect(475,675,650,22),$"{120+city.GearHealth+city.GearBonus("vitality"):0.#} HEALTH    /    {(city.WeaponDamage+city.blacksmith*2)*(1+city.GearBonus("power")*.01f):0.#} DAMAGE    /    {city.Protection:P1} PROTECTION",CardStyle(12,hudLight,TextAnchor.MiddleCenter));
   LoadoutInventoryStrip();GUI.Label(new Rect(54,884,1490,16),notice,CardStyle(11,muted));
  }
  void RemoveEquipmentPiece(int slot){if(slot<0||slot>=9)return;city.EnsureGearRolls();city.equippedRollIds[slot]=0;city.equippedGear[slot]=0;RebuildHero();Save();}
  void EquipOwnedPiece(int slot,int item) {if(slot<0||slot>=9||item==0)return;int material=Gear.Material(item),tier=Gear.Tier(item);if(Metals.IsMetal(item)?!city.OwnsPlate(slot,Metals.Index(item)):tier>city.Owned(slot,material))return;city.EquipKnownRoll(slot,item);city.equippedGear[slot]=item;gearMaterial=material;RebuildHero();Save();}
  void EquipmentSelectionDetails() {
   HudFrame(new Rect(30,718,1540,160));
   if(gearSlot>=9){MetalWeaponDetails();return;}
   if(gearMaterial==3){MetalPlateDetails();return;}
   int equipped=city.Item(gearSlot),next=Math.Min(2,city.Owned(gearSlot,gearMaterial)+1);
   GUI.Label(new Rect(54,731,355,30),Gear.Slots[gearSlot].ToUpper()+"  /  "+Gear.Name(equipped),CardStyle(21,parchment,bold:true));
   float pieceArmour=Gear.Material(equipped)==3?Metals.Armour(gearSlot,Metals.Index(equipped)):Gear.Tier(equipped)*(Gear.Material(equipped)==2?7:3)*Gear.Size(gearSlot);
   if(equipped>0)GUI.Label(new Rect(54,774,355,53),$"{pieceArmour:0.#} armour / +{pieceArmour*.15f:0.#} health\n"+(gearSlot==8?"Attaches beneath the shoulder armour.":"Cloth and leather garments add no movement penalty."),CardStyle(12,muted));
   else GUI.Label(new Rect(54,774,355,50),"No item equipped in this slot.",CardStyle(13,muted));
   for(int material=1;material<=3;material++){int selected=material;Button(435+(material-1)*150,732,140,33,(gearMaterial==material?"• ":"")+Gear.Materials[material],()=>gearMaterial=selected,gearSlot!=8||material==1);}
   Settlement.GearCost(gearSlot,gearMaterial,Math.Min(2,next),out double iron,out double cloth,out double leather,out double essence);
   string details=next<=2?$"T{next}  /  {iron:0} iron, {cloth:0} cloth, {leather:0} leather, {essence:0} essence":"This material is fully upgraded.";
   GUI.Label(new Rect(435,777,540,46),details+"\n"+Gear.Materials[gearMaterial]+" workshop level "+city.Workshop(gearMaterial),CardStyle(12,muted));
   Button(1030,733,270,40,next<=2?"CRAFT ROLL T"+next:"FULLY UPGRADED",()=>CraftGear(gearSlot,gearMaterial),next<=2);Button(1315,733,225,40,"REMOVE",()=>RemoveEquipmentPiece(gearSlot),equipped>0);
   Button(54,838,355,29,"BAG / COMPARE ROLLS",()=>OpenGearBag(gearSlot));
   GUI.Label(new Rect(435,836,70,24),"OWNED",CardStyle(11,hudGold,bold:true));int index=0;
   for(int material=1;material<=3;material++)for(int tier=1;tier<=city.Owned(gearSlot,material);tier++){int piece=material*10+tier;Button(506+index*155,827,145,34,(equipped==piece?"• ":"")+Gear.Name(piece),()=>EquipOwnedPiece(gearSlot,piece));index++;}
  }
  void OnDestroy() {ReleaseInventoryIcons();if(equipmentPortrait){equipmentPortrait.Release();Destroy(equipmentPortrait);}foreach(var icon in equipmentIcons)if(icon){icon.Release();Destroy(icon);}if(equipmentStudio)Destroy(equipmentStudio.gameObject);}
 }
}




