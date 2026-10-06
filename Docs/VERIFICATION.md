# Verification records

## Basketball defense — 6 October 2026

[Implemented controls and success rules](BASKETBALL-DEFENSE.md) cover pressure
guarding, grounded hand blocks and physical jump blocks. The motions reuse
the shared rig with lowered stance, alternating lateral steps, shoulder-led
reach, a balancing arm, takeoff load and landing absorption. The host resolves
contact from the same bounded palm path used by the pose.

The selected final feature and regression suite contains **978 passing
assertions in 22 reports, zero failures**, inventoried in
`Builds/BasketballDefenseQA/checks.json`. It covers:

- Offline defense at 20/30/60 FPS, both hands, both character LODs, continuous
  joints, palm/contact agreement, real jump height and grounded recovery.
- Rendered host/guest guarding, limited strafe speed, pressure and wall
  occlusion; dribble, ordinary shot and released-pass blocks; torso shielding,
  range misses, repeated inputs, stale requests, reset and possession changes.
- Owner RPC jump blocks against layups and dunks at both baskets, with
  interrupted releases occurring at most once and both players landing.
- Existing shot charging, physical scoring and nets, passes, steals, finishes,
  mouse/touch gesture routing, free roam, disconnect, room restart and shared
  jump behavior.
- Two final multiplayer gameplay runs confirming the guest's displayed
  release power matches host evaluation within **0.00001** and **0.00004**.

Slow rendered capture exposed a network timing defect: Netcode updates its
clock after Unity's catch-up physics. Extrapolating the action clock at each
fixed step keeps jump movement, held-ball motion and block windows together.
A further regression exposed the case where a received charge sample is ahead
of the guest's buffered clock. Release now uses that displayed sample's time,
while retaining the host's bounded timestamp validation. Earlier failed
fixtures and diagnostics were archived; they are not counted as passing runs.
The final network corrections were checked separately from the unchanged
offline pose and physics checks.

Evidence and presentation:

- `Builds/BasketballDefenseQA/defense-review.mp4`: sampled gameplay and clear
  jump-form capture at normal and half speed; `defense-storyboard.jpg` provides
  representative stance, reach and recovery frames.
- `Reviews/Local-30-20261006-223631-615/host/` under that evidence folder holds
  the rendered contact trace, motion samples and attacker/defender UI captures.
- `PortableEvidence/` and `portable-test.txt` record the review script running
  from a path containing spaces, with an unrelated working directory and its
  default complete-player path. This checks script/runtime path resolution,
  not a fresh-clone Unity build.
- The complete Windows player was promoted to `Builds/WindowsFinal/` at
  **22:54–22:55 Malaysia time**. All **315 files**, including preserved portable
  launchers, match the candidate hashes apart from `LATEST-BUILD.txt`.
- Fresh signed AndroidSubmission: **89,825,364 bytes**, **+22,360 bytes**, with
  **10,174,636 bytes** of hard-limit headroom. The 75 MB target remains exceeded.
  [Size and signature](BUILD-SIZE.md#basketball-defense--6-october-2026).
- Final Android packaging preserves 3,061/3,062 source inputs byte-for-byte;
  the regenerated Golf prefab's 14 objects retain all properties and reference
  topology. Runtime scripts and scenes are identical across packaging.

Reviewed local archives, all under ignored `Legacy/`:
`20261006-222935-294-basketball-defense-apk`,
`20261006-224938-264-basketball-defense-interim-apk`,
`20261006-225026-615-basketball-defense-portability`,
`20261006-225403-689-basketball-defense-windows` and
`20261006-225606-927-basketball-defense-iterations`. Manifests retain recovery
paths, sizes and hashes. Current APK, complete player and useful validation
evidence remain under `Builds/`. No game asset was archived.

Protocol **29** requires matching peers. No phone was connected; physical
Android touch, appearance, FPS, thermals and WAN timing remain unverified.
Git publishing remains with the user.

## Untimed finishes and shot fallback — 6 October 2026

The failed-looking up/down gesture was an unavailable finish entering the old
cancel/recovery path. Mouse and touch routing through Unity's input module
was intact. A rejected finish now releases an ordinary shot with the original
charge phase. Ready finishes hide the timing meter: **up is an unblocked 100%
dunk; down is a layup with one host-side 85% make roll**. Eligibility extends
to 4 m and checks grounding, approach, actual run-up and jump clearance.
The gather brakes while the ball returns from a bounce and keeps a short
planting allowance. [Rules and motion design](BASKETBALL-FINISHES.md).

The complete **01:04:56 Malaysia time**, protocol **28** Windows player passed
**811 assertions with zero failures** across the final retained suites:

| Suite | Passes | Evidence |
| --- | ---: | --- |
| Both hoops/hands, LOD1, standing layup, immediate release, wider approaches, planting grace, blocked jumps, reset, duplicate commands and both physical layup outcomes at 20 and 30 FPS | 368 | `Builds/BasketballFinishQA/Reviews/Offline-20261006-010545-675/`, `Offline-20261006-010546-202/` |
| Mouse/touch through StandaloneInputModule at 120 FPS: off-button release, immediate ready finishes, cancellation, gesture ownership and far-range shot fallback with preserved power | 76 | `Builds/BasketballFinishQA/Reviews/Offline-20261006-010546-584/` |
| Host dunk, guest layup make/miss, replicated pose, physical score and host-only outcome authority | 24 | `Builds/BasketballFinishQA/Reviews/Local-20261006-010544-866/` |
| Complete copied player and default launcher paths with spaces, invoked from an unrelated working directory; gesture checks at 20 FPS | 76 | `Builds/BasketballGestureFixQA/PortableEvidence/` |
| Basketball physics, power, scoring, net response and HUD | 81 | `Builds/BasketballPhysicsQA/FinishRules-20261006/` |
| Looping charge, aiming, cancellation and court boundary regression | 46 | `Builds/BasketballChargeQA/Reviews/FinishRules-20261006/` |
| Shared jump, recovery and two-player movement regression | 140 | `Builds/JumpQA/Run-20261006-010721/` |

`Builds/BasketballGestureFixQA/checks.json` records every final report. The
input tests supply mouse and touch states through a BaseInput override to the
actual Unity input module; they do not establish physical phone touch behavior.
The portability fixture verifies script-relative defaults and the complete
copied player, rather than a fresh-clone Unity build.

The **26.8-second** `Builds/BasketballGestureFixQA/finish-rules-review.mp4`
shows an untimed dunk, a successful layup, a wider planted approach and the
layup miss branch, each at normal and half speed. The inspected storyboard
shows gather, takeoff, release and landing. Final raw frames and telemetry
remain in the 30 FPS report folder. Maximum sampled held-ball palm error
during an active finish is **2.1 cm**; sampled architecture overlap is zero.
Each successful action releases once and recovers to the ground. The ready
control capture hides the timing bar and identifies the 85% layup rule;
fallback captures retain the ordinary meter and explain the normal shot.
Controlled random seeds verify both physical outcomes and exactly one host
roll, not an empirical sample estimate of the configured 85% probability.
No defensive block mechanic is added; released balls remain physical.

Fresh AndroidSubmission completed at **01:09:38 Malaysia time**, passing the
strict budget gate and independent APK v2 signature verification:
**89,798,556 → 89,803,004 bytes (+4,448)**, with **10,196,996 bytes** of hard-limit
headroom. The 75 MB development target remains exceeded by **14,803,004 bytes**.
[Packing audit and hash](BUILD-SIZE.md#untimed-finishes-and-shot-fallback--6-october-2026).
The existing Android Debug signing identity is retained. No phone was
connected; Android appearance, physical touch, FPS, thermals and WAN timing
remain unverified.

Across Android packaging, **3,048 of 3,050** source/asset/config inputs remain
byte-identical; runtime scripts and scenes are unchanged. Format-aware
comparison confirms the regenerated Golf prefab's 14-object graph retains all
properties and reference topology. The other difference removes two SSAO
entries from the Android URP runtime registration list while preserving their
definitions. Evidence is in `windows-source.json`, `android-source.json` and
`generated-input-verification.json` under `Builds/BasketballGestureFixQA/`.

At **01:11 Malaysia time**, the complete tested player was promoted to
`Builds/WindowsFinal/`. All **315 files** matched the candidate by SHA-256 before
updating `LATEST-BUILD.txt`. The delivery preserves the existing menu, Golf,
charging and steal work. The previous delivery note was checked before
replacement; no newer concurrent delivery was overwritten. Promotion and
final file hashes are retained under `Builds/BasketballGestureFixQA/`.
Rooms require matching protocol **28** builds.

Reviewed superseded local output was archived with recovery manifests:

- `Legacy/20261006-003811-039-basketball-gesture-diagnostic/` — initial diagnostic player, 313 files, 593.55 MB.
- `Legacy/20261006-010732-484-basketball-finish-rules-apk/` — previous APK, 1 file, 89.80 MB.
- `Legacy/20261006-011150-658-basketball-finish-rules-windows/` — previous complete Windows player, 315 files, 593.55 MB.
- `Legacy/20261006-011943-417-basketball-finish-rules-iterations/` — superseded reviews and duplicate player fixtures, 2,313 files, 2,183.96 MB.

Current validation, reproduction evidence, media, source snapshots and size
audits remain under ignored `Builds/`. No active art, authoring master or Unity
cache was archived. No staging, commit, push or history rewrite was performed.
Final readiness evidence is `Builds/BasketballGestureFixQA/task-ready.txt`.

## Basketball layups and dunks — 5 October 2026

The existing Shoot control selects **Dunk on an upward drag**, **Layup on a
downward drag**, and a normal shot on return to centre. Keyboard players hold
**E** and choose with the arrow keys. The host validates possession, grounding,
distance, approach, actual movement and clearance before committing the
gather, jump, release and landing. Wider timing windows reward a valid close
approach; every basket still requires the free physical ball to cross the rim.
[Controls, rules and animation design](BASKETBALL-FINISHES.md).

The complete **20:57:41 Malaysia time**, protocol **27** Windows player passed
**994 assertions with zero failures** across the final retained suites:

| Suite | Passes | Evidence |
| --- | ---: | --- |
| Both hoops/hands, LOD1, standing layup, motion contacts, approach rejection, blocked jumps, reset, duplicate commands, gesture selection and mistimed rebound at 20, 30 and 120 FPS | 384 | `Builds/BasketballFinishQA/Reviews/Offline-20261005-205814-781/`, `Offline-20261005-205814-782/`, `Offline-20261005-205814-786/` |
| Host dunk, guest layup, replicated pose, physical score and authority | 17 | `Builds/BasketballFinishQA/Reviews/Local-20261005-205919-063/` |
| Copied player and default launcher paths containing spaces, invoked from an unrelated working directory | 128 | `Builds/BasketballFinishQA/PortableEvidence/` |
| Basketball physics, power, scoring, net response and HUD | 81 | `Builds/BasketballPhysicsQA/Finish-Final/` |
| Offline shooting, passing, boundaries, travel and recovery | 98 | `Builds/BasketballGameplayQA/Offline-20261005-205920/` |
| Existing dribble, shot, pass and character motion regression at 20 FPS | 62 | `Builds/BasketballMotionQA/Finish-Final/` |
| Host/guest steals, shielding, loose-ball contests and charge cancellation | 38 | `Builds/BasketballStealQA/Local-20261005-210043-085/` |
| Looping charge, aiming, cancellation and court boundary regression | 46 | `Builds/BasketballChargeQA/Reviews/Finish-Final/` |
| Shared jump, recovery and two-player movement regression | 140 | `Builds/JumpQA/Run-20261005-210042/` |

The result registry is `Builds/BasketballFinishQA/checks.json`. The portability
fixture verifies script-relative defaults and the copied complete player; it
is not a fresh-clone Unity build.

Actual rendered gather, plant, takeoff, ball release and landing were inspected
at both hoops and with both hands. The **26.93-second**
`Builds/BasketballFinishQA/finish-review.mp4` shows four finish sequences at
normal and half speed; `finish-storyboard.jpg` and the final 30 FPS run retain
the frame evidence and contact telemetry. The final controls capture keeps
Cancel clear of the up/down drag route. Sampled held-ball architecture overlap
was zero, grounded recovery completed, and each successful action produced
exactly one physical release/attempt. The largest sampled palm error in the
30 FPS finish review was **2.1 cm** on the deliberately mistimed dunk. Fixed
capture rates and Windows renders do not establish phone performance or touch
ergonomics. No new defensive action is included.

Fresh AndroidSubmission completed at **21:03:16 Malaysia time**, passing the
strict budget gate and independent APK v2 signature verification:
**89,780,060 → 89,798,556 bytes (+18,496)**, with **10,201,444 bytes** of hard-limit
headroom. The 75 MB development target remains exceeded by **14,798,556 bytes**.
[Packing audit and hash](BUILD-SIZE.md#basketball-layups-and-dunks--5-october-2026).
The existing Android Debug signing identity is retained. No phone was
connected; Android appearance, touch, FPS, thermals and WAN timing remain
unverified.

Across Android packaging, **3,048 of 3,050** source/asset/config inputs remained
byte-identical; runtime scripts and scenes are unchanged. Format-aware
comparison confirms the regenerated Golf prefab's 14-object graph retains all
properties and reference topology. The other difference removes two SSAO
entries from the Android URP runtime registration list while preserving their
definitions. Evidence is in `windows-source.json`, `android-source.json` and
`generated-input-verification.json` under `Builds/BasketballFinishQA/`.

At **21:08 Malaysia time**, the complete tested player was promoted to
`Builds/WindowsFinal/`. All **315 files** matched the candidate by SHA-256
before updating `LATEST-BUILD.txt`. The source preserves the existing menu,
Golf, charging and steal work. The former delivery's timestamp was checked
before replacement; no newer concurrent delivery was overwritten.
`Builds/BasketballFinishQA/windows-promotion.json` records the promotion and
`candidate-player-hashes.json` retains the tested-file hashes. Rooms require
matching protocol **27** builds.

Reviewed superseded local output was archived with recovery manifests:

- `Legacy/20261005-210031-681-basketball-finishes-apk/` — previous APK, 1 file, 89.78 MB.
- `Legacy/20261005-210752-218-basketball-finishes-windows/` — previous complete Windows player, 315 files, 593.50 MB.
- `Legacy/20261005-211139-959-basketball-finishes-iterations/` — earlier captures/reviews and duplicate player fixtures, 3,796 files, 3,081.83 MB.

Current tests, media, source snapshots and size audits remain under ignored
`Builds/`. No active art, authoring master or Unity cache was archived. No
staging, commit, push or history rewrite was performed. Final readiness evidence
is retained in `Builds/BasketballFinishQA/task-ready.txt`.

## Basketball charging and energy boundary — 5 October 2026

The power needle now loops through repeated release windows, accelerating with
distance to a basket locked at charge start. Moving changes the integrated
phase continuously. Charging halves walking/sprint speed and gathers the ball
into a visible two-handed aiming pose, replicated to opponents. Drag-to-Cancel,
clicking Cancel or **X** lowers it back into the dribble without firing. The
translucent teal/gold court perimeter uses football's energy pattern with
expanding, fading player/ball contact ripples.
[Rules, calculations and controls](BASKETBALL-GAMEPLAY.md).

**575 assertions pass**, with no failing assertions in the retained suites:

| Suite | Passes | Evidence |
| --- | ---: | --- |
| Charging, half-speed movement, loops, target lock, cancellation, aim contacts and barrier | 46 | `Builds/BasketballChargeQA/Reviews/Final/` |
| Same new review through its default launcher in a path with spaces, from an unrelated working directory | 46 | `Builds/BasketballChargeQA/PortableReview/` |
| Physics, power, scoring, net response and final rendered HUD | 81 | `Builds/BasketballPhysicsQA/Charge-Delivery/` |
| Offline input, shots at both hoops, boundaries, travel and recovery | 98 | `Builds/BasketballGameplayQA/Offline-20261005-172820/` |
| Host/guest charging, visible remote aim, cancellation, shots, scoring and passes | 39 | `Builds/BasketballGameplayQA/Local-20261005-172921/` |
| Two-player stealing, secured charge, deflection and charge cancellation on strip | 38 | `Builds/BasketballStealQA/Local-20261005-172631-779/` |
| Existing motions and both character LODs at the 20 FPS review cap | 62 | `Builds/BasketballMotionQA/Charge-Final-20/` |
| Shared football boundary, kick feedback and cancellation regression | 25 | `Builds/FootballFeedbackQA/Run-20261005-172632/` |
| Shared jump and two-player movement regression | 140 | `Builds/JumpQA/Run-20261005-173045/` |

Measured charging walk speed is **4.00 → 2.00 m/s**. The moving aiming palm
stayed within the 4 cm contact tolerance (measured error rounded to **0.000 m**),
with a maximum sampled ball movement of **0.034 m/frame**. At 4, 12 and 22 m,
four successive green-window opportunities were observed without auto-fire.
Holding beyond five seconds remained active, and a later-cycle green release
scored. The authoritative release power matched the displayed value within
0.018, using host phase history rather than a client-supplied power.

Rendered close aim, player contact, ball ripple/fade, the whole court and the
power/cancel HUD were inspected. A premature capture initially sampled before
the procedural LateUpdate pose; the review now captures at end of frame and
asserts real palm contact. Visual review also found four malformed separator
characters in the existing scoreboard; they were corrected and the final
Windows player reran all 81 physics/HUD checks. The remaining gameplay source
is unchanged from the other passing suites. Football retained its translucent
look and unobstructed goal openings. These Windows observations do not certify
physical-phone visuals, touch or frame rate.

Fresh AndroidSubmission completed at **17:43:26 Malaysia time**, passing the
strict budget gate and independent APK v2 verification:
**89,762,748 → 89,780,060 bytes (+17,312)**, with **10,219,940 bytes** of hard-limit
headroom. The 75 MB working target remains exceeded by **14,780,060 bytes**.
[Packing audit and hash](BUILD-SIZE.md#basketball-charging-and-energy-boundary--5-october-2026).
Across packaging, 3,040 of 3,042 checked inputs remain identical. Canonical
comparison proves the regenerated Golf prefab has equivalent properties and
reference topology; the other change removes desktop-only URP registrations.

The complete **17:38:12 protocol 26 Windows player** is delivered under
`Builds/WindowsFinal/`, including the basketball launcher. After the user
authorized closing the old game, promotion completed at **19:53 Malaysia time**.
All **315 files** matched the tested candidate by SHA-256; the complete previous
314-file protocol 25 player was archived. Evidence is retained in
`Builds/BasketballChargeQA/windows-promotion.json`. Current source preserves
the menu, Golf and earlier basketball work. No staging, commit or push was
performed.

Reviewed obsolete output was moved to these archive batches:

- `Legacy/20261005-173200-338-basketball-charge-before-release/` — 11 files, 93.15 MB.
- `Legacy/20261005-174031-057-basketball-charge-diagnostic-apk/` — 21 files, 96.58 MB.
- `Legacy/20261005-174313-257-basketball-charge-iterations/` — 377 files, 629.94 MB.
- `Legacy/20261005-195311-178-basketball-charge-windows-delivery/` — 314 files, 593.45 MB.

Current captures, players, checks, source manifests and package audit remain
under `Builds/`. The earlier failed Windows archive attempt created only an
inventory; the successful delivery archive above supersedes that attempt.
No active art, authoring master or Unity cache was archived.
`Check-TaskReady.ps1 -RequireApk` passed, including the portable-path and Git LFS
checks. The publishable candidate set and Git diff were reviewed; nothing was
staged. The complete Windows delivery and its updated build note are verified.

## Refined basketball steal animation — 5 October 2026

The **15:25:12 Malaysia time**, protocol **25** Windows player passed
**366 assertions with zero failures**. The complete player is promoted to
`Builds/WindowsFinal/`; a hash comparison verified all **314 files**, including
the preserved SkySail launcher, before the delivery note was updated.
Results are indexed by `Builds/BasketballStealAnimationQA/check-counts.json`.

| Check | Passing assertions |
| --- | ---: |
| Challenge poses at fixed 20, 30, 60 and 120 FPS | 188 |
| Copied player/default paths, checkout path with spaces, unrelated working directory | 47 |
| Dribble, run, stop, shoot, pass, jump, LOD and release regression | 62 |
| Real-time held/released steal input and geometry/deflection rules | 31 |
| Two-player host/guest stealing and reactions | 38 |
| **Total** | **366** |

Pose coverage includes left/right hands, low/high contacts, mirrored victim
reactions, movement, repeated swipes, full recovery and the lower-detail model.
The largest sampled planted-support-foot drift is **1.6 mm**; bone lengths
remain fixed and arm reach correction stays within the configured bound.
The fixed-clock fixture schedules repeated poses on simulation time; the
separate input suite tests actual UI hold/release using its real-time clock.
Close and side frames were inspected. The **17.33-second**
`Builds/BasketballStealAnimationQA/steal-animation-preview.mp4` includes actual
two-player ball knockaways at normal speed and half speed, plus isolated pose
review. Captures are review evidence, not an FPS benchmark.

Current evidence: `Release-20/`, `Release-30/`, `Release-60/`, `Release-120/`,
`PortableEvidence/`, `promotion-hashes.json` and `windows-source.json` under
`Builds/BasketballStealAnimationQA/`; basketball motion regression under
`Builds/BasketballMotionQA/StealAnimationRelease-30/`; steal checks under
`Builds/BasketballStealQA/Offline-20261005-152726-201/` and
`Local-20261005-152713-115/`.

Fresh **15:31:44** AndroidSubmission passes the hard budget and independent v2
signature check: **89,758,540 → 89,762,748 bytes**, with **10,237,252 bytes** of
headroom. The 75 MB working target remains exceeded by **14,762,748 bytes**.
The Android build preserves **3,032 of 3,034** recorded inputs exactly;
format-aware text comparison confirms that the regenerated Golf prefab retains
its properties and references, and URP returns to the repository's Android
settings. Runtime scripts and scenes match the tested Windows source. No phone
was connected; physical-device appearance/FPS, thermals, touch feel and WAN
latency are unverified.

Reviewed local archives, with sizes, hashes and recovery paths:

- `Legacy/20261005-151802-842-basketball-steal-animation-iteration/`
- `Legacy/20261005-152321-289-basketball-steal-animation-refinement/`
- `Legacy/20261005-152652-548-basketball-steal-animation-snapshot/`
- `Legacy/20261005-152845-910-basketball-steal-animation-before-release/`
- `Legacy/20261005-153142-243-basketball-steal-animation-windows-delivery/`
- `Legacy/20261005-153421-641-basketball-steal-animation-evidence-cleanup/`

## Basketball steals and loose-ball contests — 5 October 2026

The final **protocol 25**, **14:03:09 Malaysia time** Windows player passed
**517 assertions with zero failures** and was promoted in full to
`Builds/WindowsFinal/`. Package hash comparison covers **314 files**. Current
results are indexed by `Builds/BasketballStealQA/check-counts.json`:

| Check | Passing assertions |
| --- | ---: |
| Steal controls and geometry, offline | 31 |
| Stealing, host / guest | 32 / 6 |
| Steal script in a path with spaces, unrelated working directory | 31 |
| Existing basketball gameplay, offline / host / guest | 98 / 22 / 14 |
| Basketball physics, scoring and net response | 81 |
| Basketball motion at simulated 20 FPS | 62 |
| Shared jump/momentum, offline / host / guest | 132 / 3 / 5 |

Steal checks exercise held-button repeats, repeated-request recovery, touch
ownership, release/drag/focus/disable cancellation, jumping and Free roam
exclusion, close/far/height/facing/shielded contacts, dribble exposure and secure
grips, walls, escape during windup, possession reset, host/guest owner input,
both replicated animations, real body collisions, loose-ball velocity/spin,
shot-charge cancellation, pickup grace and recovery by the original carrier.
Velocity checks cover both players' movement, opposite swipe hands, rising vs
falling dribbles, speed bounds and translated/rotated contact geometry.

Final two-player renders and telemetry are in
`Builds/BasketballStealQA/Local-20261005-140328-419/host/`.
Frames around contacts **92** and **107** show the swipe/reaction and loose-ball
flight. The maximum recorded arm target correction after clamping was **2 mm**;
this is an IK limit metric, not proof of exact palm/ball surface coincidence.
The final in-game HUD screenshot is
`Builds/BasketballGameplayQA/Offline-20261005-140328/offline/carry-controls.png`.
Close frames and control placement were visually inspected. Early checks found
a touch-test source mismatch and a jump request tested before simulation;
the final fixtures exercise real pointer ownership and the simulated jump.
Review also corrected outgoing player collisions and fitted palm targets.

The [fresh signed AndroidSubmission](BUILD-SIZE.md#basketball-steals-and-loose-ball-contests--5-october-2026) measures
**89,758,540 bytes**, up **14,428**, with **10,241,460 bytes** below the hard limit.
The 75 MB development target remains exceeded. Android packaging preserved
**3,028 of 3,030** checked inputs byte-for-byte; the two exceptions are reviewed
platform URP registrations and equivalent generated Golf prefab IDs. C# source
and scenes match the final Windows validation snapshot. Source manifests,
canonical verification, the APK signature and both packing audits are retained.

Superseded APK/audit and complete Windows deliveries were archived with hashes
and recovery paths in `Legacy/20261005-140452-410-basketball-steal-before-release/`
and `Legacy/20261005-140636-398-basketball-steal-windows-delivery/`.
Intermediate tests/logs and the portable fixture are in
`Legacy/20261005-140731-651-basketball-steal-iterations/`; its passing results
remain in `Builds/BasketballStealQA/PortablePathEvidence/`.
The hash-matched duplicate candidate is retained in
`Legacy/20261005-141102-934-basketball-steal-tested-candidate/`.
`Check-TaskReady.ps1 -RequireApk` passes, including the portable-text, Unity
metadata, LFS, whitespace and candidate-file checks; its output is retained in
`Builds/BasketballStealQA/task-ready.txt`. No binary art or active asset dependency was edited for the feature. Existing
nonfunctional art-provenance exceptions are unchanged. Git publication remains
with the user. Physical-phone touch, Android appearance/FPS/thermals and WAN
latency remain unverified; local Windows captures do not qualify them.

## Storybook Cove menu and teammate integration — 5 October 2026

The first selected direction is now the native game menu: illustrated island
selection, Create/Join, live lobby, real match conditions, football/basketball
venue editing, settings and all four sport help pages. All navigation buttons
have hold compression, release feedback, cancellation, disabled/focus states and
one-shot activation. Sliders animate their thumb without shifting their input
track. Pages crossfade/slide with input gated, and gameplay arrival fades in.
Reduced motion keeps colour/focus feedback while suppressing movement.
[Behaviour and source guide](STORYBOOK-MENU.md).

**1,658 feature assertions pass** across the following retained Windows suites:

| Suite | Passing assertions | Evidence |
| --- | ---: | --- |
| Final offline menu, pointer/keyboard states, inputs, layout, settings, preview/save/discard | 694 | `Builds/StorybookMenuQA/FinalReview/offline/` |
| Final host/guest lobby, conditions, permissions, ready resets, departure/return | 138 (70/68) | `Builds/StorybookMenuQA/FinalReview/host/`, `client/` |
| Football gameplay, clock, physics and network regressions | 532 | `Builds/FootballMatchQA/Run-20261005-125217/` |
| Basketball physics, scoring, charging and net response | 81 | `Builds/BasketballPhysicsQA/StorybookMerge/` |
| Basketball host/guest input, possession, scoring, free roam and permissions | 38 | `Builds/BasketballGameplayQA/Local-20261005-130919/` |
| Golf offline and host/guest match regression | 175 | `Builds/GolfMiniGameQA/Run-20261005-130812/` |

The final menu player includes the last visual, slider, focus and clipboard
refinements. Earlier sport suites cover unchanged gameplay sources; the menu
refinements did not alter those mechanics. Combined network layout is **protocol
24**. The pulled Golf/cart work and preserved local basketball work both pass.
No conflict markers or unmerged index entries remain; the pre-merge recovery
stash and hashed files are retained. Nothing was staged, committed or pushed.

Two fresh anonymous players also used the **real Unity cloud session/Relay**
service to create/join one private room, become ready, depart and return together,
then leave. Evidence: `Builds/StorybookMenuQA/CloudReview/` and `cloud-final.txt`.
Both players shared one internet connection; this is not different-network WAN
or Android-device qualification. Create/join completed without connection errors.
On host deletion, the guest logged a handled `lobby not found` warning during
cleanup; both players disconnected and returned to offline state.

All final copy/paste and typing checks pass. Earlier Windows clipboard writes
intermittently returned empty, reproduced separately by PowerShell outside Unity;
the final OS roundtrip succeeded. Copy retries briefly and announces success only
after readback; failure leaves the displayed room code and typing fallback usable.
Earlier failure evidence is retained in `ClipboardInvestigation/` without recording
user clipboard contents. Android clipboard/keyboard behaviour still needs a phone.

Rendered captures were visually reviewed for home, lobby, conditions, actual
football screen/basketball court previews and settings. Bounds, button raycasts,
label width and text height pass. Actual Windows sizes checked: **1600×900,
1280×800 and 1920×1080**. The requested 2340×1080 window was clamped to 1920×1080
by this display, so ultra-wide support is not claimed as tested. Two-player
network review passes another **138 assertions** from a copied default player tree
containing spaces, launched from an unrelated working directory. The font
generator produces identical hashes from that portable fixture. Evidence:
`PortableNetwork/`, `portable-menu.txt` and `portable-font-check.json`.

Fresh AndroidSubmission and APK v2 signature pass: **89,744,112 bytes**, with
**10,255,888 bytes** hard-limit headroom; the 75 MB development target is unmet.
Source manifests cover 3,022 files, with only equivalent generated Golf IDs and
platform URP registration changes. [Size and delivery](BUILD-SIZE.md#storybook-cove-menu-and-teammate-integration--5-october-2026)
and [packing audit](APK-SIZE-AUDIT.md#storybook-cove-menu-and-teammate-integration--5-october-2026).
No physical Android device was connected. Phone touch feel, appearance, sustained
FPS, thermals and WAN timing remain unverified.

The complete tested player, including all runtime DLLs/data and existing launchers,
was promoted to **`Builds/WindowsFinal/`**. Updated `LATEST-BUILD.txt`, file-hash
promotion evidence, current screenshots, test logs and audit remain under `Builds/`.
Reviewed obsolete outputs were archived with recovery inventories; see
`archive-baseline.txt`, `archive-windows.txt`, `archive-iterations.txt` and `archive-candidate.txt` in
`Builds/StorybookMenuQA/`. No active art or master was removed.

## Basketball pace and player boundaries — 5 October 2026

Basketball charging and shot preparation now take **30% less time**: full charge **1.20 → 0.84 s**, ideal release **0.78 → 0.546 s**, shot animation release **0.44 → 0.308 s**, and full shot action **0.98 → 0.686 s**. Queued dribble preparation and authored shot poses use the same time scale. Pass timing and the green power band are retained. [Controls and implementation](BASKETBALL-GAMEPLAY.md).

An invisible movement boundary keeps the complete player capsule inside the painted court during basketball play. Input and velocity projection preserve smooth sliding along sidelines, including diagonal movement. The ball can leave the court and remains physical. A loose ball stranded beyond the painted boundary near the floor returns after **0.75 s**, while airborne apron excursions retain the wider recovery envelope. **Free roam** deliberately releases the boundary and ball possession for station travel; **Play basketball** restores play. Host authority, replicated free-roam state, arrivals and session resets are covered.

**479 assertions passed** with no failing assertions:

| Suite | Passing assertions | Retained evidence |
| --- | ---: | --- |
| Offline input, charging, shots, boundaries, sliding, free roam and rebound recovery | 98 | `Builds/BasketballPaceQA/QA/Offline/` |
| Host/guest possession, charge, scoring, net events and court/free-roam state | 36 (22 host, 14 guest) | `Builds/BasketballPaceQA/QA/Local/` |
| Motion timing, dribble, hand contact and both character LODs at 30/20 FPS caps | 124 (62 each) | `Builds/BasketballPaceQA/QA/Motion-30/`, `QA/Motion-20/` |
| Shot physics, faster power timing, score/miss decisions, nets and portable launch | 81 | `Builds/BasketballPaceQA/QA/Physics-Portable/` |
| Shared jumping and two-player regression | 140 | `Builds/BasketballPaceQA/QA/Jump/` |

The rendered motion review checked gather/release hand contact, first-/third-person views, green meter feedback, sidelines and deliberate free-roam exit. Maximum measured action hand reach error was **25 mm**. Observed frame-sampled release/recovery times stayed within the expected render/fixed-step allowance around the new authored times. Net displacements in the physics samples reached approximately **6.3–7.4 cm**, with fixed upper attachments and a return to rest. Deliberately weak/strong shots missed; well-timed two/three-pointers scored. The portable suite ran from a copied tree containing spaces, launched from an unrelated working directory.

Tests exposed two issues that were fixed before release: repeated loss of diagonal sliding speed when constraining velocity alone, and a loose ball resting on an unreachable apron inside the old wider recovery envelope. The final offline/network rerun includes both fixes and additional airborne/apron recovery cases. The earlier motion/physics/jump checks cover unchanged timing and shared mechanics; the final Windows player contains all fixes.

During work the primary checkout received `af02ea7` and concurrent Storybook menu changes. Preserved basketball work was applied to that integration and tested in an isolated checkout, excluding the unfinished menu. The matched tested players in **`Builds/BasketballPaceQA/Release/` use protocol 23**. The main source uses **protocol 24** for its later menu network field. These results do not certify the combined unfinished menu build, and an older primary APK is not evidence for the new source.

The prescribed AndroidSubmission build passed at **12:56:34 Malaysia time**: **86,791,760 → 89,260,934 bytes**, leaving **10,739,066 bytes** below the strict limit and exceeding the 75 MB working target by **14,260,934 bytes**. APK v2 signature verification passed. Of 277 checked inputs, 276 remained unchanged during the build; the Golf ball prefab regenerated equivalent local IDs/document order only, confirmed by canonical property/reference comparison. [Detailed size audit](APK-SIZE-AUDIT.md#basketball-pace-and-player-boundaries--5-october-2026). No physical phone was connected; Android touch, appearance, FPS, thermals and WAN timing remain unverified.

Reviewed temporary logs, superseded test runs and the completed portable fixture were archived to **`Legacy/20261005-125501-976-basketball-pace/`** (**368 files, 607.76 MB**) with hashes and recovery paths. Current release/evidence remain under `Builds/BasketballPaceQA/`. No active art or authoring master was archived. The isolated build passed `Check-TaskReady.ps1 -RequireApk`; the primary checkout also passed repository hygiene, with its APK freshness separately qualified above. Git staging, commit and push remain with the user.

## Basketball physics and net response — 4 October 2026

The basketball now has a 30 cm visual/physical diameter (25% larger), fitted palm contacts, a hold/release power meter, deterministic ballistic launches, shared 2/3-point practice scoring, missed-shot outcomes and responsive nets. [Implementation, equations and controls](BASKETBALL-GAMEPLAY.md).

**396 Windows assertions passed** across these retained suites:

| Suite | Passing assertions | Local evidence |
| --- | ---: | --- |
| Shot physics, power, scoring, net response and portable execution | 79 | `Builds/BasketballPhysicsQA/FinalReview/` |
| Offline shooting, rebound pickup, collision, recovery and streaming | 67 | `Builds/BasketballGameplayQA/Offline-20261004-233337/` |
| Host/guest charge edges, scoring, net events, passes and lifecycle | 32 (20 host, 12 guest) | `Builds/BasketballGameplayQA/Local-20261004-233711/` |
| Dribble, enlarged-ball palm contact, gather, release and both character LODs | 56 | `Builds/BasketballMotionQA/Physics-Initial/` |
| Ball drop, spin, both rims and 35 m/s floor/backboard contacts | 22 | `Builds/BasketballModel/20261001/Offline-233712/` |
| Shared jump/input and two-player regression | 140 | `Builds/JumpQA/Run-20261004-234309/` |

The focused suite exercises complete downward passage, upward and side rejection, fast crossings, incomplete rim exits, three-point/corner lines, foreign-pointer release rejection, touch cancellation, clean timed two/three-pointers at both hoops, deliberately short/long shots, timeout, one outcome per attempt and session clearing. Scored nets reached approximately **5.6–6.3 cm** displacement in the measured samples; their upper attachments stayed fixed, and the mesh returned to rest. Outside-net touch moved the net without awarding a basket. Host and guest agreed on two three-pointers and received the authoritative net events. The regression suite also retains real descending rim crossings at **2, 5, 10, 18 and 24 m**.

Rendered Windows views of the larger ball beside the character, dribble/contact poses, first-/third-person and elevated cameras, the green power meter, both scored nets and exterior net touch were inspected. The rig retained bounded hand reach and contact errors; the two-metre drop and 35 m/s floor/backboard probes remained above the floor. These observations do not certify physical-phone visuals, touch or frame rate. The existing scoreboard architecture is decorative; the new HUD tracks shared practice, without basketball teams, a match clock or foul rules. Net response is a bounded procedural approximation with soft resistance, not full cloth simulation.

An early test found that the Unity hoop's inward court axis is positive local Z; the three-point calculation was corrected. Synchronous screenshot readback inside a timed hold then skewed the test's release, so UI capture was separated from the actual timing assertions. The corrected suite passed from a copied player/script tree named `Portable Basketball Physics`, launched from an unrelated temporary working directory. Its evidence was preserved in `FinalReview/` before archiving the fixture. All **137 checked documentation links** resolved. New scripts derive their root from their own location. Source FBX, Blender and texture bytes were not changed; their previously recorded nonfunctional provenance exceptions remain as documented in [the model report](BASKETBALL-BALL.md).

The prescribed AndroidSubmission build passed at **23:45 Malaysia time**: **86,774,768 → 86,791,760 bytes**, with **13,208,240 bytes** of hard-limit headroom. The 75 MB development target is still exceeded by **11,791,760 bytes**. APK v2 verification passed; 238 checked script/shader/prefab/scene inputs did not change during that build. [Size audit and exact artifact hash](BUILD-SIZE.md#basketball-physics-and-net-response--4-october-2026). Windows player: `Builds/WindowsFinal/`; Android player: `Builds/Android/WhatTheFish-release.apk`. Protocol **19** requires matching builds. No Android device was attached; phone performance and WAN hold timing remain unverified.

Reviewed failed trials, superseded build logs and the completed portable fixture were moved with the archive tool to **`Legacy/20261004-234732-924-basketball-physics/`** (339 files, 594.35 MB), with hashes and recovery paths. No active art or authoring master was archived. Existing menu-design work was preserved. `Check-TaskReady.ps1 -RequireApk` passed, including publishable path scanning and Git/LFS hygiene; evidence is in `Builds/BasketballPhysicsQA/task-ready.txt`. These checks are read-only; Git staging, commit and push remain with the user.

## GitHub Golf and football integration — 5 October 2026

The combined Windows player built at **00:02:12 Malaysia time** passes
**1,106 assertions with no failures or runtime exceptions**:

- **252 Golf**: 76 rendered ball/tee/physics, 135 rendered offline and 20 host
  plus 21 side-approach client. Evidence:
  `Builds/GolfBallPhysicsQA/Run-20261005-000517/` and
  `Builds/GolfMiniGameQA/Run-20261005-000518/`.
- **532 football gameplay/physics/network**: 259 offline, 16 match host,
  15 match client, 199 physics and 26 ball host plus 17 ball client. Evidence:
  `Builds/FootballMatchQA/Run-20261005-000742/`.
- **292 football motion** at the 60 timeline rate, including both LODs,
  saved takes, bounded rigs, stale snapshots, reset and allocation checks.
  Evidence: `Builds/FootballMotionQA/Run-20261005-000743-227/`.
- **30 rendered lagoon**: shared three-size fish, transparent water, all three
  gameplay camera modes, animated water/caustics and reload/cutout cleanup.
  Evidence: `Builds/GitSyncQA/Lagoon/`.

Golf's actual full-charge contact remains **18.00000 m/s** horizontally and
travels **12.27873 m** during the 0.8-second observation from release. Slow-roll
settling, slope rest/wake, five-hole progression and selected-ball strokes
remain. Room protocol **22** covers both Golf and football replicated layouts
in the runtime service and Bootstrap scene. The eight reviewed conflicts keep
both features and release histories; normalized upstream Golf scene content
matches its baseline, so the complete local five-hole scene is retained.
Conflict variants, decisions and comparisons are in `Builds/GitSyncQA/`.

All **113 runtime source files** match the tested isolated build. Reviewed
build differences are its editor-only build wrapper, generated desktop
pipeline registration, equivalent **14-object** Ball prefab IDs and a legacy
material colour difference below **1e-6**. The complete **313-file** player is
promoted to `Builds/WindowsFinal/` and retained with the fresh APK/audit in
`Builds/GitSyncQA/Release/`; package hashes are recorded in the task evidence.
Fresh primary AndroidSubmission at **00:07:25** measures **89,230,290 bytes**,
with strict gate, APK v2 signature and packing audit passing. See
[size and archives](BUILD-SIZE.md#github-golf-and-football-integration--5-october-2026).
Physical-phone rendering, touch, FPS and WAN remain unverified. Earlier
documented fishing-route issues were not part of this integration's changes.

## Golf swing speed 18 m/s — 4 October 2026

**250 assertions pass** on the Windows player built at **23:20:49 Malaysia
time**: **76 rendered ball/tee/physics**, **134 rendered Golf/club offline**,
**20 host** and **20 client**. Evidence:
`Builds/GolfEighteenSpeedQA/PhysicsFinal-20261004-232216/`,
`MatchFinal-20261004-232217/` and `NetworkFinal-20261004-232217/`.

The real qualified full-charge swing measures its first horizontal launch
velocity at **18.00000 m/s**. The ball travels **12.27873 m** during the
0.8-second observation from release, exceeding the former five-metre cap
without an extra stroke or reset. A later **18.19571 m/s** horizontal peak is
normal landing energy transfer and is measured separately from launch.
Flying and high-speed rolling have no global speed clamp. Existing slow-roll
resistance, slope settling, stop delay, resting, strike/force/collision wake,
wooden tees, all five cups, restart and host authority checks pass.

All **313 Windows player files** are promoted to `Builds/WindowsFinal/` and
retained in `Builds/GolfEighteenSpeedQA/Release/Windows/`. The final manager
includes the concurrent Unity-null trigger initialization correction.
Golf runtime/configuration, meadow source, scene and environment prefab match
the tested isolated source. Reviewed isolated build differences are the
editor-only output path, generated pipeline registrations and equivalent
prefab IDs. The later editor-start review helper does not ship in the player.
Source comparisons and package hashes: `Builds/GolfEighteenSpeedQA/`.

Fresh primary AndroidSubmission completed at **23:27:16 Malaysia time**:
**89,024,294 bytes**, strict gate and independent APK v2 signature pass.
The initial-launch probe is excluded from release APK compilation.
Physical-phone appearance, touch, FPS and WAN remain unverified. See
[size, headroom and archives](BUILD-SIZE.md#golf-swing-speed-18-ms--4-october-2026).

## Golf meadow restored — 4 October 2026

**354 checks** pass on the restored Windows player built at **22:43:13 Malaysia
time**: 78 saved-course editor, 142 rendered island, 134 rendered Golf match.
Five numbered flags, recessed cup floors, grounded tees, 48 shoreline directions,
fairway/bunkers, camera/environment switching and five-hole progression remain.
Independent comparisons verify **294 original mesh references** and the exact
**137,724-blade** pre-course runtime meadow; **2,289 blades** return to the
former large clearings. Original terrain-map/configuration hashes are unchanged.

Evidence: `Builds/GolfGrassQA/`, `Builds/GolfG2QA-20261004-224336/` (five hole
views/overview), `Builds/GolfMiniGameQA/Run-20261004-224347/`.
Exact complete player/APK/audit: `Builds/GolfGrassQA/Release/`; promoted player:
`Builds/WindowsFinal/`. Fresh shared primary AndroidSubmission at **22:44:23**:
**89,024,882 bytes**, gate/v2 signature/packing checks pass. Reviewed isolated
differences are editor-only clean packing, URP registrations and equivalent
Ball prefab IDs. Later **22:46 GolfBall settled-stroke guard changes** are outside
this captured release. Phone rendering/touch/FPS and WAN remain unverified.
Initial disk-full output, old delivery and obsolete grass are archived; see
[release records](BUILD-SIZE.md#golf-meadow-restored--4-october-2026).

## Golf cart forward-facing feet — 4 October 2026

Seated feet now align the imported toe-tip segments with the cart front, rather
than inheriting the standing foot angle or using the sloping ankle-to-toe-base
segment. Both knees stay straight and the seated hip/hand targets are retained.
Native entry, turning and both character LOD captures were reviewed. **439 checks
pass**: 221 cart offline/host/client, 134 Golf match and 84 ball physics checks.
Both loopback peers report forward-facing toes and straight knees; walking and
jump recover after leaving the cart.

The complete **313-file** Windows player built at **22:23:26 Malaysia time** is
initially promoted to `Builds/WindowsFinal/` and retained in `Builds/GolfCartFeetQA/Release/Windows/`. A later complete shared player, built **22:43:13**, includes the same feet source and concurrent grass work; it is preserved and passes another **221 cart assertions** at `Builds/GolfCartQA/Run-20261004-225224/`. Native entry/turn/LOD foot captures were reviewed again, and both loopback peers report zero-degree forward toe alignment. The earlier 439-check snapshot remains separate.
Evidence: `Builds/GolfCartQA/Run-20261004-222342/`,
`Builds/GolfMiniGameQA/Run-20261004-222431/` and
`Builds/GolfBallPhysicsQA/Run-20261004-222345/`.
The 2,974-input source snapshot includes the contemporaneous five-metre Golf
swing settings. Later unrelated GolfCourseBuilder and MobileIslandGrass changes
are outside this snapshot. Generated 14-object Ball prefab IDs are equivalent;
legacy material `_Color` changes by less than 0.000001 while `_BaseColor` matches.
The isolated editor uses a clean build cache. No art asset is added by this fix.

Fresh task AndroidSubmission built **22:40:48 Malaysia time** after recovering
from a disk-space failure. APK **89,021,758 → 89,025,074 bytes (+3,316)**;
strict headroom **10,974,926 bytes**, development-target excess **14,025,074 bytes**.
APK v2 signature and fresh packing analysis pass. SHA-256:
**4A5719AFBB1A825E1A7C4AC92CF6B00BA8CF70CF29B05D34538F65AA673AA91E**. Serialized asset delta is 4 bytes (Golf configuration);
all other per-asset estimates match. No art is added by this pose correction.
The exact APK/audit is retained in `Builds/GolfCartFeetQA/Release/`.

A newer primary-workspace APK, built **22:44:23**, is preserved at
`Builds/Android/WhatTheFish-release.apk`: **89,024,882 bytes**,
strict headroom **10,975,118 bytes**, SHA-256
**20D100869CBFAA24F9E74F7F5CE1D16BCA0075A8B8F3FDC4F935A301272706E4**. Its v2 signature and generated native code confirm the same
toe-tip correction. Later unrelated grass generator/scene/prefab changes and
archived grass assets are outside this task's native gameplay snapshot.
Phone visual quality/FPS remains untested. The predecessor APK/audit was
archived by the concurrent task in
`Legacy/20261004-224157-371-golf-five-metre-old-delivery/`; it was not overwritten
with the older isolated task build.

Reviewed first-attempt output is archived in
`Legacy/20261004-221446-653-golf-cart-feet-trial/`; the superseded shared Windows
player is archived in `Legacy/20261004-222648-806-golf-cart-feet-windows/`,
with the manifest recorded in `Builds/GolfCartFeetQA/archive-windows.txt`. Ignored historical candidates were
compressed without changing their paths or bytes. Git publishing remains with
the user.


## Reliable Golf Swing proximity — 4 October 2026

The strict arm-extension check was the cause of the gray Swing button. On the
actual rig, the original predicate accepted **0/12 angles at 0.05, 0.25 and
0.50 metres**, and only **1/12 at 0.75 and 1.0 metres**. Eligibility now uses a
**1.1-metre horizontal radius**, supported height, grounded state and terrain
visibility. Swing addresses the selected ball; camera heading still sets shot
direction. Charge selection stays locked. Strength, timing and physics remain
unchanged.

**560 assertions passed on matching gameplay and assets:**

- **104 rendered Swing checks**, including 72 distance/angle combinations at
  0.05–1.08 metres, actual UI raycast, touch hold/release, body turn with planted
  feet, selected-ball lock, lost-target cancellation, wall/transit/jump rejection
  and five very-close/side contact poses with both palms attached. Evidence:
  `Builds/GolfSwingQA/Run-20261004-213536/`.
- **134 rendered Golf offline checks**, preserving rules, strokes, cups,
  grip/idle/walk/run, first person, cancellation, cart stow/re-equip, restart
  and island switching. Evidence:
  `Builds/GolfMiniGameQA/Run-20261004-213536/`.
- **41 side-approach network checks**: 20 host and 21 rendered client, including
  automatic stance after transform interpolation, reliable charge/swing,
  other-owner ball striking, actual authoritative movement, owner progression,
  wrong-cup recovery and the real 30-second countdown/DNF. Measured client
  stance error is **0.000 degrees**. Evidence:
  `Builds/GolfMiniGameQA/Run-20261004-210915/`.
- **72 rendered ball/tee/physics checks** and **209 cart checks** (163 rendered
  offline, 23 host, 23 client), with identical gameplay/assets to the delivered
  player. Evidence: `Builds/GolfBallPhysicsQA/Run-20261004-211139/` and
  `Builds/GolfCartQA/Run-20261004-211139/`.

Two preceding gait-capture runs missed a low running-hand frame while the
fixture travelled on the real terrain. Resetting each gait to the level
fixture makes the intended animation check pass; no runtime animation changes
were needed. Failure reports remain in `Builds/GolfMiniGameQA/`. The initial
packaging attempt was interrupted under disk pressure; its incomplete player
is archived, and the delivered player is a successful complete rebuild.

The final complete Windows player is built at **21:32:44 Malaysia time** and
promoted to `Builds/WindowsFinal/`. All **313 Windows files**, the fresh APK
and **11 audit files** match `Builds/GolfSwingQA/Release/`. AndroidSubmission
built at **21:16:14**, **89,021,758 bytes**, with strict size and independent
signature checks passing. The **2,974-input** capture matches production
gameplay and authored scene/model inputs. The later gait-fixture change lives
entirely in the development-only probe, excluded from release APK compilation;
canonical Ball prefab/material comparisons and build-only differences are
recorded in `Builds/GolfSwingQA/`.

Run [Test-GolfSwing.ps1](../Tools/Build/Test-GolfSwing.ps1) with `-Render` for
the interaction matrix; [Test-GolfMatch.ps1](../Tools/Build/Test-GolfMatch.ps1)
supports `-OfflineOnly -Render` and `-NetworkOnly -SideSwing -Render` for these
cases. Physical-phone touch/rendering/FPS and WAN remain unverified. Git was
not staged, committed or pushed. See
[size and recovery batches](BUILD-SIZE.md#reliable-golf-swing-proximity--4-october-2026).


## Golf wooden opening tee — 4 October 2026

The complete **20:57:01 Malaysia time** Windows player passes **246 checks**:
72 rendered ball/tee/physics/camera/cup/restart checks at
`Builds/GolfBallPhysicsQA/Run-20261004-210427/`, and 174 Golf/club/room checks at
`Builds/GolfMiniGameQA/Run-20261004-210503/` (134 rendered offline, 20 host,
20 client). This exact player is promoted to `Builds/WindowsFinal/` and retained
in `Builds/GolfWoodTeeQA/Player/`. Package hashes and source snapshots are under
`Builds/GolfWoodTeeQA/`. The identical cart/seat gameplay also retains the
concurrent **209-check** cart evidence at `Builds/GolfCartQA/Run-20261004-204749/`.

Native checks cover a separate fixed concave wooden support, shared small
grain/material, existing dynamic ball stability, real low-charge Swing and
one stroke, support remaining fixed, outside-course and wrong-cup recovery,
all five cups, unchanged later-hole placement, clean restart, and host/client
supports for both owners. Ground-query and collision checks keep characters
and carts isolated from this tiny support. Actual close, normal camera and
cup-only captures are retained; the reference ball is never imported or used
as the live ball. Existing camera, three-times ball radius, flight, slopes,
settling, force wakeup, owner-based completion and 30-second countdown checks
also pass.

The first AndroidSubmission attempt hit native memory exhaustion. The fresh
concurrent **20:56:07** AndroidSubmission APK already contains the exact tee,
match, ball, cart and other gameplay sources and assets, so it is reused after
comparing all **2,974** captured inputs and independently verifying its APK v2
signature and detailed packing. The six reviewed differences are generated
URP registrations, an editor-only clean packing option, equivalent Ball
prefab child IDs, editor GolfSwingReview and development-only GolfMatchProbe/
GolfSwingProbe changes. Release probe logic is compiled out. APK bytes are
**89,022,058**, with **10,977,942** bytes of strict-limit headroom; the 75 MB
development target remains exceeded. Exact APK/audit:
`Builds/GolfWoodTeeQA/Release/`. This is fresh matching-source evidence; the
failed primary build and older APK are not counted as successful validation.
Physical-phone touch/rendering/FPS and WAN remain unverified.

The previous shared player is in
`Legacy/20261004-211728-900-golf-wood-tee-delivery/`. Reviewed trials, source
backups and failed compile/stance/OOM diagnostics are listed by the task's
archive records. No staging, commit, push or history changes were performed.

After the final player was captured and tested, independent primary
`GolfClubMotion` work and a development-only `GolfMatchProbe` edit continued.
Those later source edits are preserved and outside this player/APK snapshot;
this record does not validate or deliver that ongoing work. The tee and its
physics/placement/assets remain identical to the tested snapshot.

## Golf cart straight-knee driver — 4 October 2026

The current Windows snapshot passes **455 assertions**: **209 cart** checks
(163 rendered offline, 23 host, 23 client), **72** rendered ball/physics/wooden
tee checks, and **174** Golf/club/room checks (134 rendered offline, 20 host,
20 client). Evidence: `Builds/GolfCartQA/Run-20261004-204749/`,
`Builds/GolfBallPhysicsQA/Run-20261004-204749/`, and
`Builds/GolfMiniGameQA/Run-20261004-204749/`.

The former bent-leg ankle IK target is replaced only in the seated leg overlay.
Both actual bone segments align forward and slightly upward without changing
their lengths. Hip and wheel-hand targets remain. Measured knee angles are
**179.9802–180°**, ankle positions about **0.30 m forward / 0.02 m above** the
hip marker, and sampled baked shoe-region vertices at least **0.232 m forward**.
The complete avatar top stays at or below **2.27233 m**, leaving **5.77 cm**
beneath the **2.33 m** roof underside. Native close views of both character LODs
show straight legs and shoes ahead of/above the seat cushion. Entry, driving
turns and the remote guest pose pass on both peers. Exit restores walking,
jump and walking cameras. Speed, steering, rolling, collisions, ownership,
recall, terrain traversal and island travel checks also pass.

The first straight-knee trial retained too much downward leg angle for this
short rig. Its failed clearance checks and images are archived in
`Legacy/20261004-203548-973-golf-cart-seat-trial/`. An overlapping-build texture
allocation exhausted system memory; diagnostics are in
`Legacy/20261004-204222-803-golf-cart-seat-oom/`. The isolated retry completed
after the other batch editors finished. No gameplay settings were changed
to work around that build failure.

Fresh release AndroidSubmission, strict APK gate and independent v2 signature
pass at **89,022,058 bytes**. The shared full Windows player, APK and audit
match the exact retained packages under `Builds/GolfCartSeatQA/Release/`.
The **2,974-input** snapshot includes the current wooden tee and settling/contact
runtime. Later development-only probe and editor review changes are outside
this tested snapshot and compiled out of release; gameplay sources match.
Generated URP registrations and equivalent Ball prefab local IDs are reviewed,
with the **14-object graph** matching. Raw source comparisons and package hashes
are under `Builds/GolfCartSeatQA/`. [Measured delivery and archives](BUILD-SIZE.md#golf-cart-straight-knee-driver--4-october-2026).
Phone touch/rendering/FPS and WAN remain unverified.

TaskReady with `-RequireApk` passes using a read-only copy of the Git index.
The pre-existing shared `.git/index.lock` is preserved; the real index hash
is unchanged. No staging, lock removal, commit or push was performed.


## Golf slope settling and resting — 4 October 2026

Final Windows sources pass **233 assertions**: **61** rendered ball-physics
checks and **172** Golf/club/room checks (134 offline, 19 host, 19 client).
Evidence: `Builds/GolfBallPhysicsQA/Run-20261004-203111/` and
`Builds/GolfMiniGameQA/Run-20261004-203306/`. The complete tested player is
retained under `Builds/GolfSettlingQA/Release/Windows/`.

On temporary test-only 5°/15° plates, a 1.8 m/s shot enters slow rolling at
**1.7793 / 1.9049 m/s**, decelerates without downhill re-acceleration, and rests
after **0.520 s** continuously below the configured 0.25 m/s threshold (0.5 s
plus fixed-step rounding). Position remains within **1 mm** over the subsequent
2 s, with zero linear/angular velocity. The resting body remains dynamic and
collidable, with its gravity disabled. Fresh strikes immediately restore gravity
and clear timers; 5 m/s downhill shots remain above 4.5 m/s after 0.5 s.

Coverage includes gentle new ball contact resetting a nearly complete stop timer,
ball-to-ball wakeup, an impulse just before the old stop timer
would expire, a tiny direct Rigidbody impulse, loss of support, and a fresh strike
issued **after settling in the same FixedUpdate**. Flight is compared against a
separate Rigidbody with the original 0.05 linear damping: velocity differs by less
than 0.002 m/s, including that same-step strike. Settling cancels tangential gravity
in velocity rather than queuing a force that could survive a later Strike.
A level 3 m/s roll reaches **2.412 m/s at 0.6 s** and stops normally. Qualified
8%/80% Swing peaks remain **4.174 / 31.271 m/s**, with charged height gain
**1.994 m**. All five existing cups, stroke counts, next tees, results and restart
checks pass. Body/cart collision exclusions and existing Host/Client snapshot
behavior are preserved.

All **2,430 existing scene/art/terrain/environment-prefab inputs** compared to
the task capture are unchanged. Production changes are limited to GolfBall,
its new persistent physics settings script/asset, ball verification and docs.
Concurrent wooden-tee work is preserved in the main workspace but excluded from
this isolated delivery; its dependent development-probe block is excluded only
in the isolated snapshot; its tee-layer addition to the ground query is likewise
excluded there. One concurrent rendered run missed the transient low-hand capture
window; the unchanged final player passed the separate retry, recorded above. No map, slope, hole, player movement, leaderboard,
countdown or other-sport behavior is changed by this task.

[Inspector defaults and manual test steps](GOLF-MINIGAME.md) are maintained with
the Golf guide. [APK measurement and delivery records](BUILD-SIZE.md#golf-slope-settling-and-resting--4-october-2026)
record the exact release scope. Physical-phone rendering/touch/FPS and WAN remain
unverified; Windows results are not device-performance measurements.

## Golf 3x ball and optional Aim — 4 October 2026

Final Windows build: **19:50:57 Malaysia time**. **41 native checks** pass in
`Builds/GolfBallPhysicsQA/Run-20261004-195210/`: both existing ball LODs and the
single collider use **129 mm diameter / 64.5 mm radius**; match startup remains
centred; the Chinese Aim/Cancel button enters the previous shoulder framing
and returns; first-person/elevated modes and round resets clear Aim. Real tee
captures include every UI overlay and prove visible ball pixels while aiming,
without moving the ball or forcing its initial LOD.

Physics remains measured: bodies and driven carts displace the resting ball
**0.00000 m**. Level roll slows **3.000 → 2.318 m/s at 0.6 s**, then stops.
Sleeping balls roll downhill on 5°/15° slopes and reach **0.592/1.757 m/s at
one second**. Qualified 8%/80% swings peak at **4.174/31.271 m/s**; the charged
ball rises **1.994 m**. The larger sphere enters each of the five actual cups,
preserves the stroke count, returns supported at each next tee, completes the
match and restarts with zero strokes and Aim off.

The same final player passes **172 Golf/club/room assertions**: **134 rendered
offline, 19 host and 19 client**, in
`Builds/GolfMiniGameQA/Run-20261004-195211/`. This includes actual live
idle/walk/run grip views, the new radius in swing fixtures, cup order, ownership,
recovery, touch input, the real 30-second finish deadline, DNF, restart,
cameras and sport cleanup. The **203 cart regressions** in
`Builds/GolfCartQA/Run-20261004-191514/` pass on identical cart/input/camera
behavior before later development-only cup/diagnostic changes. Earlier
rendered grip checks exposed stale skinning after manually restored poses;
the shared fixture now observes live poses and the final full suite passes.

All **2,953 primary inputs** remain unchanged after final capture. Isolated
build-only clean packing and Ball prefab local IDs are reviewed; its canonical
**14-object graph** matches. The release APK predates only an independent
development-only GolfMatchProbe diagnostic correction, which is compiled out
of release; gameplay matches. Font regeneration from a checkout with spaces
and unrelated working directory preserves all required glyphs and licensing.
No Computer Use or Git publishing was used.

The complete player and fresh **89,000,984-byte** APK/audit are retained in
`Builds/GolfBallAimQA/Release/` and promoted to the shared WindowsFinal, Android
and SizeAudit folders; all delivered files match. The APK strict gate and v2
signature pass. See [size/headroom and recoverable archives](BUILD-SIZE.md).
Physical-phone touch/rendering/FPS and WAN remain unverified.

Final hygiene checks pass across **3,402 publishable files and 468 LFS paths**.
The pre-existing shared `.git/index.lock` blocks the default Git dry run;
TaskReady was rerun with a read-only copied index. The real index hash remains
unchanged and the existing lock is preserved. Diff/stat/status were reviewed.


## Golf closed grip and raised carry angle — 4 October 2026

The complete Windows player passes **172 Golf/club/room assertions**: 134
rendered offline, 19 host and 19 client. Evidence and native captures:
`Builds/GolfMiniGameQA/Run-20261004-192709/`. The live high/low frames were
inspected directly; manually restoring multiple bone poses in one rendered
frame can reuse Unity's skinning cache and is not used for these checks.

The source character has no weighted finger joints. Import now derives a
local closed-mitten blend shape for both existing character LODs, plus a grip
socket. The right grip marker is on the shaft axis; the equipped shape is
released when driving, travelling or clearing the round. Original FBX, atlas,
rig hierarchy and unrelated vertices are preserved. Palm weights transition
into the hand bone to stabilize the cavity as the wrist moves.

Idle and low-hand carry remain 12 degrees down/back. The highest walk/run
phase reaches **52.71 / 52.72 degrees**, with a minimum **0.059 m** head
clearance above foot level. Native arm travel remains: right/left forward-back
ranges **0.379/0.511 m** walking and **0.378/0.511 m** running. Right-hand angular
sway is **45.1 / 49.3 degrees**. Shaft-to-socket error stays below 1 mm. All six
idle/walk/run high/low grip poses pass actual skinned-triangle enclosure checks:
6–8 of eight radial rays hit the hand, with minimum cavity distances of
**8.4–8.6 mm**. Both near/far character LODs receive the closed shape. Full-body,
close-hand, charge, contact and follow-through captures were reviewed.

The existing two-hand charge/contact, ball impulse, cooldown, cart stow/reequip,
five-hole progress, results and sport switching checks pass. Host/client use
protocol 21. The tested snapshot also contains the concurrently developed aim
button and 3x ball runtime; a later change to the independent development-only
ball-physics probe is outside this snapshot. That probe difference does not
change shipped gameplay. Runtime inputs and generated prefab equivalence are
recorded under `Builds/GolfGripSlopeQA/`.

Tuning and regeneration are documented in
[the club source guide](../ArtSource/Golf/Club/README.md). Windows checks do not
establish physical-phone touch, rendering or FPS, or WAN behavior. Exact APK
measurement and delivery/archive records are in
[BUILD-SIZE](BUILD-SIZE.md#golf-closed-grip-and-raised-carry-angle--4-october-2026).

## Golf ball visibility and free physics — 4 October 2026

Native reproduction at the real Hole 1 tee found a correctly spawned, grounded
43 mm ball. The former rear camera hid it behind the avatar at swing distance,
and the gold indicator could cover the small rendered sphere. Live-Golf
third-person framing now uses a closer shoulder position; the indicator keeps
a screen-space gap above the ball, and the last ball LOD no longer culls.
First-person, elevated and driving camera rules are preserved.

The final Windows player passes **22 dedicated checks** in
`Builds/GolfBallPhysicsQA/Run-20261004-172433/`. Actual native images with all UI
visible prove ball pixels at default 16° and 25° pitch, without moving the ball
or forcing its LOD before the initial captures. A real sprint through the ball
moves the player about **5.04 m** and the ball **0.00000 m**. Late/re-enabled
character controllers and a moving cart collider also cannot push it.

On level ground, a **3.000 m/s** ball slows to **2.315 m/s after 0.6 seconds**
and stops. A sleeping ball on a **5° slope** accelerates from **0.242 to
0.592 m/s**, travelling **0.306 m** in one second; on **15°**, it accelerates
from **0.718 to 1.758 m/s**, travelling **0.907 m**. Both remain supported.
Qualified **8% / 80% swings** produce peaks of **4.210 / 31.271 m/s**; the
charged ball rises **1.994 m** in the observation period. Each swing counts
once. Tests use gravity/contact physics and measured motion, not force values.

The complete five-hole/club/room suite passes **143 assertions: 105 offline,
19 host and 19 client**, in `Builds/GolfMiniGameQA/Run-20261004-170433/`.
It verifies helper ownership, cup order, recovery, touch input, animation,
the actual 30-second finish deadline, DNF, restart and sport cleanup.
Existing charge tests now use a temporary level collider on both peers;
assuming a free ball stayed still on a real incline incorrectly cancelled
charge after downhill rolling was restored. No gameplay ground is replaced.
The **203 cart regressions** pass in
`Builds/GolfCartQA/Run-20261004-165059/` on identical gameplay sources before
these development-only fixture corrections, including three camera modes,
reverse follow, driving collisions and two-player replication.

The complete tested Windows snapshot is retained in
`Builds/GolfBallPhysicsQA/Release/Windows/`, built at **17:22:06 Malaysia time**.
The development capture fixture renders every overlay Canvas; the shared
capture helper previously selected only one and could omit the Golf HUD.
All **2,951 asset/package/settings
inputs** are captured; the generated Ball prefab's **14-object graph** is
equivalent to primary despite regenerated local IDs. No model, texture,
package, room protocol or scoring rules were added or replaced. Physical-phone
touch/rendering/FPS and WAN remain unverified. See
[physics, controls and repeatable checks](GOLF-MINIGAME.md) and
[release size and delivery](BUILD-SIZE.md).

The complete **313-file** player is promoted to `Builds/WindowsFinal/`, and the
matching fresh **88,984,188-byte** APK/audit is promoted to the shared Android
and SizeAudit folders. All player files and the APK match the retained release.
The reviewed preceding shared release is recoverable in
`Legacy/20261004-173632-475-golf-ball-physics-delivery/`; superseded trials and
earlier snapshots are documented in BUILD-SIZE. No Git staging or publishing
was performed.

The default physics-check launcher also passes **22 checks** against the
promoted `WindowsFinal` player from an unrelated system temporary directory,
in `Builds/GolfBallPhysicsQA/Run-20261004-174344/`; the checkout path contains
spaces. Normal readiness passes portable paths but encounters the existing
shared Git index mutex during its dry run. The same full read-only check passes
using an index copy: **3,394 publishable files, 466 LFS paths, 271 pending
operations**. The actual index SHA is unchanged and its shared lock is
preserved. Readiness and index evidence are under `Builds/GolfBallPhysicsQA/`.

## Golf palm animation — 4 October 2026

Single-hand carry preserves the right wrist's full original idle/walk/run
rotation along with the shoulder and elbow. Club orientation is stabilized at
the moving palm, and fingers curl using the actual palm normal. This removes
the counter-rotation that previously forced the palm down and folded the wrist.
Two-hand charge/contact/follow-through still use the existing blended arm pose.

The complete Windows player passes **142 Golf assertions: 105 offline, 19 host
and 18 client**, in `Builds/GolfMiniGameQA/Run-20261004-160852/`. Across complete
walking/running samples, additional wrist deviation is **0.000°**, while palm
rotation changes **82.604° / 112.407°** with the arm animation. Grip error stays
below **1 mm**. Right-hand forward/back ranges are **0.4615 / 0.4615 m**, compared
with **0.5114 / 0.5105 m** on the left. Club backward alignment remains at least
**0.978** and downward pitch stays **9.38–18.00° / 9.40–18.00°**. Idle wrist
deviation is also zero with **1.195°** natural hand sway.

Native idle, walk, run and two-hand charge views were inspected. The suite also
checks contact timing, strokes, all five holes, recovery, restart, cart stow,
sport cleanup and two-player authoritative replication. Evidence and the exact
tested player are retained under `Builds/GolfPalmCarryQA/`. No art, animation
asset or package was added. Physical-phone rendering/FPS and WAN remain
unverified. See [carry behavior](GOLF-MINIGAME.md#handheld-midnight-iron) and
[tuning](../ArtSource/Golf/Club/README.md).

Normal readiness passes portable-path scanning but its Git dry-run encounters
an existing shared index mutex. The full check passes using a temporary index
copy: **3,393 publishable files, 466 LFS paths and 270 pending operations**.
The actual index SHA stays unchanged and the shared mutex is preserved. The
obsolete one-file copy is archived in
`Legacy/20261004-162243-882-golf-palm-readiness/`; scope and checks remain under
`Builds/GolfPalmCarryQA/`. No staging, commit or push was performed. The complete
new player and APK are retained in the release folder and promoted to the shared
WindowsFinal/Android directories after the older game closed. All **314 Windows
files** match the retained player. The older shared release is recoverable in
`Legacy/20261004-162407-373-golf-palm-delivery/` (**325 files, 682.08 MB**).

## Rounded Golf ball — 4 October 2026

The supplied rounded-dimple model replaces the shared `Ball.fbx` and existing
`Ball.prefab` visual used by both practice equipment and live match balls. The
prefab and model asset identities remain intact. The original 87,048-triangle
FBX is retained byte-for-byte, with an editable Blender master and **15,998 /
720-triangle** delivery LODs. Geometric dimples share one ivory material without
the previous ball textures. The centred mesh fits the existing **21.5 mm**
sphere collider; mass, damping, swing/contact timing, ownership, authority,
stroke rules, five-hole progress, recovery and final countdown are unchanged.
Normal builds and the equipment generator preserve this replacement. See
[source, regeneration and dependency audit](../ArtSource/Golf/Ball/README.md).

The final captured Windows player passes **140 Golf assertions: 103 offline,
19 host and 18 client**, in `Builds/GolfMiniGameQA/Run-20261004-142455/`.
Checks cover the actual new mesh/material on live balls, centring, one physics
body/collider, charged contact, helper ownership, all five real cups, wrong-cup
and shoreline recovery, restart, completion/countdown, sport switching, cart
stow, current club carry and two-player authoritative replication. **20
equipment checks** also pass in `Builds/GolfBallRoundedQA/Equipment/`, including
practice placement, both LODs, material support and streamed cleanup/reentry.
Native close-ball, normal playing, equipment and both model LOD views were
inspected. The animated carry fixture samples after LateUpdate so it measures
the rendered hand and club pose together.

Format-aware audits find no image or external-library dependency. Regeneration
from a checkout path containing spaces and an unrelated working directory
preserves canonical positions, UVs and winding; pole custom-normal variation
is bounded below 0.0002. Only inactive exporter SceneInfo provenance and saved
Blender UI history are exempt historical metadata. Reports, hashes and captured
inputs are under `Builds/GolfBallRoundedQA/`. Physical-phone touch, rendering/FPS
and WAN remain unverified. Final package measurements and recovery records are
in [BUILD-SIZE](BUILD-SIZE.md#rounded-golf-ball--4-october-2026).

## Golf club natural carry — 4 October 2026

Idle, walking and running retain the right shoulder/elbow's original animation,
with the club attached to the moving palm instead of a fixed hip-space target.
The wrist compensates for the long shaft and fingers remain curled around the
grip. Default pitch is 12° down with a bounded 6–18° animated variation; side
sway stays within 12° of behind the player. This prevents the original wrist
animation from swinging the shaft upright or forward. Charging and swinging
still blend into two-hand IK and return to the moving hand afterwards. See
[carry tuning](../ArtSource/Golf/Club/README.md) and [game behavior](GOLF-MINIGAME.md#handheld-midnight-iron).

The retained carry-task Windows player passes **140 Golf checks: 103 offline,
19 host and 18 client**, in `Builds/GolfMiniGameQA/Run-20261004-142055/`.
The entire sampled walking/running cycle keeps the head behind the player
(minimum horizontal back alignment **0.978**), with downward pitch
**9.38–18.00° walking / 9.46–18.00° running**. Right-palm forward/back ranges
measure **0.3893 / 0.3888 m**, compared with **0.5114 / 0.5108 m** for the left
hand. The club stays on the right palm with measured grip error below 1 mm.
Idle head sway is **0.0125 m** and pitch **16.23–16.48°**. Carry/idle/walk/run,
two-hand charge/contact/follow-through and first-person captures were inspected.
The suite also checks strokes, helper ownership, contact timing, charge cancel,
recovery, restart, driving stow, sport cleanup and real two-player loopback.
Concurrent rounded-ball checks are included. Physical-phone touch/rendering/FPS
and WAN remain unverified.

The shared `Builds/WindowsFinal/` now contains the newer **14:23:37** rounded-ball
player, including this final carry implementation. It passes **140 checks** in
`Builds/GolfMiniGameQA/Run-20261004-142455/`, including the whole-gait backward
direction and grip assertions. All **313 delivered files** match its exact
retained player. This newer complete delivery is preserved; the older tested
carry player remains in `Builds/GolfClubCarryQA/Release/Windows/`. The matching
shared Android APK is **88,981,824 bytes**, with **11,018,176 bytes** below the
strict ceiling. Source captures, promotion hashes, signatures and archive scope
are recorded in [BUILD-SIZE](BUILD-SIZE.md#golf-club-natural-carry--4-october-2026).

## Midnight Iron handheld club — 4 October 2026

Every participant receives the supplied textured club when a Golf round starts. Walking carries it in the right hand with its head above the ground; running tilts it toward the body while the free arm retains its gait. Charging, impact and follow-through use both hands, curled fingers and the existing avatar rig. First-person hands remain visible and the locally hidden head restores in third person. Driving/travel/round cleanup hides the club; restarts reuse one instance. Feet stay planted through contact, the ball receives one authoritative impulse at **0.10 seconds**, and **0.74-second** recovery prevents duplicate strokes. Reach validation rejects impossible contacts. Round/ball resets cancel pending impacts. Reliable charge/heading and server-clock swing poses synchronize host and clients; **protocol 21 requires matching builds**. Existing ownership, helper completion, scores, hole order and countdown rules remain intact. See [controls and tuning](GOLF-MINIGAME.md#handheld-midnight-iron).

The fresh textured Windows player passes **113 Golf assertions: 84 offline, 15 host, 14 client**, in `Builds/GolfMiniGameQA/Run-20261004-130434/`. Checks cover every player's club, three LODs/one material, all three correctly sized Texture2D maps, no prop physics, hand-marker grip errors below 2.5 cm, reachable two-hand charge/impact/follow-through, first-person restoration, carry floor clearance, running tilt/free arm, charge cancellation, delayed single ball impulse, movement/jump/duplicate gates, reset cancellation, driving stow and instance reuse. Actual two-player loopback verifies reliable remote charge, shared swing timing, strokes/owner progress, rejected stale rounds and the existing thirty-second replicated DNF. Native carry, charge, first-person, contact, follow-through and running captures were inspected; near/mid/far model renders are in `Builds/GolfClubQA/Art/`.

The same final gameplay sources pass **203 cart assertions** (`Builds/GolfCartQA/Run-20261004-124646/`), **509 football match/physics/network assertions** (`Builds/FootballMatchQA/Run-20261004-124646/`) and **28 basketball gameplay checks plus two connection checks** (`Builds/BasketballGameplayQA/Local-20261004-124647/`). These regressions precede only the final club texture import/material correction and added map assertion; final Golf validation uses the corrected textured player. No physical-phone touch, rendering/FPS or WAN claim follows from these Windows tests.

The format-aware art audit passes in the primary workspace and from the alternate checkout with spaces, invoked from an unrelated working directory. It confirms the **7,702-triangle** editable master, **3,600 / 1,400 / 500** LODs, 1.02 m length, finite UVs, single connected components, closed meshes, no degenerates, original-archive/hash integrity and all seven relative Blender image dependencies. The delivery FBX's three texture references are relative; no external libraries or active machine paths remain. Source maps/original ZIP are retained outside Unity. Reports: `Builds/GolfClubQA/PortableAudit/` and `Builds/GolfClubQA/AlternateCheckout/`; see [source guide](../ArtSource/Golf/Club/README.md).

AndroidSubmission completes release IL2CPP ARM64 with a **89,418,634-byte** APK and **10,581,366 bytes** of hard-limit headroom; independent v2 signing passes. The 75 MB development target remains unmet. All **2,947 captured asset/package/settings inputs** remain stable during packaging and match the primary workspace except the isolated editor's clean-cache option. The user's editor and original caches remain open/in place. The complete tested Windows player, built at **13:04:30 Malaysia time**, is promoted to `Builds/WindowsFinal/` with updated `LATEST-BUILD.txt`; all **313 files** match the retained `Builds/GolfClubQA/Release/Windows/`. Current APK/audit and exact retained releases are documented in [BUILD-SIZE](BUILD-SIZE.md#midnight-iron-handheld-club--4-october-2026).

Reviewed superseded output and trial evidence are recoverable in `Legacy/20261004-131836-279-golf-midnight-iron-final/` (**422 files, 763.69 MB**). An unchanged empty Git index mutex was observed across separate inspections, confirmed exclusively openable and cleared with the actual index SHA unchanged; its backup is in that archive. Active sources, assets, masters, current evidence, caches and unrelated work were preserved. Git staging, commits and publishing remain with the user.

## WindowsFinal handoff — 4 October 2026

The complete reference-tyre player built at **11:43:59 Malaysia time** is promoted to `Builds/WindowsFinal/`, including executable, data, runtime DLLs, supporting folders and Golf/Fishing launchers. It is the same snapshot that passes **203 cart assertions**, retaining the later Golf-club source exclusions recorded below. `LATEST-BUILD.txt` states its feature, build time, test evidence, matching APK and snapshot limits. The standing Windows delivery requirement is recorded in [AGENTS.md](../AGENTS.md#windows-testing-player-delivery) and [repository hygiene](REPOSITORY-HYGIENE.md#windows-player-handoff).

The reviewed prior chassis-wheel player is recoverable in `Legacy/20261004-122716-284-windows-final-reference-tyres/` (**313 files, 588.95 MB**). All **313 copied files** match the tested source byte-for-byte, apart from the deliberately updated build note. The copied player passes an offline Golf startup and normal-exit check from the final directory. Current evidence is in `Builds/WindowsFinalHandoff/20261004-122709/`. Source/runtime/assets and APKs are unchanged by this handoff, so no new Unity build or Android-size claim is made. No computer-use or Git publishing action is performed.

The normal readiness check passes portable-path scanning but remains blocked at Git's dry-run by the pre-existing shared `index.lock`. The full check passes using a temporary index copy: **3,383 publishable files, 464 LFS paths and 252 pending operations**. The real index hash is unchanged and the shared lock is preserved. `readiness-normal.txt`, `readiness-index-copy.txt` and `readiness-index-scope.json` record this limit in the handoff evidence directory.

## Golf cart reference tyres — 4 October 2026

The delivery wheels follow the user-approved [side](../ArtSource/Golf/Cart/References/Wheel-side.png) and [tread](../ArtSource/Golf/Cart/References/Wheel-tread.png) references: thick rounded black sidewalls, recessed chevron channels and cream six-spoke rims with a broad central cap. All four rims are **0.38 m diameter** and centred on their rolling pivots. Tyre radii remain **0.339 m front / 0.328 m rear**. The beads share vertices with the rim, and tread channels are part of a closed shell. Every tread block retains the centred circular outer envelope; maximum groove depth is **6 mm**. All twelve wheel meshes pass outward winding, groove depth/spacing, circular/equal rims, closed-surface and welded-bead checks, alongside existing chassis connections.

Mobile totals are **33,958 / 13,805 / 6,721 triangles**, preserving five renderers per LOD and one existing atlas/material. Near/mid/far wheels have **16/10/6 tread rows**; the latter two use fewer axial samples and omit spoke bevels. This removes **1,600 / 960 triangles** against using the full new profile at those distances. No new texture, shader, runtime component or network message is added. The full **246,850-triangle** master, all three body meshes and all three delivery texture byte hashes are unchanged. All sixteen canonical geometry/UV/normal hashes agree after regeneration in a checkout containing spaces, invoked from outside it. Blender image/library and FBX texture dependencies pass format-aware portability checks; retained exporter provenance is inactive.

The matching Windows player passes **203 cart assertions (159 offline, 22 host, 22 client)** at `Builds/GolfCartQA/Run-20261004-114611/`. Checks cover measured wheel radii, 10/25/50 cm starts, reverse, stationary settling, front-only steering, existing 17/8 m/s limits, collision, forty sand-bowl crossings, seat/camera behavior, ownership, travel and two-player replication. Actual side, steering and three-LOD views were inspected, together with close tyre/rolled-rim renders. Evidence, source manifests, map/body integrity and portable regeneration records are in `Builds/GolfCartTreadQA/`. Vehicle gameplay, collision hull and camera logic are unchanged.

The exact **88,963,010-byte** APK passes AndroidSubmission and independent v2 signing and is retained with its full audit in `Builds/GolfCartTreadQA/Release/`. Cart runtime/art/prefab inputs match; generated URP registrations, isolated build-output/cache flags and later Golf-club art/editor/runtime additions and player/network/Golf/material integration changes are recorded in the source comparisons. Those ongoing changes are outside this release's scope; multiplayer verification uses matching captured builds. The shared root APK/audit are preserved. See [size/headroom, capture and recovery](BUILD-SIZE.md#golf-cart-reference-tyres--4-october-2026). No computer-use operation or Git publishing was performed. Phone touch/rendering/FPS and WAN remain unverified.

`Check-TaskReady.ps1 -RequireApk` passes portability and content checks, but its normal Git dry-run is blocked by the pre-existing shared `index.lock`. The same script passes with a temporary copy of the existing index: **3,383 publishable files, 464 LFS paths and 250 pending operations**. The real index hash is unchanged and the shared lock is preserved. Evidence is in `task-ready.txt`, `task-ready-index-copy.txt` and `readiness-index-scope.json` under the task directory. Its default-APK check sees the retained older shared package; freshness, signing and size for this task are established separately by the exact retained release above. Git publishing through the normal index remains blocked while that lock exists.

## Golf cart rim fit and circularity — 4 October 2026

The old delivery had a round tyre around an irregular, disconnected original hub, with smaller rear rims. All four delivery wheels now use **0.47 m six-spoke rims**, centred on their rolling pivots. The front/rear tyre beads share mesh edges with the rim. All **twelve wheel meshes** pass closed-surface, outward-winding, equal front/rear/left/right geometry, concentric lip/tread and uniformly spaced angular-ring checks. Tyre radii remain **0.339 m front / 0.328 m rear**, with **64/40/24 segments**. Retained concurrent fixed axle supports add **288/224/160 triangles** to the body LODs; final totals are **28,838/12,205/5,761** with the same five renderers and shared atlas/material.

The original **246,850-triangle** master, original body surfaces/UVs and all three texture bytes are preserved. This task's geometry/UV integrity hash for the full source is `633717016cb39339837685def044918982fa940decb5e32fe87952f4adaf8ca0`. UV strip packing preserves all twelve wheel geometries while reducing position/UV entries **21,928 → 15,640**. Regeneration and format-aware Blender/FBX dependency checks pass from a checkout containing spaces, invoked outside that checkout. All sixteen meshes retain equivalent canonical surface/UV/normal associations; two mirrored near meshes may differ in raw index ordering after BMesh beveling. Only inactive exporter provenance is retained as historical path metadata.

The final Windows player passes **203 cart assertions (159 offline, 22 host, 22 client)** at `Builds/GolfCartQA/Run-20261004-021152/`. Tests cover forward/reverse wheel movement, 10/25/50 cm starts, stationary body settling, rendered tyre radii, steering, 17/8 m/s limits, ownership, parking, seating, collision, all sand-bowl exits, cameras, scene/travel restrictions and two-player replication. The independent **142-assertion** island regression is at `Builds/GolfG2QA-20261004-005845/`; its course/environment inputs are unchanged by this model refinement. Actual side, turning and three-LOD player views were inspected. These builds retain parallel overhead-camera following, 0.85 visual wheel-roll calibration, supports and driving-HUD changes; this task does not redesign those systems.

The fresh **88,787,790-byte** release passes the strict AndroidSubmission gate and v2 signing. Captured inputs, source comparisons, body/map integrity, packing estimates and before/after evidence are in `Builds/GolfCartRimQA/`. Android regenerates its URP resource registrations, while runtime and art match the working source. Phone touch/rendering/FPS and WAN are unverified. [BUILD-SIZE](BUILD-SIZE.md#golf-cart-rim-fit-and-circularity--4-october-2026) records size/headroom and recovery. No computer-use operation, staging, commit or push was performed.

## Golf cart chassis connections and startup rolling — 4 October 2026

The four wheel hubs now have fixed axles and upright mounts reaching into the chassis. These are merged into the existing body meshes for all three LODs, preserving five active renderers and one shared material per LOD. Totals are **28,838 / 12,205 / 5,761 triangles**, an increase of **288 / 224 / 160**; the original 246,850-triangle source geometry/UV hash and all three delivery texture hashes are unchanged. The format-aware art audit validates twelve axle connections, tyre/rim circles, matching rims, outward normals, welded bead edges, closed wheel surfaces and all active relative dependencies. It also passes from the existing checkout with spaces, launched from an unrelated working directory. The current FBX has no retained machine-path provenance fields. Reports and low-angle/full-steering views are in `Builds/GolfCartWheelContactQA/`; standard close LOD views remain in `Builds/GolfCartQA/Art/`.

Wheel rotation follows the displayed ground path using vehicle heading and ground tangent. Body tilt or vertical settling cannot add rotation while stationary; blocked motion and summon/rebase discontinuities retain their existing protections. The default **Wheel Roll Scale = 0.85** intentionally slows the visual rotation 15%; **1.0** restores the exact tyre-circumference ratio. This does not change cart speed, ground clearance or authoritative movement. Configuration and regeneration are explained in [GOLF-CART.md](GOLF-CART.md) and [the source guide](../ArtSource/Golf/Cart/README.md).

The matching Windows development player passes **203 cart checks: 159 offline, 22 host and 22 client**, including **28 new observable wheel assertions**. A 10 cm start produces **14.36269°** on the front wheels versus **14.36617°** expected at the configured scale; 25/50 cm starts and all four reverse paths pass the 0.5° tolerance. A 15° body pitch plus 25 cm vertical settling leaves wheels stationary, as does a target speed of 17 m/s without displacement. Rendered radii measure **0.3390006 / 0.3280007 m**, agreeing with the configured 0.339/0.328 m. Existing wall/shore containment, forty real sand-bowl crossings, seat pose, camera modes, steering, ownership, interpolation and two-player loopback replication pass. Evidence: `Builds/GolfCartQA/Run-20261004-015517/`. Native near/far LOD and steering views were inspected in addition to the Blender mount views. Phone touch, mobile rendering/FPS and WAN are not validated by this Windows run.

The matching final **88,782,546-byte** release APK passes AndroidSubmission and independent v2 signing, with **11,217,454 bytes** of strict-limit headroom. All 2,925 captured inputs remain stable during final packaging and match the primary workspace except the isolated editor clean-cache option. The first package was superseded after a concurrent model re-export; final art, Windows probes and Android package use the same current FBX. Current APK/audit/player are under the standard ignored delivery paths, with an exact retained copy in `Builds/GolfCartWheelContactQA/Release/`. See [measurements and source consistency](BUILD-SIZE.md#golf-cart-chassis-connections-and-startup-rolling--4-october-2026). Superseded task/shared outputs and source backups are recoverable in `Legacy/20261004-020924-883-golf-cart-wheel-contact/` (378 files, 814.16 MB). No active source asset, cache or unrelated work was archived; staging and publishing remain with the user.

## Golf cart elevated steering follow — 4 October 2026

The elevated driving camera now uses the same displayed-heading delta as first-person and third-person, preserving manual look offset. Left steering turns the view left and right steering turns it right. Its 58° downward angle and 10 m distance remain unchanged. Reverse-facing smoothing remains specific to third-person. Presentation still runs after cart interpolation and applies only to the local driver; normal walking, exit and island-switch camera behavior remain intact. See [controls and camera behavior](GOLF-CART.md).

The fresh Unity 6000.3.20f1 Windows player passes **175 cart checks: 131 offline, 22 host and 22 client**, including the existing sand-bowl traversal checks. Actual elevated measurements are **+9.20° cart / +9.20° camera** on right input and **−8.80° / −8.80°** on left input. The driving client's interpolated left turn measures **−18.53° / −18.53°**; the host's own overhead view does not rotate with that remote driver. Checks also cover manual drag/offset, switching between all three modes, existing first/third-person reverse transitions, exit, movement/jump restoration, authority, collisions and cleanup. The native elevated capture was inspected. Evidence: `Builds/GolfCartQA/Run-20261004-005627/`; build/input records: `Builds/GolfCartOverheadQA/`.

The same player passes **81 Golf gameplay checks** in `Builds/GolfMiniGameQA/Run-20261004-005628/`, including independent progress, owner-based helper completion, incorrect cups/out-of-bounds recovery, local indicators, stroke/time results and the real shared 30-second countdown ending in replicated DNF. No new art or package is introduced by camera following. The fresh **88,787,118-byte** AndroidSubmission and independent v2 signature pass; all **2,925 captured inputs** remain stable and match the workspace apart from the isolated clean-cache editor option. Physical-phone touch, rendering/FPS and WAN remain unverified. The measured release and archive are recorded in [BUILD-SIZE](BUILD-SIZE.md#golf-cart-elevated-steering-follow--4-october-2026).

## Golf cart sand-bowl rim traversal — 4 October 2026

The reported blockage concerns driving out of the sand bowl. The pre-fix player reproduces four failures on two real exits: Bunker 4 westbound stops at **32.867 / 43.2 m**, and Bunker 5 eastbound at **26.633 / 36 m**, with equivalent reverse failures. PhysX returns **zero-distance, horizontal-normal** hits against the terrain sectors when the level body sweep starts inside an uphill triangle. Four-wheel support accepts the same ground. The motor now excludes terrain from its obstacle sweep and retains wheel-based slope, height and sea-level validation, solid-prop/cart collision and shoreline boundaries.

The fresh Windows player passes **169 assertions** (**127 offline, 21 host, 21 client**) at `Builds/GolfCartQA/Run-20261004-002512/`. This includes **40 full traversals** of all five real sand bowls, east/west/north/south, driving forwards and in reverse. The formerly blocked exits now reach **43.067 / 43.2 m** and **35.983 / 36 m**. Existing large-step wall collision, twelve shoreline directions, slope parking, 17/8 m/s limits, seated roof/footwell clearance, both character LODs, circular wheel LODs, cameras, recall, travel, ownership and two-player replication also pass without runtime exceptions. Actual side/rear player views retain round, clean tyres and the corrected seated pose.

Reproduction, diagnostics, source comparisons and build evidence are in `Builds/GolfCartRoundFixQA/hill-*`. This code change adds no art, texture, material or package. The **88,708,270-byte** APK passes AndroidSubmission and independent v2 signing. All 2,925 captured inputs match before packaging apart from the isolated editor's clean-cache option; later parallel rim/bead geometry is outside this captured APK/Windows player. The tested deliveries and fresh packing are at `Builds/GolfCartRoundFixQA/Release/`. Portable Blender/FBX and original-master integrity evidence from the seating/tyre entry below applies to the captured art. Physical-phone controls, rendering/FPS and WAN remain unverified. See [bytes, source scope and recovery](BUILD-SIZE.md#golf-cart-sand-bowl-rim-traversal--4-october-2026). Superseded deliveries and the stale-lock backup are archived in `Legacy/20261004-004451-752-golf-cart-rim-delivery/`. An abandoned, unchanged empty Git mutex was proven unheld and removed; repository readiness then passed. No Git staging or publishing was performed.

## Golf mini game — 4 October 2026

The five-hole game extends the existing island, shared athlete commands, camera, room authority and ball art. All five cups remain open. Player progress, stroke ownership, owner-based completion, recovery, countdown, DNF and ranking live in gameplay classes; the leaderboard and indicators read state. See [rules, controls and class responsibilities](GOLF-MINIGAME.md).

The final Unity 6000.3.20f1 Windows player passes **81 Golf checks: 58 offline, 12 host and 11 client**, including two existing room-readiness checks. Evidence is `Builds/GolfMiniGameQA/Run-20261004-001418/`. A real owner RPC hits another player's ball, preserves ownership and charges only the hitter. The host simulates actual motion and cup entry; the owner advances while the helper's target remains unchanged. Both peers observe different targets, wrong-cup reset poses, continued play after the first finish, and replicated DNF after an actual 30-second wait. Offline checks cover all four incorrect cups, shoreline recovery without a penalty, downward cup capture versus fly-over, all five active triggers, target/ball indicators, the presented reverse-camera bearing, touch hold/release/cancel, clean round/sport transitions, results by strokes then time, exact ties and early all-finished termination. Five hundred fractional-clock cases prevent an initial `00:31` display.

The rendered view was inspected with the five leaderboard column labels, local target arrow/distance, own-ball marker and existing movement/cart controls. Runtime balls share the two authored LODs without cloning nested equipment LOD groups. The original equipment display still reports its pre-existing nested-LOD warning; the new live balls have one group and no duplicate registration.

Regression results are **509 football checks**, **142 existing Golf map checks**, **128 cart checks** (86 offline, 21 host, 21 client) and **67 basketball interaction checks**. Reports are `Builds/FootballMatchQA/Run-20261003-231025/`, `Builds/GolfG2QA-20261003-231234/`, `Builds/GolfCartQA/Run-20261004-000952/` and `Builds/GolfMiniGameQA/Basketball-Offline-20261004-001024/`. Existing character movement, football matches, basketball pickup/shoot/recovery, cart authority/cameras and island switching pass. The final Golf suite also ran with default paths from a second checkout containing spaces, while invoked from an unrelated working directory.

Windows validation used a full build followed by scripts-only updates for Golf fixes and UI labels. A later development-only cart terrain fixture is outside this Windows player's captured scope; no new terrain behavior is claimed. Android packaging resynchronized the complete source snapshot; its **2,925 captured inputs stayed unchanged**. The fresh **88,708,082-byte** release passes AndroidSubmission and independent v2 signing. Subsequent parallel cart-rim FBX/motor/probe updates are outside that captured APK. Physical-phone touch, rendering/FPS and WAN remain unverified. Release bytes, input hashes, signing and cleanup are recorded in [BUILD-SIZE](BUILD-SIZE.md#golf-mini-game--4-october-2026).

## Golf cart driving cameras — 3 October 2026

First-person and third-person views follow the displayed cart heading one-to-one while preserving manual look offset. Third-person reverse input smoothly turns the view backwards; forward input smoothly returns it forwards. First-person never adds the reverse turn. Elevated view keeps its world heading through turns and reverse. Camera presentation updates after client cart interpolation, applies only to the local driver, and clears after exit, sport switching and travel. Inspector settings and behavior are described in [GOLF-CART.md](GOLF-CART.md).

The fresh Windows player passes **128 cart assertions** (**86 offline, 21 host, 21 client**) at `Builds/GolfCartQA/Run-20261003-231510/`. With 20% right steering, cart/camera changes both measure **+9.20°**; with 20% left steering, both measure **−8.80°**, in each following camera mode. Manual 20° offsets and an additional 13° look drag remain intact. First-person reverse at **−8.000 m/s** adds **0.00°** rear-view offset. Third-person reverse measures **10.59°** after the first 0.15 s and **179.89°** after settling; forward return measures **170.11°** initially and **0.11°** after settling. The driving client's interpolated cart/camera turn both measure **8.97°**, while another player's turn does not rotate the host's walking camera. Original 17/8 m/s limits, wheel motion, collisions, shoreline, seating, exits, recall, ownership and scene/travel restrictions pass.

An independent **142-assertion** golf traversal/camera/environment regression passes at `Builds/GolfG2QA-20261003-230821/`. Native first-person/third-person reverse, forward, turning and elevated captures were inspected. Measurements sample cart and camera together after LateUpdate; the first attempt mixed a current cart pose with the previous camera frame and produced false comparison failures. The final fixture fixes sampling without relaxing bounds or changing runtime following. Current build/source/test records are in `Builds/GolfCartCameraQA/`; Android measurements are recorded in [BUILD-SIZE.md](BUILD-SIZE.md). The combined source also retains concurrent golf rules and cart seating/tyre refinements. No physical-phone touch, clipping/FPS or WAN verification is claimed.

The **88,713,562-byte** Android camera snapshot passes the submission gate and independent v2 signature. Its 2,925 captured asset/package/settings inputs are unchanged in the isolated checkout; camera and vehicle files match the primary workspace. Later parallel golf-ball reset reception and development-probe edits are outside this APK's scope. The tested Windows camera player is in `Builds/GolfCartCameraQA/Player/`; the original shared Windows player is preserved for the other task. Superseded output, initial sampling failures and the full-disk build attempt are archived in `Legacy/20261003-235321-846-golf-cart-camera/`. The successful retry retained original caches/dependencies and compressed only this task's copied Library without changing contents. No Git publishing or computer-use interaction occurred.

## Golf cart seating and circular tyres — 3 October 2026

The fixed ankle target exceeded the sprinter's short leg reach, clamping knees almost straight. Seating now derives reachable ankle targets from thigh/shin lengths and restores the neutral forward foot orientation. The hip marker moves from 1.10 to **1.04 m**. Actual player checks measure knees around **90.75° / 90.78°** and the complete baked avatar top at **2.272 m**, beneath the **2.33 m** roof-collider underside. Both character LODs and host/client seating pass; walking and jumping restore after exit.

All twelve wheel meshes have concentric circular tread rings, with radii **0.339 m front / 0.328 m rear** and 64/40/24 angular segments across LODs. The art audit rejects vertices outside the tyre radius, uneven tread spacing and inward tyre/hub normals. Dark fragments below both rear wheel wells are removed. Delivery counts are **25,833 / 9,686 / 3,748 triangles**; the full 246,850-triangle source, source UVs and three delivered texture hashes remain unchanged. Geometry/UV integrity hash: `963e86508f7ef7a5d9f63f15495dc9f9908b0097b36facae360868fbdfa5616a`.

The combined-source cart replay in `Builds/GolfCartQA/Run-20261003-231835/` passes **128 assertions** (86 offline, 21 host, 21 client), including entry, moving/turning pose, both character LODs, footwell/roof clearance, owner restrictions, collision, shore containment, cameras, travel, recall and pose restoration. It also includes concurrent camera work: sampled car/camera turns match at 9.21° right and about 8.80° left, and first-person reverse retains a 0.00° heading offset. The camera fixture now samples both completed presentation poses in LateUpdate; coroutine timers previously mixed the current cart with the preceding camera frame. Actual side/front/rear and three cart LOD views were inspected; close left/right rear-wheel renders are in `Builds/GolfCartRoundFixQA/`. Active Blender/FBX references resolve in an alternate checkout containing spaces, including execution from an unrelated working directory. The format-aware audit permits only inactive `FBXHeaderExtension/SceneInfo` exporter provenance as historical path metadata. Android/combined-source verification is recorded in [BUILD-SIZE.md](BUILD-SIZE.md); phone rendering/FPS/touch and WAN remain unverified.

The historical **88,707,818-byte** vehicle APK passes AndroidSubmission and independent v2 signing. Its 2,925 captured inputs include the tested vehicle changes; parallel edits to `GolfBall`, `GolfMatchState` and `GolfMatchProbe` during packaging are outside this snapshot. The superseded Windows player and APK/audit are now recoverable in `Legacy/20261004-004451-752-golf-cart-rim-delivery/Builds/GolfCartRoundFixQA/Release/`, preserving the other task's shared deliveries. See [measured size, source scope and recovery](BUILD-SIZE.md#golf-cart-seating-and-circular-tyres--3-october-2026). Superseded backups and trial runs are archived in `Legacy/20261003-235959-283-golf-cart-round-seated-cleanup/`; the obsolete inspection helper is in `Legacy/20261003-220432-659-golf-cart-inspect-helper/`. No Git staging or publishing was performed.

## Golf cart rear-wheel rendering and steering — 3 October 2026

The original rear hub faces had been flipped inward by recalculating normals on extracted open wheel meshes. Unity culled their outer surface, exposing a broken pattern. The extraction helper now preserves original face winding; all **12 wheel/LOD hub orientation checks** pass, and actual Windows views show the original rear spoke pattern in all three LODs. Vertex positions, triangle budgets (24,000/8,000/2,500), texture hashes and the high-quality source geometry/UV hash stay unchanged. The regular art audit now rejects inverted hubs. Relative Blender/FBX dependencies pass in the main and alternate checkout.

Steering is **120°/s maximum** instead of 72°/s and front-wheel steering is **38° maximum** instead of 28°. Throttle uses stick magnitude with existing forward/reverse intent, so diagonal steering does not reduce drive speed. The real Unity player measures **17.000 m/s straight and turning**, **8.500 m/s for both half-stick cases**, and **8.000 m/s reversing through a turn**. The normalized turning input produces **72.56° heading change in 0.8 seconds** and **28.72° displayed front-wheel steering** (maximum input X gives the full 38°).

The fresh Windows player passes **90 cart assertions**: **56 offline**, **17 host**, **17 client**. Existing rolling/reverse/stop, steering return, collision sweeps, slope parking, shoreline, UI, seat/exit controls, cameras, sport switching, ownership and two-player steering/pose replication pass. Cart speed limits remain 17/8 m/s; protocol remains 19 with no packet layout change. Evidence: `Builds/GolfCartQA/Run-20261003-212527/` and `Builds/GolfCartHandlingQA/`. The source-only course/grass inputs retain the earlier golf-suite coverage; no new course change or physical-phone touch/rendering/FPS/WAN verification is claimed. Fresh **88,625,838-byte** AndroidSubmission, complete packing and v2 signing pass; all 2,890 publishable asset/package/settings hashes match except isolated editor output/clean-cache options. Measurements and recovery records are in [BUILD-SIZE.md](BUILD-SIZE.md#golf-cart-rear-wheel-rendering-and-steering--3-october-2026).

## Golf course refinement — 3 October 2026

Five aqua numbered flags reuse the original map material. Holes 1/2/3 are 160.6–214.6 m apart across the south, west and northeast, with hole 4 on the southeast coast and hole 5 on the northern high ground. Cup diameter is 0.285 m, 1.5× the initial opening. The original terrain itself supplies the surrounding grass; separate colour overlays are removed. Both streamed scene and reusable prefab retain five real recessed cups, grounded tees, sixteen terrain sectors and the shared grass-clearing metadata. See [coordinates and authoring](GOLF-COURSE.md).

Unity 6000.3.20f1 passed **235 assertions** across the scene, prefab and repeat generation, and **78** after reopening the saved scene. Checks cover material identity, pair separation, enlarged physical openings, inner-wall/floor collision, nearby solid ground and source-terrain positions/normals/UVs. There are 47–48 terrain samples per green; position errors remain below 0.0001 m and UV errors below 0.000001. Actual saved-scene and running-player near/distant views were inspected. Six original terrain/grass/structures/colour/aqua/Blender masters match Git HEAD LFS hashes.

Desktop runs at `Builds/GolfG2QA-20261003-192847/` and `Builds/GolfG2QA-20261003-195311/` each passed **142** assertions; Android-geometry Windows evidence at `Builds/GolfG2QA-20261003-193449/` passed **142**, with **13,122** unchanged analytic terrain samples. Test defaults resolved from their script location while invoked from an unrelated temporary directory; player/checkouts contain spaces. Later cart-only inputs did not change the course/grass configuration or generated map assets. These golf checks do not claim cart behavior coverage; its final 87-assertion validation is documented separately below.

The final combined AndroidSubmission passed its strict gate and independent v2 signing at **88,624,542 bytes**. All 2,903 authored asset/package/settings inputs match apart from the isolated Android clean-cache option. Build/source/signature/packing evidence is in `Builds/GolfCourseRefinementQA/` and `Builds/SizeAudit/latest/`; see [measured release and unresolved phone/development-budget limits](BUILD-SIZE.md#golf-course-refinement-and-current-combined-release--3-october-2026). No physical-phone touch, quality/FPS, WAN or golf shot/scoring validation is claimed.

## Golf cart speed and wheels — 3 October 2026

The existing cart now drives at 2× its original forward/reverse speeds and animates the four supplied wheels. Unity 6000.3.20f1 passed **87 cart assertions**: **53 offline**, **17 host** and **17 client**. Actual player measurements are **17.000 m/s forward** (24.435 m in the 1.5-second probe) and **−8.000 m/s reverse** (8.054 m in 1.2 seconds). Measured rear-wheel rotation is **4,247.1° forward** and **−1,384.6° reverse**, following travelled distance. Checks cover all four axle pivots/three LODs, front steering, straight rear axles, stationary wheels, steering return, swept wall collisions under large simulation steps, slope parking and twelve shoreline directions. Existing buttons, seated pose, exit walking/jumping, all camera modes, owner restrictions, other-island restrictions, shared travel and disconnect cleanup also pass.

The two-player loopback run verifies host authority, a guest driving the host's cart, replicated front-wheel steering and seated pose on both peers. Protocol **19** includes the steering angle. A separate **142-assertion headless golf run** passes the existing cups, numbered flags, tees, spawns, shoreline/fairway/bunker collision, cameras and environment switching. Actual Windows captures of turning and all three cart LODs were inspected. No physical-phone touch, rendering/FPS or WAN behavior is claimed.

Cart evidence: `Builds/GolfCartQA/Run-20261003-202128/`; golf report: `Builds/GolfG2QA-20261003-202344/`; task build/input/art evidence: `Builds/GolfCartWheelsQA/`. Format-aware Blender/FBX checks pass both in the main checkout and from an unrelated working directory against a second checkout whose path contains spaces. The high-quality 246,850-triangle source stays intact; mobile totals remain 24,000/8,000/2,500 triangles with one body plus four wheel parts per LOD, unchanged maps and relative active dependencies. Recorded FBX `SceneInfo` exporter provenance is inactive metadata, not an external asset dependency.

Validation caught and fixed connected FBX instances rejecting wheel reparenting; the final prefab generator unpacks the model before assigning axle pivots and checks each assignment. An earlier simultaneous second graphics-player run crashed in D3D12; final cart rendering ran alone and the separate golf collision suite used headless mode. A concurrent source rollback removed slope-heading stability after the Windows build; the tested motor and slope regression were restored, with exact original tested hashes, before rebuilding Android. All 84 tested runtime source hashes match the final workspace. The final **88,625,346-byte** APK passes AndroidSubmission, complete packing and independent v2 signing; 2,890 publishable asset/package/settings hashes match except isolated editor output/clean-cache options. Only the corrected build's passing evidence is claimed. Android packaging and recovery records are in [BUILD-SIZE.md](BUILD-SIZE.md#golf-cart-speed-and-wheels--3-october-2026).

## Golf cart summon and driving snapshot — 3 October 2026

The initial protocol 18 cart player passes **76 assertions**: **44 offline, 16 host and 16 client**, including room-readiness markers. Reports and final close/distant camera/LOD captures are in `Builds/GolfCartQA/Run-20261003-192340/`. The probe exercises the actual Chinese summon/recall and drive/leave buttons, plus pointer-driven joystick input. It verifies one cart per player, blocked duplicate/invalid placement, Golf-only availability, owner-only recall, public empty-cart driving, single-seat occupancy, distance rejection, forward/reverse/steering/braking, obstacles, twelve shoreline directions, seat alignment, three camera modes, parked persistence, safe occupied recall, driver locomotion restoration, sport switching/return and owner disconnect cleanup. Host/client checks confirm authoritative actions and replicated guest driving of another player's cart. The script also ran from an unrelated temporary directory against a player path containing spaces.

The same base player passed these regressions with no failed assertions or runtime exceptions:

| Suite | Passing assertions | Evidence |
| --- | ---: | --- |
| Football match, physics, fake shots and ball replication | 509 | `Builds/FootballMatchQA/Run-20261003-194404/` |
| Basketball local host/client gameplay | 30 | `Builds/BasketballGameplayQA/Local-20261003-193027/` |
| Shared jump/momentum | 140 | `Builds/JumpQA/Run-20261003-193807/` |
| Five-hole golf traversal and scene switching | 142 | `Builds/GolfG2QA-20261003-193039/` |
| Shared Football → Golf → Football travel | 18 | `Builds/GolfCartQA/TravelRegression/` |

Football's physics probe now consistently uses batch mode with graphics enabled. A preceding headless run could not validate the transparent aim material; a non-batch capture run changed the simulated pointer fixture's viewport and produced a false fake-shot failure. The final 509-check run retains the graphics device without that viewport resize. Normal light/full kicking, cancellation and existing measured ball speeds all pass. Sport force, charge and ball-physics tuning were not altered by the cart integration.

Blender topology/delivery LOD checks, seated pose and close/distant player renders passed. The final path-only FBX correction was checked with typed format-aware comparison; non-path mesh/texture payloads are preserved. Relocated base art dependencies and font generation pass from a path containing spaces and an unrelated working directory. Full source masters remain editable outside Unity. The eight Chinese glyphs, renamed subset family and shipped OFL licence were verified.

The prescribed main AndroidSubmission attempt exited on the open project; the same script and target succeeded in the isolated checkout. The final **88,756,434-byte** release APK, complete audit and separately verified v2 signature are retained as `Builds/GolfCartQA/ReleaseBase/`. See [size and archive records](BUILD-SIZE.md#golf-cart-summon-and-driving-snapshot--3-october-2026). This snapshot predates concurrent course refinement and protocol 19 speed/wheel work; it does not certify those later source edits or shared-root APK freshness. Phone touch, phone quality/FPS and WAN latency remain unverified. No Git staging, commit or publishing was performed for this task.

The final `Check-TaskReady.ps1 -RequireApk` check passes **3,324 publishable files / 452 LFS paths**. Documentation links resolve, `git diff --stat` and `git status --short` were reviewed, and unrelated course/speed/wheel changes remain in place. This hygiene check reports the shared-root APK's size only; the immutable base artifact and separate signature/audit evidence establish this task's measured release. Readiness evidence is `Builds/GolfCartQA/readiness-final.txt`.

## Five-hole golf map — 3 October 2026

The map adds five independent recessed cups, orange numbered flags, putting greens and tee markers following the entrance/left bunker/central fairway/coast/raised-green layout. Existing teal flags remain scenery. The streamed scene and environment prefab both retain the course, and the refined-island generator places it on regeneration. Golf shot controls, scoring, shared travel and room protocol are unchanged. See [course authoring](GOLF-COURSE.md).

Unity 6000.3.20f1 passed **160 editor assertions** across the actual scene, environment prefab and repeated generation: five unique flags/tees, sixteen retained terrain sectors, open terrain at each cup, recessed floors, inner-wall collisions, nearby solid ground, upward green normals, terrain clearance and complete material dependencies. Native mesh inspection confirms all **3,021 putting-surface triangles** face upward and retain complete circular areas. Original terrain/grass/structures FBXs and the Golf G2 Blender master exactly match Git HEAD.

The Windows build with Android geometry processors passed **142 golf assertions** with five real cups and cleared greens. Its 49 golf grass cells/147 meshes prepared and appeared in the running player. The final desktop player also passed **142 assertions**, including ten spawns, 48-direction shoreline containment, full fairway traversal, all bunker exits, camera behavior and Football/Basketball/Golf switching. Actual close/distant renders and five-hole views were inspected. The analytic grass-height function passed **13,122 independent samples**, with maximum error **1.7763568394002505e−15 m**. The test script ran from an unrelated temporary working directory against a player path with spaces.

Evidence and final source hashes: `Builds/GolfFiveHolesQA/`; desktop run: `Builds/GolfG2QA-20261003-171939/`; mobile-geometry run: `Builds/GolfG2QA-20261003-165611/`. The mobile geometry/collision run predates the material-only removal of an unused turf mask; final editor/desktop rendering and the Android audit cover that removal. All authored release inputs match the workspace, apart from the isolated editor's Android clean-cache option. Complete-source AndroidSubmission and APK v2 signing passed with **86,896,050 bytes**. See [size and archive provenance](BUILD-SIZE.md#five-hole-golf-map--3-october-2026). No physical-phone rendering/FPS, touch, golf scoring or shot-controller validation is claimed.

## Football animation rebuild — 4 October 2026

Rebuilt football presentation on the existing Rainbow Sprinter model: one distance-driven gait, coordinated shoulders/elbows/wrists, world-space support feet, gradual starts/stops/turns, swing-phase dribble contact, complete-pose transitions, jump/landing, directional falls and interrupted recovery. The 28 saved records are compact posture/action timelines combined with procedural motion, not 28 imported mocap clips. [Implementation and reproduction](FOOTBALL-ANIMATION.md).

The final checked source passes **2,116 assertions** across the following suites. Repeated rates and scenarios are included in that total; it is not a count of distinct animations or a quality score.

| Suite | Passing assertions |
| --- | ---: |
| Motion at 20 / 30 / 60 / 120 timeline FPS, both LODs | 292 / 292 / 296 / 292 (1,172) |
| Rendered football match, physics and host/guest replication | 532 |
| Shared jump gameplay and loopback | 140 |
| Shared tackle gameplay and loopback | 210 |
| Final shared turning gameplay and loopback | 62 |

Across the four motion runs, maximum high-weight dribble contact error is **2.13 cm**, maximum sampled rendered floor penetration is **1.35 cm**, and maximum live local joint speed is **2,637.72 degrees/second**, below the unchanged 2,700-degree regression ceiling. Walking records **17 / 28 / 59 / 123** consecutive support samples with zero horizontal displacement at the trace's recorded precision. Low-rate sampling can miss the short sprint stance; zero slip without support samples is not evidence of a good plant. Bone-length error remains below 0.1 mm. Ten athletes allocate **zero steady-frame pose bytes**; measured Windows pose work peaks at **0.711–0.809 ms**, excluding Animator, physics and rendering. This is not phone FPS.

Actual Unity renders were inspected for locomotion, dribbling, cuts, stops, jumps, slides, directional falls and recovery, plus the two LOD pose sheets. The final video is **800 × 688, 60 FPS, 86.4 seconds**, with normal and half-speed playback and original shared clips for comparison. The exporter now retains chronological repeated stages and uses the recorded capture rate. Current footage, sheets and trace: `Builds/FootballMotionQA/Run-20261004-224406-424/`. Other final rates are indexed by `Builds/FootballMotionPolishQA/final-runs.json`; numeric summaries are in `motion-results-summary.json` and the run-local `continuity.json` files. The playable Windows build remains under `Builds/WindowsFinal/`.

Gameplay evidence: football `Builds/FootballMatchQA/Run-20261004-224456/`, jump `Builds/JumpQA/Run-20261004-224456/`, tackle `Builds/TackleQA/Run-20261004-224611/`, and final turning `Builds/TurnQA/Run-20261004-225849/`. The first turning run failed two Golf LOD1 cases after accumulated route travel; a previous-player control and isolated rerun passed those cases. The rerun exposed a separate obsolete base-clip assertion during a football hand-off. The development-only turn probe now samples the final displayed pose after animation evaluation, checks the football rig/weight/finite feet or an active/incoming shared clip, and repeats the same starting route for each LOD. Travel, speed, state-change, first-person and replication assertions remain. The corrected final suite passes; initial reports and comparison evidence are retained.

Both Python review helpers passed with default root/run selection from a copied checkout path containing spaces and an unrelated working directory. No character mesh, rig, texture, material, imported clip or package changed. The new `FootballGait.cs` file has Unity metadata; the pose resource is retained as active source. The 32 recorded football source/resource inputs match the tested Windows motion and Android build; the later turn-probe edit is wholly excluded from nondevelopment players. Its hash is recorded separately. Portable path and task-readiness checks passed without staging or committing files.

AndroidSubmission, the strict size gate and APK v2 signing passed: **86,764,912 → 86,774,768 bytes (+9,856)**, leaving **13,225,232 bytes** hard-limit headroom. The **75 MB development target remains unmet**. The preserved APK is `Builds/FootballMotionPolishQA/Android/WhatTheFish-release.apk`; [size audit and exact build scope](BUILD-SIZE.md#football-animation-rebuild--4-october-2026) record its hash and contributors.

This is a stylized short-legged character at unchanged gameplay speeds. Numerical gates and frame inspection do not certify human biomechanics, every self-intersection or a subjective "top-class" rating. No Android device was connected, so physical-phone appearance, touch, sustained FPS, thermals and WAN playback remain unverified.

Reviewed superseded captures, failed iterations/logs and the completed portability fixture were archived with hashes and recovery paths in `Legacy/20261004-225208-764-football-motion-polish-iterations/`. Current review evidence, player/APK, active source, authoring masters and Unity caches remain. No Git staging, commit, push or history rewrite was performed.

## Transparent lagoon and three fish sizes — 3 October 2026

Implemented transparent moving lagoon water, a sandy basin with animated caustics, and one gold-and-teal fish in **0.4 / 0.9 / 1.8 m** variants. All sizes share the same meshes/material; the two mobile LODs contain **3,192 / 1,248 triangles**, with the **12,792-triangle editable master** retained outside Unity. Casting, catching, swimming AI and scoring are deferred. [Design and rebuild instructions](FISHING-PRESENTATION.md).

The final Windows player passes **30 rendered presentation checks**: fish lengths, shared resources and vertex colours, supported shaders, retained five fishing stands, no new colliders, one active island, three gameplay camera poses, fish visibility through water, motion at a fixed camera, and ocean-cutout cleanup/restoration across island reload. Pier, underwater, overhead, shoreline/inlet, actual game-camera and size-lineup renders were inspected. Fish sit beside the first pier; railings can occlude individual fish from some camera angles. The first-person pier view clearly shows the size gaps. Current evidence: `Builds/FishingWaterQA/Review-Delivery/`, `lagoon-final-preview.mp4`, and `Player/WhatTheFish.exe`. Raw video frames were archived after encoding the final preview.

The older fishing-route probe was corrected to wait for streaming completion, count active islands rather than retained inactive scene entries, and reacquire the lagoon after reload. It passes **50 assertions with 3 failures**: `SPAWN_FLOOR_3`, `SPAWN_FLOOR_4`, and `FULL_COASTAL_LOOP_AND_BRIDGE`. An explicit `-WithoutPresentation` control run produces the same failures and stuck coordinates **(35.87, 1.33, 2.87)**. These existing route/collision issues remain unresolved; no assertion or shoreline collider was weakened to hide them. Both reports are retained in `Builds/FishingWaterQA/RouteChecks/`.

Blender regeneration also passed from a copied checkout path containing spaces, invoked from an unrelated system temporary working directory. Reopening the master and parsing FBX dependency elements found no external image, linked-library or active file dependencies. Generator outputs, `.meta` files and integrity hashes are retained. Saved-file/exporter provenance is nonfunctional metadata; binary bytes were not blindly replaced. New Markdown links resolve within the repository. The final fishing runtime/editor/shader/mesh/material/prefab inputs match the isolated build; source hashes are in `Builds/FishingWaterQA/final-source-hashes.json`.

The review script also repeated **all 30 checks successfully** from a copied checkout path containing spaces, invoked from an unrelated system temporary directory with both player/output defaults. Its report/log are in `Builds/FishingWaterQA/PortableVerification/`. Eight new relative documentation links and both authoring/delivery integrity hashes pass. `Check-TaskReady.ps1 -RequireApk` passes, including portable text paths, metadata, LFS and publishable-file hygiene; it inspected the retained concurrent root APK separately from this task's measured package.

The first main-project Android build failed amid concurrent football source edits. Frozen-checkout AndroidSubmission, strict budget enforcement and v2 signature verification then passed: **86,757,976 bytes**, **13,242,024 bytes** hard-limit headroom. The task APK/audit is retained separately; the concurrently produced root APK was preserved. The **75 MB development target remains unmet**. See [exact baseline/delta, SHA-256 and build scope](BUILD-SIZE.md#transparent-lagoon-and-three-fish-sizes--3-october-2026). This does not validate later football edits. Physical-phone appearance, touch, sustained FPS and thermals remain unverified.

Reviewed superseded previews, intermediate packages/audits and completed portability fixtures were archived with hashes/recovery paths in `Legacy/20261003-230218-819-fishing-presentation-final-cleanup/` and `Legacy/20261003-230828-261-fishing-portable-validation/`. Earlier task batches are `Legacy/20261003-221450-901-fishing-water-baseline/`, `Legacy/20261003-223827-819-fishing-water-failed-audit/`, `Legacy/20261003-224342-327-fishing-water-shared-sand/`, and `Legacy/20261003-225548-346-fishing-water-sand-final/`. Current evidence, active art and Unity caches remain. No staging, commit, push or history rewrite was performed.

## Position-aware goal net — 3 October 2026

Both goals deform at the authoritative ball's actual contact location, using incoming direction and normal impact speed. Back, sides and sloping roof share a continuous response with fixed frame/ground attachments. Scoring keeps the approved nominal 0.65-second ball follow-through before kickoff; a golden goal decides the result immediately and then freezes at the net. See [implementation, limits and reproduction](FOOTBALL-PROTOTYPE.md#position-aware-goal-net--3-october-2026).

The final source passes **1,558 assertions** across two rendered net runs and the rendered football suite:

- **513 net checks**, repeated as **513 copied-path checks**: 224 sampled panel locations across both goals, 12 real PhysX shot cases, varied speeds/directions, high/low impacts, roof/back continuity, pinned supports, bounded overlapping waves, rest/separating rejection, deduplication, invalid event rejection, goal movement and reactivation cleanup. Six 36-frame physical-shot sequences were rendered and visually inspected; `Builds/FootballNetQA/goal-net-impact.gif` and `.mp4` show the localized deformation.
- **532 football checks**: offline matches **259**, match host **16**, match guest **15**, physics/possession/fake shots **199**, network ball host **26**, network ball guest **17**. These verify net contact before reset, paused match clock, no duplicate scoring/new kicks, immediate golden-goal result, post-impact freeze, host/guest impact agreement and rejection of guest-authored collisions, plus retained gameplay coverage.

Initial collision traces exposed an inverted collider-relative velocity vector; the callback now converts it to the ball's incoming direction. Actual contact positions/directions pass after correction. Development-only fixture repairs read the displayed cancel-rectangle position after layout, wait for streamed sport activation rather than a fixed one-second delay, and allow numeric tolerance for the kickoff deadline. A headless shader-property run is not rendering qualification; final gameplay checks ran with graphics enabled. These fixture edits do not change the release game. The seven relevant runtime/material/shader files match the inputs verified before the Android build; hashes and comparison are retained in the task evidence.

Current net evidence: `Builds/FootballNetQA/Run-20261003-223537/` and `PortableVerification/Net/`. Complete final gameplay/loopback evidence: `Builds/FootballNetQA/PortableVerification/Match/` (original run `Run-20261003-225034`). The repeat used a copied checkout path containing spaces, invoked from an unrelated system working directory. The current review player is `Builds/FootballNetQA/Player/WhatTheFish.exe`; `ProjectBuilder.BuildFootballNetReview` rebuilds it with fast development compression and the configured delivery render pipeline. A concurrent shared Windows build supplied the earlier desktop preview; later dedicated builds verified the final fixtures.

Main-project AndroidSubmission and APK v2 signature verification passed: **86,574,958 → 87,028,060 bytes**, leaving **12,971,940 bytes** below the hard limit. The 75 MB development target remains unmet. The exact tested release is retained at `Builds/FootballNetQA/Android/WhatTheFish-release.apk`, with matching logs, baseline, populated packing audit and ZIP/asset comparison. See [size provenance](BUILD-SIZE.md#position-aware-goal-net--3-october-2026). No phone was connected; physical-device rendering/FPS/touch and WAN latency remain unverified. This is visual net deformation over existing rigid collision surfaces, not soft-body ball/net physics.

Superseded task players, failed preview iterations and completed portable fixtures were archived with hashes/recovery paths in `Legacy/20261003-223749-588-football-net-iterations/` and `Legacy/20261003-225549-095-football-net-validation-cleanup/`. Current evidence, editable source and Unity caches remain in place. No Git staging, commit, push or history rewrite was performed by this task.

## Football character motion — 3 October 2026

Implemented the [complete current-control animation set](FOOTBALL-ANIMATION.md): 26 saved control-space takes, existing run/jump/slide clips, bounded limb IK, actual shoe-to-ball placement, directional fall selection and synchronized charge/kick/cancel presentation. Protocol **18** requires matching players. Movement, possession, kick release, tackle restrictions and hit/miss deadlines retain their authoritative rules.

The Windows development builds pass **1,807 assertions** across the following suites. This total includes repetitions at the four timeline rates; it is not a count of distinct scenarios.

| Suite | Passing assertions |
| --- | ---: |
| Football motion at 20/30/60/120 timeline FPS, including both LODs | 748 (187 each) |
| Football match, physics and host/guest ball/presentation replication | 530 |
| Shared jumping, offline and host/guest | 140 |
| Shared tackling, offline and host/guest, including fall variants | 210 |
| Rapid turning, offline and host/guest | 62 |
| Idle visibility, interruptions and host/guest playback | 61 |
| Basketball motion regression | 56 |

Every saved take is sampled at 31 normalized times on both LODs. Maximum measured limb-length drift is 0.001 mm; maximum reach clamping is about 2 cm. Intended shoe-to-ball-surface contact errors peak at **3.43 cm across rates** (30-rate control/fast/cut: **2.2/3.1/1.8 cm**). Maximum sampled skinned-surface floor penetration is **1.3 cm**. These slightly exceed the original design's 3 cm/1 cm targets and are disclosed rather than relabelled as exact contact. The large shoes, short legs and large head retain their original proportions. Front/side pose sheets and ordered transition frames were inspected; the actual-player video includes normal and half-speed playback.

The ten-athlete pose-layer workload preserves rig lengths and reports **zero steady per-frame managed allocations** at all four rates. Its maximum summed pose cost was **0.507 ms** in the 30-rate Windows capture, with **3.127 ms** the highest maximum among the four runs. This measures only the pose layer, excluding Animator, rendering and much of gameplay; it is not phone FPS or a whole-match performance qualification.

Current motion evidence is in `Builds/FootballMotionQA/Run-20261003-223534-834/` (video, sheets, PNG frames, metrics and results), `Run-20261003-223955-854/` (20), `Run-20261003-224057-732/` (120) and `PortableVerification/` (60). The copied test script resolved its default player/output paths from a `Portable Checkout` directory containing spaces while invoked from an unrelated system temporary directory. The fixture was archived after keeping its reports. `test-summary.json` records all regression report paths. Football match/ball evidence: `Builds/FootballMatchQA/Run-20261003-224838/`.

Earlier attempts found and corrected a scaled-mesh measurement error, real fall ground clipping, excessive get-up reach and pose-layer allocations. An idle regression ran into an obstruction in the final enumerated fishing venue; its locomotion phase now runs on the open football pitch, and all 61 checks pass. A rendered goal-reset assertion exposed deadline rounding; diagnostics were retained and the concurrent net review adds a small comparison tolerance. The unchanged runtime match passed both headless and rendered suites. These fixes do not relax animation input timing or gameplay rules.

The prescribed AndroidSubmission build, measured bytes, signature and packing evidence are recorded in [BUILD-SIZE.md](BUILD-SIZE.md#football-character-motion--3-october-2026) and [APK-SIZE-AUDIT.md](APK-SIZE-AUDIT.md#football-character-motion--3-october-2026). Current task-specific APK/audit copies are retained under `Builds/FootballMotionQA/` to preserve provenance when other chats rebuild the shared output. The review code is excluded from release. No source Blender/FBX, binary art dependency, package or character texture was changed for this animation pass. Physical-phone touch, rendering, sustained performance, thermals and WAN behavior remain unverified.

Superseded fitting runs and the completed portable fixture were archived in `Legacy/20261003-225412-407-football-motion-cleanup/`. The first successful APK was superseded by a build using a concurrent material-sharing correction and is recoverable in `Legacy/20261003-225615-892-football-motion-first-apk/`. Superseded baseline/audit copies are in `Legacy/20261003-230020-132-football-motion-release-cleanup/`; their bytes and hashes remain in the current task report. No active art, Unity cache or installed dependency was archived. Staging, commit and push remain with the user.

## Interactive football boundary and twin-rail aim — 3 October 2026

Inspected/fetched `origin/main` at `0d314fd`; the checkout already contained the latest merge. The existing authored pitch containment and goal openings drive the new visual boundary. Ball physics, possession, kicks, match rules and networking remain unchanged. [Implementation and tuning](FOOTBALL-PROTOTYPE.md#animated-boundary-and-charging-feedback--3-october-2026).

The rebuilt Windows player passes **559 assertions**: 241 offline match checks, 28 host/guest match checks, 199 football physics/input checks, 41 host/guest ball checks, 25 focused feedback checks and 25 copied-player portability repeats. The focused checks cover both open goal mouths, exact boundary alignment, absence of new colliders, a real free-ball impact ripple, no repeated ripple from a resting ball, both compiled shaders, monotonic charge growth, maximum charge hold, all three camera modes, cancellation, release and mesh reuse. The last shader-only refinement turns the interior chevrons toward the arrow tip; the final capture run repeats all 25 feedback checks.

Close wall, impact sequence, distant pitch, goal-opening, charge sequence and first/third-person/overhead camera renders were inspected. The final shader build log contains no shader compilation errors or warnings. Camera captures render actual gameplay camera poses into a render target; they do not include the screen-space HUD. The review harness uses the same completion-marker/process-lifetime convention as the other repository suites. Earlier forced-exit review experiments encountered a native shutdown error after completing their assertions; those experimental captures/logs are archived and are not a graceful-exit qualification.

Current evidence: `Builds/FootballFeedbackQA/Run-20261003-214141/`, `build-final-windows.log`, `football-feedback.mp4`, `football-feedback.gif` and `PortableVerification/`; gameplay/loopback evidence: `Builds/FootballMatchQA/Run-20261003-213934/`. A copied player and copied test script ran successfully from a `Portable Checkout` path containing spaces, invoked from an unrelated system working directory with the default player/output resolution. The fixture was archived after retaining its reports.

No source model, Blender image/library link or FBX dependency changed. New delivery assets are text shaders/materials with relative Unity GUID references; generated geometry is runtime-only. The review implementation is excluded from release. Windows renders do not qualify physical-phone touch, Android appearance/FPS, thermals or WAN latency. APK bytes and audit results are recorded in [BUILD-SIZE.md](BUILD-SIZE.md) and [APK-SIZE-AUDIT.md](APK-SIZE-AUDIT.md).

Reviewed superseded outputs are recoverable in these ignored archive batches: `Legacy/20261003-212342-227-football-feedback-preview/`, `Legacy/20261003-212431-424-football-feedback-baseline/`, `Legacy/20261003-213433-994-football-feedback-design-revision/`, `Legacy/20261003-213735-250-football-feedback-shader-check/` and `Legacy/20261003-214348-184-football-feedback-final-cleanup/`. No active art, Unity cache or installed dependency was archived. Git staging/commit/push remain with the user.

## GitHub integration release — 3 October 2026

The merge combines local football revision `571cacc` with upstream basketball revision `c9810c4`. Football match/team state, possession, fake-shot cancellation, charging aim, tackle contact and goal-net collision are retained. Basketball dribble/shoot/pass poses, reliable pass requests and shared jump momentum are retained. Protocol **17** consistently covers both replicated layouts in the runtime service and Bootstrap scene. The obsolete football `blockedPusher` state and `Push` collision path were not reintroduced.

Unity 6000.3.20f1 rebuilt the Windows development player in the ignored isolated checkout. All **112 C# input hashes** match the workspace, apart from the separately documented editor build option used for Android clean-cache reporting. The combined player passed **1,061 assertions, with no failures or runtime exceptions**:

| Suite | Passing assertions |
| --- | ---: |
| Football match, physics and host/client ball replication | 509 |
| Basketball motion at 30 FPS, including both character LODs | 56 |
| Basketball two-player gameplay and passing | 30 |
| Shared jump/momentum, offline and host/client | 140 |
| Shared tackle, offline and host/client | 208 |
| Shared turning, offline and host/client | 62 |
| Basketball script from a path with spaces and an unrelated temporary working directory | 56 |

Test summaries, input manifests and build diagnostics are in `Builds/GitMergeQA/`. Football evidence is in `Builds/FootballMatchQA/Run-20261003-150002/`; the other suite paths are recorded in `test-summary.csv`. Selected basketball front/side frames for both LODs and the football restart view were inspected. Synthetic gameplay/control checks and Windows captures do not qualify physical-phone touch, rendering/FPS or WAN latency.

The prescribed main-project AndroidSubmission attempt exited while the user's editor was running. The same script/target succeeded in the isolated project with clean-cache packing evidence and a v2-signed **86,563,288-byte** release APK. Its source manifest remains identical to the tested player. Size, headroom, growth analysis and archive batches are in [the current release record](BUILD-SIZE.md#github-integration-release--3-october-2026). Git LFS integrity and indexed portable-path checks passed; final repository readiness is checked before the user-authorized commit and push.

## Football legacy-state cleanup — 3 October 2026

The unused `blockedPusher` field, assignments/resets and fixed-update clearing branch are removed. A full runtime C# compilation passes with only the existing review-script camera-name warnings. Active source/scripts contain no remaining references. An exact comparison against the pre-task source confirms that only this unused state and its cleanup are deleted: interception, kick eligibility, waking the ball, tuning, fake shots and `mustApproach` retain their existing code. No new gameplay or phone test suite is claimed for this cleanup; the gameplay snapshot below predates it.

The prescribed AndroidSubmission build succeeds. Its incremental packing report has no asset offsets, so a clean-cache build in the existing ignored isolated checkout supplies complete current packing evidence. Runtime source hashes match the workspace; only the previously documented isolated editor output/clean-cache options differ. Logs, the source manifest, narrow source diff, ZIP deltas and signature result are in `Builds/FootballLegacyCleanupQA/`. See [the measured release record](BUILD-SIZE.md#football-legacy-state-cleanup--3-october-2026).

The pre-task package/audit/source backup and superseded incremental output are recoverable from `Legacy/20261003-143827-669-football-legacy-state/`.

## Goal nets block athletes — 3 October 2026

The current-source Unity 6000.3.20f1 Windows player passes **509 assertions**: **199** football physics/control checks, **241** offline match checks, **28** host/client match checks and **41** host/client ball checks. This includes **57 new offline net assertions** covering both goals' four solid world-layer panels, walking/sprinting/sliding against left/right/back nets from inside and outside, open-mouth entry/exit and jumps into the sloping roof. Existing tests still verify football entry/retention/return through both goals, all four unladen pitch exits, attached possession, kick/slide physics, visible restart reset, charge cancellation, goal rules, selection, overtime and match results. Host tests additionally simulate a remote athlete sprinting into all six side/back faces and both peers verify its replicated stop pose. Actual current running/kick measurements remain **4.000/7.000 m/s unladen walk/run**, **5.250 m/s controlled run**, **12.003/22.001 m/s light/full kicks** and **8.000 m/s slide ball speed**.

Evidence: `Builds/FootballGoalNetQA/` contains build logs, physics/ball reports, signatures, source/asset hash manifests, ZIP deltas and a summary. Match reports are in `Builds/FootballMatchQA/Run-20261003-130119/`. An independent ignored checkout with a path containing spaces supplied the verification player and clean-cache AndroidSubmission build. Runtime source hashes still match the workspace; the only editor-source exception is its isolated output/clean-cache build options. Two ignored Performance Test resource metadata GUIDs are regenerated by Unity; publishable assets otherwise match. No Computer Use, Git staging or publishing was used. Windows/loopback checks do not qualify phone touch/FPS or WAN behavior. The current APK and recovery batch are recorded in [BUILD-SIZE.md](BUILD-SIZE.md).

## Football fake shot — 3 October 2026

The rebuilt Windows player passes **142 offline football assertions** and **31 host/client loopback assertions**. An additional **19 gesture checks** compile the actual `KickButton` with the extracted `PlayerView` charge methods against Unity API stubs. Current evidence and the source hash manifest are in `Builds/FootballFakeShotQA/`. The isolated build uses the current runtime source, including the rounded arrow and 1.8x charge scaling; the earlier snapshot scope below remains historical.

The real Unity integration probe injects pointer events into Kick and the left joystick. It verifies same-finger cancellation, exclusion of other fingers, immediate aim hiding, retained controller/kinematic possession without releasing the ball, turning/dribbling after cancellation, no delayed kick on release, no restart by sliding back, a fresh press, normal light/full release commands and cancellation when the release lands inside the cancel region. Existing physics checks still measure approximately **12.003 m/s light**, **22.001 m/s full** and **8.000 m/s tackle**. No ball force, speed, charge curve or physics setting changed for this feature. Loopback checks cover existing authoritative ball behavior; physical phone multi-touch, phone quality/FPS and WAN remain unverified.

The original `-nographics` offline run stopped at the existing shader-material assertion. A run with Direct3D12 passed white/50% alpha, transparent surface, depth-write disabled and queue 3000, along with all gameplay assertions. The offline test runner now retains its graphics device in batch mode; network-only probes still use `-nographics`. The final scripted offline run passed. Rendered captures are also kept in the task evidence folder.

The main project was open in Unity, so builds used the existing ignored isolated project without closing the user's editor. Source hashes match the workspace. Superseded isolated APK/audit output was archived in `Legacy/20261003-002925-125-fake-shot-isolated-baseline/`; the failed Null-graphics evidence is recoverable from `Legacy/20261003-003427-835-fake-shot-null-graphics/`. See [the build-size record](BUILD-SIZE.md) for the Android outcome.

## Team A lettering — 3 October 2026

The top football Team A label now uses overlapping vector capitals with an orange gradient, coral outline and red marks matching the supplied sketch. Unity 6000.3.20f1 compiled the updated source and rendered the real Canvas at 1600 x 900 and 844 x 390, including team selection and enlarged details. The inspected previews preserve A counters, left-to-right overlap, clear score separation and the existing controls. Rendering ran from the ignored isolated checkout whose path contains spaces, with outputs resolved from `Application.dataPath` rather than the caller's working directory. The user's open editor was left running.

Evidence: `Builds/FootballTeamAWordmarkQA/Visuals/`, `wordmark-capture.log` and `validated-inputs.csv`. This UI change adds no gameplay logic or imported art. Actual Android packaging is recorded in [BUILD-SIZE.md](BUILD-SIZE.md); Windows editor renders do not validate physical-phone touch, rendering/FPS or the other working-tree gameplay changes. Existing gameplay reports below retain their original source scope.

The required main AndroidSubmission script attempt exited on the open-project lock. A concurrent successful isolated AndroidSubmission build included all final UI inputs (110 matching C# hashes); its 86,527,020-byte APK and complete audit were retained with explicit external provenance under `ExternalBuild/`. The newer current goal-net release is 86,530,508 bytes; its HUD/wordmark hashes match the reviewed UI and its APK v2 signature was independently rechecked. Measurement, signature and contribution comparison evidence is in `Builds/FootballTeamAWordmarkQA/verification.json`, `package-comparison.json`, `apk-entry-deltas.csv`, `asset-estimate-deltas.csv` and both signature logs. These measurements include other working-tree changes.

First-pass previews were archived with hashes and recovery paths to `Legacy/20261003-003046-063-football-team-a-first-review/`; the transient failed editor-startup log is in `Legacy/20261003-003137-731-football-team-a-capture-startup/`. Final previews, measured artifacts and current evidence remain under Builds. No staging, commit, push or history change was performed.

## Attached football possession — 2 October 2026

**Snapshot scope:** this record validates attached possession and the transparent 0.8–1.44 m outline aim. Other tasks subsequently changed the filled arrow and kickoff formation while these checks/builds ran. Those edits are retained, but are not included in this tested player/APK; full current-working-tree compilation, runtime checks and Android freshness are unverified by this record. `Builds/FootballControlQA/verified-runtime-source-manifest.csv` records the actual isolated source that produced the artifacts. The measured APK is [documented separately](BUILD-SIZE.md#validated-attached-control-snapshot--2-october-2026).

The rebuilt Windows player passed **323 match/physics/loopback assertions** after the latest transparent charge-aim/1.44 m length updates. Its unchanged shared movement source also passed **292 regression assertions (615 total, no failures)**. Current football evidence: `Builds/FootballMatchQA/Run-20261002-234406/` and `Builds/FootballControlQA/suite-current.txt`; shared movement evidence: `Builds/FootballControlQA/MovementVerified/`. The preceding 321-check run is retained under `Builds/FootballMatchQA/Run-20261002-232617/`. Unity 6000.3.20f1 built this combined working tree in the ignored `Builds/FootballControlQA/Isolated Workspace/` project, whose path contains spaces, without operating or closing the user's editor. No Computer Use was used.

Possession checks cover exclusive kinematic attachment, unchanged unladen speeds, the user-approved **25%** possession slowdown, no compounded penalty, blocked jump/slide requests, moving and stationary 180°/90°/270° turns, separate charging speed, light/full kick release, spatial re-approach, immediate reception by another eligible athlete, successful versus missed/ball-only tackles, duplicate contact suppression, field limits, reset, despawn and travel. Loopback host/client checks verify ownership, exact replicated foot anchoring after a 180° turn, actual tackle release, both goal areas and shared scene return. Only the host simulates free-ball physics.

| Measured or runtime-checked quantity | Result |
| --- | ---: |
| Unladen walk / sprint | 4.000 / 7.000 m/s |
| Attached walk / sprint | 3.000 / 5.250 m/s |
| Charging motor speed | at most 2.410 m/s; target 2.400 |
| Light / full release speed, including 0.25 m/s lift | 12.003 / 22.001 m/s |
| Tackled holder's ball horizontal release / lift | 8.000 / 4.000 m/s |
| Sampled maximum attached presentation gap | 0.0000 m |
| Client settled 180° turn foot gap | 0.0000 m |

The shared regression suite passed **34 jump, 202 tackle and 56 turning checks**, including all four environments, both character LODs, the full 0.8-second knockdown lock, mutual slide knockdown, airborne evasion, 40 simultaneous path victims without duplicate hits, and missed-slide recovery at 30/60/120 Hz. Both active athlete prefabs explicitly serialize the 4-second cooldown and 0.8-second recovery. The recovery tests check actual sprint travel near 2.8 m/s, followed by normal speed restoration.

Actual Windows RenderTexture captures of acquired/turned/kicked possession, charging aim and both goal restarts were inspected; the ball stays visible at restart and held at the feet after reversal. The first diagnostic runs exposed fixture issues: capture frames had already advanced a kicked ball outside its exclusion area, and a destroyed test wall persisted until frame end. These fixtures were corrected before the successful run. Physical Android touch, device rendering/FPS and WAN latency remain unverified. Fresh Android packaging evidence is recorded in [BUILD-SIZE.md](BUILD-SIZE.md).

Football kickoff, full-ball goal detection, score, restart, regulation deadlines and golden-goal overtime are described in [FOOTBALL-MATCH.md](FOOTBALL-MATCH.md). The preceding flow revision passed 294 rebuilt Windows offline/loopback/physics assertions and 40,011 pure-rules assertions: capacity-limited 10-second team selection, seamless overtime with live actions and ball momentum, and movement after the immutable final result. HUD selection/full-team, regulation, goal flash, overtime and finish captures were inspected. Physical Android and WAN checks remain pending.

The centre-spot football integration is recorded in [SOCCER-BALL.md](SOCCER-BALL.md), with scene placement, model/material audits and build status. Balanced dribbling, light/charged kicking, limited slide contacts, physical rolling and host-authoritative control/ball synchronization are described in [FOOTBALL-PROTOTYPE.md](FOOTBALL-PROTOTYPE.md).

The current golf milestone is documented in [GOLF-VERIFICATION.md](GOLF-VERIFICATION.md), including updated Windows/Android artifacts, room tests and remaining device checks. The preceding basketball milestone is recorded in [BASKETBALL-VERIFICATION.md](BASKETBALL-VERIFICATION.md).

The records below are the **earlier football baseline from 22 September 2026**. Its APK size/hash and statements that basketball is unavailable describe that earlier build, not the current version.

## Completed

- Android ARM64 IL2CPP development APK built successfully: **52,083,383 bytes (52.1 MB / 49.7 MiB)**, below the 100 MB file target. APK metadata confirmed minimum API 26 and target API 36. Android `apksigner verify` passed with a v2 signature. This is a historical build/package check, not a physical-device launch test; the current package ID and APK are documented in [BUILD-SIZE.md](BUILD-SIZE.md).
- APK SHA-256: `CC512A1431171CD4965C9EAFCD66BD8A315F87BAC0F3405F487D80F77C77F04E`.
- New general-audience Unity cloud project linked in saved project settings. Real anonymous authentication, session creation, session-code joining and Relay transport passed with two Windows processes. Movement, readiness, character appearances and changed stadium settings replicated; host start and return reached the guest. Evidence: `Builds/cloud-host2.txt`, `Builds/cloud-guest2.txt`.
- The reported camera conflict came from the diagnostic `-viewScreen` code overriding the view each frame. That override was removed. The rebuilt interactive player visibly switched from first-person to third-person using the Camera button and displayed 60 FPS on this laptop. Phone drag/multitouch remains unverified.
- Real cloud capacity/isolation test: Room A held ten players and rejected an eleventh with `lobby is full`; Room B simultaneously held two players with independent phase and movement. Joining Room B during exploration returned `lobby is locked`. A stale code returned `lobby not found`. Host departure in the two-client cloud test produced `connected=False`, `exploring=False` and the friendly connection-closed message. Evidence: `Builds/CloudQA-20260922-225455/` and `Builds/cloud-guest2.txt`. Reproduce using `Tools/Build/Test-CloudRooms.ps1`.

- Unity 6000.3.20f1 project imports and C# compilation pass. URP 17.3.0, Multiplayer Services 2.3.3 and NGO 2.13.3 compile together against the installed editor.
- Original stadium and athlete export from Blender 5.1.2. Stadium inspected in a Blender render from elevated and pitch-level views. Both assets imported into Unity and used in the Windows player.
- Stadium FBX reduced from approximately 12.1 MB to 1.93 MB by simplifying repeated seat geometry. Final editable sources remain outside Unity. No spectator NPCs or crowd audio added.
- Windows development player builds and launches. Visual inspection performed on the actual native game window: main menu, character customiser, third-person and elevated stadium views. All three camera positions also exercised by the offline probe.
- The football baseline Windows development build output was approximately 170.1 MB uncompressed. This includes the Windows engine/runtime and is not an Android APK size estimate.
- Offline movement probe moved the avatar 4.8 m; no internet/UGS initialisation was needed. Chibi Idle/Walk/Run clips imported and the runtime Animator evaluated. The on-screen FPS indicator displayed 30 FPS on this laptop; this is **not** a phone performance measurement.
- Character colour selections made in the customiser persisted across player launches. The changeable hair tuft and skin/hair/outfit preset controls are implemented.
- Two independent NGO/Transport loopback rooms ran simultaneously on ports 7777 and 7778. Room A contained ten players, Room B two. Their player counts and transforms remained separate.
- Room A rejected an eleventh process with `This room is full (10 players).`
- Server-owned spawn positions were distinct. Movement was simulated by the host and matched across client logs; character appearance IDs and ready state replicated.
- Host start changed every client's exploration phase. Host return changed every client back to the waiting phase. A later join attempt during exploration was rejected.
- Host process exit disconnected guests and returned the application from exploration to menu state (`connected=False`, `exploring=False`). A disconnect reason was available to the UI.
- A separate two-process test replicated a changed host stadium preset (`TEST 7780`, lilac palette, design 2, team welcome, flags off) to the guest during exploration. Evidence: `Builds/preset-host.txt` and `Builds/preset-client.txt`.

Evidence: `Builds/NetworkQA/*.txt` and `*.log`, `Builds/smoke.txt`, `Builds/scene-audit.txt`, build logs and Blender review PNGs. These generated evidence files are excluded from Git. The reproducible local room test is `Tools/Build/Test-LocalRooms.ps1`. Development probes run only with explicit `-probe` arguments.

## Still awaiting access / not passed

| Requirement | Status |
|---|---|
| Android development APK | Passed build, signature and package checks; physical launch pending. |
| Physical Android launch, touch/multitouch, camera clipping | Not tested on a phone. `adb devices` returned no device. Desktop checks do not prove these. |
| Sustained 30 FPS on Android and APK below 100 MB | APK size passes at 52.1 MB. Sustained Android FPS and thermal behaviour require a phone. |
| Internet sessions across different networks | Real cloud/Relay tests passed on this PC's connection. Two separate physical networks still need testing. A separate named development environment remains pending dashboard access; builds currently use the new prototype project's default environment. |
| Invalid cloud codes, cloud-full room errors, network outages | Stale-code, full-room, locked-room and host-disconnect paths passed via live services. Broader network outage/recovery testing remains pending. |
| Ten users through Relay and simultaneous cloud sessions | Passed using independent Windows processes on this PC. Different physical devices/networks remain pending. |
| Stadium preset replication over internet/Relay | Passed with two Windows processes using real Relay; separate physical networks remain pending. |
| Broad device/aspect-ratio coverage | Pending. UI uses landscape scaling and safe-area anchors; test phones/tablets before publishing. |
| Huawei achievements/results | Explicit placeholder only. HMS SDK registration/integration is a later task. |

## Known prototype limits

- No football ball, scoring, timers, bots, spectator population, basketball arena or golf island.
- No host migration, client prediction, reconnect/resume or dedicated servers.
- One static full-size stadium. Seats are consolidated by material, so editing individual seats requires Blender editing/regeneration rather than runtime object manipulation.
- Stadium signs use runtime world-space UI. Font legibility and face orientation should be reviewed at close range on the phone before release.
- The startup art uses solid colours, not image textures. It does not depend on an external asset pack.
- Native desktop drag automation delivered pointer-down but no subsequent drag events in the touch-pad trace. Actual multitouch dragging still needs testing on the phone; this check has not been counted as passed.
- Scene rebuild and art regeneration intentionally replace generated outputs; save manual variants separately.
