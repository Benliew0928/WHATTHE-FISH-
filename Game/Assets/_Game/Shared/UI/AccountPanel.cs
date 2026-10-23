using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static WhatTheFish.PlayerHubStyle;

namespace WhatTheFish {
 // Reusable from the welcome screen and profile page. No provider secrets or tokens enter this UI.
 public sealed class AccountPanel:MonoBehaviour {
  static AccountPanel instance;
  RectTransform design,card;Text detail,feedback;Action completed;bool link=true,working;Button cancelSignIn;
  readonly List<Button> controls=new();
  public static bool Visible=>instance;
  public static void Open(Action onChanged=null) {
   if(instance)return;
   instance=new GameObject("Account choices").AddComponent<AccountPanel>();instance.completed=onChanged;instance.Create();
  }
  void Create() {
   var canvas=gameObject.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=31000;
   gameObject.AddComponent<GraphicRaycaster>();
   var shade=gameObject.AddComponent<Image>();shade.color=new Color(.01f,.08f,.1f,.8f);
   design=CoveUI.Rect(transform,"Account safe area",0,0,1100,760);
   card=Card(design,"Account card",0,0,1100,760).rectTransform;
   Label(card,"Your game account",45,25,1010,60,38,true);
   detail=Label(card,"",45,105,1010,140,25,false,Muted);
   controls.Add(Action(card,"Keep current profile","Keep this profile",45,265,490,65,Lime,()=>{link=true;Describe();},CoveSymbol.None,26));
   controls.Add(Action(card,"Use existing account","Use an existing account",565,265,490,65,Panel,()=>{link=false;Describe();},CoveSymbol.None,26));
   var preferred=AndroidAccountSignIn.Capabilities.Preferred;
   Provider(preferred,365);Provider(preferred==AccountProvider.Huawei?AccountProvider.Google:AccountProvider.Huawei,455);
   feedback=Label(card,"",45,550,1010,100,24,false,Ink);
   controls.Add(Action(card,"Close account choices","Back",45,675,220,60,Panel,Close,CoveSymbol.None,25));
   if(PlayerIdentity.AccountId.Length>0)
    controls.Add(Action(card,"Sign out account","Sign out",775,675,280,60,Panel,()=>{
     try{PlayerAccount.SignOut();completed?.Invoke();Close();}catch(Exception e){feedback.text=PlayerAccount.Explain(e);}
    },CoveSymbol.None,25));
   cancelSignIn=Action(card,"Cancel account sign-in","Cancel sign-in",310,675,400,60,Raised,AndroidAccountSignIn.Cancel,size:25);cancelSignIn.gameObject.SetActive(false);
   Describe();Layout();
  }
  void Describe() {
   detail.text=link
    ?"Keep this profile: connect a provider to your current game ID. If it belongs to another game account, we will ask you to switch instead."
    :"Use an existing account: load the game ID already linked to your provider. Accounts are not merged. Your current guest will remain separate.";
   if(PlayerAccount.RoomLocked)detail.text="Leave your room before linking, switching or signing out. You can still edit your player name.";
  }
  void Provider(AccountProvider provider,float y) {
   string name=provider==AccountProvider.Huawei?"HUAWEI ID":"Google";
   controls.Add(Action(card,"Sign in with "+name,"Sign in with "+name,45,y,1010,70,Raised,()=>Run(provider),CoveSymbol.None,29));
  }
  async void Run(AccountProvider provider) {
   if(working)return;working=true;foreach(var b in controls)b.interactable=false;feedback.text="Complete sign-in on your phone...";
   try{await PlayerAccount.SignIn(provider,link);if(this)feedback.text="Loading your player profile…";await PlayerProfiles.EnsureCloud();if(this){completed?.Invoke();Close();}}
   catch(Exception e){if(this)feedback.text=e is ProfileException?PlayerProfiles.Explain(e):PlayerAccount.Explain(e);}
   finally{if(this){working=false;foreach(var b in controls)b.interactable=true;}}
  }
  void Layout() {
   var safe=Screen.safeArea;float scale=Mathf.Min(safe.width/1100,safe.height/760)*.92f;
   design.localScale=Vector3.one*scale;design.anchoredPosition=new Vector2(safe.center.x-550*scale,-(Screen.height-safe.center.y-380*scale));
  }
  void Update(){Layout();cancelSignIn.gameObject.SetActive(working&&AndroidAccountSignIn.Pending);if(!working&&Input.GetKeyDown(KeyCode.Escape))Close();}
  void Close(){if(working&&PlayerAccount.Busy)return;Destroy(gameObject);}
  void OnDestroy(){if(instance==this)instance=null;}
 }
}
