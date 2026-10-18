using UnityEngine;
using UnityEngine.Rendering;
namespace WhatTheFish {
 [DefaultExecutionOrder(1000)]
 public sealed class BasketballTrail:MonoBehaviour {
  const int PulseCount=4,Segments=40;
  public SportsMotionRibbon Wake {get;private set;}
  public int Style {get;private set;}=-1;
  public int ReleasePulses {get;private set;}
  public int BouncePulses {get;private set;}
  public int ActivePulses {get {int active=0;foreach(var ring in rings)if(ring&&ring.enabled)active++;return active;}}
  readonly LineRenderer[] rings=new LineRenderer[PulseCount];readonly float[] born=new float[PulseCount];readonly bool[] floor=new bool[PulseCount];
  readonly Vector3[] centres=new Vector3[PulseCount],axisX=new Vector3[PulseCount],axisY=new Vector3[PulseCount];
  BasketballBall ball;bool seen;uint reset,shots,passes;Vector3 previous,velocity;float impactAt;int cursor;bool shot;
  void Awake(){
   ball=GetComponent<BasketballBall>();var node=new GameObject("Basketball flight wake");node.transform.SetParent(transform,false);Wake=node.AddComponent<SportsMotionRibbon>();
   for(int i=0;i<PulseCount;i++){
    var go=new GameObject("Basketball energy ring "+i);go.transform.SetParent(transform,false);var ring=go.AddComponent<LineRenderer>();rings[i]=ring;
    ring.useWorldSpace=true;ring.loop=true;ring.positionCount=Segments;ring.enabled=false;ring.alignment=LineAlignment.View;
    ring.shadowCastingMode=ShadowCastingMode.Off;ring.receiveShadows=false;ring.numCornerVertices=2;ring.textureMode=LineTextureMode.Stretch;
   }
  }
  public void Clear(){if(Wake)Wake.Clear();foreach(var ring in rings)if(ring)ring.enabled=false;seen=false;velocity=Vector3.zero;}
  void OnDisable()=>Clear();
  void Pulse(Vector3 centre,Vector3 normal,bool ground){
   if(MenuPreferences.ReducedMotion)return;
   int i=cursor++%PulseCount;born[i]=Time.time;centres[i]=centre;floor[i]=ground;
   normal=normal.sqrMagnitude>.01f?normal.normalized:Vector3.up;
   axisX[i]=Vector3.Cross(normal,Mathf.Abs(normal.y)>.9f?Vector3.forward:Vector3.up).normalized;axisY[i]=Vector3.Cross(normal,axisX[i]);rings[i].enabled=true;
   if(ground)BouncePulses++;else ReleasePulses++;
  }
  void LateUpdate(){
   if(Style!=BasketballTrailStyle.Selected){Style=BasketballTrailStyle.Selected;Clear();}
   var match=BasketballMatch.Instance;
   if(!ball.Playing||ball.PortalBusy||ball.InboundPending||match&&match.Competitive&&!match.BallLive){Clear();return;}
   var position=transform.position;var material=BasketballTrailStyle.Material(Style);
   bool releasedShot=seen&&shots!=ball.ShotCount,releasedPass=seen&&passes!=ball.PassCount;
   var releaseDirection=position-previous;
   if(!seen||reset!=ball.PresentationReset){Clear();seen=true;reset=ball.PresentationReset;shots=ball.ShotCount;passes=ball.PassCount;previous=position;shot=ball.Score.result==BasketballResult.Flying;}
   var nextVelocity=(position-previous)/Mathf.Max(.001f,Time.deltaTime);
   if(Vector3.Distance(position,previous)>Mathf.Max(2.5f,Time.deltaTime*55)||Time.unscaledDeltaTime>.25f){Clear();return;}
   if((releasedShot||releasedPass)&&!ball.Held){shot=releasedShot;shots=ball.ShotCount;passes=ball.PassCount;Wake.Clear();Pulse(position,releaseDirection,false);}
   if(!ball.Held&&velocity.y<-.7f&&nextVelocity.y>.5f&&Time.time>=impactAt&&Physics.Raycast(position+Vector3.up*.1f,Vector3.down,out var hit,.8f,1<<8,QueryTriggerInteraction.Ignore)){
    Pulse(hit.point+hit.normal*.025f,hit.normal,true);impactAt=Time.time+.18f;
   }
   // Held dribbles/aims stay clean; the ball remains legible in the hands.
   if(ball.Held)Wake.Clear();
   else {bool reduced=MenuPreferences.ReducedMotion;float width=shot?.72f:.52f;Wake.Step(position,material,width*(reduced?.65f:1),reduced?.24f:shot?.62f:.44f,reduced?1.5f:shot?4.4f:3.2f);}
   for(int i=0;i<PulseCount;i++)if(rings[i].enabled){
    float age=(Time.time-born[i])/(floor[i]?.34f:.26f);if(age>=1||MenuPreferences.ReducedMotion){rings[i].enabled=false;continue;}
    float radius=Mathf.Lerp(.14f,floor[i]?.65f:.48f,1-(1-age)*(1-age));var ring=rings[i];ring.sharedMaterial=material;ring.widthMultiplier=.055f*(1-age);
    var color=new Color(1,1,1,(1-age)*.9f);ring.startColor=ring.endColor=color;
    for(int j=0;j<Segments;j++){float a=j*Mathf.PI*2/Segments;ring.SetPosition(j,centres[i]+(axisX[i]*Mathf.Cos(a)+axisY[i]*Mathf.Sin(a))*radius);}
   }
   previous=position;if(Time.deltaTime>0&&nextVelocity.sqrMagnitude>.0001f)velocity=nextVelocity;
  }
 }
}
