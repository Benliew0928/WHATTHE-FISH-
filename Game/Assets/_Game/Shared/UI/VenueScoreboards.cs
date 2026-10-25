using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace WhatTheFish {
 public static class VenueScoreboards {
  // Upgrade existing serialized courts too; a fresh scene generation follows the same path.
  public static void AttachBasketball(BasketballArenaView arena){
   foreach(var sign in arena.GetComponentsInChildren<StadiumSign>(true)){
    var canvas=sign.GetComponent<Canvas>();if(!sign.nameSign||!canvas||sign.GetComponentInChildren<SportsScoreboard>(true))continue;
    var rect=(RectTransform)sign.transform;string title=sign.label?sign.label.text:"RALLY COURT";
    // The old perimeter faces sit behind the upper arcade. Move the complete
    // modular screen and its piers onto the court side of the arches.
    var mount=arena.GetComponentsInChildren<MeshRenderer>(true).Where(r=>r.name.StartsWith("PerimeterScreen_")).OrderBy(r=>(r.bounds.center-sign.transform.position).sqrMagnitude).FirstOrDefault();
    if(mount){var prefix=mount.name.Substring(0,mount.name.LastIndexOf('_')+1);var offset=-sign.transform.forward*1.6f;foreach(var part in arena.GetComponentsInChildren<MeshRenderer>(true).Where(r=>r.name.StartsWith(prefix)))part.transform.position+=offset;sign.transform.position+=offset;}
    foreach(var text in sign.GetComponentsInChildren<Text>(true))text.gameObject.SetActive(false);
    var board=SportsScoreboard.Create(sign.transform,SportId.Basketball,400,true);
    float scale=Mathf.Min(rect.rect.width/400,rect.rect.height/194);board.transform.localScale=Vector3.one*scale;
    ((RectTransform)board.transform).anchoredPosition=new Vector2((rect.rect.width-400*scale)*.5f,-(rect.rect.height-194*scale)*.5f);
    sign.label=board.Title;sign.label.text=title;
   }
  }
 }
}
