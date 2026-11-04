using UnityEngine;using UnityEditor;
namespace Veilbound.Editor {public class HousingReferenceImport:AssetPostprocessor {
 public override int GetPostprocessOrder()=>1000;
 void OnPreprocessTexture(){if(assetPath.Contains("/HousingRefs/"))Configure((TextureImporter)assetImporter);}
 static void Configure(TextureImporter t){t.textureType=TextureImporterType.Default;t.alphaSource=TextureImporterAlphaSource.FromInput;t.alphaIsTransparency=true;t.mipmapEnabled=true;t.filterMode=FilterMode.Trilinear;t.anisoLevel=4;t.wrapMode=TextureWrapMode.Clamp;t.npotScale=TextureImporterNPOTScale.None;t.textureCompression=TextureImporterCompression.Uncompressed;t.maxTextureSize=4096;t.isReadable=true;}
 public static void Build(){foreach(string guid in AssetDatabase.FindAssets("t:Texture2D",new[]{"Assets/Veilbound/Resources/Sanctuary/TownProgression/Layers/Recovery/HousingRefs"})){var t=AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid)) as TextureImporter;if(t!=null){Configure(t);t.SaveAndReimport();}}PrototypeBuild.Build();}
}}
