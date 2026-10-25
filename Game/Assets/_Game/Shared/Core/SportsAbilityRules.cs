using System;
using Unity.Netcode;
using UnityEngine;

namespace WhatTheFish {
 public enum SportsProp:byte { None,LockOn,Turbo,Portal,Wall }
 // Host-authored presentation event. Gameplay starts immediately; this never
 // gates simulation, possession, input or ability deadlines.
 public struct SportsPropActivation:INetworkSerializable,IEquatable<SportsPropActivation> {
  public SportsProp kind;public ulong actor;public uint sequence;public double started;
  public const float Duration=2.6f;
  public void NetworkSerialize<T>(BufferSerializer<T> s) where T:IReaderWriter {s.SerializeValue(ref kind);s.SerializeValue(ref actor);s.SerializeValue(ref sequence);s.SerializeValue(ref started);}
  public bool Equals(SportsPropActivation b)=>kind==b.kind&&actor==b.actor&&sequence==b.sequence&&started==b.started;
 }
 // An owner sends the observed revision, never an inventory type or recipient.
 public struct SportsPropSlot:INetworkSerializable,IEquatable<SportsPropSlot> {
  public SportsProp kind;public uint revision;
  public void NetworkSerialize<T>(BufferSerializer<T> s) where T:IReaderWriter {s.SerializeValue(ref kind);s.SerializeValue(ref revision);}
  public bool Equals(SportsPropSlot b)=>kind==b.kind&&revision==b.revision;
 }
 public struct SportsPropSpawn:INetworkSerializable,IEquatable<SportsPropSpawn> {
  public SportsProp kind;public byte anchor;public double respawn;public bool countingDown;
  public int CountdownNumber(double now)=>countingDown?Mathf.Clamp((int)Math.Ceiling(respawn-now),1,5):0;
  public void NetworkSerialize<T>(BufferSerializer<T> s) where T:IReaderWriter {s.SerializeValue(ref kind);s.SerializeValue(ref anchor);s.SerializeValue(ref respawn);s.SerializeValue(ref countingDown);}
  public bool Equals(SportsPropSpawn b)=>kind==b.kind&&anchor==b.anchor&&respawn==b.respawn&&countingDown==b.countingDown;
 }
 public static class SportsAbilityRules {
  public const float TurboSeconds=5,TurboMultiplier=1.5f,PortalSeconds=.7f,RespawnSeconds=10,CountdownSeconds=5,PickupRadius=1.2f;
  // One deadline per pickup cycle. Ball restarts must never reschedule it.
  public static SportsPropSpawn SchedulePickup(double now,int anchor)=>new SportsPropSpawn{anchor=(byte)anchor,respawn=now+RespawnSeconds};
  public static bool WarnPickup(ref SportsPropSpawn spawn,double now){if(spawn.kind!=SportsProp.None||spawn.countingDown||now<spawn.respawn-CountdownSeconds)return false;spawn.countingDown=true;return true;}
  public static bool Finite(Vector3 p)=>float.IsFinite(p.x)&&float.IsFinite(p.y)&&float.IsFinite(p.z);
  // Each sport owns three contiguous IDs. Sample two alternatives directly.
  public static SportsProp NextKind(SportsProp previous,int choice,int first){int p=(int)previous-first;return (SportsProp)(first+(p<0||p>2?choice%3:(p+1+choice%2)%3));}
  public static ulong PlayerId(Athlete a){var n=a?a.GetComponent<NetworkAthlete>():null;return n&&n.IsSpawned?n.StableId:ulong.MaxValue;}
 }
}
