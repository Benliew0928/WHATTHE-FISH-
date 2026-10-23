#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using System.Linq;
using UnityEngine;

namespace WhatTheFish {
 public sealed partial class BasketballRestartReview {
  IEnumerator ClearanceChecks(){
   var limits=BasketballBall.PaintedCourtLimits;
   Check(Mathf.Abs(match.InboundClearanceRadius-28.6512f*.25f)<.001f,"inbound radius is 25 percent of full court length (7.1628 metres)");
   var exits=new[]{new Vector3(limits.x,.07f,1),new Vector3(-limits.x,.07f,-1),new Vector3(2,.07f,limits.y),new Vector3(-2,.07f,-limits.y),new Vector3(limits.x,.07f,limits.y-.7f)};
   for(int trial=0;trial<exits.Length;trial++){
    sampling=false;ball.ResetHome();DevelopmentProbe.TurnCommand=default;
    for(int i=0;i<roster.Length;i++)Place(roster[i],new Vector3(-3+i%3*3,.07f,i<3?-5:5));
    var receiving=roster.Where(a=>match.TeamOf(a)==FootballTeam.B).OrderBy(BasketballMatch.PlayerId).ToArray();
    var inbound=receiving[match.Snapshot.inboundTurnB%receiving.Length];
    Check(ball.TryInboundSpot(inbound,exits[trial],out var spot,out var inward),"clearance spot exists "+trial);
    var arena=ball.transform.parent;var inside=arena.InverseTransformPoint(spot+inward*1.3f);inside.x=Mathf.Clamp(inside.x,-limits.x+1.2f,limits.x-1.2f);inside.z=Mathf.Clamp(inside.z,-limits.y+1.2f,limits.y-1.2f);
    Place(inbound,inside);
    var defenders=roster.Where(a=>match.TeamOf(a)!=match.TeamOf(inbound)).ToArray();var mates=roster.Where(a=>a!=inbound&&match.TeamOf(a)==match.TeamOf(inbound)).ToArray();
    var localInward=arena.InverseTransformDirection(inward);var side=Vector3.Cross(Vector3.up,localInward);
    for(int i=0;i<defenders.Length;i++){var position=inside+localInward*(3+i*.6f)+side*(i-1);position.x=Mathf.Clamp(position.x,-limits.x+1,limits.x-1);position.z=Mathf.Clamp(position.z,-limits.y+1,limits.y-1);Place(defenders[i],position);}
    Place(mates[0],inside+localInward*1.5f);var secondMate=inside+localInward*3+side*1.5f;secondMate.x=Mathf.Clamp(secondMate.x,-limits.x+1,limits.x-1);secondMate.z=Mathf.Clamp(secondMate.z,-limits.y+1,limits.y-1);Place(mates[1],secondMate);
    match.Control(roster[0]);Check(match.Boundary(exits[trial]),"clearance boundary accepted "+trial);match.AfterBallStep();
    Check(match.Snapshot.inbounder==BasketballMatch.PlayerId(inbound),"protected receiver rotation is preserved "+trial);
    var mateStart=mates[0].transform.position;double frozen=match.Remaining;
    Check(!match.InboundSpaceClear&&!match.OpenInbound(inbound),"crowded inbound cannot open early "+trial);
    yield return new WaitForFixedUpdate();
    Check(defenders.All(match.AutomaticMovement)&&mates.All(a=>!match.AutomaticMovement(a)),"only opponents automatically clear the circle "+trial);
    if(app.rooms.Connected&&trial==1){match.RestoreRoom();Check(match.Snapshot.inbounder==BasketballMatch.PlayerId(inbound),"restore retains protected inbounder");}
    previous.Clear();largestStep=0;sampling=true;
    yield return Until(()=>match.RestartReady&&defenders.All(a=>!match.AutomaticMovement(a)),"opponents clear and inbound becomes ready "+trial,16);
    sampling=false;if(!match.RestartReady){Check(false,"clearance failed at "+trial+" "+string.Join(";",defenders.Select(a=>a.transform.position.ToString())));yield break;}
    Check(largestStep<2.1f,"opponents clear through continuous movement "+trial);
    Check(System.Math.Abs(match.Remaining-frozen)<.01,"clearance preserves match clock "+trial);
    Check(Vector3.Distance(mateStart,mates[0].transform.position)<.15f,"nearby teammate keeps position and control "+trial);
    Check(match.TryInboundClearance(out var center),"protection remains while inbound held "+trial);
    bool keptOut=true;
    for(int step=0;step<16;step++){
     foreach(var defender in defenders){var toward=Vector3.ProjectOnPlane(center-defender.transform.position,Vector3.up);defender.Simulate(new PlayerCommand{move=Vector2.up,heading=Quaternion.LookRotation(toward).eulerAngles.y,sprint=true,jump=true},.1f);keptOut&=Vector3.ProjectOnPlane(defender.transform.position-center,Vector3.up).magnitude>=match.InboundClearanceRadius+defender.capsule.radius-.025f;}
     yield return new WaitForFixedUpdate();
    }
    Check(keptOut,"sprinting and jumping cannot enter protected radius "+trial);
    var defender0=defenders[0];var offset=Vector3.ProjectOnPlane(defender0.transform.position-center,Vector3.up);var travel=match.ConstrainInboundMotion(defender0,-offset*3);
    Check(Vector3.ProjectOnPlane(defender0.transform.position+travel-center,Vector3.up).magnitude>=match.InboundClearanceRadius,"swept movement cannot tunnel through circle "+trial);
    var tangent=Vector3.Cross(Vector3.up,offset.normalized);Check(Vector3.Dot(match.ConstrainInboundInput(defender0,tangent),tangent)>.99f,"opponent may move along circle edge "+trial);
    var teammateDirection=Vector3.ProjectOnPlane(inbound.transform.position-mates[0].transform.position,Vector3.up).normalized*.3f;
    Check((match.ConstrainInboundMotion(mates[0],teammateDirection)-teammateDirection).sqrMagnitude<.00001f,"teammate is exempt from protected radius "+trial);
    if(trial==0)yield return Capture("inbound-opponent-clearance");
    ball.autoPickup=true;Check(ball.TryPassTo(inbound,mates[0],ball.Defense.play),"single-tap targeted inbound accepted "+trial);
    yield return Until(()=>match.Running&&!ball.InboundPending,"release removes inbound protection "+trial,4);
    Check(!match.TryInboundClearance(out _)&&(match.ConstrainInboundMotion(defender0,-offset*.2f)+offset*.2f).sqrMagnitude<.00001f,"opponent movement restriction ends at release "+trial);
    yield return Until(()=>ball.Holder==mates[0],"nearby teammate catches protected inbound "+trial,5);ball.autoPickup=false;
    Check(PlayerCollisionsRestored(),"clearance restores temporary body exceptions "+trial);
    if(failed)yield break;
   }
  }
 }
}
#endif
