using System;
using System.Collections.Generic;
using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  int resourceCategory=-1;
  static readonly string[] ResourceCategories={"Building materials","Ore","Bars","Textiles","Hides","Essence"};
  static readonly string[] ResourceCategoryIcons={"group-building-materials","group-ore","group-bars","group-textiles","group-hides","essence"};
  static readonly string[][] ResourceIconNames={new[]{"tin-ore","copper-ore","zinc-ore","iron-ore","coal","titanium-ore","tungsten-ore","veilcrystal"},new[]{"rawhide","cured-hide","thick-hide","primal-hide","veil-hide"},new[]{"flax","wool","silk","ash-silk","veil-fibre"}};
  readonly Dictionary<string,Texture2D> materialIcons=new Dictionary<string,Texture2D>();
  struct ResourceDisplay {public string name,icon;public double stock,rate;public bool locked;public ResourceDisplay(string n,string i,double s,double r,bool l=false){name=n;icon=i;stock=s;rate=r;locked=l;}}
  Texture2D MaterialIcon(string name){if(!materialIcons.TryGetValue(name,out var icon)){icon=Resources.Load<Texture2D>("MaterialIcons/"+name);materialIcons[name]=icon;}return icon;}
  Rect ResourceCategoryRect(int group)=>group==5?new Rect(1330,34,212,66):new Rect(324+184*group,34,174,66);
  List<ResourceDisplay> ResourceRows(int group) {
   city.EnsureGear();city.EnsureIndustry();var rows=new List<ResourceDisplay>();
   if(group==0){rows.Add(new ResourceDisplay("Timber","timber",city.wood,city.WoodRate));rows.Add(new ResourceDisplay("Stone","stone",city.stone,city.StoneRate));}
   else if(group==5)rows.Add(new ResourceDisplay("Essence","essence",city.essence,0));
   else if(group==2){for(int i=0;i<10;i++)rows.Add(new ResourceDisplay(Metals.All[i].name+" bar","group-bars",city.ResourceAmount(3,i),0,city.blacksmith<Metals.All[i].unlock));}
   else {int family=group==1?0:group==3?2:1;for(int i=0;i<Settlement.ResourceNames[family].Length;i++)rows.Add(new ResourceDisplay(Settlement.ResourceNames[family][i],ResourceIconNames[family][i],city.ResourceAmount(family,i),city.ProductionRate(family,i),city.ProducerLevel(family)<Settlement.ResourceUnlocks[family][i]));}
   return rows;
  }
  double ResourceCategoryStock(int group){double total=0;foreach(var row in ResourceRows(group))total+=row.stock;return total;}
  Rect ResourceDropdownRect(int group){var button=ResourceCategoryRect(group);return new Rect(Mathf.Clamp(button.center.x-230,30,1110),122,460,100+ResourceRows(group).Count*48);}
  void ToggleResourceCategory(int group){if(state!=ScreenState.Settlement||group<0||group>5)return;resourceCategory=resourceCategory==group?-1:group;selection=0;}
  bool HandleResourcePointer(Vector2 point) {
   if(new Rect(213,36,93,30).Contains(point))return false;
   for(int i=0;i<6;i++)if(ResourceCategoryRect(i).Contains(point)){ToggleResourceCategory(i);return true;}
   if(resourceCategory>=0){Rect drop=ResourceDropdownRect(resourceCategory);if(new Rect(drop.xMax-32,drop.y+10,22,22).Contains(point)){resourceCategory=-1;return true;}if(drop.Contains(point))return true;resourceCategory=-1;}
   return new Rect(30,18,1540,96).Contains(point);
  }
  void ResourceBarInput() {
   if(Event.current.type==EventType.MouseDown&&Event.current.button==0){resourceFocus=false;}
   if(Event.current.type==EventType.MouseDown&&Event.current.button==0&&HandleResourcePointer(Event.current.mousePosition))Event.current.Use();
   if(resourceCategory>=0&&Event.current.type==EventType.ScrollWheel&&ResourceDropdownRect(resourceCategory).Contains(Event.current.mousePosition))Event.current.Use();
  }
  void ResourceTopBar() {
   if(tab==2&&!gearBagOpen&&!sanctuaryWorkshopOpen){CityResourceHeader();return;}
   city.EnsureGear();city.EnsureIndustry();HudFrame(new Rect(30,18,1540,96));
   string section=gearBagOpen?"CRAFTED GEAR":tab==1?"LOADOUT":tab==2?"SETTLEMENT":"VEILBOUND";
   GUI.Label(new Rect(54,37,155,28),section,CardStyle(20,parchment,bold:true));GUI.Label(new Rect(54,77,243,23),resourceFocus?"LB / RB  ·  A open  ·  B back":CityClock+"  /  ×1",CardStyle(12,muted));
   SettingsAccessButton();
   if(!resourceFocus)GUI.Label(new Rect(165,96,145,15),(gamepadCombat?PadLabel(ControlAction.Resources):KeyLabel(ControlAction.Resources))+"  Resources",CardStyle(9,muted,TextAnchor.UpperRight));
   if(resourceCategory>=0)buttons.Clear();
   for(int i=0;i<6;i++){int group=i;Rect r=ResourceCategoryRect(i);bool open=resourceCategory==i;Box(r,new Color(.025f,.033f,.034f));CardOutline(r,open?hudLight:new Color(.24f,.26f,.24f));var icon=MaterialIcon(ResourceCategoryIcons[i]);if(icon)GUI.DrawTexture(new Rect(r.x+8,r.y+11,44,44),icon);
    GUI.Label(new Rect(r.x+62,r.y+7,r.width-69,30),ResourceCategories[i],CardStyle(12,open?hudLight:muted,bold:true));GUI.Label(new Rect(r.x+62,r.y+39,r.width-69,22),Math.Floor(Math.Max(0,ResourceCategoryStock(i))).ToString("N0"),CardStyle(16,parchment,bold:true));
    int index=buttons.Count;buttons.Add(()=>ToggleResourceCategory(group));if(resourceFocus&&resourceFocusCategory==i||!resourceFocus&&index==selection)CardOutline(r,hudLight,2);
   }
   HudLine(new Vector2(1280,35),new Vector2(1280,98),hudGold);
   if(resourceCategory>=0)ResourceDropdown();
  }
  void ResourceDropdown() {
   int group=resourceCategory;Rect r=ResourceDropdownRect(group);var rows=ResourceRows(group);HudFrame(r,true);GUI.Label(new Rect(r.x+17,r.y+12,365,30),ResourceCategories[group],CardStyle(21,hudLight,bold:true));GUI.Label(new Rect(r.xMax-29,r.y+11,20,23),"×",CardStyle(20,muted,TextAnchor.MiddleCenter));
   GUI.Label(new Rect(r.x+70,r.y+47,165,20),"MATERIAL",CardStyle(10,muted));GUI.Label(new Rect(r.x+241,r.y+47,80,20),"IN STOCK",CardStyle(10,muted,TextAnchor.UpperRight));GUI.Label(new Rect(r.x+334,r.y+47,105,20),"CURRENT RATE",CardStyle(10,muted,TextAnchor.UpperRight));
   for(int i=0;i<rows.Count;i++){var row=rows[i];float y=r.y+71+i*48;if(resourceFocus&&resourceFocusRow==i)Box(new Rect(r.x+8,y-2,r.width-16,43),new Color(.16f,.13f,.08f));var icon=MaterialIcon(row.icon);if(icon){GUI.color=row.locked?new Color(.5f,.5f,.5f):Color.white;GUI.DrawTexture(new Rect(r.x+17,y,36,36),icon);GUI.color=Color.white;}
    GUI.Label(new Rect(r.x+70,y+6,169,30),row.name,CardStyle(14,row.locked?muted:parchment));GUI.Label(new Rect(r.x+242,y+6,79,25),Math.Floor(Math.Max(0,row.stock)).ToString("N0"),CardStyle(15,parchment,TextAnchor.UpperRight));GUI.Label(new Rect(r.x+328,y+7,111,25),"+"+row.rate.ToString("0.###")+" /hr",CardStyle(13,row.rate>0?hudLight:muted,TextAnchor.UpperRight));
    HudLine(new Vector2(r.x+17,y+42),new Vector2(r.xMax-17,y+42),new Color(.13f,.15f,.14f));
   }
   string note=group==2?"Bars are refined at the Blacksmith; no passive output.":group==5?"Essence is earned in expeditions; no passive output.":group==0?"Rates per city hour · building production is accelerated.":"Rates per city hour (1 real minute), before bonuses.";
   GUI.Label(new Rect(r.x+17,r.yMax-24,r.width-34,20),note,CardStyle(11,muted));
  }
 }
}

