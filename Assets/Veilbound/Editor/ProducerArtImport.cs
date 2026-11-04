using UnityEditor;
using UnityEngine;
namespace Veilbound.Editor {
 public static class ProducerArtImport {
  public static void Prepare(){foreach(string name in new[]{"MineBackdrop","HuntersCabinBackdrop","TextileTavernBackdrop"}){
   var importer=AssetImporter.GetAtPath("Assets/Veilbound/Resources/ProducerUI/"+name+".png") as TextureImporter;if(importer==null)throw new System.Exception("Missing producer backdrop: "+name);
   if(importer.npotScale==TextureImporterNPOTScale.None&&!importer.mipmapEnabled&&importer.wrapMode==TextureWrapMode.Clamp)continue;
   importer.textureType=TextureImporterType.Default;importer.npotScale=TextureImporterNPOTScale.None;importer.mipmapEnabled=false;importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Bilinear;importer.maxTextureSize=2048;importer.sRGBTexture=true;importer.alphaSource=TextureImporterAlphaSource.None;importer.textureCompression=TextureImporterCompression.CompressedHQ;importer.isReadable=false;importer.SaveAndReimport();
  }}
 }
}
