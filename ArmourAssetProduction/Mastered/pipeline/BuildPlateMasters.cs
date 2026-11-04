#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Animations;
using UnityEngine.Playables;
namespace Veilbound.PlateMasters.Editor
{
 public static class BuildPlateMasters
 {
  const string Root="Assets/VeilboundPlateMasters";
  static readonly string[] TierNames={"Tin","Copper","Bronze","Brass","Iron","Steel","HardenedSteel","Tungsten","Titanium","Veilcrystal"};
  static readonly int[] TierIds={100,101,102,103,104,105,106,108,107,109};
  static readonly int[] Families={0,1,1,1,0,0,0,0,1,1};
  static readonly string[] Slots={"Head","Shoulders","Chest","Hands","Belt","Bracers","Legs","Feet","Cape"};
  [Serializable] class VariantCheck {public string variant;public int bodyTriangles,maskedBodyTriangles,masterMeshes,prefabs;public bool masksRestore,slotToggles,mixedTier,uv2,waistTuck;}
  [Serializable] class MotionCheck {public string variant,family;public bool finite=true;public float maxMotion;public int clips;}
  [Serializable] class Results {public bool passed;public int tierCount,materials;public List<VariantCheck> variants=new List<VariantCheck>();public List<MotionCheck> motion=new List<MotionCheck>();}
  static int Slot(string name)
  {
   if(name.Contains("_Shoulder_")||name.Contains("_UpperArm_"))return 1;if(name.Contains("_Bracer_")||name.Contains("_Elbow_"))return 5;if(name.Contains("_Hand_"))return 3;
   if(name.Contains("_Leg_")||name.Contains("_Thigh_"))return 6;if(name.Contains("_Foot_"))return 7;
   for(int i=0;i<Slots.Length;i++)if(name.EndsWith("_"+Slots[i],StringComparison.Ordinal))return i;
   throw new InvalidOperationException("Unclassified master renderer "+name);
  }
        static Mesh Combine(SkinnedMeshRenderer[] sources,Transform slotRoot,Transform[] bones)
        {
            var boneIndices=bones.Select((b,i)=>(b.name,i)).ToDictionary(p=>p.name,p=>p.i);
            var vertices=new List<Vector3>();var normals=new List<Vector3>();var uv=new List<Vector2>();var weights=new List<BoneWeight>();var triangles=new List<int>();
            foreach(var renderer in sources)
            {
                var mesh=renderer.sharedMesh;int offset=vertices.Count;var matrix=slotRoot.worldToLocalMatrix*renderer.localToWorldMatrix;var normalMatrix=matrix.inverse.transpose;
                var sourceVertices=mesh.vertices;var sourceNormals=mesh.normals;var sourceUv=mesh.uv;var sourceWeights=mesh.boneWeights;
                if(sourceWeights.Length!=sourceVertices.Length)throw new InvalidOperationException("Missing source skin weights");
                int Map(int index)=>boneIndices.TryGetValue(renderer.bones[index].name,out int result)?result:throw new InvalidOperationException("Wrong rig bone "+renderer.bones[index].name);
                for(int i=0;i<sourceVertices.Length;i++)
                {
                    vertices.Add(matrix.MultiplyPoint3x4(sourceVertices[i]));normals.Add(normalMatrix.MultiplyVector(sourceNormals[i]).normalized);uv.Add(sourceUv.Length>i?sourceUv[i]:Vector2.zero);
                    var w=sourceWeights[i];w.boneIndex0=Map(w.boneIndex0);w.boneIndex1=Map(w.boneIndex1);w.boneIndex2=Map(w.boneIndex2);w.boneIndex3=Map(w.boneIndex3);weights.Add(w);
                }
                foreach(int index in mesh.triangles)triangles.Add(index+offset);
            }
            var combined=new Mesh{name=slotRoot.name};if(vertices.Count>65535)combined.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;
            combined.SetVertices(vertices);combined.SetNormals(normals);combined.SetUVs(0,uv);combined.SetTriangles(triangles,0);combined.boneWeights=weights.ToArray();
            combined.bindposes=bones.Select(b=>b.worldToLocalMatrix*slotRoot.localToWorldMatrix).ToArray();combined.RecalculateBounds();return combined;
        }
  static Mesh MaskFullSet(Mesh source)
  {
   var mesh=UnityEngine.Object.Instantiate(source);mesh.name=source.name+" Full set preview";var uv=source.uv2;
   for(int sub=0;sub<source.subMeshCount;sub++)
   {
    var sourceTris=source.GetTriangles(sub);var kept=new List<int>();
    for(int i=0;i<sourceTris.Length;i+=3)
    {
     var bits=uv[sourceTris[i]];if(Mathf.RoundToInt(bits.x*512)!=0||Mathf.RoundToInt(bits.y*512)!=0)continue;
     kept.Add(sourceTris[i]);kept.Add(sourceTris[i+1]);kept.Add(sourceTris[i+2]);
    }
    mesh.SetTriangles(kept,sub,false);
   }
   mesh.bounds=source.bounds;return mesh;
  }
  static int Triangles(Mesh mesh)=>Enumerable.Range(0,mesh.subMeshCount).Sum(sub=>mesh.GetTriangles(sub).Length/3);
  public static void Run()
  {
   Directory.CreateDirectory(Root+"/Generated");AssetDatabase.Refresh();var results=new Results{tierCount=10,materials=10};
   var materials=new Material[10];
   foreach(string file in AssetDatabase.FindAssets("t:Texture2D",new[]{Root+"/Atlas"}).Select(AssetDatabase.GUIDToAssetPath))
   {
    var importer=(TextureImporter)AssetImporter.GetAtPath(file);importer.sRGBTexture=!file.Contains("MetallicSmoothness");importer.textureCompression=TextureImporterCompression.Uncompressed;importer.filterMode=FilterMode.Point;importer.wrapMode=TextureWrapMode.Clamp;importer.SaveAndReimport();
   }
   for(int tier=0;tier<10;tier++)
   {
    var material=new Material(Shader.Find("Standard")){name="Plate_"+TierNames[tier]};material.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Atlas/"+TierNames[tier]+"_BaseColor.png");
    material.SetTexture("_MetallicGlossMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Atlas/Plate_MetallicSmoothness.png"));material.EnableKeyword("_METALLICGLOSSMAP");material.SetFloat("_GlossMapScale",.45f);
    AssetDatabase.CreateAsset(material,Root+"/Generated/Plate_"+TierNames[tier]+".mat");materials[tier]=material;
   }
   foreach(string variant in new[]{"Male","Female"})
   {
    var character=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/VeilbornePlayer/Generated/"+variant+"_Review.prefab"));
    var imported=new List<GameObject>();
    try
    {
     var animator=character.GetComponent<Animator>();animator.enabled=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
     var body=character.GetComponentInChildren<SkinnedMeshRenderer>();var bones=body.bones;var dictionary=bones.ToDictionary(b=>b.name);
     string bodyPath=Root+"/Models/"+variant+"/"+variant+"_BodyMaskSource.fbx";
     var bodyImporter=(ModelImporter)AssetImporter.GetAtPath(bodyPath);bodyImporter.importAnimation=false;bodyImporter.importBlendShapes=true;bodyImporter.isReadable=true;bodyImporter.generateSecondaryUV=false;bodyImporter.meshCompression=ModelImporterMeshCompression.Off;bodyImporter.SaveAndReimport();
     var bodyModel=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(bodyPath));imported.Add(bodyModel);
     var sourceBody=bodyModel.GetComponentInChildren<SkinnedMeshRenderer>();var sourceMesh=UnityEngine.Object.Instantiate(sourceBody.sharedMesh);sourceMesh.name=variant+" body with slot masks";
     var toBody=body.worldToLocalMatrix*sourceBody.localToWorldMatrix;var normalMatrix=toBody.inverse.transpose;
     var originalBodyMesh=sourceBody.sharedMesh;sourceMesh.ClearBlendShapes();
     for(int shape=0;shape<originalBodyMesh.blendShapeCount;shape++)for(int frame=0;frame<originalBodyMesh.GetBlendShapeFrameCount(shape);frame++)
     {
      var dv=new Vector3[originalBodyMesh.vertexCount];var dn=new Vector3[dv.Length];var dt=new Vector3[dv.Length];originalBodyMesh.GetBlendShapeFrameVertices(shape,frame,dv,dn,dt);
      for(int i=0;i<dv.Length;i++){dv[i]=toBody.MultiplyVector(dv[i]);dn[i]=normalMatrix.MultiplyVector(dn[i]);dt[i]=toBody.MultiplyVector(dt[i]);}
      sourceMesh.AddBlendShapeFrame(originalBodyMesh.GetBlendShapeName(shape),originalBodyMesh.GetBlendShapeFrameWeight(shape,frame),dv,dn,dt);
     }
     sourceMesh.vertices=sourceMesh.vertices.Select(v=>toBody.MultiplyPoint3x4(v)).ToArray();sourceMesh.normals=sourceMesh.normals.Select(v=>normalMatrix.MultiplyVector(v).normalized).ToArray();
     var bodyBones=sourceBody.bones.Select(b=>dictionary[b.name]).ToArray();sourceMesh.bindposes=bodyBones.Select(b=>b.worldToLocalMatrix*body.localToWorldMatrix).ToArray();sourceMesh.RecalculateBounds();
     AssetDatabase.CreateAsset(sourceMesh,Root+"/Generated/"+variant+"_BodySource.asset");body.sharedMesh=sourceMesh;body.bones=bodyBones;
     var preview=MaskFullSet(sourceMesh);AssetDatabase.CreateAsset(preview,Root+"/Generated/"+variant+"_BodyFullSetPreview.asset");
     var root=new GameObject("Modular plate equipment");root.transform.SetParent(character.transform,false);var equipment=root.AddComponent<MasterPlateEquipment>();equipment.body=body;equipment.fullBodySource=sourceMesh;equipment.fullSetBodyPreview=preview;
     equipment.tiers=Enumerable.Range(0,10).Select(t=>new PlateTierBinding{tier=TierNames[t],itemId=TierIds[t],meshFamily=Families[t],material=materials[t]}).ToArray();
     for(int slot=0;slot<9;slot++)
     {
      var go=new GameObject(Slots[slot]);go.transform.SetParent(root.transform,false);var renderer=go.AddComponent<SkinnedMeshRenderer>();renderer.bones=bones;renderer.rootBone=body.rootBone;renderer.updateWhenOffscreen=true;equipment.slots[slot]=renderer;
     }
     for(int family=0;family<2;family++)
     {
      string familyName=family==0?"Tin":"Copper";string modelPath=Root+"/Models/"+variant+"/"+familyName+"_"+variant+"_AllSlots.fbx";
      var modelImporter=(ModelImporter)AssetImporter.GetAtPath(modelPath);modelImporter.importAnimation=false;modelImporter.SaveAndReimport();
      var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(modelPath));imported.Add(model);var sources=model.GetComponentsInChildren<SkinnedMeshRenderer>();
      if(sources.Length!=20)throw new InvalidOperationException("Expected twenty master parts: "+modelPath);
      for(int slot=0;slot<9;slot++)
      {
       var grouped=sources.Where(r=>Slot(r.name)==slot).ToArray();if(grouped.Length==0)throw new InvalidOperationException("Missing slot");
       var mesh=Combine(grouped,equipment.slots[slot].transform,bones);mesh.name=familyName+"_"+variant+"_"+Slots[slot];AssetDatabase.CreateAsset(mesh,Root+"/Generated/"+mesh.name+".asset");equipment.familySlotMeshes[family*9+slot]=mesh;
      }
     }
     var check=new VariantCheck{variant=variant,bodyTriangles=Triangles(sourceMesh),maskedBodyTriangles=Triangles(preview),masterMeshes=20,masksRestore=true,slotToggles=true,mixedTier=true,waistTuck=true,uv2=sourceMesh.isReadable&&sourceMesh.uv2.Length==sourceMesh.vertexCount};
     for(int tier=0;tier<10;tier++)
     {
      equipment.EquipWholeTier(TierIds[tier]);int masked=Triangles(body.sharedMesh);int tuckShape=Enumerable.Range(0,sourceMesh.blendShapeCount).First(i=>sourceMesh.GetBlendShapeName(i).Contains("BeltTuck"));
      check.waistTuck &= body.GetBlendShapeWeight(tuckShape)>99;equipment.SetSlotEnabled(4,false);check.waistTuck &= body.GetBlendShapeWeight(tuckShape)<1;equipment.SetSlotEnabled(4,true);check.waistTuck &= body.GetBlendShapeWeight(tuckShape)>99;
      if(masked!=check.maskedBodyTriangles)throw new InvalidOperationException("Body mask mismatch");
      for(int slot=0;slot<9;slot++)
      {
       equipment.SetSlotEnabled(slot,false);check.slotToggles &= !equipment.slots[slot].gameObject.activeSelf&&Triangles(body.sharedMesh)>=masked;equipment.SetSlotEnabled(slot,true);
      }
      for(int slot=0;slot<9;slot++)equipment.SetSlotEnabled(slot,false);
      check.masksRestore &= Triangles(body.sharedMesh)==check.bodyTriangles;
      equipment.EquipWholeTier(TierIds[tier]);var meshes=equipment.slots.Select(r=>r.sharedMesh).ToArray();equipment.SetSlotTier(2,TierIds[(tier+1)%10]);
      check.mixedTier &= equipment.slots.Where((r,i)=>i!=2).Select(r=>r.sharedMesh).SequenceEqual(meshes.Where((m,i)=>i!=2));
      equipment.EquipWholeTier(TierIds[tier]);equipment.UseSavedFullSetPreview();animator.enabled=true;character.name=TierNames[tier]+"_"+variant+"_Master";
      PrefabUtility.SaveAsPrefabAsset(character,Root+"/Generated/"+character.name+".prefab");check.prefabs++;animator.enabled=false;
     }
     results.variants.Add(check);
     var clips=AssetDatabase.LoadAllAssetsAtPath("Assets/VeilbornePlayer/Models/"+variant+"/"+variant+"_Locomotion.fbx").OfType<AnimationClip>().Where(c=>!c.name.StartsWith("__preview__")).ToArray();animator.enabled=true;
     for(int family=0;family<2;family++)
     {
      equipment.EquipWholeTier(TierIds[family]);var motion=new MotionCheck{variant=variant,family=family==0?"Tin":"Copper",clips=clips.Length};
      foreach(var clip in clips)
      {
       animator.Rebind();animator.Update(0);var graph=PlayableGraph.Create("Plate master QA");graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);var playable=AnimationClipPlayable.Create(graph,clip);playable.SetApplyFootIK(false);var output=AnimationPlayableOutput.Create(graph,"Plate",animator);output.SetSourcePlayable(playable);graph.Play();Vector3[] first=null;
       try
       {
        for(int frame=0;frame<4;frame++)
        {
         playable.SetTime(clip.length*frame/4);graph.Evaluate(.00001f);var positions=new List<Vector3>();
         foreach(var renderer in equipment.slots.Concat(new[]{body}))
         {
          var mesh=new Mesh();renderer.BakeMesh(mesh);
          foreach(var vertex in mesh.vertices)
          {
           var p=renderer.transform.TransformPoint(vertex);motion.finite &= float.IsFinite(p.x)&&float.IsFinite(p.y)&&float.IsFinite(p.z)&&p.y>-.25f&&p.y<2.4f;positions.Add(p);
          }
          UnityEngine.Object.DestroyImmediate(mesh);
         }
         if(first==null)first=positions.ToArray();else for(int i=0;i<positions.Count;i++)motion.maxMotion=Mathf.Max(motion.maxMotion,Vector3.Distance(first[i],positions[i]));
        }
       }
       finally{graph.Destroy();}
      }
      results.motion.Add(motion);
     }
    }
    finally{foreach(var model in imported)UnityEngine.Object.DestroyImmediate(model);UnityEngine.Object.DestroyImmediate(character);}
   }
   results.passed=results.variants.Count==2&&results.variants.All(v=>v.masksRestore&&v.slotToggles&&v.mixedTier&&v.uv2&&v.waistTuck&&v.prefabs==10&&v.masterMeshes==20&&v.maskedBodyTriangles<v.bodyTriangles)&&results.motion.Count==4&&results.motion.All(m=>m.finite&&m.maxMotion>.02f&&m.clips==3);
   string project=Directory.GetParent(Application.dataPath).FullName;File.WriteAllText(Path.Combine(project,"PLATE_MASTER_VALIDATION.json"),JsonUtility.ToJson(results,true));AssetDatabase.SaveAssets();
   if(!results.passed)throw new InvalidOperationException("Master validation failed");Debug.Log("PLATE_MASTERS_PASS");
  }
  public static void Export()
  {
   string project=Directory.GetParent(Application.dataPath).FullName;var result=JsonUtility.FromJson<Results>(File.ReadAllText(Path.Combine(project,"PLATE_MASTER_VALIDATION.json")));if(!result.passed)throw new InvalidOperationException("Validation required");
   AssetDatabase.ExportPackage(Root,Path.Combine(project,"Veilbound_Plate_Masters.unitypackage"),ExportPackageOptions.Recurse|ExportPackageOptions.IncludeDependencies);Debug.Log("PLATE_MASTERS_EXPORTED");
  }
 }
}
#endif
