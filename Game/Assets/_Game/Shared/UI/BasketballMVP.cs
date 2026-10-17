using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace WhatTheFish {
 [DefaultExecutionOrder(400)]
 public sealed class BasketballMVP:MonoBehaviour {
  public static BasketballMVP Instance {get;private set;}
  public static bool Presenting=>Instance&&Instance.Visible;
  public bool Visible=>panel&&panel.gameObject.activeSelf;
  RectTransform panel,content;Text nameText,result,scorePoints,assists,passes,interceptions,rating;BasketballPlate teamStripe,teamBadge;Text teamTitle;readonly Text[,] rows=new Text[10,5];GameObject hero,stats;Image portrait;uint shown=uint.MaxValue;float opened;bool priorBlocked;
  Camera stadiumCamera;Vector3 savedPosition;Quaternion savedRotation;float savedFov,savedNear;bool savedOrtho;readonly Dictionary<Renderer,bool> hidden=new();
  static readonly Color Gold=new Color32(224,100,36,255),Ink=new Color32(27,47,72,255),Cream=new Color32(255,248,233,255);
  public static BasketballMVP Create(){var go=new GameObject("Basketball match honours",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));var canvas=go.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=900;var scale=go.GetComponent<CanvasScaler>();scale.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scale.referenceResolution=new Vector2(1600,900);scale.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;return go.AddComponent<BasketballMVP>();}
  void Awake(){Instance=this;Build();}
  Text Label(Transform p,string text,float x,float y,float w,float h,int size,Color? color=null,TextAnchor align=TextAnchor.MiddleLeft){var t=CoveUI.Text(p,text,x,y,w,h,size,true,color??Ink,align);t.verticalOverflow=VerticalWrapMode.Overflow;return t;}
  void Build(){
   panel=CoveUI.Rect(transform,"MVP victory",0,0,1600,900);CoveUI.Stretch(panel);panel.gameObject.AddComponent<Image>().color=new Color(1,.93f,.80f,.04f);
   content=CoveUI.Rect(panel,"MVP safe composition",0,0,1520,840);content.anchorMin=content.anchorMax=content.pivot=Vector2.one*.5f;content.anchoredPosition=Vector2.zero;
   BasketballUIStyle.Plate(content,"Courtside scorecard",630,18,870,802,new Color(1,.975f,.93f,.94f),35,2);
   teamStripe=BasketballUIStyle.Plate(content,"Team colour rail",630,18,13,802,CoveUI.Sky,6,0);
   var courtArt=CoveUI.Rect(content,"Basketball court line art",25,145,565,590).gameObject.AddComponent<BasketballHonoursArt>();courtArt.raycastTarget=false;
   teamBadge=BasketballUIStyle.Plate(content,"Team award ribbon",52,760,506,55,Ink,16,0);
   teamTitle=Label(teamBadge.transform,"",0,0,506,55,28,Cream,TextAnchor.MiddleCenter);
   CoveUI.Image(content,"Official game logo",Resources.Load<Sprite>("Menu/WhatTheFishLogo"),24,0,220,150);
   var p=CoveUI.Rect(content,"MVP player illustration",10,68,620,680);portrait=p.gameObject.AddComponent<Image>();portrait.preserveAspect=true;portrait.raycastTarget=false;
   BasketballUIStyle.Art(content,BasketballIcon.Ball,458,598,134,134);
   Label(content,"F I N A L   B U Z Z E R",675,45,360,32,23,Gold);result=Label(content,"",1040,40,400,44,27,Ink,TextAnchor.MiddleRight);
   hero=CoveUI.Rect(content,"MVP award",675,103,770,610).gameObject;
   Label(hero.transform,"PLAYER OF THE GAME",2,0,760,35,27,Gold);var title=Label(hero.transform,"MVP",-7,32,760,136,136);title.fontStyle=FontStyle.Italic;
   nameText=Label(hero.transform,"",2,180,760,59,43);nameText.resizeTextForBestFit=true;nameText.resizeTextMinSize=29;nameText.resizeTextMaxSize=43;nameText.supportRichText=false;
   scorePoints=Stat(hero.transform,"POINTS",0,258,350,132);assists=Stat(hero.transform,"ASSISTS",402,258,350,132);
   passes=Stat(hero.transform,"COMPLETED PASSES",0,465,350,58);interceptions=Stat(hero.transform,"PASS INTERCEPTIONS",402,465,350,58);
   rating=Label(hero.transform,"",0,591,770,30,21,Ink);
   stats=CoveUI.Rect(content,"Everyone's match stats",675,112,770,600).gameObject;Label(stats.transform,"MATCH STATS",0,0,770,60,49);
   string[] columns={"PLAYER","PTS","AST","PASS","INT"};float[] left={0,388,465,542,655};float[] widths={375,70,70,105,105};
   for(int c=0;c<5;c++){Label(stats.transform,columns[c],left[c],80,widths[c],33,23,Gold,c==0?TextAnchor.MiddleLeft:TextAnchor.MiddleCenter);for(int r=0;r<10;r++)rows[r,c]=Label(stats.transform,"",left[c],123+r*43,widths[c],40,24,null,c==0?TextAnchor.MiddleLeft:TextAnchor.MiddleCenter);}stats.SetActive(false);
   BasketballUIStyle.Button(content,"Toggle match stats","Match stats",675,746,350,58,()=>{bool show=!stats.activeSelf;stats.SetActive(show);hero.SetActive(!show);},CoveUI.Mint,24);
   BasketballUIStyle.Button(content,"Close MVP","Next adventure",1050,746,385,58,Dismiss,CoveUI.Cream,24);
   panel.gameObject.SetActive(false);
  }
  Text Stat(Transform p,string label,float x,float y,float w,int size){Label(p,label,x,y,w,34,25,Gold);return Label(p,"0",x,y+37,w,size+14,size);}
  void Update(){
   var match=BasketballMatch.Instance;bool finished=match&&match.Context&&match.Snapshot.phase==BasketballMatchPhase.Finished;
   if(!finished){if(Visible)Dismiss();if(!match||match.Snapshot.phase!=BasketballMatchPhase.Finished)shown=uint.MaxValue;return;}
   if(shown!=match.Snapshot.revision&&match.PlayerRecords.Any()){shown=match.Snapshot.revision;Show(match);}
   if(Visible){var safe=Screen.safeArea;var root=(RectTransform)transform;float scale=Mathf.Min(1,safe.width/Screen.width*root.rect.width/1520,safe.height/Screen.height*root.rect.height/840);content.localScale=Vector3.one*scale;content.anchoredPosition=new Vector2((safe.center.x/Screen.width-.5f)*root.rect.width,(safe.center.y/Screen.height-.5f)*root.rect.height);portrait.rectTransform.localScale=Vector3.one*Mathf.Lerp(.96f,1,Mathf.SmoothStep(0,1,(Time.unscaledTime-opened)/.65f));}
  }
  void Show(BasketballMatch match){
   var winner=match.MVP;portrait.sprite=Resources.Load<Sprite>("Football/MVP/mvp-"+Mathf.Clamp(winner.number,1,10).ToString("00"));var teamColor=winner.team==FootballTeam.A?new Color32(42,113,203,255):new Color32(207,67,60,255);teamStripe.color=teamBadge.color=teamColor;teamTitle.text=CharacterIdentity.TeamName(winner.team).ToUpperInvariant()+" / COURT MVP";opened=Time.unscaledTime;panel.gameObject.SetActive(true);hero.SetActive(true);stats.SetActive(false);
   priorBlocked=PlayerView.Instance&&PlayerView.Instance.InputBlocked;if(PlayerView.Instance){PlayerView.Instance.ClearMatchInput();PlayerView.Instance.InputBlocked=true;}
   stadiumCamera=Camera.main;if(stadiumCamera){savedPosition=stadiumCamera.transform.position;savedRotation=stadiumCamera.transform.rotation;savedFov=stadiumCamera.fieldOfView;savedNear=stadiumCamera.nearClipPlane;savedOrtho=stadiumCamera.orthographic;}
   foreach(var actor in Athlete.Active)if(actor&&actor.visual)foreach(var r in actor.visual.GetComponentsInChildren<Renderer>()){hidden[r]=r.forceRenderingOff;r.forceRenderingOff=true;}
   var s=match.Snapshot;result.text=(s.result==FootballMatchResult.Draw?"DRAW":s.result==FootballMatchResult.TeamA?"BLUE WINS":"RED WINS")+"  "+s.scoreA+" : "+s.scoreB;
   nameText.text="#"+winner.number+"  "+winner.name+"  /  "+CharacterIdentity.TeamName(winner.team).ToUpperInvariant();
   scorePoints.text=winner.points.ToString();assists.text=winner.assists.ToString();passes.text=winner.passes.ToString();interceptions.text=winner.interceptions.ToString();rating.text=winner.Rating+" MVP RATING   /   POINTS + ASSISTS";
   var records=BasketballStatistics.Ranked(match.PlayerRecords).ToArray();
   for(int i=0;i<10;i++)for(int c=0;c<5;c++){var t=rows[i,c];t.supportRichText=false;t.text=i>=records.Length?"":c==0?"#"+records[i].number+" "+records[i].name:c==1?records[i].points.ToString():c==2?records[i].assists.ToString():c==3?records[i].passes.ToString():records[i].interceptions.ToString();if(c==0&&i<records.Length)t.color=records[i].team==FootballTeam.A?new Color(.12f,.30f,.65f):new Color(.68f,.20f,.16f);}
  }
  void LateUpdate(){
   if(!Visible)return;
   var ball=BasketballBall.Active;if(!stadiumCamera||!ball||!ball.transform.parent)return;var court=ball.transform.parent;
   var position=court.TransformPoint(new Vector3(-5.6f,1.55f,-7.4f));var focus=court.TransformPoint(new Vector3(1.8f,3.2f,12.5f));
   stadiumCamera.orthographic=false;stadiumCamera.fieldOfView=68;stadiumCamera.nearClipPlane=.15f;stadiumCamera.transform.SetPositionAndRotation(position,Quaternion.LookRotation(focus-position,Vector3.up));
  }
  public void Dismiss(){if(!Visible)return;panel.gameObject.SetActive(false);portrait.sprite=null;foreach(var pair in hidden)if(pair.Key)pair.Key.forceRenderingOff=pair.Value;hidden.Clear();if(stadiumCamera){stadiumCamera.transform.SetPositionAndRotation(savedPosition,savedRotation);stadiumCamera.fieldOfView=savedFov;stadiumCamera.nearClipPlane=savedNear;stadiumCamera.orthographic=savedOrtho;}stadiumCamera=null;if(PlayerView.Instance)PlayerView.Instance.InputBlocked=priorBlocked;}
  void OnDisable(){Dismiss();if(Instance==this)Instance=null;}
 }
}
