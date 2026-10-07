#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace WhatTheFish {
 public sealed partial class BasketballPassReview {
  void Odds(){
   var random=new System.Random(61723);
   foreach(var anchor in new[]{(4f,.95f),(7.2644f,.80f),(13f,.30f),(26f,.05f)}){
    float chance=BasketballShotOdds.Chance(anchor.Item1,.65f);Check(Mathf.Abs(chance-anchor.Item2)<.00001f,"perfect chance anchor "+anchor.Item1+"m = "+chance);
    int made=0;const int trials=100000;for(int i=0;i<trials;i++)if(random.NextDouble()<chance)made++;
    Check(Mathf.Abs(made/(float)trials-chance)<.005f,"seeded outcome frequency "+anchor.Item1+"m "+made+"/"+trials);
   }
   bool monotonic=true,bounded=true;
   for(float distance=0;distance<=32;distance+=.25f){
    float best=BasketballShotOdds.Chance(distance,.65f),previous=best;
    foreach(float direction in new[]{-1f,1f})for(int i=0;i<=100;i++){
     float power=Mathf.Lerp(.65f,direction<0?0:1,i/100f);float p=BasketballShotOdds.Chance(distance,power);
     if(i==0)previous=best;monotonic&=p<=previous+.000001f;bounded&=p>0&&p<=best&&best<1;previous=p;
    }
    if(distance>.25f)monotonic&=best<=BasketballShotOdds.Chance(distance-.25f,.65f)+.000001f;
   }
   Check(monotonic,"odds fall continuously with range and timing error on both sides");Check(bounded,"perfect is never guaranteed and poor is never zero");
   Check(BasketballShotOdds.Chance(26,.50f)>.02f&&BasketballShotOdds.Chance(26,.50f)<.04f,"imperfect full-court example remains near three percent");
   Check(BasketballShotOdds.Chance(float.NaN,.65f)==0&&BasketballShotOdds.Chance(4,float.PositiveInfinity)==0,"nonfinite odds inputs rejected");
   for(float distance=4;distance<=26;distance+=1)File.AppendAllText(Path.Combine(folder,"shot-odds.csv"),$"{distance:F2},{BasketballShotOdds.Chance(distance,.65f):F5},{BasketballShotOdds.Chance(distance,.5f):F5},{BasketballShotOdds.Chance(distance,.2f):F5},{BasketballShotOdds.Chance(distance,0):F5}\n");
  }
  IEnumerator Arcs(){
   var control=FindObjectsByType<BasketballShootButton>(FindObjectsSortMode.None).First(b=>b.pass);
   float scale=control.GetComponentInParent<Canvas>().scaleFactor;
   foreach(float bend in new[]{1f,-1f,0f}){
    yield return Possess(actor,new Vector3(0,.07f,-5));yield return null;
    var start=RectTransformUtility.WorldToScreenPoint(null,control.transform.position);
    var touch=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left,pointerId=71,position=start};control.OnPointerDown(touch);
    touch.position=start+Vector2.up*(150*scale);control.OnDrag(touch);Check(app.view.PassBend>.99f&&app.view.PassCharging,"up swipe selects loft without cancelling");
    control.OnDrag(new PointerEventData(EventSystem.current){pointerId=72,position=start-Vector2.up*150*scale});Check(app.view.PassBend>.99f,"other finger cannot change pass arc");
    touch.position=start-Vector2.up*(150*scale);control.OnDrag(touch);Check(app.view.PassBend<-.99f&&app.view.PassCharging,"down swipe selects bounce without cancelling");
    touch.position=start;control.OnDrag(touch);Check(app.view.PassBend==0,"return to center restores chest pass");
    touch.position=start+Vector2.up*(bend*150*scale);control.OnDrag(touch);yield return Observe(1.0f,BasketballPassRules.Name(bend)+"-aim");
    Check(Mathf.Abs(ball.PassAim.bend-bend)<.01f,"host receives selected bend "+bend);
    Check(!ball.ReleasePassCharge(actor,0,ball.Defense.play,ball.PassClock,float.NaN),"invalid arc release rejected");
    var preview=lane.Path;Check(preview.Bounces==(bend<0),"preview recognizes rebound "+bend);
    if(bend>0)Check(preview.Point(preview.duration*.5f).y-preview.start.y>2.5f,"loft preview rises above normal reach");
    uint count=ball.PassCount,bounces=ball.PassBounces;control.OnPointerUp(touch);float end=Time.time+2;while(ball.PassCount==count&&Time.time<end)yield return Observe(.001f,"arc-push");
    Check(ball.PassCount==count+1,"arc release creates one pass "+bend);ball.autoPickup=false;
    var path=BasketballPassPath.Create(ball.PassAim.origin,ball.PassAim.heading,ball.PassAim.power,ball.PassAim.bend,BasketballBall.PassFloor(ball.PassAim.origin));
    Check(Vector3.Distance(path.velocity,ball.LastLaunchVelocity)<.001f,"actual launch equals preview velocity "+bend);
    double released=ball.PassAim.released;float error=0,peak=ball.Body.position.y;int samples=0;
    while(BasketballMotion.Clock-released<path.duration){
     yield return new WaitForFixedUpdate();float time=(float)(Time.fixedTimeAsDouble-released)+Time.fixedDeltaTime;
     error=Mathf.Max(error,Vector3.Distance(ball.Body.position,path.Point(time)));peak=Mathf.Max(peak,ball.Body.position.y);samples++;
     File.AppendAllText(Path.Combine(folder,"trajectory.csv"),$"{bend},{time:F4},{ball.Body.position.x:F4},{ball.Body.position.y:F4},{ball.Body.position.z:F4},{error:F4}\n");
    }
    Check(samples>10&&error<(bend<0?.55f:.30f),"physical path matches curved preview "+bend+" error="+error.ToString("F3"));
    Check(ball.PassBounces==bounces+(bend<0?1:0),"exactly the intended floor bounce "+bend+" before="+bounces+" after="+ball.PassBounces);
    if(bend>0)Check(peak-path.start.y>2.5f,"loft ball clears standing reach");
    yield return Observe(.4f,"arc-recovery");
   }
   yield return Possess(actor,new Vector3(0,.07f,-5));yield return null;
   var point=RectTransformUtility.WorldToScreenPoint(null,control.transform.position);var pointer=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left,pointerId=73,position=point};control.OnPointerDown(pointer);
   pointer.position=point+Vector2.up*(150*scale);control.OnDrag(pointer);yield return new WaitForSeconds(.3f);uint passes=ball.PassCount;
   var cancel=RectTransformUtility.WorldToScreenPoint(null,control.cancelArea.position);Check(Mathf.Abs(cancel.x-point.x)>200*scale&&Mathf.Abs(cancel.y-point.y)<10*scale,"cancel is lateral and clear of vertical swipe corridor");
   pointer.position=cancel;control.OnDrag(pointer);control.OnPointerUp(pointer);yield return new WaitForSeconds(.5f);Check(ball.Held&&!lane.Visible&&ball.PassCount==passes,"lateral cancel after loft keeps possession");
   yield return Possess(actor,new Vector3(0,.07f,-5));app.view.BeginPass();app.view.SelectPass(-1);yield return new WaitForSeconds(.9f);app.view.EndPass();float until=Time.time+2;while(ball.PassCount==passes&&Time.time<until)yield return null;ball.autoPickup=false;
   var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.layer=8;wall.transform.position=new Vector3(0,.45f,-3.2f);wall.transform.localScale=new Vector3(2,.9f,.1f);Physics.SyncTransforms();uint beforeWall=ball.PassBounces;
   yield return new WaitForSeconds(.9f);Check(ball.PassBounces==beforeWall,"wall interception cancels planned floor rebound");Destroy(wall);
  }
  IEnumerator ShotTrials(){
   var hoop=ball.transform.parent.GetComponentsInChildren<Transform>().First(t=>t.name=="Hoop_North");
   foreach(var trial in new[]{(4f,.65f,false),(7.2644f,.65f,true),(13f,.65f,true),(26f,.65f,false),(26f,.65f,true),(26f,.50f,true),(26f,0f,false),(4f,.10f,true),(13f,.65f,false)}){
    var point=hoop.TransformPoint(new Vector3(0,.07f,trial.Item1));float heading=Quaternion.LookRotation(Vector3.ProjectOnPlane(hoop.position-point,Vector3.up)).eulerAngles.y;
    yield return Possess(actor,point,heading);ball.ReviewShotRoll=()=>trial.Item3?0:1;uint shots=ball.ShotCount,rolls=ball.ShotRolls,made=ball.Score.made;
    Check(ball.TryShoot(actor,heading,trial.Item2),"shot trial accepted "+trial);
    Check(!ball.TryShoot(actor,heading,trial.Item2)&&ball.ShotRolls==rolls,"duplicate queued shot cannot reroll");
    float end=Time.time+2;while(ball.ShotCount==shots&&Time.time<end)yield return null;ball.autoPickup=false;
    Check(ball.ShotCount==shots+1&&ball.ShotRolls==rolls+1,"one host outcome roll per released shot");
    Check(Mathf.Abs(ball.LastShotChance-BasketballShotOdds.Chance(trial.Item1,trial.Item2))<.004f,"host odds use actual release range and power: actual="+ball.LastShotChance+" expected="+BasketballShotOdds.Chance(trial.Item1,trial.Item2));
    end=Time.time+8;while(ball.Score.result==BasketballResult.Flying&&Time.time<end)yield return null;
    Check(trial.Item3?ball.Score.made==made+1:ball.Score.made==made&&ball.Score.result==BasketballResult.Missed,"physical probability outcome "+trial+" = "+ball.Score.result);
   }
   ball.ReviewShotRoll=null;
  }
  IEnumerator HostOdds(Athlete guest){
   var hoop=ball.transform.parent.GetComponentsInChildren<Transform>().First(t=>t.name=="Hoop_North");
   for(int i=0;i<2;i++){
    bool make=i==1;ball.ReviewShotRoll=()=>make?0:1;var point=hoop.TransformPoint(new Vector3(0,.07f,make?26:4));float heading=Quaternion.LookRotation(Vector3.ProjectOnPlane(hoop.position-point,Vector3.up)).eulerAngles.y;
    yield return Possess(guest,point,heading);uint shots=ball.ShotCount,rolls=ball.ShotRolls;Signal("odds-start-"+i);yield return Await("odds-input-"+i);
    float end=Time.time+6;while(ball.ShotCount==shots&&Time.time<end)yield return null;Check(ball.ShotCount==shots+1&&ball.ShotRolls==rolls+1,"guest request gets exactly one host shot roll");ball.autoPickup=false;
    end=Time.time+9;while(ball.Score.result==BasketballResult.Flying&&Time.time<end)yield return null;
    Check(ball.Score.result==(make?BasketballResult.Scored:BasketballResult.Missed),"host controls guest "+(make?"imperfect full-court make":"perfect close miss"));
    if(make)Check(ball.LastShotChance>.01f&&ball.LastShotChance<.05f,"guest imperfect full-court odds remain below perfect");
    yield return Await("odds-observed-"+i);
   }
   ball.ReviewShotRoll=null;
  }
  IEnumerator GuestOdds(){
   for(int i=0;i<2;i++){
    yield return Await("odds-start-"+i);float end=Time.time+4;while(ball.Holder!=actor&&Time.time<end)yield return null;yield return new WaitForSeconds(.3f);
    var hoop=ball.transform.parent.GetComponentsInChildren<Transform>().First(t=>t.name=="Hoop_North");app.view.yaw=Quaternion.LookRotation(Vector3.ProjectOnPlane(hoop.position-actor.transform.position,Vector3.up)).eulerAngles.y;DevelopmentProbe.TurnCommand=new PlayerCommand{heading=app.view.yaw};
    bool make=i==1;ball.ReviewShotRoll=()=>make?1:0;uint attempts=ball.Score.attempts;Check(app.view.BeginShot(),"guest begins odds trial");
    end=Time.time+5;float target=make?.5f:.65f;while(Mathf.Abs(app.view.ShotPower-target)>.02f&&Time.time<end)yield return null;Check(Mathf.Abs(app.view.ShotPower-target)<=.03f,"guest releases chosen timing quality");app.view.EndShot();Signal("odds-input-"+i);
    end=Time.time+12;while((ball.Score.attempts==attempts||ball.Score.result==BasketballResult.Flying)&&Time.time<end)yield return null;
    Check(ball.Score.attempts==attempts+1&&ball.Score.result==(make?BasketballResult.Scored:BasketballResult.Missed),"replicated result follows host roll, not guest roll");Signal("odds-observed-"+i);
   }
   ball.ReviewShotRoll=null;
  }
 }
}
#endif
