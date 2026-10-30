#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace WhatTheFish {
 // Development-only equivalence checks and allocation measurements. Never
 // enters a non-development Android submission or changes game rules.
 public sealed class OptimizationReview:MonoBehaviour {
  string folder;bool failed;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-optimizationReview");if(i>=0&&i+1<args.Length)new GameObject("Optimization review").AddComponent<OptimizationReview>().folder=args[i+1];}
  void Check(bool pass,string name){failed|=!pass;File.AppendAllText(Path.Combine(folder,"results.txt"),(pass?"PASS ":"FAIL ")+name+"\n");}
  static long HeapBytes()=>UnityEngine.Profiling.Profiler.GetMonoUsedSizeLong();
  void Measure(string name,Action action,int count){
   action();var clock=new System.Diagnostics.Stopwatch();var mode=UnityEngine.Scripting.GarbageCollector.GCMode;
   long bytes;try{UnityEngine.Scripting.GarbageCollector.GCMode=UnityEngine.Scripting.GarbageCollector.Mode.Disabled;long start=HeapBytes();clock.Start();for(int i=0;i<count;i++)action();clock.Stop();bytes=HeapBytes()-start;}finally{UnityEngine.Scripting.GarbageCollector.GCMode=mode;}
   File.AppendAllText(Path.Combine(folder,"measurements.csv"),FormattableString.Invariant($"{name},{count},{bytes},{clock.Elapsed.TotalMilliseconds:F3}\n"));
  }
  void NoAllocation(string name,Action action){
   action();var mode=UnityEngine.Scripting.GarbageCollector.GCMode;long bytes;
   try{UnityEngine.Scripting.GarbageCollector.GCMode=UnityEngine.Scripting.GarbageCollector.Mode.Disabled;long start=HeapBytes();for(int i=0;i<1000;i++)action();bytes=HeapBytes()-start;}finally{UnityEngine.Scripting.GarbageCollector.GCMode=mode;}
   Check(bytes==0,name+" allocates "+bytes+" bytes in 1000 calls");
  }
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");File.WriteAllText(Path.Combine(folder,"measurements.csv"),"operation,calls,managedBytes,milliseconds\n");
   DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;
   double deadline=Time.realtimeSinceStartupAsDouble+120;
   while(!DevelopmentProbe.ReadyToReviewSport&&Time.realtimeSinceStartupAsDouble<deadline)yield return null;
   Check(DevelopmentProbe.ReadyToReviewSport,"offline football ready");if(failed){Finish();yield break;}
   long heapBefore=HeapBytes();var calibration=new byte[65536];long heapAfter=HeapBytes();GC.KeepAlive(calibration);Check(heapAfter-heapBefore>=65536,"managed allocation counter detects a known 64 KiB allocation");
   var selection=new FootballTeamSelection();
   for(int size=2;size<=10;size+=2){
    Check(selection.Begin(System.Linq.Enumerable.Range(1,size).Select(i=>(ulong)i)),"team roster begins with "+size+" players");
    Check(selection.Count(FootballTeam.None)==size&&selection.Count(FootballTeam.A)==0&&selection.Count(FootballTeam.B)==0,"unselected team counts "+size);
    Check(selection.Choose(1,FootballTeam.A)&&selection.Choose(2,FootballTeam.B),"team choices accepted "+size);selection.Complete(new System.Random(17));
    Check(selection.Count(FootballTeam.A)==size/2&&selection.Count(FootballTeam.B)==size/2&&selection.Count(FootballTeam.None)==0,"balanced locked team counts "+size);
   }
   NoAllocation("shared team counts",()=>selection.Count(FootballTeam.A));
   var choices=(Dictionary<ulong,FootballTeam>)typeof(FootballTeamSelection).GetField("choices",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(selection);
   Measure("team_count_previous",()=>ReferenceTeamCount(choices,FootballTeam.A),10000);Measure("team_count",()=>selection.Count(FootballTeam.A),10000);
   selection.Restore(new[]{(id:1UL,team:FootballTeam.A),(id:2UL,team:FootballTeam.B),(id:3UL,team:FootballTeam.None)},2,false);
   Check(selection.Count(FootballTeam.A)==1&&selection.Count(FootballTeam.B)==1&&selection.Count(FootballTeam.None)==1,"restored room team counts preserved");selection.Clear();Check(selection.Count(FootballTeam.A)==0&&selection.Count(FootballTeam.B)==0&&selection.Count(FootballTeam.None)==0,"cleared team counts reset");
   var labelObject=new GameObject("Localization allocation fixture");labelObject.SetActive(false);var label=labelObject.AddComponent<LocalizedText>();label.font=CoveUI.TextFont;
   var previousLanguage=GameLocalization.Current;
   foreach(GameLanguage language in Enum.GetValues(typeof(GameLanguage))){
    GameLocalization.Select(language,false);label.UserContent=false;label.text="Football";
    Check(label.text==GameLocalization.Translate("Football")&&label.Source=="Football",language+" source and translation preserved");
    NoAllocation(language+" repeated label",()=>label.text="Football");
    label.font=CoveUI.DisplayFont;label.text="Football";Check(label.font==GameLocalization.FontFor(CoveUI.DisplayFont,GameLocalization.Translate("Football")),language+" unchanged source applies a changed font immediately");
    label.UserContent=true;label.text="小明 Amir 42";Check(label.text=="小明 Amir 42",language+" player text preserved");
   }
   GameLocalization.Select(previousLanguage,false);label.UserContent=false;label.text="Football";
   var refresh=(Action)Delegate.CreateDelegate(typeof(Action),label,typeof(LocalizedText).GetMethod("Refresh",BindingFlags.Instance|BindingFlags.NonPublic));
   Measure("unchanged_label_previous_refresh",refresh,10000);Measure("unchanged_label",()=>label.text="Football",10000);
   NoAllocation("Latin font selection",()=>GameLocalization.FontFor(CoveUI.TextFont,"Player 42"));
   var welcome=StartupWelcome.Ensure(true);welcome.BeginLoading();welcome.SetProgress(0,"Preparing your islands...");welcome.SetReady(false);welcome.BeginLoading();
   Check(welcome.Progress==0&&!welcome.IsReady,"ready-to-retry progress resets");welcome.SetReady();
   if(RoomChatUI.Instance){var chat=RoomChatUI.Instance;var update=(Action)Delegate.CreateDelegate(typeof(Action),chat,typeof(RoomChatUI).GetMethod("LateUpdate",BindingFlags.Instance|BindingFlags.NonPublic));NoAllocation("chat with no speech bubbles",update);Measure("idle_chat",update,10000);}
   var ball=FootballBall.Instance;var random=new System.Random(91273);
   for(int walls=0;walls<4;walls++){
    ball.ReviewExpireFootballEffects();
    if((walls&1)!=0)ball.ReviewFootballWall(FootballTeam.A,ball.Pitch.TransformPoint(new Vector3(-3,ball.PitchBounds.max.y,0)),25,600);
    if((walls&2)!=0)ball.ReviewFootballWall(FootballTeam.B,ball.Pitch.TransformPoint(new Vector3(3,ball.PitchBounds.max.y,3)),-40,600);
    bool equivalent=true;int detours=0;
    for(int i=0;i<1024;i++){
     Vector3 Point()=>ball.Pitch.TransformPoint(new Vector3((float)random.NextDouble()*30-15,ball.PitchBounds.max.y,(float)random.NextDouble()*28-14));
     var from=Point();var to=Point();var expected=ReferenceWaypoint(ball,from,to);var actual=FootballNavigation.Waypoint(ball,from,to);equivalent&=expected.Equals(actual);if(!actual.Equals(to))detours++;
    }
    Check(equivalent,"1024 exact navigation results with wall mask "+walls+" (detours="+detours+")");
   }
   var a=ball.Pitch.TransformPoint(new Vector3(-4,ball.PitchBounds.max.y,-8));var b=ball.Pitch.TransformPoint(new Vector3(3,ball.PitchBounds.max.y,10));
   Measure("wall_navigation_previous",()=>ReferenceWaypoint(ball,a,b),1000);Measure("wall_navigation",()=>FootballNavigation.Waypoint(ball,a,b),1000);NoAllocation("wall navigation",()=>FootballNavigation.Waypoint(ball,a,b));
   GameLocalization.Select(GameLanguage.SimplifiedChinese,false);label.text="Aim and shoot";Check(label.text==GameLocalization.Translate("Aim and shoot at goal"),"football contextual caption preserved");
   var app=AppRoot.Instance;var streaming=app.environments.GetComponent<SkySailStreaming>();while(streaming.Busy)yield return null;yield return streaming.Prepare(SportId.Basketball);app.SelectSport(SportId.Basketball);yield return null;
   label.text="Aim and shoot";Check(label.text==GameLocalization.Translate("Aim and shoot"),"same source refreshes when its sport context changes");GameLocalization.Select(previousLanguage,false);Destroy(labelObject);
   var match=BasketballMatch.Instance;Check(match,"basketball match available");
   if(match){
    var arena=BasketballBall.Active.transform.parent;
    Transform ReferenceHoop(FootballTeam team){float sign=team==FootballTeam.A?1:-1;Transform best=null;float distance=0;foreach(var h in arena.GetComponentsInChildren<BasketballHoop>()){float z=arena.InverseTransformPoint(h.transform.position).z*sign;if(z>distance){distance=z;best=h.transform;}}return best;}
    Check(match.Hoop(FootballTeam.A)==ReferenceHoop(FootballTeam.A)&&match.Hoop(FootballTeam.B)==ReferenceHoop(FootballTeam.B),"hoop selection equals original hierarchy scan");
    var hoop=match.Hoop(FootballTeam.A).GetComponent<BasketballHoop>();hoop.enabled=false;Check(match.Hoop(FootballTeam.A)==ReferenceHoop(FootballTeam.A),"disabled hoop component keeps original selection");hoop.enabled=true;
    hoop.gameObject.SetActive(false);Check(match.Hoop(FootballTeam.A)==ReferenceHoop(FootballTeam.A),"inactive hoop object excluded");hoop.gameObject.SetActive(true);
    Measure("hoop_lookup_previous",()=>ReferenceHoop(FootballTeam.A),1000);Measure("hoop_lookup",()=>match.Hoop(FootballTeam.A),1000);NoAllocation("hoop lookup",()=>match.Hoop(FootballTeam.A));
    yield return streaming.Release(SportId.Football);yield return streaming.Release(SportId.Basketball);yield return null;Check(BasketballHoop.Active.Count==0,"unloaded island releases hoop registry");
   }
   Finish();
  }
  void Finish(){File.AppendAllText(Path.Combine(folder,"results.txt"),"OPTIMIZATION_REVIEW_COMPLETE success="+!failed+"\n");Application.Quit(failed?1:0);}
  static int ReferenceTeamCount(Dictionary<ulong,FootballTeam> choices,FootballTeam team)=>choices.Values.Count(value=>value==team);

  // Frozen pre-optimization algorithm: the seeded wall fixtures above require
  // component-exact outputs, including blocked starts and unreachable targets.
  static Vector3 ReferenceWaypoint(FootballBall ball,Vector3 from,Vector3 destination){
   var walls=new[]{ball.Props.wallA,ball.Props.wallB};
   bool Clear(Vector3 a,Vector3 b){foreach(var wall in walls)if(ball.WallActive(wall)&&FootballPropRules.Sweep(wall,a+Vector3.up*.6f,b+Vector3.up*.6f,.5f,out float t,out var normal)){if(t>0||Vector3.Dot(b-a,normal)<=0)return false;}return true;}
   if(Clear(from,destination))return destination;
   var nodes=new Vector3[10];nodes[0]=from;nodes[1]=destination;int count=2;
   foreach(var wall in walls)if(ball.WallActive(wall))for(int i=0;i<4;i++){var p=wall.position+wall.Rotation*new Vector3(((i&1)==0?-1:1)*(FootballPropRules.WallWidth*.5f+.85f),0,((i&2)==0?-1:1)*(FootballPropRules.WallThickness*.5f+.85f));p.y=from.y;if((FootballNavigation.Clamp(ball,p,.5f)-p).sqrMagnitude<.001f)nodes[count++]=p;}
   var cost=new float[count];var previous=new int[count];var visited=new bool[count];for(int i=0;i<count;i++){cost[i]=float.PositiveInfinity;previous[i]=-1;}cost[0]=0;
   for(int step=0;step<count;step++){int current=-1;for(int i=0;i<count;i++)if(!visited[i]&&(current<0||cost[i]<cost[current]))current=i;if(current<0||float.IsPositiveInfinity(cost[current]))break;if(current==1){int next=1;while(previous[next]>0)next=previous[next];return nodes[next];}visited[current]=true;for(int i=0;i<count;i++)if(!visited[i]&&Clear(nodes[current],nodes[i])){float candidate=cost[current]+Vector3.Distance(nodes[current],nodes[i]);if(candidate<cost[i]){cost[i]=candidate;previous[i]=current;}}}
   return from;
  }
 }
}
#endif
