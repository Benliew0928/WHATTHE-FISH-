using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace WhatTheFish {
 public enum FootballStadiumFeedMode {Off,Live,Replay,Mvp}
 public sealed partial class FootballStadiumCelebration {
  // One offscreen render serves both displays. Layer 30 excludes the screens
  // from their own feed; layer 31 is reserved for the isolated MVP studio.
  public FootballStadiumFeedMode Mode {get;private set;}
  public bool TransitionActive {get;private set;}
  public Camera BroadcastCamera=>feedCamera;
  public string ScoreHeader=>boards!=null?boards[0].scoreboard.ScoreA.text+"  :  "+boards[0].scoreboard.ScoreB.text:"";
  public int LiveAngle {get;private set;}
  public float LiveHeaderFraction=>boards==null?0:1-boards[0].liveVideo.rectTransform.rect.height/600;
  bool cameraReady;Vector3 broadcastFocus;
  Athlete visibleActor;int visiblePreset=-1;Renderer[] visibleBody=Array.Empty<Renderer>();bool[] bodyEnabled=Array.Empty<bool>();
  static readonly Vector3[] Angles={new Vector3(21,17,-12),new Vector3(24,20,6),new Vector3(21,25,19)};

  void BuildLiveBoard(Board b,RectTransform parent){
   b.live=CoveUI.Rect(parent,"Match broadcast",0,0,1280,600);b.live.gameObject.AddComponent<RectMask2D>();
   b.liveVideo=CoveUI.Rect(b.live,"Live coverage — lower 70 percent",0,180,1280,420).gameObject.AddComponent<RawImage>();b.liveVideo.raycastTarget=false;
   b.scoreboard=SportsScoreboard.Create(b.live,SportId.Football,400,true);b.scoreboard.transform.localScale=Vector3.one*3.2f;
   CoveUI.Plate(b.live,"Coverage divider",0,177,1280,3,SportsScoreboard.Lime,0,0);
   b.replay=CoveUI.Rect(b.live,"Full-screen recorded goal replay",0,0,1280,600);
   b.replayVideo=CoveUI.Rect(b.replay,"Recorded footage",0,0,1280,600).gameObject.AddComponent<RawImage>();b.replayVideo.raycastTarget=false;
   CoveUI.Plate(b.replay,"Replay bug",26,22,208,48,CoveUI.Ink,4,0);
   CoveUI.Text(b.replay,"●  REPLAY",42,26,180,38,28,true,CoveUI.Cream);
   b.replayTime=CoveUI.Text(b.replay,"",960,23,290,42,25,true,CoveUI.Cream,TextAnchor.MiddleRight);b.replayTime.gameObject.AddComponent<Outline>().effectColor=CoveUI.Ink;
   b.wipe=CoveUI.Rect(b.live,"Full-screen goal transition",0,0,1280,600);b.wipe.gameObject.AddComponent<FootballSlashGraphic>().raycastTarget=false;
   CoveUI.Text(b.wipe,"SUNVALE  /  THE MOMENT",230,80,820,50,31,true,CoveUI.Mint,TextAnchor.MiddleCenter);
   CoveUI.Text(b.wipe,"GOAL",170,148,940,230,190,true,CoveUI.Cream,TextAnchor.MiddleCenter);
   b.goalScore=CoveUI.Text(b.wipe,"",220,391,840,68,48,true,CoveUI.Gold,TextAnchor.MiddleCenter);
   b.goalCaption=CoveUI.Text(b.wipe,"",220,494,840,48,26,true,CoveUI.Mint,TextAnchor.MiddleCenter);
   b.mvp.gameObject.SetActive(false);b.replay.gameObject.SetActive(false);b.wipe.gameObject.SetActive(false);
  }

  void CreateMatchCamera(){
   feedCamera=new GameObject("Stadium ball-following broadcast camera").AddComponent<Camera>();feedCamera.transform.SetParent(root,false);
   feedCamera.enabled=false;feedCamera.nearClipPlane=.15f;feedCamera.farClipPlane=220;feedCamera.clearFlags=CameraClearFlags.Skybox;
   feedCamera.cullingMask=~((1<<ScreenLayer)|(1<<FeedLayer)|(1<<2));feedCamera.allowHDR=feedCamera.allowMSAA=false;
   var data=feedCamera.gameObject.AddComponent<UniversalAdditionalCameraData>();data.renderPostProcessing=data.renderShadows=false;data.requiresColorOption=data.requiresDepthOption=CameraOverrideOption.Off;
   cameraReady=false;nextFeed=0;FeedFrames=0;
  }
  void MatchTexture(bool replay){
   int height=replay?480:336;
   if(SystemInfo.graphicsDeviceType==GraphicsDeviceType.Null||feed&&feed.height==height)return;
   if(feed){feed.Release();Destroy(feed);}
   feed=new RenderTexture(1024,height,24,RenderTextureFormat.ARGB32){name="Stadium shared match broadcast",filterMode=FilterMode.Bilinear,antiAliasing=1};feed.Create();nextFeed=0;
   foreach(var b in boards){b.liveVideo.texture=b.replayVideo.texture=feed;}
  }
  void PresentMatch(){
   if(!feedCamera)CreateMatchCamera();
   var state=match.Snapshot;double age=state.goalId==0?-1:match.Now-state.goalAt;
   bool sequence=match.GoalSequence;
   // Resize and exchange camera views only beneath the opaque middle of each
   // wipe. The replay owns the existing visual copies, so nothing is simulated twice.
   bool replay=sequence&&age>=FootballMatch.ReplayEnterHandoff&&age<FootballMatch.ReplayReturnHandoff;
   Mode=replay?FootballStadiumFeedMode.Replay:FootballStadiumFeedMode.Live;MatchTexture(replay);
   feedCamera.aspect=replay?1280f/600:1280f/420;
   var recorded=FootballReplay.Instance;Pose pose=default;float fov=54;bool available=replay&&recorded&&recorded.TryGetBroadcastView(out pose,out fov);
   if(available){
    feedCamera.transform.SetPositionAndRotation(pose.position,pose.rotation);feedCamera.fieldOfView=fov;cameraReady=false;
   }else FrameLiveCamera();
   double elapsed=age-FootballMatch.ReplayCaptureSeconds;bool returning=age>=FootballMatch.ReplayEndOffset;
   TransitionActive=sequence&&(elapsed>=0&&elapsed<FootballMatch.ReplayWipeSeconds||returning);
   float shift=0;
   if(TransitionActive){
    if(returning)elapsed=age-FootballMatch.ReplayEndOffset;
    double enter=returning?FootballMatch.ReplayReturnInSeconds:FootballMatch.ReplayWipeInSeconds,hold=returning?FootballMatch.ReplayReturnHoldSeconds:FootballMatch.ReplayWipeHoldSeconds,leave=returning?FootballMatch.ReplayReturnOutSeconds:FootballMatch.ReplayWipeOutSeconds;
    shift=elapsed<enter?Mathf.Lerp(-2.1f,0,Mathf.SmoothStep(0,1,(float)(elapsed/enter))):elapsed<enter+hold?0:Mathf.Lerp(0,2.1f,Mathf.SmoothStep(0,1,(float)((elapsed-enter-hold)/leave)));
   }
   // Missing history (e.g. joining just after a goal) gets an honest goal card,
   // never a live picture labelled as a replay.
   bool missing=replay&&!available&&!TransitionActive;
   foreach(var b in boards){
    b.mvp.gameObject.SetActive(false);b.live.gameObject.SetActive(true);
    b.replay.gameObject.SetActive(replay);b.replayTime.text=Mathf.Clamp((float)(age-FootballMatch.ReplayStartOffset),0,5).ToString("0.0")+" / 5.0";
    b.wipe.gameObject.SetActive(TransitionActive||missing);b.wipe.anchoredPosition=Vector2.right*(missing?0:shift*1280);
    b.goalScore.text="BLUE   "+state.scoreA+"  :  "+state.scoreB+"   RED";
    b.goalCaption.text=missing?"REPLAY UNAVAILABLE  /  RETURNING TO THE MATCH":returning?(state.phase==FootballMatchPhase.Finished?"FULL TIME":CharacterIdentity.TeamName(state.kickoffTeam).ToUpperInvariant()+"  /  CENTRE KICKOFF"):"WATCH IT AGAIN  /  GOAL REPLAY";
   }
   RenderFeed();
  }
  string MatchStatus(FootballMatchSnapshot s){
   if(match.GoalSequence)return "GOAL  /  "+CharacterIdentity.TeamName(s.kickoffTeam==FootballTeam.A?FootballTeam.B:FootballTeam.A).ToUpperInvariant();
   if(s.phase==FootballMatchPhase.Idle||s.phase==FootballMatchPhase.Cancelled)return "PITCH LIVE  /  WELCOME TO SUNVALE";
   if(s.phase==FootballMatchPhase.TeamSelection)return "PRE-MATCH  /  TEAMS GETTING READY";
   if(s.phase==FootballMatchPhase.Kickoff)return CharacterIdentity.TeamName(s.kickoffTeam).ToUpperInvariant()+"  /  CENTRE KICKOFF";
   int remaining=Mathf.CeilToInt((float)match.Remaining);
   return (s.phase==FootballMatchPhase.Overtime?"EXTRA TIME":"MATCH LIVE")+"   /   "+(remaining/60).ToString("00")+":"+(remaining%60).ToString("00");
  }
  void FrameLiveCamera(){
   var pitch=ball.Pitch;var bounds=ball.PitchBounds;var p=pitch.InverseTransformPoint(ball.transform.position);
   var v=ball.Body?pitch.InverseTransformDirection(ball.Body.linearVelocity):Vector3.zero;
   float ground=bounds.max.y;
   // Retain an inward sightline at the near touchline. Looking almost straight
   // down would let a change of end angle spin the shot around the ball.
   var aim=new Vector3(Mathf.Clamp(p.x+Mathf.Clamp(v.x*.16f,-3,3),bounds.min.x+2,bounds.max.x-7),ground+1.4f+Mathf.Clamp((p.y-ground)*.55f,0,12),Mathf.Clamp(p.z+Mathf.Clamp(v.z*.16f,-3,3),bounds.min.z+3,bounds.max.z-3));
   var network=Unity.Netcode.NetworkManager.Singleton;double now=network&&network.IsListening?RoomService.GameClock(network.ServerTime.Time):Time.timeAsDouble;
   double cycle=now/14;LiveAngle=(int)(cycle%Angles.Length);float blend=Mathf.SmoothStep(0,1,Mathf.Clamp01((float)(cycle-Math.Floor(cycle))*14/2.8f));
   var offset=Vector3.Lerp(Angles[(LiveAngle+Angles.Length-1)%Angles.Length],Angles[LiveAngle],blend);
   offset.y+=8*(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(0,12,bounds.max.x-p.x)));
   var desired=aim+offset;desired.x=Mathf.Clamp(desired.x,bounds.min.x+1,bounds.max.x-1);desired.z=Mathf.Clamp(desired.z,bounds.min.z+1,bounds.max.z-1);
   float follow=1-Mathf.Exp(-Time.unscaledDeltaTime*6);
   var position=pitch.TransformPoint(desired);var focus=pitch.TransformPoint(aim);
   broadcastFocus=cameraReady?Vector3.Lerp(broadcastFocus,focus,follow):focus;
   feedCamera.transform.position=cameraReady?Vector3.Lerp(feedCamera.transform.position,position,follow):position;
   feedCamera.fieldOfView=44;feedCamera.transform.LookAt(broadcastFocus);
   // High lobs and abrupt authoritative corrections must remain in frame.
   var ballView=feedCamera.WorldToViewportPoint(ball.transform.position);
   if(ballView.z<=0||ballView.x<.1f||ballView.x>.9f||ballView.y<.13f||ballView.y>.9f){
    broadcastFocus=focus;feedCamera.transform.position=position;feedCamera.transform.LookAt(focus);
    ballView=feedCamera.WorldToViewportPoint(ball.transform.position);
    if(ballView.z<=0||ballView.y<.1f||ballView.y>.9f){broadcastFocus=ball.transform.position;feedCamera.transform.LookAt(broadcastFocus);}
   }
   cameraReady=true;
  }
  void RenderFeed(){
   if(!feedCamera||!feed||Time.unscaledTime<nextFeed)return;
   nextFeed=Time.unscaledTime+.05f;var request=new UniversalRenderPipeline.SingleCameraRequest{destination=feed};
   if(!RenderPipeline.SupportsRenderRequest(feedCamera,request))return;
   // First-person hides the local body for the player's camera only. Restore
   // exactly those renderer flags after the independent broadcast render.
   var actor=AppRoot.Instance?AppRoot.Instance.LocalAthlete:null;var identity=actor?actor.GetComponent<CharacterIdentity>():null;int preset=identity?identity.Preset:0;
   bool reveal=Mode==FootballStadiumFeedMode.Live&&!FootballReplay.Presenting&&actor&&actor.visual;
   if(reveal){
    if(visibleActor!=actor||visiblePreset!=preset){visibleActor=actor;visiblePreset=preset;visibleBody=actor.visual.GetComponentsInChildren<SkinnedMeshRenderer>(true).Cast<Renderer>().Concat(identity&&preset!=0?identity.AddedRenderers:Array.Empty<Renderer>()).Where(r=>r).ToArray();bodyEnabled=new bool[visibleBody.Length];}
    for(int i=0;i<visibleBody.Length;i++)if(visibleBody[i]){bodyEnabled[i]=visibleBody[i].enabled;visibleBody[i].enabled=true;}
   }
   try{RenderPipeline.SubmitRenderRequest(feedCamera,request);FeedFrames++;}
   finally{if(reveal)for(int i=0;i<visibleBody.Length;i++)if(visibleBody[i])visibleBody[i].enabled=bodyEnabled[i];}
  }
 }
}
