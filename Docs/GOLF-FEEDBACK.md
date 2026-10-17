# User-recorded GO and walkway steps — 17 October 2026

GO now plays the supplied Screen Recording 2026-10-17 191059.mp4 audio,
cropped from .53 to 1.46 seconds, once at the existing GO transition. The
synthetic countdown-number beep remains. The generated GO tone is only a
missing-resource fallback; the delivered player is checked against the loaded
recorded GO asset.

The supplied Screen Recording 2026-10-17 192235 - Trim.mp4 provides three
individual walkway steps (.79-1.14, 1.48-1.80 and 2.20-2.57 seconds). Existing
ankle planting and actual ground support select one take per step on Path,
Approach or Paving colliders, cycling three takes at original pitch. The full
multi-step recording is never played as one footstep. Other stone/turf/sand/
water/wood surfaces retain their existing clips. Idle/air/aim/cart/travel gates,
minimum interval, master/SFX gains and spatial voice pool remain unchanged.

Full decoded 48 kHz stereo masters and provenance are retained outside Unity
in ArtSource/Audio/Golf/Recordings/. Tools/Audio/prepare_golf_recordings.py
verifies hashes and creates four mono 32 kHz PCM delivery clips under
Game/Assets/_Game/Resources/GolfAudio/. DC removal, step-only 70 Hz high-pass,
peak normalization to .65 and 5 ms edge ramps preserve each source's tone and
remove cut clicks. No pitch/time change or synthesized replacement is used.
Original videos are represented by filename/hash provenance; the retained audio
masters are sufficient for offline regeneration. WAVs follow existing LFS rules.
Recorded shared clips are not destroyed on course teardown; only generated
fallbacks are owned by the components.

Final verification: 216 rendered HUD/feedback checks with 34 captures plus 17 targeted shot checks pass. Actual planted walkway contacts select recorded clips, one animation cycle gives two steps, and GO uses the loaded recording once. WAV regeneration from an unrelated working directory is byte-identical. Complete WindowsFinal updated; AndroidSubmission 95,067,465 bytes. Evidence: Builds/GolfRecordedAudioQA/. Physical speaker/phone quality and live WAN remain unverified.

Historical release notes follow below.
# Golf preview and presentation refinements — 17 October 2026

The near-full Swing screen edge now reaches 30% pale-red opacity. The automatic
full-charge ready cue is removed; ordinary charging/contact audio remains.
The character-side meter uses rounded teal edging, a dark inset and a slim
highlight while retaining yellow through 75% and red at full power. The roaming
Walk near your ball to aim plate is hidden entirely; charging and driving hints
remain. GO uses one continuous 2400 Hz 620 ms tone with gentle end ramps, no
noise, frequency modulation, tremolo or repeated bursts. Countdown beeps remain.

Actual Golf CoastalPath, WelcomeApproach and PracticeApproach support colliders
select stone footsteps before the wood/deck classification. Turf, sand, water,
wood and other surfaces keep their prior sounds and planted-foot scheduling.
Wheel track opacity increases .12 to .48, grass factor .8; each contact emits
8 flecks instead of 2, sized .045-.085 m with broader position/velocity spread.
The particle pool is capped at 256 per used cart; the existing 512 mark quads
still fade fully after 3 s. No new textures or meshes are imported.

Golf ponds already used the Fishing LagoonWater shader. Their material now
uses the same .28 base opacity and ripple speed as Fishing, with cooler blue tints to offset the green bed;
previously opacity was 1. The pond generator defaults match, preserving future
regeneration. Fishing's own material and all pond geometry remain unchanged.

Putt's display preview sweeps its existing sampled path for the first solid
object on default/scene/player/cart layers, ignoring supporting terrain and
the golfer's collider. The displayed line and target end at that first contact
and do not show rebound or the subsequent resting point. Dynamic cart/obstacle
changes are checked each frame even with unchanged aim power. Swing uses its
original plan endpoint. This is display-only: authoritative shot plans, actual
movement/collision, scoring and course geometry are unchanged.

Inspector: Game/Assets/_Game/Resources/GolfFeedback.asset for edgeOpacity=.3,
wheelTrackOpacity=.48, wheelParticles=8 and track lifetime=3. Pond appearance:
Game/Assets/_Game/Art/Golf/FiveHoleCourse/Materials/Pond water.mat.

Final verification: 211 rendered HUD/feedback checks with 34 captures and 17 targeted shot checks pass. Complete WindowsFinal updated; final AndroidSubmission 94,935,333 bytes. Evidence: Builds/GolfRefineQA/, Builds/GolfHUDQA/Run-20261017-191146/ and Builds/GolfShotQA/Run-20261017-191146/. Physical speaker quality, phone appearance/FPS and live WAN remain unverified.

Historical release notes follow below.
# Golf charge meter, foot plants, start audio and wheel contacts — 17 October 2026

Swing near-full screen edges are pale red. The action button has no charging
ring. A 22x116 UI meter tracks the local golfer's upper body on the screen,
on the side away from the ball, clamped to the safe HUD frame. Its fill is
yellow through 75%, blends yellow to red over the final quarter, and is pure
red at full charge. It hides on release, cancellation, focus loss or leaving
Golf. Putt also uses the character meter and retains no edge vignette.

Footsteps now read the existing TurnFootPlacement ankle bones after displayed
animation. A foot must lift and then lower into its planted height before one
sound can fire. Grounded actual motion, minimum interval, aim/cart/jump/travel
and idle gates prevent repeated stationary or blocked sounds. Tiny no-motion
render frames between physics ticks retain the lifted-foot state. Rigs lacking
ankles use animation phase, then a slower distance fallback when no animator
exists. This reuses the existing rig without editing locomotion or animation.

The five opening countdown numbers share one 160 ms racing-style beep clip;
the transition to GO plays a separate 620 ms breathy, modulated whistle.
The existing host-clock countdown drives local audio; late frames do not replay
missed numbers. Focus, pause, course exit and disable stop it. All sounds use
existing master/SFX settings. Clips are generated once, not imported assets.

Moving cart wheels emit a few small soil/sand flecks at actual supported wheel
contacts and leave shallow world-space marks. Four wheel contact histories use
observed displacement, including reverse/turning; parked carts, summons and
teleports do not generate trails. Marks fade continuously and are fully gone
after 3 s. One bounded 512-quad mesh and one 96-particle pool per used cart
reuse the Golf feedback material. No terrain, collision or driving changes.

Inspector: Game/Assets/_Game/Resources/GolfFeedback.asset. Foot lift/plant
heights and minimumFootstepInterval tune sound contacts; footContactPhase and
walkingStepDistance/runningStepDistance are fallback-only. countdownVolume,
whistleVolume, wheelParticles, wheelTrackOpacity and wheelTrackLifetime tune
these additions. Existing edgeOpacity controls pale-red edge strength.

Final validation: 203 rendered HUD/feedback checks (33 captures) and 17 targeted shot checks pass. One real animation cycle produces two planted-foot sounds; the countdown produces five identical beeps and one whistle; parked marks disappear by 3.2 s. Full WindowsFinal promoted; AndroidSubmission 94,934,277 bytes. Evidence: Builds/GolfPolishQA/. Physical speakers, mobile FPS and live WAN are not verified.

Historical release notes follow below.
# Golf follow inset, footsteps and aim clearance — 17 October 2026

The Swing edge glow is now wider and reaches 14% opacity at full charge,
starting above 80%. Previously its 6.5% maximum was hard to distinguish in the
bright outdoor scene. Ready remains one audible cue per peak, and Putt retains
its deliberate no-vignette presentation. The central scene stays clear.

Golf now has distance-driven grass/sand/shallow-water/wood/stone footsteps.
Cadence uses actual grounded movement, with a longer stride when running.
Blocked input, idle, jumps, aiming, carts and travel produce no steps. Surface
selection uses existing water footprints/sea height, ground-support labels and
course resistance data. This does not add water traversal or modify terrain.
Five synthesized mono footsteps use shared master/SFX gains and eight pooled
spatial sources; remote athletes use their observed movement on each device.
There are no footstep network messages. Ground-level water is required for a
splash; walking on a bridge above water does not count as wading.

A separate silent 512x288 render camera opens a rectangular inset below the
Island Greens badge on an actual local shot contact, including Putt. It follows
the displayed ball throughout flight and rolling while the main view remains
under player control. It does not follow another player's strike. The camera
uses obstacle checks and no post-processing/shadows/audio listener. Once the
ball has been stable on ground for 0.25 s, it holds for 3 s and fades over
0.65 s. Motion resuming restarts that timer. Cup/reset holds the last location
rather than jumping to a new tee. A new local shot replaces the target; leaving
Golf, round change and disable hide it. Hidden cameras stop rendering, and
render textures/cameras/UI are released when the course is destroyed.

Inspector: `Game/Assets/_Game/Resources/GolfFeedback.asset`: `footstepVolume`,
`walkingStepDistance`, `runningStepDistance`, `followWindowSize`,
`followRestHold`, `followFadeSeconds`, and the existing `edgeOpacity`.

Aim still validates line of sight, walkable ground, reach, player separation,
body clearance and a clear swept path. If the preferred side and approach side
are blocked, it now searches 16 alternating angles at two reach distances.
It accepts walkable normals compatible with the existing controller slope
limit and checks sight to the top of the ball from the chosen stance. Solid
obstacles are never ignored, and the ball/map is not moved. The capture from
the prior running player shows the ball at a tree base beside sand; the previous
two-direction search could reject nearby legal stances. Synthetic trunk/bunker
fixtures validate the fallback and fully enclosed rejection.

Final verification: 196 rendered HUD/feedback checks with 32 captures plus 17 targeted ball checks pass. The camera render texture contains world pixels; exact-frame edge on/off captures confirm visible rendering. Complete WindowsFinal was updated and the fresh Android APK is 94,926,465 bytes. Evidence: Builds/GolfFollowQA/; detailed release record: BUILD-SIZE.md. Physical speaker quality, mobile FPS and live WAN remain unverified.

Historical feedback behavior and earlier validation notes follow below.

# Golf charge, contact, landing and cup feedback

This pass adds presentation to the existing authoritative Golf flow. It does
not change launch speed, shot planning, rolling/resting, map/cup geometry,
progress, scoring or other sports. Ball-follow camera remains deferred.

## Player experience

- Holding Swing/Putt shows a gold progress ring and live percentage. A soft
  rising loop follows charge; Putt uses half the charging gain. Cancelling
  fades the loop out in roughly 50 ms and clears charge graphics.
- Above 80% Swing charge, the screen edge gradually warms to a very faint
  gold (maximum opacity 0.065). Putt has no edge overlay. At 98% a short ready
  cue fires once per peak; falling charge also reduces the graphics/audio.
- A tiny 0.06 m charge camera push and 0.1 s high-power Swing contact kick
  affect only the local camera. Putt does not kick. Reduced Motion disables
  both camera effects; the central settings can also disable camera shake.
- Swing has an accepted-swing whoosh and a separate crisp contact sound.
  Putt has a softer tap. Impact sound/small gold-white particles happen at
  the existing actual club contact (0.10 s after the accepted swing starts),
  not on pointer release. Rejected/cancelled actions do not emit impact.
- Actual turf/sand landings have different sounds and small particles.
  Impact strength controls gain and particle count. Tiny contacts are quiet;
  cooldown suppresses the same contact firing through both guide and PhysX.
  Reset/spawn contacts and wall normals do not make landing feedback.
- Only a successful current cup emits its drop/chime, finite gold particles
  and a short local success label. The fifth valid hole has a separate finish
  cue/label. Wrong cups, water reset and teleport do not celebrate.

## Settings and sound sources

Edit `Game/Assets/_Game/Resources/GolfFeedback.asset` (Golf Feedback settings):
charge/putt/ready gains, ready threshold, edge opacity, charge camera push,
hit/landing/cup gains, minimum landing speed/cooldown, camera shake enable,
duration/distance, Swing/Putt/landing/cup particle counts. Default shake is
0.012 m for 0.1 s, only for local Swing power >=65%. Settings are separate
from `GolfBallPhysics.asset`; they cannot change a ball's trajectory.

All voices obey shared master/SFX volume. Master is the existing AudioListener
volume; SFX mute updates active voices too. Charge/ready/local cup cues are
non-spatial; world whoosh/contact/landing/other-player cup sounds use distance
attenuation. There are eight deterministic short mono cues plus a charge loop,
synthesized once at 24 kHz by `GolfFeedbackSounds.cs`, following the project's
Fishing feedback architecture. These are synthesized effects, not recordings.
The pool is capped at 12 one-shot voices and one charging source. Eight pooled
particle systems share one unlit material, with at most 64 particles each;
there are no added lights, rigidbodies, colliders or imported bitmap textures.

Focus loss, pause, round reset, leaving Golf and disable clear transient sounds,
particles/charge graphics and camera kick. Success graphics expire after 1.1 s.

## Multiplayer

The Host emits confirmed contact, landing and successful-cup events on the
cosmetic `Golf.Feedback.v1` reliable named channel. Clients accept only the
server, matching round, finite/bounded data and a recent timestamp; duplicate
and late events are discarded. Reconnecting resets the receive cursor and
leaving unregisters the handler. There is no snapshot schema or protocol
change and no late-join replay. All players should use the updated player to
see/hear the feature. Only the hitter's device kicks its camera; the ball
owner gets the local cup announcement even when somebody else hits their ball.

## Verification and known baseline

Rendered HUD checks cover clean cues, charging/cancellation/full-power feedback,
actual pointer release/contact separation, local/Putt/remote/reduced-motion/
Inspector shake behavior, focus cleanup and existing UI regression. Production
wire encode/decode and stale/wrong-round/nonfinite/duplicate rejection are
exercised with an explicit cosmetic fixture, not a live WAN room. The targeted
shot review covers actual PhysX handoff/impulses, a real .008-charge putt from
0.65 m scoring exactly once, wrong-cup rejection, actual sand impact and finite
cup particles. The final-hole cue is a valid progress-transition fixture.
The old .35-charge near-cup test overshoots with the current launch settings;
only the fixture charge is recalibrated, not gameplay power.

The broad shot regression stops at
`HOLE_4_Putt_0.80_PUTT_RESTS_AT_GLOW` in BOTH the previously delivered player
and the feedback build, after correct path/contact/stroke checks. It is an
existing stop-position issue, not repaired or hidden in this feedback pass.
See the latest measured release record for run paths, counts and APK bytes.
Desktop screenshots/source-signal checks do not establish phone FPS, physical
speaker quality/volume or live WAN synchronization. Those remain manual tests.
