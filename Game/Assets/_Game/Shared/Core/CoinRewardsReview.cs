#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WhatTheFish {
 // Exercises the production calendar with a deterministic, delayed account transport.
 public sealed class CoinRewardsReview:MonoBehaviour {
  string folder;bool failed,failClaim;int count,posts,reads;CoinWallet server;AppRoot app;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-coinRewardsReview");if(i<0||i+1>=args.Length)return;var r=new GameObject("Coin rewards review").AddComponent<CoinRewardsReview>();r.folder=args[i+1];PlayerEconomy.ReviewOwner="coin_calendar_review";PlayerEconomy.ReviewTransport=r.Transport;r.SetDay(1);}
  CoinWallet Copy()=>JsonUtility.FromJson<CoinWallet>(JsonUtility.ToJson(server));
  void SetDay(int day,bool claimed=false,long? amount=null){server=new CoinWallet{playerId=PlayerEconomy.ReviewOwner,storeVersion=2,revision=(server?.revision??0)+1,balance=amount??PlayerEconomy.DailyRewards.Take(claimed?day:day-1).Sum(),serverTime=1700000000,nextReset=1700003600,nextDay=day,streak=claimed?day:day-1,todayClaimed=claimed,owned=Array.Empty<string>()};}
  async Task<string> Transport(string path,string method,object body){
   if(path=="daily")posts++;else if(path=="wallet")reads++;else throw new Exception("Unexpected fixture route "+path);
   await Task.Delay(120);
   if(path=="daily"){
    if(failClaim){failClaim=false;throw new Exception("Simulated connection failure");}
    if(!server.todayClaimed){server.balance+=PlayerEconomy.DailyRewards[server.nextDay-1];server.streak=server.nextDay;server.todayClaimed=true;server.revision++;}
   }
   return JsonUtility.ToJson(server);
  }
  void Publish()=>PlayerEconomy.Receive(Copy(),PlayerEconomy.Scope);
  void Check(bool ok,string label){count++;failed|=!ok;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+label+"\n");}
  IEnumerator Until(Func<bool> condition,string label,float timeout=120){float end=Time.realtimeSinceStartup+timeout;while(!condition()&&Time.realtimeSinceStartup<end)yield return null;Check(condition(),label);}
  Button Day(int day)=>GameObject.Find("Daily coin card "+(day-1)).GetComponent<Button>();
  Transform Hint=>app.Menu.Content.Find("Rewards/Unclaimed reward hint");
  void State(int day,bool claimed,string context){
   for(int i=1;i<=7;i++){
    var b=Day(i);bool eligible=i==day&&!claimed,received=i<day||i==day&&claimed;
    Check(b.interactable==eligible,"only current unclaimed day interactive "+context+" "+i);
    Check(b.GetComponent<CovePlate>().color==(eligible?CoinWalletUI.ClaimYellow:Color.white),"yellow current day, white others "+context+" "+i);
    Check(b.transform.Find("Check").gameObject.activeSelf==received,"claimed tick state "+context+" "+i);
    Check(b.transform.Find("Daily reward coin").gameObject.activeSelf!=received,"coin replaced with tick "+context+" "+i);
   }
   Check(Hint.gameObject.activeSelf==!claimed,"red hint follows unclaimed state "+context);
  }
  void Fits(GameObject root,string context){
   Canvas.ForceUpdateCanvases();
   foreach(var t in root.GetComponentsInChildren<Text>().Where(t=>t.isActiveAndEnabled&&!string.IsNullOrEmpty(t.text))){
    if(t is LocalizedText localized)Check(localized.HasUnclippedText(),"all glyphs fit "+context+" "+GameLocalization.Source(t));
    Check(t.resizeTextForBestFit||t.preferredHeight<=t.rectTransform.rect.height+4,"text height fits "+context+" "+GameLocalization.Source(t));
   }
  }
  static Rect Bounds(RectTransform r){var corners=new Vector3[4];r.GetWorldCorners(corners);return Rect.MinMaxRect(corners.Min(p=>p.x),corners.Min(p=>p.y),corners.Max(p=>p.x),corners.Max(p=>p.y));}
  void Click(Button b){
   Canvas.ForceUpdateCanvases();var pointer=new PointerEventData(EventSystem.current){position=Bounds((RectTransform)b.transform).center,button=PointerEventData.InputButton.Left};
   var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);var target=hits.Count>0?ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject):null;
   Check(b.IsInteractable()&&target==b.gameObject,"pointer reaches "+b.name);if(target!=b.gameObject)return;
   ExecuteEvents.Execute(b.gameObject,pointer,ExecuteEvents.pointerDownHandler);ExecuteEvents.Execute(b.gameObject,pointer,ExecuteEvents.pointerUpHandler);ExecuteEvents.Execute(b.gameObject,pointer,ExecuteEvents.pointerClickHandler);
  }
  IEnumerator Start(){
   DontDestroyOnLoad(gameObject);Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");Application.targetFrameRate=30;
   yield return Until(()=>DevelopmentProbe.ReadyToReviewSport,"game initialized");app=AppRoot.Instance;if(!app){Finish();yield break;}
   yield return Until(()=>PlayerEconomy.Confirmed&&!PlayerEconomy.Busy,"initial wallet read");
   Check(posts==0&&PlayerEconomy.Wallet.balance==0&&!PlayerEconomy.Wallet.todayClaimed,"startup does not auto claim");
   app.LeaveCourt();yield return null;app.Show("home");yield return new WaitForSecondsRealtime(.5f);
   foreach(var language in new[]{GameLanguage.English,GameLanguage.SimplifiedChinese,GameLanguage.Malay}){
    GameLocalization.Select(language,false);yield return new WaitForSecondsRealtime(.4f);SetDay(3);Publish();yield return new WaitForSecondsRealtime(.2f);
    var display=app.Menu.Content.Find("Coin balance");var rewards=app.Menu.Content.Find("Rewards").GetComponent<Button>();
    Check(!display.GetComponent<Selectable>()&&!display.GetComponentsInChildren<Graphic>().Any(g=>g.raycastTarget),"balance is a non-clickable display "+language);
    Check(display.GetComponentInChildren<Image>().sprite==CoinWalletUI.Coin&&CoinWalletUI.Coin,"gold coin asset loaded "+language);
    Check(rewards.GetComponentInChildren<CoveIcon>().symbol==CoveSymbol.Gift,"rewards uses gift icon "+language);
    Check(Hint.gameObject.activeSelf&&Hint.GetComponent<CovePlate>().color==CoveUI.Hex("F05D63")&&!Hint.GetComponentsInChildren<Text>().Any(),"red dot has no number "+language);
    Fits(app.Menu.Content.gameObject,"home "+language);
    Check(!Bounds((RectTransform)display).Overlaps(Bounds((RectTransform)rewards.transform)),"coin and rewards do not overlap "+language);
    yield return Capture("home-"+GameLocalization.Code);int startPosts=posts;Click(rewards);yield return new WaitForSecondsRealtime(.4f);
    Check(CoinWalletUI.Visible&&posts==startPosts,"opening rewards only reads coins "+language);
    var panel=GameObject.Find("Coin wallet safe area");var bounds=Bounds((RectTransform)panel.transform);
    Check(bounds.xMin>=Screen.safeArea.xMin&&bounds.xMax<=Screen.safeArea.xMax&&bounds.yMin>=Screen.safeArea.yMin&&bounds.yMax<=Screen.safeArea.yMax,"calendar remains inside safe area "+language);
    Check(panel.GetComponentsInChildren<Text>().Count(t=>GameLocalization.Source(t).StartsWith("Day "))==7,"exactly seven days "+language);
    Check(!panel.GetComponentsInChildren<Text>().Any(t=>new[]{"Received","Next","Today"}.Contains(GameLocalization.Source(t))),"no received next today subtitles "+language);
    Fits(panel,"calendar "+language);
    for(int day=1;day<=7;day++){
     SetDay(day);Publish();yield return new WaitForSecondsRealtime(.2f);State(day,false,language+" day="+day);
     if(day==3)yield return Capture("calendar-available-"+GameLocalization.Code);
     int beforePosts=posts;long beforeBalance=PlayerEconomy.Wallet.balance;
     int wrong=day==7?1:day+1;var ignored=PlayerEconomy.ClaimDaily(wrong);Day(wrong).onClick.Invoke();
     Check(posts==beforePosts&&ignored.IsCompleted,"future/past days cannot claim "+language+" "+day);
     Click(Day(day));Day(day).onClick.Invoke();
     Check(PlayerEconomy.Busy&&posts==beforePosts+1,"rapid repeat click submits once "+language+" "+day);
     yield return Until(()=>!PlayerEconomy.Busy,"claim completes "+language+" "+day,10);yield return new WaitForSecondsRealtime(.2f);
     Check(PlayerEconomy.Wallet.balance==beforeBalance+PlayerEconomy.DailyRewards[day-1],"correct daily credit "+language+" "+day);State(day,true,language+" claimed="+day);
     Day(day).onClick.Invoke();Check(posts==beforePosts+1,"claimed day cannot repeat "+language+" "+day);
     if(day==3){
      yield return Capture("calendar-claimed-"+GameLocalization.Code);Click(GameObject.Find("Close coin wallet").GetComponent<Button>());yield return null;
      Check(!CoinWalletUI.Visible,"close calendar "+language);Click(rewards);yield return new WaitForSecondsRealtime(.4f);State(day,true,"reopened "+language);Check(posts==beforePosts+1,"reopen does not claim "+language);
     }
    }
    Click(GameObject.Find("Close coin wallet").GetComponent<Button>());yield return null;
   }
   GameLocalization.Select(GameLanguage.English,false);yield return new WaitForSecondsRealtime(.3f);
   SetDay(3,true);Publish();yield return null;int rolloverPosts=posts,rolloverReads=reads;
   var stale=Copy();stale.nextReset=stale.serverTime;SetDay(4,false,stale.balance);PlayerEconomy.Receive(stale,PlayerEconomy.Scope);
   Check(!PlayerEconomy.CanClaimDaily(3)&&!PlayerEconomy.CanClaimDaily(4),"expired snapshot cannot claim");
   yield return Until(()=>reads>rolloverReads&&!PlayerEconomy.Busy,"midnight refresh reads next day",10);yield return null;
   Check(posts==rolloverPosts&&PlayerEconomy.Wallet.nextDay==4&&!PlayerEconomy.Wallet.todayClaimed&&Hint.gameObject.activeSelf,"next day shows red hint without auto claim");
   CoinWalletUI.Open();yield return new WaitForSecondsRealtime(.4f);State(4,false,"midnight");
   SetDay(1,false,900);Publish();yield return new WaitForSecondsRealtime(.2f);State(1,false,"missed day resets streak only");Check(PlayerEconomy.Wallet.balance==900,"missed day retains coins");
   failClaim=true;int failedPosts=posts;Click(Day(1));yield return Until(()=>!PlayerEconomy.Busy,"failed claim completes",10);yield return new WaitForSecondsRealtime(.2f);
   Check(!PlayerEconomy.Confirmed&&PlayerEconomy.Wallet.balance==900&&!Day(1).interactable&&Hint.gameObject.activeSelf,"failed claim keeps balance and red hint without false tick");
   Check(GameObject.Find("Retry reward sync")&&GameObject.Find("Retry reward sync").GetComponent<Button>().interactable,"failure offers read-only retry");yield return Capture("calendar-offline");
   Click(GameObject.Find("Retry reward sync").GetComponent<Button>());yield return Until(()=>!PlayerEconomy.Busy&&PlayerEconomy.Confirmed,"retry restores wallet",10);yield return new WaitForSecondsRealtime(.2f);
   Check(posts==failedPosts+1&&PlayerEconomy.Wallet.balance==900,"retry reads without claiming");State(1,false,"retry");
   Click(Day(1));yield return Until(()=>!PlayerEconomy.Busy,"manual retry claim",10);yield return new WaitForSecondsRealtime(.2f);State(1,true,"successful retry");
   Check(PlayerEconomy.Wallet.balance==1000,"retry credits exactly once");
   Click(GameObject.Find("Close coin wallet").GetComponent<Button>());yield return null;
   // The header must handle the largest protocol balance without moving its neighbours.
   SetDay(1,true,long.MaxValue);Publish();yield return new WaitForSecondsRealtime(.2f);Fits(app.Menu.Content.Find("Coin balance").gameObject,"large balance");yield return Capture("home-large-balance");
   Finish();
  }
  IEnumerator Capture(string name){
   yield return new WaitForEndOfFrame();var camera=Camera.main;GameObject temporary=null;if(!camera){temporary=new GameObject("Reward capture camera");camera=temporary.AddComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;}
   var canvases=FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.isRootCanvas&&c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();
   foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=camera;c.planeDistance=1;}
   Canvas.ForceUpdateCanvases();var rt=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var old=RenderTexture.active;var original=camera.targetTexture;var image=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);
   try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);image.Apply();Check(image.GetPixels32().Where((p,i)=>i%101==0).Select(p=>p.r+p.g*256+p.b*65536).Distinct().Count()>30,"capture has rendered colour "+name);File.WriteAllBytes(Path.Combine(folder,name+".png"),image.EncodeToPNG());}
   finally{Destroy(image);camera.targetTexture=original;RenderTexture.active=old;RenderTexture.ReleaseTemporary(rt);foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceOverlay;c.worldCamera=null;}Canvas.ForceUpdateCanvases();if(temporary)Destroy(temporary);}
  }
  void Finish(){File.AppendAllText(Path.Combine(folder,"results.txt"),"COIN_REWARDS_COMPLETE success="+!failed+" checks="+count+"\n");Application.Quit(failed?1:0);}
 }
}
#endif
