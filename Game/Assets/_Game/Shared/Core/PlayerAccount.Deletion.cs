using System;
using System.Text;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Scripting;

namespace WhatTheFish {
 public static partial class PlayerAccount {
  // Only identity and progress are persisted: never provider/access/session tokens.
  static string DeletionKey=>PlayerIdentity.DeletionStorageKey;
  [Serializable,Preserve]sealed class DeletionJournal {public string id,scope;public int stage;}
  [Serializable,Preserve]sealed class DeletionReply {public int version;public bool profileDeleted;}
  [Serializable,Preserve]sealed class DeletionRequest {public string confirm="delete-game-account";}
  public static bool DeletionPending=>PlayerPrefs.HasKey(DeletionKey);
  public static bool Deleting {get;private set;}
  public static string DeletionMessage {get;private set;}="";
  static void Journal(DeletionJournal state){PlayerPrefs.SetString(DeletionKey,JsonUtility.ToJson(state));PlayerPrefs.Save();}

  public static async Task DeleteGameAccount() {
   if(Busy||RoomLocked)throw new InvalidOperationException("Leave your room before deleting your game account.");
   Busy=Deleting=true;DeletionMessage="Checking your game account…";
   DeletionJournal state=DeletionPending?JsonUtility.FromJson<DeletionJournal>(PlayerPrefs.GetString(DeletionKey)):null;
   try {
    if(state!=null&&state.stage==2){FinishLocalDeletion(state);return;}
    await Initialize();var auth=AuthenticationService.Instance;
    // A purely offline guest has no cloud identity to create or delete.
    if(state==null&&!auth.IsSignedIn&&!auth.SessionTokenExists&&PlayerIdentity.AccountId.Length==0){
     FinishLocalDeletion(new DeletionJournal{scope=PlayerIdentity.ProfileStorageKey,id="",stage=2});return;
    }
    await EnsureSession();string owner=auth.PlayerId;
    if(state!=null&&state.id!=owner)throw new InvalidOperationException("The saved deletion belongs to another account. No data was deleted.");
    if(state==null){
     var info=await auth.GetPlayerInfoAsync();
     if(!string.IsNullOrEmpty(info.GetCustomId()))await ConfirmDeletionIdentity(AccountProvider.Huawei,owner);
     else if(!string.IsNullOrEmpty(info.GetGoogleId()))await ConfirmDeletionIdentity(AccountProvider.Google,owner);
     else if(info.Identities!=null&&info.Identities.Count>0)throw new InvalidOperationException("This account's sign-in provider is not supported for deletion in this build.");
     if(auth.PlayerId!=owner)throw new InvalidOperationException("Sign in with the account you are deleting.");
     // Fail closed on older/unavailable backends, before recording or deleting data.
     var capability=await DeletionHttp("GET");
     if(capability.version!=1)throw new InvalidOperationException("Account deletion is not enabled on this server yet. Nothing was deleted.");
     state=new DeletionJournal{id=owner,scope=PlayerIdentity.ProfileStorageKey,stage=0};Journal(state);
    }
    foreach(var voice in UnityEngine.Object.FindObjectsByType<RoomVoiceService>(FindObjectsSortMode.None))voice.Leave();
    if(state.stage==0){
     DeletionMessage="Removing your profile, photo and social data…";
     var result=await DeletionHttp("DELETE");
     if(result.version!=1||!result.profileDeleted)throw new InvalidOperationException("Cloud deletion was not confirmed. Please retry.");
     state.stage=1;Journal(state);
    }
    DeletionMessage="Removing your game sign-in account…";
    await auth.DeleteAccountAsync();
    state.stage=2;Journal(state);
    FinishLocalDeletion(state);
   }catch(OperationCanceledException){
    throw new InvalidOperationException("Verification cancelled. No new deletion was started.");
   }catch(Exception e){
    if(DeletionPending)throw new InvalidOperationException("Deletion is not yet confirmed complete. Your account stays blocked from online play. Retry here with a connection; if sign-in cannot be restored, keep this installation for support. "+(e is InvalidOperationException?e.Message:""));
    throw;
   }finally{Busy=Deleting=false;}
  }

  static async Task ConfirmDeletionIdentity(AccountProvider provider,string expected) {
   string unavailable=AndroidAccountSignIn.Unavailable(provider);if(unavailable.Length>0)throw new InvalidOperationException(unavailable);
   DeletionMessage="Verify the same account on your phone…";
   string token=await AndroidAccountSignIn.SignIn(provider);
   var auth=AuthenticationService.Instance;string oldAccess=auth.AccessToken,oldSession=auth.SessionToken;
   try {
    if(provider==AccountProvider.Huawei){
     var result=await ExchangeHuawei(token,null,false);
     if(result.playerId!=expected)throw new InvalidOperationException("That Huawei ID belongs to a different game account. Nothing was deleted.");
     auth.SignOut();auth.ProcessAuthenticationTokens(result.accessToken,result.sessionToken);
    }else{
     auth.SignOut();await auth.SignInWithGoogleAsync(token,new SignInOptions{CreateAccount=false});
     if(auth.PlayerId!=expected)throw new InvalidOperationException("That Google account belongs to a different game account. Nothing was deleted.");
    }
   }catch{
    auth.SignOut();auth.ProcessAuthenticationTokens(oldAccess,oldSession);throw;
   }finally{token=oldAccess=oldSession=null;}
  }

  static async Task<DeletionReply> DeletionHttp(string method) {
   var auth=AuthenticationService.Instance;string owner=auth.PlayerId;
   using var request=new UnityWebRequest(PlayerProfiles.Origin+"/v1/account-deletion",method);
   request.downloadHandler=new DownloadHandlerBuffer();request.timeout=25;request.redirectLimit=0;
   request.SetRequestHeader("Authorization","Bearer "+auth.AccessToken);
   request.SetRequestHeader("User-Agent","WhatTheFish/AccountDeletion");
   if(method=="DELETE"){
    request.uploadHandler=new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonUtility.ToJson(new DeletionRequest())));
    request.SetRequestHeader("Content-Type","application/json");
   }
   var operation=request.SendWebRequest();while(!operation.isDone)await Task.Yield();
   if(owner!=auth.PlayerId)throw new InvalidOperationException("Account changed during deletion. Please reopen this screen.");
   if(request.result!=UnityWebRequest.Result.Success)throw new InvalidOperationException(request.responseCode==404?"Account deletion is not enabled on this server yet.":"The deletion service could not confirm the request. Check your connection and retry.");
   if(request.downloadHandler.data.Length>4096)throw new InvalidOperationException("Unexpected deletion response.");
   return JsonUtility.FromJson<DeletionReply>(request.downloadHandler.text)??new DeletionReply();
  }

  static void FinishLocalDeletion(DeletionJournal state) {
   DeletionMessage="Clearing this device's profile…";
   PlayerProfiles.ForgetDeletedIdentity(state.scope,state.id);
   PlayerIdentity.ForgetDeletedIdentity(state.id,state.scope);
   if(UnityServices.State==ServicesInitializationState.Initialized)AuthenticationService.Instance.SignOut(true);
   AndroidAccountSignIn.SignOutNative();PlayerIdentity.ClearAccount();
   PlayerPrefs.DeleteKey(DeletionKey);PlayerPrefs.Save();
   DeletionMessage="Game account deleted. Your Huawei ID and Google account have not been deleted.";
  }
 }
}
