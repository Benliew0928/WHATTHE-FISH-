using UnityEngine;
namespace WhatTheFish {
 // Basketball control-space masters on the shared athlete skeleton. Grounded
 // knee dips, a shooting follow-through and jersey acknowledgement distinguish
 // the court ceremony from football. Matching endpoints keep each loop smooth.
 public static class BasketballCelebrationPoses {
  public static FootballTake[] Defaults(){
   var rest=FootballMotionLibrary.Ready;rest.body=Vector3.zero;rest.pelvis=new(0,-.025f,0);rest.head=new(-2,0,0);
   rest.leftHand=new(-.26f,.65f,.10f);rest.rightHand=new(.26f,.65f,.10f);
   var breath=rest;breath.pelvis.y=-.018f;breath.chest.x=-2;breath.head.y=6;
   var dip=rest;dip.pelvis.y=-.075f;dip.body.x=6;dip.leftHand=new(-.28f,.68f,.15f);dip.rightHand=new(.28f,.68f,.15f);
   var cheer=rest;cheer.body=new(-3,0,-3);cheer.head=new(-5,8,2);cheer.leftHand=new(-.32f,1.09f,.12f);cheer.rightHand=new(.32f,1.12f,.12f);
   var left=cheer;left.body.z=3;left.head.y=-12;left.leftHand.y=1.12f;left.rightHand.y=1.07f;
   var jersey=rest;jersey.chest.y=-5;jersey.head=new(4,5,0);jersey.rightHand=new(.10f,.84f,.23f);
   var gather=dip;gather.leftHand=new(-.14f,.85f,.27f);gather.rightHand=new(.14f,.88f,.27f);gather.head.x=-6;
   var release=rest;release.body=new(-2,4,-2);release.head=new(-8,3,0);release.leftHand=new(-.24f,1.04f,.17f);release.rightHand=new(.24f,1.13f,.13f);
   var follow=release;follow.leftHand=new(-.27f,.76f,.17f);follow.rightHand=new(.29f,1.12f,.15f);follow.chest.y=3;
   var salute=follow;salute.head.y=-10;salute.rightHand=new(.37f,1.04f,.16f);
   var open=rest;open.chest.x=-3;open.head.y=-7;open.leftHand=new(-.36f,.91f,.17f);open.rightHand=new(.36f,.93f,.17f);
   var clapOpen=rest;clapOpen.body.x=2;clapOpen.leftHand=new(-.16f,.84f,.28f);clapOpen.rightHand=new(.16f,.84f,.28f);
   var clap=clapOpen;clap.leftHand.x=-.045f;clap.rightHand.x=.045f;
   var proud=rest;proud.body=new(-2,-4,-2);proud.chest.x=-3;proud.head.y=7;proud.leftHand=new(-.27f,.76f,.12f);proud.rightHand=new(.28f,.74f,.13f);
   FootballPoseKey K(float t,FootballPose p)=>new(t,p);
   FootballTake Take(string name,float seconds,params FootballPoseKey[] keys)=>new(){name=name,duration=seconds,loop=true,keys=keys};
   return new[]{
    Take("Court_Team",8.6f,K(0,rest),K(.10f,dip),K(.24f,cheer),K(.36f,left),K(.49f,cheer),K(.62f,open),K(.78f,proud),K(.92f,breath),K(1,rest)),
    Take("Court_Shooter",9.8f,K(0,rest),K(.12f,gather),K(.23f,release),K(.33f,follow),K(.44f,follow),K(.56f,salute),K(.72f,jersey),K(.90f,breath),K(1,rest)),
    Take("Court_Applause",9.4f,K(0,rest),K(.14f,clapOpen),K(.20f,clap),K(.26f,clapOpen),K(.32f,clap),K(.38f,clapOpen),K(.44f,clap),K(.58f,open),K(.72f,cheer),K(.90f,breath),K(1,rest)),
    Take("Court_MVP",13.6f,K(0,rest),K(.08f,breath),K(.17f,jersey),K(.23f,jersey),K(.31f,gather),K(.41f,release),K(.48f,follow),K(.56f,follow),K(.65f,salute),K(.76f,open),K(.86f,proud),K(.94f,breath),K(1,rest))
   };
  }
 }
}
