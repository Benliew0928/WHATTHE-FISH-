#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 public sealed class FootballBroadcastReview:MonoBehaviour {
  string folder;bool failed,frames,record;int frame;float nextCapture;AppRoot app;FootballMatch match;FootballBall ball;FootballStadiumCelebration show;Camera boardCamera;RenderTexture boardImage;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-footballBroadcastReview");if(i>=0&&i+1<args.Length)new GameObject("Football broadcast review").AddComponent<FootballBroadcastReview>().folder=args[i+1];}
  void Check(bool ok,string label){failed|=!ok;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+label+"\n");}
  IEnumerator Until(Func<bool> condition,string label,float seconds=35){float end=Time.realtimeSinceStartup+seconds;while(!condition()&&Time.realtimeSinceStartup<end)yield return null;Check(condition(),label);}
  void Bots(bool active){foreach(var a in FootballBots.Roster){var bot=a.GetComponent<FootballBot>();if(bot)bot.enabled=active;}}
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");frames=Environment.GetCommandLineArgs().Contains("-broadcastFrames");Application.targetFrameRate=60;
   yield return Until(()=>DevelopmentProbe.ReadyToReviewSport&&FootballMatch.Instance&&FootballStadiumCelebration.Instance,"football ready",100);
   app=AppRoot.Instance;match=FootballMatch.Instance;ball=FootballBall.Instance;show=FootballStadiumCelebration.Instance;if(!app||!match||!ball||!show){Finish();yield break;}
   DevelopmentProbe.Driving=false;DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;
   yield return Until(()=>show.Mode==FootballStadiumFeedMode.Live&&show.FeedFrames>2,"idle pitch has live footage");
   Check(show.ScreenCount==2&&Mathf.Abs(show.LiveHeaderFraction-.3f)<.001f,"two screens with 30 percent header and 70 percent footage");
   Check(show.Screens.GetComponentsInChildren<RawImage>().Count(v=>v.enabled&&v.texture==show.Footage)==2,"both screens share one render texture");
   Check(!show.Screens.GetComponentsInChildren<Text>().Any(t=>t.text=="PLAYER OF THE MATCH"),"MVP labels hidden outside celebration");
   Check((show.BroadcastCamera.cullingMask&(1<<30|1<<31))==0,"live feed excludes screens and isolated MVP studio");
   BuildCapture();SaveBoard("idle-live");
   match.rules=Instantiate(match.rules);match.rules.teamSelectionSeconds=.2f;match.rules.kickoffSeconds=.1f;
   Check(FootballBots.Instance.StartSolo(5),"5v5 broadcast fixture");yield return null;Check(match.StartMatch(),"match starts");match.RequestTeam(FootballTeam.A);Check(match.ConfirmSoloTeam(),"confirm teams");Bots(false);
   yield return Until(()=>match.KickoffReady,"kickoff ready");yield return new WaitForEndOfFrame();Check(show.Mode==FootballStadiumFeedMode.Live&&show.ScoreHeader=="0  :  0","kickoff keeps live coverage and score");SaveBoard("kickoff-live");
   Check(match.TryKickoff(match.KickoffActor,90,.2f,match.Snapshot.revision),"kickoff released");
   // Continuous passes, high lobs and all four extremes of the field exercise
   // framing while the director changes angles on the shared clock.
   float started=Time.unscaledTime;var angles=new HashSet<int>();bool framed=true;float maximumTurn=0;Quaternion previous=show.BroadcastCamera.transform.rotation;var trace=new System.Text.StringBuilder("time,angle,ballX,ballY,cameraX,cameraY,cameraZ,rotationStep\n");
   while(Time.unscaledTime-started<60){
    float t=Time.unscaledTime-started;var bounds=ball.PitchBounds;
    var point=new Vector3(bounds.center.x+bounds.extents.x*.86f*Mathf.Sin(t*.37f),bounds.max.y+ball.WorldRadius+7*Mathf.Pow(Mathf.Sin(t*.5f),2),bounds.center.z+bounds.extents.z*.88f*Mathf.Sin(t*.23f));
    if(t>=30)point=new Vector3(bounds.max.x-ball.WorldRadius-.1f,bounds.max.y+ball.WorldRadius,bounds.center.z+bounds.extents.z*.85f*Mathf.Sin((t-30)*.15f));
    ball.ReviewFootballBall(ball.Pitch.TransformPoint(point),Vector3.zero);
    yield return new WaitForEndOfFrame();var cam=show.BroadcastCamera;var p=cam.WorldToViewportPoint(ball.transform.position);framed&=p.z>0&&p.x>.04f&&p.x<.96f&&p.y>.04f&&p.y<.96f;
    float turn=Quaternion.Angle(previous,cam.transform.rotation);if(t>1&&(t<30||t>31))maximumTurn=Mathf.Max(maximumTurn,turn);previous=cam.transform.rotation;
    trace.AppendLine(FormattableString.Invariant($"{t:F4},{show.LiveAngle},{p.x:F4},{p.y:F4},{cam.transform.position.x:F3},{cam.transform.position.y:F3},{cam.transform.position.z:F3},{turn:F3}"));
    if(angles.Add(show.LiveAngle))SaveBoard("live-angle-"+show.LiveAngle);
   }
   File.WriteAllText(Path.Combine(folder,"live-camera.csv"),trace.ToString());Check(framed,"ball stays inside frame across field, high lobs and near touchline");Check(angles.Count==3,"all three director angles exercised");Check(maximumTurn<7,"no abrupt live or touchline camera rotation "+maximumTurn.ToString("F2"));
   app.view.mode=0;yield return new WaitForSeconds(.3f);Check(app.LocalAthlete.visual.GetComponentsInChildren<SkinnedMeshRenderer>(true).All(r=>!r.enabled),"broadcast restores first-person body visibility flags");app.view.mode=1;
   ball.ReviewFootballBall(ball.Pitch.TransformPoint(new Vector3(0,ball.PitchBounds.max.y+ball.WorldRadius,0)),ball.Pitch.forward*3);Bots(true);
   record=frames;nextCapture=0;StartCoroutine(RecordBoards());yield return new WaitForSeconds(5.5f);Bots(false);
   if(!match.Running){yield return Until(()=>match.KickoffReady,"AI goal restart");match.TryKickoff(match.KickoffActor,90,.2f,match.Snapshot.revision);yield return new WaitForSeconds(4.4f);}
   int index=Enumerable.Range(0,ball.GoalCount).First(i=>ball.GoalSign(i)==1);var goal=ball.GoalBounds(index);int before=match.Snapshot.scoreA;
   ball.ReviewFootballBall(ball.Pitch.TransformPoint(new Vector3(goal.center.x+.6f,ball.PitchBounds.max.y+ball.WorldRadius+.05f,ball.GoalFront(index)-5)),ball.Pitch.forward*12);match.ResetAbilityGoalTracking();
   yield return Until(()=>match.GoalSequence,"physical goal triggers broadcast");
   yield return Until(()=>match.Now-match.Snapshot.goalAt>1.7,"goal wipe reaches opaque hold");yield return new WaitForEndOfFrame();Check(show.TransitionActive&&show.ScoreHeader==(before+1)+"  :  "+match.Snapshot.scoreB,"full-screen goal wipe carries updated score");SaveBoard("goal-transition");
   yield return Until(()=>match.Now-match.Snapshot.goalAt>3.1,"recorded replay begins");yield return new WaitForEndOfFrame();Check(show.Mode==FootballStadiumFeedMode.Replay&&FootballReplay.Instance.HasFootage&&!show.TransitionActive,"full screen shows genuine recorded replay");
   Check(show.Footage.height==480&&Mathf.Abs(show.BroadcastCamera.aspect-1280f/600)<.001f,"replay uses full display aspect ratio");
   Check(show.Screens.GetComponentsInChildren<RawImage>().Count(v=>v.rectTransform.rect.height==600&&v.texture==show.Footage)==2,"both screens have full-height replay footage");
   var stage=GameObject.Find("Goal replay · visual copies only");var copy=stage?stage.transform.Cast<Transform>().FirstOrDefault(t=>t.name==ball.name):null;var view=copy?show.BroadcastCamera.WorldToViewportPoint(copy.position):Vector3.zero;
   Check(copy&&view.z>0&&view.x>.04f&&view.x<.96f&&view.y>.04f&&view.y<.96f,"recorded ball is visible on giant screen");SaveBoard("goal-replay");
   yield return Until(()=>match.Now-match.Snapshot.goalAt>7.3,"replay reaches goal impact");SaveBoard("goal-impact");
   yield return Until(()=>!match.GoalSequence,"replay returns to pitch");yield return new WaitForEndOfFrame();Check(show.Mode==FootballStadiumFeedMode.Live&&!show.TransitionActive&&show.Footage.height==336,"live layout restored after return wipe");SaveBoard("return-live");yield return new WaitForSeconds(1);record=false;
   var old=show.Footage;show.enabled=false;yield return null;Check(!show.Footage&&!old&&show.Mode==FootballStadiumFeedMode.Off,"disable releases live camera and texture");show.enabled=true;yield return Until(()=>show.Mode==FootballStadiumFeedMode.Live&&show.FeedFrames>1,"re-enable resumes live programme");
   match.ClearMatch();yield return new WaitForEndOfFrame();Check(show.ScoreHeader=="0  :  0"&&show.Mode==FootballStadiumFeedMode.Live,"new round clears score and retains live pitch");
   yield return null;Check(match.StartMatch(),"missing-history fixture starts");match.RequestTeam(FootballTeam.A);Check(match.ConfirmSoloTeam(),"confirm missing-history teams");Bots(false);yield return Until(()=>match.KickoffReady,"missing-history kickoff ready");match.TryKickoff(match.KickoffActor,90,.2f,match.Snapshot.revision);
   ball.ReviewFootballBall(ball.Pitch.TransformPoint(new Vector3(goal.center.x+.6f,ball.PitchBounds.max.y+ball.WorldRadius+.05f,ball.GoalFront(index)-5)),ball.Pitch.forward*12);match.ResetAbilityGoalTracking();
   yield return Until(()=>match.GoalSequence,"early goal before four seconds of history");yield return Until(()=>match.Now-match.Snapshot.goalAt>3.2,"missing-history replay slot");yield return new WaitForEndOfFrame();
   Check(!FootballReplay.Instance.HasFootage&&show.Mode==FootballStadiumFeedMode.Replay&&show.Screens.GetComponentsInChildren<Text>().Any(t=>t.text.StartsWith("REPLAY UNAVAILABLE")),"missing history shows explicit full-screen goal card");SaveBoard("missing-history");
   match.ClearMatch();yield return new WaitForEndOfFrame();Check(show.Mode==FootballStadiumFeedMode.Live&&!show.TransitionActive,"cancellation immediately restores live coverage");
   app.LeaveCourt();yield return null;yield return new WaitForEndOfFrame();Check(show.Mode==FootballStadiumFeedMode.Off&&!show.Footage&&!show.Screens.gameObject.activeSelf,"leaving football releases broadcast resources");Finish();
  }
  void BuildCapture(){
   boardCamera=new GameObject("Review screen camera").AddComponent<Camera>();boardCamera.CopyFrom(Camera.main);boardCamera.enabled=false;boardCamera.orthographic=true;boardCamera.orthographicSize=5.625f;boardCamera.aspect=1280f/600;
   var board=show.Screens.GetChild(0);boardCamera.transform.position=board.TransformPoint(new Vector3(0,0,-28));boardCamera.transform.LookAt(board.position);boardImage=new RenderTexture(1280,600,24);boardCamera.targetTexture=boardImage;
  }
  IEnumerator RecordBoards(){while(record){yield return new WaitForEndOfFrame();if(Time.unscaledTime>=nextCapture){nextCapture=Time.unscaledTime+.125f;File.AppendAllText(Path.Combine(folder,"frame-times.csv"),Time.unscaledTime.ToString("F5",System.Globalization.CultureInfo.InvariantCulture)+"\n");SaveBoard("frames/frame-"+(frame++).ToString("0000"));}}}
  void SaveBoard(string name){
   Canvas.ForceUpdateCanvases();boardCamera.Render();var previous=RenderTexture.active;RenderTexture.active=boardImage;var tex=new Texture2D(1280,600,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1280,600),0,0);tex.Apply();string path=Path.Combine(folder,name+".png");Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllBytes(path,tex.EncodeToPNG());Destroy(tex);RenderTexture.active=previous;
  }
  void Finish(){record=false;if(boardCamera)Destroy(boardCamera.gameObject);if(boardImage){boardImage.Release();Destroy(boardImage);}File.AppendAllText(Path.Combine(folder,"results.txt"),"FOOTBALL_BROADCAST_COMPLETE success="+!failed+"\n");Application.Quit(failed?1:0);}
 }
}
#endif
