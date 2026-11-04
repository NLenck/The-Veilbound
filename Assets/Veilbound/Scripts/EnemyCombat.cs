using System.Collections.Generic;
using UnityEngine;

namespace Veilbound
{
    public partial class VeilGame
    {
        ArenaNavigation navigation;
        readonly List<Foe> foeTick=new List<Foe>();
        readonly List<Lob> lobs=new List<Lob>();
        readonly List<EnemyArrivalFog> enemyArrivalFog=new List<EnemyArrivalFog>();
        void UpdateEnemyArrivalFog(float dt){for(int i=enemyArrivalFog.Count-1;i>=0;i--)if(!enemyArrivalFog[i]||!enemyArrivalFog[i].Advance(dt))enemyArrivalFog.RemoveAt(i);}
        class Lob {public Transform t,warning;public AreaWarningVisual area;public Vector3 from,to;public float age,duration=1.4f,damage;public Foe owner;}
        Foe SpawnFoe(int boss,int forcedKind=-1)
        {
            int kind=0,variant=0;
            if(boss==0){if(forcedKind>=0){kind=forcedKind;variant=0;}else SelectEncounter(out kind,out variant);}
            float radius=boss>0?1.1f:EnemyTypes.Radius(kind);
            var rig=new HuskRig(actors,kind,boss,activeMap,variant);var t=rig.root;
            if(!EnemySpawnPlacement.TryFind(random,collision,navigation,position,radius,EnemySpawnPlacement.Measure(t),cameraRig,out Vector3 p)){t.gameObject.SetActive(false);Destroy(t.gameObject);return null;}
            t.position=p;Vector3 direction=position-p;direction.y=0;if(direction.sqrMagnitude>.001f)t.rotation=Quaternion.LookRotation(direction);
            float baseSpeed=2.1f+elapsed/Duration*.6f;
            float hp=(boss>0?260+boss*220:EnemyTypes.RegularHealth(1,elapsed/Duration,kind))*RunModifierFactor(boss>0?2:activeMap==1&&kind==2?9:0);
            var f=new Foe{spawnedAt=elapsed,spawnDamageFactor=1+.1f*EnemyPursuit.AliveMinutes(elapsed),t=t,hp=hp,maxHp=hp,boss=boss,kind=kind,variant=variant,radius=radius,rig=rig,side=random.Next(2)==0?-1:1,
                speed=boss>0?1.3f:baseSpeed*EnemyTypes.Speed(kind)*ModifierFactor(7),contact=boss>0?18:9*EnemyTypes.Damage(kind),attack=boss>0?2:kind==0||kind==3?0:2.5f};
            foes.Add(f);enemyArrivalFog.Add(EnemyArrivalFog.Create(effects,p,boss>0));if(boss>0){bossArrival[boss-1]=elapsed;notice=BossName(boss)+" is emerging from the Veil.";}return f;
        }
        void UpdateFoes(float dt)
        {
            foeTick.Clear();foeTick.AddRange(foes);
            foreach(Foe f in foeTick)
            {
                if(!foes.Contains(f))continue;
                if(AscensionInterrupted(f)){f.rig.SetMotion(Vector3.zero);f.rig.Animate(animClock,0,dt);continue;}
                Vector3 diff=position-f.t.position;diff.y=0;float d=diff.magnitude;
                if(!f.dead&&f.slamRecovery<=0&&f.slamHazard==null&&!f.chargeWarning&&f.chargeRemaining<=0&&f.chargeRecovery<=0)f.attack-=dt*ExpeditionCooldownRate(f);
                if(HandleCharge(f,dt)){RefreshVirtueMarks(f);f.hitTimer=Mathf.Max(0,f.hitTimer-dt);continue;}
                if(HandleMelee(f,dt)||HandleLobWindup(f,dt)){RefreshVirtueMarks(f);f.hitTimer=Mathf.Max(0,f.hitTimer-dt);continue;}
                if(HandleGroundSlam(f,dt)){RefreshVirtueMarks(f);f.hitTimer=Mathf.Max(0,f.hitTimer-dt);continue;}
                if(!f.dead&&d>.05f)f.t.rotation=Quaternion.LookRotation(diff);
                if(f.kind==4&&f.boss==0&&!f.fused&&d<=4.8f)StartFuse(f,false);
                Vector3 before=f.t.position;
                if(!f.dead && (d>(f.kind==1?9:f.radius+.55f)||!collision.ClearLine(f.t.position,position,.1f)))
                    f.t.position=MoveEnemyTerrain(f,f.t.position,(position-f.t.position).normalized*EnemyMovementSpeed(f)*(f.fused?.8f:1)*dt,true);
                // Separation stays within ordinary, fast and aberration groups, so
                // faster bodies and aberrations can pass through ordinary crowds.
                if(!f.dead)
                {
                    Vector3 separation=Vector3.zero;
                    foreach(Foe other in foes)if(other!=f&&!other.dead&&SeparateEnemyBodies(f,other))
                    {Vector3 v=f.t.position-other.t.position;float min=(f.radius+other.radius)*.8f;if(v.sqrMagnitude<min*min&&v.sqrMagnitude>.0001f)separation+=v.normalized*(min-v.magnitude);}
                    if(separation.sqrMagnitude>.0001f)f.t.position=MoveEnemyTerrain(f,f.t.position,Vector3.ClampMagnitude(separation,1)*dt*1.5f);
                    f.t.position=ClipEnemyMovement(f,before,f.t.position);
                    f.rig.SetMotion(dt>0?(f.t.position-before)/dt:Vector3.zero);
                    f.rig.Animate(animClock,(f.t.position-before).sqrMagnitude>.000001f?1:0,dt);
                }
                RefreshVirtueMarks(f);
                f.hitTimer=Mathf.Max(0,f.hitTimer-dt);
                if(f.fused)
                {
                    f.fuse-=dt;f.fuseRing.position=f.t.position+Vector3.up*.07f;
                    f.fuseWarning.SetProgress(1-f.fuse/3);
                    f.rig.FuseGlow(1-f.fuse/3);
                    if(f.fuse<=0){Detonate(f);continue;}
                }
                if(f.dead)continue;
                if(f.attack>0)continue;
                if(f.boss>0)
                {
                    Telegraph(position,2.4f+f.boss*.4f,1.25f,25+f.boss*7,owner:f);
                    if(f.boss>=2)for(int j=0;j<4;j++)Telegraph(f.t.position+new Vector3(Mathf.Cos(j*Mathf.PI/2),0,Mathf.Sin(j*Mathf.PI/2))*5,1.6f,1.8f,24,owner:f);
                    f.attack=4.8f-f.boss*.5f;
                }
                else if(f.kind==1&&d<14){f.lobWindup=.6f;f.lobTarget=position;}
            }
        }
        void StartFuse(Foe f,bool killed)
        {
            f.fused=true;f.fuse=3;
            if(!f.fuseRing)
            {
                f.fuseWarning=AreaWarningVisual.Create(effects,"Volatile husk 3 second blast warning",f.t.position+Vector3.up*.07f,3.4f,CombatIndicatorColors.Enemy);f.fuseRing=f.fuseWarning.transform;
                
                
            }
            f.fuseRing.position=f.t.position+Vector3.up*.07f;
            if(killed){f.dead=true;f.t.rotation*=Quaternion.Euler(55,0,12);}
        }
        void Detonate(Foe f)
        {
            Vector3 p=f.t.position;float damage=64;
            if(!f.dead)RewardFoe(f);
            foes.Remove(f);Destroy(f.t.gameObject);if(f.fuseRing)Destroy(f.fuseRing.gameObject);
            var victims=foes.ToArray();foreach(Foe other in victims)if(!other.dead&&Vector3.Distance(other.t.position,p)<=3.4f+other.radius)Hit(other,damage,"volatile",false);
            if(Vector3.Distance(position,p)<=3.82f)Hurt(damage,f);
            Burst(p,3.4f,CombatIndicatorColors.Enemy,.55f);
        }
        void LobArea(Vector3 from,Vector3 to,float damage,Foe owner=null)
        {
            var area=AreaWarningVisual.Create(effects,"Incoming lob impact area",to+Vector3.up*.06f,2.2f,CombatIndicatorColors.Enemy);var warning=area.transform;
            
            var missile=RoundedArt.Shape(effects,"Lobbed corruption",from,Vector3.one*.38f,WorldArt.VeilRed);
            missile.GetComponent<Renderer>().sharedMaterial=WorldArt.Glow(new Color(.95f,.20f,.07f));
            lobs.Add(new Lob{t=missile,warning=warning,area=area,from=from,to=to,damage=damage,owner=owner});
        }
        void UpdateLobs(float dt)
        {
            for(int i=lobs.Count-1;i>=0;i--)
            {
                Lob lob=lobs[i];lob.age+=dt;float progress=Mathf.Clamp01(lob.age/lob.duration);lob.area.SetProgress(progress);
                lob.t.position=Vector3.Lerp(lob.from,lob.to+Vector3.up*.1f,progress)+Vector3.up*Mathf.Sin(progress*Mathf.PI)*4;
                if(progress<1)continue;
                if(Vector3.Distance(position,lob.to)<2.62f)Hurt(lob.damage,lob.owner);
                Burst(lob.to,2.2f,CombatIndicatorColors.Enemy,.32f);Destroy(lob.t.gameObject);Destroy(lob.warning.gameObject);lobs.RemoveAt(i);
            }
        }
        void RewardFoe(Foe f)
        {
            if(f.rewarded)return;f.rewarded=true;f.diedAt=elapsed;AscensionOnDeath(f);kills++;
            var shard=WorldArt.Crystal(effects,f.t.position+Vector3.up*.3f,f.boss>0?.65f:.22f,f.boss>0?WorldArt.Gold:WorldArt.VeilRed);
            shards.Add(new Shard{t=shard,value=f.boss>0?Threshold*.6f:2.5f*EnemyTypes.Experience(f.kind)});
            if(f.boss>0){bossesDead++;float reward=(3+f.boss*2)*tier;earnedEssence+=reward;city.essence+=reward;Save();Burst(f.t.position,3,WorldArt.Gold,.6f);if(bossesDead==3)EndRun(true);}
        }
    }
}

