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
 // Tests the same legacy EventSystem used by the Android player, with OS-like
 // touch phases. Fixtures isolate click eligibility without invoking account APIs.
 public sealed class MobileInteractionReview:MonoBehaviour {
  string folder,report;int checks,clicks;bool failed;StandaloneInputModule module;BaseInput previous;TouchCameraReviewInput input;Canvas canvas;TouchButton button;Vector2 point;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-mobileInteractionReview");if(i>=0&&i+1<args.Length)new GameObject("Mobile input review").AddComponent<MobileInteractionReview>().folder=args[i+1];}
  void Check(bool value,string name){checks++;failed|=!value;File.AppendAllText(report,(value?"PASS ":"FAIL ")+name+"\n");}
  IEnumerator Step(){EventSystem.current.SendMessage("OnApplicationFocus",true,SendMessageOptions.DontRequireReceiver);yield return new WaitForEndOfFrame();input.Settle();}
  IEnumerator Touch(int id,TouchPhase phase){input.Set(id,point,phase);yield return Step();}
  IEnumerator Start(){
   Directory.CreateDirectory(folder);report=Path.Combine(folder,"results.txt");File.WriteAllText(report,"");float end=Time.realtimeSinceStartup+120;
   while(!DevelopmentProbe.ReadyToReviewSport&&Time.realtimeSinceStartup<end)yield return null;
   Check(DevelopmentProbe.ReadyToReviewSport,"real packaged island ready");if(failed){Finish();yield break;}
   while(IslandTutorialUI.DismissTop()){}DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;yield return new WaitForSecondsRealtime(1);
   var activeButtons=FindObjectsByType<Button>(FindObjectsSortMode.None);Check(activeButtons.Length>0&&activeButtons.All(b=>b is TouchButton),"all active authored buttons use finger-owned click lifecycle");
   module=EventSystem.current.GetComponent<StandaloneInputModule>();previous=module.inputOverride;input=EventSystem.current.gameObject.AddComponent<TouchCameraReviewInput>();module.inputOverride=input;module.enabled=true;AppRoot.Instance.view.ReviewMouseLookInput=input;
   canvas=new GameObject("Mobile button fixtures",typeof(RectTransform),typeof(Canvas),typeof(GraphicRaycaster)).GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=32765;
   foreach(var kind in new[]{typeof(TouchButton),typeof(CoveButton),typeof(WelcomeButton)}){
    var node=new GameObject("Fixture "+kind.Name,typeof(RectTransform),typeof(Image));var rect=(RectTransform)node.transform;rect.SetParent(canvas.transform,false);rect.anchorMin=rect.anchorMax=rect.pivot=Vector2.one*.5f;rect.sizeDelta=new Vector2(160,90);rect.anchoredPosition=Vector2.zero;
    button=(TouchButton)node.AddComponent(kind);button.targetGraphic=node.GetComponent<Image>();button.onClick.AddListener(()=>clicks++);Canvas.ForceUpdateCanvases();point=RectTransformUtility.WorldToScreenPoint(null,rect.position);clicks=0;
    yield return Touch(701,TouchPhase.Began);yield return Touch(701,TouchPhase.Ended);Check(clicks==1,kind.Name+" normal tap clicks once");
    var click=new PointerEventData(EventSystem.current){pointerId=701,position=point};button.OnPointerClick(click);Check(clicks==1,kind.Name+" duplicate click is ignored");
    yield return Touch(702,TouchPhase.Began);yield return Touch(703,TouchPhase.Began);yield return Touch(703,TouchPhase.Ended);Check(clicks==1,kind.Name+" foreign finger cannot release owner");yield return Touch(702,TouchPhase.Ended);Check(clicks==2,kind.Name+" owner still completes once");
    yield return Touch(704,TouchPhase.Began);yield return Touch(704,TouchPhase.Canceled);Check(clicks==2,kind.Name+" canceled touch never clicks");
    yield return Touch(705,TouchPhase.Began);button.OnCancel(null);yield return Touch(705,TouchPhase.Ended);Check(clicks==2,kind.Name+" cancel event rejects stale release");
    yield return Touch(706,TouchPhase.Began);button.SendMessage("OnApplicationFocus",false);yield return Touch(706,TouchPhase.Ended);Check(clicks==2,kind.Name+" focus loss rejects stale release");
    yield return Touch(707,TouchPhase.Began);button.SendMessage("OnApplicationPause",true);yield return Touch(707,TouchPhase.Ended);Check(clicks==2,kind.Name+" background pause rejects stale release");
    yield return Touch(708,TouchPhase.Began);button.enabled=false;button.enabled=true;yield return Touch(708,TouchPhase.Ended);Check(clicks==2,kind.Name+" disable releases captured finger");
    yield return Touch(709,TouchPhase.Began);button.interactable=false;yield return Touch(709,TouchPhase.Ended);Check(clicks==2,kind.Name+" disabled action cannot click");button.interactable=true;
    yield return Touch(710,TouchPhase.Began);input.Set(710,point+Vector2.right*220,TouchPhase.Moved);yield return Step();input.Set(710,point,TouchPhase.Moved);yield return Step();yield return Touch(710,TouchPhase.Ended);Check(clicks==2,kind.Name+" dragging out cancels tap");
    yield return Touch(711,TouchPhase.Began);yield return Touch(711,TouchPhase.Ended);Check(clicks==3,kind.Name+" fresh touch works after all interruptions");
    button.OnSubmit(new BaseEventData(EventSystem.current));Check(clicks==4,kind.Name+" keyboard submit remains available");Destroy(node);yield return null;
   }
   Check(PlayerView.LookDelta==Vector2.zero,"menu touch fixtures never add camera motion");ReplayChecks();Finish();
  }
  void ReplayChecks(){
   var view=AppRoot.Instance.view;var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;var type=typeof(PlayerView);
   var phase=type.GetField("golfCameraPhase",flags);var at=type.GetField("golfCameraAt",flags);var reset=type.GetMethod("ResetReplayPress",flags);var touch=type.GetMethod("ReplayTouch",flags);
   var savedPhase=phase.GetValue(view);var savedAt=at.GetValue(view);var point=new Vector2(100,100);
   void Begin(){reset.Invoke(view,null);phase.SetValue(view,Enum.Parse(phase.FieldType,"Flight"));at.SetValue(view,Time.time-1);}
   bool Send(int id,TouchPhase state,Vector2 pos)=>(bool)touch.Invoke(view,new object[]{new Touch{fingerId=id,phase=state,position=pos}});
   try{
    Begin();Send(1,TouchPhase.Began,point);Send(2,TouchPhase.Began,point+Vector2.right*100);Check(!Send(2,TouchPhase.Ended,point+Vector2.right*100)&&view.GolfFollowingBall,"foreign finger cannot skip golf replay");Check(Send(1,TouchPhase.Ended,point)&&!view.GolfFollowingBall,"replay owner can complete its own tap");
    Begin();Send(1,TouchPhase.Began,point);Send(1,TouchPhase.Canceled,point);Check(!Send(1,TouchPhase.Ended,point)&&view.GolfFollowingBall,"canceled replay tap cannot skip");
    Begin();Send(1,TouchPhase.Began,point);Send(1,TouchPhase.Moved,point+Vector2.right*50);Send(1,TouchPhase.Moved,point);Check(!Send(1,TouchPhase.Ended,point)&&view.GolfFollowingBall,"camera drag returning to start is never a replay tap");
    Begin();Send(3,TouchPhase.Began,point);Check(Send(3,TouchPhase.Ended,point),"replay accepts a fresh finger after cancellation");
   }finally{reset.Invoke(view,null);phase.SetValue(view,savedPhase);at.SetValue(view,savedAt);}
  }
  void Finish(){if(AppRoot.Instance&&AppRoot.Instance.view)AppRoot.Instance.view.ReviewMouseLookInput=null;if(module){module.inputOverride=previous;module.enabled=true;}if(canvas)Destroy(canvas.gameObject);if(input)Destroy(input);DevelopmentProbe.TurnCommandActive=false;File.AppendAllText(report,"MOBILE_INTERACTION_COMPLETE checks="+checks+" success="+!failed+"\n");Application.Quit(failed?1:0);}
 }
}
#endif
