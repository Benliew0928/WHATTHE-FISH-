#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 // Opt-in real-service, two-player review. Uses isolated UGS/guest profiles.
 public sealed class PlayerProfilesReview:MonoBehaviour {
  string folder,shared,role;bool failed,cloudTouched;Task flow;string otherId;
  static string Arg(string key){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,key);return i>=0&&i+1<args.Length?args[i+1]:"";}
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){
   if(Arg("-profilesReview").Length>0&&Arg("-authProfile").Length>0&&Arg("-guestProfile").Length>0){
    var r=new GameObject("Player profile review").AddComponent<PlayerProfilesReview>();r.folder=Arg("-profilesReview");r.shared=Arg("-profilesShared");r.role=Arg("-profilesRole");
   }
  }
  void Check(bool ok,string label){failed|=!ok;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+label+"\n");}
  async Task Until(Func<bool> condition,string label,int seconds=75){
   float end=Time.realtimeSinceStartup+seconds;while(!condition()&&Time.realtimeSinceStartup<end)await Task.Delay(150);
   Check(condition(),label);if(!condition())throw new Exception("Review timeout: "+label);
  }
  static Button B(string name)=>FindObjectsByType<Button>(FindObjectsSortMode.None).First(b=>b.name==name&&b.gameObject.activeInHierarchy);
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");
   flow=Run();while(!flow.IsCompleted)yield return null;
   if(flow.IsFaulted){failed=true;File.AppendAllText(Path.Combine(folder,"results.txt"),"FAIL "+flow.Exception.GetBaseException().Message+"\n");}
   File.AppendAllText(Path.Combine(folder,"results.txt"),"PLAYER_PROFILES_COMPLETE success="+!failed+"\n");Application.Quit(failed?1:0);
  }
  async Task Run(){
   try{
    await Until(()=>AppRoot.Instance&&PackagedContentBoot.Ready&&StartupWelcome.Instance&&StartupWelcome.Instance.IsReady,"packaged game ready",120);
    if(Arg("-profilesMode")=="cache"){
     Check(PlayerProfiles.Current.name==role+" Renamed","saved name survives separate player launch");
     Check(PlayerProfiles.CurrentPhoto.Length==0&&PlayerProfiles.Current.hasPhoto==0&&PlayerProfiles.Current.avatar==(role=="Host"?9:8),"selected character survives separate player launch");
     Check(PlayerProfiles.Current.id==File.ReadAllText(Path.Combine(shared,role+"-id.txt"))&&PlayerProfiles.Current.code.Length==16,"cached profile preserves its own account identity");
     return;
    }
    var welcome=StartupWelcome.Instance;welcome.ContinueAsGuest();welcome.GuestNameField.text=role+" Profile";welcome.GuestContinueButton.onClick.Invoke();
    await Until(()=>!StartupWelcome.Blocking,"guest enters game");
    cloudTouched=true;var profile=await PlayerProfiles.EnsureCloud();Check(profile.id.Length>0&&profile.code.Length==16,"guest has cloud profile and permanent code");
    var pixels=Enumerable.Repeat((byte)(role=="Host"?80:170),PlayerProfiles.PhotoBytes).ToArray();
    await PlayerProfiles.Save(role+" Profile",role=="Host"?1:4,Convert.ToBase64String(pixels));
    Check(PlayerProfiles.Current.hasPhoto==1&&!PlayerProfiles.PendingSync,"photo saved to cloud");
    var restored=await PlayerProfiles.EnsureCloud(true);Check(restored.name==role+" Profile"&&PlayerProfiles.CurrentPhoto.Length==65536,"cloud reload restores name and photo");
    File.WriteAllText(Path.Combine(shared,role+"-id.txt"),restored.id);
    if(role=="Host"){
     await AppRoot.Instance.rooms.Create();Check(AppRoot.Instance.rooms.Connected&&AppRoot.Instance.rooms.Session!=null,"live Relay room created");
     if(!AppRoot.Instance.rooms.Connected)throw new Exception(AppRoot.Instance.rooms.Error);
     File.WriteAllText(Path.Combine(shared,"room.txt"),AppRoot.Instance.rooms.Code);
    }else{
     await Until(()=>File.Exists(Path.Combine(shared,"room.txt")),"host shares room");
     await AppRoot.Instance.rooms.Join(File.ReadAllText(Path.Combine(shared,"room.txt")));Check(AppRoot.Instance.rooms.Connected,"guest joins live Relay room");
    }
    await Until(()=>RoomTeleport.Players().Count(p=>p.IsPlayerObject&&p.ProfileId.Value.Length>0)==2,"both room profiles verified and replicated");
    otherId=File.ReadAllText(Path.Combine(shared,(role=="Host"?"Guest":"Host")+"-id.txt"));
    var other=RoomTeleport.Players().Single(p=>p.ProfileId.Value.ToString()==otherId);
    Check(other.PublicProfile.hasPhoto==1&&other.Nickname.Value.ToString()==(role=="Host"?"Guest":"Host")+" Profile","peer photo metadata and name replicated");
    await PlayerProfiles.Save(role+" Renamed",role=="Host"?2:5,Convert.ToBase64String(pixels));
    await Until(()=>RoomTeleport.Players().All(p=>!p.IsPlayerObject||p.Nickname.Value.ToString().EndsWith("Renamed")),"live rename reaches both clients");
    var result=await PlayerProfiles.Search((role=="Host"?"Guest":"Host")+" Renamed");
    Check(result.players.Any(p=>p.id==otherId),"name search locates peer");
    if(role=="Host"){
     var sent=await PlayerProfiles.Friend(otherId,"request");Check(sent.relationship=="outgoing","friend request sent");
     File.WriteAllText(Path.Combine(shared,"requested.txt"),"ready");
     await Until(()=>File.Exists(Path.Combine(shared,"accepted.txt")),"guest accepts request");
     Check((await PlayerProfiles.Friends()).friends.Any(p=>p.id==otherId),"accepted friendship saved for sender");
    }else{
     await Until(()=>File.Exists(Path.Combine(shared,"requested.txt")),"request available");
     Check((await PlayerProfiles.Friends()).incoming.Any(p=>p.id==otherId),"incoming request visible to recipient");
     SocialPanel.OpenPlayer(otherId);
     await Until(()=>FindObjectsByType<Button>(FindObjectsSortMode.None).Any(b=>b.name=="Accept friend request"&&b.gameObject.activeInHierarchy),"incoming profile exposes accept action");
     Check(B("Decline friend request").interactable,"incoming profile exposes decline action");
     await Capture("incoming-request");B("Accept friend request").onClick.Invoke();
     await Until(()=>FindObjectsByType<Button>(FindObjectsSortMode.None).Any(b=>b.name=="Player friendship action"&&b.gameObject.activeInHierarchy),"accept action completes");
     Check((await PlayerProfiles.Get(otherId)).relationship=="friends","accept request saves friendship");
     B("Close social panel").onClick.Invoke();await Task.Delay(150);
     File.WriteAllText(Path.Combine(shared,"accepted.txt"),"ready");
    }
    SocialPanel.OpenOwn();await Task.Delay(1500);
    Check(SocialPanel.Visible&&AppRoot.Instance.view.InputBlocked,"profile panel blocks gameplay input");
    Check(B("Camera profile picture")&&B("Gallery profile picture")&&B("Save player profile"),"camera gallery and save controls present");
    await Capture("profile-editor");
    Check(FindObjectsByType<Button>(FindObjectsSortMode.None).Count(b=>b.name.StartsWith("Character avatar ")&&b.gameObject.activeInHierarchy)==10,"all ten character choices available");
    for(int i=0;i<PlayerProfiles.AvatarCount;i++)Check(PlayerProfiles.DefaultAvatar(i)&&PlayerProfiles.DefaultAvatar(i).width==1024,"original character art loaded "+i);
    B("Character avatar 0").onClick.Invoke();await Capture("character-roster");
    B("Character avatar "+(role=="Host"?9:8)).onClick.Invoke();
    B("Save player profile").onClick.Invoke();
    await Until(()=>PlayerProfiles.Current.hasPhoto==0&&PlayerProfiles.Current.avatar==(role=="Host"?9:8)&&B("Save player profile").interactable,"character selection saved through profile UI");
    await Until(()=>RoomTeleport.Players().All(p=>!p.IsPlayerObject||p.PublicProfile.hasPhoto==0&&p.PublicProfile.avatar>=8),"new characters replicate to both clients");
    await Capture("character-editor");
    var hub=FindFirstObjectByType<SocialPanel>();var safe=Screen.safeArea;
    foreach(var button in hub.GetComponentsInChildren<Button>()){
     var corners=new Vector3[4];((RectTransform)button.transform).GetWorldCorners(corners);
     Check(corners.All(v=>v.x>=safe.xMin-1&&v.x<=safe.xMax+1&&v.y>=safe.yMin-1&&v.y<=safe.yMax+1),"profile control inside safe area "+button.name);
    }
    if(role=="Host"){
     Screen.SetResolution(1920,824,false);await Task.Delay(750);await Capture("wide-character-editor");
     Screen.SetResolution(1600,900,false);await Task.Delay(500);
    }
    B("Friends tab").onClick.Invoke();
    await Until(()=>FindObjectsByType<Button>(FindObjectsSortMode.None).Any(b=>b.name.StartsWith("View player ")&&b.gameObject.activeInHierarchy),"friends tab displays accepted teammate");
    await Capture("friends-list");B("Incoming requests").onClick.Invoke();
    await Until(()=>FindObjectsByType<Button>(FindObjectsSortMode.None).Any(b=>b.name=="Find more players"&&b.gameObject.activeInHierarchy),"accepted request leaves a clear empty requests state");
    await Capture("empty-requests");
    B("Find players tab").onClick.Invoke();await Task.Delay(200);
    var search=FindObjectsByType<InputField>(FindObjectsSortMode.None).Single(f=>f.name=="Search player names");
    search.text=(role=="Host"?"Guest":"Host")+" Renamed";
    await Until(()=>FindObjectsByType<Button>(FindObjectsSortMode.None).Any(b=>b.name.StartsWith("View player ")&&b.gameObject.activeInHierarchy),"debounced search displays results");
    await Capture("friend-search");
    search.text="";await Task.Delay(650);
    Check(!FindObjectsByType<Button>(FindObjectsSortMode.None).Any(b=>b.name.StartsWith("View player ")&&b.gameObject.activeInHierarchy),"clearing search removes old results");
    SocialPanel.OpenPlayer(otherId);await Task.Delay(1600);
    Check(B("Player friendship action").GetComponentInChildren<Text>().text=="Remove friend","profile card shows accepted relationship");
    await Capture("peer-profile");
    B("Close social panel").onClick.Invoke();await Task.Delay(200);Check(!SocialPanel.Visible,"social overlay closes");
    RoomTeleport.Players().Single(p=>p.IsOwner).ReadyRpc(true);
    await Until(()=>RoomTeleport.Players().All(p=>p.Ready.Value),"both players ready to travel");
    if(role=="Host")await AppRoot.Instance.rooms.SetExploring(true);
    await Until(()=>AppRoot.Instance.Exploring&&!RoomTeleport.Busy&&AppRoot.Instance.view.active,"shared travel reaches playable world",90);
    Check(B("Open room profiles").gameObject.activeInHierarchy,"Players control available during gameplay");
    await Until(()=>FindObjectsByType<Button>(FindObjectsSortMode.None).Any(b=>b.name=="View character profile "+otherId&&b.gameObject.activeInHierarchy),"peer nameplate is tappable in gameplay");
    Check(B("View character profile "+otherId).GetComponentInChildren<ProfileAvatar>(),"peer nameplate includes profile picture");
    await Capture("world-profiles");
    B("View character profile "+otherId).onClick.Invoke();await Task.Delay(1200);
    Check(SocialPanel.Visible&&B("Player friendship action").GetComponentInChildren<Text>().text=="Remove friend","tapping gameplay nameplate opens correct peer profile");
    B("Close social panel").onClick.Invoke();await Task.Delay(200);
    B("Open room profiles").onClick.Invoke();await Task.Delay(500);
    Check(FindObjectsByType<Button>(FindObjectsSortMode.None).Count(b=>b.name.StartsWith("View player ")&&b.gameObject.activeInHierarchy)==2,"in-game player list exposes both complete profiles");
    B("Close social panel").onClick.Invoke();await Task.Delay(200);
    File.WriteAllText(Path.Combine(shared,role+"-done.txt"),"done");
    await Until(()=>File.Exists(Path.Combine(shared,(role=="Host"?"Guest":"Host")+"-done.txt")),"both peer reviews finish");
   }finally{
    if(AppRoot.Instance&&AppRoot.Instance.rooms.Connected)await AppRoot.Instance.rooms.Leave();
    if(cloudTouched)try{await PlayerProfiles.Request("/v1/me","DELETE",new DeleteRequest());}catch(Exception){Check(false,"temporary cloud profile cleanup");}
   }
  }
  [Serializable]sealed class DeleteRequest{public string confirm="delete-profile";}
  async Task Capture(string name){
   await Task.Delay(250);Canvas.ForceUpdateCanvases();var camera=Camera.main;
   var canvases=FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.isRootCanvas&&c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();
   foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=camera;c.planeDistance=1;}
   var rt=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var old=RenderTexture.active;var original=camera.targetTexture;
   var image=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);
   try{Canvas.ForceUpdateCanvases();camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);image.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),image.EncodeToPNG());}
   finally{Destroy(image);camera.targetTexture=original;RenderTexture.active=old;RenderTexture.ReleaseTemporary(rt);foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceOverlay;c.worldCamera=null;}}
  }
 }
}
#endif
