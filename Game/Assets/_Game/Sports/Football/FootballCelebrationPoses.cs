using UnityEngine;
namespace WhatTheFish {
 // Control-space masters, in metres on the existing football rig. The long
 // phrases include anticipation, asymmetric accents, recovery and breath.
 // Matching endpoints and cyclic cubic tangents make the wrap continuous.
 public static class FootballCelebrationPoses {
  public static FootballTake[] Defaults(){
   var rest=FootballMotionLibrary.Ready;rest.body=new(0,0,0);rest.pelvis=new(0,-.025f,0);rest.head=new(-2,0,0);
   rest.leftHand=new(-.26f,.65f,.10f);rest.rightHand=new(.26f,.65f,.10f);
   var breath=rest;breath.pelvis.y=-.018f;breath.chest.x=-2;breath.head.y=5;
   var load=rest;load.pelvis.y=-.070f;load.body.x=7;load.leftHand=new(-.28f,.68f,.14f);load.rightHand=new(.28f,.68f,.14f);
   var cheer=rest;cheer.pelvis.y=-.012f;cheer.body=new(-3,0,-3);cheer.chest.y=-6;cheer.head=new(-6,7,2);cheer.leftHand=new(-.32f,1.10f,.10f);cheer.rightHand=new(.32f,1.13f,.10f);
   var cheerLeft=cheer;cheerLeft.pelvis.x=-.022f;cheerLeft.body.z=4;cheerLeft.head.y=-12;cheerLeft.leftHand.y=1.13f;cheerLeft.rightHand.y=1.08f;
   var chest=rest;chest.body=new(3,-6,2);chest.head=new(4,6,-2);chest.rightHand=new(.08f,.82f,.24f);chest.leftHand=new(-.28f,.69f,.16f);
   var prepareSalute=rest;prepareSalute.rightHand=new(.36f,.91f,.16f);prepareSalute.chest.y=3;prepareSalute.head.y=-3;
   var salute=rest;salute.body=new(-2,6,-3);salute.chest.y=5;salute.head=new(-5,-8,0);salute.rightHand=new(.32f,1.12f,.10f);salute.leftHand=new(-.27f,.72f,.16f);
   var sweep=salute;sweep.body.y=-5;sweep.head.y=12;sweep.rightHand=new(.39f,1.03f,.16f);
   var open=rest;open.chest.x=-3;open.head.y=-7;open.leftHand=new(-.37f,.91f,.18f);open.rightHand=new(.37f,.93f,.18f);
   var clapOpen=rest;clapOpen.body.x=2;clapOpen.head.x=1;clapOpen.leftHand=new(-.16f,.84f,.28f);clapOpen.rightHand=new(.16f,.84f,.28f);
   var clap=clapOpen;clap.leftHand.x=-.045f;clap.rightHand.x=.045f;clap.chest.x=1;
   var proud=rest;proud.pelvis.x=.015f;proud.body=new(-2,-4,-2);proud.chest.x=-3;proud.head.y=7;proud.leftHand=new(-.27f,.76f,.12f);proud.rightHand=new(.28f,.74f,.13f);
   FootballPoseKey K(float t,FootballPose p)=>new(t,p);
   FootballTake Take(string name,float seconds,params FootballPoseKey[] keys)=>new(){name=name,duration=seconds,loop=true,keys=keys};
   return new[]{
    Take("Celebrate_Team",8.8f,K(0,rest),K(.10f,load),K(.24f,cheer),K(.36f,cheerLeft),K(.49f,cheer),K(.62f,open),K(.77f,proud),K(.90f,breath),K(1,rest)),
    Take("Celebrate_Salute",9.6f,K(0,rest),K(.13f,chest),K(.19f,prepareSalute),K(.29f,salute),K(.40f,sweep),K(.51f,salute),K(.67f,proud),K(.84f,breath),K(1,rest)),
    Take("Celebrate_Applause",10.4f,K(0,rest),K(.15f,clapOpen),K(.20f,clap),K(.25f,clapOpen),K(.30f,clap),K(.35f,clapOpen),K(.40f,clap),K(.49f,open),K(.64f,salute),K(.78f,proud),K(.92f,breath),K(1,rest)),
    Take("Celebrate_MVP",14.4f,K(0,rest),K(.09f,breath),K(.19f,chest),K(.25f,chest),K(.30f,prepareSalute),K(.40f,salute),K(.47f,sweep),K(.54f,open),K(.62f,clapOpen),K(.655f,clap),K(.69f,clapOpen),K(.725f,clap),K(.76f,clapOpen),K(.85f,proud),K(.94f,breath),K(1,rest))
   };
  }
 }
}
