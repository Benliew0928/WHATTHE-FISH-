using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace WhatTheFish {
 // Casual policy keeps attacking skill, with readable defensive commitments.
 public sealed class FootballBot:MonoBehaviour {
  public const string PolicyVersion="football-casual-v1";
  public const float DecisionMin=.28f,DecisionJitter=.1f,MarkBeforeTackle=.65f,ChallengeMin=3.5f,ChallengeJitter=.75f,DefensiveAbilityDelay=1;
  public const float AttackDecisionMin=.18f,AttackDecisionJitter=.07f;
  public int Slot {get;private set;}
  public int Decisions {get;private set;}
  public string Intent {get;private set;}="Waiting";
  public int Kicks {get;private set;}public int Passes {get;private set;}public int Abilities {get;private set;}
  Athlete actor,aimReceiver,observedCarrier,markedCarrier;System.Random random;FootballBotAction action;float think,decisionElapsed,chargeTime,nextKick,nextChallenge,aimError,stuckTime,resetUntil,possessionSince,markingSince;Vector3 resetPoint;uint round,abilityRevision;bool aiming;
  public void Initialize(int slot,int seed){Slot=slot;actor=GetComponent<Athlete>();random=new System.Random(unchecked(seed+slot*7919));think=(float)random.NextDouble()*DecisionMin;decisionElapsed=chargeTime=nextKick=nextChallenge=stuckTime=resetUntil=0;action=default;aiming=false;aimReceiver=observedCarrier=markedCarrier=null;possessionSince=markingSince=Time.time;Decisions=Kicks=Passes=Abilities=0;round=uint.MaxValue;}
  public PlayerCommand Read(float dt){
   var match=FootballMatch.Instance;if(!actor||!match||!match.Context||!match.Running||match.TeamOf(actor)==FootballTeam.None){action=default;chargeTime=0;Intent="Waiting";return default;}
   if(round!=match.Snapshot.revision){round=match.Snapshot.revision;action=default;think=decisionElapsed=chargeTime=stuckTime=resetUntil=0;aiming=false;observedCarrier=markedCarrier=null;possessionSince=markingSince=Time.time;nextKick=nextChallenge=Time.time+.3f;}
   decisionElapsed+=dt;
   if((think-=dt)<=0){var owner=FootballBall.Instance.CurrentController;bool attacking=owner&&match.TeamOf(owner)==match.TeamOf(actor);think=(attacking?AttackDecisionMin:DecisionMin)+(float)random.NextDouble()*(attacking?AttackDecisionJitter:DecisionJitter);action=Decide(decisionElapsed);abilityRevision=FootballBall.Instance.PropSlot(actor).revision;decisionElapsed=0;Decisions++;}
   var ball=FootballBall.Instance;
   if(action.useProp&&ball&&ball.TryUseProp(actor,abilityRevision,round,action.propHeading))Abilities++;
   var result=action.Command(actor);action.kick=action.tackle=action.useProp=false;return result;
  }
  void FixedUpdate(){var n=GetComponent<NetworkObject>();if(n&&n.IsSpawned)return;if(!actor)return;var app=AppRoot.Instance;if(app&&app.Exploring)actor.Simulate(Read(Time.fixedDeltaTime),Time.fixedDeltaTime);}
  FootballBotAction Decide(float elapsed){
   var ball=FootballBall.Instance;var match=FootballMatch.Instance;var team=match.TeamOf(actor);var roster=FootballBots.Roster;
   int goalIndex=Enumerable.Range(0,ball.GoalCount).First(i=>ball.GoalSign(i)==(team==FootballTeam.A?1:-1));
   var goal=ball.Pitch.TransformPoint(ball.GoalBounds(goalIndex).center);goal.y=actor.transform.position.y;
   var position=actor.transform.position;var bp=ball.Body.position;bp.y=position.y;
   var mates=roster.Where(a=>match.TeamOf(a)==team).ToArray();var enemies=roster.Where(a=>match.TeamOf(a)!=team&&match.TeamOf(a)!=FootballTeam.None).ToArray();
   var owner=ball.CurrentController;bool carrying=owner==actor,ourBall=owner&&match.TeamOf(owner)==team;Vector3 destination;bool shoot=false,pass=false;Athlete receiver=null;
   if(owner!=observedCarrier){observedCarrier=owner;possessionSince=Time.time;markedCarrier=null;aiming=false;chargeTime=0;}
   var home=ball.Pitch.TransformPoint(ball.GoalBounds(1-goalIndex).center);home.y=position.y;
   var hunter=mates.OrderBy(a=>Vector3.Distance(a.transform.position,bp)+(a.Action!=FootballAction.None||a.WhiffRemaining>0?5:0)-(a.FootballEffort.Pressuring?1.2f:0)).ThenBy(FootballBots.Id).First();
   var blockers=roster.Where(a=>a!=actor).ToArray();
   float propHeading=actor.transform.eulerAngles.y;bool useProp=false;var prop=ball.PropSlot(actor).kind;
   if(ball.PropUnavailable(actor)==""){
    if(prop==SportsProp.Turbo)useProp=ourBall||hunter==actor&&Vector3.Distance(position,bp)>6;
    bool reacted=Time.time-possessionSince>=DefensiveAbilityDelay;
    if(prop==SportsProp.Portal)useProp=reacted&&owner&&!ourBall&&ball.InOwnHalf(actor);
    if(prop==SportsProp.Wall&&reacted&&owner&&!ourBall&&Vector3.Distance(bp,home)<28&&Vector3.Distance(position,home)<Vector3.Distance(bp,home)){var toward=bp-position;propHeading=Mathf.Atan2(toward.x,toward.z)*Mathf.Rad2Deg;useProp=ball.TryWallPlacement(actor,propHeading,out _,out _);}
   }
   if(useProp&&prop!=SportsProp.Turbo){Intent=prop==SportsProp.Portal?"Portal":"Wall";aiming=false;chargeTime=0;markedCarrier=null;return new(){useProp=true,propHeading=propHeading};}
   float power=.65f;
   bool firstTouch=!owner&&actor.KickReady&&Vector3.Angle(actor.transform.forward,goal-position)<40;
   if(carrying||firstTouch){
    Intent="Attack";destination=goal;float distance=Vector3.Distance(position,goal);
    bool pressured=enemies.Any(a=>Vector3.Distance(a.transform.position,position)<4);
    receiver=mates.Where(a=>a!=actor&&a.Action==FootballAction.None&&Vector3.Distance(a.transform.position,position)>4&&Vector3.Distance(a.transform.position,position)<23&&Vector3.Distance(a.transform.position,goal)<distance+(pressured?18:-3)&&FootballNavigation.ClearLane(position,a.transform.position,blockers,a)).OrderBy(a=>Vector3.Distance(a.transform.position,goal)).FirstOrDefault();
    Vector3 shot=goal;bool clearShot=false;
    foreach(float side in new[]{0f,-.36f,.36f}){var target=goal+ball.Pitch.right*(ball.GoalBounds(goalIndex).size.x*Mathf.Abs(ball.Pitch.lossyScale.x)*side);if(distance<23&&FootballNavigation.ClearLane(position,target,blockers)){shot=target;clearShot=true;break;}}
    // Turning for a back pass before securing a loose ball loses kick reach.
    if(carrying&&receiver&&(pressured&&distance>18||!clearShot)){destination=receiver.transform.position+receiver.transform.forward*Mathf.Min(.8f,receiver.speed*.08f);power=Mathf.Clamp01((Vector3.Distance(position,destination)-12)/35);shoot=pass=true;Intent="Pass";}
    else if(clearShot){destination=shot;shoot=true;power=Mathf.Clamp01((distance-8)/24);Intent="Shoot";receiver=null;}
    if(shoot){
     if(!aiming||receiver!=aimReceiver){chargeTime=0;aimError=((float)random.NextDouble()-.5f)*(pass?.4f:1.1f);}
     destination+=ball.Pitch.right*aimError;chargeTime+=elapsed;
    }else {
     chargeTime=0;receiver=null;
     var forward=(goal-position).normalized;var marker=enemies.OrderBy(a=>Vector3.Distance(position,a.transform.position)).FirstOrDefault();
     if(marker&&Vector3.Distance(position,marker.transform.position)<8&&Vector3.Dot(marker.transform.position-position,forward)>0){var side=Vector3.Cross(Vector3.up,forward);float lateral=Vector3.Dot(marker.transform.position-position,side);float escape=Mathf.Abs(lateral)>.7f?-Mathf.Sign(lateral):Slot%2==0?1:-1;destination=position+forward*4+side*escape*5;Intent="Evade";}
    }
   }else {
    chargeTime=0;
    if(!ourBall&&hunter==actor){
     Intent="Chase";destination=bp+Vector3.ProjectOnPlane(ball.Body.linearVelocity,Vector3.up)*.15f;
    }
    else if(ourBall){Intent="Support";var forward=(goal-bp).normalized;var side=Vector3.Cross(Vector3.up,forward);float ahead=Vector3.Distance(bp,goal)<14?-4:7;
     var support=mates.Where(a=>a!=owner).OrderBy(FootballBots.Id).ToArray();int rank=Array.IndexOf(support,actor);float lane=Slot%2==0?6:-6;
     if(mates.Length>2){lane=(rank%2==0?-1:1)*(rank<2?7:4);if(rank>=2)ahead=-8;}
     destination=bp+forward*ahead+side*lane;
    }
    else {Intent="Cover";float depth=Mathf.Min(8,Vector3.Distance(home,bp)*.55f);destination=bp+(home-bp).normalized*depth;
     if(mates.Length>2){var cover=mates.Where(a=>a!=hunter).OrderBy(FootballBots.Id).ToArray();int rank=Array.IndexOf(cover,actor);float lane=(rank-(cover.Length-1)*.5f)*4.5f;destination+=Vector3.Cross(Vector3.up,(home-bp).normalized)*lane;}
     destination.y=position.y;
    }
    var spawn=ball.Props.pickup;var pickup=ball.PropAnchor(spawn.anchor);pickup.y=position.y;
    if(prop==SportsProp.None&&spawn.kind!=SportsProp.None&&(ourBall||!owner&&hunter!=actor)&&Vector3.Distance(position,pickup)<(ourBall?18:12)&&mates.Where(a=>a!=owner).OrderBy(a=>Vector3.Distance(a.transform.position,pickup)).ThenBy(FootballBots.Id).FirstOrDefault()==actor){destination=pickup;Intent="Collect";}
   }
   if(!owner&&hunter==actor&&Vector3.Distance(position,bp)<2&&actor.Action==FootballAction.None&&actor.WhiffRemaining<=0&&ball.Body.linearVelocity.sqrMagnitude<.04f)stuckTime+=elapsed;else stuckTime=0;
   if(stuckTime>.8f){stuckTime=0;resetUntil=Time.time+.65f;var away=Vector3.ProjectOnPlane(position-bp,Vector3.up).normalized;resetPoint=bp+away*2.1f+ball.Pitch.right*(Slot%2==0?.5f:-.5f);}
   if(!owner&&Time.time<resetUntil){destination=resetPoint;shoot=false;chargeTime=0;Intent="Reapproach";}
   aiming=shoot;aimReceiver=receiver;
   if(!shoot){destination=FootballNavigation.Clamp(ball,destination,Intent=="Chase"?.2f:Intent=="Collect"?.6f:1.1f);destination=FootballNavigation.Waypoint(ball,position,destination);}destination.y=position.y;
   var delta=destination-position;float length=delta.magnitude;var direction=length>.01f?delta/length:Vector3.zero;
   bool kick=shoot&&actor.KickReady&&chargeTime>=Mathf.Max(.22f,power*ball.maximumChargeTime)&&Time.time>=nextKick&&Vector3.Angle(actor.transform.forward,direction)<9;
   if(kick){nextKick=Time.time+.8f;chargeTime=0;Kicks++;if(pass)Passes++;}
   var move=direction*Mathf.Clamp01(length/1.3f);if(shoot)move*=.18f;
   // Spread teammates without backing away from a contested loose ball.
   if(!shoot)foreach(var other in mates)if(other!=actor){var away=Vector3.ProjectOnPlane(position-other.transform.position,Vector3.up);if(away.sqrMagnitude>.001f&&away.sqrMagnitude<2.25f)move+=away.normalized*(1.5f-away.magnitude)*.55f;}
   bool marking=!ourBall&&owner&&hunter==actor&&Vector3.Distance(position,owner.transform.position)<5&&FootballPressure.CarrierFor(actor)==owner;
   if(!marking)markedCarrier=null;
   else if(markedCarrier!=owner){markedCarrier=owner;markingSince=Time.time;}
   // The carrier gets a readable guard window before a committed slide. This
   // never changes the human/engine cooldown, contact radius or movement speed.
   bool tackle=marking&&Time.time-markingSince>=MarkBeforeTackle&&Time.time>=nextChallenge&&Vector3.Distance(position,owner.transform.position)<3.2f&&Vector3.Angle(actor.transform.forward,owner.transform.position-position)<25&&Vector3.Dot(owner.transform.forward,(position-owner.transform.position).normalized)>-.2f&&actor.TackleReady;
   if(tackle)foreach(var mate in mates)if(mate!=actor){var offset=Vector3.ProjectOnPlane(mate.transform.position-position,Vector3.up);float along=Vector3.Dot(offset,actor.transform.forward);if(along>0&&along<5&&(offset-actor.transform.forward*along).sqrMagnitude<1.5f)tackle=false;}
   if(tackle)nextChallenge=Time.time+ChallengeMin+(float)random.NextDouble()*ChallengeJitter;
   bool dash=!shoot&&!marking&&length>8&&actor.FootballEffort.Stamina>45&&!actor.FootballEffort.Exhausted&&Vector3.Angle(actor.transform.forward,direction)<45;
   if(marking){move=Vector3.zero;Intent=tackle?"Tackle":"Guard";}
   return new FootballBotAction{move=new Vector2(move.x,move.z),sprint=length>3&&!shoot,dash=dash,pressure=marking,charge=shoot,kick=kick,power=power,tackle=tackle,useProp=useProp,propHeading=propHeading};
  }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
  public FootballBotAction ReviewDecision(float elapsed=.23f)=>Decide(elapsed);
#endif
 }
}
