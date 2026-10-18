using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace WhatTheFish {
 [DefaultExecutionOrder(250)]
 public sealed class FootballReplay:MonoBehaviour {
  public const int Capacity=240;
  public static FootballReplay Instance {get;private set;}
  public static bool Presenting=>Instance&&Instance.presenting;
  public bool HasFootage {get;private set;}
  public double FootageStart {get;private set;}
  public double FootageEnd {get;private set;}
  public double PlaybackTime {get;private set;}
  public int RecordedFrames=>count;
  public int ReplayCount {get;private set;}
  public bool TryGetBroadcastView(out Pose pose,out float fov){
   pose=default;fov=54;if(!presenting||!HasFootage||!camera)return false;
   pose=new Pose(camera.transform.position,camera.transform.rotation);fov=camera.fieldOfView;return true;
  }
  readonly double[] times=new double[Capacity],clipTimes=new double[Capacity];readonly List<FootballReplayTrack> tracks=new();
  readonly List<FootballAudioEvent> sounds=new(),clipSounds=new();
  FootballMatch match;FootballBall ball;FootballReplayTrack ballTrack;FootballReplayHUD hud;
  int head,count,clipHead,clipCount,roster;uint goal;double nextRecord,lastSound;bool presenting;Transform stage;
  Camera camera;float fieldOfView,near;bool orthographic;Vector3 cameraSide,goalMouth;float cameraDistance;Vector3 cameraFocus;
  void Awake(){Instance=this;match=GetComponent<FootballMatch>();ball=GetComponent<FootballBall>();hud=FootballReplayHUD.Create();}
  void OnDisable(){Restore();ResetHistory();if(hud)hud.gameObject.SetActive(false);if(Instance==this)Instance=null;}
  void OnEnable(){Instance=this;if(hud)hud.gameObject.SetActive(true);}
  void OnDestroy(){if(hud)Destroy(hud.gameObject);}
  void OnApplicationPause(bool paused){if(paused){Restore();ResetHistory();}}
  void ResetHistory(){foreach(var t in tracks)t.Restore();tracks.Clear();sounds.Clear();clipSounds.Clear();ballTrack=null;head=count=clipHead=clipCount=0;goal=0;roster=0;HasFootage=false;nextRecord=0;}
  int RosterKey(){int key=17;foreach(var a in Athlete.Active)if(a&&!a.inTransit)key=unchecked(key*31+a.GetInstanceID());return key;}
  void Prepare(){
   ResetHistory();roster=RosterKey();
   foreach(var actor in Athlete.Active)if(actor&&!actor.inTransit&&actor.visual)tracks.Add(new FootballReplayTrack(actor.visual,true));
   ballTrack=new FootballReplayTrack(ball.transform);tracks.Add(ballTrack);
   if(ball.Pitch){foreach(var net in ball.Pitch.GetComponentsInChildren<FootballNetSurface>())tracks.Add(new FootballReplayTrack(net.transform));var props=ball.Pitch.Find("Football ability presentation");if(props)tracks.Add(new FootballReplayTrack(props));}
  }
  public void RecordSound(FootballSound cue,Vector3 point,float gain){
   if(!match||!match.Context||presenting||cue>=FootballSound.Kickoff)return;
   double now=match.Now;if(sounds.Count>=600)sounds.RemoveAt(0);sounds.Add(new FootballAudioEvent{time=now,cue=cue,point=point,gain=gain});
  }
  void Record(double now){
   if(now<nextRecord)return;nextRecord=now+1.0/30;times[head]=now;foreach(var t in tracks)t.Record(head);head=(head+1)%Capacity;count=Mathf.Min(count+1,Capacity);
   sounds.RemoveAll(e=>e.time<now-8);
  }
  void Find(double at,out int a,out int b,out float fraction,bool replay=false){
   int length=replay?clipCount:count;var clock=replay?clipTimes:times;a=b=((replay?clipHead:head)-length+Capacity)%Capacity;fraction=0;
   for(int i=1;i<length;i++){b=(a+1)%Capacity;if(clock[b]>=at){fraction=Mathf.Clamp01((float)((at-clock[a])/(clock[b]-clock[a])));return;}a=b;}b=a;
  }
  void Begin(FootballMatchSnapshot s){
   FootageStart=s.goalAt-FootballMatch.ReplayBefore;FootageEnd=s.goalAt+FootballMatch.ReplayAfter;
   HasFootage=count>1&&times[(head-count+Capacity)%Capacity]<=FootageStart&&times[(head-1+Capacity)%Capacity]>=FootageEnd;
   for(int i=1;i<count&&HasFootage;i++){int previous=(head-count+i-1+Capacity)%Capacity,current=(previous+1)%Capacity;if(times[current]>=FootageStart&&times[previous]<=FootageEnd&&times[current]-times[previous]>.25)HasFootage=false;}
   // A late join or interrupted application cannot manufacture missing history.
   if(!HasFootage)return;
   camera=Camera.main;if(!camera){HasFootage=false;return;}
   fieldOfView=camera.fieldOfView;orthographic=camera.orthographic;near=camera.nearClipPlane;
   stage=new GameObject("Goal replay · visual copies only").transform;foreach(var t in tracks)t.Create(stage);
   var scoring=s.kickoffTeam==FootballTeam.A?FootballTeam.B:FootballTeam.A;var attack=match.AttackDirection(scoring);
   int index=Enumerable.Range(0,ball.GoalCount).First(i=>ball.GoalSign(i)==(scoring==FootballTeam.A?1:-1));
   var mouth=ball.GoalBounds(index).center;mouth.z=ball.GoalFront(index);goalMouth=ball.Pitch.TransformPoint(mouth);
   Find(s.goalAt-.3,out int a,out _,out _);var shot=ballTrack.Position(a);var right=Vector3.Cross(Vector3.up,attack);float side=Vector3.Dot(shot-goalMouth,right)>=0?1:-1;
   cameraSide=(right*side*.72f-attack*.7f+Vector3.up*.47f).normalized;
   Find(FootageStart,out int first,out _,out _);FrameCamera(ballTrack.Position(first),out cameraFocus,out cameraDistance);
   System.Array.Copy(times,clipTimes,Capacity);clipHead=head;clipCount=count;clipSounds.Clear();clipSounds.AddRange(sounds);foreach(var t in tracks)t.Freeze();head=count=0;
   lastSound=FootageStart-.001;presenting=true;ReplayCount++;FootballAudio.Instance?.BeginReplay();
  }
  void FrameCamera(Vector3 position,out Vector3 focus,out float distance){
   float separation=Vector3.Distance(position,goalMouth);
   // Follow distant build-up closely; reveal the mouth as the shot approaches.
   // Including both ends of the whole pitch would make the ball unreadably small.
   float goalWeight=Mathf.Lerp(.035f,.4f,Mathf.InverseLerp(50,15,separation));
   focus=Vector3.Lerp(position,goalMouth,goalWeight)+Vector3.up*.35f;
   distance=Mathf.Clamp(separation*.65f+9,13,26)*Mathf.Max(1,1.5f/Mathf.Max(.6f,camera.aspect));
  }
  void Show(double at){
   PlaybackTime=at;Find(at,out int a,out int b,out float fraction,true);foreach(var t in tracks)t.Present(a,b,fraction);
   var position=Vector3.Lerp(ballTrack.Position(a,true),ballTrack.Position(b,true),fraction);FrameCamera(position,out var focus,out float distance);
   cameraFocus=Vector3.Lerp(cameraFocus,focus,1-Mathf.Exp(-Time.unscaledDeltaTime*6));cameraDistance=Mathf.Lerp(cameraDistance,distance,1-Mathf.Exp(-Time.unscaledDeltaTime*4));
   var desired=ball.Pitch.InverseTransformPoint(cameraFocus+cameraSide*cameraDistance);var bounds=ball.PitchBounds;desired.x=Mathf.Clamp(desired.x,bounds.min.x+1,bounds.max.x-1);desired.z=Mathf.Clamp(desired.z,bounds.min.z+1,bounds.max.z-1);
   camera.orthographic=false;camera.fieldOfView=54;camera.nearClipPlane=.15f;camera.transform.position=ball.Pitch.TransformPoint(desired);camera.transform.LookAt(cameraFocus);
   foreach(var e in clipSounds)if(e.time>lastSound&&e.time<=at)FootballAudio.Instance?.Play(e.cue,e.point,e.gain,"replay");lastSound=at;
  }
  void Restore(){
   if(presenting){foreach(var t in tracks)t.Restore();if(stage)Destroy(stage.gameObject);if(camera){camera.fieldOfView=fieldOfView;camera.orthographic=orthographic;camera.nearClipPlane=near;}FootballAudio.Instance?.EndReplay();}
   stage=null;presenting=false;if(FootballHUD.Instance)FootballHUD.Instance.SetReplayHidden(false);
  }
  void LateUpdate(){
   if(!match||!match.Context||!match.Active&&match.Snapshot.phase!=FootballMatchPhase.Finished){Restore();if(count>0)ResetHistory();hud.Present(null,0,false);return;}
   double now=match.Now;var s=match.Snapshot;double age=s.goalId==0?-1:now-s.goalAt;
   if(s.goalId==0)goal=0;
   if(!presenting&&(tracks.Count==0||roster!=RosterKey()))Prepare();
   Record(now);
   if(s.goalId!=0&&goal!=s.goalId&&age>=FootballMatch.ReplayEnterHandoff&&age<FootballMatch.ReplayEndOffset){goal=s.goalId;Begin(s);}
   if(presenting&&(!match.GoalSequence||age>=FootballMatch.ReplayReturnHandoff))Restore();
   if(presenting)Show(FootageStart+System.Math.Clamp(age-FootballMatch.ReplayStartOffset,0,FootballMatch.ReplaySeconds));
   bool hide=match.GoalSequence&&age>=FootballMatch.ReplayCaptureSeconds;
   if(FootballHUD.Instance)FootballHUD.Instance.SetReplayHidden(hide);
   hud.Present(match,age,HasFootage);
  }
 }
}
