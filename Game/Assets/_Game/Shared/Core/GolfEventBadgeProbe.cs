#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace WhatTheFish {
 public sealed partial class GolfPolishProbe {
  IEnumerator EventBadgePresentation(){
   var app=AppRoot.Instance;var local=app.LocalAthlete;var helper=Instantiate(app.athletePrefab).GetComponent<Athlete>();helper.Setup();
   ulong id=GolfCartWorld.Key(local);bool ready=false;float until=Time.time+18;while(!ready&&Time.time<until){ready=match.StartTestMatch(new[]{(id,local,"You"),(101UL,helper,"Other")});if(!ready)yield return null;}Check(ready,"BADGE_MATCH_STARTED");yield return new WaitForSeconds(.8f);
   var hud=GolfEventHUD.Instance;Check(hud&&Resources.Load<Texture2D>("GolfUI/Events/BadgeSheet"),"SUPPLIED_BADGE_ATLAS_LOADED");
   foreach(var kind in GolfEventSchedule.RandomKinds.Where(k=>!GolfHazardRules.IsHazard(k))){int i=(int)kind-1;
    match.ReviewEventHotkey((KeyCode)((int)KeyCode.F1+i));yield return null;yield return new WaitForEndOfFrame();
    var art=hud.Card.GetComponentsInChildren<GolfEventBadgeGraphic>().Single(a=>a.enabled);Check(art.Badge==i,"CORRESPONDING_BADGE_"+i);
    var label=art.GetComponentInChildren<Text>();string first=label.text;Check(first.EndsWith("s"),"LIVE_SECONDS_"+i);Canvas.ForceUpdateCanvases();Check(label.cachedTextGenerator.vertexCount>4,"SECONDS_GLYPHS_RENDER_"+i);CaptureUI("badge-"+i,null,0,0,0,false);
    yield return new WaitForSeconds(1.1f);Check(label.text!=first,"SECONDS_DECREMENT_"+i);
   }
   match.ReviewEventHotkey(KeyCode.F6);yield return null;yield return new WaitForEndOfFrame();Check(hud.Card.GetComponentsInChildren<GolfEventBadgeGraphic>().Count(a=>a.enabled)==2,"RUSH_AND_WEATHER_BOTH_VISIBLE");CaptureUI("badge-rush",null,0,0,0,false);
   var ball=match.Ball(helper);for(int hole=1;hole<=5;hole++){match.EnterHole(ball,hole);yield return new WaitForSeconds(.15f);}yield return new WaitForSeconds(1.6f);
   var countdown=FindFirstObjectByType<GolfFinalCountdown>();Check(countdown&&countdown.Visible,"FINISH_COUNTDOWN_VISIBLE");
   yield return new WaitForEndOfFrame();var corners=new Vector3[4];countdown.Card.GetWorldCorners(corners);float top=hud.Card.parent.InverseTransformPoint(corners[1]).y;float eventBottom=hud.Card.parent.GetComponent<RectTransform>().rect.yMax-20-hud.StackHeight;Check(top<=eventBottom+.5f,"FINAL_COUNTDOWN_DOCKS_BELOW_EVENTS");Check(Vector2.Distance(countdown.RenderedDigitSize,Vector2.one*50)<.1f,"FINAL_DIGITS_RETAIN_50_BY_50");CaptureUI("badge-final-countdown",null,0,0,0,false);
   match.ReviewEventHotkey(KeyCode.F7);yield return null;yield return null;Check(hud.StackHeight==0&&!hud.Card.gameObject.activeSelf,"EVENT_STACK_HIDES_WHEN_CLEARED");match.ClearMatch();Destroy(helper.gameObject);
  }
 }
}
#endif
