#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
namespace WhatTheFish {
 public sealed class RefinedIslandNetworkReview:MonoBehaviour {
  string report;bool failed;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,"-islandNetworkReview");if(i<0)return;new GameObject("Island network route validation").AddComponent<RefinedIslandNetworkReview>().report=a[i+1];}
  void Check(bool ok,string label){failed|=!ok;File.AppendAllText(report,(ok?"PASS ":"FAIL ")+label+"\n");}
  List<Vector3> Points(IslandLayout layout,bool host){
   if(layout.sport=="Golf"){
    var route=layout.routes.Single(r=>r.name==(host?"Main fairway":"Coastal promenade")).points;
    return route.Concat(route.Reverse()).ToList();
   }
   var points=new List<Vector3>();var circuit=layout.routes.Single(r=>r.name=="Lagoon circuit").points;
   // Separate lanes keep this environment test independent of athlete-to-athlete contact.
   for(int i=0;i<circuit.Length;i++){
    var p=circuit[i];
    var point=p;if(!host){if(point.z>33&&Mathf.Abs(point.x)<8)point.z+=.8f;else{var radial=new Vector3(p.x,0,p.z).normalized;point+=radial*.8f;}}points.Add(point);
    if(host)foreach(var route in layout.routes.Where(r=>r.name.StartsWith("Station")))if(i==Enumerable.Range(0,circuit.Length).OrderBy(n=>Vector3.Distance(circuit[n],route.points[0])).First()){points.AddRange(route.points);points.AddRange(route.points.Reverse());}
   }
   if(!host){var bridge=layout.routes.Single(r=>r.name=="Inlet bridge").points.Select(p=>p+Vector3.forward*.8f).ToArray();points.AddRange(bridge);points.AddRange(bridge.Reverse());}
   return points;
  }
  IEnumerator Start(){
   File.WriteAllText(report,"");AppRoot app=null;while(!(app=AppRoot.Instance)||!app.Exploring||!NetworkManager.Singleton||!NetworkManager.Singleton.IsListening)yield return null;
   var net=NetworkManager.Singleton;var layout=app.stadium.GetComponent<RefinedIslandEnvironment>().layout;var points=Points(layout,net.IsServer);
   if(net.IsServer)foreach(var player in FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None)){
    var start=Points(layout,player.OwnerClientId==net.LocalClientId)[0];var athlete=player.GetComponent<Athlete>();athlete.capsule.enabled=false;player.GetComponent<NetworkTransform>().Teleport(start,Quaternion.identity,Vector3.one);athlete.ResetLocomotion();athlete.capsule.enabled=true;
   }
   float wait=0;while(Vector3.Distance(app.LocalAthlete.transform.position,points[0])>2&&wait<10){wait+=Time.deltaTime;yield return null;}Check(wait<10,"Authoritative route placement");
   var remote=FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None).First(p=>!p.IsOwner);var initial=remote.transform.position;bool moved=false;DevelopmentProbe.TurnCommandActive=true;int reached=0;
   foreach(var target in points){
    float timeout=Vector3.Distance(app.LocalAthlete.transform.position,target)/3+5,elapsed=0;
    while(elapsed<timeout){var d=target-app.LocalAthlete.transform.position;d.y=0;if(d.magnitude<.38f)break;DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.up,heading=Mathf.Atan2(d.x,d.z)*Mathf.Rad2Deg,sprint=layout.sport=="Golf"};elapsed+=Time.deltaTime;moved|=Vector3.Distance(remote.transform.position,initial)>2;yield return null;}
    DevelopmentProbe.TurnCommand=default;var e=target-app.LocalAthlete.transform.position;bool ok=new Vector2(e.x,e.z).magnitude<.7f&&Mathf.Abs(e.y)<.8f;if(ok)reached++;else{Check(false,"Route waypoint "+target+" actual="+app.LocalAthlete.transform.position);break;}
   }
   Check(reached==points.Count,"Replicated route waypoints "+reached+"/"+points.Count);Check(moved,"Remote movement replicated");Check(app.LocalAthlete.capsule.enabled==net.IsServer,"Server owns collision simulation");
   DevelopmentProbe.TurnCommandActive=false;File.AppendAllText(report,"ISLAND_NETWORK_COMPLETE success="+(!failed)+"\n");
  }
 }
}
#endif
