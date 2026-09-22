using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class SceneAudit {
 public static void SetupAndRender(){ProjectBuilder.Setup();RenderPreview();}
 public static void RenderPreview(){
  EditorSceneManager.OpenScene("Assets/_Game/Scenes/Bootstrap.unity");
  var camera=Camera.main;camera.transform.position=new Vector3(10,4,-40);camera.transform.LookAt(new Vector3(0,11,-71));
  ShaderUtil.allowAsyncCompilation=false;Canvas.ForceUpdateCanvases();var rt=new RenderTexture(1600,900,24);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
  var texture=new Texture2D(1600,900,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,1600,900),0,0);texture.Apply();File.WriteAllBytes("../Builds/unity-screen-review.png",texture.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=null;Object.DestroyImmediate(rt);Object.DestroyImmediate(texture);
 }
 public static void Run(){EditorSceneManager.OpenScene("Assets/_Game/Scenes/Bootstrap.unity");var lines=Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Where(t=>t.name.Contains("screen")||t.name=="Screen"||t.name.Contains("Stadium")||t.name=="Ground").Select(t=>t.name+" pos="+t.position+" local="+t.localPosition+" rotation="+t.rotation.eulerAngles+" scale="+t.lossyScale).ToList();var athlete=PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Resources/OfflineAthlete.prefab")) as GameObject;foreach(var r in athlete.GetComponentsInChildren<Renderer>())lines.Add(r.name+" center="+r.bounds.center+" size="+r.bounds.size);File.WriteAllLines("../Builds/scene-audit.txt",lines);}
}
