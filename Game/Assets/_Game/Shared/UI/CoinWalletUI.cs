using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static WhatTheFish.PlayerHubStyle;

namespace WhatTheFish {
 public sealed class CoinWalletUI:MonoBehaviour {
  static CoinWalletUI instance;static int closedFrame=-1;
  public static bool Visible=>instance&&instance.shade&&instance.shade.activeSelf;
  public static bool BlocksBack=>Visible||CosmeticPurchaseUI.BlocksBack||closedFrame==Time.frameCount;
  GameObject shade;RectTransform card;Text balance,status,clock,history;Text[] days=new Text[7];CovePlate[] tiles=new CovePlate[7];Button sync;
  bool wasBlocked;float refresh;
  public static string Balance=>PlayerEconomy.Wallet==null?"Coins: —":"Coins: "+PlayerEconomy.Wallet.balance.ToString("N0");
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
   card=CoveUI.Rect(shade.transform,"Coin wallet safe area",0,0,1280,740);Card(card,"Coin wallet surface",0,0,1280,740);
   Label(card,"Your coins",40,24,760,60,42,true);balance=Label(card,"",850,24,390,60,34,true,Lime,TextAnchor.MiddleRight);balance.resizeTextForBestFit=true;balance.resizeTextMinSize=16;balance.resizeTextMaxSize=34;
   Label(card,"Return each day. Play together. Make your island style your own.",40,94,1200,60,25,false,Muted);
   for(int i=0;i<7;i++){
    var tile=tiles[i]=Card(card,"Daily coin card "+i,40+i*172,174,164,170,Raised);
    Label(tile.transform,"Day "+(i+1),10,12,144,35,24,true,Cyan,TextAnchor.MiddleCenter);
    Label(tile.transform,"+"+PlayerEconomy.DailyRewards[i],10,52,144,60,39,true,i==6?Lime:Ink,TextAnchor.MiddleCenter);
    days[i]=Label(tile.transform,"",10,121,144,35,20,false,Muted,TextAnchor.MiddleCenter);
   }
   clock=Label(card,"",40,360,1200,44,24,true,Cyan);
   Label(card,"Miss a day to restart at Day 1. After Day 7, a new cycle begins. Your coins stay.",40,408,1200,58,23,false,Muted);
   Card(card,"Recent coin activity",40,484,580,112,Background);history=Label(card,"",58,493,544,94,22);
   Card(card,"Cosmetic ownership concept",640,484,600,112,Background);
   Label(card,"Two free choices per category. Unlock more for 1,000 coins each.",658,493,564,94,23,false,Muted);
   status=Label(card,"",40,600,1200,58,18,false,Muted);
   Action(card,"Close coin wallet","Back",40,660,220,54,Raised,Close,CoveSymbol.Back,25);
   Action(card,"Link wallet account","Sign-in options",280,660,340,54,Raised,()=>{Close();AccountPanel.Open();},CoveSymbol.People,24);
   sync=Action(card,"Sync coin wallet","Sync coins",930,660,310,54,Lime,()=>{PlayerEconomy.RefreshSoon();_ = PlayerEconomy.Refresh(true);},CoveSymbol.Star,25);
  }
  void Update(){if(!Visible)return;Layout();if(Time.unscaledTime>=refresh){refresh=Time.unscaledTime+.25f;Refresh();}if(Input.GetKeyDown(KeyCode.Escape))Close();if(AppRoot.Instance&&AppRoot.Instance.view)AppRoot.Instance.view.InputBlocked=true;}
  void Layout(){var safe=Screen.safeArea;float scale=Mathf.Min(safe.width/1280,safe.height/740)*.94f;card.localScale=Vector3.one*scale;card.anchoredPosition=new Vector2(safe.center.x-640*scale,-(Screen.height-safe.center.y-370*scale));}
  void Refresh(){
   if(!card)return;var w=PlayerEconomy.Wallet;balance.text=Balance;
   int today=w?.nextDay??1;bool claimed=w!=null&&w.todayClaimed;
   for(int i=0;i<7;i++){days[i].text=claimed&&i<today?"Received":!claimed&&i==today-1?"Today":"Next";tiles[i].borderColor=i==today-1?Lime:Line;tiles[i].border=i==today-1?3:1.5f;tiles[i].SetVerticesDirty();}
   var remaining=w==null?TimeSpan.Zero:TimeSpan.FromSeconds(Math.Max(0,w.nextReset-PlayerEconomy.ServerNow));
   clock.text=w==null||!PlayerEconomy.Confirmed?"Daily rewards reset at midnight, Malaysia time.":"Next daily reward in "+remaining.ToString(@"hh\:mm\:ss")+" · Malaysia time";
   status.text=PlayerEconomy.Message+(PlayerIdentity.AccountId.Length==0?"\nLink an account to recover your coins on another device.":"");
   var entries=w?.history?.Take(3).Select(t=>GameLocalization.Translate(t.reason=="purchase"?"Cosmetic purchase":t.reason=="daily"?"Daily reward":t.reason=="football"?"Football":t.reason=="basketball"?"Basketball":t.reason=="golf"?"Golf":"Fishing")+"  "+(t.amount>=0?"+":"")+t.amount).ToArray();
   history.text=entries!=null&&entries.Length>0?string.Join("\n",entries):"Your recent coin rewards appear here.";sync.interactable=!PlayerEconomy.Busy;Layout();
  }
  void Close(){if(!Visible)return;shade.SetActive(false);closedFrame=Time.frameCount;if(AppRoot.Instance&&AppRoot.Instance.view)AppRoot.Instance.view.InputBlocked=wasBlocked;}
  void OnDestroy(){Close();if(instance==this)instance=null;}
 }
 public sealed class CoinBalanceLabel:MonoBehaviour {
  public bool Result;Text label;int maximum;
  void Awake(){label=GetComponent<Text>();maximum=label?label.fontSize:25;if(label){label.resizeTextForBestFit=true;label.resizeTextMinSize=14;label.resizeTextMaxSize=maximum;}}
  void Update(){if(label)label.text=Result?CoinWalletUI.ResultSummary:CoinWalletUI.Balance;}
 }
}
