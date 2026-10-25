#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

namespace WhatTheFish {
 public sealed class IslandAwardsReview:MonoBehaviour {
  string folder;FishingCeremony ceremony;bool failed,originalMotion;Camera camera;RenderTexture target;Texture2D image;
  static string Arg(string key){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,key);return i>=0&&i+1<args.Length?args[i+1]:"";}
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){var folder=Arg("-islandAwardsReview");if(folder.Length>0)new GameObject("Podium review").AddComponent<IslandAwardsReview>().folder=folder;else if(Environment.GetCommandLineArgs().Contains("-fishingAwardsPreview"))new GameObject("Fishing awards preview").AddComponent<IslandAwardsReview>();}
  void Check(bool value,string label){failed|=!value;File.AppendAllText(Path.Combine(folder,"results.txt"),(value?"PASS ":"FAIL ")+label+"\n");}
  IEnumerator Start(){
   float deadline=Time.realtimeSinceStartup+120;while(!DevelopmentProbe.ReadyToReviewSport&&Time.realtimeSinceStartup<deadline)yield return null;
   if(!DevelopmentProbe.ReadyToReviewSport){Debug.LogError("Podium review requires a loaded Golf/Fishing island.");Application.Quit(1);yield break;}
   while(IslandTutorialUI.DismissTop()){}yield return new WaitForSecondsRealtime(2);originalMotion=MenuPreferences.ReducedMotion;MenuPreferences.SetMotion(false);ceremony=gameObject.AddComponent<FishingCeremony>();camera=Camera.main;
   if(string.IsNullOrEmpty(folder)){ceremony.BeginFishingPreview(FishingGame.Instance);yield break;}
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");target=new RenderTexture(Screen.width,Screen.height,24);image=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);
   var player=AppRoot.Instance.LocalAthlete;var position=player.transform.position;bool golf=AppRoot.Instance.SelectedSport==SportId.Golf;
   bool StartPreview(bool crew=false,bool tie=false)=>golf?ceremony.BeginGolfPreview(GolfMatchManager.Instance):ceremony.BeginFishingPreview(FishingGame.Instance,crew,tie);
   Check(StartPreview(),"preview starts with baked podium");Check(ceremony.CastCount==3,"three winner identities rendered");
   var stageRoot=GameObject.Find("Island honours - render-only cast");Check(stageRoot.GetComponentsInChildren<Collider>(true).Length==0,"podium/cast do not add gameplay collisions");Check(AppRoot.Instance.view.InputBlocked,"ceremony blocks player input");
   ceremony.ReviewTime(golf?2.5f:5);yield return new WaitForEndOfFrame();var entryActors=stageRoot.transform.Cast<Transform>().Where(t=>t.name.StartsWith("Honours #")).ToArray();
   Check(entryActors[0].gameObject.activeSelf&&!entryActors[1].gameObject.activeSelf,"both sports start single-file with the winner first");
   float walkAt=golf?1.5f:4.8f;
   Check(!GameObject.Find("Soft lagoon reveal mist"),"fishing rise has no grey mist overlay");
   var next=FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None).FirstOrDefault(b=>b.name==(golf?"Golf":"Fishing")+" next adventure");
   Check(next&&next.interactable,"bottom-left next adventure is available in awards");
   if(!golf){
    var podium=stageRoot.transform.GetChild(0);ceremony.ReviewTime(1.8f);yield return new WaitForEndOfFrame();float below=podium.localPosition.y;
    ceremony.ReviewTime(3.9f);yield return new WaitForEndOfFrame();Check(below<-2&&Mathf.Abs(podium.localPosition.y)<.001f,"podium still rises from below the water");
    Check(podium.GetComponentsInChildren<MeshFilter>().Any(m=>m.sharedMesh.bounds.max.z>36),"matching timber bridge extends beyond the entrance camera");
   }
   var previousCamera=Vector3.zero;bool forwardOnly=true;
   for(int i=0;i<=30;i++){
    ceremony.ReviewTime(walkAt+i*7.1f/30);yield return new WaitForEndOfFrame();var at=camera.transform.position;
    if(i>0)forwardOnly&=Vector3.Dot(at-previousCamera,camera.transform.forward)>=-.0001f;previousCamera=at;
   }
   Check(forwardOnly,"entrance camera only advances; no backward correction as winners appear");
   ceremony.ReviewTime(walkAt+5.5f);yield return new WaitForEndOfFrame();
   Check(entryActors.All(t=>t.gameObject.activeSelf&&Mathf.Abs(t.localPosition.x)<.001f),"all winners share one entrance lane");
   for(int i=1;i<entryActors.Length;i++)Check(Vector3.Distance(entryActors[i-1].position,entryActors[i].position)>1.8f,"queue keeps at least 1.8 metres between players");
   Check(QueueInFrame(entryActors),"entire entrance queue avoids title and result controls");
   var foot=entryActors[0].GetComponentsInChildren<Transform>().First(t=>t.name=="mixamorig:LeftFoot");
   ceremony.ReviewTime(walkAt+.12f);yield return new WaitForEndOfFrame();var footStart=foot.position;var rootStart=entryActors[0].position;
   ceremony.ReviewTime(walkAt+.32f);yield return new WaitForEndOfFrame();
   float footSlide=Vector3.ProjectOnPlane(foot.position-footStart,Vector3.up).magnitude;
   Check(Vector3.Distance(rootStart,entryActors[0].position)>.15f&&footSlide<.025f,"planted foot stays in place as character advances: "+footSlide);
   Check(!FindObjectsByType<UnityEngine.UI.Image>(FindObjectsSortMode.None).Any(i=>i.name=="Original rank medal"),"no medals above heads");
   Check(stageRoot.GetComponentsInChildren<TextMesh>(true).All(t=>t.GetComponent<Renderer>().sharedMaterial.shader.name=="WhatTheFish/Awards Depth Text"),"podium numbers use depth testing after localization");
   bool film=Environment.GetCommandLineArgs().Contains("-podiumFilm");int frames=film?(golf?510:630):8;
   float[] samples={1.2f,2.5f,5.6f,8.2f,10.5f,12.5f,16,19.5f};
   for(int i=0;i<frames;i++){
    float age=film?i/30f:samples[i];ceremony.ReviewTime(age);yield return new WaitForEndOfFrame();Capture(Path.Combine(folder,film?i.ToString("D5")+".jpg":"shot-"+age.ToString("0.0",System.Globalization.CultureInfo.InvariantCulture)+".jpg"));
   }
   ceremony.ReviewTime(13);yield return new WaitForEndOfFrame();var cameraPosition=camera.transform.position;var cameraRotation=camera.transform.rotation;Check(ceremony.PodiumSettled,"actors reach their platforms");
   var actors=GameObject.Find("Island honours - render-only cast").transform.Cast<Transform>().Where(t=>t.name.StartsWith("Honours #")).ToArray();
   Check(actors.Length==3,"render-only actor count");for(int i=0;i<actors.Length;i++)Check(Vector3.Distance(actors[i].localPosition,IslandAwardsStage.Seat(i+1))<.01f,"rank "+(i+1)+" feet anchored to podium");
   ceremony.ReviewTime(15);yield return new WaitForEndOfFrame();Check(Vector3.Distance(cameraPosition,camera.transform.position)>.1f,"settled camera moves smoothly around the podium");
   float settled=walkAt+7.2f;var path=new Vector3[6];float[] cameraTimes={.05f,.25f,3.9f,4.1f,7.75f,7.95f};
   for(int i=0;i<path.Length;i++){ceremony.ReviewTime(settled+cameraTimes[i]);yield return new WaitForEndOfFrame();path[i]=stageRoot.transform.InverseTransformPoint(camera.transform.position);}
   float beginning=Vector3.Distance(path[0],path[1]),middle=Vector3.Distance(path[2],path[3]),ending=Vector3.Distance(path[4],path[5]);
   Check(beginning<middle*.08f&&ending<middle*.08f&&middle>.2f,"camera eases in, accelerates, then eases out");
   Check(path[0].x>path[2].x&&path[2].x>path[4].x,"camera sweeps from screen-left to screen-right");
   Check(Vector3.Distance(player.transform.position,position)<.025f,"real player position retained within controller grounding tolerance: "+Vector3.Distance(player.transform.position,position));ceremony.Cancel();yield return null;Check(!FishingCeremony.Presenting&&!GameObject.Find("Island honours - render-only cast"),"cancel removes the whole stage");
   if(!golf){foreach(bool crew in new[]{false,true}){Check(StartPreview(crew,!crew),crew?"crew preview":"shared first preview");if(crew){ceremony.ReviewTime(14.5f);yield return new WaitForEndOfFrame();var queue=GameObject.Find("Island honours - render-only cast").transform.Cast<Transform>().Where(t=>t.name.StartsWith("Honours #")).ToArray();Check(queue.All(t=>t.gameObject.activeSelf)&&QueueInFrame(queue),"five-member Crew queue fits safely on screen");Capture(Path.Combine(folder,"crew-entrance.jpg"));}ceremony.ReviewTime(18);yield return new WaitForEndOfFrame();Check(ceremony.CastCount==(crew?5:3),"team/tie cast retained");if(!crew){Check(ceremony.PresentedRanks.All(r=>r==1),"tie keeps equal first ranks");Check(ceremony.PodiumSettled,"shared first players celebrate on the same podium");}Capture(Path.Combine(folder,crew?"crew.jpg":"tie.jpg"));ceremony.Cancel();yield return null;}}
   MenuPreferences.SetMotion(true);Check(StartPreview(),"reduced-motion preview starts");ceremony.ReviewTime(1);yield return new WaitForEndOfFrame();Check(ceremony.PodiumSettled,"reduced motion skips walking and camera cuts");var reducedPosition=camera.transform.position;ceremony.ReviewTime(4);yield return new WaitForEndOfFrame();Check(Vector3.Distance(reducedPosition,camera.transform.position)<.0001f,"reduced motion retains a fixed camera");ceremony.Skip();if(golf){yield return null;Check(FishingCeremony.Presenting,"Golf keeps the podium while showing results");FindFirstObjectByType<GolfLeaderboardUI>()?.CloseResults();ceremony.Cancel();}else{float skipDeadline=Time.realtimeSinceStartup+3;while(FishingCeremony.Presenting&&Time.realtimeSinceStartup<skipDeadline)yield return null;}Check(!FishingCeremony.Presenting,"exit restores gameplay");
   MenuPreferences.SetMotion(originalMotion);Destroy(target);Destroy(image);File.AppendAllText(Path.Combine(folder,"results.txt"),"ISLAND_AWARDS_COMPLETE success="+!failed+"\n");Application.Quit(failed?1:0);
  }
  void Update(){if(string.IsNullOrEmpty(folder)&&ceremony&&Input.GetKeyDown(KeyCode.F10)&&!FishingCeremony.Presenting)ceremony.BeginFishingPreview(FishingGame.Instance);}
  bool QueueInFrame(Transform[] actors){
   foreach(var actor in actors)foreach(var point in new[]{actor.position,actor.position+Vector3.up*1.6f}){
    var screen=camera.WorldToViewportPoint(point);if(screen.z<=0||screen.x<.04f||screen.x>.96f||screen.y<.18f||screen.y>.79f)return false;
   }
   return true;
  }
  void OnDisable(){ceremony?.Cancel();MenuPreferences.SetMotion(originalMotion);}
  void Capture(string path){
   var canvases=FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.isRootCanvas&&c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();var cameras=canvases.Select(c=>c.worldCamera).ToArray();var planes=canvases.Select(c=>c.planeDistance).ToArray();var old=camera.targetTexture;var active=RenderTexture.active;
   try{foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=camera;c.planeDistance=.5f;}camera.targetTexture=target;Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;image.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToJPG(92));}
   finally{camera.targetTexture=old;RenderTexture.active=active;for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=RenderMode.ScreenSpaceOverlay;canvases[i].worldCamera=cameras[i];canvases[i].planeDistance=planes[i];}Canvas.ForceUpdateCanvases();}
  }
 }
}
#endif
