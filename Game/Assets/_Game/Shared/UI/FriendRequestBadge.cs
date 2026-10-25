using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 // The component remains active at zero so incoming requests can reveal the circle.
 public sealed class FriendRequestBadge:MonoBehaviour {
  CovePlate circle;Text label;bool includeInvitations;
  public static void Add(Transform parent,float x,float y,float diameter=36,bool includeInvitations=true){
   var root=CoveUI.Rect(parent,"Pending friend requests",x,y,diameter,diameter);
   var badge=root.gameObject.AddComponent<FriendRequestBadge>();
   badge.includeInvitations=includeInvitations;
   badge.circle=CoveUI.Plate(root,"Request count circle",0,0,diameter,diameter,CoveUI.Hex("DF344B"),diameter/2,0);
   badge.label=PlayerHubStyle.Label(badge.circle.transform,"",0,0,diameter,diameter,20,true,Color.white,TextAnchor.MiddleCenter);
   badge.Refresh();
  }
  void Update(){PlayerProfiles.PollFriendRequests();Refresh();}
  void Refresh(){int count=PlayerProfiles.PendingRequestCount+(includeInvitations&&RoomInvitations.Instance?RoomInvitations.Instance.Count:0);circle.gameObject.SetActive(count>0);label.text=count.ToString();label.fontSize=count>99?16:20;}
 }
}
