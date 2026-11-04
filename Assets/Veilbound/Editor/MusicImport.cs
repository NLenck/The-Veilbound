using UnityEditor;
using UnityEngine;
namespace Veilbound.Editor {
 public sealed class MusicImport:AssetPostprocessor {
  void OnPreprocessAudio(){
   if(!assetPath.StartsWith("Assets/Veilbound/Resources/Music/"))return;
   var importer=(AudioImporter)assetImporter;importer.forceToMono=false;importer.loadInBackground=false;
   var settings=importer.defaultSampleSettings;settings.preloadAudioData=true;settings.loadType=AudioClipLoadType.DecompressOnLoad;settings.compressionFormat=AudioCompressionFormat.PCM;settings.sampleRateSetting=AudioSampleRateSetting.PreserveSampleRate;importer.defaultSampleSettings=settings;
  }
  public static void Build(){AssetDatabase.Refresh();PrototypeBuild.Build();}
 }
}
