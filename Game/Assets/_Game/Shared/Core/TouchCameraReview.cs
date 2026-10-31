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
 public sealed class TouchCameraReview:MonoBehaviour {
  string folder,report;bool failed;int checks;PlayerView view;TouchPad look,stick;StandaloneInputModule module;BaseInput previous;TouchCameraReviewInput input;
  static string Arg(string key){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,key);return i>=0&&i+1<args.Length?args[i+1]:"";}
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){var folder=Arg("-touchCameraReview");if(folder.Length>0)new GameObject("Multitouch camera review").AddComponent<TouchCameraReview>().folder=folder;}
  void Check(bool ok,string name){checks++;failed|=!ok;File.AppendAllText(report,(ok?"PASS ":"FAIL ")+name+"\n");}
  Vector2 Heading=>new Vector2(view.yaw,view.pitch);
  IEnumerator Frame(){yield return new WaitForEndOfFrame();}
  void Pump(){EventSystem.current.SendMessage("OnApplicationFocus",true,SendMessageOptions.DontRequireReceiver);}
  IEnumerator Step(){Pump();yield return Frame();input.Settle();}
  IEnumerator Stable(string name){var h=Heading;yield return Step();Check(Vector2.Distance(h,Heading)<.001f,name);}
  IEnumerator Move(int id,Vector2 point,string name){var h=Heading;var before=input.Point(id);input.Set(id,point,TouchPhase.Moved);yield return Step();var pixels=point-before;var expected=new Vector2(pixels.x*.13f,-pixels.y*.10f);Check(Vector2.Distance(Heading-h,expected)<.003f,name);}
  Vector2 FindLookPoint(){var hits=new List<RaycastResult>();foreach(var point in new[]{new Vector2(.55f,.52f),new Vector2(.65f,.6f),new Vector2(.48f,.4f),new Vector2(.74f,.7f)}){var pos=Vector2.Scale(point,new Vector2(Screen.width,Screen.height));hits.Clear();EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=pos},hits);if(hits.Count>0&&hits[0].gameObject.GetComponentInParent<TouchPad>()==look)return pos;}return Vector2.zero;}
  IEnumerator Start(){
   Directory.CreateDirectory(folder);report=Path.Combine(folder,"results.txt");File.WriteAllText(report,"");float until=Time.realtimeSinceStartup+100;
   while(!DevelopmentProbe.ReadyToReviewSport&&Time.realtimeSinceStartup<until)yield return null;
   Check(DevelopmentProbe.ReadyToReviewSport,"real island ready");if(failed){Finish();yield break;}
   while(IslandTutorialUI.DismissTop()){}yield return new WaitForSecondsRealtime(1);
   var lang=Arg("-language");GameLocalization.Select(lang=="zh-Hans"?GameLanguage.SimplifiedChinese:lang=="ms"?GameLanguage.Malay:GameLanguage.English,false);
   view=AppRoot.Instance.view;view.ClearMatchInput();view.mode=1;view.pitch=16;DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;
   stick=view.stick;look=FindObjectsByType<TouchPad>(FindObjectsSortMode.None).First(p=>p.look&&p.isActiveAndEnabled);
   module=EventSystem.current.GetComponent<StandaloneInputModule>();previous=module.inputOverride;input=EventSystem.current.gameObject.AddComponent<TouchCameraReviewInput>();module.inputOverride=input;module.enabled=true;view.ReviewMouseLookInput=input;
   yield return Frame();Canvas.ForceUpdateCanvases();Vector2 joystick=RectTransformUtility.WorldToScreenPoint(null,stick.transform.position),point=FindLookPoint();Check(point!=Vector2.zero,"actual HUD look region is reachable");Check(!view.InputBlocked,"normal gameplay accepts input");
   // OS-like mouse contamination accompanies a real StandaloneInputModule touch stream.
   input.axes=new Vector2(300,-200);input.Set(11,joystick,TouchPhase.Began);yield return Stable("first movement finger does not turn camera");
   Button action=null;
   if(AppRoot.Instance.SelectedSport==SportId.Football)action=FindFirstObjectByType<FootballPassButton>().GetComponent<Button>();
   else if(AppRoot.Instance.SelectedSport==SportId.Basketball)action=FindObjectsByType<BasketballShootButton>(FindObjectsSortMode.None).First(b=>!b.pass).button;
   else action=FindFirstObjectByType<JumpButton>().button;
   Vector2 actionPoint=RectTransformUtility.WorldToScreenPoint(null,action.transform.position);input.Set(22,actionPoint,TouchPhase.Began);yield return Stable("joystick plus actual action button cannot activate mouse look");input.Set(22,actionPoint,TouchPhase.Ended);yield return Stable("action finger lift cannot rotate camera");
   input.Set(22,point,TouchPhase.Began);yield return Stable("second finger press cannot activate simulated right-mouse look");
   input.Set(11,joystick+Vector2.up*30,TouchPhase.Moved);yield return Stable("moving joystick while look finger rests does not turn camera");Check(stick.value.y>.1f,"joystick remains independently responsive");
   point+=new Vector2(12,-6);yield return Move(22,point,"two fingers use only look owner's exact pixel delta");
   input.Set(11,joystick,TouchPhase.Ended);yield return Stable("lifting first finger does not jump");yield return Stable("touch index reorder after first finger lifts stays anchored");
   point+=new Vector2(-8,4);yield return Move(22,point,"remaining look finger keeps its own anchor");
   input.Set(33,point+new Vector2(60,0),TouchPhase.Began);yield return Stable("third finger cannot replace look owner");input.Set(33,point+new Vector2(100,40),TouchPhase.Moved);yield return Stable("foreign dragging cannot turn camera");
   point+=new Vector2(7,3);yield return Move(22,point,"owner still turns once with another finger on look region");
   input.Set(22,point,TouchPhase.Ended);yield return Stable("owner lift cannot hand off to held foreign finger");input.Set(33,point+new Vector2(150,60),TouchPhase.Moved);yield return Stable("held foreign finger cannot resume without a fresh press");
   input.Set(33,point,TouchPhase.Ended);yield return Stable("all fingers release without residual look");
   input.Set(44,point,TouchPhase.Began);yield return Stable("fresh owner press is anchored");input.Set(55,joystick,TouchPhase.Began);yield return Stable("look-first movement-second order also stays anchored");
   point+=new Vector2(9,-5);yield return Move(44,point,"look-first order preserves exact drag sensitivity");input.Set(44,point,TouchPhase.Canceled);yield return Stable("canceled look finger cannot turn camera");input.Set(55,joystick,TouchPhase.Ended);yield return Stable("cancel releases both controls cleanly");Check(stick.value==Vector2.zero,"movement resets on release");
   input.Set(66,point,TouchPhase.Began);yield return Stable("look resumes after cancel without a jump");view.InputBlocked=true;input.Set(66,point+new Vector2(80,30),TouchPhase.Moved);yield return Stable("modal suppresses finger drag");view.InputBlocked=false;point+=new Vector2(80,30);point+=new Vector2(3,2);yield return Move(66,point,"closing modal does not accumulate blocked travel");input.Set(66,point,TouchPhase.Ended);yield return Stable("post-modal release is clean");
   // Mouse remains available on desktop; its first frame is an anchor.
   input.forceRight=true;input.axes=new Vector2(500,-300);yield return Stable("mouse activation discards stale axis displacement");var heading=Heading;input.axes=new Vector2(1,-.5f);yield return Step();Check(Vector2.Distance(Heading-heading,new Vector2(1.82f,.7f))<.003f,"desktop right-drag retains original sensitivity");input.forceRight=false;yield return Stable("mouse release stops look");
   // Pointer event reuse must not inject a down-event delta or duplicate a drag.
   PlayerView.LookDelta=Vector2.zero;var e=new PointerEventData(EventSystem.current){pointerId=77,position=point,delta=new Vector2(1000,-1000)};look.OnPointerDown(e);Check(PlayerView.LookDelta==Vector2.zero,"nonzero pointer-down event delta is ignored");e.position+=new Vector2(5,2);look.OnDrag(e);Check(PlayerView.LookDelta==new Vector2(5,2),"owned position delta ignores contaminated event delta");look.OnDrag(e);Check(PlayerView.LookDelta==new Vector2(5,2),"duplicate event position cannot double camera movement");look.OnCancel(null);PlayerView.LookDelta=Vector2.zero;e.position+=Vector2.one*50;look.OnDrag(e);Check(PlayerView.LookDelta==Vector2.zero,"cancel rejects stale drags");
   e.button=PointerEventData.InputButton.Right;look.OnPointerDown(e);e.position+=Vector2.one*30;look.OnDrag(e);Check(PlayerView.LookDelta==Vector2.zero,"right mouse is not counted a second time by touch pad");
   PlayerView.LookDelta=new Vector2(80,40);view.SendMessage("OnApplicationFocus",false);look.SendMessage("OnApplicationFocus",false);Check(PlayerView.LookDelta==Vector2.zero,"focus loss clears queued camera movement");
   PlayerView.LookDelta=new Vector2(80,40);view.SendMessage("OnApplicationPause",true);look.SendMessage("OnApplicationPause",true);Check(PlayerView.LookDelta==Vector2.zero,"pause clears queued camera movement");
   input.Set(88,point,TouchPhase.Began);yield return Stable("fresh press after direct cancellation is stable");look.gameObject.SetActive(false);look.gameObject.SetActive(true);input.Set(88,point+Vector2.one*40,TouchPhase.Moved);yield return Stable("disabled look region releases captured pointer");input.Set(88,point,TouchPhase.Ended);yield return Step();
   Check(GameLocalization.Code==(lang.Length>0?lang:"en"),"requested language remains active");yield return Frame();FishingResultsChecks.Capture(report,"touch-hud",0);yield return Frame();Finish();
  }
  void Finish(){if(view)view.ReviewMouseLookInput=null;if(module){module.inputOverride=previous;module.enabled=true;}if(input)Destroy(input);if(view){PlayerView.LookDelta=Vector2.zero;view.ClearMatchInput();}DevelopmentProbe.TurnCommandActive=false;File.AppendAllText(report,"TOUCH_CAMERA_COMPLETE checks="+checks+" success="+!failed+"\n");Application.Quit(failed?1:0);}
 }
 public sealed class TouchCameraReviewInput:BaseInput {
  readonly List<Touch> touches=new();public Vector2 axes;public bool forceRight;
  public Vector2 Point(int id)=>touches.Find(t=>t.fingerId==id).position;
  public void Set(int id,Vector2 position,TouchPhase phase){int i=touches.FindIndex(t=>t.fingerId==id);var previous=i>=0?touches[i].position:position;var t=new Touch{fingerId=id,position=position,deltaPosition=position-previous,phase=phase,type=TouchType.Direct};if(i<0)touches.Add(t);else touches[i]=t;}
  public void Settle(){for(int i=touches.Count-1;i>=0;i--){var t=touches[i];if(t.phase==TouchPhase.Ended||t.phase==TouchPhase.Canceled)touches.RemoveAt(i);else {t.phase=TouchPhase.Stationary;t.deltaPosition=Vector2.zero;touches[i]=t;}}}
  public override bool mousePresent=>false;public override bool touchSupported=>true;public override int touchCount=>touches.Count;public override Touch GetTouch(int i)=>touches[i];
  public override bool GetMouseButton(int button)=>button==1&&(forceRight||touches.Count>=2);public override bool GetMouseButtonDown(int button)=>false;public override bool GetMouseButtonUp(int button)=>false;
  public override float GetAxisRaw(string axis)=>axis=="Mouse X"?axes.x:axis=="Mouse Y"?axes.y:0;public override bool GetButtonDown(string name)=>false;
 }
}
#endif
