#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;

namespace WhatTheFish {
 public sealed class BasketballBallReview:MonoBehaviour {
  string folder;bool failed;BasketballBall ball;AppRoot app;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){
   var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-basketballBallReview");if(i<0||i+1>=args.Length)return;
   var review=new GameObject("Basketball ball review").AddComponent<BasketballBallReview>();review.folder=args[i+1];
  }
  void Check(bool ok,string message){failed|=!ok;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+message+"\n");}
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");
   float timeout=Time.realtimeSinceStartup+90;
   while((!(app=AppRoot.Instance)||!app.Exploring||app.SelectedSport!=SportId.Basketball)&&Time.realtimeSinceStartup<timeout)yield return null;
   if(!app||!app.Exploring||app.SelectedSport!=SportId.Basketball){Check(false,"basketball exploration initialized");Finish();yield break;}
   DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;
   ball=app.stadium.GetComponentInChildren<BasketballBall>();
   if(!ball){Check(false,"ball exists in streamed arena");Finish();yield break;}
   ball.autoPickup=false; // Isolate the existing material/contact tests from possession.
   yield return new WaitForFixedUpdate();
   Check(app.stadium.GetComponentsInChildren<BasketballBall>().Length==1,"one ball in active basketball arena");
   Check(ball.GetComponentsInChildren<Collider>().Length==1&&ball.GetComponent<SphereCollider>().radius==.12f,"one analytical sphere collider at 0.12 m radius");
   Check(ball.GetComponentsInChildren<SkinnedMeshRenderer>().Length==0,"rigid mesh has no skeleton");
   Check(ball.GetComponent<LODGroup>().GetLODs().Length==2,"two delivery LODs");
   Check(app.stadium.GetComponentsInChildren<CapsuleCollider>().Count(c=>c.name.StartsWith("Rim segment"))==64,"both rims have 32 physical segments");
   if(!ball.Authority){yield return Guest();Finish();yield break;}
   Check(!ball.Body.isKinematic&&ball.Simulating,"authority simulates rigid body");
   Check(ball.Body.collisionDetectionMode==CollisionDetectionMode.ContinuousSpeculative,"speculative contacts prevent bounce-speed floor penetration");
   Check(!ball.Place(new Vector3(float.NaN,0,0),Quaternion.identity,Vector3.zero,Vector3.zero),"invalid launch coordinates rejected");
   var body=ball.Body;
   ball.Place(new Vector3(0,2.12f,-1.5f),Quaternion.identity,Vector3.zero,Vector3.zero);
   float upward=0,minY=100;
   File.WriteAllText(Path.Combine(folder,"drop.csv"),"step,physicsY,renderY,velocityY\n");
   for(int i=0;i<100;i++){yield return new WaitForFixedUpdate();upward=Mathf.Max(upward,body.linearVelocity.y);minY=Mathf.Min(minY,body.position.y);File.AppendAllText(Path.Combine(folder,"drop.csv"),$"{i},{body.position.y:R},{ball.transform.position.y:R},{body.linearVelocity.y:R}\n");}
   Check(upward>3.5f&&upward<6.5f,"2 m drop rebounds; max upward speed="+upward.ToString("F3"));
   Check(minY>.105f,"floor contact does not tunnel; minimum centre height="+minY.ToString("F3"));
   ball.Place(new Vector3(0,1.12f,-1.5f),Quaternion.identity,Vector3.down*35,Vector3.zero);
   upward=0;minY=100;
   for(int i=0;i<12;i++){yield return new WaitForFixedUpdate();upward=Mathf.Max(upward,body.linearVelocity.y);minY=Mathf.Min(minY,body.position.y);}
   Check(minY>.105f&&upward>15,"35 m/s downward shot rebounds above floor; minimum centre="+minY.ToString("F3")+" speed="+upward.ToString("F2"));
   ball.Place(new Vector3(-2,.125f,0),Quaternion.identity,new Vector3(3,0,0),new Vector3(0,0,-25));
   var start=body.position;float rotation=0;var q=body.rotation;
   for(int i=0;i<50;i++){yield return new WaitForFixedUpdate();rotation+=Quaternion.Angle(q,body.rotation);q=body.rotation;}
   Check(body.position.x-start.x>1,"ball rolls across the real court; distance="+(body.position.x-start.x).ToString("F3"));Check(rotation>180,"rolling changes orientation; accumulated degrees="+rotation.ToString("F1"));
   foreach(string hoopName in new[]{"Hoop_North","Hoop_South"}){
    var hoop=app.stadium.GetComponentsInChildren<Transform>().Single(t=>t.name==hoopName);
    Vector3 Local(Vector3 p)=>ball.transform.parent.InverseTransformPoint(hoop.TransformPoint(p));
    ball.Place(Local(new Vector3(0,3.7f,0)),Quaternion.identity,new Vector3(0,-4,0),Vector3.zero);
    for(int i=0;i<14;i++)yield return new WaitForFixedUpdate();
    Check(ball.transform.localPosition.y<2.55f,hoopName+" clean centre shot passes through rim and decorative net");
    ball.Place(Local(new Vector3(.2476f,3.7f,0)),Quaternion.identity,new Vector3(0,-4,0),Vector3.zero);
    upward=0;for(int i=0;i<22;i++){yield return new WaitForFixedUpdate();upward=Mathf.Max(upward,body.linearVelocity.y);}
    Check(upward>1,hoopName+" off-centre shot hits rim; upward speed="+upward.ToString("F2"));
    var board=hoop.GetComponentsInChildren<MeshRenderer>().Single(r=>r.name=="Hoop__BoardInset");
    var centre=board.bounds.center;var inward=Vector3.ProjectOnPlane(-hoop.position,Vector3.up).normalized;
    ball.Place(ball.transform.parent.InverseTransformPoint(centre+inward*.8f),Quaternion.identity,-inward*35,Vector3.right*18);
    float rebound=0;for(int i=0;i<12;i++){yield return new WaitForFixedUpdate();rebound=Mathf.Max(rebound,Vector3.Dot(body.linearVelocity,inward));}
    Check(rebound>8,hoopName+" 35 m/s backboard collision rebounds without tunnelling; speed="+rebound.ToString("F2"));
   }
   ball.Place(new Vector3(0,-4,-1.5f),Quaternion.identity,Vector3.zero,Vector3.zero);yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();
   Check(ball.transform.localPosition.y>.1f,"out-of-bounds ball resets to court");
   bool network=NetworkManager.Singleton&&NetworkManager.Singleton.IsListening;
   if(!network){
    ball.Body.useGravity=false;ball.Place(new Vector3(0,1.2f,-1.5f),Quaternion.Euler(10,20,15),Vector3.zero,Vector3.zero);
    yield return new WaitForFixedUpdate();yield return new WaitForEndOfFrame();
    app.view.enabled=false;foreach(var canvas in FindObjectsByType<Canvas>(FindObjectsSortMode.None))canvas.enabled=false;
    var camera=Camera.main;camera.nearClipPlane=.025f;
    foreach(var view in new[]{("close",new Vector3(.1f,.045f,-.44f)),("reverse",new Vector3(.12f,.08f,.44f)),("court",new Vector3(.8f,.35f,-2.8f))}){
     camera.transform.position=ball.transform.position+view.Item2;camera.transform.LookAt(ball.transform.position);
     yield return new WaitForEndOfFrame();yield return new WaitForEndOfFrame();Capture(camera,view.Item1);
    }
    body.useGravity=true;ball.ResetHome();
    app.SelectSport(SportId.Football);
    yield return new WaitForSeconds(.5f);var stream=app.environments.GetComponent<SkySailStreaming>();while(stream.Busy)yield return null;
    Check(!FindFirstObjectByType<BasketballBall>(),"basketball releases when travelling to another sport");
    yield return stream.Prepare(SportId.Basketball);app.SelectSport(SportId.Basketball);yield return new WaitForSeconds(.3f);
    Check(FindObjectsByType<BasketballBall>(FindObjectsSortMode.None).Length==1,"returning to basketball creates exactly one ball");
    app.Show("sport"); // Exploring is tested through the public room flag in network review.
   }else{
    ball.Place(new Vector3(-1,1.5f,-1.5f),Quaternion.identity,new Vector3(2,3,0),new Vector3(4,12,7));
    yield return new WaitForSeconds(4);ball.ResetHome();
   }
   Finish();
  }
  IEnumerator Guest(){
   Check(ball.Body.isKinematic&&!ball.Body.detectCollisions,"guest cannot simulate independent ball physics");
   Check(!ball.Place(Vector3.up*10,Quaternion.identity,Vector3.one,Vector3.one),"guest cannot change authoritative ball pose");
   Vector3 start=ball.transform.position;Quaternion first=ball.transform.rotation;bool moved=false,rotated=false;int received=0,checks=0;uint reset=0;double tick=0;float settle=0,maxError=0;
   float end=Time.unscaledTime+23;
   while(Time.unscaledTime<end){
    var host=NetworkAthlete.HostPlayer;if(!host)break;var snapshot=host.Basketball.Value;
    if(snapshot.valid&&snapshot.time!=tick){tick=snapshot.time;received++;if(snapshot.reset!=reset){reset=snapshot.reset;settle=Time.unscaledTime+.35f;}}
    moved|=Vector3.Distance(start,ball.transform.position)>.5f;rotated|=Quaternion.Angle(first,ball.transform.rotation)>25;
    if(snapshot.valid&&Time.unscaledTime>settle){var error=Vector3.Distance(ball.transform.localPosition,snapshot.position);maxError=Mathf.Max(error,maxError);checks++;}
    yield return null;
   }
   Check(received>150,"guest received host ball snapshots="+received);Check(moved&&rotated,"guest sees ball translation and rotation");
   Check(checks>50&&maxError<1.2f,"guest follows host snapshots within interpolation window; max error="+maxError.ToString("F3"));
   Check(ball.Body.isKinematic,"guest remains kinematic after host shots");
  }
  void Capture(Camera camera,string name){
   if(SystemInfo.graphicsDeviceType==GraphicsDeviceType.Null)return;
   var rt=RenderTexture.GetTemporary(1400,1000,24);var active=RenderTexture.active;var previous=camera.targetTexture;
   camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
   var image=new Texture2D(1400,1000,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1400,1000),0,0);image.Apply();
   File.WriteAllBytes(Path.Combine(folder,name+".png"),image.EncodeToPNG());Destroy(image);camera.targetTexture=previous;RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);
  }
  void Finish(){File.AppendAllText(Path.Combine(folder,"results.txt"),"BASKETBALL_REVIEW_COMPLETE success="+(!failed)+"\n");}
 }
}
#endif
