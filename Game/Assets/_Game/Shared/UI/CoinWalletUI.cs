using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static WhatTheFish.PlayerHubStyle;

namespace WhatTheFish {
 public sealed class CoinWalletUI:MonoBehaviour {
  static CoinWalletUI instance;static int closedFrame=-1;static Sprite coin;
  public static bool Visible=>instance&&instance.shade&&instance.shade.activeSelf;
  public static bool BlocksBack=>Visible||CosmeticPurchaseUI.BlocksBack||closedFrame==Time.frameCount;
  public static Sprite Coin=>coin?coin:coin=Resources.Load<Sprite>("Menu/GoldFishCoin");
  public static string Amount=>PlayerEconomy.Wallet==null?"—":PlayerEconomy.Wallet.balance.ToString("N0");
  public static string Balance=>"Coins: "+Amount;
  public static readonly Color ClaimYellow=CoveUI.Hex("FFD45A");
  GameObject shade;RectTransform card;Text balance,status;Button retry;
  readonly Button[] days=new Button[7];readonly CovePlate[] tiles=new CovePlate[7];
  readonly GameObject[] checks=new GameObject[7],coins=new GameObject[7];
  bool wasBlocked;float refresh;
  public static string ResultSummary {
   get {string id=NetworkAthlete.HostPlayer?NetworkAthlete.HostPlayer.CoinMatch.Value.ToString():"";
    var transaction=PlayerEconomy.Wallet?.history?.FirstOrDefault(t=>t.key=="match:"+id);
    return transaction!=null?"Reward: +"+transaction.amount+" coins":MatchRewards.Ineligible||NetworkAthlete.HostPlayer&&NetworkAthlete.HostPlayer.CoinState.Value==3?"This match has no coin reward.":id.Length>0?"Match rewards pending.":"Online matches earn coins.";
   }
  }
  void Awake(){instance=this;}
  public static void Open(){if(!instance||Visible)return;instance.Build();instance.wasBlocked=AppRoot.Instance&&AppRoot.Instance.view&&AppRoot.Instance.view.ExplicitInputBlocked;if(AppRoot.Instance&&AppRoot.Instance.view){AppRoot.Instance.view.ClearMatchInput();AppRoot.Instance.view.InputBlocked=true;}instance.shade.SetActive(true);instance.Refresh();PlayerEconomy.RefreshSoon();}
  void Build(){
   if(shade)return;
   shade=new GameObject("Coin wallet overlay",typeof(RectTransform),typeof(Canvas),typeof(GraphicRaycaster));shade.transform.SetParent(transform,false);
   var canvas=shade.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=32000;
   shade.AddComponent<Image>().color=new Color(.015f,.045f,.07f,.88f);
   card=CoveUI.Rect(shade.transform,"Coin wallet safe area",0,0,1280,520);Card(card,"Daily login surface",0,0,1280,520);
   CoveUI.Icon(card,CoveSymbol.Gift,40,38,42,ClaimYellow);
   Label(card,"Daily login",102,25,670,64,42,true);
   CoveUI.Image(card,"Reward balance coin",Coin,872,33,46,46);
   balance=Label(card,"",928,32,232,48,30,true,Ink,TextAnchor.MiddleRight);balance.resizeTextForBestFit=true;balance.resizeTextMinSize=16;balance.resizeTextMaxSize=30;
   Action(card,"Close coin wallet","",1182,30,58,58,Raised,Close);
   CoveUI.Icon(card.Find("Close coin wallet"),CoveSymbol.Close,15,15,28,Ink);
   for(int i=0;i<7;i++){
    int day=i+1;
    var tile=tiles[i]=Card(card,"Daily coin card "+i,39+i*174,140,158,270,Color.white,16);tile.border=0;tile.raycastTarget=true;
    var button=days[i]=tile.gameObject.AddComponent<CoveButton>();button.targetGraphic=tile;button.transition=Selectable.Transition.None;
    button.onClick.AddListener(()=>Claim(day));
    Label(tile.transform,"Day "+day,8,22,142,40,26,true,Dark,TextAnchor.MiddleCenter);
    Card(tile.transform,"Calendar divider",16,76,126,2,CoveUI.Hex("D6DEE0"),0).border=0;
    foreach(float x in new[]{37f,107f}){var ring=Card(tile.transform,"Calendar binding",x,-12,14,28,Muted,7);ring.border=0;}
    coins[i]=CoveUI.Image(tile.transform,"Daily reward coin",Coin,37,98,84,84).gameObject;
    checks[i]=CoveUI.Icon(tile.transform,CoveSymbol.Check,43,105,72,Dark).gameObject;
    Label(tile.transform,"+"+PlayerEconomy.DailyRewards[i],8,205,142,47,36,true,Dark,TextAnchor.MiddleCenter);
   }
   status=Label(card,"",40,442,970,48,23,false,Muted);
   retry=Action(card,"Retry reward sync","Retry",1040,440,200,54,Raised,()=>{PlayerEconomy.RefreshSoon();_ = PlayerEconomy.Refresh();},CoveSymbol.None,25);
  }
  void Claim(int day){if(!PlayerEconomy.CanClaimDaily(day))return;_ = PlayerEconomy.ClaimDaily(day);Refresh();}
  void Update(){if(!Visible)return;Layout();if(Time.unscaledTime>=refresh){refresh=Time.unscaledTime+.1f;Refresh();}if(Input.GetKeyDown(KeyCode.Escape))Close();if(AppRoot.Instance&&AppRoot.Instance.view)AppRoot.Instance.view.InputBlocked=true;}
  void Layout(){var safe=Screen.safeArea;float scale=Mathf.Min(safe.width/1280,safe.height/520)*.94f;card.localScale=Vector3.one*scale;card.anchoredPosition=new Vector2(safe.center.x-640*scale,-(Screen.height-safe.center.y-260*scale));}
  void Refresh(){
   if(!card)return;var w=PlayerEconomy.Wallet;balance.text=Amount;
   int today=Mathf.Clamp(w?.nextDay??1,1,7),claimedCount=w==null?0:w.todayClaimed?today:today-1;
   for(int i=0;i<7;i++){
    bool canClaim=PlayerEconomy.CanClaimDaily(i+1),claimed=i<claimedCount;
    days[i].interactable=canClaim;tiles[i].color=canClaim?ClaimYellow:Color.white;
    coins[i].SetActive(!claimed);checks[i].SetActive(claimed);
   }
   bool waiting=PlayerEconomy.Busy&&!PlayerEconomy.Confirmed,unavailable=!PlayerEconomy.Confirmed||!PlayerEconomy.Enabled;
   status.text=waiting?"Syncing coins...":!PlayerEconomy.Enabled?"Connect to sync your coins.":unavailable?PlayerEconomy.Message:"";
   status.gameObject.SetActive(status.text.Length>0);retry.gameObject.SetActive(unavailable&&!PlayerEconomy.Busy);Layout();
  }
  void Close(){if(!Visible)return;shade.SetActive(false);closedFrame=Time.frameCount;if(AppRoot.Instance&&AppRoot.Instance.view)AppRoot.Instance.view.InputBlocked=wasBlocked;}
  void OnDestroy(){Close();if(instance==this)instance=null;}
 }
 public sealed class CoinBalanceLabel:MonoBehaviour {
  public bool Result,AmountOnly;Text label;int maximum;
  void Awake(){label=GetComponent<Text>();maximum=label?label.fontSize:25;if(label){label.resizeTextForBestFit=true;label.resizeTextMinSize=14;label.resizeTextMaxSize=maximum;}}
  void Update(){if(label)label.text=Result?CoinWalletUI.ResultSummary:AmountOnly?CoinWalletUI.Amount:CoinWalletUI.Balance;}
 }
 public sealed class DailyRewardHint:MonoBehaviour {
  GameObject dot;
  void Awake(){var plate=Card(transform,"Unclaimed reward hint",250,-6,18,18,CoveUI.Hex("F05D63"),9);plate.border=0;dot=plate.gameObject;Refresh();}
  void Update()=>Refresh();
  void Refresh(){var wallet=PlayerEconomy.Wallet;dot.SetActive(wallet!=null&&(!wallet.todayClaimed||PlayerEconomy.ServerNow>=wallet.nextReset));}
 }
}
