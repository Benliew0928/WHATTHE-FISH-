#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WhatTheFish {
 public sealed partial class ScoreboardReview {
  SportsScoreboard Hud=>Boards.First(v=>!v.Venue);
  void SeedPlayerStatistics(){
   var pointsA=sport==SportId.Basketball?new[]{9,5,4,2,2}:new[]{1,1,0,0,0};var pointsB=sport==SportId.Basketball?new[]{6,5,4,3,0}:new[]{1,0,0,0,0};int a=0,b=0,sa=0,sb=0;
   if(sport==SportId.Basketball){var m=BasketballMatch.Instance;foreach(var old in m.Statistics.Players.Values.OrderBy(p=>p.id).ToArray()){var p=old;p.points=p.team==FootballTeam.A?pointsA[a++]:pointsB[b++];p.passes=4+(a+b);p.assists=(a+b)%3;p.interceptions=(a+b)%2;m.Statistics.Players[p.id]=p;if(p.team==FootballTeam.A)sa+=p.points;else sb+=p.points;}}
   else{var m=FootballMatch.Instance;foreach(var old in m.Statistics.Players.Values.OrderBy(p=>p.id).ToArray()){var p=old;p.goals=p.team==FootballTeam.A?pointsA[a++]:pointsB[b++];p.passes=7+(a+b);p.assists=p.goals==0?1:0;p.interceptions=(a+b)%4;m.Statistics.Players[p.id]=p;if(p.team==FootballTeam.A)sa+=p.goals;else sb+=p.goals;}}
   SetScore(sa,sb);
  }
  void ScoreForFirstBlue(){
   if(sport==SportId.Basketball){var m=BasketballMatch.Instance;var p=m.Statistics.Players.Values.First(p=>p.team==FootballTeam.A);p.points+=3;m.Statistics.Players[p.id]=p;SetScore(m.Snapshot.scoreA+3,m.Snapshot.scoreB);}
   else{var m=FootballMatch.Instance;var p=m.Statistics.Players.Values.First(p=>p.team==FootballTeam.A);p.goals++;m.Statistics.Players[p.id]=p;SetScore(m.Snapshot.scoreA+1,m.Snapshot.scoreB);}
  }
  IEnumerator DetailsHost(){
   SeedPlayerStatistics();yield return new WaitForSecondsRealtime(.3f);var hud=Hud;var details=hud.Details;
   Check(details&&!details.Expanded,"details start collapsed");Check(Boards.Where(v=>v.Venue).All(v=>!v.Details),"physical venue boards stay passive");
   foreach(var board in Boards){var face=board.transform.Find("Sport score chassis").GetComponent<BasketballPlate>();Check(Mathf.Approximately(face.color.a,board.Venue?1:.8f),board.Venue?"venue display retains solid face":"HUD background is 80 percent opaque");}
   foreach(var point in new[]{new Vector2(.02f,.02f),new Vector2(.98f,.02f),new Vector2(.02f,.98f),new Vector2(.98f,.98f),new Vector2(.5f,.5f)}){
    ClickScore(point);yield return new WaitForSecondsRealtime(.27f);Check(details.Expanded,"whole scoreboard opens at "+point);ClickScore(point);yield return new WaitForSecondsRealtime(.27f);Check(!details.Expanded,"whole scoreboard closes at "+point);
   }
   ClickScore(Vector2.one*.5f);yield return new WaitForSecondsRealtime(.3f);CheckStatistics();
   Check(details.Players.Count==(app.rooms.Connected?4:10),"every participating player has a row");
   Check(Mathf.Approximately(details.Panel.Find("Translucent details background").GetComponent<BasketballPlate>().color.a,.8f),"expanded background is 80 percent opaque");
   Check(details.Panel.GetComponentsInChildren<Text>().All(t=>Mathf.Approximately(t.color.a,1)),"statistics text stays fully opaque");
   Check(Time.timeScale==1&&!app.view.InputBlocked,"details do not pause the match or lock movement");
   if(sport==SportId.Football){var red=details.Players.First(p=>p.team==FootballTeam.B);Check(details.SelectPlayer(red.id),"red player can be selected");Check(details.SelectedName.text==ScoreboardDetails.ShortName(red.name,26),"football spotlight names the selected player");Check(!details.SelectPlayer(ulong.MaxValue-1),"missing player cannot replace selection");}
   foreach(var size in new[]{new Vector2Int(1600,900),new Vector2Int(844,390),new Vector2Int(1024,768)}){
    Screen.SetResolution(size.x,size.y,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.45f);Canvas.ForceUpdateCanvases();CheckPanelBounds(details,size);
    foreach(var scroll in new[]{details.BlueScroll,details.RedScroll}){scroll.verticalNormalizedPosition=0;yield return new WaitForSecondsRealtime(.15f);Check(scroll.verticalNormalizedPosition<.02f||!scroll.vertical,"last player remains reachable "+size);scroll.verticalNormalizedPosition=1;}
    yield return Capture("expanded-"+size.x+"x"+size.y);
   }
   Screen.SetResolution(1600,900,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.3f);
   details.BlueScroll.verticalNormalizedPosition=0;yield return new WaitForSecondsRealtime(.3f);Check(details.BlueScroll.verticalNormalizedPosition<.02f||!details.BlueScroll.vertical,"live refresh preserves roster scroll position");details.BlueScroll.verticalNormalizedPosition=1;
   if(app.rooms.Connected)yield return new WaitForSecondsRealtime(5);
   ScoreForFirstBlue();yield return new WaitForSecondsRealtime(.3f);CheckStatistics();Check(hud.Animating,"scoring animation plays while expanded");yield return Capture("expanded-scoring");
   yield return new WaitForSecondsRealtime(2);details.HandleBack();Check(!details.Expanded&&ScoreboardDetails.BlocksBack,"Back consumes close before leaving the island");yield return null;yield return null;Check(!ScoreboardDetails.BlocksBack,"Back consumption clears next frame");
   bool reduced=MenuPreferences.ReducedMotion;MenuPreferences.SetMotion(true);details.SetExpanded(true);yield return null;yield return null;Check(details.Panel.GetComponent<CanvasGroup>().alpha==1,"reduced motion opens immediately");details.SetExpanded(false);yield return null;yield return null;Check(!details.Panel.gameObject.activeSelf,"reduced motion closes immediately");MenuPreferences.SetMotion(reduced);
   details.SetExpanded(true);yield return new WaitForSecondsRealtime(.3f);details.SendMessage("OnApplicationFocus",false);Check(!details.Expanded,"losing focus closes details");details.SendMessage("OnApplicationFocus",true);
   details.SetExpanded(true);yield return null;hud.gameObject.SetActive(false);yield return null;hud.gameObject.SetActive(true);yield return null;Check(!details.Expanded,"HUD reactivation does not restore stale expansion");
   details.SetExpanded(true);yield return new WaitForSecondsRealtime(.3f);if(sport==SportId.Basketball)BasketballMatch.Instance.Statistics.Clear();else FootballMatch.Instance.Statistics.Clear();SetScore(0,0);
   yield return new WaitForSecondsRealtime(.35f);Check(details.Players.Count==0,"reset removes stale player statistics");CheckStatistics();details.SetExpanded(false);yield return new WaitForSecondsRealtime(3);
  }
  IEnumerator DetailsGuest(){
   yield return Wait(()=>Hud&&Hud.Frame.a>0,"replicated detailed match",100);var details=Hud.Details;int initial=Hud.Frame.a;ClickScore(Vector2.one*.5f);yield return new WaitForSecondsRealtime(.35f);Check(details.Expanded,"guest expands the entire scoreboard");CheckStatistics();Check(details.Players.Count==4,"guest receives every player record");yield return Capture("guest-expanded");
   yield return Wait(()=>Hud.Frame.a==initial+(sport==SportId.Basketball?3:1),"replicated detailed scoring",75);yield return new WaitForSecondsRealtime(.2f);CheckStatistics();yield return Capture("guest-expanded-score");
   yield return Wait(()=>Hud.Frame.a==0&&details.Players.Count==0,"guest receives clean statistics reset",35);CheckStatistics();details.HandleBack();
  }
  void CheckStatistics(){
   var details=Hud.Details;Check(details.Panel.Find("Blue player list/Team score").GetComponent<Text>().text==Hud.Frame.a.ToString()&&details.Panel.Find("Red player list/Team score").GetComponent<Text>().text==Hud.Frame.b.ToString(),"expanded team scores match the HUD");
   var rows=details.Panel.GetComponentsInChildren<RectTransform>(true);
   foreach(var p in details.Players){var row=rows.FirstOrDefault(r=>r.name=="Player "+p.id);Check(row,"player identity row "+p.id);if(!row)continue;
    Check(row.Find(sport==SportId.Basketball?"Points":"Goals").GetComponent<Text>().text==p.score.ToString()&&row.Find("Assists").GetComponent<Text>().text==p.assists.ToString(),"score and assists displayed for "+p.id);
    if(sport==SportId.Basketball)Check(row.Find("Passes").GetComponent<Text>().text==p.passes.ToString()&&row.Find("Interceptions").GetComponent<Text>().text==p.interceptions.ToString(),"passes and interceptions displayed for "+p.id);
    else {details.SelectPlayer(p.id);Check(details.Panel.Find("Selected football player/Selected PASSES").GetComponent<Text>().text==p.passes.ToString()&&details.Panel.Find("Selected football player/Selected INTERCEPTS").GetComponent<Text>().text==p.interceptions.ToString(),"selected football details displayed for "+p.id);}
   }
  }
  void CheckPanelBounds(ScoreboardDetails details,Vector2Int size){
   var corners=new Vector3[4];details.Panel.GetWorldCorners(corners);Check(corners[0].x>=Screen.safeArea.xMin&&corners[0].y>=Screen.safeArea.yMin&&corners[2].x<=Screen.safeArea.xMax&&corners[2].y<=Screen.safeArea.yMax,"expanded panel fits safe area "+size);var panel=new Rect(corners[0],corners[2]-corners[0]);
   foreach(var text in details.Panel.GetComponentsInChildren<Text>().Where(t=>new[]{"Team score","Selected GOALS","Selected ASSISTS","Selected PASSES","Selected INTERCEPTS"}.Contains(t.name))){Check(text.cachedTextGenerator.vertexCount>=4,"number renders "+text.name+" "+size);}
   foreach(var label in Hud.GetComponentsInChildren<LocalizedText>().Where(t=>!t.canvasRenderer.cull&&!string.IsNullOrWhiteSpace(t.text))){
    Check(label.HasUnclippedText(),"complete scoreboard caption "+label.name+" "+GameLocalization.Code+" "+size);
    if(label.name=="Player name"||label.name=="Selected player name")Check(label.UserContent,"player name stays user content "+label.name);
    else if(!label.UserContent)Check(label.text==GameLocalization.Translate(label.Source),"scoreboard caption follows language "+label.name);
   }
   if(sport==SportId.Football)foreach(var rect in FootballHUD.Instance.GetComponentsInChildren<RectTransform>().Where(r=>new[]{"Movement joystick","Kick button","Football dash button","Football pass button","Tackle button"}.Contains(r.name))){rect.GetWorldCorners(corners);Check(!panel.Overlaps(new Rect(corners[0],corners[2]-corners[0])),"football detail panel clears "+rect.name+" "+size);}
  }
  void ClickScore(Vector2 normalized){
   var button=Hud.Details.ToggleButton;var rect=(RectTransform)button.transform;var local=new Vector3(Mathf.Lerp(rect.rect.xMin,rect.rect.xMax,normalized.x),Mathf.Lerp(rect.rect.yMin,rect.rect.yMax,normalized.y));var point=RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(local));var data=new PointerEventData(EventSystem.current){position=point,button=PointerEventData.InputButton.Left,pointerId=-1};var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(data,hits);var hit=hits.FirstOrDefault();var target=hit.gameObject?ExecuteEvents.GetEventHandler<IPointerClickHandler>(hit.gameObject):null;Check(target==button.gameObject,"scoreboard hit target at "+normalized);if(target!=button.gameObject)return;data.pointerPressRaycast=hit;data.pointerCurrentRaycast=hit;ExecuteEvents.Execute(target,data,ExecuteEvents.pointerDownHandler);ExecuteEvents.Execute(target,data,ExecuteEvents.pointerUpHandler);ExecuteEvents.Execute(target,data,ExecuteEvents.pointerClickHandler);
  }
 }
}
#endif
