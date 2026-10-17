using System;
using System.Linq;
using UnityEngine;

namespace WhatTheFish {
 // One join flow, two independent numbered rows. Only the server changes a seat.
 public static class RoomSeats {
  public const byte Unassigned=255;
  public const int Spectators=10;
  public static bool Watching(byte seat)=>seat>=10&&seat<20;
  public static NetworkAthlete[] Occupants=>UnityEngine.Object.FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None).Where(n=>n.IsSpawned).ToArray();
  public static int PlayingCount=>RoomTeleport.Players().Count(n=>!n.IsSpectator);
  public static bool Allowed=>NetworkAthlete.HostPlayer&&NetworkAthlete.HostPlayer.AllowSpectators.Value;
  public static bool Fits(SportId sport)=>PlayingCount<=TeleportDestinations.Capacity(sport);
  public static byte Free(SportId sport,bool spectator,int preferred=0){
   int count=spectator?10:TeleportDestinations.Capacity(sport),start=spectator?10:0;
   var used=Occupants.Select(n=>n.RoomSeat.Value).Concat(AppRoot.Instance.rooms.PendingSeats).ToArray();
   for(int i=0;i<count;i++){byte seat=(byte)(start+(Mathf.Clamp(preferred,0,count-1)+i)%count);if(!used.Contains(seat))return seat;}
   return Unassigned;
  }
  public static bool Choose(NetworkAthlete actor,byte seat,out string reason){
   reason="";var host=NetworkAthlete.HostPlayer;
   if(!actor||!actor.IsServer||!actor.IsPlayerObject||!host)return false;
   if(host.Exploring.Value||RoomTeleport.Busy){reason="Choose your place in the waiting room before travelling.";return false;}
   bool spectator=Watching(seat);
   if(seat>=20||!spectator&&seat>=TeleportDestinations.Capacity(host.WorldSport.Value)){reason="That player place is unavailable on this island.";return false;}
   if(spectator&&!host.AllowSpectators.Value){reason="The host has closed spectator places.";return false;}
   if(AppRoot.Instance.rooms.PendingSeats.Contains(seat)||Occupants.Any(n=>n!=actor&&n.RoomSeat.Value==seat)){reason="That place is already taken. Choose an empty place.";return false;}
   if(actor.RoomSeat.Value==seat)return true;
   actor.SetRoomSeat(seat);actor.Ready.Value=false;return true;
  }
  public static void Normalize(SportId sport){
   foreach(var n in Occupants.OrderBy(n=>n.RoomSeat.Value))if(n.IsServer&&!n.IsSpectator&&n.RoomSeat.Value>=TeleportDestinations.Capacity(sport)){byte seat=Free(sport,false);if(seat!=Unassigned)n.SetRoomSeat(seat);}
  }
  public static string Signature()=>string.Join(",",RoomTeleport.Players().Select(n=>n.NetworkObjectId+":"+n.RoomSeat.Value+":"+n.Nickname.Value))+"/"+Allowed;
  public static TeleportLanding Landing(SportId sport,TeleportDestination kind,Athlete actor,int fallback,int playerIndex){
   var net=actor.GetComponent<NetworkAthlete>();
   bool spectator=actor.IsSpectator;int slot=net&&net.IsSpawned?net.RoomSeat.Value%10:fallback;
   if(kind==TeleportDestination.Station){
    // Four rows fit the existing boarding platform: players in front, spectators behind.
    int index=spectator?10+slot:playerIndex;
    return StationLanding(sport,index);
   }
   if(!spectator){
    if(net&&net.IsSpawned){
     int capacity=TeleportDestinations.Capacity(sport);
     if(slot>=capacity){var used=Occupants.Where(n=>!n.IsSpectator&&n.RoomSeat.Value<capacity).Select(n=>(int)n.RoomSeat.Value).ToHashSet();foreach(var n in Occupants.Where(n=>!n.IsSpectator&&n.RoomSeat.Value>=capacity).OrderBy(n=>n.RoomSeat.Value)){int free=Enumerable.Range(0,capacity).First(i=>!used.Contains(i));used.Add(free);if(n==net){slot=free;break;}}}
    }else slot=playerIndex;
    return TeleportDestinations.Nominal(sport,kind,slot);
   }
   return SpectatorLanding(sport,slot);
  }
  public static TeleportLanding StationLanding(SportId sport,int index)=>new(SkySailMap.Port(sport)+SkySailMap.Facing(sport)*new Vector3((index%5-2)*1.8f,.76f,-2.6f-index/5*1.4f),SkySailMap.Facing(sport)*Quaternion.Euler(0,180,0));
  public static TeleportLanding SpectatorLanding(SportId sport,int slot){
   if(slot<0||slot>=10)throw new ArgumentOutOfRangeException(nameof(slot));
   Vector3 p;
   if(sport==SportId.Football||sport==SportId.Basketball){
    // Arc-length spacing along the existing seating promenade. Dimensions match
    // build_football_stadium.py / build_rally_arena.py; Blender XY -> Unity -X/-Z.
    bool football=sport==SportId.Football;
    p=SeatingPerimeter(football?42:12.5f,football?61:19.5f,football?17:6,football?10.85f:7.05f,(slot+(football?.395f:.35f))/10);
    p.y=football?8.25f:6.10f;
   }else if(sport==SportId.Fishing){float angle=(slot*36+7.2f)*Mathf.Deg2Rad;p=new Vector3(Mathf.Cos(angle)*30,3,Mathf.Sin(angle)*30);}
   else p=new Vector3(-20+slot%5*4,5,-154-slot/5*3);
   return new TeleportLanding(p,Quaternion.LookRotation(new Vector3(-p.x,0,-p.z)));
  }
  public static Vector3 SeatingPerimeter(float x,float z,float radius,float depth,float fraction){
   x+=depth;z+=depth;radius+=depth;float vertical=2*(z-radius),horizontal=2*(x-radius),arc=radius*Mathf.PI/2;
   float s=Mathf.Repeat(fraction,1)*(2*vertical+2*horizontal+4*arc);
   var starts=new[]{new Vector2(x,-z+radius),new Vector2(x-radius,z),new Vector2(-x,z-radius),new Vector2(-x+radius,-z)};
   var directions=new[]{Vector2.up,Vector2.left,Vector2.down,Vector2.right};
   var centers=new[]{new Vector2(x-radius,z-radius),new Vector2(-x+radius,z-radius),new Vector2(-x+radius,-z+radius),new Vector2(x-radius,-z+radius)};
   for(int i=0;i<4;i++){float length=i%2==0?vertical:horizontal;if(s<=length){var v=starts[i]+directions[i]*s;return new Vector3(-v.x,0,-v.y);}s-=length;if(s<=arc){float angle=i*Mathf.PI/2+s/radius;var v=centers[i]+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius;return new Vector3(-v.x,0,-v.y);}s-=arc;}
   return Vector3.zero;
  }
 }
}
