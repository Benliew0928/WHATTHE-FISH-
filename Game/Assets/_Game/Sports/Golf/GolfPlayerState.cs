using System;

namespace WhatTheFish {
 // One ball owner, one independent sequence. Only GolfMatchState mutates scores.
 public sealed class GolfPlayerState {
  public const int HoleCount=5;
  public ulong PlayerId {get;}
  public string PlayerName {get;}
  public int CurrentHole=>Math.Min(CompletedHoleCount+1,HoleCount);
  public int CompletedHoleCount {get;private set;}
  public int CurrentHoleStroke {get;private set;}
  public int TotalStroke {get;private set;}
  public double FinishTime {get;private set;}
  public bool IsFinished {get;private set;}
  public bool IsDNF {get;private set;}
  public string Status=>IsFinished?"Finished":IsDNF?"DNF":"Hole "+CurrentHole;
  public GolfPlayerState(ulong id,string name){PlayerId=id;PlayerName=name;}
  internal bool Stroke(){if(IsFinished||IsDNF)return false;CurrentHoleStroke++;TotalStroke++;return true;}
  internal bool Complete(int hole,double elapsed){
   if(IsFinished||IsDNF||hole!=CurrentHole||hole<1||hole>HoleCount)return false;
   CompletedHoleCount++;
   if(CompletedHoleCount==HoleCount){IsFinished=true;FinishTime=elapsed;}else CurrentHoleStroke=0;
   return true;
  }
  internal void DidNotFinish(){if(!IsFinished)IsDNF=true;}
  internal void Receive(GolfPlayerSnapshot s){CompletedHoleCount=s.completed;CurrentHoleStroke=s.holeStroke;TotalStroke=s.totalStroke;FinishTime=s.finishTime;IsFinished=s.finished;IsDNF=s.dnf;}
 }
}
