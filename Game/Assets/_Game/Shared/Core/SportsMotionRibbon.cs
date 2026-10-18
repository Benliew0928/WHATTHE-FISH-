using UnityEngine;
using UnityEngine.Rendering;
namespace WhatTheFish {
 // Explicit world-space head avoids gaps between physics/network updates.
 public sealed class SportsMotionRibbon:MonoBehaviour {
  const int Capacity=96;
  readonly Vector3[] history=new Vector3[Capacity];readonly float[] times=new float[Capacity];
  int count;float stoppedAt=-1;Vector3 previous;bool sampled;
  public LineRenderer Line {get;private set;}
  public bool Emitting {get;private set;}
  public float Speed {get;private set;}
  public float Length {get;private set;}
  public Vector3 Head=>Line&&Line.positionCount>0?Line.GetPosition(0):transform.position;
  void Awake(){
   Line=gameObject.AddComponent<LineRenderer>();Line.useWorldSpace=true;Line.enabled=false;
   Line.alignment=LineAlignment.View;Line.textureMode=LineTextureMode.Stretch;
   Line.numCornerVertices=3;Line.numCapVertices=3;Line.shadowCastingMode=ShadowCastingMode.Off;
   Line.receiveShadows=false;Line.generateLightingData=false;Line.positionCount=0;
   Line.widthCurve=new AnimationCurve(new Keyframe(0,.65f),new Keyframe(.12f,1),new Keyframe(.6f,.6f),new Keyframe(1,0));
   Line.colorGradient=new Gradient{colorKeys=new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},alphaKeys=new[]{new GradientAlphaKey(1,0),new GradientAlphaKey(.8f,.5f),new GradientAlphaKey(0,1)}};
  }
  public void Clear(){count=0;sampled=false;Speed=Length=0;stoppedAt=-1;Emitting=false;if(Line){Line.positionCount=0;Line.enabled=false;}}
  public void Step(Vector3 position,Material material,float width,float lifetime,float maxLength){
   Line.sharedMaterial=material;
   if(!material||Time.deltaTime<=0||Time.unscaledDeltaTime>.25f){Clear();return;}
   if(!sampled){previous=position;history[0]=position;times[0]=Time.time;count=1;sampled=true;return;}
   float distance=Vector3.Distance(position,previous);
   if(distance>Mathf.Max(2.5f,Time.deltaTime*55)){Clear();return;}
   float speed=distance/Mathf.Max(.001f,Time.deltaTime);
   Speed=Mathf.Lerp(Speed,speed,1-Mathf.Exp(-18*Time.deltaTime));
   Emitting=distance>.0001f&&Speed>.15f;
   if(Emitting){
    stoppedAt=-1;
    if(count==0||Vector3.Distance(position,history[0])>.055f||Time.time-times[0]>.045f){
     int last=Mathf.Min(count,Capacity-1);for(int i=last;i>0;i--){history[i]=history[i-1];times[i]=times[i-1];}
     history[0]=position;times[0]=Time.time;count=Mathf.Min(Capacity,count+1);
    }
   }else if(stoppedAt<0)stoppedAt=Time.time;
   while(count>0&&Time.time-times[count-1]>lifetime)count--;
   if(count==0||stoppedAt>=0&&Time.time-stoppedAt>=lifetime){Line.enabled=false;Line.positionCount=0;Length=0;previous=position;return;}
   // Even a stationary interpolation frame keeps the leading vertex at the ball.
   Line.positionCount=count+1;Line.SetPosition(0,position);Length=0;Vector3 lastPoint=position;int used=1;
   for(int i=0;i<count;i++){
    float segment=Vector3.Distance(lastPoint,history[i]);if(segment<.001f)continue;
    var next=Length+segment>maxLength?Vector3.Lerp(lastPoint,history[i],(maxLength-Length)/segment):history[i];
    Length+=Vector3.Distance(lastPoint,next);Line.SetPosition(used++,next);lastPoint=next;if(Length>=maxLength-.001f)break;
   }
   Line.positionCount=used;Line.widthMultiplier=width*(stoppedAt<0?1:Mathf.Clamp01(1-(Time.time-stoppedAt)/lifetime));
   Line.enabled=used>1&&Length>.015f;previous=position;
  }
  void OnDisable()=>Clear();
 }
}
