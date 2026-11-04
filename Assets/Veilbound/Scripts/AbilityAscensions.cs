using System;using System.Collections.Generic;
namespace Veilbound {
 public sealed class AscensionSpec {
  public string id,name,description;public int branch;
  public AscensionSpec(string id,int branch,string name,string description){this.id=id;this.branch=branch;this.name=name;this.description=description;}
 }
 public static class AbilityAscensions {
  public const int UnlockRank=7;
  public static readonly AscensionSpec[] All={
   new AscensionSpec("cleave",1,"Worldsplitter","Every swing also sends an eight-metre fissure forward for 65% of swing damage."),
   new AscensionSpec("cleave",2,"Butcher's Waltz","Alternating wide sweeps. Four consecutive hits on an enemy unleash a finishing strike for 120% extra swing damage."),
   new AscensionSpec("whirl",1,"Eye of the Reaper","Spin for three seconds, striking every 0.3 seconds for 30% of cast damage per pass. Recasts refresh the whirlwind."),
   new AscensionSpec("whirl",2,"Harvest Moon","Throw a spinning sword eight metres outward and recall it. Each enemy can take 90% cast damage on each journey."),
   new AscensionSpec("lance",1,"Veilbreaker","Larger piercing blades leave a damaging rift along their path for two seconds."),
   new AscensionSpec("lance",2,"Shattered Edge","Each main blade splits on first impact into two smaller piercing blades, each dealing 65% damage. Fragments cannot split again."),
   new AscensionSpec("orbit",1,"Crown of Steel","Three orbiting swords form a moving perimeter. Each sword strikes with your upgraded Oathblade damage."),
   new AscensionSpec("orbit",2,"Oathbound Hunters","Swords leave their orbit to hunt nearby enemies and return, striking on both journeys. Isolated targets take stronger hits."),
   new AscensionSpec("smite",1,"Final Sentence","Judgement targets the toughest nearby enemy. Repeated judgements build up to 120% additional damage."),
   new AscensionSpec("smite",2,"Court of the Hollow","Judge up to three enemies at once for 85% damage each. Marked enemies release a smaller judgement when slain."),
   new AscensionSpec("brand",1,"Brand of Ruin","Marks store 25% of damage received and erupt when they expire or their bearer dies. Eruptions cannot store damage."),
   new AscensionSpec("brand",2,"Brand of Pursuit","Repeated hits increase a marked enemy's vulnerability by 3% per hit, up to 95%. Marks transfer to a nearby enemy on death."),
   new AscensionSpec("bloodthirst",1,"Crimson Feast","An enlarged blood wave deals 20% extra damage. Overhealing becomes a five-second barrier, capped at 25% maximum health."),
   new AscensionSpec("bloodthirst",2,"Heartseeker","Blood spears strike up to three enemies for 150% damage and tether them for three seconds. Nearby tethered enemies keep returning health."),
   new AscensionSpec("vengeance",1,"Reckoning","Counterattacks strike all around you and grant 30% protection against follow-up hits for 0.75 seconds."),
   new AscensionSpec("vengeance",2,"Blood Debt","Damage taken builds a reserve up to 50% maximum health. Your next counter consumes it for twice that amount in extra damage."),
   new AscensionSpec("warcry",1,"Voice of Dominion","The cry interrupts ordinary enemies for 0.8 seconds and cancels their current attack. Bosses receive a brief 0.15-second interruption."),
   new AscensionSpec("warcry",2,"Battle Hymn","The weakening cry leaves a moving aura for its duration. Nearby enemies fuel an extra 12 Haste rating, improved by effect upgrades."),
   new AscensionSpec("rend",1,"Open Wounds","Bleeds stack up to five times. Reapplications add a stack and refresh the duration while preserving tick timing."),
   new AscensionSpec("rend",2,"Red Plague","Dying bleeding enemies spread a weaker bleed nearby. Plague chains through up to six generations without triggering weapon procs."),
   new AscensionSpec("rage",1,"Endless Wrath","Kills extend active Fury by 0.5 seconds each, up to six extra seconds per activation."),
   new AscensionSpec("rage",2,"Avatar of Ruin","Fury begins with a 75% stronger, wider eruption. Standard swings release short shockwaves throughout Fury.")
  };
  public static AscensionSpec Get(string id,int branch)=>Array.Find(All,s=>s.id==id&&s.branch==branch);
 }
 public sealed partial class RunBuild {
  readonly Dictionary<string,int> ascensions=new Dictionary<string,int>();
  public bool AscensionRound {get;private set;}
  public string AscendingAbility {get;private set;}
  public int Ascension(string id)=>ascensions.TryGetValue(id,out int branch)?branch:0;
  public bool BeginAscension(){
   if(AscensionRound)return true;
   foreach(var ability in Pool)if(ability.ability&&Rank(ability.id)>=AbilityAscensions.UnlockRank&&Ascension(ability.id)==0){
    SetOfferMode(false);AscensionRound=true;AscendingAbility=ability.id;offers.Clear();
    for(int branch=1;branch<=2;branch++){var spec=AbilityAscensions.Get(ability.id,branch);offers.Add(new Choice(ability.id,spec.name,spec.description,true){ascension=branch});}return true;
   }
   return false;
  }
  bool PickAscension(Choice choice){
   if(!AscensionRound||choice.id!=AscendingAbility||choice.ascension<1||choice.ascension>2||Ascension(choice.id)!=0||Rank(choice.id)<7)return false;
   ascensions[choice.id]=choice.ascension;AscensionRound=false;AscendingAbility=null;return true;
  }
 }
}
