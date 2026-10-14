using System;
using Unity.Netcode;

namespace WhatTheFish {
 public struct BasketballMatchSnapshot:INetworkSerializable,IEquatable<BasketballMatchSnapshot> {
  public BasketballMatchPhase phase;public FootballMatchResult result;public FootballTeam possession;
  public BasketballRestart restart;public int scoreA,scoreB,teamA,teamB,capacity;
  public double deadline,remaining,shotDeadline;public uint revision;
  public void NetworkSerialize<T>(BufferSerializer<T> s) where T:IReaderWriter {
   s.SerializeValue(ref phase);s.SerializeValue(ref result);s.SerializeValue(ref possession);s.SerializeValue(ref restart);
   s.SerializeValue(ref scoreA);s.SerializeValue(ref scoreB);s.SerializeValue(ref teamA);s.SerializeValue(ref teamB);s.SerializeValue(ref capacity);
   s.SerializeValue(ref deadline);s.SerializeValue(ref remaining);s.SerializeValue(ref shotDeadline);s.SerializeValue(ref revision);
  }
  public bool Equals(BasketballMatchSnapshot v)=>phase==v.phase&&result==v.result&&possession==v.possession&&restart==v.restart&&scoreA==v.scoreA&&scoreB==v.scoreB&&teamA==v.teamA&&teamB==v.teamB&&capacity==v.capacity&&deadline==v.deadline&&remaining==v.remaining&&shotDeadline==v.shotDeadline&&revision==v.revision;
 }
}
