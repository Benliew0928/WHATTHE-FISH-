#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using UnityEngine;

namespace WhatTheFish {
 public static class GolfFollowChecks {
  public static IEnumerator Run(GolfMatchManager match,Athlete actor,Action<bool,string> check,Action<string> capture){
   var app=AppRoot.Instance;app.view.CancelGolfAim();var ball=match.Ball(actor);var follow=match.GetComponent<GolfFollowCamera>();var feet=match.GetComponent<GolfFootsteps>();
   feet.ReviewFocus(true);var savedActor=actor.transform.position;var savedBall=ball.Body.position;var floor=GameObject.CreatePrimitive(PrimitiveType.Cube);floor.name="Temporary Golf Paving support";floor.layer=8;floor.transform.position=match.Course.holes[0].tee.position+Vector3.up*5;floor.transform.localScale=new Vector3(12,.2f,12);Physics.SyncTransforms();
   floor.GetComponent<Renderer>().enabled=false;var origin=floor.transform.position+Vector3.up*.1f;
   var trunk=GameObject.CreatePrimitive(PrimitiveType.Cube);trunk.name="Temporary Golf trunk fixture";trunk.layer=8;trunk.transform.localScale=new Vector3(.35f,2,.35f);trunk.transform.position=origin+Vector3.left*.7f+Vector3.up;
   var back=GameObject.CreatePrimitive(PrimitiveType.Cube);back.name="Temporary Golf bunker edge fixture";back.layer=8;back.transform.localScale=new Vector3(.35f,2,.35f);back.transform.position=origin+Vector3.back*.7f+Vector3.up;
   try {
    actor.capsule.enabled=false;actor.transform.position=origin+Vector3.back*1.7f+Vector3.up*.035f;actor.ResetLocomotion();actor.capsule.enabled=true;ball.Place(origin+Vector3.up*(GolfBall.Radius+.002f),true);Physics.SyncTransforms();
    yield return new WaitForSeconds(.3f);
    check(match.TryStance(actor,ball,out var stance,0),"TREE_AND_BUNKER_BLOCKED_PREFERRED_SIDES_FIND_SAFE_ALTERNATE_STANCE");
    check(Vector3.Distance(trunk.GetComponent<Collider>().ClosestPoint(stance+Vector3.up*.5f),stance+Vector3.up*.5f)>actor.capsule.radius*.85f&&Vector3.Distance(back.GetComponent<Collider>().ClosestPoint(stance+Vector3.up*.5f),stance+Vector3.up*.5f)>actor.capsule.radius*.85f&&Vector3.Distance(stance,ball.Body.position)<=GolfClubMotion.SwingReach,"ALTERNATE_STANCE_IS_WITHIN_REACH_AND_CLEAR_OF_FIXTURE_OBSTACLES");
    trunk.transform.localScale=new Vector3(4,3,4);trunk.transform.position=origin+Vector3.up*1.5f;Physics.SyncTransforms();
    check(!match.TryStance(actor,ball,out _,0),"COMPLETELY_BLOCKED_BALL_DOES_NOT_TELEPORT_PLAYER_THROUGH_OBSTACLE");
    trunk.SetActive(false);back.SetActive(false);Physics.SyncTransforms();
    for(int k=0;k<5;k++){var clip=feet.Clip((GolfFootSurface)k);var data=new float[clip.samples];clip.GetData(data,0);float peak=0;foreach(float sample in data)peak=Mathf.Max(peak,Mathf.Abs(sample));check(peak>.05f&&peak<.9f,"FOOTSTEP_SOUND_HAS_VALID_SIGNAL_"+(GolfFootSurface)k);}
    var water=match.Course.waterPatches[0];check(feet.Surface(match.Course.transform.TransformPoint(water.centre))==GolfFootSurface.Water,"WATER_PATCH_CLASSIFIES_SPLASH_FOOTSTEP");
    int recordedBefore=feet.RecordedContacts;int total=0;for(int k=0;k<5;k++)total+=feet.Count((GolfFootSurface)k);
    var animator=actor.GetComponentInChildren<Animator>();for(int i=0;i<40;i++){actor.speed=2;animator.Play("Run",0,.1f+i*.025f);animator.Update(0);actor.capsule.Move(Vector3.right*.04f+Vector3.down*.005f);yield return new WaitForSeconds(.025f);}
    int after=0;for(int k=0;k<5;k++)after+=feet.Count((GolfFootSurface)k);check(after>total&&after-total<=3,"ONE_LOCOMOTION_CYCLE_CONTACT_COUNT_"+(after-total));check(feet.RecordedContacts-recordedBefore==after-total,"PLANTED_WALKWAY_CONTACTS_ACTUALLY_PLAY_RECORDED_TAKES");actor.speed=0;animator.Play("Idle",0,0);
    yield return new WaitForSeconds(.2f);int idle=0;for(int k=0;k<5;k++)idle+=feet.Count((GolfFootSurface)k);yield return new WaitForSeconds(.3f);int still=0;for(int k=0;k<5;k++)still+=feet.Count((GolfFootSurface)k);check(idle==still,"IDLE_PLAYER_DOES_NOT_PRODUCE_FOOTSTEPS");
    ball.Place(origin+Vector3.up*(GolfBall.Radius+.002f),true);ball.Strike(Vector3.forward*2+Vector3.up*3);
    GolfFeedback.Emit(GolfFeedbackCue.Swing,ball.Body.position,Vector3.up,.8f,GolfCartWorld.Key(actor),ball.Owner);yield return null;yield return new WaitForEndOfFrame();
    yield return new WaitForSeconds(.2f);yield return new WaitForEndOfFrame();
    var image=follow.Window.GetComponentInChildren<UnityEngine.UI.RawImage>();
    check(image.rectTransform.rect.width>200&&image.rectTransform.rect.height>100,"BALL_CAMERA_IMAGE_HAS_VISIBLE_RECT");
    var rt=image.texture as RenderTexture;var previous=RenderTexture.active;RenderTexture.active=rt;var pixels=new Texture2D(rt.width,rt.height,TextureFormat.RGB24,false);pixels.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);pixels.Apply();RenderTexture.active=previous;var colours=pixels.GetPixels32();int differences=0;foreach(var colour in colours)if(Mathf.Abs(colour.r-colours[0].r)+Mathf.Abs(colour.g-colours[0].g)+Mathf.Abs(colour.b-colours[0].b)>40)differences++;UnityEngine.Object.Destroy(pixels);
    check(differences>colours.Length/10,"BALL_CAMERA_TEXTURE_CONTAINS_RENDERED_WORLD_NOT_EMPTY_FRAME");
    check(follow.Visible&&follow.Rendering&&follow.TrackedBall==ball,"ACTUAL_CONTACT_STARTS_SEPARATE_RENDERING_BALL_CAMERA");
    check(follow.Window.anchoredPosition.y<-140&&follow.Window.rect.width>200&&follow.Window.rect.width/follow.Window.rect.height>1.5f,"BALL_CAMERA_IS_RECTANGULAR_BELOW_ISLAND_BADGE");capture("ball-camera-in-flight");
    ball.Place(origin+Vector3.up*(GolfBall.Radius+.002f),true);yield return null;yield return new WaitForEndOfFrame();
    check(follow.Visible,"RESET_OR_CUP_HOLDS_LAST_BALL_VIEW_INSTEAD_OF_FOLLOWING_TELEPORT");
    yield return new WaitForSecondsRealtime(2.8f);check(follow.Visible&&follow.Opacity>.99f,"BALL_CAMERA_REMAINS_FOR_THREE_SECONDS_AFTER_STOP");
    yield return new WaitForSecondsRealtime(.5f);check(follow.Opacity>0&&follow.Opacity<1,"BALL_CAMERA_FADES_AFTER_THREE_SECOND_HOLD");capture("ball-camera-fading");
    yield return new WaitForSecondsRealtime(.6f);check(!follow.Visible&&!follow.Rendering,"HIDDEN_BALL_CAMERA_STOPS_RENDERING");
    follow.Contact(999,ball.Owner);check(!follow.Visible,"OTHER_PLAYER_CONTACT_DOES_NOT_OPEN_LOCAL_BALL_CAMERA");
   }finally {
    UnityEngine.Object.Destroy(trunk);UnityEngine.Object.Destroy(back);UnityEngine.Object.Destroy(floor);
    actor.capsule.enabled=false;actor.transform.position=savedActor;actor.ResetLocomotion();actor.capsule.enabled=true;ball.Place(savedBall,false);Physics.SyncTransforms();
   }
  }
 }
}
#endif
