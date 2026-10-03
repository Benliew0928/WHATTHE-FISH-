using System.IO;
using UnityEditor;
using UnityEngine;
using WhatTheFish;
[InitializeOnLoad]
public static class FootballMatchChecks {
 static FootballMatchChecks(){EditorApplication.playModeStateChanged+=Changed;}
 [MenuItem("WHATTHE FISH?/Football/Verify match rules")]
 public static void Run(){if(EditorApplication.isPlaying)return;SessionState.SetBool("FootballMatchChecks",true);EditorApplication.isPlaying=true;}
 static void Changed(PlayModeStateChange state){if(state!=PlayModeStateChange.EnteredPlayMode||!SessionState.GetBool("FootballMatchChecks",false))return;SessionState.SetBool("FootballMatchChecks",false);var probe=new GameObject("Football match checks").AddComponent<FootballMatchProbe>();probe.ReportPath=Path.GetFullPath(Path.Combine(Application.dataPath,"../../Builds/FootballMatchQA/editor.txt"));}
 [MenuItem("WHATTHE FISH?/Football/Build match verification player")]
 public static void Build(){ProjectBuilder.BuildFootballMatchVerification();}
 [MenuItem("WHATTHE FISH?/Football/Start single-player test match")]
 public static void StartSinglePlayerTest(){if(EditorApplication.isPlaying&&FootballMatch.Instance)FootballMatch.Instance.StartTestMatch();}
}
