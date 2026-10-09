using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 // Small shared-material UI meshes: no full-screen image, imported atlas or effects package.
 public sealed class FishingStickerGraphic:MaskableGraphic {
  public enum Kind {Panel,Gauge,Fish,Avatar,Flame,Crown,Reel,Warning,Shine,Aim,Camera,Jump,Close}
  public Kind kind;public Color tint=LocalProfile.Hex("FFF3D8"),accent=LocalProfile.Hex("38DBD7");
  public float value=1;public bool hot;
  [Min(0)]public float cornerRadius=24;
  Image artwork;Sprite resizedFrame;bool fullArtwork,solidPanel;public bool portraitVisible;
  public bool SolidPanel=>solidPanel;
  static readonly Color Ink=LocalProfile.Hex("10394D");
  public void Decorate(){
   string resource=kind==Kind.Panel||kind==Kind.Gauge?"LagoonFrame":kind==Kind.Fish?"FishBadge":kind==Kind.Flame?"Flame":kind==Kind.Avatar?"FisherBadge":null;if(resource!=null)UseIllustration(resource,kind==Kind.Panel||kind==Kind.Gauge);
  }
  public void UseIllustration(string resource,bool border=false){
   var sprite=Resources.Load<Sprite>("FishingUI/"+resource);if(!sprite)return;
   if(!artwork){var node=new GameObject("Illustrated sticker",typeof(RectTransform),typeof(Image));node.transform.SetParent(transform,false);node.transform.SetAsFirstSibling();var rect=(RectTransform)node.transform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;artwork=node.GetComponent<Image>();}
   if(resizedFrame){Destroy(resizedFrame);resizedFrame=null;}
   if(border){float corner=sprite.rect.width*.18f;resizedFrame=Sprite.Create(sprite.texture,sprite.rect,new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,Vector4.one*corner);artwork.sprite=resizedFrame;artwork.pixelsPerUnitMultiplier=corner/20f;}
   else artwork.sprite=sprite;
   artwork.type=border?Image.Type.Sliced:Image.Type.Simple;artwork.fillCenter=!border;artwork.preserveAspect=!border&&kind!=Kind.Flame;artwork.raycastTarget=false;fullArtwork=!border;if(fullArtwork)solidPanel=false;artwork.enabled=!solidPanel||!border;if(kind==Kind.Avatar){fullArtwork=false;portraitVisible=true;artwork.rectTransform.offsetMin=Vector2.one*3;artwork.rectTransform.offsetMax=-Vector2.one*3;}SetVerticesDirty();
  }
  protected override void OnDestroy(){if(resizedFrame)Destroy(resizedFrame);base.OnDestroy();}
  public void Refresh(float fill,Color main,bool danger=false){if(Mathf.Abs(value-fill)<.001f&&tint==main&&hot==danger)return;value=fill;tint=main;hot=danger;if(artwork&&fullArtwork&&kind==Kind.Panel)artwork.color=Color.Lerp(Color.white,main,danger?.25f:.08f);SetVerticesDirty();}
  // Pulse only the illustration; labels and the button's hit area stay fixed.
  public void PulseIllustration(float scale){if(artwork)artwork.rectTransform.localScale=Vector3.one*Mathf.Clamp(scale,.85f,1.2f);}
  public void UseSolidPanel(bool solid=true){
   solid=solid&&kind==Kind.Panel&&!fullArtwork;if(solidPanel==solid)return;solidPanel=solid;
   if(artwork&&artwork.type==Image.Type.Sliced)artwork.enabled=!solid;SetVerticesDirty();
  }
  protected override void OnPopulateMesh(VertexHelper vh){
   vh.Clear();var r=rectTransform.rect;float w=r.width,h=r.height;if(fullArtwork&&artwork){Rounded(vh,Inset(r,12),Mathf.Min(w,h)*.45f,new Color(0,0,0,.001f));return;}
   switch(kind){
    case Kind.Aim:
     for(int i=0;i<4;i++){
      float x=(i<2?-1:1)*w*.34f,y=(i%2==0?-1:1)*h*.34f;
      var vertical=new Rect(r.center.x+x-1.5f,r.center.y+y-(y>0?h*.15f:0),3,h*.15f);
      var horizontal=new Rect(r.center.x+x-(x>0?w*.15f:0),r.center.y+y-1.5f,w*.15f,3);
      Rounded(vh,new Rect(vertical.x-1,vertical.y-1,vertical.width+2,vertical.height+2),2,Ink);Rounded(vh,new Rect(horizontal.x-1,horizontal.y-1,horizontal.width+2,horizontal.height+2),2,Ink);
      Rounded(vh,vertical,1.5f,tint);Rounded(vh,horizontal,1.5f,tint);
     }
     if(value>0){Ellipse(vh,r.center,new Vector2(3,3),Ink);Ellipse(vh,r.center,new Vector2(1.6f,1.6f),tint);}break;
    case Kind.Panel:
    case Kind.Gauge:
     if(kind==Kind.Panel&&solidPanel){
      float radius=Mathf.Clamp(cornerRadius,0,Mathf.Min(w,h)*.5f),outline=Mathf.Min(2,Mathf.Min(w,h)*.08f);
      Rounded(vh,r,radius,Ink);Rounded(vh,Inset(r,outline),Mathf.Max(0,radius-outline),tint);
      Rounded(vh,new Rect(r.x+13,r.y+h-10,w-26,2),1,new Color(1,1,1,.23f));break;
     }
     Rounded(vh,new Rect(r.x+2,r.y-4,w,h),Mathf.Min(24,h*.5f),new Color(.01f,.13f,.19f,.35f));
     Rounded(vh,r,Mathf.Min(24,h*.5f),Ink);Rounded(vh,Inset(r,3),Mathf.Min(21,(h-6)*.5f),new Color(.93f,1,1));
     Rounded(vh,Inset(r,6),Mathf.Min(18,(h-12)*.5f),kind==Kind.Gauge?Ink:tint,true);
     if(kind==Kind.Gauge&&value>.005f){var fill=Inset(r,9);fill.width*=Mathf.Clamp01(value);Rounded(vh,fill,Mathf.Min(fill.height*.5f,fill.width*.5f),tint,true);var shine=new Rect(fill.x+6,fill.y+fill.height*.66f,Mathf.Max(0,fill.width-15),4);Rounded(vh,shine,2,new Color(1,1,1,.42f));}
     if(kind==Kind.Panel)Rounded(vh,new Rect(r.x+13,r.y+h-13,w-26,3),1.5f,new Color(1,1,1,.34f));
     break;
    case Kind.Fish:
     Poly(vh,new[]{P(-.48f,-.27f,r),P(-.14f,0,r),P(-.48f,.27f,r)},Ink);Poly(vh,new[]{P(-.42f,-.18f,r),P(-.13f,0,r),P(-.42f,.18f,r)},accent);
     Ellipse(vh,P(.09f,0,r),new Vector2(w*.38f,h*.32f),Ink);Ellipse(vh,P(.09f,.01f,r),new Vector2(w*.33f,h*.26f),tint,true);
     Poly(vh,new[]{P(-.05f,.22f,r),P(.07f,.39f,r),P(.19f,.23f,r)},accent);
     Ellipse(vh,P(.28f,.10f,r),new Vector2(w*.09f,h*.11f),Color.white);Ellipse(vh,P(.31f,.09f,r),new Vector2(w*.04f,h*.06f),Ink);
     Ellipse(vh,P(.35f,-.09f,r),new Vector2(w*.045f,h*.027f),Ink);break;
    case Kind.Avatar:
     Ellipse(vh,r.center,new Vector2(w*.5f,h*.5f),Ink);Ellipse(vh,r.center,new Vector2(w*.43f,h*.43f),accent);
     if(portraitVisible)break;
     Ellipse(vh,P(0,-.02f,r),new Vector2(w*.31f,h*.36f),LocalProfile.Hex("FFD5B1"));
     Poly(vh,new[]{P(-.34f,.12f,r),P(-.26f,.39f,r),P(-.08f,.27f,r),P(.06f,.44f,r),P(.20f,.30f,r),P(.34f,.34f,r),P(.27f,.08f,r),P(.09f,.23f,r),P(-.12f,.14f,r)},tint);
     Ellipse(vh,P(-.12f,-.02f,r),new Vector2(w*.036f,h*.06f),Ink);Ellipse(vh,P(.12f,-.02f,r),new Vector2(w*.036f,h*.06f),Ink);
     Ellipse(vh,P(0,-.20f,r),new Vector2(w*.10f,h*.037f),Ink);break;
    case Kind.Flame:
     Poly(vh,new[]{P(-.38f,-.45f,r),P(-.48f,-.04f,r),P(-.20f,-.18f,r),P(-.05f,.45f,r),P(.12f,.18f,r),P(.20f,.36f,r),P(.42f,-.10f,r),P(.36f,-.42f,r)},LocalProfile.Hex("FF5A39"));
     Poly(vh,new[]{P(-.25f,-.43f,r),P(-.20f,-.05f,r),P(-.05f,-.13f,r),P(.05f,.17f,r),P(.28f,-.22f,r),P(.19f,-.43f,r)},LocalProfile.Hex("FFD857"));break;
    case Kind.Crown:
     Poly(vh,new[]{P(-.46f,.30f,r),P(-.28f,-.31f,r),P(.29f,-.31f,r),P(.46f,.30f,r),P(.17f,.08f,r),P(0,.44f,r),P(-.17f,.08f,r)},Ink);
     Poly(vh,new[]{P(-.36f,.18f,r),P(-.21f,-.24f,r),P(.22f,-.24f,r),P(.36f,.18f,r),P(.14f,-.02f,r),P(0,.29f,r),P(-.14f,-.02f,r)},LocalProfile.Hex("FFD857"));break;
    case Kind.Reel:
     Ellipse(vh,r.center,new Vector2(w*.45f,h*.34f),Ink);Ellipse(vh,r.center,new Vector2(w*.39f,h*.27f),accent,true);
     Rounded(vh,new Rect(r.center.x-w*.23f,r.center.y-h*.25f,w*.4f,h*.5f),5,LocalProfile.Hex("FFB653"));
     for(int i=0;i<4;i++)Rounded(vh,new Rect(r.center.x-w*.20f+i*w*.08f,r.center.y-h*.22f,w*.035f,h*.44f),2,LocalProfile.Hex("E8733A"));
     Ellipse(vh,P(-.24f,0,r),new Vector2(w*.15f,h*.32f),Ink);Ellipse(vh,P(-.24f,0,r),new Vector2(w*.11f,h*.25f),LocalProfile.Hex("5ADDD5"));
     Rounded(vh,new Rect(r.center.x+w*.13f,r.center.y,w*.075f,h*.35f),3,Ink);Ellipse(vh,P(.23f,.34f,r),new Vector2(w*.14f,h*.075f),LocalProfile.Hex("FFE4A0"));break;
    case Kind.Warning:
     Poly(vh,new[]{P(-.46f,-.39f,r),P(0,.44f,r),P(.46f,-.39f,r)},Ink);Poly(vh,new[]{P(-.34f,-.31f,r),P(0,.28f,r),P(.34f,-.31f,r)},LocalProfile.Hex("FFD857"));
     Rounded(vh,new Rect(r.center.x-2,r.center.y-3,4,h*.20f),2,Ink);Ellipse(vh,P(0,-.21f,r),new Vector2(2.5f,2.5f),Ink);break;
    case Kind.Shine:
     Poly(vh,new[]{P(-.08f,-.08f,r),P(-.45f,0,r),P(-.08f,.08f,r),P(0,.48f,r),P(.08f,.08f,r),P(.45f,0,r),P(.08f,-.08f,r),P(0,-.48f,r)},tint);break;
    case Kind.Camera:
     Rounded(vh,new Rect(r.center.x-w*.29f,r.center.y+h*.22f,w*.29f,h*.15f),Mathf.Min(w,h)*.04f,Ink);
     Rounded(vh,new Rect(r.center.x-w*.42f,r.center.y-h*.29f,w*.84f,h*.57f),Mathf.Min(w,h)*.10f,Ink);
     Rounded(vh,new Rect(r.center.x-w*.36f,r.center.y-h*.23f,w*.72f,h*.45f),Mathf.Min(w,h)*.07f,tint);
     Ellipse(vh,P(.06f,-.005f,r),new Vector2(w*.23f,h*.25f),Ink);Ellipse(vh,P(.06f,-.005f,r),new Vector2(w*.18f,h*.19f),accent);
     Ellipse(vh,P(.06f,-.005f,r),new Vector2(w*.105f,h*.115f),Ink);Ellipse(vh,P(.01f,.06f,r),new Vector2(w*.045f,h*.045f),Color.white);
     Rounded(vh,new Rect(r.center.x-w*.29f,r.center.y+h*.10f,w*.12f,h*.065f),h*.03f,Ink);break;
    case Kind.Jump:
     var limbs=new[]{P(-.13f,.06f,r),P(-.10f,-.15f,r),P(-.13f,.06f,r),P(-.31f,.13f,r),P(-.31f,.13f,r),P(-.40f,.31f,r),P(-.13f,.06f,r),P(.05f,.13f,r),P(.05f,.13f,r),P(.15f,.31f,r),P(-.10f,-.15f,r),P(-.30f,-.31f,r),P(-.30f,-.31f,r),P(-.41f,-.22f,r),P(-.10f,-.15f,r),P(.09f,-.28f,r),P(.09f,-.28f,r),P(.20f,-.13f,r)};
     for(int pass=0;pass<2;pass++)for(int i=0;i<limbs.Length;i+=2)Stroke(vh,limbs[i],limbs[i+1],Mathf.Min(w,h)*(pass==0?.115f:.065f),pass==0?Ink:tint);
     Ellipse(vh,P(-.13f,.29f,r),Vector2.one*Mathf.Min(w,h)*.115f,Ink);Ellipse(vh,P(-.13f,.29f,r),Vector2.one*Mathf.Min(w,h)*.078f,tint);
     Stroke(vh,P(.37f,-.22f,r),P(.37f,.20f,r),Mathf.Min(w,h)*.12f,Ink);Stroke(vh,P(.37f,-.22f,r),P(.37f,.20f,r),Mathf.Min(w,h)*.065f,tint);
     Poly(vh,new[]{P(.20f,.15f,r),P(.37f,.39f,r),P(.49f,.15f,r)},Ink);Poly(vh,new[]{P(.27f,.19f,r),P(.37f,.32f,r),P(.43f,.19f,r)},tint);break;
    case Kind.Close:
     for(int pass=0;pass<2;pass++){float width=Mathf.Min(w,h)*(pass==0?.23f:.13f);var color=pass==0?Ink:tint;
      Stroke(vh,P(-.28f,-.28f,r),P(.28f,.28f,r),width,color);Stroke(vh,P(-.28f,.28f,r),P(.28f,-.28f,r),width,color);
     }break;
   }
  }
  static Vector2 P(float x,float y,Rect r)=>r.center+new Vector2(x*r.width,y*r.height);
  static Rect Inset(Rect r,float n)=>new Rect(r.x+n,r.y+n,Mathf.Max(0,r.width-n*2),Mathf.Max(0,r.height-n*2));
  static void Poly(VertexHelper vh,Vector2[] points,Color color){if(points.Length<3)return;int first=vh.currentVertCount;foreach(var p in points)vh.AddVert(p,color,Vector2.zero);for(int i=1;i<points.Length-1;i++)vh.AddTriangle(first,first+i,first+i+1);}
  static void Stroke(VertexHelper vh,Vector2 from,Vector2 to,float width,Color color){
   var direction=to-from;if(direction.sqrMagnitude<.0001f)return;var side=new Vector2(-direction.y,direction.x).normalized*width*.5f;
   Poly(vh,new[]{from-side,from+side,to+side,to-side},color);Ellipse(vh,from,Vector2.one*width*.5f,color);Ellipse(vh,to,Vector2.one*width*.5f,color);
  }
  static void Ellipse(VertexHelper vh,Vector2 centre,Vector2 radius,Color color,bool gradient=false){
   int first=vh.currentVertCount;vh.AddVert(centre,color,Vector2.zero);const int count=24;
   for(int i=0;i<count;i++){float a=i*Mathf.PI*2/count;var p=centre+new Vector2(Mathf.Cos(a)*radius.x,Mathf.Sin(a)*radius.y);vh.AddVert(p,gradient?Color.Lerp(color,Color.white,Mathf.Max(0,Mathf.Sin(a))*.28f):color,Vector2.zero);}
   for(int i=0;i<count;i++)vh.AddTriangle(first,first+1+i,first+1+(i+1)%count);
  }
  static void Rounded(VertexHelper vh,Rect r,float radius,Color color,bool gradient=false){
   if(r.width<=0||r.height<=0)return;radius=Mathf.Clamp(radius,0,Mathf.Min(r.width,r.height)*.5f);int first=vh.currentVertCount;vh.AddVert(r.center,color,Vector2.zero);const int segments=6;
   for(int corner=0;corner<4;corner++)for(int i=0;i<=segments;i++){
    float angle=(corner*90+i*90f/segments)*Mathf.Deg2Rad;var centre=new Vector2(corner==0||corner==3?r.xMax-radius:r.xMin+radius,corner<2?r.yMax-radius:r.yMin+radius);
    var p=centre+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius;vh.AddVert(p,gradient?Color.Lerp(color,Color.white,Mathf.InverseLerp(r.yMin,r.yMax,p.y)*.24f):color,Vector2.zero);
   }
   int count=4*(segments+1);for(int i=0;i<count;i++)vh.AddTriangle(first,first+1+i,first+1+(i+1)%count);
  }
 }
}
