using UnityEngine;

namespace WhatTheFish {
 // Shared by the HUD, radar and basketball rules. Join IDs are stable on every
 // peer, so alternating basketball squads need no extra network messages.
 public static class SportsPossession {
  public static FootballTeam TeamOf(Athlete actor,SportId sport){
   if(!actor)return FootballTeam.None;
   if(sport==SportId.Football)return FootballMatch.Instance?FootballMatch.Instance.TeamOf(actor):FootballTeam.None;
   if(sport!=SportId.Basketball)return FootballTeam.None;
   var net=actor.GetComponent<NetworkAthlete>();
   return net&&net.IsSpawned?BasketballTeam(net.OwnerClientId):actor.BasketballPracticeTeam;
  }
  public static FootballTeam BasketballTeam(ulong player)=>player%2==0?FootballTeam.A:FootballTeam.B;
  public static bool SameTeam(Athlete a,Athlete b,SportId sport){
   if(!a||!b)return false;if(a==b)return true;
   var team=TeamOf(a,sport);return team!=FootballTeam.None&&team==TeamOf(b,sport);
  }
  public static Athlete Holder(SportId sport)=>sport==SportId.Football?(FootballBall.Instance?FootballBall.Instance.CurrentController:null):sport==SportId.Basketball&&BasketballBall.Active?BasketballBall.Active.Holder:null;
  public static bool Attacking(Athlete local,SportId sport)=>SameTeam(local,Holder(sport),sport);
 }
}
