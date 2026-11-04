#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Veilbound.PlayerStaging.Editor
{
    public static class PlayerValidation
    {
        public static void ExportValidatedPackage()
        {
            string project=Directory.GetParent(Application.dataPath).FullName;
            var results=JsonUtility.FromJson<Results>(File.ReadAllText(Path.Combine(project,"PLAYER_UNITY_VALIDATION.json")));
            if(!results.passed)throw new InvalidOperationException("Validation must pass before packaging.");
            AssetDatabase.ExportPackage("Assets/VeilbornePlayer",Path.Combine(project,"Veilborne_Player_Review.unitypackage"),ExportPackageOptions.Recurse);
            Debug.Log("PLAYER_REVIEW_PACKAGE_EXPORTED");
        }
        [Serializable] class ClipCheck { public string variant, clip; public float duration; public bool loop, finite, humanMotion; public int bakedVertices; public float minHeight, maxHeight, footMotion; public Vector3 hipsScale, headScale; public string scaleCurves; }
        [Serializable] class RendererCheck { public string name; public Vector3 scale, worldMin, worldMax; public int vertices; }
        [Serializable] class VariantCheck { public string variant; public bool avatarValid, humanoid, separateHumanoidValid; public int bones, renderers; public Vector3 rootScale; public List<RendererCheck> restMeshes = new List<RendererCheck>(); }
        [Serializable] class Results { public bool passed; public List<VariantCheck> variants = new List<VariantCheck>(); public List<ClipCheck> clips = new List<ClipCheck>(); }

        public static void Run()
        {
            if (!AssetDatabase.LoadAssetAtPath<GameObject>("Assets/VeilbornePlayer/Generated/Female_Review.prefab")) VeilbornePlayerImport.Build();
            var results = new Results();
            foreach (string variant in new[] { "Male", "Female" })
            {
                string root = "Assets/VeilbornePlayer";
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(root + "/Generated/" + variant + "_Review.prefab");
                var instance = UnityEngine.Object.Instantiate(prefab);
                try
                {
                    var animator = instance.GetComponent<Animator>();
                    animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;animator.Rebind();animator.Update(0f);
                    var renderers = instance.GetComponentsInChildren<SkinnedMeshRenderer>();
                    var variantCheck = new VariantCheck { variant = variant, avatarValid = animator.avatar && animator.avatar.isValid, humanoid = animator.isHuman,
                        bones = renderers.SelectMany(r => r.bones).Distinct().Count(), renderers = renderers.Length, rootScale=instance.transform.lossyScale };
                    var human=AssetDatabase.LoadAllAssetsAtPath(root+"/Models/"+variant+"/"+variant+"_Humanoid.fbx").OfType<Avatar>().FirstOrDefault();
                    variantCheck.separateHumanoidValid=human && human.isValid && human.isHuman;
                    Transform leftFoot=instance.GetComponentsInChildren<Transform>().First(t=>t.name.EndsWith("LeftFoot",StringComparison.Ordinal));
                    foreach (var renderer in renderers)
                    {
                        var restMesh = new Mesh(); renderer.BakeMesh(restMesh);
                        var world = restMesh.vertices.Select(p => renderer.transform.TransformPoint(p)).ToArray();
                        variantCheck.restMeshes.Add(new RendererCheck { name=renderer.name, scale=renderer.transform.lossyScale, vertices=world.Length,
                            worldMin=new Vector3(world.Min(v=>v.x),world.Min(v=>v.y),world.Min(v=>v.z)),
                            worldMax=new Vector3(world.Max(v=>v.x),world.Max(v=>v.y),world.Max(v=>v.z)) });
                        UnityEngine.Object.DestroyImmediate(restMesh);
                    }
                    results.variants.Add(variantCheck);
                    var clips = AssetDatabase.LoadAllAssetsAtPath(root + "/Models/" + variant + "/" + variant + "_Locomotion.fbx")
                        .OfType<AnimationClip>().Where(c => !c.name.StartsWith("__preview__", StringComparison.Ordinal)).ToArray();
                    foreach (string expected in new[] { "Idle", "Walk", "Run" })
                    {
                        var clip = clips.FirstOrDefault(c => c.name.IndexOf(expected, StringComparison.OrdinalIgnoreCase) >= 0)
                            ?? throw new InvalidOperationException("Missing clip " + variant + "/" + expected);
                        var check = new ClipCheck { variant = variant, clip = expected, duration = clip.length, loop = clip.isLooping, humanMotion=clip.humanMotion,
                            finite = true, minHeight = float.PositiveInfinity, maxHeight = float.NegativeInfinity };
                        check.scaleCurves=string.Join(";",AnimationUtility.GetCurveBindings(clip).Where(b=>b.propertyName.ToLowerInvariant().Contains("scale")).Select(b=>b.path+"/"+b.propertyName+"="+AnimationUtility.GetEditorCurve(clip,b).Evaluate(0)));
                        Vector3 firstFoot=Vector3.zero;bool firstSample=true;
                        var graph = PlayableGraph.Create("Player isolated audit");
                        try
                        {
                            graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
                            var playable = AnimationClipPlayable.Create(graph, clip);
                            playable.SetApplyFootIK(false);
                            var output = AnimationPlayableOutput.Create(graph, "Humanoid", animator);
                            output.SetSourcePlayable(playable); graph.Play();
                            foreach (float fraction in new[] { 0f, .25f, .5f, .75f })
                            {
                                playable.SetTime(clip.length * fraction); graph.Evaluate(0.00001f);
                                Vector3 foot=leftFoot.position;
                                if(firstSample){firstFoot=foot;firstSample=false;}else check.footMotion=Mathf.Max(check.footMotion,Vector3.Distance(foot,firstFoot));
                                foreach (var renderer in renderers)
                                {
                                    if (renderer.bones.Any(b => !b)) throw new InvalidOperationException("Missing skin bone.");
                                    var mesh = new Mesh(); renderer.BakeMesh(mesh);
                                    check.bakedVertices = Math.Max(check.bakedVertices, mesh.vertexCount);
                                    foreach (Vector3 vertex in mesh.vertices)
                                    {
                                        Vector3 p = renderer.transform.TransformPoint(vertex);
                                        check.finite &= float.IsFinite(p.x) && float.IsFinite(p.y) && float.IsFinite(p.z);
                                        check.minHeight = Mathf.Min(check.minHeight, p.y); check.maxHeight = Mathf.Max(check.maxHeight, p.y);
                                    }
                                    UnityEngine.Object.DestroyImmediate(mesh);
                                }
                            }
                        }
                        finally { graph.Destroy(); }
                        results.clips.Add(check);
                    }
                }
                finally { UnityEngine.Object.DestroyImmediate(instance); }
            }
            results.passed = results.variants.Count == 2 && results.variants.All(v => v.avatarValid && v.separateHumanoidValid)
                && results.clips.Count == 6 && results.clips.All(c => c.duration > 0 && c.loop && c.finite && !c.humanMotion && c.bakedVertices > 0 && c.maxHeight-c.minHeight<2.2f && c.minHeight>-.15f && c.maxHeight<2.2f && (c.clip=="Idle" || c.footMotion>.05f));
            File.WriteAllText(Path.Combine(Directory.GetParent(Application.dataPath).FullName, "PLAYER_UNITY_VALIDATION.json"), JsonUtility.ToJson(results, true));
            if (!results.passed) throw new InvalidOperationException("Player isolated Unity validation failed; inspect PLAYER_UNITY_VALIDATION.json");
            Debug.Log("PLAYER_UNITY_VALIDATION_PASS");
        }
    }
}
#endif
