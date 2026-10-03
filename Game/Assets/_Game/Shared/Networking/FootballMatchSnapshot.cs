using System;
using Unity.Netcode;
namespace WhatTheFish {
 public struct FootballMatchSnapshot:INetworkSerializable,IEquatable<FootballMatchSnapshot> {
  public FootballMatchPhase phase;public FootballMatchResult result;public int scoreA,scoreB,teamA,teamB,teamCapacity;public double deadline,remaining;public uint revision;
  public void NetworkSerialize<T>(BufferSerializer<T> s) where T:IReaderWriter {s.SerializeValue(ref phase);s.SerializeValue(ref result);s.SerializeValue(ref scoreA);s.SerializeValue(ref scoreB);s.SerializeValue(ref deadline);s.SerializeValue(ref remaining);s.SerializeValue(ref revision);s.SerializeValue(ref teamA);s.SerializeValue(ref teamB);s.SerializeValue(ref teamCapacity);}
  public bool Equals(FootballMatchSnapshot v)=>phase==v.phase&&result==v.result&&scoreA==v.scoreA&&scoreB==v.scoreB&&deadline==v.deadline&&remaining==v.remaining&&revision==v.revision&&teamA==v.teamA&&teamB==v.teamB&&teamCapacity==v.teamCapacity;
 }
}
