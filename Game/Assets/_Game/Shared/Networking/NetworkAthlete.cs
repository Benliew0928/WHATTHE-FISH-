using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace WhatTheFish {
 public sealed partial class NetworkAthlete:NetworkBehaviour {
  public static NetworkAthlete HostPlayer;
  public bool IsPlayerObject=>NetworkObject&&NetworkObject.IsPlayerObject;
  public NetworkVariable<bool> Ready=new(false);
  public NetworkVariable<byte> BotSlot=new(0);
  public NetworkVariable<SportId> BotSport=new(SportId.Football);
  byte initialBotSlot;
  SportId initialBotSport;
  public void ConfigureBot(byte slot,SportId sport=SportId.Football){if(!IsSpawned){initialBotSlot=slot;initialBotSport=sport;}}
  public NetworkVariable<FixedString64Bytes> Nickname=new();
  public NetworkVariable<bool> Exploring=new(false);
  public NetworkVariable<int> FootballMinutes=new(0);
  public NetworkVariable<int> BasketballMinutes=new(3);
  public NetworkVariable<BasketballMatchSnapshot> BasketballMatch=new();
  public NetworkVariable<FootballTeam> BasketballTeam=new(FootballTeam.None);
  public NetworkVariable<SportId> WorldSport=new(SportId.Football);
  public NetworkVariable<SkySailJourney> WorldTravel=new();
  public NetworkVariable<RoomTeleportState> WorldTeleport=new();
  public NetworkVariable<RoomTeleportAck> TeleportReady=new();
  [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)] public void TeleportReadyRpc(uint sequence,byte stage,bool failed){
   if(!IsPlayerObject||!HostPlayer||!RoomTeleport.Busy||HostPlayer.WorldTeleport.Value.sequence!=sequence||stage<1||stage>3)return;
   var phase=HostPlayer.WorldTeleport.Value.phase;if(!failed&&(stage==1&&phase!=RoomTeleportPhase.Preparing||stage==2&&phase!=RoomTeleportPhase.Departing||stage==3&&phase!=RoomTeleportPhase.Arriving))return;
   var previous=TeleportReady.Value;if(previous.sequence==sequence&&(previous.failed||stage<previous.stage&&!failed))return;
   TeleportReady.Value=new RoomTeleportAck{sequence=sequence,stage=stage,failed=failed};
  }
  public NetworkVariable<uint> TravelReady=new();
  public NetworkVariable<FixedString512Bytes> WorldAppearance=new();
  public NetworkVariable<FixedString128Bytes> AppearanceData=new();
  public NetworkVariable<byte> IdentityNumber=new(0);
  public NetworkVariable<byte> RoomSeat=new(RoomSeats.Unassigned);
  public NetworkVariable<bool> AllowSpectators=new(true);
  public bool IsSpectator=>RoomSeats.Watching(RoomSeat.Value);
  [Rpc(SendTo.Owner,InvokePermission=RpcInvokePermission.Server)] public void MatchArrivalRpc(float yaw){if(!IsOwner||!AppRoot.Instance)return;AppRoot.Instance.view.ClearMatchInput();AppRoot.Instance.view.yaw=yaw;}
  public void SetRoomSeat(byte seat){if(!IsServer)return;RoomSeat.Value=seat;IdentityNumber.Value=(byte)(seat%10+1);if(athlete){athlete.capsule.excludeLayers=IsSpectator?~(1<<8):0;athlete.ResetLocomotion();}Team.Value=BasketballTeam.Value=FootballTeam.None;ClearMatchInput();}
  [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)] public void ChooseRoomSeatRpc(byte seat){if(!RoomSeats.Choose(this,seat,out var reason))RoomSeatRejectedRpc(reason);}
  [Rpc(SendTo.Owner,InvokePermission=RpcInvokePermission.Server)] void RoomSeatRejectedRpc(string reason){AppRoot.Instance.rooms.SeatMessage=reason;}
  [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)] public void AllowSpectatorsRpc(bool allow){
   if(this!=HostPlayer||Exploring.Value||RoomTeleport.Busy)return;
   if(!allow&&RoomTeleport.Players().Any(n=>n.IsSpectator)){RoomSeatRejectedRpc("Spectators are already here. Ask them to choose a player place first.");return;}
   AllowSpectators.Value=allow;
  }
  bool appearanceInitialized;
  public NetworkVariable<LocomotionSnapshot> Motion=new();
  public NetworkVariable<JumpSnapshot> Jump=new();
  public NetworkVariable<FootballSnapshot> Football=new();
  public NetworkVariable<FootballEffortSnapshot> Effort=new(new FootballEffortSnapshot{stamina=100});
  public NetworkVariable<BasketballSnapshot> Basketball=new();
  public NetworkVariable<SportsPropSlot> BasketballProp=new();
  public NetworkVariable<SportsPropSlot> FootballProp=new();
  [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)] public void UseFootballPropRpc(uint revision,uint round,float heading){if(IsSpectator)return;if(IsSpawned&&HostPlayer&&HostPlayer.Exploring.Value&&HostPlayer.WorldSport.Value==SportId.Football&&FootballBall.Instance){if(!FootballBall.Instance.TryUseProp(athlete,revision,round,heading))FootballPropRejectedRpc();}}
  [Rpc(SendTo.Owner,InvokePermission=RpcInvokePermission.Server)] void FootballPropRejectedRpc(){FootballPropHUD.Instance?.Rejected();}
  [Rpc(SendTo.NotServer,InvokePermission=RpcInvokePermission.Server)] public void FootballAudioRpc(FootballAudioEvent sound){if(IsSpawned&&this==HostPlayer)WhatTheFish.FootballAudio.Receive(sound,NetworkObjectId);}
  [Rpc(SendTo.Owner,InvokePermission=RpcInvokePermission.Server)] void FootballTeamRejectedRpc(){WhatTheFish.FootballAudio.PlayMenu(FootballSound.UIError);}
  [Rpc(SendTo.NotServer,InvokePermission=RpcInvokePermission.Server)] public void BasketballAudioRpc(BasketballAudioEvent sound){if(IsSpawned&&this==HostPlayer)WhatTheFish.BasketballAudio.Receive(sound,NetworkObjectId);}
  [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)] public void UseBasketballPropRpc(uint revision){if(IsSpectator)return;if(IsSpawned&&!WhatTheFish.BasketballMatch.BlocksActions&&HostPlayer&&HostPlayer.Exploring.Value&&HostPlayer.WorldSport.Value==SportId.Basketball&&BasketballBall.Active){if(!BasketballBall.Active.TryUseProp(athlete,revision))BasketballPropRejectedRpc();}}
  [Rpc(SendTo.Owner,InvokePermission=RpcInvokePermission.Server)] void BasketballPropRejectedRpc(){BasketballAudio.PlayMenu(BasketballSound.UIError);}
  public NetworkVariable<FootballBallSnapshot> Ball=new();
  public NetworkVariable<FootballMatchSnapshot> Match=new();
  public NetworkList<FootballPlayerRecord> FootballPlayers=new();
  public NetworkList<BasketballPlayerRecord> BasketballPlayers=new();
  public NetworkVariable<FootballTeam> Team=new(FootballTeam.None);
  public void ClearMatchInput(){command=default;lastInput=0;}
  uint MatchRevision=>HostPlayer?HostPlayer.WorldSport.Value==SportId.Basketball?HostPlayer.BasketballMatch.Value.revision:HostPlayer.Match.Value.revision:0;
  bool AcceptRound(uint round){if(IsSpectator||RoomTeleport.Busy||athlete&&athlete.inTransit)return false;if(WhatTheFish.BasketballMatch.Instance&&WhatTheFish.BasketballMatch.Instance.Context)return !WhatTheFish.BasketballMatch.BlocksMovement&&round==MatchRevision;return !FootballMatch.Instance||!FootballMatch.Instance.Context||(!FootballMatch.BlocksMovement||FootballMatch.CanAimKickoff(athlete))&&round==MatchRevision;}
  bool AcceptBasketball(uint round)=>AcceptRound(round)&&!WhatTheFish.BasketballMatch.BlocksActions&&!(WhatTheFish.BasketballMatch.Instance&&WhatTheFish.BasketballMatch.Instance.AutomaticMovement(athlete));
  public NetworkVariable<BasketballMotionState> BasketballPose=new();
  public NetworkVariable<bool> BasketballFreeRoam=new(false);
  public NetworkVariable<FootballMotionState> FootballPose=new();
  public NetworkVariable<GolfCartState> Cart=new();
  public NetworkVariable<GolfMatchSnapshot> GolfMatch=new();
  public NetworkList<GolfPlayerRecord> GolfPlayers=new();
  public NetworkVariable<GolfBallSnapshot> GolfBall=new();
  public NetworkVariable<GolfClubState> GolfPose=new();
  public NetworkVariable<FishingMatchSnapshot> FishingMatch=new();
  public NetworkList<FishingPlayerRecord> FishingPlayers=new();
  public NetworkList<FishingFishRecord> FishingFish=new();
  public NetworkVariable<GolfAimState> GolfAim=new();
  [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)] public void GolfAimRpc(bool enabled,ulong owner,uint round,float heading){if(IsSpectator)return;if(IsSpawned&&GolfMatchManager.Instance)GolfMatchManager.Instance.SetAim(athlete,enabled,owner,round,heading);}
  bool sentGolfCharge;uint sentGolfRound;float sentGolfHeading;
  Athlete athlete; PlayerCommand command,lastSentCommand; float lastInput; float sendTimer;
  uint inputSequence,acceptedInputSequence;bool receivedInput;
  // NetworkTransform interpolates guests, but deliberately skips its server.
  // The host's own sports character therefore uses the offline per-render-frame
  // movement path; stepping it at 50 Hz makes 30/60/120 Hz frames visibly uneven.
  // Guest commands still simulate on the server's fixed clock.
  bool FrameDrivenHost=>IsServer&&IsOwner&&BotSlot.Value==0&&HostPlayer&&(HostPlayer.WorldSport.Value==SportId.Football||HostPlayer.WorldSport.Value==SportId.Basketball);
  public override void OnNetworkSpawn(){
   athlete=GetComponent<Athlete>();athlete.Setup();athlete.capsule.enabled=IsServer;
   if(IsServer){var sport=AppRoot.Instance.rooms.Sport;byte seat=IsPlayerObject?AppRoot.Instance.rooms.ClaimSeat(OwnerClientId):RoomSeats.Free(sport,false);if(seat==RoomSeats.Unassigned)seat=RoomSeats.Free(sport,false,IsOwner&&IsPlayerObject?LocalProfile.Character.preset:0);if(seat==RoomSeats.Unassigned&&IsPlayerObject&&RoomSeats.Allowed)seat=RoomSeats.Free(sport,true);if(seat==RoomSeats.Unassigned)throw new System.InvalidOperationException("No room seat available");SetRoomSeat(seat);}
   IdentityNumber.OnValueChanged+=OnIdentity;OnIdentity(0,IdentityNumber.Value);
   if(IsServer&&initialBotSlot>0){BotSlot.Value=initialBotSlot;BotSport.Value=initialBotSport;Nickname.Value="BOT "+initialBotSlot;Ready.Value=true;}
   if(IsServer){athlete.capsule.enabled=false;var used=FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None).Where(n=>n!=this&&n.IsSpawned).Select(n=>n.transform.position).ToArray();var spawns=AppRoot.Instance.environments.Definition(AppRoot.Instance.rooms.Sport).spawnPositions;var spawn=spawns.FirstOrDefault(p=>used.All(q=>Vector3.Distance(p,q)>1));if(spawn==Vector3.zero)spawn=spawns[0];transform.position=spawn;GetComponent<Unity.Netcode.Components.NetworkTransform>().Teleport(spawn,Quaternion.identity,Vector3.one);athlete.capsule.enabled=true;}
   if(IsPlayerObject&&OwnerClientId==NetworkManager.ServerClientId){HostPlayer=this;if(IsServer){FootballMinutes.Value=MenuMatchRules.OverrideMinutes;BasketballMinutes.Value=MenuMatchRules.BasketballMinutes;WorldSport.Value=AppRoot.Instance.rooms.Sport;WorldAppearance.Value=JsonUtility.ToJson(LocalProfile.ForSport(WorldSport.Value));}}
   if(IsServer&&IsPlayerObject)AppRoot.Instance.rooms.RestoreMember(this);
   AppearanceData.OnValueChanged+=OnAppearance;OnAppearance(default,AppearanceData.Value);
   if(BotSlot.Value>0&&IsServer){if(BotSport.Value==SportId.Basketball)gameObject.AddComponent<BasketballBot>().Initialize(BotSlot.Value);else gameObject.AddComponent<FootballBot>().Initialize(BotSlot.Value,FootballBots.Instance?FootballBots.Instance.Seed:1729);}
   if(IsOwner&&IsPlayerObject){RecoveryAcknowledgedRpc();if(!AppRoot.Instance.rooms.Recovering){AppearanceRpc(JsonUtility.ToJson(LocalProfile.Character));NicknameRpc(PlayerIdentity.DisplayName);}AppRoot.Instance.LocalAthlete=athlete;}
   StartPublicProfile();
  }
  public override void OnNetworkDespawn(){StopPublicProfile();if(HostPlayer==this)AppRoot.Instance.rooms.HostDespawned();IdentityNumber.OnValueChanged-=OnIdentity;GolfCartWorld.Forget(athlete);AppearanceData.OnValueChanged-=OnAppearance;if(HostPlayer==this)HostPlayer=null;}
  void OnIdentity(byte previous,byte current){if(athlete&&current>0)CharacterIdentity.Ensure(athlete).SetNumber(current);}
  void OnAppearance(FixedString128Bytes oldValue,FixedString128Bytes newValue){if(newValue.Length>0)athlete.Appearance(JsonUtility.FromJson<CharacterAppearance>(newValue.ToString()));}
  [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)] public void AppearanceRpc(FixedString128Bytes json){CharacterAppearance a;try{a=JsonUtility.FromJson<CharacterAppearance>(json.ToString());}catch(System.ArgumentException){return;}a.Clamp();if(!appearanceInitialized&&IsPlayerObject&&!(HostPlayer&&HostPlayer.Exploring.Value)){byte preferred=RoomSeats.Free(AppRoot.Instance.rooms.Sport,IsSpectator,a.preset);if(preferred!=RoomSeats.Unassigned&&preferred%10==a.preset)SetRoomSeat(preferred);}appearanceInitialized=true;a.preset=IdentityNumber.Value-1;AppearanceData.Value=JsonUtility.ToJson(a);}
  [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)] public void ReadyRpc(bool value){if(!HostPlayer||!HostPlayer.Exploring.Value)Ready.Value=value;}
  public static string CleanNickname(string value)=>PlayerIdentity.CleanName(value);
  [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)] public void NicknameRpc(FixedString64Bytes value){if(ProfileId.Value.Length>0&&!AppRoot.Instance.rooms.LocalTest)return;string text=CleanNickname(value.ToString());Nickname.Value=text.Length>0?text:OwnerClientId==0?"HOST":"FRIEND "+OwnerClientId;}
  [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)] public void ChooseFootballTeamRpc(FootballTeam team,uint selectionRevision){if(IsSpectator)return;if(IsSpawned&&FootballMatch.Instance&&!FootballMatch.Instance.ChooseTeam(athlete,team,selectionRevision))FootballTeamRejectedRpc();}
  [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)] public void ChooseBasketballTeamRpc(FootballTeam team,uint revision){if(IsSpectator)return;if(IsSpawned&&WhatTheFish.BasketballMatch.Instance)WhatTheFish.BasketballMatch.Instance.ChooseTeam(athlete,team,revision);}
  [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)] public void TravelReadyRpc(uint sequence){if(HostPlayer&&HostPlayer.WorldTravel.Value.phase==SkySailPhase.Preparing&&HostPlayer.WorldTravel.Value.sequence==sequence)TravelReady.Value=sequence;}
  [Rpc(SendTo.Server,Delivery=RpcDelivery.Unreliable,InvokePermission=RpcInvokePermission.Owner)] void InputRpc(Vector2 move,float heading,bool sprint,bool dash,bool pressure,bool charging,bool guard,bool guardBlock,uint defensePlay,uint round,float passBend,uint inputId){
   // A delayed UDP packet must never undo a newer stop/turn/guard command.
   if(receivedInput&&!NetworkResponsiveness.Newer(inputId,acceptedInputSequence))return;
   if(float.IsNaN(move.x)||float.IsNaN(move.y)||float.IsNaN(heading)||float.IsInfinity(heading)||float.IsInfinity(move.x)||float.IsInfinity(move.y))return;
   if((IsSpectator?RoomTeleport.Busy||!HostPlayer||!HostPlayer.Exploring.Value||athlete.inTransit:!AcceptRound(round))||!float.IsFinite(passBend))return;
   receivedInput=true;acceptedInputSequence=inputId;
   command=new PlayerCommand{passBend=Mathf.Clamp(passBend,-1,1),move=Vector2.ClampMagnitude(move,1),heading=heading%360,sprint=sprint,dash=dash,pressure=pressure,charging=charging,guard=guard&&BasketballBall.Active&&defensePlay==BasketballBall.Active.Defense.play,guardBlock=guard&&guardBlock,defensePlay=defensePlay};lastInput=Time.time;
  }
  [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)] void TackleRpc(uint round){if(AcceptRound(round)&&IsSpawned&&HostPlayer&&HostPlayer.Exploring.Value&&HostPlayer.WorldSport.Value==SportId.Football)athlete.TryTackle();}
  [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)] void KickRpc(float charge,uint round,uint abilityRevision,float heading){if(IsSpectator)return;if(FootballMatch.Instance&&FootballMatch.Instance.AwaitingKickoff){FootballMatch.Instance.TryKickoff(athlete,heading,charge,round);return;}if(float.IsNaN(charge)||float.IsInfinity(charge)||!FootballBall.Instance||abilityRevision!=FootballBall.Instance.PropBallRevision)return;if(AcceptRound(round)&&IsSpawned&&HostPlayer&&HostPlayer.Exploring.Value&&HostPlayer.WorldSport.Value==SportId.Football)athlete.TryKick(Mathf.Clamp01(charge));}
  [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)] public void FootballPassRpc(float heading,uint round,uint abilityRevision){
   bool valid=IsSpawned&&AcceptRound(round)&&HostPlayer&&HostPlayer.Exploring.Value&&HostPlayer.WorldSport.Value==SportId.Football&&FootballBall.Instance&&abilityRevision==FootballBall.Instance.PropBallRevision;
   bool passed=valid&&FootballBall.Instance.TryPass(athlete,heading);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
   if(System.Environment.GetCommandLineArgs().Contains("-footballPlaymakerReview"))Debug.Log("PLAYMAKER_PASS_RPC actor="+NetworkObjectId+" heading="+heading+" valid="+valid+" passed="+passed+" round="+round+" current="+MatchRevision+" position="+transform.position+" ball="+(FootballBall.Instance?FootballBall.Instance.Body.position:Vector3.zero));
#endif
  }
  [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)] public void TargetPassRpc(ulong receiverId,uint possession,uint round,uint abilityRevision){
   if(!IsSpawned||!HostPlayer||!HostPlayer.Exploring.Value||!AcceptRound(round))return;
   var receiver=TeammatePassing.Find(receiverId);
   if(HostPlayer.WorldSport.Value==SportId.Basketball&&AcceptBasketball(round)&&BasketballBall.Active)BasketballBall.Active.TryPassTo(athlete,receiver,possession);
   else if(HostPlayer.WorldSport.Value==SportId.Football&&!FootballMatch.BlocksActions&&FootballBall.Instance&&abilityRevision==FootballBall.Instance.PropBallRevision)FootballBall.Instance.TryPassTo(athlete,receiver,possession);
  }
  [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)] void JumpRpc(uint round){if(AcceptRound(round)&&IsSpawned&&HostPlayer&&HostPlayer.Exploring.Value)athlete.RequestJump();}
  // Reliable begin/release edges let the host measure the hold duration. A
  // guest never supplies launch velocity, score or the identity of a shooter.
  [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)] void ShotChargeRpc(bool cancel,float heading,uint round){if(AcceptBasketball(round)&&IsSpawned&&HostPlayer&&HostPlayer.Exploring.Value&&HostPlayer.WorldSport.Value==SportId.Basketball&&BasketballBall.Active){if(cancel)BasketballBall.Active.CancelShotCharge(athlete);else BasketballBall.Active.BeginShotCharge(athlete,heading);}}
  [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)] void ShootRpc(float heading,double releasedAt,BasketballFinish finish,uint round){if(AcceptBasketball(round)&&IsSpawned&&HostPlayer&&HostPlayer.Exploring.Value&&HostPlayer.WorldSport.Value==SportId.Basketball&&BasketballBall.Active)BasketballBall.Active.ReleaseShotCharge(athlete,heading,releasedAt,finish);}
  [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)] void StealRpc(float heading,uint round){if(AcceptBasketball(round)&&IsSpawned&&HostPlayer&&HostPlayer.Exploring.Value&&HostPlayer.WorldSport.Value==SportId.Basketball&&BasketballBall.Active)BasketballBall.Active.TrySteal(athlete,heading);}
  [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)] void BlockRpc(float heading,bool jumping,uint play,uint round){if(AcceptBasketball(round)&&IsSpawned&&HostPlayer&&HostPlayer.Exploring.Value&&HostPlayer.WorldSport.Value==SportId.Basketball&&BasketballBall.Active)BasketballBall.Active.TryBlock(athlete,heading,jumping,play);}
  [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)] void PassChargeRpc(bool cancel,float heading,uint play,uint round){if(AcceptBasketball(round)&&IsSpawned&&HostPlayer&&HostPlayer.Exploring.Value&&HostPlayer.WorldSport.Value==SportId.Basketball&&BasketballBall.Active){if(cancel)BasketballBall.Active.CancelPassCharge(athlete,play);else BasketballBall.Active.BeginPassCharge(athlete,heading,play);}}
  [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)] void PassRpc(float heading,uint play,double releasedAt,float bend,uint round){if(AcceptBasketball(round)&&IsSpawned&&HostPlayer&&HostPlayer.Exploring.Value&&HostPlayer.WorldSport.Value==SportId.Basketball&&BasketballBall.Active)BasketballBall.Active.ReleasePassCharge(athlete,heading,play,releasedAt,bend);}
  [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)] public void BasketballFreeRoamRpc(bool enabled){if(IsSpectator)return;if(IsSpawned&&HostPlayer&&HostPlayer.Exploring.Value&&HostPlayer.WorldSport.Value==SportId.Basketball&&BasketballBall.Active)BasketballBall.Active.SetFreeRoam(athlete,enabled);}
  [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)] public void GolfCartRpc(GolfCartAction action,ulong cartOwner){if(IsSpectator)return;if(IsSpawned&&!(GolfMatchManager.Instance&&GolfMatchManager.Instance.IsAiming(athlete))&&HostPlayer&&HostPlayer.Exploring.Value&&HostPlayer.WorldSport.Value==SportId.Golf)GolfCartWorld.Execute(athlete,action,cartOwner);}
  [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)] public void GolfSwingRpc(ulong ballOwner,float heading,float charge,uint round){if(IsSpectator)return;if(IsSpawned&&HostPlayer&&HostPlayer.Exploring.Value&&HostPlayer.WorldSport.Value==SportId.Golf&&GolfMatchManager.Instance)GolfMatchManager.Instance.TrySwing(athlete,ballOwner,heading,charge,round);}
  [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)] public void GolfChargeRpc(bool charging,ulong ballOwner,float heading,uint round){if(IsSpectator)return;if(IsSpawned&&GolfMatchManager.Instance)GolfMatchManager.Instance.SetCharging(athlete,charging,ballOwner,heading,round);}
  [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)] public void FishingRpc(FishingAction action,uint round,uint sequence,int fish=-1,double pressedAt=double.NaN){if(IsSpectator)return;if(IsSpawned&&FishingGame.Instance)FishingGame.Instance.Execute(athlete,action,round,sequence,fish,pressedAt);}
  void Update(){if(RoomService.Paused||!IsSpawned||!AppRoot.Instance||!PlayerView.Instance)return;
   if(IsOwner&&IsPlayerObject){
    var c=PlayerView.Instance.ReadCommand();bool exploring=AppRoot.Instance.Exploring;
    if(!exploring)c.move=Vector2.zero;
    // Reliable action edges are sent this frame, independently of movement throttling.
    sendTimer-=Time.deltaTime;
    bool edge=!NetworkResponsiveness.Baseline&&NetworkResponsiveness.InputChanged(c,lastSentCommand);
    if(exploring&&!athlete.inTransit&&(FrameDrivenHost||sendTimer<=0||edge||c.jump||c.tackle||c.shoot||c.pass||c.footballPass||c.kick||c.cartAction!=GolfCartAction.None)){
     sendTimer=NetworkResponsiveness.Baseline?NetworkResponsiveness.Interval:sendTimer+NetworkResponsiveness.Interval;
     // Carry the periodic remainder, but never build a backlog after a hitch or
     // postpone the heartbeat after a burst of immediate direction changes.
     if(!NetworkResponsiveness.Baseline)sendTimer=Mathf.Clamp(sendTimer,0,NetworkResponsiveness.Interval);
     lastSentCommand=c;InputRpc(c.move,c.heading,c.sprint,c.dash&&exploring,c.pressure&&exploring,c.charging&&exploring,c.guard&&exploring,c.guardBlock&&exploring,c.defensePlay,MatchRevision,c.passBend,++inputSequence);
    }
    if((c.block||c.jumpBlock)&&exploring)BlockRpc(c.heading,c.jumpBlock,c.defensePlay,MatchRevision);
    if(c.steal&&exploring)StealRpc(c.heading,MatchRevision);
    if(c.passBegin&&exploring)PassChargeRpc(false,c.heading,c.passPlay,MatchRevision);if(c.passCancel)PassChargeRpc(true,c.heading,c.passPlay,MatchRevision);
    if(c.shotBegin&&exploring)ShotChargeRpc(false,c.heading,MatchRevision);if(c.shotCancel)ShotChargeRpc(true,c.heading,MatchRevision);
    if(c.kick&&exploring)KickRpc(c.kickCharge,MatchRevision,FootballBall.Instance?FootballBall.Instance.PropBallRevision:0,c.heading);if(c.jump&&exploring)JumpRpc(MatchRevision);if(c.tackle&&exploring)TackleRpc(MatchRevision);if(c.shoot&&exploring)ShootRpc(c.heading,c.shotReleasedAt,c.finish,MatchRevision);else if(c.pass&&exploring)PassRpc(c.heading,c.passPlay,c.passReleasedAt,c.passBend,MatchRevision);
    if(c.targetedPass&&exploring)TargetPassRpc(c.passReceiver,c.passPossession,MatchRevision,FootballBall.Instance?FootballBall.Instance.PropBallRevision:0);
    if(c.footballPass&&exploring)FootballPassRpc(c.heading,MatchRevision,FootballBall.Instance?FootballBall.Instance.PropBallRevision:0);
    if(c.cartAction!=GolfCartAction.None&&exploring)GolfCartRpc(c.cartAction,c.cartOwner);
    bool golfCharge=c.golfCharging&&exploring;
    if(sentGolfCharge!=golfCharge||sentGolfRound!=c.golfRound||golfCharge&&Mathf.Abs(Mathf.DeltaAngle(sentGolfHeading,c.heading))>3){sentGolfCharge=golfCharge;sentGolfRound=c.golfRound;sentGolfHeading=c.heading;GolfChargeRpc(golfCharge,c.golfBallOwner,c.heading,c.golfRound);}
    if(c.fishingAction!=FishingAction.None&&exploring)FishingRpc(c.fishingAction,c.fishingRound,c.fishingSequence,c.fishingFish,c.fishingPressedAt);
    if(c.golfSwing&&exploring)GolfSwingRpc(c.golfBallOwner,c.heading,c.golfCharge,c.golfRound);
   }
   if(FrameDrivenHost)SimulateAuthoritative(Time.deltaTime);
   if(!IsServer){GolfCartWorld.Receive(athlete,Cart.Value);athlete.BasketballFreeRoam=BasketballFreeRoam.Value;athlete.BasketballMotion.Receive(BasketballPose.Value);athlete.FootballMotion.Receive(FootballPose.Value);athlete.GolfClubMotion.Receive(GolfPose.Value);athlete.FootballEffort.Receive(Effort.Value);athlete.ApplySnapshot(Motion.Value);athlete.ApplyJump(Jump.Value);athlete.ApplyFootball(Football.Value,NetworkManager.ServerTime.Time);}
  }
  void FixedUpdate(){if(IsSpawned&&IsServer&&!FrameDrivenHost)SimulateAuthoritative(Time.fixedDeltaTime);}
  void SimulateAuthoritative(float dt){
   // Waiting-room avatars are stationary. Avoid publishing timestamp-only motion
   // snapshots for every occupant to every peer while the menu is open.
   if(!HostPlayer||!HostPlayer.Exploring.Value){command=default;return;}
   Cart.Value=GolfCartWorld.State(athlete);
   if(SkySailWorld.Instance&&SkySailWorld.Instance.Travelling){command=default;athlete.FootballEffort.Stop();Effort.Value=athlete.FootballEffort.Snapshot;return;}
   var c=command;
   if(BotSlot.Value==0&&(Time.time-lastInput>.25f||!HostPlayer||!HostPlayer.Exploring.Value)){c.move=Vector2.zero;c.charging=c.guard=c.guardBlock=c.dash=c.pressure=false;if(BasketballBall.Active)BasketballBall.Active.CancelPassCharge(athlete,BasketballBall.Active.Defense.play);}
   if(BotSlot.Value>0)c=BotSport.Value==SportId.Basketball?GetComponent<BasketballBot>().Read(dt):GetComponent<FootballBot>().Read(dt);
   athlete.Simulate(c,dt);
   Effort.Value=athlete.FootballEffort.Snapshot;BasketballFreeRoam.Value=athlete.BasketballFreeRoam;Cart.Value=GolfCartWorld.State(athlete);BasketballPose.Value=athlete.BasketballMotion.State;FootballPose.Value=athlete.FootballMotion.State;GolfPose.Value=athlete.GolfClubMotion.State;Jump.Value=athlete.JumpState();Motion.Value=athlete.Snapshot(NetworkManager.ServerTime.Time);Football.Value=athlete.FootballState(NetworkManager.ServerTime.Time);
  }
 }
}
