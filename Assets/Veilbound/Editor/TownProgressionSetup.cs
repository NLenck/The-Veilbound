#if UNITY_EDITOR
using System;using System.IO;using UnityEditor;using UnityEngine;
namespace Veilbound.Editor {
 public static class TownProgressionSetup {
  public static void Build(){
   AssetDatabase.Refresh();foreach(string file in Directory.GetFiles("Assets/Veilbound/Resources/Sanctuary/TownProgression","*.png")){
    var t=AssetImporter.GetAtPath(file) as TextureImporter;if(t==null)throw new Exception("Missing town texture "+file);t.textureType=TextureImporterType.Default;t.alphaSource=TextureImporterAlphaSource.FromInput;t.alphaIsTransparency=true;t.mipmapEnabled=false;t.wrapMode=TextureWrapMode.Clamp;t.filterMode=FilterMode.Bilinear;t.npotScale=TextureImporterNPOTScale.None;t.maxTextureSize=2048;t.textureCompression=TextureImporterCompression.Uncompressed;t.SaveAndReimport();
   }
   var camp=Settlement.NewCamp();if(camp.WoodRate*24!=10||camp.StoneRate!=0||camp.mine!=0||camp.housingTier!=0)throw new Exception("Invalid new camp");for(int i=1;i<=10;i++)if(VeilGame.TownArtStage(i)<0||VeilGame.TownArtStage(i)>5)throw new Exception("Invalid art stage");
   AssetDatabase.SaveAssets();ProductionModelImport.Build();
  }
 }
}
#endif
