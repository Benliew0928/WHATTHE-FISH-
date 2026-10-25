using UnityEngine;
namespace WhatTheFish {
 public sealed partial class PlayerView {
  float activityBaseFov;bool activityLensApplied;
  void RestoreActivityLens(){if(activityLensApplied&&cam)cam.fieldOfView=activityBaseFov;activityLensApplied=false;}
  internal void PresentActivityCinematic(){
   var cue=ActivityCinematic.Instance;if(!isActiveAndEnabled||!active||!target||InputBlocked||!cue||!cue.CameraPose(out var focus,out var desired,out var weight))return;
   var direction=focus-desired;if(direction.sqrMagnitude<.01f)return;
   transform.SetPositionAndRotation(Vector3.Lerp(transform.position,desired,weight),Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(direction),weight));
   activityBaseFov=cam.fieldOfView;activityLensApplied=true;cam.fieldOfView=Mathf.Lerp(activityBaseFov,50,weight*.5f);
   target.HideHead(Vector3.Distance(transform.position,target.transform.position+Vector3.up*1.57f)<.85f);
  }
 }
}
