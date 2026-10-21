using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 // Appearance uses the existing room payload: palette = island accents,
 // design = equipment finish, screen = welcome-sign style.
 public sealed class IslandVenueStyle:MonoBehaviour {
  public StadiumAppearance Appearance {get;private set;}
  public Text Welcome {get;private set;}
  public Transform Equipment {get;private set;}
  MaterialPropertyBlock block;
  public int AccentSlots {get;private set;}
  public static IslandVenueStyle Apply(StadiumView view,StadiumAppearance appearance){
   var style=view.GetComponent<IslandVenueStyle>()??view.gameObject.AddComponent<IslandVenueStyle>();
   style.ApplyLook(view,appearance);return style;
  }
  void ApplyLook(StadiumView view,StadiumAppearance appearance){
   block??=new MaterialPropertyBlock();
   appearance.Clamp();Appearance=appearance;bool golf=view is GolfIslandView;
   if(!Welcome)Welcome=GetComponentsInChildren<Text>(true).FirstOrDefault(t=>t.transform.parent.name.StartsWith("Wayfinding LIMESTONE"));
   if(Welcome){Welcome.supportRichText=false;Welcome.text=appearance.title.ToUpperInvariant()+"\n"+(appearance.screen==1?"WELCOME, TEAM "+new[]{"MINT","ROSE","LILAC","GOLD"}[appearance.palette]:golf?"FIVE HOLES / COASTAL LINKS":"FIVE PIERS / SHARED SHORE");view.title=Welcome;}
   if(!Equipment)Equipment=transform.Find(golf?"Golf Equipment":"Fishing Rods");
   if(Equipment)foreach(Transform child in Equipment){
    if(golf?(child.name=="Driver"||child.name=="Iron"||child.name=="Putter"):(child.name=="BlueLime"||child.name=="TealOrange"))
     VenueEquipmentFinish.Attach(child.gameObject,golf?SportId.Golf:SportId.Fishing).ApplyFinish(appearance.design);
   }
   AccentSlots=0;
   if(view is FishingLagoonView lagoon){
    foreach(var pier in lagoon.playerStands)if(pier){pier.SetAccent(LocalProfile.Teams[appearance.palette]);AccentSlots++;}
   }else{
    var course=GetComponentInChildren<GolfCourse>(true);
    if(course)foreach(var hole in course.holes)if(hole.flag)foreach(var renderer in hole.flag.GetComponentsInChildren<Renderer>(true)){
     var materials=renderer.sharedMaterials;
     for(int i=0;i<materials.Length;i++){
      var material=materials[i];if(!material||!(material.name.StartsWith("Reference hole flag")||material.name.StartsWith("Flag teal bindings")))continue;
      renderer.GetPropertyBlock(block,i);
      if(material.HasProperty("_VenueAccent")){var color=LocalProfile.Teams[appearance.palette];color.a=appearance.palette==0?0:1;block.SetColor("_VenueAccent",color);}
      else if(material.HasProperty("_BaseColor"))block.SetColor("_BaseColor",appearance.palette==0?material.GetColor("_BaseColor"):LocalProfile.Teams[appearance.palette]);
      renderer.SetPropertyBlock(block,i);block.Clear();AccentSlots++;
     }
    }
   }
  }
  public static void Frame(Camera camera,StadiumView view,int mode){
   var style=view.GetComponent<IslandVenueStyle>();if(!style)return;
   if(mode==1&&style.Equipment){
    var renderers=style.Equipment.GetComponentsInChildren<Renderer>().Where(r=>r.enabled).ToArray();if(renderers.Length==0)return;
    var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
    var focus=bounds.center;float distance=Mathf.Max(2.2f,bounds.size.y*2.3f);
    camera.transform.position=focus+style.Equipment.forward*distance+Vector3.up*.25f;camera.transform.LookAt(focus);camera.fieldOfView=34;
   }else if(mode==2){
    if(view is FishingLagoonView lagoon&&lagoon.playerStands.Length>0){
     var pier=lagoon.playerStands[0].transform;var focus=pier.position+Vector3.up*1.2f;
     camera.transform.position=focus+pier.forward*9+Vector3.up*5;camera.transform.LookAt(focus);camera.fieldOfView=46;
    }else{
     var course=view.GetComponentInChildren<GolfCourse>(true);if(!course||course.holes.Length==0||!course.holes[0].flag)return;
     var flag=course.holes[0].flag;var renderers=flag.GetComponentsInChildren<Renderer>();if(renderers.Length==0)return;
     var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
     camera.transform.position=bounds.center+flag.forward*6+Vector3.up*.3f;camera.transform.LookAt(bounds.center);camera.fieldOfView=38;
    }
   }else if(style.Welcome){
    var sign=style.Welcome.transform;camera.transform.position=sign.position-sign.forward*6;camera.transform.LookAt(sign.position);camera.fieldOfView=36;
   }
  }
 }
}
