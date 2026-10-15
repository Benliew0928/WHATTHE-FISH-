using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
namespace WhatTheFish {
 public struct FootballPlayerRecord:INetworkSerializable,IEquatable<FootballPlayerRecord> {
  public ulong id;public FootballTeam team;public FixedString64Bytes name;public byte number;public int goals,assists,passes,interceptions;
  public int Points=>2*goals+assists;
  public void NetworkSerialize<T>(BufferSerializer<T> s) where T:IReaderWriter {s.SerializeValue(ref id);s.SerializeValue(ref team);s.SerializeValue(ref name);s.SerializeValue(ref number);s.SerializeValue(ref goals);s.SerializeValue(ref assists);s.SerializeValue(ref passes);s.SerializeValue(ref interceptions);}
  public bool Equals(FootballPlayerRecord v)=>id==v.id&&team==v.team&&name==v.name&&number==v.number&&goals==v.goals&&assists==v.assists&&passes==v.passes&&interceptions==v.interceptions;
 }
 // Only the host calls these events. A pass can complete or be intercepted once.
 public sealed class FootballStatistics {
  public const double AssistSeconds=5,PassLifetime=5;
  public readonly Dictionary<ulong,FootballPlayerRecord> Players=new();
  ulong? last,passer,assistant,receiver;double released,assisted;
  public void Clear(){Players.Clear();ResetPlay();}
  public void ResetPlay(){last=passer=assistant=receiver=null;}
  public void Touch(ulong id,double now,bool controlled){
   if(!Players.TryGetValue(id,out var actor))return;
   if(passer.HasValue){
    var from=Players[passer.Value];
    if(now-released>PassLifetime||now<released){passer=null;}
    else if(id!=passer&&actor.team!=from.team){actor.interceptions++;Players[id]=actor;passer=null;assistant=receiver=null;}
    else if(id!=passer&&controlled){from.passes++;Players[from.id]=from;assistant=from.id;receiver=id;assisted=released;passer=null;last=id;return;}
    else if(id==passer&&controlled)passer=null;
   }
   if(last!=id){assistant=receiver=null;last=id;}
  }
  public void Pass(ulong id,double now){if(!Players.ContainsKey(id))return;Touch(id,now,true);assistant=receiver=null;passer=last=id;released=now;}
  public void Shot(ulong id,double now){Touch(id,now,true);passer=null;}
  public void Goal(FootballTeam scoring,double now){
   if(last.HasValue&&Players.TryGetValue(last.Value,out var scorer)&&scorer.team==scoring){scorer.goals++;Players[last.Value]=scorer;
    if(receiver==last&&assistant.HasValue&&assistant!=last&&now>=assisted&&now-assisted<=AssistSeconds&&Players.TryGetValue(assistant.Value,out var helper)&&helper.team==scoring){helper.assists++;Players[assistant.Value]=helper;}}
   ResetPlay();
  }
  public static FootballPlayerRecord MVP(IEnumerable<FootballPlayerRecord> records)=>records.OrderByDescending(p=>p.Points).ThenByDescending(p=>p.goals).ThenByDescending(p=>p.passes).ThenBy(p=>p.id).FirstOrDefault();
 }
 public sealed partial class FootballMatch {
  public readonly FootballStatistics Statistics=new();
  public IEnumerable<FootballPlayerRecord> PlayerRecords {get {if(Authority)return Statistics.Players.Values;var host=NetworkAthlete.HostPlayer;var result=new List<FootballPlayerRecord>();if(host)foreach(var p in host.FootballPlayers)result.Add(p);return result;}}
  public FootballPlayerRecord MVP=>FootballStatistics.MVP(PlayerRecords);
  public void RecordTouch(Athlete actor,bool controlled=false){if(Authority&&Running&&actor)Statistics.Touch(PlayerId(actor),Now,controlled);}
  public void RecordPass(Athlete actor){if(Authority&&Running&&actor)Statistics.Pass(PlayerId(actor),Now);}
  public void RecordShot(Athlete actor){if(Authority&&Running&&actor)Statistics.Shot(PlayerId(actor),Now);}
  void RefreshStatisticsRoster(){
   foreach(var pair in participants){var a=pair.Value;if(!a)continue;Statistics.Players.TryGetValue(pair.Key,out var record);record.id=pair.Key;record.team=TeamOf(a);record.number=CharacterIdentity.Ensure(a).Number;var net=a.GetComponent<NetworkAthlete>();record.name=net&&net.IsSpawned?net.Nickname.Value:new FixedString64Bytes(FootballBots.IsBot(a)?"BOT "+a.GetComponent<FootballBot>().Slot:PlayerPrefs.GetString("fishing.nickname","YOU"));Statistics.Players[pair.Key]=record;}
  }
  void PublishStatistics(){var host=NetworkAthlete.HostPlayer;if(!Authority||!host||!host.IsSpawned)return;var records=Statistics.Players.Values.OrderBy(p=>p.id).ToArray();if(host.FootballPlayers.Count!=records.Length){host.FootballPlayers.Clear();foreach(var r in records)host.FootballPlayers.Add(r);}else for(int i=0;i<records.Length;i++)if(!host.FootballPlayers[i].Equals(records[i]))host.FootballPlayers[i]=records[i];}
 }
}
