using System;
using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 public enum BasketballIcon { Ball,Block,Book,Close,Court,Exit,Eye,Follow,Gear,Guard,Jump,Move,Pass,Points,Shoot,Steal }
 public static class BasketballUIStyle {
  static Sprite[] sprites;
  public static readonly Color Shoot=CoveUI.Hex("FFDA89"),Paper=CoveUI.Hex("F4ECD5"),Teal=CoveUI.Hex("40B8B8"),Green=CoveUI.Hex("43B385");
  public static Sprite Icon(BasketballIcon kind){
   if(sprites==null){var atlas=Resources.Load<Texture2D>("BasketballUI/Icons");sprites=new Sprite[16];if(atlas)for(int i=0;i<16;i++)sprites[i]=Sprite.Create(atlas,new Rect(i%4*128+4,512-(i/4+1)*128+4,120,120),Vector2.one*.5f,100,0,SpriteMeshType.FullRect);}
   return sprites[(int)kind];
  }
  public static Image Art(Transform p,BasketballIcon icon,float x,float y,float w,float h)=>CoveUI.Image(p,icon.ToString(),Icon(icon),x,y,w,h);
  // Rasterize at the parent canvas's full 1600-unit density. The basketball
  // layout uses half-size design units; scaling small font glyphs up blurs them.
  public static Text Text(Transform p,string text,float x,float y,float w,float h,int size=13,bool bold=false,TextAnchor align=TextAnchor.MiddleLeft,Color? color=null){var t=CoveUI.Text(p,text,x,y,w*2,Mathf.Max(h,size*1.5f)*2,size*2,bold,color??CoveUI.Ink,align);t.font=bold?SportsUIStyle.DisplayFont:SportsUIStyle.TextFont;t.rectTransform.localScale=Vector3.one*.5f;return t;}
  public static RectTransform Center(Transform p,string name,Vector2 size,Vector2 point,Vector2 anchor){var r=CoveUI.Rect(p,name,0,0,size.x,size.y);r.anchorMin=r.anchorMax=anchor;r.pivot=Vector2.one*.5f;r.anchoredPosition=point;return r;}
  public static BasketballPlate Plate(Transform p,string name,float x,float y,float w,float h,Color color,float radius=16,float border=2,bool shadow=false){var r=CoveUI.Rect(p,name,x,y,w,h);var g=r.gameObject.AddComponent<BasketballPlate>();g.color=color;g.radius=Vector2.one*radius;g.border=border;g.shadow=shadow?3:0;g.raycastTarget=false;return g;}
  public static Button Button(Transform p,string name,string caption,float x,float y,float w,float h,Action action,Color? fill=null,int size=14){
   var g=Plate(p,name,x,y,w,h,fill??CoveUI.Cream,13,2,true);g.raycastTarget=true;var b=g.gameObject.AddComponent<Button>();b.targetGraphic=g;b.transition=Selectable.Transition.None;b.navigation=new Navigation{mode=Navigation.Mode.None};b.onClick.AddListener(()=>{if(!BasketballButtonFeedback.UsesPressSound(g.gameObject))BasketballAudio.PlayMenu(BasketballAudio.ButtonCue(name));action?.Invoke();});
   Text(g.transform,caption,8,0,w-16,h,size,true,TextAnchor.MiddleCenter);b.gameObject.AddComponent<BasketballButtonFeedback>();MenuDesigns.Apply(b,h>=75?MenuSurface.Card:MenuSurface.Button);return b;
  }
 }
 // Geometry matches the proposal's elliptical CSS corners and inset cream rim.
 // Frames, shadows, paths and meters share the default UI material; no baked panels.
 public sealed class BasketballPlate:MaskableGraphic {
  public MenuSurface menuSurface;public bool menuSelected,menuDisabled;public float menuHover;
  public Vector2 radius=new(16,16);public float border=2,inset,shadow,pressure;public Color edge=CoveUI.Ink,insetColor=CoveUI.Cream;public bool dashed;
  public void Refresh()=>SetVerticesDirty();
  protected override void OnPopulateMesh(VertexHelper v){if(menuSurface!=MenuSurface.None){MenuDesigns.Draw(v,rectTransform.rect,color,radius.x,menuSurface,pressure,menuSelected,menuDisabled,menuHover);return;}Draw(v,rectTransform.rect,color,radius,border,inset,shadow,pressure,edge,insetColor,dashed);}
  public static void Draw(VertexHelper v,Rect r,Color color,Vector2 radius,float border=2,float inset=0,float shadow=3,float pressure=0,Color? borderColor=null,Color? rimColor=null,bool dashed=false){
   Color edge=borderColor??CoveUI.Ink,insetColor=rimColor??CoveUI.Cream;
   v.Clear();if(shadow>0){var s=r;s.y-=shadow;Round(v,s,radius,edge);}r=Inset(r,r.width*.02f*pressure);r.y-=3*pressure;
   if(border>0&&!dashed)Round(v,r,radius,edge);
   var inner=Inset(r,border);Round(v,inner,radius-Vector2.one*border,inset>0?insetColor:color);
   if(inset>0)Round(v,Inset(inner,inset),radius-Vector2.one*(border+inset),color);
   if(dashed){Vector2 previous=Corner(r,radius,0);for(int i=1;i<=96;i++){var next=Corner(r,radius,i/96f);if(i%4<2)BasketballUIMesh.Line(v,previous,next,edge,border);previous=next;}}
  }
  static Rect Inset(Rect r,float n)=>new(r.x+n,r.y+n,Mathf.Max(0,r.width-2*n),Mathf.Max(0,r.height-2*n));
  static Vector2 Corner(Rect r,Vector2 radius,float t){int corner=Mathf.Min(3,(int)(t*4));float angle=(corner*90+(t*4-corner)*90)*Mathf.Deg2Rad;radius=Vector2.Min(Vector2.Max(Vector2.zero,radius),r.size*.5f);var c=new Vector2(corner==0||corner==3?r.xMax-radius.x:r.xMin+radius.x,corner<2?r.yMax-radius.y:r.yMin+radius.y);return c+new Vector2(Mathf.Cos(angle)*radius.x,Mathf.Sin(angle)*radius.y);}
  static void Round(VertexHelper v,Rect r,Vector2 radius,Color c){if(r.width<=0||r.height<=0)return;int n=v.currentVertCount;v.AddVert(r.center,c,Vector2.zero);const int count=96;for(int i=0;i<count;i++)v.AddVert(Corner(r,radius,i/(float)count),c,Vector2.zero);for(int i=0;i<count;i++)v.AddTriangle(n,n+1+i,n+1+(i+1)%count);}
 }
 internal static class BasketballUIMesh {
  public static void Line(VertexHelper v,Vector2 a,Vector2 b,Color c,float width){var normal=new Vector2(-(b-a).y,(b-a).x).normalized*(width*.5f);int n=v.currentVertCount;v.AddVert(a+normal,c,Vector2.zero);v.AddVert(b+normal,c,Vector2.zero);v.AddVert(b-normal,c,Vector2.zero);v.AddVert(a-normal,c,Vector2.zero);v.AddTriangle(n,n+1,n+2);v.AddTriangle(n,n+2,n+3);}
  public static void Dot(VertexHelper v,Vector2 p,float r,Color c)=>CovePlate.Round(v,new Rect(p-Vector2.one*r,Vector2.one*r*2),r,c);
  public static Vector2 Polar(float r,float angle){float a=angle*Mathf.Deg2Rad;return new Vector2(Mathf.Cos(a),-Mathf.Sin(a))*r;}
  public static void Arc(VertexHelper v,float r,float from,float to,Color c,float width){int steps=Mathf.Max(1,Mathf.CeilToInt((to-from)/3));for(int i=0;i<steps;i++)Line(v,Polar(r,Mathf.Lerp(from,to,i/(float)steps)),Polar(r,Mathf.Lerp(from,to,(i+1f)/steps)),c,width);}
 }
}
