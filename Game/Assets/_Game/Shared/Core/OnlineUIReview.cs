#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
namespace WhatTheFish {
 // Real island backgrounds and actual UI controls; no simulated scene artwork.
 public sealed class OnlineUIReview:MonoBehaviour {
  string folder;AppRoot app;int checks;bool failed;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-onlineUIReview");if(i>=0&&i+1<args.Length)new GameObject("Online UI review").AddComponent<OnlineUIReview>().folder=args[i+1];}
  void Check(bool ok,string label){checks++;failed|=!ok;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+label+"\n");}
  IEnumerator Until(Func<bool> predicate,string label,float seconds=90){float deadline=Time.realtimeSinceStartup+seconds;while(!predicate()&&Time.realtimeSinceStartup<deadline)yield return null;Check(predicate(),label);}
  static void Property(object owner,string name,object value)=>owner.GetType().GetProperty(name,BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(owner,value);
  void Click(Transform root,string name){var b=root.GetComponentsInChildren<Button>(true).FirstOrDefault(x=>x.name==name);Check(b&&b.IsInteractable()&&b.gameObject.activeInHierarchy,"available control: "+name);if(b&&b.IsInteractable())b.onClick.Invoke();}
  bool Fits(RectTransform rect){var corners=new Vector3[4];rect.GetWorldCorners(corners);return corners.All(c=>Screen.safeArea.Contains(new Vector2(c.x,c.y)));}
  IEnumerator Capture(string name){
   yield return null;yield return new WaitForEndOfFrame();var camera=Camera.main;
   var canvases=FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.enabled&&c.renderMode==RenderMode.ScreenSpaceOverlay).Select(c=>(canvas:c,mode:c.renderMode,camera:c.worldCamera,plane:c.planeDistance)).ToArray();
   foreach(var c in canvases){c.canvas.renderMode=RenderMode.ScreenSpaceCamera;c.canvas.worldCamera=camera;c.canvas.planeDistance=1;}Canvas.ForceUpdateCanvases();
   var target=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var previous=camera.targetTexture;var active=RenderTexture.active;camera.targetTexture=target;camera.Render();RenderTexture.active=target;
   var image=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);image.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),image.EncodeToPNG());Destroy(image);camera.targetTexture=previous;RenderTexture.active=active;RenderTexture.ReleaseTemporary(target);
   foreach(var c in canvases){c.canvas.renderMode=c.mode;c.canvas.worldCamera=c.camera;c.canvas.planeDistance=c.plane;}Canvas.ForceUpdateCanvases();
  }
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");
   yield return Until(()=>AppRoot.Instance&&AppRoot.Instance.Menu&&!StartupWelcome.Blocking,"app ready");app=AppRoot.Instance;if(!app){Application.Quit(1);yield break;}
   DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;
   foreach(var sport in new[]{SportId.Football,SportId.Basketball,SportId.Golf,SportId.Fishing}){
    app.SelectSport(sport);yield return Until(()=>!app.PendingSelection.HasValue&&!app.environments.GetComponent<SkySailStreaming>().Busy,"loaded "+sport);app.EnterOffline();yield return new WaitForSeconds(.8f);
    foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1920,864),new Vector2Int(1024,768)}){
     Screen.SetResolution(size.x,size.y,false);yield return new WaitForSeconds(.5f);PostMatchFlow.Instance.Open();yield return null;Canvas.ForceUpdateCanvases();
     var card=PostMatchFlow.Instance.GetComponentsInChildren<RectTransform>().First(r=>r.name=="Safe adventure card");Check(Fits(card),sport+" chooser fits safe frame "+size);
     foreach(var text in card.GetComponentsInChildren<Text>().Where(t=>t.text.Length>0))Check(text.preferredHeight<=text.rectTransform.rect.height+1,sport+" readable: "+text.text);
     yield return Capture(sport+"-chooser-"+size.x+"x"+size.y);Click(PostMatchFlow.Instance.transform,"Keep exploring");Check(!app.view.InputBlocked,"exploration restored");
    }
    var actor=app.LocalAthlete;actor.capsule.enabled=false;actor.transform.position=SkySailMap.Exit(sport,0);actor.capsule.enabled=true;app.view.yaw=SkySailMap.Facing(sport).eulerAngles.y;yield return new WaitForSeconds(.5f);yield return Capture(sport+"-skyrail-station");
   }
   Screen.SetResolution(1280,720,false);yield return new WaitForSeconds(.5f);
   var fish=FishingGame.Instance;var ability=FishingHUD.Instance.GetComponent<FishingAbilityHUD>();
   // Back must never silently pick the first practice skill.
   if(ability){ability.ReviewPracticeChoice=true;var before=fish.Player(app.LocalAthlete);ability.CloseOverlay();var after=fish.Player(app.LocalAthlete);Check(before.HasValue&&after.HasValue&&before.Value.skillPicked==after.Value.skillPicked,"back does not choose a fishing skill");yield return null;Check(ability.SkillVisible&&ability.SkillButton.interactable,"dismissed practice draft has a choose action");ability.SkillButton.onClick.Invoke();yield return null;Check(ability.Modal,"practice skill choice can reopen");ability.CloseOverlay();}
   Property(app.rooms,"Recovering",true);var recovery=RoomRecoveryUI.Create(app.rooms);Property(app.rooms,"RecoveryMessage","A crew member is becoming host…");yield return Capture("recovery-over-fishing");Check(Fits(recovery.GetComponentsInChildren<RectTransform>().First(r=>r.name=="Recovery card")),"recovery fits safe frame");Destroy(recovery.gameObject);Property(app.rooms,"Recovering",false);
   app.LeaveCourt();yield return null;app.Show("join");yield return new WaitForSeconds(.4f);var input=app.Menu.Content.GetComponentsInChildren<InputField>().First();input.text="WAVE24";app.rooms.ReportError("Could not connect. Check the room code and connection, and use the same game build as your host.");yield return Capture("join-network-error");Check(input.text=="WAVE24","network failure preserves room code");Check(!RoomFeedback.Explain(app.rooms.Error).Contains("find that room"),"network failure does not blame room code");app.rooms.DismissError();
   Check(RoomFeedback.Explain("This room has no open places").Contains("no open places"),"full room explanation");Check(RoomFeedback.Explain("NetworkConfig mismatch").Contains("same build"),"incompatible build explanation");Check(RoomFeedback.Explain("Exploration has started").Contains("waiting room"),"locked room explanation");Check(RoomFeedback.Explain("The room could not reconnect").Contains("current room code"),"recovery failure explanation");
   Property(app,"SelectionError","The island could not be prepared. Retry, or return to choose again.");Property(app,"FailedSelection",SportId.Fishing);app.Show("sport");yield return new WaitForSeconds(.4f);yield return Capture("loading-error");Click(app.Menu.Content,"Back from island loading");Check(app.SelectionError==null&&!app.PendingSelection.HasValue&&app.Menu.Page=="home","loading error has working exit");
   typeof(AppRoot).GetField("pendingSelection",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(app,(SportId?)SportId.Football);app.EnterOffline();app.CancelSelection();yield return null;yield return null;
   Check(!app.Exploring&&app.Menu.Page=="home","back during loading cancels queued offline entry");
   var complete=new System.Threading.Tasks.TaskCompletionSource<bool>();
   Func<System.Threading.Tasks.Task> connection=async()=>{await complete.Task;throw new OperationCanceledException();};
   var operation=(System.Threading.Tasks.Task)typeof(RoomService).GetMethod("ConnectOperation",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(app.rooms,new object[]{connection});
   yield return null;Check(app.rooms.CanCancelConnection,"pending room operation offers cancellation");app.rooms.CancelConnection();Check(app.rooms.busy&&!app.rooms.CanCancelConnection,"cancellation waits for cleanup and prevents duplicate requests");complete.SetResult(true);while(!operation.IsCompleted)yield return null;Check(!app.rooms.busy&&!app.rooms.Connected&&app.rooms.Session==null,"cancelled operation leaves no room or transport");
   Func<System.Threading.Tasks.Task> sdkCancel=()=>{app.rooms.StartLocal(true,7925);return System.Threading.Tasks.Task.FromCanceled(new System.Threading.CancellationToken(true));};
   operation=(System.Threading.Tasks.Task)typeof(RoomService).GetMethod("ConnectOperation",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(app.rooms,new object[]{sdkCancel});while(!operation.IsCompleted)yield return null;
   Check(!app.rooms.busy&&!app.rooms.Connected&&!Unity.Netcode.NetworkManager.Singleton.IsListening,"service cancellation also cleans an already-started transport");
   AccountPanel.Open();yield return Capture("account-choices");var account=FindFirstObjectByType<AccountPanel>();Click(account.transform,"Close account choices");
   DevelopmentProbe.TurnCommandActive=false;File.AppendAllText(Path.Combine(folder,"results.txt"),"ONLINE_UI_REVIEW_COMPLETE success="+!failed+" checks="+checks+"\n");Application.Quit(failed?1:0);
  }
 }
}
#endif
