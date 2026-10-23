#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace WhatTheFish {
 public sealed partial class TargetPassReview {
  PassFlightSnapshot Flight=>basket?bb.PassFlight:fb.PassFlight;
  Rigidbody PassBody=>basket?bb.Body:fb.Body;
  PassFlightVisual FlightVisual=>PassBody.GetComponent<PassFlightVisual>();
  void OpenLane(){for(int i=0;i<roster.Length;i++)if(roster[i]!=actor&&roster[i]!=mate)Place(roster[i],new Vector3(-6,.07f,-10+i));}
  IEnumerator PredictionChecks(){
   Fixture();yield return new WaitForSeconds(.3f);Place(other,new Vector3(2,.07f,-1));Place(mate,new Vector3(0,.07f,6));
   Check((basket?bb.SelectPassReceiver(actor,app.view.yaw):fb.SelectPassReceiver(actor,app.view.yaw))==mate,"camera angle wins over closer off-axis teammate");
   if(basket){var control=BasketballHUD.Instance.GetComponentsInChildren<BasketballShootButton>().First(b=>b.pass);control.Refresh();control.OnPointerDown(new PointerEventData(EventSystem.current){pointerId=71});Check(!control.Tracking&&!app.view.PassCharging&&!control.guide,"tap commits without pass hold or drag guide");}
   else {var control=FootballHUD.Instance.GetComponentInChildren<FootballPassButton>();control.Refresh();control.OnPointerDown(new PointerEventData(EventSystem.current){pointerId=71});Check(!control.Tracking&&!app.view.FootballPassAiming,"tap commits without pass hold");}
   yield return Until(()=>Flight.active,"tap releases without pointer up",3);yield return null;
   Check(FlightVisual.Visible,"flight path visible after tap");yield return Capture("single-tap-flight");
   yield return Until(()=>Holder==mate,"tap catches camera-selected teammate",4);yield return null;Check(!Flight.active&&!FlightVisual.Visible,"receipt clears shared flight path");
   if(basket)BasketballHUD.Instance.GetComponentsInChildren<BasketballShootButton>().First(b=>b.pass).OnPointerUp(new PointerEventData(EventSystem.current){pointerId=71});else FootballHUD.Instance.GetComponentInChildren<FootballPassButton>().OnPointerUp(new PointerEventData(EventSystem.current){pointerId=71});
   yield return new WaitForSeconds(.2f);Check(Holder==mate,"lifting after committed tap cannot pass again");
   foreach(var movement in new[]{Vector2.zero,Vector2.right,Vector2.up,new Vector2(.5f,.8f)}){
    Fixture();OpenLane();Place(mate,new Vector3(-2,.07f,3));yield return new WaitForSeconds(.25f);
    var running=new PlayerCommand{move=movement,heading=field.eulerAngles.y,sprint=true};float until=Time.time+.35f;
    while(Time.time<until){mate.Simulate(running,Time.deltaTime);yield return null;}
    Check(PassTo(actor,mate,Possession),"predictive pass accepted "+movement);until=Time.time+4;
    while(Holder!=mate&&Time.time<until){mate.Simulate(running,Time.deltaTime);File.AppendAllText(Path.Combine(folder,"prediction.csv"),$"{movement},{Time.time:F3},{mate.transform.position:F3},{mate.Motor.Velocity:F3},{PassBody.position:F3},{PassBody.linearVelocity:F3},active={Flight.active},remaining={Flight.remaining:F3},destination={Flight.destination:F3}\n");yield return null;}
    Check(Holder==mate,"stationary/crossing/away/diagonal receiver automatically catches "+movement);
   }
   // Longitudinal lead must remain inside the playable court/pitch, including
   // a runner who reaches the line while the pass is already travelling.
   Fixture();OpenLane();Place(actor,new Vector3(0,.07f,-12));Place(mate,new Vector3(0,.07f,4));
   if(basket)bb.GiveMatchPossession(actor);else {fb.ReviewFootballBall(actor.transform.position+actor.transform.forward*.8f+Vector3.up*fb.WorldRadius,Vector3.zero);fb.RefreshControl(actor);}
   yield return new WaitForSeconds(.25f);var longRun=new PlayerCommand{move=Vector2.up,heading=field.eulerAngles.y,sprint=true};float longEnd=Time.time+.35f;
   while(Time.time<longEnd){mate.Simulate(longRun,Time.deltaTime);yield return null;}
   Check(PassTo(actor,mate,Possession),"long pass to boundary runner accepted");longEnd=Time.time+4;
   while(Holder!=mate&&Time.time<longEnd){mate.Simulate(longRun,Time.deltaTime);yield return null;}
   Check(Holder==mate,"long pass catches runner without leaving court/pitch");
   foreach(bool teammate in new[]{false,true}){
    Fixture();OpenLane();Place(mate,new Vector3(0,.07f,4));Place(teammate?other:enemy,new Vector3(0,.07f,-2.3f));yield return new WaitForSeconds(.25f);
    Check(PassTo(actor,mate,Possession),"blocked lane pass accepted");yield return Until(()=>Holder==(teammate?other:enemy),(teammate?"teammate":"opponent")+" in front intercepts before target",4);Check(!Flight.active,"interception ends guidance");
   }
   // A deliberate 90-degree turn must abandon the receiver and retain the
   // release-time destination. Continue recording velocity to reject U-turns.
   Fixture();OpenLane();Place(mate,new Vector3(-3,.07f,6));yield return new WaitForSeconds(.25f);
   var run=new PlayerCommand{move=Vector2.right,heading=field.eulerAngles.y,sprint=true};float end=Time.time+.35f;
   while(Time.time<end){mate.Simulate(run,Time.deltaTime);yield return null;}
   Check(PassTo(actor,mate,Possession),"turn case launches");end=Time.time+2;
   while(!Flight.active&&Time.time<end){mate.Simulate(run,Time.deltaTime);yield return null;}
   Check(Flight.active,"turn case in flight");var original=basket?bb.OriginalPassDestination:fb.OriginalPassDestination;var forward=Vector3.ProjectOnPlane(PassBody.linearVelocity,Vector3.up).normalized;
   run.move=Vector2.down;end=Time.time+1.5f;bool abandoned=false,forwardOnly=true,originalKept=true;
   while(Flight.active&&Time.time<end){mate.Simulate(run,Time.deltaTime);yield return null;bool follows=basket?bb.PassFollowing:fb.PassFollowing;abandoned|=!follows;if(!follows&&Flight.active)originalKept&=Vector3.Distance(Flight.destination,original)<.01f;forwardOnly&=Vector3.Dot(Vector3.ProjectOnPlane(PassBody.linearVelocity,Vector3.up),forward)>=-.01f;}
   Check(abandoned&&originalKept,"sharp turn keeps original predicted destination");Check(forwardOnly,"sharp turn never makes ball U-turn");
   Fixture();OpenLane();Place(mate,new Vector3(0,.07f,5));yield return new WaitForSeconds(.25f);Check(PassTo(actor,mate,Possession),"reset case launches");yield return Until(()=>Flight.active,"reset case in flight",3);if(basket)bb.ResetHome();else fb.ResetBall(true);yield return null;Check(!Flight.active&&!FlightVisual.Visible,"reset removes guidance and path");
  }
 }
}
#endif
