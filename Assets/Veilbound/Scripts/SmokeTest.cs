using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Veilbound
{
    public partial class VeilGame
    {
        void FundTestCity()
        {
            for(int family=4;family<6;family++)for(int i=0;i<5;i++)city.AddResource(family,i,3000);city.equippedMetalWeapon=-1;city.wood=5000;city.stone=5000;city.iron=3000;city.cloth=3000;city.leather=3000;city.essence=300;
        }
        void CheckEnemyRules()
        {
            StartRun();Foe normal=SpawnFoe(0,0),ranged=SpawnFoe(0,1),brute=SpawnFoe(0,2),small=SpawnFoe(0,3),volatileHusk=SpawnFoe(0,4);
            Require(normal!=null&&ranged!=null&&brute!=null&&small!=null&&volatileHusk!=null,"All enemy variants spawn from reachable edges");
            foreach(Foe f in foes)Require(!EnemySpawnPlacement.IsVisible(cameraRig,f.t),"Normal enemies enter nearby through Veil mist");
            Require(Mathf.Abs(ranged.maxHp/normal.maxHp-1.8f)<.001f,"Ranged health is 180 percent");
            Require(Mathf.Abs(brute.maxHp/normal.maxHp-2)<.001f&&Mathf.Abs(brute.contact/normal.contact-1.5f)<.001f&&brute.speed<normal.speed,"Brute health damage and slow speed");
            Require(Mathf.Abs(small.maxHp/normal.maxHp-.5f)<.001f&&Mathf.Abs(small.contact/normal.contact-.5f)<.001f&&Mathf.Abs(small.speed/normal.speed-1.4f)<.001f,"Quick ghoul has half health/damage and 140 percent speed");
            Require(small.rig.visual.kind==3&&small.rig.visual.bones.Length==17,"Quick enemy uses its dedicated skinned four-limbed ghoul rig");
            var hand=small.rig.visual.bones[7];small.rig.Animate(.03f,1,.03f);Vector3 handBefore=hand.position;
            small.rig.Animate(.1f,1,.07f);Require(Vector3.Distance(handBefore,hand.position)>.02f,"Ghoul hands animate through their ground-contact stride");
            small.rig.Animate(0,0,0);Require(small.t.InverseTransformPoint(hand.position).y<.12f,"Idle ghoul rests its hands close to the ground");
            string portraits=Path.Combine(Application.dataPath,"..","Smoke");
            MonsterPortrait(Path.Combine(portraits,"15-ghoul.png"),small.rig.root);
            MonsterPortrait(Path.Combine(portraits,"16-volatile.png"),volatileHusk.rig.root,1.35f,1.1f);
            MonsterPortrait(Path.Combine(portraits,"17-brute.png"),brute.rig.root,2.1f,1.6f,true);
            MonsterPortrait(Path.Combine(portraits,"18-lobber.png"),ranged.rig.root,1.35f,1.1f);
            Require(volatileHusk.rig.visual.bodyWidth>normal.rig.visual.bodyWidth&&volatileHusk.rig.visual.Count("VEIL - inset ember fracture")>=5,"Volatile has a swollen connected silhouette and multiple emissive belly fractures");
            int backCrystals=brute.rig.visual.Count("Brute back crystal");
            Require(backCrystals==5,"Brute keeps its large back crystal cluster");
            Require(ranged.rig.visual.bodyWidth<normal.rig.visual.bodyWidth,"Lobber has a narrower silhouette than the normal husk");
            Require(WorldArt.VeilRed.r>WorldArt.VeilRed.b*3,"Corruption palette is red");
            Foe[] rewards={normal,ranged,brute,small,volatileHusk};float[] expectedXp={2.5f,2.5f,4,1.75f,2.5f};
            for(int i=0;i<rewards.Length;i++)
            {
                int count=shards.Count;RewardFoe(rewards[i]);
                Require(shards.Count==count+1&&Mathf.Abs(shards[shards.Count-1].value-expectedXp[i])<.0001f,"Actual XP shard reward matches enemy type "+i);
                RewardFoe(rewards[i]);Require(shards.Count==count+1,"Enemy rewards cannot be collected twice");
            }
            Hit(normal,Damage);Require(foes.Contains(normal)&&normal.hp>0,"Standard survives upgraded sword hit");Hit(normal,Damage);Require(!foes.Contains(normal),"Second upgraded sword hit kills standard");
            ReturnHome();StartRun();
            var openGround=new ArenaCollision();
            for(int i=0;i<24;i++)
            {
                Vector3 direction=Quaternion.Euler(0,i*15,0)*Vector3.forward;
                Vector3 stopped=openGround.Move(direction*29,direction*15,.42f,ArenaCollision.PlayerBoundary);
                Require(stopped.magnitude+.42f<=ArenaCollision.PlayerBoundary,"Swept player movement cannot enter the cloud at any angle");
            }
            position=collision.Resolve(new Vector3(0,0,29),.42f,ArenaCollision.PlayerBoundary);hero.root.position=position;
            dodgeVector=Vector3.forward;dodgeTime=.23f;dodgeCooldown=1;Step(.23f);
            Require(position.magnitude+.42f<=ArenaCollision.PlayerBoundary,"Actual dodge stops before the Veil");
            dodgeTime=0;position=Vector3.zero;
            position=new Vector3(0,0,2.5f);hero.root.position=position;navigation.Tick(position,1,true);invulnerability=0;
            Foe bomb=SpawnFoe(0,4),victim=SpawnFoe(0,0);bomb.t.position=Vector3.zero;victim.t.position=new Vector3(1.2f,0,0);victim.speed=0;victim.attack=999;
            Vector3 fuseStart=bomb.t.position;UpdateFoes(.01f);Require(bomb.fused&&!bomb.dead,"Proximity starts a live fuse");
            Require(Vector3.Distance(fuseStart,bomb.t.position)<=bomb.speed*.0081f,"Triggered volatile movement slows to 80 percent");
            Hit(bomb,99999);Vector3 corpse=bomb.t.position;Require(bomb.dead&&Mathf.Abs(bomb.fuse-3)<.001f,"Killing volatile resets a three second stationary fuse");
            float hp=victim.hp,playerHp=health;UpdateFoes(2.98f);
            Require(foes.Contains(bomb)&&bomb.t.position==corpse&&victim.hp==hp,"Corpse stays still and explosion waits for fuse");
            UpdateFoes(.03f);Require(!foes.Contains(bomb)&&victim.hp<hp&&health<playerHp,"Explosion damages enemies and player");
            ReturnHome();StartRun();invulnerability=0;
            LobArea(new Vector3(0,1.5f,8),Vector3.zero,14);UpdateLobs(.7f);
            Require(lobs.Count==1&&lobs[0].warning&&lobs[0].t.position.y>3,"Lob arcs while its target warning remains visible");
            position=Vector3.right*5;float before=health;UpdateLobs(.71f);Require(lobs.Count==0&&health==before,"Moving out of the landing area avoids a lob");
            position=Vector3.zero;LobArea(new Vector3(0,1.5f,8),Vector3.zero,14);UpdateLobs(1.41f);Require(health<before,"Remaining in lob area takes damage");
            ReturnHome();
            Debug.Log("VEILBOUND_ENEMY_RULES_PASS");
        }
        IEnumerator SmokeTest()
        {
            string output=Path.Combine(Application.dataPath,"..","Smoke");Directory.CreateDirectory(output);
            string report=Path.Combine(output,"PASS.txt");if(File.Exists(report))File.Delete(report);
            yield return new WaitForSecondsRealtime(1);
            CheckCharacter();Require(!hero.root.Find("Pelvis/Spine/Chest/Equipped removable cape"),"Cape starts unequipped");
            CharacterPortrait(Path.Combine(output,"12-character-neutral.png"),0,0,new Vector3(2.2f,1.7f,5),true);
            CharacterPortrait(Path.Combine(output,"17-player-quiet.png"),0,0,new Vector3(2.2f,1.7f,5),true,1);
            CharacterPortrait(Path.Combine(output,"18-player-ember.png"),0,0,new Vector3(2.2f,1.7f,5),true,6.4f);
            CharacterPortrait(Path.Combine(output,"22-player-face.png"),0,0,new Vector3(1.4f,2.0f,4),true,6.4f,true);
            ScreenCapture.CaptureScreenshot(Path.Combine(output,"01-settlement.png"));yield return new WaitForSecondsRealtime(.2f);
            FundTestCity();
            foreach(string building in Settlement.Buildings){Buy(building);if(city.BuildingLevel(building)<2)Buy(building);}
            Buy("sword");Buy("sword");
            for(int slot=0;slot<9;slot++){int material=slot==8?1:3;CraftGear(slot,material);CraftGear(slot,material);}
            Mesh baseBody=hero.skin.sharedMesh;
            CraftGear(2,1);CraftGear(5,2);CraftGear(7,2);
            Require(city.Item(2)==11&&city.Item(5)==21&&city.Item(7)==21&&hero.skin.sharedMesh==baseBody,"Materials mix across slots over the unchanged connected body");
            CharacterPortrait(Path.Combine(output,"14-mixed-gear.png"),0,0,new Vector3(2.8f,1.9f,4.8f));
            city.equippedGear[2]=32;city.equippedGear[5]=32;city.equippedGear[7]=32;RebuildHero();
            CheckCharacter();Require(hero.root.GetComponentsInChildren<MeshRenderer>().Length>18,"Equipped armour adds separate visible geometry");
            CharacterPortrait(Path.Combine(output,"11-connected-plate.png"),0,0,new Vector3(2.8f,1.9f,4.8f));
            CharacterPortrait(Path.Combine(output,"10-character-back.png"),0,0,new Vector3(-2.8f,1.9f,-4.8f));
            int plateReviewCape=city.equippedGear[8];city.equippedGear[8]=0;RebuildHero();
            CharacterPortrait(Path.Combine(output,"23-plate-neutral.png"),0,0,new Vector3(1.65f,1.7f,5),true,6.4f);
            CharacterPortrait(Path.Combine(output,"24-plate-back.png"),0,0,new Vector3(-2.2f,1.7f,-5),true,1);
            CharacterPortrait(Path.Combine(output,"25-plate-detail.png"),0,0,new Vector3(1.15f,1.65f,5),true,6.4f,false,.59f,1.44f);
            CharacterPortrait(Path.Combine(output,"26-plate-sword.png"),1,.65f,new Vector3(2.8f,1.9f,4.8f),false,6.4f);
            city.equippedGear[8]=plateReviewCape;RebuildHero();
            int[] reviewGear=(int[])city.equippedGear.Clone();
            city.equippedGear=new[]{0,21,0,21,21,21,0,21,0};RebuildHero();CheckCharacter();
            CharacterPortrait(Path.Combine(output,"19-player-fitted-shoulders.png"),0,0,new Vector3(2.2f,1.7f,5),true,6.4f);
            CharacterPortrait(Path.Combine(output,"20-player-sword-pose.png"),1,.65f,new Vector3(2.8f,1.9f,4.8f),false,6.4f);
            city.equippedGear=reviewGear;RebuildHero();
            city.equippedGear[8]=0;RebuildHero();Require(!hero.root.Find("Pelvis/Spine/Chest/Equipped removable cape"),"Cape can be independently removed");
            tab=1;gearSlot=2;yield return new WaitForSecondsRealtime(.5f);
            ScreenCapture.CaptureScreenshot(Path.Combine(output,"02-forge.png"));yield return new WaitForSecondsRealtime(.2f);
            tab=2;ScreenCapture.CaptureScreenshot(Path.Combine(output,"13-workshops.png"));yield return new WaitForSecondsRealtime(.2f);
            CheckEnemyRules();
            StartRun();float originalDamage=Damage,originalHealth=MaxHealth;
            build.Pick(new Choice("power","","",false){rarity=UpgradeRarity.Legendary});
            build.Pick(new Choice("vitality","","",false){rarity=UpgradeRarity.Rare});
            Require(Mathf.Abs(Damage/originalDamage-1.3f)<.0001f&&Mathf.Abs(MaxHealth-originalHealth-30)<.001f,"Rarity strength reaches actual combat damage and health");
            for(int i=0;i<3;i++)build.Pick(new Choice("guard","","",false){rarity=UpgradeRarity.Legendary});
            invulnerability=0;float beforeHit=health;Hurt(10);
            Require(Mathf.Abs(beforeHit-health-4.4f*(1-city.Protection))<.001f,"Rare guard upgrades respect the 56 percent cap");
            ReturnHome();
            shortRun=true;StartRun();invulnerability=999;
            var controller=InputSystem.AddDevice<Gamepad>();Vector3 start=position;
            InputSystem.QueueStateEvent(controller,new GamepadState{leftStick=Vector2.right,rightStick=Vector2.up});
            yield return new WaitForSecondsRealtime(.25f);Require(position.x>start.x+.2f&&facing.z>.9f,"Independent controller move and aim");
            InputSystem.QueueStateEvent(controller,new GamepadState().WithButton(GamepadButton.South));
            yield return new WaitForSecondsRealtime(.1f);Require(dodgeCooldown>0,"Controller dodge");InputSystem.QueueStateEvent(controller,new GamepadState());InputSystem.RemoveDevice(controller);
            position=Vector3.zero;hero.root.position=position;navigation.Tick(position,1,true);
            Vector3[] spots={new Vector3(2,0,2),new Vector3(-5,0,8),new Vector3(5,0,4),new Vector3(-3,0,4),new Vector3(6,0,6)};
            for(int kind=0;kind<5;kind++){Foe f=SpawnFoe(0,kind);f.t.position=collision.Resolve(spots[kind],f.radius);}
            build.ranks["whirl"]=1;build.ranks["orbit"]=1;UpdateCombatWeapons(0);BeginWeaponSweep(Damage);
            Transform held=hero.swordPivot.Find("Equipped greatsword T"+city.equippedSword+"/Blade"),copy=weaponSweeps[0].root.Find("Equipped greatsword T"+city.equippedSword+"/Blade");
            Require(held.GetComponent<MeshFilter>().sharedMesh==copy.GetComponent<MeshFilter>().sharedMesh,"Spinning weapon still matches equipment");
            LobArea(new Vector3(-5,1.5f,8),position+Vector3.left*3,14);
            yield return new WaitForSecondsRealtime(.6f);ScreenCapture.CaptureScreenshot(Path.Combine(output,"03-combat.png"));yield return new WaitForSecondsRealtime(.2f);
            BeginWeaponSweep(Damage);ScreenCapture.CaptureScreenshot(Path.Combine(output,"07-spinning-weapon.png"));yield return new WaitForSecondsRealtime(.15f);
            xp=Threshold;yield return new WaitForSecondsRealtime(.2f);double hours=city.hours;float timer=elapsed;
            yield return new WaitForSecondsRealtime(.3f);Require(state==ScreenState.Choice&&city.hours==hours&&elapsed==timer,"Choices pause both clocks");
            ScreenCapture.CaptureScreenshot(Path.Combine(output,"04-choice.png"));yield return new WaitForSecondsRealtime(.15f);Choose(0);Require(state==ScreenState.Expedition,"Selection resumes expedition");
            EndRun(false);Save();double savedCloth=city.cloth;int savedChest=city.Item(2);city=new Settlement();Load();Require(Math.Abs(city.cloth-savedCloth)<.001&&city.Item(2)==savedChest,"New resources and independent slots survive save roundtrip");
            ScreenCapture.CaptureScreenshot(Path.Combine(output,"05-result.png"));yield return new WaitForSecondsRealtime(.2f);
            ReturnHome();shortRun=false;StartRun();invulnerability=999;
            for(int boss=1;boss<=3;boss++){elapsed=boss*300-.01f;Step(.02f);Foe f=foes.Find(x=>x.boss==boss);Require(f!=null&&!EnemySpawnPlacement.IsVisible(cameraRig,f.t),"Boss enters nearby at its timer threshold");Hit(f,999999);}
            Require(state==ScreenState.Result&&bossesDead==3,"Victory still resolves");
            ReturnHome();StartRun();position=collision.Resolve(new Vector3(0,0,32),.42f);hero.root.position=position;
            yield return new WaitForSecondsRealtime(1.5f);ScreenCapture.CaptureScreenshot(Path.Combine(output,"06-veil-boundary.png"));yield return new WaitForSecondsRealtime(.2f);
            ReturnHome();StartRun();position=new Vector3(0,0,30);hero.root.position=position;navigation.Tick(position,1,true);invulnerability=999;
            for(int i=0;i<110;i++)SpawnFoe(0,i%5);
            double sampleStart=Time.realtimeSinceStartupAsDouble;int firstFrame=Time.frameCount;
            yield return new WaitForSecondsRealtime(2);
            double fps=(Time.frameCount-firstFrame)/(Time.realtimeSinceStartupAsDouble-sampleStart);
            Debug.Log("VEILBOUND_CROWD_SAMPLE_FPS "+fps.ToString("0.0"));
            ReturnHome();StartRun();invulnerability=0;Hurt(99999);Step(.01f);Require(state==ScreenState.Result,"Death still resolves");
            File.WriteAllText(report,"PASS: connected body; independent armour geometry and removable cape; specialist workshops; resource/save migration; edge-only spawns; enemy health/damage/speed/XP ratios; four-legged ghoul animation; bloated emissive volatile, crystal-backed brute and lean lobber; red corruption palette; inner cloud movement and dodge boundary; live/dead explosive fuses and friendly fire; lob arc, warning and avoidance; controller; spinning weapon; pause clocks; bosses; victory; defeat.\n"+DateTime.UtcNow);
            Debug.Log("VEILBOUND_RUNTIME_SMOKE_PASS");Application.Quit(0);
        }
        static void Require(bool condition,string message)
        {if(!condition){Debug.LogError("SMOKE FAILED: "+message);Application.Quit(2);throw new Exception(message);}}
    }
}
