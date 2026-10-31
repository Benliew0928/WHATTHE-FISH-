using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 // The only builders for layout-editable control artwork. Gameplay adds input
 // separately; menu previews call these same builders without creating players.
 public static partial class ControlDesigns {
  public sealed class View {
   public RectTransform Root,Visual,Thumb,IconRoot,PressVisual;
   public bool Glossy;public string SkinStyle;
   public Graphic Face;
   public Image Art;
   public Text Label,Hint,Badge;
   public BasketballPlate Meter;
   public BasketballPropProgress Progress;
   public FishingSkillGraphic Ring;
   public Graphic[] Arrows;
   public int LabelPoints;
   public Color FaceColor;
   public Color LabelColor;
   public Vector2 LabelPosition,LabelSize;
  }
  static GolfHUDLayout golf;static FishingHUDLayout fishing;static Sprite circle;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
  public static System.Action<SportId,string,View> ReviewDesign;
#endif
  public static GolfHUDLayout GolfLayout=>golf?golf:golf=Resources.Load<GolfHUDLayout>("GolfUILayout")??ScriptableObject.CreateInstance<GolfHUDLayout>();
  public static FishingHUDLayout FishingLayout=>fishing?fishing:fishing=Resources.Load<FishingHUDLayout>("FishingUILayout")??ScriptableObject.CreateInstance<FishingHUDLayout>();
  public static Vector2 Size(SportId sport,string id){
   if(sport==SportId.Basketball)return id switch{"move"=>new(108,108),"shoot"=>new(106,106),"pass" or "ability"=>new(80,80),"jump"=>new(64,64),_=>throw new System.ArgumentException(id)};
   if(sport==SportId.Football)return id switch{"move" or "dash"=>new(108,108),"pass"=>new(66,72),"tackle"=>new(76,79),"shoot" or "ability"=>new(80,80),_=>throw new System.ArgumentException(id)};
   if(sport==SportId.Golf){var c=GolfLayout;return id switch{"move"=>c.joystickSize,"jump"=>c.jumpSize,"power"=>new(210,350),"aim"=>c.aimSize,"start"=>c.startSize,"buggy" or "drive"=>c.cartSize,_=>throw new System.ArgumentException(id)};}
   var f=FishingLayout;return id switch{"move"=>f.joystickSize,"jump"=>Size(SportId.Basketball,"jump")*2,"cast"=>f.actionSize,"left"=>f.fightControlSize,"cancel"=>f.cancelSize,"ability"=>f.skillButtonSize,_=>throw new System.ArgumentException(id)};
  }
  public static View Build(RectTransform root,SportId sport,string id,Text label=null){
   root.sizeDelta=Size(sport,id);
   var view=new View{Root=root,Visual=root,Label=label};
   if(sport==SportId.Basketball||sport==SportId.Football)Court(view,sport,id);
   else if(sport==SportId.Golf)Golf(view,id);
   else Fishing(view,id);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
   ReviewDesign?.Invoke(sport,id,view);
#endif
   view.LabelPoints=view.Label?view.Label.fontSize:0;view.FaceColor=view.Face?view.Face.color:Color.white;
   if(view.Label){view.LabelColor=view.Label.color;view.LabelPosition=view.Label.rectTransform.anchoredPosition;view.LabelSize=view.Label.rectTransform.sizeDelta;}
   var source=root.gameObject.AddComponent<ControlDesignSource>();source.Sport=sport;source.Id=id;source.View=view;
   return view;
  }
  public static void Caption(Text label,string value,float scale=1){
   var source=label.GetComponentInParent<ControlDesignSource>();label.text=source&&source.View.Glossy?value.ToUpperInvariant():value;
   if(source&&source.View.Label==label){label.fontSize=Mathf.RoundToInt(source.View.LabelPoints*scale);if(source.View.Glossy&&label.resizeTextForBestFit){label.resizeTextMaxSize=label.fontSize;label.resizeTextMinSize=Mathf.RoundToInt(label.fontSize*.75f);}}
  }
  public static Color FaceColor(Button button){var source=button.GetComponent<ControlDesignSource>();return source?source.View.FaceColor:button.targetGraphic.color;}
  // All geometry/typography lives here or in the sport partials. Input setup
  // uses this helper only after Build; it is never called for menu samples.
  public static Button BindButton(View view,bool feedback=true,RectTransform inputRoot=null){
   var root=inputRoot?inputRoot:view.Root;var button=root.GetComponent<Button>()??root.gameObject.AddComponent<TouchButton>();
   if(view.Glossy){var hit=root.GetComponent<Image>()??root.gameObject.AddComponent<Image>();hit.enabled=true;hit.sprite=circle?circle:circle=AppRoot.MakeCircle();hit.color=Color.clear;hit.raycastTarget=true;view.Face.raycastTarget=false;}
   else view.Face.raycastTarget=true;
   button.targetGraphic=view.Face;button.transition=Selectable.Transition.None;button.navigation=new Navigation{mode=Navigation.Mode.None};
   if(feedback&&!button.GetComponent<BasketballButtonFeedback>())button.gameObject.AddComponent<BasketballButtonFeedback>();
   return button;
  }
  static RectTransform Center(Transform parent,string name,Vector2 size,Vector2 at=default)=>GolfHUDStyle.Rect(name,parent,Vector2.one*.5f,at,size);
  static BasketballPlate Plate(RectTransform root,Color color,float radius,float border=2.5f){
   var p=root.gameObject.AddComponent<BasketballPlate>();p.color=color;p.radius=Vector2.one*radius;p.border=border;p.inset=3;p.shadow=5;p.raycastTarget=false;return p;
  }
  static Text Label(Transform parent,string value,Vector2 at,Vector2 size,int points,Color? color=null){
   var t=GolfHUDStyle.Text(value,parent,value,size,points,TextAnchor.MiddleCenter,color??CoveUI.Ink);t.rectTransform.anchoredPosition=at;t.font=points<=23?SportsUIStyle.TextFont:SportsUIStyle.DisplayFont;return t;
  }
  static void Stick(View view,bool isGolf){
   var root=view.Root;var face=root.GetComponent<Image>()??root.gameObject.AddComponent<Image>();
   face.sprite=circle?circle:circle=AppRoot.MakeCircle();face.type=Image.Type.Simple;face.color=isGolf?new Color(.44f,.82f,.74f,.28f):new Color(1,1,1,.25f);face.raycastTarget=false;view.Face=face;
   var thumb=Center(root,"Joystick thumb",Vector2.one*82).gameObject.AddComponent<Image>();thumb.sprite=face.sprite;thumb.color=isGolf?GolfHUDStyle.Cream:CoveUI.Cream;thumb.raycastTarget=false;view.Thumb=thumb.rectTransform;
  }
 }
 // A passive reference to the shared design; no event, match or update logic.
 public sealed class ControlDesignSource:MonoBehaviour {
  public SportId Sport;public string Id;public ControlDesigns.View View;
 }
}
