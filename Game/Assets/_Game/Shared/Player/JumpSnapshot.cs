using System;
using Unity.Netcode;

namespace WhatTheFish {
 public struct JumpSnapshot:INetworkSerializable,IEquatable<JumpSnapshot> {
  public bool airborne,preparing;public float velocity,landing,load;public uint sequence;
  public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T:IReaderWriter {
   serializer.SerializeValue(ref airborne);serializer.SerializeValue(ref preparing);serializer.SerializeValue(ref load);serializer.SerializeValue(ref velocity);serializer.SerializeValue(ref landing);serializer.SerializeValue(ref sequence);
  }
  public bool Equals(JumpSnapshot other)=>airborne==other.airborne&&preparing==other.preparing&&load==other.load&&velocity==other.velocity&&landing==other.landing&&sequence==other.sequence;
 }
}
