using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace Veilbound.Editor {
 public static class UpgradeCardImport {
  public static void Build() {
   AssetDatabase.Refresh();int count=0;
   foreach(string path in Directory.GetFiles("Assets/Veilbound/Resources/UpgradeIcons","*.png")){
    var importer=(TextureImporter)AssetImporter.GetAtPath(path.Replace('\\','/'));
    importer.textureType=TextureImporterType.Default;importer.mipmapEnabled=false;importer.sRGBTexture=true;
    importer.maxTextureSize=512;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.filterMode=FilterMode.Bilinear;importer.wrapMode=TextureWrapMode.Clamp;
    importer.SaveAndReimport();count++;
   }
   if(count!=22)throw new Exception("Expected 18 supplied icons, the fortune emblem, and three new passive icons.");
   Debug.Log("VEILBOUND_UPGRADE_ICONS_PASS "+count);PrototypeBuild.Build();
  }
 }
}

