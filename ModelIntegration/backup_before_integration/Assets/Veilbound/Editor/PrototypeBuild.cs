using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Veilbound.Editor
{
    public static class PrototypeBuild
    {
        [MenuItem("Veilbound/Generate scene and build Windows")]
        public static void Build()
        {
            SanctuaryCityImport.PrepareArt();
            Checks();
            Directory.CreateDirectory("Assets/Veilbound/Resources");
            if(!AssetDatabase.LoadAssetAtPath<Material>("Assets/Veilbound/Resources/VeilMaterial.mat"))
                AssetDatabase.CreateAsset(new Material(Shader.Find("Standard")),"Assets/Veilbound/Resources/VeilMaterial.mat");
            var template=AssetDatabase.LoadAssetAtPath<Material>("Assets/Veilbound/Resources/VeilMaterial.mat");
            template.EnableKeyword("_EMISSION");template.SetColor("_EmissionColor",Color.black);template.enableInstancing=true;EditorUtility.SetDirty(template);
            // A non-black serialized emission material keeps the runtime glow variant in the player build.
            const string glowPath="Assets/Veilbound/Resources/VeilGlowMaterial.mat";
            if(!AssetDatabase.LoadAssetAtPath<Material>(glowPath))AssetDatabase.CreateAsset(new Material(Shader.Find("Standard")),glowPath);
            var glow=AssetDatabase.LoadAssetAtPath<Material>(glowPath);
            glow.SetColor("_EmissionColor",new Color(.65f,.2f,.05f));glow.EnableKeyword("_EMISSION");
            glow.globalIlluminationFlags=MaterialGlobalIlluminationFlags.RealtimeEmissive;glow.enableInstancing=true;EditorUtility.SetDirty(glow);
            if(!AssetDatabase.LoadAssetAtPath<Material>("Assets/Veilbound/Resources/VeilFog.mat"))
                AssetDatabase.CreateAsset(new Material(Shader.Find("Veilbound/LivingFog")),"Assets/Veilbound/Resources/VeilFog.mat");
            if(!AssetDatabase.LoadAssetAtPath<Material>("Assets/Veilbound/Resources/HeroSurface.mat"))
                AssetDatabase.CreateAsset(new Material(Shader.Find("Veilbound/CharacterSurface")),"Assets/Veilbound/Resources/HeroSurface.mat");
            if(!AssetDatabase.LoadAssetAtPath<Material>("Assets/Veilbound/Resources/VeilVolume.mat"))
                AssetDatabase.CreateAsset(new Material(Shader.Find("Hidden/Veilbound/Volume")),"Assets/Veilbound/Resources/VeilVolume.mat");
            for(int armour=0;armour<3;armour++)
            {
                string path="Assets/Veilbound/Resources/HeroBody"+armour+".asset";
                Mesh generated=HeroBodyMesh.Generate(armour);
                Mesh existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if(existing){EditorUtility.CopySerialized(generated,existing);UnityEngine.Object.DestroyImmediate(generated);EditorUtility.SetDirty(existing);}
                else AssetDatabase.CreateAsset(generated,path);
            }
            Directory.CreateDirectory("Assets/Veilbound/Scenes");
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            new GameObject("Veilbound Game").AddComponent<VeilGame>();
            EditorSceneManager.SaveScene(scene,"Assets/Veilbound/Scenes/HollowCourt.unity");
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Veilbound/Scenes/HollowCourt.unity",true)};
            PlayerSettings.companyName="Veilbound";PlayerSettings.productName="The Veilbound Prototype";
            PlayerSettings.defaultScreenWidth=1600;PlayerSettings.defaultScreenHeight=900;
            PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
            PlayerSettings.runInBackground=true;
            // Direct3D 11 provides distinct exclusive and borderless fullscreen modes.
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64,false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64,new[]{UnityEngine.Rendering.GraphicsDeviceType.Direct3D11});
            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone,ScriptingImplementation.Mono2x);
            var settings=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
            settings.FindProperty("activeInputHandler").intValue=1;settings.ApplyModifiedPropertiesWithoutUndo();
            QualitySettings.SetQualityLevel(2,true); QualitySettings.shadows=ShadowQuality.All;
            AssetDatabase.SaveAssets();
            Directory.CreateDirectory("Builds/Windows");
            var result=BuildPipeline.BuildPlayer(EditorBuildSettings.scenes,"Builds/Windows/TheVeilbound.exe",BuildTarget.StandaloneWindows64,BuildOptions.Development);
            if(result.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded) throw new Exception("Build failed: "+result.summary.result);
            Debug.Log("VEILBOUND_BUILD_PASS");
        }
        [MenuItem("Veilbound/Run logic checks")]
        public static void Checks()
        {
            UpgradeValidation.Run();
            var c=new Settlement();double w=c.wood;
            c.Tick(60,1);Assert(Math.Abs(c.hours-1)<.00001,"Baseline clock");Assert(Math.Abs(c.wood-w-c.WoodRate)<.00001,"Baseline production");
            c.Tick(600,2.4);Assert(Math.Abs(c.hours-25)<.0001,"Dilated clock");
            double before=c.wood;Assert(!c.Spend(1e12,0),"Cannot overspend");Assert(c.wood==before,"Failed transaction atomic");
            var b=new RunBuild();foreach(string id in AbilitySpecs.Weapons)b.ranks[id]=1;
            var rng=new System.Random(17);
            for(int i=0;i<100;i++) {b.Roll(rng);Assert(b.offers.Count==3,"Three choices");foreach(var offer in b.offers) Assert(!offer.ability||b.Rank(offer.id)>0,"Full slots filter new abilities");}
            var held=b.offers[0];Assert(b.Lock(held),"Lock available");b.Roll(rng);Assert(b.offers.Contains(held),"Locked choice retained");
            Assert(b.Banish(held),"Banish available");b.Roll(rng);Assert(!b.offers.Exists(c=>c.Key==held.Key),"Banished choice excluded");Assert(b.locked==null,"Banish clears lock");
            Assert(!b.Lock(b.offers[0]),"Locks finite");
            var collision=new ArenaCollision();collision.Add(Vector3.zero,1);
            Vector3 stop=collision.Move(new Vector3(-3,0,0),Vector3.right*7,.42f);
            Assert(stop.x< -1.4f,"Dodge cannot tunnel through a pillar");
            Vector3 slide=collision.Move(new Vector3(-1.5f,0,-.2f),new Vector3(.6f,0,1.5f),.42f);
            Assert(collision.Free(slide,.42f)&&slide.z>.8f,"Movement slides along props");
            Vector3 routed=new Vector3(-3,0,.1f);
            for(int i=0;i<240;i++)routed=collision.Steer(routed,new Vector3(3,0,0),.04f,.43f,1);
            Assert(routed.x>2,"Husks can route around a pillar");
            Assert(collision.Move(new Vector3(ArenaCollision.Boundary-1,0,0),Vector3.right*5,.42f).magnitude<ArenaCollision.Boundary-.41f,"Arena boundary blocks movement");
            Assert(!collision.ClearLine(new Vector3(-3,0,0),new Vector3(3,0,0)),"Props block line attacks");
            var maze=new ArenaCollision();
            for(int x=-2;x<=2;x++)maze.Add(new Vector3(x,0,1),.55f);
            for(int z=-2;z<1;z++){maze.Add(new Vector3(-2,0,z),.55f);maze.Add(new Vector3(2,0,z),.55f);}
            var map=new ArenaNavigation(maze);Vector3 target=new Vector3(0,0,4),walker=new Vector3(0,0,-.5f);map.Tick(target,1,true);
            for(int i=0;i<650;i++){walker=map.Move(walker,.05f,.43f);Assert(maze.Free(walker,.43f),"Route never penetrates U-shaped obstacle cluster");}
            Assert(Vector3.Distance(walker,target)<.7f,"Flow field escapes concave prop clusters");
            var gear=new Settlement{wood=2000,stone=2000,iron=2000,cloth=2000,leather=2000,essence=100};gear.EnsureGear();
            double initialIron=gear.iron;Assert(!gear.Craft(2,3,out _),"Crafting requires its workshop");Assert(gear.iron==initialIron,"Rejected crafting is atomic");
            Assert(gear.UpgradeBuilding("blacksmith",out _),"Workshop construction");Assert(gear.iron==initialIron,"Buildings use timber and stone, not iron");
            Assert(gear.Craft(2,3,out _)&&gear.Item(2)==31&&gear.Item(1)==0,"Equipment slots are independent");
            gear.tailor=1;Assert(gear.Craft(8,1,out _)&&gear.Item(8)==11,"Tailor crafts cloth capes");gear.equippedGear[8]=0;
            Assert(gear.Item(8)==0&&gear.Owned(8,1)==1,"Removing cape retains ownership");
            gear.weaver=1;double oldCloth=gear.cloth;gear.Tick(60,2);Assert(System.Math.Abs(gear.cloth-oldCloth-8)<.001,"Cloth production follows city dilation at half the former yield");
            var legacy=new Settlement{armour=2,equippedArmour=2,sword=2};legacy.EnsureGear();Assert(legacy.Item(2)==32&&legacy.Item(8)==0&&legacy.blacksmith>=2,"Legacy gear migration preserves unlocks without an automatic cape");
            var roundtrip=JsonUtility.FromJson<Settlement>(JsonUtility.ToJson(gear));roundtrip.EnsureGear();Assert(roundtrip.Item(2)==31&&roundtrip.Item(8)==0&&roundtrip.cloth==gear.cloth,"Independent equipment and resources roundtrip");
            File.WriteAllText("logic-checks.txt","PASS: clocks and resources; atomic costs; workshop gates; nine-slot equipment and legacy migration; card rules; swept collision; concave-cluster route mapping.");
            Debug.Log("VEILBOUND_LOGIC_CHECKS_PASS");
        }
        static void Assert(bool value,string name) { if(!value) throw new Exception("Check failed: "+name); }
    }
}



