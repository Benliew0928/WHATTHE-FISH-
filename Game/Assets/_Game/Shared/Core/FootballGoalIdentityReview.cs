#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

namespace WhatTheFish {
 public sealed class FootballGoalIdentityReview:MonoBehaviour {
  string folder;bool failed;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,"-goalIdentityReview");if(i>=0&&i+1<a.Length)new GameObject("Goal identity review").AddComponent<FootballGoalIdentityReview>().folder=a[i+1];}
  void Check(bool ok,string label){failed|=!ok;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+label+"\n");}
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");
   float end=Time.realtimeSinceStartup+100;while(!DevelopmentProbe.ReadyToReviewSport&&Time.realtimeSinceStartup<end)yield return null;
   var ball=FootballBall.Instance;Check(ball&&ball.GoalCount==2,"both goals found");if(!ball)yield break;
   var goals=ball.Pitch.GetComponentsInChildren<FootballGoalIdentity>();Check(goals.Length==2,"both ends have side identity");
   yield return new WaitForSeconds(2);
   var camera=new GameObject("Goal identity capture").AddComponent<Camera>();camera.enabled=false;camera.fieldOfView=58;camera.farClipPlane=600;
   foreach(var goal in goals){
    string name=goal.Sign<0?"blue":"red";
    Check(goal.Team==(goal.Sign<0?FootballTeam.A:FootballTeam.B),name+" matches defending team");
    Check(goal.GetComponentsInChildren<TextMesh>().Count(t=>t.text==(goal.Sign<0?"BLUE GOAL":"RED GOAL"))==2,name+" labels on both sides of mouth");
    var markers=goal.transform.Find("Goal side markers");Check(markers&&markers.GetComponentsInChildren<Collider>().All(c=>!c.enabled),name+" decoration adds no gameplay collision");
    var frame=goal.GetComponentsInChildren<MeshRenderer>(true).FirstOrDefault(r=>r.name=="Goal__Frame");var tint=new MaterialPropertyBlock();if(frame)frame.GetPropertyBlock(tint);
    Check(frame&&tint.GetColor("_BaseColor")==FootballGoalIdentity.EndColor(goal.Sign),name+" frame tinted");
    int index=Enumerable.Range(0,2).First(i=>ball.GoalSign(i)==goal.Sign);var bounds=ball.GoalBounds(index);float z=ball.GoalFront(index);
    foreach(bool wide in new[]{false,true}){
     camera.transform.position=ball.Pitch.TransformPoint(new Vector3(wide?12:7,bounds.min.y+(wide?10:2.1f),z-goal.Sign*(wide?33:12)));
     camera.transform.LookAt(ball.Pitch.TransformPoint(new Vector3(bounds.center.x,bounds.min.y+.8f,z)));
     yield return new WaitForEndOfFrame();Capture(camera,name+(wide?"-wide":"-close"));
    }
   }
   Destroy(camera.gameObject);Check(!RoomTeleport.Busy,"viewing did not change gameplay state");
   File.AppendAllText(Path.Combine(folder,"results.txt"),"GOAL_IDENTITY_COMPLETE success="+!failed+"\n");Application.Quit(failed?1:0);
  }
  void Capture(Camera camera,string name){var rt=RenderTexture.GetTemporary(1440,810,24);var previous=RenderTexture.active;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var pixels=new Texture2D(1440,810,TextureFormat.RGB24,false);pixels.ReadPixels(new Rect(0,0,1440,810),0,0);pixels.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),pixels.EncodeToPNG());Destroy(pixels);camera.targetTexture=null;RenderTexture.active=previous;RenderTexture.ReleaseTemporary(rt);}
 }
}
#endif
