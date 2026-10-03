#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

namespace WhatTheFish {
 // Opt-in coverage of real contacts, arbitrary locations and rendered deformation.
 public sealed class FootballNetReview:MonoBehaviour {
  string folder;bool failed;int count;Camera review;FootballBall ball;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){
   var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-footballNetReview");if(i<0||i+1>=args.Length)return;
   new GameObject("Football net review").AddComponent<FootballNetReview>().folder=args[i+1];
  }
  void Check(bool ok,string name){failed|=!ok;count++;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+name+"\n");}
  void Capture(string name,FootballNetSurface net){
   review.transform.position=net.transform.TransformPoint(new Vector3(-5,3,-6));review.transform.LookAt(net.transform.TransformPoint(new Vector3(0,1,1.3f)));
   var rt=RenderTexture.GetTemporary(1280,720,24);var old=RenderTexture.active;review.targetTexture=rt;review.Render();RenderTexture.active=rt;
   var texture=new Texture2D(1280,720,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,1280,720),0,0);texture.Apply();
   File.WriteAllBytes(Path.Combine(folder,name+".png"),texture.EncodeToPNG());Destroy(texture);review.targetTexture=null;RenderTexture.active=old;RenderTexture.ReleaseTemporary(rt);
  }
  void Shot(FootballNetSurface net,Vector3 contact,Vector3 direction){
   net.Clear();ball.ResetBall();ball.Body.position=net.transform.TransformPoint(contact-direction.normalized*.85f);
   ball.Body.linearVelocity=net.transform.TransformDirection(direction);ball.Body.WakeUp();Physics.SyncTransforms();
  }
  void Locations(FootballNetSurface net){
   var d=net.Dimensions;float back=d.y-d.w;int sample=0;
   for(int panel=0;panel<4;panel++)for(int x=1;x<=7;x++)for(int y=1;y<=4;y++){
    float u=x/8f,v=y/5f;Vector3 p,normal;
    if(panel==0){p=new Vector3(Mathf.Lerp(-d.x,d.x,u),back*v,d.z);normal=Vector3.back;}
    else if(panel<3){p=new Vector3(panel==1?-d.x:d.x,(d.y-d.w*u)*v,d.z*u);normal=panel==1?Vector3.right:Vector3.left;}
    else {p=new Vector3(Mathf.Lerp(-d.x,d.x,u),d.y-d.w*v,d.z*v);normal=new Vector3(0,-1,-d.w/d.z).normalized;}
    net.Clear();uint before=net.ImpactCount;var direction=-normal*18+Vector3.right*.8f;
    net.Contact(net.transform.TransformPoint(p),net.transform.TransformDirection(direction),net.transform.TransformDirection(normal));
    var move=net.Displacement(p,Time.time+.1f);
    Check(net.ImpactCount==before+1&&Vector3.Distance(net.LastContact,p)<.001f,"position "+net.Sign+" / "+sample);
    Check(move.magnitude>.01f&&Vector3.Dot(move,direction)>0&&move.magnitude<=FootballNetSurface.MaximumDeflection+.001f,"direction and bounded stretch "+net.Sign+" / "+sample++);
   }
   var centre=new Vector3(0,back*.5f,d.z);net.Clear();net.Receive(centre,Vector3.forward,.9f,Time.time);
   Check(net.Displacement(centre,Time.time+.1f).magnitude>net.Displacement(centre+Vector3.right*2,Time.time+.1f).magnitude*20,"localized response "+net.Sign);
   Check(net.Displacement(centre,Time.time+3)==Vector3.zero,"settles completely "+net.Sign);
   Check(net.Displacement(new Vector3(d.x,back*.5f,d.z),Time.time+.1f)==Vector3.zero&&FootballNetSurface.PinWeight(new Vector3(0,0,d.z),d)==0&&FootballNetSurface.PinWeight(new Vector3(d.x,d.y,0),d)==0,"frame and ground remain attached "+net.Sign);
   var seam=new Vector3(0,back,d.z);net.Clear();net.Receive(seam,Vector3.forward,.8f,Time.time);
   Check(Vector3.Distance(net.Displacement(seam+Vector3.down*.001f,Time.time+.1f),net.Displacement(seam-Vector3.forward*.001f,Time.time+.1f))<.002f,"roof-back seam continuous "+net.Sign);
   uint valid=net.ImpactCount;net.Receive(Vector3.positiveInfinity,Vector3.forward,1,Time.time);net.Receive(centre,Vector3.one*9,1,Time.time);net.Receive(centre,Vector3.forward,float.NaN,Time.time);net.Receive(centre+Vector3.up*30,Vector3.forward,1,Time.time);
   Check(net.ImpactCount==valid,"invalid events rejected "+net.Sign);
   net.Clear();net.Contact(net.transform.TransformPoint(centre),net.transform.forward*.1f,-net.transform.forward);net.Contact(net.transform.TransformPoint(centre),-net.transform.forward*22,-net.transform.forward);
   Check(net.ImpactCount==valid,"resting and separating ball ignored "+net.Sign);
   net.Contact(net.transform.TransformPoint(centre),net.transform.forward*2,-net.transform.forward);float gentle=net.LastImpulse.magnitude;uint once=net.ImpactCount;
   net.Contact(net.transform.TransformPoint(centre),net.transform.forward*2,-net.transform.forward);Check(net.ImpactCount==once,"same contact deduplicated "+net.Sign);
   net.Clear();net.Contact(net.transform.TransformPoint(centre),net.transform.forward*22,-net.transform.forward);Check(net.LastImpulse.magnitude>gentle*5,"hard shots stretch more than gentle shots "+net.Sign);
   for(int i=0;i<8;i++)net.Receive(centre+Vector3.right*(i%2)*.2f,Vector3.forward*1.4f,1.1f,Time.time);
   bool bounded=net.SurfaceMesh.vertices.All(p=>net.Displacement(p,Time.time+.1f).magnitude<=FootballNetSurface.MaximumDeflection+.001f);
   Check(bounded,"overlapping shots bounded "+net.Sign);net.Clear();
  }
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"Position-aware goal net review\n");
   float deadline=Time.realtimeSinceStartup+60;
   while((!AppRoot.Instance||!AppRoot.Instance.LocalAthlete||!FootballBall.Instance)&&Time.realtimeSinceStartup<deadline)yield return null;
   if(!FootballBall.Instance){Check(false,"football loaded");Finish();yield break;}
   AppRoot.Instance.EnterOffline();DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;yield return new WaitForSeconds(.6f);
   ball=FootballBall.Instance;ball.gameObject.AddComponent<FootballNetCollisionTrace>().folder=folder;var nets=ball.Pitch.GetComponentsInChildren<FootballNetSurface>();Check(nets.Length==2,"both goals have reactive net");
   var actor=AppRoot.Instance.LocalAthlete;actor.capsule.enabled=false;actor.transform.position=ball.KickoffPosition+Vector3.right*8;actor.ResetLocomotion();actor.capsule.enabled=true;
   review=new GameObject("Net capture camera").AddComponent<Camera>();review.CopyFrom(Camera.main);review.enabled=false;review.fieldOfView=48;
   bool gravity=ball.Body.useGravity;float damping=ball.Body.linearDamping;ball.Body.useGravity=false;ball.Body.linearDamping=0;
   foreach(var net in nets){
    Check(net.GetComponent<Renderer>().sharedMaterial.shader.isSupported,"shader supported "+net.Sign);
    Check(net.SurfaceMesh.vertexCount<5000,"bounded procedural mesh "+net.Sign);Locations(net);
    var d=net.Dimensions;float back=d.y-d.w;
    var points=new[]{new Vector3(0,1,d.z),new Vector3(-d.x+.5f,.35f,d.z),new Vector3(d.x-.5f,back-.4f,d.z),new Vector3(-d.x,1,1.1f),new Vector3(d.x,.7f,1.4f),new Vector3(.8f,d.y-d.w*.5f,d.z*.5f)};
    var directions=new[]{new Vector3(0,0,22),new Vector3(-2,0,12),new Vector3(2,0,32),new Vector3(-18,0,3),new Vector3(18,0,2),new Vector3(1,15,3)};
    for(int shot=0;shot<points.Length;shot++){
     Shot(net,points[shot],directions[shot]);uint before=net.ImpactCount;float until=Time.time+.4f;
     while(net.ImpactCount==before&&Time.time<until)yield return new WaitForFixedUpdate();
     File.AppendAllText(Path.Combine(folder,"contacts.txt"),$"SHOT sign={net.Sign} target={points[shot]:F3} last={net.LastContact:F3} impulse={net.LastImpulse:F3} ball={net.transform.InverseTransformPoint(ball.Body.position):F3} speed={ball.Body.linearVelocity:F3} hits={net.ImpactCount-before}\n");
     Check(net.ImpactCount>before,"real ball collision "+net.Sign+" / "+shot);
     Check(Vector3.Distance(net.LastContact,points[shot])<.16f,"real contact location "+net.Sign+" / "+shot);
     Check(Vector3.Dot(net.LastImpulse.normalized,directions[shot].normalized)>.97f,"real incoming direction "+net.Sign+" / "+shot);
     if(net.Sign>0){
      // Replay this physical shot with a deterministic frame step for visual review.
      Time.captureFramerate=30;ball.Body.useGravity=true;Shot(net,points[shot],directions[shot]);
      for(int frame=0;frame<36;frame++){yield return new WaitForEndOfFrame();Capture("shot-"+shot+"-"+frame.ToString("D2"),net);}
      Time.captureFramerate=0;ball.Body.useGravity=false;
     }
    }
    net.Clear();uint hits=net.ImpactCount;ball.ResetBall();ball.Body.position=net.transform.TransformPoint(new Vector3(0,.23f,-3));ball.Body.linearVelocity=Vector3.zero;yield return new WaitForSeconds(.15f);
    Check(net.ImpactCount==hits,"ball away from net does not animate "+net.Sign);
    var original=net.transform.parent.localPosition;net.transform.parent.localPosition+=Vector3.right*3;net.Contact(net.transform.TransformPoint(new Vector3(0,1,d.z)),net.transform.forward*22,-net.transform.forward);
    Check(Vector3.Distance(net.LastContact,new Vector3(0,1,d.z))<.001f,"goal movement preserves contact coordinates "+net.Sign);net.transform.parent.localPosition=original;Physics.SyncTransforms();
    net.gameObject.SetActive(false);net.gameObject.SetActive(true);Check(net.Displacement(new Vector3(0,1,d.z),Time.time+.1f)==Vector3.zero,"reactivation clears old ripple "+net.Sign);
   }
   ball.Body.useGravity=gravity;ball.Body.linearDamping=damping;ball.ResetBall();Finish();
  }
  void Finish(){Time.captureFramerate=0;DevelopmentProbe.TurnCommandActive=false;if(review)Destroy(review.gameObject);File.AppendAllText(Path.Combine(folder,"results.txt"),"FOOTBALL_NET_COMPLETE success="+!failed+" checks="+count+"\n");}
 }
 public sealed class FootballNetCollisionTrace:MonoBehaviour {
  public string folder;
  void OnCollisionEnter(Collision collision){
   var surface=collision.collider.GetComponentInParent<FootballGoalNet>();if(!surface)return;
   var c=collision.GetContact(0);var net=surface.Surface;
   File.AppendAllText(Path.Combine(folder,"contacts.txt"),$"ENTER {collision.collider.name} point={net.transform.InverseTransformPoint(c.point):F3} normal={net.transform.InverseTransformDirection(c.normal):F3} rel={net.transform.InverseTransformDirection(collision.relativeVelocity):F3} impulse={collision.impulse:F3}\n");
  }
 }
}
#endif
