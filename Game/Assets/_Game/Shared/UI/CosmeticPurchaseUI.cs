using System;
using UnityEngine;
using UnityEngine.UI;
using static WhatTheFish.PlayerHubStyle;

namespace WhatTheFish {
 public sealed class CosmeticLock:MonoBehaviour {
  public CosmeticItem Item;GameObject overlay;
  public static void Bind(Button button,string slot,int index,Action select){
   var item=CosmeticStore.Item(slot,index);if(item==null)return;
   button.onClick.RemoveAllListeners();button.onClick.AddListener(()=>{if(CosmeticStore.Owned(item))select();else CosmeticPurchaseUI.Open(item,()=>{if(button)select();});});
   var gate=button.gameObject.AddComponent<CosmeticLock>();gate.Item=item;
   var shade=CoveUI.Rect(button.transform,"Locked cosmetic",0,0,1,1);CoveUI.Stretch(shade);gate.overlay=shade.gameObject;
   var tint=shade.gameObject.AddComponent<Image>();tint.color=new Color(.16f,.18f,.20f,.70f);tint.raycastTarget=false;
   var icon=CoveUI.Rect(shade,"Lock icon",0,0,30,30);icon.anchorMin=icon.anchorMax=icon.pivot=new Vector2(1,1);icon.anchoredPosition=new Vector2(-7,-7);
   icon.gameObject.AddComponent<CosmeticLockIcon>().raycastTarget=false;gate.Refresh();
  }
  void OnEnable(){PlayerEconomy.Changed+=Refresh;Refresh();}
  void OnDisable()=>PlayerEconomy.Changed-=Refresh;
  void Refresh(){if(overlay)overlay.SetActive(!CosmeticStore.Owned(Item));}
 }
 public sealed class CosmeticLockIcon:MaskableGraphic {
  protected override void OnPopulateMesh(VertexHelper vh){
   vh.Clear();var r=rectTransform.rect;
   void Quad(float x,float y,float w,float h){int i=vh.currentVertCount;var c=new Color32(235,239,242,255);vh.AddVert(new Vector3(r.x+x*r.width,r.y+y*r.height),c,Vector2.zero);vh.AddVert(new Vector3(r.x+x*r.width,r.y+(y+h)*r.height),c,Vector2.zero);vh.AddVert(new Vector3(r.x+(x+w)*r.width,r.y+(y+h)*r.height),c,Vector2.zero);vh.AddVert(new Vector3(r.x+(x+w)*r.width,r.y+y*r.height),c,Vector2.zero);vh.AddTriangle(i,i+1,i+2);vh.AddTriangle(i,i+2,i+3);}
   Quad(.16f,.1f,.68f,.48f);Quad(.26f,.5f,.13f,.32f);Quad(.61f,.5f,.13f,.32f);Quad(.32f,.79f,.36f,.13f);
  }
 }
 public sealed class CosmeticPurchaseUI:MonoBehaviour {
  static CosmeticPurchaseUI instance;static int closedFrame=-1;public static bool Visible=>instance;public static bool BlocksBack=>Visible||closedFrame==Time.frameCount;
  RectTransform card;Text status,balance;Button buy,back;CosmeticItem item;Action selected;string scope;bool working,wasBlocked;
  public static void Open(CosmeticItem item,Action selected){
   if(instance)return;var go=new GameObject("Cosmetic purchase",typeof(RectTransform),typeof(Canvas),typeof(GraphicRaycaster));
   instance=go.AddComponent<CosmeticPurchaseUI>();instance.item=item;instance.selected=selected;instance.scope=PlayerEconomy.Scope;instance.Build();PlayerEconomy.RefreshSoon();
  }
  void Build(){
   var canvas=GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=32600;
   gameObject.AddComponent<Image>().color=new Color(.015f,.045f,.07f,.92f);
   card=CoveUI.Rect(transform,"Purchase safe area",0,0,860,520);Card(card,"Purchase card",0,0,860,520);
   Label(card,"Unlock cosmetic",36,24,788,52,36,true);
   Label(card,item.name,36,88,788,58,30,true,Cyan);
   Label(card,"Permanent unlock for this account.",36,156,788,44,24,false,Muted);
   Label(card,"Price: 1,000 coins",36,212,788,45,30,true,Lime);
   balance=Label(card,"",36,265,788,38,25,true);
   status=Label(card,"",36,316,788,82,23,false,Muted);
   back=Action(card,"Cancel cosmetic purchase","Back",36,426,260,58,Raised,Close,CoveSymbol.Back,25);
   buy=Action(card,"Buy cosmetic","Buy for 1,000 coins",326,426,498,58,Lime,Buy,CoveSymbol.Star,26);
   wasBlocked=AppRoot.Instance&&AppRoot.Instance.view&&AppRoot.Instance.view.ExplicitInputBlocked;
   if(AppRoot.Instance&&AppRoot.Instance.view){AppRoot.Instance.view.ClearMatchInput();AppRoot.Instance.view.InputBlocked=true;}
   status.text="Choose Buy to unlock this item.";Layout();
  }
  void Layout(){var safe=Screen.safeArea;float scale=Mathf.Min(safe.width/860,safe.height/520)*.92f;card.localScale=Vector3.one*scale;card.anchoredPosition=new Vector2(safe.center.x-430*scale,-(Screen.height-safe.center.y-260*scale));}
  void Update(){
   Layout();if(scope!=PlayerEconomy.Scope){Close();return;}
   if(AppRoot.Instance&&AppRoot.Instance.view)AppRoot.Instance.view.InputBlocked=true;
   balance.text=CoinWalletUI.Balance;buy.interactable=!working&&!PlayerEconomy.Busy&&PlayerEconomy.Enabled&&PlayerEconomy.Confirmed&&(PlayerEconomy.Wallet?.balance??0)>=item.price;back.interactable=!working;
   if(!working&&!PlayerEconomy.Confirmed)status.text="Connect to sync your coins.";
   else if(!working&&(PlayerEconomy.Wallet?.balance??0)<item.price)status.text="Not enough coins. Earn more by signing in and playing matches.";
   if(!working&&Input.GetKeyDown(KeyCode.Escape))Close();
  }
  async void Buy(){
   if(working||!buy.interactable)return;working=true;status.text="Saving your purchase…";
   string message=await CosmeticStore.Purchase(item);if(!this)return;working=false;status.text=message;
   if(scope==PlayerEconomy.Scope&&CosmeticStore.Owned(item)){var action=selected;Close();action?.Invoke();}
  }
  void Close(){if(!this)return;if(AppRoot.Instance&&AppRoot.Instance.view)AppRoot.Instance.view.InputBlocked=wasBlocked;closedFrame=Time.frameCount;instance=null;Destroy(gameObject);}
  void OnDestroy(){if(instance==this)instance=null;}
 }
}
