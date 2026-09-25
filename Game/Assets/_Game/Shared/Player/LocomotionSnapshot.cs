using System;
using Unity.Netcode;

namespace WhatTheFish {
 public struct LocomotionSnapshot : INetworkSerializable,IEquatable<LocomotionSnapshot> {
  public LocomotionPhase phase;
  public float speed,angle,startYaw,duration;
  public double started;
  public uint sequence;
  public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T:IReaderWriter {
   serializer.SerializeValue(ref phase);serializer.SerializeValue(ref speed);serializer.SerializeValue(ref angle);
   serializer.SerializeValue(ref startYaw);serializer.SerializeValue(ref duration);serializer.SerializeValue(ref started);serializer.SerializeValue(ref sequence);
  }
  public bool Equals(LocomotionSnapshot other)=>phase==other.phase&&speed==other.speed&&angle==other.angle&&startYaw==other.startYaw&&duration==other.duration&&started==other.started&&sequence==other.sequence;
 }
}
