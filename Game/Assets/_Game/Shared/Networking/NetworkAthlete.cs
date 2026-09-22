using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace SportsPrototype {
 public sealed class NetworkAthlete:NetworkBehaviour {
  public static NetworkAthlete HostPlayer;
  public NetworkVariable<bool> Ready=new(false);
  public NetworkVariable<bool> Exploring=new(false);
  public NetworkVariable<FixedString512Bytes> WorldAppearance=new();
  public NetworkVariable<FixedString128Bytes> AppearanceData=new();
  public NetworkVariable<float> MoveSpeed=new();
  Athlete athlete; PlayerCommand command; float lastInput; float sendTimer;
  public override void OnNetworkSpawn(){
   athlete=GetComponent<Athlete>();athlete.Setup();athlete.capsule.enabled=IsServer;
   if(IsServer){athlete.capsule.enabled=false;var spawn=new Vector3(((int)OwnerClientId%5-2)*2,1,-8-((int)OwnerClientId/5)*3);transform.position=spawn;GetComponent<Unity.Netcode.Components.NetworkTransform>().Teleport(spawn,Quaternion.identity,Vector3.one);athlete.capsule.enabled=true;}
   if(OwnerClientId==NetworkManager.ServerClientId){HostPlayer=this;if(IsServer)WorldAppearance.Value=JsonUtility.ToJson(LocalProfile.Stadium);}
   AppearanceData.OnValueChanged+=OnAppearance;OnAppearance(default,AppearanceData.Value);
   if(IsOwner){AppearanceRpc(JsonUtility.ToJson(LocalProfile.Character));AppRoot.Instance.LocalAthlete=athlete;}
  }
  public override void OnNetworkDespawn(){AppearanceData.OnValueChanged-=OnAppearance;if(HostPlayer==this)HostPlayer=null;}
  void OnAppearance(FixedString128Bytes oldValue,FixedString128Bytes newValue){if(newValue.Length>0)athlete.Appearance(JsonUtility.FromJson<CharacterAppearance>(newValue.ToString()));}
  [Rpc(SendTo.Server)] public void AppearanceRpc(FixedString128Bytes json){var a=JsonUtility.FromJson<CharacterAppearance>(json.ToString());a.Clamp();AppearanceData.Value=JsonUtility.ToJson(a);}
  [Rpc(SendTo.Server)] public void ReadyRpc(bool value){Ready.Value=value;}
  [Rpc(SendTo.Server,Delivery=RpcDelivery.Unreliable)] void InputRpc(Vector2 move,float heading,bool sprint){
   if(float.IsNaN(move.x)||float.IsNaN(move.y)||float.IsNaN(heading)||float.IsInfinity(heading)||float.IsInfinity(move.x)||float.IsInfinity(move.y))return;
   command=new PlayerCommand{move=Vector2.ClampMagnitude(move,1),heading=heading%360,sprint=sprint};lastInput=Time.time;
  }
  void Update(){if(!IsSpawned)return;
   if(IsOwner&&(sendTimer-=Time.deltaTime)<=0){sendTimer=1f/30;var c=PlayerView.Instance.ReadCommand();if(!AppRoot.Instance.Exploring)c.move=Vector2.zero;InputRpc(c.move,c.heading,c.sprint);}
   if(!IsServer)athlete.speed=MoveSpeed.Value;
  }
  void FixedUpdate(){if(!IsSpawned||!IsServer)return;var c=command;if(Time.time-lastInput>.25f||!HostPlayer||!HostPlayer.Exploring.Value)c.move=Vector2.zero;athlete.Simulate(c,Time.fixedDeltaTime);MoveSpeed.Value=athlete.speed;}
 }
}
