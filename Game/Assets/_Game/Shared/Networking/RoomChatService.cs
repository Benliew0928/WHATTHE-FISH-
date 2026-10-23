using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace WhatTheFish {
 [Serializable] public sealed class RoomChatMessage {
  public string id,member,name,text,sticker,context;
  public long time,order;
 }
 // Lives with the room, not a scene or player object. Every peer retains accepted
 // messages across NGO restarts; the elected host serves its retained timeline.
 public sealed class RoomChatService:MonoBehaviour {
  public const int TextLimit=120,PageSize=32;
  public const float SendInterval=1.2f;
  const string Submit="wtf.chat.submit",Fetch="wtf.chat.fetch",Deliver="wtf.chat.deliver";
  public static readonly string[] Stickers={"ggwp","laugh","cry","angry","thanks","oops","wow","lets-go"};
  public static readonly string[] StickerNames={"GGWP","Laugh","Cry","Angry","Thanks","Oops","Wow","Let's go!"};
  [Serializable] sealed class Packet {
   public string request,room,text,sticker,error;
   public int cursor,next,total;
   public bool snapshot;
   public RoomChatMessage[] messages;
  }
  public IReadOnlyList<RoomChatMessage> History=>history;
  public event Action<RoomChatMessage,bool> Received;
  public event Action Changed;
  public string Status {get;private set;}="Connecting chat…";
  public string RoomId {get;private set;}="";
  public bool Synced {get;private set;}
  public bool HistoryInterrupted {get;private set;}
  public bool Ready=>rooms&&rooms.Connected&&!rooms.Recovering&&bound!=null&&Synced&&NetworkManager.Singleton.IsConnectedClient;
  public float Cooldown=>Mathf.Max(0,nextSend-Time.realtimeSinceStartup);
  public bool HideBubbles {get;set;}
  readonly List<RoomChatMessage> history=new();
  readonly Dictionary<string,RoomChatMessage> byId=new(StringComparer.Ordinal);
  readonly HashSet<string> muted=new(StringComparer.Ordinal);
  readonly Dictionary<ulong,float> lastSend=new(),lastFetch=new();
  readonly List<RoomChatMessage> pageBuffer=new();
  RoomService rooms;CustomMessagingManager bound;
  float nextSend,nextSync,pendingDeadline;int cursor;long order;
  string pending;bool hadConnection,wasRecovering;
  public static RoomChatService Ensure(RoomService room){var chat=room.GetComponent<RoomChatService>();if(!chat)chat=room.gameObject.AddComponent<RoomChatService>();chat.rooms=room;return chat;}
  public bool IsMuted(string member)=>muted.Contains(member??"");
  public void ToggleMute(string member){if(member==rooms.MemberId)return;if(!muted.Add(member))muted.Remove(member);Changed?.Invoke();}
  public void ResetRoom(){
   Unbind();history.Clear();byId.Clear();muted.Clear();lastSend.Clear();lastFetch.Clear();
   RoomId="";Synced=false;HistoryInterrupted=false;hadConnection=false;wasRecovering=false;cursor=0;order=0;pending=null;nextSend=0;
   Status="Connecting chat…";Changed?.Invoke();
  }
  void Update(){
   var n=NetworkManager.Singleton;
   if(!rooms||!n)return;
   if(rooms.Recovering){wasRecovering=true;Synced=false;Status="Reconnecting room…";}
   if(!n.IsListening||n.ShutdownInProgress||n.CustomMessagingManager==null){Unbind();return;}
   if(bound!=n.CustomMessagingManager){
    Unbind();bound=n.CustomMessagingManager;
    bound.RegisterNamedMessageHandler(Submit,OnSubmit);bound.RegisterNamedMessageHandler(Fetch,OnFetch);bound.RegisterNamedMessageHandler(Deliver,OnDeliver);
    cursor=0;nextSync=0;lastSend.Clear();lastFetch.Clear();Synced=false;
    if(hadConnection){HistoryInterrupted=true;Status="Reconnected · history may be incomplete";}
    hadConnection=true;
   }
   if(rooms.Recovering||!n.IsConnectedClient)return;
   if(wasRecovering){wasRecovering=false;HistoryInterrupted=true;Synced=false;cursor=0;nextSync=0;Changed?.Invoke();}
   if(n.IsServer){if(RoomId.Length==0)RoomId=Guid.NewGuid().ToString("N");Synced=true;}
   else if(!Synced&&Time.realtimeSinceStartup>=nextSync){nextSync=Time.realtimeSinceStartup+1;Send(Fetch,NetworkManager.ServerClientId,new Packet{cursor=cursor});}
   if(pending!=null&&Time.realtimeSinceStartup>pendingDeadline){pending=null;Status="Message not confirmed. Try sending again.";Changed?.Invoke();}
   if(Synced&&pending==null&&(Status=="Connecting chat…"||Status=="Reconnecting room…"))Status=HistoryInterrupted?"Reconnected · history may be incomplete":"One room · all islands";
  }
  void OnDestroy(){Unbind();}
  void Unbind(){if(bound==null)return;bound.UnregisterNamedMessageHandler(Submit);bound.UnregisterNamedMessageHandler(Fetch);bound.UnregisterNamedMessageHandler(Deliver);bound=null;Synced=false;}
  public static string CleanText(string value){
   if(string.IsNullOrWhiteSpace(value))return "";
   var clean=new StringBuilder();
   foreach(char c in value){if(c=='\r'||c=='\n'||c=='\t')clean.Append(' ');else if(!char.IsControl(c)&&c!='\u202a'&&c!='\u202b'&&c!='\u202c'&&c!='\u202d'&&c!='\u202e'&&c!='\u2066'&&c!='\u2067'&&c!='\u2068'&&c!='\u2069')clean.Append(c);}
   string result=clean.ToString().Trim();var indices=StringInfo.ParseCombiningCharacters(result);
   // Bound bytes as well as visible characters, including combining sequences.
   // This keeps every 32-message history page below the receive budget.
   int end=0,bytes=0;for(int i=0;i<Math.Min(indices.Length,TextLimit);i++){int next=i+1<indices.Length?indices[i+1]:result.Length;int count=Encoding.UTF8.GetByteCount(result.Substring(indices[i],next-indices[i]));if(bytes+count>480)break;bytes+=count;end=next;}
   return result.Substring(0,end);
  }
  public bool SendText(string text)=>SubmitLocal(CleanText(text),"");
  public bool SendSticker(string sticker)=>Array.IndexOf(Stickers,sticker)>=0&&SubmitLocal("",sticker);
  bool SubmitLocal(string text,string sticker){
   if(!Ready){Status="Wait for the room to reconnect.";Changed?.Invoke();return false;}
   if(Cooldown>0||pending!=null){Status="Give your crew a moment…";Changed?.Invoke();return false;}
   if(text.Length==0&&sticker.Length==0)return false;
   pending=Guid.NewGuid().ToString("N");pendingDeadline=Time.realtimeSinceStartup+8;nextSend=Time.realtimeSinceStartup+SendInterval;
   var packet=new Packet{request=pending,text=text,sticker=sticker};
   if(NetworkManager.Singleton.IsServer)Accept(NetworkManager.Singleton.LocalClientId,packet);else Send(Submit,NetworkManager.ServerClientId,packet);
   return true;
  }
  void OnSubmit(ulong sender,FastBufferReader reader){if(!NetworkManager.Singleton.IsServer)return;var p=Read(reader,2048);if(p!=null)Accept(sender,p);}
  void Accept(ulong sender,Packet p){
   var actor=RoomTeleport.Players().FirstOrDefault(a=>a.OwnerClientId==sender);
   if(!actor||rooms.Recovering||string.IsNullOrEmpty(p.request)||!Guid.TryParseExact(p.request,"N",out _)||p.text?.Length>1024)return;
   string member=actor.MemberId.Value.ToString(),id=member+":"+p.request;
   if(byId.TryGetValue(id,out var duplicate)){Reply(sender,new Packet{room=RoomId,request=p.request,messages=new[]{duplicate}});return;}
   float now=Time.realtimeSinceStartup;
   if(lastSend.TryGetValue(sender,out var last)&&now-last<SendInterval-.03f){Reply(sender,new Packet{request=p.request,error="Give your crew a moment…"});return;}
   string text=CleanText(p.text),sticker=p.sticker??"";
   if(sticker.Length>0&&(Array.IndexOf(Stickers,sticker)<0||text.Length>0)||sticker.Length==0&&text.Length==0){Reply(sender,new Packet{request=p.request,error="Choose a sticker or type a message."});return;}
   lastSend[sender]=now;
   var host=NetworkAthlete.HostPlayer;
   long timestamp=Math.Max(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),history.Count>0?history[history.Count-1].time+1:0);
   var message=new RoomChatMessage{id=id,member=member,name=actor.Nickname.Value.ToString(),text=text,sticker=sticker,time=timestamp,order=++order,context=SkySailWorld.Instance&&SkySailWorld.Instance.Travelling?"Travelling":host&&host.Exploring.Value?host.WorldSport.Value.ToString():"Room"};
   Add(message,true);var packet=new Packet{room=RoomId,request=p.request,messages=new[]{message}};
   foreach(ulong client in NetworkManager.Singleton.ConnectedClientsIds)Reply(client,packet);
  }
  void OnFetch(ulong sender,FastBufferReader reader){
   if(!NetworkManager.Singleton.IsServer||rooms.Recovering||!RoomTeleport.Players().Any(p=>p.OwnerClientId==sender))return;
   float now=Time.realtimeSinceStartup;if(lastFetch.TryGetValue(sender,out float last)&&now-last<.15f)return;
   lastFetch[sender]=now;var p=Read(reader,512);if(p==null||p.cursor<0||p.cursor>history.Count)return;
   pageBuffer.Clear();for(int i=p.cursor;i<Math.Min(history.Count,p.cursor+PageSize);i++)pageBuffer.Add(history[i]);
   Reply(sender,new Packet{room=RoomId,snapshot=true,cursor=p.cursor,next=p.cursor+pageBuffer.Count,total=history.Count,messages=pageBuffer.ToArray()});
  }
  void OnDeliver(ulong sender,FastBufferReader reader){if(sender!=NetworkManager.ServerClientId||NetworkManager.Singleton.IsServer)return;var p=Read(reader,32768);if(p!=null)Receive(p);}
  void Reply(ulong client,Packet p){if(client==NetworkManager.Singleton.LocalClientId)Receive(p);else Send(Deliver,client,p);}
  void Receive(Packet p){
   if(p.request==pending){pending=null;Status=!string.IsNullOrEmpty(p.error)?p.error:(HistoryInterrupted?"Reconnected · history may be incomplete":"One room · all islands");}
   if(!string.IsNullOrEmpty(p.room)){
    if(RoomId.Length>0&&RoomId!=p.room){history.Clear();byId.Clear();cursor=0;HistoryInterrupted=true;}
    RoomId=p.room;
   }
   if(p.messages!=null)foreach(var m in p.messages)Add(m,!p.snapshot);
   if(p.snapshot){cursor=p.next;Synced=cursor>=p.total;nextSync=Time.realtimeSinceStartup+.2f;history.Sort((a,b)=>{int c=a.time.CompareTo(b.time);return c!=0?c:a.order.CompareTo(b.order);});}
   Changed?.Invoke();
  }
  void Add(RoomChatMessage message,bool live){
   if(message==null||string.IsNullOrEmpty(message.id)||byId.ContainsKey(message.id))return;
   byId.Add(message.id,message);history.Add(message);order=Math.Max(order,message.order);Received?.Invoke(message,live);Changed?.Invoke();
  }
  void Send(string channel,ulong client,Packet p){
   if(bound==null)return;byte[] data=Encoding.UTF8.GetBytes(JsonUtility.ToJson(p));
   using var writer=new FastBufferWriter(data.Length+4,Allocator.Temp);writer.WriteValueSafe(data.Length);writer.WriteBytesSafe(data);
   bound.SendNamedMessage(channel,client,writer,NetworkDelivery.ReliableFragmentedSequenced);
  }
  static Packet Read(FastBufferReader reader,int limit){
   try{if(reader.Length<4||reader.Length>limit+4)return null;reader.ReadValueSafe(out int length);if(length<2||length>limit||length!=reader.Length-reader.Position)return null;
    var data=new byte[length];reader.ReadBytesSafe(ref data,length);return JsonUtility.FromJson<Packet>(Encoding.UTF8.GetString(data));
   }catch(Exception e)when(e is ArgumentException||e is OverflowException||e is IndexOutOfRangeException){return null;}
  }
 }
}
