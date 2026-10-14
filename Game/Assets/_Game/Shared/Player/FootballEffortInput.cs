using UnityEngine;

namespace WhatTheFish {
 public sealed partial class PlayerView {
  public bool DashHeld {get;private set;}
  public bool PressureHeld {get;private set;}
  int dashSource,pressureSource;
  public bool FootballInputReady=>!InputBlocked&&active&&target&&FootballPressure.Eligible(target);
  public bool BeginDash(int source=int.MinValue){if(DashHeld||!(FootballInputReady||PressureHeld&&FootballPressureReady)||Charging||target.FootballEffort.Exhausted)return false;DashHeld=true;dashSource=source;if(source==int.MinValue)FootballAudio.PlayMenu();return true;}
  public void EndDash(int source=int.MinValue){if(source==dashSource)DashHeld=false;}
  public bool FootballPressureReady=>!InputBlocked&&active&&target&&FootballPressure.CanHold(target);
  public bool BeginPressure(int source=int.MinValue){if(PressureHeld||!FootballPressureReady||!FootballPressure.CarrierFor(target))return false;PressureHeld=true;pressureSource=source;if(source==int.MinValue)FootballAudio.PlayMenu();return true;}
  public void EndPressure(int source=int.MinValue){if(source==pressureSource)PressureHeld=false;}
  void ClearFootballInput(){DashHeld=PressureHeld=false;}
  void UpdateFootballInput(){
   if(!FootballPressureReady){ClearFootballInput();return;}
   if(!FootballInputReady&&!PressureHeld)DashHeld=false;
   if(Charging)DashHeld=false;
   if(!FootballPressure.CarrierFor(target))PressureHeld=false;
   if(Input.GetKeyDown(KeyCode.LeftControl))BeginDash();if(Input.GetKeyUp(KeyCode.LeftControl))EndDash();
   if(Input.GetKeyDown(KeyCode.Q))BeginPressure();if(Input.GetKeyUp(KeyCode.Q))EndPressure();
  }
  void AddFootballInput(ref PlayerCommand command){command.dash|=DashHeld;command.pressure|=PressureHeld;}
 }
}
