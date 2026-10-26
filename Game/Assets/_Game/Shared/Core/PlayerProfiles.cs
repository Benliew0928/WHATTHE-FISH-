using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Scripting;

namespace WhatTheFish {
 [Serializable,Preserve] public sealed class PublicProfile {
  public string id="",code="",name="",relationship="",match="";
  public int revision,avatarVersion,avatar,hasPhoto;
 }
 [Serializable,Preserve] public sealed class ProfileResults {public PublicProfile[] players=Array.Empty<PublicProfile>();public bool more;public int offset;}
 [Serializable,Preserve] public sealed class FriendResults {
  public PublicProfile[] friends=Array.Empty<PublicProfile>(),incoming=Array.Empty<PublicProfile>(),outgoing=Array.Empty<PublicProfile>();
 }
 public sealed class ProfileException:Exception {
  public long Status {get;} public string Code {get;}
  public ProfileException(long status,string code):base(code){Status=status;Code=code;}
 }
 public static class PlayerProfiles {
  public const string Origin="https://whatthefish-huawei-auth.benliew28262826.workers.dev";
  public const int PhotoBytes=128*128*3;
  [Serializable,Preserve] sealed class Saved {public PublicProfile profile=new();public string photo="";public bool dirty;}
  [Serializable,Preserve] sealed class SaveRequest {public string name,photo;public int revision,avatar;}
  [Serializable,Preserve] sealed class ActionRequest {public string target,action,reason;}
  [Serializable,Preserve] sealed class ErrorResponse {public string code;}
  [Serializable,Preserve] public sealed class ProofRequest {public string room,challenge,proof;}
  [Serializable,Preserve] public sealed class ProofResponse {public string proof;}
  static Saved saved;static string scope="",loadedId="";static Task<PublicProfile> loading;
  static readonly Dictionary<string,Texture2D> photos=new();
  static readonly Queue<string> photoOrder=new();
  static readonly Dictionary<string,Task<Texture2D>> fetching=new();
  public const int AvatarCount=10;
  // A square face crop in the original 2:3 illustration keeps the hair silhouettes visible.
  public static readonly Rect AvatarCrop=new Rect(.12f,.48f,.76f,.76f*2f/3f);
  // The twin-puff illustration leans right; keep both puffs inside its square portrait.
  public static Rect AvatarCropFor(int index)=>index==7?new Rect(.16f,.46f,.8f,.8f*2f/3f):AvatarCrop;
  static readonly Texture2D[] defaults=new Texture2D[AvatarCount];
  public static event Action Changed;
  static readonly HashSet<string> incomingRequests=new();
  static string friendsScope="";static int friendsVersion;static float friendsPollAt;
  static Task<FriendResults> friendsLoading;
  public static int PendingRequestCount=>friendsScope==Scope?incomingRequests.Count:0;
  public static async void PollFriendRequests(){
   if(friendsLoading!=null||Time.unscaledTime<friendsPollAt||UnityServices.State!=ServicesInitializationState.Initialized||!AuthenticationService.Instance.IsSignedIn)return;
   friendsPollAt=Time.unscaledTime+25;
   try{await Friends();}catch(Exception){ /* Keep the last known count on transient failures. */ }
  }
  static string Scope=>PlayerIdentity.ProfileStorageKey;
  static string FileFor(string key) {
   using var sha=SHA256.Create();string hash=BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(key))).Replace("-","");
   return Path.Combine(Application.persistentDataPath,"profiles",hash+".json");
  }
  static Saved Data {
   get {
    if(saved!=null&&scope==Scope)return saved;
    scope=Scope;loadedId="";saved=new Saved();
    try{string file=FileFor(scope);if(File.Exists(file))saved=JsonUtility.FromJson<Saved>(File.ReadAllText(file))??saved;}catch(Exception){ }
    saved.profile??=new PublicProfile();if(saved.photo==null||saved.photo.Length!=65536)saved.photo="";
    if(string.IsNullOrEmpty(saved.profile.name))saved.profile.name=PlayerIdentity.DisplayName;
    return saved;
   }
  }
  public static PublicProfile Current=>Data.profile;
  public static string CurrentPhoto=>Data.photo;
  public static bool PendingSync=>Data.dirty;
  public static string Status=>Data.dirty?"Saved on this device · waiting to sync":string.IsNullOrEmpty(Current.code)?"Device profile":"Saved to cloud";
  public static void SaveLocalName(string name){
   if(PlayerAccount.DeletionPending||PlayerAccount.Deleting)return;
   if(!PlayerIdentity.ValidateName(name,out name,out _))return;
   Data.profile.name=name;Data.dirty=true;Persist();Changed?.Invoke();
  }
  static void Persist() {
   var file=FileFor(Scope);Directory.CreateDirectory(Path.GetDirectoryName(file));
   string temporary=file+".tmp";File.WriteAllText(temporary,JsonUtility.ToJson(Data));File.Copy(temporary,file,true);File.Delete(temporary);
  }
  public static void IdentityChanging(string oldKey,string newKey,bool link) {
   if(link&&oldKey!=newKey&&!File.Exists(FileFor(newKey))) {
    string oldFile=FileFor(oldKey);if(File.Exists(oldFile)){Directory.CreateDirectory(Path.GetDirectoryName(FileFor(newKey)));File.Copy(oldFile,FileFor(newKey));}
   }
   saved=null;scope="";loadedId="";loading=null;friendsVersion++;friendsScope="";incomingRequests.Clear();friendsLoading=null;friendsPollAt=0;Changed?.Invoke();
  }
  internal static void ForgetDeletedIdentity(string key,string id) {
   if(string.IsNullOrEmpty(key))throw new InvalidOperationException("Missing deletion profile scope.");
   string file=FileFor(key);if(File.Exists(file))File.Delete(file);if(File.Exists(file+".tmp"))File.Delete(file+".tmp");
   foreach(var texture in photos.Values)if(texture)UnityEngine.Object.Destroy(texture);
   photos.Clear();photoOrder.Clear();fetching.Clear();
   IdentityChanging(key,"",false);
  }
  public static string Explain(Exception e) {
   if(e is OperationCanceledException)return "Cancelled. Your profile is unchanged.";
   if(e is ProfileException p)return p.Code switch {
    "profile_changed"=>"Your profile changed on another device. Reload it, then save your edits again.",
    "invalid_name"=>"Use 1–16 characters: letters, numbers, spaces, - or _.",
    "offensive_name"=>TextSafety.NameError,
    "report_limit"=>"Please wait before reporting again. Reports are limited to prevent spam.",
    "report_details_required"=>"Please describe what happened when choosing Other.",
    "invalid_report"=>"Check your report reason and details, then try again.",
    "report_changed"=>"This report changed during submission. Close it and start a new report.",
    "request_cooldown"=>"Please wait 24 hours before sending another request to this player.",
    "friend_limit"=>"One of these players has reached the 150 friends and pending requests limit.",
    "rate_limited"=>"Too many changes at once. Please wait a minute and try again.",
    "profile_not_found"=>"This profile is unavailable.",
    "player_unavailable"=>"This player is unavailable.",
    "invalid_session"=>"Your session expired. Sign in again.",
    _=>"The profile service is unavailable. Check your connection and try again."
   };
   return e is InvalidOperationException?e.Message:"Could not save right now. Please try again.";
  }
  public static async Task<byte[]> Request(string path,string method="GET",object body=null) {
   await PlayerAccount.EnsureOnline();
   string owner=AuthenticationService.Instance.PlayerId;
   using var request=new UnityWebRequest(Origin+path,method);
   request.downloadHandler=new DownloadHandlerBuffer();request.timeout=18;request.redirectLimit=0;
   request.SetRequestHeader("Authorization","Bearer "+AuthenticationService.Instance.AccessToken);
   request.SetRequestHeader("User-Agent","WhatTheFish/Profiles");
   if(body!=null){request.uploadHandler=new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonUtility.ToJson(body)));request.SetRequestHeader("Content-Type","application/json");}
   var operation=request.SendWebRequest();while(!operation.isDone)await Task.Yield();
   if(owner!=AuthenticationService.Instance.PlayerId)throw new OperationCanceledException();
   if(request.result!=UnityWebRequest.Result.Success){
    string code="network_unavailable";try{code=JsonUtility.FromJson<ErrorResponse>(request.downloadHandler.text)?.code??code;}catch(Exception){ }
    throw new ProfileException(request.responseCode,code);
   }
   if(request.downloadHandler.data.Length>131072)throw new ProfileException(502,"invalid_response");
   return request.downloadHandler.data;
  }
  public static async Task<T> Json<T>(string path,string method="GET",object body=null)=>JsonUtility.FromJson<T>(Encoding.UTF8.GetString(await Request(path,method,body)));
  public static async Task<PublicProfile> EnsureCloud(bool reload=false) {
   await PlayerAccount.EnsureOnline();string id=AuthenticationService.Instance.PlayerId;
   if(!reload&&loadedId==id&&!Data.dirty)return Current;
   if(loading!=null)return await loading;
   loading=Load(id,reload);try{return await loading;}finally{loading=null;}
  }
  static async Task<PublicProfile> Load(string id,bool discardPending) {
   string key=Scope;var local=Data;PublicProfile remote=null;
   try{remote=await Json<PublicProfile>("/v1/me");}
   catch(ProfileException e)when(e.Status==404){ }
   if(key!=Scope)throw new OperationCanceledException();
   if(remote==null) {
    remote=await SaveRemote(new SaveRequest{name=PlayerIdentity.DisplayName,avatar=local.profile.avatar,photo=local.photo,revision=0});
   }else if(local.dirty&&!discardPending&&(local.profile.id==id||string.IsNullOrEmpty(local.profile.id))) {
    remote=await SaveRemote(new SaveRequest{name=local.profile.name,avatar=local.profile.avatar,photo=local.photo,revision=local.profile.revision>0?local.profile.revision:remote.revision});
   }
   if(key!=Scope)throw new OperationCanceledException();
   string photo=remote.hasPhoto>0?Convert.ToBase64String(await Request("/v1/photos/"+id+"/"+remote.avatarVersion)):"";
   saved=new Saved{profile=remote,photo=photo};loadedId=id;Persist();
   PlayerIdentity.SaveName(remote.name,out _);Changed?.Invoke();return remote;
  }
  static async Task<PublicProfile> SaveRemote(SaveRequest request){
   // Ask the server before replacing a pending offline avatar. The wallet cache
   // may not have loaded yet during account recovery.
   try{return await Json<PublicProfile>("/v1/me","PUT",request);}
   catch(ProfileException e)when(e.Code=="cosmetic_not_owned"){request.avatar=0;return await Json<PublicProfile>("/v1/me","PUT",request);}
  }
  public static async Task<string> Save(string name,int avatar,string photo) {
   if(!CosmeticStore.Owned("profileAvatar",avatar))throw new InvalidOperationException("Unlock this cosmetic before using it.");
   if(!PlayerIdentity.ValidateName(name,out name,out var error))throw new InvalidOperationException(error);
   if(!string.IsNullOrEmpty(photo)&&(photo.Length!=65536||Convert.FromBase64String(photo).Length!=PhotoBytes))throw new InvalidOperationException("Choose a valid profile picture.");
   if(string.IsNullOrEmpty(PlayerIdentity.GuestId))PlayerIdentity.SaveName(PlayerIdentity.DisplayName,out _);
   string key=Scope;
   try {
    var current=await EnsureCloud();
    var result=await Json<PublicProfile>("/v1/me","PUT",new SaveRequest{name=name,avatar=avatar,photo=photo??"",revision=current.revision});
    if(key!=Scope)throw new OperationCanceledException();
    saved=new Saved{profile=result,photo=photo??""};Persist();PlayerIdentity.SaveName(name,out _);Changed?.Invoke();return "Profile saved. Other players can see your name and picture.";
   }catch(Exception e)when(e is ProfileException p&&(p.Status==0||p.Status==503)||e is RequestFailedException&&!(e is AuthenticationException)) {
    if(key!=Scope)throw new OperationCanceledException();
    Data.profile.name=name;Data.profile.avatar=avatar;Data.profile.hasPhoto=string.IsNullOrEmpty(photo)?0:1;Data.photo=photo??"";Data.dirty=true;
    PlayerIdentity.SaveName(name,out _);Persist();Changed?.Invoke();return "Saved on this device. Open your profile online to sync.";
   }
  }
  public static Task<PublicProfile> Get(string id)=>Json<PublicProfile>("/v1/players/"+Uri.EscapeDataString(id));
  public static Task<ProfileResults> Search(string query,int offset=0)=>Json<ProfileResults>("/v1/search?q="+Uri.EscapeDataString(query)+"&offset="+offset);
  public static Task<FriendResults> Friends()=>friendsLoading??=LoadFriends(Scope,friendsVersion);
  static async Task<FriendResults> LoadFriends(string owner,int version){
   try{
    var result=await Json<FriendResults>("/v1/friends");
    if(owner==Scope&&version==friendsVersion){friendsScope=owner;incomingRequests.Clear();foreach(var p in result.incoming)incomingRequests.Add(p.id);friendsPollAt=Time.unscaledTime+25;}
    return result;
   }finally{if(version==friendsVersion)friendsLoading=null;}
  }
  static void FriendshipChanged(string target,string owner){
   if(owner!=Scope)return;friendsVersion++;friendsLoading=null;friendsPollAt=0;incomingRequests.Remove(target);
  }
  public static async Task<PublicProfile> Friend(string target,string action){string owner=Scope;var result=await Json<PublicProfile>("/v1/friends","POST",new ActionRequest{target=target,action=action});FriendshipChanged(target,owner);return result;}
  public static async Task<byte[]> Block(string target){string owner=Scope;var result=await Request("/v1/blocks","POST",new ActionRequest{target=target});FriendshipChanged(target,owner);return result;}
  public static Task<byte[]> Report(string target,string reason)=>Request("/v1/reports","POST",new ActionRequest{target=target,reason=reason});
  public static Task<ProfileResults> Blocks()=>Json<ProfileResults>("/v1/blocks");
  public static Task<byte[]> Unblock(string target)=>Request("/v1/blocks","DELETE",new ActionRequest{target=target});
  public static Texture2D DecodePhoto(string base64)=>DecodePhoto(Convert.FromBase64String(base64));
  public static Texture2D DecodePhoto(byte[] data) {
   if(data==null||data.Length!=PhotoBytes)throw new InvalidOperationException("Invalid profile picture.");
   var colors=new Color32[128*128];for(int y=0;y<128;y++)for(int x=0;x<128;x++){int i=(y*128+x)*3;colors[(127-y)*128+x]=new Color32(data[i],data[i+1],data[i+2],255);}
   var texture=new Texture2D(128,128,TextureFormat.RGB24,false){name="Profile picture",filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};
   texture.SetPixels32(colors);texture.Apply(false,true);return texture;
  }
  public static Texture2D DefaultAvatar(int index) {
   index=Mathf.Clamp(index,0,AvatarCount-1);if(defaults[index])return defaults[index];
   return defaults[index]=Resources.Load<Texture2D>("PlayerPortraits/Character-"+(index+1).ToString("00"));
  }
  public static async Task<Texture2D> Picture(PublicProfile p) {
   if(p==null||p.hasPhoto==0)return DefaultAvatar(p?.avatar??0);
   string key=p.id+":"+p.avatarVersion;
   if(photos.TryGetValue(key,out var cached)&&cached)return cached;
   if(fetching.TryGetValue(key,out var existing))return await existing;
   var task=FetchPicture(p,key);fetching[key]=task;try{return await task;}finally{fetching.Remove(key);}
  }
  static async Task<Texture2D> FetchPicture(PublicProfile p,string key) {
   try{
    var t=DecodePhoto(await Request("/v1/photos/"+Uri.EscapeDataString(p.id)+"/"+p.avatarVersion));photos[key]=t;photoOrder.Enqueue(key);
    while(photoOrder.Count>64){string old=photoOrder.Dequeue();if(photos.Remove(old,out var texture))UnityEngine.Object.Destroy(texture);}
    return t;
   }catch(Exception){return DefaultAvatar(p.avatar);}
  }
 }
}
