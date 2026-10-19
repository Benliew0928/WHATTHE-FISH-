using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace WhatTheFish {
 // Pitch-local, cosmetic end markings. Team A defends negative Z in match rules.
 // The coloured apron is outside the painted goal line; collision and scoring
 // still use the original authored goal and white pitch markings.
 public sealed class FootballGoalIdentity:MonoBehaviour {
  readonly List<Material> materials=new();
  public FootballTeam Team {get;private set;}
  public int Sign {get;private set;}
  public static FootballTeam DefendingTeam(int sign)=>sign<0?FootballTeam.A:FootballTeam.B;
  public static Color EndColor(int sign)=>sign<0?CharacterIdentity.Blue:CharacterIdentity.Red;
  public static void Attach(Transform goal,Transform pitch,Bounds opening,Bounds field,float front,int sign){
   if(goal.GetComponent<FootballGoalIdentity>())return;
   var identity=goal.gameObject.AddComponent<FootballGoalIdentity>();identity.Build(pitch,opening,field,front,sign);
  }
  void Build(Transform pitch,Bounds opening,Bounds field,float front,int sign){
   Sign=sign;Team=DefendingTeam(sign);var color=EndColor(sign);
   var block=new MaterialPropertyBlock();block.SetColor("_BaseColor",color);block.SetColor("_Color",color);
   foreach(var renderer in GetComponentsInChildren<MeshRenderer>(true))if(renderer.name=="Goal__Frame")renderer.SetPropertyBlock(block);
   var source=Resources.Load<Material>("FootballStadiumScreenFrame");if(!source)return;
   var paint=new Material(source){name=sign<0?"Blue defending end":"Red defending end"};paint.SetColor("_BaseColor",color);paint.SetColor("_Color",color);materials.Add(paint);
   var root=new GameObject("Goal side markers").transform;root.SetParent(transform,false);
   // Reuse the primitive mesh and one material per end. No new texture set.
   void Strip(string name,Vector3 position,Vector3 size){
    var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.layer=2;
    var collision=go.GetComponent<Collider>();collision.enabled=false;Destroy(collision);
    go.transform.SetParent(root,false);go.transform.SetPositionAndRotation(pitch.TransformPoint(position),pitch.rotation);
    var scale=Vector3.Scale(size,pitch.lossyScale);var parentScale=root.lossyScale;
    go.transform.localScale=new Vector3(Mathf.Abs(scale.x/parentScale.x),Mathf.Abs(scale.y/parentScale.y),Mathf.Abs(scale.z/parentScale.z));
    var renderer=go.GetComponent<MeshRenderer>();renderer.sharedMaterial=paint;renderer.shadowCastingMode=ShadowCastingMode.Off;
   }
   float z=sign<0?field.min.z:field.max.z,y=field.max.y;
   Strip("Coloured goal apron",new Vector3(field.center.x,y+.015f,z+sign*.65f),new Vector3(field.size.x,.025f,1.1f));
   foreach(int side in new[]{-1,1}){
    float x=side<0?field.min.x:field.max.x;
    Strip("End touchline ribbon",new Vector3(x+side*.35f,y+.018f,z-sign*4.5f),new Vector3(.42f,.025f,9));
    // Low boards beside the opening remain below the spectator sightline.
    float boardX=opening.center.x+side*(opening.extents.x+3.2f);
    Strip("Goal identity board",new Vector3(boardX,y+.65f,front+sign*.45f),new Vector3(4.5f,1.15f,.12f));
    var label=new GameObject("Defending team label").AddComponent<TextMesh>();label.transform.SetParent(root,false);
    label.text=sign<0?"BLUE GOAL":"RED GOAL";label.fontSize=64;label.characterSize=.105f;label.anchor=TextAnchor.MiddleCenter;label.alignment=TextAlignment.Center;label.color=Color.white;
    label.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");label.GetComponent<MeshRenderer>().sharedMaterial=label.font.material;
    label.transform.SetPositionAndRotation(pitch.TransformPoint(new Vector3(boardX,y+.7f,front+sign*.37f)),pitch.rotation*Quaternion.Euler(0,sign<0?180:0,0));
    label.GetComponent<MeshRenderer>().shadowCastingMode=ShadowCastingMode.Off;
   }
  }
  void OnDestroy(){foreach(var material in materials)if(material)Destroy(material);}
 }
}
