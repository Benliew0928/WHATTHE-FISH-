using UnityEngine;
namespace WhatTheFish {
 public sealed partial class GolfMatchManager {
  public GolfEventSchedule Events {get;}=new GolfEventSchedule();
  public GolfShotConditions ShotConditions(GolfShotMode mode)=>Context&&State.Running?Events.Conditions(Now,mode):default;
  public float EventMoveMultiplier(Athlete actor)=>Context&&actor&&!actor.IsSpectator&&Player(actor)!=null&&Events.RushActive(Now)?2.1f:1;
  public float EventCartMultiplier=>Context&&Events.RushActive(Now)?1.9f:1;
  void UpdateEvents(){if(TutorialSession.Is(SportId.Golf))Events.Clear();else Events.Advance(State,Now,0);}
#if UNITY_EDITOR || DEVELOPMENT_BUILD
  public void ReviewEvent(GolfEventKind kind){Events.ReviewEvent(kind,Now,Vector3.right*GolfEventSchedule.WindAcceleration);UpdateEvents();Publish();}
#endif
 }
}
