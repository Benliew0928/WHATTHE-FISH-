using UnityEngine;

namespace WhatTheFish {
 // Reacts to public bite, wanted-fish and tension cues. All actions still pass
 // FishingGame's distance, ownership, sequence, lease and scoring validation.
 public sealed class FishingBot:MonoBehaviour {
  public int Slot {get;private set;}
  public int Decisions {get;private set;}
  public string Intent {get;private set;}="Waiting";
  Athlete actor;float think;bool cooling;uint round;System.Random random;
  public void Initialize(int slot){Slot=slot;actor=GetComponent<Athlete>();random=new System.Random(4729+slot*7919);think=0;}
  void FixedUpdate(){
   var app=AppRoot.Instance;var game=FishingGame.Instance;
   if(!actor||!app||app.rooms.Connected||!game||!game.Context||!game.Authority||actor.inTransit)return;
   var command=new PlayerCommand{heading=actor.transform.eulerAngles.y};
   var player=game.Player(actor);
   if(player.HasValue){
    var p=player.Value;
    if(round!=game.State.Match.round){round=game.State.Match.round;think=.2f+Slot*.09f;cooling=false;}
    if((think-=Time.fixedDeltaTime)<=0){
     think=.16f+(float)random.NextDouble()*.09f;Decisions++;
     var action=FishingAction.None;int fish=-1;
     var phase=game.State.Match.phase;
     if((phase==FishingRoundPhase.Drafting||phase==FishingRoundPhase.Practice)&&!p.skillPicked){
      Intent="Choose round skill";action=FishingAction.ChooseSkill;fish=(int)game.State.Offer(Slot%3);
     }else if(phase==FishingRoundPhase.Practice||phase==FishingRoundPhase.Ended){
      Intent="Ready";if(!p.ready&&!p.Busy)action=FishingAction.Ready;
     }else if(phase==FishingRoundPhase.Playing){
      if(p.phase==FishingPhase.Ready){
       Intent="Choose fish";float best=float.NegativeInfinity;
       for(int i=0;i<game.State.Fish.Count;i++)if(game.Available(actor,i)){
        int size=FishingState.SizeForFish(i);bool wanted=size==game.State.Wanted(game.Now)&&(p.wantedMask&(1<<game.State.Window(game.Now)))==0;
        float distance=Vector3.ProjectOnPlane(game.FishPosition(i,game.Now)-actor.transform.position,Vector3.up).magnitude;
        // Expected points per fight, with a bonus for this minute's wanted size.
        float value=(FishingState.Points(i)+(wanted?3:0))/(8+size*6+distance*.16f)+(float)random.NextDouble()*.09f;
        if(value>best){best=value;fish=i;}
       }
       if(fish>=0)action=FishingAction.Cast;
      }else if(p.phase==FishingPhase.Bite&&game.Now>=p.biteAt+.22+Slot*.045){Intent="Hook";action=FishingAction.Hook;}
      else if(p.phase==FishingPhase.Reeling){
       // Hysteresis prevents rapidly alternating release/hold. Renew a hold
       // frequently enough to obey the same expiring input lease as a player.
       if(p.tension>.76f||p.strain>.54f)cooling=true;
       if(p.tension<.42f&&p.strain<.16f)cooling=false;
       Intent=cooling?"Cool line":"Counter pull";
       action=cooling?FishingAction.ReelStop:FishingState.CounterAction(p,game.Now);
      }else Intent=p.phase.ToString();
     }
     command.fishingAction=action;command.fishingFish=fish;command.fishingRound=round;command.fishingSequence=p.sequence;command.fishingPressedAt=game.Now;
    }
    if(p.Busy)command.heading=game.Heading(actor);
   }
   actor.Simulate(command,Time.fixedDeltaTime);
  }
 }
}
