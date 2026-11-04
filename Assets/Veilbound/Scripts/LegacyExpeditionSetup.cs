using System;
using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  void LegacyExpeditionSetupPanel() {
   GUI.Label(new Rect(52,249,810,43),"Choose your expedition",CardStyle(31,parchment,bold:true));RandomDestinationNotice();if(characterProfilesEnabled)Button(710,249,174,40,"Switch Character",OpenCharacterSelection);
   HudFrame(new Rect(52,361,832,100));GUI.Label(new Rect(124,376,78,20),"TIER",CardStyle(11,hudLight,TextAnchor.UpperCenter,true));GUI.Label(new Rect(124,400,78,43),tier.ToString(),CardStyle(32,parchment,TextAnchor.MiddleCenter,true));
   Button(68,400,43,39,"‹",()=>SelectExpeditionTier(tier-1),tier>1);Button(221,400,43,39,"›",()=>SelectExpeditionTier(tier+1),tier<HighestExpeditionTier);
   GUI.Label(new Rect(314,379,360,25),"Highest unlocked: Tier "+HighestExpeditionTier,CardStyle(17,parchment,bold:true));GUI.Label(new Rect(314,413,360,23),"SETTLEMENT PRODUCTION  ×"+Multiplier.ToString("0.0"),CardStyle(13,hudLight));Button(718,399,148,39,"Highest",SelectHighestExpeditionTier,tier!=HighestExpeditionTier);
   DurationOption(new Rect(52,479,408,67),false,"Standard","15 minutes  ·  Bosses at 5 / 10 / 15 min");DurationOption(new Rect(476,479,408,67),true,"Trial","3 minutes  ·  Bosses at 1 / 2 / 3 min");
   GUI.Label(new Rect(52,568,430,23),"PROJECTED GATHERING",CardStyle(13,hudLight,bold:true));GUI.Label(new Rect(506,569,378,23),"Estimate for a completed run",CardStyle(12,muted,TextAnchor.UpperRight));
   string[] icons={"timber","stone","group-ore","group-textiles","group-hides"},names={"Timber","Stone","Ore","Textiles","Hides"};
   for(int i=0;i<5;i++){Rect r=new Rect(52+i*169,603,156,57);Box(r,new Color(.035f,.043f,.044f));CardOutline(r,new Color(.24f,.25f,.22f));var icon=MaterialIcon(icons[i]);if(icon)GUI.DrawTexture(new Rect(r.x+9,r.y+10,36,36),icon);GUI.Label(new Rect(r.x+54,r.y+7,96,19),names[i],CardStyle(11,muted));GUI.Label(new Rect(r.x+54,r.y+27,96,25),ProjectedGathering(i).ToString(i<2?"N0":"0.##"),CardStyle(17,parchment,bold:true));}
   GUI.Label(new Rect(52,673,832,24),"Your loadout is fixed when the expedition begins.",CardStyle(12,muted));Button(52,713,832,61,"ENTER THE VEIL  →",StartRun);
  }
  void DurationOption(Rect r,bool trial,string title,string detail){Button(r.x,r.y,r.width,r.height,"",()=>shortRun=trial);bool active=shortRun==trial;CardOutline(r,active?hudLight:new Color(.25f,.27f,.25f),active?2:1);GUI.Label(new Rect(r.x+18,r.y+10,r.width-36,26),title,CardStyle(20,active?hudLight:parchment,bold:true));GUI.Label(new Rect(r.x+18,r.y+41,r.width-36,21),detail,CardStyle(12,muted));}
 }
}
