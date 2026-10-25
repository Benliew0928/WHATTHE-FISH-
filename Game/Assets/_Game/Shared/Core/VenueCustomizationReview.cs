#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WhatTheFish {
 public sealed class VenueCustomizationReview:MonoBehaviour {
  AppRoot app;string folder,role;bool failed;readonly Dictionary<SportId,StadiumAppearance> original=new();
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){
   var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-venueReview");if(i<0||i+1>=args.Length)return;
   var review=new GameObject("Venue customization review").AddComponent<VenueCustomizationReview>();review.folder=args[i+1];
   review.role=args.Contains("-localHost")?"host":args.Contains("-localClient")?"guest":"offline";
  }
  void Check(bool value,string label){failed|=!value;File.AppendAllText(Path.Combine(folder,"results.txt"),(value?"PASS ":"FAIL ")+label+"\n");}
  IEnumerator Wait(Func<bool> ready,string label,float seconds=80){float end=Time.realtimeSinceStartup+seconds;while(!ready()&&Time.realtimeSinceStartup<end)yield return null;Check(ready(),label);}
  IEnumerator Settled(){yield return null;yield return Wait(()=>!app.PendingSelection.HasValue&&!app.Menu.IsTransitioning,"page settled");yield return new WaitForSecondsRealtime(.3f);Canvas.ForceUpdateCanvases();}
  CoveButton Button(string name)=>app.Menu.Content.GetComponentsInChildren<CoveButton>().FirstOrDefault(b=>b.name==name);
  void Click(string name){
   var b=Button(name);Check(b&&b.IsInteractable(),"action available: "+name);if(!b||!b.IsInteractable())return;
   Canvas.ForceUpdateCanvases();var rect=(RectTransform)b.transform;var data=new PointerEventData(EventSystem.current){position=rect.TransformPoint(rect.rect.center),pointerId=991,button=PointerEventData.InputButton.Left};
   var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(data,hits);Check(hits.Count>0&&hits[0].gameObject.GetComponentInParent<CoveButton>()==b,"action receives pointer: "+name);
   ExecuteEvents.Execute(b.gameObject,data,ExecuteEvents.pointerDownHandler);ExecuteEvents.Execute(b.gameObject,data,ExecuteEvents.pointerUpHandler);ExecuteEvents.Execute(b.gameObject,data,ExecuteEvents.pointerClickHandler);
  }
  InputField NameField=>app.Menu.Content.GetComponentsInChildren<InputField>().FirstOrDefault(f=>f.name=="Venue name");
  void Name(string value){var field=NameField;Check(field&&field.IsInteractable(),"venue name editable");if(field){field.text=value;field.onEndEdit.Invoke(value);}}
  Camera PreviewCamera=>FindObjectsByType<Camera>(FindObjectsSortMode.None).FirstOrDefault(c=>c.name=="Venue preview camera");
  Color32[] PreviewPixels(){
   var camera=PreviewCamera;Check(camera&&camera.targetTexture,"preview render target exists");if(!camera||!camera.targetTexture)return Array.Empty<Color32>();
   var target=camera.targetTexture;var active=RenderTexture.active;var image=new Texture2D(target.width,target.height,TextureFormat.RGB24,false);
   try{Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;image.ReadPixels(new Rect(0,0,target.width,target.height),0,0);image.Apply();return image.GetPixels32();}
   finally{RenderTexture.active=active;Destroy(image);}
  }
  void ChangedPixels(Color32[] before,Color32[] after,string label,int minimum=40){
   int changed=0;for(int i=0;i<Mathf.Min(before.Length,after.Length);i++)if(Mathf.Abs(before[i].r-after[i].r)+Mathf.Abs(before[i].g-after[i].g)+Mathf.Abs(before[i].b-after[i].b)>24)changed++;
   Check(before.Length>0&&before.Length==after.Length&&changed>=minimum,label+" visible pixels="+changed);
  }
  void VisibleTarget(GameObject target,string label){
   Check(target&&target.activeInHierarchy,"preview target active: "+label);if(!target||!target.activeInHierarchy)return;
   var before=PreviewPixels();try{target.SetActive(false);ChangedPixels(before,PreviewPixels(),"preview actually renders "+label);}
   finally{target.SetActive(true);PreviewPixels();}
  }
  IEnumerator PreviewChoices(SportId sport){
   var camera=PreviewCamera;Check(camera&&!camera.orthographic&&Mathf.Abs(camera.aspect-(float)camera.targetTexture.width/camera.targetTexture.height)<.001f,"preview aspect matches image: "+sport);
   if(sport==SportId.Basketball){
    var arena=(BasketballArenaView)app.stadium;var names=new[]{"Rally crest","Star","Lightning","Shield"};
    for(int i=0;i<names.Length;i++){
     Click("Logo "+names[i]);yield return Settled();VisibleTarget(arena.logoCatalog[i],"basketball "+names[i]);
     yield return Capture("Basketball-logo-"+i);
    }
    Click("Palette Lilac");yield return Settled();var before=PreviewPixels();Click("Palette Rose");yield return Settled();ChangedPixels(before,PreviewPixels(),"basketball palette changes preview",500);yield return Capture("Basketball-palette");
   }else if(sport==SportId.Football){
    Click("Board design");yield return Settled();VisibleTarget(app.stadium.subtitle.gameObject,"football board message");yield return Capture("Football-message");
    Click("Screen design");yield return Settled();Check(!Button("Board design").IsInteractable(),"welcome screen disables board message");VisibleTarget(app.stadium.subtitle.gameObject,"football team welcome");yield return Capture("Football-welcome");
    Click("Decorative flags");yield return Settled();var before=PreviewPixels();yield return Capture("Football-flags-off");
    Click("Decorative flags");yield return Settled();ChangedPixels(before,PreviewPixels(),"football flags toggle in preview");VisibleTarget(((SunvaleStadiumView)app.stadium).flagModules[0],"football decorative banners");yield return Capture("Football-flags-on");
   }else{
    var style=app.stadium.GetComponent<IslandVenueStyle>();Click("Preview detail");yield return Settled();VisibleTarget(style.Equipment.gameObject,sport+" equipment");
    var before=PreviewPixels();Click("Equipment Sea glass");yield return Settled();ChangedPixels(before,PreviewPixels(),sport+" finish changes preview");
    Click("Preview sign");yield return Settled();VisibleTarget(style.Welcome.gameObject,sport+" welcome sign");
   }
   Name("PREVIEW "+sport.ToString().ToUpperInvariant());yield return Settled();
   var title=sport==SportId.Basketball?app.stadium.GetComponentsInChildren<StadiumSign>().First(s=>s.nameSign).label:app.stadium.title;
   VisibleTarget(title.gameObject,sport+" edited venue name");yield return Capture(sport+"-name");
   var pose=PreviewCamera.transform.position;Click("Save venue");yield return Settled();Check(Vector3.Distance(pose,PreviewCamera.transform.position)<.01f,"save retains focused view: "+sport);
   var field=NameField;field.ActivateInputField();yield return null;field.text="CLICK SAVE "+sport;Click("Save venue");yield return Settled();
   Check(LocalProfile.ForSport(sport).title=="CLICK SAVE "+sport,"one click saves while editing name: "+sport);
   if(sport==SportId.Football){
    app.EnterOffline();yield return Wait(()=>FootballStadiumCelebration.Instance&&FootballStadiumCelebration.Instance.Mode==FootballStadiumFeedMode.Live,"match coverage resumes");
    Check(!app.stadium.title.gameObject.activeInHierarchy,"venue sign yields to match broadcast");
    app.Show("venues");yield return Settled();Check(app.stadium.title.gameObject.activeInHierarchy&&!FootballStadiumCelebration.Instance.Screens.gameObject.activeSelf,"customization restores venue sign during exploration");
    app.LeaveCourt();yield return Settled();app.Show("venues");yield return Settled();Check(app.stadium.title.gameObject.activeInHierarchy,"venue sign survives leaving pitch");
   }
  }
  void Layout(){
   foreach(var b in app.Menu.Content.GetComponentsInChildren<CoveButton>()){
    var rect=(RectTransform)b.transform;var corners=new Vector3[4];rect.GetWorldCorners(corners);
    Check(corners[0].x>=-1&&corners[0].y>=-1&&corners[2].x<=Screen.width+1&&corners[2].y<=Screen.height+1,"control in viewport: "+b.name);
    if(!b.IsInteractable())continue;var hits=new List<RaycastResult>();var p=new PointerEventData(EventSystem.current){position=rect.TransformPoint(rect.rect.center)};EventSystem.current.RaycastAll(p,hits);
    Check(hits.Count>0&&hits[0].gameObject.GetComponentInParent<CoveButton>()==b,"control receives pointer: "+b.name);
   }
   foreach(var t in app.Menu.Content.GetComponentsInChildren<Text>()){
    if(!t.font||string.IsNullOrEmpty(t.text))continue;
    Check(t.preferredHeight<=t.rectTransform.rect.height+3,"text fits: "+t.text.Replace("\n"," / "));
    if(t.GetComponentInParent<CoveButton>())Check(t.preferredWidth<=t.rectTransform.rect.width+3,"label width: "+t.text);
   }
  }
  IEnumerator Capture(string name){
   yield return new WaitForEndOfFrame();var camera=app.view.GetComponent<Camera>();var canvas=app.Menu.Content.GetComponentInParent<Canvas>();
   var mode=canvas.renderMode;var oldCamera=canvas.worldCamera;float distance=canvas.planeDistance;
   canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
   var rt=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var previous=camera.targetTexture;var active=RenderTexture.active;
   camera.targetTexture=rt;Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=rt;
   var image=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);image.Apply();
   Check(image.GetPixels32().Where((_,i)=>i%1000==0).Select(p=>p.r+p.g*256+p.b*65536).Distinct().Count()>12,"capture renders: "+name);
   File.WriteAllBytes(Path.Combine(folder,name+".png"),image.EncodeToPNG());Destroy(image);camera.targetTexture=previous;RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);
   canvas.renderMode=mode;canvas.worldCamera=oldCamera;canvas.planeDistance=distance;
  }
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");
   yield return Wait(()=>AppRoot.Instance&&AppRoot.Instance.Menu,"app ready");app=AppRoot.Instance;if(!app){Finish();yield break;}
   yield return Wait(()=>!StartupWelcome.Blocking,"welcome dismissed");
   foreach(SportId sport in Enum.GetValues(typeof(SportId)))original[sport]=LocalProfile.ForSport(sport);
   if(role!="offline"){yield return Network();Finish();yield break;}
   app.Show("home");yield return Settled();Click("Customize venue");yield return Settled();
   foreach(SportId sport in Enum.GetValues(typeof(SportId))){
    Click("Venue - "+sport);yield return Settled();
    Check(app.Menu.Page=="venues"&&app.SelectedSport==sport,"sport tab opens editor directly: "+sport);
    Check(NameField&&Button("Save venue")&&!Button("Edit selected venue"),"controls exposed without extra edit step: "+sport);
    Click("Reset venue");yield return Settled();Click("Palette Rose");yield return Settled();Layout();
    Check(app.Menu.Content.GetComponentsInChildren<RawImage>().Any(i=>i.texture),"live world preview: "+sport);
    if(sport==SportId.Golf||sport==SportId.Fishing){
     var style=app.stadium.GetComponent<IslandVenueStyle>();Check(style&&style.AccentSlots>0,"island accent targets exist: "+sport);
     yield return Capture(sport+"-accents");
     Click("Equipment Copper");yield return Settled();
     Check(style.Equipment.GetComponentsInChildren<VenueEquipmentFinish>(true).All(f=>f.Finish==2),"equipment rack updates: "+sport);
     var finish=style.Equipment.GetComponentInChildren<VenueEquipmentFinish>();var renderer=finish.GetComponentInChildren<Renderer>();var material=renderer.sharedMaterial;
     var block=new MaterialPropertyBlock();renderer.GetPropertyBlock(block,0);
     Check(block.GetColor("_BaseColor")!=material.GetColor("_BaseColor"),"finish uses property block without changing shared material: "+sport);
     GameObject held;
     if(sport==SportId.Golf)held=Instantiate(Resources.Load<GolfClubModel>("GolfClub")).gameObject;
     else {held=FishingPresentation.CloneVisual(style.Equipment.Find("BlueLime"),transform,"Review held rod");VenueEquipmentFinish.Attach(held,SportId.Fishing);}
     yield return new WaitForSecondsRealtime(.1f);Check(held.GetComponent<VenueEquipmentFinish>().Finish==2,"new held equipment inherits room finish: "+sport);Destroy(held);
     Click("Sign welcome");yield return Settled();Check(style.Welcome&&style.Welcome.text.Contains("TEAM ROSE"),"welcome sign updates: "+sport);
     Click("Preview detail");yield return Settled();Layout();yield return Capture(sport+"-equipment");
     Click("Preview sign");yield return Settled();yield return Capture(sport+"-sign");
    }else if(sport==SportId.Basketball){Click("Logo Star");yield return Settled();Check(((BasketballArenaView)app.stadium).logoCatalog[1].activeSelf,"court logo updates");}
    else {Click("Board design");yield return Settled();Check(app.stadium.subtitle.text.Contains("GOOD ENERGY"),"football message updates");}
    Click("Preview overview");yield return Settled();yield return Capture(sport+"-overview");
    yield return PreviewChoices(sport);
    Name("VENUE "+sport.ToString().ToUpperInvariant());Click("Save venue");yield return Settled();
    Check(LocalProfile.ForSport(sport).title=="VENUE "+sport.ToString().ToUpperInvariant(),"saved per sport: "+sport);
    Check(app.Menu.Page=="venues","saving stays on sport page: "+sport);
    Click("Palette Gold");yield return Settled();app.Menu.Back();yield return null;
    Check(Button("Discard changes")&&!Button("Save venue").IsInteractable(),"Back protects unsaved changes: "+sport);Click("Discard changes");yield return Settled();
    Check(LocalProfile.ForSport(sport).palette==1,"discard keeps saved palette: "+sport);
    app.Show("venues");yield return Settled();
   }
   Name("UNSAVED SWITCH");Click("Venue - Golf");yield return null;Check(Button("Discard changes"),"switch protects unsaved name");Click("Discard changes");yield return Settled();
   Check(app.SelectedSport==SportId.Golf&&LocalProfile.Fishing.title=="VENUE FISHING","discard switches without saving old sport into new");
   foreach(var resolution in new[]{new Vector2Int(1280,800),new Vector2Int(2340,1080)}){
    Screen.SetResolution(resolution.x,resolution.y,false);yield return new WaitForSecondsRealtime(.8f);Canvas.ForceUpdateCanvases();Layout();yield return Capture("layout-"+Screen.width+"x"+Screen.height);
   }
   app.Menu.Back();yield return Settled();app.Show("venues");yield return Settled();Check(NameField.text==LocalProfile.Golf.title,"saved design reloads on re-entry");
   foreach(var entry in original)LocalProfile.SaveSport(entry.Key,entry.Value);app.Show("home");app.stadium?.Apply(app.CurrentAppearance);Finish();
  }
  IEnumerator Network(){
   yield return Wait(()=>app.rooms.Connected&&FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None).Length>=2,"two peers connected");
   app.Show("venues");yield return Settled();
   if(role=="host"){
    yield return new WaitForSecondsRealtime(4);Click("Reset venue");yield return Settled();Name("SHARED ISLAND");Click("Palette Lilac");yield return Settled();
    Click(app.SelectedSport==SportId.Basketball?"Logo Lightning":app.SelectedSport==SportId.Football?"Board design":"Equipment Sea glass");yield return Settled();Click("Save venue");yield return Settled();
    Check(NetworkAthlete.HostPlayer.WorldAppearance.Value.ToString().Contains("SHARED ISLAND"),"host publishes saved appearance");
    yield return new WaitForSecondsRealtime(8);LocalProfile.SaveSport(app.SelectedSport,original[app.SelectedSport]);
   }else{
    Check(!NameField.IsInteractable()&&!Button("Save venue").IsInteractable()&&!Button("Palette Rose").IsInteractable(),"guest can inspect but cannot edit shared venue");
    Click("Preview detail");yield return Settled();var pose=PreviewCamera.transform.position;
    yield return Wait(()=>NameField&&NameField.text=="SHARED ISLAND","guest receives host appearance",30);yield return Settled();
    var shared=JsonUtility.FromJson<StadiumAppearance>(NetworkAthlete.HostPlayer.WorldAppearance.Value.ToString());Check(shared.palette==2,"guest receives palette");
    Check(Vector3.Distance(pose,PreviewCamera.transform.position)<.3f,"host update preserves guest preview view");
    if(app.SelectedSport==SportId.Basketball){var arena=(BasketballArenaView)app.stadium;Check(arena.logoCatalog[2].activeSelf,"guest receives logo");VisibleTarget(arena.logoCatalog[2],"guest basketball logo");}
    else if(app.SelectedSport==SportId.Football){Check(app.stadium.subtitle.text.Contains("GOOD ENERGY"),"guest receives football board");VisibleTarget(app.stadium.subtitle.gameObject,"guest football message");}
    else {var style=app.stadium.GetComponent<IslandVenueStyle>();Check(style.Appearance.design==1,"guest receives equipment finish");VisibleTarget(style.Equipment.gameObject,"guest equipment");}
    Check(NameField.text=="SHARED ISLAND","guest editor refreshes shared title");
    var change=LocalProfile.DefaultVenue(app.SelectedSport);change.title="GUEST OVERRIDE";app.SaveStadium(change);
    Check(NameField.text=="SHARED ISLAND"&&NetworkAthlete.HostPlayer.WorldAppearance.Value.ToString().Contains("SHARED ISLAND"),"guest write rejected");Layout();yield return Capture("guest-"+app.SelectedSport);
   }
  }
  void Finish(){File.AppendAllText(Path.Combine(folder,"results.txt"),"VENUE_REVIEW_COMPLETE success="+!failed+"\n");Application.Quit(failed?1:0);}
 }
}
#endif
