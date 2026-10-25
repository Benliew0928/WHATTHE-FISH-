using System;
using System.Linq;
using System.Globalization;
using System.Text;
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
  public static string AccountId=>PlayerPrefs.GetString(Prefix+"player.accountId","");
  internal static string DeletionStorageKey=>Prefix+"player.deletion.pending.v1";
  internal static void ForgetDeletedIdentity(string id,string scope) {
   if(!string.IsNullOrEmpty(id))PlayerPrefs.DeleteKey(Prefix+"player.account."+id+".name");
   if(!string.IsNullOrEmpty(id)){PlayerPrefs.DeleteKey("economy.wallet."+id);PlayerPrefs.DeleteKey("economy.outbox."+id);}
   foreach(SportId sport in Enum.GetValues(typeof(SportId)))PlayerPrefs.DeleteKey("economy.venue."+scope+"."+sport);
   foreach(string slot in new[]{"footballTrail","basketballTrail"}){string key="economy.trail."+scope+"."+slot;PlayerPrefs.DeleteKey(key);PlayerPrefs.DeleteKey(key+".pending");}
   foreach(SportId sport in Enum.GetValues(typeof(SportId)))PlayerPrefs.DeleteKey(ControlLayouts.Key(scope,sport));
   PlayerPrefs.Save();
  }
  public static string NameKey=>AccountId.Length>0?Prefix+"player.account."+AccountId+".name":Prefix+"player.displayName";
  public static string IdKey=>Prefix+"player.guestId";
  public static string LegacyNameKey=>Prefix+"fishing.nickname";
  public static string GuestId=>PlayerPrefs.GetString(IdKey,"");
  public static string ProfileStorageKey=>Prefix+(AccountId.Length>0?"account:"+AccountId:"guest:"+GuestId);
  public static string SavedName=>CleanName(PlayerPrefs.GetString(NameKey,AccountId.Length>0?"":PlayerPrefs.GetString(LegacyNameKey,"")));
  public static string DisplayName=>SavedName.Length>0?SavedName:"Guest";
  public static bool HasProfile=>SavedName.Length>0&&(AccountId.Length>0||Guid.TryParseExact(GuestId,"N",out _));
  public static void ActivateAccount(string playerId,string linkedName) {
   if(string.IsNullOrEmpty(playerId)||!playerId.All(char.IsLetterOrDigit))throw new ArgumentException("Invalid game player ID.");
   string old=ProfileStorageKey;
   PlayerPrefs.SetString(Prefix+"player.accountId",playerId);
   if(!PlayerPrefs.HasKey(NameKey)&&ValidateName(linkedName,out var name,out _))PlayerPrefs.SetString(NameKey,name);
   PlayerPrefs.Save();
   ControlLayouts.IdentityChanging(old,ProfileStorageKey,!string.IsNullOrEmpty(linkedName));
   PlayerProfiles.IdentityChanging(old,ProfileStorageKey,!string.IsNullOrEmpty(linkedName));
  }
  public static void ClearAccount() {
   string old=ProfileStorageKey;
   PlayerPrefs.DeleteKey(Prefix+"player.accountId");
   PlayerPrefs.DeleteKey(Prefix+"player.displayName");PlayerPrefs.DeleteKey(IdKey);PlayerPrefs.DeleteKey(LegacyNameKey);PlayerPrefs.Save();
   PlayerProfiles.IdentityChanging(old,ProfileStorageKey,false);
  }
  static bool Allowed(char c)=>char.IsLetterOrDigit(c)||char.GetUnicodeCategory(c)==UnicodeCategory.NonSpacingMark||char.GetUnicodeCategory(c)==UnicodeCategory.SpacingCombiningMark||c==' '||c=='_'||c=='-';
  public static string CleanName(string value)=>new string((value??"").Normalize(NormalizationForm.FormC).Where(Allowed).Take(NameLimit).ToArray()).Trim();
  public static bool ValidateName(string value,out string name,out string error){
   name=(value??"").Normalize(NormalizationForm.FormC).Trim();error="";
   if(name.Length==0){error="Please enter your name.";return false;}
   if(name.Length>NameLimit){error="Use up to 16 characters.";return false;}
   if(!name.All(Allowed)||!name.Any(char.IsLetterOrDigit)){error="Use letters, numbers, spaces, - or _.";return false;}
   return true;
  }
  public static bool SaveName(string value,out string error){
   if(!ValidateName(value,out var name,out error))return false;
   if(!Guid.TryParseExact(GuestId,"N",out _))PlayerPrefs.SetString(IdKey,Guid.NewGuid().ToString("N"));
   PlayerPrefs.SetString(NameKey,name);if(AccountId.Length==0)PlayerPrefs.SetString(LegacyNameKey,name);PlayerPrefs.Save();
   foreach(var player in UnityEngine.Object.FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None))
    if(player.IsSpawned&&player.IsOwner&&player.IsPlayerObject)player.NicknameRpc(name);
   return true;
  }
 }
}
