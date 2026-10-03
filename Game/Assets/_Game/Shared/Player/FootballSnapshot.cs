using System;
using Unity.Netcode;

namespace WhatTheFish {
 public struct FootballSnapshot:INetworkSerializable,IEquatable<FootballSnapshot> {
  public FootballAction action;public double started,cooldownUntil,whiffUntil;public uint sequence;public byte hitVariant;
  public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T:IReaderWriter {
   serializer.SerializeValue(ref action);serializer.SerializeValue(ref started);serializer.SerializeValue(ref cooldownUntil);serializer.SerializeValue(ref sequence);
   serializer.SerializeValue(ref hitVariant);serializer.SerializeValue(ref whiffUntil);
  }
  public bool Equals(FootballSnapshot other)=>action==other.action&&started==other.started&&cooldownUntil==other.cooldownUntil&&sequence==other.sequence&&hitVariant==other.hitVariant&&whiffUntil==other.whiffUntil;
 }
}
