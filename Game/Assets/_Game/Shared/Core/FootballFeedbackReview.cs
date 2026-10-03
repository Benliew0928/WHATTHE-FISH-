#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

namespace WhatTheFish {
 // Opt-in, real player render/behaviour checks. No review implementation in release.
 public sealed class FootballFeedbackReview:MonoBehaviour {
  string folder;bool failed;Camera review;Athlete actor;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){
   var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-footballFeedbackReview");if(i<0||i+1>=args.Length)return;
   new GameObject("Football feedback review").AddComponent<FootballFeedbackReview>().folder=args[i+1];
  }
  void Check(bool ok,string message){failed|=!ok;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+message+"\n");}
  void Place(Vector3 position,float yaw=0){actor.capsule.enabled=false;actor.transform.SetPositionAndRotation(position,Quaternion.Euler(0,yaw,0));actor.ResetLocomotion();actor.capsule.enabled=true;Physics.SyncTransforms();}
  void Capture(string name,Vector3 position,Vector3 focus){
   review.transform.position=position;review.transform.LookAt(focus);
   var rt=RenderTexture.GetTemporary(1280,720,24);var old=RenderTexture.active;review.targetTexture=rt;review.Render();RenderTexture.active=rt;
   var texture=new Texture2D(1280,720,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,1280,720),0,0);texture.Apply();
   File.WriteAllBytes(Path.Combine(folder,name+".png"),texture.EncodeToPNG());Destroy(texture);review.targetTexture=null;RenderTexture.active=old;RenderTexture.ReleaseTemporary(rt);
  }
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"Football feedback render and lifecycle checks\n");
   float deadline=Time.realtimeSinceStartup+60;
   while((!AppRoot.Instance||!AppRoot.Instance.LocalAthlete||!FootballBall.Instance)&&Time.realtimeSinceStartup<deadline)yield return null;
   if(!AppRoot.Instance||!AppRoot.Instance.LocalAthlete||!FootballBall.Instance){Check(false,"football loaded");Finish();yield break;}
   AppRoot.Instance.EnterOffline();DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;
   yield return new WaitForSeconds(.6f);
   var ball=FootballBall.Instance;actor=AppRoot.Instance.LocalAthlete;var view=PlayerView.Instance;
   var wall=ball.Pitch.GetComponentInChildren<FootballBoundaryVisual>();Check(wall,"boundary attached to authored pitch");
   if(!wall){Finish();yield break;}
   var mesh=wall.GetComponent<MeshFilter>().sharedMesh;var renderer=wall.GetComponent<MeshRenderer>();
   Check(mesh.vertexCount==24&&mesh.triangles.Length==36,"six lightweight panels leave both goal mouths open");
   Check(wall.GetComponentsInChildren<Collider>().Length==0,"visual wall does not change player or ball collision");
   Check(renderer.sharedMaterial.shader.isSupported&&renderer.sharedMaterial.renderQueue==3000,"transparent wall shader available in built player");
   Check(Mathf.Abs(renderer.bounds.size.y-FootballBoundaryVisual.Height)<.002f,"world wall height is 1.8 metres");
   var bounds=ball.PitchBounds;
   Check(Mathf.Abs(mesh.bounds.min.x-bounds.min.x)<.001f&&Mathf.Abs(mesh.bounds.max.z-bounds.max.z)<.001f,"visual aligns with authoritative pitch edges");
   var vertices=mesh.vertices;
   for(int g=0;g<ball.GoalCount;g++){
    var goal=ball.GoalBounds(g);float z=ball.GoalSign(g)>0?bounds.max.z:bounds.min.z;bool clear=true;
    for(int i=0;i<vertices.Length;i+=4)if(Mathf.Abs(vertices[i].z-z)<.001f&&Mathf.Abs(vertices[i+1].z-z)<.001f)
     clear&=Mathf.Max(vertices[i].x,vertices[i+1].x)<=goal.min.x+.001f||Mathf.Min(vertices[i].x,vertices[i+1].x)>=goal.max.x-.001f;
    Check(clear,"goal "+g+" mouth has no visual panel");
   }
   FootballBoundaryVisual.Attach(ball);Check(ball.Pitch.GetComponentsInChildren<FootballBoundaryVisual>().Length==1,"attachment is idempotent");
   review=new GameObject("Football feedback capture camera").AddComponent<Camera>();review.CopyFrom(Camera.main);review.enabled=false;review.fieldOfView=55;
   var centre=ball.Pitch.TransformPoint(new Vector3(bounds.center.x,bounds.max.y,bounds.center.z));
   var corner=ball.Pitch.TransformPoint(new Vector3(bounds.min.x,bounds.max.y,bounds.min.z));
   Capture("wall-close",corner+new Vector3(7,2.5f,9),corner+new Vector3(0,.6f,4));
   Place(corner+new Vector3(2,0,4),-90);ball.ResetBall();
   ball.Body.position=corner+new Vector3(2,ball.WorldRadius,7);ball.Body.WakeUp();ball.Body.linearVelocity=Vector3.left*14;
   float previousImpact=wall.LastImpactTime;
   Time.captureFramerate=30;
   for(int frame=0;frame<48;frame++){
    yield return new WaitForEndOfFrame();Capture("wall-"+frame.ToString("D2"),corner+new Vector3(7,2.5f,9),corner+new Vector3(0,.6f,4));
   }
   Check(wall.LastImpactTime>previousImpact,"free ball boundary contact triggers a ripple");
   float impactTime=wall.LastImpactTime;yield return new WaitForSeconds(.15f);
   Check(wall.LastImpactTime==impactTime,"resting ball does not repeatedly trigger ripple");
   Time.captureFramerate=0;
   Capture("wall-overview",centre+new Vector3(42,36,-52),centre);
   var goalCentre=ball.Pitch.TransformPoint(new Vector3(ball.GoalBounds(0).center.x,bounds.max.y,ball.GoalFront(0)));
   float sign=ball.GoalSign(0);Capture("goal-opening",goalCentre+new Vector3(12,4,-sign*14),goalCentre+Vector3.up);
   ball.ResetBall();Place(ball.Body.position+new Vector3(0,-ball.WorldRadius,-.7f));yield return new WaitForSeconds(.4f);
   Check(view.BeginKick(),"charge starts through actual input path");yield return null;yield return null;
   var aim=view.GetComponentsInChildren<MeshRenderer>(true).FirstOrDefault(r=>r.name=="Kick aim");Check(aim&&aim.enabled,"charge arrow visible");
   if(!aim){Finish();yield break;}
   Check(aim.sharedMaterial.shader.isSupported,"charge shader available in built player");
   var aimMesh=aim.GetComponent<MeshFilter>().sharedMesh;
   Check(aimMesh.uv.Length==aimMesh.vertexCount&&aimMesh.bounds.size.x>1.3f,"rail coordinates and glow padding available");
   float previous=0;Color first=aim.sharedMaterial.GetColor("_BaseColor");bool monotonic=true;
   Time.captureFramerate=30;
   for(int frame=0;frame<48;frame++){
    yield return new WaitForEndOfFrame();float length=aim.transform.localScale.z;monotonic&=length>=previous-.0001f;previous=length;
    var focus=ball.transform.position+Vector3.forward*1.25f;
    Capture("charge-"+frame.ToString("D2"),focus+new Vector3(2.6f,3.4f,-3.6f),focus);
   }
   Check(monotonic&&view.Charge==1&&Mathf.Abs(previous-4.4f)<.001f,"arrow grows monotonically to double length then holds");
   Check(aim.sharedMaterial.GetColor("_BaseColor")!=first&&aim.sharedMaterial.GetFloat("_Charge")==1,"charge changes colour and reaches full-power effect");
   for(int mode=0;mode<3;mode++){
    view.mode=mode;view.yaw=0;view.pitch=32;yield return new WaitForEndOfFrame();
    // Hidden Windows players can return black ScreenCapture images. Render the
    // actual gameplay camera pose explicitly, including its FOV and near plane.
    var gameplay=Camera.main;review.fieldOfView=gameplay.fieldOfView;review.nearClipPlane=gameplay.nearClipPlane;
    Capture("game-camera-"+mode,gameplay.transform.position,gameplay.transform.position+gameplay.transform.forward*10);
    Check(aim.enabled,"charge remains visible in camera mode "+mode);
   }
   int meshId=aimMesh.GetInstanceID();view.CancelKick(int.MinValue);Check(!aim.enabled&&!view.ReadCommand().kick,"cancel immediately hides arrow without shooting");
   Check(view.BeginKick(),"charge can restart");yield return null;yield return null;
   Check(aim.GetComponent<MeshFilter>().sharedMesh.GetInstanceID()==meshId,"repeated charge reuses geometry");
   view.EndKick();Check(!aim.enabled,"release immediately hides arrow");
   yield return new WaitForSeconds(.2f);Check(ball.Body.linearVelocity.magnitude>5,"release still kicks the ball");
   Time.captureFramerate=0;DevelopmentProbe.TurnCommand=default;Finish();
  }
  void Finish(){Time.captureFramerate=0;DevelopmentProbe.TurnCommandActive=false;if(review)Destroy(review.gameObject);File.AppendAllText(Path.Combine(folder,"results.txt"),"FOOTBALL_FEEDBACK_COMPLETE success="+!failed+"\n");}
 }
}
#endif
