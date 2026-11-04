using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Veilbound
{
    public partial class VeilGame
    {
        readonly List<Action> buttons=new List<Action>();
        int selection, tab, cardFocus;
        float navigationCooldown;
        GUIStyle textStyle, titleStyle, smallStyle, buttonStyle;
        Texture2D white;
        readonly Color ink=new Color(.025f,.032f,.034f,.96f), panel=new Color(.055f,.065f,.065f,.96f), muted=new Color(.60f,.62f,.60f), parchment=new Color(.91f,.88f,.8f);
        void MenuInput()
        {
            if(state==ScreenState.Expedition) return;
            navigationCooldown-=Time.unscaledDeltaTime;
            var k=Keyboard.current; var p=Gamepad.current;
            bool next=k?.tabKey.wasPressedThisFrame==true||k?.downArrowKey.wasPressedThisFrame==true||k?.rightArrowKey.wasPressedThisFrame==true||p?.dpad.down.wasPressedThisFrame==true||p?.dpad.right.wasPressedThisFrame==true;
            bool prev=k?.upArrowKey.wasPressedThisFrame==true||k?.leftArrowKey.wasPressedThisFrame==true||p?.dpad.up.wasPressedThisFrame==true||p?.dpad.left.wasPressedThisFrame==true;
            if(p!=null && navigationCooldown<=0)
            {
                Vector2 axis=p.leftStick.ReadValue(); if(axis.y<-.6f||axis.x>.6f) next=true; else if(axis.y>.6f||axis.x<-.6f) prev=true;
                if(next||prev) navigationCooldown=.22f;
            }
            if(next||prev)upgradePointerFocus=false;
            if(buttons.Count>0)
            {
                if(next) selection=(selection+1)%buttons.Count;
                if(prev) selection=(selection+buttons.Count-1)%buttons.Count;
                if(k?.enterKey.wasPressedThisFrame==true||p?.buttonSouth.wasPressedThisFrame==true) buttons[Mathf.Clamp(selection,0,buttons.Count-1)]();
            }
            if(state==ScreenState.Choice && k!=null)
            { if(k.digit1Key.wasPressedThisFrame) Choose(0); else if(k.digit2Key.wasPressedThisFrame) Choose(1); else if(k.digit3Key.wasPressedThisFrame) Choose(2); }
        }
        void InitUI()
        {
            if(white) return;
            white=Texture2D.whiteTexture;
            textStyle=new GUIStyle(GUI.skin.label){fontSize=20,normal={textColor=parchment},wordWrap=true};
            titleStyle=new GUIStyle(textStyle){fontSize=42,fontStyle=FontStyle.Bold};
            smallStyle=new GUIStyle(textStyle){fontSize=16,normal={textColor=muted}};
            buttonStyle=new GUIStyle(textStyle){alignment=TextAnchor.MiddleCenter,fontSize=18,fontStyle=FontStyle.Bold};
        }
        void Box(Rect r,Color c) { GUI.color=c; GUI.DrawTexture(r,white); GUI.color=Color.white; }
        void Label(float x,float y,float w,float h,string s,GUIStyle style=null) { GUI.Label(new Rect(x,y,w,h),s,style??textStyle); }
        void Rule(float x,float y,float w) { Box(new Rect(x,y,w,1),new Color(.45f,.34f,.21f)); }
        void Button(float x,float y,float w,float h,string title,Action action,bool enabled=true)
        {
            if(state==ScreenState.Settlement&&tab==2){CityButton(x,y,w,h,title,action,enabled);return;}
            var rect=new Rect(x,y,w,h); int id=buttons.Count;
            if(enabled) buttons.Add(action);
            bool focused=enabled&&selection==id;
            Box(rect,enabled?(focused?new Color(.19f,.145f,.085f):new Color(.065f,.078f,.078f)):new Color(.07f,.075f,.1f));
            CardOutline(rect,focused?new Color(.65f,.50f,.29f):new Color(.25f,.27f,.25f));
            if(focused) { Box(new Rect(x,y,3,h),WorldArt.Gold); Box(new Rect(x,y+h-2,w,2),WorldArt.Gold); }
            GUI.color=enabled?Color.white:new Color(.45f,.45f,.48f);
            GUI.Label(rect,title,buttonStyle); GUI.color=Color.white;
            if(enabled&&Event.current.type==EventType.MouseDown&&rect.Contains(Event.current.mousePosition))
            { selection=id; action(); Event.current.Use(); }
        }
        void Bar(float x,float y,float w,float h,float amount,Color c)
        { Box(new Rect(x,y,w,h),new Color(.1f,.1f,.15f)); Box(new Rect(x,y,w*Mathf.Clamp01(amount),h),c); }
        void OnGUI()
        {
            InitUI(); buttons.Clear();
            float scale=Mathf.Min(Screen.width/1600f,Screen.height/900f);
            GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-1600*scale)/2,(Screen.height-900*scale)/2,0),Quaternion.identity,Vector3.one*scale);
            if(state==ScreenState.Settlement){ResourceBarInput();Hub();if(state==ScreenState.Settlement)ResourceTopBar();} else if(state!=ScreenState.Choice&&state!=ScreenState.Character&&state!=ScreenState.Settings&&state!=ScreenState.CharacterSelect) HUD();
            if(state==ScreenState.Choice) ChoiceScreen();
            if(state==ScreenState.Paused) PauseScreen();
            if(state==ScreenState.Result) Results();
            if(state==ScreenState.Character) CharacterSheet();
            if(state==ScreenState.Settings)SettingsScreen();
            if(state==ScreenState.CharacterSelect)CharacterSelectionScreen();
            GUI.matrix=Matrix4x4.identity;
        }
        string Clock(float seconds) => $"{(int)seconds/60:00}:{(int)seconds%60:00}";
        string CityClock => $"DAY {(int)city.hours/24+1}   {(int)city.hours%24:00}:00";
        string SwordName(int n)=>city.WeaponName;
        string ArmourName(int n)=>n==0?"Wayfarer's coat":n==1?"Ironbound leathers":"Runebound plate";
        void ProductionRow(float y,string name,int n,string rate,string kind)
        {
            Label(52,y,630,30,$"{name}  {n}  /  {rate}");
            Button(52,y+42,648,48,$"UPGRADE  •  {35*(n+1)} timber / {15*(n+1)} iron",()=>Buy(kind));
        }
        void HUD()
        {
            float scale=Mathf.Min(Screen.width/1600f,Screen.height/900f);
            foreach(Foe f in foes) if(f.boss==0 && !f.dead && f.hitTimer>0)
            {
                Vector3 screen=cameraRig.WorldToScreenPoint(f.t.position+Vector3.up*(f.kind==3&&f.boss==0?1.05f:2.25f*f.t.localScale.y));
                if(screen.z<=0)continue;
                float x=(screen.x-(Screen.width-1600*scale)/2)/scale;
                float y=(Screen.height-screen.y-(Screen.height-900*scale)/2)/scale;
                Box(new Rect(x-31,y-3,62,11),ink);
                Bar(x-28,y,56,5,f.hp/f.maxHp,new Color(.78f,.16f,.10f));
            }
            ExpeditionIdentity();ExpeditionTimeline();
            GlobeDock();AbilityBar();DrawFloatingDamage();CompactDamageMeter();HudSheetControls();ExperienceBar();
            if(state==ScreenState.Expedition)Button(1300,160,276,36,"Settings",OpenSettings);
            foreach(Foe f in foes) if(f.boss>0)
            {
                float y=141+(f.boss-1)*53; HudFrame(new Rect(550,y,500,46));
                Label(563,y+5,480,23,BossName(f.boss),CardStyle(12,new Color(.89f,.74f,.47f),TextAnchor.UpperCenter,true)); Bar(563,y+31,474,5,f.hp/f.maxHp,new Color(.78f,.35f,.2f));
            }
        }
        void Overlay() { Box(new Rect(0,0,1600,900),new Color(.015f,.02f,.04f,.9f)); }
        void PauseScreen()
        {
            Overlay(); Label(480,160,700,62,"BETWEEN HEARTBEATS",titleStyle);
            Label(480,239,650,45,"Combat and settlement production are paused.",smallStyle);
            Button(480,320,640,62,"RESUME",()=>SetState(ScreenState.Expedition));
            Button(480,406,640,62,"DODGE DIRECTION  /  "+(dodgeFacing?"FACING":"MOVEMENT"),()=>dodgeFacing=!dodgeFacing);
            Button(480,492,640,62,"SETTINGS",OpenSettings);
            Button(480,578,640,50,"RETURN TO SETTLEMENT",SettingsReturnToSettlement);
            Button(480,641,640,42,"EXIT GAME",ExitGame);
            Label(480,704,650,108,"Movement: mapped keys / left stick. Aim: mouse / right stick.\nDodge: "+KeyLabel(ControlAction.Dodge)+" / "+PadLabel(ControlAction.Dodge)+" or "+PadLabel(ControlAction.DodgeAlt)+". Rage: "+KeyLabel(ControlAction.Virtue)+" / "+PadLabel(ControlAction.Virtue)+".\nAttacks are automatic. Rage requires a full globe.\nRetreat keeps production and essence already earned.",smallStyle);
        }
        void Results()
        {
            Overlay(); Label(270,168,1100,65,resultTitle,titleStyle);
            Label(274,270,1000,52,$"{Clock(elapsed)} survived   •   {kills} defeated   •   {bossesDead}/3 aberrations");
            Rule(274,351,1046);
            Label(274,388,1046,100,$"THE CITY KEPT WORKING\n+{city.wood-startWood:0.0} timber     +{city.stone-startStone:0.0} stone     +{Math.Floor(city.ResourceTotal(0)-startOreUnits):0} ore\n+{Math.Floor(city.ResourceTotal(2)-startTextileUnits):0} textiles     +{Math.Floor(city.ResourceTotal(1)-startHideUnits):0} hides     +{earnedEssence:0} essence");
            Label(274,515,1046,80,"Loadout and settlement upgrades persist. Expedition powers fade when you return.\nOnly time actually spent in active play produced these resources.",smallStyle);
            Button(274,646,1046,70,"RETURN TO THE SETTLEMENT",ReturnHome);
        }
    }
}





