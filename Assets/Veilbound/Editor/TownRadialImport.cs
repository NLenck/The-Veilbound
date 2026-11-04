using UnityEditor;using UnityEngine;
namespace Veilbound.Editor {public sealed class TownRadialImport:AssetPostprocessor {
 public override int GetPostprocessOrder()=>2000;
 void OnPreprocessTexture(){if(assetPath.Contains("/TownProgression/Radial/"))Configure((TextureImporter)assetImporter);}
 static void Configure(TextureImporter importer){importer.textureType=TextureImporterType.Default;importer.alphaSource=TextureImporterAlphaSource.FromInput;importer.alphaIsTransparency=true;importer.mipmapEnabled=true;importer.filterMode=FilterMode.Trilinear;importer.anisoLevel=4;importer.wrapMode=TextureWrapMode.Clamp;importer.npotScale=TextureImporterNPOTScale.None;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=4096;importer.isReadable=importer.assetPath.EndsWith("/housing-5.png");}
 public static void Build(){AssetDatabase.Refresh();foreach(string guid in AssetDatabase.FindAssets("t:Texture2D",new[]{"Assets/Veilbound/Resources/Sanctuary/TownProgression/Radial"})){var importer=AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid)) as TextureImporter;if(importer!=null){Configure(importer);importer.SaveAndReimport();}}PrototypeBuild.Build();}
}}
