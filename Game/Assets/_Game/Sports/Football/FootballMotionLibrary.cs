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
  public FootballPose Sample(float t){if(keys==null||keys.Length==0)return default;t=loop?Mathf.Repeat(t,1):Mathf.Clamp01(t);for(int i=1;i<keys.Length;i++)if(t<=keys[i].time){float u=Mathf.InverseLerp(keys[i-1].time,keys[i].time,t);return FootballPose.Lerp(keys[i-1].pose,keys[i].pose,u*u*(3-2*u));}return keys[^1].pose;}
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
   var back=r;back.pelvis=new(0,-.31f,-.025f);back.body=new(-65,0,5);back.chest.x=8;back.head.x=20;back.leftFoot=new(.035f,.015f,.12f);back.rightFoot=new(-.025f,.07f,.09f);back.leftHand=new(-.24f,.14f,-.13f);back.rightHand=new(.23f,.34f,-.15f);
   var rise=r;rise.pelvis=new(0,-.22f,.02f);rise.body=new(28,0,0);rise.leftFoot.z=-.055f;rise.rightFoot.z=.06f;rise.leftHand=new(-.23f,.30f,.30f);rise.rightHand=new(.23f,.32f,.29f);
   var recoil=r;recoil.body.x=-20;recoil.head.x=12;recoil.pelvis.y=-.09f;
   var roll=rise;roll.pelvis.y=-.31f;roll.body=new(-8,0,10);roll.leftHand=new(-.24f,.40f,.09f);roll.rightHand=new(.23f,.43f,.10f);
   Add("Fall_Back",.8f,false,K(0,r),K(.10f,recoil),K(.30f,back),K(.46f,back),K(.60f,roll),K(.76f,rise),K(1,r));
   var front=back;front.body=new(62,0,0);front.head.x=-22;front.pelvis=new(0,-.31f,0);front.leftFoot=new(.03f,0,-.12f);front.rightFoot=new(-.02f,.04f,-.07f);front.leftHand=new(-.22f,.17f,.39f);front.rightHand=new(.22f,.17f,.39f);recoil.body.x=21;recoil.head.x=-10;
   Add("Fall_Forward",.8f,false,K(0,r),K(.10f,recoil),K(.30f,front),K(.48f,front),K(.7f,rise),K(1,r));
   var side=back;side.pelvis=new(-.015f,-.26f,0);side.body=new(12,0,62);side.head=new(-5,0,-18);side.leftFoot=new(.02f,.015f,.06f);side.rightFoot=new(-.02f,.07f,.025f);side.leftHand=new(-.34f,.17f,.17f);side.rightHand=new(-.03f,.43f,.28f);recoil=r;recoil.body.z=22;
   var sk=new[]{K(0,r),K(.1f,recoil),K(.3f,side),K(.48f,side),K(.7f,rise),K(1,r)};Add("Fall_Left",.8f,false,sk);var sr=new FootballPoseKey[sk.Length];for(int i=0;i<sk.Length;i++)sr[i]=K(sk[i].time,sk[i].pose.Mirror());Add("Fall_Right",.8f,false,sr);
   var whiff=r;whiff.pelvis.y=-.07f;whiff.body.x=14;whiff.leftHand=new(-.24f,.70f,.16f);whiff.rightHand=new(.24f,.70f,.16f);Add("WhiffRecover",.8f,false,K(0,whiff),K(.65f,whiff),K(1,r));
   var bump=r;bump.body=new(-8,0,6);bump.chest.y=8;bump.leftHand=new(-.25f,.80f,.27f);bump.rightHand=new(.25f,.8f,.27f);Add("Bump",.22f,false,K(0,r),K(.25f,bump),K(1,r));
   var win=r;win.leftHand=new(-.25f,1.09f,.19f);win.rightHand=new(.25f,1.09f,.19f);win.body.x=-4;win.pelvis.y=-.01f;var sigh=r;sigh.head.x=16;sigh.body.x=9;sigh.leftHand=new(-.24f,.61f,.16f);sigh.rightHand=new(.24f,.61f,.16f);
   Add("Result_Win",1.7f,false,K(0,r),K(.3f,win),K(.75f,win),K(1,r));Add("Result_Disappointed",1.7f,false,K(0,r),K(.35f,sigh),K(.8f,sigh),K(1,r));
   return list.ToArray();
  }
 }
}
