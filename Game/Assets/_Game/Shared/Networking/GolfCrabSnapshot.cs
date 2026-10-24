using System;
using Unity.Netcode;
using UnityEngine;
namespace WhatTheFish {
 public struct GolfCrabSnapshot:INetworkSerializable,IEquatable<GolfCrabSnapshot> {
  public int lane;public uint round,sequence;public bool hasTarget;public ulong target;public Vector3 position,velocity,retreatGoal;public double time,retreatUntil;
  public void NetworkSerialize<T>(BufferSerializer<T> s) where T:IReaderWriter {s.SerializeValue(ref lane);s.SerializeValue(ref round);s.SerializeValue(ref sequence);s.SerializeValue(ref hasTarget);s.SerializeValue(ref target);s.SerializeValue(ref position);s.SerializeValue(ref velocity);s.SerializeValue(ref retreatGoal);s.SerializeValue(ref time);s.SerializeValue(ref retreatUntil);}
  public bool Equals(GolfCrabSnapshot b)=>lane==b.lane&&round==b.round&&sequence==b.sequence&&hasTarget==b.hasTarget&&target==b.target&&position==b.position&&velocity==b.velocity&&retreatGoal==b.retreatGoal&&time==b.time&&retreatUntil==b.retreatUntil;
 }
 public struct GolfCrabCooldown:INetworkSerializable,IEquatable<GolfCrabCooldown> {
  public ulong owner;public uint round,sequence;public double until;
  public void NetworkSerialize<T>(BufferSerializer<T> s) where T:IReaderWriter {s.SerializeValue(ref owner);s.SerializeValue(ref round);s.SerializeValue(ref sequence);s.SerializeValue(ref until);}
  public bool Equals(GolfCrabCooldown b)=>owner==b.owner&&round==b.round&&sequence==b.sequence&&until==b.until;
 }
}
