using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace WhatTheFish {
 public sealed partial class FootballMVP {
  public const float TeamEnds=15f,SoloStarts=17.4f,CardStarts=25f,CardReady=27.2f;
  bool presenting,castRevealed,alternateMvp;
  Transform stage;readonly List<FootballCelebrationActor> cast=new();FootballCelebrationActor star;
  FootballTake[] performances;CanvasGroup cardGroup,captionGroup,skipGroup;Image curtain,topBar,bottomBar;Text ceremonyTitle,ceremonyDetail;
  float skipAt=-1,nextHideAt;Vector3 skipPosition;Quaternion skipRotation;float skipFov;
  public bool ResultsReady=>Visible&&cardGroup&&cardGroup.interactable;
  public float CelebrationAge=>FootballStadiumCelebration.SharedAge(FootballMatch.Instance);
  public int CelebrationCastCount=>cast.Count;
  public ulong CelebratedMvp=>star!=null?star.Id:ulong.MaxValue;
  public float PoseReachError=>cast.Count==0?0:cast.Max(c=>c.ReachError);
  public float PoseBoneError=>cast.Count==0?0:cast.Max(c=>c.BoneError);
  public Transform CelebrationStage=>stage;
  static float Ease(float t){t=Mathf.Clamp01(t);return t*t*t*(t*(t*6-15)+10);}
  void BuildCelebration(){
   cardGroup=content.gameObject.AddComponent<CanvasGroup>();
   Image ScreenImage(string name,Vector2 min,Vector2 max){var r=CoveUI.Rect(panel,name,0,0,0,0);r.anchorMin=min;r.anchorMax=max;r.offsetMin=r.offsetMax=Vector2.zero;var image=r.gameObject.AddComponent<Image>();image.color=new Color(.025f,.075f,.08f,1);image.raycastTarget=false;return image;}
   topBar=ScreenImage("Cinema top",new(0,.945f),Vector2.one);bottomBar=ScreenImage("Cinema bottom",Vector2.zero,new(1,.055f));
   var captions=CoveUI.Rect(panel,"Ceremony titles",0,0,1000,110);captions.anchorMin=captions.anchorMax=new(.5f,.93f);captions.pivot=new(.5f,1);captions.anchoredPosition=Vector2.zero;
   captionGroup=captions.gameObject.AddComponent<CanvasGroup>();captionGroup.blocksRaycasts=false;
   BasketballUIStyle.Plate(captions,"Ceremony title backdrop",85,-9,830,116,new Color(.025f,.075f,.08f,.84f),22,0);
   ceremonyTitle=Label(captions,"",0,0,1000,58,43,Cream,TextAnchor.MiddleCenter);ceremonyDetail=Label(captions,"",0,62,1000,35,24,Cream,TextAnchor.MiddleCenter);ceremonyDetail.supportRichText=false;
   var skip=CoveUI.Rect(panel,"Skip ceremony",0,0,230,50);skip.anchorMin=skip.anchorMax=new(1,0);skip.pivot=new(1,0);skip.anchoredPosition=new(-35,28);skipGroup=skip.gameObject.AddComponent<CanvasGroup>();
   FootballUIStyle.Button(skip,"Skip celebration","Show results",0,0,230,50,SkipCelebration,CoveUI.Cream,21);
   curtain=ScreenImage("Full time dissolve",Vector2.zero,Vector2.one);
  }
  void BeginCelebration(FootballMatch match,FootballPlayerRecord winner,FootballPlayerRecord[] records){
   presenting=true;castRevealed=false;skipAt=-1;nextHideAt=0;cardGroup.alpha=0;cardGroup.interactable=cardGroup.blocksRaycasts=false;captionGroup.alpha=0;curtain.color=new(.025f,.075f,.08f,0);
   var library=Resources.Load<FootballMotionLibrary>("FootballMotionLibrary");performances=FootballCelebrationPoses.Defaults().Select(t=>library?library.Find(t.name)??t:t).ToArray();
   var ball=FootballBall.Instance;stage=new GameObject("Football honours - render-only cast").transform;
   stage.SetPositionAndRotation(ball&&ball.Pitch?ball.Pitch.TransformPoint(new Vector3(ball.PitchBounds.center.x,ball.PitchBounds.max.y+.012f,ball.PitchBounds.center.z)):Vector3.zero,ball&&ball.Pitch?ball.Pitch.rotation:Quaternion.identity);
   var snapshot=match.Snapshot;var winning=snapshot.result==FootballMatchResult.TeamA?FootballTeam.A:snapshot.result==FootballMatchResult.TeamB?FootballTeam.B:FootballTeam.None;
   var participants=records.Where(r=>winning==FootballTeam.None||r.team==winning).OrderBy(r=>r.id==winner.id?0:1).ThenBy(r=>r.number).ToList();
   alternateMvp=!participants.Any(r=>r.id==winner.id);if(alternateMvp)participants.Add(winner);
   foreach(var record in participants){
    var source=Athlete.Active.FirstOrDefault(a=>a&&FootballBots.Id(a)==record.id&&a.visual);
    // Retained records can outlive a disconnected actor. Rebuild its visual
    // identity using the existing prefab without registering a gameplay actor.
    if(!source)source=AppRoot.Instance?AppRoot.Instance.LocalAthlete:null;if(!source)source=Athlete.Active.FirstOrDefault(a=>a&&a.visual);if(!source)continue;
    var actor=new FootballCelebrationActor(source,record,stage);if(!actor.Ready){Destroy(actor.Root.gameObject);continue;}cast.Add(actor);if(record.id==winner.id)star=actor;
   }
   int slot=0;foreach(var actor in cast){
    bool reserve=alternateMvp&&actor==star;int index=reserve?0:slot++;
    // The wings stand slightly nearer the lens. The dolly passes between
    // them, taking them naturally beyond the frame before the solo hold.
    float spacing=participants.Count>5?1.65f:2.3f;
    float x=index==0?0:((index+1)/2)*(index%2==1?-spacing:spacing);float z=index==0?0:1.2f+Mathf.Max(0,Mathf.Abs(x)-spacing)*.16f;
    actor.Root.localPosition=new(x,0,z);actor.Root.localRotation=Quaternion.Euler(0,-x*3,0);actor.Root.gameObject.SetActive(false);
   }
   ceremonyTitle.text=snapshot.result==FootballMatchResult.Draw?"HONOURS SHARED":CharacterIdentity.TeamName(winning).ToUpperInvariant()+" TAKES THE WIN";
   ceremonyDetail.text="FULL TIME    "+snapshot.scoreA+" : "+snapshot.scoreB;
   // Hide live renderers only after the incoming full-time dissolve covers
   // the camera, avoiding a one-frame disappearance in the gameplay view.
  }
  public void SkipCelebration(){if(!Visible||ResultsReady||skipAt>=0)return;skipAt=CelebrationAge;skipPosition=stadiumCamera?stadiumCamera.transform.position:Vector3.zero;skipRotation=stadiumCamera?stadiumCamera.transform.rotation:Quaternion.identity;skipFov=stadiumCamera?stadiumCamera.fieldOfView:32;}
  void HideLive(){
   if(Time.unscaledTime>=nextHideAt){nextHideAt=Time.unscaledTime+.5f;
   foreach(var actor in Athlete.Active)if(actor&&actor.visual)foreach(var r in actor.visual.GetComponentsInChildren<Renderer>(true))if(!hidden.ContainsKey(r)){hidden[r]=r.forceRenderingOff;r.forceRenderingOff=true;}
   var ball=FootballBall.Instance;if(ball)foreach(var r in ball.GetComponentsInChildren<Renderer>())if(!hidden.ContainsKey(r)){hidden[r]=r.forceRenderingOff;r.forceRenderingOff=true;}
   }
   foreach(var pair in hidden)if(pair.Key)pair.Key.forceRenderingOff=true;
  }
  void PresentCelebration(){
   float age=CelebrationAge;float reveal=Ease((age-.65f)/.85f),solo=Ease((age-TeamEnds)/(SoloStarts-TeamEnds));
   float card=skipAt>=0?Ease((age-skipAt)/1.25f):Ease((age-CardStarts)/(CardReady-CardStarts));
   bool skip=skipAt>=0;
   if(age>=.65f&&!castRevealed){castRevealed=true;HideLive();}
   if(castRevealed)HideLive();
   // If an opposing player earned MVP, use a brief covered handoff. The
   // winning squad still receives its own full celebration beforehand.
   bool coveredSolo=alternateMvp||cast.Count>5;
   float switchCover=coveredSolo?Ease((age-14.95f)/.45f)*(1-Ease((age-15.45f)/.60f)):0;
   float skipCover=skip?Ease((age-skipAt)/.25f)*(1-Ease((age-skipAt-.65f)/.6f)):0;
   float cover=age<.65f?Ease(age/.65f):1-reveal;curtain.color=new(.025f,.075f,.08f,Mathf.Max(cover,skip?skipCover:switchCover));
   for(int i=0;i<cast.Count;i++){
    var actor=cast[i];bool isStar=actor==star;
    bool visible=castRevealed&&(isStar?(!alternateMvp||age>=15.4f||skip):age<SoloStarts&&(!coveredSolo||age<15.4f)&&card<.25f);
    actor.Root.gameObject.SetActive(visible);if(!visible)continue;
    var team=performances[i%3];float elapsed=Mathf.Max(0,age-.65f);var pose=team.Sample((elapsed+i*.67f)/team.duration);
    if(isStar){float blend=skip?card:Ease((age-14f)/2);var mvp=performances[3];pose=FootballPose.Lerp(pose,mvp.Sample(Mathf.Max(0,age-14f)/mvp.duration),blend);actor.Root.localRotation=Quaternion.Slerp(actor.Root.localRotation,Quaternion.identity,blend);}
    actor.Pose(pose);
   }
   float caption=Ease((age-1.2f)/.5f)*(1-Ease((age-13.9f)/.65f));
   if(age>=16f){caption=Ease((age-16f)/.7f)*(1-Ease((age-24.3f)/.7f));ceremonyTitle.text="MOST VALUABLE PLAYER";ceremonyDetail.text=nameText.text;}
   captionGroup.alpha=skip?caption*(1-card):caption;cardGroup.alpha=card;cardGroup.interactable=cardGroup.blocksRaycasts=card>=.999f;
   content.anchoredPosition+=Vector2.right*((1-card)*28);topBar.color=bottomBar.color=new(.025f,.075f,.08f,.94f*(1-card));
   skipGroup.alpha=Ease((age-1.7f)/.5f)*(1-card);skipGroup.interactable=skipGroup.blocksRaycasts=age>1.7f&&!skip&&card<.01f;
   if(!stadiumCamera||!castRevealed)return;
   float aspect=Mathf.Max(.6f,stadiumCamera.aspect);int teamCount=cast.Count-(alternateMvp?1:0);float groupWidth=Mathf.Ceil((teamCount-1)/2f)*(cast.Count>5?1.65f:2.3f)+.8f;float distance=Mathf.Max(5.5f,groupWidth/(Mathf.Tan(22*Mathf.Deg2Rad)*aspect)+1.6f);
   float arc=Mathf.Lerp(-12,8,Ease((age-6f)/(TeamEnds-6f)));var groupPosition=Quaternion.Euler(0,arc,0)*new Vector3(0,1.75f,distance);var groupFocus=new Vector3(0,.90f,-.35f);
   // Establish the complete arena, twin screens and sky shells before
   // descending to the winners. All viewers share this 15 + 10 second show.
   float descend=Ease((age-4f)/5f);
   groupPosition=Vector3.Lerp(new Vector3(8,24,47),groupPosition,descend);
   groupFocus=Vector3.Lerp(new Vector3(0,18,-27),groupFocus,descend);
   var soloPosition=new Vector3(.05f,1.22f,3.85f);var soloFocus=new Vector3(0,.93f,0);
   var position=Vector3.Lerp(groupPosition,soloPosition,solo);var focus=Vector3.Lerp(groupFocus,soloFocus,solo);float fov=Mathf.Lerp(Mathf.Lerp(62,44,descend),32,solo);
   // Frame the 3D player in the actual safe-area column, including tablets
   // and wide phones. No UI portrait or separate animation restart is used.
   var viewport=stadiumCamera.ScreenToViewportPoint(RectTransformUtility.WorldToScreenPoint(null,content.TransformPoint(new Vector3(-450,-10,0))));
   float finalDistance=4.2f*Mathf.Max(1,((RectTransform)transform).rect.height/(900*content.localScale.y));
   float side=(.5f-viewport.x)*2*finalDistance*Mathf.Tan(16*Mathf.Deg2Rad)*aspect,vertical=(.5f-viewport.y)*2*finalDistance*Mathf.Tan(16*Mathf.Deg2Rad);
   var finalPosition=new Vector3(-side,1.22f+vertical,finalDistance);var finalFocus=new Vector3(-side,.94f+vertical,0);
   position=Vector3.Lerp(position,finalPosition,card);focus=Vector3.Lerp(focus,finalFocus,card);fov=Mathf.Lerp(fov,32,card);
   Vector3 world=stage.TransformPoint(position);Quaternion rotation=Quaternion.LookRotation(stage.TransformPoint(focus)-world,Vector3.up);
   if(skip){world=Vector3.Lerp(skipPosition,stage.TransformPoint(finalPosition),card);rotation=Quaternion.Slerp(skipRotation,Quaternion.LookRotation(stage.TransformDirection(finalFocus-finalPosition)),card);fov=Mathf.Lerp(skipFov,32,card);}
   stadiumCamera.orthographic=false;stadiumCamera.fieldOfView=fov;stadiumCamera.nearClipPlane=.10f;stadiumCamera.transform.SetPositionAndRotation(world,rotation);
  }
  void EndCelebration(){presenting=false;if(stage){stage.gameObject.SetActive(false);Destroy(stage.gameObject);}stage=null;cast.Clear();star=null;castRevealed=false;}
 }
}
