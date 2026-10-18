#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace WhatTheFish {
 public sealed class FootballCelebrationReview:MonoBehaviour {
  string folder;bool failed,frames;FootballMatch match;AppRoot app;FootballMVP mvp;int capture;float reach,bone;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-footballCelebrationReview");if(i>=0&&i+1<args.Length)new GameObject("Football honours review").AddComponent<FootballCelebrationReview>().folder=args[i+1];}
  void Check(bool ok,string name){failed|=!ok;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+name+"\n");}
  IEnumerator Until(Func<bool> condition,string label,float seconds=30){float end=Time.realtimeSinceStartup+seconds;while(!condition()&&Time.realtimeSinceStartup<end)yield return null;Check(condition(),label);}
  IEnumerator Fixture(int size,FootballMatchResult result,bool opponentMvp=false,bool departedMvp=false){
   match.ClearMatch();yield return null;Check(FootballBots.Instance.StartSolo(size),"prepare "+size+"v"+size);yield return null;
   Check(match.StartMatch(),"start fixture");match.RequestTeam(FootballTeam.A);Check(match.ConfirmSoloTeam(),"confirm fixture");
   foreach(var a in FootballBots.Roster){var bot=a.GetComponent<FootballBot>();if(bot)bot.enabled=false;}
   yield return Until(()=>match.KickoffReady,"kickoff ready");Check(match.TryKickoff(match.KickoffActor,0,.2f,match.Snapshot.revision),"release kickoff");yield return null;
   var records=match.PlayerRecords.OrderBy(p=>p.number).ToArray();var team=opponentMvp?FootballTeam.B:result==FootballMatchResult.TeamB?FootballTeam.B:FootballTeam.A;
   var winner=records.First(r=>r.team==team);winner.goals=2;winner.assists=1;winner.passes=7;winner.interceptions=3;match.Statistics.Players[winner.id]=winner;
   if(departedMvp){match.Statistics.Players.Remove(winner.id);winner.id=1UL<<62;winner.number=8;winner.name="DEPARTED MVP";match.Statistics.Players[winner.id]=winner;}
   var state=match.Snapshot;state.phase=FootballMatchPhase.Finished;state.deadline=match.Now;state.result=result;state.scoreA=result==FootballMatchResult.TeamB?1:3;state.scoreB=result==FootballMatchResult.TeamA?1:3;state.revision=match.State.Revision+1;match.State.Restore(state,180,60,0);
   yield return Until(()=>FootballMVP.Presenting,"ceremony begins");mvp=FootballMVP.Instance;
   Check(mvp.CelebratedMvp==winner.id,"correct MVP identity");Check(!mvp.ResultsReady,"results withheld during celebration");Check(app.view.InputBlocked,"ceremony blocks local input");
   Check(mvp.CelebrationStage&&!mvp.CelebrationStage.GetComponentsInChildren<Athlete>(true).Any()&&!mvp.CelebrationStage.GetComponentsInChildren<Collider>(true).Any(),"cast has no gameplay or collisions");
   Check(!mvp.GetComponentsInChildren<Image>(true).Any(i=>i.name=="MVP player illustration"),"no 2D player portrait");
  }
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");frames=Environment.GetCommandLineArgs().Contains("-celebrationFrames");Application.targetFrameRate=60;
   yield return Until(()=>AppRoot.Instance&&AppRoot.Instance.Exploring&&FootballMatch.Instance&&AppRoot.Instance.LocalAthlete,"football initialized",90);
   app=AppRoot.Instance;match=FootballMatch.Instance;if(!app||!match){Finish();yield break;}DevelopmentProbe.Driving=false;DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;
   var original=match.rules;match.rules=Instantiate(original);match.rules.teamSelectionSeconds=.2f;match.rules.kickoffSeconds=.1f;
   foreach(var take in Resources.Load<FootballMotionLibrary>("FootballMotionLibrary").takes.Where(t=>t.name.StartsWith("Celebrate_"))){
    var a=take.Sample(.00001f);var b=take.Sample(.99999f);Check(take.loop&&take.duration>=8&&(a.leftHand-b.leftHand).magnitude<.001f&&(a.rightHand-b.rightHand).magnitude<.001f&&(a.pelvis-b.pelvis).magnitude<.001f,take.name+" long seamless loop");
   }
   yield return Fixture(5,FootballMatchResult.TeamA);Check(mvp.CelebrationCastCount==5,"all five winners represented");
   var positions=Athlete.Active.ToDictionary(a=>a,a=>a.transform.position);var scores=match.PlayerRecords.ToArray();float next=0;int still=0;float[] moments={.4f,2.8f,5.5f,10f,14f,16.2f,20f,25.5f,28f,36f,42f};
   bool sawFireworks=false,teamHeld=false,soloHeld=false;var broadcast=FootballStadiumCelebration.Instance;
   var trace=new System.Text.StringBuilder("age,cameraX,cameraY,cameraZ,fov,reach,bone\n");
   while(mvp.CelebrationAge<43){
    yield return new WaitForEndOfFrame();float age=mvp.CelebrationAge;reach=Mathf.Max(reach,mvp.PoseReachError);bone=Mathf.Max(bone,mvp.PoseBoneError);var cam=Camera.main;
    trace.AppendLine(FormattableString.Invariant($"{age:F4},{cam.transform.position.x:F4},{cam.transform.position.y:F4},{cam.transform.position.z:F4},{cam.fieldOfView:F4},{mvp.PoseReachError:F5},{mvp.PoseBoneError:F6}"));
    sawFireworks|=broadcast.FireworksActive;teamHeld|=age>14&&age<15&&!mvp.ResultsReady&&broadcast.Headline.Contains("WIN");soloHeld|=age>24&&age<25&&!mvp.ResultsReady&&broadcast.Headline.Contains("VALUABLE");
    if(frames&&age>=next){next=age+.10f;yield return Capture("frames/frame-"+(capture++).ToString("0000"));}
    if(still<moments.Length&&age>=moments[still]){yield return Capture("shot-"+still.ToString("00"));still++;}
   }
   File.WriteAllText(Path.Combine(folder,"motion.csv"),trace.ToString());Check(mvp.ResultsReady,"stats become interactive after camera settles");
   Check(teamHeld&&soloHeld,"15 second team and 10 second MVP stages");Check(sawFireworks&&!broadcast.FireworksActive,"fireworks appear and stop after 15 seconds");Check(broadcast.ScreenCount==2&&broadcast.FeedFrames>30,"twin stadium screens receive live MVP footage");
   if(broadcast.Footage){var previous=RenderTexture.active;RenderTexture.active=broadcast.Footage;var tex=new Texture2D(broadcast.Footage.width,broadcast.Footage.height,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,tex.width,tex.height),0,0);tex.Apply();File.WriteAllBytes(Path.Combine(folder,"screen-live-footage.png"),tex.EncodeToPNG());Destroy(tex);RenderTexture.active=previous;}
   Check(bone<.001f,"bone lengths preserved "+bone.ToString("F6"));Check(reach<.025f,"celebration reach bounded "+reach.ToString("F4"));
   Check(positions.All(p=>!p.Key||Vector3.Distance(p.Key.transform.position,p.Value)<.15f),"ceremony leaves live actors in place");Check(scores.SequenceEqual(match.PlayerRecords),"statistics preserved");
   var toggle=mvp.GetComponentsInChildren<Button>().First(b=>b.name=="Toggle match stats");toggle.onClick.Invoke();yield return Capture("match-stats");toggle.onClick.Invoke();
   foreach(var resolution in new[]{new Vector2Int(1280,720),new Vector2Int(1560,720),new Vector2Int(1600,1200)}){
    Screen.SetResolution(resolution.x,resolution.y,FullScreenMode.Windowed);yield return new WaitForSeconds(.5f);yield return Capture("results-"+resolution.x+"x"+resolution.y);
    var star=mvp.CelebrationStage.Find("Honours #"+match.MVP.number+" "+match.MVP.name);var point=Camera.main.WorldToViewportPoint(star.position+Vector3.up*.9f);Check(point.x>.1f&&point.x<.40f&&point.y>.3f&&point.y<.7f,"MVP inside left column "+resolution+" "+point);
   }
   mvp.Dismiss();yield return null;Check(!FootballMVP.Presenting&&PostMatchFlow.Instance.Visible,"dismiss hands off to next adventure");PostMatchFlow.Instance.Close();Check(!app.view.InputBlocked,"closing next adventure restores input");Check(!Athlete.Active.SelectMany(a=>a.visual.GetComponentsInChildren<Renderer>()).Any(r=>r.forceRenderingOff),"dismiss restores renderers");yield return new WaitForSeconds(.2f);Check(!FootballMVP.Presenting,"dismissed revision stays dismissed");
   Screen.SetResolution(1600,900,FullScreenMode.Windowed);
   yield return Fixture(2,FootballMatchResult.TeamB);yield return new WaitForSeconds(2);mvp.SkipCelebration();yield return new WaitForSeconds(1.5f);Check(mvp.ResultsReady,"skip eases into results");yield return Capture("red-skipped");mvp.Dismiss();yield return null;PostMatchFlow.Instance.Close();Check(broadcast.BroadcastActive&&broadcast.FireworksActive,"local skip and dismissal preserve stadium broadcast");
   yield return Fixture(5,FootballMatchResult.Draw);Check(mvp.CelebrationCastCount==10,"draw celebrates both teams");yield return new WaitForSeconds(3);yield return Capture("draw-team");match.ClearMatch();yield return null;yield return null;Check(!mvp.Visible&&!app.view.InputBlocked,"new round cancels ceremony cleanly");
   yield return Fixture(2,FootballMatchResult.TeamA,true);Check(mvp.CelebrationCastCount==3,"winning team plus opposing MVP");yield return new WaitForSeconds(19);yield return Capture("opposing-mvp");Check(mvp.CelebrationStage.Cast<Transform>().Count(t=>t.gameObject.activeSelf)==1,"only MVP remains in solo reveal");
   mvp.gameObject.SetActive(false);yield return null;PostMatchFlow.Instance.Close();Check(!FootballMVP.Presenting&&!app.view.InputBlocked,"disable restores presentation ownership");mvp.gameObject.SetActive(true);
   yield return Fixture(2,FootballMatchResult.TeamA,false,true);yield return new WaitForSeconds(2);mvp.SkipCelebration();yield return new WaitForSeconds(1.5f);yield return Capture("departed-mvp");
   var retained=mvp.CelebrationStage.GetComponentsInChildren<SkinnedMeshRenderer>().First();Check(retained.sharedMaterial.GetFloat("_IdentityNumber")==8&&mvp.CelebratedMvp==(1UL<<62),"departed MVP retains recorded visual identity");
   match.ClearMatch();yield return null;yield return null;match.rules=original;DevelopmentProbe.TurnCommandActive=false;Finish();
  }
  IEnumerator Capture(string name){
   yield return new WaitForEndOfFrame();string path=Path.Combine(folder,name+".png");Directory.CreateDirectory(Path.GetDirectoryName(path));var camera=Camera.main;
   var canvases=FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.isRootCanvas&&c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();
   foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=camera;c.planeDistance=1;}Canvas.ForceUpdateCanvases();
   var rt=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var active=RenderTexture.active;var target=camera.targetTexture;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
   var texture=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());Destroy(texture);camera.targetTexture=target;RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);
   foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceOverlay;c.worldCamera=null;}Canvas.ForceUpdateCanvases();
  }
  void Finish(){File.AppendAllText(Path.Combine(folder,"results.txt"),"FOOTBALL_CELEBRATION_COMPLETE success="+!failed+"\n");Application.Quit(failed?1:0);}
 }
}
#endif
