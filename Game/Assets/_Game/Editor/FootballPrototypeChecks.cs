using System.IO;
using UnityEditor;
using UnityEngine;
using WhatTheFish;
[InitializeOnLoad]
public static class FootballPrototypeChecks {
 static FootballPrototypeChecks(){EditorApplication.playModeStateChanged+=Changed;}
 [MenuItem("WHATTHE FISH?/Football/Verify physics prototype")]
 public static void Run(){if(EditorApplication.isPlaying)return;SessionState.SetBool("FootballPhysicsChecks",true);EditorApplication.isPlaying=true;}
 static void Changed(PlayModeStateChange state){if(state!=PlayModeStateChange.EnteredPlayMode||!SessionState.GetBool("FootballPhysicsChecks",false))return;SessionState.SetBool("FootballPhysicsChecks",false);var probe=new GameObject("Football physics checks").AddComponent<FootballBallProbe>();probe.ReportPath=Path.GetFullPath(Path.Combine(Application.dataPath,"../../Builds/FootballPrototype/offline.txt"));}
 [MenuItem("WHATTHE FISH?/Football/Build verification Windows player")]
 public static void BuildPlayer(){SkySailBuilder.RebuildPlayer();}
}
