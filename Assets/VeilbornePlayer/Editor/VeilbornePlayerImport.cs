#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Veilbound.PlayerStaging.Editor
{
    // Copy this entire VeilbornePlayer staging folder into Assets/VeilbornePlayer.
    // Creates new review prefabs only; it never edits the game's existing prefab.
    public static class VeilbornePlayerImport
    {
        const string Root = "Assets/VeilbornePlayer";

        [MenuItem("Veilbound/Player staging/Build review prefabs")]
        public static void Build()
        {
            Directory.CreateDirectory(Root + "/Generated");
            AssetDatabase.Refresh();
            foreach (string variant in new[] { "Male", "Female" }) BuildVariant(variant);
            AssetDatabase.SaveAssets();
            Debug.Log("VEILBORNE_PLAYER_STAGING_PASS: new male/female review prefabs created.");
        }

        static void BuildVariant(string variant)
        {
            string folder = Root + "/Models/" + variant;
            string rigPath = folder + "/" + variant + "_Rigged.fbx";
            string motionPath = folder + "/" + variant + "_Locomotion.fbx";
            var rigImporter = AssetImporter.GetAtPath(rigPath) as ModelImporter;
            if (rigImporter == null) throw new FileNotFoundException(rigPath);
            rigImporter.animationType = ModelImporterAnimationType.Generic;
            rigImporter.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            rigImporter.importAnimation = false;
            rigImporter.SaveAndReimport();
            Avatar avatar = AssetDatabase.LoadAllAssetsAtPath(rigPath).OfType<Avatar>().FirstOrDefault();
            if (!avatar || !avatar.isValid)
                throw new InvalidOperationException(variant + " did not import as a valid Generic avatar.");
            var humanImporter=AssetImporter.GetAtPath(folder+"/"+variant+"_Humanoid.fbx") as ModelImporter;
            if(humanImporter){humanImporter.animationType=ModelImporterAnimationType.Human;humanImporter.avatarSetup=ModelImporterAvatarSetup.CreateFromThisModel;humanImporter.importAnimation=false;humanImporter.SaveAndReimport();}

            var motionImporter = AssetImporter.GetAtPath(motionPath) as ModelImporter;
            if (motionImporter == null) throw new FileNotFoundException(motionPath);
            motionImporter.animationType = ModelImporterAnimationType.Generic;
            motionImporter.avatarSetup = ModelImporterAvatarSetup.CopyFromOther;
            motionImporter.sourceAvatar = avatar;
            motionImporter.importAnimation = true;
            var takes = motionImporter.defaultClipAnimations;
            foreach (var take in takes)
            {
                take.loopTime = true;
                take.loopPose = true;
                take.lockRootRotation = true;
                take.lockRootHeightY = true;
                take.lockRootPositionXZ = true;
                take.keepOriginalOrientation = true;
                take.keepOriginalPositionXZ = true;
            }
            motionImporter.clipAnimations = takes;
            motionImporter.SaveAndReimport();
            var clips = AssetDatabase.LoadAllAssetsAtPath(motionPath).OfType<AnimationClip>()
                .Where(c => !c.name.StartsWith("__preview__", StringComparison.Ordinal)).ToArray();
            AnimationClip Clip(string name) => clips.FirstOrDefault(c => c.name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                ?? throw new InvalidOperationException(variant + " is missing clip " + name);
            string controllerPath = Root + "/Generated/" + variant + "_Locomotion.controller";
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(controllerPath))
                throw new InvalidOperationException(controllerPath + " already exists; review it before rebuilding.");
            var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            controller.AddParameter("PlanarSpeed", AnimatorControllerParameterType.Float);
            var tree = new BlendTree { name = "Idle walk run", blendParameter = "PlanarSpeed", useAutomaticThresholds = false };
            AssetDatabase.AddObjectToAsset(tree, controller);
            tree.AddChild(Clip("Idle"), 0f);
            tree.AddChild(Clip("Walk"), 1.2f);
            tree.AddChild(Clip("Run"), 3.6f);
            var state = controller.layers[0].stateMachine.AddState("Locomotion");
            state.motion = tree;
            state.iKOnFeet = false;
            controller.layers[0].stateMachine.defaultState = state;

            var model = AssetDatabase.LoadAssetAtPath<GameObject>(rigPath);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(model);
            try
            {
                instance.name = "Veilborne_" + variant + "_Review";
                var animator = instance.GetComponent<Animator>() ?? instance.AddComponent<Animator>();
                animator.avatar = avatar;
                animator.runtimeAnimatorController = controller;
                animator.applyRootMotion = false;
                ApplyMaterials(instance, variant, folder);
                instance.AddComponent<VeilborneAnimationDriver>();
                PrefabUtility.SaveAsPrefabAsset(instance, Root + "/Generated/" + variant + "_Review.prefab");
            }
            finally { UnityEngine.Object.DestroyImmediate(instance); }
        }

        static void ApplyMaterials(GameObject instance, string variant, string folder)
        {
            Material Surface(string name)
            {
                string path = Root + "/Generated/" + variant + "_" + name + ".mat";
                var material = new Material(Shader.Find("Standard")) { name = name };
                if (name.StartsWith("tripo_material", StringComparison.OrdinalIgnoreCase))
                {
                    material.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(folder + "/textures/" + variant + "_BaseColor.png");
                    material.SetTexture("_BumpMap", AssetDatabase.LoadAssetAtPath<Texture2D>(folder + "/textures/" + variant + "_Normal.png"));
                    material.SetTexture("_MetallicGlossMap", AssetDatabase.LoadAssetAtPath<Texture2D>(folder + "/textures/" + variant + "_MetallicSmoothness.png"));
                    material.SetTexture("_OcclusionMap", AssetDatabase.LoadAssetAtPath<Texture2D>(folder + "/textures/" + variant + "_Occlusion.png"));
                    material.EnableKeyword("_NORMALMAP"); material.EnableKeyword("_METALLICGLOSSMAP");
                    material.SetFloat("_GlossMapScale", 0.8f);
                }
                else if (name.IndexOf("Emission", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    bool pupil = name.Contains("Socket_Pupil");
                    material.color = pupil ? new Color(0.35f, 0.006f, 0.002f) : new Color(0.7f, 0.008f, 0.002f);
                    material.EnableKeyword("_EMISSION");
                    material.SetColor("_EmissionColor", pupil ? new Color(1f, 0.012f, 0.003f) * 0.65f : new Color(1f, 0.018f, 0.004f) * 0.7f);
                    material.SetFloat("_Glossiness", pupil ? 0.35f : 0.2f);
                }
                else material.color = new Color(0.003f, 0.001f, 0.001f);
                AssetDatabase.CreateAsset(material, path); return material;
            }
            var materials = new System.Collections.Generic.Dictionary<string, Material>();
            foreach (var renderer in instance.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                var slots = renderer.sharedMaterials;
                for (int i = 0; i < slots.Length; i++)
                {
                    string name = slots[i] ? slots[i].name : "Iris_Dark_Centre";
                    if (!materials.TryGetValue(name, out var material)) materials[name] = material = Surface(name);
                    slots[i] = material;
                }
                renderer.sharedMaterials = slots;
            }
        }
    }

    public sealed class VeilborneTextureImport : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith("Assets/VeilbornePlayer/Models/", StringComparison.Ordinal)) return;
            var importer = (TextureImporter)assetImporter;
            importer.maxTextureSize = 4096;
            if (assetPath.EndsWith("_Normal.png", StringComparison.Ordinal)) importer.textureType = TextureImporterType.NormalMap;
            if (assetPath.EndsWith("_ORM.png", StringComparison.Ordinal) || assetPath.EndsWith("_MetallicSmoothness.png", StringComparison.Ordinal) || assetPath.EndsWith("_Occlusion.png", StringComparison.Ordinal)) importer.sRGBTexture = false;
        }
    }
}
#endif
