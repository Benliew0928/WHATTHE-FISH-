using System;
using UnityEngine;

namespace WhatTheFish {
 public enum SportId { Football, Basketball, Golf, Fishing }
 [Serializable] public struct SessionConfig { public SportId sport; public int capacity; public bool exploration; public static SessionConfig Default => new SessionConfig { sport=SportId.Football,capacity=10 }; }
 [Serializable] public struct CharacterAppearance { public int skin,hair,outfit,hairstyle; public void Clamp(){ skin=Mathf.Clamp(skin,0,3);hair=Mathf.Clamp(hair,0,3);outfit=Mathf.Clamp(outfit,0,3);hairstyle=Mathf.Clamp(hairstyle,0,1); } }
 [Serializable] public struct StadiumAppearance { public string title; public int palette,design,screen,logo; public bool flags; public static StadiumAppearance Default=>new StadiumAppearance{title="SUNNY PARK",flags=true}; public void Clamp(){ title=string.IsNullOrWhiteSpace(title)?"SUNNY PARK":title.Trim();if(title.Length>24)title=title.Substring(0,24);palette=Mathf.Clamp(palette,0,3);design=Mathf.Clamp(design,0,2);screen=Mathf.Clamp(screen,0,1);logo=Mathf.Clamp(logo,0,3); } }
 public struct PlayerCommand { public Vector2 move; public float heading; public bool sprint,tackle; }
 public interface IPlayerCommandSource { PlayerCommand ReadCommand(); }
 public interface ISportMode { SportId Sport {get;} void Enter(); void Exit(); }
 public interface IPlatformGameServices { bool Available {get;} void ReportResult(string resultId,int score); }
 public sealed class HuaweiServicesPlaceholder:IPlatformGameServices { public bool Available=>false; public void ReportResult(string resultId,int score){ Debug.Log("Huawei integration is not configured."); } }
 public static class LocalProfile {
  public static CharacterAppearance Character {get { var v=JsonUtility.FromJson<CharacterAppearance>(PlayerPrefs.GetString("character","{}"));v.Clamp();return v;} set{value.Clamp();PlayerPrefs.SetString("character",JsonUtility.ToJson(value));PlayerPrefs.Save();}}
  public static StadiumAppearance Stadium {get{var v=JsonUtility.FromJson<StadiumAppearance>(PlayerPrefs.GetString("stadium",JsonUtility.ToJson(StadiumAppearance.Default)));v.Clamp();return v;}set{value.Clamp();PlayerPrefs.SetString("stadium",JsonUtility.ToJson(value));PlayerPrefs.Save();}}
  public static StadiumAppearance Basketball {get{var v=JsonUtility.FromJson<StadiumAppearance>(PlayerPrefs.GetString("basketball",JsonUtility.ToJson(new StadiumAppearance{title="COURTSIDE CLUB",flags=true})));v.Clamp();return v;}set{value.Clamp();PlayerPrefs.SetString("basketball",JsonUtility.ToJson(value));PlayerPrefs.Save();}}
  public static StadiumAppearance Golf=>new StadiumAppearance{title="ISLAND GREENS"};
  public static StadiumAppearance Fishing=>new StadiumAppearance{title="LAGOON ISLAND"};
  public static StadiumAppearance ForSport(SportId sport)=>sport switch{SportId.Football=>Stadium,SportId.Basketball=>Basketball,SportId.Golf=>Golf,SportId.Fishing=>Fishing,_=>throw new ArgumentOutOfRangeException(nameof(sport))};
  public static void SaveSport(SportId sport,StadiumAppearance value){switch(sport){case SportId.Football:Stadium=value;break;case SportId.Basketball:Basketball=value;break;case SportId.Golf:case SportId.Fishing:break;default:throw new ArgumentOutOfRangeException(nameof(sport));}}
  public static readonly Color[] Teams={Hex("24B6AE"),Hex("EE8FAD"),Hex("7A84D6"),Hex("F0B956")};
  public static Color Hex(string s){ColorUtility.TryParseHtmlString("#"+s,out var c);return c;}
 }
}
