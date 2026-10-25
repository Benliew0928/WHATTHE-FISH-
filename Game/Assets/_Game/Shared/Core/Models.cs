using System;
using UnityEngine;

namespace WhatTheFish {
 public enum SportId { Football, Basketball, Golf, Fishing }
 [Serializable] public struct SessionConfig { public SportId sport; public int capacity; public bool exploration; public static SessionConfig Default => new SessionConfig { sport=SportId.Football,capacity=10 }; }
 [Serializable] public struct CharacterAppearance { public int version,preset,skin,hair,outfit,hairstyle; public void Clamp(){ skin=Mathf.Clamp(skin,0,3);hair=Mathf.Clamp(hair,0,3);outfit=Mathf.Clamp(outfit,0,3);hairstyle=Mathf.Clamp(hairstyle,0,1);if(version<1){preset=(outfit+hairstyle*4+hair)%10;version=1;}preset=Mathf.Clamp(preset,0,9); } }
 [Serializable] public struct StadiumAppearance { public string title; public int palette,design,screen,logo; public bool flags; public static StadiumAppearance Default=>new StadiumAppearance{title="SUNNY PARK",flags=true}; public void Clamp(){ title=string.IsNullOrWhiteSpace(title)?"SUNNY PARK":title.Trim();if(title.Length>24)title=title.Substring(0,24);palette=Mathf.Clamp(palette,0,3);design=Mathf.Clamp(design,0,2);screen=Mathf.Clamp(screen,0,1);logo=Mathf.Clamp(logo,0,3); } }
 public struct PlayerCommand { public bool targetedPass;public ulong passReceiver;public uint passPossession; public bool dash,pressure,footballPass; public int fishingFish;public double fishingPressedAt;public FishingAction fishingAction;public uint fishingRound,fishingSequence; public float passBend;public bool passBegin,passCancel;public uint passPlay;public double passReleasedAt;public bool guard,guardBlock,block,jumpBlock;public uint defensePlay;public BasketballFinish finish;public double shotReleasedAt;public Vector2 move; public float heading,kickCharge,golfCharge; public bool sprint,tackle,jump,shoot,pass,steal,kick,charging,golfSwing,golfCharging,shotBegin,shotCancel; public GolfCartAction cartAction; public ulong cartOwner,golfBallOwner; public uint golfRound; }
 public interface IPlayerCommandSource { PlayerCommand ReadCommand(); }
 public interface ISportMode { SportId Sport {get;} void Enter(); void Exit(); }
 public interface IPlatformGameServices { bool Available {get;} void ReportResult(string resultId,int score); }
 public sealed class HuaweiServicesPlaceholder:IPlatformGameServices { public bool Available=>false; public void ReportResult(string resultId,int score){ Debug.Log("Huawei integration is not configured."); } }
 public static class LocalProfile {
  public static CharacterAppearance Character {get { var v=JsonUtility.FromJson<CharacterAppearance>(PlayerPrefs.GetString("character","{}"));v.Clamp();return v;} set{value.Clamp();PlayerPrefs.SetString("character",JsonUtility.ToJson(value));PlayerPrefs.Save();}}
  public static StadiumAppearance Stadium {get=>ForSport(SportId.Football);set=>SaveSport(SportId.Football,value);}
  public static StadiumAppearance Basketball {get=>ForSport(SportId.Basketball);set=>SaveSport(SportId.Basketball,value);}
  public static StadiumAppearance Golf=>ForSport(SportId.Golf);
  public static StadiumAppearance Fishing=>ForSport(SportId.Fishing);
  public static StadiumAppearance DefaultVenue(SportId sport)=>new StadiumAppearance{title=sport switch{SportId.Football=>"SUNNY PARK",SportId.Basketball=>"COURTSIDE CLUB",SportId.Golf=>"ISLAND GREENS",_=>"LAGOON ISLAND"},flags=true};
  public static StadiumAppearance ForSport(SportId sport){
   string legacy=sport==SportId.Football?"stadium":sport==SportId.Basketball?"basketball":"venue."+sport;
   string json=PlayerPrefs.GetString(CosmeticStore.VenueKey(sport),PlayerPrefs.GetString(legacy,JsonUtility.ToJson(DefaultVenue(sport))));
   return CosmeticStore.Allowed(sport,JsonUtility.FromJson<StadiumAppearance>(json));
  }
  public static void SaveSport(SportId sport,StadiumAppearance value){value=CosmeticStore.Allowed(sport,value);PlayerPrefs.SetString(CosmeticStore.VenueKey(sport),JsonUtility.ToJson(value));PlayerPrefs.Save();}
  public static readonly Color[] Teams={Hex("24B6AE"),Hex("EE8FAD"),Hex("7A84D6"),Hex("F0B956")};
  public static Color Hex(string s){ColorUtility.TryParseHtmlString("#"+s,out var c);return c;}
 }
}
