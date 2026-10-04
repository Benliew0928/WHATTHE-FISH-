using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace WhatTheFish {
 // Cosmetic reliable events; does not change the athlete/ball snapshot schema.
 public static class FootballNetEvents {
  const string Channel="Football.NetImpact.v1";
  static readonly List<FootballNetSurface> nets=new();
  static CustomMessagingManager bus;
  public static void Add(FootballNetSurface net){if(!nets.Contains(net))nets.Add(net);EnsureConnected();}
  public static void Remove(FootballNetSurface net){nets.Remove(net);if(nets.Count==0&&bus!=null){bus.UnregisterNamedMessageHandler(Channel);bus=null;}}
  public static void EnsureConnected(){
   var network=NetworkManager.Singleton;var current=network&&network.IsListening?network.CustomMessagingManager:null;
   if(ReferenceEquals(current,bus))return;
   if(bus!=null)bus.UnregisterNamedMessageHandler(Channel);bus=current;
   if(bus!=null&&nets.Count>0)bus.RegisterNamedMessageHandler(Channel,Receive);
  }
  static uint Journey=>NetworkAthlete.HostPlayer?NetworkAthlete.HostPlayer.WorldTravel.Value.sequence:0;
  public static void Send(FootballNetSurface net,Vector3 point,Vector3 impulse,float radius){
   EnsureConnected();var network=NetworkManager.Singleton;if(bus==null||!network.IsServer)return;
   using var writer=new FastBufferWriter(64,Allocator.Temp);
   writer.WriteValueSafe(net.Sign);writer.WriteValueSafe(point);writer.WriteValueSafe(impulse);writer.WriteValueSafe(radius);writer.WriteValueSafe(network.ServerTime.Time);writer.WriteValueSafe(Journey);
   bus.SendNamedMessageToAll(Channel,writer,NetworkDelivery.ReliableSequenced);
  }
  static void Receive(ulong sender,FastBufferReader reader){
   var network=NetworkManager.Singleton;
   if(!network||network.IsServer||sender!=NetworkManager.ServerClientId||!FootballTackle.EnvironmentAllowed||!reader.TryBeginRead(44))return;
   reader.ReadValueSafe(out int sign);reader.ReadValueSafe(out Vector3 point);reader.ReadValueSafe(out Vector3 impulse);reader.ReadValueSafe(out float radius);reader.ReadValueSafe(out double started);reader.ReadValueSafe(out uint journey);
   double age=network.ServerTime.Time-started;
   if(!double.IsFinite(age)||age<-.25||age>FootballNetSurface.Lifetime||journey!=Journey)return;
   foreach(var net in nets)if(net&&net.Sign==sign){net.Receive(point,impulse,radius,Time.time-(float)System.Math.Max(0,age));break;}
  }
 }
}
