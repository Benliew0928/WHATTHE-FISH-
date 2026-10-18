using UnityEngine;

namespace WhatTheFish {
 public sealed partial class PlayerView {
  bool pendingBlock,pendingJumpBlock;uint blockPlay,guardPlay;int guardSource;
  public bool GuardHeld {get;private set;}
  public bool AutoBlockHeld {get;private set;}
  public BasketballRole BasketballRole=>BasketballBall.Active?BasketballBall.Active.Role(target):WhatTheFish.BasketballRole.Inactive;
  public bool BeginGuard(int source=int.MinValue){
   if(GuardHeld||InputBlocked||!active||BasketballRole!=WhatTheFish.BasketballRole.Defense)return false;
   GuardHeld=true;guardSource=source;guardPlay=BasketballBall.Active.Defense.play;return true;
  }
  public bool RequestGuardSteal(int source){
   var ball=BasketballBall.Active;if(!GuardHeld||AutoBlockHeld||source!=guardSource||InputBlocked||!ball||!ball.CanSteal(target)||target.BasketballMotion.Busy)return false;
   pendingSteal=true;return true;
  }
  public bool SetGuardBlock(bool enabled,int source=int.MinValue){
   if(!GuardHeld||source!=guardSource||InputBlocked)return false;
   AutoBlockHeld=enabled;if(enabled){pendingSteal=false;StealHeld=false;}return true;
  }
  public void EndGuard(int source=int.MinValue){if(GuardHeld&&source==guardSource)GuardHeld=AutoBlockHeld=false;}
  public bool RequestBlock(bool jumping=false){
   var ball=BasketballBall.Active;if(!active||!ball||!ball.CanBlock(target)||pendingBlock||pendingJumpBlock)return false;
   pendingBlock=!jumping;pendingJumpBlock=jumping;blockPlay=ball.Defense.play;return true;
  }
  void ClearDefenseInput(){GuardHeld=AutoBlockHeld=pendingBlock=pendingJumpBlock=false;}
  void UpdateDefenseInput(){
   var ball=BasketballBall.Active;
   if(!ball||BasketballRole!=WhatTheFish.BasketballRole.Defense){ClearDefenseInput();return;}
   if(GuardHeld&&guardPlay!=ball.Defense.play)GuardHeld=AutoBlockHeld=false;
  }
  void AddDefenseInput(ref PlayerCommand command){
   command.guard|=GuardHeld;command.guardBlock|=GuardHeld&&AutoBlockHeld;command.block|=pendingBlock;command.jumpBlock|=pendingJumpBlock;
   command.defensePlay=pendingBlock||pendingJumpBlock?blockPlay:GuardHeld?guardPlay:BasketballBall.Active?BasketballBall.Active.Defense.play:0;
   pendingBlock=pendingJumpBlock=false;
  }
 }
}
