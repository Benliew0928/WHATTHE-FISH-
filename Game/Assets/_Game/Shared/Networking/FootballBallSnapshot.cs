using System;
using Unity.Netcode;
using UnityEngine;
namespace WhatTheFish {
 public struct FootballBallSnapshot:INetworkSerializable,IEquatable<FootballBallSnapshot> {
  public bool valid;public Vector3 position;public Quaternion rotation;public uint sequence;public ulong controllerId;
  public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T:IReaderWriter {serializer.SerializeValue(ref valid);serializer.SerializeValue(ref position);serializer.SerializeValue(ref rotation);serializer.SerializeValue(ref sequence);serializer.SerializeValue(ref controllerId);}
  public bool Equals(FootballBallSnapshot other)=>valid==other.valid&&position.Equals(other.position)&&rotation.Equals(other.rotation)&&sequence==other.sequence&&controllerId==other.controllerId;
 }
}
