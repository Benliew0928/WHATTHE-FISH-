using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace WhatTheFish {
 // The arena broadcast has its own lifetime. Dismissing or skipping a local
 // results page never stops the stadium show for the other room members.
 [DefaultExecutionOrder(450)]
 public sealed partial class FootballStadiumCelebration:MonoBehaviour {
  public static FootballStadiumCelebration Instance {get;private set;}
  public const float TeamSeconds=15,MvpSeconds=10;
  const int ScreenLayer=30,FeedLayer=31;
  FootballMatch match;FootballBall ball;Transform root,studio,venueScreen;FootballCelebrationActor performer;
  FootballTake take;Camera feedCamera;RenderTexture feed;FootballStadiumFireworks fireworks;
  Board[] boards;uint round=uint.MaxValue;bool finished;float nextFeed;
  public bool BroadcastActive=>finished&&Age<25;
  public float Age=>SharedAge(match);
  public bool FireworksActive=>fireworks!=null&&fireworks.VisibleParticles>0;
  public int FireworkParticles=>fireworks?.VisibleParticles??0;
  public int FlameJets=>fireworks?.FlameJets??0;
  public int LaserBeams=>fireworks?.LaserBeams??0;
  public int ElectricArcs=>fireworks?.ElectricArcs??0;
  public Transform EffectsRoot=>fireworks?.Root;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
  public void ReviewEffects(float age,uint revision,Camera camera)=>fireworks.Present(age,revision,camera);
#endif
  public int ScreenCount=>boards?.Length??0;
  public int FeedFrames {get;private set;}
  public RenderTexture Footage=>feed;
  public string Headline=>boards!=null?boards[0].headline.text:"";
  public Transform Screens=>root;
  public static float SharedAge(FootballMatch value){
   if(!value)return 0;var state=value.Snapshot;
   double start=state.deadline;
   if(state.goalId>0)start=System.Math.Max(start,state.goalAt+FootballMatch.GoalSequenceSeconds);
   // The match stamp is authoritative, but presentation uses the render-frame
   // clock so a 50 Hz physics tick cannot introduce steps into the camera.
   var network=Unity.Netcode.NetworkManager.Singleton;
   double now=network&&network.IsListening?RoomService.GameClock(network.ServerTime.Time):Time.timeAsDouble;
   return Mathf.Max(0,(float)(now-start));
  }
  sealed class Board {
   public SportsScoreboard scoreboard;
   public Transform root;public RectTransform mvp,live,replay,wipe;
   public Text headline,score,name,goals,assists,detail,phase,liveScore,liveStatus,replayTime,goalScore,goalCaption;
   public RawImage video,liveVideo,replayVideo;
  }
  void Awake(){Instance=this;match=GetComponent<FootballMatch>();ball=GetComponent<FootballBall>();}
  void Build(){
   root=new GameObject("Stadium live broadcast displays").transform;root.SetParent(ball.Pitch,false);
   venueScreen=ball.Pitch.Find("Stadium screen");
   boards=new[]{BuildBoard(-1),BuildBoard(1)};
   fireworks=new FootballStadiumFireworks(ball.Pitch,Resources.Load<Material>("FootballStadiumFireworks"));
  }
  Board BuildBoard(int end){
   var mount=new GameObject(end<0?"North giant screen":"South giant screen").transform;mount.SetParent(root,false);
   mount.localPosition=new Vector3(0,25,end*73.4f);mount.localRotation=Quaternion.Euler(0,end<0?180:0,0);
   var frame=Resources.Load<Material>("FootballStadiumScreenFrame");
   void Frame(string name,Vector3 position,Vector3 scale){var part=GameObject.CreatePrimitive(PrimitiveType.Cube);part.name=name;part.transform.SetParent(mount,false);part.transform.localPosition=position;part.transform.localScale=scale;Destroy(part.GetComponent<Collider>());var renderer=part.GetComponent<MeshRenderer>();renderer.sharedMaterial=frame;renderer.shadowCastingMode=ShadowCastingMode.Off;}
   Frame("Solid screen chassis",new Vector3(0,0,.5f),new Vector3(24.8f,11.8f,.8f));
   Frame("West support pylon",new Vector3(-9,-11,.7f),new Vector3(.65f,12,.8f));Frame("East support pylon",new Vector3(9,-11,.7f),new Vector3(.65f,12,.8f));
   Frame("Lower cross brace",new Vector3(0,-5.9f,.65f),new Vector3(24.8f,.4f,.8f));
   // A modular 24 m screen uses the existing scoreboard roof line. The
   // world-space frame needs no new art dependency or floor collider.
   var canvas=new GameObject("LED broadcast",typeof(RectTransform),typeof(Canvas)).GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;
   var rect=(RectTransform)canvas.transform;rect.SetParent(mount,false);rect.sizeDelta=new Vector2(1280,600);rect.localScale=Vector3.one*(24f/1280);
   CoveUI.Plate(rect,"Bronze outer frame",-16,-16,1312,632,CoveUI.Gold,20,0);
   CoveUI.Plate(rect,"Deep teal chassis",-7,-7,1294,614,CoveUI.Ink,15,0);
   CoveUI.Plate(rect,"LED black",0,0,1280,600,new Color(.018f,.045f,.065f),12,0);
   rect=CoveUI.Rect(rect,"Post-match MVP programme",0,0,1280,600);
   CoveUI.Plate(rect,"Upper mint trim",22,18,1236,5,CoveUI.Mint,0,0);
   var board=new Board{root=mount,mvp=rect};
   board.headline=CoveUI.Text(rect,"",32,35,950,78,58,true,CoveUI.Cream);
   board.score=CoveUI.Text(rect,"",990,35,255,78,64,true,CoveUI.Gold,TextAnchor.MiddleRight);
   CoveUI.Plate(rect,"Footage surround",30,132,485,420,new Color(.05f,.16f,.20f),18,0);
   board.video=CoveUI.Rect(rect,"Live MVP camera",42,142,461,395).gameObject.AddComponent<RawImage>();board.video.raycastTarget=false;
   CoveUI.Text(rect,"PLAYER OF THE MATCH",554,142,685,42,30,true,CoveUI.Mint);
   board.name=CoveUI.Text(rect,"",550,184,690,65,44,true,CoveUI.Cream);board.name.resizeTextForBestFit=true;board.name.resizeTextMinSize=26;board.name.resizeTextMaxSize=44;
   CoveUI.Text(rect,"GOALS",556,265,290,35,28,true,CoveUI.Gold);CoveUI.Text(rect,"ASSISTS",906,265,320,35,28,true,CoveUI.Gold);
   board.goals=CoveUI.Text(rect,"",551,297,285,120,100,true,CoveUI.Cream);board.assists=CoveUI.Text(rect,"",901,297,325,120,100,true,CoveUI.Cream);
   board.detail=CoveUI.Text(rect,"",554,429,682,72,28,true,CoveUI.Mint);
   board.phase=CoveUI.Text(rect,"",554,518,682,44,28,true,CoveUI.Gold);
   CoveUI.Text(rect,"SUNVALE  /  LIVE",34,555,470,30,21,true,CoveUI.Mint);
   BuildLiveBoard(board,(RectTransform)canvas.transform);
   foreach(var t in mount.GetComponentsInChildren<Transform>(true))t.gameObject.layer=ScreenLayer;
   foreach(var text in mount.GetComponentsInChildren<Text>(true)){text.verticalOverflow=VerticalWrapMode.Overflow;text.supportRichText=false;}
   return board;
  }
  void Begin(){
   ReleaseFeed();finished=true;Mode=FootballStadiumFeedMode.Mvp;round=match.Snapshot.revision;var winner=match.MVP;
   var source=Athlete.Active.FirstOrDefault(a=>a&&a.visual&&FootballBots.Id(a)==winner.id);
   if(!source)source=AppRoot.Instance?AppRoot.Instance.LocalAthlete:null;
   if(source){
    studio=new GameObject("Isolated MVP broadcast studio").transform;studio.SetParent(root,false);studio.localPosition=new Vector3(0,-80,0);
    performer=new FootballCelebrationActor(source,winner,studio);
    foreach(var t in studio.GetComponentsInChildren<Transform>(true))t.gameObject.layer=FeedLayer;
    var library=Resources.Load<FootballMotionLibrary>("FootballMotionLibrary");take=library?library.Find("Celebrate_MVP"):FootballCelebrationPoses.Defaults()[3];
    if(SystemInfo.graphicsDeviceType!=GraphicsDeviceType.Null){
     feed=new RenderTexture(640,548,24,RenderTextureFormat.ARGB32){name="MVP stadium live feed",filterMode=FilterMode.Bilinear,antiAliasing=1};feed.Create();
     feedCamera=new GameObject("MVP feed camera").AddComponent<Camera>();feedCamera.transform.SetParent(studio,false);feedCamera.transform.localPosition=new Vector3(0,1.07f,3.5f);feedCamera.transform.LookAt(studio.TransformPoint(new Vector3(0,.9f,0)));
     feedCamera.enabled=false;feedCamera.fieldOfView=35;feedCamera.aspect=640f/548;feedCamera.nearClipPlane=.1f;feedCamera.farClipPlane=10;feedCamera.clearFlags=CameraClearFlags.SolidColor;feedCamera.backgroundColor=new Color(.035f,.12f,.16f);feedCamera.cullingMask=1<<FeedLayer;feedCamera.allowHDR=feedCamera.allowMSAA=false;
     var data=feedCamera.gameObject.AddComponent<UniversalAdditionalCameraData>();data.renderPostProcessing=data.renderShadows=false;data.requiresColorOption=data.requiresDepthOption=CameraOverrideOption.Off;
    }
   }
   foreach(var b in boards){b.mvp.gameObject.SetActive(true);b.live.gameObject.SetActive(false);b.name.text="#"+winner.number+"  "+winner.name;b.score.text=match.Snapshot.scoreA+" : "+match.Snapshot.scoreB;b.goals.text=winner.goals.ToString();b.assists.text=winner.assists.ToString();b.detail.text=winner.passes+" PASSES    "+winner.interceptions+" INTERCEPTIONS\n"+winner.Points+" MVP POINTS";b.video.texture=feed;b.video.enabled=feed;}
   nextFeed=0;FeedFrames=0;
  }
  void LateUpdate(){
   if(!ball||!ball.Pitch)return;if(!root)Build();
   var app=AppRoot.Instance;
   bool customizing=app&&app.Menu&&app.Menu.Page=="venues"&&app.Menu.Content.gameObject.activeInHierarchy;
   bool context=match&&match.Context&&!customizing;root.gameObject.SetActive(context);fireworks.SetVisible(context);
   // The editable sign owns the screen outside match coverage. Do this only
   // during normal presentation, never in teardown callbacks during unload.
   if(venueScreen&&venueScreen.gameObject.activeSelf==context)venueScreen.gameObject.SetActive(!context);
   if(!context){finished=false;ReleaseFeed();fireworks.Present(-1,0,null);return;}
   bool result=context&&match.Snapshot.phase==FootballMatchPhase.Finished&&!match.GoalSequence&&match.PlayerRecords.Any();
   if(!result){if(finished){finished=false;ReleaseFeed();}fireworks.Present(-1,0,Camera.main);PresentMatch();return;}
   if(!finished||round!=match.Snapshot.revision)Begin();
   float age=Age;var state=match.Snapshot;
   string headline=age<TeamSeconds?(state.result==FootballMatchResult.Draw?"HONOURS SHARED":state.result==FootballMatchResult.TeamA?"BLUE TAKES THE WIN":"RED TAKES THE WIN"):"MOST VALUABLE PLAYER";
   foreach(var b in boards){b.headline.text=headline;b.phase.text=age<TeamSeconds?"FULL TIME  /  CELEBRATE TOGETHER":age<TeamSeconds+MvpSeconds?"MVP SPOTLIGHT  /  LIVE":"FULL TIME  /  THANK YOU FOR PLAYING";}
   fireworks.Present(age,state.revision,Camera.main);
   if(performer!=null&&performer.Ready&&take!=null)performer.Pose(take.Sample(Mathf.Max(0,age-14)/take.duration));
   RenderFeed();
  }
  void ReleaseFeed(){if(feedCamera){feedCamera.gameObject.SetActive(false);Destroy(feedCamera.gameObject);}if(studio){studio.gameObject.SetActive(false);Destroy(studio.gameObject);}studio=null;performer=null;feedCamera=null;if(feed){feed.Release();Destroy(feed);}feed=null;Mode=FootballStadiumFeedMode.Off;cameraReady=false;TransitionActive=false;}
  void OnDisable(){finished=false;ReleaseFeed();if(root)root.gameObject.SetActive(false);fireworks?.Present(-1,0,null);fireworks?.SetVisible(false);}
  // The original sign belongs to the arena. Reactivating it here can target a
  // GameObject already being destroyed during island unload or application exit.
  void OnDestroy(){ReleaseFeed();fireworks?.Dispose();if(root)Destroy(root.gameObject);if(Instance==this)Instance=null;}
 }
}
