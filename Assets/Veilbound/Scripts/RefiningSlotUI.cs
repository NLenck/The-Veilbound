using System;
using UnityEngine;
namespace Veilbound {public partial class VeilGame {
 void OpenBarSlotResearch(){producerOpen=false;sanctuaryWorkshopOpen=false;libraryResearchOpen=true;SelectResearchTab(1);selectedResearch="blacksmith-slots";selectedResearchTier=Math.Min(3,city.ResearchRank(selectedResearch)+1);researchScroll.y=Mathf.Clamp((selectedResearchTier*3-3)*88,0,510);selection=0;}
 void RefiningSlotsScreen(){DrawProcessingPage(city.ProcessingLine(3),true);}
}}
