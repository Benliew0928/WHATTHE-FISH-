using UnityEngine;
namespace WhatTheFish {
 [CreateAssetMenu(menuName="Sports/Sport Definition")]
 public sealed class SportDefinition:ScriptableObject {
  public SportId id;public string displayName;public bool available;public int maxPlayers=10;
  public GameObject environmentPrefab;public Vector3[] spawnPositions;
  public Vector3 menuCamera,menuFocus;public float elevatedDistance=19;
  public float farClip=450,fogStart=180,fogEnd=430;
  public Color ambientColor=Color.gray,backgroundColor=Color.gray;public Vector3 lightRotation;public float lightIntensity=1.1f;public bool indoor;
  public Vector3 Spawn(int index)=>spawnPositions!=null&&spawnPositions.Length>0?spawnPositions[Mathf.Abs(index)%spawnPositions.Length]:new Vector3(0,1,0);
 }
}
