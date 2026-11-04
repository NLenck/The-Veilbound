#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace Veilbound.Editor {
 public static class ProductionModelImport {
  const string Root="Assets/Veilbound/Resources/ProductionModels",Source="Assets/Veilbound/ProductionModels/Enemies";
  [Serializable] class Check {public string id;public int triangles,bones,materials;public bool passed;public string motion;}
  [Serializable] class Audit {public bool passed;public int enemyModels,playerModels,tierPrefabs;public List<Check> checks=new List<Check>();}
  static readonly string[] Town={"Husk","Lobber","Brute","Ghoul","Burster","Shrieker","Bulwark","Impaler","Ravager","Conduit"};
  static readonly string[] Shrine={"Acolyte","Cinder_Priest","Ash_Wraith","Shrine_Guardian","Blade_Sentinel","Ritual_Orb","Lamenter","Gargoyle","Veil_Angel","High_Priest"};
  static readonly int[] TownKinds={0,1,2,3,4,0,2,3,3,1},ShrineKinds={0,1,3,2,0,4,1,3,5,5};
  static readonly float[] TownHeights={1.9f,2.0f,2.4f,1.15f,2.05f,2.1f,2.5f,2.0f,1.95f,2.5f},ShrineHeights={2.0f,2.2f,1.9f,2.6f,2.3f,1.45f,2.2f,1.45f,3.0f,2.65f};
  public static void Run() {
   Directory.CreateDirectory(Root+"/Enemies");AssetDatabase.Refresh();
   var catalog=ScriptableObject.CreateInstance<ProductionModelCatalog>();var players=new List<ProductionPlayerEntry>();
   foreach(string gender in new[]{"Male","Female"}) {
    string folder="Assets/VeilbornePlayer/Models/"+gender;
    var clips=AssetDatabase.LoadAllAssetsAtPath(folder+"/"+gender+"_Locomotion.fbx").OfType<AnimationClip>().Where(c=>!c.name.StartsWith("__preview__")).ToArray();
    AnimationClip Clip(string id)=>clips.First(c=>c.name.IndexOf(id,StringComparison.OrdinalIgnoreCase)>=0);
    var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/VeilboundPlateMasters/Generated/Tin_"+gender+"_Master.prefab");if(!prefab)throw new InvalidOperationException("Missing staged "+gender+" master");
    players.Add(new ProductionPlayerEntry{gender=gender,prefab=prefab,idle=Clip("Idle"),walk=Clip("Walk"),run=Clip("Run")});
   }
   catalog.players=players.ToArray();catalog.tierPrefabs=AssetDatabase.FindAssets("t:Prefab",new[]{"Assets/VeilboundPlateMasters/Generated"}).Select(g=>AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(g))).ToArray();
   var enemies=new List<ProductionEnemyEntry>();
   for(int i=0;i<Town.Length;i++)enemies.Add(BuildEnemy("Town",Town[i],TownKinds[i],TownHeights[i]));
   for(int i=0;i<Shrine.Length;i++)enemies.Add(BuildEnemy("Shrine",Shrine[i],ShrineKinds[i],ShrineHeights[i]));
   enemies.Add(BuildEnemy("Forest","Wolf",3,1.1f));catalog.enemies=enemies.ToArray();
   Store(catalog,Root+"/Catalog.asset");AssetDatabase.SaveAssets();AssetDatabase.Refresh();Validate();
   Debug.Log("VEILBOUND_PRODUCTION_IMPORT_PASS");
  }
  static T Store<T>(T obj,string path) where T:UnityEngine.Object {
   var existing=AssetDatabase.LoadAssetAtPath<T>(path);
   if(existing){EditorUtility.CopySerialized(obj,existing);UnityEngine.Object.DestroyImmediate(obj);EditorUtility.SetDirty(existing);return existing;}
   AssetDatabase.CreateAsset(obj,path);return obj;
  }
  static Texture2D Texture(string path) {
   var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.maxTextureSize=1024;importer.textureCompression=TextureImporterCompression.Compressed;importer.sRGBTexture=true;importer.isReadable=false;importer.mipmapEnabled=true;importer.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
  }
  static ProductionEnemyEntry BuildEnemy(string biome,string name,int kind,float height) {
   string src=Source+"/"+biome+"/"+name+"/"+name,path=src+".fbx",id=biome+"/"+name,asset=Root+"/Enemies/"+biome+"_"+name;
   var importer=AssetImporter.GetAtPath(path) as ModelImporter;if(!importer)throw new FileNotFoundException(path);importer.isReadable=true;importer.importAnimation=false;importer.meshCompression=ModelImporterMeshCompression.Off;importer.SaveAndReimport();
   var source=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path));var root=new GameObject("Production "+id);
   try {
    var filters=source.GetComponentsInChildren<MeshFilter>();var mesh=new Mesh{name=biome+" "+name+" locally skinned"};mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;
    mesh.CombineMeshes(filters.Select(f=>new CombineInstance{mesh=f.sharedMesh,transform=f.transform.localToWorldMatrix}).ToArray(),true,true);mesh.RecalculateBounds();
    var bounds=mesh.bounds;float factor=height/bounds.size.y;var center=new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);mesh.vertices=mesh.vertices.Select(v=>(v-center)*factor).ToArray();mesh.RecalculateBounds();
    bool floating=new[]{"Ash_Wraith","Lamenter","Ritual_Orb","Veil_Angel"}.Contains(name),quad=new[]{"Ghoul","Gargoyle","Wolf"}.Contains(name);string motion=floating?"floating":quad?"quadruped":"biped";
    float bodyRatio=name=="Shrine_Guardian"?.79f:name=="Conduit"?.89f:name=="Blade_Sentinel"?.87f:name=="Acolyte"?.91f:name=="High_Priest"?.88f:1f;
    var rest=Rest(mesh.bounds,height*bodyRatio,quad,name=="Wolf");var bones=new Transform[17];
    for(int i=0;i<17;i++){bones[i]=new GameObject(HeroBodyMesh.Names[i]).transform;bones[i].SetParent(i==0?root.transform:bones[MonsterVisual.Parents[i]],false);bones[i].localPosition=rest[i]-(i==0?Vector3.zero:rest[MonsterVisual.Parents[i]]);}
    mesh.bindposes=bones.Select(b=>b.worldToLocalMatrix*root.transform.localToWorldMatrix).ToArray();mesh.boneWeights=mesh.vertices.Select(v=>Weights(v,rest,floating)).ToArray();
    mesh=Store(mesh,asset+"_Mesh.asset");
    var mat=new Material(Shader.Find("Standard")){name=biome+" "+name+" ashen/crystal"};mat.mainTexture=Texture(src+"_BaseColor.png");mat.SetFloat("_Glossiness",.18f);mat.SetFloat("_Metallic",0);mat.SetTexture("_EmissionMap",Texture(src+"_Emission.png"));mat.EnableKeyword("_EMISSION");mat.SetColor("_EmissionColor",new Color(.35f,.07f,.055f));mat.enableInstancing=true;mat=Store(mat,asset+"_Material.mat");
    var skin=root.AddComponent<SkinnedMeshRenderer>();skin.sharedMesh=mesh;skin.bones=bones;skin.rootBone=bones[0];skin.sharedMaterial=mat;skin.localBounds=new Bounds(mesh.bounds.center,mesh.bounds.size+Vector3.one*1.8f);skin.updateWhenOffscreen=false;
    var visual=root.AddComponent<MonsterVisual>();visual.kind=kind;visual.skin=skin;visual.bones=bones;visual.rest=rest;visual.bodyWidth=mesh.bounds.size.x;visual.features=new[]{"Production/"+id,"Local procedural "+motion};
    var driver=root.AddComponent<ProductionEnemyMotion>();driver.assetId=id;driver.motion=motion;driver.visual=visual;visual.production=driver;
    var prefab=PrefabUtility.SaveAsPrefabAsset(root,asset+".prefab");return new ProductionEnemyEntry{id=id,biome=biome,displayName=name.Replace('_',' '),kind=kind,motion=motion,prefab=prefab,triangles=mesh.triangles.Length/3};
   } finally {UnityEngine.Object.DestroyImmediate(source);UnityEngine.Object.DestroyImmediate(root);}
  }
  static Vector3[] Rest(Bounds bounds,float h,bool quad,bool wolf) {
   var r=new Vector3[17];
   r[0]=new Vector3(0,h*.50f,0);r[1]=new Vector3(0,h*.60f,0);r[2]=new Vector3(0,h*.73f,0);r[3]=new Vector3(0,h*.84f,0);r[4]=new Vector3(0,h*.90f,0);
   for(int side=0;side<2;side++){float sign=side==0?-1:1;int a=side==0?5:8,l=side==0?11:14;r[a]=new Vector3(sign*h*.16f,h*.75f,0);r[a+1]=new Vector3(sign*h*.22f,h*.58f,h*.015f);r[a+2]=new Vector3(sign*h*.25f,h*.42f,h*.03f);r[l]=new Vector3(sign*h*.075f,h*.49f,0);r[l+1]=new Vector3(sign*h*.095f,h*.27f,h*.025f);r[l+2]=new Vector3(sign*h*.10f,h*.055f,h*.02f);}
   if(quad){float z=bounds.size.z,x=Mathf.Min(bounds.size.x*.34f,h*.40f);r[0]=new Vector3(0,h*.64f,-z*.20f);r[1]=new Vector3(0,h*.67f,0);r[2]=new Vector3(0,h*.65f,z*.20f);r[3]=new Vector3(0,h*.74f,z*.29f);r[4]=new Vector3(0,h*.80f,z*.37f);
    for(int side=0;side<2;side++){float sign=side==0?-1:1;int a=side==0?5:8,l=side==0?11:14;r[a]=new Vector3(sign*x*.65f,h*.60f,z*.19f);r[a+1]=new Vector3(sign*x*.9f,h*.30f,z*.28f);r[a+2]=new Vector3(sign*x,h*.045f,z*.36f);r[l]=new Vector3(sign*x*.55f,h*.59f,-z*.22f);r[l+1]=new Vector3(sign*x*.85f,h*.28f,-z*.29f);r[l+2]=new Vector3(sign*x,h*.045f,-z*.37f);}
   }
   return r;
  }
  static BoneWeight Weights(Vector3 vertex,Vector3[] rest,bool floating) {
   if(floating)return new BoneWeight{boneIndex0=0,weight0=1};
   int[] child={1,2,3,4,-1,6,7,-1,9,10,-1,12,13,-1,15,16,-1};var candidates=new List<KeyValuePair<int,float>>();
   for(int i=0;i<17;i++){Vector3 end=child[i]>=0?rest[child[i]]:rest[i]+Vector3.up*.035f;Vector3 delta=end-rest[i];float t=Mathf.Clamp01(Vector3.Dot(vertex-rest[i],delta)/Mathf.Max(.00001f,delta.sqrMagnitude));float d=(vertex-(rest[i]+delta*t)).sqrMagnitude;
    if(i>=11&&vertex.y>rest[0].y+.08f)d+=10;
    candidates.Add(new KeyValuePair<int,float>(i,1/Mathf.Pow(.025f+d,2)));
   }
   var best=candidates.OrderByDescending(p=>p.Value).Take(4).ToArray();float sum=best.Sum(p=>p.Value);
   return new BoneWeight{boneIndex0=best[0].Key,weight0=best[0].Value/sum,boneIndex1=best[1].Key,weight1=best[1].Value/sum,boneIndex2=best[2].Key,weight2=best[2].Value/sum,boneIndex3=best[3].Key,weight3=best[3].Value/sum};
  }
  public static void Validate() {
   var catalog=AssetDatabase.LoadAssetAtPath<ProductionModelCatalog>(Root+"/Catalog.asset");var audit=new Audit{enemyModels=catalog.enemies.Length,playerModels=catalog.players.Length,tierPrefabs=catalog.tierPrefabs.Length};
   foreach(var entry in catalog.players) {
    var city=new Settlement{female=entry.gender=="Female",bronzeHeroVersion=1};city.EnsureGear();for(int s=0;s<8;s++)city.equippedGear[s]=100;city.equippedGear[8]=11;
    var rig=new HeroRig(null,city);
    try {var visual=rig.root.GetComponent<ProductionHeroVisual>();if(!visual)throw new Exception("Live HeroRig did not use production player");
     rig.SetMotion(Vector3.forward*3.6f);rig.Animate(0,1,0);Vector3 foot=visual.facade.bones[13].position;rig.Animate(.2f,1,0);if(Vector3.Distance(foot,visual.facade.bones[13].position)<.02f)throw new Exception("Direct skeleton locomotion did not animate the foot: "+entry.gender);
     for(int metal=0;metal<10;metal++){for(int s=0;s<9;s++)visual.equipment.SetSlotTier(s,100+metal);rig.SetMotion(Vector3.forward*3.6f);rig.Animate(.27f+metal*.13f,1,.45f);var baked=new Mesh();rig.skin.BakeMesh(baked);bool finite=baked.vertices.All(v=>float.IsFinite(v.x)&&float.IsFinite(v.y)&&float.IsFinite(v.z)&&v.sqrMagnitude<100);UnityEngine.Object.DestroyImmediate(baked);if(!finite)throw new Exception("Nonfinite production hero "+entry.gender);}
     rig.Animate(0,0,0);if(visual.lastGripError>.07f)throw new Exception(entry.gender+" grip error "+visual.lastGripError);
     for(int s=0;s<9;s++)visual.equipment.SetSlotEnabled(s,false);if(rig.skin.sharedMesh.triangles.Length!=visual.equipment.fullBodySource.triangles.Length)throw new Exception("Unequip did not restore complete source body");
     audit.checks.Add(new Check{id=entry.gender,triangles=rig.skin.sharedMesh.triangles.Length/3,bones=rig.skin.bones.Length,materials=rig.skin.sharedMaterials.Length,passed=true,motion="Generic idle/walk/run + two hand IK"});
    } finally {rig.Release();UnityEngine.Object.DestroyImmediate(rig.root.gameObject);}
   }
   foreach(var entry in catalog.enemies) {
    var go=UnityEngine.Object.Instantiate(entry.prefab);var v=go.GetComponent<MonsterVisual>();bool finite=true,moved=false;Vector3 before=Vector3.zero;
    for(int i=0;i<5;i++){v.Animate(i*.19f,1,.19f);if(i==3)v.GroundSlamPose(.65f);var baked=new Mesh();v.skin.BakeMesh(baked);finite&=baked.vertices.All(p=>float.IsFinite(p.x)&&float.IsFinite(p.y)&&float.IsFinite(p.z)&&p.sqrMagnitude<200);if(i==0)before=baked.vertices[0];else moved|=Vector3.Distance(before,baked.vertices[0])>.00001f;UnityEngine.Object.DestroyImmediate(baked);}
    var check=new Check{id=entry.id,triangles=v.skin.sharedMesh.triangles.Length/3,bones=v.bones.Length,materials=v.skin.sharedMaterials.Length,passed=finite&&moved&&v.skin.sharedMesh.boneWeights.All(w=>Mathf.Abs(w.weight0+w.weight1+w.weight2+w.weight3-1)<.001f),motion=entry.motion};audit.checks.Add(check);UnityEngine.Object.DestroyImmediate(go);
   }
   audit.passed=audit.enemyModels==21&&audit.playerModels==2&&audit.tierPrefabs==20&&audit.checks.All(c=>c.passed);Directory.CreateDirectory("ModelIntegration");File.WriteAllText("ModelIntegration/EDITOR_VALIDATION.json",JsonUtility.ToJson(audit,true));
   if(!audit.passed)throw new Exception("Production model integration validation failed");Debug.Log("VEILBOUND_PRODUCTION_VALIDATION_PASS");
  }
  public static void DiagnoseBody() {
   var lines=new List<string>();
   foreach(bool female in new[]{false,true}) {
    var c=new Settlement{female=female,bronzeHeroVersion=1};c.EnsureGear();var rig=new HeroRig(null,c);var v=rig.root.GetComponent<ProductionHeroVisual>();
    try {
     for(int slot=0;slot<9;slot++)v.equipment.SetSlotEnabled(slot,false);
     lines.Add("SEX "+female+" body "+rig.skin.transform.name+" localScale "+rig.skin.transform.localScale+" lossy "+rig.skin.transform.lossyScale);
     var original=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/VeilbornePlayer/Generated/"+(female?"Female":"Male")+"_Review.prefab"));
     var originalSkin=original.GetComponentInChildren<SkinnedMeshRenderer>();
     string prefix="ModelIntegration/"+(female?"Female":"Male");
     File.WriteAllLines(prefix+"_original_vertices.csv",originalSkin.sharedMesh.vertices.Select(p=>originalSkin.transform.TransformPoint(p)).Select(p=>p.x.ToString("R",System.Globalization.CultureInfo.InvariantCulture)+","+p.y.ToString("R",System.Globalization.CultureInfo.InvariantCulture)+","+p.z.ToString("R",System.Globalization.CultureInfo.InvariantCulture)));
     File.WriteAllLines(prefix+"_source_vertices.csv",v.equipment.fullBodySource.vertices.Select(p=>rig.skin.transform.TransformPoint(p)).Select(p=>p.x.ToString("R",System.Globalization.CultureInfo.InvariantCulture)+","+p.y.ToString("R",System.Globalization.CultureInfo.InvariantCulture)+","+p.z.ToString("R",System.Globalization.CultureInfo.InvariantCulture)));
     UnityEngine.Object.DestroyImmediate(original);
     for(int i=0;i<rig.skin.sharedMesh.blendShapeCount;i++)lines.Add("BEFORE "+rig.skin.sharedMesh.GetBlendShapeName(i)+"="+rig.skin.GetBlendShapeWeight(i));
     rig.NeutralPose(0);
     for(int i=0;i<rig.skin.sharedMesh.blendShapeCount;i++)lines.Add("AFTER "+rig.skin.sharedMesh.GetBlendShapeName(i)+"="+rig.skin.GetBlendShapeWeight(i));
     var mesh=new Mesh();rig.skin.BakeMesh(mesh);lines.Add("MESH "+rig.skin.sharedMesh.bounds+" BAKED "+mesh.bounds+" RENDERER "+rig.skin.bounds);mesh.RecalculateBounds();lines.Add("RECALC "+mesh.bounds+" ROOT "+rig.root.localScale);
     foreach(int i in new[]{0,4,7,13})lines.Add("BONE "+i+"="+rig.root.InverseTransformPoint(v.facade.bones[i].position));
     UnityEngine.Object.DestroyImmediate(mesh);
    }finally{rig.Release();UnityEngine.Object.DestroyImmediate(rig.root.gameObject);}
   }
   File.WriteAllLines("ModelIntegration/body_diagnostic.txt",lines);
  }
  public static void Build() {
   PrototypeBuild.Checks();Validate();AssetDatabase.SaveAssets();Directory.CreateDirectory("Builds/Integrated");
   var result=BuildPipeline.BuildPlayer(new[]{"Assets/Veilbound/Scenes/HollowCourt.unity"},"Builds/Integrated/TheVeilbound.exe",BuildTarget.StandaloneWindows64,BuildOptions.Development);
   if(result.summary.result!=BuildResult.Succeeded)throw new Exception("Integrated build failed: "+result.summary.result);Debug.Log("VEILBOUND_PRODUCTION_BUILD_PASS");
  }
 }
}
#endif
