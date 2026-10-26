#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 public sealed class ModerationReview:MonoBehaviour {
  string folder;int checks;bool failed;AppRoot app;PublicProfile peer;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-moderationReview");if(i<0||i+1>=args.Length)return;var r=new GameObject("Moderation review").AddComponent<ModerationReview>();r.folder=args[i+1];EconomyRenderReview.UnlockAll();}
  void Check(bool pass,string name){checks++;failed|=!pass;File.AppendAllText(Path.Combine(folder,"results.txt"),(pass?"PASS ":"FAIL ")+name+"\n");}
  IEnumerator Wait(Func<bool> ready,string name){float end=Time.realtimeSinceStartup+100;while(!ready()&&Time.realtimeSinceStartup<end)yield return null;Check(ready(),name);}
  static object Call(object obj,string name,params object[] args)=>obj.GetType().GetMethod(name,BindingFlags.NonPublic|BindingFlags.Instance).Invoke(obj,args);
  Button Button(string name)=>FindObjectsByType<Button>(FindObjectsSortMode.None).First(b=>b.name==name&&b.gameObject.activeInHierarchy);
  IEnumerator Start(){
   DontDestroyOnLoad(gameObject);Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");Application.targetFrameRate=30;
   yield return Wait(()=>DevelopmentProbe.ReadyToReviewSport,"sport initialized");app=AppRoot.Instance;DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;
   foreach(string s in new[]{"fuck","F.U.C.K","f u c k","ＦＵＣＫ","f\u200bu\u200bc\u200bk","fuuuck","sh1t","bull$hit","fuck123","pukimak","P U K I M A K","cibai","你好 傻逼 lah","你是傻 逼","草泥马","約炮","send nudes"})Check(TextSafety.Blocked(s),"reject listed/evasive text "+s);
   foreach(string s in new[]{"class","Scunthorpe","Dick","assistant","Shitake","Mishit","sex education","sexual health","gay","transgender","babi","anjing","Terima kasih","Selamat bermain","你好朋友","Nice pass!","福克斯","Sialkot"})Check(!TextSafety.Blocked(s),"allow benign text "+s);
   Check(!PlayerIdentity.ValidateName("fuck123",out _,out var error)&&error==TextSafety.NameError,"name validation blocks offensive content");Check(NetworkAthlete.CleanNickname("fuck123")=="","host nickname fallback rejects bypass");
   var chat=new GameObject("Text safety chat fixture").AddComponent<RoomChatService>();Check(!chat.SendText("sh1t")&&chat.Status==TextSafety.ChatError,"sender sees chat rejection");Destroy(chat.gameObject);
   PlayerProfiles.Current.id="moderation_review_self";peer=new PublicProfile{id="moderation_review_peer",code="0123456789ABCDEF",name="PemainAtlas甲乙丙",avatar=0};
   PlayerReportUI.Open(PlayerProfiles.Current);Check(!PlayerReportUI.Visible,"self report cannot open");Check(PlayerReports.Resolve(ulong.MaxValue)==null,"unverified/bot target has no report identity");
   foreach(var language in new[]{GameLanguage.English,GameLanguage.SimplifiedChinese,GameLanguage.Malay}){
    GameLocalization.Select(language,false);
    foreach(var size in new[]{new Vector2Int(1600,900),new Vector2Int(844,390)}){
     Screen.SetResolution(size.x,size.y,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.4f);PlayerReportUI.Open(peer);yield return new WaitForSecondsRealtime(.3f);
     Check(PlayerReportUI.Visible&&app.view.InputBlocked&&Time.timeScale==1,"report blocks only local controls");
     var panel=FindFirstObjectByType<PlayerReportUI>();LabelsFit(panel.gameObject);Check(panel.GetComponentsInChildren<Button>().Count(b=>b.name.StartsWith("Report reason "))==8,"all report reasons available");
     Check(!Button("Submit player report").interactable,"reason required");Button("Report reason other").onClick.Invoke();Check(!Button("Submit player report").interactable,"Other requires details");
     var input=panel.GetComponentInChildren<InputField>();input.text="Quoted abuse: fuck you / 侮辱 / gangguan";Check(Button("Submit player report").interactable,"report may quote abuse as evidence");
     yield return Capture("report-"+GameLocalization.Code+"-"+size.x);Button("Cancel player report").onClick.Invoke();yield return null;Check(!PlayerReportUI.Visible,"cancel closes report");
    }
   }
   Screen.SetResolution(1600,900,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.3f);PlayerReportUI.Open(peer,true);yield return null;Check(!FindObjectsByType<InputField>(FindObjectsSortMode.None).Any(f=>f.name=="Report explanation"),"fishing card opens before reasons");Button("Report player").onClick.Invoke();yield return null;Button("Report reason abuse").onClick.Invoke();var modal=FindFirstObjectByType<PlayerReportUI>();modal.GetComponentInChildren<InputField>().text="A quoted insult";
   string retry=null;int calls=0;PlayerReports.ReviewSubmit=r=>{calls++;if(retry==null)retry=r.requestId;else Check(retry==r.requestId,"retry keeps submission identity");Check(r.target==peer.id&&r.details=="A quoted insult","report preserves selected account and details");return calls==1?Task.FromException<PlayerReportReceipt>(new ProfileException(0,"network_unavailable")):Task.FromResult(new PlayerReportReceipt{ok=true,version=2,id="review-receipt"});};
   Button("Submit player report").onClick.Invoke();yield return null;Check(Button("Submit player report").interactable,"failure permits retry");Button("Submit player report").onClick.Invoke();yield return null;Check(!Button("Submit player report").interactable&&calls==2,"success prevents duplicate submit");Button("Close report").onClick.Invoke();yield return null;PlayerReports.ReviewSubmit=null;
   yield return Social();yield return Placement();yield return Results();
   PlayerReports.ReviewTargets.Clear();File.AppendAllText(Path.Combine(folder,"results.txt"),"MODERATION_REVIEW_COMPLETE success="+!failed+" checks="+checks+"\n");Application.Quit(failed?2:0);
  }
  IEnumerator Social(){
   foreach(var lang in new[]{GameLanguage.English,GameLanguage.SimplifiedChinese,GameLanguage.Malay}){
    GameLocalization.Select(lang,false);Screen.SetResolution(844,390,false);yield return new WaitForSecondsRealtime(.3f);
    var social=new GameObject("Social report review").AddComponent<SocialPanel>();Call(social,"Create");
    var flags=BindingFlags.Instance|BindingFlags.NonPublic;var design=social.transform.Find("Social safe area");
    typeof(SocialPanel).GetField("body",flags).SetValue(social,CoveUI.Rect(design,"Social player",310,170,1250,630));typeof(SocialPanel).GetField("page",flags).SetValue(social,"player");Call(social,"Navigation");Call(social,"PresentPlayer",peer);yield return null;
    Check(social.GetComponentsInChildren<Button>().Count(b=>b.name=="Report player")==1,"one combined profile report action");Check(!social.GetComponentsInChildren<Button>().Any(b=>b.name=="Report name"||b.name=="Report picture"),"no separate name/photo buttons");LabelsFit(social.gameObject);yield return Capture("social-"+GameLocalization.Code);
    Button("Report player").onClick.Invoke();yield return null;Check(PlayerReportUI.Visible,"profile action opens shared report");Button("Close report").onClick.Invoke();Destroy(social.gameObject);yield return null;
   }
  }
  IEnumerator Results(){
   if(app.SelectedSport==SportId.Golf)yield break;
   GameObject results=null;
   if(app.SelectedSport==SportId.Football){Call(FootballMatch.Instance.State,"Finish",FootballMatchResult.TeamA,FootballMatch.Instance.Now);yield return Wait(()=>FootballMVP.Presenting,"football final results open");FootballMVP.Instance.SkipCelebration();yield return Wait(()=>FootballMVP.Instance.ResultsReady,"football final camera settled");results=FootballMVP.Instance.gameObject;Button("Toggle match stats").onClick.Invoke();}
   else if(app.SelectedSport==SportId.Basketball){Call(BasketballMatch.Instance.State,"Finish");yield return Wait(()=>BasketballMVP.Presenting,"basketball final results open");BasketballMVP.Instance.SkipCelebration();yield return Wait(()=>BasketballMVP.Instance.ResultsReady,"basketball final camera settled");results=BasketballMVP.Instance.gameObject;Button("Toggle match stats").onClick.Invoke();}
   else {var f=FishingGame.Instance;f.ReviewSnapshot=true;f.State.Advance(f.State.Match.draftUntil+1,0);f.State.Advance(f.State.Match.started+1,0);f.State.Advance(f.State.Match.deadline+1,0);yield return Wait(()=>FishingHUD.Instance.Results.Visible,"fishing final results open");results=FishingHUD.Instance.Results.gameObject;}
   foreach(var lang in new[]{GameLanguage.English,GameLanguage.SimplifiedChinese,GameLanguage.Malay}){
    GameLocalization.Select(lang,false);Screen.SetResolution(844,390,false);yield return new WaitForSecondsRealtime(.3f);
    var actions=results.GetComponentsInChildren<PlayerReportAction>();Check(actions.Length>0&&actions.All(a=>a.TargetId!=PlayerProfiles.Current.id),"final results report only other players");yield return Capture("results-"+GameLocalization.Code);
    if(actions.Length>0){actions[0].GetComponent<Button>().onClick.Invoke();yield return null;Check(PlayerReportUI.Visible,"final results open shared report");Button("Close report").onClick.Invoke();yield return null;}
   }
  }
  IEnumerator Placement(){
   ulong[] ids;ulong own;
   switch(app.SelectedSport){
    case SportId.Football:
     Check(FootballBots.Instance.StartSolo(2),"football solo fixture");Check(FootballMatch.Instance.StartMatch(),"football starts");FootballMatch.Instance.RequestTeam(FootballTeam.A);FootballMatch.Instance.ConfirmSoloTeam();yield return Wait(()=>FootballMatch.Instance.KickoffReady||FootballMatch.Instance.Running,"football ready");ids=FootballMatch.Instance.PlayerRecords.Select(p=>p.id).ToArray();own=FootballBots.Id(app.LocalAthlete);break;
    case SportId.Basketball:
     Check(BasketballBots.Instance.StartSolo(2),"basketball solo fixture");Check(BasketballMatch.Instance.StartMatch(),"basketball starts");BasketballMatch.Instance.RequestTeam(FootballTeam.A);BasketballMatch.Instance.ConfirmSoloTeam();yield return Wait(()=>BasketballMatch.Instance.Running,"basketball ready");ids=BasketballMatch.Instance.PlayerRecords.Select(p=>p.id).ToArray();own=BasketballMatch.PlayerId(app.LocalAthlete);break;
    case SportId.Golf:
     Check(IslandBots.Instance.StartSolo(2),"golf solo fixture");Check(GolfMatchManager.Instance.StartMatch(),"golf starts");ids=GolfMatchManager.Instance.State.Players.Select(p=>p.PlayerId).ToArray();own=GolfCartWorld.Key(app.LocalAthlete);break;
    default:
     Check(IslandBots.Instance.StartSolo(2),"fishing solo fixture");yield return Wait(()=>FishingGame.Instance.CanStart(FishingMode.Round),"fishing arrival ready");Check(FishingGame.Instance.StartMatch(),"fishing starts");ids=FishingGame.Instance.State.Players.Select(p=>p.owner).ToArray();own=FishingGame.Key(app.LocalAthlete);break;
   }
   Check(ids.Length>1,"fixture has reportable opponents");foreach(ulong id in ids){var profile=PlayerReports.Copy(id==own?PlayerProfiles.Current:peer);if(id!=own)profile.id+="_"+id;PlayerReports.ReviewTargets[id]=profile;}
   foreach(var lang in new[]{GameLanguage.English,GameLanguage.SimplifiedChinese,GameLanguage.Malay}){
    GameLocalization.Select(lang,false);
    foreach(var size in new[]{new Vector2Int(1600,900),new Vector2Int(844,390)}){
     Screen.SetResolution(size.x,size.y,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.4f);
     var scoreboard=FindObjectsByType<SportsScoreboard>(FindObjectsSortMode.None).FirstOrDefault(s=>!s.Venue&&s.gameObject.activeInHierarchy);
     if(scoreboard)scoreboard.Details.SetExpanded(true);if(app.SelectedSport==SportId.Golf)FindFirstObjectByType<GolfLeaderboardUI>().OpenResults();yield return new WaitForSecondsRealtime(.4f);
     var actions=FindObjectsByType<PlayerReportAction>(FindObjectsSortMode.None).Where(a=>a.gameObject.activeInHierarchy).ToArray();Check(actions.Length>=ids.Length-1,"each other player's row can report");Check(actions.All(a=>a.TargetId!=PlayerProfiles.Current.id),"own report is hidden");
     foreach(var action in actions){var r=(RectTransform)action.transform;var corners=new Vector3[4];r.GetWorldCorners(corners);Check(corners[2].x>corners[0].x&&corners[2].y>corners[0].y,"row hit target has area");if(scoreboard)Check(!Overlap(r,r.parent.Find("Player name") as RectTransform),"report clears player name");}
     if(scoreboard)LabelsFit(scoreboard.Details.Panel.gameObject);yield return Capture("placement-"+GameLocalization.Code+"-"+size.x);
     if(actions.Length>0){string expected=actions[0].TargetId;actions[0].GetComponent<Button>().onClick.Invoke();yield return null;Check(PlayerReportUI.Visible,"row opens player report/card");if(app.SelectedSport==SportId.Fishing)Button("Report player").onClick.Invoke();yield return null;Check(FindFirstObjectByType<PlayerReportUI>().GetComponentsInChildren<Text>().Any(t=>t.text==peer.name),"correct player's profile shown");Button("Close report").onClick.Invoke();yield return null;}
    }
   }
  }
  bool Overlap(RectTransform a,RectTransform b){if(!b)return false;var x=new Vector3[4];var y=new Vector3[4];a.GetWorldCorners(x);b.GetWorldCorners(y);return new Rect(x[0],x[2]-x[0]).Overlaps(new Rect(y[0],y[2]-y[0]));}
  void LabelsFit(GameObject panel){Canvas.ForceUpdateCanvases();foreach(var t in panel.GetComponentsInChildren<LocalizedText>().Where(t=>!t.UserContent&&!string.IsNullOrWhiteSpace(t.text)&&!t.canvasRenderer.cull)){Check(t.text==GameLocalization.Translate(t.Source),"translated "+t.Source);Check(t.HasUnclippedText(),"caption fits "+t.Source+" "+GameLocalization.Code);}}
  IEnumerator Capture(string name){
   yield return new WaitForEndOfFrame();var camera=Camera.main;var canvases=FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.isRootCanvas&&c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();
   foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=camera;c.planeDistance=1;}Canvas.ForceUpdateCanvases();
   var rt=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var prior=RenderTexture.active;var target=camera.targetTexture;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
   var pixels=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);pixels.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);pixels.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),pixels.EncodeToPNG());Destroy(pixels);camera.targetTexture=target;RenderTexture.active=prior;RenderTexture.ReleaseTemporary(rt);
   foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceOverlay;c.worldCamera=null;}Canvas.ForceUpdateCanvases();
  }
 }
}
#endif
