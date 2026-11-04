using UnityEditor;using UnityEngine;
namespace Veilbound.Editor {
 public class TownLayerImport:AssetPostprocessor {
  void OnPreprocessTexture(){if(!assetPath.StartsWith("Assets/Veilbound/Resources/Sanctuary/TownProgression/Layers/"))return;Configure((TextureImporter)assetImporter);}
  static void Configure(TextureImporter t){t.textureType=TextureImporterType.Default;t.alphaSource=TextureImporterAlphaSource.FromInput;t.alphaIsTransparency=true;t.mipmapEnabled=true;t.filterMode=FilterMode.Trilinear;t.anisoLevel=4;t.wrapMode=TextureWrapMode.Clamp;t.npotScale=TextureImporterNPOTScale.None;t.maxTextureSize=4096;t.textureCompression=TextureImporterCompression.Uncompressed;t.isReadable=t.assetPath.EndsWith("/housing.png")||t.assetPath.EndsWith("/home.png");}
  public static void Build(){foreach(string guid in AssetDatabase.FindAssets("t:Texture2D",new[]{"Assets/Veilbound/Resources/Sanctuary/TownProgression"})){string path=AssetDatabase.GUIDToAssetPath(guid);var t=AssetImporter.GetAtPath(path) as TextureImporter;if(t==null)continue;Configure(t);t.SaveAndReimport();}PrototypeBuild.Build();}
 }
}

