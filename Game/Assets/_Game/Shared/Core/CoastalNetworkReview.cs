#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace WhatTheFish {
 public sealed class CoastalNetworkReview:MonoBehaviour {
  string report;bool failed;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){
   var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-coastalNetworkReview");if(i<0)return;
   var review=new GameObject("Coastal network route review").AddComponent<CoastalNetworkReview>();review.report=args[i+1];
  }
  void Check(bool ok,string text){failed|=!ok;File.AppendAllText(report,(ok?"PASS ":"FAIL ")+text+"\n");}
  IEnumerator Start(){
   File.WriteAllText(report,"");AppRoot app=null;
   while(!(app=AppRoot.Instance)||!app.Exploring||!NetworkManager.Singleton||!NetworkManager.Singleton.IsListening)yield return null;
   var net=NetworkManager.Singleton;var routes=app.stadium.GetComponent<CoastalVenueRoutes>();
   if(net.IsServer){
    foreach(var player in FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None)){
     int n=(int)player.OwnerClientId%4;var athlete=player.GetComponent<Athlete>();athlete.capsule.enabled=false;
     player.GetComponent<NetworkTransform>().Teleport(routes.entries[n],Quaternion.identity,Vector3.one);athlete.ResetLocomotion();athlete.capsule.enabled=true;
    }
   }
   int index=(int)net.LocalClientId%4;float wait=0;
   while(Vector3.Distance(app.LocalAthlete.transform.position,routes.entries[index])>2&&wait<8){wait+=Time.deltaTime;yield return null;}
   Check(wait<8,"server placed local player at entrance");
   var remote=FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None).First(p=>!p.IsOwner);var remoteStart=remote.transform.position;bool remoteMoved=false;
   var outside=routes.entries[index]+(routes.entries[index]-routes.inside[index]).normalized*12;outside.y=-.30f;
   var points=new[]{routes.inside[index],routes.entries[index],outside,routes.entries[index],routes.stairBottom[index]}
    .Concat(routes.stairs[index].points).Concat(new[]{routes.concourse[index],routes.stairTop[index]})
    .Concat(routes.stairs[index].points.Reverse()).Concat(new[]{routes.entries[index]}).ToList();
   if(index==1){
    bool football=app.SelectedSport==SportId.Football;
    var approach=new Vector3(0,.15f,football?-139:-69);var dock=new Vector3(0,-.48f,football?-165:-94);
    points.AddRange(new[]{approach,dock,approach,routes.entries[index],routes.inside[index]});
   }
   DevelopmentProbe.TurnCommandActive=true;
   foreach(var target in points){
    var start=app.LocalAthlete.transform.position;float timeout=Vector3.Distance(start,target)/2+6,elapsed=0;
    while(elapsed<timeout){
     var d=target-app.LocalAthlete.transform.position;d.y=0;if(d.magnitude<.32f)break;
     DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.up,heading=Mathf.Atan2(d.x,d.z)*Mathf.Rad2Deg};
     elapsed+=Time.deltaTime;remoteMoved|=Vector3.Distance(remote.transform.position,remoteStart)>2;yield return null;
    }
    DevelopmentProbe.TurnCommand=default;
    var error=target-app.LocalAthlete.transform.position;Check(new Vector2(error.x,error.z).magnitude<.6f&&Mathf.Abs(error.y)<.6f,"replicated route target="+target+" actual="+app.LocalAthlete.transform.position);
   }
   Check(remoteMoved,"remote athlete movement replicated");Check(app.LocalAthlete.capsule.enabled==net.IsServer,"server owns collision simulation");
   File.AppendAllText(report,"NETWORK_COASTAL_COMPLETE success="+(!failed)+"\n");
  }
 }
}
#endif
