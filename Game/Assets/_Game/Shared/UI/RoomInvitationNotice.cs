using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static WhatTheFish.PlayerHubStyle;
namespace WhatTheFish {
 // A brief, non-modal notification. It never accepts an invitation or leaves a room.
 public sealed class RoomInvitationNotice:MonoBehaviour {
  static RoomInvitationNotice instance;CrewInvitation invitation;RectTransform card;float shown;
  public static void Clear(){if(instance)Destroy(instance.gameObject);instance=null;}
  public static void Show(CrewInvitation invite){Clear();instance=new GameObject("Friend room invitation",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster)).AddComponent<RoomInvitationNotice>();instance.invitation=invite;instance.Build();}
  void Build(){
   WorldUI.Canvas(gameObject,28000);card=CoveUI.Rect(transform,"Invitation notice safe area",0,0,760,110);
   Card(card,"Invitation notice",0,0,760,110,Panel,18);CoveUI.Icon(card,CoveSymbol.People,22,31,46,Cyan);
   var name=Label(card,invitation.name+" invited you",86,12,420,43,28,true);name.resizeTextForBestFit=true;name.resizeTextMinSize=20;name.resizeTextMaxSize=28;
   Label(card,"A place with your crew · "+(SportId)invitation.sport,88,61,420,30,20,false,Muted);
   Action(card,"View new room invitation","View invite",530,23,208,64,Lime,()=>{SocialPanel.OpenInvitations();Clear();},size:24);
  }
  void Update(){
   var service=RoomInvitations.Instance;
   if(!service||service.Remaining(invitation)<=0||!service.Inbox.Any(p=>p.id==invitation.id)||shown>=9){Clear();return;}
   bool visible=!StartupWelcome.Blocking&&!SocialPanel.Visible&&!AccountPanel.Visible&&!RoomService.Paused&&!RoomMatchStart.Travelling;
   card.gameObject.SetActive(visible);if(!visible)return;shown+=Time.unscaledDeltaTime;
   var root=(RectTransform)transform;var safe=Screen.safeArea;float width=safe.width/Screen.width*root.rect.width,height=safe.height/Screen.height*root.rect.height;
   float scale=Mathf.Min(1,(width-32)/760);card.localScale=Vector3.one*scale;
   card.anchorMin=card.anchorMax=new Vector2(.5f,1);card.pivot=new Vector2(.5f,1);
   card.anchoredPosition=new Vector2((safe.center.x/Screen.width-.5f)*root.rect.width,-(Screen.height-safe.yMax)/Screen.height*root.rect.height-Mathf.Min(150,height*.16f));
  }
 }
}
