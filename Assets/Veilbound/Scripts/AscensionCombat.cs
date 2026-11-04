using System;using System.Collections.Generic;using UnityEngine;
namespace Veilbound {
 public partial class VeilGame {
  sealed class AscendedFoe {public float stored,comboUntil,judgementUntil,markedUntil,interruptUntil,lastBleedTick=-1;public int brandHits,combo,judgements,bleedStacks,generation;}
  readonly Dictionary<Foe,AscendedFoe> ascendedFoes=new Dictionary<Foe,AscendedFoe>();
  sealed class AscensionZone {public Transform root;public Vector3 from,to;public float radius,life,clock,interval,damage;public string source;public bool follow,line;}
  sealed class AscensionEvent {public Vector3 point;public float damage,radius,duration;public string source;public bool bleed;public int generation;}
  sealed class AscensionFlight {public Transform root;public Vector3 start,end;public Foe target;public float age,duration,damage,radius;public string source;public bool returning;public HashSet<Foe> outward=new HashSet<Foe>(),inward=new HashSet<Foe>();}
  sealed class BloodTether {public Foe foe;public LineRenderer line;public float life=3,clock=.5f;}
  readonly List<AscensionZone> ascensionZones=new List<AscensionZone>();
  readonly Queue<AscensionEvent> ascensionEvents=new Queue<AscensionEvent>();
  readonly List<AscensionFlight> ascensionFlights=new List<AscensionFlight>();
  readonly List<BloodTether> bloodTethers=new List<BloodTether>();
  readonly List<Transform> crownSwords=new List<Transform>();
  float ascensionShield,shieldUntil,bloodDebt,reckoningUntil,hymnUntil,hymnHaste,rageExtension;Transform hymnRing;int waltzSide;
  void ResumeRunOrChoice(){SetState(build.AscensionRound?ScreenState.Choice:ScreenState.Expedition);}
  bool Ascended(string id,int branch)=>build.Ascension(id)==branch;
  AscendedFoe AscensionState(Foe f){if(!ascendedFoes.TryGetValue(f,out var data)){data=new AscendedFoe();ascendedFoes[f]=data;}return data;}
  string AbilityDisplayName(string id){var spec=AbilityAscensions.Get(id,build.Ascension(id));return spec!=null?spec.name:Array.Find(RunBuild.Pool,c=>c.id==id)?.name??id;}
  void ResetAscensionCombat(){ascendedFoes.Clear();ascensionZones.Clear();ascensionFlights.Clear();ascensionEvents.Clear();bloodTethers.Clear();crownSwords.Clear();ascensionShield=shieldUntil=bloodDebt=reckoningUntil=hymnUntil=hymnHaste=rageExtension=0;hymnRing=null;waltzSide=0;}
  bool AscensionInterrupted(Foe f)=>ascendedFoes.TryGetValue(f,out var data)&&data.interruptUntil>elapsed;
  LineRenderer AscensionLine(string name,Vector3 from,Vector3 to,float width){var root=WorldArt.Group(name,effects);var line=root.gameObject.AddComponent<LineRenderer>();line.useWorldSpace=true;line.positionCount=2;line.SetPosition(0,from);line.SetPosition(1,to);line.widthMultiplier=width;line.sharedMaterial=WorldArt.Glow(CombatIndicatorColors.Player);return line;}
  void LineStrike(Vector3 from,Vector3 direction,float length,float width,float damage,string source){
   Vector3 end=from+direction.normalized*length;var visual=AscensionLine(AbilityDisplayName(source),from+Vector3.up*.16f,end+Vector3.up*.16f,.10f);flashes.Add(new Flash{t=visual.transform,life=.3f,duration=.3f});
   foreach(var f in foes.ToArray())if(!f.dead&&EnemyAttackGeometry.SegmentDistance(f.t.position,from,end)<=width+f.radius&&collision.ClearLine(from,f.t.position,.05f))Hit(f,damage,source);
  }
  void AscendedBasicSwing(string source){
   float damage=AbilityDamage("cleave");if(source=="vengeance"&&Ascended("vengeance",2)){damage+=bloodDebt*2;bloodDebt=0;}
   PlayerCast(source,position,SkillRadius("cleave"));
   if(Ascended("cleave",2)&&!(source=="vengeance"&&Ascended("vengeance",1))){
    Vector3 direction=Quaternion.Euler(0,(waltzSide++%2==0?-1:1)*22,0)*facing;
    foreach(var f in foes.ToArray()){Vector3 delta=f.t.position-position;if(f.dead||delta.magnitude>SkillRadius("cleave")+f.radius||Vector3.Dot(delta.normalized,direction)<0||!collision.ClearLine(position,f.t.position))continue;
     Hit(f,damage,source);if(f.dead||f.hp<=0)continue;var data=AscensionState(f);data.combo=elapsed<=data.comboUntil?data.combo+1:1;data.comboUntil=elapsed+3;if(data.combo>=4){data.combo=0;Hit(f,damage*1.2f,source);Burst(f.t.position,1,CombatIndicatorColors.Player,.25f);}
    }
   }else AreaDamage(position,SkillRadius("cleave"),damage,!(source=="vengeance"&&Ascended("vengeance",1)),source);
   if(Ascended("cleave",1))LineStrike(position,facing,8*SkillArea("cleave"),.7f*SkillArea("cleave"),damage*.65f,source);
   if(RageActive&&Ascended("rage",2))LineStrike(position,facing,5*SkillArea("rage"),1,damage*.35f,"rage");
   Arc(position,SkillRadius("cleave"));swing=1;hero?.PlayBasicAttack(AbilityCooldown("cleave"));
  }
  void AddFlight(string source,float damage,Vector3 start,Vector3 end,Foe target=null){
   var root=WorldArt.Group(AbilityDisplayName(source),effects);root.position=start+Vector3.up;SpinningSword(root,Vector3.zero);root.localScale=Vector3.one*SkillArea(source);
   ascensionFlights.Add(new AscensionFlight{root=root,start=start,end=end,target=target,damage=damage,duration=source=="whirl"?1.4f:.8f,radius=source=="whirl"?1.3f:.7f,source=source});
  }
  bool BeginAscendedWhirl(float damage){
   if(Ascended("whirl",2)){AddFlight("whirl",damage*.9f,position,position+facing*8*SkillArea("whirl"));return true;}
   if(!Ascended("whirl",1))return false;
   var zone=ascensionZones.Find(z=>z.source=="whirl"&&z.follow);if(zone!=null){zone.life=3;zone.damage=damage*.3f;return true;}
   var root=WorldArt.Group("Eye of the Reaper",effects);SpinningSword(root,new Vector3(0,0,1.65f));root.localScale=Vector3.one*SkillArea("whirl");
   ascensionZones.Add(new AscensionZone{root=root,follow=true,radius=SkillRadius("whirl"),life=3,clock=.3f,interval=.3f,damage=damage*.3f,source="whirl"});return true;
  }
  Vector3 CrownPosition(int index)=>OathPositionAt(index);
  Foe NearestAscensionTarget(Vector3 from,float range=100,Foe exclude=null){Foe chosen=null;float best=range*range;foreach(var f in foes)if(f!=exclude&&!f.dead&&f.hp>0){float distance=(f.t.position-from).sqrMagnitude;if(distance<best&&collision.ClearLine(from,f.t.position,.05f)){best=distance;chosen=f;}}return chosen;}
  void AscendedOrbitAttack(){
   for(int i=0;i<OathBladeCount;i++){
    Vector3 p=OathPositionAt(i);
    if(Ascended("orbit",2)){var target=NearestAscensionTarget(position,12*SkillArea("orbit"));if(target!=null){int close=0;foreach(var f in foes)if(!f.dead&&(f.t.position-target.t.position).sqrMagnitude<16)close++;AddFlight("orbit",AbilityDamage("orbit")*(close<=1?1.3f:.9f),p,target.t.position,target);}}
    else{if(!Ascended("orbit",1))PlayerCast("orbit",p,SkillRadius("orbit"));AreaDamage(p,SkillRadius("orbit"),AbilityDamage("orbit"),false,"orbit");}
   }
  }

  void AscendedJudgement(){
   if(build.Ascension("smite")==0){Foe nearest=null;foreach(var f in foes)if(!f.dead&&(nearest==null||(f.t.position-position).sqrMagnitude<(nearest.t.position-position).sqrMagnitude))nearest=f;if(nearest!=null)Telegraph(nearest.t.position,SkillRadius("smite"),.5f,-AbilityDamage("smite"));return;}
   if(Ascended("smite",1)){Foe chosen=null;foreach(var f in foes)if(!f.dead&&collision.ClearLine(position,f.t.position)&&(chosen==null||f.maxHp>chosen.maxHp||Mathf.Abs(f.maxHp-chosen.maxHp)<.01f&&(f.t.position-position).sqrMagnitude<(chosen.t.position-position).sqrMagnitude))chosen=f;
    if(chosen==null)return;var data=AscensionState(chosen);data.judgements=elapsed<=data.judgementUntil?Mathf.Min(6,data.judgements+1):0;data.judgementUntil=elapsed+10;Telegraph(chosen.t.position,SkillRadius("smite"),.5f,-AbilityDamage("smite")*(1+data.judgements*.2f));return;
   }
   var targets=new List<Foe>();foreach(var f in foes)if(!f.dead&&collision.ClearLine(position,f.t.position))targets.Add(f);targets.Sort((a,b)=>(a.t.position-position).sqrMagnitude.CompareTo((b.t.position-position).sqrMagnitude));
   int count=Ascended("smite",2)?Mathf.Min(3,targets.Count):Mathf.Min(1,targets.Count);
   for(int i=0;i<count;i++){if(Ascended("smite",2))AscensionState(targets[i]).markedUntil=elapsed+2;Telegraph(targets[i].t.position,SkillRadius("smite"),.5f,-AbilityDamage("smite")*(Ascended("smite",2)?.85f:1));}
  }
  void AscensionRiftCast(){
   if(!Ascended("lance",1))return;Vector3 end=position+facing*14*SkillArea("lance");var line=AscensionLine("Veilbreaker rift",position+Vector3.up*.12f,end+Vector3.up*.12f,.22f*SkillArea("lance"));
   ascensionZones.Add(new AscensionZone{root=line.transform,from=position,to=end,line=true,radius=.8f*SkillArea("lance"),life=2,clock=.5f,interval=.5f,damage=AbilityDamage("lance")*.18f,source="lance"});
  }
  void AscensionBoltHit(Bolt bolt,Foe hit){
   if(!Ascended("lance",2)||bolt.hostile||bolt.source!="lance"||bolt.fragment||bolt.split)return;bolt.split=true;
   for(int i=0;i<2;i++){var target=NearestAscensionTarget(hit.t.position,10,hit);Vector3 direction=target!=null?(target.t.position-bolt.t.position).normalized:Quaternion.Euler(0,i==0?-35:35,0)*bolt.direction;if(direction.sqrMagnitude<.001f)direction=bolt.direction;
    direction=Quaternion.Euler(0,i==0?-12:12,0)*direction;FireBolt(bolt.t.position,direction,bolt.damage*.65f,false);var fragment=bolts[bolts.Count-1];fragment.fragment=true;fragment.hit.Add(hit);fragment.size*=.65f;fragment.t.localScale*=.65f;
   }
  }
  void ApplyAscendedBrand(Foe f){var data=AscensionState(f);if(f.brandUntil<=elapsed)data.brandHits=0;f.brandUntil=elapsed+BrandDuration;f.brandBonus=Mathf.Min(.95f,BrandBonus+(Ascended("brand",2)?data.brandHits*.03f:0));StatusMark(f,true);}
  void AscensionOnHit(Foe f,float actual,string source){
   if(actual<=0)return;
   if(f.brandUntil>elapsed&&source!="brand"&&build.Ascension("brand")>0){var data=AscensionState(f);if(Ascended("brand",1))data.stored+=actual*.25f;else{data.brandHits++;f.brandBonus=Mathf.Min(.95f,BrandBonus+data.brandHits*.03f);}}
   if(source=="rend"&&Ascended("rend",2))AscensionState(f).lastBleedTick=elapsed;
  }
  void AscensionOnDeath(Foe f){
   if(RageActive&&Ascended("rage",1)){float extra=Mathf.Min(.5f,6-rageExtension);rageExtension+=extra;rageRemaining+=extra;rageWindowDuration+=extra;}
   ascendedFoes.TryGetValue(f,out var data);
   if(Ascended("brand",1)&&data!=null&&data.stored>0){ascensionEvents.Enqueue(new AscensionEvent{point=f.t.position,radius=3*SkillArea("brand"),damage=data.stored,source="brand"});data.stored=0;}
   if(Ascended("brand",2)&&f.brandUntil>elapsed){var next=NearestAscensionTarget(f.t.position,8,f);if(next!=null)ApplyAscendedBrand(next);}
   if(Ascended("smite",2)&&data!=null&&data.markedUntil>elapsed)ascensionEvents.Enqueue(new AscensionEvent{point=f.t.position,radius=2*SkillArea("smite"),damage=AbilityDamage("smite")*.45f,source="smite"});
   if(Ascended("rend",2)&&(f.bleedRemaining>0||data!=null&&Mathf.Abs(data.lastBleedTick-elapsed)<.0001f)&&(data==null||data.generation<6))ascensionEvents.Enqueue(new AscensionEvent{point=f.t.position,radius=3.5f,damage=f.bleedDps*.65f,duration=RendDuration,bleed=true,generation=(data?.generation??0)+1,source="rend"});
   ascendedFoes.Remove(f);
  }
  float AscensionBleedMultiplier(Foe f,bool fresh){if(build.Ascension("rend")==0)return 1;var data=AscensionState(f);data.generation=0;data.bleedStacks=fresh?1:Ascended("rend",1)?Mathf.Min(5,data.bleedStacks+1):1;return data.bleedStacks;}
  void AscendedWarCry(){
   if(Ascended("warcry",1))foreach(var f in foes)if(!f.dead&&Vector3.Distance(position,f.t.position)<=SkillRadius("warcry")+f.radius&&collision.ClearLine(position,f.t.position)){
    AscensionState(f).interruptUntil=elapsed+(f.boss>0?.15f:.8f);if(f.boss>0)continue;f.meleeActive=false;f.lobWindup=0;CancelEnemyCharge(f);f.chargeCooldown=Mathf.Max(1,f.chargeCooldown);f.attack=Mathf.Max(.8f,f.attack);
    if(f.slamHazard!=null){hazards.Remove(f.slamHazard);Destroy(f.slamHazard.t.gameObject);f.slamHazard=null;f.slamRecovery=.8f;}
   }
   if(Ascended("warcry",2)){hymnUntil=elapsed+WarCryDuration;if(!hymnRing){hymnRing=WorldArt.Group("Battle Hymn aura",effects);WorldArt.Ring(hymnRing,Vector3.up*.08f,SkillRadius("warcry"),CombatIndicatorColors.Player,.055f,48);}}
  }
  void HealAscendedBlood(float actual,bool barrier){float restored=Mathf.Min(BloodHealCap,actual*BloodHealing),missing=MaxHealth-health;health=Mathf.Min(MaxHealth,health+restored);if(barrier&&restored>missing){ascensionShield=Mathf.Min(MaxHealth*.25f,ascensionShield+restored-missing);shieldUntil=elapsed+5;}}
  bool CastAscendedBlood(){
   if(build.Ascension("bloodthirst")==0)return false;
   float total=0;PlayerCast("bloodthirst",position,SkillRadius("bloodthirst"));
   if(Ascended("bloodthirst",1)){float radius=SkillRadius("bloodthirst")*1.6f;foreach(var f in foes.ToArray())if(!f.dead&&Vector3.Distance(position,f.t.position)<=radius+f.radius&&collision.ClearLine(position,f.t.position))total+=Hit(f,AbilityDamage("bloodthirst")*1.2f,"bloodthirst");HealAscendedBlood(total,true);Burst(position,radius,CombatIndicatorColors.Player,.5f);}
   else{var targets=new List<Foe>();foreach(var f in foes)if(!f.dead&&(f.t.position-position).sqrMagnitude<Mathf.Pow(7*SkillArea("bloodthirst"),2)&&collision.ClearLine(position,f.t.position))targets.Add(f);targets.Sort((a,b)=>(a.t.position-position).sqrMagnitude.CompareTo((b.t.position-position).sqrMagnitude));
    for(int i=0;i<Mathf.Min(3,targets.Count);i++){var f=targets[i];total+=Hit(f,AbilityDamage("bloodthirst")*1.5f,"bloodthirst");if(f.hp<=0)continue;var old=bloodTethers.Find(t=>t.foe==f);if(old!=null){old.life=3;continue;}bloodTethers.Add(new BloodTether{foe=f,line=AscensionLine("Heartseeker tether",position+Vector3.up,f.t.position+Vector3.up,.05f)});}HealAscendedBlood(total,false);
   }
   bloodClock=SkillBaseCooldown("bloodthirst");return true;
  }
  float AbsorbAscensionDamage(float incoming){if(elapsed<reckoningUntil)incoming*=.7f;if(ascensionShield>0&&shieldUntil>elapsed){float absorbed=Mathf.Min(ascensionShield,incoming);ascensionShield-=absorbed;incoming-=absorbed;}return incoming;}
  void AscensionDamageTaken(float actual){if(Ascended("vengeance",2))bloodDebt=Mathf.Min(MaxHealth*.5f,bloodDebt+actual);}
  void TickAscensionEffects(float dt){
   if(shieldUntil<=elapsed)ascensionShield=0;
   hymnHaste=hymnUntil>elapsed&&NearbyEnemy(SkillRadius("warcry"))?12+2*build.Attribute("warcry","effect"):0;
   if(hymnRing){hymnRing.position=position;if(hymnUntil<=elapsed){Destroy(hymnRing.gameObject);hymnRing=null;}}
   foreach(var f in foes.ToArray())if(!f.dead&&ascendedFoes.TryGetValue(f,out var state)&&Ascended("brand",1)&&f.brandUntil<=elapsed&&state.stored>0){ascensionEvents.Enqueue(new AscensionEvent{point=f.t.position,damage=state.stored,radius=3*SkillArea("brand"),source="brand"});state.stored=0;}
   for(int i=0;i<128&&ascensionEvents.Count>0;i++){var item=ascensionEvents.Dequeue();Burst(item.point,item.radius,CombatIndicatorColors.Player,.35f);if(!item.bleed)AreaDamage(item.point,item.radius,item.damage,false,item.source);else foreach(var f in foes.ToArray())if(!f.dead&&Vector3.Distance(f.t.position,item.point)<=item.radius+f.radius&&collision.ClearLine(item.point,f.t.position)){if(f.bleedRemaining<=0){f.bleedClock=.5f;f.bleedPending=0;f.bleedDps=item.damage;AscensionState(f).generation=item.generation;}else f.bleedDps=Mathf.Max(f.bleedDps,item.damage);f.bleedRemaining=Mathf.Max(f.bleedRemaining,item.duration);}}
   for(int i=ascensionZones.Count-1;i>=0;i--){var zone=ascensionZones[i];float active=Mathf.Min(dt,zone.life);zone.life-=active;zone.clock-=active;if(zone.follow){zone.from=position;zone.radius=SkillRadius(zone.source);zone.root.position=position+Vector3.up;zone.root.Rotate(0,dt*1080,0);}
    while(zone.clock<=.000001f){zone.clock+=zone.interval;if(zone.line){foreach(var f in foes.ToArray())if(!f.dead&&EnemyAttackGeometry.SegmentDistance(f.t.position,zone.from,zone.to)<=zone.radius+f.radius&&collision.ClearLine(zone.from,f.t.position))Hit(f,zone.damage,zone.source);}else AreaDamage(zone.from,zone.radius,zone.damage,false,zone.source);}
    if(zone.life<=.000001f){Destroy(zone.root.gameObject);ascensionZones.RemoveAt(i);}
   }
   for(int i=ascensionFlights.Count-1;i>=0;i--){var flight=ascensionFlights[i];Vector3 previous=flight.root.position-Vector3.up;flight.age+=dt;float fraction=Mathf.Clamp01(flight.age/flight.duration);bool returning=fraction>=.5f;if(flight.target!=null&&flight.target.t&&!flight.target.dead)flight.end=flight.target.t.position;
    Vector3 current=returning?Vector3.Lerp(flight.end,position,(fraction-.5f)*2):Vector3.Lerp(flight.start,flight.end,fraction*2);flight.root.position=current+Vector3.up;flight.root.Rotate(0,dt*1080,0);
    if(!collision.ClearLine(previous,current,.1f)){Destroy(flight.root.gameObject);ascensionFlights.RemoveAt(i);continue;}
    var hit=returning?flight.inward:flight.outward;foreach(var f in foes.ToArray())if(!f.dead&&!hit.Contains(f)&&EnemyAttackGeometry.SegmentDistance(f.t.position,previous,current)<=flight.radius*SkillArea(flight.source)+f.radius&&collision.ClearLine(current,f.t.position)){hit.Add(f);Hit(f,flight.damage,flight.source);}
    if(fraction>=1){Destroy(flight.root.gameObject);ascensionFlights.RemoveAt(i);}
   }
   for(int i=bloodTethers.Count-1;i>=0;i--){var tether=bloodTethers[i];tether.life-=dt;tether.clock-=dt;if(tether.life<=0||tether.foe.dead||!foes.Contains(tether.foe)||Vector3.Distance(position,tether.foe.t.position)>7*SkillArea("bloodthirst")||!collision.ClearLine(position,tether.foe.t.position)){Destroy(tether.line.gameObject);bloodTethers.RemoveAt(i);continue;}tether.line.SetPosition(0,position+Vector3.up);tether.line.SetPosition(1,tether.foe.t.position+Vector3.up);while(tether.clock<=0){tether.clock+=.5f;HealAscendedBlood(Hit(tether.foe,AbilityDamage("bloodthirst")*.25f,"bloodthirst"),false);}}
   int extraSwords=build.Rank("orbit")>0&&!Ascended("orbit",2)?OathBladeCount-1:0;
   while(crownSwords.Count<extraSwords){var root=WorldArt.Group("Additional Oathblade",effects);SpinningSword(root,Vector3.zero);crownSwords.Add(root);}
   while(crownSwords.Count>extraSwords){int last=crownSwords.Count-1;Destroy(crownSwords[last].gameObject);crownSwords.RemoveAt(last);}
   for(int i=0;i<crownSwords.Count;i++){crownSwords[i].position=OathPositionAt(i+1)+Vector3.up*1.05f;crownSwords[i].rotation=Quaternion.Euler(0,elapsed*480,0);crownSwords[i].localScale=Vector3.one*SkillArea("orbit");}
  }
  void DrawAscensionStatus(){if(ascensionShield>0&&shieldUntil>elapsed)GUI.Label(new Rect(323,662,150,24),"BARRIER "+ascensionShield.ToString("0"),CardStyle(12,new Color(.5f,.9f,1),TextAnchor.MiddleCenter,true));}
  void DrawAscensionBadge(Rect rect,string id){int branch=build.Ascension(id);if(branch==0)return;var badge=new Rect(rect.xMax-21,rect.yMax-20,20,18);Box(badge,branch==1?new Color(.08f,.22f,.25f,.95f):new Color(.27f,.09f,.12f,.95f));GUI.Label(badge,branch==1?"I":"II",CardStyle(12,new Color(1,.8f,.3f),TextAnchor.MiddleCenter,true));}
 }
}
