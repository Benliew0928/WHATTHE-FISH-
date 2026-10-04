using System;
using Unity.Netcode;
using UnityEngine;

namespace WhatTheFish {
 public enum GolfCartAction : byte { None, Summon, Recall, Drive, Leave }
 public struct GolfCartState : INetworkSerializable, IEquatable<GolfCartState> {
  public const ulong NoDriver=ulong.MaxValue;
  public bool summoned; public Vector3 position; public Quaternion rotation;
  public float speed,steering; public ulong driver; public uint revision;
  public void NetworkSerialize<T>(BufferSerializer<T> s) where T:IReaderWriter {
   s.SerializeValue(ref summoned);s.SerializeValue(ref position);s.SerializeValue(ref rotation);
   s.SerializeValue(ref speed);s.SerializeValue(ref steering);s.SerializeValue(ref driver);s.SerializeValue(ref revision);
  }
  public bool Equals(GolfCartState b)=>summoned==b.summoned&&position.Equals(b.position)&&rotation.Equals(b.rotation)&&speed==b.speed&&steering==b.steering&&driver==b.driver&&revision==b.revision;
 }
}
