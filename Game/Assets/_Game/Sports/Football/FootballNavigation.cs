using UnityEngine;

namespace WhatTheFish {
 public static class FootballNavigation {
  public static bool ClearLane(Vector3 from,Vector3 to,Athlete[] blockers,Athlete receiver=null){
   var line=Vector3.ProjectOnPlane(to-from,Vector3.up);float squared=line.sqrMagnitude;
   if(squared<.01f)return false;
   if(Physics.SphereCast(from+Vector3.up*.45f,.22f,line.normalized,out _,line.magnitude,1<<8,QueryTriggerInteraction.Ignore))return false;
   foreach(var other in blockers){
    if(!other||other==receiver||other.inTransit)continue;
    var offset=Vector3.ProjectOnPlane(other.transform.position-from,Vector3.up);float t=Vector3.Dot(offset,line)/squared;
    float reach=1.05f+Mathf.Min(.45f,t*line.magnitude*.035f);
    if(t>.025f&&t<.98f&&(offset-line*t).sqrMagnitude<reach*reach)return false;
   }
   return true;
  }
  public static Vector3 Clamp(FootballBall ball,Vector3 point,float margin=1.1f){
   var p=ball.Pitch.InverseTransformPoint(point);var b=ball.PitchBounds;
   p.x=Mathf.Clamp(p.x,b.min.x+margin,b.max.x-margin);p.z=Mathf.Clamp(p.z,b.min.z+margin,b.max.z-margin);
   var result=ball.Pitch.TransformPoint(p);result.y=point.y;return result;
  }
  // A tiny visibility graph around the two legal walls. Edges use the same
  // expanded-box sweep as the ball rules; the character controller owns travel.
  public static Vector3 Waypoint(FootballBall ball,Vector3 from,Vector3 destination){
   var wallA=ball.Props.wallA;var wallB=ball.Props.wallB;
   // Match state and its fixed clock cannot advance within this synchronous
   // query. Resolve liveness once instead of rebuilding snapshots per edge.
   bool activeA=ball.WallActive(wallA),activeB=ball.WallActive(wallB);
   bool Clear(Vector3 a,Vector3 b){
    for(int i=0;i<2;i++){var wall=i==0?wallA:wallB;if((i==0?activeA:activeB)&&FootballPropRules.Sweep(wall,a+Vector3.up*.6f,b+Vector3.up*.6f,.5f,out float t,out var normal)){
     if(t>0||Vector3.Dot(b-a,normal)<=0)return false;
    }}
    return true;
   }
   if(Clear(from,destination))return destination;
   System.Span<Vector3> nodes=stackalloc Vector3[10];nodes[0]=from;nodes[1]=destination;int count=2;
   for(int wallIndex=0;wallIndex<2;wallIndex++){var wall=wallIndex==0?wallA:wallB;if(wallIndex==0?activeA:activeB)for(int i=0;i<4;i++){
    var p=wall.position+wall.Rotation*new Vector3(((i&1)==0?-1:1)*(FootballPropRules.WallWidth*.5f+.85f),0,((i&2)==0?-1:1)*(FootballPropRules.WallThickness*.5f+.85f));p.y=from.y;
    if((Clamp(ball,p,.5f)-p).sqrMagnitude<.001f)nodes[count++]=p;
   }}
   System.Span<float> cost=stackalloc float[10];System.Span<int> previous=stackalloc int[10];System.Span<bool> visited=stackalloc bool[10];
   for(int i=0;i<count;i++){cost[i]=float.PositiveInfinity;previous[i]=-1;visited[i]=false;}cost[0]=0;
   for(int step=0;step<count;step++){
    int current=-1;for(int i=0;i<count;i++)if(!visited[i]&&(current<0||cost[i]<cost[current]))current=i;
    if(current<0||float.IsPositiveInfinity(cost[current]))break;
    if(current==1){int next=1;while(previous[next]>0)next=previous[next];return nodes[next];}
    visited[current]=true;
    for(int i=0;i<count;i++)if(!visited[i]&&Clear(nodes[current],nodes[i])){float candidate=cost[current]+Vector3.Distance(nodes[current],nodes[i]);if(candidate<cost[i]){cost[i]=candidate;previous[i]=current;}}
   }
   return from;
  }
 }
}
