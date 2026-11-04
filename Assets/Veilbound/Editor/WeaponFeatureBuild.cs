#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Veilbound.Greatswords;

namespace Veilbound.Editor
{
    public static class WeaponFeatureBuild
    {
        static int checks;
        static void Check(bool pass,string message)
        {
            if(!pass)throw new Exception("Weapon domain: "+message);
            checks++;
        }

        public static void Validate()
        {
            checks=0;
            float[] damage={10,18,24,35,34,39,48,61,64,109,88,150};
            float[] intervals={2,2.24f,2.4f,2.32f,2.08f,1.92f,1.92f,2.24f,1.6f,2.72f,1.6f,2.72f};
            float[] weight={1.8f,4.185f,5.13f,5.04f,4.86f,4.455f,4.5f,4.725f,2.52f,10.17f,3.15f,3.15f};
            var library=Resources.Load<GreatswordVisualLibrary>("WeaponVisuals/GreatswordLibrary");
            Check(WeaponCatalog.All.Count==12&&WeaponRules.Count==11,"Wooden starter and all eleven craftable profiles are present");
            Check(WeaponCatalog.All.Select(x=>x.Id).Distinct().Count()==12,"Stable definition identities are unique");
            foreach(var weapon in WeaponCatalog.All)
            {
                int index=weapon.ItemId+1;
                Check(weapon.Damage==damage[index]&&weapon.AttackInterval==intervals[index]&&Math.Abs(weapon.Weight-weight[index])<.0001f,"Existing damage, timing and weight are unchanged for "+weapon.Name);
                Check(weapon.Slot==9&&weapon.Type==WeaponType.Greatsword&&weapon.HandsRequired==2,"Greatswords retain two-handed main-hand occupancy");
                Check(ReferenceEquals(weapon,WeaponCatalog.Find(weapon.ItemId)),"Catalogue returns shared definitions");
                Check(WeaponRules.Name(weapon.ItemId)==weapon.Name&&WeaponRules.Damage(weapon.ItemId)==weapon.Damage&&WeaponRules.Interval(weapon.ItemId)==weapon.AttackInterval,"Existing helper callers use definitions");
                var town=new Settlement();town.InitializeWoodenStarter();town.blacksmith=10;town.library=1;town.EnsureResearch();
                for(int i=0;i<town.researchRanks.Length;i++)town.researchRanks[i]=LibraryResearch.Limit(LibraryResearch.Ids[i]);
                var owned=town.RecordCraft(9,weapon.ItemId);town.EquipRoll(owned.id);
                owned.enchantments=new[]{new GearAffix{stat="power",value=2},new GearAffix{stat="crit",value=1}};
                var duplicate=town.RecordCraft(9,weapon.ItemId);
                Check(ReferenceEquals(owned.WeaponDefinition,duplicate.WeaponDefinition)&&owned.id!=duplicate.id&&duplicate.enchantments.Length==0,"Owned weapon copies share data but keep independent enchantments");
                Check(owned.ArmourDefinition==null&&town.Loadout.Sets().Heavy+town.Loadout.Sets().Leather+town.Loadout.Sets().Cloth==0,"Weapons cannot grant armour set counts");
                Check(town.WeaponDamage==weapon.Damage&&town.WeaponInterval==weapon.AttackInterval&&Math.Abs(town.EquipmentMass-weapon.Weight)<.0001f,"Equipped combat stats and total mass use the definition");
                var metrics=GearItemMetrics.From(owned);
                Check(metrics.damage==weapon.Damage&&metrics.interval==weapon.AttackInterval&&Math.Abs(metrics.mass-weapon.Weight)<.0001f,"Comparison metrics agree with equipped stats");
                var restored=JsonUtility.FromJson<Settlement>(JsonUtility.ToJson(town));
                Check(restored.EquippedRoll(9).id==owned.id&&ReferenceEquals(restored.EquippedRoll(9).WeaponDefinition,weapon)&&restored.EquippedRoll(9).enchantments[0].value==2,"Existing weapon IDs and exact enchantments survive save/load");
                if(!weapon.CanCraft)
                {
                    Check(weapon.Recipe==null&&Resources.Load<GameObject>(weapon.PrefabResource),"Starter keeps its prefab reference and is not craftable");
                    Check(!town.CraftMetalWeapon(-1,out _),"Crafting still rejects the wooden starter");
                    continue;
                }
                Check(library&&library.swords.Any(x=>x.metalIndex==weapon.VisualId&&x.prefab),"Definition visual ID resolves to the original prefab");
                var recipe=weapon.Recipe.CopyResources();
                var savedAmounts=recipe.Select(x=>town.ResourceAmount(x.family,x.index)).ToArray();
                foreach(var resource in recipe)town.AddResource(resource.family,resource.index,resource.amount);
                Check(town.CraftMetalWeapon(weapon.ItemId,out _),"Every material profile remains craftable with its exact recipe");
                for(int i=0;i<recipe.Length;i++)Check(Math.Abs(town.ResourceAmount(recipe[i].family,recipe[i].index)-savedAmounts[i])<.0001,"Craft charges the exact original resource amounts");
                Check(town.ItemEnchantmentLimit==0,"Crafting does not unlock the Mystic");
                var returned=weapon.Recipe.CopyResources();returned[0]=new ResourceCost(3,0,999);
                Check(weapon.Recipe.Resources[0].amount!=999,"Callers cannot mutate the shared recipe through exported arrays");
            }
            var light=WeaponCatalog.Find(9);var heavy=WeaponCatalog.Find(10);
            Check(light.Variant==WeaponVariant.TitaniumBase&&heavy.Variant==WeaponVariant.TungstenBase&&light.Recipe.Resources[0].index==9&&heavy.Recipe.Resources[0].index==10,"Both Veilcrystal variants retain separate identities, visuals and bars");
            var starter=WeaponCatalog.Find(0).Recipe.Resources;
            Check(starter[0].amount==8&&starter[1].amount==2&&starter[2].amount==2,"Tin greatsword still costs eight bars and two of each grip/wrap");
            Check(WeaponCatalog.Find(-2)==null&&WeaponCatalog.Find(11)==null,"New catalogue rejects unknown IDs");
            var invalid=new Settlement();invalid.InitializeWoodenStarter();string snapshot=JsonUtility.ToJson(invalid);
            Check(!invalid.CraftMetalWeapon(11,out _)&&JsonUtility.ToJson(invalid)==snapshot,"Invalid crafting requests are atomic");
            Directory.CreateDirectory("Docs/Validation");File.WriteAllText("Docs/Validation/weapon-domain.json","{\"passed\":true,\"checks\":"+checks+",\"profiles\":12}");
            Debug.Log("VEILBOUND_WEAPON_DOMAIN_PASS checks="+checks);
        }

        public static void Build()
        {
            Validate();ArmourFeatureBuild.Validate();PrototypeBuild.Checks();AssetDatabase.SaveAssets();
            Directory.CreateDirectory("Builds/WeaponClassFeature");
            var result=BuildPipeline.BuildPlayer(new[]{"Assets/Veilbound/Scenes/HollowCourt.unity"},"Builds/WeaponClassFeature/TheVeilbound.exe",BuildTarget.StandaloneWindows64,BuildOptions.Development);
            if(result.summary.result!=BuildResult.Succeeded)throw new Exception("Weapon feature build failed");
            Debug.Log("VEILBOUND_WEAPON_FEATURE_BUILD_PASS");
        }
    }
}
#endif
