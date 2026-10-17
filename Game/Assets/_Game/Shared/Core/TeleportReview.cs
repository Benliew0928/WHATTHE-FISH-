#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
namespace WhatTheFish {
 public sealed class TeleportReview:MonoBehaviour {
  string folder;bool failed;int checks;AppRoot app;TeleportSequence service;TeleportPreview preview;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-teleportReview");if(at>=0&&at+1<args.Length)new GameObject("Teleport review").AddComponent<TeleportReview>().folder=args[at+1];}
  void Check(bool good,string text){checks++;failed|=!good;File.AppendAllText(Path.Combine(folder,"results.txt"),(good?"PASS ":"FAIL ")+text+"\n");}
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");float until=Time.realtimeSinceStartup+100;
   while((!AppRoot.Instance||!AppRoot.Instance.Exploring||!TeleportPreview.Instance||!TeleportSequence.Instance)&&Time.realtimeSinceStartup<until)yield return null;
   app=AppRoot.Instance;preview=TeleportPreview.Instance;service=TeleportSequence.Instance;Check(app&&preview&&service,"preview bootstraps from complete player");if(!app||!preview||!service){Finish();yield break;}
   DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;yield return new WaitForSeconds(1);app.view.mode=1;
   if(Environment.GetCommandLineArgs().Contains("-teleportFilm")){yield return Film();Finish();yield break;}
   foreach(var island in SkySailMap.Circuit)foreach(TeleportDestination destination in Enum.GetValues(typeof(TeleportDestination))){
    yield return Idle();bool started=preview.Play(island,destination,true);Check(started,island+" "+destination+" starts: "+service.Error);if(!started)continue;
    bool charged=false,absorbed=false,flight=false,arrived=false,hitch=false;float chargeStart=0;var actors=Athlete.Active.Where(a=>a&&a.isActiveAndEnabled).ToArray();until=Time.realtimeSinceStartup+90;
    while(TeleportSequence.Busy&&Time.realtimeSinceStartup<until){
     if(service.Phase==TeleportPhase.Charging&&!charged){charged=true;chargeStart=Time.unscaledTime;Check(actors.All(a=>a.inTransit&&!a.capsule.enabled),"group movement locked during charge");}
     if(service.Phase==TeleportPhase.Charging&&service.PhaseProgress>.75f&&!File.Exists(Path.Combine(folder,island+"-"+destination+"-charge.png")))yield return Capture(island+"-"+destination+"-charge");
     if(service.Phase==TeleportPhase.Absorbing&&!absorbed&&service.PhaseProgress>.45f){absorbed=true;Check(Time.unscaledTime-chargeStart>=1,"one second charge precedes absorption");yield return Capture(island+"-"+destination+"-absorb");}
     if(island==SportId.Golf&&destination==TeleportDestination.Station&&service.Phase==TeleportPhase.Travelling&&service.PhaseProgress>.25f&&!hitch){hitch=true;System.Threading.Thread.Sleep(650);File.AppendAllText(Path.Combine(folder,"results.txt"),"INFO injected 650 ms render hitch during cross-island flight\n");}
     if(service.Phase==TeleportPhase.Travelling&&!flight&&service.PhaseProgress>.45f){flight=true;Check(actors.All(a=>a.visual.GetComponentsInChildren<Renderer>().All(r=>r.forceRenderingOff)),"every character disappears during travel");Check(Camera.main.clearFlags==CameraClearFlags.Skybox,"world stays visible without blackout");yield return Capture(island+"-"+destination+"-travel");}
     if(service.Phase==TeleportPhase.Arriving&&!arrived&&service.PhaseProgress>.45f){arrived=true;yield return Capture(island+"-"+destination+"-arrival");}
     yield return null;
    }
    Check(!TeleportSequence.Busy&&service.Error.Length==0,island+" "+destination+" completes: "+service.Error);
    Check(charged&&absorbed&&flight&&arrived,"all four visible phases occur");Check(app.SelectedSport==island,"destination island selected");
    Check(service.Landings.Length==TeleportDestinations.Capacity(island),"full capacity has distinct assigned slots");
    for(int i=0;i<service.Landings.Length;i++){
     var landing=service.Landings[i];Check(Physics.Raycast(landing.position+Vector3.up*.15f,Vector3.down,.3f,1<<8),island+" slot "+i+" supported by ground");
     for(int j=0;j<i;j++)Check(Vector3.Distance(landing.position,service.Landings[j].position)>1.3f,island+" slots "+j+" and "+i+" separated");
    }
    Check(actors.All(a=>!a.inTransit&&a.visual.GetComponentsInChildren<Renderer>(true).All(r=>!r.forceRenderingOff)),"every identity reappears");
    Check(actors.All(a=>a.visual.GetComponentsInChildren<Renderer>(true).All(r=>r.sharedMaterials.All(m=>!m||m.shader.name!="WhatTheFish/TeleportCharacter"))),"original materials restored");
    Check(app.LocalAthlete.capsule.enabled&&!app.view.InputBlocked,"local controls restored");Check(service.LargestCameraStep<60,"camera has no destination jump: "+service.LargestCameraStep.ToString("F2"));
    yield return Idle();Check(app.environments.roots.Count(r=>r&&r.activeSelf)==1,"only destination detail remains after arrival");yield return Capture(island+"-"+destination+"-landed");
   }
   yield return Idle();var group=preview.Group(6);Check(!service.StartPreview(SportId.Fishing,TeleportDestination.Game,group),"six-player fishing request rejected atomically");
   yield return null;
   // Cancel before and after rebasing; both must restore source positions and presentation.
   foreach(var phase in new[]{TeleportPhase.Preparing,TeleportPhase.Absorbing,TeleportPhase.Arriving}){
    yield return Idle();var source=app.SelectedSport;Vector3 position=app.LocalAthlete.transform.position;group=preview.Group(1);yield return null;
    bool start=service.StartPreview(SportId.Football,TeleportDestination.Station,group);Check(start,"cancel scenario starts "+phase);until=Time.realtimeSinceStartup+60;
    while(TeleportSequence.Busy&&service.Phase!=phase&&Time.realtimeSinceStartup<until)yield return null;
    service.Cancel();yield return Idle();Check(!TeleportSequence.Busy&&app.SelectedSport==source,"cancel restores island "+phase);Check(Vector3.Distance(position,app.LocalAthlete.transform.position)<.08f,"cancel restores position "+phase);Check(app.LocalAthlete.capsule.enabled&&!app.LocalAthlete.inTransit&&!app.view.InputBlocked,"cancel restores controls "+phase);
   }
   yield return Idle();group=preview.Group(1);var blocked=TeleportDestinations.Nominal(app.SelectedSport,TeleportDestination.Game,0).position;var obstacle=GameObject.CreatePrimitive(PrimitiveType.Cube);obstacle.name="Teleport test obstruction";obstacle.transform.position=blocked+Vector3.up;obstacle.transform.localScale=new Vector3(1.4f,2,1.4f);Physics.SyncTransforms();
   Check(!TeleportDestinations.Resolve(app.SelectedSport,TeleportDestination.Game,group,Vector3.zero,out _,out _),"occupied landing rejects plan");Destroy(obstacle);
   Check(Vector3.Distance(TeleportSequence.CameraPath(Vector3.zero,Vector3.right*300,100,0),Vector3.zero)<.001f&&Vector3.Distance(TeleportSequence.CameraPath(Vector3.zero,Vector3.right*300,100,1),Vector3.right*300)<.001f,"camera route has exact endpoints");
   foreach(int mode in new[]{0,2}){yield return Idle();app.view.mode=mode;yield return null;Check(preview.Play(SportId.Football,TeleportDestination.Game,false),"camera mode starts "+mode);yield return Idle();Check(service.Error.Length==0&&app.view.mode==mode&&!app.view.InputBlocked,"camera mode restored "+mode);}
   Finish();
  }
  IEnumerator Film(){
   yield return Idle();preview.Play(SportId.Football,TeleportDestination.Game,true);yield return Idle();
   foreach(var target in new[]{SportId.Football,SportId.Golf}){
    string frames=Path.Combine(folder,"film-"+target);Directory.CreateDirectory(frames);Time.captureFramerate=24;
    bool started=preview.Play(target,TeleportDestination.Station,true);Check(started,"film starts "+target);int frame=0;
    while(TeleportSequence.Busy){if(service.Phase!=TeleportPhase.Preparing)yield return Capture("film-"+target+"/frame-"+(frame++).ToString("D5"));else yield return null;}
    for(int i=0;i<24;i++)yield return Capture("film-"+target+"/frame-"+(frame++).ToString("D5"));Time.captureFramerate=0;yield return Idle();Check(service.Error.Length==0,"film completes "+target);
   }
  }
  IEnumerator Idle(){float until=Time.realtimeSinceStartup+100;var stream=app.environments.GetComponent<SkySailStreaming>();while((TeleportSequence.Busy||stream.Busy)&&Time.realtimeSinceStartup<until)yield return null;yield return new WaitForSeconds(.25f);}
  IEnumerator Capture(string name){yield return new WaitForEndOfFrame();var camera=Camera.main;var rt=RenderTexture.GetTemporary(1280,720,24);var prior=RenderTexture.active;var target=camera.targetTexture;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var pixels=new Texture2D(1280,720,TextureFormat.RGB24,false);pixels.ReadPixels(new Rect(0,0,1280,720),0,0);pixels.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),pixels.EncodeToPNG());Destroy(pixels);camera.targetTexture=target;RenderTexture.active=prior;RenderTexture.ReleaseTemporary(rt);}
  void Finish(){File.AppendAllText(Path.Combine(folder,"results.txt"),"TELEPORT_REVIEW_COMPLETE success="+!failed+" checks="+checks+"\n");Application.Quit(failed?1:0);}
 }
}
#endif
