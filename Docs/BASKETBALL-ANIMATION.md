# Basketball movement and ball handling

Basketball adds a procedural pose layer to the existing Rainbow Sprinter rig. It reuses the authored running and jumping clips, both character LODs, the basketball mesh and all existing materials. No animation package, texture or duplicate character mesh is required. The current 30 cm ball has updated dribble positions, palm contacts and two-handed grips; [shot physics and net response](BASKETBALL-GAMEPLAY.md) describe the hold/release controls and scoring.

## Close finishes

[Layups and dunks](BASKETBALL-FINISHES.md) add a committed gather and plant,
ballistic character jump, mirrored shooting hand, physical ball release and
landing absorption. The dunk lifts beside the head, rolls the palm around the
ball and pushes downward; the layup uses an upward-facing palm and a soft arc.
The whole movement path is swept before commitment and uses the real character
capsule during execution. A new obstruction stops guided travel and lets gravity
complete the landing. The animation adds no imported clip, mesh or texture.
The 4 m approach range uses a slightly longer extension into release: 0.60 s
for layup and 0.76 s for dunk. Eligible finishes ignore charge timing. A queued
request brakes into the returning dribble before the plant, and a 0.20 s
allowance preserves a real approach when the player releases movement just
before the shoot gesture. The layup's 15% miss branch releases wide into a
physical rebound; the same gather and landing remain continuous.

## Steal and stripped reaction

The **Steal / F** control repeats a committed swipe with recovery. The animation in [`BasketballChallengeMotion.cs`](../Game/Assets/_Game/Sports/Basketball/BasketballChallengeMotion.cs) uses a short load, a planted lead-foot step, hip transfer and shoulder rotation. The striking hand rakes across and down through the ball, then folds back toward the hip. The opposite elbow balances behind the torso. The palm faces across the sweep instead of presenting an upright waving hand. Both hands mirror with the chosen side, and low contacts deepen the knee bend and forward lean within the short-limbed rig's reach.

On a successful strip, the former carrier's dribbling hand drops, reaches late toward the lost ball, and withdraws toward the ribs. The shoulders first flinch back, then lean after the ball as the other arm opens low for balance. A small diagonal recovery step catches that weight shift. The head counterbalances the torso while keeping attention near the ball. The reaction avoids raising both hands together and blends from the last visible dribble hand pose.

The swipe still lasts **0.68 seconds**, its authoritative contact window is **0.14–0.26 seconds**, held input repeats every **0.74 seconds**, and the victim recovers in **0.60 seconds**. Defensive poses have their own phase curves instead of passing through the shooting gather's extra smoothing. The support foot anchors a stationary action, the leading foot plants and returns, and moving players retain the authored gait. IK keeps the original bone lengths. Visual weight transfer does not teleport the player capsule. Movement remains available at reduced speed; jumping waits until recovery completes. The poses reuse the existing skeleton, both character LODs and materials, without importing clips or textures.

The host sends the action, timestamp, sequence, contact point and hand choice. Clients present both timelines; only the host decides whether the ball becomes loose. The ball's deflection and automatic pickup rules are described in [stealing and loose-ball contests](BASKETBALL-GAMEPLAY.md#stealing-and-loose-ball-contests). The release does not attach the ball to the defender.

The 5 October refinement passed **366 checks**, including pose reviews at fixed
20/30/60/120 FPS, real hold/release input, multiplayer reactions and offensive
motion regressions. Maximum sampled stationary support-foot drift was **1.6 mm**.
The complete Windows delivery and normal/half-speed gameplay preview are
recorded in [verification](VERIFICATION.md#refined-basketball-steal-animation--5-october-2026).
The fresh Android APK grows by **4,208 bytes** to **89,762,748 bytes**; it remains
above the 75 MB development target. Windows captures do not establish phone
appearance or performance.

## Movement

- **Ready and stopping:** a slightly lowered centre of gravity, bent knees, balanced feet, restrained torso lean and a short braking response blend over the existing gait. Foot targets preserve the running animation's contacts before settling into the ready stance.
- **Running:** the existing speed-driven running legs continue under the basketball torso and arms. Cadence increases from 1.65 to 2.15 bounces per second with speed, without restarting the dribble phase.
- **Dribbling:** the right palm presses the ball, separates during free flight, then meets the returning ball. The left hand balances and protects. Elbow poles and a two-bone solver prevent joints from bending backwards or stretching. The held ball is kinematic; its floor bounce follows a shared timeline, while released balls use the existing rigid-body physics.
- **Shooting:** the current bounce finishes at the hand before the gather. The character brings the ball into the body, bends its legs, raises and extends the shooting arm, releases, flexes the wrist and eases out of follow-through. The guide hand supports the lower side of the ball, then separates. A grounded set shot briefly brakes movement during the gather; recovery restores movement. Turning toward a hoop includes staggered foot pivots.
- **Passing:** both hands gather at chest height, push forward, release and recover. **Q** or the **Pass** touch button requests a chest pass. The host selects a reachable player within a forward cone, or throws ahead when none is available. Hold **E** or **Shoot**, then release in the green band to request a shot.

The character has deliberately exaggerated proportions: large head, short arms and a single mitten endpoint per hand. Targets are fitted to that skeleton. The pose layer moves the wrists and hands as units; it does not invent individual finger joints or lengthen the arms. Authoring meshes, skeleton and saved Blender actions remain unchanged.

## Timing and integration

Shot preparation retains the 30% shorter timeline. The meter now repeats in both directions, taking **0.84–0.42 seconds per sweep** according to distance. On charge, the character completes the current bounce, gathers with both hands, bends into a ready stance and keeps the ball raised while aiming at the locked hoop. This pose is replicated to opponents and remains active through repeated meter cycles. Walking and sprinting during the hold run at half speed. The existing rig supplies all poses; no animation clip was imported.

Releasing a prepared aim lifts directly from the held position. Cancelling lowers the ball into the dribble over **0.22 seconds**, preserving possession and normal movement. Shooting releases **0.308 seconds** into its **0.686-second** action; passing still releases at 0.30 seconds in a 0.70-second action. Ball spin remains steady in the grip. The ball, palm contacts, guide hand, body dip, foot pivots, turn and follow-through share the pose clock. Release checks use the hand endpoint and solve the trajectory at release, including movement during preparation. Obstructed releases retain possession. Repeated input cannot restart a queued or active action.

[`BasketballMotion.cs`](../Game/Assets/_Game/Sports/Basketball/BasketballMotion.cs) restores its previous overlay before the Animator evaluates, then applies basketball poses in LateUpdate before the ball follows the hands. This avoids accumulating rotation on unanimated bones. Foot and arm IK preserve bone lengths. Exponential blending uses elapsed time, rather than a fixed per-frame interpolation amount.

[`BasketballInteraction.cs`](../Game/Assets/_Game/Sports/Basketball/BasketballInteraction.cs) owns the dribble phase, queued input, shot/pass release, assisted receiver selection and physics. [`NetworkAthlete.cs`](../Game/Assets/_Game/Shared/Networking/NetworkAthlete.cs) replicates each player's action timeline; ball snapshots carry possession, dribble phase/cadence, release counts, scores and net events. Host and guest must use matching builds. Reliable owner-only action RPCs convey charge edges and heading; the host measures the hold and chooses release positions, recipients and velocities. Travel, leaving the session and resetting the player clear active poses and pending releases.

## Reproduction

The development-only player review records joint movement, hand reach, dribble contact, action timing and actual rendered frames, including the lower-detail character. Its implementation is compiled out of the Android release (Unity retains a small MonoScript metadata stub).

```powershell
Tools/Build/Build.ps1 -Target Windows
Tools/Build/Test-BasketballMotion.ps1
Tools/Build/Test-BasketballMotion.ps1 -Fps 20 -NoCapture
Tools/Build/Test-BasketballMotion.ps1 -Fps 60 -NoCapture
Tools/Build/Test-BasketballMotion.ps1 -Fps 120 -NoCapture
Tools/Build/Test-BasketballChallenge.ps1
Tools/Build/Test-BasketballChallenge.ps1 -Fps 20 -NoCapture
Tools/Build/Test-BasketballChallenge.ps1 -Fps 60 -NoCapture
Tools/Build/Test-BasketballChallenge.ps1 -Fps 120 -NoCapture
Tools/Build/Test-BasketballSteal.ps1 -Mode Local
Tools/Build/Test-BasketballGameplay.ps1 -Mode Offline
Tools/Build/Test-BasketballGameplay.ps1 -Mode Local
Tools/Build/Test-BasketballBall.ps1 -Mode Offline
Tools/Build/Test-BasketballBall.ps1 -Mode Local
Tools/Build/Test-Jump.ps1
Tools/Build/Build.ps1 -Target AndroidSubmission
Tools/Build/Check-TaskReady.ps1 -RequireApk
```

Current evidence belongs under ignored `Builds/BasketballMotionQA/`; related gameplay and ball-contact evidence remains in their existing QA folders. Fixed-rate Windows capture checks presentation and integration, not physical-phone rendering, frame rate, thermals, touch ergonomics or WAN latency. See [APK measurements](BUILD-SIZE.md) and [basketball gameplay](BASKETBALL-GAMEPLAY.md).

## Verified results, 1–2 October 2026

- **220 motion checks:** 55 each at fixed 20, 30, 60 and 120 FPS. Readiness, running, stopping, stationary/moving/turning dribbles, a reversing gather, shot and pass follow-through, stable ball grip, jumping with possession, both character LODs, blocked release, reset cancellation and session return passed. Maximum sampled dribble palm contact error was 0.3 mm; the maximum arm target correction was 32.8 mm during an action.
- **95 gameplay checks:** 67 offline, 18 host and 10 guest. Both hoops, 2–24 m shot trajectories, rebound pickup, owner-only actions, duplicate rejection, host-to-guest and guest-to-host passes, an 11 m catch, holder disconnect and room restart passed. Current runs: `Builds/BasketballGameplayQA/Offline-20261002-152529/` and `Local-20261002-153824/`.
- **53 foundation checks:** floor/rim/backboard collisions, spin, scene lifecycle and host/guest replication; `Builds/BasketballModel/20261001/Offline-152719/` and `Local-152719/`.
- **140 jump/momentum checks:** offline and two-player regression passed in `Builds/JumpQA/Run-20261002-152719/`. The editor build also passed 65 motor checks.
- **55 portability checks:** the new review script ran from a copied player in a path with spaces, launched from an unrelated temporary working directory. Current evidence is in `Builds/BasketballMotionQA/PortableFinal/`.
- **Final network regression:** the default idle pose now uses component quaternion equality, so it compares equal to itself and does not trigger redundant pose updates. The 30 FPS review passed 56 checks, including this regression, in `Builds/BasketballMotionQA/NetworkVerified-30/`; the two-player run above was repeated after this correction. This equality change leaves the rendered poses unchanged.
- **Visual review:** 419 rendered Windows frames form `Builds/BasketballMotionQA/basketball-motion-review.mp4`. Close/front and side views cover the five requested actions, jumping with possession and the lower-detail model. The frames and telemetry remain in `Release-30/`; other rates use `Release-20/`, `Release-60/` and `Release-120/`.
- **Android:** signed submission build passed, **85,970,874 → 85,993,642 bytes (+22,768)**, with **14,006,358 bytes** to the hard limit. The 75 MB working target remains unmet by **10,993,642 bytes**. No phone was connected.

The first pose targets exceeded the short-armed rig's reach. They were refitted to the actual palm landmarks and reviewed again. A low-rate reversal exposed a gather that crossed the torso; the corrected ball/hand frame now turns with the body before aligning to the hoop. Long passes use a bounded 14 m/s catch allowance until their first collision.

Superseded captures, logs, intermediate APKs and portable fixtures were archived with sizes, hashes and recovery paths under these local-only batches:

- `Legacy/20261002-151603-774-basketball-motion/`
- `Legacy/20261002-152830-451-basketball-motion-final/`
- `Legacy/20261002-153048-938-basketball-motion-portability/`
- `Legacy/20261002-153921-524-basketball-network-final/`
- `Legacy/20261002-154704-645-basketball-network-evidence/`

No binary art or active dependencies were edited, so the existing documented nonfunctional source-metadata exceptions are unchanged. The four-island/shared-room model and existing jump work are preserved. Nothing was staged, committed or pushed.
