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
 public sealed class MenuDesignReview:MonoBehaviour {
  string folder;int checks;AppRoot app;SportId sport;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-menuDesignReview");if(i>=0&&i+1<args.Length&&args.Contains("-guestProfile"))new GameObject("Menu design review").AddComponent<MenuDesignReview>().folder=args[i+1];}
  void Check(bool ok,string name){File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+name+"\n");if(!ok)throw new InvalidOperationException(name);checks++;}
  IEnumerator Until(Func<bool> ready,string name,float seconds=90){float end=Time.realtimeSinceStartup+seconds;while(!ready()&&Time.realtimeSinceStartup<end)yield return null;Check(ready(),name);}
  Button Find(string name)=>FindObjectsByType<Button>(FindObjectsSortMode.None).First(b=>b.name==name&&b.gameObject.activeInHierarchy);
  Vector2 Point(RectTransform r)=>RectTransformUtility.WorldToScreenPoint(null,r.TransformPoint(r.rect.center));
  void Click(Button button){
   Canvas.ForceUpdateCanvases();var data=new PointerEventData(EventSystem.current){position=Point((RectTransform)button.transform),pointerId=997,button=PointerEventData.InputButton.Left};
   var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(data,hits);
   if(!button.IsInteractable()||hits.Count==0||hits[0].gameObject.GetComponentInParent<Button>()!=button)File.AppendAllText(Path.Combine(folder,"results.txt"),"RAYCAST "+button.name+" at "+data.position+" interactable="+button.IsInteractable()+" hits="+string.Join(", ",hits.Select(h=>h.gameObject.name))+"\n");
   Check(button.IsInteractable()&&hits.Count>0&&hits[0].gameObject.GetComponentInParent<Button>()==button,"reachable "+button.name);
   ExecuteEvents.Execute(button.gameObject,data,ExecuteEvents.pointerDownHandler);ExecuteEvents.Execute(button.gameObject,data,ExecuteEvents.pointerUpHandler);ExecuteEvents.Execute(button.gameObject,data,ExecuteEvents.pointerClickHandler);
  }
  void OpenSettings(){if(sport==SportId.Basketball)BasketballHUD.Instance.ShowSettings();else if(sport==SportId.Football)FootballHUD.Instance.ShowSettings();else (sport==SportId.Golf?GolfHUD.Instance.IslandMenu:FishingHUD.Instance.IslandMenu).Open();}
  void CloseSettings(){if(sport==SportId.Basketball)BasketballHUD.Instance.CloseMenu();else if(sport==SportId.Football)FootballHUD.Instance.CloseMenu();else (sport==SportId.Golf?GolfHUD.Instance.IslandMenu:FishingHUD.Instance.IslandMenu).Close();}
  void LeaveDialog(){if(sport==SportId.Basketball)BasketballHUD.Instance.OpenLeave();else if(sport==SportId.Football)FootballHUD.Instance.OpenLeave();else (sport==SportId.Golf?GolfHUD.Instance.IslandMenu:FishingHUD.Instance.IslandMenu).OpenLeave();}
  void Bounds(RectTransform rect,string name){var corners=new Vector3[4];rect.GetWorldCorners(corners);Check(corners.All(v=>{var p=RectTransformUtility.WorldToScreenPoint(null,v);return p.x>=-1&&p.x<=Screen.width+1&&p.y>=-1&&p.y<=Screen.height+1;}),"screen bounds "+name);}
  void VisibleControls(){foreach(var b in FindObjectsByType<MenuButtonState>(FindObjectsSortMode.None)){if(!b.gameObject.activeInHierarchy)continue;Bounds((RectTransform)b.transform,b.name);Check(b.GetComponent<Button>().targetGraphic,"target graphic "+b.name);}}
  IEnumerator PressPose(Button button){var r=(RectTransform)button.transform;var at=r.position;var size=r.rect.size;var scale=r.lossyScale;var data=new PointerEventData(EventSystem.current){position=Point(r),pointerId=997,button=PointerEventData.InputButton.Left};ExecuteEvents.Execute(button.gameObject,data,ExecuteEvents.pointerDownHandler);yield return new WaitForSecondsRealtime(.2f);Check(r.position==at&&r.rect.size==size&&r.lossyScale==scale,"fixed hit area during press "+button.name);yield return Capture("pressed-"+sport);ExecuteEvents.Execute(button.gameObject,data,ExecuteEvents.pointerUpHandler);}
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;
   yield return Until(()=>DevelopmentProbe.ReadyToReviewSport&&AppRoot.Instance.view.active,"sport ready",120);app=AppRoot.Instance;sport=app.SelectedSport;
   yield return new WaitForSecondsRealtime(2);IslandTutorialUI.DismissTop();
   foreach(var size in new[]{new Vector2Int(1600,900),new Vector2Int(1024,768),new Vector2Int(1920,864)}){
    Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.5f);OpenSettings();yield return null;Check(app.view.InputBlocked,"settings block gameplay "+size);VisibleControls();yield return Capture("settings-"+size.x);
    var camera=Find("Follow");Check(camera.GetComponent<MenuButtonState>(),"shared camera design");Click(camera);Check(app.view.mode==1,"camera choice action");
    if(size.x==1600){yield return PressPose(camera);LeaveDialog();yield return null;VisibleControls();yield return Capture("leave");CloseSettings();OpenSettings();yield return null;
     yield return new WaitForEndOfFrame();Click(Find("Layout"));yield return null;Check(ControlLayoutEditor.Active,"layout editor opens");VisibleControls();yield return Capture("layout-tools");Click(Find("Cancel layout"));yield return null;Check(!ControlLayoutEditor.Active,"layout cancel returns");
    }
    CloseSettings();Check(!app.view.InputBlocked,"settings restore input "+size);
   }
   Screen.SetResolution(1600,900,false);yield return new WaitForSecondsRealtime(.4f);
   if(sport==SportId.Football||sport==SportId.Basketball){
    OpenSettings();yield return new WaitForEndOfFrame();if(sport==SportId.Football)FootballHUD.Instance.SelectMenu(3);else Click(Find("Customize basketball"));yield return null;VisibleControls();yield return Capture("ball-effects");CloseSettings();
    bool basket=sport==SportId.Basketball;Check(basket?BasketballBots.Instance.StartSolo(2):FootballBots.Instance.StartSolo(2),"solo bots created");yield return null;
    Check(basket?BasketballMatch.Instance.StartMatch():FootballMatch.Instance.StartMatch(),"team selection starts");yield return null;
    var confirm=Find("Start bot match");Check(!confirm.interactable,"team required before confirm");
    foreach(var size in new[]{new Vector2Int(1600,900),new Vector2Int(1024,768),new Vector2Int(1920,864)}){Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.4f);VisibleControls();Bounds((RectTransform)confirm.transform.parent,"team dialog");yield return Capture("teams-"+size.x);}
    var blue=Find(basket?"Basketball Team A":"Team A choice");var red=Find(basket?"Basketball Team B":"Team B choice");Click(blue);yield return null;Check(blue.GetComponent<MenuButtonState>().Selected&&confirm.interactable,"blue selected and start enabled");Click(red);yield return null;Check(red.GetComponent<MenuButtonState>().Selected&&!blue.GetComponent<MenuButtonState>().Selected,"red replaces selection");yield return Capture("team-selected");Click(confirm);yield return null;Check(!confirm.gameObject.activeInHierarchy,"confirmation leaves picker");
   }else if(sport==SportId.Fishing){
    var abilities=FindFirstObjectByType<FishingAbilityHUD>();abilities.ReviewPracticeChoice=true;yield return null;Check(abilities.Modal,"skill choice visible");VisibleControls();yield return Capture("skill-choices");Click(abilities.ChoiceButtons[0]);yield return null;Check(FishingGame.Instance.Player(app.LocalAthlete).Value.skillPicked,"skill action preserved");abilities.ReviewPracticeChoice=false;
   }
   File.AppendAllText(Path.Combine(folder,"results.txt"),"MENU_DESIGN_PASS checks="+checks+"\n");Application.Quit(0);
  }
  IEnumerator Capture(string name){
   yield return new WaitForEndOfFrame();var canvases=FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();var cameras=canvases.Select(c=>c.worldCamera).ToArray();var distances=canvases.Select(c=>c.planeDistance).ToArray();var camera=Camera.main;var previousTarget=camera.targetTexture;var previousActive=RenderTexture.active;var target=new RenderTexture(Screen.width,Screen.height,24);var image=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);
   try{foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=camera;c.planeDistance=.5f;}camera.targetTexture=target;for(int pass=0;pass<3;pass++){foreach(var t in FindObjectsByType<Text>(FindObjectsSortMode.None))t.SetAllDirty();Canvas.ForceUpdateCanvases();camera.Render();}RenderTexture.active=target;image.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);image.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),image.EncodeToPNG());}
   finally{camera.targetTexture=previousTarget;RenderTexture.active=previousActive;Destroy(target);Destroy(image);for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=RenderMode.ScreenSpaceOverlay;canvases[i].worldCamera=cameras[i];canvases[i].planeDistance=distances[i];}Canvas.ForceUpdateCanvases();}
  }
 }
}
#endif
