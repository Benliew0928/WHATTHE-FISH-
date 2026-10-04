# Football animation

Football presentation has been rebuilt on the unchanged Rainbow Sprinter character, 4 October 2026. The original diagnosis below is retained as a record of the defects in the 3 October implementation. The schematic is not runtime evidence; reviews use the actual Unity player.

## Coordinated movement rebuild — 4 October 2026

[FootballGait](../Game/Assets/_Game/Sports/Football/FootballGait.cs) and [FootballMotion](../Game/Assets/_Game/Sports/Football/FootballMotion.cs) replace the old mixture of independently timed running legs and procedural arms. One continuous phase drives the feet, opposite arm swing, pelvis, chest and dribble timing. Ground travel determines stride; the short-legged rig uses a shorter support phase at higher speed. Supporting feet retain world-space positions. A foot can acquire a plant at touchdown, never halfway through a support phase when an activation threshold changes.

The football pose removes the controller's ground clearance from the rendered body and feet. That correction is retained through flight, so the jump has a consistent origin. It does not change the controller, movement speeds, jump trajectory, tackle sweep, ball authority, room/world model or network protocol.

| Movement | Current implementation |
| --- | --- |
| Ready, walk, run, sprint | Coordinated shoulder/elbow arcs, restrained wrist follow-through, weight shift, torso counter-rotation and speed-dependent posture; no 1.25 m/s pose switch |
| Starts, stops, changes of direction | Acceleration lean, braking compression, continuous arm relaxation, alternating settling steps, visual facing lag, gaze lead and balance-arm widening |
| Receiving, dribbling, ball stops | Either-foot receiving/settling; touches during the correct foot's forward swing; lower inside-surface contact adapted to the actual ball/shoe size; smoothly weighted contact approach with swing filtering |
| Charge, fake, kicks and bumps | Existing authoritative events select the gesture and foot; complete-skeleton entry/exit blends retain the last visible pose; restrained wrists and body counterbalance |
| Jump and landing | Football-specific leg/body posture and coordinated forward-kinematic arm swing follow the shared jump phase, including landing compression |
| Slide, miss and four falls | Directional recoil, continuous hand/foot arcs, bracing and recovery; added slide recovery passing pose; final floor checks also cover blended entry poses |
| Results and lifecycle | Win/disappointment poses, reset/travel cleanup and existing sequenced network gesture playback |

Gameplay locks remain **0.425 s for a slide** and **0.8 s for a hit**. Cosmetic recovery lasts up to **0.68 s / 1.10 s**, respectively. Control returns at the original authoritative deadline while the visible recovery finishes. No animation introduces an extra action lock. The old shared imported clips remain available to the other sports.

The IK solver constructs a stable hinge frame, avoids the fully straight singularity, uses continuous bend guides and limits action wrist deviation to 50 degrees before final contact corrections. The saved posture/action library uses shape-preserving cubic tangents: passing poses have continuous velocity, while actual holds and extrema settle. Its **28 records are editable control-space timelines**, combined with the continuous gait and FK/IK code; they are not 28 imported motion-capture clips or independently animated fingers. No skeleton, mesh, texture or package dependency was added.

The review records actual joint quaternions and positions, support samples, contact error and skinned-mesh floor penetration. It checks the final displayed skeleton, after transition blending. Its angular-speed ceiling is a regression detector for large snaps, not a certificate of human biomechanics. Likewise, zero reported support slip is meaningful only when the accompanying support-sample count is nonzero. A 20 FPS sample can miss a complete short sprint contact.

The final 20/30/60/120-rate reviews pass **1,172 assertions**. Maximum measured shoe-to-ball error during high-weight dribble touches is **2.13 cm** across these runs; maximum sampled skinned floor penetration is **1.35 cm**. Walking has **17 / 28 / 59 / 123** consecutive support samples, respectively, with zero horizontal displacement at the trace's recorded precision. Large-snap checks retain their 2,700 degrees/second ceiling; the highest measured live local joint speed is **2,637.72 degrees/second**. These are bounded continuity/contact measurements, not evidence that every pose is anatomically realistic.

The reviewed actual-player video is **60 FPS**, 86.4 seconds including normal and half-speed playback, under `Builds/FootballMotionQA/Run-20261004-224406-424/`. It includes original shared clips for comparison, live motion, interrupted recoveries and the resulting 28-record pose sheets on both LODs. Final rate traces and gameplay evidence are indexed in `Builds/FootballMotionPolishQA/final-runs.json`. The Android release adds **9,856 actual APK bytes**; see the linked release records below.

The unchanged **7 m/s** sprint and approximately **0.31 m** leg chains still imply fast, stylized footwork. Physical-phone rendering/performance and a human assessment of the final motion remain separate from automated checks. See [verification](VERIFICATION.md) for measured results and [build size](BUILD-SIZE.md) for the actual release artifact; earlier dated sections describe older snapshots.

### Reproduce the current review

```powershell
Tools/Build/Build.ps1 -Target Windows
Tools/Build/Test-FootballMotion.ps1 -Fps 60 -CompareSourceClips
Tools/Build/Test-FootballMotion.ps1 -Fps 20 -NoCapture
Tools/Build/Test-FootballMotion.ps1 -Fps 30 -NoCapture
Tools/Build/Test-FootballMotion.ps1 -Fps 120 -NoCapture
python Tools/Blender/audit_football_motion.py --run Builds/FootballMotionQA/Run-<timestamp>
python Tools/Blender/package_football_motion.py --run Builds/FootballMotionQA/Run-<timestamp>
```

Every live review frame is captured. The encoder reads `capture-rate.txt`, so a 60 FPS timeline produces a 60 FPS video. Half-speed playback repeats those full-rate samples. `-CompareSourceClips` includes the original shared run/stop clips as a comparison on the same model; their lack of support samples must not be reported as evidence of zero sliding. `joints.csv`, `motion.csv`, `continuity.json`, frames and videos remain ignored under `Builds/`.

Edit named pose keys in the Unity Inspector. Ordinary builds preserve the saved resource; **Reset motion library to generator poses** deliberately replaces it with the C# defaults. [FootballMotionBuilder](../Game/Assets/_Game/Editor/FootballMotionBuilder.cs) validates the required records before building. `BuildInitialReview` is the explicit regeneration command, not the ordinary build path.

## Historical motion quality diagnosis — 4 October 2026

**The action set is functionally covered, but the current movement is not accepted as natural or sufficiently smooth.** The user observed awkward hands and legs in both the playable game and the actual-player video. The earlier test count establishes functional behavior and limited pose constraints; it does not establish animation quality. This analysis changes documentation only, with no new runtime or APK release.

The primary problem is the animation implementation. The model's proportions amplify it. Read-only inspection of the retained Blender master measures a **1.70 m** near-LOD mesh, pelvis rest height **0.470 m**, thigh/shin chain **0.307 m**, shoulder-to-wrist chain **0.343 m**, and ankle-to-toe marker distance **0.252 m** on the left. The avatar has deliberately short legs and large shoes, but its overall height is not an import-scale failure. Uniformly scaling the model cannot repair the timing and transition defects below. Skin deformation under extreme joint rotations deserves a separate audit; this inspection does not certify every skin weight.

| Finding in current source | Visible consequence | Correction needed |
| --- | --- | --- |
| `Athlete` plays the 40-frame/60 FPS run at `speed/7`, while `FootballMotion` advances an independent phase. At 7 m/s these are **1.5 versus 2.4 cycles/s**. Dribble contact uses the independent phase too. | Arms drift out of step with the source legs; a foot can be pulled toward the ball during the wrong part of its stride. | One shared gait/contact phase; preserve the authored arm/leg relationship. |
| The `authored` flag switches at **1.25 m/s**. Below it the layer resets every bone to its saved reference; above it it retains the Animator pose. The usual overlay weight is already one. | Walk/run and slowing transitions can jump between unrelated poses despite the Animator's own transitions. | Blend complete poses through a speed range with aligned foot phases. |
| Walk targets move relative to the athlete with a fixed 0.18 m nominal fore/aft sweep. World pins exist only for a short stop interval, not ordinary stance. The calculated displacement is used only to detect teleports. | Feet slide with the body instead of carrying its weight. At steady 0.8 m/s the target formula predicts roughly **0.56 m/s forward stance slip**, before IK constraints. This is an analytical prediction, not a live slip measurement. | Derive travel from actual displacement, retain stance contacts in world space, and fit cadence/stride to this rig. |
| At 7 m/s the body travels **4.67 m per source run cycle** on 0.31 m leg chains. | Motion speed and visible step length are poorly matched. Short proportions make this especially apparent. | Calibrate stride and cadence against distance, then assess whether proportion or speed changes are desirable as a separate design decision. |
| Every frame replaces arm motion with absolute wrist targets and a fixed wrist-to-forearm relationship. Knees use a generic forward bend direction. | Hands look carried or paddled; knees/elbows can look mechanically steered even when all bone lengths remain valid. | Authored shoulder, elbow and wrist arcs; use IK as a limited contact correction with anatomical bend limits. |
| The fall elbow direction changes abruptly at 25% and 72% of the hit timeline (0.20 and 0.576 s). Ground hand correction is applied directly. | Elbow or hand direction can snap during a fall. | Continuous bend guides and smoothly weighted ground contact. |
| Walk and both dribble entries each contain two identical posture keys. All other key intervals independently ease to zero velocity; fall poses hold and then recover rapidly within 0.8 s. Hit/slide transitions do not preserve the previous visible pose as an entry blend. | The set contains procedural posture recipes and mirrored variants, rather than 26 finished full-body performances. Falls can feel like pose changes instead of transferred weight and momentum. | Reauthor complete performances for this rig, with continuous arcs, support changes, impact and recovery; retain responsiveness and explicitly review the existing gameplay time limits. |
| The delivered preview is **15 FPS**; half-speed playback duplicates those frames. | The video exaggerates the stepping and cannot be used to judge fine temporal smoothness. It does not explain the defects also seen in the game. | Capture every frame at 60 FPS and retain a frame-by-frame pose trace. |

The existing checks measure limb length, reach, a toe-to-ball distance during selected contact samples, floor penetration, allocations and gameplay/network state. They do **not** measure stance-foot slip, joint angular velocity/acceleration, bend-guide continuity, phase alignment, self-intersection or credible weight support. Preserving bone lengths is especially weak evidence of natural movement because pure joint rotations preserve lengths by construction.

The next correction should begin with an A/B capture on the **unchanged model**: original clips alone versus the current football layer, then a rebuilt walk/run/stop cycle using one phase and real support contacts. Once that baseline moves convincingly, extend it to dribbling and the four falls. Review modest leg/shoe/rig adjustments only after that comparison; replacing or resizing the character first would leave the confirmed animation defects in place. This is a recommendation, not a claim that those corrections have already been implemented.

Evidence retained under ignored `Builds/FootballMotionDiagnosis/`: `rig-audit.json`, the read-only Blender inspection script/output and `findings.json`. The existing Unity renders and motion traces are under `Builds/FootballMotionQA/Run-20261003-223534-834/`. No art master, imported FBX, gameplay setting, pose library or runtime source changed in this diagnosis.

## Historical implementation — 3 October 2026

[FootballMotion](../Game/Assets/_Game/Sports/Football/FootballMotion.cs) composes **26 editable control-space takes** from [FootballMotionLibrary.asset](../Game/Assets/_Game/Resources/FootballMotionLibrary.asset) with the existing authored run, jump and slide clips. These are compact skeletal pose timelines, not 26 imported FBX clips. The character mesh, textures and bone lengths are shared and unchanged.

| Coverage | Delivered behavior |
| --- | --- |
| Ready, walk, run, sprint | Restrained breathing/gaze, low-speed foot roll, continuous gait phase, opposite arm swing and speed-dependent posture |
| Starts, stops and turning | Left/right accents, deceleration sink, short conditional support-foot pin, immediate restart cancellation, banking and head/chest counter-rotation |
| Control and receiving | Slow/fast alternating shoe contact at the authoritative ball anchor, left/right receiving and ball-aware stopping, inside/outside cut accents |
| Kick, charge and fake | Moving/stationary preparation, either-foot contact and follow-through weighted by power, held charge and cancellation recovery |
| Tackles and contact | Existing authored slide and landing, miss recovery, minor wall brace, four directional falls with recoil, landing and complete recovery within the existing hit deadline |
| Match flow | Ready during resets; short win/disappointment reactions; resets, travel and returning to the room clear the overlay |

The two-bone solvers preserve limb lengths, keep knees bending forward, place elbows outward and keep wrists aligned to forearms. Floor and wall constraints shorten decorative reaches without moving the gameplay capsule. Hand ground clearance applies during live falls. The short, stylized get-up fits the existing **0.8-second** hit lock; it does not add recovery time.

Host snapshots now include fall direction, miss-recovery deadline and sequenced kick/charge/cancel presentation. Remote animation uses the shared server clock and rejects older sequences. Network protocol **18** requires matching builds. Cosmetic poses never award tackles, release balls or add movement locks. Football disables the old running turn-expression layer while its replacement is active; other sports retain their own presentation.

### Editing and reproduction

Edit the resource asset's named keys in the Unity Inspector. [FootballMotionBuilder](../Game/Assets/_Game/Editor/FootballMotionBuilder.cs) checks all required takes before every build and **preserves saved edits**. The explicit **Reset motion library to generator poses** menu command replaces those edits with the defaults in [FootballMotionLibrary.cs](../Game/Assets/_Game/Sports/Football/FootballMotionLibrary.cs). `BuildInitialReview` deliberately performs that reset and is only for regenerating those authored defaults; use normal builds for artist-edited assets.

```powershell
Tools/Build/Build.ps1 -Target Windows
Tools/Build/Test-FootballMotion.ps1 -Fps 30
Tools/Build/Test-FootballMotion.ps1 -Fps 20 -NoCapture
Tools/Build/Test-FootballMotion.ps1 -Fps 60 -NoCapture
Tools/Build/Test-FootballMotion.ps1 -Fps 120 -NoCapture
python Tools/Blender/package_football_motion.py --run Builds/FootballMotionQA/Run-<timestamp>
```

The development-only review samples every take at **31 times on both LODs**, checks finite bounded IK, unchanged limb lengths, skinned-mesh floor clearance, real ball contact, live transitions, ten-athlete allocations and session/network resets. Captures include normal and half-speed movement plus front/side pose sheets. The packaging helper needs Pillow and imageio-ffmpeg in the developer's Python environment; these are review tools, not shipped dependencies. `-NoCapture` retains numeric pose/mesh checks. A chosen timeline rate is not a measured phone frame rate.

Measured 30-rate review: control/fast/cut contact errors **2.2/3.1/1.8 cm**; maximum sampled surface penetration **1.3 cm**. The latter two results slightly exceed the original 3 cm contact/1 cm surface targets below and remain documented tolerances, not a claim of perfect contact. All sampled limb lengths remain within 0.1 mm. Physical-phone touch, rendering, sustained performance and WAN playback still require device testing. Release bytes and current evidence are in [BUILD-SIZE.md](BUILD-SIZE.md) and [VERIFICATION.md](VERIFICATION.md).

## Original design and authoring targets

## Intended feel

The athlete should look springy, balanced and aware of the ball. Starts push from the ground; stops finish with a catching step; changes of direction lead with the gaze and chest; dribbling uses small deliberate touches; a tackle visibly knocks the player off balance before they land and get up. Exaggerate the existing large head, compact body and oversized shoes without stretching the skeleton or making every action a comic tumble.

Responsiveness comes first. Animation follows the actual movement and authoritative ball state. A plant, wind-up, fall or celebration must never create an additional gameplay delay. Existing tackle restrictions remain intentional gameplay rules.

Keep the four islands, one game per host room and shared travel unchanged. Scope covers animation for the existing football controls, plus explicitly separated future moves.

## Foundations and intended additions

The design is grounded in the current [Athlete](../Game/Assets/_Game/Shared/Player/Athlete.cs), [LocomotionMotor](../Game/Assets/_Game/Shared/Player/LocomotionMotor.cs), [FootballTackle](../Game/Assets/_Game/Shared/Player/FootballTackle.cs), [FootballBall](../Game/Assets/_Game/Sports/Football/FootballBall.cs) and [Animator controller](../Game/Assets/_Game/Settings/Athlete.controller). Source takes precedence over historical timing/protocol descriptions in older documents.

| Area | Current foundation | Design addition |
| --- | --- | --- |
| Movement | Idle/run, eased speed, independent body facing | A football ready pose, proper low-speed walk, start push, phase-aware stop and sprint transition |
| Turning | Four authored turns, used as masked head/torso expression while running | Banking, hip counterbalance, foot recovery and ball-side cuts without reintroducing stationary turn locks |
| Possession | Attached ball at a nominal 0.65 m front anchor; rotation follows travel | Visible receiving, alternating control touches, ball-aware stopping and turn footwork |
| Kicking | Immediate authoritative release; one-second charge; fake-shot cancellation | Preparation while moving, contact, follow-through and cancel recovery |
| Tackling | Authored slide, hit reaction, sweep contacts and recovery | More readable entry/exit, directional falls, ground contact and complete get-ups |
| Jumping | Authored preparation, flight and landing | Football-ready landing blend; preserve airborne evasion and possession restrictions |
| Match flow | Team selection, kickoff, live play, overtime, finish and reset | Ready anticipation and brief optional result reactions that obey those phases |

### Existing gameplay rules to preserve

- Grounded input changes translation on the next simulation tick. Facing smooths independently. Normal ground reversals retain speed; do not insert a brake-to-zero or pivot wait. [Responsive movement](VisualDirection/RESPONSIVE-MOVEMENT.md) explains the earlier regression to avoid.
- Base movement/sprint limits are 4/7 m/s. Possession multiplies these by 0.75: 3/5.25 m/s. Charging replaces the possession penalty with 4 x 0.6 = 2.4 m/s. Treat these as animation inputs, not tuning to change in this pass.
- A controlled ball remains attached. Walking, sprinting and turning do not release it. Acquisition only accepts an eligible grounded player and a sufficiently slow reachable ball; the current speed ceiling is 6.2 m/s. Depict fast unclaimed balls as interceptions/deflections, not automatic catches.
- Possession blocks jumping and tackling. Airborne players cannot initiate or receive a ground tackle. Existing jump preparation and landing do not add horizontal control locks.
- A slide lasts **0.425 s**, with up to **4.62 m** unobstructed travel over **0.275 s**. Cooldown is **4 s**. A complete miss adds **0.8 s at 40% movement speed**; steering remains active during that miss recovery.
- A received tackle blocks player-controlled movement for **0.8 s**, with up to **0.72 m** knockback over the first **0.24 s**. Teammates can also be hit. Mutual slides knock both players down. Do not downgrade accepted hits to cosmetic stumbles or add extra stun after the get-up.
- Kick release is immediate when accepted, with a **0.35 s** repeat cooldown. Holding reaches full charge in **1 s**, and can remain held. Cancel retains possession and requires a new press to start another charge.
- Host simulation decides contact, possession, release and match outcomes. Animation events cannot independently kick a ball, award a tackle or score a goal.

## Complete performance coverage

All durations in this section describe visible performance, not additional input locks. Variants are chosen by speed, support foot, contact direction and actual action state, rather than random changes that alter timing.

### Locomotion and stopping

| Situation | Performance | Blend/exit contract |
| --- | --- | --- |
| Ready without ball | Soft knees, weight shared between feet, low open hands, restrained breathing and occasional gaze to play | Replace the playful idle only during active football; movement cancels immediately |
| Ready with ball | Slightly wider stance, protecting arm and gaze alternating between ball and field | Toe hovers beside the ball; no perpetual stamping or idle ball orbit |
| Start from rest | Chest inclines, rear foot pushes, opposite arm leads, first step is short | Visual accent around 0.12-0.18 s; movement begins immediately; choose left/right support from the pose |
| Walk / small stick input | Heel-to-toe steps, minimal bounce, arms close to body | Dedicated low-speed gait; avoid stretching a sprint cycle into slow motion |
| Run / sprint | Progressively longer stride and forward lean; stronger arm drive at sprint | Preserve continuous gait phase when speed or sprint changes; use actual travel to avoid running against a wall |
| Gentle release | Shorten the next step, chest rises, trailing foot catches up | Body begins slowing with the motor; visible settle around 0.16-0.22 s |
| Fast release | Lead foot reaches slightly forward, knee absorbs, hips settle back, small corrective second step | Settle around 0.24-0.32 s; the final in-place settling may outlast translation; do not slide the root forward to finish the pose |
| Immediate restart | Push from the nearest usable support foot | Cancel the obsolete stop on the next update; no queued stop completion |
| Wall or goal-net obstruction | Short brace, shortened stride, hands/shoulders respond if close | Use actual displacement and collision normal; no run-in-place at full cadence and no mesh passing through the net |
| Sideways/backward movement during a reversal | A brief crossing/recovery step while body catches up with input | A transient pose correction, not a new strafe control mode or a full stationary pivot |

Stop selection uses deceleration and the current gait contact phase. If the wrong foot is down, select the other variant or reduce the accent; never teleport a planted foot to the preferred pose. Foot locking is short and conditional on reach. Release it immediately when new input, a wall, or a reversal makes the target invalid.

### Dribbling and receiving

| Situation | Performance | Ball/contact contract |
| --- | --- | --- |
| Acquire slow ball while still | Open the near foot, cushion with the inside, draw weight onto the other leg | Begin from the actual acquisition state; about 0.16-0.24 s; possession is already authoritative |
| Acquire while moving | Receiving foot meets the ball on its recovery step, chest remains directed into travel | Blend into the correct dribble phase without stopping the motor |
| Slow control | Small inside-foot touches, a slightly wider base, relaxed protecting arm | Initial visual cadence 1.6-2.0 touches/s; alternating feet where reachable |
| Fast control | Longer approach step, firmer instep touch, stronger arm counterbalance | Initial 2.2-2.8 touches/s; derive from stride phase and distance, not an independent looping timer |
| Stop with possession | Cushion beside the ball; one sole-check pose when the stance permits; then ready | Nominal anchor remains 0.65 m; do not pull the collider back under the shoe |
| Start with possession | First pushing touch blends directly into the first running step | Choose the available foot; do not wait for a preferred foot's turn |
| Curved travel | Touch from the outside of the path, hips bank inward, free arm balances | Preserve continuous cycle phase; scale the accent by measured yaw change |
| Tight cut | Inside/outside of the appropriate shoe guides the ball's new heading | Foot targets follow the constrained ball anchor; do not invent an independent ball arc through the body |
| Ball stolen / released | Contact foot follows through briefly, protecting arm drops, gaze tracks the free ball | Lose possession overlay immediately; never finish a dribble touch on an absent ball |
| Boundary correction | Short checking step with less extension | Follow the actual constrained anchor; no decorative ball offset across a pitch line or into a post |

The ball must look contacted, not magnetically chased by both feet. Only one foot receives a ball target at a time. Fit contact to a point on the ball surface, not its centre. Preserve the rig's bone lengths; cap reach correction and select a different phase when unreachable.

**First implementation choice:** keep the authoritative ball and visible centre together. Build convincing foot approach, touch and withdrawal around the existing anchor. Do not add rhythmic rigidbody impulses, release/reacquire cycles, independent collider motion, or a second visible ball. This constrains how realistic a long sprint push can look. If close-camera review shows convincing dribbling cannot fit that anchor, report it as a separate ball-control design decision rather than silently changing the approved possession model.

Stationary dribbling ends in ready after the last settling touch. A stationary ball must not rotate endlessly. Never extend the legs, widen the capsule or shift the gameplay root solely to reach the ball.

### Direction changes

| Heading change | Body detail | Feet and possession |
| --- | --- | --- |
| Small curve, roughly 0-30 degrees | Eyes/head lead slightly, chest follows, mild bank | Keep the current running contacts; adjust free-foot recovery |
| Medium cut, roughly 30-75 degrees | Outside hip loads, torso leans into the new heading, opposite arm opens | Short outside support accent; inside foot makes the next recovery step |
| Sharp cut, roughly 75-135 degrees | Lower centre of gravity, clear shoulder/hip counter-rotation | Brief outward shoe placement with a compact ball-side touch; stay in locomotion |
| Reversal, roughly 135-180 degrees | Look over selected shoulder, chest unwinds, hips catch up | A recovery/crossover step while translation already follows input; no 180-degree planted-root spin |
| Rapid alternating input | Small bank and head tracking only | Suppress large cuts and release foot locks; sustained input, not every input edge, selects a cut accent |
| Camera-only rotation | Keep the character's actual motion/facing interpretation | Never trigger a cut from camera yaw alone |

These angle bands are presentation starting points. Use hysteresis and a short visual refractory interval (initial target 0.10 s) to prevent flickering. They must never debounce player movement. The existing turn layer affects spine/neck/head; add hip/foot work through an explicitly coordinated football pose pass, not by turning on full-body authored pivots underneath running.

### Kick, charge and fake shot

1. **Press/hold:** weight shifts toward the support leg and the kicking ankle opens. While moving, keep alternating steps; add shoulder preparation rather than dragging a permanently raised leg along the pitch.
2. **Charge:** preparation grows with the existing one-second charge fraction. At maximum, use a sustainable ready pose with small breathing, not repeated wind-ups. The pose remains turnable and compatible with 2.4 m/s movement.
3. **Accepted release:** choose the reachable foot, brace the opposite knee, show contact and follow-through in the actual `KickDirection`. Light kicks use a compact inside-foot action; stronger kicks increase instep extension and torso counter-rotation. Initial follow-through target: 0.18-0.28 s, blended out by 0.35 s.
4. **Cancel/fake:** retract the prepared foot, make a small shoulder dip, return to the appropriate dribble phase in about 0.12-0.18 s. Keep the ball; do not emit release VFX or sound. Cancel while turning must not freeze the legs.
5. **Rejected release or lost eligibility:** relax the pose, retain only the real simulation result. A cosmetic swing must not imply that a blocked/rejected kick launched the ball.

**Immediate-release constraint:** the present simulation has no wind-up delay. A held charge can anticipate contact, but a zero-duration tap cannot play a new 150 ms backswing before an already released ball. For taps, start from a compact contact pose on the first rendered accepted-action frame and blend rapidly into follow-through; test the resulting foot discontinuity. Do not promise perfect pre-contact anticipation for every tap. If this looks unacceptable on the real rig, a short authoritative wind-up is a separate gameplay change, not an animation-only fix.

Add an accepted-kick presentation event/timeline; `TryKick` currently performs physics without a kick animation state. Ball sequence changes alone are insufficient to identify who kicked or whether the change was a reset. Cancel must also have a distinguishable presentation edge; the current input charge Boolean is not a full remote animation timeline.

### Tackle attacker

Refine the existing saved slide rather than adding a duplicate full-body move. These milestones fit its current 0.425 s gameplay clock:

| Time | Pose and contact |
| --- | --- |
| 0.000-0.065 s | Immediate low drop, reaching leg extends, other knee folds, one hand prepares to support. Travel has already begun. |
| 0.065-0.215 s | Clear low slide silhouette; leading shoe reaches, torso reclines, support hand stays clear of turf. |
| 0.215-0.275 s | Slide decelerates; hips rotate toward a recoverable side, support knee begins coming underneath. |
| 0.275-0.425 s | Push through hand and folded leg, bring torso up, transition toward current input. Normal action lock ends exactly at 0.425 s. |

At 30 fps the entry spans only about two frames: silhouette and contact matter more than extra flailing. Collision may shorten travel; pose progression still follows the action clock. Do not keep the visual sliding through a wall when capsule movement stops.

Successful contact uses a short shoulder/leg recoil layered onto this timeline. A complete miss then uses a low, laboured running/ready overlay for the existing 0.8 s penalty; it must not show the player lying down while steering is allowed. A received hit overrides any active slide or miss recovery. Do not add a separate recovery lock for the attacker.

### Tackled player: directional fall and recovery

Every accepted hit gets a complete performance: **impact -> loss of support -> ground contact -> get-up -> playable stance**. The full sequence fits the existing 0.8 s hit duration. This is a fast arcade knockdown, not a realistic long incapacitation.

Choose direction once at the host's accepted contact, in the victim's facing frame. Use the push vector projected onto victim forward/right: push backward produces a backward fall; push forward produces a forward trip; dominant left/right push produces the matching side fall. Direction means the victim's fall direction, not the attacker's screen position. Resolve diagonal ties deterministically and retain the chosen variant for that action sequence.

| Variant | Loss of support and landing | Getting up |
| --- | --- | --- |
| Backward | Chest recoils, knees buckle, hips meet ground first, back rounds; head stays lifted | Roll toward one hip, plant foot/hand, press into crouch and stand |
| Forward | Trailing foot is lost, chest tips forward, arms protect, knee/forearm absorb landing | Hands under shoulders, one knee under hips, step through into ready |
| Left side | Left hip drops, right arm counterbalances, left forearm/hip take the landing | Roll partly forward, use right foot and left hand to rise |
| Right side | Mirror the left-side performance with a validated skeletal mirror | Opposite supporting limbs; same timing and root travel |

| Hit time | Required reading |
| --- | --- |
| 0.00-0.08 s | Sharp torso/head recoil and immediate release of any foot/ball contact constraints |
| 0.08-0.24 s | Support foot fails, pelvis lowers and body follows the authoritative knockback |
| 0.24-0.40 s | Clear hip/forearm ground contact and a short compression/settle |
| 0.40-0.68 s | One hand and one foot find support; pelvis rises into crouch |
| 0.68-0.80 s | Recover toward ready; finish with a usable foot phase for the held movement direction |

At 0.8 s, movement resumes under the existing rules, even if a visual blend has a few frames left. Get-up variation never changes vulnerability, cooldown or stun length. Head and face do not strike the ground; avoid injury acting, repeated body bounces and ragdoll flailing.

The existing hit source is a backward recoil/balance adjustment; stretching that one action does not create four readable falls. Author the four whole performances on the saved rig. Keep the standing CharacterController and authoritative displacement intact; the low mesh is presentation, not a new prone hitbox. Fit falls near walls/goals within the reachable visual envelope, using a compact variant/pose reduction rather than clipping limbs through a collider. Full procedural ragdolls are outside this design.

**Minor bumps:** a brief shoulder/upper-body stumble may decorate an ordinary collision only when an actual collision signal exists. It does not remove possession or stun. All successful slide hits still use the full hit contract above.

### Jump, match reactions and small details

- Reuse the authored jump, including its existing preparation, air steering and landing compression. Blend to running when moving, ready when still. Do not add aerial ball control, tackleable airborne frames or jumps while possessing the ball.
- Head direction anticipates a cut by a small visual offset and then follows the chest. During a hit, use the authored head pose; ball tracking must not twist the neck through a fall.
- Hands balance at starts/stops, protect space during dribbling and open before a fall. The rig has mitten-like hands; do not specify finger curls unsupported by the skeleton.
- Keep foot roll, knee flex and hip height coordinated. Small shoulder/head follow-through supplies weight; avoid whole-character scale pulsing and foot jitter.
- Drive ball spin from actual displacement. Tie optional step/touch/slide sounds and sparse turf marks to confirmed contact phases. Reuse available effects/audio only after verifying they exist; none are required or added by this design.
- During kickoff, restrained weight shifts communicate readiness. A regulation goal immediately follows the existing reset/countdown, so do not delay it for a celebration or carry an old kick/fall across the teleport.
- Overtime begins continuously: no celebratory reset or interruption at the transition. A finished win/loss/draw may use a short result pose. Exit/travel/new match cancels it immediately. No new goal-scorer attribution is assumed.
- First-person mode continues hiding the local whole-body mesh under the current implementation. Do not expose an incomplete headless mesh or move the camera with pelvis dips/falls in this pass.

## Authoring and asset inventory

Use the same 34-bone Generic rig, bind transforms, shared mesh/materials and both LODs. Root motion remains disabled. Preserve the original model and existing idle/run/turn/jump/football masters. Extend the football authoring workflow with saved editable actions; setup must not overwrite artists' existing curves.

The following is a **candidate upper bound of 24 new core takes**, not a requirement to ship 24 files. Start with the earliest phase and consolidate variants when actual-rig review proves quality is preserved. Left/right variants are separately audited exports; do not assume Unity Humanoid mirroring works on this Generic rig or use negative character scale.

| Proposed take family | Takes | Phase | Treatment |
| --- | ---: | --- | --- |
| `FB_Ready` | 1 | A | Loop, ball/no-ball differences supplied by a small overlay |
| `FB_Walk` | 1 | A | Loop; shared for low-speed football locomotion |
| `FB_Start_L`, `FB_Start_R` | 2 | A | Short support-foot accents |
| `FB_Stop_L`, `FB_Stop_R` | 2 | A | Scale weight by deceleration; do not double the files for walk/sprint |
| `FB_Dribble_Control`, `FB_Dribble_Fast` | 2 | B | Phase-aligned loops with both feet in each cycle |
| `FB_Receive_L`, `FB_Receive_R` | 2 | B | Reach-aware short actions |
| `FB_BallSettle_L`, `FB_BallSettle_R` | 2 | B | Stop/control accents around the real anchor |
| `FB_Cut_L`, `FB_Cut_R` | 2 | B | Graded accents; existing turn expression supplies the head/chest component |
| `FB_Kick_L`, `FB_Kick_R` | 2 | B | One light-to-strong pose family per foot |
| `FB_FakeCancel_L`, `FB_FakeCancel_R` | 2 | B | Return prepared limb to the current gait |
| `FB_Fall_Back`, `FB_Fall_Forward`, `FB_Fall_Left`, `FB_Fall_Right` | 4 | C | Complete impact/ground/get-up takes, each mapped to 0.8 s |
| `FB_WhiffRecover` | 1 | C | Masked movement-compatible overlay for the existing penalty |
| `FB_Bump` | 1 | C | Small collision accent; direction supplied by bounded overlay |
| `FB_Result_Win`, `FB_Result_Disappointed` | 2 optional | D | Outside the 24 core takes; only after core motion and budget pass |

Reuse/refine the existing slide, running, idle, four turns and jump; they are outside the new-take count. Frame rate in the master may remain 60 fps for editing, but exported key density and compression must be justified by contact quality. Animation-only exports must not embed mesh/material/texture duplicates. Exporters read saved curves and preserve relative dependency paths. Any modified Blender/FBX dependency must pass format-aware inspection and applicable integrity checks.

## Runtime integration contract

This is a proposed implementation layout, not new code already present in the repository.

1. **Simulation produces facts:** speed/velocity, actual yaw, acceleration/deceleration, grounding, possession, charge, accepted kick, cancel, slide, hit direction and action sequence/time. Derive presentation speed from actual travel after constraints.
2. **Base locomotion supplies continuity:** idle/ready/walk/run and continuous stride phase. Starts/stops are interrupts/accents with no animation-gated locomotion. With possession, phase-aligned dribble loops supply the lower body instead of blending unrelated cycles.
3. **One football pose owner composes accents:** bank, gaze, arms, ball contact and short foot placement. Existing turn expression must be attenuated where it would double chest rotation. Adopt the reversible-pose approach used by [BasketballMotion](../Game/Assets/_Game/Sports/Basketball/BasketballMotion.cs), but never run both sports' overlays on the same skeleton in a frame.
4. **Full-body overrides own their contact windows:** jump/slide/hit suppress inappropriate gaze, dribble and foot locks. Do not add another layer that fights the current `Football action` or `Jump` layer.
5. **Constrained contact runs last:** ground/ball targets solve only active limbs using bounded two-bone corrections. Restore last frame's procedural changes before Animator evaluation; avoid accumulating rotations on unanimated bones. Cache bones and scratch data; no per-frame object allocation or rig rebuild.

Priority: lifecycle reset/travel and authoritative match reset -> accepted hit -> valid slide or jump -> accepted kick contact -> receive/charge/cancel -> dribble/stop/cut -> ready and optional expression. Existing eligibility prevents most conflicts; priority resolves presentation only and does not grant actions the simulation rejected.

Suggested fade targets: ordinary overlays enter over 0.06-0.10 s and leave over 0.08-0.14 s; hit recoil enters within one rendered frame; full-body recovery finishes on its authoritative deadline. Evaluate from elapsed seconds/server time, not frame counts. These are tuning values to inspect, not guaranteed final settings.

### Multiplayer and interruption

The existing [FootballSnapshot](../Game/Assets/_Game/Shared/Player/FootballSnapshot.cs) carries only action, start time, cooldown and sequence. It does not carry hit direction, chosen foot, charge timeline or accepted kick/cancel identity. A future integration needs an explicit, compact host-authored presentation payload; do not infer accepted hits/kicks from visual overlap or generic ball sequence changes.

Replicate action kind, sequence, server start time, chosen variant/foot and quantized impact direction as needed. Charge requires active state/start time and cancel/release edges. Reuse existing player network objects and owner-authorized request paths. Increment the then-current protocol when serialization changes; inspected merged source uses protocol 17. Matching clients are required.

For a remote action, sample its current elapsed phase on arrival. Never restart a late fall at frame zero, replay an expired kick, or add another 0.8 s of stun. Ignore duplicate/out-of-order action sequences. Locally predicted anticipation may affect pose only; accepted release and possession remain authoritative. Do not imply a new movement-prediction system is already available.

Reset phase, locks, overlays, pending contact effects and action caches on kickoff teleport, sport/travel transition, despawn/disconnect, disable and match restart. Focus loss/cancel clears charging without a kick. Sustained input after a get-up enters the appropriate moving phase; disallowed presses do not queue a surprise tackle or jump.

## Budget and performance gates

The task inspected an existing APK of **86,498,912 bytes** with **13,501,088 bytes** to the strict 100,000,000-byte threshold, and **11,498,912 bytes above** the 75,000,000-byte development target. Its recorded SHA-256 is `6EB19CFA79CF55571963DA49DFE687B6402E6F7A675A375B9C1503C9D0F91418`. The [build-size document](BUILD-SIZE.md) records a different 86,563,288-byte integration release. This discrepancy is deliberately not relabelled as a new validated release; the inspected file alone does not establish current-source freshness.

That earlier design pass changed documentation only and added no shipped data. Its evidence belongs under ignored `Builds/FootballAnimationDesignQA/`. The implementation has a separately measured baseline and fresh build, recorded in [BUILD-SIZE.md](BUILD-SIZE.md).

For implementation, use a **provisional 500,000-byte maximum incremental APK allowance for the complete core animation pass**, not a prediction of its cost or permission to spend the remaining reserve. Measure each phase; stop and investigate unexpected growth even below that allowance. Continue reducing avoidable shipped duplication, with unused-asset proof before archiving. The 75 MB target remains unmet.

Prefer saved compact skeletal curves, shared clips, bounded procedural accents and existing materials. Add no animation middleware, duplicated character mesh, new texture atlas, full ragdoll system or bulk motion pack by default. Keep high-quality editable masters outside shipped Unity resources. Use key reduction only after comparing feet, ball contact and face/hand silhouette at both LODs.

Measure CPU/allocations with the maximum current match roster of ten players and a crowded tackle. Distant LOD may reduce cosmetic IK/gaze update frequency, but never change simulated hit timing, possession or movement. Set device performance acceptance against a measured phone baseline; Windows preview cannot establish phone FPS, thermals or touch quality.

## Production order and review gates

| Phase | Deliverable | Gate before expanding |
| --- | --- | --- |
| A: movement | Ready/walk, support-foot starts/stops, bank and recovery on turns | No delayed movement, no rapid-reversal stall, no ice-skating feet; front/side/game-camera review |
| B: possession | Slow/fast dribble, receive/settle, cuts, kick/charge/cancel | Ball contact fits the real rig and fixed anchor; immediate tap, sustained charge and cancelled fake shot remain responsive |
| C: contact | Refined slide, four complete falls, miss overlay, minor bump | Four fall directions readable, no extra stun, correct mutual/teammate hits, no wall penetration |
| D: polish | Optional result poses, small gaze/hand/effect/audio refinement | Match lifecycle, network timing, ten-player cost and APK reserve still pass |

Each phase produces a playable slice and normal-speed/half-speed captured review before the next asset batch. Build only what improves the actual gameplay camera. A pose sheet alone does not prove transition quality.

### Required review matrix for implementation

| Scenario | Acceptance |
| --- | --- |
| Start/stop with either support foot at walk/run/sprint | Root obeys motor; feet do not visibly skid on held contact; fresh movement interrupts settling |
| Turns at 30/60/90/135/180 degrees; alternating every 1/3/6/12 frames | Correct first-tick travel, stable foot phase, no endless pivot or accumulated pose twist |
| Slow/fast possession, receiving, abrupt stop, boundary/corner/post | Correct owner and anchor, single-foot contact, no stretch or ball through shoe/body/net |
| Zero-time tap, half/full charge, held maximum, moving/turning cancel | Physics timing unchanged; direction matches actual release; cancel produces no release and new press works |
| All four hit directions, diagonal ties, mutual slides, friendly contact | Correct host-selected fall and complete get-up by 0.8 s; no duplicate hit presentation |
| Slide success, ball-only contact, complete miss, wall-shortened slide | Existing 0.425/0.275 s timing and miss-only 0.8 s slowdown preserved |
| Jump evasion, landing into run/idle, possession jump rejection | Existing motor/eligibility preserved; landing overlay does not fight football poses |
| Goal reset during kick/fall, overtime transition, result/travel/leave | Reset clears pose; overtime stays continuous; no stale action after teleport or scene activation |
| Host/guest initiate/receive; duplicate/late snapshots; disconnect | Same variant/time and possession; no replayed kick/fall or client-authored physics |
| 20/30/60/120 fps, both LODs, close/third-person/first-person | Stable durations, limb lengths/contact, mesh visibility and camera height |
| Ten players and crowded tackles on target Android | Profile against baseline, inspect touch and rendering; document remaining limits |

Initial visual tolerances for actual-rig review: supported soles should not visibly penetrate the surface; investigate more than 1 cm penetration or 2 cm drift during a declared planted interval. At intended ball contact, target less than 3 cm shoe-to-ball-surface separation. These are proposed measurement gates, not results. At high-speed reversals, explicitly releasing an unreachable plant is preferable to stretching the leg to pass a numeric threshold.

Run existing movement/football suites alongside the implemented dedicated motion review above. Commands, from the repository root:

```powershell
Tools/Build/Build.ps1 -Target Windows
Tools/Build/Test-Idle.ps1
Tools/Build/Test-Turn.ps1
Tools/Build/Test-Jump.ps1
Tools/Build/Test-Tackle.ps1
Tools/Build/Test-FootballBall.ps1
Tools/Build/Test-FootballMatch.ps1 -PlayerPath Builds/WindowsFinal/WhatTheFish.exe
Tools/Build/Build.ps1 -Target AndroidSubmission
Tools/Build/Check-TaskReady.ps1 -RequireApk
```

Inspect the new actual APK and `Builds/SizeAudit/latest/`, report before/after bytes and headroom, update [build size](BUILD-SIZE.md) and [verification](VERIFICATION.md) for that measured release, and retain current evidence under ignored `Builds/`. Never use an older APK or this design study as validation of implemented motion.

## Moves reserved for future gameplay

Dedicated short/long pass controls, lob/chip trajectories, headers/volleys, goalkeeper dives/catches, standing poke tackles, manual shielding, step-overs/drag-backs, fouls, throw-ins and corner/free-kick ceremonies need corresponding gameplay rules and authoritative events. They are not required to animate the existing control set and are not silently introduced here. Light kicks can use passing-like footwork without adding a new pass mechanic. Add these families when those features are deliberately implemented.

## Related sources

- [Football possession and physics](FOOTBALL-PROTOTYPE.md) and [match rules](FOOTBALL-MATCH.md)
- [Existing tackle authoring](VisualDirection/TACKLE-AUTHORING.md), [turn authoring](VisualDirection/TURN-AUTHORING.md), [idle authoring](VisualDirection/IDLE-AUTHORING.md) and [jump](JUMP-ANIMATION.md)
- [Character pipeline](VisualDirection/CHARACTER-PIPELINE.md), [basketball pose-layer precedent](BASKETBALL-ANIMATION.md), [APK audit](APK-SIZE-AUDIT.md) and [repository hygiene](REPOSITORY-HYGIENE.md)
