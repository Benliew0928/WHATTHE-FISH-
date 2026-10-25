using System;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static WhatTheFish.PlayerHubStyle;
namespace WhatTheFish {
 public sealed partial class SocialPanel {
  string inboxKey="";float invitationTick;RectTransform inviteConfirmation,inviteConnecting;CancellationTokenSource inviteCancellation;Text inviteConnectionStatus;
  Selectable[] confirmationControls;bool[] confirmationStates;GameObject invitationPreviousFocus;
  void SendInvitation(PublicProfile friend)=>Run("Inviting "+friend.name+"…",async()=>{
   try{await RoomInvitations.Instance.Send(friend.id);feedback.text="Invitation sent to "+friend.name+" · valid for 2 minutes.";}
   catch(Exception error){feedback.text=RoomInvitations.Explain(error);}
  });
  void Invitations(){
   title.text="ROOM INVITATIONS";subtitle.text="Your friends. One room. The next adventure.";subtitle.gameObject.SetActive(true);
   Label(body,"JOIN YOUR CREW",0,0,860,47,30,true,Cyan);
   Action(body,"Refresh room invitations","Refresh",1030,0,220,60,Raised,async()=>{await RoomInvitations.Instance.Refresh();if(this&&page=="invites")DrawInvitations();},size:25);
   ListArea(83,547);DrawInvitations();_=RoomInvitations.Instance.Refresh();
  }
  void DrawInvitations(){
   if(page!="invites"||!list||working)return;ClearList();var service=RoomInvitations.Instance;
   var invitations=service.Inbox.Where(p=>service.Remaining(p)>0).ToArray();inboxKey=string.Join("|",invitations.Select(p=>p.id));
   if(invitations.Length==0){
    var empty=Card(list,"No room invitations",0,0,1250,400);
    ProfileAvatar.Add(empty.transform,new PublicProfile{avatar=8},32,17,234,true);
    Label(empty.transform,"YOUR NEXT ADVENTURE AWAITS",310,80,890,58,36,true);
    Label(empty.transform,"Friends can invite you from their waiting room.\nNew invitations will appear here while you’re online.",312,155,850,100,24,false,Muted);
    Action(empty.transform,"Back to friends","Find your crew",312,286,320,66,Lime,()=>Show("friends"),CoveSymbol.People,26);list.sizeDelta=new Vector2(1250,400);
   }
   for(int i=0;i<invitations.Length;i++){
    var invite=invitations[i];float y=i*184;bool compatible=invite.protocol==RoomService.ProtocolVersion;
    var row=Card(list,"Room invitation "+invite.id,0,y,1250,168);ProfileAvatar.Add(row.transform,invite.Profile,20,20,100);
    var name=Label(row.transform,invite.name+" invited you",142,15,760,44,31,true);name.resizeTextForBestFit=true;name.resizeTextMinSize=23;name.resizeTextMaxSize=31;
    Label(row.transform,((SportId)invite.sport)+" · Waiting room",144,62,670,35,23,false,Cyan);
    var expires=Label(row.transform,compatible?"":"Different game build · update to join",144,110,710,32,21,false,Muted);expires.name="Invitation expiry "+invite.id;
    var join=Action(row.transform,"Join invitation "+invite.id,"Join room",944,19,284,62,Lime,()=>ChooseInvitation(invite),CoveSymbol.Join,26);join.interactable=compatible;
    Action(row.transform,"Dismiss invitation "+invite.id,"Dismiss",944,97,284,50,Raised,()=>Run("Dismissing invitation…",async()=>{await service.Dismiss(invite);ShowAfter("invites");}),size:23);
   }
   if(invitations.Length>0)list.sizeDelta=new Vector2(1250,invitations.Length*184);
   feedback.text=service.Status.Length>0?service.Status:"Invitations expire after 2 minutes. Your friend’s room must still have an open place.";
  }
  void UpdateInvitations(){
   if(inviteConnecting&&inviteConnectionStatus){var room=AppRoot.Instance.rooms;inviteConnectionStatus.text=inviteCancellation.IsCancellationRequested?"Cancelling your connection…":string.IsNullOrEmpty(room.Operation)?"Checking your invitation and preparing your place…":room.Operation;}
   if(page!="invites"||working||Time.unscaledTime<invitationTick)return;invitationTick=Time.unscaledTime+.25f;
   var service=RoomInvitations.Instance;if(!service)return;
   string key=string.Join("|",service.Inbox.Where(p=>service.Remaining(p)>0).Select(p=>p.id));if(key!=inboxKey)DrawInvitations();
   foreach(var invite in service.Inbox){var label=list.GetComponentsInChildren<Text>().FirstOrDefault(t=>t.name=="Invitation expiry "+invite.id);if(label&&invite.protocol==RoomService.ProtocolVersion)label.text="Expires in "+Mathf.CeilToInt(service.Remaining(invite))+"s · tap Join room when you’re ready";}
  }
  void ChooseInvitation(CrewInvitation invite){
   var room=AppRoot.Instance.rooms;
   if(!room.Connected){JoinInvitation(invite,false);return;}
   if(inviteConfirmation)return;
   confirmationControls=design.GetComponentsInChildren<Selectable>();confirmationStates=confirmationControls.Select(c=>c.interactable).ToArray();foreach(var control in confirmationControls)control.interactable=false;
   invitationPreviousFocus=EventSystem.current?EventSystem.current.currentSelectedGameObject:null;
   inviteConfirmation=CoveUI.Rect(design,"Leave room for invitation",0,0,1600,900);inviteConfirmation.gameObject.AddComponent<Image>().color=new Color(.02f,.07f,.1f,.88f);
   var card=Card(inviteConfirmation,"Confirm new crew",360,210,880,480);
   Label(card.transform,"A NEW ADVENTURE",32,30,816,38,24,true,Cyan);
   var heading=Label(card.transform,"Join "+invite.name+"?",30,90,820,76,42,true);heading.resizeTextForBestFit=true;heading.resizeTextMinSize=28;heading.resizeTextMaxSize=42;
   Label(card.transform,room.Host?(RoomTeleport.Players().Length>1?"You’ll leave your current room. Your crew will choose a new host before you join your friend.":"Your current room will close before you join your friend. Their room may have filled since this invitation arrived."):"You’ll leave your current room and your place in its game. Your friend’s room may have filled since the invitation arrived.",34,185,810,100,25,false,Muted);
   var stay=Action(card.transform,"Stay in current room","Stay here",34,352,340,70,Raised,DismissInvitationConfirmation,size:27);
   Action(card.transform,"Confirm invitation join","Leave & join",394,352,450,70,Lime,()=>JoinInvitation(invite,true),CoveSymbol.Join,28);
   if(EventSystem.current)EventSystem.current.SetSelectedGameObject(stay.gameObject);
  }
  void DismissInvitationConfirmation(){
   if(inviteConfirmation){inviteConfirmation.gameObject.SetActive(false);Destroy(inviteConfirmation.gameObject);inviteConfirmation=null;}
   if(confirmationControls!=null){for(int i=0;i<confirmationControls.Length;i++)if(confirmationControls[i])confirmationControls[i].interactable=confirmationStates[i];confirmationControls=null;confirmationStates=null;}
   if(EventSystem.current&&invitationPreviousFocus)EventSystem.current.SetSelectedGameObject(invitationPreviousFocus);invitationPreviousFocus=null;
  }
  void JoinInvitation(CrewInvitation invite,bool leaveCurrent){DismissInvitationConfirmation();Run("Connecting to "+invite.name+"’s room…",async()=>{
   inviteCancellation=new CancellationTokenSource();
   inviteConnecting=CoveUI.Rect(design,"Invitation connection",0,0,1600,900);inviteConnecting.gameObject.AddComponent<Image>().color=new Color(.02f,.07f,.1f,.9f);
   var card=Card(inviteConnecting,"Joining your crew",360,245,880,410);
   Label(card.transform,"YOUR CREW IS WAITING",34,30,812,38,24,true,Cyan);
   Label(card.transform,"Joining your friend",32,94,816,64,44,true);
   inviteConnectionStatus=Label(card.transform,"Checking your invitation…",36,184,808,86,26,false,Muted);
   var cancel=Action(card.transform,"Cancel invitation connection","Cancel",34,306,812,64,Raised,CancelInvitationConnection,CoveSymbol.Back,27);
   if(EventSystem.current)EventSystem.current.SetSelectedGameObject(cancel.gameObject);
   try{await RoomInvitations.Instance.Join(invite,leaveCurrent,inviteCancellation.Token);working=false;Close();}
   catch(Exception error){feedback.text=RoomInvitations.Explain(error);}
   finally{if(inviteConnecting)Destroy(inviteConnecting.gameObject);inviteConnecting=null;inviteCancellation.Dispose();inviteCancellation=null;}
  });}
  void CancelInvitationConnection(){if(inviteCancellation==null||inviteCancellation.IsCancellationRequested)return;inviteCancellation.Cancel();AppRoot.Instance.rooms.CancelConnection();}
  bool InvitationBack(){
   if(inviteCancellation!=null){CancelInvitationConnection();return true;}
   if(!inviteConfirmation)return false;DismissInvitationConfirmation();return true;
  }
 }
 public sealed class InvitationBadge:MonoBehaviour {
  CovePlate badge;Text text;
  public static void Add(Transform parent,float x,float y){var r=CoveUI.Rect(parent,"Room invitation count",x,y,36,36);var component=r.gameObject.AddComponent<InvitationBadge>();component.badge=Card(r,"Invitation badge",0,0,36,36,Cyan,18);component.text=Label(component.badge.transform,"",0,0,36,36,19,true,Dark,TextAnchor.MiddleCenter);}
  void Update(){int count=RoomInvitations.Instance?RoomInvitations.Instance.Count:0;badge.gameObject.SetActive(count>0);text.text=count.ToString();}
 }
}
