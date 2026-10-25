using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 // Translation changes the original graphic's lettering pixels. There is no
 // background label object, replacement button, or substitute icon.
 public sealed class LocalizedArtwork:MonoBehaviour {
  Text label,source;Graphic art;LocalizedArtworkSurface surface;Rect area;Rect? captionArea;string caption;bool plain;int points;
  public int FixedPoints {get;set;}
  public float LineSpacing {get;set;}=1;
  public bool FitCaptionInk {get;set;}
  public Text Caption=>label;
  public Rect Area=>area;
  public static LocalizedArtwork Add(Graphic art,Rect area,string caption,int points,Color background,Color ink,Text source=null){
   var owner=art.gameObject.AddComponent<LocalizedArtwork>();owner.art=art;owner.caption=caption;owner.source=source;owner.area=area;owner.points=points;
   owner.surface=art.GetComponent<LocalizedArtworkSurface>()??art.gameObject.AddComponent<LocalizedArtworkSurface>();owner.surface.Register(owner);
   owner.label=CoveUI.Text(art.transform,caption,0,0,1,1,points,true,ink,TextAnchor.MiddleCenter);owner.label.name="Translated lettering";
   var r=owner.label.rectTransform;r.anchorMin=r.anchorMax=r.pivot=Vector2.zero;
   owner.label.font=CoveUI.DisplayFont;owner.label.fontStyle=FontStyle.Normal;owner.label.resizeTextForBestFit=true;
   owner.label.supportRichText=true;((LocalizedText)owner.label).Artwork=true;
   owner.label.gameObject.AddComponent<ArtworkLetterGradient>();owner.SetColors(background,ink);owner.Refresh();return owner;
  }
  public void Plain(){plain=true;label.GetComponent<ArtworkLetterGradient>().enabled=false;foreach(var shadow in label.GetComponents<Shadow>())shadow.enabled=false;}
  public void SetCaptionArea(Rect value){captionArea=value;Layout();}
  public void SetCaption(string value){if(value==caption)return;caption=value;Refresh();}
  public void SetArea(Rect value){if(area==value)return;area=value;surface.Dirty();Layout();}
  public void SetColors(Color background,Color ink){
   if(!label||label.color==ink&&label.GetComponent<Outline>())return;label.color=ink;
   var gradient=label.GetComponent<ArtworkLetterGradient>();gradient.top=ink.grayscale>.5f?Color.Lerp(ink,Color.white,.65f):Color.Lerp(ink,Color.white,.12f);gradient.bottom=ink;
   if(!plain){var outline=label.GetComponent<Outline>()??label.gameObject.AddComponent<Outline>();outline.effectColor=ink.grayscale>.5f?CoveUI.Hex("103B60"):Color.white;outline.effectDistance=Vector2.one*1.8f;}
   label.SetVerticesDirty();
  }
  public Rect TextureArea(){
   Rect uv=new Rect(0,0,1,1);
   if(art is Image img&&img.sprite){var s=img.sprite;uv=new Rect(s.rect.x/s.texture.width,s.rect.y/s.texture.height,s.rect.width/s.texture.width,s.rect.height/s.texture.height);}
   else if(art is RawImage raw)uv=raw.uvRect;
   else if(art is GolfEventBadgeGraphic badge&&badge.Sheet){var crop=GolfEventBadgeGraphic.Crop(badge.Badge);uv=new Rect(crop.x/badge.Sheet.width,1-crop.yMax/badge.Sheet.height,crop.width/badge.Sheet.width,crop.height/badge.Sheet.height);}
   return new Rect(uv.min+Vector2.Scale(uv.size,area.min),Vector2.Scale(uv.size,area.size));
  }
  internal void LayoutCaption()=>Layout();
  void Layout(){
   if(!art||!label)return;var bounds=art.GetPixelAdjustedRect();
   if(source){var selected=art.mainTexture.name switch{
    "Buggy"=>new Rect(.47f,.31f,.42f,.43f),"Drive"=>new Rect(.455f,.22f,.435f,.49f),
    "Recall"=>new Rect(.465f,.27f,.43f,.42f),"Exit"=>new Rect(.50f,.25f,.36f,.45f),
    "StartGame"=>new Rect(.115f,.14f,.82f,.65f),_=>area};
    if(selected!=area){area=selected;surface.Dirty();}
   }
   if(art is Image img&&img.sprite&&img.preserveAspect){var size=img.sprite.rect.size;float scale=Mathf.Min(bounds.width/size.x,bounds.height/size.y);size*=scale;bounds=new Rect(bounds.min+Vector2.Scale(bounds.size-size,art.rectTransform.pivot),size);}
   var layout=captionArea??area;
   var rect=label.rectTransform;rect.anchoredPosition=bounds.min-art.rectTransform.rect.min+Vector2.Scale(bounds.size,layout.min);rect.sizeDelta=Vector2.Scale(bounds.size,layout.size);
   label.lineSpacing=LineSpacing;
   int sizePoints=plain?points:Mathf.Max(14,Mathf.RoundToInt(rect.rect.height*.9f));if(FitCaptionInk){label.verticalOverflow=VerticalWrapMode.Overflow;((LocalizedText)label).FitSingleLine(GameLocalization.Source(label),sizePoints);return;}if(FixedPoints>0){label.fontSize=FixedPoints;label.resizeTextForBestFit=false;((LocalizedText)label).ManualFit=true;return;}label.fontSize=sizePoints;label.resizeTextMinSize=Mathf.Max(5,Mathf.RoundToInt(sizePoints*.25f));label.resizeTextMaxSize=sizePoints;
  }
  void OnEnable(){GameLocalization.Changed+=Refresh;Refresh();}
  void OnDisable(){GameLocalization.Changed-=Refresh;if(label)label.gameObject.SetActive(false);if(surface)surface.Dirty();}
  void OnDestroy(){if(label)Destroy(label.gameObject);if(surface)surface.Dirty();}
  void LateUpdate(){if(source&&label&&GameLocalization.Source(label)!=GameLocalization.Source(source))label.text=GameLocalization.Source(source);if(label&&label.gameObject.activeSelf)Layout();}
  void Refresh(){if(!label)return;label.gameObject.SetActive(isActiveAndEnabled&&!GameLocalization.English);label.text=source?GameLocalization.Source(source):caption;surface.Dirty();Layout();}
 }
 // Apply the clean companion only inside reviewed lettering regions. English
 // retains the original material; all pixels outside those regions are original.
 public sealed class LocalizedArtworkSurface:MonoBehaviour {
  [System.Serializable]sealed class Alignment {public string name;public Vector4 uv;}
  [System.Serializable]sealed class Alignments {public Alignment[] entries;}
  static Alignments alignments;
  readonly List<LocalizedArtwork> captions=new();readonly Vector4[] regions=new Vector4[12];
  static readonly Dictionary<string,Texture2D> cleanArt=new();Graphic art;Material original,localized;Texture lastTexture;bool dirty=true,lastEnglish;
  void Awake(){art=GetComponent<Graphic>();original=art.material;localized=new Material(Resources.Load<Shader>("Localization/Shaders/LetteringBackground"));}
  public void Register(LocalizedArtwork caption){captions.Add(caption);dirty=true;}
  public void Dirty()=>dirty=true;
  void LateUpdate(){
   if(!art)return;var texture=art.mainTexture;
   if(!dirty&&lastTexture==texture&&lastEnglish==GameLocalization.English)return;dirty=false;lastTexture=texture;lastEnglish=GameLocalization.English;
   if(lastEnglish){art.material=original;return;}
   if(!cleanArt.TryGetValue(texture.name,out var clean))cleanArt[texture.name]=clean=Resources.Load<Texture2D>("Localization/Artwork/"+texture.name);
   if(!clean){art.material=original;return;}
   int count=0;foreach(var caption in captions)if(caption&&caption.isActiveAndEnabled){var r=caption.TextureArea();regions[count++]=new Vector4(r.x,r.y,r.width,r.height);}
   if(alignments==null)alignments=JsonUtility.FromJson<Alignments>(Resources.Load<TextAsset>("Localization/Artwork/Alignment").text);
   Vector4 transform=new Vector4(0,0,1,1);foreach(var entry in alignments.entries)if(entry.name==texture.name){transform=entry.uv;break;}
   localized.SetVector("_CleanUV",transform);localized.SetTexture("_CleanTex",clean);localized.SetVectorArray("_Regions",regions);localized.SetInt("_RegionCount",count);art.material=localized;art.SetMaterialDirty();
  }
  void OnDestroy(){if(art)art.material=original;if(localized)Destroy(localized);}
 }
 public sealed class ArtworkLetterGradient:BaseMeshEffect {
  public Color top=Color.white,bottom=Color.white;
  public override void ModifyMesh(VertexHelper mesh){
   if(!IsActive()||mesh.currentVertCount==0)return;var v=new UIVertex();float low=float.MaxValue,high=float.MinValue;
   for(int i=0;i<mesh.currentVertCount;i++){mesh.PopulateUIVertex(ref v,i);low=Mathf.Min(low,v.position.y);high=Mathf.Max(high,v.position.y);}
   bool rich=graphic is Text text&&text.supportRichText&&text.text.Contains("<color=");
   for(int i=0;i<mesh.currentVertCount;i++){mesh.PopulateUIVertex(ref v,i);float t=Mathf.InverseLerp(low,high,v.position.y);var color=rich?(Color)v.color*Color.Lerp(new Color(.93f,.93f,.93f,1),Color.white,t):Color.Lerp(bottom,top,t);if(!rich)color.a*=v.color.a/255f;v.color=color;mesh.SetUIVertex(v,i);}
  }
 }
}
