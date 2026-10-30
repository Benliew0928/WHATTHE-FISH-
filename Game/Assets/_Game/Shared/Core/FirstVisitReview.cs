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
 public sealed class FirstVisitReview:MonoBehaviour {
  float savedUI;bool hadUI;string folder;int checks;bool failed;const string Key="menu.firstVisitGuide.v1";
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-firstVisitReview");if(i<0||i+1>=args.Length)return;var review=new GameObject("First visit review").AddComponent<FirstVisitReview>();review.folder=args[i+1];}
  void Check(bool ok,string label){checks++;failed|=!ok;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+label+"\n");}
  static Rect Bounds(RectTransform r){var corners=new Vector3[4];r.GetWorldCorners(corners);return Rect.MinMaxRect(corners.Min(p=>p.x),corners.Min(p=>p.y),corners.Max(p=>p.x),corners.Max(p=>p.y));}
  Button Button(string name)=>GameObject.Find(name)?.GetComponent<Button>();
  void Click(Button b,Vector2 point,string label){
   Canvas.ForceUpdateCanvases();var e=new PointerEventData(EventSystem.current){position=point,button=PointerEventData.InputButton.Left};var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(e,hits);
   var target=hits.Count>0?ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject):null;Check(b&&b.IsInteractable()&&target==b.gameObject,"raycast "+label);
   if(target!=b.gameObject)return;ExecuteEvents.Execute(target,e,ExecuteEvents.pointerDownHandler);ExecuteEvents.Execute(target,e,ExecuteEvents.pointerUpHandler);ExecuteEvents.Execute(target,e,ExecuteEvents.pointerClickHandler);
   if(b.name=="First visit guide"||b.name=="Skip introduction"){var sound=FindFirstObjectByType<CoveSound>();Check(sound&&sound.GetComponent<AudioSource>().isPlaying,"existing menu tap plays "+label);}
  }
  IEnumerator Capture(string name){
   var canvas=GameObject.Find("Mobile UI").GetComponent<Canvas>();var cameraObject=new GameObject("Guide capture camera");var camera=cameraObject.AddComponent<Camera>();
   cameraObject.transform.position=new Vector3(10000,10000,10000);camera.orthographic=true;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.black;camera.nearClipPlane=.1f;camera.farClipPlane=10;
   var target=new RenderTexture(Screen.width,Screen.height,24);camera.targetTexture=target;canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
   Canvas.ForceUpdateCanvases();yield return new WaitForEndOfFrame();camera.Render();var previous=RenderTexture.active;RenderTexture.active=target;
   var image=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);image.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),image.EncodeToPNG());
   RenderTexture.active=previous;canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.worldCamera=null;camera.targetTexture=null;target.Release();Destroy(target);Destroy(image);Destroy(cameraObject);yield return null;
  }
  IEnumerator Start(){
   DontDestroyOnLoad(gameObject);Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");bool had=PlayerPrefs.HasKey(Key);int saved=PlayerPrefs.GetInt(Key);float deadline=Time.realtimeSinceStartup+120;
   while(!DevelopmentProbe.ReadyToReviewSport&&Time.realtimeSinceStartup<deadline)yield return null;
   var app=AppRoot.Instance;Check(app&&app.Menu,"game ready");if(!app||!app.Menu){Finish();yield break;}
   savedUI=MenuPreferences.UIVolume;hadUI=PlayerPrefs.HasKey("cove.ui");MenuPreferences.SetUI(.65f);
   app.LeaveCourt();yield return null;app.Show("home");yield return new WaitForSecondsRealtime(.5f);
   foreach(var language in new[]{GameLanguage.English,GameLanguage.SimplifiedChinese,GameLanguage.Malay}){
    GameLocalization.Select(language,false);PlayerPrefs.DeleteKey(Key);app.Menu.BeginFirstVisitGuide();yield return null;
    for(int step=0;step<15;step++){
     yield return new WaitForSecondsRealtime(.1f);Canvas.ForceUpdateCanvases();var guide=GameObject.Find("First visit guide");Check(guide,"guide present "+language+" "+step);if(!guide)break;
     var narration=guide.GetComponent<FirstVisitNarration>();var voice=narration? narration.Source:null;
     Check(voice&&voice.clip==Resources.Load<AudioClip>("FirstVisitNarration/"+GameLocalization.Code+"/"+(step+1).ToString("00")),"matching narration language and step "+language+" "+step);
     Check(voice&&voice.isPlaying&&!voice.loop&&voice.spatialBlend==0&&voice.ignoreListenerPause,"single non-looping menu narration plays "+language+" "+step);
     Check(voice&&voice.clip&&voice.clip.length>.3f&&voice.clip.length<30&&Mathf.Approximately(voice.volume,MenuPreferences.UIVolume),"narration duration and UI volume "+language+" "+step);
     var design=guide.transform.Find("First visit design");var panel=design.Find("Guide dialogue");var portrait=design.Find("Guide character").GetComponent<RawImage>();var skip=Button("Skip introduction");
     Check(portrait.texture,"character artwork loaded "+language+" "+step);var db=Bounds((RectTransform)design);var pb=Bounds((RectTransform)panel);var sb=Bounds((RectTransform)skip.transform);var cb=Bounds((RectTransform)portrait.transform);
     Check(db.Contains(pb.min)&&db.Contains(pb.max)&&db.Contains(sb.min)&&db.Contains(sb.max)&&db.Contains(cb.min)&&db.Contains(cb.max),"guide elements in bounds "+language+" "+step);
     Check(!pb.Overlaps(sb)&&pb.Contains(cb.min)&&pb.Contains(cb.max),"character fits inside dialogue and skip is separate "+language+" "+step);
     foreach(var text in panel.GetComponentsInChildren<Text>())Check(!Bounds(text.rectTransform).Overlaps(cb),"dialogue text clear of character "+language+" "+step);
     var hi=design.Find("Guide highlight top");var lo=design.Find("Guide highlight bottom");if(hi&&lo){var h=Bounds((RectTransform)hi);var l=Bounds((RectTransform)lo);var target=Rect.MinMaxRect(h.xMin,l.yMin,h.xMax,h.yMax);Check(!target.Overlaps(pb)&&!target.Overlaps(cb)&&!target.Overlaps(sb),"guide does not cover highlighted control "+language+" "+step);}
     foreach(var text in guide.GetComponentsInChildren<LocalizedText>().Where(x=>x.isActiveAndEnabled&&!string.IsNullOrEmpty(x.text))){Check(text.HasUnclippedText(),"text fits "+language+" "+step+" "+text.Source);}
     if(step==0||step==1||step==3||step==13)yield return Capture(GameLocalization.Code+"-step-"+step);
     var advance=guide.GetComponent<Button>();Click(advance,db.center,"advance "+language+" "+step);yield return null;
    }
    Check(!GameObject.Find("First visit guide")&&PlayerPrefs.GetInt(Key)==1,"completion saves "+language);app.Menu.BeginFirstVisitGuide();yield return null;Check(!GameObject.Find("First visit guide"),"completed guide stays closed "+language);
    PlayerPrefs.DeleteKey(Key);app.Menu.BeginFirstVisitGuide();yield return null;var skipVoice=GameObject.Find("First visit guide").GetComponent<FirstVisitNarration>().Source;var skipAgain=Button("Skip introduction");Click(skipAgain,Bounds((RectTransform)skipAgain.transform).center,"skip "+language);Check(!skipVoice.isPlaying,"skip stops narration immediately "+language);yield return null;
    Check(!GameObject.Find("First visit guide")&&PlayerPrefs.GetInt(Key)==1,"skip saves "+language);
   }
   PlayerPrefs.DeleteKey(Key);app.Menu.BeginFirstVisitGuide();yield return null;
   var live=GameObject.Find("First visit guide").GetComponent<FirstVisitNarration>();GameLocalization.Select(GameLanguage.SimplifiedChinese,false);yield return null;
   Check(live.Source.clip==Resources.Load<AudioClip>("FirstVisitNarration/zh-Hans/01")&&live.Source.isPlaying,"live language change switches narration");
   MenuPreferences.SetUI(0);yield return null;Check(live.Source.volume==0,"UI mute silences narration");MenuPreferences.SetUI(.65f);
   var hiddenSource=live.Source;app.Menu.Hide();Check(!hiddenSource.isPlaying&&PlayerPrefs.GetInt(Key)==0,"hiding stops speech without marking interrupted guide seen");yield return null;app.Show("home");
   MenuPreferences.SetUI(savedUI);if(!hadUI)PlayerPrefs.DeleteKey("cove.ui");
   if(had)PlayerPrefs.SetInt(Key,saved);else PlayerPrefs.DeleteKey(Key);PlayerPrefs.Save();GameLocalization.Select(GameLanguage.English,false);Finish();
  }
  void Finish(){File.AppendAllText(Path.Combine(folder,"results.txt"),"FIRST_VISIT_COMPLETE success="+!failed+" checks="+checks+"\n");Application.Quit(failed?1:0);}
 }
}
#endif
