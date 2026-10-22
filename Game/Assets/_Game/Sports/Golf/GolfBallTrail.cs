using UnityEngine;
using UnityEngine.Rendering;

namespace WhatTheFish {
 // Presentation only: follows the displayed ball, including client interpolation.
 [DefaultExecutionOrder(200)]
 public sealed class GolfBallTrail:MonoBehaviour {
  public const float Lifetime=1f;
  [Range(.1f,2)] public float lifetime=Lifetime;
  [Range(.03f,.4f)] public float width=.18f;
  public TrailRenderer Ribbon {get;private set;}
  public bool Active {get;private set;}
  public uint SeenStrike {get;private set;}
  GolfBall ball;Vector3 lastPosition;float lastMovement;
  MaterialPropertyBlock properties;
  GolfEventKind tintKind;
  public void EventTint(GolfEventKind kind){
   if(!Ribbon||kind==tintKind)return;tintKind=kind;var config=GolfPresentationSettings.Current;
   var color=kind==GolfEventKind.StickyGreen?config.stickyColor:kind==GolfEventKind.BounceParty?config.bounceColor:new Color(1,.83f,.22f);
   Ribbon.colorGradient=new Gradient{colorKeys=new[]{new GradientColorKey(color,0),new GradientColorKey(Color.Lerp(color,Color.white,.6f),1)},alphaKeys=new[]{new GradientAlphaKey(1,0),new GradientAlphaKey(.6f,.45f),new GradientAlphaKey(0,1)}};
  }
  static readonly int Fade=Shader.PropertyToID("_Fade");
  void Awake(){
   ball=GetComponent<GolfBall>();
   var node=new GameObject("Golden shot wake");node.transform.SetParent(transform,false);
   Ribbon=node.AddComponent<TrailRenderer>();Ribbon.sharedMaterial=Resources.Load<Material>("GolfBallTrail");
   Ribbon.time=lifetime;Ribbon.minVertexDistance=.025f;Ribbon.widthMultiplier=width;
   // Newest point is the ball centre; taper continuously toward the oldest point.
   Ribbon.widthCurve=AnimationCurve.Linear(0,1,1,0);
   Ribbon.colorGradient=new Gradient {
    colorKeys=new[]{new GradientColorKey(new Color(1,.83f,.22f),0),new GradientColorKey(new Color(1,.94f,.68f),1)},
    alphaKeys=new[]{new GradientAlphaKey(1,0),new GradientAlphaKey(.6f,.45f),new GradientAlphaKey(0,1)}
   };
   Ribbon.alignment=LineAlignment.View;Ribbon.textureMode=LineTextureMode.Stretch;
   Ribbon.numCornerVertices=4;Ribbon.numCapVertices=0;Ribbon.shadowCastingMode=ShadowCastingMode.Off;Ribbon.receiveShadows=false;
   Ribbon.generateLightingData=false;Ribbon.autodestruct=false;Ribbon.emitting=false;
   properties=new MaterialPropertyBlock();Ribbon.enabled=false;SetFade(0);
  }
  internal void Begin(uint sequence){
   SeenStrike=sequence;Ribbon.Clear();lastPosition=transform.position;lastMovement=Time.time;
   Active=true;Ribbon.enabled=true;Ribbon.emitting=true;Ribbon.time=lifetime;Ribbon.widthMultiplier=width;SetFade(1);
  }
  internal void Clear(){Active=false;Ribbon.emitting=false;Ribbon.enabled=false;Ribbon.Clear();SetFade(0);}
  void SetFade(float alpha){properties.SetFloat(Fade,alpha);Ribbon.SetPropertyBlock(properties);}
  void LateUpdate(){
   if(!Active)return;
   if(!ball||!ball.Live){Clear();return;}
   bool moved=(transform.position-lastPosition).sqrMagnitude>.000001f;
   if(moved){lastPosition=transform.position;lastMovement=Time.time;}
   // The client body is kinematic; measure displayed motion instead of velocity.
   Ribbon.emitting=moved;
   float age=Time.time-lastMovement;
   SetFade(moved?1:Mathf.Clamp01(1-age/lifetime));
   if(age>=lifetime)Clear();
  }
  void OnDisable(){if(Ribbon)Clear();}
 }
}
