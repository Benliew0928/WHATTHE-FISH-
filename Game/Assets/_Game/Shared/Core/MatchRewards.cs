using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Scripting;

namespace WhatTheFish {
 [Serializable,Preserve] public sealed class RewardEntrant {public string id;public int team;public ulong actor;}
 [Serializable,Preserve] public sealed class RewardStart {public string requestKey,room,sport,mode;public RewardEntrant[] players;}
 [Serializable,Preserve] public sealed class RewardPlayerResult {public string id;public bool finished;public int score;public double secondary;}
 [Serializable,Preserve] public sealed class RewardFinish {public string id;public bool cancelled;public int scoreA,scoreB;public RewardPlayerResult[] players;}
 [Serializable,Preserve] public sealed class RewardTicket {public string id,status;}
 [Serializable,Preserve] public sealed class RewardJoin {public string id,room;}
 [Serializable,Preserve] public sealed class PendingReward {public string owner,id;public RewardStart start;public RewardFinish finish;public long created;}
 [DefaultExecutionOrder(300)] public sealed class MatchRewards:MonoBehaviour {
  static MatchRewards instance;static PendingReward live;static readonly List<PendingReward> queue=new();
  static string loadedOwner="",joined="",ineligibleTicket="";static bool working;static float retry;static bool disabledMatch;
  [Serializable] sealed class Outbox {public PendingReward[] items;}
  public static bool Pending=>queue.Any(p=>p.owner==PlayerEconomy.Owner&&p.finish!=null&&!p.finish.cancelled);
  public static string Status {get;private set;}="";
  public static bool Ineligible=>NetworkAthlete.HostPlayer&&NetworkAthlete.HostPlayer.CoinMatch.Value.Length>0&&(NetworkAthlete.HostPlayer.CoinMatch.Value.ToString()==ineligibleTicket||AppRoot.Instance&&AppRoot.Instance.LocalAthlete&&AppRoot.Instance.LocalAthlete.IsSpectator);
  static bool Online=>AppRoot.Instance&&AppRoot.Instance.rooms.Connected&&RoomId.Length>0;
  static string RoomId {
   get {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    if(PlayerEconomy.ReviewTransport!=null&&AppRoot.Instance&&AppRoot.Instance.rooms.LocalTest)return "economy-review-"+AppRoot.Instance.SelectedSport;
#endif
    return AppRoot.Instance&&!AppRoot.Instance.rooms.LocalTest?AppRoot.Instance.rooms.Session?.Id??"":"";
   }
  }
  static bool Host=>Online&&AppRoot.Instance.rooms.Host;
  void Awake(){instance=this;}
  void Update(){
   string owner=PlayerEconomy.Owner;if(owner!=loadedOwner){
    loadedOwner=owner;queue.Clear();live=null;disabledMatch=false;joined=ineligibleTicket="";retry=0;
    if(owner.Length>0)try{var saved=JsonUtility.FromJson<Outbox>(PlayerPrefs.GetString("economy.outbox."+owner,""));foreach(var p in saved?.items??Array.Empty<PendingReward>())if(p.owner==owner&&DateTimeOffset.UtcNow.ToUnixTimeSeconds()-p.created<86400){if(p.finish==null)p.finish=new RewardFinish{cancelled=true};queue.Add(p);}}catch(Exception){}
   }
   if(live!=null&&(!Host||RoomId!=live.start.room))End(new RewardFinish{cancelled=true});
   if(!working&&PlayerEconomy.Enabled&&owner.Length>0&&Time.realtimeSinceStartup>=retry)_=Process(owner);
  }
  static void Save(){if(loadedOwner.Length>0){PlayerPrefs.SetString("economy.outbox."+loadedOwner,JsonUtility.ToJson(new Outbox{items=queue.ToArray()}));PlayerPrefs.Save();}}
  static void Begin(SportId sport,int expected,string mode){
   if(live!=null||disabledMatch||!Host)return;
   disabledMatch=true;
   if(NetworkAthlete.HostPlayer){NetworkAthlete.HostPlayer.CoinMatch.Value=default;NetworkAthlete.HostPlayer.CoinState.Value=3;}
   var players=Athlete.Active.Where(a=>a&&a.isActiveAndEnabled&&!a.IsSpectator&&!a.inTransit).Select(a=>new{actor=a,net=a.GetComponent<NetworkAthlete>()}).Where(p=>p.net&&p.net.IsSpawned&&p.net.IsPlayerObject).ToArray();
   if(players.Length!=expected||players.Length<2||players.Any(p=>p.net.ProfileId.Value.Length==0)) {Status="Coins require an online match with human players.";return;}
   var roster=players.Select(p=>new RewardEntrant{id=p.net.ProfileId.Value.ToString(),actor=p.net.StableId,team=sport==SportId.Football?(int)FootballMatch.Instance.TeamOf(p.actor):sport==SportId.Basketball?(int)BasketballMatch.Instance.TeamOf(p.actor):0}).ToArray();
   if(roster.Select(p=>p.id).Distinct().Count()!=roster.Length)return;
   live=new PendingReward{owner=PlayerEconomy.Owner,created=DateTimeOffset.UtcNow.ToUnixTimeSeconds(),start=new RewardStart{requestKey=Guid.NewGuid().ToString("N"),room=RoomId,sport=sport.ToString().ToLowerInvariant(),mode=mode,players=roster}};
   queue.Add(live);Save();retry=0;Status="Match rewards pending.";
   if(NetworkAthlete.HostPlayer)NetworkAthlete.HostPlayer.CoinState.Value=1;
  }
  static void End(RewardFinish result){if(live==null)return;live.finish=result;live=null;Save();retry=0;PlayerEconomy.RefreshSoon();}
  static RewardFinish Result(Func<RewardEntrant,RewardPlayerResult> convert,int a=0,int b=0)=>new RewardFinish{scoreA=a,scoreB=b,players=live.start.players.Select(convert).ToArray()};
  public static void Football(FootballMatch match){
   if(!match.Authority)return;var phase=match.State.Phase;
   if(phase is FootballMatchPhase.Idle or FootballMatchPhase.TeamSelection){if(live?.start.sport=="football")End(new RewardFinish{cancelled=true});disabledMatch=false;return;}
   if(phase==FootballMatchPhase.Cancelled){if(live?.start.sport=="football")End(new RewardFinish{cancelled=true});return;}
   if(phase!=FootballMatchPhase.Finished){Begin(SportId.Football,match.Selection.Count(FootballTeam.A)+match.Selection.Count(FootballTeam.B),"match");return;}
   if(live?.start.sport=="football")End(Result(p=>new RewardPlayerResult{id=p.id,finished=true},match.State.ScoreA,match.State.ScoreB));
  }
  public static void Basketball(BasketballMatch match){
   if(!match.Authority)return;var phase=match.State.Phase;
   if(phase is BasketballMatchPhase.Idle or BasketballMatchPhase.TeamSelection){if(live?.start.sport=="basketball")End(new RewardFinish{cancelled=true});disabledMatch=false;return;}
   if(phase==BasketballMatchPhase.Cancelled){if(live?.start.sport=="basketball")End(new RewardFinish{cancelled=true});return;}
   if(phase!=BasketballMatchPhase.Finished){Begin(SportId.Basketball,match.Selection.Count(FootballTeam.A)+match.Selection.Count(FootballTeam.B),"match");return;}
   if(live?.start.sport=="basketball")End(Result(p=>new RewardPlayerResult{id=p.id,finished=true},match.State.ScoreA,match.State.ScoreB));
  }
  public static void Golf(GolfMatchManager match){
   if(!match.Authority)return;
   if(match.State.Phase==GolfMatchPhase.Idle){if(live?.start.sport=="golf")End(new RewardFinish{cancelled=true});disabledMatch=false;return;}
   if(match.State.Running){Begin(SportId.Golf,match.State.Players.Count,"match");return;}
   if(live?.start.sport=="golf")End(Result(p=>{var r=match.State.Player(p.actor);return new RewardPlayerResult{id=p.id,finished=r!=null&&r.IsFinished&&!r.IsDNF,score=r?.TotalStroke??0,secondary=r?.FinishTime??0};}));
   disabledMatch=false;
  }
  public static void Fishing(FishingGame match){
   if(!match.Authority)return;var m=match.State.Match;
   if(m.phase is FishingRoundPhase.Idle or FishingRoundPhase.Practice||m.mode==FishingMode.Crew){if(live?.start.sport=="fishing")End(new RewardFinish{cancelled=true});disabledMatch=false;return;}
   if(m.outcome==FishingOutcome.Abandoned||m.mode==FishingMode.Cup&&!m.cupValid){if(live?.start.sport=="fishing")End(new RewardFinish{cancelled=true});disabledMatch=false;return;}
   if(m.phase!=FishingRoundPhase.Ended){Begin(SportId.Fishing,match.State.Players.Count,m.mode==FishingMode.Cup?"cup":"round");return;}
   if(m.mode==FishingMode.Cup&&!m.cupFinished)return;
   if(live?.start.sport=="fishing")End(Result(p=>{var r=match.State.Player(p.actor);return new RewardPlayerResult{id=p.id,finished=r.HasValue&&r.Value.connected,score=r.HasValue?(m.cupFinished?r.Value.cupPoints:r.Value.score):0,secondary=r.HasValue&&m.cupFinished?r.Value.cupScore:0};}));
   disabledMatch=false;
  }
  async Task Process(string owner){
   working=true;retry=Time.realtimeSinceStartup+5;
   try{
    if(Online&&NetworkAthlete.HostPlayer){
     string ticket=NetworkAthlete.HostPlayer.CoinMatch.Value.ToString();
     if(ticket.Length>0&&!Ineligible&&NetworkAthlete.HostPlayer.CoinState.Value==2&&PlayerEconomy.Wallet?.history?.Any(t=>t.key=="match:"+ticket)!=true)PlayerEconomy.RefreshSoon();
     if(ticket.Length>0&&ticket!=joined){
      try{await PlayerEconomy.Request<RewardTicket>("matches/join","POST",new RewardJoin{id=ticket,room=RoomId});if(owner==PlayerEconomy.Owner)joined=ticket;}
      catch(ProfileException e)when(e.Status==404||e.Status==403){if(owner==PlayerEconomy.Owner)joined=ineligibleTicket=ticket;}
     }
    }
    if(owner!=PlayerEconomy.Owner)return;
    var item=queue.FirstOrDefault(p=>p.owner==owner);if(item==null)return;
    if(string.IsNullOrEmpty(item.id)){
     // A completed match that never registered cannot mint an offline result later.
     if(item.finish!=null&&DateTimeOffset.UtcNow.ToUnixTimeSeconds()-item.created>14400){queue.Remove(item);Save();return;}
     var ticket=await PlayerEconomy.Request<RewardTicket>(item.finish==null?"matches/start":"matches/lookup","POST",item.start);
     if(owner!=PlayerEconomy.Owner)return;item.id=ticket.id;Save();
     if((live==item||live==null)&&Host&&NetworkAthlete.HostPlayer&&RoomId==item.start.room)NetworkAthlete.HostPlayer.CoinMatch.Value=new FixedString64Bytes(item.id);
    }
    if(item.finish==null)return;
    item.finish.id=item.id;
    var result=await PlayerEconomy.Request<RewardTicket>("matches/finish","POST",item.finish);
    if(owner!=PlayerEconomy.Owner)return;queue.Remove(item);Save();Status=result.status=="finished"?"Match rewards saved.":"This match has no coin reward.";
    if(Host&&NetworkAthlete.HostPlayer&&NetworkAthlete.HostPlayer.CoinMatch.Value.ToString()==item.id)NetworkAthlete.HostPlayer.CoinState.Value=(byte)(result.status=="finished"?2:3);
    PlayerEconomy.RefreshSoon();
   }catch(ProfileException e){
    if(owner!=PlayerEconomy.Owner)return;
    if(e.Code is "match_cancelled" or "match_changed" or "invalid_roster" or "invalid_results" or "profile_not_found" or "match_not_found"){
     var item=queue.FirstOrDefault();if(item!=null){queue.Remove(item);if(live==item)live=null;Save();if(Host&&NetworkAthlete.HostPlayer&&NetworkAthlete.HostPlayer.CoinMatch.Value.ToString()==item.id)NetworkAthlete.HostPlayer.CoinState.Value=3;}Status="This match has no coin reward.";
    }else Status="Match rewards pending.";
   }catch(Exception){Status="Match rewards pending.";}
   finally{working=false;}
  }
 }
}
