using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WhatTheFish {
 public enum TeleportDestination:byte { Station, Game }
 public readonly struct TeleportLanding {
  public readonly Vector3 position;public readonly Quaternion rotation;
  public TeleportLanding(Vector3 position,Quaternion rotation){this.position=position;this.rotation=rotation;}
 }
 // Coordinates are island-local. Slot order belongs to a stable room roster, never a hash or modulo.
 public static class TeleportDestinations {
  public static int Capacity(SportId island)=>island==SportId.Fishing?5:10;
  public static TeleportLanding Nominal(SportId island,TeleportDestination destination,int slot){
   if(slot<0||slot>=Capacity(island))throw new ArgumentOutOfRangeException(nameof(slot));
   if(destination==TeleportDestination.Station)return RoomSeats.StationLanding(island,slot);
   if(destination!=TeleportDestination.Game)throw new ArgumentOutOfRangeException(nameof(destination));
   if(island==SportId.Football)return new TeleportLanding(new Vector3((slot%5-2)*2.4f,1,slot<5?-7:7),Quaternion.Euler(0,slot<5?0:180,0));
   if(island==SportId.Basketball)return new TeleportLanding(new Vector3((slot%5-2)*1.8f,1,slot<5?-4:4),Quaternion.Euler(0,slot<5?0:180,0));
   if(island==SportId.Golf)return new TeleportLanding(new Vector3(-17+slot%5*2,5,-146-slot/5*3),Quaternion.identity);
   float angle=(-90-slot*72)*Mathf.Deg2Rad;var point=new Vector3(Mathf.Cos(angle)*22.5f,1.35f,Mathf.Sin(angle)*22.5f);
   return new TeleportLanding(point,Quaternion.LookRotation(-new Vector3(point.x,0,point.z)));
  }
  // The entire plan must pass before anyone disappears. A blocked slot rejects the group,
  // rather than stacking players or silently moving somebody to an unrelated fallback.
  public static bool Resolve(SportId island,TeleportDestination destination,IReadOnlyList<Athlete> roster,Vector3 islandOffset,out TeleportLanding[] landings,out string error){
   landings=Array.Empty<TeleportLanding>();error="";
   if(!Enum.IsDefined(typeof(SportId),island)||roster==null||roster.Count==0||roster.Count(a=>a&&!a.IsSpectator)>Capacity(island)||roster.Count(a=>a&&a.IsSpectator)>10){error="This destination cannot hold this group.";return false;}
   int playerIndex=0;var plan=new TeleportLanding[roster.Count];var unique=new HashSet<Athlete>();
   for(int i=0;i<roster.Count;i++){
    var actor=roster[i];if(!actor||!actor.capsule||!unique.Add(actor)){error="The teleport roster is invalid.";return false;}
    var slot=RoomSeats.Landing(island,destination,actor,i,playerIndex);if(!actor.IsSpectator)playerIndex++;Vector3 nominal=slot.position+islandOffset;
    if(!Physics.Raycast(nominal+Vector3.up*3,Vector3.down,out var floor,10,1<<8,QueryTriggerInteraction.Ignore)||floor.normal.y<.92f){error=$"{island} {destination} slot {i+1} needs safe ground.";return false;}
    Vector3 point=floor.point+Vector3.up*.035f;float radius=actor.capsule.radius*Mathf.Max(Mathf.Abs(actor.transform.lossyScale.x),Mathf.Abs(actor.transform.lossyScale.z));
    float height=Mathf.Max(radius*2,actor.capsule.height*Mathf.Abs(actor.transform.lossyScale.y));
    // Lift the clearance capsule slightly above its supporting ground.
    foreach(var hit in Physics.OverlapCapsule(point+Vector3.up*(radius+.06f),point+Vector3.up*(height-radius),radius+.04f,~0,QueryTriggerInteraction.Ignore)){
     var player=hit.GetComponentInParent<Athlete>();bool passenger=false;if(player)for(int j=0;j<roster.Count;j++)if(roster[j]==player){passenger=true;break;}
     if(passenger)continue;
     error=$"{island} {destination} slot {i+1} is obstructed by {hit.name}.";return false;
    }
    for(int j=0;j<i;j++){float other=roster[j].capsule.radius*Mathf.Max(Mathf.Abs(roster[j].transform.lossyScale.x),Mathf.Abs(roster[j].transform.lossyScale.z));if(Vector3.Distance(plan[j].position,point-islandOffset)<radius+other+.15f){error="Landing slots overlap.";return false;}}
    plan[i]=new TeleportLanding(point-islandOffset,slot.rotation);
   }
   landings=plan;return true;
  }
 }
}
