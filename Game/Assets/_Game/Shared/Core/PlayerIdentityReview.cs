#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

namespace WhatTheFish {
 public sealed class PlayerIdentityReview:MonoBehaviour {
  string folder;bool failed;string role;
  static string Arg(string key){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,key);return i>=0&&i+1<args.Length?args[i+1]:"";}
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]static void Seed(){if(Arg("-playerIdentityReview").Length>0&&Arg("-guestProfile").Length>0)PlayerIdentity.SaveName(Arg("-guestName"),out _);}
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){if(Arg("-playerIdentityReview").Length>0){var review=new GameObject("Player identity review").AddComponent<PlayerIdentityReview>();review.folder=Arg("-playerIdentityReview");review.role=Arg("-guestName");}}
  void Check(bool ok,string label){failed|=!ok;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+label+"\n");}
  IEnumerator Until(Func<bool> condition,string label){double end=Time.realtimeSinceStartupAsDouble+90;while(!condition()&&Time.realtimeSinceStartupAsDouble<end)yield return null;Check(condition(),label);}
  static bool Names(string host,string guest){var players=RoomTeleport.Players().Where(n=>n.IsPlayerObject).ToArray();return players.Length==2&&players.Any(n=>n.OwnerClientId==0&&n.Nickname.Value.ToString()==host)&&players.Any(n=>n.OwnerClientId!=0&&n.Nickname.Value.ToString()==guest);}
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");
   yield return Until(()=>AppRoot.Instance&&AppRoot.Instance.rooms.Connected&&Names("Host Name","Guest Name"),"distinct saved host and guest names replicated");
   if(failed){Finish();yield break;}
   string id=PlayerIdentity.GuestId;Check(PlayerIdentity.HasProfile,"local guest has a stable ID");
   yield return new WaitForSecondsRealtime(2);
   Check(PlayerIdentity.SaveName(role=="Host Name"?"Host Renamed":"Guest Renamed",out _),"display name can be changed while in a room");
   yield return Until(()=>Names("Host Renamed","Guest Renamed"),"renames replicated to both peers");
   Check(PlayerIdentity.GuestId==id,"rename preserves local identity");
   Check(!PlayerIdentity.SaveName("<b>Bad</b>",out _)&&PlayerIdentity.GuestId==id&&PlayerIdentity.DisplayName.EndsWith("Renamed"),"invalid edit preserves saved identity");
   File.WriteAllText(Path.Combine(folder,"roster.txt"),string.Join("\n",RoomTeleport.Players().OrderBy(n=>n.OwnerClientId).Select(n=>n.OwnerClientId+":"+n.Nickname.Value)));
   yield return new WaitForSecondsRealtime(3);Finish();
  }
  void Finish(){File.AppendAllText(Path.Combine(folder,"results.txt"),"PLAYER_IDENTITY_COMPLETE success="+!failed+"\n");Application.Quit(failed?1:0);}
 }
}
#endif
