#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Animations;
using UnityEngine.Playables;
namespace Veilbound.ArmourProduction.Editor
{
    public static class ArmourPrototypeReview
    {
        const string Root = "Assets/ArmourPrototype";
        static readonly string[] Slots={"Head","Shoulders","Chest","Hands","Belt","Bracers","Legs","Feet","Cape"};
        [Serializable] class Check { public string variant,tier;public int slots,triangles,bones;public bool finite,independentToggle;public float maxMotion; }
        [Serializable] class Results { public bool passed;public List<Check> checks=new List<Check>(); }
        static int Slot(string name)
        {
            if(name.Contains("_Shoulder_"))return 1;if(name.Contains("_Bracer_"))return 5;
            if(name.Contains("_Hand_"))return 3;if(name.Contains("_Leg_"))return 6;if(name.Contains("_Foot_"))return 7;
            for(int i=0;i<Slots.Length;i++)if(name.EndsWith("_"+Slots[i],StringComparison.Ordinal))return i;
            throw new InvalidOperationException("Unclassified renderer "+name);
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
        public static void Run()
        {
            Directory.CreateDirectory(Root+"/Generated");AssetDatabase.Refresh();
            foreach(string name in new[]{"Plate_BaseColor.png","Plate_MetallicSmoothness.png","Plate_ORM.png"})
            {
                var textureImporter=AssetImporter.GetAtPath(Root+"/Atlas/"+name) as TextureImporter;
                if(textureImporter){textureImporter.sRGBTexture=name.Contains("BaseColor");textureImporter.filterMode=FilterMode.Point;textureImporter.wrapMode=TextureWrapMode.Clamp;textureImporter.textureCompression=TextureImporterCompression.Uncompressed;textureImporter.SaveAndReimport();}
            }
            var material=new Material(Shader.Find("Standard")){name="Plate_Shared_Atlas"};
            material.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Atlas/Plate_BaseColor.png");
            material.SetTexture("_MetallicGlossMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Atlas/Plate_MetallicSmoothness.png"));material.EnableKeyword("_METALLICGLOSSMAP");material.SetFloat("_GlossMapScale",1);
            AssetDatabase.CreateAsset(material,Root+"/Generated/Plate_Shared_Atlas.mat");var results=new Results();
            foreach(string variant in new[]{"Male","Female"})foreach(string tier in new[]{"Tin","Copper"})
            {
                string path=Root+"/Models/"+variant+"/"+tier+"_"+variant+"_AllSlots.fbx";
                var importer=AssetImporter.GetAtPath(path) as ModelImporter;importer.importAnimation=false;importer.animationType=ModelImporterAnimationType.Generic;importer.SaveAndReimport();
                var character=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/VeilbornePlayer/Generated/"+variant+"_Review.prefab"));
                var source=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path));
                try
                {
                    character.name=tier+"_"+variant+"_FITTING_DRAFT";var animator=character.GetComponent<Animator>();animator.enabled=false;
                    var body=character.GetComponentInChildren<SkinnedMeshRenderer>();var bones=body.bones;
                    var equipment=new GameObject("Prototype modular plate slots");equipment.transform.SetParent(character.transform,false);var controller=equipment.AddComponent<ModularPlateSlots>();controller.runtimeMetalItemId=tier=="Tin"?100:101;
                    var renderers=source.GetComponentsInChildren<SkinnedMeshRenderer>();if(renderers.Length!=14)throw new InvalidOperationException("Expected fourteen independent source pieces.");
                    for(int slot=0;slot<9;slot++)
                    {
                        var group=renderers.Where(r=>Slot(r.name)==slot).ToArray();if(group.Length==0)throw new InvalidOperationException("Missing slot "+slot);
                        var go=new GameObject(tier+"_"+Slots[slot]);go.transform.SetParent(equipment.transform,false);controller.slots[slot]=go;
                        var mesh=Combine(group,go.transform,bones);AssetDatabase.CreateAsset(mesh,Root+"/Generated/"+tier+"_"+variant+"_"+Slots[slot]+".asset");
                        var renderer=go.AddComponent<SkinnedMeshRenderer>();renderer.sharedMesh=mesh;renderer.bones=bones;renderer.rootBone=body.rootBone;renderer.sharedMaterial=material;renderer.updateWhenOffscreen=true;
                    }
                    var check=new Check{variant=variant,tier=tier,slots=9,bones=bones.Length,finite=true,independentToggle=true};
                    for(int slot=0;slot<9;slot++)
                    {
                        controller.SetSlotEnabled(slot,false);check.independentToggle &= controller.slots.Count(o=>o.activeSelf)==8;controller.SetSlotEnabled(slot,true);
                        check.triangles+=controller.slots[slot].GetComponent<SkinnedMeshRenderer>().sharedMesh.triangles.Length/3;
                    }
                    animator.enabled=true;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
                    string prefabPath=Root+"/Generated/"+tier+"_"+variant+"_FITTING_DRAFT.prefab";PrefabUtility.SaveAsPrefabAsset(character,prefabPath);
                    var clips=AssetDatabase.LoadAllAssetsAtPath("Assets/VeilbornePlayer/Models/"+variant+"/"+variant+"_Locomotion.fbx").OfType<AnimationClip>().Where(c=>!c.name.StartsWith("__preview__")).ToArray();
                    animator.enabled=true;
                    foreach(var clip in clips)
                    {
                        animator.Rebind();animator.Update(0);var graph=PlayableGraph.Create("Plate motion check");graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
                        var playable=AnimationClipPlayable.Create(graph,clip);playable.SetApplyFootIK(false);var output=AnimationPlayableOutput.Create(graph,"Armour",animator);output.SetSourcePlayable(playable);graph.Play();
                        Vector3[] first=null;
                        try
                        {
                            for(int frame=0;frame<4;frame++)
                            {
                                playable.SetTime(clip.length*frame/4);graph.Evaluate(0.00001f);var samples=new List<Vector3>();
                                foreach(var renderer in equipment.GetComponentsInChildren<SkinnedMeshRenderer>())
                                {
                                    var mesh=new Mesh();renderer.BakeMesh(mesh);
                                    foreach(var vertex in mesh.vertices)
                                    {
                                        var p=renderer.transform.TransformPoint(vertex);check.finite &= float.IsFinite(p.x)&&float.IsFinite(p.y)&&float.IsFinite(p.z)&&p.y>-.2f&&p.y<2.4f;samples.Add(p);
                                    }
                                    UnityEngine.Object.DestroyImmediate(mesh);
                                }
                                if(first==null)first=samples.ToArray();else for(int i=0;i<samples.Count;i++)check.maxMotion=Mathf.Max(check.maxMotion,Vector3.Distance(first[i],samples[i]));
                            }
                        }
                        finally{graph.Destroy();}
                    }
                    if(clips.Length!=3)throw new InvalidOperationException("Three player clips required");results.checks.Add(check);
                }
                finally{UnityEngine.Object.DestroyImmediate(character);UnityEngine.Object.DestroyImmediate(source);}
            }
            results.passed=results.checks.Count==4&&results.checks.All(c=>c.slots==9&&c.bones==52&&c.finite&&c.independentToggle&&c.maxMotion>.02f);
            File.WriteAllText(Path.Combine(Directory.GetParent(Application.dataPath).FullName,"ARMOUR_UNITY_VALIDATION.json"),JsonUtility.ToJson(results,true));AssetDatabase.SaveAssets();
            if(!results.passed)throw new InvalidOperationException("Armour prototype validation failed");Debug.Log("ARMOUR_PROTOTYPE_VALIDATION_PASS");
        }
        public static void Export()
        {
            string project=Directory.GetParent(Application.dataPath).FullName;var r=JsonUtility.FromJson<Results>(File.ReadAllText(Path.Combine(project,"ARMOUR_UNITY_VALIDATION.json")));if(!r.passed)throw new InvalidOperationException("Validation required");
            AssetDatabase.ExportPackage(Root,Path.Combine(project,"Plate_Prototype_FITTING_DRAFT.unitypackage"),ExportPackageOptions.Recurse|ExportPackageOptions.IncludeDependencies);Debug.Log("ARMOUR_PACKAGE_EXPORTED");
        }
    }
}
#endif
