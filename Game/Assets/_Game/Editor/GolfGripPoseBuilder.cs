using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

// The supplied mitten has no weighted finger joints. Derive a small, local
// blend shape at import time; preserve the editable FBX and both original LODs.
public sealed class GolfGripPoseBuilder:AssetPostprocessor {
 const string Model="Assets/_Game/Art/RainbowSprinter.fbx";
 public override uint GetVersion()=>3;
 [MenuItem("WHATTHE FISH?/Golf/Prepare gripping hand")]
 public static void Prepare()=>AssetDatabase.ImportAsset(Model,ImportAssetOptions.ForceUpdate);
 void OnPostprocessModel(GameObject root){
  if(assetPath!=Model)return;
  var bones=root.GetComponentsInChildren<Transform>(true);
  var hand=bones.First(t=>t.name=="mixamorig:RightHand");
  var tip=bones.First(t=>t.name=="mixamorig:RightHandMiddle4");
  float length=Vector3.Distance(hand.position,tip.position);
  var along=(tip.position-hand.position).normalized;
  var across=Vector3.ProjectOnPlane(root.transform.forward,along).normalized;
  var normal=Vector3.Cross(along,across).normalized;
  float start=length*.418f,radius=length*.265f,arc=length*.213f;
  var socket=new GameObject("Golf grip socket").transform;socket.SetParent(hand,false);
  socket.position=hand.position+along*start+normal*radius;
  socket.rotation=Quaternion.LookRotation(across,normal);
  foreach(var skin in root.GetComponentsInChildren<SkinnedMeshRenderer>(true)){
   var mesh=skin.sharedMesh;int bone=Array.IndexOf(skin.bones,hand);if(bone<0)continue;
   var points=mesh.vertices;var normals=mesh.normals;var tangents=mesh.tangents;var weights=mesh.boneWeights;
   var delta=new Vector3[points.Length];var dn=new Vector3[points.Length];var dt=new Vector3[points.Length];int changed=0;
   for(int i=0;i<points.Length;i++){
    var bw=weights[i];float weight=(bw.boneIndex0==bone?bw.weight0:0)+(bw.boneIndex1==bone?bw.weight1:0)+(bw.boneIndex2==bone?bw.weight2:0)+(bw.boneIndex3==bone?bw.weight3:0);
    if(weight<.5f)continue;
    var p=skin.transform.TransformPoint(points[i])-hand.position;
    float x=Vector3.Dot(p,across),y=Vector3.Dot(p,along),z=Vector3.Dot(p,normal);
    // The mitten's palm was partly weighted to the forearm. Fade to a rigid
    // hand across the palm so the grip opening cannot collapse as the wrist turns.
    float rigid=Mathf.SmoothStep(0,1,Mathf.InverseLerp(length*.15f,length*.4f,y));
    bw.weight0=bw.weight0*(1-rigid)+(bw.boneIndex0==bone?rigid:0);bw.weight1=bw.weight1*(1-rigid)+(bw.boneIndex1==bone?rigid:0);
    bw.weight2=bw.weight2*(1-rigid)+(bw.boneIndex2==bone?rigid:0);bw.weight3=bw.weight3*(1-rigid)+(bw.boneIndex3==bone?rigid:0);weights[i]=bw;
    if(y<=start||z>=radius)continue;
    float angle=(y-start)/arc;float r=radius-z;
    var curled=hand.position+across*x+along*(start+r*Mathf.Sin(angle))+normal*(radius-r*Mathf.Cos(angle));
    float blend=Mathf.SmoothStep(0,1,(y-start)/(length*.05f));
    delta[i]=(skin.transform.InverseTransformPoint(curled)-points[i])*blend;
    // Rotate the original custom normals/tangents locally instead of changing
    // smoothing on the rest of the character.
    Vector3 BendVector(Vector3 source,bool surfaceNormal){
     var v=skin.transform.TransformDirection(source);float vx=Vector3.Dot(v,across),vy=Vector3.Dot(v,along),vz=Vector3.Dot(v,normal);
     float scale=surfaceNormal?arc/r:r/arc;
     return skin.transform.InverseTransformDirection((across*vx+along*(scale*Mathf.Cos(angle)*vy-Mathf.Sin(angle)*vz)+normal*(scale*Mathf.Sin(angle)*vy+Mathf.Cos(angle)*vz)).normalized);
    }
    if(normals.Length==points.Length)dn[i]=(BendVector(normals[i],true)-normals[i])*blend;
    if(tangents.Length==points.Length){Vector3 t=tangents[i];dt[i]=(BendVector(t,false)-t)*blend;}
    changed++;
   }
   mesh.boneWeights=weights;mesh.AddBlendShapeFrame("GolfRightGrip",100,delta,dn,dt);
   Debug.Log($"GOLF_GRIP_IMPORT {mesh.name} changed={changed}/{points.Length} socket={socket.localPosition}");
  }
 }
}
