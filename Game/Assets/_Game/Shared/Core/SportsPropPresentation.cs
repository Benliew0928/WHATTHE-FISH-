using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace WhatTheFish {
 // One pooled set per venue. Reads host snapshots; never changes a gameplay
 // transform, timer, inventory slot, animation state or physics body.
 [DefaultExecutionOrder(220)]
 public sealed class SportsPropPresentation:MonoBehaviour {
  Mesh quad;Material material;MaterialPropertyBlock block;MeshRenderer[] rings=new MeshRenderer[4];
  GameObject root;BasketballBall basketball;FootballBall football;
  Canvas announcement;Text caption,ownerName;RectTransform ribbon;CanvasGroup ribbonFade;BasketballPlate ribbonPlate;Image propIcon,accent;
  public bool WarningVisible {get;private set;}
  public bool ActivationVisible {get;private set;}
  public static Color Tint(SportsProp kind)=>kind==SportsProp.Turbo?new Color(.12f,1,.62f):kind==SportsProp.Wall?new Color(1,.48f,.12f):kind==SportsProp.LockOn?new Color(1,.72f,.08f):new Color(.08f,.68f,1);
  public static SportsProp Held(Athlete actor){
   var app=AppRoot.Instance;if(!actor||!actor.isActiveAndEnabled||actor.IsSpectator||actor.inTransit||!app||!app.Exploring)return SportsProp.None;
   if(app.SelectedSport==SportId.Football&&FootballBall.Instance&&FootballBall.Instance.PropsVisible)return FootballBall.Instance.PropSlot(actor).kind;
   if(app.SelectedSport==SportId.Basketball&&BasketballBall.Active&&BasketballBall.Active.Playing&&!actor.BasketballFreeRoam)return BasketballBall.Active.PropSlot(actor).kind;
   return SportsProp.None;
  }
  public static bool Read(out SportsPropActivation state,out Athlete actor,out double now){
   state=default;actor=null;now=0;var app=AppRoot.Instance;
   if(!app||!app.Exploring||TeleportSequence.OwnsCamera||SkySailWorld.Instance&&SkySailWorld.Instance.Travelling)return false;
   if(app.SelectedSport==SportId.Football){var b=FootballBall.Instance;if(!b||!b.PropsLive)return false;state=b.Props.activation;actor=b.PropActor(state.actor);now=FootballBall.PropClock;}
   else if(app.SelectedSport==SportId.Basketball){var b=BasketballBall.Active;if(!b||!b.Playing||BasketballMatch.BlocksActions)return false;state=b.Props.activation;actor=b.PropActor(state.actor);now=BasketballMotion.Clock;}
   else return false;
   return state.sequence!=0&&actor&&actor.isActiveAndEnabled&&!actor.IsSpectator&&!actor.inTransit;
  }
  public static bool LockOnCounterplay {
   get {var app=AppRoot.Instance;var b=BasketballBall.Active;return app&&app.SelectedSport==SportId.Basketball&&b&&b.Playing&&(b.Props.aiming||b.Props.guided&&b.Score.result==BasketballResult.Flying);}
  }
  public static bool OnCourt(Athlete actor){
   if(!actor||!actor.isActiveAndEnabled||actor.IsSpectator||actor.inTransit)return false;
   var app=AppRoot.Instance;if(!app)return false;
   if(app.SelectedSport==SportId.Football){var b=FootballBall.Instance;if(!b||!b.Pitch)return false;var p=b.Pitch.InverseTransformPoint(actor.transform.position);var bounds=b.PitchBounds;return b.PropsLive&&p.x>=bounds.min.x-.5f&&p.x<=bounds.max.x+.5f&&p.z>=bounds.min.z-.5f&&p.z<=bounds.max.z+.5f;}
   if(app.SelectedSport==SportId.Basketball){var b=BasketballBall.Active;if(!b||!b.Playing||actor.BasketballFreeRoam)return false;var p=b.transform.parent.InverseTransformPoint(actor.transform.position);var limits=BasketballBall.PlayerCourtLimits;return Mathf.Abs(p.x)<=limits.x+.5f&&Mathf.Abs(p.z)<=limits.y+.5f;}
   return false;
  }
  // Called inside each sport's pose layer, so its normal restore pass removes
  // these offsets next frame. Ball-contact and defensive poses take precedence.
  public static void Gesture(Athlete actor,Transform upper,Transform lower){
   if(!upper||!lower||actor.Airborne||actor.LoadingJump||actor.Action!=FootballAction.None||actor.Charging||!Read(out var state,out var owner,out var now)||owner!=actor||state.kind==SportsProp.LockOn)return;
   float age=(float)(now-state.started);if(age<0||age>1.1f)return;
   // Wind up, drive, then settle: a Turbo arm pump, Portal reach, or Wall signal.
   float windup=Mathf.SmoothStep(0,1,age/.12f)*(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.12f,.3f,age)));
   float drive=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.12f,.38f,age))*(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.62f,1.1f,age)));
   float lift=state.kind==SportsProp.Turbo?-60:state.kind==SportsProp.Portal?-40:-75;
   float bend=state.kind==SportsProp.Turbo?-70:state.kind==SportsProp.Portal?-25:-15;
   upper.rotation=Quaternion.AngleAxis(lift*drive+12*windup,actor.transform.right)*upper.rotation;
   lower.rotation=Quaternion.AngleAxis(bend*drive-25*windup,actor.transform.right)*lower.rotation;
  }
  void Awake(){
   basketball=GetComponent<BasketballBall>();football=GetComponent<FootballBall>();material=Resources.Load<Material>("BasketballProps/PropEffect");block=new MaterialPropertyBlock();
   root=new GameObject("Prop activation energy");root.transform.SetParent(transform.parent,false);
   quad=new Mesh{name="Prop activation quad"};quad.vertices=new[]{new Vector3(-.5f,-.5f,0),new Vector3(.5f,-.5f,0),new Vector3(.5f,.5f,0),new Vector3(-.5f,.5f,0)};quad.uv=new[]{Vector2.zero,Vector2.right,Vector2.one,Vector2.up};quad.triangles=new[]{0,2,1,0,3,2};quad.RecalculateBounds();quad.RecalculateNormals();
   for(int i=0;i<rings.Length;i++){var go=new GameObject("Activation ring "+i);go.transform.SetParent(root.transform,false);go.AddComponent<MeshFilter>().sharedMesh=quad;var r=go.AddComponent<MeshRenderer>();r.sharedMaterial=material;r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;r.enabled=false;rings[i]=r;}
   announcement=new GameObject("Prop activation announcement",typeof(RectTransform)).AddComponent<Canvas>();announcement.renderMode=RenderMode.ScreenSpaceOverlay;announcement.sortingOrder=3;
   ribbonPlate=BasketballUIStyle.Plate(announcement.transform,"Prop announcement",0,0,300,52,new Color(.025f,.09f,.14f,.94f),10,1);
   ribbon=ribbonPlate.rectTransform;ribbon.pivot=new Vector2(.5f,0);ribbonFade=ribbon.gameObject.AddComponent<CanvasGroup>();ribbonFade.blocksRaycasts=ribbonFade.interactable=false;
   accent=CoveUI.Rect(ribbon,"Team stripe",8,9,4,34).gameObject.AddComponent<Image>();accent.raycastTarget=false;
   propIcon=CoveUI.Rect(ribbon,"Prop icon",19,6,40,40).gameObject.AddComponent<Image>();propIcon.preserveAspect=true;propIcon.raycastTarget=false;
   caption=CoveUI.Text(ribbon,"",69,3,220,28,21,true,Color.white);caption.font=SportsUIStyle.DisplayFont;
   ownerName=CoveUI.Text(ribbon,"",70,30,218,18,12,false,new Color(.8f,.89f,.94f));ownerName.font=SportsUIStyle.TextFont;ownerName.horizontalOverflow=HorizontalWrapMode.Overflow;
   CharacterIdentityBadges.ReserveHUD(ribbon);announcement.enabled=false;
  }
  void LateUpdate(){
   Clear();if(!material||!AppRoot.Instance||!AppRoot.Instance.Exploring||TeleportSequence.OwnsCamera)return;
   if(basketball&&AppRoot.Instance.SelectedSport!=SportId.Basketball||football&&AppRoot.Instance.SelectedSport!=SportId.Football)return;
   // A Lock-on warning belongs to its actual aiming window, including when a
   // different player activates Turbo during that window.
   if(basketball&&basketball.Playing&&basketball.Props.aiming){var aimer=basketball.PropActor(basketball.Props.aimer);if(aimer){WarningVisible=true;float aimAge=Mathf.Max(0,BasketballPropRules.AimSeconds-(float)(basketball.Props.aimUntil-BasketballMotion.Clock));Show(aimer,SportsProp.LockOn,aimAge,Mathf.SmoothStep(0,1,aimAge/.12f));return;}}
   if(!Read(out var state,out var actor,out var now))return;
   float age=(float)(now-state.started);if(state.kind==SportsProp.LockOn||age<0||age>=SportsPropActivation.Duration)return;
   ActivationVisible=true;float fade=Mathf.SmoothStep(0,1,age/.15f)*(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(2.1f,SportsPropActivation.Duration,age)));
   Show(actor,state.kind,age,fade);
  }
  void Show(Athlete actor,SportsProp kind,float age,float fade){
   var colour=Tint(kind);colour.a=fade;var p=actor.transform.position;var camera=Camera.main;
   ShowAnnouncement(actor,kind,age,fade);
   for(int i=0;i<rings.Length;i++){
    var r=rings[i];r.enabled=true;float cycle=Mathf.Repeat(age*(kind==SportsProp.Turbo?1.6f:.85f)+i*.24f,1);
    if(i==0){r.transform.SetPositionAndRotation(p+Vector3.up*.04f,Quaternion.Euler(90,0,0));r.transform.localScale=Vector3.one*(kind==SportsProp.LockOn?2.7f:2.55f+.25f*Mathf.Sin(age*6));}
    else if(i==3){var center=p+Vector3.up*1.15f;var direction=camera?center-camera.transform.position:Vector3.forward;r.transform.SetPositionAndRotation(center,direction.sqrMagnitude>.001f?Quaternion.LookRotation(direction):Quaternion.identity);r.transform.localScale=new Vector3(2.4f,3.2f,1);}
    else {r.transform.SetPositionAndRotation(p+Vector3.up*(.25f+cycle*1.65f),Quaternion.Euler(90+(kind==SportsProp.Portal?25*Mathf.Sin(age*3+i):0),age*65+i*90,0));r.transform.localScale=Vector3.one*(1.65f+.45f*Mathf.Sin(cycle*Mathf.PI));}
    var ringColour=colour;if(i==1||i==2)ringColour.a*=Mathf.Sin(cycle*Mathf.PI);
    block.Clear();block.SetColor("_Tint",ringColour);block.SetFloat("_Mode",i==3?8:9);block.SetFloat("_Progress",kind==SportsProp.LockOn?1:age);r.SetPropertyBlock(block);
   }
  }
  void ShowAnnouncement(Athlete actor,SportsProp kind,float age,float fade){
   if(SocialPanel.Visible||AccountPanel.Visible||IslandTutorialUI.ModalOpen)return;
   float scale=Mathf.Clamp(Screen.height/720f,.8f,2);var area=Screen.safeArea;
   float enter=Mathf.SmoothStep(0,1,age/.3f),leave=kind==SportsProp.LockOn?0:Mathf.SmoothStep(0,1,Mathf.InverseLerp(2.1f,2.6f,age));
   announcement.enabled=true;ribbonFade.alpha=fade;ribbon.localScale=Vector3.one*scale;
   ribbon.sizeDelta=new Vector2(Mathf.Min(300,(area.width-24*scale)/scale),52);
   // Top-centred sports ribbon. Slide and fade have zero velocity at each end.
   ribbon.anchoredPosition=new Vector2(area.center.x+24*scale*(1-enter)-12*scale*leave,-(Screen.height-area.yMax+218*scale-8*scale*(1-enter)));
   var tint=Tint(kind);accent.color=tint;ribbonPlate.edge=Color.Lerp(new Color(.1f,.2f,.26f),tint,.45f);ribbonPlate.Refresh();
   propIcon.sprite=ControlArt.Prop(kind);propIcon.rectTransform.localScale=Vector3.one*(.86f+.14f*enter);
   caption.text=(kind==SportsProp.LockOn?"Lock-on Shot":FootballPropRules.Name(kind)).ToUpperInvariant();
   string name=actor==AppRoot.Instance.LocalAthlete?"YOU":CharacterIdentityBadges.PlayerName(actor);
   ownerName.text=name;ownerName.rectTransform.sizeDelta=new Vector2(ribbon.rect.width-82,18);ownerName.horizontalOverflow=HorizontalWrapMode.Wrap;
  }
  void Clear(){WarningVisible=ActivationVisible=false;if(announcement)announcement.enabled=false;foreach(var r in rings)if(r)r.enabled=false;}
  void OnDisable()=>Clear();
  void OnDestroy(){if(root)Destroy(root);if(quad)Destroy(quad);if(announcement)Destroy(announcement.gameObject);}
 }
}
