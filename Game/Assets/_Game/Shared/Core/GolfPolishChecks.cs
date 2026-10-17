#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using UnityEngine;

namespace WhatTheFish {
 public static class GolfPolishChecks {
  public static IEnumerator Run(GolfMatchManager match,Athlete actor,Action<bool,string> check,Action<string> capture){
   check(GolfChargeGraphic.PowerColor(.2f)==GolfChargeGraphic.PowerColor(.75f),"POWER_METER_REMAINS_YELLOW_THROUGH_SEVENTY_FIVE_PERCENT");
   var middle=GolfChargeGraphic.PowerColor(.875f);check(middle.r==1&&middle.g>0&&middle.g<.86f&&middle.b==0&&GolfChargeGraphic.PowerColor(1)==Color.red,"POWER_METER_BLENDS_TO_PURE_RED_AT_FULL");
   var meter=GolfHUD.Instance.FrameRect.Find("Golf feedback HUD/Golf character charge meter");check(meter&&meter.parent==GolfHUD.Instance.FrameRect.Find("Golf feedback HUD")&&!GolfHUD.Instance.ActionRect.GetComponentInChildren<GolfChargeGraphic>(),"CHARACTER_POWER_METER_REPLACES_ACTION_BUTTON_RING");
   var start=match.GetComponent<GolfStartAudio>();start.ReviewFocus(true);int beep=start.Beeps,go=start.Whistles;match.State.Clear();match.State.Start(new[]{new GolfEntrant(GolfCartWorld.Key(actor),"You")},match.Now,5);
   float deadline=Time.realtimeSinceStartup+6;while(Time.realtimeSinceStartup<deadline&&start.Whistles==go){start.ReviewFocus(true);yield return null;}
   check(start.Beeps-beep==5&&start.Whistles-go==1,"FIVE_IDENTICAL_COUNTDOWN_BEEPS_AND_ONE_GO_WHISTLE");check(start.Beep&&start.Whistle&&start.Beep!=start.Whistle&&start.Whistle.length>.5f,"GO_USES_SEPARATE_WHISTLE_CLIP");
   var at=match.Course.holes[0].tee.position+Vector3.right*8;GolfCartMotor.Surface(at,0,out at,out var rotation);var prefab=Resources.Load<GolfCart>("GolfCart");var cart=UnityEngine.Object.Instantiate(prefab,at,rotation);cart.name="Temporary wheel effect review";
   var state=new GolfCartState{summoned=true,position=at,rotation=rotation,driver=999,speed=3};cart.Apply(state,true);var effects=cart.GetComponent<GolfCartGroundFX>();
   try{
    for(int i=0;i<65;i++){var proposed=at+Vector3.forward*(i*.04f);if(GolfCartMotor.Surface(proposed,0,out var supported,out var facing)){state.position=supported;state.rotation=facing;cart.Apply(state,true);}yield return new WaitForSeconds(.02f);}
    check(effects.TotalMarks>=4&&effects.TotalParticles>0&&effects.LiveMarks>0,"MOVING_WHEELS_CREATE_GROUND_MARKS_AND_SOIL_FLECKS");
    var camera=Camera.main;var before=camera.transform.position;var beforeRotation=camera.transform.rotation;camera.transform.position=cart.transform.position+new Vector3(3,2.5f,-4);camera.transform.LookAt(cart.transform.position+Vector3.up*.4f);capture("cart-wheel-ground-effects");camera.transform.SetPositionAndRotation(before,beforeRotation);
    int marks=effects.TotalMarks;state.driver=GolfCartState.NoDriver;state.speed=0;cart.Apply(state,true);yield return new WaitForSeconds(3.2f);check(effects.TotalMarks==marks&&effects.LiveMarks==0,"PARKED_CART_EMITS_NOTHING_AND_MARKS_EXPIRE_AFTER_THREE_SECONDS");
   }finally{UnityEngine.Object.Destroy(cart.gameObject);}
  }
 }
}
#endif
