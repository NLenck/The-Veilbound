namespace Veilbound {
 public partial class VeilGame {
  void SettingsReturnToSettlement() {
   if(settingsReturn==ScreenState.CharacterSelect){CloseSettings();return;}
   captureAction=-1;SavePreferences();CloseGearBag();LeaveResourceBar();
   if(state!=ScreenState.Settlement)EndRun(false);
   ReturnHome();tab=0;selection=0;settingsReturn=ScreenState.Settlement;
   notice="Returned to settlement. Earned resources and equipment are saved.";
  }
  void SaveBeforeExit(){Save();SavePreferences();}
  void ExitGame(){SaveBeforeExit();UnityEngine.Application.Quit(0);}
 }
}
