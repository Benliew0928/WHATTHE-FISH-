using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class SceneAudit {
 public static void Run(){EditorSceneManager.OpenScene("Assets/_Game/Scenes/Bootstrap.unity");var lines=Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Where(t=>t.name.Contains("screen")||t.name=="Screen"||t.name.Contains("Stadium")||t.name=="Ground").Select(t=>t.name+" pos="+t.position+" local="+t.localPosition+" rotation="+t.rotation.eulerAngles+" scale="+t.lossyScale).ToList();var athlete=PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Resources/OfflineAthlete.prefab")) as GameObject;foreach(var r in athlete.GetComponentsInChildren<Renderer>())lines.Add(r.name+" center="+r.bounds.center+" size="+r.bounds.size);File.WriteAllLines("../Builds/scene-audit.txt",lines);}
}
