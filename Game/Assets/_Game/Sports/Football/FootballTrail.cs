using UnityEngine;
using UnityEngine.Rendering;
namespace WhatTheFish {
 // Measures the displayed transform: works equally on authority, interpolated guests,
 // attached dribbles and render-only replay copies. Never reads the kicker's identity.
 [DefaultExecutionOrder(300)]
 public sealed class FootballTrail:MonoBehaviour {
  public const float Lifetime=.55f;
  public TrailRenderer Ribbon {get;private set;}
  public bool IsReplay {get;set;}
  public int Style {get;private set;}=-1;
  FootballBall ball;Vector3 previous;bool sampled;float lastMotion;
  void Awake(){
   ball=GetComponent<FootballBall>();
   var node=new GameObject("Personal football trail");node.transform.SetParent(transform,false);
   Ribbon=node.AddComponent<TrailRenderer>();Ribbon.emitting=false;Ribbon.time=Lifetime;
   Ribbon.minVertexDistance=.045f;Ribbon.widthMultiplier=.64f;
   Ribbon.widthCurve=new AnimationCurve(new Keyframe(0,.65f),new Keyframe(.2f,1),new Keyframe(1,0));
   Ribbon.colorGradient=new Gradient{colorKeys=new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},alphaKeys=new[]{new GradientAlphaKey(.95f,0),new GradientAlphaKey(.65f,.55f),new GradientAlphaKey(0,1)}};
   Ribbon.alignment=LineAlignment.View;Ribbon.textureMode=LineTextureMode.Stretch;
   Ribbon.numCornerVertices=2;Ribbon.numCapVertices=2;Ribbon.shadowCastingMode=ShadowCastingMode.Off;Ribbon.receiveShadows=false;Ribbon.generateLightingData=false;Ribbon.autodestruct=false;
  }
  public void Clear(){if(!Ribbon)return;Ribbon.emitting=false;Ribbon.enabled=false;Ribbon.Clear();sampled=false;}
  void OnDisable()=>Clear();
  void LateUpdate(){
   if(Style!=FootballTrailStyle.Selected){Style=FootballTrailStyle.Selected;Clear();}
   Ribbon.sharedMaterial=FootballTrailStyle.Material(Style);
   bool visible=IsReplay?FootballReplay.Presenting:ball&&FootballTackle.EnvironmentAllowed&&!FootballReplay.Presenting&&!ball.PortalBusy&&!(SkySailWorld.Instance&&SkySailWorld.Instance.Travelling);
   if(!visible){Clear();return;}
   var position=transform.position;
   if(!sampled){previous=position;sampled=true;lastMotion=Time.time;return;}
   float distance=Vector3.Distance(position,previous);
   if(distance>3||Time.unscaledDeltaTime>.2f){Clear();return;}
   float speed=distance/Mathf.Max(.001f,Time.deltaTime);
   Ribbon.time=Mathf.Clamp(3.5f/Mathf.Max(1,speed),.12f,Lifetime);
   Ribbon.emitting=speed>.18f;
   if(Ribbon.emitting){lastMotion=Time.time;Ribbon.enabled=true;}
   else if(Time.time-lastMotion>=Lifetime){Ribbon.enabled=false;Ribbon.Clear();}
   previous=position;
  }
 }
}
