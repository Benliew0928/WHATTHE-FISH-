#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace WhatTheFish {
 // Opt-in render regression fixtures only; never compiled into production Android.
 public static class EconomyRenderReview {
  public static void UnlockAll(){
   PlayerEconomy.ReviewOwner="economy_review_render";
   var wallet=new CoinWallet{playerId=PlayerEconomy.ReviewOwner,storeVersion=2,revision=1,todayClaimed=true,nextReset=long.MaxValue,owned=CosmeticStore.Items.Select(i=>i.id).ToArray()};
   PlayerEconomy.ReviewTransport=(path,method,body)=>Task.FromResult(JsonUtility.ToJson(wallet));PlayerEconomy.IdentityChanged();PlayerEconomy.Receive(wallet,PlayerEconomy.Scope);
  }
 }
 public sealed class EconomyReview:MonoBehaviour {
  string folder,role;bool failed;int count;AppRoot app;
  [Serializable]sealed class Error {public string code;}
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-economyReview");if(i<0||i+1>=args.Length)return;var r=new GameObject("Economy review").AddComponent<EconomyReview>();r.folder=args[i+1];i=Array.IndexOf(args,"-economyRole");r.role=i>=0?args[i+1]:"host";PlayerEconomy.ReviewOwner="economy_review_"+r.role;PlayerEconomy.ReviewTransport=r.Transport;}
  async Task<string> Transport(string path,string method,object body){
   using var request=new UnityWebRequest("http://127.0.0.1:8857/v1/economy/"+path,method);request.downloadHandler=new DownloadHandlerBuffer();request.timeout=10;
   request.SetRequestHeader("X-Economy-Player",PlayerEconomy.Owner);request.SetRequestHeader("Content-Type","application/json");
   if(body!=null)request.uploadHandler=new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonUtility.ToJson(body)));
   var op=request.SendWebRequest();while(!op.isDone)await Task.Yield();
   if(request.result!=UnityWebRequest.Result.Success){string code="fixture_error";try{code=JsonUtility.FromJson<Error>(request.downloadHandler.text).code;}catch{}throw new ProfileException(request.responseCode,code);}
   return request.downloadHandler.text;
  }
  void Check(bool pass,string label){count++;failed|=!pass;File.AppendAllText(Path.Combine(folder,"results.txt"),(pass?"PASS ":"FAIL ")+label+"\n");}
  IEnumerator Wait(Func<bool> condition,string label,float seconds=100){float end=Time.realtimeSinceStartup+seconds;while(!condition()&&Time.realtimeSinceStartup<end)yield return null;Check(condition(),label);}
  static object Invoke(object obj,string name,params object[] args)=>obj.GetType().GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(obj,args);
  IEnumerator Start(){
   DontDestroyOnLoad(gameObject);Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");Application.targetFrameRate=30;
   yield return Wait(()=>DevelopmentProbe.ReadyToReviewSport,"sport initialized");app=AppRoot.Instance;if(!app){Finish();yield break;}
   PlayerIdentity.SaveName(role,out _);yield return Wait(()=>PlayerEconomy.Confirmed,"wallet confirmed");Check(PlayerEconomy.Wallet?.balance==0,"background sync does not claim daily coins");
   var firstClaim=PlayerEconomy.ClaimDaily(1);while(!firstClaim.IsCompleted)yield return null;Check(PlayerEconomy.Wallet?.balance==100,"explicit first daily claim is 100");
   yield return new WaitForSecondsRealtime(2);
   if(!app.rooms.Connected){yield return WalletChecks();Finish();yield break;}
   int expected=app.SelectedSport==SportId.Basketball?4:2;
   yield return Wait(()=>Athlete.Active.Count(a=>a&&a.GetComponent<NetworkAthlete>() is NetworkAthlete n&&n.IsSpawned&&n.IsPlayerObject)==expected,"complete human roster");
   yield return new WaitForSecondsRealtime(3);
   if(app.rooms.Host){
    foreach(var n in FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None).Where(n=>n.IsSpawned&&n.IsPlayerObject))n.ProfileId.Value=new FixedString128Bytes("economy_review_"+n.Nickname.Value.ToString());
    yield return new WaitForSecondsRealtime(1);
    switch(app.SelectedSport){
     case SportId.Football:Check(FootballMatch.Instance.StartMatch(),"football starts");Invoke(FootballMatch.Instance,"ConfirmTeams",FootballMatch.Instance.Now);break;
     case SportId.Basketball:Check(BasketballMatch.Instance.StartMatch(),"basketball starts");Invoke(BasketballMatch.Instance,"ConfirmTeams");break;
     case SportId.Golf:Check(GolfMatchManager.Instance.StartMatch(),"golf starts");break;
     case SportId.Fishing:Check(FishingGame.Instance.StartMatch(FishingMode.Cup),"fishing cup starts");break;
    }
   }
   yield return Wait(()=>NetworkAthlete.HostPlayer&&NetworkAthlete.HostPlayer.CoinMatch.Value.Length>0,"reward ticket replicated");
   string ticket=NetworkAthlete.HostPlayer?NetworkAthlete.HostPlayer.CoinMatch.Value.ToString():"";
   if(app.rooms.Host){
    yield return new WaitForSecondsRealtime(14);
    switch(app.SelectedSport){
     case SportId.Football:
      typeof(FootballMatchState).GetProperty("ScoreA").SetValue(FootballMatch.Instance.State,2);
      Invoke(FootballMatch.Instance.State,"Finish",FootballMatchResult.TeamA,FootballMatch.Instance.Now);Invoke(FootballMatch.Instance,"Publish");break;
     case SportId.Basketball:
      typeof(BasketballMatchState).GetProperty("ScoreA").SetValue(BasketballMatch.Instance.State,2);
      Invoke(BasketballMatch.Instance.State,"Finish");Invoke(BasketballMatch.Instance,"Publish");break;
     case SportId.Golf:
      var g=GolfMatchManager.Instance;int order=0;
      foreach(var p in g.State.Players){for(int h=0;h<5;h++){g.State.RecordSwing(p.PlayerId,g.Now+10+order);g.State.EnterHole(p.PlayerId,p.CurrentHole,g.Now+10+order);}order++;}Invoke(g,"Publish");break;
     case SportId.Fishing:
      var f=FishingGame.Instance;
      for(int round=1;round<=3;round++){
       f.State.Advance(f.State.Match.draftUntil+1,0);f.State.Advance(f.State.Match.started+1,0);
       for(int i=0;i<f.State.Players.Count;i++){var p=f.State.Players[i];p.score=10-i;f.State.Players[i]=p;}
       f.State.Advance(f.State.Match.deadline+1,0);Invoke(f,"Publish");
       if(round<3){Check(NetworkAthlete.HostPlayer.CoinMatch.Value.ToString()==ticket,"cup round retains one ticket");Check(PlayerEconomy.Wallet.history.All(t=>!t.key.StartsWith("match:")),"no intermediate cup payout");Check(f.StartMatch(FishingMode.Cup),"next cup round starts");}
      }break;
    }
   }
   PlayerEconomy.RefreshSoon();
   yield return Wait(()=>PlayerEconomy.Wallet?.history?.Any(t=>t.key=="match:"+ticket)==true,"confirmed match receipt",100);
   var receipt=PlayerEconomy.Wallet?.history?.FirstOrDefault(t=>t.key=="match:"+ticket);
   if(receipt!=null){
    var n=app.LocalAthlete.GetComponent<NetworkAthlete>();int amount=app.SelectedSport==SportId.Football?(n.Team.Value==FootballTeam.A?100:30):app.SelectedSport==SportId.Basketball?(n.BasketballTeam.Value==FootballTeam.A?100:30):role=="host"?200:100;
    Check(receipt.amount==amount,"correct participant payout "+amount);Check(PlayerEconomy.Wallet.balance==100+amount,"daily plus match balance");
    var task=PlayerEconomy.Refresh(true);while(!task.IsCompleted)yield return null;Check(PlayerEconomy.Wallet.history.Count(t=>t.key=="match:"+ticket)==1,"refresh and repeated daily request do not duplicate match payout");
   }
   if(app.rooms.Host){yield return WalletChecks();yield return new WaitForSecondsRealtime(12);}else yield return new WaitForSecondsRealtime(15);
   Finish();
  }
  IEnumerator WalletChecks(){
   const string removed="economy_review_removed",kept="economy_review_kept",removedScope="economy_review_removed_scope";
   var removedKeys=new[]{"economy.wallet."+removed,"economy.outbox."+removed,"economy.trail."+removedScope+".footballTrail","economy.trail."+removedScope+".footballTrail.pending","economy.trail."+removedScope+".basketballTrail","economy.trail."+removedScope+".basketballTrail.pending"};
   foreach(var key in removedKeys)PlayerPrefs.SetString(key,"fixture");PlayerPrefs.SetString("economy.wallet."+kept,"fixture");
   PlayerIdentity.ForgetDeletedIdentity(removed,removedScope);Check(removedKeys.All(key=>!PlayerPrefs.HasKey(key)),"account deletion clears local coin and cosmetic records");
   Check(PlayerPrefs.HasKey("economy.wallet."+kept),"account deletion preserves another wallet cache");PlayerPrefs.DeleteKey("economy.wallet."+kept);PlayerPrefs.Save();
   while(PlayerEconomy.Busy)yield return null;
   var before=PlayerEconomy.Wallet.balance;var task=PlayerEconomy.Refresh(true);while(!task.IsCompleted)yield return null;Check(PlayerEconomy.Wallet.balance==before,"daily repeat is idempotent");
   FootballTrailStyle.Select(1);BasketballTrailStyle.Select(1);task=PlayerEconomy.Refresh();while(!task.IsCompleted)yield return null;
   Check(PlayerEconomy.Wallet.equipped.Any(e=>e.id=="football.trail.ocean")&&PlayerEconomy.Wallet.equipped.Any(e=>e.id=="basketball.trail.lightning"),"both trail selections saved to account");
   Check(PlayerEconomy.Wallet.owned.Length==30,"two free choices per category");
   if(app.rooms.Connected&&PostMatchFlow.Instance){
    PostMatchFlow.CloseResults();yield return null;PostMatchFlow.Instance.Open();yield return null;
    PostMatchFlow.Instance.Close();yield return null;PostMatchFlow.Instance.Close();
   }
   foreach(var language in new[]{GameLanguage.English,GameLanguage.SimplifiedChinese,GameLanguage.Malay}){
    GameLocalization.Select(language,false);CoinWalletUI.Open();yield return new WaitForSecondsRealtime(.6f);Canvas.ForceUpdateCanvases();
    var panel=GameObject.Find("Coin wallet safe area");Check(panel,"wallet is visible "+language);
    foreach(var t in panel.GetComponentsInChildren<Text>().Where(t=>t.gameObject.activeInHierarchy&&t.text.Length>0)){
     Check(!string.IsNullOrWhiteSpace(t.text),"nonempty label "+language+" "+GameLocalization.Source(t));
     Check(t.preferredHeight<=t.rectTransform.rect.height+5,"label fits height "+language+" "+GameLocalization.Source(t));
     if(t is LocalizedText localized)Check(localized.HasUnclippedText(),"all label glyphs visible "+language+" "+GameLocalization.Source(t));
    }
    yield return Capture("wallet-"+GameLocalization.Code);
    panel.GetComponentsInChildren<Button>().First(b=>b.name=="Close coin wallet").onClick.Invoke();yield return null;Check(!CoinWalletUI.Visible,"wallet closes "+language);
    if(app.SelectedSport==SportId.Football){FootballHUD.Instance.ShowSettings();FootballHUD.Instance.SelectMenu(3);yield return new WaitForSecondsRealtime(.3f);Check(FootballHUD.Instance.MenuOpen&&!PostMatchFlow.Instance.Visible&&!FootballMVP.Presenting,"football trail screen unobscured "+language);yield return Capture("trail-"+GameLocalization.Code);FootballHUD.Instance.CloseMenu();}
    if(app.SelectedSport==SportId.Basketball){BasketballHUD.Instance.ShowCustomization();yield return new WaitForSecondsRealtime(.3f);Check(BasketballHUD.Instance.MenuOpen&&!PostMatchFlow.Instance.Visible&&!BasketballMVP.Presenting,"basketball trail screen unobscured "+language);yield return Capture("trail-"+GameLocalization.Code);BasketballHUD.Instance.CloseMenu();}
    if(app.rooms.Connected&&PostMatchFlow.Instance){PostMatchFlow.Instance.Open();yield return new WaitForSecondsRealtime(.3f);yield return Capture("results-"+GameLocalization.Code);PostMatchFlow.Instance.Close();}
   }
   if(!app.rooms.Connected){
    yield return StoreChecks();
    app.LeaveCourt();yield return null;app.Show("home");yield return new WaitForSecondsRealtime(.3f);
    foreach(var language in new[]{GameLanguage.English,GameLanguage.SimplifiedChinese,GameLanguage.Malay}){
     GameLocalization.Select(language,false);yield return new WaitForSecondsRealtime(.3f);yield return Capture("home-"+GameLocalization.Code);
     Check(!GameObject.Find("Coin balance").GetComponent<Button>(),"home coin balance is read-only "+language);
     var button=GameObject.Find("Rewards").GetComponent<Button>();button.onClick.Invoke();yield return null;Check(CoinWalletUI.Visible,"home rewards button opens calendar "+language);
     GameObject.Find("Close coin wallet").GetComponent<Button>().onClick.Invoke();
    }
   }
  }
  void LabelsFit(GameObject panel,string context){
   foreach(var label in panel.GetComponentsInChildren<Text>().Where(t=>t.isActiveAndEnabled&&t.canvasRenderer.GetAlpha()>.01f&&t.text.Length>0)){
    if(label is LocalizedText localized)Check(localized.HasUnclippedText(),"visible text "+context+" "+GameLocalization.Source(label));
   }
  }
  IEnumerator StoreChecks(){
   Check(CosmeticStore.Items.Length==58,"shared catalog has 58 choices");
   FootballTrailStyle.Select(3);BasketballTrailStyle.Select(4);
   Check(FootballTrailStyle.Selected==1&&BasketballTrailStyle.Selected==1,"direct selection rejects locked trails");
   var insufficient=CosmeticStore.Purchase(CosmeticStore.Item("footballTrail",3));while(!insufficient.IsCompleted)yield return null;
   Check(!CosmeticStore.Owned("footballTrail",3)&&PlayerEconomy.Wallet.balance==100,"insufficient balance does not grant item or change coins");
   foreach(var language in new[]{GameLanguage.English,GameLanguage.SimplifiedChinese,GameLanguage.Malay}){
    GameLocalization.Select(language,false);CosmeticPurchaseUI.Open(CosmeticStore.Item("footballTrail",3),null);yield return new WaitForSecondsRealtime(.4f);
    Check(!GameObject.Find("Buy cosmetic").GetComponent<Button>().interactable,"insufficient purchase button disabled "+language);LabelsFit(GameObject.Find("Purchase safe area"),language.ToString());yield return Capture("store-insufficient-"+GameLocalization.Code);
    GameObject.Find("Cancel cosmetic purchase").GetComponent<Button>().onClick.Invoke();yield return null;
   }
   var funding=PlayerEconomy.Request<CoinWallet>("review-fund","POST",new Error());while(!funding.IsCompleted)yield return null;PlayerEconomy.Receive(funding.Result,PlayerEconomy.Scope);
   app.LeaveCourt();yield return null;
   foreach(SportId sport in Enum.GetValues(typeof(SportId))){
    app.Show("home");app.SelectSport(sport);yield return Wait(()=>!app.PendingSelection.HasValue&&app.SelectedSport==sport,"store sport ready "+sport);
    foreach(var language in new[]{GameLanguage.English,GameLanguage.SimplifiedChinese,GameLanguage.Malay}){
     GameLocalization.Select(language,false);app.Show("venues");yield return new WaitForSecondsRealtime(.8f);LabelsFit(app.Menu.Content.gameObject,"venue "+sport+" "+language);
     Check(app.Menu.Content.GetComponentsInChildren<CosmeticLock>().Count(g=>!CosmeticStore.Owned(g.Item))==(sport==SportId.Basketball?4:3),"locked venue count "+sport+" "+language);yield return Capture("store-venue-"+sport+"-"+GameLocalization.Code);
     if(sport==SportId.Football||sport==SportId.Basketball){app.Show("customize");yield return new WaitForSecondsRealtime(.5f);LabelsFit(app.Menu.Content.gameObject,"trail "+sport+" "+language);yield return Capture("store-trails-"+sport+"-"+GameLocalization.Code);}
    }
   }
   foreach(var language in new[]{GameLanguage.English,GameLanguage.SimplifiedChinese,GameLanguage.Malay}){
    GameLocalization.Select(language,false);SocialPanel.OpenOwn();yield return new WaitForSecondsRealtime(.6f);
    Check(FindObjectsByType<CosmeticLock>(FindObjectsSortMode.None).Count(g=>g.Item.slot=="profileAvatar"&&!CosmeticStore.Owned(g.Item))==8,"eight locked avatars "+language);yield return Capture("store-avatars-"+GameLocalization.Code);
    GameObject.Find("Close social panel").GetComponent<Button>().onClick.Invoke();yield return null;
    CosmeticPurchaseUI.Open(CosmeticStore.Item("footballTrail",3),()=>FootballTrailStyle.Select(3));yield return new WaitForSecondsRealtime(.4f);
    LabelsFit(GameObject.Find("Purchase safe area"),"purchase "+language);yield return Capture("store-buy-"+GameLocalization.Code);
    if(language==GameLanguage.Malay){GameObject.Find("Buy cosmetic").GetComponent<Button>().onClick.Invoke();yield return Wait(()=>!CosmeticPurchaseUI.Visible,"purchase dialog completes");}
    else{GameObject.Find("Cancel cosmetic purchase").GetComponent<Button>().onClick.Invoke();yield return null;}
   }
   Check(CosmeticStore.Owned("footballTrail",3)&&FootballTrailStyle.Selected==3&&PlayerEconomy.Wallet.balance==29100,"purchase persists and selects item after 1000 debit");
   long balance=PlayerEconomy.Wallet.balance;var retry=CosmeticStore.Purchase(CosmeticStore.Item("footballTrail",3));while(!retry.IsCompleted)yield return null;Check(PlayerEconomy.Wallet.balance==balance,"purchase retry does not charge twice");
   string owner=PlayerEconomy.ReviewOwner;PlayerEconomy.ReviewOwner="economy_review_other";PlayerEconomy.IdentityChanged();var refresh=PlayerEconomy.Refresh();while(!refresh.IsCompleted)yield return null;
   Check(!CosmeticStore.Owned("footballTrail",3)&&FootballTrailStyle.Selected<2,"another account cannot use purchased item");
   PlayerEconomy.ReviewOwner=owner;PlayerEconomy.IdentityChanged();refresh=PlayerEconomy.Refresh();while(!refresh.IsCompleted)yield return null;Check(CosmeticStore.Owned("footballTrail",3),"ownership restored after account switch");
   foreach(var item in CosmeticStore.Items.Where(i=>i.price>0)){while(PlayerEconomy.Busy)yield return null;var purchase=CosmeticStore.Purchase(item);while(!purchase.IsCompleted)yield return null;Check(CosmeticStore.Owned(item),"purchase category item "+item.id);}
   Check(PlayerEconomy.Wallet.balance==2100,"all 28 paid items charged exactly once");
   foreach(SportId sport in Enum.GetValues(typeof(SportId))){var appearance=LocalProfile.ForSport(sport);appearance.palette=3;appearance.design=2;appearance.logo=3;LocalProfile.SaveSport(sport,appearance);Check(LocalProfile.ForSport(sport).palette==3,"purchased venue can be saved "+sport);}
   app.Show("home");app.SelectSport(SportId.Football);yield return Wait(()=>!app.PendingSelection.HasValue&&app.SelectedSport==SportId.Football,"return to football after store review");
  }
  IEnumerator Capture(string name){
   yield return new WaitForEndOfFrame();var camera=Camera.main;var canvases=FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.isRootCanvas&&c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();
   foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=camera;c.planeDistance=1;}Canvas.ForceUpdateCanvases();
   var rt=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var prior=RenderTexture.active;var target=camera.targetTexture;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
   var pixels=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);pixels.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);pixels.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),pixels.EncodeToPNG());Destroy(pixels);camera.targetTexture=target;RenderTexture.active=prior;RenderTexture.ReleaseTemporary(rt);
   foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceOverlay;c.worldCamera=null;}Canvas.ForceUpdateCanvases();
  }
  void Finish(){File.AppendAllText(Path.Combine(folder,"results.txt"),"ECONOMY_REVIEW_COMPLETE success="+!failed+" checks="+count+"\n");Application.Quit(failed?2:0);}
 }
}
#endif
