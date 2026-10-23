using UnityEngine;
namespace WhatTheFish {
 [DefaultExecutionOrder(200)]
 public sealed class StadiumCameraPresenter:MonoBehaviour {
  PlayerView view;
  void Awake(){view=GetComponent<PlayerView>();}
  void LateUpdate(){if(view&&!TeleportSequence.OwnsCamera){view.PresentStadiumCamera();view.PresentActivityStadiumCamera();}}
 }
}
