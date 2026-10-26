using System;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Scripting;

namespace WhatTheFish {
 [Serializable,Preserve] public sealed class CoinTransaction {public string key,reason;public int amount;public long time;}
 [Serializable,Preserve] public sealed class EquippedCosmetic {public string slot,id;}
 [Serializable,Preserve] public sealed class CoinWallet {
  public string playerId;public long balance,serverTime,nextReset;public int revision,lastDay,streak,nextDay,storeVersion;
  public bool todayClaimed;public int[] dailyRewards;public CoinTransaction[] history=Array.Empty<CoinTransaction>();
  public string[] owned=Array.Empty<string>();public EquippedCosmetic[] equipped=Array.Empty<EquippedCosmetic>();
 }
 public static class PlayerEconomy {
  public static event Action Changed;
  public static CoinWallet Wallet {get;private set;}
  public static bool Confirmed {get;private set;}
  public static bool Busy {get;private set;}
  public static string Message {get;private set;}="Connect to sync your coins.";
  public static string Owner {
   get {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    if(ReviewTransport!=null)return ReviewOwner;
#endif
    return UnityServices.State==ServicesInitializationState.Initialized&&AuthenticationService.Instance.IsAuthorized?AuthenticationService.Instance.PlayerId:"";
   }
  }
  public static string Scope=>PlayerIdentity.ProfileStorageKey+":"+Owner;
  public static string TrailKey(string slot)=>"economy.trail."+PlayerIdentity.ProfileStorageKey+"."+slot;
  public static readonly int[] DailyRewards={100,200,300,400,500,600,1000};
  public static readonly string[] FootballItems={"electric","ocean","rose","solar","aurora"},BasketballItems={"solar","lightning","glacier","rose","aurora","championship"};
  static string scope="";static double receivedAt;static bool applying;static float nextSync;static string pendingFootball,pendingBasketball;static int refreshVersion;
  [Serializable,Preserve] sealed class Empty {public int version=1;}
  [Serializable,Preserve] sealed class EquipRequest {public string slot,id;}
#if UNITY_EDITOR || DEVELOPMENT_BUILD
  public static Func<string,string,object,Task<string>> ReviewTransport;
  public static string ReviewOwner="economy_review_player";
#endif
  public static async Task<T> Request<T>(string path,string method="GET",object body=null){
#if UNITY_EDITOR || DEVELOPMENT_BUILD
   if(ReviewTransport!=null)return JsonUtility.FromJson<T>(await ReviewTransport(path,method,body));
#endif
   return await PlayerProfiles.Json<T>("/v1/economy/"+path,method,body);
  }
  public static long ServerNow=>Wallet==null?0:Wallet.serverTime+(long)Math.Max(0,Time.realtimeSinceStartupAsDouble-receivedAt);
  public static bool Enabled {
   get {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    if(ReviewTransport!=null)return true;
    var args=Environment.GetCommandLineArgs();if(args.Contains("-guestProfile")||args.Contains("-offline")||args.Contains("-probe"))return false;
#endif
    return PlayerIdentity.HasProfile&&Application.internetReachability!=NetworkReachability.NotReachable&&!PlayerAccount.Busy&&!PlayerAccount.DeletionPending;
   }
  }
  public static void IdentityChanged(){
   refreshVersion++;Busy=false;scope=Scope;Wallet=null;Confirmed=false;Message="Connect to sync your coins.";pendingFootball=pendingBasketball=null;nextSync=0;
   FootballTrailStyle.Load();BasketballTrailStyle.Load();Changed?.Invoke();
  }
  internal static void Receive(CoinWallet wallet,string expected){
   if(expected!=Scope||wallet==null||wallet.playerId!=Owner||wallet.balance<0)return;
   if(Confirmed&&Wallet!=null&&Wallet.playerId==wallet.playerId&&wallet.revision<Wallet.revision)return;
   Wallet=wallet;Confirmed=true;receivedAt=Time.realtimeSinceStartupAsDouble;Message="Saved to your account.";
   PlayerPrefs.SetString("economy.wallet."+wallet.playerId,JsonUtility.ToJson(wallet));PlayerPrefs.Save();
   applying=true;try{foreach(var item in wallet.equipped??Array.Empty<EquippedCosmetic>()){
    if(item.slot=="footballTrail"&&pendingFootball==null){int i=Array.IndexOf(FootballItems,item.id?.Replace("football.trail.",""));if(i>=0)FootballTrailStyle.Select(i);}
    if(item.slot=="basketballTrail"&&pendingBasketball==null){int i=Array.IndexOf(BasketballItems,item.id?.Replace("basketball.trail.",""));if(i>=0)BasketballTrailStyle.Select(i);}
   }
   FootballTrailStyle.Load();BasketballTrailStyle.Load();
   }finally{applying=false;}Changed?.Invoke();
  }
  public static async Task Refresh(bool daily=false){
   if(Busy||!Enabled)return;Busy=true;string expected=Scope;int version=++refreshVersion;
   try{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    if(ReviewTransport==null)
#endif
     await PlayerProfiles.EnsureCloud();
    if(expected!=Scope||version!=refreshVersion)return;
    var wallet=await Request<CoinWallet>(daily?"daily":"wallet",daily?"POST":"GET",daily?new Empty():null);
    if(expected!=Scope||version!=refreshVersion)return;Receive(wallet,expected);
    if(expected==Scope){
     foreach(string slot in new[]{"footballTrail","basketballTrail"}){
      string item=slot=="footballTrail"?pendingFootball:pendingBasketball;if(item==null)continue;
      if(!wallet.owned.Contains(item)){if(slot=="footballTrail")pendingFootball=null;else pendingBasketball=null;PlayerPrefs.DeleteKey(TrailKey(slot)+".pending");continue;}
      var updated=await Request<CoinWallet>("equip","POST",new EquipRequest{slot=slot,id=item});
      if(expected!=Scope||version!=refreshVersion)break;
      if(slot=="footballTrail"&&pendingFootball==item){pendingFootball=null;PlayerPrefs.DeleteKey(TrailKey(slot)+".pending");}
      if(slot=="basketballTrail"&&pendingBasketball==item){pendingBasketball=null;PlayerPrefs.DeleteKey(TrailKey(slot)+".pending");}
      Receive(updated,expected);
     }
    }
   }catch(Exception){if(expected==Scope&&version==refreshVersion){Confirmed=false;Message="Coin sync unavailable. Please retry online.";Changed?.Invoke();}}
   finally{if(version==refreshVersion){Busy=false;nextSync=expected==Scope?Time.realtimeSinceStartup+(Confirmed?45:20):0;}}
  }
  public static void SelectTrail(string slot,int index){
   if(applying||!CosmeticStore.Owned(slot,index))return;string item=(slot=="footballTrail"?"football.trail.":"basketball.trail.")+(slot=="footballTrail"?FootballItems:BasketballItems)[index];
   if(slot=="footballTrail")pendingFootball=item;else pendingBasketball=item;
   PlayerPrefs.SetString(TrailKey(slot)+".pending",item);PlayerPrefs.Save();nextSync=0;
  }
  public static void RefreshSoon(){nextSync=0;}
  public static bool CanClaimDaily(int day)=>day>=1&&day<=7&&Enabled&&Confirmed&&!Busy&&Wallet!=null&&!Wallet.todayClaimed&&Wallet.nextDay==day&&ServerNow<Wallet.nextReset;
  public static Task ClaimDaily(int day)=>CanClaimDaily(day)?Refresh(true):Task.CompletedTask;
  public static void Tick(){
   if(scope!=Scope){IdentityChanged();if(Owner.Length>0){
    try{var cached=JsonUtility.FromJson<CoinWallet>(PlayerPrefs.GetString("economy.wallet."+Owner,""));if(cached?.playerId==Owner&&cached.storeVersion==2)Wallet=cached;}catch(Exception){}
    FootballTrailStyle.Load();BasketballTrailStyle.Load();
    pendingFootball=PlayerPrefs.HasKey(TrailKey("footballTrail")+".pending")?PlayerPrefs.GetString(TrailKey("footballTrail")+".pending"):null;
    pendingBasketball=PlayerPrefs.HasKey(TrailKey("basketballTrail")+".pending")?PlayerPrefs.GetString(TrailKey("basketballTrail")+".pending"):null;
   }}
   // Background sync reads the wallet. Only the calendar's current day claims coins.
   if(!Busy&&Enabled&&(Time.realtimeSinceStartup>=nextSync||Confirmed&&Wallet!=null&&ServerNow>=Wallet.nextReset))_=Refresh();
  }
 }
 public sealed class EconomyRuntime:MonoBehaviour {
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){if(!FindFirstObjectByType<EconomyRuntime>()){var go=new GameObject("Account economy");DontDestroyOnLoad(go);go.AddComponent<EconomyRuntime>();go.AddComponent<MatchRewards>();go.AddComponent<CoinWalletUI>();}}
  void Update(){PlayerEconomy.Tick();}
  void OnApplicationFocus(bool focused){if(focused)PlayerEconomy.RefreshSoon();}
 }
}
