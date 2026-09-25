using System;
using Unity.Netcode;

namespace WhatTheFish {
 public struct FootballSnapshot:INetworkSerializable,IEquatable<FootballSnapshot> {
  public FootballAction action;public double started,cooldownUntil;public uint sequence;
  public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T:IReaderWriter {
   serializer.SerializeValue(ref action);serializer.SerializeValue(ref started);serializer.SerializeValue(ref cooldownUntil);serializer.SerializeValue(ref sequence);
  }
  public bool Equals(FootballSnapshot other)=>action==other.action&&started==other.started&&cooldownUntil==other.cooldownUntil&&sequence==other.sequence;
 }
}
