using UnityEngine;
namespace WhatTheFish {
 [CreateAssetMenu(menuName="WHATTHE FISH?/Football match rules")]
 public sealed class FootballMatchRules:ScriptableObject {
  [Min(.01f)] public float regulationSeconds=180,overtimeSeconds=60;
  [Min(0)] public float kickoffSeconds=3;
  [Min(0)] public float teamSelectionSeconds=10;
  [Tooltip("Full painted goal-line width, in pitch metres. Authored markings use 0.12 m.")]
  [Min(0)] public float goalLineWidth=.12f;
  [Header("Kickoff formations (pitch metres from the existing ball kickoff point)")]
  [Tooltip("X is lateral offset; Y is distance toward the team's defending goal. The other team is mirrored. All players face the ball.")]
  public Vector2[] onePlayer={new(0,9.5f)};
  public Vector2[] twoPlayers={new(-3.5f,9.5f),new(3.5f,9.5f)};
  public Vector2[] threePlayers={new(0,9.5f),new(-3.5f,13.5f),new(3.5f,13.5f)};
  public Vector2[] fourPlayers={new(0,9.5f),new(-3.5f,13.5f),new(3.5f,13.5f),new(0,17.5f)};
  public Vector2[] fivePlayers={new(0,9.5f),new(-3.5f,13.5f),new(3.5f,13.5f),new(-3.5f,17.5f),new(3.5f,17.5f)};
  public Vector2[] Formation(int teamSize)=>teamSize switch {1=>onePlayer,2=>twoPlayers,3=>threePlayers,4=>fourPlayers,5=>fivePlayers,_=>null};
 }
}
