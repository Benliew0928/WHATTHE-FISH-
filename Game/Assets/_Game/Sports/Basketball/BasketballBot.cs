using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace WhatTheFish {
 // A small, delayed rule-based policy. All actions go through the human motor
 // and ball rules; no direct possession, scoring or accuracy overrides.
 public sealed class BasketballBot:MonoBehaviour {
  public int Slot {get;private set;}
  public int Decisions {get;private set;}
  public string Intent {get;private set;}="Waiting";
  Athlete actor,receiver,observedHolder;System.Random random;
  Vector3 destination;float think,heldSince,nextChallenge,shotPower,passPower;uint revision=uint.MaxValue;
  bool shoot,pass,guard;
  public void Initialize(int slot){Slot=slot;actor=GetComponent<Athlete>();random=new System.Random(1729+slot*7919);think=0;Decisions=0;revision=uint.MaxValue;receiver=observedHolder=null;shoot=pass=guard=false;}
  void FixedUpdate(){var net=GetComponent<NetworkObject>();if(net&&net.IsSpawned)return;if(actor&&AppRoot.Instance&&AppRoot.Instance.Exploring)actor.Simulate(Read(Time.fixedDeltaTime),Time.fixedDeltaTime);}
  public PlayerCommand Read(float dt){
   var m=BasketballMatch.Instance;var ball=BasketballBall.Active;
   if(!enabled||!actor||!ball||!m||!m.Context||!m.Live||!m.Contains(actor)||m.TeamOf(actor)==FootballTeam.None){Intent="Waiting";return default;}
   if(revision!=m.Snapshot.revision){revision=m.Snapshot.revision;think=0;shoot=pass=guard=false;receiver=observedHolder=null;nextChallenge=Time.time+.8f;}
   if(observedHolder!=ball.Holder){observedHolder=ball.Holder;heldSince=Time.time;think=0;shoot=pass=false;receiver=null;}
   if((think-=dt)<=0){think=.24f+(float)random.NextDouble()*.12f;Decide(ball,m);Decisions++;}
   var position=actor.transform.position;var delta=Vector3.ProjectOnPlane(destination-position,Vector3.up);float distance=delta.magnitude;
   var hoop=m.AttackingHoop(actor);var aim=guard&&ball.Holder?ball.Holder.transform.position:pass&&receiver?receiver.transform.position:shoot&&hoop?hoop.position:destination;
   float heading=Heading(aim-position);var move=delta.normalized*Mathf.Clamp01(distance/1.1f);
   if(shoot||pass||ball.ActionQueued&&ball.Holder==actor)move=Vector3.zero;
   else foreach(var mate in Athlete.Active)if(mate!=actor&&m.Contains(mate)&&m.TeamOf(mate)==m.TeamOf(actor)){
    var away=Vector3.ProjectOnPlane(position-mate.transform.position,Vector3.up);if(away.sqrMagnitude>.001f&&away.sqrMagnitude<1.44f)move+=away.normalized*(1.2f-away.magnitude)*.7f;
   }
   var local=Quaternion.Euler(0,-heading,0)*Vector3.ClampMagnitude(move,1);
   var command=new PlayerCommand{move=new Vector2(local.x,local.z),heading=heading,sprint=distance>3&&!guard,guard=guard,defensePlay=ball.Defense.play,passPlay=ball.Defense.play};
   if(ball.Holder==actor){
    if(ball.IsCharging(actor)){command.move=Vector2.zero;if(ball.PresentedChargePhase>=shotPower){command.shoot=true;command.shotReleasedAt=BasketballMotion.Clock;}}
    else if(ball.IsPassAiming(actor)){
     command.move=Vector2.zero;
     if(!receiver||m.TeamOf(receiver)!=m.TeamOf(actor)){command.passCancel=true;pass=false;}
     else {command.heading=Heading(receiver.transform.position-position);float range=Vector3.ProjectOnPlane(receiver.transform.position-position,Vector3.up).magnitude;passPower=Mathf.Clamp01((range-6)/9);if(ball.PassPower>=passPower){command.pass=true;command.passReleasedAt=BasketballMotion.Clock;}}
    }
    else if(!ball.ActionQueued){command.shotBegin=shoot&&ball.CanShoot(actor);command.passBegin=pass&&receiver&&ball.CanPass(actor);}
   }
   if(guard&&ball.Holder&&Time.time>=nextChallenge&&Time.time-heldSince>.65f){
    float gap=Vector3.ProjectOnPlane(ball.Holder.transform.position-position,Vector3.up).magnitude;
    if(gap<2.1f&&Vector3.Angle(actor.transform.forward,ball.Holder.transform.position-position)<35){
     if(ball.IsCharging(ball.Holder)&&ball.CanBlock(actor)){command.jumpBlock=true;nextChallenge=Time.time+3.5f;}
     else if(gap<1.55f&&ball.CanSteal(actor)){command.steal=true;nextChallenge=Time.time+3.5f;}
    }
   }
   return command;
  }
  static float Heading(Vector3 delta)=>Mathf.Atan2(delta.x,delta.z)*Mathf.Rad2Deg;
  void Decide(BasketballBall ball,BasketballMatch m){
   var team=m.TeamOf(actor);var roster=BasketballBots.Roster;var mates=roster.Where(a=>m.Contains(a)&&m.TeamOf(a)==team).ToArray();
   var enemies=roster.Where(a=>m.Contains(a)&&m.TeamOf(a)!=team).ToArray();
   var hoop=m.AttackingHoop(actor);var home=m.Hoop(BasketballMatchState.Other(team));if(!hoop||!home||mates.Length==0)return;
   var position=actor.transform.position;var owner=ball.Holder;var bp=ball.Body.position;bp.y=position.y;
   bool own=owner&&m.TeamOf(owner)==team;var hunter=mates.OrderBy(a=>(a.transform.position-bp).sqrMagnitude).ThenBy(BasketballMatch.PlayerId).First();
   shoot=pass=guard=false;destination=position;
   var prop=ball.PropSlot(actor);
   if(ball.PropUnavailable(actor)==""&&(prop.kind==SportsProp.Turbo&&(own||hunter==actor)||prop.kind==SportsProp.LockOn&&owner==actor||prop.kind==SportsProp.Portal&&owner&&!own&&Time.time-heldSince>1)){
    if(ball.TryUseProp(actor,prop.revision)&&prop.kind!=SportsProp.Turbo){Intent="Ability";return;}
   }
   if(owner==actor&&ball.IsInbound(actor)){
    receiver=mates.Where(a=>a!=actor).OrderBy(a=>Vector3.Distance(a.transform.position,position)).FirstOrDefault();
    pass=receiver;shoot=guard=false;Intent="Inbound pass";return;
   }
   if(owner==actor){
    Intent="Dribble";float range=Vector3.ProjectOnPlane(hoop.position-position,Vector3.up).magnitude;
    var forward=Vector3.ProjectOnPlane(hoop.position-position,Vector3.up).normalized;
    bool pressured=enemies.Any(e=>Vector3.Distance(e.transform.position,position)<2.8f);
    if(!ball.IsCharging(actor)&&!ball.IsPassAiming(actor)&&!ball.ActionQueued){
     receiver=mates.Where(a=>a!=actor&&Vector3.Distance(a.transform.position,position)>3&&Vector3.Distance(a.transform.position,position)<13&&ClearLane(position,a.transform.position,enemies)&&Vector3.Distance(a.transform.position,hoop.position)<range+(pressured?4:-2)).OrderBy(a=>Vector3.Distance(a.transform.position,hoop.position)).FirstOrDefault();
     pass=receiver&&Time.time-heldSince>.7f&&(pressured||range>8||ball.IsInbound(actor));
     shoot=!pass&&(range<6.5f||m.Snapshot.shotDeadline-m.Now<3)&&Time.time-heldSince>.5f;
     if(shoot)shotPower=BasketballBall.SweetSpot+((float)random.NextDouble()-.5f)*.20f;
    }
    pass|=ball.IsPassAiming(actor);shoot|=ball.IsCharging(actor);
    destination=hoop.position-forward*4.2f;
    if(pass)Intent="Pass";else if(shoot)Intent="Shoot";
    else {
     var marker=enemies.OrderBy(a=>(a.transform.position-position).sqrMagnitude).FirstOrDefault();
     if(marker&&Vector3.Distance(position,marker.transform.position)<3.2f&&Vector3.Dot(marker.transform.position-position,forward)>0){var side=Vector3.Cross(Vector3.up,forward);destination=position+forward*2+side*(Slot%2==0?3:-3);}
    }
   }else if(!owner){Intent="Recover";destination=bp+Vector3.ProjectOnPlane(ball.Body.linearVelocity,Vector3.up)*.2f;if(hunter!=actor){var toward=Vector3.ProjectOnPlane(hoop.position-bp,Vector3.up).normalized;destination=bp+toward*(-2)+Vector3.Cross(Vector3.up,toward)*(Slot%2==0?3:-3);}}
   else if(own){
    Intent="Support";var support=mates.Where(a=>a!=owner).OrderBy(BasketballMatch.PlayerId).ToArray();int rank=Array.IndexOf(support,actor);
    var forward=Vector3.ProjectOnPlane(hoop.position-bp,Vector3.up).normalized;destination=bp+forward*(rank<2?3:-3)+Vector3.Cross(Vector3.up,forward)*(rank%2==0?-1:1)*(rank<2?4.2f:3);
   }else {
    Intent="Guard";Athlete mark=owner;
    if(hunter!=actor){var cover=mates.Where(a=>a!=hunter).OrderBy(BasketballMatch.PlayerId).ToArray();var threats=enemies.Where(a=>a!=owner).OrderBy(BasketballMatch.PlayerId).ToArray();if(threats.Length>0)mark=threats[Math.Max(0,Array.IndexOf(cover,actor))%threats.Length];}
    destination=mark.transform.position+Vector3.ProjectOnPlane(home.position-mark.transform.position,Vector3.up).normalized*1.65f;
    guard=Vector3.Distance(position,bp)<3.8f;
   }
   var court=ball.transform.parent;
   var pickup=court.TransformPoint(BasketballPropRules.Anchor(ball.Props.pickup.anchor));
   if(owner!=actor&&prop.kind==SportsProp.None&&ball.Props.pickup.kind!=SportsProp.None&&(own||!owner&&hunter!=actor)&&Vector3.Distance(position,pickup)<6&&mates.Where(a=>a!=owner).OrderBy(a=>(a.transform.position-pickup).sqrMagnitude).ThenBy(BasketballMatch.PlayerId).FirstOrDefault()==actor){destination=pickup;Intent="Collect";}
   var point=court.InverseTransformPoint(destination);var limits=BasketballBall.PaintedCourtLimits-new Vector2(.85f,.85f);
   point.x=Mathf.Clamp(point.x,-limits.x,limits.x);point.z=Mathf.Clamp(point.z,-limits.y,limits.y);destination=court.TransformPoint(point);destination.y=position.y;
   // Route around a nearby placed wall rather than pushing into its centre.
   var to=destination-position;
   if(!shoot&&!pass&&to.sqrMagnitude>.1f&&Physics.SphereCast(position+Vector3.up*.8f,.38f,to.normalized,out var hit,Mathf.Min(3,to.magnitude),1<<8,QueryTriggerInteraction.Ignore)){
    var side=Vector3.Cross(Vector3.up,hit.normal).normalized;destination=position+side*(Slot%2==0?2:-2)+hit.normal*.8f;
   }
  }
  static bool ClearLane(Vector3 start,Vector3 end,Athlete[] opponents){
   var lane=Vector3.ProjectOnPlane(end-start,Vector3.up);float length=lane.magnitude;if(length<.1f||Physics.Linecast(start+Vector3.up,end+Vector3.up,1<<8,QueryTriggerInteraction.Ignore))return false;
   foreach(var enemy in opponents){var offset=Vector3.ProjectOnPlane(enemy.transform.position-start,Vector3.up);float along=Vector3.Dot(offset,lane/length);if(along>.4f&&along<length-.5f&&(offset-lane/length*along).magnitude<1.1f)return false;}return true;
  }
 }
}
