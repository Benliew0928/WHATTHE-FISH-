using System;
using System.Text;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Scripting;

namespace WhatTheFish {
 // The UGS player ID is the game identity. Provider IDs never enter room connection data.
 public static class PlayerAccount {
  static Task initialization;
  public static bool Busy {get;private set;}
  public static bool RoomLocked=>AppRoot.Instance&&AppRoot.Instance.rooms&&(AppRoot.Instance.rooms.Connected||AppRoot.Instance.rooms.busy);
  public static string Status=>PlayerIdentity.AccountId.Length>0?"Account profile on this device":"Guest on this device";
  [Serializable,Preserve]sealed class HuaweiRequest {public string idToken,accessToken;public bool link;}
  [Serializable,Preserve]sealed class HuaweiResponse {public string accessToken,sessionToken,playerId,code;}
  public static async Task Initialize() {
   if(UnityServices.State==ServicesInitializationState.Initialized)return;
   if(string.IsNullOrEmpty(Application.cloudProjectId))throw new InvalidOperationException("Online accounts need the game's Unity project configuration.");
   if(initialization==null)initialization=InitializeServices();
   try{await initialization;}catch{initialization=null;throw;}
  }
  static async Task InitializeServices() {
   var options=new InitializationOptions().SetEnvironmentName("production");
#if UNITY_EDITOR || DEVELOPMENT_BUILD
   var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-authProfile");
   if(i>=0&&i+1<args.Length)options.SetProfile(args[i+1]);
   else {
    // Existing device-profile reviews must never restore the user's normal cloud session.
    int guest=Array.IndexOf(args,"-guestProfile");
    if(guest>=0&&guest+1<args.Length){
     using var sha=System.Security.Cryptography.SHA256.Create();
     options.SetProfile("review"+BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(args[guest+1]))).Replace("-","").Substring(0,20).ToLowerInvariant());
    }
   }
#endif
   await UnityServices.InitializeAsync(options);
  }
  public static async Task EnsureOnline() {
   if(Busy)throw new InvalidOperationException("Finish signing in before joining a room.");
   await EnsureSession();
  }
  static async Task EnsureSession() {
   await Initialize();var auth=AuthenticationService.Instance;
   if(auth.IsAuthorized)return;
   if(auth.IsSignedIn)auth.SignOut();
   // SignInAnonymously restores the cached session, including linked accounts.
   if(PlayerIdentity.AccountId.Length>0&&!auth.SessionTokenExists)
    throw new InvalidOperationException("Please sign in again from Your profile to restore this account.");
   await auth.SignInAnonymouslyAsync();
   if(PlayerIdentity.AccountId.Length>0&&auth.PlayerId!=PlayerIdentity.AccountId){
    auth.SignOut(true);throw new InvalidOperationException("The saved session does not match this profile. Please sign in again.");
   }
  }
  public static async Task SignIn(AccountProvider provider,bool link) {
   if(Busy||RoomLocked)throw new InvalidOperationException("Leave your room before changing accounts.");
   string unavailable=AndroidAccountSignIn.Unavailable(provider);if(unavailable.Length>0)throw new InvalidOperationException(unavailable);
   Busy=true;string token=null,oldAccess=null,oldSession=null,oldPlayerId=null;
   try{
    // Cancellation of the phone prompt leaves the current UGS session untouched.
    token=await AndroidAccountSignIn.SignIn(provider);
    if(link)await EnsureSession();else await Initialize();
    var auth=AuthenticationService.Instance;string oldId=auth.PlayerId,oldName=PlayerIdentity.SavedName;
    oldAccess=auth.IsAuthorized?auth.AccessToken:null;oldSession=auth.SessionToken;oldPlayerId=auth.PlayerId;
    if(provider==AccountProvider.Google){
     if(link)await auth.LinkWithGoogleAsync(token);
     else {auth.SignOut();await auth.SignInWithGoogleAsync(token,new SignInOptions{CreateAccount=false});}
    }else{
     var result=await ExchangeHuawei(token,link?auth.AccessToken:null,link);
     if(link&&result.playerId!=oldId)throw new InvalidOperationException("The account link did not preserve your player ID.");
     auth.SignOut();auth.ProcessAuthenticationTokens(result.accessToken,result.sessionToken);
     if(auth.PlayerId!=result.playerId)throw new InvalidOperationException("The account service returned an inconsistent player ID.");
    }
    // Successful linking keeps the name; recovering a different account cannot inherit it.
    PlayerIdentity.ActivateAccount(auth.PlayerId,link?oldName:"");
   }catch{
    if(!string.IsNullOrEmpty(oldAccess)&&!string.IsNullOrEmpty(oldSession)){
     try{var auth=AuthenticationService.Instance;if(!auth.IsAuthorized||auth.PlayerId!=oldPlayerId){auth.SignOut();auth.ProcessAuthenticationTokens(oldAccess,oldSession);}}catch{ }
    }
    throw;
   }finally{token=oldAccess=oldSession=null;Busy=false;}
  }
  static async Task<HuaweiResponse> ExchangeHuawei(string token,string accessToken,bool link) {
   string endpoint=AndroidAccountSignIn.Capabilities.huaweiEndpoint;
   if(!Uri.TryCreate(endpoint,UriKind.Absolute,out var uri)||uri.Scheme!="https")throw new InvalidOperationException("Huawei account service is not configured.");
   var body=Encoding.UTF8.GetBytes(JsonUtility.ToJson(new HuaweiRequest{idToken=token,accessToken=accessToken,link=link}));
   using(var request=new UnityWebRequest(endpoint,"POST")){
    request.uploadHandler=new UploadHandlerRaw(body);request.downloadHandler=new DownloadHandlerBuffer();
    request.SetRequestHeader("Content-Type","application/json");request.timeout=25;request.redirectLimit=0;
    var operation=request.SendWebRequest();while(!operation.isDone)await Task.Yield();
    if(request.result!=UnityWebRequest.Result.Success){
     if(request.responseCode==409)throw new InvalidOperationException("This Huawei account is already linked. Choose Use an existing account to switch; your current profile will not be merged.");
     if(request.responseCode==404)throw new InvalidOperationException("No game account is linked to that Huawei ID yet. Choose Keep this profile to create one.");
     throw new InvalidOperationException("Huawei account verification failed. Check your connection and try again.");
    }
    if(request.downloadHandler.data.Length>32768)throw new InvalidOperationException("Unexpected response from the account service.");
    var result=JsonUtility.FromJson<HuaweiResponse>(request.downloadHandler.text);
    if(result==null||string.IsNullOrEmpty(result.accessToken)||string.IsNullOrEmpty(result.sessionToken)||string.IsNullOrEmpty(result.playerId))
     throw new InvalidOperationException("The account service did not complete sign-in.");
    return result;
   }
  }
  public static void SignOut() {
   if(Busy||RoomLocked)throw new InvalidOperationException("Leave your room before signing out.");
   if(UnityServices.State==ServicesInitializationState.Initialized)AuthenticationService.Instance.SignOut(true);
   AndroidAccountSignIn.SignOutNative();PlayerIdentity.ClearAccount();
  }
  public static string Explain(Exception error) {
   if(error is OperationCanceledException)return "Sign-in cancelled. Your current profile is unchanged.";
   if(error is AuthenticationException auth){
    if(auth.ErrorCode==AuthenticationErrorCodes.AccountAlreadyLinked)return "That account is already linked. Choose Use an existing account to switch. Your current profile will not be merged.";
    return "The game could not verify this account (code "+auth.ErrorCode+"). Check the provider setup and connection.";
   }
   if(error is RequestFailedException)return "The account service could not be reached. Your saved profile is unchanged.";
   return error is InvalidOperationException?error.Message:"Sign-in failed. Please try again.";
  }
 }
}
