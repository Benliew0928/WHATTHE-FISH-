#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace WhatTheFish {
 public static class GolfRefineChecks {
  public static IEnumerator Run(GolfMatchManager match,Athlete actor,Action<bool,string> check,Action<string> capture){
   var feet=match.GetComponent<GolfFootsteps>();var path=match.GetComponentInParent<RefinedIslandEnvironment>().GetComponentsInChildren<Collider>().FirstOrDefault(c=>c.name.Contains("WelcomeApproach")&&c.name.Contains("Paving"));
   check(path&&feet.Surface(path.bounds.center,path)==GolfFootSurface.Stone,"ACTUAL_WELCOME_WALKWAY_USES_STONE_FOOTSTEP");
   var text=GolfHUD.Instance.FrameRect.GetComponentsInChildren<UnityEngine.UI.Text>(true).First(t=>t.name=="Golf hold and release hint");check(!text.gameObject.activeInHierarchy||text.text!="Walk near your ball to aim","WALK_NEAR_BALL_HINT_IS_NEVER_VISIBLE_"+text.text);
   var feedback=GolfFeedback.Instance;check(Mathf.Approximately(feedback.Settings.edgeOpacity,.3f)&&feedback.Settings.wheelTrackOpacity>=.45f&&feedback.Settings.wheelParticles>=8,"RED_EDGE_THIRTY_PERCENT_AND_STRONGER_WHEEL_CONTACT_SETTINGS");
   var water=match.Course.transform.Find(match.Course.waterPatches[0].name+"/Pond water").GetComponent<Renderer>().sharedMaterial;
   check(water.shader.name=="WhatTheFish/LagoonWater"&&Mathf.Approximately(water.GetFloat("_Opacity"),.28f),"GOLF_POND_REUSES_TRANSPARENT_FISHING_WATER_SHADER_AND_OPACITY");
   var camera=Camera.main;var oldPosition=camera.transform.position;var oldRotation=camera.transform.rotation;var pond=match.Course.transform.TransformPoint(match.Course.waterPatches[0].centre);camera.transform.position=pond+new Vector3(-8,6,-10);camera.transform.LookAt(pond);capture("transparent-blue-pond");camera.transform.SetPositionAndRotation(oldPosition,oldRotation);
   var clip=match.GetComponent<GolfStartAudio>().Whistle;check(clip==Resources.Load<AudioClip>("GolfAudio/go")&&clip.channels==1&&clip.length>.8f&&clip.length<1.1f,"GO_PLAYS_USER_RECORDING_INSTEAD_OF_SYNTHESIZED_TONE");
   check(feet.WalkwayClips.Length==3&&GolfFootsteps.IsWalkway(path),"ACTUAL_WALKWAY_SELECTS_THREE_RECORDED_SINGLE_FOOTSTEPS");
   foreach(var sample in feet.WalkwayClips){var data=new float[sample.samples];sample.GetData(data,0);float peak=0;foreach(float value in data)peak=Mathf.Max(peak,Mathf.Abs(value));check(sample.channels==1&&sample.length>.25f&&sample.length<.45f&&peak>.5f&&peak<.8f&&Mathf.Abs(data[0])<.001f&&Mathf.Abs(data[^1])<.001f,"RECORDED_STEP_IS_ONE_SHORT_UNCLIPPED_CLEAN_EDGE_CONTACT_"+sample.name);}
   var floor=GameObject.CreatePrimitive(PrimitiveType.Cube);floor.name="Terrain__temporary preview test";floor.layer=8;floor.transform.position=match.Course.holes[0].tee.position+Vector3.up*10;floor.transform.localScale=new Vector3(12,.2f,12);floor.GetComponent<Renderer>().enabled=false;
   var obstacle=GameObject.CreatePrimitive(PrimitiveType.Cube);obstacle.name="Temporary preview obstacle";obstacle.layer=0;var origin=floor.transform.position+Vector3.up*(.1f+GolfBall.Radius+.002f);obstacle.transform.position=origin+Vector3.forward*2+Vector3.up*.4f;obstacle.transform.localScale=new Vector3(2,1,.2f);
   var go=new GameObject("Temporary display-only putt preview");var preview=go.AddComponent<GolfShotPreview>();var ball=match.Ball(actor);Physics.SyncTransforms();
   try{
    preview.Show(ball,match.Course,origin,0,.65f,GolfShotMode.Putt,camera);var first=preview.EndPoint;
    preview.Show(ball,match.Course,origin,0,1,GolfShotMode.Putt,camera);check(Vector3.Distance(first,preview.EndPoint)<.12f&&Vector3.Distance(origin,preview.EndPoint)>1.7f&&Vector3.Distance(origin,preview.EndPoint)<2.1f,"PUTT_PREVIEW_STOPS_AT_FIRST_DEFAULT_LAYER_OBSTACLE_WHILE_CHARGE_INCREASES");
    obstacle.layer=8;Physics.SyncTransforms();preview.Show(ball,match.Course,origin,0,1,GolfShotMode.Putt,camera);check(Vector3.Distance(origin,preview.EndPoint)>1.7f&&Vector3.Distance(origin,preview.EndPoint)<2.1f,"PUTT_PREVIEW_STOPS_AT_SCENE_LAYER_OBSTACLE");
    preview.Show(ball,match.Course,origin,0,.6f,GolfShotMode.Swing,camera);var plan=new GolfShotPlan();plan.Build(origin,Vector3.forward,.6f,GolfShotMode.Swing,match.Course,ball.PhysicsSettings);check(Vector3.Distance(preview.EndPoint,plan.EndPoint)<.001f,"SWING_PREVIEW_PRESERVES_EXISTING_PLAN_ENDPOINT");
   }finally{UnityEngine.Object.Destroy(go);UnityEngine.Object.Destroy(obstacle);UnityEngine.Object.Destroy(floor);}
   yield return null;
  }
 }
}
#endif
