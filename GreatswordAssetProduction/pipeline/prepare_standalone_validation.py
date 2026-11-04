from pathlib import Path
import tarfile,json
r=Path(r'C:\The Veilbound\GreatswordAssetProduction');project=r/'UnityValidation';project.mkdir(exist_ok=True);(project/'Assets/Editor').mkdir(parents=True,exist_ok=True);(project/'Packages').mkdir(exist_ok=True);(project/'ProjectSettings').mkdir(exist_ok=True)
with tarfile.open(r/'Veilborne_Greatswords_Review.unitypackage','r:gz') as archive:
 members={m.name:m for m in archive.getmembers()}
 for member in archive.getmembers():
  if not member.name.endswith('/pathname'):continue
  prefix=member.name.rsplit('/',1)[0];path=archive.extractfile(member).read().decode('utf-8-sig').strip();assert path.startswith('Assets/VeilboundGreatswords') and '..' not in Path(path).parts
  target=(project/path).resolve();assert target.is_relative_to(project.resolve());data=members.get(prefix+'/asset');target.parent.mkdir(parents=True,exist_ok=True)
  if data:target.write_bytes(archive.extractfile(data).read())
  else:target.mkdir(exist_ok=True)
  meta=members.get(prefix+'/asset.meta')
  if meta:Path(str(target)+'.meta').write_bytes(archive.extractfile(meta).read())
(project/'Packages/manifest.json').write_text(json.dumps({'dependencies':{'com.unity.modules.animation':'1.0.0','com.unity.modules.imageconversion':'1.0.0'}},indent=2));(project/'ProjectSettings/ProjectVersion.txt').write_text('m_EditorVersion: 6000.3.9f1\n')
source='''#if UNITY_EDITOR
using System;using System.IO;using System.Linq;using UnityEditor;using UnityEngine;using Veilbound.Greatswords;
public static class GreatswordStandaloneValidation {
 public static void Run(){var library=AssetDatabase.LoadAssetAtPath<GreatswordVisualLibrary>("Assets/VeilboundGreatswords/Generated/GreatswordVisualLibrary.asset");if(!library||library.swords.Length!=10)throw new Exception("Missing standalone sword library");var materials=library.swords.Select(e=>e.prefab.GetComponentInChildren<MeshRenderer>().sharedMaterial).Distinct().ToArray();if(materials.Length!=1)throw new Exception("Swords must share one material");
 foreach(var entry in library.swords){var go=UnityEngine.Object.Instantiate(entry.prefab);try{var v=go.GetComponent<GreatswordVariant>();var meshes=go.GetComponentsInChildren<MeshFilter>();if(!v||meshes.Length!=1||meshes[0].sharedMesh.triangles.Length/3!=entry.triangles)throw new Exception("Incomplete standalone sword: "+entry.tier);if(Vector3.Distance(v.mainHand.localPosition,new Vector3(0,.035f,.015f))>.0001f||Vector3.Distance(v.offHand.localPosition,new Vector3(0,.035f,.17f))>.0001f)throw new Exception("Grip mismatch");if(go.GetComponentsInChildren<MonoBehaviour>().Any(x=>!x))throw new Exception("Missing script dependency");}finally{UnityEngine.Object.DestroyImmediate(go);}}
 if(!materials[0].mainTexture||!materials[0].GetTexture("_EmissionMap")||!materials[0].GetTexture("_MetallicGlossMap"))throw new Exception("Missing atlas dependency");File.WriteAllText("STANDALONE_PACKAGE_VALIDATION.json","{\\\"passed\\\":true,\\\"swords\\\":10,\\\"shared_materials\\\":1,\\\"missing_dependencies\\\":0}");Debug.Log("GREATSWORDS_STANDALONE_PACKAGE_PASS");}
}
#endif
'''
(project/'Assets/Editor/GreatswordStandaloneValidation.cs').write_text(source);print('ACTUAL_UNITYPACKAGE_EXTRACTED_TO_CLEAN_PROJECT')

