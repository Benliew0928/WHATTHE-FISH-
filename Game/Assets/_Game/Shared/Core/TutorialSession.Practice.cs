using System.Linq;
using UnityEngine;

namespace WhatTheFish {
 public sealed partial class TutorialSession {
  int fishCasts,fishHooks,tackleHits;Vector3 lastPosition;float initialTension;bool directAttempt;float opponentShotAt;
  void Face(Vector3 point){var delta=point-app.LocalAthlete.transform.position;app.view.yaw=Mathf.Atan2(delta.x,delta.z)*Mathf.Rad2Deg;app.LocalAthlete.transform.rotation=Quaternion.Euler(0,app.view.yaw,0);}
  void PrepareStep(){
   var a=app.LocalAthlete;string id=Step.id;a.BasketballPracticeTeam=FootballTeam.A;
   if(friend)friend.gameObject.SetActive(false);if(opponent)opponent.gameObject.SetActive(false);directAttempt=false;
   if(Sport==SportId.Basketball||Sport==SportId.Football){
    bool basketball=Sport==SportId.Basketball;var origin=basketball?BasketballBall.Active.transform.parent.TransformPoint(new Vector3(0,.1f,4)):FootballBall.Instance.KickoffPosition;
    Vector3 forward=basketball?BasketballBall.Active.transform.parent.forward:FootballBall.Instance.Pitch.forward;
    Place(a,origin-forward*3,Quaternion.LookRotation(forward).eulerAngles.y);app.view.yaw=a.transform.eulerAngles.y;
    bool attack=id is "shoot" or "cancelShot" or "pass" or "directPass";bool defense=id is "guard" or "steal" or "block" or "pressure" or "tackle";
    if(id is "pass" or "directPass"){friend.gameObject.SetActive(true);Place(friend,a.transform.position+forward*6,app.view.yaw+180);TargetPoint=friend.transform.position;}
    if(defense){opponent.gameObject.SetActive(true);Place(opponent,a.transform.position+forward*(id=="steal"?1.15f:1.7f),app.view.yaw+(id=="steal"?90:180));TargetPoint=opponent.transform.position;opponentShotAt=Time.unscaledTime+3;}
    var carrier=defense?opponent:a;var loose=Ground(a.transform.position+forward*2)+Vector3.up*(basketball?.5f:.22f);
    if(basketball)BasketballBall.Active.PrepareTutorial(carrier,attack||defense,loose);else FootballBall.Instance.PrepareTutorial(carrier,attack||defense,loose);
    if(id=="collect")TargetPoint=loose;
    if(id=="prop"||id=="ability"){
     var prop=basketball?BasketballBall.Active.TutorialProp(a,id=="ability"):FootballBall.Instance.TutorialProp(a,id=="ability");
     if(id=="prop"){Place(a,prop+(origin-prop).normalized*2.7f,0);Face(prop);TargetPoint=prop;}
    }
    if(!basketball)tackleHits=a.GetComponent<FootballTackle>().HitsDealt;
   }
   if(Sport==SportId.Golf){
    var g=GolfMatchManager.Instance;
    if(id=="start")g.ClearMatch();
    else if(Index>System.Array.FindIndex(Steps,s=>s.id=="start")){
     if(!g.State.Running)g.StartMatch();var ball=g.Ball(a);var hole=g.Target(a);
     if(ball&&hole!=null&&id is "aim" or "power" or "cancelAim" or "putt"){
      var at=id=="putt"?hole.cup.position-hole.cup.forward*2:hole.tee.position;
      ball.Place(Ground(at)+Vector3.up*.08f,true);Place(a,Ground(at)-hole.cup.forward*1.1f,0);Face(hole.cup.position);TargetPoint=ball.transform.position;
     }
    }
    if(id=="buggy"){GolfCartWorld.ParkDrivers(true);if(GolfCartWorld.HasCart(a))GolfCartWorld.Execute(a,GolfCartAction.Recall,GolfCartWorld.Key(a));}
    if(id is "drive" or "steer" or "exitCart" or "recall"){
     if(!GolfCartWorld.HasCart(a))GolfCartWorld.Execute(a,GolfCartAction.Summon,GolfCartWorld.Key(a));var cart=GolfCartWorld.Owned(a);
     if(cart&&!GolfCartWorld.Driving(a))Place(a,cart.transform.position+cart.transform.right*1.8f,cart.transform.eulerAngles.y);
     if(id is "steer" or "exitCart")GolfCartWorld.Execute(a,GolfCartAction.Drive,GolfCartWorld.Key(a));
     if(id=="recall")GolfCartWorld.Execute(a,GolfCartAction.Leave,GolfCartWorld.Key(a));
     TargetPoint=cart?cart.transform.position:null;
    }
   }
   if(Sport==SportId.Fishing){
    var g=FishingGame.Instance;var pier=g.Pier(0);var inland=(pier-g.transform.position).normalized;inland.y=0;
    bool movement=id is "forward" or "backward" or "sideways" or "look" or "jump";
    Place(a,pier+inland*(movement?7:id=="pier"?4.2f:0),0);Face(g.transform.position);g.PrepareTutorial(a,id);
    if(id=="pier")TargetPoint=pier;
    var p=g.Player(a);if(p.HasValue){fishCasts=p.Value.casts;fishHooks=p.Value.hooks;initialTension=p.Value.tension;}
   }
   lastPosition=a.transform.position;
  }
  bool CheckStep(float dt){
   var a=app.LocalAthlete;var v=app.view;string id=Step.id;float moved=Vector3.ProjectOnPlane(a.transform.position-lastPosition,Vector3.up).magnitude;lastPosition=a.transform.position;
   switch(id){
    case "forward":if(lastCommand.move.y>.35f)observedMove+=moved;return observedMove>1.2f;
    case "backward":if(lastCommand.move.y<-.35f)observedMove+=moved;return observedMove>1.2f;
    case "sideways":if(lastCommand.move.x<-.35f)held+=moved;if(lastCommand.move.x>.35f)otherHeld+=moved;return held>.65f&&otherHeld>.65f;
    case "look":return Mathf.Abs(Mathf.DeltaAngle(startYaw,v.yaw))>24;
    case "jump":return a.Airborne&&a.transform.position.y>startPosition.y+.2f;
   }
   if(Sport==SportId.Basketball){var b=BasketballBall.Active;
    switch(id){
     case "collect":return b.Holder==a;
     case "shoot":return b.ShotCount>startCount;
     case "cancelShot":sawAction|=v.ShotCharging;return sawAction&&!v.ShotCharging&&lastCommand.shotCancel&&b.ShotCount==startCount;
     case "pass":return b.PassCount>startCount;
     case "directPass":directAttempt|=lastCommand.targetedPass;return directAttempt&&b.PassCount>startCount;
     case "prop":return b.PropSlot(a).kind==SportsProp.Turbo;
     case "ability":return startProp.kind!=SportsProp.None&&b.PropSlot(a).kind==SportsProp.None;
     case "guard":if(a.BasketballMotion.Guarding&&(a.transform.position-opponent.transform.position).sqrMagnitude<16)held+=dt;return held>1.3f;
     case "steal":sawAction|=b.StealCount>startCount;return sawAction&&b.Holder==a;
     case "block":if(a.BasketballMotion.Blocking&&(a.transform.position-opponent.transform.position).sqrMagnitude<12)held+=dt;return b.Defense.blocks>startCount||held>1.2f;
    }
   }
   if(Sport==SportId.Football){var b=FootballBall.Instance;
    switch(id){
     case "collect":return b.CurrentController==a&&moved>.002f;
     case "shoot":return kickCount>startCount;
     case "cancelShot":sawAction|=v.Charging;if(lastCommand.kick)sawAction=false;return sawAction&&!v.Charging&&kickCount==startCount;
     case "pass":return b.PassPossession>startSecondary&&b.CurrentController!=a;
     case "directPass":directAttempt|=lastCommand.targetedPass;return directAttempt&&b.PassPossession>startSecondary&&b.CurrentController!=a;
     case "dash":if(a.FootballEffort.Dashing&&moved>.001f)held+=dt;return held>.5f&&!lastCommand.dash;
     case "pressure":if(a.FootballEffort.Pressuring)held+=dt;return held>1.3f;
     case "tackle":sawAction|=lastCommand.tackle&&a.Action==FootballAction.Slide;return a.GetComponent<FootballTackle>().HitsDealt>tackleHits||sawAction&&b.CurrentController!=opponent;
     case "prop":return b.PropSlot(a).kind==SportsProp.Turbo;
     case "ability":return startProp.kind!=SportsProp.None&&b.PropSlot(a).kind==SportsProp.None;
    }
   }
   if(Sport==SportId.Golf){var g=GolfMatchManager.Instance;var b=g.Ball(a);
    switch(id){
     case "start":return g.State.Running&&!g.Starting;
     case "aim":return v.GolfAiming;
     case "power":case "putt":return b&&b.StrikeSequence>startCount;
     case "cancelAim":sawAction|=v.GolfAiming;if(lastCommand.golfSwing)sawAction=false;return sawAction&&!v.GolfAimRequested&&b&&b.StrikeSequence==startCount;
     case "buggy":return GolfCartWorld.HasCart(a);
     case "drive":return GolfCartWorld.Driving(a);
     case "steer":if(GolfCartWorld.Driving(a)){if(lastCommand.move.y>.2f)held+=moved;if(Mathf.Abs(lastCommand.move.x)>.2f)otherHeld+=dt;}return held>2&&otherHeld>.5f;
     case "exitCart":return !GolfCartWorld.Driving(a);
     case "recall":return !GolfCartWorld.HasCart(a);
    }
   }
   if(Sport==SportId.Fishing){var g=FishingGame.Instance;var record=g.Player(a);if(!record.HasValue)return false;var p=record.Value;
    switch(id){
     case "pier":return g.NearestPier(a)>=0;
     case "skillChoice":return p.skillPicked;
     case "cast":return p.casts>fishCasts;
     case "hook":return p.hooks>fishHooks;
     case "reel":if(p.phase==FishingPhase.Reeling&&p.held){if(p.steer<0)held+=dt;else if(p.steer>0)otherHeld+=dt;}return held>.5f&&otherHeld>.5f;
     case "cool":return p.phase==FishingPhase.Reeling&&!p.held&&p.tension<initialTension-.1f;
     case "fishSkill":return p.itemSequence>startSecondary;
     case "catch":return p.catches>startCount;
     case "cancelCast":return p.casts>fishCasts&&p.result==FishingResult.Cancelled;
    }
   }
   return false;
  }
  void MaintainScenario(){
   if(Sport==SportId.Basketball&&Step.id=="block"&&Time.unscaledTime>=opponentShotAt&&!Succeeded){
    var b=BasketballBall.Active;if(b.Holder!=opponent){b.PrepareTutorial(opponent,true,Vector3.zero);opponentShotAt=Time.unscaledTime+2;}
    else {b.TryShoot(opponent,opponent.transform.eulerAngles.y);opponentShotAt=Time.unscaledTime+5;}
   }
   if(Sport==SportId.Fishing&&Step.id is "hook" or "reel" or "cool" or "fishSkill" or "catch"){
    var g=FishingGame.Instance;var p=g.Player(app.LocalAthlete);
    if(p.HasValue&&!(Step.id=="fishSkill"&&FishingAbilityRules.BeforeCast(p.Value.item))&&(p.Value.phase==FishingPhase.Escaped||p.Value.phase==FishingPhase.Ready)&&Time.unscaledTime>retryAt){g.PrepareTutorial(app.LocalAthlete,Step.id);retryAt=Time.unscaledTime+5;Status="Try again — your practice fish is ready.";}
   }
  }
 }
}
