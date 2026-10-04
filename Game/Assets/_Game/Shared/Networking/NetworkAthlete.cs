using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace WhatTheFish {
 public sealed class NetworkAthlete:NetworkBehaviour {
  public static NetworkAthlete HostPlayer;
  public NetworkVariable<bool> Ready=new(false);
  public NetworkVariable<bool> Exploring=new(false);
  public NetworkVariable<SportId> WorldSport=new(SportId.Football);
  public NetworkVariable<SkySailJourney> WorldTravel=new();
  public NetworkVariable<uint> TravelReady=new();
  public NetworkVariable<FixedString512Bytes> WorldAppearance=new();
  public NetworkVariable<FixedString128Bytes> AppearanceData=new();
  public NetworkVariable<LocomotionSnapshot> Motion=new();
  public NetworkVariable<JumpSnapshot> Jump=new();
  public NetworkVariable<FootballSnapshot> Football=new();
  public NetworkVariable<BasketballSnapshot> Basketball=new();
  public NetworkVariable<FootballBallSnapshot> Ball=new();
  public NetworkVariable<FootballMatchSnapshot> Match=new();
  public NetworkVariable<FootballTeam> Team=new(FootballTeam.None);
  public void ClearMatchInput(){command=default;lastInput=0;}
  uint MatchRevision=>HostPlayer?HostPlayer.Match.Value.revision:0;
  bool AcceptRound(uint round)=>!FootballMatch.Instance||!FootballMatch.Instance.Context||!FootballMatch.BlocksMovement&&round==MatchRevision;
  public NetworkVariable<BasketballMotionState> BasketballPose=new();
  public NetworkVariable<GolfCartState> Cart=new();
  public NetworkVariable<GolfMatchSnapshot> GolfMatch=new();
  public NetworkList<GolfPlayerRecord> GolfPlayers=new();
  public NetworkVariable<GolfBallSnapshot> GolfBall=new();
  public NetworkVariable<GolfClubState> GolfPose=new();
  bool sentGolfCharge;uint sentGolfRound;float sentGolfHeading;
  Athlete athlete; PlayerCommand command; float lastInput; float sendTimer;
  public override void OnNetworkSpawn(){
   athlete=GetComponent<Athlete>();athlete.Setup();athlete.capsule.enabled=IsServer;
   if(IsServer){athlete.capsule.enabled=false;var used=NetworkManager.ConnectedClientsList.Where(c=>c.ClientId!=OwnerClientId&&c.PlayerObject).Select(c=>c.PlayerObject.transform.position).ToArray();var spawns=AppRoot.Instance.environments.Definition(AppRoot.Instance.rooms.Sport).spawnPositions;var spawn=spawns.FirstOrDefault(p=>used.All(q=>Vector3.Distance(p,q)>1));if(spawn==Vector3.zero)spawn=spawns[0];transform.position=spawn;GetComponent<Unity.Netcode.Components.NetworkTransform>().Teleport(spawn,Quaternion.identity,Vector3.one);athlete.capsule.enabled=true;}
   if(OwnerClientId==NetworkManager.ServerClientId){HostPlayer=this;if(IsServer){WorldSport.Value=AppRoot.Instance.rooms.Sport;WorldAppearance.Value=JsonUtility.ToJson(LocalProfile.ForSport(WorldSport.Value));}}
   AppearanceData.OnValueChanged+=OnAppearance;OnAppearance(default,AppearanceData.Value);
   if(IsOwner){AppearanceRpc(JsonUtility.ToJson(LocalProfile.Character));AppRoot.Instance.LocalAthlete=athlete;}
  }
  public override void OnNetworkDespawn(){GolfCartWorld.Forget(athlete);AppearanceData.OnValueChanged-=OnAppearance;if(HostPlayer==this)HostPlayer=null;}
  void OnAppearance(FixedString128Bytes oldValue,FixedString128Bytes newValue){if(newValue.Length>0)athlete.Appearance(JsonUtility.FromJson<CharacterAppearance>(newValue.ToString()));}
  [Rpc(SendTo.Server)] public void AppearanceRpc(FixedString128Bytes json){var a=JsonUtility.FromJson<CharacterAppearance>(json.ToString());a.Clamp();AppearanceData.Value=JsonUtility.ToJson(a);}
  [Rpc(SendTo.Server)] public void ReadyRpc(bool value){Ready.Value=value;}
  [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)] public void ChooseFootballTeamRpc(FootballTeam team,uint selectionRevision){if(IsSpawned&&FootballMatch.Instance)FootballMatch.Instance.ChooseTeam(athlete,team,selectionRevision);}
  [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)] public void TravelReadyRpc(uint sequence){if(HostPlayer&&HostPlayer.WorldTravel.Value.phase==SkySailPhase.Preparing&&HostPlayer.WorldTravel.Value.sequence==sequence)TravelReady.Value=sequence;}
  [Rpc(SendTo.Server,Delivery=RpcDelivery.Unreliable,InvokePermission=RpcInvokePermission.Owner)] void InputRpc(Vector2 move,float heading,bool sprint,bool charging,uint round){
   if(float.IsNaN(move.x)||float.IsNaN(move.y)||float.IsNaN(heading)||float.IsInfinity(heading)||float.IsInfinity(move.x)||float.IsInfinity(move.y))return;
   if(!AcceptRound(round))return;
   command=new PlayerCommand{move=Vector2.ClampMagnitude(move,1),heading=heading%360,sprint=sprint,charging=charging};lastInput=Time.time;
  }
  [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)] void TackleRpc(uint round){if(AcceptRound(round)&&IsSpawned&&HostPlayer&&HostPlayer.Exploring.Value&&HostPlayer.WorldSport.Value==SportId.Football)athlete.TryTackle();}
  [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)] void KickRpc(float charge,uint round){if(float.IsNaN(charge)||float.IsInfinity(charge))return;if(AcceptRound(round)&&IsSpawned&&HostPlayer&&HostPlayer.Exploring.Value&&HostPlayer.WorldSport.Value==SportId.Football)athlete.TryKick(Mathf.Clamp01(charge));}
  [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)] void JumpRpc(uint round){if(AcceptRound(round)&&IsSpawned&&HostPlayer&&HostPlayer.Exploring.Value)athlete.RequestJump();}
  // The caller can request a shot only for their own avatar. The host chooses
  // the hoop and velocity; clients never supply a ball pose, owner or power.
  [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)] void ShootRpc(float heading){if(IsSpawned&&HostPlayer&&HostPlayer.Exploring.Value&&HostPlayer.WorldSport.Value==SportId.Basketball&&BasketballBall.Active)BasketballBall.Active.TryShoot(athlete,heading);}
  [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)] void PassRpc(float heading){if(IsSpawned&&HostPlayer&&HostPlayer.Exploring.Value&&HostPlayer.WorldSport.Value==SportId.Basketball&&BasketballBall.Active)BasketballBall.Active.TryPass(athlete,heading);}
  [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)] public void GolfCartRpc(GolfCartAction action,ulong cartOwner){if(IsSpawned&&HostPlayer&&HostPlayer.Exploring.Value&&HostPlayer.WorldSport.Value==SportId.Golf)GolfCartWorld.Execute(athlete,action,cartOwner);}
  [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)] public void GolfSwingRpc(ulong ballOwner,float heading,float charge,uint round){if(IsSpawned&&HostPlayer&&HostPlayer.Exploring.Value&&HostPlayer.WorldSport.Value==SportId.Golf&&GolfMatchManager.Instance)GolfMatchManager.Instance.TrySwing(athlete,ballOwner,heading,charge,round);}
  [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)] public void GolfChargeRpc(bool charging,ulong ballOwner,float heading,uint round){if(IsSpawned&&GolfMatchManager.Instance)GolfMatchManager.Instance.SetCharging(athlete,charging,ballOwner,heading,round);}
  void Update(){if(!IsSpawned)return;
   if(IsOwner){
    var c=PlayerView.Instance.ReadCommand();bool exploring=AppRoot.Instance.Exploring;
    if(!exploring)c.move=Vector2.zero;
    // Reliable action edges are sent this frame, independently of movement throttling.
    if((sendTimer-=Time.deltaTime)<=0||c.jump||c.tackle||c.shoot||c.pass||c.kick||c.cartAction!=GolfCartAction.None){sendTimer=1f/30;InputRpc(c.move,c.heading,c.sprint,c.charging&&exploring,MatchRevision);}
    if(c.kick&&exploring)KickRpc(c.kickCharge,MatchRevision);if(c.jump&&exploring)JumpRpc(MatchRevision);if(c.tackle&&exploring)TackleRpc(MatchRevision);if(c.shoot&&exploring)ShootRpc(c.heading);else if(c.pass&&exploring)PassRpc(c.heading);
    if(c.cartAction!=GolfCartAction.None&&exploring)GolfCartRpc(c.cartAction,c.cartOwner);
    bool golfCharge=c.golfCharging&&exploring;
    if(sentGolfCharge!=golfCharge||sentGolfRound!=c.golfRound||golfCharge&&Mathf.Abs(Mathf.DeltaAngle(sentGolfHeading,c.heading))>3){sentGolfCharge=golfCharge;sentGolfRound=c.golfRound;sentGolfHeading=c.heading;GolfChargeRpc(golfCharge,c.golfBallOwner,c.heading,c.golfRound);}
    if(c.golfSwing&&exploring)GolfSwingRpc(c.golfBallOwner,c.heading,c.golfCharge,c.golfRound);
   }
   if(!IsServer){GolfCartWorld.Receive(athlete,Cart.Value);athlete.BasketballMotion.Receive(BasketballPose.Value);athlete.GolfClubMotion.Receive(GolfPose.Value);athlete.ApplySnapshot(Motion.Value);athlete.ApplyJump(Jump.Value);athlete.ApplyFootball(Football.Value,NetworkManager.ServerTime.Time);}
  }
  void FixedUpdate(){if(!IsSpawned||!IsServer)return;Cart.Value=GolfCartWorld.State(athlete);if(SkySailWorld.Instance&&SkySailWorld.Instance.Travelling){command=default;return;}var c=command;if(Time.time-lastInput>.25f||!HostPlayer||!HostPlayer.Exploring.Value){c.move=Vector2.zero;c.charging=false;}athlete.Simulate(c,Time.fixedDeltaTime);Cart.Value=GolfCartWorld.State(athlete);BasketballPose.Value=athlete.BasketballMotion.State;GolfPose.Value=athlete.GolfClubMotion.State;Jump.Value=athlete.JumpState();Motion.Value=athlete.Snapshot(NetworkManager.ServerTime.Time);Football.Value=athlete.FootballState(NetworkManager.ServerTime.Time);}
 }
}
