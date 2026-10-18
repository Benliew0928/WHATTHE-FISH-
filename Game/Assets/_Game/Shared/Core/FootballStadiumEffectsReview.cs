#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
namespace WhatTheFish {
 public sealed class FootballStadiumEffectsReview:MonoBehaviour {
  string folder;bool failed;FootballStadiumCelebration show;FootballMatch match;Camera camera;int frame;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,"-stadiumEffectsReview");if(i>=0&&i+1<a.Length)new GameObject("Daylight stadium effects review").AddComponent<FootballStadiumEffectsReview>().folder=a[i+1];}
  void Check(bool ok,string name){failed|=!ok;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+name+"\n");}
  IEnumerator Until(Func<bool> condition,string label){float end=Time.realtimeSinceStartup+90;while(!condition()&&Time.realtimeSinceStartup<end)yield return null;Check(condition(),label);}
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");Application.targetFrameRate=60;
   yield return Until(()=>DevelopmentProbe.ReadyToReviewSport&&FootballStadiumCelebration.Instance&&FootballStadiumCelebration.Instance.EffectsRoot,"football and permanent fixtures ready");
   match=FootballMatch.Instance;show=FootballStadiumCelebration.Instance;if(!match||!show){Finish();yield break;}
   DevelopmentProbe.Driving=false;DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;
   camera=new GameObject("Effects review camera").AddComponent<Camera>();camera.CopyFrom(Camera.main);camera.enabled=false;
   Check(!show.FireworksActive&&show.FlameJets==0&&show.LaserBeams==0&&show.ElectricArcs==0,"no celebration emissions in normal play");
   Check(show.EffectsRoot.childCount==1&&show.EffectsRoot.GetComponentsInChildren<MeshRenderer>().Length==2,"one effects mesh and one combined hardware mesh");
   Check(!show.EffectsRoot.GetComponentsInChildren<Collider>().Any(),"decoration adds no pitch collisions");
   Check(Enumerable.Range(0,8).All(i=>Mathf.Abs(FootballStadiumEffectsRig.FlameOrigin(i).x)>FootballBall.Instance.PitchBounds.extents.x+2),"all flame mounts outside playable pitch");
   View(0);Capture("normal-stadium");
   var mesh=show.EffectsRoot.GetComponent<MeshFilter>().sharedMesh;
   show.ReviewEffects(6.25f,123,camera);var first=mesh.vertices;show.ReviewEffects(10,456,camera);show.ReviewEffects(6.25f,123,camera);
   Check(first.SequenceEqual(mesh.vertices),"same shared clock and revision recreate identical geometry after catch-up");
   Check(mesh.vertexCount<24000,"bounded effects geometry "+mesh.vertexCount+" vertices");
   show.ReviewEffects(-1,0,camera);
   match.rules=Instantiate(match.rules);match.rules.teamSelectionSeconds=.2f;match.rules.kickoffSeconds=.1f;
   Check(FootballBots.Instance.StartSolo(5),"prepare winning squad");yield return null;Check(match.StartMatch(),"start match");match.RequestTeam(FootballTeam.A);Check(match.ConfirmSoloTeam(),"confirm team");
   foreach(var a in FootballBots.Roster){var bot=a.GetComponent<FootballBot>();if(bot)bot.enabled=false;}
   yield return Until(()=>match.KickoffReady,"kickoff ready");Check(match.TryKickoff(match.KickoffActor,0,.2f,match.Snapshot.revision),"kickoff");yield return null;
   var star=match.PlayerRecords.First(r=>r.team==FootballTeam.A);star.goals=2;star.assists=1;match.Statistics.Players[star.id]=star;
   var state=match.Snapshot;state.phase=FootballMatchPhase.Finished;state.deadline=match.Now;state.result=FootballMatchResult.TeamA;state.scoreA=3;state.scoreB=1;state.revision=match.State.Revision+1;match.State.Restore(state,180,60,0);
   yield return Until(()=>show.BroadcastActive&&FootballMVP.Presenting,"room celebration started");
   bool continuous=true,bounded=true;int samples=0,still=0;float next=0;float[] moments={1.5f,3,5.4f,7.5f,10.5f,12.5f,14.3f,15.5f};
   bool frames=Environment.GetCommandLineArgs().Contains("-effectsFrames");File.WriteAllText(Path.Combine(folder,"frame-times.csv"),"frame,age\n");
   while(show.Age<16){
    yield return new WaitForEndOfFrame();float age=show.Age;
    if(age>.7f&&age<14.9f){samples++;continuous&=show.FlameJets==8&&show.LaserBeams==12&&show.ElectricArcs==6&&show.FireworksActive;bounded&=mesh.vertexCount<24000;}
    View(age);show.ReviewEffects(age,state.revision,camera);
    if(frames&&age>=next){next=age+.05f;string name="frames/frame-"+frame.ToString("0000");Capture(name);File.AppendAllText(Path.Combine(folder,"frame-times.csv"),FormattableString.Invariant($"{frame},{age:F4}\n"));frame++;}
    if(still<moments.Length&&age>=moments[still]){Capture("daylight-"+still.ToString("00"));still++;}
    show.ReviewEffects(age,state.revision,Camera.main);
   }
   Check(continuous&&samples>60,"eight sustained flames, twelve lasers and six electric spans throughout team show; samples="+samples);
   Check(bounded,"all sampled frames stay within geometry budget");Check(!show.FireworksActive&&show.FlameJets==0&&show.LaserBeams==0&&show.ElectricArcs==0,"all emissions stop before solo MVP");
   Check(show.Headline=="MOST VALUABLE PLAYER"&&show.Footage,"MVP footage and data continue after effects");
   show.enabled=false;yield return null;Check(!show.EffectsRoot.gameObject.activeInHierarchy,"disable hides hardware and effects");show.enabled=true;yield return new WaitForEndOfFrame();Check(show.EffectsRoot.gameObject.activeInHierarchy,"re-enable restores permanent decoration");
   match.ClearMatch();yield return new WaitForEndOfFrame();Check(show.Mode==FootballStadiumFeedMode.Live&&!show.FireworksActive,"new match restores live screen without emissions");
   Destroy(camera.gameObject);Finish();
  }
  void View(float age){
   var pitch=FootballBall.Instance.Pitch;Vector3 position,focus;
   if(age<5||age>=13){position=new(8,24,47);focus=new(0,18,-27);camera.fieldOfView=62;}
   else if(age<9){position=new(25,6,26);focus=new(39,10,-12);camera.fieldOfView=66;}
   else {position=new(16,16,-40);focus=new(0,29,-73.4f);camera.fieldOfView=70;}
   camera.transform.SetPositionAndRotation(pitch.TransformPoint(position),Quaternion.LookRotation(pitch.TransformDirection(focus-position),pitch.up));
  }
  void Capture(string name){
   string path=Path.Combine(folder,name+".png");Directory.CreateDirectory(Path.GetDirectoryName(path));var rt=RenderTexture.GetTemporary(1600,900,24);var previous=RenderTexture.active;var target=camera.targetTexture;
   camera.targetTexture=rt;camera.aspect=1600f/900;camera.Render();RenderTexture.active=rt;var tex=new Texture2D(1600,900,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1600,900),0,0);tex.Apply();File.WriteAllBytes(path,tex.EncodeToPNG());Destroy(tex);camera.targetTexture=target;RenderTexture.active=previous;RenderTexture.ReleaseTemporary(rt);
  }
  void Finish(){File.AppendAllText(Path.Combine(folder,"results.txt"),"STADIUM_EFFECTS_COMPLETE success="+!failed+"\n");Application.Quit(failed?1:0);}
 }
}
#endif
