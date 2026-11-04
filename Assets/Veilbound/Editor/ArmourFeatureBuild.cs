#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Veilbound.Editor
{
    public static class ArmourFeatureBuild
    {
        static int checks;
        static void Check(bool pass, string message)
        {
            if (!pass) throw new Exception("Armour domain: " + message);
            checks++;
        }

        static Settlement Empty()
        {
            var town = new Settlement();
            town.InitializeWoodenStarter();
            return town;
        }

        static CraftedGear Add(Settlement town, int slot, int material, int tier = 1)
        {
            var gear = town.RecordCraft(slot, material == 3 ? Metals.Item(tier - 1) : LightGear.Item(material, tier));
            town.EquipRoll(gear.id);
            return gear;
        }

        public static void Validate()
        {
            checks = 0;
            for (int material = 1; material <= 3; material++)
            {
                var town = Empty();
                for (int pieces = 1; pieces <= 8; pieces++)
                {
                    Add(town, pieces - 1, material, pieces);
                    var sets = town.Loadout.Sets();
                    var type = material == 3 ? ArmourType.Heavy : material == 2 ? ArmourType.Leather : ArmourType.Cloth;
                    Check(sets.Count(type) == pieces, "Each body slot counts once across mixed tiers");
                    Check(town.GearBonus("regen") == (material == 3 && pieces >= 4 ? .3f : 0), "Heavy regeneration threshold");
                    Check(town.GearBonus("vitality") == (material == 3 && pieces == 8 ? 100 : 0), "Heavy health threshold");
                    Check(town.GearBonus("speed") == (material == 2 && pieces >= 4 ? 5 : 0), "Leather movement threshold");
                    Check(town.GearBonus("attackspeed") == (material == 2 && pieces == 8 ? 10 : 0), "Leather attack-speed threshold");
                    Check(town.GearBonus("area") == (material == 1 && pieces >= 4 ? 15 : 0), "Cloth area threshold");
                    Check(town.GearBonus("wisdom") == (material == 1 && pieces == 8 ? 50 : 0), "Cloth XP threshold");
                    Check(town.GearHealth == 0 && town.EquipmentArmour > 0 && town.EquipmentMass > 0, "Base armour provides armour and weight, not implicit health");
                }
                int owned = town.craftedGear.Count;
                for (int i = 0; i < 12; i++) town.RecordCraft(2, material == 3 ? 100 : LightGear.Item(material, 1));
                Add(town, 8, 1);
                Check(town.Loadout.Sets().Count(material == 3 ? ArmourType.Heavy : material == 2 ? ArmourType.Leather : ArmourType.Cloth) == 8, "Backpack copies, capes and weapons cannot increase set counts");
                Check(town.craftedGear.Count == owned + 13, "Separate owned copies are retained");
            }

            var mixed = Empty();
            for (int slot = 0; slot < 3; slot++) Add(mixed, slot, 1);
            for (int slot = 3; slot < 7; slot++) Add(mixed, slot, 3);
            var candidate = mixed.RecordCraft(3, LightGear.Item(1, 5));
            string before = JsonUtility.ToJson(mixed);
            var comparison = new GearComparisonData(candidate, mixed.EquippedRoll(3), mixed.Loadout);
            Check(comparison.bonuses.Any(x => x.key == "regen" && Math.Abs(x.Delta + .3f) < .0001f), "Comparison shows lost four-piece heavy regeneration");
            Check(comparison.bonuses.Any(x => x.key == "area" && x.Delta == 15), "Comparison shows gained four-piece cloth area");
            Check(JsonUtility.ToJson(mixed) == before, "Comparison never mutates equipment or saves");
            mixed.EquipRoll(candidate.id);
            Check(mixed.GearBonus("regen") == 0 && mixed.GearBonus("area") == 15, "Actual equip agrees with threshold preview");

            var townWithEnchants = Empty();
            for (int slot = 0; slot < 8; slot++) Add(townWithEnchants, slot, 3);
            var chest = townWithEnchants.EquippedRoll(2);
            Check(!townWithEnchants.EnchantGear(chest.id, 0, 0, "regen", 0, out _), "Unbuilt Mystic cannot enchant");
            townWithEnchants.essence = 10000;
            townWithEnchants.mysticLevel = 1;
            Check(townWithEnchants.EnchantGear(chest.id, 0, 0, "regen", 0, out _), "Mystic level one can enchant once");
            double essence = townWithEnchants.essence;
            Check(!townWithEnchants.EnchantGear(chest.id, 0, 0, "regen", 1, out _) && townWithEnchants.essence == essence, "Second slot stays locked at level one without charging");
            townWithEnchants.mysticLevel = 2;
            Check(townWithEnchants.EnchantGear(chest.id, 0, 0, "regen", 1, out _), "Mystic level two unlocks the second slot");
            essence = townWithEnchants.essence;
            Check(!townWithEnchants.EnchantGear(chest.id, 0, 0, "regen", 2, out _) && townWithEnchants.essence == essence, "A third enchantment is never allowed");
            chest.enchantments = new[] { new GearAffix { stat = "regen", value = .2f }, new GearAffix { stat = "vitality", value = 12 } };
            Check(Math.Abs(townWithEnchants.GearBonus("regen") - .5f) < .0001f && townWithEnchants.GearBonus("vitality") == 112, "Set bonuses and Mystic enchantments stack independently");
            Check(chest.enchantments.Length == 2 && townWithEnchants.Loadout.Sets().Heavy == 8, "Set bonuses consume no enchantment slots");
            int id = chest.id;
            chest.rarity = UpgradeRarity.Legendary;
            chest.affixes = new[] { new GearAffix { stat = "power", value = 999 } };
            var restored = JsonUtility.FromJson<Settlement>(JsonUtility.ToJson(townWithEnchants));
            restored.EnsureGearRolls();
            var loaded = restored.EquippedRoll(2);
            Check(loaded.id == id && loaded.slot == 2 && loaded.item == chest.item && loaded.enchantments.Length == 2 && loaded.enchantments[0].value == .2f, "Inherited instance fields and exact enchantments survive existing save JSON");
            Check(loaded.rarity == UpgradeRarity.Common && loaded.affixes.Length == 0, "Legacy armour rarity and retired craft affixes have no effect");
            Check(Math.Abs(restored.GearBonus("regen") - .5f) < .0001f && restored.GearBonus("vitality") == 112, "Set and enchantment effects survive save/load independently");
            var duplicate = restored.RecordCraft(2, loaded.item);
            Check(ReferenceEquals(loaded.ArmourDefinition, duplicate.ArmourDefinition) && loaded.id != duplicate.id && duplicate.enchantments.Length == 0, "Owned copies share definitions but keep independent enchantments");
            float strength = MysticRules.Strength(loaded, "regen");
            double cost = MysticRules.Cost(loaded, 0, 0);
            loaded.rarity = UpgradeRarity.Legendary;
            Check(MysticRules.Strength(loaded, "regen") == strength && MysticRules.Cost(loaded, 0, 0) == cost, "Armour enchanting never depends on saved rarity");
            var recipe = ArmourCatalog.Find(2, 100).Recipe;
            Check(recipe.Resources.Count == 3 && recipe.Resources[0].amount == 8 && recipe.Resources[1].amount == 2 && recipe.Resources[2].amount == 2 && recipe.Essence == 0, "Tin chest recipe remains eight bars and two of each lining");
            Check(ReferenceEquals(ArmourCatalog.Find(2, 31), ArmourCatalog.Find(2, 102)), "Legacy plate item IDs resolve to the same definition");
            Directory.CreateDirectory("Docs/Validation");
            File.WriteAllText("Docs/Validation/armour-domain.json", "{\"passed\":true,\"checks\":" + checks + "}");
            Debug.Log("VEILBOUND_ARMOUR_DOMAIN_PASS checks=" + checks);
        }

        public static void Build()
        {
            Validate();
            RawGearBuild.Validate();
            PrototypeBuild.Checks();
            AssetDatabase.SaveAssets();
            Directory.CreateDirectory("Builds/ArmourSetsFeature");
            var result = BuildPipeline.BuildPlayer(new[] { "Assets/Veilbound/Scenes/HollowCourt.unity" }, "Builds/ArmourSetsFeature/TheVeilbound.exe", BuildTarget.StandaloneWindows64, BuildOptions.Development);
            if (result.summary.result != BuildResult.Succeeded) throw new Exception("Armour feature build failed");
            Debug.Log("VEILBOUND_ARMOUR_FEATURE_BUILD_PASS");
        }
    }
}
#endif
