#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using UnityEngine;
namespace WhatTheFish {
 public static class MatchBoundaryChecks {
  public static void Verify(Athlete actor,bool basketball,bool restricted,Action<bool,string> check){
   var football=FootballBall.Instance;var court=BasketballBall.Active;
   var frame=basketball?court.transform.parent:football.Pitch;
   var bounds=basketball?new Bounds(Vector3.zero,new Vector3(BasketballBall.PlayerCourtLimits.x*2,0,BasketballBall.PlayerCourtLimits.y*2)):football.PitchBounds;
   var saved=actor.transform.position;bool enabled=actor.capsule.enabled;var centre=bounds.center;centre.y=basketball?.08f:bounds.max.y+.04f;
   foreach(var direction in new[]{Vector3.right,Vector3.left,Vector3.forward,Vector3.back}){
    var start=centre;start.x+=direction.x*(bounds.extents.x-1);start.z+=direction.z*(bounds.extents.z-1);
    actor.capsule.enabled=false;actor.transform.position=frame.TransformPoint(start);actor.capsule.enabled=true;Physics.SyncTransforms();
    var movement=frame.TransformVector(direction*3+Vector3.up*.4f);
    var result=basketball?court.ConstrainPlayerMotion(actor,movement):football.ConstrainPlayerMotion(actor,movement,.1f);
    var target=frame.InverseTransformPoint(actor.transform.position+result);
    bool inside=target.x<=bounds.max.x+.01f&&target.x>=bounds.min.x-.01f&&target.z<=bounds.max.z+.01f&&target.z>=bounds.min.z-.01f;
    check(inside==restricted,(basketball?"court":"pitch")+" "+direction+" "+(restricted?"contains match player":"opens after result")+" "+actor.name);
    check(Mathf.Abs(frame.InverseTransformVector(result).y-.4f)<.01f,"boundary preserves jump height");
   }
   actor.capsule.enabled=false;actor.transform.position=saved;actor.capsule.enabled=enabled;Physics.SyncTransforms();
  }
 }
}
#endif
