using System;
using Unity.Netcode;
using Unity.Collections;
using UnityEngine;

namespace WhatTheFish {
 public struct BasketballMatchSnapshot:INetworkSerializable,IEquatable<BasketballMatchSnapshot> {
  public BasketballMatchPhase phase;public FootballMatchResult result;public FootballTeam possession;
  public BasketballRestart restart;public int scoreA,scoreB,teamA,teamB,capacity;
  public double deadline,remaining,shotDeadline;public uint revision;
  public ulong inbounder;public FixedList128Bytes<ulong> automaticPlayers;
  public int inboundTurnA,inboundTurnB;public Vector3 inboundExit;
  public bool IsAutomatic(ulong id){for(int i=0;i<automaticPlayers.Length;i++)if(automaticPlayers[i]==id)return true;return false;}
  public void NetworkSerialize<T>(BufferSerializer<T> s) where T:IReaderWriter {
   s.SerializeValue(ref phase);s.SerializeValue(ref result);s.SerializeValue(ref possession);s.SerializeValue(ref restart);
   s.SerializeValue(ref scoreA);s.SerializeValue(ref scoreB);s.SerializeValue(ref teamA);s.SerializeValue(ref teamB);s.SerializeValue(ref capacity);
   s.SerializeValue(ref deadline);s.SerializeValue(ref remaining);s.SerializeValue(ref shotDeadline);s.SerializeValue(ref revision);
   s.SerializeValue(ref inbounder);s.SerializeValue(ref inboundTurnA);s.SerializeValue(ref inboundTurnB);s.SerializeValue(ref inboundExit);int count=automaticPlayers.Length;s.SerializeValue(ref count);
   if(count<0||count>10)throw new InvalidOperationException("Invalid basketball restart roster");
   if(s.IsReader)automaticPlayers.Clear();
   for(int i=0;i<count;i++){ulong id=s.IsReader?0:automaticPlayers[i];s.SerializeValue(ref id);if(s.IsReader)automaticPlayers.Add(id);}
  }
  public bool Equals(BasketballMatchSnapshot v)=>inboundTurnA==v.inboundTurnA&&inboundTurnB==v.inboundTurnB&&inboundExit==v.inboundExit&&inbounder==v.inbounder&&automaticPlayers.Equals(v.automaticPlayers)&&phase==v.phase&&result==v.result&&possession==v.possession&&restart==v.restart&&scoreA==v.scoreA&&scoreB==v.scoreB&&teamA==v.teamA&&teamB==v.teamB&&capacity==v.capacity&&deadline==v.deadline&&remaining==v.remaining&&shotDeadline==v.shotDeadline&&revision==v.revision;
 }
}
