# Basketball movement and ball handling

Basketball adds a procedural pose layer to the existing Rainbow Sprinter rig. It reuses the authored running and jumping clips, both character LODs, the basketball mesh and all existing materials. No animation package, texture or duplicate character mesh is required.

## Movement

- **Ready and stopping:** a slightly lowered centre of gravity, bent knees, balanced feet, restrained torso lean and a short braking response blend over the existing gait. Foot targets preserve the running animation's contacts before settling into the ready stance.
- **Running:** the existing speed-driven running legs continue under the basketball torso and arms. Cadence increases from 1.65 to 2.15 bounces per second with speed, without restarting the dribble phase.
- **Dribbling:** the right palm presses the ball, separates during free flight, then meets the returning ball. The left hand balances and protects. Elbow poles and a two-bone solver prevent joints from bending backwards or stretching. The held ball is kinematic; its floor bounce follows a shared timeline, while released balls use the existing rigid-body physics.
- **Shooting:** the current bounce finishes at the hand before the gather. The character brings the ball into the body, bends its legs, raises and extends the shooting arm, releases, flexes the wrist and eases out of follow-through. The guide hand supports the lower side of the ball, then separates. A grounded set shot briefly brakes movement during the gather; recovery restores movement. Turning toward a hoop includes staggered foot pivots.
- **Passing:** both hands gather at chest height, push forward, release and recover. **Q** or the **Pass** touch button requests a chest pass. The host selects a reachable player within a forward cone, or throws ahead when none is available. **E** and **Shoot** still request a shot.

The character has deliberately exaggerated proportions: large head, short arms and a single mitten endpoint per hand. Targets are fitted to that skeleton. The pose layer moves the wrists and hands as units; it does not invent individual finger joints or lengthen the arms. Authoring meshes, skeleton and saved Blender actions remain unchanged.

## Timing and integration

The gather begins when the ball returns to the hand, so an input near the bottom of a bounce waits for that bounce to finish. Ball spin is held steady during the gather. Shooting releases 0.44 seconds into its 0.98-second action; passing releases at 0.30 seconds in a 0.70-second action. Release checks use the same endpoint as the hand pose and solve the trajectory at release, including movement during preparation. Obstructed releases retain possession. Repeated input cannot restart a queued or active action.

[`BasketballMotion.cs`](../Game/Assets/_Game/Sports/Basketball/BasketballMotion.cs) restores its previous overlay before the Animator evaluates, then applies basketball poses in LateUpdate before the ball follows the hands. This avoids accumulating rotation on unanimated bones. Foot and arm IK preserve bone lengths. Exponential blending uses elapsed time, rather than a fixed per-frame interpolation amount.

[`BasketballInteraction.cs`](../Game/Assets/_Game/Sports/Basketball/BasketballInteraction.cs) owns the dribble phase, queued input, shot/pass release, assisted receiver selection and physics. [`NetworkAthlete.cs`](../Game/Assets/_Game/Shared/Networking/NetworkAthlete.cs) replicates each player's action timeline; ball snapshots carry possession, dribble phase/cadence and release counts. **The merged protocol 14 requires matching players.** Reliable owner-only action RPCs request a heading; the host chooses release positions, recipients and velocities. Travel, leaving the session and resetting the player clear active poses and pending releases.

## Reproduction

The development-only player review records joint movement, hand reach, dribble contact, action timing and actual rendered frames, including the lower-detail character. Its implementation is compiled out of the Android release (Unity retains a small MonoScript metadata stub).

```powershell
Tools/Build/Build.ps1 -Target Windows
Tools/Build/Test-BasketballMotion.ps1
Tools/Build/Test-BasketballMotion.ps1 -Fps 20 -NoCapture
Tools/Build/Test-BasketballMotion.ps1 -Fps 60 -NoCapture
Tools/Build/Test-BasketballMotion.ps1 -Fps 120 -NoCapture
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
