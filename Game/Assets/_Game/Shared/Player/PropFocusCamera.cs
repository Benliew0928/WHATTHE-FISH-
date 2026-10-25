using UnityEngine;

namespace WhatTheFish {
 public sealed partial class PlayerView {
  SportsPropActivation propFocus;Athlete propSubject;SportId propSport;uint propSeen;
  Quaternion propEntryRotation,propPresentedRotation;Vector3 propInputForward;bool propFocusing,propHeadingHeld;int propCameraMode;
  float propSelfYaw,propBaseFov;double propObservedAt;bool propLensApplied;
  public bool PropFocusActive=>propFocusing;
  public bool PropHeadingHeld=>propHeadingHeld;
  public Athlete PropFocusSubject=>propSubject;
  // Preserve the input basis while the broadcast view turns automatically.
  Vector3 PropMovementForward=>propFocusing||propHeadingHeld?propInputForward:transform.forward;
  bool PropForcesLook=>propFocusing;
  void ReleasePropLook(){
   if(!propHeadingHeld)return;
   yaw=transform.eulerAngles.y;pitch=Mathf.Clamp(Mathf.DeltaAngle(0,transform.eulerAngles.x),-25,65);
   propHeadingHeld=false;propSubject=null;
  }
  void RestorePropLens(){if(propLensApplied&&cam)cam.fieldOfView=propBaseFov;propLensApplied=false;}
  void ResetPropCamera(){propFocusing=propHeadingHeld=false;propSubject=null;}
  internal void PresentPropFocusCamera(){
   if(!isActiveAndEnabled||!active||!target||!SportsPropPresentation.OnCourt(target)||TeleportSequence.OwnsCamera||FootballReplay.Presenting||FootballMVP.Presenting||BasketballMVP.Presenting||InputBlocked||!SportsPropPresentation.Read(out var state,out var subject,out var now)) {ResetPropCamera();return;}
   var sport=AppRoot.Instance.SelectedSport;
   if(propSport!=sport){ResetPropCamera();propSeen=0;propSport=sport;}
   if(SportsPropPresentation.LockOnCounterplay){propSeen=state.sequence;propFocusing=propHeadingHeld=false;propSubject=null;return;}
   if(state.sequence!=propSeen){
    propSeen=state.sequence;float age=(float)(now-state.started);
    if(state.kind!=SportsProp.LockOn&&age>=-.05f&&age<SportsPropActivation.Duration){
     propFocus=state;propSubject=subject;propEntryRotation=propFocusing||propHeadingHeld?propPresentedRotation:transform.rotation;if(!propFocusing&&!propHeadingHeld)propInputForward=transform.forward;propCameraMode=mode;propSelfYaw=target.transform.eulerAngles.y;propObservedAt=now;
     propFocusing=true;propHeadingHeld=false;
    }
   }
   if((!propFocusing&&!propHeadingHeld)||!propSubject)return;
   if(mode!=propCameraMode||propFocus.sequence!=state.sequence||!SportsPropPresentation.OnCourt(propSubject)){propFocusing=propHeadingHeld=false;propSubject=null;return;}
   float elapsed=Mathf.Max(0,(float)(now-propFocus.started));
   if(elapsed>=SportsPropActivation.Duration){propFocusing=false;propHeadingHeld=true;}
   var nativePosition=transform.position;var position=nativePosition;Quaternion rotation;
   bool self=propSubject==target;
   if(self){
    // Eye-level users briefly see their own activation from behind, then return
    // to their eyes facing their character's forward direction, never its face.
    if(mode==0&&propFocusing){float amount=Mathf.SmoothStep(0,1,elapsed/.35f)*(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(2.1f,2.6f,elapsed)));var back=Quaternion.Euler(16,propSelfYaw,0)*Vector3.back*4.2f;var focus=target.transform.position+Vector3.up*1.4f;var desired=focus+back;if(Physics.SphereCast(focus,.2f,back.normalized,out var hit,back.magnitude,1<<8,QueryTriggerInteraction.Ignore))desired=focus+back.normalized*Mathf.Max(.35f,hit.distance-.15f);position=Vector3.Lerp(nativePosition,desired,amount);target.HideHead(amount<.2f);}
    rotation=Quaternion.Euler(mode==0?pitch:mode==2?38:pitch,propSelfYaw,0);
    if(mode!=0){var direction=target.transform.position+Vector3.up*1.2f-position;if(direction.sqrMagnitude>.01f)rotation=Quaternion.LookRotation(direction);}
   }else {
    var direction=propSubject.transform.position+Vector3.up*1.25f-position;
    rotation=direction.sqrMagnitude>.01f?Quaternion.LookRotation(direction):transform.rotation;
   }
   float enter=Mathf.SmoothStep(0,1,Mathf.Min(elapsed,(float)(now-propObservedAt))/.35f);
   rotation=Quaternion.Slerp(propEntryRotation,rotation,enter);
   transform.SetPositionAndRotation(position,rotation);
   propPresentedRotation=rotation;
   // No camera zoom persists beyond the animation. The selected mode remains.
   if(propFocusing){propBaseFov=cam.fieldOfView;propLensApplied=true;float accent=Mathf.SmoothStep(0,1,elapsed/.4f)*(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(1.9f,2.6f,elapsed)));cam.fieldOfView=Mathf.Lerp(propBaseFov,Mathf.Min(propBaseFov,46),accent);}
  }
 }
}
