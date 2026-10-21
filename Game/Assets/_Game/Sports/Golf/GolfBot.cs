using System.Collections;
using UnityEngine;

namespace WhatTheFish {
 public sealed class GolfBot:MonoBehaviour {
  public int Slot {get;private set;}
  public int Decisions {get;private set;}
  public int Shots {get;private set;}
  public string Intent {get;private set;}="Waiting";
  public string NavigationStatus=>navigation.Status;
  Athlete actor;readonly GolfBotNavigation navigation=new();readonly GolfShotPlan preview=new();
  uint round;bool planning,planned;float heading,power,chargeAt,nextThink,stuck;Vector3 lastPosition;
  public void Initialize(int slot){Slot=slot;actor=GetComponent<Athlete>();lastPosition=actor.transform.position;}
  void FixedUpdate(){
   var app=AppRoot.Instance;var match=GolfMatchManager.Instance;
   if(!actor||!app||app.rooms.Connected||!match||!match.Authority||!match.Context||actor.inTransit)return;
   if(round!=match.Round){StopAllCoroutines();navigation.Clear();round=match.Round;planning=planned=false;chargeAt=0;stuck=0;nextThink=Time.time+.4f+Slot*.15f;}
   var command=new PlayerCommand{heading=actor.transform.eulerAngles.y,golfRound=round};
   var player=match.Player(actor);var ball=match.Ball(actor);
   if(!match.State.Running||match.Starting||player==null||player.IsFinished||player.IsDNF||!ball){Intent="Waiting";actor.Simulate(command,Time.fixedDeltaTime);return;}
   command.golfBallOwner=ball.Owner;
   if(actor.GolfClubMotion.Busy||!ball.ReadyToAim){Intent="Watch ball";planned=false;chargeAt=0;actor.Simulate(command,Time.fixedDeltaTime);return;}
   var delta=Vector3.ProjectOnPlane(ball.Body.position-actor.transform.position,Vector3.up);
   if(!match.IsAiming(actor)){
    if(delta.magnitude<2.75f&&match.CanSwing(actor)){
     var target=match.Target(actor);
     float facing=target!=null?Heading(target.cup.position-ball.Body.position):actor.transform.eulerAngles.y;
     if(match.SetAim(actor,true,ball.Owner,round,facing)){StopAllCoroutines();navigation.Clear();planning=false;heading=facing;nextThink=Time.time+.35f;planned=false;}
    }
    if(!match.IsAiming(actor)){
     Intent="Walk to ball";var move=navigation.Direction(this,actor,match.Course,ball.Body.position);
     float distance=move.magnitude;
     if(distance>.05f){command.heading=Heading(move);command.move=Vector2.up*Mathf.Clamp01(distance);command.sprint=delta.magnitude>5;}
     if(Vector3.Distance(actor.transform.position,lastPosition)<.01f&&distance>.6f)stuck+=Time.fixedDeltaTime;else stuck=0;
     lastPosition=actor.transform.position;
     if(stuck>2.5f){StopAllCoroutines();navigation.Clear();planning=planned=false;stuck=0;}
    }
   }else if(!planning&&!planned&&Time.time>=nextThink){Intent="Plan shot";planning=true;StartCoroutine(Plan(match,ball,round));}
   if(planned&&match.IsAiming(actor)){
    Intent="Charge shot";command.heading=heading;command.golfCharging=true;
    if(chargeAt==0)chargeAt=Time.time+.6f+power*1.5f;
    if(Time.time>=chargeAt){command.golfSwing=true;command.golfCharge=power;command.golfCharging=false;planned=false;chargeAt=0;Shots++;nextThink=Time.time+.8f;}
   }
   actor.Simulate(command,Time.fixedDeltaTime);
  }
  static float Heading(Vector3 v)=>Mathf.Atan2(v.x,v.z)*Mathf.Rad2Deg;
  IEnumerator Plan(GolfMatchManager match,GolfBall ball,uint expectedRound){
   var hole=match.Target(actor);if(hole==null){planning=false;yield break;}
   var origin=ball.Body.position;var mode=match.ShotMode(ball);float direct=Heading(hole.cup.position-origin);
   float best=float.PositiveInfinity,bestHeading=direct,bestPower=.2f;int work=0,batch=Application.isMobilePlatform?1:3;
   // Spread physics queries over frames. The preview is the same trajectory a
   // human sees; it does not change the real ball or award a hole.
   float maxPower=mode==GolfShotMode.Putt?.35f:1;
   for(int angle=-3;angle<=3;angle++)for(int charge=0;charge<=20;charge++){
    float h=direct+angle*(mode==GolfShotMode.Putt?8:22),p=maxPower*charge/20;
    float score=Evaluate(match,ball,hole,origin,mode,h,p);
    if(score<best){best=score;bestHeading=h;bestPower=p;}
    if(++work%batch==0)yield return null;
    if(!ball||!match.State.Running||match.Round!=expectedRound){planning=false;yield break;}
   }
   float angleStep=mode==GolfShotMode.Putt?3:9,powerStep=maxPower/30;
   for(int pass=0;pass<4;pass++){
    float centreH=bestHeading,centreP=bestPower;
    for(int a=-2;a<=2;a++)for(int c=-2;c<=2;c++){
     float h=centreH+a*angleStep,p=Mathf.Clamp01(centreP+c*powerStep);
     float score=Evaluate(match,ball,hole,origin,mode,h,p);
     if(score<best){best=score;bestHeading=h;bestPower=p;}
     if(++work%batch==0)yield return null;
     if(!ball||!match.State.Running||match.Round!=expectedRound){planning=false;yield break;}
    }
    angleStep*=.3f;powerStep*=.3f;
   }
   Decisions++;planning=false;
   if(match.Round==expectedRound&&ball.ReadyToAim&&Vector3.Distance(origin,ball.Body.position)<.1f){
    heading=bestHeading;power=bestPower;planned=true;
   }
  }
  float Evaluate(GolfMatchManager match,GolfBall ball,GolfHole hole,Vector3 origin,GolfShotMode mode,float h,float p){
   var course=match.Course;preview.Build(origin,Quaternion.Euler(0,h,0)*Vector3.forward,p,mode,course,ball.PhysicsSettings);
   if(!preview.HasTarget||match.OutsideCourse(preview.EndPoint))return 100000;
   float nearest=float.PositiveInfinity;
   for(int i=1;i<preview.Count;i++){
    if(course.TouchesWater(preview.Points[i-1],preview.Points[i],GolfBall.Radius))return 100000;
    var v=preview.Points[i]-hole.cup.position;
    if(Mathf.Abs(v.y)<.18f)nearest=Mathf.Min(nearest,Vector3.ProjectOnPlane(v,Vector3.up).magnitude);
   }
   var end=preview.EndPoint;
   if(mode==GolfShotMode.Swing){
    // Approximate run-out for long approaches. Putts use the complete guide.
    var velocity=Vector3.ProjectOnPlane(preview.Velocities[preview.Count-1],Vector3.up);
    float run=Mathf.Min(30,velocity.sqrMagnitude/(2*course.RollingResistance(end,ball.PhysicsSettings)));
    var roll=end+velocity.normalized*run;
    if(!course.SafeDryPosition(roll,1)||course.TouchesWater(end,roll,GolfBall.Radius))return 10000;
    end=roll;
   }
   float distance=Vector3.ProjectOnPlane(end-hole.cup.position,Vector3.up).magnitude;
   if(mode==GolfShotMode.Putt&&nearest<hole.cupRadius+GolfBall.Radius*.7f)return -20+nearest+distance*.001f;
   return distance+(preview.Normal.y<.5f?20:0)+(mode==GolfShotMode.Putt?nearest*.2f:0);
  }
 }
}
