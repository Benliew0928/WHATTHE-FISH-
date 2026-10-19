using UnityEngine;

namespace WhatTheFish {
 // Diagnostics describe the same target/range/obstruction checks as casting.
 // They never reserve a fish, select a replacement, or change an input request.
 public static class FishingTargetFeedback {
  public static string ReadyLabel(FishingGame game,PlayerView view) {
   if(!game||!view||!view.target||!view.FishingCamera)return "AIM AT\nA FISH";
   int pier=game.NearestPier(view.target);
   if(pier<0)return "FIND A PIER";
   int selected=view.SelectedFishingFish;
   if(game.VisibleTarget(view.target,view.FishingCamera,selected))
    return "CAST\n"+FishingState.SizeName(selected)+" +"+FishingState.Points(selected);
   var camera=view.FishingCamera;var ray=camera.ScreenPointToRay(view.FishingAimScreenPoint);
   int candidate=-1;float nearest=FishingHUD.Instance?FishingHUD.Instance.AimAcquireDegrees:8;
   for(int i=0;i<game.State.Fish.Count;i++) {
    var point=game.Presentation.TargetPosition(i);var screen=camera.WorldToViewportPoint(point);
    if(screen.z<=camera.nearClipPlane||screen.x<0||screen.x>1||screen.y<0||screen.y>1)continue;
    float angle=Vector3.Angle(ray.direction,point-ray.origin);
    if(angle<nearest){candidate=i;nearest=angle;}
   }
   if(candidate<0)return "AIM AT\nA FISH";
   return FishLabel(game,view.target,camera,candidate);
  }
  public static string FishLabel(FishingGame game,Athlete actor,Camera camera,int fish) {
   if(!game||!actor||!camera||fish<0||fish>=game.State.Fish.Count)return "AIM AT\nA FISH";
   var record=game.State.Fish[fish];
   if(record.claimed)return "FRIEND'S\nFISH";
   if(record.respawnAt>game.Now)return "FISH\nRETURNING";
   var point=game.Presentation.TargetPosition(fish);
   if(Vector3.ProjectOnPlane(game.FishPosition(fish,game.Now)-actor.transform.position,Vector3.up).magnitude>FishingState.MaxCastDistance)return "MOVE\nCLOSER";
   if(Physics.Linecast(actor.transform.position+Vector3.up*1.4f,game.FishPosition(fish,game.Now),1<<8,QueryTriggerInteraction.Ignore)
    ||Physics.Linecast(camera.transform.position,point,1<<8,QueryTriggerInteraction.Ignore))return "CAST PATH\nBLOCKED";
   return "AIM AT\nA FISH";
  }
  public static string FailureLabel(FishingResult result)=>result switch {
   FishingResult.Snapped=>"LINE SNAPPED",
   FishingResult.Missed=>"MISSED BITE",
   FishingResult.Cancelled=>"CAST\nCANCELLED",
   FishingResult.TimedOut=>"FISH\nESCAPED",
   _=>"TRY AGAIN"
  };
 }
}
