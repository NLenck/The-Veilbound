using System.Collections.Generic;using UnityEngine;using UnityEngine.InputSystem;
namespace Veilbound {public partial class VeilGame {
 struct MenuFocusControl {public int index;public Rect rect;public string title;public bool enabled;}
 readonly List<MenuFocusControl> menuFocusControls=new List<MenuFocusControl>();
 int menuFocusSuppression;bool menuControllerActive;bool menuNavigationActive=true;int menuNavigationFrame=-100;Vector2 menuPointer;bool menuPointerKnown;int menuFocusMarkerCount;string menuFocusMarkerTitle;Rect menuFocusMarkerRect;
 void ObserveMenuNavigationInput(){
  var k=Keyboard.current;var p=PreferredPad;
  bool keyboard=k!=null&&(k.tabKey.wasPressedThisFrame||k.enterKey.wasPressedThisFrame||k.escapeKey.wasPressedThisFrame||k.upArrowKey.wasPressedThisFrame||k.downArrowKey.wasPressedThisFrame||k.leftArrowKey.wasPressedThisFrame||k.rightArrowKey.wasPressedThisFrame);
  bool controller=p!=null&&(p.dpad.ReadValue().sqrMagnitude>.1f||p.leftStick.ReadValue().sqrMagnitude>.36f||p.buttonSouth.wasPressedThisFrame||p.buttonEast.wasPressedThisFrame||p.buttonWest.wasPressedThisFrame||p.buttonNorth.wasPressedThisFrame||p.leftShoulder.wasPressedThisFrame||p.rightShoulder.wasPressedThisFrame||p.rightStickButton.wasPressedThisFrame||p.startButton.wasPressedThisFrame||p.selectButton.wasPressedThisFrame);
  if(k!=null)foreach(var key in k.allKeys)if(key.wasPressedThisFrame)menuControllerActive=false;
  if(controller)menuControllerActive=true;
  if(p==null)menuControllerActive=false;
  if(keyboard||controller){menuNavigationActive=true;menuNavigationFrame=Time.frameCount;}
  var mouse=Mouse.current;if(mouse!=null){Vector2 point=mouse.position.ReadValue();if(menuPointerKnown&&((point-menuPointer).sqrMagnitude>4||mouse.leftButton.wasPressedThisFrame||mouse.scroll.ReadValue().sqrMagnitude>0)&&menuNavigationFrame!=Time.frameCount){menuNavigationActive=false;menuControllerActive=false;}menuPointer=point;menuPointerKnown=true;}
 }
 bool GenericMenuFocusAllowed {
  get {
   if(state==ScreenState.Expedition)return false;
   if(InventoryScreenOpen&&!resourceFocus&&resourceCategory<0)return false;
   if(state==ScreenState.Settlement&&(resourceFocus||resourceCategory>=0||settlementQueueOpen&&queuePicker!=null||!settlementQueueOpen&&(SmithScreenOpen||ClothierScreenOpen)&&workshopCostInspect))return false;
   if(state==ScreenState.Choice||state==ScreenState.Character&&characterTab==1)return false;
   if(state==ScreenState.Settlement&&!settlementQueueOpen){if(libraryResearchOpen)return false;if(SmithScreenOpen&&city.Crafter("blacksmith").Settings.pages[smithPage].kind==WorkshopPageKind.Equipment)return false;if(ClothierScreenOpen&&city.Crafter("clothier").Settings.pages[clothierPage].kind==WorkshopPageKind.Equipment)return false;}
   return true;
  }
 }
 void RegisterMenuFocus(Rect rect,int index,string title,bool enabled){
  menuFocusControls.Add(new MenuFocusControl{index=enabled?index:-1,rect=rect,title=title,enabled=enabled});
  if(menuFocusSuppression==0&&enabled&&selection==index&&GenericMenuFocusAllowed)DrawMenuNavigationFocus(rect,title);
 }
 void DrawMenuNavigationFocus(Rect rect,string title=""){
  if(!menuNavigationActive)return;menuFocusMarkerCount++;menuFocusMarkerTitle=title;menuFocusMarkerRect=rect;
  DrawMenuFocusGlow(rect);
 }
 Texture2D menuFocusGlow;
 void DrawMenuFocusGlow(Rect rect){
  if(!menuFocusGlow){
   const int size=128;menuFocusGlow=new Texture2D(size,size,TextureFormat.RGBA32,false){name="Menu focus / soft yellow glow",filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp,hideFlags=HideFlags.HideAndDontSave};var pixels=new Color[size*size];
   for(int y=0;y<size;y++)for(int x=0;x<size;x++){
    // Rounded frame with a broad, feathered halo and a gentle inner warmth.
    Vector2 q=new Vector2(Mathf.Abs(x+.5f-64)-32,Mathf.Abs(y+.5f-64)-32);float distance=new Vector2(Mathf.Max(q.x,0),Mathf.Max(q.y,0)).magnitude+Mathf.Min(Mathf.Max(q.x,q.y),0)-8;
    float alpha=.55f*Mathf.Exp(-distance*distance/18)+.36f*Mathf.Exp(-distance*distance/162)+(distance<0?.055f:0);pixels[y*size+x]=new Color(1,.80f,.12f,Mathf.Clamp01(alpha));
   }
   menuFocusGlow.SetPixels(pixels);menuFocusGlow.Apply(false,true);
  }
  Rect area=new Rect(rect.x-8,rect.y-8,rect.width+16,rect.height+16);float edge=Mathf.Min(14,Mathf.Min(area.width,area.height)*.48f);float[] dx={area.x,area.x+edge,area.xMax-edge,area.xMax},dy={area.y,area.y+edge,area.yMax-edge,area.yMax},uv={0,40f/128,88f/128,1};
  Color prior=GUI.color;GUI.color=Color.white;for(int y=0;y<3;y++)for(int x=0;x<3;x++)GUI.DrawTextureWithTexCoords(new Rect(dx[x],dy[y],dx[x+1]-dx[x],dy[y+1]-dy[y]),menuFocusGlow,new Rect(uv[x],1-uv[y+1],uv[x+1]-uv[x],uv[y+1]-uv[y]),true);GUI.color=prior;
 }
 void KeepMenuFocusVisible(ref Vector2 scroll,int first,int count,float rowHeight,float viewportHeight){
  if(!menuNavigationActive||!GenericMenuFocusAllowed)return;int relative=selection-first;if(relative<0||relative>=count)return;float top=relative*rowHeight,bottom=top+rowHeight;
  if(top<scroll.y)scroll.y=top;else if(bottom>scroll.y+viewportHeight)scroll.y=bottom-viewportHeight;
 }
 void KeepReorderFocusVisible(ref Vector2 scroll,int first,int rows,float rowHeight,float viewportHeight){
  if(!menuNavigationActive||!GenericMenuFocusAllowed||rows==0)return;int offset=selection-first,total=rows==1?1:rows*3-2;if(offset<0||offset>=total)return;
  int row=rows==1||offset<2?0:offset>=2+(rows-2)*3?rows-1:1+(offset-2)/3;
  float top=row*rowHeight,bottom=top+rowHeight;if(top<scroll.y)scroll.y=top;else if(bottom>scroll.y+viewportHeight)scroll.y=bottom-viewportHeight;
 }

}}
