using System;
using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  int HighestExpeditionTier {get{city.EnsureExpeditionModifiers();return city.ExpeditionColumns*10;}}
  void SelectHighestExpeditionTier(){expeditionModifiersActive=false;city.EnsureExpeditionModifiers();tier=Math.Max(1,VeilDepth);}
  void SelectExpeditionTier(int target){SelectHighestExpeditionTier();int remaining=Mathf.Clamp(target,0,HighestExpeditionTier);for(int i=0;i<10;i++){city.expeditionRanks[i]=Math.Min(city.ExpeditionColumns,remaining);remaining-=city.expeditionRanks[i];}tier=Math.Max(1,VeilDepth);}
  double ProjectedGathering(int category){double hours=Duration/60d*Multiplier;if(category==2)return city.ProjectedMineUnits(hours);return hours*(category==0?city.WoodRate:category==1?city.StoneRate:category==3?city.TotalProduction(2):city.TotalProduction(1));}
  readonly Color[] modifierColours={new Color(.92f,.19f,.24f),new Color(.94f,.48f,.16f),new Color(.15f,.72f,.94f),new Color(.77f,.29f,.94f),new Color(.15f,.8f,.77f),new Color(.5f,.78f,.2f),new Color(.98f,.66f,.16f),new Color(.2f,.79f,.92f),new Color(.62f,.36f,.9f),new Color(.94f,.22f,.51f)};
  void ExpeditionText(float x,float y,float w,float h,string text,int size=15,Color? colour=null,TextAnchor align=TextAnchor.UpperLeft,bool bold=false){var style=CardStyle(size,colour??parchment,align,bold);if(bold&&expeditionSerif)style.font=expeditionSerif;GUI.Label(new Rect(x,y,w,h),text,style);}
  void ExpeditionFrame(Rect r){Box(new Rect(r.x+7,r.y,r.width-14,r.height),new Color(.025f,.055f,.063f,.97f));Box(new Rect(r.x,r.y+7,r.width,r.height-14),new Color(.025f,.055f,.063f,.97f));ExpeditionBevel(r,new Color(.43f,.46f,.39f));ExpeditionBevel(new Rect(r.x+4,r.y+4,r.width-8,r.height-8),new Color(.12f,.19f,.19f),6);}
  void ExpeditionSetupPanel(){
   city.EnsureExpeditionModifiers();InitExpeditionArt();
   if(expeditionBackdrop)GUI.DrawTexture(new Rect(0,0,1600,900),expeditionBackdrop,ScaleMode.ScaleAndCrop);else Box(new Rect(0,0,1600,900),ink);
   var expeditionMatrix=GUI.matrix;GUI.matrix=expeditionMatrix*Matrix4x4.TRS(new Vector3(0,115,0),Quaternion.identity,new Vector3(1,.85f,1));
   Box(new Rect(0,0,1600,112),new Color(.01f,.02f,.025f,.35f));
   Button(28,28,50,50,"",()=>{tab=2;selection=0;});HudDiamond(new Vector2(53,53),34,hudLight);HudLine(new Vector2(37,52),new Vector2(53,36),hudLight,3);HudLine(new Vector2(53,36),new Vector2(69,52),hudLight,3);HudLine(new Vector2(42,49),new Vector2(42,67),hudLight,3);HudLine(new Vector2(64,49),new Vector2(64,67),hudLight,3);HudLine(new Vector2(42,67),new Vector2(64,67),hudLight,3);CardOutline(new Rect(50,56,7,11),hudLight,2);
   ExpeditionText(100,23,620,55,"EXPEDITION",42,parchment,bold:true);
   ExpeditionText(102,80,720,25,"Customise your Veil Depth by selecting modifiers",16,muted);
   Button(850,30,190,44,"CHARACTERS",OpenCharacterSelection,characterProfilesEnabled);
   ExpeditionActionButton(new Rect(1052,30,190,44),"LOADOUT",()=>{tab=1;selection=0;});ExpeditionActionButton(new Rect(1254,30,224,44),"SETTLEMENT",()=>{tab=2;selection=0;});
   Button(1512,24,48,48,"×",()=>{tab=2;selection=0;});
   ExpeditionFrame(new Rect(28,125,286,623));ExpeditionFrame(new Rect(326,125,856,623));ExpeditionFrame(new Rect(1194,125,378,623));
   ExpeditionText(48,145,246,25,"V E I L  D E P T H",17,hudLight,TextAnchor.MiddleCenter,true);
   ExpeditionSeal(new Rect(121,183,100,65));ExpeditionText(48,174,246,76,VeilDepth.ToString(),46,new Color(.95f,.84f,.59f),TextAnchor.MiddleCenter,true);Rule(48,261,246);
   ExpeditionText(48,278,246,23,"TIME DILATION",14,hudLight,bold:true);ExpeditionText(48,307,246,45,"+"+((Multiplier-1)*100).ToString("0")+"%",32,hudLight,bold:true);
   ExpeditionText(48,354,246,25,Multiplier.ToString("0.00")+"× city time",15,muted);Rule(48,392,246);
   ExpeditionText(48,409,246,24,"PROJECTED TOWN YIELD",14,hudLight,bold:true);
   string[] icons={"timber","stone","group-ore","group-textiles","group-hides"},names={"Timber","Stone","Ore","Textiles","Hides"};
   for(int i=0;i<5;i++){float y=448+i*43;var icon=MaterialIcon(icons[i]);if(icon)GUI.DrawTexture(new Rect(48,y,30,30),icon);ExpeditionText(86,y+5,125,28,names[i],15,muted);ExpeditionText(210,y+3,82,30,ProjectedGathering(i).ToString("0.##"),18,hudLight,TextAnchor.MiddleRight);}
   ExpeditionText(48,679,246,51,"Completed-run estimate. Actual production follows active city time and queued orders.",12,muted);
   string[] roman={"I","II","III","IV","V","VI","VII","VIII","IX","X"};
   ExpeditionText(344,144,208,32,"MODIFIERS",14,hudLight,bold:true);
   HudLine(new Vector2(566,158),new Vector2(1155,158),new Color(.29f,.35f,.31f));
   for(int c=0;c<10;c++){HudDiamond(new Vector2(590+c*59,158),20,c<city.ExpeditionColumns?hudLight:new Color(.23f,.31f,.31f));ExpeditionText(564+c*59,140,53,36,roman[c],17,c<city.ExpeditionColumns?hudLight:muted,TextAnchor.MiddleCenter,true);}
   for(int row=0;row<10;row++){
    int r=row;float y=188+row*47;bool focus=modifierFocus==row;Color colour=modifierColours[row];
    Button(342,y,213,42,"",()=>modifierFocus=r);ExpeditionIcon(new Rect(346,y+3,37,36),row,new Color(1,.83f,.5f));ExpeditionText(389,y+3,161,38,ExpeditionModifiers.Names[row],13,focus?parchment:muted,TextAnchor.MiddleLeft,focus);
    for(int col=0;col<10;col++){
     int rank=col+1;bool unlocked=col<city.ExpeditionColumns,on=city.expeditionRanks[row]>=rank;Rect cell=new Rect(564+col*59,y,53,42);
     Button(cell.x,cell.y,cell.width,cell.height,unlocked?"":"",()=>SetModifierRank(r,city.expeditionRanks[r]==rank?rank-1:rank),unlocked);
     if(on){Box(new Rect(cell.x+3,cell.y+3,47,36),new Color(colour.r*.23f,colour.g*.23f,colour.b*.23f,1));ExpeditionBevel(cell,colour,5);}
     if(unlocked)ExpeditionIcon(new Rect(cell.x+7,cell.y+3,39,36),row,on?Color.white:new Color(.2f,.27f,.28f));
     else {Box(new Rect(cell.x+22,cell.y+19,10,11),new Color(.22f,.29f,.31f));CardOutline(new Rect(cell.x+23,cell.y+12,8,12),new Color(.22f,.29f,.31f),2);}
    }
   }
   int needed=city.ExpeditionColumns==10?100:5+(city.ExpeditionColumns-1)*10;
   ExpeditionText(346,673,814,29,city.ExpeditionColumns==10?"All columns unlocked · Best clear: "+city.expeditionBestDepth:"Clear at Depth "+needed+" to unlock Column "+roman[city.ExpeditionColumns]+" · Best clear: "+city.expeditionBestDepth,14,hudLight,TextAnchor.MiddleCenter);
   ExpeditionText(346,710,814,24,"Each filled cell = 1 Veil Depth. Select a filled cell again to remove that rank.",12,muted,TextAnchor.MiddleCenter);
   int selected=city.expeditionRanks[modifierFocus];Color accent=modifierColours[modifierFocus];
   ExpeditionText(1216,145,330,25,"MODIFIER DETAILS",17,hudLight,bold:true);Rule(1216,180,330);
   ExpeditionIcon(new Rect(1216,195,49,48),modifierFocus,Color.white);ExpeditionText(1277,197,267,49,ExpeditionModifiers.Names[modifierFocus],22,parchment,bold:true);
   ExpeditionText(1216,252,330,27,"Rank "+selected+" / 10 · "+city.ExpeditionColumns+" available",15,muted);
   Button(1216,292,46,38,"−",()=>SetModifierRank(modifierFocus,selected-1),selected>0);Button(1500,292,46,38,"+",()=>SetModifierRank(modifierFocus,selected+1),selected<city.ExpeditionColumns);
   for(int rank=0;rank<10;rank++){var center=new Vector2(1280+rank*21,311);HudDiamond(center,9,rank<selected?accent:new Color(.22f,.3f,.3f));if(rank<selected)HudDiamond(center,5,accent);}
   ExpeditionText(1216,337,330,29,"+"+(selected*ExpeditionModifiers.Percent[modifierFocus])+"% "+ExpeditionModifiers.Names[modifierFocus].ToLower(),17,hudLight,bold:true);
   string[] descriptions={"Increases normal enemy maximum health.","Increases all damage dealt by normal enemies.","Increases boss maximum health.","Increases all damage dealt by bosses.","Reduces time between boss attacks, up to 50%.","Increases the rate at which enemies emerge.","Brutes and Bulwarks appear more often in eligible waves. Existing caps still apply.","Increases normal enemy movement speed.","Speeds up normal enemy attack cooldowns and melee / lob casts.","Increases Brute and Bulwark health, in addition to Enemy Health."};
   ExpeditionText(1216,372,330,39,descriptions[modifierFocus],13,muted);Rule(1216,415,330);
   ExpeditionText(1216,431,330,24,"RUN SUMMARY",16,hudLight,bold:true);
   for(int i=0;i<10;i++){float y=470+i*23;ExpeditionText(1216,y,260,23,ExpeditionModifiers.Names[i],12,muted);ExpeditionText(1473,y,73,23,RunModifierSummary(i),14,Mathf.Abs(RunModifierFactor(i)-1)>.0001f?hudLight:muted,TextAnchor.UpperRight);}
   ExpeditionText(1216,704,330,19,"Totals = Veil Depth base × selected modifiers",11,hudLight);
   ExpeditionText(1216,724,330,18,"Elite health also includes Enemy Health",10,muted);
   ExpeditionText(32,764,550,42,"STANDARD EXPEDITION · 15 MINUTES",18,hudLight,TextAnchor.MiddleLeft,true);
   ExpeditionActionButton(new Rect(970,764,200,42),"CLEAR RANKS",()=>{for(int i=0;i<10;i++)city.expeditionRanks[i]=0;tier=1;Save();});
   ExpeditionActionButton(new Rect(1194,764,378,58),"START EXPEDITION",StartRun,21);
   ExpeditionText(30,840,1260,30,notice,13,muted);
   ExpeditionText(30,872,1530,22,"Random destination · Loadout locks when the expedition begins",12,muted);
   GUI.matrix=expeditionMatrix;
  }
 }
}
