using UnityEngine;

namespace WhatTheFish {
 public struct FootballBotAction {
  public Vector2 move;public bool sprint,dash,pressure,charge,kick,tackle,useProp;public float power,propHeading;
  public PlayerCommand Command(Athlete actor){
   if(!float.IsFinite(move.x)||!float.IsFinite(move.y)||!float.IsFinite(power)||!float.IsFinite(propHeading)||!FootballMatch.Instance||!FootballMatch.Instance.Running)return default;
   return new PlayerCommand{move=Vector2.ClampMagnitude(move,1),heading=0,sprint=sprint,dash=dash,pressure=pressure,charging=charge&&actor.KickReady,kick=kick&&actor.KickReady,kickCharge=Mathf.Clamp01(power),tackle=tackle&&actor.TackleReady};
  }
 }
}
