using UnityEngine;
using UnityEngine.Rendering;
namespace WhatTheFish {
 // Presentation follows replicated shot conditions, not the weather at landing.
 // Contact rings reuse the host's existing feedback channel. No extra RPCs.
 [DefaultExecutionOrder(210)]
 public sealed class GolfBallEventFX:MonoBehaviour {
  sealed class Ring {public LineRenderer line;public Vector3 point,normal;public Color tint;public float born=-100;}
  readonly Ring[] rings=new Ring[3];int cursor;uint reset,strike;
  GolfBall ball;GolfEventKind lastKind;Transform visual;Vector3 homeScale;Quaternion homeRotation;
  LineRenderer aura;float impactAt=-100;Vector3 impactNormal=Vector3.up;
  public int Contacts {get;private set;}
  public GolfEventKind PresentedKind=>lastKind;
  public bool AuraVisible=>aura&&aura.enabled;
  void Awake(){ball=GetComponent<GolfBall>();visual=transform.Find("Visual");if(visual){homeScale=visual.localScale;homeRotation=visual.localRotation;}}
  LineRenderer Line(string name,int count,bool loop){
   var go=new GameObject(name);var line=go.AddComponent<LineRenderer>();line.sharedMaterial=Resources.Load<Material>("GolfBallTrail");line.positionCount=count;line.loop=loop;line.useWorldSpace=true;line.widthMultiplier=.025f;line.shadowCastingMode=ShadowCastingMode.Off;line.receiveShadows=false;line.enabled=false;return line;
  }
  public void Contact(GolfFeedbackEvent e){
   if(e.cue==GolfFeedbackCue.Swing||e.cue==GolfFeedbackCue.Putt){lastKind=e.cue==GolfFeedbackCue.Putt?GolfEventKind.None:GolfMatchManager.Instance.ShotConditions(GolfShotMode.Swing).kind;return;}
   if(e.normal.y<.35f||!(e.cue==GolfFeedbackCue.Turf||e.cue==GolfFeedbackCue.Sand||e.cue==GolfFeedbackCue.Stone||e.cue==GolfFeedbackCue.Wood))return;
   var kind=ball.Conditions.kind!=GolfEventKind.None?ball.Conditions.kind:lastKind;
   if(kind!=GolfEventKind.StickyGreen&&kind!=GolfEventKind.BounceParty)return;
   var config=GolfPresentationSettings.Current;int index=cursor++%rings.Length;
   if(rings[index]==null)rings[index]=new Ring{line=Line("Golf event contact ring",40,true)};
   var ring=rings[index];ring.point=e.point+e.normal*.012f;ring.normal=e.normal.normalized;ring.born=Time.time;ring.tint=kind==GolfEventKind.StickyGreen?config.stickyColor:config.bounceColor;
   ring.line.enabled=true;impactAt=Time.time;impactNormal=ring.normal;lastKind=kind;Contacts++;
  }
  void LateUpdate(){
   if(!ball)return;
   if(reset!=ball.ResetSequence||strike!=ball.StrikeSequence){Clear();reset=ball.ResetSequence;strike=ball.StrikeSequence;lastKind=ball.Conditions.kind;}
   var match=GolfMatchManager.Instance;bool live=match&&match.Context&&match.State.Running&&ball.Live&&ball.GetComponentInChildren<MeshRenderer>()?.enabled==true;
   if(!live){Clear();return;}
   if(ball.Conditions.kind!=GolfEventKind.None)lastKind=ball.Conditions.kind;
   bool affected=ball.Trail.Active&&(lastKind==GolfEventKind.StickyGreen||lastKind==GolfEventKind.BounceParty);
   var config=GolfPresentationSettings.Current;var color=lastKind==GolfEventKind.StickyGreen?config.stickyColor:config.bounceColor;
   ball.Trail.EventTint(affected?lastKind:GolfEventKind.None);
   if(affected&&!aura)aura=Line("Golf event ball outline",32,true);
   if(aura){aura.enabled=affected&&!ball.Sinking;if(aura.enabled)DrawAura(aura,transform.position,Camera.main,color,Time.time);}
   float squash=Squash(Time.time-impactAt);
   if(visual){visual.localScale=Vector3.Scale(homeScale,new Vector3(1+squash*.5f,1-squash,1+squash*.5f));if(squash>0)visual.rotation=Quaternion.FromToRotation(Vector3.up,impactNormal)*homeRotation;else visual.localRotation=homeRotation;}
   foreach(var ring in rings)if(ring!=null)DrawContact(ring.line,ring.point,ring.normal,ring.tint,Time.time-ring.born);
  }
  // The teaching shot uses these same contact/outline shapes and timing.
  public static float Squash(float age)=>MenuPreferences.ReducedMotion?0:Mathf.Sin(Mathf.Clamp01(age/.28f)*Mathf.PI)*GolfPresentationSettings.Current.bounceSquash;
  public static void DrawContact(LineRenderer line,Vector3 point,Vector3 normal,Color tint,float seconds,float opacity=1){
   var config=GolfPresentationSettings.Current;float age=seconds/config.impactSeconds;line.enabled=age>=0&&age<1;if(!line.enabled)return;
   float radius=MenuPreferences.ReducedMotion?config.impactRadius*.6f:Mathf.Lerp(.06f,config.impactRadius,1-(1-age)*(1-age));
   var right=Vector3.Cross(normal,Vector3.forward).normalized;if(right.sqrMagnitude<.1f)right=Vector3.right;var forward=Vector3.Cross(right,normal);
   line.positionCount=40;for(int i=0;i<40;i++){float a=i*Mathf.PI/20;line.SetPosition(i,point+(right*Mathf.Cos(a)+forward*Mathf.Sin(a))*radius);}
   tint.a=(1-Mathf.SmoothStep(.25f,1,age))*opacity;line.startColor=line.endColor=tint;line.widthMultiplier=.08f*(1-age*.45f);
  }
  public static void DrawAura(LineRenderer line,Vector3 point,Camera camera,Color color,float clock){
   if(!camera)return;float radius=GolfBall.Radius*(MenuPreferences.ReducedMotion?2.2f:2.2f+.18f*Mathf.Sin(clock*4));line.positionCount=32;
   for(int i=0;i<32;i++){float a=i*Mathf.PI/16;line.SetPosition(i,point+(camera.transform.right*Mathf.Cos(a)+camera.transform.up*Mathf.Sin(a))*radius);}
   line.widthMultiplier=.035f;line.startColor=line.endColor=color;
  }

  void Clear(){if(aura)aura.enabled=false;foreach(var r in rings)if(r!=null){r.line.enabled=false;r.born=-100;}impactAt=-100;if(visual){visual.localScale=homeScale;visual.localRotation=homeRotation;}if(ball&&ball.Trail)ball.Trail.EventTint(GolfEventKind.None);}
  void OnDisable()=>Clear();
  void OnDestroy(){if(aura)Destroy(aura.gameObject);foreach(var r in rings)if(r!=null&&r.line)Destroy(r.line.gameObject);}
 }
}
