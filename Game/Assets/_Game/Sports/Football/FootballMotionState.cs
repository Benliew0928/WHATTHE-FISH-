using System;
using Unity.Netcode;
using UnityEngine;

namespace WhatTheFish {
 public enum FootballGesture:byte { None,Kick,Cancel,Bump }
 public struct FootballMotionState:INetworkSerializable,IEquatable<FootballMotionState> {
  public FootballGesture gesture;public uint sequence;public double started,chargeStarted;public bool charging,left;public float power;public Vector3 contact;
  public void NetworkSerialize<T>(BufferSerializer<T> s)where T:IReaderWriter{s.SerializeValue(ref gesture);s.SerializeValue(ref sequence);s.SerializeValue(ref started);s.SerializeValue(ref chargeStarted);s.SerializeValue(ref charging);s.SerializeValue(ref left);s.SerializeValue(ref power);s.SerializeValue(ref contact);}
  public bool Equals(FootballMotionState b)=>gesture==b.gesture&&sequence==b.sequence&&started==b.started&&chargeStarted==b.chargeStarted&&charging==b.charging&&left==b.left&&power==b.power&&contact.Equals(b.contact);
 }
}
