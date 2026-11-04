using UnityEditor;
using UnityEngine;
namespace Veilbound.Editor {
 public static class SanctuaryCityImport {
  public static void PrepareArt(){const string path="Assets/Veilbound/Resources/Sanctuary/SanctuaryMap.png";AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate);var importer=AssetImporter.GetAtPath(path) as TextureImporter;if(importer==null)throw new System.Exception("Missing Sanctuary map artwork");importer.textureType=TextureImporterType.Default;importer.mipmapEnabled=false;importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Bilinear;importer.maxTextureSize=2048;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();}
 }
}
