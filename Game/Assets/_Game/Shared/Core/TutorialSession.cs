using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WhatTheFish {
 // A private practice controller. Every adapter is scoped to this session;
 // normal offline games and rooms continue to use their existing rules.
 [DefaultExecutionOrder(1100)]
 public sealed partial class TutorialSession:MonoBehaviour {
  public static TutorialSession Active {get;private set;}
  public SportId Sport {get;private set;}
  public TutorialStep[] Steps {get;private set;}
  public int Index {get;private set;}
  public TutorialStep Step=>Index>=0&&Index<Steps.Length?Steps[Index]:null;
  public bool Ready {get;private set;}
  public bool Finished {get;private set;}
  public bool Paused {get;private set;}
  public TutorialView UI {get;private set;}
  public ControlLayoutEditor Layout {get;private set;}
  public Vector3? TargetPoint {get;private set;}
  public string Status {get;private set;}="Preparing your practice…";
  public bool Succeeded=>successAt>0;
  public const float SuccessDuration=2.2f;
  public float SuccessAge=>Succeeded?Time.unscaledTime-successAt:0;
  public IReadOnlyCollection<string> Revealed=>revealed;
  readonly HashSet<string> revealed=new();
  AppRoot app;string profile;TutorialProgress.Record progress;int oldMode;float oldYaw,oldPitch;
  float entered,successAt,held,otherHeld,retryAt,observedMove;bool sawAction,sawSecond;
  Vector3 startPosition;float startYaw;PlayerCommand lastCommand;uint startCount,startSecondary;SportsPropSlot startProp;
  bool stopping;float kickCount;Athlete friend,opponent;Coroutine preparation;
  public static bool Is(SportId sport)=>Active&&Active.Sport==sport;
  public static bool Allows(string control)=>!Active||Active.Ready&&!Active.Paused&&!Active.Finished&&Active.revealed.Contains(control);
  public static bool Owns(Athlete a)=>Active&&a&&(a==Active.app.LocalAthlete||a==Active.friend||a==Active.opponent);
  public static FootballTeam Team(Athlete a)=>!Owns(a)?FootballTeam.None:a==Active.opponent?FootballTeam.B:FootballTeam.A;
  public static void Launch(SportId sport,bool restart=false,int chapter=-1){
   var root=AppRoot.Instance;if(!root||root.rooms.Connected||root.rooms.busy||Active)return;
   var value=root.gameObject.AddComponent<TutorialSession>();Active=value;value.app=root;value.Sport=sport;value.Steps=TutorialCourse.For(sport);value.profile=PlayerIdentity.ProfileStorageKey;
   value.progress=TutorialProgress.Load(value.profile,sport);value.oldMode=root.view.mode;value.oldYaw=root.view.yaw;value.oldPitch=root.view.pitch;
   int resume=Array.FindIndex(value.Steps,s=>s.id==value.progress.current);
   value.Index=chapter>=0?Mathf.Clamp(chapter,0,value.Steps.Length-1):restart?0:Mathf.Max(0,resume);
   value.UI=TutorialView.Create(value,root.SafeFrame);value.preparation=value.StartCoroutine(value.Enter());
  }
  IEnumerator Enter(){
   app.SelectSport(Sport);float deadline=Time.realtimeSinceStartup+90;
   while(app.PendingSelection.HasValue&&Time.realtimeSinceStartup<deadline)yield return null;
   if(app.SelectionError!=null||app.PendingSelection.HasValue){Status="The island could not load. Quit and try again.";yield break;}
   app.EnterOffline();app.view.mode=1;app.view.pitch=18;
   yield return null;
   while((!app.view.active||!app.LocalAthlete.Grounded)&&Time.realtimeSinceStartup<deadline)yield return null;
   Layout=app.HudFrame.GetComponentsInChildren<ControlLayoutEditor>(true).FirstOrDefault(l=>l.Sport==Sport&&!l.Preview);
   if(!Layout){Status="The controls could not load. Quit and try again.";yield break;}
   if(Sport==SportId.Basketball||Sport==SportId.Football){friend=Partner("Practice teammate",FootballTeam.A);opponent=Partner("Practice opponent",FootballTeam.B);opponent.gameObject.SetActive(false);friend.gameObject.SetActive(false);}
   if(Sport==SportId.Football)FootballBall.Instance.Kicked+=OnKick;
   Ready=true;UI.Bind(app.HudFrame,Layout);Begin(Index);
  }
  void OnKick(Athlete a,float power){if(a==app.LocalAthlete)kickCount++;}
  Athlete Partner(string label,FootballTeam team){var go=Instantiate(app.athletePrefab);go.name=label;var actor=go.GetComponent<Athlete>();actor.Setup();actor.BasketballPracticeTeam=team;var helper=go.AddComponent<TutorialPartner>();helper.actor=actor;helper.session=this;actor.Appearance(LocalProfile.Character);return actor;}
  public void Begin(int index){
   if(!Ready||stopping)return;Index=Mathf.Clamp(index,0,Steps.Length-1);Finished=false;Paused=false;UI.CloseChapters();
   revealed.Clear();for(int i=0;i<=Index;i++)foreach(var id in Steps[i].reveal.Split(' ',StringSplitOptions.RemoveEmptyEntries))revealed.Add(id);
   app.view.ClearMatchInput();app.view.stick?.ResetInput();successAt=held=otherHeld=observedMove=0;sawAction=sawSecond=false;lastCommand=default;TargetPoint=null;
   PrepareStep();entered=Time.unscaledTime;retryAt=entered+8;startPosition=app.LocalAthlete.transform.position;startYaw=app.view.yaw;
   CaptureCounters();Status="";progress.current=Step.id;TutorialProgress.Save(profile,Sport,progress);UI.Refresh();
  }
  void CaptureCounters(){
   if(Sport==SportId.Basketball){var b=BasketballBall.Active;startCount=Step.id=="pass"||Step.id=="directPass"?b.PassCount:Step.id=="block"?b.Defense.blocks:Step.id=="steal"?b.StealCount:b.ShotCount;startProp=b.PropSlot(app.LocalAthlete);}
   if(Sport==SportId.Football){startCount=(uint)kickCount;startSecondary=FootballBall.Instance.PassPossession;startProp=FootballBall.Instance.PropSlot(app.LocalAthlete);}
   if(Sport==SportId.Golf){var b=GolfMatchManager.Instance.Ball(app.LocalAthlete);startCount=b?b.StrikeSequence:0;}
   if(Sport==SportId.Fishing){var p=FishingGame.Instance.Player(app.LocalAthlete);if(p.HasValue){startCount=(uint)p.Value.catches;startSecondary=p.Value.itemSequence;}}
  }
  public static PlayerCommand Filter(PlayerCommand command){
   var t=Active;if(!t)return command;if(!t.Ready||t.Paused||t.Finished)return default;
   if(!Allows("move"))command.move=Vector2.zero;if(!Allows("jump"))command.jump=command.jumpBlock=false;
   if(!Allows("shoot")){command.kick=command.charging=command.shoot=command.shotBegin=command.guard=command.guardBlock=command.block=command.pressure=false;}
   if(!Allows("pass")){command.pass=command.passBegin=command.footballPass=command.steal=command.targetedPass=false;}
   if(!Allows("dash"))command.dash=false;if(!Allows("tackle"))command.tackle=false;
   if(!Allows("power"))command.golfSwing=command.golfCharging=false;
   if(!Allows("buggy"))command.cartAction=GolfCartAction.None;
   if(!Allows("cast")&&command.fishingAction!=FishingAction.ChooseSkill)command.fishingAction=FishingAction.None;
   if(!Allows("skills")&&command.fishingAction==FishingAction.ChooseSkill)command.fishingAction=FishingAction.None;
   if(!Allows("ability")&&command.fishingAction==FishingAction.UseAbility)command.fishingAction=FishingAction.None;
   if(!Allows("left")&&command.fishingAction==FishingAction.ReelLeft)command.fishingAction=FishingAction.None;
   t.lastCommand=command;return command;
  }
  void Update(){
   if(stopping)return;if(profile!=PlayerIdentity.ProfileStorageKey||app.rooms.Connected){Quit();return;}
   if(!Ready||Finished||Paused)return;
   if(!app.Exploring||app.SelectedSport!=Sport){Quit();return;}
   if(successAt>0){if(SuccessAge>SuccessDuration)Advance(false);return;}
   if(!app.view.active||!Application.isFocused&&!Application.isBatchMode)return;
   if(Time.unscaledTime-entered<.35f)return;
   bool done=CheckStep(Time.unscaledDeltaTime);
   if(done){successAt=Time.unscaledTime;Status="";if(!progress.complete.Contains(Step.id))progress.complete.Add(Step.id);progress.skipped.Remove(Step.id);TutorialProgress.Save(profile,Sport,progress);UI.Celebrate();}
   else if(Time.unscaledTime-entered>18&&Status==""){Status="Take your time. Retry resets this exercise.";UI.Refresh();}
   MaintainScenario();
  }
  public void Retry(){if(Ready&&!Finished)Begin(Index);}
  public void Skip(){if(Ready&&!Finished)Advance(true);}
  void Advance(bool skip){
   if(skip&&!progress.complete.Contains(Step.id)&&!progress.skipped.Contains(Step.id))progress.skipped.Add(Step.id);
   if(Index+1<Steps.Length){Begin(Index+1);return;}
   Finished=true;Paused=false;app.view.ClearMatchInput();app.view.stick?.ResetInput();progress.current="";TutorialProgress.Save(profile,Sport,progress);Status="";UI.Refresh();
  }
  public void Pause(bool value){Paused=value;app.view.ClearMatchInput();app.view.stick?.ResetInput();if(!value&&!Finished)Begin(Index);}
  public bool Completed(string id)=>progress.complete.Contains(id);
  public int CompletedCount=>Steps.Count(s=>progress.complete.Contains(s.id));
  public void Quit(){
   if(stopping)return;stopping=true;if(preparation!=null)StopCoroutine(preparation);
   app.view.ClearMatchInput();app.view.stick?.ResetInput();if(UI)UI.Restore();
   if(FootballBall.Instance)FootballBall.Instance.Kicked-=OnKick;
   if(friend){friend.gameObject.SetActive(false);Destroy(friend.gameObject);}if(opponent){opponent.gameObject.SetActive(false);Destroy(opponent.gameObject);}
   GolfCartWorld.ParkDrivers(true);if(Sport==SportId.Golf&&GolfCartWorld.Owned(app.LocalAthlete))GolfCartWorld.Execute(app.LocalAthlete,GolfCartAction.Recall,GolfCartWorld.Key(app.LocalAthlete));
   app.view.mode=oldMode;app.view.yaw=oldYaw;app.view.pitch=oldPitch;Active=null;
   app.ExitTutorial();if(UI)Destroy(UI.gameObject);Destroy(this);
  }
  void OnDestroy(){if(Active==this)Active=null;}
  public static Vector3 Ground(Vector3 point){if(Physics.Raycast(point+Vector3.up*5,Vector3.down,out var hit,15,1<<8,QueryTriggerInteraction.Ignore))point.y=hit.point.y+.04f;return point;}
  internal static void Place(Athlete a,Vector3 point,float yaw){a.capsule.enabled=false;a.transform.SetPositionAndRotation(Ground(point),Quaternion.Euler(0,yaw,0));a.ResetLocomotion();a.capsule.enabled=true;Physics.SyncTransforms();}
 }
 public sealed class TutorialPartner:MonoBehaviour {
  internal Athlete actor;internal TutorialSession session;
  void Update(){if(session&&TutorialSession.Active==session&&actor&&actor.isActiveAndEnabled&&!session.Paused)actor.Simulate(new PlayerCommand{heading=actor.transform.eulerAngles.y},Time.deltaTime);}
 }
}
