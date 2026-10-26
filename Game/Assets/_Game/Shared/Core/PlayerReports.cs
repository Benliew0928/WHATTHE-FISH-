using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;

namespace WhatTheFish {
 [Serializable,Preserve] public sealed class PlayerReportRequest {
  public int version=2;
  public string target,reason,details,context,evidence,requestId;
 }
 [Serializable,Preserve] public sealed class PlayerReportReceipt {public bool ok;public int version;public string id;}
 public static class PlayerReports {
  public static readonly string[] Reasons={"name","photo","abuse","sexual","behaviour","cheating","spam","other"};
  public static readonly string[] Labels={"Inappropriate name","Inappropriate picture","Abusive text or harassment","Sexual content","Disruptive behaviour","Cheating or exploiting","Spam or scams","Other"};
  static readonly Dictionary<ulong,PublicProfile> profiles=new();
  static readonly Dictionary<string,string> members=new();
  static string roomId="";
#if UNITY_EDITOR || DEVELOPMENT_BUILD
  public static readonly Dictionary<ulong,PublicProfile> ReviewTargets=new();
  public static Func<PlayerReportRequest,Task<PlayerReportReceipt>> ReviewSubmit;
#endif
  public static bool CanReport(PublicProfile p)=>p!=null&&!string.IsNullOrEmpty(p.id)&&p.id!=PlayerProfiles.Current.id;
  public static PublicProfile Copy(PublicProfile p)=>p==null?null:new PublicProfile{id=p.id,name=p.name,code=p.code,revision=p.revision,avatar=p.avatar,avatarVersion=p.avatarVersion,hasPhoto=p.hasPhoto};
  public static PublicProfile Resolve(ulong stableId){
#if UNITY_EDITOR || DEVELOPMENT_BUILD
   if(ReviewTargets.TryGetValue(stableId,out var review))return review;
#endif
   var app=AppRoot.Instance;var rooms=app?app.rooms:null;var chat=rooms?rooms.GetComponent<RoomChatService>():null;
   string key=rooms&&rooms.Connected&&chat?chat.RoomId:"";
   if(key!=roomId||key.Length==0){profiles.Clear();members.Clear();roomId=key;}
   if(!rooms||!rooms.Connected)return null;
   foreach(var n in RoomTeleport.Players())if(n.IsPlayerObject&&n.BotSlot.Value==0&&n.StableId==stableId){
    var p=n.PublicProfile;if(string.IsNullOrEmpty(p.id))return null;
    profiles[stableId]=Copy(p);members[p.id]=n.MemberId.Value.ToString();return profiles[stableId];
   }
   return profiles.TryGetValue(stableId,out var cached)?cached:null;
  }
  public static string Context {
   get{var app=AppRoot.Instance;var chat=app&&app.rooms?app.rooms.GetComponent<RoomChatService>():null;return (app?app.SelectedSport.ToString():"Profile")+(chat&&!string.IsNullOrEmpty(chat.RoomId)?" | "+chat.RoomId:"");}
  }
  public static string Evidence(string id){
   var app=AppRoot.Instance;var chat=app&&app.rooms?app.rooms.GetComponent<RoomChatService>():null;
   if(!chat)return "";
   if(!members.TryGetValue(id,out var member)){var n=RoomTeleport.Players().FirstOrDefault(p=>p.ProfileId.Value.ToString()==id);member=n?n.MemberId.Value.ToString():"";}
   if(string.IsNullOrEmpty(member))return "";
   // A bounded, reporter-supplied excerpt, never authoritative proof. No voice,
   // unrelated players' chat, tokens, or join codes are included.
   string result=string.Join("\n",chat.History.Where(m=>m.member==member&&!string.IsNullOrEmpty(m.text)).TakeLast(5).Select(m=>m.time+" | "+m.id+" | "+m.text));
   return result.Length<=2400?result:result.Substring(0,2400);
  }
  public static async Task<PlayerReportReceipt> Submit(PlayerReportRequest request){
#if UNITY_EDITOR || DEVELOPMENT_BUILD
   if(ReviewSubmit!=null)return await ReviewSubmit(request);
#endif
   // Refuse the old service contract instead of claiming that details were saved.
   var capability=await PlayerProfiles.Json<PlayerReportReceipt>("/v1/reports");
   if(capability.version!=2)throw new InvalidOperationException("Player reports are temporarily unavailable. Please try again later.");
   var receipt=await PlayerProfiles.Json<PlayerReportReceipt>("/v1/reports","POST",request);
   if(!receipt.ok||receipt.version!=2||string.IsNullOrEmpty(receipt.id))throw new InvalidOperationException("Your report was not confirmed. Please try again.");
   return receipt;
  }
 }
}
