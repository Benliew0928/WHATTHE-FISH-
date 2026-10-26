using System;
using UnityEngine;
using UnityEngine.UI;
using static WhatTheFish.PlayerHubStyle;

namespace WhatTheFish {
 public sealed class PrivacyDataPanel:MonoBehaviour {
  static PrivacyDataPanel instance;
  RectTransform design,card;Text body,status;Button remove,back,policy;InputField confirmation;
  bool working,confirming;Action changed;
  public static bool Visible=>instance;
  public static void Open(Action onChanged=null){
   if(instance)return;instance=new GameObject("Privacy and data").AddComponent<PrivacyDataPanel>();instance.changed=onChanged;instance.Create();
  }
  void Create(){
   var canvas=gameObject.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=31100;
   gameObject.AddComponent<GraphicRaycaster>();gameObject.AddComponent<Image>().color=new Color(.01f,.08f,.1f,.88f);
   design=CoveUI.Rect(transform,"Privacy safe area",0,0,1100,760);card=Card(design,"Privacy card",0,0,1100,760).rectTransform;
   Label(card,"Privacy & data",45,25,1010,60,38,true);
   body=Label(card,"",45,110,1010,365,25,false,Ink);body.alignment=TextAnchor.UpperLeft;
   status=Label(card,"",45,500,1010,145,21,false,Muted);status.alignment=TextAnchor.UpperLeft;
   confirmation=CoveUI.Rect(card,"Deletion confirmation",45,430,1010,60).gameObject.AddComponent<InputField>();
   var bg=confirmation.gameObject.AddComponent<Image>();bg.color=Background;confirmation.targetGraphic=bg;
   confirmation.textComponent=Label(confirmation.transform,"",15,4,980,52,28,false,Ink);confirmation.characterLimit=6;GameLocalization.UserContent(confirmation.textComponent);
   confirmation.placeholder=Label(confirmation.transform,"Type DELETE to confirm",15,4,980,52,25,false,Muted);
   back=Action(card,"Close privacy","Back",45,675,220,60,Panel,Back,size:25);
   policy=Action(card,"Read published privacy policy","Privacy policy",295,675,340,60,Panel,OpenPolicy,size:25);
   remove=Action(card,"Delete game account","Delete game account",665,675,390,60,Raised,Confirm,size:25);
   Overview();Layout();
  }
  string PolicyUrl {
   get=>PublicInfoDocuments.UrlFor("privacy");
  }
  void Overview(){
   body.rectTransform.sizeDelta=new Vector2(1010,365);body.fontSize=23;
   confirming=false;confirmation.gameObject.SetActive(false);
   body.text="WHATTHE FISH? is free to download, with no ads or in-app purchases.\n\n"+
    "Accounts and online play use Huawei or Google sign-in, Unity services and our Cloudflare profile service. Other players can see your name and chosen avatar or profile photo.\n\n"+
    "Camera access is optional for room QR scanning. Selected QR images use the system picker. Voice chat is optional: join, allow microphone access and unmute to speak with your room. You can leave voice or revoke permissions in device settings.";
   status.text=PlayerAccount.DeletionPending?"An earlier deletion is pending. Retry to finish it; online play is blocked until it completes.":
    "You can manage friends, block players and report names, photos or behaviour. Sign-out does not delete your game account. "+
    (PolicyUrl.Length==0?"The publisher's full privacy policy is awaiting release details.":"Read the published policy for contact, retention and service information.");
   policy.interactable=PolicyUrl.Length>0;
   remove.GetComponentInChildren<Text>().text=PlayerAccount.DeletionPending?"Retry account deletion":"Delete game account";
  }
  void Confirm(){
   if(working)return;
   if(PlayerAccount.RoomLocked){status.text="Leave your room before deleting your account.";return;}
   if(confirming){if(confirmation.text!="DELETE"){status.text="Type DELETE exactly to confirm.";return;}Delete();return;}
   confirming=true;body.rectTransform.sizeDelta=new Vector2(1010,305);body.fontSize=23;
   body.text="Delete the game profile for "+PlayerIdentity.DisplayName+"?\n\n"+
    "Permanently remove your cloud profile/photo, social data and game sign-in account, plus this device's active profile and control layouts. An offline-only guest is removed from this device.\n\n"+
    "Your Huawei ID or Google account will NOT be deleted. Service security records and backups are separate. Linked accounts require same-provider verification. This cannot be undone.";
   status.text="If a network error interrupts deletion, keep this installation and retry here. We will not claim success until each step is confirmed.";
   policy.interactable=false;confirmation.text="";confirmation.gameObject.SetActive(true);confirmation.Select();
   remove.GetComponentInChildren<Text>().text="Permanently delete";
  }
  async void Delete(){
   working=true;remove.interactable=back.interactable=confirmation.interactable=false;
   try{await PlayerAccount.DeleteGameAccount();if(!this)return;body.text=PlayerAccount.DeletionMessage;status.text="You can return and create a new guest profile.";confirmation.gameObject.SetActive(false);remove.gameObject.SetActive(false);confirming=false;changed?.Invoke();}
   catch(Exception e){if(this)status.text=e is InvalidOperationException?e.Message:PlayerAccount.Explain(e);}
   finally{if(this){working=false;remove.interactable=back.interactable=confirmation.interactable=true;}}
  }
  void OpenPolicy(){string url=PolicyUrl;if(url.Length>0)Application.OpenURL(url);}
  void Back(){if(working)return;if(confirming){Overview();return;}Destroy(gameObject);}
  void Layout(){var safe=Screen.safeArea;float scale=Mathf.Min(safe.width/1100,safe.height/760)*.92f;design.localScale=Vector3.one*scale;design.anchoredPosition=new Vector2(safe.center.x-550*scale,-(Screen.height-safe.center.y-380*scale));}
  void Update(){Layout();if(working)status.text=PlayerAccount.DeletionMessage;else if(Input.GetKeyDown(KeyCode.Escape))Back();}
  void OnDestroy(){if(instance==this)instance=null;}
 }
}
