using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
namespace WhatTheFish {
 public struct BasketballPlayerRecord:INetworkSerializable,IEquatable<BasketballPlayerRecord> {
  public ulong id;public FootballTeam team;public FixedString64Bytes name;public byte number;public int points,assists,passes,interceptions;
  public int Rating=>points+assists;
  public void NetworkSerialize<T>(BufferSerializer<T> s) where T:IReaderWriter {s.SerializeValue(ref id);s.SerializeValue(ref team);s.SerializeValue(ref name);s.SerializeValue(ref number);s.SerializeValue(ref points);s.SerializeValue(ref assists);s.SerializeValue(ref passes);s.SerializeValue(ref interceptions);}
  public bool Equals(BasketballPlayerRecord v)=>id==v.id&&team==v.team&&name==v.name&&number==v.number&&points==v.points&&assists==v.assists&&passes==v.passes&&interceptions==v.interceptions;
 }
 // Intentional releases only. Rebounds, dribbles and shot blocks never become passes.
 public sealed class BasketballStatistics {
  public const double AssistSeconds=5,PassLifetime=5;
  public readonly Dictionary<ulong,BasketballPlayerRecord> Players=new();
  ulong? last,passer,assistant,receiver,firstContact;double released,assisted;bool shot;
  public void Clear(){Players.Clear();ResetPlay();}
  public void ResetPlay(){last=passer=assistant=receiver=firstContact=null;shot=false;}
  public void Touch(ulong id,double now,bool controlled){
   if(!Players.TryGetValue(id,out var actor)||actor.team==FootballTeam.None)return;
   if(shot&&(controlled||last!=id)){assistant=receiver=null;shot=false;}
   if(passer.HasValue){
    var from=Players[passer.Value];
    if(now<released||now-released>PassLifetime)passer=null;
    else if(id!=passer&&actor.team!=from.team){actor.interceptions++;Players[id]=actor;passer=null;assistant=receiver=null;}
    else if(id!=passer&&controlled){from.passes++;Players[from.id]=from;assistant=!firstContact.HasValue||firstContact==id?from.id:null;receiver=id;assisted=released;passer=null;last=id;return;}
    else if(id==passer&&controlled)passer=null;
    if(id!=from.id&&!firstContact.HasValue)firstContact=id;
   }
   if(last!=id){assistant=receiver=null;last=id;}
  }
  public void Pass(ulong id,double now){if(!Players.ContainsKey(id))return;Touch(id,now,true);assistant=receiver=firstContact=null;shot=false;passer=last=id;released=now;}
  public void Shot(ulong id,double now){Touch(id,now,true);passer=null;shot=true;}
  public void Miss(){assistant=receiver=null;shot=false;}
  public void Basket(FootballTeam scoring,int value,double now,ulong? shooter=null){
   var credited=shooter??last;
   if(value>=2&&value<=3&&credited.HasValue&&Players.TryGetValue(credited.Value,out var scorer)&&scorer.team==scoring){
    scorer.points+=value;Players[scorer.id]=scorer;
    if(receiver==credited&&assistant.HasValue&&assistant!=credited&&now>=assisted&&now-assisted<=AssistSeconds&&Players.TryGetValue(assistant.Value,out var helper)&&helper.team==scoring){helper.assists++;Players[helper.id]=helper;}
   }
   ResetPlay();
  }
  public static IOrderedEnumerable<BasketballPlayerRecord> Ranked(IEnumerable<BasketballPlayerRecord> records)=>records.OrderByDescending(p=>p.Rating).ThenByDescending(p=>p.points).ThenByDescending(p=>p.passes).ThenBy(p=>p.id);
  public static BasketballPlayerRecord MVP(IEnumerable<BasketballPlayerRecord> records)=>Ranked(records).FirstOrDefault();
 }
 public sealed partial class BasketballMatch {
  public readonly BasketballStatistics Statistics=new();
  public IEnumerable<BasketballPlayerRecord> PlayerRecords {get {if(Authority)return Statistics.Players.Values;var result=new List<BasketballPlayerRecord>();var host=NetworkAthlete.HostPlayer;if(host)foreach(var p in host.BasketballPlayers)result.Add(p);return result;}}
  public BasketballPlayerRecord MVP=>BasketballStatistics.MVP(PlayerRecords);
  public void RecordPass(Athlete actor){if(Authority&&Running&&Contains(actor))Statistics.Pass(PlayerId(actor),Now);}
  public void RecordShot(Athlete actor){if(Authority&&Running&&Contains(actor))Statistics.Shot(PlayerId(actor),Now);}
  void RefreshStatisticsRoster(){
   foreach(var pair in participants){var a=pair.Value;if(!a)continue;Statistics.Players.TryGetValue(pair.Key,out var r);r.id=pair.Key;r.team=Selection.TeamOf(pair.Key);r.number=CharacterIdentity.Ensure(a).Number;var net=a.GetComponent<NetworkAthlete>();var bot=a.GetComponent<BasketballBot>();r.name=net&&net.IsSpawned?net.Nickname.Value:new FixedString64Bytes(bot?"BOT "+bot.Slot:PlayerPrefs.GetString("fishing.nickname","YOU"));Statistics.Players[pair.Key]=r;}
  }
  void PublishStatistics(){var host=NetworkAthlete.HostPlayer;if(!Authority||!host||!host.IsSpawned)return;var records=Statistics.Players.Values.OrderBy(p=>p.id).ToArray();if(host.BasketballPlayers.Count!=records.Length){host.BasketballPlayers.Clear();foreach(var r in records)host.BasketballPlayers.Add(r);}else for(int i=0;i<records.Length;i++)if(!host.BasketballPlayers[i].Equals(records[i]))host.BasketballPlayers[i]=records[i];}
 }
}
