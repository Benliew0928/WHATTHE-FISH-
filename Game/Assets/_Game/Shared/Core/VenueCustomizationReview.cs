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
  void Click(string name){var b=Button(name);Check(b&&b.IsInteractable(),"action available: "+name);if(b&&b.IsInteractable())b.onClick.Invoke();}
  InputField NameField=>app.Menu.Content.GetComponentsInChildren<InputField>().FirstOrDefault(f=>f.name=="Venue name");
  void Name(string value){var field=NameField;Check(field&&field.IsInteractable(),"venue name editable");if(field){field.text=value;field.onEndEdit.Invoke(value);}}
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
    yield return new WaitForSecondsRealtime(4);Name("SHARED ISLAND");Click("Palette Lilac");yield return Settled();Click("Equipment Sea glass");yield return Settled();Click("Save venue");yield return Settled();
    Check(NetworkAthlete.HostPlayer.WorldAppearance.Value.ToString().Contains("SHARED ISLAND"),"host publishes saved appearance");
    yield return new WaitForSecondsRealtime(8);LocalProfile.SaveSport(app.SelectedSport,original[app.SelectedSport]);
   }else{
    Check(!NameField.IsInteractable()&&!Button("Save venue").IsInteractable()&&!Button("Palette Rose").IsInteractable(),"guest can inspect but cannot edit shared venue");
    yield return Wait(()=>app.stadium.GetComponent<IslandVenueStyle>()?.Appearance.title=="SHARED ISLAND","guest receives host appearance",30);yield return Settled();
    var style=app.stadium.GetComponent<IslandVenueStyle>();Check(style.Appearance.palette==2&&style.Appearance.design==1,"guest receives accent and equipment finish");
    Check(NameField.text=="SHARED ISLAND","guest editor refreshes shared title");
    var change=LocalProfile.DefaultVenue(app.SelectedSport);change.title="GUEST OVERRIDE";app.SaveStadium(change);
    Check(style.Appearance.title=="SHARED ISLAND","guest write rejected");Layout();yield return Capture("guest-"+app.SelectedSport);
   }
  }
  void Finish(){File.AppendAllText(Path.Combine(folder,"results.txt"),"VENUE_REVIEW_COMPLETE success="+!failed+"\n");Application.Quit(failed?1:0);}
 }
}
#endif
