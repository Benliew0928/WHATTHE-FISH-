using UnityEngine;
namespace WhatTheFish {
 [DefaultExecutionOrder(1000)]
 public sealed class FootballTrail:MonoBehaviour {
  public const float Lifetime=.48f;
  public SportsMotionRibbon Wake {get;private set;}
  public LineRenderer Ribbon=>Wake.Line;
  public bool IsReplay {get;set;}
  public int Style {get;private set;}=-1;
  FootballBall ball;
  void Awake(){ball=GetComponent<FootballBall>();var node=new GameObject("Personal football trail");node.transform.SetParent(transform,false);Wake=node.AddComponent<SportsMotionRibbon>();}
  public void Clear(){if(Wake)Wake.Clear();}
  void OnDisable()=>Clear();
  void LateUpdate(){
   if(Style!=FootballTrailStyle.Selected){Style=FootballTrailStyle.Selected;Clear();}
   bool visible=IsReplay?FootballReplay.Presenting:ball&&FootballTackle.EnvironmentAllowed&&!FootballReplay.Presenting&&!ball.PortalBusy&&!(SkySailWorld.Instance&&SkySailWorld.Instance.Travelling);
   if(!visible){Clear();return;}
   bool reduced=MenuPreferences.ReducedMotion;float strength=Mathf.InverseLerp(1,16,Wake.Speed);
   Wake.Step(transform.position,FootballTrailStyle.Material(Style),Mathf.Lerp(.28f,.54f,strength)*(reduced?.7f:1),reduced?.23f:Lifetime,reduced?1.35f:3.2f);
  }
 }
}
