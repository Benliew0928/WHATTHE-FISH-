using System;
using System.Linq;
using UnityEngine;

namespace WhatTheFish {
 // A device-local guest profile. This ID is not a credential or a UGS player ID.
 public static class PlayerIdentity {
  public const int NameLimit=16;
  static string Prefix {
   get {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-guestProfile");
    if(i>=0&&i+1<args.Length)return "identity.review."+new string(args[i+1].Where(char.IsLetterOrDigit).Take(64).ToArray())+".";
#endif
    return "";
   }
  }
  public static string NameKey=>Prefix+"player.displayName";
  public static string IdKey=>Prefix+"player.guestId";
  public static string LegacyNameKey=>Prefix+"fishing.nickname";
  public static string GuestId=>PlayerPrefs.GetString(IdKey,"");
  public static string SavedName=>CleanName(PlayerPrefs.GetString(NameKey,PlayerPrefs.GetString(LegacyNameKey,"")));
  public static string DisplayName=>SavedName.Length>0?SavedName:"Guest";
  public static bool HasProfile=>SavedName.Length>0&&Guid.TryParseExact(GuestId,"N",out _);
  static bool Allowed(char c)=>char.IsLetterOrDigit(c)||c==' '||c=='_'||c=='-';
  public static string CleanName(string value)=>new string((value??"").Where(Allowed).Take(NameLimit).ToArray()).Trim();
  public static bool ValidateName(string value,out string name,out string error){
   name=(value??"").Trim();error="";
   if(name.Length==0){error="Please enter your name.";return false;}
   if(name.Length>NameLimit){error="Use up to 16 characters.";return false;}
   if(!name.All(Allowed)||!name.Any(char.IsLetterOrDigit)){error="Use letters, numbers, spaces, - or _.";return false;}
   return true;
  }
  public static bool SaveName(string value,out string error){
   if(!ValidateName(value,out var name,out error))return false;
   if(!Guid.TryParseExact(GuestId,"N",out _))PlayerPrefs.SetString(IdKey,Guid.NewGuid().ToString("N"));
   PlayerPrefs.SetString(NameKey,name);PlayerPrefs.SetString(LegacyNameKey,name);PlayerPrefs.Save();
   foreach(var player in UnityEngine.Object.FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None))
    if(player.IsSpawned&&player.IsOwner&&player.IsPlayerObject)player.NicknameRpc(name);
   return true;
  }
 }
}
