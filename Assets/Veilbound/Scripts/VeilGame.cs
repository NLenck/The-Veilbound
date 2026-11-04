using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Veilbound
{
    public partial class VeilGame : MonoBehaviour
    {
        public enum ScreenState { Settlement, Expedition, Choice, Paused, Result, Character, Settings, CharacterSelect }
        public ScreenState state = ScreenState.Settlement;
        public Settlement city = new Settlement();
        public RunBuild build = new RunBuild();
        public int tier = 1;
        public bool shortRun, dodgeFacing;
        public float elapsed, health, xp;
        public int level = 1, kills, bossesDead;
        const int RegularEnemyCap=440;
        float spawnClock, dodgeCooldown, dodgeTime, invulnerability, saveClock, animClock, swing;
        float[] attacks = new float[5];
        int bossesSpawned;
        Vector3 position, facing = Vector3.forward, dodgeVector;
        Vector2 movement;
        Transform arena, actors, effects;
        HeroRig hero;
        readonly ArenaCollision collision=new ArenaCollision();
        VeilBoundary veil;
        Camera cameraRig;
        readonly System.Random random = new System.Random();
        readonly List<Foe> foes = new List<Foe>();
        readonly List<Shard> shards = new List<Shard>();
        readonly List<Hazard> hazards = new List<Hazard>();
        readonly List<Bolt> bolts = new List<Bolt>();
        readonly List<Flash> flashes = new List<Flash>();
        string savePath, notice = "The tower still stands. Prepare your next expedition.", resultTitle;
        double startOreUnits,startHideUnits,startTextileUnits;
        double startWood, startIron,startStone,startCloth,startLeather, earnedEssence;
        bool smoke;
        AudioSource audioSource;
        AudioClip pickSound;
        float soundCooldown;
        public float Duration => 900;
        public double Multiplier => CityTimeMultiplier(tier);
        public float MaxHealth => 120 + city.GearHealth + city.GearBonus("vitality") + build.Amount("vitality") * 20;
        float Damage => city.WeaponDamage * (1 + build.PassivePercent("power") * .01f + city.GearBonus("power")*.01f)*RageDamageMultiplier;
        float Speed => (6.4f * city.GearSpeed) * (1 + build.PassivePercent("speed") * .01f + city.GearBonus("speed")*.01f);
        internal static float ExperienceRequired(int currentLevel) => 25f + 10f * Mathf.Max(0,currentLevel-1);
        float Threshold => ExperienceRequired(level);
        public bool SimulationActive => state == ScreenState.Expedition || state == ScreenState.Settlement;
        class Foe { public float spawnedAt,diedAt=-1,spawnDamageFactor=1;public int crowdLayer=-1;public Transform t; public float hp, maxHp, speed, attack, radius; public int kind,boss,variant;public bool meleeActive,meleeLanded;public float meleeAge,lobWindup;public Vector3 strikeDirection,lobTarget; public HuskRig rig; public float hitTimer; public int side; public float contact,fuse,brandUntil,brandBonus,weakUntil,weakReduction;public Transform brandMark,weakMark;public Hazard slamHazard;public float slamRecovery;public float bleedRemaining,bleedClock,bleedPending,bleedDps;public bool dead,fused,rewarded;public Transform fuseRing;public AreaWarningVisual fuseWarning;public float chargeWindup,chargeRemaining,chargeLength,chargeRecovery,chargeCooldown;public bool chargeHit;public Vector3 chargeOrigin,chargeDirection;public ChargeWarningVisual chargeWarning; }
        class Shard { public Transform t; public float value; }
        class Hazard { public Transform t; public AreaWarningVisual warning; public Vector3 p; public float radius, remaining, duration, damage; public string source;public Foe owner;public bool directional;public Vector3 direction;public float halfAngle=55; }
        class Bolt { public Transform t; public Vector3 direction; public float life, damage; public bool hostile,fragment,split; public string source; public Foe owner; public float size=1; public int impacts; public readonly HashSet<Foe> hit = new HashSet<Foe>(); }
        class Flash { public Transform t; public float life, duration; }

        void Awake()
        {
            smoke = Array.Exists(Environment.GetCommandLineArgs(), x => x == "-veilSmoke");if(smoke)AudioListener.volume=0;
            savePath = Path.Combine(Application.persistentDataPath, smoke ? "smoke-save.json" : "settlement-v1.json");
            if(!smoke&&!CharacterAccountExists()) Load(); city.EnsureGear();BronzeHeroVisual.EquipStarterSet(city);city.EnsureGearRolls();city.EnsureVirtue();SelectHighestExpeditionTier();
            if(!smoke)InitializeCharacterProfiles(importLegacy:File.Exists(savePath));
            LoadPreferences();
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.36f,.35f,.32f);
            RenderSettings.fog = true; RenderSettings.fogColor = new Color(.075f,.083f,.075f); RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 35; RenderSettings.fogEndDistance = 85;
            var sun = new GameObject("Moonlight").AddComponent<Light>(); sun.type=LightType.Directional;
            sun.color=new Color(.98f,.94f,.86f); sun.intensity=1.05f; sun.transform.rotation=Quaternion.Euler(45,-35,0); sun.shadows=LightShadows.Soft;
            cameraRig=new GameObject("Main Camera").AddComponent<Camera>(); cameraRig.tag="MainCamera";
            cameraRig.backgroundColor=RenderSettings.fogColor; cameraRig.clearFlags=CameraClearFlags.SolidColor;
            cameraRig.orthographic=true; cameraRig.orthographicSize=12; cameraRig.farClipPlane=160;
            cameraRig.gameObject.AddComponent<AudioListener>();cameraRig.gameObject.AddComponent<VeilFogEffect>();
            arena=WorldArt.Group("Graveyard"); WorldArt.Arena(arena,collision); cameraRig.GetComponent<VeilFogEffect>().ConfigureBounds(collision);veil=new VeilBoundary(arena,collision.PlayerRadius); navigation=new ArenaNavigation(collision);
            actors=WorldArt.Group("Actors"); effects=WorldArt.Group("Effects");
            RebuildHero();
            audioSource=gameObject.AddComponent<AudioSource>(); audioSource.volume=.14f;
            pickSound=Tone(660,.12f);InitAbilityPresentation();InitMusic();
            health=MaxHealth;if(!smoke)SetState(ScreenState.CharacterSelect);
            if(smoke){InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;StartCoroutine(Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-weaponClassReview")?WeaponScalingReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-armourSetsReview")?ArmourFeatureReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-pointerScaleReview")?PointerScaleReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-armIconsReview")?ArmIconsReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-interfaceCompletionReview")?InterfaceCompletionReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-enemyPursuitReview")?EnemyPursuitReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-starterBuildingReview")?StarterBuildingReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-buildOverviewReview")?BuildOverviewReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-ascensionReview")?AscensionReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-relicPointerReview")?RelicPointerReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-choiceGridReview")?ChoiceGridReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-passiveSummaryReview")?PassiveSummaryReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-gearCostReview")?GearCostReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-gearComparisonReview")?GearComparisonReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-controllerFocusReview")?ControllerFocusReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-productionQueueReview")?ProductionQueueReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-sharedProductionReview")?SharedProductionReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-clothierWorkshopReview")?ClothierWorkshopReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-lightGearReview")?LightGearReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-countUpgradeReview")?CountUpgradeReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-riftBalanceReview")?RiftBalanceReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-bodyCollisionReview")?BodyCollisionReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-crowdMeleeReview")?CrowdMeleeReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-abilityFeedbackReview")?AbilityFeedbackReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-screenSpawnReview")?ScreenSpawnReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-enemyPressureReview")?EnemyPressureReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-combatFlowReview")?CombatFlowReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-rawGearReview")?RawGearReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-characterSelectReview")?CharacterSelectReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-modifierReview")?ExpeditionModifierReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-mysticReview")?MysticReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-settlementQueueReview")?SettlementQueueReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-musicReview")?MusicReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-arenaExpansionReview")?ArenaExpansionReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-enemyAttackReview")?EnemyAttackReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-townLayersReview")?TownLayersReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-enemyLocomotionReview")?EnemyLocomotionReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-greatswordReview")?GreatswordRuntimeReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-citySidebarReview")?CitySidebarReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-producerReview")?ProducerReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-repeatReview")?RepeatReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-mineReview")?MineReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-smithReview")?SmithReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-libraryReview")?LibraryReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-expeditionFlowReview")?ExpeditionFlowReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-townProgressionReview")?TownProgressionReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-townDailyRatesReview")?TownDailyRatesReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-veilAtmosphereReview")?VeilAtmosphereReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-graveyardReview")?GraveyardReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-weaponScalingReview")?WeaponScalingReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-bossScaleReview")?BossScaleReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-villageImplementationReview")?VillageImplementationReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-productionModelsReview")?ProductionModelsReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-characterRosterReview")?CharacterRosterReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-randomMapReview")?RandomMapReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-townReview")?RuinedTownReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-arenaReview")?GraveyardReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-groundSlamReview")?GroundSlamReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-abilityPresentationReview")?AbilityPresentationReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-rageKitReview")?RageKitReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-enemyStyleReview")?EnemyStyleReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-allArmourReview")?AllArmourReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-sanctuaryKnightReview")?SanctuaryKnightReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-sanctuaryReview")?SanctuaryCityReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-settingsNavigationReview")?SettingsNavigationReview():Array.Exists(Environment.GetCommandLineArgs(),x=>x=="-expeditionReview")?ExpeditionLoadoutReviewRunner():BronzeSmokeTest());}
        }
        AudioClip Tone(float hz,float length)
        {
            int samples=(int)(22050*length); var data=new float[samples];
            for(int i=0;i<samples;i++) data[i]=Mathf.Sin(i*hz*2*Mathf.PI/22050)*Mathf.Pow(1-(float)i/samples,2);
            var clip=AudioClip.Create("Veil tone",samples,1,22050,false); clip.SetData(data,0); return clip;
        }
        void Sound(AudioClip clip) { if(soundCooldown<=0) { audioSource.PlayOneShot(clip,clip==pickSound?preferences.menu:preferences.effects); soundCooldown=.06f; } }
        void RebuildHero()
        {
            if(hero!=null) {hero.Release();hero.root.gameObject.SetActive(false);Destroy(hero.root.gameObject);}
            hero=WorldArt.Hero(actors,city);
            hero.root.position=position;
        }
        void Load()
        {
            try { if(File.Exists(savePath)) { var loaded=JsonUtility.FromJson<Settlement>(File.ReadAllText(savePath)); if(loaded!=null && loaded.version==1) {city=loaded;if(city.ownedGear==null){city.stone=Math.Max(city.stone,80);city.quarry=Math.Max(city.quarry,1);}city.EnsureGear();} } }
            catch(Exception e) { Debug.LogWarning("Save could not be read: "+e.Message); notice="Save could not be read. A fresh settlement is in memory."; }
        }
        public void Save()
        {
            if(characterProfilesEnabled){SaveActiveCharacter();return;}
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(savePath));
                string temp=savePath+".tmp"; File.WriteAllText(temp,JsonUtility.ToJson(city,true));
                if(File.Exists(savePath)) File.Replace(temp,savePath,savePath+".bak"); else File.Move(temp,savePath);
            }
            catch(Exception e) { Debug.LogWarning("Save failed: "+e.Message); notice="Could not save progress: "+e.Message; }
        }
        void OnApplicationQuit() { Save();SavePreferences(); }
        void OnApplicationFocus(bool focus) { if(!focus && state==ScreenState.Expedition && !smoke) SetState(ScreenState.Paused); }
        int combatResumeFrame=-1;
        void SetState(ScreenState next) { if(next!=ScreenState.Settlement&&next!=ScreenState.Settings){mysticOpen=false;materialProcessingOpen=false;producerOpen=producerEditing=false;cityDetailsOpen=false;} if(next==ScreenState.Expedition&&state!=next)combatResumeFrame=Time.frameCount; if(next!=ScreenState.Settlement){resourceCategory=-1;resourceFocus=false;}state=next; selection=0; if(next==ScreenState.Choice){ResetChoiceNavigation();choiceOpenedAt=Time.unscaledTime;Array.Clear(upgradeLift,0,upgradeLift.Length);} }
        void Update()
        {
            ObserveMenuNavigationInput();
            float dt=Mathf.Min(Time.unscaledDeltaTime,.1f); soundCooldown-=dt;TickAbilityAudio();TickMusic(dt);TickTownInteriors(dt);
            if(state==ScreenState.CharacterSelect){CharacterSelectionUpdate(dt);return;}
            if(state==ScreenState.Settings){SettingsUpdate(dt);return;}
            ScreenState inputState=state;bool resourceHandled=ReadInventoryInput();if(!resourceHandled)resourceHandled=ReadSettlementQueueInput();if(!resourceHandled)resourceHandled=ReadMysticInput();if(!resourceHandled)resourceHandled=ReadClothierInput();if(!resourceHandled)resourceHandled=ReadProducerInput();if(!resourceHandled)resourceHandled=ReadSmithInput();if(!resourceHandled)resourceHandled=ReadLibraryInput();if(!resourceHandled)resourceHandled=ReadResourceController();if(!resourceHandled)resourceHandled=ReadCityController();if(!resourceHandled){MenuInput();ReadCharacterSheetInput();}
            if(!resourceHandled&&state==inputState&&(ActionPressed(ControlAction.Pause)||Keyboard.current?.escapeKey.wasPressedThisFrame==true))
            { if(resourceCategory>=0&&state==ScreenState.Settlement)resourceCategory=-1;else if(gearBagOpen&&state==ScreenState.Settlement) CloseGearBag(); else if(state==ScreenState.Expedition) SetState(ScreenState.Paused); else if(state==ScreenState.Paused) ResumeRunOrChoice(); else if(state==ScreenState.Character) CloseCharacterSheet();else if(state==ScreenState.Settlement&&tab==2&&sanctuaryWorkshopOpen){sanctuaryWorkshopOpen=false;selection=0;}else if(state==ScreenState.Settlement)OpenSettings(); }
            if(SimulationActive&&!(smoke&&combatReviewFreeze)) { city.Tick(dt,state==ScreenState.Expedition?Multiplier:1); veil.Tick(dt); }
            if(state==ScreenState.Expedition&&!(smoke&&combatReviewFreeze)) Step(dt);
            else if(state==ScreenState.Settlement)
            { hero.SetMotion(Vector3.zero);if(tab==1)StaticEquipmentPose();else{animClock+=dt; hero.root.rotation=Quaternion.Euler(0,155+Mathf.Sin(animClock*.25f)*18,0); hero.Animate(animClock,0,0);} }
            saveClock+=dt;
            if(saveClock>15) { saveClock=0; Save(); }
            if(!(smoke&&combatReviewFreeze))PositionCamera(dt);
        }
        void PositionCamera(float dt)
        {
            if(state==ScreenState.CharacterSelect){PositionCharacterSelectCamera(dt);return;}
            bool hub=state==ScreenState.Settlement||state==ScreenState.CharacterSelect;
            cameraRig.orthographicSize=Mathf.Lerp(cameraRig.orthographicSize,hub?3.8f:12,1-Mathf.Exp(-dt*5));
            Vector3 target=hero.root.position+(hub?new Vector3(-3.0f,7.5f,-10):new Vector3(0,20,-16));
            cameraRig.transform.position=Vector3.Lerp(cameraRig.transform.position,target,1-Mathf.Exp(-dt*8));
            cameraRig.transform.rotation=Quaternion.Euler(hub?32:51,0,0);
        }
        void ClearRunObjects()
        {
            ResetAscensionCombat();ResetAbilityPresentation();ResetExpeditionFlow();enemyArrivalFog.Clear();
            foreach(Transform child in actors) if(hero==null || child!=hero.root){child.gameObject.SetActive(false);Destroy(child.gameObject);}
            foreach(Transform child in effects){child.gameObject.SetActive(false);Destroy(child.gameObject);}
            foes.Clear(); shards.Clear(); hazards.Clear(); bolts.Clear(); flashes.Clear(); ClearCombatWeapons();lobs.Clear();
        }
        public void StartRun()
        {
            if(!CanEnterCharacterRun())return;
            BeginExpeditionModifiers();libraryResearchOpen=false;ClearRunObjects();RollExpeditionMap();EnsureExpeditionMap(); ResetCombatReadouts();ResetRage();gamepadCombat=false; build=new RunBuild{CritBasePercent=5+city.GearBonus("crit")}; level=1; xp=0; elapsed=0; kills=0; bossesDead=0; bossesSpawned=0;
            position=Vector3.zero; facing=Vector3.forward; hero.root.position=position;
            health=MaxHealth; spawnClock=.5f; dodgeCooldown=0; dodgeTime=0; invulnerability=0; attacks=new float[5];
            startOreUnits=city.ResourceTotal(0);startHideUnits=city.ResourceTotal(1);startTextileUnits=city.ResourceTotal(2);
            earnedEssence=0; startWood=city.wood; startIron=city.iron;startStone=city.stone;startCloth=city.cloth;startLeather=city.leather;
            navigation.Tick(position,1,true);SetState(ScreenState.Expedition); notice=""; Save();
        }
        void ReadCombatActionInput()
        {
            // Menu confirmation and gameplay share bindings. Consume the transition
            // frame so the press that closes a menu cannot also cast or dodge.
            if(Time.frameCount<=combatResumeFrame)return;
            ReadRageInput();
            if(ActionPressed(ControlAction.Dodge)||ActionPressed(ControlAction.DodgeAlt))TryDodge();
        }
        void Step(float dt)
        {
            elapsed+=dt;UpdateDamageReadouts(dt); animClock+=dt; invulnerability-=dt; dodgeCooldown-=dt;
            TickRage(dt);ReadCombatControls();
            var k=Keyboard.current;var pad=Gamepad.current;
            ReadCombatActionInput();
            Vector3 beforePlayerMove=position;
            if(dodgeTime>0) { position=MovePlayerBody(position,dodgeVector*22*dt); dodgeTime-=dt; }
            else position=MovePlayerBody(position,new Vector3(movement.x,0,movement.y)*Speed*dt);
            hero.SetMotion(dt>0?(position-beforePlayerMove)/dt:Vector3.zero);
            hero.root.position=position;
            hero.root.rotation=Quaternion.LookRotation(facing);
            UpdatePlayerPresentation(dt);swing=Mathf.Max(0,swing-dt*4); hero.Animate(animClock,movement.magnitude,swing);
            health=Mathf.Min(MaxHealth,health+(build.Amount("regen")*.3f+city.GearBonus("regen"))*dt);
            spawnClock-=dt;
            if(spawnClock<=0 && foes.Count<RegularEnemyCap) { SpawnFoe(0); spawnClock=Mathf.Lerp(1.45f,.48f,Mathf.Clamp01(elapsed/Duration))/(RunModifierFactor(5)); }
            if(bossesSpawned<3 && elapsed >= Duration/3*(bossesSpawned+1)) {if(SpawnFoe(bossesSpawned+1)!=null)bossesSpawned++;}
            navigation.Tick(position,dt); TickVirtueAbilities(dt); Attack(dt); UpdateCombatWeapons(dt); UpdateFoes(dt); UpdateEnemyArrivalFog(dt); UpdateLobs(dt); UpdateHazards(dt); UpdateBolts(dt); TickAscensionEffects(dt); UpdateShards(dt); UpdateRelics(dt); UpdateFlashes(dt);
            if(state!=ScreenState.Expedition) return;
            if(health<=0) EndRun(false);
            else if(xp>=Threshold) { xp-=Threshold; level++; build.Roll(random,level); SetState(ScreenState.Choice); Sound(pickSound); }
        }
        string BossName(int b) {var catalog=ProductionModelCatalog.Current;if(catalog){var enemy=catalog.Enemy(ProductionEnemySelection.Pick(activeMap,0,b));if(enemy!=null)return enemy.displayName.ToUpperInvariant();}return b==1?"THE ASH WARDEN":b==2?"THE VEIL CANTOR":"THE HOLLOW REGENT";}
        void Hurt(float amount,Foe source=null)
        {
            if(invulnerability>0||state!=ScreenState.Expedition||health<=0||amount<=0)return;
            bool counter=TryVengeance((float)retaliationRandom.NextDouble());
            float lost=Mathf.Min(Mathf.Max(0,health),AbsorbAscensionDamage(amount*ExpeditionDamageFactor(source)*EnemyDamageMultiplier(source)*(1-Mathf.Min(.56f,build.PassivePercent("guard")*.01f))*(1-city.Protection)*(counter?1-VengeanceProtection:1)));
            health-=lost;AscensionDamageTaken(lost);GainRage(lost,false);AddDamageNumber(position+Vector3.up*1.8f,lost,false,false);
            invulnerability=.65f;Burst(position,.8f,new Color(1,.2f,.15f),.18f);
            if(counter&&lost>0)Counterattack();
        }
        float Hit(Foe f,float damage,string source="cleave",bool playerDamage=true)
        {
            if(f.dead||!foes.Contains(f))return 0;
            damage=Mathf.Max(0,damage);if(playerDamage&&f.brandUntil>elapsed)damage*=1+f.brandBonus;
            bool critical=playerDamage&&source!="rage"&&source!="rend"&&criticalRandom.NextDouble()<CritChance;if(critical)damage*=CritMultiplier;
            float applied=Mathf.Min(Mathf.Max(0,f.hp),damage);if(playerDamage){GainRage(applied,true);RecordAbilityDamage(source,applied,critical);AddDamageNumber(f.t.position+Vector3.up*(f.kind==3&&f.boss==0?.9f:1.7f*f.t.localScale.y),applied,true,critical);}
            f.hp-=damage;if(playerDamage)AscensionOnHit(f,applied,source);f.hitTimer=4;f.rig.Hit();
            if(f.hp>0){if(playerDamage&&applied>0)TryRend(f,source,(float)retaliationRandom.NextDouble());Burst(f.t.position+Vector3.up,.22f,playerDamage?CombatIndicatorColors.Player:CombatIndicatorColors.Enemy,.13f);return applied;}
            CancelEnemyCharge(f);RewardFoe(f);if(f.kind==4&&f.boss==0){StartFuse(f,true);return applied;}
            foes.Remove(f);Destroy(f.t.gameObject);return applied;
        }
        void AreaDamage(Vector3 p,float radius,float damage,bool cone=false,string source="cleave")
        {
            foreach(Foe f in foes.ToArray())
            {
                if(f.dead)continue;Vector3 diff=f.t.position-p;
                if(diff.magnitude<radius+f.radius && (!cone||Vector3.Dot(diff.normalized,facing)>.2f) && collision.ClearLine(p,f.t.position)) Hit(f,damage,source);
            }
        }
        void Attack(float dt)
        {
            for(int i=0;i<5;i++) attacks[i]-=dt*SkillAttackRate(AbilitySpecs.Weapons[i]);
            if(attacks[0]<=0)
            {
                ExecuteBasicAttack(); attacks[0]=SkillBaseCooldown("cleave");
            }
            if(build.Amount("whirl")>0 && attacks[1]<=0)
            { BeginWeaponSweep(AbilityDamage("whirl")); attacks[1]=SkillBaseCooldown("whirl"); }
            if(build.Amount("lance")>0 && attacks[2]<=0)
            { ExecuteRiftBlades(); attacks[2]=SkillBaseCooldown("lance"); }
            if(build.Amount("orbit")>0 && attacks[3]<=0)
            {
                AscendedOrbitAttack(); attacks[3]=SkillBaseCooldown("orbit");
            }
            if(build.Amount("smite")>0 && attacks[4]<=0 && foes.Count>0)
            {
                AscendedJudgement(); attacks[4]=SkillBaseCooldown("smite");
            }
        }
        void Arc(Vector3 p,float radius)
        {
            Transform t=WorldArt.Group("Sword crescent",effects); t.position=p+Vector3.up*.9f; t.rotation=Quaternion.LookRotation(facing);
            var l=t.gameObject.AddComponent<LineRenderer>(); l.useWorldSpace=false; l.positionCount=18; l.widthMultiplier=.12f;
            l.sharedMaterial=WorldArt.Glow(CombatIndicatorColors.Player);
            for(int i=0;i<18;i++) { float a=Mathf.Lerp(-1.25f,1.25f,i/17f); l.SetPosition(i,new Vector3(Mathf.Sin(a)*radius,0,Mathf.Cos(a)*radius)); }
            flashes.Add(new Flash{t=t,life=.2f,duration=.2f});
        }
        void Burst(Vector3 p,float radius,Color c,float duration)
        {
            var t=WorldArt.Group("Impact",effects); t.position=p;
            WorldArt.Ring(t,Vector3.up*.08f,radius,c,.1f,32);
            flashes.Add(new Flash{t=t,life=duration,duration=duration});
        }
        void UpdateFlashes(float dt)
        {
            for(int i=flashes.Count-1;i>=0;i--) { var f=flashes[i]; f.life-=dt; if(f.life<=0) { Destroy(f.t.gameObject); flashes.RemoveAt(i); } else f.t.localScale=Vector3.one*(1+(1-f.life/f.duration)*.18f); }
        }
        void Telegraph(Vector3 p,float radius,float seconds,float damage,string source="smite",Foe owner=null)
        {
            if(damage<0&&source=="smite")PlayerCast("smite",p,radius);
            var warning=AreaWarningVisual.Create(effects,"Telegraph",p+Vector3.up*.06f,radius,damage>0?CombatIndicatorColors.Enemy:CombatIndicatorColors.Player); var t=warning.transform;
            
            hazards.Add(new Hazard{t=t,warning=warning,p=p,radius=radius,remaining=seconds,duration=seconds,damage=damage,source=source,owner=owner});
        }
        void UpdateHazards(float dt)
        {
            for(int i=hazards.Count-1;i>=0;i--)
            {
                var h=hazards[i];if(SlamCancelled(h)){if(h.owner!=null)h.owner.slamHazard=null;Destroy(h.t.gameObject);hazards.RemoveAt(i);continue;}h.remaining-=dt;
                h.warning.SetProgress(1-h.remaining/h.duration);
                if(h.remaining>0) continue;
                if(h.damage>0) { if((h.directional?EnemyAttackGeometry.SectorContains(position,h.p,h.direction,h.radius,h.halfAngle,.35f):Vector3.Distance(position,h.p)<h.radius+.35f)&&(h.source!="enemy-slam"||collision.ClearLine(h.p,position,.1f))) Hurt(h.damage,h.owner); }
                else {AreaDamage(h.p,h.radius,-h.damage,false,h.source);if(h.source=="smite"){PlayAbilitySound("smite-impact");PlayerEffect("smite-impact",h.p,h.radius,.45f);}}
                if(h.source=="enemy-slam")CompleteGroundSlam(h);
                if(!h.directional)Burst(h.p,h.radius,h.damage>0?CombatIndicatorColors.Enemy:CombatIndicatorColors.Player,.3f);
                Destroy(h.t.gameObject); hazards.RemoveAt(i);
            }
        }
        void FireBolt(Vector3 p,Vector3 dir,float damage,bool hostile,string source="lance")
        {
            var t=hostile?WorldArt.Part(effects,"Enemy bolt",PrimitiveType.Cube,p,Vector3.one*.3f,new Color(1,.35f,.2f)):CreateThrownDagger(p,dir,SkillArea("lance"));
            t.rotation=Quaternion.LookRotation(dir);if(!hostile&&source=="lance"&&Ascended("lance",1)){t.localScale*=1.5f;damage*=1.2f;} bolts.Add(new Bolt{t=t,direction=dir,life=!hostile&&source=="lance"?RiftBladeRange/RiftBladeSpeed:3,damage=damage,hostile=hostile,source=source,size=hostile?1:SkillArea("lance")*(Ascended("lance",1)?1.5f:1)});
        }
        void UpdateBolts(float dt)
        {
            for(int i=bolts.Count-1;i>=0;i--)
            {
                var b=bolts[i]; Vector3 previous=b.t.position; b.t.position+=b.direction*(b.hostile?7:RiftBladeSpeed)*(!b.hostile&&b.source=="lance"?Mathf.Min(dt,Mathf.Max(0,b.life)):dt); b.life-=dt;if(!b.hostile){var model=b.t.Find("Dagger spin");if(model)model.Rotate(0,0,dt*720,Space.Self);}
                if(!collision.ClearLine(previous,b.t.position,.12f)) { Destroy(b.t.gameObject); bolts.RemoveAt(i); continue; }
                if(b.hostile) { if(Vector3.Distance(b.t.position,position+Vector3.up*.7f)<.65f) { Hurt(b.damage,b.owner); b.life=0; } }
                else foreach(var f in foes.ToArray()) { if(f.dead)continue; if(!b.hit.Contains(f) && new Vector2(b.t.position.x-f.t.position.x,b.t.position.z-f.t.position.z).magnitude<f.radius+.6f*b.size) { b.hit.Add(f);b.impacts++; Hit(f,b.damage,b.source);AscensionBoltHit(b,f);if(b.source=="lance"&&b.impacts>=RiftBladeMaxHits){b.life=0;break;} } }
                if(b.life<=0) { Destroy(b.t.gameObject); bolts.RemoveAt(i); }
            }
        }
        void UpdateShards(float dt)
        {
            for(int i=shards.Count-1;i>=0;i--)
            {
                var s=shards[i]; float d=Vector3.Distance(s.t.position,position);
                s.t.Rotate(0,dt*120,0,Space.World);
                if(d<3.1f*(1+build.Amount("reach")*.25f)) s.t.position=Vector3.MoveTowards(s.t.position,position+Vector3.up*.3f,dt*12);
                if(d<.8f) { xp+=s.value*city.GearExperience*(1+build.PassivePercent("wisdom")*.01f); Destroy(s.t.gameObject); shards.RemoveAt(i); }
            }
            // Bound mesh count without discarding earned XP: merge distant pickups.
            if(shards.Count>220) { shards[0].value+=shards[1].value; Destroy(shards[1].t.gameObject); shards.RemoveAt(1); }
        }
        public void Choose(int i)
        {
            if(i<0 || i>=build.offers.Count || state!=ScreenState.Choice) return;
            var c=build.offers[i]; build.Pick(c);if(c.ability&&build.BeginAscension()){SetState(ScreenState.Choice);Sound(pickSound);return;} if(c.id=="vitality") health=Mathf.Min(MaxHealth,health+20*c.Strength);
            if(build.PassiveRound){relicMessage="RELIC: "+c.name+" — "+build.Describe(c);relicMessageUntil=elapsed+6;}
            SetState(ScreenState.Expedition); Sound(pickSound);
        }
        void EndRun(bool won)
        {
            if(state==ScreenState.Result) return;
            if(won) { city.clearedTier=Math.Max(city.clearedTier,tier);city.expeditionBestDepth=Math.Max(city.expeditionBestDepth,VeilDepth); city.victories++; }
            resultTitle=won?"THE ABERRATIONS FALL":"THE VEIL TAKES ITS TOLL";
            ResetRage();SetState(ScreenState.Result); Save();
        }
        void ReturnHome()
        { sanctuaryWorkshopOpen=false;industryPage=0;ResetRage();ClearRunObjects(); position=Vector3.zero; hero.root.position=position; SetState(ScreenState.Settlement);SelectHighestExpeditionTier(); Save(); }
        void Buy(string kind)
        {
            if(kind=="sword")
            {
                int n=city.sword;
                if(n>=2){notice="This greatsword is fully forged.";return;}
                if(city.blacksmith<n+1){notice="Requires Blacksmith level "+(n+1)+".";return;}
                if(!city.Spend(40*(n+1),25*(n+1),n==1?8:0)){notice="More timber, iron or essence is needed.";return;}
                city.sword++;city.equippedSword=city.sword;notice="Greatsword forged and equipped.";RebuildHero();
            }
            else city.UpgradeBuilding(kind,out notice);
            Sound(pickSound);Save();
        }
        void CraftGear(int slot,int material)
        {
            if(city.Craft(slot,material,out notice)){RebuildHero();Sound(pickSound);Save();OpenGearBag(slot,city.lastCraftedRollId);}
        }
    }
}















