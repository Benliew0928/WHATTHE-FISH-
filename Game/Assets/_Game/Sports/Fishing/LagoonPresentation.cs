using UnityEngine;

namespace WhatTheFish {
 // Owns the opening in the shared ocean while the detailed lagoon is loaded.
 // This is presentation only; no catch, scoring or network state lives here.
 public sealed class LagoonPresentation : MonoBehaviour {
  static readonly int Cutout=Shader.PropertyToID("_FishingLagoonCutout");
  public const float Radius=28f;
  void OnEnable(){UpdateCutout();}
  void LateUpdate(){UpdateCutout();}
  void UpdateCutout(){var p=transform.position;Shader.SetGlobalVector(Cutout,new Vector4(p.x,p.z,Radius-.06f,1));}
  void OnDisable(){Shader.SetGlobalVector(Cutout,Vector4.zero);}
 }
}
