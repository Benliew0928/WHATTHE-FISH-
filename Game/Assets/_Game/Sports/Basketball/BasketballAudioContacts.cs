using UnityEngine;

namespace WhatTheFish {
 public enum BasketballSurfaceKind { Floor,Board,Rim,Padding }
 public sealed class BasketballAudioSurface:MonoBehaviour {public BasketballSurfaceKind kind;}
 public sealed partial class BasketballBall {
  readonly double[] audioContactAt={-100,-100,-100,-100,-100};
  void PrepareAudioSurfaces(){
   if(!transform.parent)return;
   // Resolve authored collider names once, then use explicit material markers
   // at contacts. No per-collision substring heuristic or rim/net double sound.
   foreach(var collider in transform.parent.GetComponentsInChildren<Collider>(true)){
    if(collider.isTrigger||collider==GetComponent<SphereCollider>())continue;
    string name=collider.name;BasketballSurfaceKind kind;
    if(name.StartsWith("Rim segment "))kind=BasketballSurfaceKind.Rim;
    else if(name=="Hoop__BoardShell_Collision"||name=="Hoop__BoardInset_Collision"||name=="Hoop__RimMount_Collision")kind=BasketballSurfaceKind.Board;
    else if(name=="Hoop__BoardBumper_Collision")kind=BasketballSurfaceKind.Padding;
    else continue;
    var marker=collider.GetComponent<BasketballAudioSurface>()??collider.gameObject.AddComponent<BasketballAudioSurface>();marker.kind=kind;
   }
  }
  void AudioContact(Collision collision){
   if(!Playing||Held||collision.contactCount==0)return;
   var contact=collision.GetContact(0);float speed=0;
   for(int i=0;i<collision.contactCount;i++)speed=Mathf.Max(speed,Mathf.Abs(Vector3.Dot(collision.relativeVelocity,collision.GetContact(i).normal)));
   if(speed<.45f)return;
   var marker=collision.collider.GetComponent<BasketballAudioSurface>();
   var cue=BasketballSound.Body;int group=4;
   if(marker){group=(int)marker.kind;cue=marker.kind==BasketballSurfaceKind.Rim?BasketballSound.Rim:marker.kind==BasketballSurfaceKind.Board?BasketballSound.Board:marker.kind==BasketballSurfaceKind.Floor?BasketballSound.Bounce:BasketballSound.Body;}
   else if(contact.normal.y>.55f&&CourtPoint(contact.point).y<.35f){cue=BasketballSound.Bounce;group=0;}
   if(BasketballMotion.Clock-audioContactAt[group]<.065)return;audioContactAt[group]=BasketballMotion.Clock;
   BasketballAudio.Emit(cue,contact.point,Mathf.Lerp(.22f,1.25f,Mathf.InverseLerp(.45f,9,speed)));
  }
 }
}
