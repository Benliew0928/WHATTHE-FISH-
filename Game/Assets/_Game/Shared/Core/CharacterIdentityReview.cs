#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 public sealed class CharacterIdentityReview:MonoBehaviour {
  string folder;bool failed,basketball;AppRoot app;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-characterIdentityReview");if(i>=0&&i+1<args.Length)new GameObject("Character identity review").AddComponent<CharacterIdentityReview>().folder=args[i+1];}
  void Check(bool ok,string label){failed|=!ok;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+label+"\n");}
  Athlete[] Roster=>basketball?BasketballBots.Roster:FootballBots.Roster;
  bool Running=>basketball?BasketballMatch.Instance.Running:FootballMatch.Instance.Running;
  FootballTeam Team(Athlete actor)=>basketball?BasketballMatch.Instance.TeamOf(actor):FootballMatch.Instance.TeamOf(actor);
  string ExpectedName(Athlete actor){var net=actor.GetComponent<NetworkAthlete>();if(net&&net.Nickname.Value.Length>0)return net.Nickname.Value.ToString();var b=actor.GetComponent<BasketballBot>();if(b)return "BOT "+b.Slot;var f=actor.GetComponent<FootballBot>();return f?"BOT "+f.Slot:actor==app.LocalAthlete?PlayerIdentity.DisplayName:"PLAYER";}
  static bool SameRGB(Color a,Color b)=>Mathf.Abs(a.r-b.r)<.01f&&Mathf.Abs(a.g-b.g)<.01f&&Mathf.Abs(a.b-b.b)<.01f;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]static void SeedReviewName(){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-identityName");if(args.Contains("-characterIdentityReview")&&args.Contains("-guestProfile")&&i>=0&&i+1<args.Length)PlayerIdentity.SaveName(args[i+1],out _);}
  void CheckVisibleBadges(string stage){
   int visible=0;
   Rect noticeBounds=default;var notice=GameObject.Find("Automatic basketball movement");if(notice){var corners=new Vector3[4];((RectTransform)notice.transform).GetWorldCorners(corners);noticeBounds=new Rect(corners[0].x,Screen.height-corners[2].y,corners[2].x-corners[0].x,corners[2].y-corners[0].y);}
   foreach(var actor in Roster){
    if(!CharacterIdentityBadges.TryGetScreenRect(actor,out var screenRect))continue;visible++;
    var identity=actor.GetComponent<CharacterIdentity>();bool presented=CharacterIdentityBadges.TryGetPresentation(actor,out var number,out var playerName,out var team,out var plate,out var accent);
    bool local=actor==app.LocalAthlete;
    Check(presented&&(local?number==""&&playerName=="You":number==identity.Number.ToString("00")&&(playerName==""||playerName==ExpectedName(actor)||playerName.EndsWith("…"))),stage+" open type identity "+identity.Number);
    Check(presented&&team==Team(actor)&&SameRGB(accent,CharacterIdentity.TeamColor(team))&&plate.a==0,stage+" team accent and transparent name background "+identity.Number);
    if(notice)Check(!noticeBounds.Overlaps(screenRect),stage+" nameplate clears automatic-movement hint "+identity.Number);
   }
   Check(visible>0,stage+" has visible nameplates");
   var badges=FindAnyObjectByType<CharacterIdentityBadges>();
   foreach(var text in badges.GetComponentsInChildren<Text>().Where(t=>t.name=="Player name"&&t.gameObject.activeInHierarchy)){
    Check(text.rectTransform.rect.width+.1f>=text.preferredWidth,stage+" full name fits "+text.text);
    var role=text.transform.parent.Find("Player role") as RectTransform;
    if(role&&role.gameObject.activeInHierarchy)Check(text.rectTransform.anchoredPosition.x+text.rectTransform.rect.width<=role.anchoredPosition.x+.1f,stage+" name clears role chip "+text.text);
   }
  }
  IEnumerator Until(Func<bool> condition,string label,float seconds=60){float end=Time.realtimeSinceStartup+seconds;while(!condition()&&Time.realtimeSinceStartup<end){var match=FootballMatch.Instance;if(match&&match.Authority&&match.KickoffReady&&match.KickoffActor)match.TryKickoff(match.KickoffActor,match.BotKickoffHeading(match.KickoffActor),.3f,match.Snapshot.revision);yield return null;}Check(condition(),label);}
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");
   var leases=new IdentitySlots();for(ulong i=1;i<=10;i++)Check(leases.Reserve(i,4)>0,"colliding preference allocated "+i);
   Check(Enumerable.Range(1,10).Select(i=>leases.Reserve((ulong)i)).Distinct().Count()==10,"all ten leases unique");
   Check(leases.Reserve(11)==0,"eleventh participant rejected");byte retained=leases.Reserve(7);leases.Release(3);Check(leases.Reserve(7)==retained,"departure does not shuffle existing identity");Check(leases.Reserve(11)>0,"vacant identity reused");leases.Clear();Check(leases.Count==0,"room reset clears leases");
   leases.Reserve(1);leases.Reserve(2);Check(leases.Prefer(2,0)==2&&leases.Reserve(1)==1,"occupied preference cannot evict another player");Check(leases.Prefer(2,7)==8&&leases.Reserve(1)==1,"free initial preference preserves other identities");
   var legacy=new CharacterAppearance{outfit=3,hair=2,hairstyle=1};legacy.Clamp();int preset=legacy.preset;legacy.Clamp();Check(legacy.version==1&&legacy.preset==preset&&preset>=0&&preset<10,"legacy appearance migrates once");
   yield return Until(()=>DevelopmentProbe.ReadyToReviewSport&&PackagedContentBoot.Ready,"packaged sport ready",100);
   app=AppRoot.Instance;if(!app||!app.Exploring){Finish();yield break;}basketball=app.SelectedSport==SportId.Basketball;
   DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;app.view.mode=2;
   bool network=app.rooms.Connected,authority=!network||app.rooms.Host;
   if(network)yield return Until(()=>RoomTeleport.Players().Count(n=>n.IsPlayerObject&&!n.IsSpectator&&n.GetComponent<Athlete>() is Athlete a&&!a.inTransit)==2,"both network players ready",100);
   if(authority){Check(basketball?(network?BasketballBots.Instance.FillRoom(5):BasketballBots.Instance.StartSolo(5)):(network?FootballBots.Instance.FillRoom(5):FootballBots.Instance.StartSolo(5)),"create ten participants");yield return null;Check(basketball?BasketballMatch.Instance.StartMatch():FootballMatch.Instance.StartMatch(),"begin 5v5");}
   if(authority&&!network){if(basketball){BasketballMatch.Instance.RequestTeam(FootballTeam.A);Check(BasketballMatch.Instance.ConfirmSoloTeam(),"explicit solo start");}else{FootballMatch.Instance.RequestTeam(FootballTeam.A);Check(FootballMatch.Instance.ConfirmSoloTeam(),"explicit solo start");}}
   yield return Until(()=>Roster.Length==10&&Running,"ten participants in live 5v5");yield return null;
   if(Roster.Length!=10){Finish();yield break;}
   var actors=Roster.OrderBy(a=>a.GetComponent<CharacterIdentity>().Number).ToArray();
   Check(actors.Select(a=>a.GetComponent<CharacterIdentity>().Number).OrderBy(n=>n).SequenceEqual(Enumerable.Range(1,10).Select(i=>(byte)i)),"exactly one each of identities 01 to 10");
   Check(actors.Count(a=>Team(a)==FootballTeam.A)==5&&actors.Count(a=>Team(a)==FootballTeam.B)==5,"five blue and five red players");
   var original=actors.Select(a=>a.GetComponent<CharacterIdentity>().Number).ToArray();
   foreach(var actor in actors){
    var identity=actor.GetComponent<CharacterIdentity>();var skins=actor.visual.GetComponentsInChildren<SkinnedMeshRenderer>(true);
    Check(identity.UniformTeam==Team(actor),"uniform matches assigned team "+identity.Number);
    Check(skins.All(s=>s.sharedMaterial.GetColor("_JerseyColor")==CharacterIdentity.TeamColor(Team(actor))),"rendered material has assigned team color "+identity.Number);
    Check(skins.Length==2&&skins.All(s=>s.sharedMesh.GetBlendShapeIndex("GolfRightGrip")>=0),"both LOD hand grips retained "+identity.Number);
    Check(skins.All(s=>s.sharedMaterial.shader.isSupported&&s.sharedMaterial.GetFloat("_IdentityNumber")==identity.Number),"supported numbered material "+identity.Number);
    Check(skins.All(s=>s.sharedMaterial.GetTexture("_BaseMap")==actor.identityLibrary.bodyMaterial.GetTexture("_BaseMap")),"original color texture shared "+identity.Number);
    actor.HideHead(true);Check(identity.AddedRenderers.All(r=>!r.enabled),"first-person attachment hidden "+identity.Number);actor.HideHead(false);
    if(!identity.OriginalHair)Check(identity.AddedRenderers.All(r=>r.GetComponent<MeshFilter>().sharedMesh&&r.sharedMaterial),"both silhouette LODs present "+identity.Number);
    else Check(identity.AddedRenderers.All(r=>!r.GetComponent<MeshFilter>().sharedMesh),"original preset has no accessory geometry in replay copies");
   }
   File.WriteAllLines(Path.Combine(folder,"roster.txt"),actors.Select(a=>$"{(a.GetComponent<NetworkAthlete>()?a.GetComponent<NetworkAthlete>().NetworkObjectId:0)} {a.GetComponent<CharacterIdentity>().Number} {Team(a)} {ExpectedName(a)}"));
   if(network){yield return new WaitForSeconds(12);Check(actors.All(a=>a)&&actors.Select(a=>a.GetComponent<CharacterIdentity>().Number).SequenceEqual(original),"identities stable during network gameplay");CheckVisibleBadges("network gameplay");yield return Capture("network-nameplates");if(authority)yield return new WaitForSeconds(4);Finish();yield break;}
   yield return new WaitForSeconds(3);yield return Capture("live-camera");CheckVisibleBadges("live gameplay");
   app.enabled=false;app.view.enabled=false;SkySailWorld.Instance.reviewCamera=true;
   foreach(var a in actors){var f=a.GetComponent<FootballBot>();if(f)f.enabled=false;var b=a.GetComponent<BasketballBot>();if(b)b.enabled=false;a.HideHead(false);}
   Time.timeScale=0;var camera=Camera.main;float ground=actors.Min(a=>a.transform.position.y);Vector3 center=new Vector3(0,ground,0);
   for(int i=0;i<actors.Length;i++){actors[i].capsule.enabled=false;actors[i].transform.SetPositionAndRotation(center+Vector3.right*((i-4.5f)*1.15f),Quaternion.identity);actors[i].ResetLocomotion();var animator=actors[i].GetComponentInChildren<Animator>();animator.Play("Idle",0,0);animator.Update(0);}
   foreach(var canvas in FindObjectsByType<Canvas>(FindObjectsSortMode.None))if(canvas.renderMode!=RenderMode.WorldSpace&&canvas.name!="Player identity badges")canvas.enabled=false;
   camera.transform.position=center+new Vector3(0,3.8f,12);camera.transform.LookAt(center+Vector3.up);camera.fieldOfView=48;yield return null;yield return Capture("lineup-front");CheckVisibleBadges("lineup");
   camera.transform.position=center+new Vector3(0,3.8f,-12);camera.transform.LookAt(center+Vector3.up);yield return null;yield return Capture("lineup-back");
   foreach(var actor in actors){
    var pos=actor.transform.position;camera.transform.position=pos+new Vector3(.8f,1.5f,2.7f);camera.transform.LookAt(pos+Vector3.up*.95f);camera.fieldOfView=38;yield return null;
    yield return Capture("identity-"+actor.GetComponent<CharacterIdentity>().Number.ToString("00"));
   }
   // The runner uses an isolated profile with a maximum-length, wide-glyph name.
   var local=app.LocalAthlete;foreach(var actor in actors)actor.transform.position=actor==local?center:center+Vector3.right*(100+actor.GetComponent<CharacterIdentity>().Number*3);
   camera.transform.position=center+new Vector3(0,3.8f,8);camera.transform.LookAt(center+Vector3.up);camera.fieldOfView=48;yield return null;
   yield return Capture("long-name");Check(CharacterIdentityBadges.TryGetScreenRect(local,out _),"maximum-length local name is visible");CheckVisibleBadges("long name");
   Time.timeScale=1;if(basketball)BasketballMatch.Instance.Clear();else FootballMatch.Instance.ClearMatch();yield return null;
   Check(actors.All(a=>a.GetComponent<CharacterIdentity>().UniformTeam==FootballTeam.None),"outside match personal uniform restored");Check(actors.Select(a=>a.GetComponent<CharacterIdentity>().Number).SequenceEqual(original),"clearing match preserves all personal identities");Finish();
  }
  IEnumerator Capture(string name){
   yield return null;yield return new WaitForEndOfFrame();var camera=Camera.main;
   var canvases=FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.enabled&&c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();
   foreach(var canvas in canvases){canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;}
   int width=Screen.width,height=Screen.height;
   Canvas.ForceUpdateCanvases();var rt=RenderTexture.GetTemporary(width,height,24);var previous=RenderTexture.active;var target=camera.targetTexture;var pixels=new Texture2D(width,height,TextureFormat.RGB24,false);
   try{camera.targetTexture=rt;foreach(var instances in FindObjectsByType<MobileVegetationInstances>(FindObjectsSortMode.None))instances.RenderForCamera(camera);camera.Render();RenderTexture.active=rt;pixels.ReadPixels(new Rect(0,0,width,height),0,0);pixels.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),pixels.EncodeToPNG());var colors=pixels.GetPixels32();Check(colors.Where((_,i)=>i%100==0).Select(c=>(c.r/16,c.g/16,c.b/16)).Distinct().Count()>20,"nonblank capture "+name);Check(colors.Count(c=>c.r>230&&c.b>230&&c.g<30)<colors.Length/100,"no missing shader "+name);}
   finally{camera.targetTexture=target;RenderTexture.active=previous;RenderTexture.ReleaseTemporary(rt);Destroy(pixels);foreach(var canvas in canvases)canvas.renderMode=RenderMode.ScreenSpaceOverlay;Canvas.ForceUpdateCanvases();}
  }
  void Finish(){Time.timeScale=1;File.AppendAllText(Path.Combine(folder,"results.txt"),"CHARACTER_IDENTITY_REVIEW_COMPLETE success="+!failed+"\n");Application.Quit(failed?1:0);}
 }
}
#endif
