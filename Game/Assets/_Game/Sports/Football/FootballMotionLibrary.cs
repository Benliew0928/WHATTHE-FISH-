using System;
using UnityEngine;

namespace WhatTheFish {
 // Editable, small control-space performances on the existing skeleton. All
 // offsets are in athlete metres; no bone lengths or gameplay roots are scaled.
 [Serializable] public struct FootballPose {
  public Vector3 pelvis,body,chest,head,leftFoot,rightFoot,leftHand,rightHand;
  public float leftToe,rightToe;
  public static FootballPose Lerp(FootballPose a,FootballPose b,float t)=>new(){pelvis=Vector3.Lerp(a.pelvis,b.pelvis,t),body=Vector3.Lerp(a.body,b.body,t),chest=Vector3.Lerp(a.chest,b.chest,t),head=Vector3.Lerp(a.head,b.head,t),leftFoot=Vector3.Lerp(a.leftFoot,b.leftFoot,t),rightFoot=Vector3.Lerp(a.rightFoot,b.rightFoot,t),leftHand=Vector3.Lerp(a.leftHand,b.leftHand,t),rightHand=Vector3.Lerp(a.rightHand,b.rightHand,t),leftToe=Mathf.Lerp(a.leftToe,b.leftToe,t),rightToe=Mathf.Lerp(a.rightToe,b.rightToe,t)};
  public FootballPose Mirror(){var p=this;p.pelvis.x=-p.pelvis.x;p.body.y=-p.body.y;p.body.z=-p.body.z;p.chest.y=-p.chest.y;p.chest.z=-p.chest.z;p.head.y=-p.head.y;p.head.z=-p.head.z;p.leftFoot=rightFoot;p.leftFoot.x=-p.leftFoot.x;p.rightFoot=leftFoot;p.rightFoot.x=-p.rightFoot.x;p.leftHand=rightHand;p.leftHand.x=-p.leftHand.x;p.rightHand=leftHand;p.rightHand.x=-p.rightHand.x;p.leftToe=rightToe;p.rightToe=leftToe;return p;}
 }
 [Serializable] public struct FootballPoseKey {public float time;public FootballPose pose;public FootballPoseKey(float t,FootballPose p){time=t;pose=p;}}
 [Serializable] public sealed class FootballTake {
  public string name;public float duration;public bool loop;public FootballPoseKey[] keys;
  // Shape-preserving cubic tangents continue through passing poses. Extrema
  // and explicit holds settle without overshoot; every key is not a stop.
  public FootballPose Sample(float t){
   if(keys==null||keys.Length==0)return default;t=loop?Mathf.Repeat(t,1):Mathf.Clamp01(t);
   for(int i=1;i<keys.Length;i++)if(t<=keys[i].time){
    int a=i-1,b=i,c=a>0?a-1:loop?keys.Length-2:a,d=b<keys.Length-1?b+1:loop?1:b;
    float h=Mathf.Max(.0001f,keys[b].time-keys[a].time),h0=a>0?keys[a].time-keys[c].time:loop?1-keys[c].time:0,h1=b<keys.Length-1?keys[d].time-keys[b].time:loop?keys[d].time:0;
    float u=Mathf.Clamp01((t-keys[a].time)/h);var p=keys[c].pose;var q=keys[a].pose;var r=keys[b].pose;var s=keys[d].pose;
    Vector3 V(Vector3 w,Vector3 x,Vector3 y,Vector3 z)=>new(Cubic(w.x,x.x,y.x,z.x,h0,h,h1,u),Cubic(w.y,x.y,y.y,z.y,h0,h,h1,u),Cubic(w.z,x.z,y.z,z.z,h0,h,h1,u));
    return new FootballPose{pelvis=V(p.pelvis,q.pelvis,r.pelvis,s.pelvis),body=V(p.body,q.body,r.body,s.body),chest=V(p.chest,q.chest,r.chest,s.chest),head=V(p.head,q.head,r.head,s.head),leftFoot=V(p.leftFoot,q.leftFoot,r.leftFoot,s.leftFoot),rightFoot=V(p.rightFoot,q.rightFoot,r.rightFoot,s.rightFoot),leftHand=V(p.leftHand,q.leftHand,r.leftHand,s.leftHand),rightHand=V(p.rightHand,q.rightHand,r.rightHand,s.rightHand),leftToe=Cubic(p.leftToe,q.leftToe,r.leftToe,s.leftToe,h0,h,h1,u),rightToe=Cubic(p.rightToe,q.rightToe,r.rightToe,s.rightToe,h0,h,h1,u)};
   }return keys[^1].pose;
  }
  static float Tangent(float a,float b,float h0,float h1){if(a*b<=0||h0<=0||h1<=0)return 0;float w0=2*h1+h0,w1=h1+2*h0;return (w0+w1)/(w0/a+w1/b);}
  static float Cubic(float p,float a,float b,float n,float hp,float h,float hn,float u){float slope=(b-a)/h,m0=hp>0?Tangent((a-p)/hp,slope,hp,h):0,m1=hn>0?Tangent(slope,(n-b)/hn,h,hn):0;float u2=u*u,u3=u2*u;return (2*u3-3*u2+1)*a+(u3-2*u2+u)*h*m0+(-2*u3+3*u2)*b+(u3-u2)*h*m1;}
 }
 [CreateAssetMenu(menuName="WHATTHE FISH?/Football motion library")]
 public sealed class FootballMotionLibrary:ScriptableObject {
  public FootballTake[] takes;
  public FootballTake Find(string id){foreach(var take in takes)if(take.name==id)return take;return null;}
  public static FootballPose Ready=>new(){pelvis=new(0,-.035f,0),body=new(4,0,0),head=new(-3,0,0),leftHand=new(-.24f,.70f,.21f),rightHand=new(.24f,.70f,.21f),leftFoot=new(-.012f,0,-.02f),rightFoot=new(.012f,0,.02f)};
  public static FootballTake[] Defaults(){
   var list=new System.Collections.Generic.List<FootballTake>();var r=Ready;
   void Add(string n,float d,bool loop,params FootballPoseKey[] k){list.Add(new(){name=n,duration=d,loop=loop,keys=k});}
   void Pair(string n,float d,params FootballPoseKey[] k){Add(n+"_L",d,false,k);var m=new FootballPoseKey[k.Length];for(int i=0;i<k.Length;i++)m[i]=new(k[i].time,k[i].pose.Mirror());Add(n+"_R",d,false,m);}
   FootballPoseKey K(float t,FootballPose p)=>new(t,p);
   var breathe=r;breathe.pelvis.y+=.006f;breathe.chest.x=-1;breathe.head.y=4;
   Add("Ready",2.8f,true,K(0,r),K(.5f,breathe),K(1,r));
   // Walk and dribble records set posture; continuous distance-driven footwork
   // is composed by FootballMotion, so input changes never restart a loop.
   var walk=r;walk.body.x=7;walk.pelvis.y=-.02f;Add("Walk",1,true,K(0,walk),K(1,walk));
   var start=r;start.body.x=15;start.pelvis=new(0,-.055f,.018f);start.chest.y=-5;start.leftHand=new(-.23f,.74f,.25f);start.rightHand=new(.23f,.65f,.10f);
   Pair("Start",.18f,K(0,r),K(.36f,start),K(1,r));
   var stop=r;stop.body.x=-7;stop.pelvis=new(.012f,-.085f,-.015f);stop.leftFoot.z+=.09f;stop.rightFoot.z-=.05f;stop.leftHand=new(-.27f,.73f,.20f);stop.rightHand=new(.27f,.72f,.21f);
   Pair("Stop",.30f,K(0,r),K(.28f,stop),K(.65f,stop),K(1,r));
   var dribble=r;dribble.pelvis=new(0,-.065f,.025f);dribble.body.x=10;dribble.head.x=3;dribble.leftHand=new(-.25f,.73f,.23f);dribble.rightHand=new(.25f,.70f,.19f);
   Add("Dribble_Control",1.1f,true,K(0,dribble),K(1,dribble));var fast=dribble;fast.body.x=15;fast.head.x=-3;Add("Dribble_Fast",.8f,true,K(0,fast),K(1,fast));
   var receive=dribble;receive.body.x=7;receive.leftFoot=new(.04f,.035f,.10f);receive.rightFoot.z=-.03f;receive.chest.y=-7;
   Pair("Receive",.22f,K(0,r),K(.35f,receive),K(.65f,receive),K(1,dribble));
   var settle=dribble;settle.leftFoot=new(.035f,.04f,.11f);settle.body.x=-2;settle.pelvis.y=-.085f;
   Pair("BallSettle",.28f,K(0,dribble),K(.35f,settle),K(.6f,settle),K(1,r));
   var cut=dribble;cut.pelvis.x=.028f;cut.body.z=-12;cut.chest.y=-9;cut.head.y=-9;cut.leftFoot.x=-.035f;cut.rightFoot=new(-.015f,.04f,.025f);cut.leftHand=new(-.27f,.75f,.23f);cut.rightHand=new(.26f,.77f,.25f);
   Pair("Cut",.25f,K(0,dribble),K(.38f,cut),K(.7f,cut),K(1,dribble));
   var kick=dribble;kick.leftFoot=new(.06f,.065f,.18f);kick.rightFoot.z=-.045f;kick.pelvis=new(.025f,-.055f,.03f);kick.body=new(-5,-6,3);kick.chest.y=12;kick.leftHand=new(-.28f,.75f,.12f);kick.rightHand=new(.26f,.80f,.24f);
   var follow=kick;follow.leftFoot=new(.035f,.17f,.15f);follow.body.x=-10;
   Pair("Kick",.35f,K(0,kick),K(.28f,follow),K(.6f,follow),K(1,r));
   var cancel=dribble;cancel.leftFoot=new(0,.04f,-.065f);cancel.body=new(8,-8,0);cancel.leftHand=new(-.25f,.73f,.16f);
   Pair("FakeCancel",.18f,K(0,cancel),K(.4f,dribble),K(1,r));
   var back=r;back.pelvis=new(0,-.24f,-.025f);back.body=new(-65,0,5);back.chest.x=8;back.head.x=20;back.leftFoot=new(.035f,.015f,.12f);back.rightFoot=new(-.025f,.07f,.09f);back.leftHand=new(-.24f,.16f,-.20f);back.rightHand=new(.23f,.34f,-.15f);
   var rise=r;rise.pelvis=new(0,-.16f,.02f);rise.body=new(28,0,0);rise.leftFoot.z=-.055f;rise.rightFoot.z=.06f;rise.leftHand=new(-.23f,.37f,.45f);rise.rightHand=new(.23f,.39f,.44f);
   var recoil=r;recoil.body.x=-20;recoil.head.x=12;recoil.pelvis.y=-.09f;
   var roll=rise;roll.pelvis.y=-.24f;roll.body=new(-8,0,10);roll.leftHand=new(-.24f,.40f,.09f);roll.rightHand=new(.23f,.43f,.10f);
   Add("Fall_Back",1.10f,false,K(0,r),K(.09f,recoil),K(.27f,back),K(.36f,back),K(.54f,roll),K(.73f,rise),K(1,r));
   var front=back;front.body=new(62,0,0);front.head.x=-22;front.pelvis=new(0,-.24f,0);front.leftFoot=new(.03f,0,-.12f);front.rightFoot=new(-.02f,.04f,-.07f);front.leftHand=new(-.22f,.18f,.66f);front.rightHand=new(.22f,.18f,.66f);recoil.body.x=21;recoil.head.x=-10;recoil.leftHand=new(-.23f,.78f,.43f);recoil.rightHand=new(.23f,.78f,.43f);
   Add("Fall_Forward",1.10f,false,K(0,r),K(.09f,recoil),K(.27f,front),K(.38f,front),K(.69f,rise),K(1,r));
   var side=back;side.pelvis=new(-.015f,-.19f,0);side.body=new(12,0,62);side.head=new(-5,0,-18);side.leftFoot=new(.02f,.015f,.06f);side.rightFoot=new(-.02f,.07f,.025f);side.leftHand=new(-.50f,.20f,.17f);side.rightHand=new(-.17f,.45f,.31f);recoil=r;recoil.body.z=22;
   var sk=new[]{K(0,r),K(.09f,recoil),K(.27f,side),K(.38f,side),K(.69f,rise),K(1,r)};Add("Fall_Left",1.10f,false,sk);var sr=new FootballPoseKey[sk.Length];for(int i=0;i<sk.Length;i++)sr[i]=K(sk[i].time,sk[i].pose.Mirror());Add("Fall_Right",1.10f,false,sr);
   var slideLoad=r;slideLoad.pelvis=new(0,-.10f,0);slideLoad.body=new(-16,0,-6);slideLoad.leftHand=new(-.23f,.53f,-.04f);slideLoad.rightHand=new(.24f,.77f,.13f);
   var slide=back;slide.pelvis=new(.015f,-.23f,-.01f);slide.body=new(-62,0,-8);slide.head.x=18;slide.leftFoot=new(.01f,.04f,.12f);slide.rightFoot=new(-.02f,.015f,-.06f);slide.leftHand=new(-.25f,.16f,-.29f);slide.rightHand=new(.24f,.46f,-.03f);
   var gather=rise;gather.body.x=12;gather.pelvis=new(0,-.15f,0);gather.leftFoot.z=.06f;gather.rightFoot.z=-.05f;gather.leftHand=new(-.25f,.45f,.23f);gather.rightHand=new(.25f,.45f,.23f);
   var slideRise=gather;slideRise.pelvis.y=-.20f;slideRise.body.x=-35;slideRise.leftHand=new(-.25f,.35f,-.13f);slideRise.rightHand=new(.25f,.53f,.10f);
   Add("Slide_Tackle",.68f,false,K(0,r),K(.12f,slideLoad),K(.29f,slide),K(.43f,slide),K(.55f,slideRise),K(.72f,gather),K(1,r));
   var load=r;load.pelvis.y=-.075f;load.body.x=10;load.leftHand=new(-.23f,.64f,.08f);load.rightHand=new(.23f,.64f,.08f);
   var launch=r;launch.pelvis.y=-.005f;launch.body.x=2;launch.leftHand=new(-.24f,.90f,.30f);launch.rightHand=new(.24f,.90f,.30f);
   var apex=launch;apex.body.x=6;apex.leftFoot=new(.01f,.065f,-.025f);apex.rightFoot=new(-.01f,.055f,-.015f);apex.leftToe=-8;apex.rightToe=-8;apex.leftHand=new(-.24f,.84f,.24f);apex.rightHand=new(.24f,.84f,.24f);
   var land=load;land.pelvis.y=-.08f;land.leftHand=new(-.26f,.73f,.27f);land.rightHand=new(.26f,.73f,.27f);
   Add("Jump",1,false,K(0,r),K(.12f,load),K(.32f,launch),K(.52f,apex),K(.72f,r),K(.82f,land),K(1,r));
   var whiff=r;whiff.pelvis.y=-.07f;whiff.body.x=14;whiff.leftHand=new(-.24f,.70f,.16f);whiff.rightHand=new(.24f,.70f,.16f);Add("WhiffRecover",.8f,false,K(0,whiff),K(.65f,whiff),K(1,r));
   var bump=r;bump.body=new(-8,0,6);bump.chest.y=8;bump.leftHand=new(-.25f,.80f,.27f);bump.rightHand=new(.25f,.8f,.27f);Add("Bump",.22f,false,K(0,r),K(.25f,bump),K(1,r));
   var win=r;win.leftHand=new(-.25f,1.09f,.19f);win.rightHand=new(.25f,1.09f,.19f);win.body.x=-4;win.pelvis.y=-.01f;var sigh=r;sigh.head.x=16;sigh.body.x=9;sigh.leftHand=new(-.24f,.61f,.16f);sigh.rightHand=new(.24f,.61f,.16f);
   Add("Result_Win",1.7f,false,K(0,r),K(.3f,win),K(.75f,win),K(1,r));Add("Result_Disappointed",1.7f,false,K(0,r),K(.35f,sigh),K(.8f,sigh),K(1,r));
   return list.ToArray();
  }
 }
}
