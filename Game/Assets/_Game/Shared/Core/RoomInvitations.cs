using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Scripting;
namespace WhatTheFish {
 [Serializable,Preserve] public sealed class CrewInvitation {
  public string id,sender,recipient,name,code;public int sport,protocol,avatar,avatarVersion,hasPhoto;public long expiresAt;
  public PublicProfile Profile=>new(){id=sender,name=name,code=code,avatar=avatar,avatarVersion=avatarVersion,hasPhoto=hasPhoto};
 }
 [Serializable,Preserve] public sealed class CrewInbox {public CrewInvitation[] invites=Array.Empty<CrewInvitation>();public long serverTime;}
 [Serializable,Preserve] public sealed class ResolvedInvitation {public string roomCode;public int protocol,sport;}
 public sealed class RoomInvitations:MonoBehaviour {
  [Serializable,Preserve] sealed class Request {public string id,target,roomCode,action;public int sport,protocol;}
  public static RoomInvitations Instance {get;private set;}
  public CrewInvitation[] Inbox {get;private set;}=Array.Empty<CrewInvitation>();
  public string Status {get;private set;}="";public bool Loading {get;private set;}
  public event Action Changed;
  float nextPoll,receivedAt;long serverTime;string scope="";int failures;uint generation;bool joining;
  public int Count=>Inbox.Count(p=>Remaining(p)>0);
  public static void Ensure(RoomService room){if(!room.GetComponent<RoomInvitations>())room.gameObject.AddComponent<RoomInvitations>();}
  void Awake(){Instance=this;}
  void OnDestroy(){generation++;if(Instance==this)Instance=null;}
  public float Remaining(CrewInvitation invite)=>Mathf.Max(0,invite.expiresAt-serverTime-(Time.realtimeSinceStartup-receivedAt));
  void Update(){
   string current=PlayerIdentity.ProfileStorageKey;
   if(scope!=current){scope=current;generation++;Inbox=Array.Empty<CrewInvitation>();RoomInvitationNotice.Clear();nextPoll=0;failures=0;Status="";Changed?.Invoke();}
   if(Application.isFocused&&!Loading&&Time.realtimeSinceStartup>=nextPoll&&UnityServices.State==ServicesInitializationState.Initialized&&AuthenticationService.Instance.IsSignedIn)_=Refresh();
  }
  public async Task Refresh(){
   if(Loading)return;Loading=true;uint version=generation;nextPoll=Time.realtimeSinceStartup+20;
   try{await PlayerProfiles.EnsureCloud();var result=await PlayerProfiles.Json<CrewInbox>("/v1/invites");
    if(version!=generation)return;var previous=Inbox.Select(p=>p.id).ToHashSet();Inbox=result.invites??Array.Empty<CrewInvitation>();serverTime=result.serverTime;receivedAt=Time.realtimeSinceStartup;Status="";failures=0;Changed?.Invoke();
    var fresh=Inbox.FirstOrDefault(p=>Remaining(p)>0&&!previous.Contains(p.id));if(fresh!=null)RoomInvitationNotice.Show(fresh);
   }catch(Exception e){if(version==generation){Status=Explain(e);nextPoll=Time.realtimeSinceStartup+Mathf.Min(120,20*Mathf.Pow(2,++failures));Changed?.Invoke();}}
   finally{Loading=false;}
  }
  public static bool CanInvite(out string reason){
   var room=AppRoot.Instance?AppRoot.Instance.rooms:null;reason="";
   if(!room||!room.Connected||room.LocalTest||room.Session==null){reason="Create or join an online room to invite friends.";return false;}
   if(room.busy||RoomService.Paused){reason="Wait for your room connection to be ready.";return false;}
   if(!NetworkAthlete.HostPlayer||NetworkAthlete.HostPlayer.Exploring.Value){reason="Return to the waiting room before inviting friends.";return false;}
   int count=RoomSeats.Occupants.Length+room.PendingSeats.Count();if(count>=room.Capacity+(RoomSeats.Allowed?10:0)){reason="Your room is full. Free a place before inviting friends.";return false;}
   return true;
  }
  public async Task Send(string target){
   if(!CanInvite(out var reason))throw new InvalidOperationException(reason);
   if(RoomTeleport.Players().Any(p=>p.MemberId.Value.ToString()==target))throw new InvalidOperationException("This friend is already in your room.");
   var room=AppRoot.Instance.rooms;await PlayerProfiles.Json<CrewInvitation>("/v1/invites","POST",new Request{target=target,roomCode=room.Code,sport=(int)room.Sport,protocol=RoomService.ProtocolVersion});
  }
  public async Task Dismiss(CrewInvitation invitation){await Respond(invitation,"declined");Inbox=Inbox.Where(p=>p.id!=invitation.id).ToArray();Changed?.Invoke();}
  Task<byte[]> Respond(CrewInvitation invitation,string action)=>PlayerProfiles.Request("/v1/invites/respond","POST",new Request{id=invitation.id,action=action});
  public async Task Join(CrewInvitation invitation,bool leaveCurrent,CancellationToken cancellation=default){
   if(joining)throw new InvalidOperationException("An invitation is already connecting.");
   joining=true;uint version=generation;
   try{
   cancellation.ThrowIfCancellationRequested();
   if(Remaining(invitation)<=0)throw new InvalidOperationException("This invitation expired. Ask your friend to send another.");
   var room=AppRoot.Instance.rooms;
   if(room.busy||RoomService.Paused)throw new InvalidOperationException("Wait for the current connection to finish.");
   var resolved=await PlayerProfiles.Json<ResolvedInvitation>("/v1/invites/resolve","POST",new Request{id=invitation.id});
   cancellation.ThrowIfCancellationRequested();
   if(version!=generation)throw new InvalidOperationException("Your account changed. Open invitations again to continue.");
   if(room.busy||RoomService.Paused)throw new InvalidOperationException("Wait for the current connection to finish.");
   if(resolved.protocol!=RoomService.ProtocolVersion)throw new InvalidOperationException("You and your friend need the same game build. Update both games, then try a new invitation.");
   if(!StorybookMenu.ValidCode(resolved.roomCode))throw new InvalidOperationException("This invitation is unavailable. Ask your friend for a new one.");
   if(room.Connected&&room.Code!=resolved.roomCode){if(!leaveCurrent)throw new InvalidOperationException("Leave your current room before joining your friend.");await room.Leave();}
   cancellation.ThrowIfCancellationRequested();
   if(!room.Connected)await room.Join(resolved.roomCode);
   cancellation.ThrowIfCancellationRequested();
   if(!room.Connected||!string.IsNullOrEmpty(room.Error))throw new InvalidOperationException(string.IsNullOrEmpty(room.Error)?"Could not join. Your friend may have closed the room or started playing.":RoomFeedback.Explain(room.Error));
   // Acknowledgement is best effort: a failed inbox write must never undo a successful room join.
   _=AcknowledgeJoin(invitation);
   Inbox=Inbox.Where(p=>p.id!=invitation.id).ToArray();Changed?.Invoke();
   }finally{joining=false;}
  }
  async Task AcknowledgeJoin(CrewInvitation invitation){try{await Respond(invitation,"joined");}catch(Exception){ }}
  public static string Explain(Exception error)=>error is ProfileException p?p.Code switch{
   "invite_cooldown"=>"Invitation already sent. Wait a moment before sending another.",
   "invite_limit"=>"There are too many pending invitations. Try again after some expire.",
   "invite_friends_only"=>"Only accepted friends can exchange room invitations.",
   "invite_expired"=>"This invitation expired or was withdrawn. Ask your friend for a new one.",
   "not_found"=>"Room invitations are temporarily unavailable. You can still share a room code.",
   _=>PlayerProfiles.Explain(error)
  }:error is OperationCanceledException?"Joining cancelled. You can try the invitation again while it’s valid.":error is InvalidOperationException?error.Message:PlayerProfiles.Explain(error);
 }
}
