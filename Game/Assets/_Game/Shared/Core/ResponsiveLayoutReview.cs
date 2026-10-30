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
 public sealed class ResponsiveLayoutReview:MonoBehaviour {
  string folder;int checks;bool failed;AppRoot app;
  struct Case {
   public string name;public int w,h,left,right,top,bottom;
   public Case(string n,int width,int height,int l=0,int r=0,int t=0,int b=0){name=n;w=width;h=height;left=l;right=r;top=t;bottom=b;}
  }
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-responsiveLayoutReview");if(i>=0&&i+1<args.Length){var review=new GameObject("Responsive layout review").AddComponent<ResponsiveLayoutReview>();review.folder=args[i+1];}}
  void Check(bool ok,string name){checks++;failed|=!ok;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+name+"\n");}
  static Rect Bounds(RectTransform r){var c=new Vector3[4];r.GetWorldCorners(c);return Rect.MinMaxRect(c.Min(p=>p.x),c.Min(p=>p.y),c.Max(p=>p.x),c.Max(p=>p.y));}
  static bool Contains(Rect outer,Rect inner)=>inner.xMin>=outer.xMin-1&&inner.yMin>=outer.yMin-1&&inner.xMax<=outer.xMax+1&&inner.yMax<=outer.yMax+1;
  void Reachable(Button b){
   var e=new PointerEventData(EventSystem.current){position=Bounds((RectTransform)b.transform).center,button=PointerEventData.InputButton.Left};var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(e,hits);
   Check(hits.Count>0&&ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject)==b.gameObject,"pointer reaches "+b.name);
  }
  void TextChecks(Transform root){foreach(var t in root.GetComponentsInChildren<LocalizedText>().Where(t=>t.isActiveAndEnabled&&!string.IsNullOrWhiteSpace(t.text)))Check(t.HasUnclippedText(),"unclipped "+t.Source);}
  void MenuChecks(string page){
   Canvas.ForceUpdateCanvases();var area=ResponsiveScreen.SafeArea;var content=app.Menu.Content;
   var buttons=content.GetComponentsInChildren<Button>().Where(b=>b.isActiveAndEnabled).ToArray();
   foreach(var b in buttons){Check(Contains(area,Bounds((RectTransform)b.transform)),page+" safe control "+b.name);if(b.IsInteractable())Reachable(b);}
   for(int i=0;i<buttons.Length;i++)for(int j=i+1;j<buttons.Length;j++)Check(!Bounds((RectTransform)buttons[i].transform).Overlaps(Bounds((RectTransform)buttons[j].transform)),page+" separate "+buttons[i].name+" / "+buttons[j].name);
   TextChecks(content);
   var background=app.Menu.DesignFrame.parent.Find("Sports background") as RectTransform;
   Check(Contains(Bounds(background),new Rect(0,0,Screen.width,Screen.height)),"menu background covers every edge including unsafe inset");
   var design=Bounds(app.Menu.DesignFrame);Check(Contains(area,design),"design inside real safe area");
   Check(Mathf.Abs(design.center.x-Screen.width*.5f)<1&&Mathf.Abs(design.center.y-Screen.height*.5f)<1,"balanced design margins");
   if(page=="home"){
    var cards=content.GetComponentsInChildren<CoveButton>().Where(b=>b.name.StartsWith("Island - ")).Select(b=>Bounds((RectTransform)b.transform)).ToArray();
    Check(cards.Length==4,"all four sports remain present");if(cards.Length==4)Check(app.Menu.CompactLayout?cards.Select(c=>Mathf.Round(c.x)).Distinct().Count()<=2&&cards[2].y<cards[0].y:cards.All(c=>Mathf.Abs(c.y-cards[0].y)<1),"sport grid reflows for aspect ratio");
    if(cards.Length==4)Check(Mathf.Abs(cards.Min(c=>c.xMin)-(Screen.width-cards.Max(c=>c.xMax)))<2,"card grid has equal outside margins");
   }
  }
  IEnumerator Page(string page){app.Show(page);yield return new WaitForSecondsRealtime(.32f);MenuChecks(page);}
  IEnumerator Start(){
   DontDestroyOnLoad(gameObject);Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");
   float end=Time.realtimeSinceStartup+120;while(!DevelopmentProbe.ReadyToReviewSport&&Time.realtimeSinceStartup<end)yield return null;
   app=AppRoot.Instance;Check(app&&app.Menu,"game loaded");if(!app||!app.Menu){Finish();yield break;}app.LeaveCourt();yield return null;
   var cases=new[]{new Case("phone-16x9",1600,900),new Case("phone-18x9",2160,1080),new Case("phone-19.5x9",2340,1080),new Case("reno-left",2374,1080,120),new Case("reno-right",2374,1080,0,120),new Case("phone-20x9",2400,1080,120),new Case("phone-21x9",2520,1080,96),new Case("phone-22x9",2640,1080),new Case("tablet-4x3",1200,900),new Case("tablet-16x10",1280,800),new Case("tablet-3x2",1500,1000),new Case("foldable",1800,1200,0,0,32,40),new Case("square",1000,1000),new Case("small-window",800,600),new Case("portrait-window",900,1600)};
   if(Environment.GetCommandLineArgs().Contains("-responsivePortraitOnly"))cases=new[]{new Case("portrait-phone-20x9",1080,2400),new Case("portrait-tablet-3x4",900,1200),new Case("portrait-tablet-10x16",800,1280)};
   else cases=cases.Concat(new[]{new Case("desktop-32x9",3840,1080)}).ToArray();
   foreach(var c in cases){
    // Windows caps a hidden player at its monitor's dimensions. Exercise the
    // same ratio and proportional insets within that limit; record actual pixels.
    float fit=Mathf.Min(1,Screen.currentResolution.width/(float)c.w,Screen.currentResolution.height/(float)c.h);
    int width=Mathf.RoundToInt(c.w*fit),height=Mathf.RoundToInt(c.h*fit);
    Screen.SetResolution(width,height,false);yield return new WaitForSecondsRealtime(.3f);
    Check(Screen.width==width&&Screen.height==height,"actual ratio window "+c.name+" "+Screen.width+"x"+Screen.height+" requested source "+c.w+"x"+c.h);
    ResponsiveScreen.ReviewSafeArea=new Rect(c.left*fit,c.bottom*fit,Screen.width-(c.left+c.right)*fit,Screen.height-(c.top+c.bottom)*fit);
    foreach(var language in new[]{GameLanguage.English,GameLanguage.SimplifiedChinese,GameLanguage.Malay}){
     GameLocalization.Select(language,false);yield return Page("home");
     bool capture=c.name=="reno-left"||c.name=="tablet-4x3"||c.name.StartsWith("portrait-");
     if(capture)yield return Capture(c.name+"-"+GameLocalization.Code+"-home");
     app.SelectSport(SportId.Golf);while(app.PendingSelection.HasValue)yield return null;yield return Page("sport");if(capture)yield return Capture(c.name+"-"+GameLocalization.Code+"-golf");
     foreach(var page in new[]{"create","join","settings","tutorials"})yield return Page(page);
     var welcome=StartupWelcome.Ensure(true);welcome.SetReady(false);yield return null;welcome.Layout();Canvas.ForceUpdateCanvases();
     Check(Contains(Bounds(welcome.Canvas.transform.Find("Safe frame backdrop") as RectTransform),new Rect(0,0,Screen.width,Screen.height)),"welcome background covers every edge");
     Check(Contains(ResponsiveScreen.SafeArea,Bounds(welcome.Design)),"complete welcome artwork stays inside safe area");
     Check(Mathf.Abs(Bounds(welcome.Design).center.x-Screen.width*.5f)<1,"welcome margins balanced");
     var scene=welcome.Canvas.transform.Find("Safe frame backdrop").GetComponent<RawImage>();
     Check(scene.texture is Texture2D&&scene.texture.width>=1500&&scene.texture.height>=700&&scene.color==Color.white,"welcome uses full-resolution sharp scenery without a blur surface");
     Check(Contains(ResponsiveScreen.SafeArea,Bounds(welcome.Mascot))&&Contains(ResponsiveScreen.SafeArea,Bounds(welcome.Brand))&&Contains(ResponsiveScreen.SafeArea,Bounds(welcome.SignCard)),"welcome focal layers stay fully inside safe area");
     Check(!Bounds(welcome.Mascot).Overlaps(Bounds(welcome.SignCard)),"welcome mascot clear of sign-in controls");
     foreach(var b in new[]{welcome.GuestButton,welcome.HuaweiButton,welcome.LanguageButton,welcome.SettingsButton,welcome.InfoButton}){Check(Contains(ResponsiveScreen.SafeArea,Bounds((RectTransform)b.transform)),"welcome safe button "+b.name);Reachable(b);}
     var focal=new[]{welcome.Mascot,welcome.Brand,welcome.SignCard,(RectTransform)welcome.LanguageButton.transform.parent};
     for(int i=0;i<focal.Length;i++)for(int j=i+1;j<focal.Length;j++)Check(!Bounds(focal[i]).Overlaps(Bounds(focal[j])),"welcome focal layers do not overlap "+focal[i].name+" / "+focal[j].name);
     var uv=scene.uvRect;Check(Mathf.Abs(scene.texture.width*uv.width/(scene.texture.height*uv.height)-(Screen.width+4f)/(Screen.height+4f))<.001f,"sharp scenery keeps its original proportions");
     TextChecks(welcome.Canvas.transform);if(capture||c.name=="phone-16x9"||c.name=="phone-20x9"||c.name=="tablet-16x10"||c.name=="desktop-32x9")yield return Capture(c.name+"-"+GameLocalization.Code+"-welcome");
     Destroy(welcome.gameObject);yield return null;
    }
   }
   // Resize and flip the inset while a guide is open. It must retain its step and
   // follow the actual control, rather than its old authored coordinate.
   GameLocalization.Select(GameLanguage.English,false);yield return Page("home");const string key="menu.firstVisitGuide.v1";bool had=PlayerPrefs.HasKey(key);int saved=PlayerPrefs.GetInt(key);PlayerPrefs.DeleteKey(key);app.Menu.BeginFirstVisitGuide();yield return null;
   var guide=GameObject.Find("First visit guide");guide.GetComponent<Button>().onClick.Invoke();yield return null;
   Screen.SetResolution(1200,900,false);ResponsiveScreen.ReviewSafeArea=new Rect(0,0,1200,900);yield return new WaitForSecondsRealtime(.4f);Canvas.ForceUpdateCanvases();
   var highlight=guide.transform.Find("First visit design/Guide highlight top") as RectTransform;var create=app.Menu.Content.Find("Create room") as RectTransform;
   Check(highlight&&Mathf.Abs(Bounds(highlight).center.x-Bounds(create).center.x)<2,"guide follows resized create button");TextChecks(guide.transform);
   guide.transform.Find("First visit design/Skip introduction").GetComponent<Button>().onClick.Invoke();yield return null;
   if(had)PlayerPrefs.SetInt(key,saved);else PlayerPrefs.DeleteKey(key);PlayerPrefs.Save();ResponsiveScreen.ReviewSafeArea=null;Finish();
  }
  IEnumerator Capture(string name){
   yield return new WaitForEndOfFrame();var go=new GameObject("Layout capture camera");go.transform.position=new Vector3(10000,10000,10000);var camera=go.AddComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.magenta;camera.nearClipPlane=.1f;camera.farClipPlane=10;
   var canvases=FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.isRootCanvas&&c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();
   foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=camera;c.planeDistance=1;}
   Canvas.ForceUpdateCanvases();var rt=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var old=RenderTexture.active;var image=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);
   try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);image.Apply();
    // A missing full-bleed background exposes this camera's magenta clear colour.
    foreach(var point in new[]{new Vector2Int(0,0),new Vector2Int(0,Screen.height/2),new Vector2Int(Screen.width-1,Screen.height/2),new Vector2Int(Screen.width-1,Screen.height-1)})Check(image.GetPixel(point.x,point.y)!=Color.magenta,"painted display edge "+name+" "+point);
    File.WriteAllBytes(Path.Combine(folder,name+".png"),image.EncodeToPNG());
   }finally{Destroy(image);camera.targetTexture=null;RenderTexture.active=old;RenderTexture.ReleaseTemporary(rt);foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceOverlay;c.worldCamera=null;}Destroy(go);Canvas.ForceUpdateCanvases();}
  }
  void Finish(){File.AppendAllText(Path.Combine(folder,"results.txt"),"RESPONSIVE_LAYOUT_COMPLETE success="+!failed+" checks="+checks+"\n");Application.Quit(failed?1:0);}
 }
}
#endif
