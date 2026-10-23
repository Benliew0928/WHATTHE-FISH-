using UnityEngine;

using UnityEngine.UI;



namespace WhatTheFish {

 // Each device reads only its local player's target and owned ball.

 [DefaultExecutionOrder(150)]

 public sealed class GolfTargetIndicator:MonoBehaviour {

  RectTransform page,targetArrow,targetCard,ballMarker,ballArrow,ballCard;Text targetLabel,targetDot,targetDistance,ballLabel;bool nearBall;
  public bool BallCaptionVisible=>ballCard&&ballCard.gameObject.activeSelf;
  public bool BallArrowVisible=>ballArrow&&ballArrow.gameObject.activeInHierarchy;
  public const float HideCaptionDistance=1.5f,ShowCaptionDistance=1.8f;GolfHUDLayout layout;GolfLeaderboardUI scorecard;readonly Vector3[] corners=new Vector3[4];

  public int TargetHole {get;private set;}
  CanvasGroup ballVisibility;
  RectTransform targetEdgeArrow;
  GolfHoleGuideGraphic holeGuide;Text holeNumber;CanvasGroup guideOpacity;bool guideOffscreen;
  LineRenderer cupGlow;
  public bool HoleGuideVisible=>targetEdgeArrow&&targetEdgeArrow.gameObject.activeInHierarchy&&guideOpacity.alpha>.01f;
  public RectTransform TargetCard=>targetCard;
  public RectTransform EdgeArrow=>targetEdgeArrow;
  public bool LargeTargetArrowVisible=>targetArrow&&targetArrow.gameObject.activeInHierarchy;
  public bool EdgeArrowVisible=>HoleGuideVisible&&guideOffscreen;

  public ulong BallOwner {get;private set;}
  static readonly Vector2[] CaptionOffsets={new Vector2(103,56),new Vector2(-103,56),new Vector2(103,-78),new Vector2(-103,-78)};

  public void Build(RectTransform parent,Font font){

   page=parent;scorecard=parent.GetComponentInChildren<GolfLeaderboardUI>();layout=Resources.Load<GolfHUDLayout>("GolfUILayout");if(!layout)layout=ScriptableObject.CreateInstance<GolfHUDLayout>();

   targetArrow=Arrow("Your target hole",parent,new Vector2(.5f,1),layout.targetArrowPosition,layout.targetArrowSize,GolfHUDStyle.Gold,"Arrow");
   targetEdgeArrow=GolfHUDStyle.Rect("Numbered current hole guide",parent,new Vector2(.5f,.5f),Vector2.zero,GolfPresentationSettings.Current.holeGuideSize);
   holeGuide=targetEdgeArrow.gameObject.AddComponent<GolfHoleGuideGraphic>();holeGuide.raycastTarget=false;
   guideOpacity=targetEdgeArrow.gameObject.AddComponent<CanvasGroup>();guideOpacity.blocksRaycasts=guideOpacity.interactable=false;guideOpacity.alpha=0;
   holeNumber=GolfHUDStyle.Text("Current hole number",targetEdgeArrow,"1",new Vector2(48,45),32,TextAnchor.MiddleCenter,GolfHUDStyle.Ink);holeNumber.rectTransform.anchoredPosition=new Vector2(0,8);
   targetEdgeArrow.gameObject.SetActive(false);

   targetCard=GolfHUDStyle.Rect("Golf target card",parent,new Vector2(.5f,1),layout.targetPosition,layout.targetSize);GolfReferenceUI.Plate(targetCard,"Hole",false);

   // Reference flag illustration is retained, including the overhanging pole.

   targetLabel=Caption("Your target");targetDot=Caption("Target separator");targetDistance=Caption("Target distance");

   ballMarker=GolfLeaderboardUI.Rect("Your ball indicator",parent,new Vector2(.5f,.5f),Vector2.zero,new Vector2(180,62));
   ballVisibility=ballMarker.gameObject.AddComponent<CanvasGroup>();ballVisibility.interactable=ballVisibility.blocksRaycasts=false;

   ballArrow=Arrow("Ball arrow",ballMarker,new Vector2(.5f,.5f),new Vector2(0,-10),new Vector2(48,42),GolfHUDStyle.Gold,"BallArrow");

   ballCard=GolfHUDStyle.Rect("Golf ball caption card",ballMarker,new Vector2(.5f,.5f),new Vector2(103,56),new Vector2(290,74));var ballArt=GolfReferenceUI.Plate(ballCard,"Ball",false);ballArt.sprite=Resources.Load<Sprite>("GolfUI/Badges/YourBall");ballArt.preserveAspect=true;ballCard.sizeDelta=new Vector2(290,290*ballArt.sprite.rect.height/ballArt.sprite.rect.width);
   ballLabel=GolfHUDStyle.Text("Your ball",ballCard,"",new Vector2(192,38),layout.ballFontSize,TextAnchor.MiddleCenter,GolfHUDStyle.Ink);ballLabel.font=CoveUI.DisplayFont??font;ballLabel.rectTransform.anchoredPosition=new Vector2(25,-ballCard.sizeDelta.y*.5f-13);ballLabel.horizontalOverflow=HorizontalWrapMode.Wrap;ballLabel.verticalOverflow=VerticalWrapMode.Truncate;ballLabel.resizeTextForBestFit=false;ballLabel.resizeTextMinSize=11;ballLabel.resizeTextMaxSize=layout.ballFontSize;

  }

  public string TargetCaption {get;private set;}
  string lastCaption;Vector3 lastCaptionLayout;
  Text Caption(string name){
   var text=GolfHUDStyle.Text(name,targetCard,"",new Vector2(500,54),layout.targetFontSize,TextAnchor.MiddleLeft,GolfHUDStyle.Ink);
   text.rectTransform.pivot=new Vector2(0,.5f);text.horizontalOverflow=HorizontalWrapMode.Overflow;text.resizeTextForBestFit=false;return text;
  }
  // Measure visible glyph edges so the two gaps remain equal with proportional fonts.
  static Vector2 Ink(Text text){
   text.cachedTextGenerator.Populate(text.text,text.GetGenerationSettings(new Vector2(500,54)));
   var vertices=text.cachedTextGenerator.verts;float low=float.PositiveInfinity,high=float.NegativeInfinity;
   for(int i=0;i+3<vertices.Count;i+=4){float a=vertices[i].position.x,b=vertices[i+2].position.x;if(Mathf.Abs(b-a)<.001f)continue;low=Mathf.Min(low,Mathf.Min(a,b));high=Mathf.Max(high,Mathf.Max(a,b));}
   return float.IsInfinity(low)?Vector2.zero:new Vector2(low,high)/text.pixelsPerUnit;
  }
  internal void SetCaption(int number,float distance){
   string caption=$"HOLE {number}  •  {distance:0} m";var settings=new Vector3(layout.targetSize.x,layout.targetFontSize,layout.targetCaptionGap);
   TargetCaption=caption;if(lastCaption==caption&&lastCaptionLayout==settings)return;lastCaption=caption;lastCaptionLayout=settings;
   targetLabel.text=$"HOLE {number}";targetDot.text="•";targetDistance.text=$"{distance:0} m";
   var labels=new[]{targetLabel,targetDot,targetDistance};var bounds=new Vector2[3];
   float available=layout.targetSize.x-layout.targetTextLeftPadding-layout.targetTextRightPadding;
   for(int size=layout.targetFontSize;size>=19;size--){float width=layout.targetCaptionGap*2;for(int i=0;i<3;i++){labels[i].fontSize=size;bounds[i]=Ink(labels[i]);width+=bounds[i].y-bounds[i].x;}if(width<=available||size==19)break;}
   float x=-layout.targetSize.x*.5f+layout.targetTextLeftPadding;
   for(int i=0;i<3;i++){labels[i].rectTransform.anchoredPosition=new Vector2(x-bounds[i].x,-15);x+=bounds[i].y-bounds[i].x+layout.targetCaptionGap;}
  }

  static RectTransform Arrow(string name,Transform parent,Vector2 anchor,Vector2 position,Vector2 size,Color color,string art){var r=GolfLeaderboardUI.Rect(name,parent,anchor,position,size);var arrow=r.gameObject.AddComponent<GolfArrowGraphic>();arrow.enabled=false;arrow.color=color;arrow.raycastTarget=false;GolfReferenceUI.Plate(r,art,false);return r;}

  bool LabelOverlaps(RectTransform obstacle){

   if(!obstacle||!obstacle.gameObject.activeInHierarchy)return false;ballCard.GetWorldCorners(corners);var labelBounds=new Rect(corners[0].x,corners[0].y,corners[2].x-corners[0].x,corners[2].y-corners[0].y);

   obstacle.GetWorldCorners(corners);return labelBounds.Overlaps(new Rect(corners[0].x,corners[0].y,corners[2].x-corners[0].x,corners[2].y-corners[0].y));

  }

  public static float Bearing(Vector3 from,Vector3 to,float yaw){var direction=Quaternion.Euler(0,-yaw,0)*(to-from);return -Mathf.Atan2(direction.x,direction.z)*Mathf.Rad2Deg;}

  void AimDirection(Camera camera,Vector3 target){
   var viewport=camera.WorldToViewportPoint(target);
   bool visible=viewport.z>=camera.nearClipPlane&&viewport.z<=camera.farClipPlane&&viewport.x>=0&&viewport.x<=1&&viewport.y>=0&&viewport.y<=1;
   RectTransformUtility.ScreenPointToLocalPointInRectangle(page,camera.WorldToScreenPoint(target),null,out var point);
   var safe=page.rect;float pad=layout.margin+GolfPresentationSettings.Current.holeGuideSize.x*.6f;
   visible&=point.x>safe.xMin+pad&&point.x<safe.xMax-pad&&point.y>safe.yMin+200&&point.y<safe.yMax-150;
   guideOffscreen=!visible;
   if(visible){targetEdgeArrow.anchoredPosition=point+Vector2.up*34;holeGuide.Point(0);return;}
   var direction=point-page.rect.center;
   // Behind-camera projection can point up at the sky. Give a horizontal
   // turn cue instead, choosing the nearest side in camera space.
   if(viewport.z<=camera.nearClipPlane||point.y>safe.yMax-pad){
    var flat=Vector3.ProjectOnPlane(target-camera.transform.position,Vector3.up);
    var right=Vector3.ProjectOnPlane(camera.transform.right,Vector3.up).normalized;
    float side=Vector3.Dot(flat,right);direction=new Vector2(Mathf.Abs(side)<.05f?0:Mathf.Sign(side),-.35f);
   }
   if(direction.sqrMagnitude<.001f)direction=Vector2.down;
   var half=page.rect.size*.5f-Vector2.one*layout.margin-GolfPresentationSettings.Current.holeGuideSize*.6f;
   half.x=Mathf.Max(1,half.x);half.y=Mathf.Max(1,half.y);
   var edge=direction/Mathf.Max(Mathf.Abs(direction.x)/half.x,Mathf.Abs(direction.y)/half.y);
   // Keep the top-centre card clear even when the hole is above the view.
   float clearance=layout.targetSize.x*layout.aimTargetScale*.5f+layout.aimEdgeArrowSize.x;
   if(edge.y>half.y-1&&Mathf.Abs(edge.x)<clearance)edge.x=Mathf.Min(half.x,clearance)*(edge.x<0?-1:1);
   // The lower edge contains the power hint and shot controls. Use a side
   // edge above that band for targets behind/below the camera instead.
   float bottom=-half.y+layout.actionSize.y+layout.gap+layout.aimEdgeArrowSize.y;
   if(edge.y<bottom){edge.x=(direction.x>0?1:-1)*half.x;edge.y=Mathf.Min(half.y,bottom);}
   targetEdgeArrow.anchoredPosition=edge;
   holeGuide.Point(Mathf.Atan2(direction.x,-direction.y)*Mathf.Rad2Deg);
  }

  void Guide(Camera camera,GolfHole hole,bool aiming){
   var config=GolfPresentationSettings.Current;float desired=aiming&&hole!=null?1:0;
   guideOpacity.alpha=Mathf.MoveTowards(guideOpacity.alpha,desired,Time.unscaledDeltaTime/Mathf.Max(.01f,config.guideFadeSeconds));
   bool visible=hole!=null&&guideOpacity.alpha>.001f;targetEdgeArrow.gameObject.SetActive(visible);
   if(!visible){if(cupGlow)cupGlow.enabled=false;return;}
   holeNumber.text=hole.number.ToString();targetEdgeArrow.sizeDelta=config.holeGuideSize;
   float pulse=MenuPreferences.ReducedMotion?1:1+.035f*Mathf.Sin(Time.unscaledTime*4);targetEdgeArrow.localScale=Vector3.one*pulse;
   // Project the real cup, then offset its badge in screen space. Lifting a
   // nearby world target above an aiming camera can send the pointer skyward.
   AimDirection(camera,hole.cup.position+Vector3.up*.08f);
   if(!cupGlow){var node=new GameObject("Current cup aim halo");cupGlow=node.AddComponent<LineRenderer>();cupGlow.sharedMaterial=Resources.Load<Material>("GolfBallTrail");cupGlow.useWorldSpace=true;cupGlow.loop=true;cupGlow.positionCount=40;cupGlow.widthMultiplier=.022f;cupGlow.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;cupGlow.receiveShadows=false;}
   cupGlow.enabled=true;var tint=GolfHUDStyle.Gold;tint.a=guideOpacity.alpha*.65f;cupGlow.startColor=cupGlow.endColor=tint;
   for(int i=0;i<40;i++){float a=i*Mathf.PI/20;cupGlow.SetPosition(i,hole.cup.TransformPoint(new Vector3(Mathf.Cos(a)*(hole.cupRadius+.12f),.015f,Mathf.Sin(a)*(hole.cupRadius+.12f))));}
  }
  void OnDisable(){if(cupGlow)cupGlow.enabled=false;}
  void OnDestroy(){if(cupGlow)Destroy(cupGlow.gameObject);}

  void LateUpdate(){

   var app=AppRoot.Instance;var match=GolfMatchManager.Instance;var actor=app?app.LocalAthlete:null;

   var hole=match&&match.Context?match.Target(actor):null;var ball=match&&match.Context?match.Ball(actor):null;

   var position=layout.targetPosition;var hud=GolfHUD.Instance;
   bool aiming=app&&app.view&&app.view.GolfAimRequested;
   ballVisibility.alpha=hud?hud.AuxiliaryOpacity:1;

   if(!aiming&&hud&&hud.CourseRect&&hud.CourseRect.gameObject.activeInHierarchy&&scorecard&&scorecard.BoardRect){

    hud.CourseRect.GetWorldCorners(corners);float left=page.InverseTransformPoint(corners[2]).x+layout.gap+layout.targetSize.x*.5f;

    scorecard.BoardRect.GetWorldCorners(corners);float right=page.InverseTransformPoint(corners[0]).x-layout.gap-layout.targetSize.x*.5f;

    if(left<=right)position.x=Mathf.Clamp(position.x,left,right);

   }

   if(aiming)position=new Vector2(0,-layout.aimTargetTopMargin-layout.targetArrowSize.y*layout.aimTargetScale+4-layout.targetSize.y*layout.aimTargetScale*.5f);
   targetCard.localScale=Vector3.one*(aiming?layout.aimTargetScale:1);
   targetCard.anchoredPosition=position;targetCard.sizeDelta=layout.targetSize;targetArrow.anchoredPosition=(aiming?new Vector2(0,-layout.aimTargetTopMargin-layout.targetArrowSize.y*layout.aimTargetScale*.5f):layout.targetArrowPosition+Vector2.right*(position.x-layout.targetPosition.x));targetArrow.sizeDelta=layout.targetArrowSize*(aiming?layout.aimTargetScale:1);

   bool running=hole!=null&&match.State.Running;
   TargetHole=hole?.number??0;targetArrow.gameObject.SetActive(running&&!aiming);targetCard.gameObject.SetActive(running&&!aiming);
   var camera=app&&app.view?app.view.GetComponent<Camera>():null;
   if(camera)Guide(camera,running?hole:null,aiming);else {targetEdgeArrow.gameObject.SetActive(false);if(cupGlow)cupGlow.enabled=false;}

   // Read the presented camera, including the cart's third-person reverse turn.

   if(hole!=null){targetArrow.localRotation=Quaternion.Euler(0,0,Bearing(actor.transform.position,hole.cup.position,app.view.transform.eulerAngles.y));SetCaption(hole.number,Vector3.Distance(actor.transform.position,hole.cup.position));}

   bool show=ball&&match.State.Running&&match.Player(actor)?.IsFinished==false;ballMarker.gameObject.SetActive(show);if(!show)return;BallOwner=ball.Owner;

   var screen=camera.WorldToScreenPoint(ball.Body.position);

   // Keep the arrow tip above the tiny ball at every camera distance.

   RectTransformUtility.ScreenPointToLocalPointInRectangle(page,screen,null,out var point);if(screen.z<0)point=-point;else point+=Vector2.up*34;

   var r=page.rect;var clamped=new Vector2(Mathf.Clamp(point.x,r.xMin+100,r.xMax-265),Mathf.Clamp(point.y,r.yMin+155,r.yMax-180));

   bool off=screen.z<0||Vector2.Distance(clamped,point)>1;ballMarker.anchoredPosition=clamped;

   ballArrow.localRotation=Quaternion.Euler(0,0,off?180+Bearing(actor.transform.position,ball.Body.position,camera.transform.eulerAngles.y):0);

   float distance=Vector3.Distance(actor.transform.position,ball.Body.position);
   nearBall=nearBall?distance<ShowCaptionDistance:distance<HideCaptionDistance;
   ballCard.gameObject.SetActive(!nearBall);
   ballLabel.text="";ballLabel.enabled=false;
   if(nearBall)return;

   // Keep the world arrow fixed; only move its caption clear of fixed HUD cards.

   foreach(var offset in CaptionOffsets){
    ballCard.anchoredPosition=offset;ballCard.GetWorldCorners(corners);var low=page.InverseTransformPoint(corners[0]);var high=page.InverseTransformPoint(corners[2]);
    bool blocked=low.x<r.xMin||high.x>r.xMax||low.y<r.yMin||high.y>r.yMax||LabelOverlaps(targetCard)||hud&&LabelOverlaps(hud.CourseRect)||scorecard&&(LabelOverlaps(scorecard.BoardRect)||LabelOverlaps(scorecard.CountdownRect)||LabelOverlaps(scorecard.NoticeRect));
    if(hud)blocked|=hud.CaptionObscured(ballCard);if(!blocked)break;
   }

  }

 }

}
