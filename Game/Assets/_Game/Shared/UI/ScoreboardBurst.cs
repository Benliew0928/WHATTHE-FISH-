using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 // Short, bounded motion uses unscaled render-frame time. No scoring or gameplay side effects.
 public sealed class ScoreboardBurst:MonoBehaviour {
  public RectTransform Rect=>(RectTransform)transform;public bool Visible=>group&&group.alpha>.001f;
  CanvasGroup group;Text points,title,team;ScoreboardBurstGraphic graphic;SportId sport;float age=10,duration,y;bool three;
  public static ScoreboardBurst Create(Transform parent,SportId sport,float width,float top,bool inset,bool translucent=false){
   float w=Mathf.Min(width-12,inset?width-12:272);var r=CoveUI.Rect(parent,"Scoring celebration",(width-w)*.5f,top,w,46);
   r.pivot=Vector2.one*.5f;r.anchoredPosition+=new Vector2(w*.5f,-23);
   var effect=r.gameObject.AddComponent<ScoreboardBurst>();effect.sport=sport;effect.y=r.anchoredPosition.y;
   effect.group=r.gameObject.AddComponent<CanvasGroup>();effect.group.interactable=effect.group.blocksRaycasts=false;
   effect.graphic=r.gameObject.AddComponent<ScoreboardBurstGraphic>();effect.graphic.raycastTarget=false;effect.graphic.football=sport==SportId.Football;effect.graphic.translucent=translucent;
   effect.points=SportsScoreboard.Label(r,"Score increase","+3",11,1,68,44,33,SportsScoreboard.Ivory);
   effect.title=SportsScoreboard.Label(r,"Scoring headline","THREE POINTER",84,7,w-97,20,15,SportsScoreboard.Ivory,true,TextAnchor.MiddleLeft);
   effect.team=SportsScoreboard.Label(r,"Scoring team","BLUE",86,28,w-99,12,8,SportsScoreboard.Orange,false,TextAnchor.MiddleLeft);effect.Hide();return effect;
  }
  public void Play(int value,FootballTeam scoring,bool practice){
   age=0;three=sport==SportId.Basketball&&value==3;duration=three?1.8f:1.25f;
   points.text="+"+value;title.text=sport==SportId.Football?"GOAL!":three?"THREE POINTER":"BUCKET!";
   team.text=practice?"RALLY COURT":scoring==FootballTeam.A?"BLUE TEAM":"RED TEAM";
   team.color=scoring==FootballTeam.B?SportsScoreboard.Red:SportsScoreboard.Blue;graphic.three=three;graphic.teamColor=team.color;Tick();
  }
  public void Hide(){age=10;if(group)group.alpha=0;}
  public void SetTop(float top){y=-top-23;if(Visible)Tick();}
  void Update(){if(age>duration)return;age+=Time.unscaledDeltaTime;Tick();}
  void Tick(){
   float enter=Mathf.Clamp01(age/.24f),exit=Mathf.Clamp01((age-(duration-.35f))/.35f),ease=1-Mathf.Pow(1-enter,3);
   group.alpha=Mathf.SmoothStep(0,1,enter)*(1-Mathf.SmoothStep(0,1,exit));
   Rect.anchoredPosition=new Vector2(Rect.anchoredPosition.x,y+Mathf.Lerp(-9,0,ease)+exit*8);
   Rect.localScale=Vector3.one*(1-.1f*(1-ease)+.035f*Mathf.Sin(Mathf.PI*enter));
   graphic.progress=Mathf.Clamp01(age/duration);graphic.SetVerticesDirty();
  }
 }
 public sealed class ScoreboardBurstGraphic:MaskableGraphic {
  public bool football,three,translucent;public float progress;public Color teamColor;
  protected override void OnPopulateMesh(VertexHelper v){
   v.Clear();Rect r=rectTransform.rect;Color accent=football?SportsScoreboard.Lime:SportsScoreboard.Orange;
   var backdrop=football?SportsScoreboard.Green:SportsScoreboard.Charcoal;if(translucent)backdrop.a=ScoreboardDetails.BackgroundOpacity;
   Quad(v,new Vector2(r.xMin+10,r.yMin),new Vector2(r.xMax-10,r.yMin),new Vector2(r.xMax,r.yMax),new Vector2(r.xMin,r.yMax),backdrop);
   BasketballUIMesh.Line(v,new Vector2(r.xMin+11,r.yMin+1),new Vector2(r.xMax-12,r.yMin+1),accent,2);
   Quad(v,new Vector2(r.xMin+4,r.yMax),new Vector2(r.xMin+7,r.yMax),new Vector2(r.xMin+17,r.yMin),new Vector2(r.xMin+12,r.yMin),teamColor);
   if(!three)return;
   // Three speed chevrons travel once; the long-range score has its own silhouette.
   for(int i=0;i<3;i++){
    float x=r.xMax-28+i*8+Mathf.Sin(progress*Mathf.PI)*5;Color c=accent;c.a=.85f*(1-Mathf.Clamp01((progress-.7f)/.3f));
    BasketballUIMesh.Line(v,new Vector2(x-6,r.yMin+3),new Vector2(x+3,r.yMax-3),c,2);
   }
   for(int i=0;i<6;i++){
    float p=Mathf.Clamp01(progress*2),x=r.xMin+25+i*31,y=r.yMax+2+Mathf.Sin(p*Mathf.PI)*(5+i%2*6);Color c=accent;c.a=(1-p)*.8f;
    BasketballUIMesh.Line(v,new Vector2(x,y),new Vector2(x+8,y+3),c,1.5f);
   }
  }
  static void Quad(VertexHelper v,Vector2 a,Vector2 b,Vector2 c,Vector2 d,Color color){int n=v.currentVertCount;v.AddVert(a,color,Vector2.zero);v.AddVert(b,color,Vector2.zero);v.AddVert(c,color,Vector2.zero);v.AddVert(d,color,Vector2.zero);v.AddTriangle(n,n+1,n+2);v.AddTriangle(n,n+2,n+3);}
 }
}
