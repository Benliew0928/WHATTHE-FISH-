using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WhatTheFish {
 public sealed class TutorialStep {
  public readonly string id,title,copy,focus,reveal;
  public TutorialStep(string id,string title,string copy,string focus,string reveal=""){this.id=id;this.title=title;this.copy=copy;this.focus=focus;this.reveal=reveal;}
 }
 // Stable lesson IDs make progress independent of course order. Only verified
 // actions earn completion; skipping remains a separate state.
 public static class TutorialCourse {
  public static TutorialStep[] For(SportId sport){
   var steps=new List<TutorialStep>{
    new("forward","Move forward","Push the stick up and walk a few steps.","move","move"),
    new("backward","Move backward","Pull the stick down and walk back.","move"),
    new("sideways","Move sideways","Move left, then right with the stick.","move"),
    new("look","Look around","Swipe on the open right side of the screen to look around.","look","look")
   };
   if(sport!=SportId.Football)steps.Add(new("jump","Jump","Tap Jump once. You can still move and look.","jump","jump"));
   if(sport==SportId.Basketball){steps.AddRange(new[]{
    new TutorialStep("collect","Collect the ball","Walk into the ball. It is picked up automatically.","move"),
    new TutorialStep("shoot","Aim and shoot","Hold Shoot. Aim toward the hoop, then release in the marked green zone.","shoot","shoot"),
    new TutorialStep("cancelShot","Cancel a shot","Hold Shoot, slide onto Cancel, then lift your finger.","shoot"),
    new TutorialStep("pass","Pass to your teammate","Face your teammate and tap Pass.","pass","pass"),
    new TutorialStep("directPass","Choose your receiver","Double-tap your teammate's character to pass directly.","world"),
    new TutorialStep("prop","Collect a prop","Walk into the marked floating prop. It gives you one use.","move"),
    new TutorialStep("ability","Use your prop","Tap the illustrated ability button to activate your speed boost.","ability","ability"),
    new TutorialStep("guard","Guard the opponent","Without the ball, Shoot becomes Guard. Hold it near the opponent.","shoot"),
    new TutorialStep("steal","Steal the ball","Pass becomes Steal on defense. Hold Steal beside the opponent, then collect the loose ball.","pass"),
    new TutorialStep("block","Block a shot","Hold Guard, swipe up and keep holding. Stay near the shooter to contest the shot.","shoot")
   });}
   if(sport==SportId.Football){steps.AddRange(new[]{
    new TutorialStep("collect","Collect and dribble","Walk into the ball, then move with it at your feet.","move"),
    new TutorialStep("shoot","Aim and shoot","Face the goal. Hold Shoot to build power, then release.","shoot","shoot"),
    new TutorialStep("cancelShot","Cancel a shot","Hold Shoot, slide to Cancel, then lift your finger.","shoot"),
    new TutorialStep("pass","Pass to your teammate","Face your teammate and tap Pass.","pass","pass"),
    new TutorialStep("directPass","Choose your receiver","Double-tap your teammate's character to pass directly.","world"),
    new TutorialStep("dash","Dash and recover","Hold Dash while moving. Release it to let your stamina recover.","dash","dash"),
    new TutorialStep("pressure","Pressure the opponent","Shoot becomes Pressure on defense. Hold it to follow the ball carrier.","shoot"),
    new TutorialStep("tackle","Win the ball","Move close to the opponent, then tap Tackle.","tackle","tackle"),
    new TutorialStep("prop","Collect an ability","Walk into the marked touchline prop to collect it.","move"),
    new TutorialStep("ability","Use your ability","Tap the illustrated ability button to activate your speed boost.","ability","ability")
   });}
   if(sport==SportId.Golf){steps.AddRange(new[]{
    new TutorialStep("start","Start golfing","Tap Start. This practice uses the real course without events.","start","start"),
    new TutorialStep("aim","Enter Aim","Walk near your ball and tap Aim. Drag on the right to change direction.","aim","aim"),
    new TutorialStep("power","Set power and swing","Pull the ball handle down for power. Drag up to reduce it. Release to strike.","power","power"),
    new TutorialStep("cancelAim","Leave Aim","Tap Aim, then Cancel to return to walking without taking a shot.","aim"),
    new TutorialStep("putt","Try a short putt","Aim at the cup. Pull down just a little and release for a gentle putt.","power"),
    new TutorialStep("buggy","Call your buggy","Tap Buggy to summon it nearby.","buggy","buggy"),
    new TutorialStep("drive","Get in","Walk beside the buggy and tap Drive.","drive","drive"),
    new TutorialStep("steer","Drive and steer","Use the stick to drive forward and turn.","move"),
    new TutorialStep("exitCart","Get out","Tap Exit to leave the buggy.","drive"),
    new TutorialStep("recall","Recall your buggy","Tap Recall to put the buggy away.","buggy")
   });}
   if(sport==SportId.Fishing){steps.AddRange(new[]{
    new TutorialStep("pier","Find your fishing spot","Walk to the marked pier and face the water.","move"),
    new TutorialStep("skillChoice","Choose one skill","Choose one of the three cards. You keep this skill and reuse it after its cooldown.","skills","skills"),
    new TutorialStep("cast","Aim and cast","Aim at a fish until the target turns teal, then tap Cast.","cast","cast"),
    new TutorialStep("hook","Hook the fish","Wait for the ! cue, then tap Hook.","cast"),
    new TutorialStep("reel","Reel and countersteer","Hold the glowing Left or Right button. Switch sides when the glow changes.","left","left"),
    new TutorialStep("cool","Ease the tension","Release both reel buttons to cool the rope. Hold again when it is safe.","cast"),
    new TutorialStep("fishSkill","Use your skill","Tap your skill when it is bright. The clock border shows its cooldown.","ability","ability"),
    new TutorialStep("catch","Land a fish","Follow the glowing side and release when tension is high. Fill the catch meter.","cast"),
    new TutorialStep("cancelCast","Cancel fishing","Cast again, then tap Cancel to put the rod away.","cancel","cancel")
   });}
   return steps.ToArray();
  }
 }
 public static class TutorialProgress {
  [Serializable,UnityEngine.Scripting.Preserve]public sealed class Record {public int version=1;public string current="";public List<string> complete=new(),skipped=new();}
  public static string Key(string profile,SportId sport)=>ControlLayouts.Key(profile,sport)+".tutorial.v1";
  public static Record Load(string profile,SportId sport){try{var r=JsonUtility.FromJson<Record>(PlayerPrefs.GetString(Key(profile,sport),""));if(r!=null&&r.version==1&&r.complete!=null&&r.skipped!=null)return r;}catch(ArgumentException){}return new Record();}
  public static void Save(string profile,SportId sport,Record value){PlayerPrefs.SetString(Key(profile,sport),JsonUtility.ToJson(value));PlayerPrefs.Save();}
  public static string Caption(SportId sport){var r=Load(PlayerIdentity.ProfileStorageKey,sport);int n=TutorialCourse.For(sport).Count(s=>r.complete.Contains(s.id));return n==0&&r.current==""?"Start learning":n==TutorialCourse.For(sport).Length?"Completed · Replay":"Continue · "+n+" / "+TutorialCourse.For(sport).Length+" learned";}
 }
}
