using UnityEngine;
using UnityEngine.UI;
namespace WhatTheFish {
 public sealed class FootballReplayHUD:MonoBehaviour {
  RectTransform plate,safe;Text score,replay,time,kickoff,subtitle;Image logo;FootballSlashGraphic slash;
  static readonly Color Navy=new Color32(12,35,46,255),Cream=new Color32(255,244,216,255);
  public static FootballReplayHUD Create(){
   var go=new GameObject("Broadcast Slash replay",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler));var canvas=go.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=1000;
   var scale=go.GetComponent<CanvasScaler>();scale.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scale.referenceResolution=new Vector2(1600,900);scale.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
   var h=go.AddComponent<FootballReplayHUD>();h.Build();return h;
  }
  static RectTransform Stretch(Transform parent,string name){var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;return r;}
  static Text Label(Transform parent,string value,Vector2 anchor,Vector2 size,Vector2 offset,int fontSize,TextAnchor align=TextAnchor.MiddleCenter){
   var go=new GameObject(value,typeof(RectTransform),typeof(Text));var r=go.GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=anchor;r.sizeDelta=size;r.anchoredPosition=offset;
   var t=go.GetComponent<Text>();t.text=value;t.font=SportsUIStyle.DisplayFont;t.fontSize=fontSize;t.color=Cream;t.alignment=align;t.raycastTarget=false;t.horizontalOverflow=HorizontalWrapMode.Overflow;t.verticalOverflow=VerticalWrapMode.Overflow;return t;
  }
  void Build(){
   safe=Stretch(transform,"Replay safe area");
   replay=Label(safe,"●  REPLAY",new Vector2(0,1),new Vector2(240,55),new Vector2(160,-58),27,TextAnchor.MiddleLeft);
   var tag=replay.gameObject.AddComponent<Outline>();tag.effectColor=Navy;tag.effectDistance=new Vector2(2,-2);
   time=Label(safe,"",new Vector2(1,0),new Vector2(260,50),new Vector2(-170,46),23,TextAnchor.MiddleRight);time.gameObject.AddComponent<Outline>().effectColor=Navy;
   kickoff=Label(safe,"",new Vector2(.5f,.19f),new Vector2(1200,100),Vector2.zero,29);kickoff.gameObject.AddComponent<Outline>().effectColor=Navy;
   plate=Stretch(transform,"Full-screen diagonal wipe");slash=plate.gameObject.AddComponent<FootballSlashGraphic>();slash.raycastTarget=false;
   // A fixed design field keeps illustration, type and logo together on wide phones.
   var content=Stretch(plate,"Broadcast title safe area");content.anchorMin=content.anchorMax=new Vector2(.5f,.5f);content.sizeDelta=new Vector2(1600,900);
   var art=Stretch(content,"Football celebration illustration").gameObject.AddComponent<FootballReplayIllustration>();art.raycastTarget=false;
   Label(content,"F O O T B A L L   /   R E P L A Y",new Vector2(.665f,.70f),new Vector2(650,60),Vector2.zero,25);
   var title=Label(content,"GOAL",new Vector2(.66f,.52f),new Vector2(740,230),Vector2.zero,210);title.fontStyle=FontStyle.Italic;var shadow=title.gameObject.AddComponent<Shadow>();shadow.effectColor=new Color(0,0,0,.25f);shadow.effectDistance=new Vector2(7,-9);
   score=Label(content,"",new Vector2(.665f,.345f),new Vector2(700,80),Vector2.zero,43);score.supportRichText=true;
   subtitle=Label(content,"THE ISLAND. THE GAME. THE MOMENT.",new Vector2(.5f,.105f),new Vector2(1250,40),Vector2.zero,21);subtitle.color=new Color32(172,200,195,255);
   var brand=new GameObject("Official game logo",typeof(RectTransform),typeof(Image));var r=brand.GetComponent<RectTransform>();r.SetParent(content,false);r.anchorMin=r.anchorMax=new Vector2(.285f,.53f);r.sizeDelta=new Vector2(560,560);r.localRotation=Quaternion.Euler(0,0,3);logo=brand.GetComponent<Image>();logo.sprite=Resources.Load<Sprite>("Menu/WhatTheFishLogo");logo.preserveAspect=true;logo.raycastTarget=false;
   plate.gameObject.SetActive(false);replay.gameObject.SetActive(false);time.gameObject.SetActive(false);kickoff.gameObject.SetActive(false);
  }
  public void Present(FootballMatch match,double age,bool footage){
   var screen=Screen.safeArea;safe.anchorMin=new Vector2(screen.xMin/Screen.width,screen.yMin/Screen.height);safe.anchorMax=new Vector2(screen.xMax/Screen.width,screen.yMax/Screen.height);
   bool sequence=match&&match.GoalSequence;bool inReplay=sequence&&age>=FootballMatch.ReplayStartOffset&&age<FootballMatch.ReplayEndOffset;
   replay.gameObject.SetActive(inReplay);time.gameObject.SetActive(inReplay);replay.text=footage?"●  REPLAY":"GOAL";
   time.text=footage?Mathf.Clamp((float)(age-FootballMatch.ReplayStartOffset),0,5).ToString("0.0")+" / 5.0":"REPLAY UNAVAILABLE";
   double wipe=sequence?age-FootballMatch.ReplayCaptureSeconds:-1;bool returning=sequence&&age>=FootballMatch.ReplayEndOffset;
   bool visible=sequence&&(wipe>=0&&wipe<FootballMatch.ReplayWipeSeconds||returning);plate.gameObject.SetActive(visible);
   if(visible){
    double elapsed=returning?age-FootballMatch.ReplayEndOffset:wipe;
    double enter=returning?FootballMatch.ReplayReturnInSeconds:FootballMatch.ReplayWipeInSeconds;
    double hold=returning?FootballMatch.ReplayReturnHoldSeconds:FootballMatch.ReplayWipeHoldSeconds;
    double leave=returning?FootballMatch.ReplayReturnOutSeconds:FootballMatch.ReplayWipeOutSeconds;
    float shift=elapsed<enter?Mathf.Lerp(-2.1f,0,Mathf.SmoothStep(0,1,(float)(elapsed/enter))):elapsed<enter+hold?0:Mathf.Lerp(0,2.1f,Mathf.SmoothStep(0,1,(float)((elapsed-enter-hold)/leave)));
    plate.anchoredPosition=Vector2.right*shift*((RectTransform)transform).rect.width;
    score.text="<color=#71B8FF>BLUE</color>   "+match.Snapshot.scoreA+"  :  "+match.Snapshot.scoreB+"   <color=#F18B8B>RED</color>";
    subtitle.text=returning?(match.Snapshot.phase==FootballMatchPhase.Finished?"FULL TIME":CharacterIdentity.TeamName(match.Snapshot.kickoffTeam).ToUpperInvariant()+"  ·  CENTRE KICKOFF"):"THE ISLAND. THE GAME. THE MOMENT.";
   }
   bool ready=match&&match.KickoffReady;kickoff.gameObject.SetActive(ready);
   if(ready){var actor=AppRoot.Instance?AppRoot.Instance.LocalAthlete:null;bool yours=FootballMatch.CanAimKickoff(actor);kickoff.text=yours?"YOUR KICKOFF\nSwipe to aim · Hold Shoot, then release":CharacterIdentity.TeamName(match.Snapshot.kickoffTeam).ToUpperInvariant()+" KICKOFF\nWaiting for the centre kick";}
  }
 }
 // Small native vector drawings share the UI material and add no shipped textures.
 public sealed class FootballReplayIllustration:MaskableGraphic {
  VertexHelper mesh;Vector2 origin;
  static readonly Color Aqua=new Color32(115,216,209,255),Gold=new Color32(210,173,96,255),Ink=new Color32(23,58,68,255),Soft=new Color32(43,78,84,255),Cream=new Color32(255,244,216,255),Coral=new Color32(241,139,114,255);
  void Poly(Color color,params Vector2[] points){
   int start=mesh.currentVertCount;foreach(var p in points)mesh.AddVert(origin+p,color,Vector2.zero);
   for(int i=1;i<points.Length-1;i++)mesh.AddTriangle(start,start+i,start+i+1);
  }
  void Line(Vector2 a,Vector2 b,float width,Color color){var d=(b-a).normalized;var n=new Vector2(-d.y,d.x)*width*.5f;Poly(color,a-n,b-n,b+n,a+n);}
  static Vector2 Radial(Vector2 c,float radius,float degrees){float t=degrees*Mathf.Deg2Rad;return c+new Vector2(Mathf.Cos(t),Mathf.Sin(t))*radius;}
  void Arc(Vector2 c,float radius,float from,float to,float width,Color color){int steps=Mathf.CeilToInt(Mathf.Abs(to-from)/5);for(int i=0;i<steps;i++)Line(Radial(c,radius,Mathf.Lerp(from,to,(float)i/steps)),Radial(c,radius,Mathf.Lerp(from,to,(float)(i+1)/steps)),width,color);}
  void Disc(Vector2 c,float radius,Color color){for(int i=0;i<64;i++)Poly(color,c,Radial(c,radius,i*360f/64),Radial(c,radius,(i+1)*360f/64));}
  void Spark(Vector2 c,float radius,Color color){for(int i=0;i<4;i++){float a=45+i*90;Poly(color,c,Radial(c,radius*.23f,a-45),Radial(c,radius,a),Radial(c,radius*.23f,a+45));}}
  protected override void OnPopulateMesh(VertexHelper vh){
   vh.Clear();mesh=vh;origin=rectTransform.rect.min;
   var badge=new Vector2(456,477);Disc(badge+new Vector2(12,-18),284,new Color32(8,27,37,255));Disc(badge,278,Ink);
   Arc(badge,291,22,143,3,Gold);Arc(badge,291,172,277,3,Aqua);Arc(badge,298,305,346,7,Coral);
   for(int i=0;i<7;i++)Arc(badge,306,70+i*8,73+i*8,3,Soft);
   // Goal-net sketch behind the headline: perspective posts and a quiet diamond mesh.
   Vector2 a=new Vector2(950,212),b=new Vector2(1430,212),c=new Vector2(1520,663),d=new Vector2(1040,663);
   for(int i=0;i<=10;i++){float t=i/10f;Line(Vector2.Lerp(a,b,t),Vector2.Lerp(d,c,t),2,Ink);Line(Vector2.Lerp(a,d,t),Vector2.Lerp(b,c,t),2,Ink);}
   Line(a,d,4,Soft);Line(d,c,4,Soft);Line(c,b,4,Soft);
   // Curved shot trail leads into a hand-drawn football above the goal word.
   Arc(new Vector2(1050,980),255,218,302,3,Soft);Arc(new Vector2(1050,980),274,233,297,2,Soft);
   var ball=new Vector2(1364,747);Disc(ball,64,Cream);Arc(ball,70,0,360,3,Gold);
   var panel=new Vector2[5];for(int i=0;i<5;i++)panel[i]=Radial(ball,27,90+i*72);Poly(Ink,panel);
   for(int i=0;i<5;i++){float t=90+i*72;var edge=Radial(ball,60,t);Line(panel[i],edge,3,Ink);Poly(Ink,edge,Radial(ball,62,t-17),Radial(ball,47,t),Radial(ball,62,t+17));}
   Line(new Vector2(1218,730),new Vector2(1274,744),5,Aqua);Line(new Vector2(1229,709),new Vector2(1278,723),3,Gold);
   Spark(new Vector2(768,738),21,Gold);Spark(new Vector2(245,720),17,Aqua);Spark(new Vector2(695,224),19,Coral);Spark(new Vector2(1460,496),13,Gold);
   Line(new Vector2(249,255),new Vector2(231,234),5,Gold);Line(new Vector2(275,237),new Vector2(266,212),4,Aqua);
   Line(new Vector2(839,241),new Vector2(1289,241),2,Gold);
   Line(new Vector2(373,146),new Vector2(435,146),3,Aqua);Line(new Vector2(448,146),new Vector2(510,146),3,Gold);Line(new Vector2(523,146),new Vector2(585,146),3,Coral);
  }
 }
 public sealed class FootballSlashGraphic:MaskableGraphic {
  static Color C(byte r,byte g,byte b)=>new Color32(r,g,b,255);
  protected override void OnPopulateMesh(VertexHelper vh){
   vh.Clear();var rect=rectTransform.rect;float w=rect.width,h=rect.height,x=rect.xMin,y=rect.yMin;
   void Quad(float a,float b,Color color){int i=vh.currentVertCount;vh.AddVert(new Vector3(x+a*w,y),color,Vector2.zero);vh.AddVert(new Vector3(x+b*w,y),color,Vector2.zero);vh.AddVert(new Vector3(x+(b+.22f)*w,y+h),color,Vector2.zero);vh.AddVert(new Vector3(x+(a+.22f)*w,y+h),color,Vector2.zero);vh.AddTriangle(i,i+1,i+2);vh.AddTriangle(i,i+2,i+3);}
   Quad(-.4f,1.2f,C(12,35,46));Quad(-.22f,-.035f,C(85,198,191));Quad(-.035f,.007f,C(246,235,206));Quad(.007f,.068f,C(233,126,101));Quad(.068f,.074f,C(210,173,96));
   Quad(.88f,.886f,C(210,173,96));Quad(.886f,1.04f,C(88,202,195));Quad(1.04f,1.09f,C(246,235,206));Quad(1.09f,1.18f,C(233,126,101));
  }
 }
}
