#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace WhatTheFish {
 public sealed partial class LocalizationReview {
  static Rect GlyphBounds(Text text,Transform relative){
   var mesh=text.canvasRenderer.GetMesh();if(!mesh||mesh.vertexCount==0)return new Rect();
   Vector2 min=Vector2.positiveInfinity,max=Vector2.negativeInfinity;
   foreach(var p in mesh.vertices){Vector2 q=relative.InverseTransformPoint(text.transform.TransformPoint(p));min=Vector2.Min(min,q);max=Vector2.Max(max,q);}
   return Rect.MinMaxRect(min.x,min.y,max.x,max.y);
  }
  static bool Contains(Rect outer,Rect inner,float pad=2)=>inner.width<=0||inner.height<=0||inner.xMin>=outer.xMin-pad&&inner.xMax<=outer.xMax+pad&&inner.yMin>=outer.yMin-pad&&inner.yMax<=outer.yMax+pad;
  void CheckNameSelection(InputField field){
   var caret=field.GetComponentsInChildren<CanvasRenderer>().FirstOrDefault(c=>c.name.EndsWith("Input Caret",StringComparison.Ordinal));
   Check(caret&&caret.GetMesh()&&caret.GetMesh().vertexCount>0,"selected name has selection geometry");if(!caret||!caret.GetMesh())return;
   var frame=field.GetComponent<RectTransform>();Vector2 min=Vector2.positiveInfinity,max=Vector2.negativeInfinity;
   foreach(var p in caret.GetMesh().vertices){Vector2 q=frame.InverseTransformPoint(caret.transform.TransformPoint(p));min=Vector2.Min(min,q);max=Vector2.Max(max,q);}
   var selection=Rect.MinMaxRect(min.x,min.y,max.x,max.y);var ink=GlyphBounds(field.textComponent,frame);
   Check(Contains(frame.rect,selection)&&selection.Overlaps(ink),"name highlight stays inside input behind the name");
  }
  IEnumerator TutorialLayouts(SportId sport,string code){
   var tutorial=FindObjectsByType<IslandTutorialUI>(FindObjectsSortMode.None).FirstOrDefault(t=>t.Sport==sport);
   Check(tutorial,"illustrated tutorial exists "+sport);if(!tutorial)yield break;
   tutorial.Open();yield return null;
   for(int step=0;step<4;step++){tutorial.Show(step);yield return new WaitForSecondsRealtime(.2f);yield return Capture(code+"-"+sport+"-tutorial-"+step);
    var modal=tutorial.GetComponentInChildren<Canvas>();Check(modal&&modal.overrideSorting&&modal.sortingOrder>100,"tutorial draws above ranking and finish clock "+sport);
    if(sport==SportId.Golf&&step==2){var captions=tutorial.GetComponentsInChildren<LocalizedArtwork>(true);Check(captions.Length==5,"all five tutorial event illustrations have localized lettering");Check(captions.All(c=>GameLocalization.English?!c.Caption.gameObject.activeSelf:c.Caption.gameObject.activeSelf&&GameLocalization.Source(c.Caption)!=c.Caption.text),"tutorial event illustrations follow current language "+code);}
   }
   tutorial.Close();yield return null;
   if(sport==SportId.Fishing)yield return FishingResultLayouts(code);
  }
  IEnumerator FishingResultLayouts(string code){
   var game=FishingGame.Instance;var hud=FishingHUD.Instance;var saved=game.State.Match;var players=game.State.Players.ToArray();var fish=game.State.Fish.ToArray();bool prior=game.ReviewSnapshot;game.ReviewSnapshot=true;
   foreach(FishingMode mode in Enum.GetValues(typeof(FishingMode))){
    var ended=saved;ended.phase=FishingRoundPhase.Ended;ended.outcome=mode==FishingMode.Crew?FishingOutcome.Success:FishingOutcome.Finished;ended.mode=mode;ended.round+=1000+(uint)mode;ended.cupFinished=mode==FishingMode.Cup;ended.crewMask=7;
    hud.Results.ClearContext();game.State.Receive(ended,players,fish);yield return new WaitForSecondsRealtime(.3f);Check(hud.Results.Visible,"fishing results fixture opens "+mode);yield return Capture(code+"-Fishing-results-"+mode);var gear=hud.IslandMenu.GearRect;Check(!Hits(gear),"fishing result shade intercepts settings pointer "+mode);hud.Results.Close();yield return null;Check(Hits(gear),"closing fishing result restores settings pointer "+mode);
   }
   game.State.Receive(saved,players,fish);hud.Results.ClearContext();PostMatchFlow.Instance?.Close();game.ReviewSnapshot=prior;yield return null;
  }
  static bool Hits(RectTransform target){var pointer=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,target.TransformPoint(target.rect.center))};var hits=new System.Collections.Generic.List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);return hits.Count>0&&(hits[0].gameObject.transform==target||hits[0].gameObject.transform.IsChildOf(target));}
  IEnumerator GolfSignLayouts(string code){
   var signs=GolfMatchManager.Instance.Course.GetComponentsInChildren<Text>(true).Where(t=>t.name=="Hole and par").ToArray();Check(signs.Length==5,"five translated tee signs");Canvas.ForceUpdateCanvases();
   foreach(var sign in signs){Check(sign.cachedTextGenerator.lines.Count==2,"hole/par and course name each retain one line "+sign.transform.parent.name+" "+code);var settings=sign.GetGenerationSettings(sign.rectTransform.rect.size);settings.verticalOverflow=VerticalWrapMode.Overflow;using(var full=new TextGenerator()){full.Populate(sign.text,settings);Check(full.verts.Count==sign.cachedTextGenerator.verts.Count,"tee course name is not truncated "+code);}}
   if(signs.Length>0){var sign=signs[0];var go=new GameObject("Sign layout evidence camera");var cam=go.AddComponent<Camera>();cam.CopyFrom(Camera.main);cam.enabled=false;go.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();cam.fieldOfView=38;cam.transform.position=sign.transform.position-sign.transform.forward*5+Vector3.up*.25f;cam.transform.LookAt(sign.transform.position);var target=RenderTexture.GetTemporary(1100,700,24);var active=RenderTexture.active;cam.targetTexture=target;cam.Render();RenderTexture.active=target;var pic=new Texture2D(1100,700,TextureFormat.RGB24,false);pic.ReadPixels(new Rect(0,0,1100,700),0,0);pic.Apply();File.WriteAllBytes(Path.Combine(folder,code+"-tee-sign.png"),pic.EncodeToPNG());Destroy(pic);RenderTexture.active=active;cam.targetTexture=null;RenderTexture.ReleaseTemporary(target);Destroy(go);}
   yield return null;
  }
  void AuditLayout(string capture){
   var texts=FindObjectsByType<LocalizedText>(FindObjectsSortMode.None).Where(t=>t.isActiveAndEnabled&&t.color.a>.01f&&!t.GetComponentsInParent<CanvasGroup>().Any(g=>g.alpha<=.01f)&&t.canvas&&t.canvas.renderMode!=RenderMode.WorldSpace&&!string.IsNullOrWhiteSpace(t.text)&&t.canvasRenderer.GetMesh()?.vertexCount>0).ToArray();
   foreach(var t in texts){
    var bounds=GlyphBounds(t,t.transform);float tolerance=Mathf.Max(2,t.fontSize*.15f);
    if(!Contains(t.rectTransform.rect,bounds,tolerance))File.AppendAllText(Path.Combine(folder,"layout-findings.tsv"),capture+"\tOUTSIDE\t"+t.transform.parent.name+"\t"+t.Source.Replace("\n","\\n")+"\n");
    if(t.verticalOverflow==VerticalWrapMode.Truncate&&!t.UserContent&&!t.HasUnclippedText())File.AppendAllText(Path.Combine(folder,"layout-findings.tsv"),capture+"\tTRUNCATED\t"+t.transform.parent.name+"\t"+t.Source.Replace("\n","\\n")+"\n");
   }
   foreach(var group in texts.GroupBy(t=>t.transform.parent)){
    var a=group.ToArray();for(int i=0;i<a.Length;i++)for(int j=i+1;j<a.Length;j++){
     var x=GlyphBounds(a[i],group.Key);var y=GlyphBounds(a[j],group.Key);x.xMin+=2;x.xMax-=2;x.yMin+=2;x.yMax-=2;y.xMin+=2;y.xMax-=2;y.yMin+=2;y.yMax-=2;
     if(x.width>0&&y.width>0&&x.Overlaps(y))File.AppendAllText(Path.Combine(folder,"layout-findings.tsv"),capture+"\tOVERLAP\t"+group.Key.name+"\t"+a[i].Source.Replace("\n","\\n")+" / "+a[j].Source.Replace("\n","\\n")+"\n");
    }
   }
   var columns=FindObjectsByType<LocalizedArtwork>(FindObjectsSortMode.None).Where(a=>a.name=="Leaderboard art Columns"&&a.Caption.gameObject.activeInHierarchy).ToArray();if(columns.Length==5)Check(columns.Select(a=>a.Caption.fontSize).Distinct().Count()==1,"translated leaderboard headers use identical font size "+capture);
   var target=FindFirstObjectByType<GolfTargetIndicator>();if(target&&target.TargetCard.gameObject.activeInHierarchy){
    var labels=target.TargetCard.GetComponentsInChildren<Text>();var boxes=labels.Select(t=>GlyphBounds(t,target.TargetCard)).ToArray();
    Check(boxes.All(b=>Contains(target.TargetCard.rect,b)),"golf hole distance glyphs inside original card "+capture);
    if(boxes.Length==3){float a=boxes[1].xMin-boxes[0].xMax,b=boxes[2].xMin-boxes[1].xMax;Check(a>=8&&Mathf.Abs(a-b)<2,"golf hole dot distance gaps match "+capture);}
   }
  }
 }
}
#endif
