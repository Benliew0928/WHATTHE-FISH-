using UnityEngine;
using UnityEngine.UI;
using static WhatTheFish.CoveUI;

namespace WhatTheFish {
 public sealed class PlayerProfilePresence:MonoBehaviour {
  RectTransform hud;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
  static void StartPresence(){if(!FindFirstObjectByType<PlayerProfilePresence>()){var go=new GameObject("Live player profiles");DontDestroyOnLoad(go);go.AddComponent<PlayerProfilePresence>();}}
  void Awake(){
   var c=gameObject.AddComponent<Canvas>();c.renderMode=RenderMode.ScreenSpaceOverlay;c.sortingOrder=24000;gameObject.AddComponent<GraphicRaycaster>();
   hud=Rect(transform,"Crew controls",0,0,190,62);
   Button(hud,"Open room profiles","Players",0,0,190,62,Cream,SocialPanel.OpenRoom,CoveSymbol.People,24);
   CharacterIdentityBadges.ReserveHUD(hud);
  }
  void LateUpdate(){
   bool show=AppRoot.Instance&&AppRoot.Instance.Exploring&&AppRoot.Instance.rooms.Connected&&!SocialPanel.Visible&&!AccountPanel.Visible;
   hud.gameObject.SetActive(show);
   var safe=Screen.safeArea;float scale=Mathf.Clamp(Mathf.Min(safe.width/1600,safe.height/900),.35f,2);
   hud.localScale=Vector3.one*scale;hud.anchoredPosition=new Vector2(safe.xMax-210*scale,-(Screen.height-safe.yMax+112*scale));
  }
 }
}
