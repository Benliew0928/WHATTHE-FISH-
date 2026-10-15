using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace WhatTheFish {
 [DefaultExecutionOrder(400)]
 public sealed class FootballMVP:MonoBehaviour {
  public static FootballMVP Instance {get;private set;}
  public static bool Presenting=>Instance&&Instance.Visible;
  public bool Visible=>panel&&panel.gameObject.activeSelf;
  RectTransform panel,content;Text nameText,result,goals,assists,passes,interceptions,points;readonly Text[,] rows=new Text[10,5];GameObject hero,stats;Image portrait;uint shown=uint.MaxValue;float opened;bool priorBlocked;
  Camera stadiumCamera;Vector3 savedPosition;Quaternion savedRotation;float savedFov,savedNear;bool savedOrtho;readonly Dictionary<Renderer,bool> hidden=new();
  static readonly Color Gold=new Color32(215,155,41,255),Ink=new Color32(17,63,60,255),Cream=new Color32(255,247,226,255);
  public static FootballMVP Create(){var go=new GameObject("Football match honours",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));var canvas=go.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=900;var scale=go.GetComponent<CanvasScaler>();scale.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scale.referenceResolution=new Vector2(1600,900);scale.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;return go.AddComponent<FootballMVP>();}
  void Awake(){Instance=this;Build();}
  Text Label(Transform p,string text,float x,float y,float w,float h,int size,Color? color=null,TextAnchor align=TextAnchor.MiddleLeft){var t=CoveUI.Text(p,text,x,y,w,h,size,true,color??Ink,align);t.verticalOverflow=VerticalWrapMode.Overflow;return t;}
  void Build(){
   panel=CoveUI.Rect(transform,"MVP victory",0,0,1600,900);CoveUI.Stretch(panel);panel.gameObject.AddComponent<Image>().color=new Color(1,.96f,.84f,.06f);
   content=CoveUI.Rect(panel,"MVP safe composition",0,0,1520,840);content.anchorMin=content.anchorMax=content.pivot=Vector2.one*.5f;content.anchoredPosition=Vector2.zero;
   BasketballUIStyle.Plate(content,"Award ivory panel",630,18,870,802,new Color(1,.975f,.90f,.94f),35,2);
   CoveUI.Image(content,"Official game logo",Resources.Load<Sprite>("Menu/WhatTheFishLogo"),24,0,220,150);
   var p=CoveUI.Rect(content,"MVP player illustration",10,68,620,730);portrait=p.gameObject.AddComponent<Image>();portrait.preserveAspect=true;portrait.raycastTarget=false;
   Label(content,"F U L L   T I M E",675,45,360,32,23,Gold);result=Label(content,"",1040,40,400,44,27,Ink,TextAnchor.MiddleRight);
   hero=CoveUI.Rect(content,"MVP award",675,103,770,610).gameObject;
   Label(hero.transform,"MOST VALUABLE PLAYER",2,0,760,35,27,Gold);var title=Label(hero.transform,"MVP",-7,32,760,136,136);title.fontStyle=FontStyle.Italic;
   nameText=Label(hero.transform,"",2,180,760,59,43);nameText.resizeTextForBestFit=true;nameText.resizeTextMinSize=29;nameText.resizeTextMaxSize=43;nameText.supportRichText=false;
   goals=Stat(hero.transform,"GOALS",0,258,350,132);assists=Stat(hero.transform,"ASSISTS",402,258,350,132);
   passes=Stat(hero.transform,"COMPLETED PASSES",0,465,350,58);interceptions=Stat(hero.transform,"INTERCEPTIONS",402,465,350,58);
   points=Label(hero.transform,"",0,591,770,30,21,Ink);
   stats=CoveUI.Rect(content,"Everyone's match stats",675,112,770,600).gameObject;Label(stats.transform,"MATCH STATS",0,0,770,60,49);
   string[] columns={"PLAYER","G","A","PASS","INT"};float[] left={0,388,465,542,655};float[] widths={375,70,70,105,105};
   for(int c=0;c<5;c++){Label(stats.transform,columns[c],left[c],80,widths[c],33,23,Gold,c==0?TextAnchor.MiddleLeft:TextAnchor.MiddleCenter);for(int r=0;r<10;r++)rows[r,c]=Label(stats.transform,"",left[c],123+r*43,widths[c],40,24,null,c==0?TextAnchor.MiddleLeft:TextAnchor.MiddleCenter);}stats.SetActive(false);
   FootballUIStyle.Button(content,"Toggle match stats","Match stats",675,746,350,58,()=>{bool show=!stats.activeSelf;stats.SetActive(show);hero.SetActive(!show);},CoveUI.Mint,24);
   FootballUIStyle.Button(content,"Close MVP","Next adventure",1050,746,385,58,Dismiss,CoveUI.Cream,24);
   panel.gameObject.SetActive(false);
  }
  Text Stat(Transform p,string label,float x,float y,float w,int size){Label(p,label,x,y,w,34,25,Gold);return Label(p,"0",x,y+37,w,size+14,size);}
  void Update(){
   var match=FootballMatch.Instance;bool finished=match&&match.Context&&match.Snapshot.phase==FootballMatchPhase.Finished&&!match.GoalSequence&&!FootballReplay.Presenting;
   if(!finished){if(Visible)Dismiss();if(!match||match.Snapshot.phase!=FootballMatchPhase.Finished)shown=uint.MaxValue;return;}
   if(shown!=match.Snapshot.revision&&match.PlayerRecords.Any()){shown=match.Snapshot.revision;Show(match);}
   if(Visible){var safe=Screen.safeArea;var root=(RectTransform)transform;float scale=Mathf.Min(1,safe.width/Screen.width*root.rect.width/1520,safe.height/Screen.height*root.rect.height/840);content.localScale=Vector3.one*scale;content.anchoredPosition=new Vector2((safe.center.x/Screen.width-.5f)*root.rect.width,(safe.center.y/Screen.height-.5f)*root.rect.height);portrait.rectTransform.localScale=Vector3.one*Mathf.Lerp(.96f,1,Mathf.SmoothStep(0,1,(Time.unscaledTime-opened)/.65f));}
  }
  void Show(FootballMatch match){
   var winner=match.MVP;portrait.sprite=Resources.Load<Sprite>("Football/MVP/mvp-"+Mathf.Clamp(winner.number,1,10).ToString("00"));opened=Time.unscaledTime;panel.gameObject.SetActive(true);hero.SetActive(true);stats.SetActive(false);
   priorBlocked=PlayerView.Instance&&PlayerView.Instance.InputBlocked;if(PlayerView.Instance){PlayerView.Instance.ClearMatchInput();PlayerView.Instance.InputBlocked=true;}
   stadiumCamera=Camera.main;if(stadiumCamera){savedPosition=stadiumCamera.transform.position;savedRotation=stadiumCamera.transform.rotation;savedFov=stadiumCamera.fieldOfView;savedNear=stadiumCamera.nearClipPlane;savedOrtho=stadiumCamera.orthographic;}
   foreach(var actor in Athlete.Active)if(actor&&actor.visual)foreach(var r in actor.visual.GetComponentsInChildren<Renderer>()){hidden[r]=r.forceRenderingOff;r.forceRenderingOff=true;}
   var s=match.Snapshot;result.text=(s.result==FootballMatchResult.Draw?"DRAW":s.result==FootballMatchResult.TeamA?"BLUE WINS":"RED WINS")+"  "+s.scoreA+" : "+s.scoreB;
   nameText.text="#"+winner.number+"  "+winner.name+"  /  "+CharacterIdentity.TeamName(winner.team).ToUpperInvariant();
   goals.text=winner.goals.ToString();assists.text=winner.assists.ToString();passes.text=winner.passes.ToString();interceptions.text=winner.interceptions.ToString();points.text=winner.Points+" MVP POINTS   /   2 PER GOAL + 1 PER ASSIST";
   var records=match.PlayerRecords.OrderByDescending(r=>r.Points).ThenByDescending(r=>r.goals).ThenByDescending(r=>r.passes).ThenBy(r=>r.id).ToArray();
   for(int i=0;i<10;i++)for(int c=0;c<5;c++){var t=rows[i,c];t.supportRichText=false;t.text=i>=records.Length?"":c==0?"#"+records[i].number+" "+records[i].name:c==1?records[i].goals.ToString():c==2?records[i].assists.ToString():c==3?records[i].passes.ToString():records[i].interceptions.ToString();if(c==0&&i<records.Length)t.color=records[i].team==FootballTeam.A?new Color(.12f,.30f,.65f):new Color(.68f,.20f,.16f);}
  }
  void LateUpdate(){
   if(!Visible)return;if(FootballHUD.Instance)FootballHUD.Instance.SetReplayHidden(true);
   var ball=FootballBall.Instance;if(!stadiumCamera||!ball||!ball.Pitch)return;var b=ball.PitchBounds;
   var position=new Vector3(b.center.x-b.extents.x*.38f,b.max.y+1.55f,b.center.z-b.extents.z*.28f);
   var focus=new Vector3(b.center.x+b.extents.x*.20f,b.max.y+2.0f,b.max.z+8);
   stadiumCamera.orthographic=false;stadiumCamera.fieldOfView=64;stadiumCamera.nearClipPlane=.15f;stadiumCamera.transform.SetPositionAndRotation(ball.Pitch.TransformPoint(position),Quaternion.LookRotation(ball.Pitch.TransformPoint(focus)-ball.Pitch.TransformPoint(position),Vector3.up));
  }
  public void Dismiss(){if(!Visible)return;panel.gameObject.SetActive(false);portrait.sprite=null;foreach(var pair in hidden)if(pair.Key)pair.Key.forceRenderingOff=pair.Value;hidden.Clear();if(stadiumCamera){stadiumCamera.transform.SetPositionAndRotation(savedPosition,savedRotation);stadiumCamera.fieldOfView=savedFov;stadiumCamera.nearClipPlane=savedNear;stadiumCamera.orthographic=savedOrtho;}stadiumCamera=null;if(FootballHUD.Instance)FootballHUD.Instance.SetReplayHidden(false);if(PlayerView.Instance)PlayerView.Instance.InputBlocked=priorBlocked;}
  void OnDisable(){Dismiss();if(Instance==this)Instance=null;}
 }
}
