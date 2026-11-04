using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace Veilbound
{
    public partial class VeilGame
    {
        void ReviewArmourSuit(int material, int count)
        {
            city = FreshCharacterTown();
            city.InitializeWoodenStarter();
            for (int slot = 0; slot < count; slot++)
                city.EquipRoll(city.RecordCraft(slot, material == 3 ? 100 : LightGear.Item(material, 1)).id);
            RebuildHero();
            build = new RunBuild();
        }

        IEnumerator ArmourFeatureReview()
        {
            yield return new WaitForSecondsRealtime(.5f);
            combatReviewFreeze = true;
            int exceptions = 0;
            Application.LogCallback errors = (message, stack, type) => { if (type == LogType.Exception) exceptions++; };
            Application.logMessageReceived += errors;
            string folder = Path.Combine(Application.dataPath, "..", "Smoke", "ArmourSets");
            Directory.CreateDirectory(folder);

            ReviewArmourSuit(2, 7);
            float sevenRate = SkillAttackRate("cleave");
            city.EquipRoll(city.RecordCraft(7, LightGear.Item(2, 1)).id);
            Require(Mathf.Abs(SkillAttackRate("cleave") / sevenRate - 1.1f) < .0001f, "Eight leather pieces give exactly ten percent faster attacks");
            Require(Mathf.Abs(Speed - 6.72f) < .0001f, "Four leather pieces give five percent movement speed");
            ReviewArmourSuit(1, 8);
            Require(Mathf.Abs(AreaMultiplier - 1.15f) < .0001f && city.GearExperience == 1.5f, "Cloth area and XP thresholds reach combat");
            StartRun();
            spawnClock = 999;
            var shardObject = new GameObject("Armour XP regression shard");
            shardObject.transform.position = position;
            shards.Add(new Shard { t = shardObject.transform, value = 10 });
            float priorXp = xp;
            UpdateShards(0);
            Require(Mathf.Abs(xp - priorXp - 15) < .0001f, "Collecting actual XP uses the fifty-percent cloth bonus");
            ReturnHome();

            ReviewArmourSuit(3, 8);
            Require(MaxHealth == 220 && city.GearBonus("regen") == .3f, "Eight heavy pieces retain regeneration and add one hundred health");
            var chest = city.EquippedRoll(2);
            chest.enchantments = new[] { new GearAffix { stat = "regen", value = .2f }, new GearAffix { stat = "vitality", value = 12 } };
            Require(MaxHealth == 232 && chest.enchantments.Length == 2, "Set health stacks with Mystic enchantments without occupying slots");
            StartRun();
            spawnClock = 999;
            health = 100;
            Step(1);
            Require(Mathf.Abs(health - 100.5f) < .001f, "Combat tick independently adds set and enchanted regeneration");
            ReturnHome();
            tab = 1;
            notice = "";
            yield return null;
            yield return CardScreenshot(Path.Combine(folder, "heavy-eight-piece-inventory.png"));
            city.mysticLevel = 2;
            OpenMystic(chest.id);
            yield return CardScreenshot(Path.Combine(folder, "mystic-two-independent-slots.png"));
            CloseMystic();

            ReviewArmourSuit(1, 3);
            for (int slot = 3; slot < 7; slot++) city.EquipRoll(city.RecordCraft(slot, 100).id);
            var candidate = city.RecordCraft(3, LightGear.Item(1, 5));
            RebuildHero();
            ReturnHome();
            tab = 1;
            OpenGearBag(3, candidate.id);
            inventoryCompare = true;
            yield return null;
            yield return CardScreenshot(Path.Combine(folder, "comparison-crosses-two-set-thresholds.png"));
            inventoryCompare = false;
            CloseGearBag();
            CheckMystic();
            Require(exceptions == 0, "Armour feature screens render without exceptions");
            Application.logMessageReceived -= errors;
            File.WriteAllText(Path.Combine(folder, "PASS.txt"), "PASS: exact leather attack rate and movement; actual cloth XP pickup and area; cumulative heavy health/regeneration; set bonuses independent from the two Mystic slots; real combat regeneration; save-compatible domain validation; inventory/Mystic/comparison captures; existing Mystic regression checks.");
            Debug.Log("VEILBOUND_ARMOUR_FEATURE_RUNTIME_PASS");
            Application.Quit(0);
        }
    }
}
