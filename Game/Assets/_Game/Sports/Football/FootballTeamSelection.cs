using System;
using System.Collections.Generic;
using System.Linq;
namespace WhatTheFish {
 // Authority-owned roster and capacity rules. No UI or networking dependency.
 public sealed class FootballTeamSelection {
  readonly Dictionary<ulong,FootballTeam> choices=new();
  public int Capacity {get;private set;}
  public bool Locked {get;private set;}
  public int Count(FootballTeam team)=>choices.Values.Count(value=>value==team);
  public FootballTeam TeamOf(ulong player)=>choices.TryGetValue(player,out var team)?team:FootballTeam.None;
  public bool Contains(ulong player)=>choices.ContainsKey(player);
  public void Clear(){choices.Clear();Capacity=0;Locked=false;}
  public bool Begin(IEnumerable<ulong> players,bool singlePlayerTest=false){
   var roster=players.Distinct().ToArray();
   if(roster.Length>10)return false;
   if(roster.Length<2||roster.Length%2!=0){if(!singlePlayerTest||roster.Length!=1)return false;}
   Clear();Capacity=Math.Max(1,roster.Length/2);foreach(var player in roster)choices.Add(player,FootballTeam.None);return true;
  }
  public bool Choose(ulong player,FootballTeam team){
   if(Locked||!Contains(player)||(team!=FootballTeam.A&&team!=FootballTeam.B))return false;
   if(choices[player]==team)return true;
   if(Count(team)>=Capacity)return false;
   choices[player]=team;return true;
  }
  public void Complete(Random random){
   if(Locked)return;
   // Shuffle players and available seats separately; selected players keep their seat.
   var players=choices.Where(p=>p.Value==FootballTeam.None).Select(p=>p.Key).ToArray();
   var seats=new List<FootballTeam>();
   for(int i=Count(FootballTeam.A);i<Capacity;i++)seats.Add(FootballTeam.A);
   for(int i=Count(FootballTeam.B);i<Capacity;i++)seats.Add(FootballTeam.B);
   for(int i=players.Length-1;i>0;i--){int j=random.Next(i+1);(players[i],players[j])=(players[j],players[i]);}
   for(int i=seats.Count-1;i>0;i--){int j=random.Next(i+1);(seats[i],seats[j])=(seats[j],seats[i]);}
   for(int i=0;i<players.Length;i++)choices[players[i]]=seats[i];Locked=true;
  }
  // Called once per round by the authority, independently for each locked team.
  // Array index is the fixed formation slot; only player order is randomized.
  public ulong[] AllocateKickoffSlots(FootballTeam team,Random random){
   if(!Locked||(team!=FootballTeam.A&&team!=FootballTeam.B))throw new InvalidOperationException("Kickoff requires locked teams.");
   var players=choices.Where(p=>p.Value==team).Select(p=>p.Key).OrderBy(id=>id).ToArray();
   for(int i=players.Length-1;i>0;i--){int j=random.Next(i+1);(players[i],players[j])=(players[j],players[i]);}
   return players;
  }
 }
}
