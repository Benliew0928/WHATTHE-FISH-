#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
namespace WhatTheFish {
 // File commands allow several real NGO processes to exercise the same room.
 public sealed class RoomChatReview:MonoBehaviour {
  string folder,last;RoomChatService chat;AppRoot app;bool busy;int checks;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-roomChatProbe");if(i>=0&&i+1<args.Length)new GameObject("Room chat verification").AddComponent<RoomChatReview>().folder=args[i+1];}
  IEnumerator Start(){Directory.CreateDirectory(folder);while(!AppRoot.Instance||!AppRoot.Instance.rooms||!RoomChatUI.Instance)yield return null;app=AppRoot.Instance;chat=RoomChatService.Ensure(app.rooms);DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;
   Check(RoomChatService.CleanText("  \nhello\tcrew\u202e ")=="hello crew","whitespace and directional controls normalized");
   Check(RoomChatService.CleanText(new string('a',160)).Length==120,"text length bounded");Check(RoomChatService.CleanText("<b>GG</b>")=="<b>GG</b>","literal markup preserved");
   Check(System.Globalization.StringInfo.ParseCombiningCharacters(RoomChatService.CleanText(string.Concat(Enumerable.Repeat("🐟",140)))).Length==120,"Unicode text limit keeps complete emoji");
   foreach(var id in RoomChatService.Stickers){var s=RoomChatUI.Sticker(id);Check(s&&s.texture.width==256&&s.texture.height==256,"delivery sticker "+id);}
   while(true){
    var own=RoomTeleport.Players().FirstOrDefault(a=>a.IsOwner);if(own&&!own.Ready.Value&&!app.rooms.Recovering)own.ReadyRpc(true);
    if(!busy){string command=null;try{string path=Path.Combine(folder,"command.txt");if(File.Exists(path))command=File.ReadAllText(path);}catch(IOException){}
     if(command!=null&&command!=last){last=command;busy=true;StartCoroutine(Command(command));}}
    var state=new State{ready=chat.Ready,connected=app.rooms.Connected,host=app.rooms.Host,recovering=app.rooms.Recovering,exploring=app.Exploring,sport=app.SelectedSport.ToString(),room=chat.RoomId,member=app.rooms.MemberId,count=chat.History.Count,players=RoomTeleport.Players().Length,spectator=own&&own.IsSpectator,status=chat.Status,command=last,busy=busy,bubbles=RoomChatUI.Instance.VisibleBubbleCount,interrupted=chat.HistoryInterrupted,messages=chat.History.ToArray()};
    string temp=Path.Combine(folder,"state.pending"),file=Path.Combine(folder,"state.json");File.WriteAllText(temp,JsonUtility.ToJson(state,true));try{if(File.Exists(file))File.Replace(temp,file,null);else File.Move(temp,file);}catch(IOException){}
    yield return new WaitForSecondsRealtime(.15f);
   }
  }
  [Serializable]sealed class State{public bool ready,connected,host,recovering,exploring,spectator,busy,interrupted;public string sport,room,member,status,command;public int count,players,bubbles;public RoomChatMessage[] messages;}
  void Check(bool pass,string label){checks++;File.AppendAllText(Path.Combine(folder,"results.txt"),(pass?"PASS ":"FAIL ")+label+"\n");}
  IEnumerator Command(string command){
   int separator=command.IndexOf('|');string action=separator<0?command:command.Substring(separator+1);var own=RoomTeleport.Players().FirstOrDefault(a=>a.IsOwner);var ui=RoomChatUI.Instance;
   if(action.StartsWith("text:"))Check(chat.SendText(action.Substring(5)),"send text "+action.Substring(5));
   else if(action.StartsWith("sticker:"))Check(chat.SendSticker(action.Substring(8)),"send sticker "+action.Substring(8));
   else if(action.StartsWith("nick:"))own.NicknameRpc(action.Substring(5));
   else if(action.StartsWith("seat:"))own.ChooseRoomSeatRpc(byte.Parse(action.Substring(5)));
   else if(action.StartsWith("sport:")){Check(app.rooms.TrySetLobbySport(Enum.Parse<SportId>(action.Substring(6)),out var reason),"change sport "+reason);}
   else if(action.StartsWith("explore:")){var task=app.rooms.SetExploring(action.Substring(8)=="true");while(!task.IsCompleted)yield return null;}
   else if(action=="open")ui.SetOpen(true);
   else if(action=="close")ui.Close();
   else if(action=="typing"){
    ui.SetOpen(true);yield return null;ui.MessageInput.ActivateInputField();yield return null;yield return null;
    Check(ui.MessageInput.isFocused&&RoomChatUI.Typing&&app.view.InputBlocked,"chat focus blocks gameplay input");
    ui.MessageInput.text="<b>Literal text</b>";Check(!ui.MessageInput.textComponent.supportRichText,"composer renders markup literally");
    ui.MessageInput.text=new string('x',180);Check(ui.MessageInput.text.Length==120,"composer enforces visible text limit");ui.MessageInput.text="";
    ui.Close();yield return null;yield return null;Check(!RoomChatUI.Typing,"closing releases chat keyboard capture");
   }
   else if(action=="rate"){
    int before=chat.History.Count;Check(chat.SendText("rate-first"),"first send accepted");Check(!chat.SendText("rate-second"),"local cooldown rejects immediate second send");yield return new WaitForSecondsRealtime(.5f);
    Raw("{\"request\":\""+Guid.NewGuid().ToString("N")+"\",\"text\":\"rate-forged\"}");yield return new WaitForSecondsRealtime(.4f);Check(chat.History.Count==before+1,"server enforces cooldown against raw client");
   }
   else if(action=="invalid"){
    int before=chat.History.Count;Check(!chat.SendSticker("unknown"),"unknown sticker rejected locally");Check(!chat.SendText(" \n\t"),"empty text rejected locally");
    Raw("{\"request\":\""+Guid.NewGuid().ToString("N")+"\",\"sticker\":\"evil/path\"}");yield return new WaitForSecondsRealtime(.3f);Raw("{invalid json");yield return new WaitForSecondsRealtime(.3f);Raw("{}",int.MaxValue);yield return new WaitForSecondsRealtime(.3f);
    Check(chat.History.Count==before,"invalid sticker, malformed JSON and oversized length rejected");Check(chat.Ready,"malformed messages keep connection alive");
   }
   else if(action=="dedup"){
    int before=chat.History.Count;string json="{\"request\":\""+Guid.NewGuid().ToString("N")+"\",\"text\":\"deduplicated\",\"member\":\"imposter\",\"name\":\"imposter\",\"spectator\":true}";
    Raw(json);yield return new WaitForSecondsRealtime(.35f);Raw(json);yield return new WaitForSecondsRealtime(.35f);
    Check(chat.History.Count==before+1,"duplicate request adds one message");var message=chat.History.Last();Check(message.member==app.rooms.MemberId&&message.name==own.Nickname.Value.ToString()&&message.spectator==own.IsSpectator,"server supplies authoritative sender identity and role");
   }
   else if(action.StartsWith("mute:")){string member=chat.History.First(m=>m.text==action.Substring(5)).member;chat.ToggleMute(member);Check(chat.IsMuted(member),"sender muted");chat.ToggleMute(member);Check(!chat.IsMuted(member),"sender unmuted");}
   else if(action=="burst-history"){
    for(int i=0;i<35;i++){while(chat.Cooldown>0)yield return null;Check(chat.SendText("History page "+i),"history page send "+i);yield return new WaitForSecondsRealtime(1.3f);}
   }
   else if(action.StartsWith("capture:")){ui.SetOpen(true);yield return new WaitForSecondsRealtime(.4f);yield return Capture(action.Substring(8));}
   else if(action=="tray"){
    ui.SetChatAudience(chat.LocalSpectator);ui.SetOpen(true);yield return null;ui.GetComponentsInChildren<Button>().First(b=>b.name=="Compose sticker").onClick.Invoke();yield return null;
    Check(RoomChatService.Stickers.All(id=>ui.GetComponentsInChildren<Button>().Any(b=>b.name=="Send "+id)),"all eight sticker choices visible");Check(!ui.Panel.gameObject.activeSelf,"sticker picker replaces history without overlapping composer");yield return Capture("sticker-picker");
    int before=chat.History.Count;ui.GetComponentsInChildren<Button>().First(b=>b.name=="Send laugh").onClick.Invoke();yield return new WaitForSecondsRealtime(.5f);Check(chat.History.Count==before+1&&chat.History.Last().sticker=="laugh","picker sends selected sticker");
   }
   else if(action=="ui-review"){
    foreach(var size in new[]{new Vector2Int(1600,900),new Vector2Int(1920,864),new Vector2Int(1024,768)}){Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.5f);ui.SetOpen(true);yield return null;
     var corners=new Vector3[4];ui.Panel.GetWorldCorners(corners);Check(corners.All(p=>p.x>=-1&&p.x<=Screen.width+1&&p.y>=-1&&p.y<=Screen.height+1),"chat fits "+size);
     if(app.Exploring&&app.SelectedSport==SportId.Fishing){var board=app.HudFrame.GetComponentsInChildren<RectTransform>().First(t=>t.name=="Lagoon sticker leaderboard");var boardCorners=new Vector3[4];board.GetWorldCorners(boardCorners);var dockCorners=new Vector3[4];ui.Launchers.GetWorldCorners(dockCorners);Check(dockCorners[1].y<boardCorners[0].y&&corners[1].y<dockCorners[0].y,"fishing chat stays beneath scoreboard and start controls "+size);}
     Check(ui.GetComponentsInChildren<Text>().All(t=>!t.supportRichText),"all chat labels are literal "+size);Canvas.ForceUpdateCanvases();Check(ui.GetComponentsInChildren<Text>().First(t=>t.text=="CHAT").cachedTextGenerator.characterCountVisible>0,"chat heading renders "+size);Check(ui.Launchers.GetComponentsInChildren<Button>().Length==2,"only chat and microphone launcher buttons");yield return Capture("chat-"+app.SelectedSport+"-"+size.x+"x"+size.y);}
    Screen.SetResolution(1600,900,false);
   }
   else if(action=="role-tabs"){
    ui.SetOpen(true);ui.SetChatAudience(true);yield return new WaitForSecondsRealtime(.3f);
    Check(ui.SpectatorChat&&ui.MessageInput.interactable==chat.LocalSpectator,"spectator tab composer follows own authoritative role");
    Check(ui.GetComponentsInChildren<Text>().Any(t=>t.text=="Spectators can chat too"),"spectator history is visible in its tab");
    yield return Capture("spectator-chat");ui.SetChatAudience(false);yield return new WaitForSecondsRealtime(.3f);
    Check(!ui.GetComponentsInChildren<Text>().Any(t=>t.text=="Spectators can chat too"),"player history excludes spectator messages");
    Check(ui.MessageInput.interactable!=chat.LocalSpectator,"player tab composer follows own authoritative role");
    Check(chat.HistoryFor(true).All(m=>m.spectator)&&chat.HistoryFor(false).All(m=>!m.spectator),"role timelines are disjoint");
    ui.SetChatAudience(chat.LocalSpectator);ui.Close();
   }
   else if(action=="bubbles"){
    float deadline=Time.realtimeSinceStartup+90;while((!ui.Visible||RoomTeleport.Busy||TeleportSequence.Busy)&&Time.realtimeSinceStartup<deadline)yield return null;
    Check(ui.Visible&&!RoomTeleport.Busy&&!TeleportSequence.Busy,"gameplay is visible before bubble review");
    ui.Close();app.view.mode=1;yield return new WaitForSecondsRealtime(.7f);Check(chat.SendText("Nice pass! Let's go!"),"text bubble send");yield return new WaitForSecondsRealtime(.3f);Check(app.rooms.Host?ui.VisibleBubbleCount==0:ui.VisibleBubbleCount>0,app.rooms.Host?"host setup controls suppress overlapping text bubble":"text bubble appears above player");yield return Capture("game-text");
    yield return new WaitForSecondsRealtime(4.2f);Check(ui.VisibleBubbleCount==0,"text bubble expires");Check(chat.SendSticker("ggwp"),"sticker bubble send");yield return new WaitForSecondsRealtime(.3f);Check(app.rooms.Host?ui.VisibleBubbleCount==0:ui.VisibleBubbleCount>0,app.rooms.Host?"host setup controls suppress overlapping sticker":"sticker bubble appears above player");yield return Capture("game-sticker");
    chat.HideBubbles=true;yield return new WaitForEndOfFrame();Check(ui.VisibleBubbleCount==0,"hide bubbles preference");chat.HideBubbles=false;yield return new WaitForSecondsRealtime(3.2f);Check(ui.VisibleBubbleCount==0,"sticker bubble expires");
   }
   else if(action=="leave-new-room"){
    var leave=app.rooms.Leave();while(!leave.IsCompleted)yield return null;Check(chat.History.Count==0&&chat.RoomId=="","leaving clears room history");app.rooms.StartLocal(true,7991);app.Show("room");yield return new WaitForSecondsRealtime(2);Check(chat.History.Count==0&&chat.Ready,"new room starts a new empty timeline");
   }
   else if(action=="quit"){File.AppendAllText(Path.Combine(folder,"results.txt"),"ROOM_CHAT_REVIEW_COMPLETE checks="+checks+"\n");Application.Quit();}
   busy=false;
  }
  void Raw(string json,int? length=null){byte[] bytes=Encoding.UTF8.GetBytes(json);using var writer=new FastBufferWriter(bytes.Length+4,Allocator.Temp);writer.WriteValueSafe(length??bytes.Length);writer.WriteBytesSafe(bytes);NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("wtf.chat.submit",NetworkManager.ServerClientId,writer,NetworkDelivery.ReliableFragmentedSequenced);}
  IEnumerator Capture(string name){yield return null;yield return new WaitForEndOfFrame();var camera=Camera.main;var overlays=FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();foreach(var c in overlays){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=camera;c.planeDistance=1;}Canvas.ForceUpdateCanvases();var target=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var previous=camera.targetTexture;var active=RenderTexture.active;camera.targetTexture=target;camera.Render();RenderTexture.active=target;var image=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);image.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),image.EncodeToPNG());Destroy(image);camera.targetTexture=previous;RenderTexture.active=active;RenderTexture.ReleaseTemporary(target);foreach(var c in overlays){c.renderMode=RenderMode.ScreenSpaceOverlay;c.worldCamera=null;} }
 }
}
#endif
