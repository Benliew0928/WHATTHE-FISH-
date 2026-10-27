# Natural awards entrances and Golf event consistency

Updated 26 October 2026.

## Behavior and maintenance

- Golf first establishes the podium, then dissolves to the entrance at 2.5 seconds.
  The awards-only walk uses shoulder-driven, straight hanging arms and aligned
  wrists. Palms face the body and thumbs face forward throughout the stride;
  corrected whole-arm roll avoids an abrupt wrist twist. Celebration gestures
  and normal gameplay locomotion are unchanged.
- Fishing and Golf prepare their complete render-only cast outside the entrance
  shot. Everyone starts with fixed spacing; first place crosses into frame first.
  No character is activated later at a visible spawn point. Fishing follows the
  bridge; Golf follows the central axis toward the podium stairs over the existing ground.
- `FishingCeremony.Podium.cs` centralizes walking speed, queue gap, gait distance,
  approach direction, starting position, entrance framing and transition timing.
  No map, bridge mesh, ranking or real player position is changed.
- Tropical Wind uses a stable oblique shot. A subdued dashed normal trajectory
  and ground target contrast with a golden, visibly wind-bent flight. Terrain
  response still uses `GolfBallPhysicsSettings.LandingVelocity`; no live ball
  physics, scoring or network state is driven by the demonstration.
- The three example balls normalize the copied gameplay mesh in ball-local
  space to the actual 0.129 m diameter. Camera distance can change apparent size;
  no larger substitute ball is used. Sticky Green retains the game's brief
  impact squash. Bounce Party keeps
  a uniform sphere throughout its demonstration, including all contacts.
- The demonstration uses `GolfBallTrail` itself (one-second lifetime and normal
  width/gradient). `GolfBallEventFX.DrawAura`, `DrawContact` and `Squash` are shared
  with gameplay. `GolfWindWorld.DrawStreak` shares wind shape, colour and opacity.
  `GolfFeedback.DemonstrationLanding` uses the same pooled turf particles and
  sound without dispatching gameplay feedback/network events or updating counts.
- The demonstration outline faces the final render camera, including camera
  blending. Render callbacks and all temporary objects are removed on exit.
- Island Rush's driver root is raised 0.23 m relative to the existing seat
  placement. The cart, running cast, camera timing and gameplay are unchanged.
- No player-visible wording is added. Existing English, Simplified Chinese and
  Malay UI remains in use. Reduced Motion and interruption rules are retained.

## Awards opening duration and hand orientation — 26 October 2026

Golf's opening podium view now lasts 2.5 seconds. Fishing/Golf entrance walking
reverses the outward arm roll at shoulder and elbow together, retaining the
reference wrist relation and straight swinging arms. The simplified rig has
Hand/Middle4 landmarks, not separate index/pinky bones. Celebration poses are unchanged.

AndroidSubmission: **168,030,424 -> 168,037,240 bytes (+6,816)**;
headroom **831,962,760 bytes**. SHA-256:
**BDCB2DE441C8567028D06CC0DD1F7694C8F13DCA66B7CE9C666660A54B58C935**. APK v2 signature, ZIP CRC, all 1,405 audit entries
and five-scene bundle integrity pass. Largest compressed entries: island bundle
45,946,749; IL2CPP 13,256,662; Vivox 10,240,163; Unity 9,828,173 bytes.
The small bundle/compiled-code changes introduce no new art or quality reduction.

231 awards assertions across English, Simplified Chinese and Malay pass, with
16:9 and 4:3 captures reviewed; 3,489 localization entries pass. All 530 Windows
and 373 Android source hashes match. The complete 337-file Windows candidate was
copied and hash-verified. Evidence: `Builds/AwardsPalmQA/`.
Phone rendering/touch/FPS and live cloud multiplayer remain unverified.

Reviewed superseded releases and promoted candidate: `Legacy/20261026-223207-773-awards-palm-android/`, `Legacy/20261026-224640-194-awards-palm-windows/`, `Legacy/20261026-224647-044-awards-palm-candidate/`.

## Latest refinement verification — 26 October 2026

Golf again opens on the podium before the entrance cut. Both queues walk on the
central axis toward the stairs, with straight arms swinging from the shoulders.
Bounce Party's demonstration retains uniform spherical scale at every landing;
actual gameplay physics and celebration gestures are unchanged.

AndroidSubmission: **168,029,336 -> 168,030,424 bytes (+1,088)**;
headroom **831,969,576 bytes**. SHA-256:
**3FD05F6805269962448160AD9E371DA45B8A69BC6FACAB7A6742D494E3180B80**.
APK v2 signature, ZIP CRC, all 1,405 audit entries and five-scene bundle integrity
pass. Largest compressed entries: island bundle **45,940,329**, IL2CPP
**13,256,273**, Vivox **10,240,163**, Unity **9,828,173** bytes. Entry deltas were
reviewed; no new texture or quality reduction is involved.

**225 three-language awards checks** (including 4:3), **194 Golf cinematic
checks**, and 3,489 localization entries pass. All **530 Windows / 373 Android**
source hashes match. The full 335-file candidate copy is hash-verified; retained
preview launchers bring WindowsFinal to 337 files. Evidence and silent preview
videos: `Builds/AwardsWalkRefineQA/`; final events:
`Builds/ActivityCinematicsQA/Golf-20261026-220115-946/`.
See [presentation maintenance and archive record](NATURAL-ENTRANCES.md).
Physical-phone rendering/touch/FPS and live cloud multiplayer remain unverified.

Reviewed superseded releases and promoted candidate: `Legacy/20261026-220139-904-awards-walk-android/`, `Legacy/20261026-220334-553-awards-walk-windows/`, `Legacy/20261026-220554-693-awards-walk-candidate/`.

## Previous verification and delivery

- 188 Golf cinematic checks pass, including stable wind framing, real mesh
  diameter, shared trail settings, interruption, cleanup and unchanged gameplay.
- 32 Golf / 40 Fishing awards assertions per language pass in English, Simplified
  Chinese and Malay: 216 combined. Checks cover offscreen initial visibility,
  continuous spacing, complete queue framing, planted feet, ties, Crew, podium
  anchors, Reduced Motion and restored controls. 1280x720 and 960x720 captures
  were visually reviewed. Diagnostic `zh-CN` runs fell back to English; only the
  successful `zh-Hans` runs count as Chinese validation.
- 160 Golf audio/feedback checks and 3,489 localization catalog/font entries pass.
- Actual Sticky/Bounce landing effects pass in the supplemental old cup review.
  Its subsequent `slow ball visibly sinks before next tee` assertion fails on
  both the previous WindowsFinal player and this candidate. Both reports remain
  in `Builds/NaturalEntranceQA/`; this pre-existing failure is not counted as a
  passing suite, and cup physics was not changed.
- Source hashes: 530 Windows / 373 Android documents match. All 335 candidate
  files were hash-compared during promotion; two awards launchers are retained.
- AndroidSubmission is **168,029,336 bytes**, down 6,756 from **168,036,092**;
  **831,970,664 bytes** remain below the hard cap. Signature v2, CRC, 1,405 audited
  entries and five-scene bundle integrity pass. See [size audit](APK-SIZE-AUDIT.md).

Evidence: `Builds/NaturalEntranceQA/`, activity film run
`Builds/ActivityCinematicsQA/Golf-20261026-212655-965/`, and audio run
`Builds/GolfFishingAudioQA/Golf-20261026-212940-139/`. Silent 30 simulation-fps
videos of both ceremonies and six Golf events are under
`Builds/NaturalEntranceQA/Videos/`; they do not establish device performance.

The complete player is in `Builds/WindowsFinal/`; the existing awards preview
launchers remain. No Git staging, commit or push was performed. Phone rendering,
touch/FPS and cloud multiplayer have not been tested.


Superseded measured deliveries: `Legacy/20261026-212822-852-natural-entrance-android/`, `Legacy/20261026-213347-513-natural-entrance-windows/`.

The promoted duplicate review player was archived to
`Legacy/20261026-213645-238-natural-entrance-candidate/`. Current evidence and
preview videos remain under Builds. `Check-TaskReady.ps1 -RequireApk`, diff
whitespace checks and all 16 localization artwork integrity checks pass.
