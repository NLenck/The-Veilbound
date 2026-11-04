using UnityEditor;using UnityEngine;
namespace Veilbound.Editor {public class TownRecoveryImport:AssetPostprocessor {
 void OnPreprocessTexture(){if(assetPath.Contains("/TownProgression/Layers/Recovery/"))Set((TextureImporter)assetImporter);}
 static void Set(TextureImporter t){t.textureType=TextureImporterType.Default;t.alphaSource=TextureImporterAlphaSource.FromInput;t.alphaIsTransparency=true;t.mipmapEnabled=true;t.filterMode=FilterMode.Trilinear;t.anisoLevel=4;t.wrapMode=TextureWrapMode.Clamp;t.npotScale=TextureImporterNPOTScale.None;t.maxTextureSize=4096;t.textureCompression=TextureImporterCompression.Uncompressed;t.isReadable=t.assetPath.EndsWith("/housing.png")||t.assetPath.EndsWith("/home.png");}
 public static void Build(){foreach(string guid in AssetDatabase.FindAssets("t:Texture2D",new[]{"Assets/Veilbound/Resources/Sanctuary/TownProgression/Layers/Recovery"})){var t=AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid)) as TextureImporter;if(t!=null){Set(t);t.SaveAndReimport();}}PrototypeBuild.Build();}
}}
