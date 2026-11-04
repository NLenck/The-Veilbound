#if UNITY_EDITOR
using System.IO;
namespace Veilbound.Editor {
 public static class LandscapePolishValidation {
  public static void Build(){
   Directory.CreateDirectory("LandscapePolish/Review");
   GraveyardValidation.Run();Copy("GraveyardDesign/Implementation","shrine");
   VillageValidation.Run();Copy("VillageDesign/Implementation","village");
   ProductionModelImport.Build();
  }
  static void Copy(string folder,string prefix){
   foreach(var file in Directory.GetFiles(folder))if((file.EndsWith(".png")&&Path.GetFileName(file).StartsWith("0"))||Path.GetFileName(file)=="validation.json")File.Copy(file,"LandscapePolish/Review/"+prefix+"-"+Path.GetFileName(file),true);
  }
 }
}
#endif
