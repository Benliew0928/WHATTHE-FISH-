using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace WhatTheFish {
 // A separate, silent render camera; never takes control of the player's view.
 [DefaultExecutionOrder(240)]
 public sealed class GolfFollowCamera:MonoBehaviour {
  GolfMatchManager match;GolfBall ball;GolfFeedbackSettings settings;Camera lens;RenderTexture texture;
  RectTransform panel;CanvasGroup opacity;RawImage picture;Vector3 direction=Vector3.forward,lastPoint,cameraVelocity;
  uint reset,round;float stoppedAt=-1,stillFor;bool tracking,settled;int renderedFrame=-1;
  readonly Vector3[] corners=new Vector3[4];
  public bool Visible=>tracking&&opacity&&opacity.alpha>0;
  public float Opacity=>opacity?opacity.alpha:0;
  public bool Rendering=>tracking&&lens&&renderedFrame>=Time.frameCount-1;
  public RectTransform Window=>panel;
  public GolfBall TrackedBall=>ball;
  void Awake(){match=GetComponent<GolfMatchManager>();settings=Resources.Load<GolfFeedbackSettings>("GolfFeedback");}
  void Build(){
   if(panel||!GolfHUD.Instance)return;
   var parent=GolfHUD.Instance.FrameRect;
   panel=GolfHUDStyle.Rect("Golf ball camera inset",parent,new Vector2(0,1),Vector2.zero,settings.followWindowSize);panel.pivot=new Vector2(0,1);
   var border=panel.gameObject.AddComponent<Image>();border.color=new Color(.03f,.75f,.9f,1);border.raycastTarget=false;
   opacity=panel.gameObject.AddComponent<CanvasGroup>();opacity.interactable=opacity.blocksRaycasts=false;opacity.alpha=0;
   var inner=GolfHUDStyle.Rect("Live ball view",panel,new Vector2(.5f,.5f),Vector2.zero,Vector2.zero);inner.anchorMin=Vector2.zero;inner.anchorMax=Vector2.one;inner.offsetMin=Vector2.one*4;inner.offsetMax=-Vector2.one*4;
   picture=inner.gameObject.AddComponent<RawImage>();picture.raycastTarget=false;
   var title=GolfHUDStyle.Text("Ball camera caption",panel,"BALL CAM",new Vector2(120,26),16,TextAnchor.MiddleLeft,Color.white);title.rectTransform.anchorMin=title.rectTransform.anchorMax=new Vector2(0,1);title.rectTransform.pivot=new Vector2(0,1);title.rectTransform.anchoredPosition=new Vector2(12,-6);title.raycastTarget=false;
   var shadow=title.gameObject.AddComponent<Outline>();shadow.effectColor=new Color(0,.14f,.2f,1);shadow.effectDistance=Vector2.one;
   texture=new RenderTexture(512,288,16,RenderTextureFormat.ARGB32){name="Golf ball inset",antiAliasing=1};texture.Create();picture.texture=texture;
   var go=new GameObject("Golf ball inset camera");go.transform.SetParent(transform,false);lens=go.AddComponent<Camera>();lens.enabled=false;lens.targetTexture=texture;lens.fieldOfView=48;lens.nearClipPlane=.08f;lens.farClipPlane=180;lens.cullingMask=~(1<<5);lens.clearFlags=CameraClearFlags.Skybox;lens.allowHDR=false;lens.allowMSAA=false;lens.depth=-2;
   var data=go.AddComponent<UniversalAdditionalCameraData>();data.renderPostProcessing=false;data.renderShadows=false;data.requiresColorTexture=false;data.requiresDepthTexture=false;
  }
  public void Contact(ulong hitter,ulong owner){
   var app=AppRoot.Instance;if(!app||!app.LocalAthlete||hitter!=GolfCartWorld.Key(app.LocalAthlete)||!match.Context)return;
   Build();if(!lens)return;ball=match.Ball(owner);if(!ball)return;
   reset=ball.ResetSequence;round=match.Round;lastPoint=ball.transform.position;
   var v=ball.Velocity;direction=Vector3.ProjectOnPlane(v.sqrMagnitude>.1f?v:app.view.transform.forward,Vector3.up).normalized;
   if(direction.sqrMagnitude<.1f)direction=Vector3.forward;
   tracking=true;settled=false;stoppedAt=-1;stillFor=0;cameraVelocity=Vector3.zero;opacity.alpha=1;
   Position(lastPoint,true);panel.SetAsLastSibling();
  }
  public void Cup(ulong owner){if(tracking&&ball&&ball.Owner==owner){Freeze();}}
  void Freeze(){settled=true;stoppedAt=Time.unscaledTime;ball=null;}
  void Position(Vector3 point,bool snap){
   var desired=point-direction*3.4f+Vector3.up*2.1f;
   var delta=desired-point;
   if(Physics.SphereCast(point+Vector3.up*.12f,.15f,delta.normalized,out var hit,delta.magnitude,1<<8,QueryTriggerInteraction.Ignore))desired=point+Vector3.up*.12f+delta.normalized*Mathf.Max(.5f,hit.distance-.15f);
   lens.transform.position=snap?desired:Vector3.SmoothDamp(lens.transform.position,desired,ref cameraVelocity,.12f,100,Time.unscaledDeltaTime);
   lens.transform.LookAt(point+Vector3.up*.12f);
  }
  void LateUpdate(){
   if(!tracking)return;
   if(!match.Context||match.Round!=round){Hide();return;}
   var hud=GolfHUD.Instance;if(!hud){Hide();return;}
   hud.CourseRect.GetWorldCorners(corners);var parent=hud.FrameRect;var top=parent.InverseTransformPoint(corners[0]);
   float width=Mathf.Min(settings.followWindowSize.x,parent.rect.width*.37f),height=width*settings.followWindowSize.y/settings.followWindowSize.x;
   float available=Mathf.Max(60,parent.rect.height*.45f);if(height>available){width*=available/height;height=available;}
   panel.sizeDelta=new Vector2(width,height);panel.anchoredPosition=new Vector2(top.x-parent.rect.xMin,top.y-parent.rect.yMax-12);
   if(ball){
    if(ball.ResetSequence!=reset||!ball.Live){Freeze();}
    else {
     var point=ball.transform.position;float speed=Vector3.Distance(point,lastPoint)/Mathf.Max(.001f,Time.unscaledDeltaTime);lastPoint=point;
     Position(point,false);
     bool grounded=Physics.Raycast(point+Vector3.up*.03f,Vector3.down,GolfBall.Radius+.1f,(1<<8)|(1<<GolfTee.Layer),QueryTriggerInteraction.Ignore);
     bool quiet=!ball.FollowingShot&&grounded&&speed<.1f;
     stillFor=quiet?stillFor+Time.unscaledDeltaTime:0;
     if(stillFor>=.25f){if(!settled){settled=true;stoppedAt=Time.unscaledTime;}}
     else {settled=false;stoppedAt=-1;}
    }
   }
   if(settled){float age=Time.unscaledTime-stoppedAt;opacity.alpha=1-Mathf.Clamp01((age-settings.followRestHold)/settings.followFadeSeconds);if(opacity.alpha<=0)Hide();}
   if(tracking){lens.Render();renderedFrame=Time.frameCount;}
  }
  void Hide(){tracking=settled=false;ball=null;if(opacity)opacity.alpha=0;if(lens)lens.enabled=false;}
  void OnDisable(){Hide();}
  void OnDestroy(){if(lens)Destroy(lens.gameObject);if(panel)Destroy(panel.gameObject);if(texture){texture.Release();Destroy(texture);}}
 }
}
