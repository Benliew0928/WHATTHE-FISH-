using System;
using Unity.Netcode;

namespace WhatTheFish {
 public struct BasketballCharge:INetworkSerializable,IEquatable<BasketballCharge> {
  public bool active;public double phase,time;public float period,pressure;public byte hoop;
  public void NetworkSerialize<T>(BufferSerializer<T> s) where T:IReaderWriter {s.SerializeValue(ref active);s.SerializeValue(ref phase);s.SerializeValue(ref time);s.SerializeValue(ref period);s.SerializeValue(ref pressure);s.SerializeValue(ref hoop);}
  public bool Equals(BasketballCharge other)=>active==other.active&&phase==other.phase&&time==other.time&&period==other.period&&pressure==other.pressure&&hoop==other.hoop;
 }
}
