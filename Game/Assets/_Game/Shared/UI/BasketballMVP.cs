using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace WhatTheFish {
 [DefaultExecutionOrder(400)]
 public sealed partial class BasketballMVP:MonoBehaviour {
  public static BasketballMVP Instance {get;private set;}
  public static bool Presenting=>Instance&&Instance.Visible;
  public bool Visible=>panel&&panel.gameObject.activeSelf;
  RectTransform panel,content;Text nameText,result,scorePoints,assists,passes,interceptions,rating;BasketballPlate teamStripe,teamBadge;Text teamTitle;readonly Text[,] rows=new Text[10,5];GameObject hero,stats;uint shown=uint.MaxValue;float opened;bool priorBlocked;Vector2 displaySize;
  Camera stadiumCamera;Vector3 savedPosition;Quaternion savedRotation;float savedFov,savedNear;bool savedOrtho;readonly Dictionary<Renderer,bool> hidden=new();
  static readonly Color Gold=new Color32(224,100,36,255),Ink=new Color32(27,47,72,255),Cream=new Color32(255,248,233,255);
  public static BasketballMVP Create(){var go=new GameObject("Basketball match honours",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));var canvas=go.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=900;var scale=go.GetComponent<CanvasScaler>();scale.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scale.referenceResolution=new Vector2(1600,900);scale.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;return go.AddComponent<BasketballMVP>();}
  void Awake(){Instance=this;Build();}
  Text Label(Transform p,string text,float x,float y,float w,float h,int size,Color? color=null,TextAnchor align=TextAnchor.MiddleLeft){var t=CoveUI.Text(p,text,x,y,w,h,size,true,color??Ink,align);t.verticalOverflow=VerticalWrapMode.Overflow;return t;}
  void Build(){
   panel=CoveUI.Rect(transform,"MVP victory",0,0,1600,900);CoveUI.Stretch(panel);panel.gameObject.AddComponent<Image>().color=Color.clear;
   content=CoveUI.Rect(panel,"MVP safe composition",0,0,1520,840);content.anchorMin=content.anchorMax=content.pivot=Vector2.one*.5f;content.anchoredPosition=Vector2.zero;
   BasketballUIStyle.Plate(content,"Courtside scorecard",795,30,700,780,Ink,28,0);
   teamStripe=BasketballUIStyle.Plate(content,"Team colour rail",795,30,700,9,CoveUI.Sky,4,0);
   BasketballUIStyle.Plate(content,"Orange scoreboard header",821,56,648,76,Gold,16,0);
   Label(content,"FINAL BUZZER",843,65,265,55,26,Cream);
   result=Label(content,"",1090,65,355,55,27,Cream,TextAnchor.MiddleRight);
   CoveUI.Image(content,"Official game logo",Resources.Load<Sprite>("Menu/WhatTheFishLogo"),24,0,220,150);
   teamBadge=BasketballUIStyle.Plate(content,"Team award ribbon",95,744,550,64,Ink,20,0);
   teamTitle=Label(teamBadge.transform,"",0,0,550,64,26,Cream,TextAnchor.MiddleCenter);
   hero=CoveUI.Rect(content,"MVP award",835,157,620,540).gameObject;
   Label(hero.transform,"PLAYER OF THE GAME",0,0,560,35,25,Gold);
   var title=Label(hero.transform,"MVP",-6,32,400,115,108,Cream);title.fontStyle=FontStyle.Italic;
   var courtArt=CoveUI.Rect(hero.transform,"Basketball seams",464,32,130,130).gameObject.AddComponent<BasketballHonoursArt>();courtArt.raycastTarget=false;
   nameText=Label(hero.transform,"",0,158,620,65,34,Cream);nameText.resizeTextForBestFit=true;nameText.resizeTextMinSize=22;nameText.resizeTextMaxSize=34;nameText.supportRichText=false;
   scorePoints=Stat(hero.transform,"POINTS",0,245,284,86);assists=Stat(hero.transform,"ASSISTS",316,245,284,86);
   passes=Stat(hero.transform,"COMPLETED PASSES",0,402,284,44);interceptions=Stat(hero.transform,"PASS INTERCEPTIONS",316,402,284,44);
   rating=Label(hero.transform,"",0,520,620,30,19,Cream);
   stats=CoveUI.Rect(content,"Everyone's match stats",835,160,620,535).gameObject;Label(stats.transform,"BOX SCORE",0,0,620,60,43,Cream);
   string[] columns={"PLAYER","PTS","AST","PASS","INT"};float[] left={0,320,390,460,540};float[] widths={310,62,62,72,72};
   for(int c=0;c<5;c++){Label(stats.transform,columns[c],left[c],70,widths[c],33,20,Gold,c==0?TextAnchor.MiddleLeft:TextAnchor.MiddleCenter);for(int row=0;row<10;row++){var t=Label(stats.transform,"",left[c],110+row*40,widths[c],38,22,Cream,c==0?TextAnchor.MiddleLeft:TextAnchor.MiddleCenter);t.resizeTextForBestFit=true;t.resizeTextMinSize=14;t.resizeTextMaxSize=22;rows[row,c]=t;}}stats.SetActive(false);
   BasketballUIStyle.Button(content,"Toggle match stats","Box score",835,735,270,52,()=>{bool show=!stats.activeSelf;stats.SetActive(show);hero.SetActive(!show);},CoveUI.Sky,23);
   BuildCelebration();
   panel.gameObject.SetActive(false);
  }
  Text Stat(Transform p,string label,float x,float y,float w,int size){Label(p,label,x,y,w,34,20,Gold);return Label(p,"0",x,y+37,w,size+14,size,Cream);}
  void Update(){
   var match=BasketballMatch.Instance;bool finished=match&&match.Context&&match.Snapshot.phase==BasketballMatchPhase.Finished;
   if(!finished){if(Visible)Dismiss();if(!match||match.Snapshot.phase!=BasketballMatchPhase.Finished)shown=uint.MaxValue;return;}
   if(shown!=match.Snapshot.revision&&match.PlayerRecords.Any()){shown=match.Snapshot.revision;Show(match);}
   if(Visible){var size=new Vector2(Screen.width,Screen.height);if(size!=displaySize){displaySize=size;foreach(var text in panel.GetComponentsInChildren<Text>(true))text.SetAllDirty();}var safe=Screen.safeArea;var root=(RectTransform)transform;float scale=Mathf.Min(1,safe.width/Screen.width*root.rect.width/1520,safe.height/Screen.height*root.rect.height/840);content.localScale=Vector3.one*scale;content.anchoredPosition=new Vector2((safe.center.x/Screen.width-.5f)*root.rect.width,(safe.center.y/Screen.height-.5f)*root.rect.height);}
  }
  void Show(BasketballMatch match){
   if(Visible)Dismiss();var winner=match.MVP;var teamColor=winner.team==FootballTeam.A?new Color32(42,113,203,255):new Color32(207,67,60,255);teamStripe.color=teamBadge.color=teamColor;teamTitle.text=CharacterIdentity.TeamName(winner.team).ToUpperInvariant()+" / COURT MVP";opened=Time.unscaledTime;panel.gameObject.SetActive(true);hero.SetActive(true);stats.SetActive(false);
   priorBlocked=PlayerView.Instance&&PlayerView.Instance.ExplicitInputBlocked;if(PlayerView.Instance){PlayerView.Instance.ClearMatchInput();PlayerView.Instance.InputBlocked=true;}
   stadiumCamera=Camera.main;if(stadiumCamera){savedPosition=stadiumCamera.transform.position;savedRotation=stadiumCamera.transform.rotation;savedFov=stadiumCamera.fieldOfView;savedNear=stadiumCamera.nearClipPlane;savedOrtho=stadiumCamera.orthographic;}
   var s=match.Snapshot;result.text=(s.result==FootballMatchResult.Draw?"DRAW":s.result==FootballMatchResult.TeamA?"BLUE WINS":"RED WINS")+"  "+s.scoreA+" : "+s.scoreB;
   GameLocalization.UserContent(nameText);nameText.text="#"+winner.number+"  "+winner.name+"  /  "+GameLocalization.Translate(CharacterIdentity.TeamName(winner.team).ToUpperInvariant());
   scorePoints.text=winner.points.ToString();assists.text=winner.assists.ToString();passes.text=winner.passes.ToString();interceptions.text=winner.interceptions.ToString();rating.text=winner.Rating+" MVP RATING   /   POINTS + ASSISTS";
   var records=BasketballStatistics.Ranked(match.PlayerRecords).ToArray();
   for(int i=0;i<10;i++)for(int c=0;c<5;c++){var t=rows[i,c];if(c==0)GameLocalization.UserContent(t);t.supportRichText=false;t.text=i>=records.Length?"":c==0?"#"+records[i].number+" "+records[i].name:c==1?records[i].points.ToString():c==2?records[i].assists.ToString():c==3?records[i].passes.ToString():records[i].interceptions.ToString();if(c==0&&i<records.Length)t.color=records[i].team==FootballTeam.A?new Color(.48f,.77f,1):new Color(1,.58f,.48f);}
   BeginCelebration(match,winner,records);
  }
  void LateUpdate(){
   if(Visible)PresentCelebration();
  }
  public void Dismiss(){if(!Visible)return;EndCelebration();panel.gameObject.SetActive(false);foreach(var pair in hidden)if(pair.Key)pair.Key.forceRenderingOff=pair.Value;hidden.Clear();if(stadiumCamera){stadiumCamera.transform.SetPositionAndRotation(savedPosition,savedRotation);stadiumCamera.fieldOfView=savedFov;stadiumCamera.nearClipPlane=savedNear;stadiumCamera.orthographic=savedOrtho;}stadiumCamera=null;if(PlayerView.Instance){PlayerView.Instance.ClearMatchInput();PlayerView.Instance.InputBlocked=priorBlocked;}}
  void OnEnable(){Instance=this;}
  void OnDisable(){Dismiss();if(Instance==this)Instance=null;}
 }
}
