# Four-island activation presentation

## Fishing skills and golf events

Fishing uses a personal 2.6-second presentation for each accepted skill. A
named card slides in with the existing skill icon and a short explanation.
Steel Wire frames the reinforced line, Power Reel brings the rod closer,
Fishing Buddy frames the helper and spotlights the active skill UI, Bubble
Guard turns toward the protected fish, Sonar widens over the water, and School
Call looks toward the gathering fish. The camera eases back to its normal mode.
Other fishers keep their own cameras and gameplay continues.

Golf announces Tropical Wind, Sticky Green, Bounce Party, Tornado, Crab Crossing
and Final Rush with animated emblems, event names and a brief gameplay cue.
Walking players get a short fairway/hazard view. Aiming, charging, shot-follow
and spectator cameras retain control; the announcement remains visible.
Reduced motion retains the notices without camera travel. Changing camera mode,
opening a modal, leaving the island or ending the relevant action clears the
camera presentation. Gameplay durations, physics and network snapshots are
unchanged; existing host timestamps avoid replaying stale events.

`Tools/Build/Build-ActivityCinematicReview.ps1` builds a complete review player.
`Tools/Build/Test-ActivityCinematic.ps1 -Sport Fishing -Film` (or Golf, Football,
Basketball) records actual game-camera/HUD clips under
`Builds/ActivityCinematicsQA/`. `Tools/Build/encode_activity_cinematics.py --runs`
accepts the successful run folders and produces individual five-second clips,
four sport reels and a local video gallery. Capture uses 30 simulation frames
per second and is not a physical phone FPS test. These clips are silent.

## Four-island delivery verification — 24 October 2026

The extension passed **1,968 assertions** across 18 recorded activations,
three-peer football and basketball sessions, and four-sport identity/camera
coverage. The final complete Windows player reran **1,101 Fishing checks**
and **363 football multiplayer checks**. Other successful runs cover unchanged
feature code. Fishing checks include delayed future-timestamp activation and
stale-packet suppression; golf checks cover all five events, Final Rush, camera
modes, reduced motion, aiming priority, expiry and exit cleanup.

The [video gallery](../Builds/ActivityCinematicsQA/Videos/index.html) contains
18 individual five-second clips. The football and basketball reels each last
15 seconds; Fishing and Golf each last 30 seconds. All decoded frame counts
and durations were checked. Captures show the actual game camera and HUD,
including pre-use nameplate icons, smooth camera turns, post-focus direction,
Lock-on's aura and Buddy's transparent UI spotlight.

The final full Windows build completed at 19:32 MYT. All **334 delivered files**
were hash-checked after promotion to `Builds/WindowsFinal/`, including seven
retained relative-path trailer/highlight launchers. All **472 compiled source
documents** match the isolated source snapshot. The snapshot restores **470**
documents to their exact prior compiler hashes and adds the final timing fix
and its tests in two cinematic files. This isolates ongoing voice, reconnect
and invitation edits without changing them in the shared workspace. This
delivery uses **protocol 82**, and matching players are required. Those ongoing
features are outside this snapshot.

Evidence is under `Builds/ActivityCinematicsQA/`: `verification-runs.json`,
`source-snapshot.json`, `snapshot-baseline-proof.json`, `windows-sources.json`,
`windows-promotion.json`, `portable-check.json` and `Videos/`. Portable command
construction was checked from a copied script directory containing spaces and
an unrelated working directory. Android physical touch, quality/FPS and live
WAN behavior remain unverified.

The official AndroidSubmission APK is **113,789,392 bytes**, compared with
**113,758,996 bytes** at entry (**+30,396**), leaving **886,210,608 bytes** below
the strict cap. Its signature, ZIP CRC, 1,243-entry audit and five-scene bundle
integrity passed; **338** compiled Android source documents match the isolated
snapshot. See [build size](BUILD-SIZE.md) and [APK audit](APK-SIZE-AUDIT.md).

Recovery batches are under `Legacy/`: `20261024-191220-194-activity-cinematics-qa`,
`20261024-192631-808-activity-cinematics-snapshot`,
`20261024-193310-240-activity-cinematics-candidate`,
`20261024-193512-307-activity-cinematics-final-media`,
`20261024-193558-161-activity-cinematics-windows`, and
`20261024-194730-823-activity-cinematics-android`. They contain reviewed rehearsal
output, copied sources excluded from the isolated snapshot, and superseded
deliveries. Original shared-workspace source was preserved.

## Football and basketball props

Team Turbo and Portal Steal in both sports, and confirmed football Wall placement,
start a 2.6-second shared activation presentation. Gameplay continues: movement,
ball physics, match/shot clocks, ability deadlines and defensive actions are not
paused. Rejected requests and wall previews do not start a presentation.

Every active court/pitch player's camera turns smoothly toward the user. The
selected first-person, third-person or stadium mode is retained. After the
effect ends the camera continues facing the user until manual look, a camera
mode change, a newer activation or a context change releases it. The prop user's
own first-person camera briefly pulls back to show the effect, then returns to
their eyes facing forward. Spectators see the effects and inventory icons but
retain their camera controls.

The forced view turn is separate from gameplay heading. Stadium movement keeps
its pre-event input basis during the focus. Existing action restrictions still
apply, including Portal's short receiving lock. An activation never delays the
real ability: Turbo retains five seconds, Portal its 0.7-second transfer, and
Wall its 15-second lifetime. The newest accepted activation takes presentation
priority; no uses are queued or consumed by the camera system.

Green Turbo rings, blue Portal energy and orange Wall energy follow the user.
The ground pulse is continuous; rising rings fade across their wrap points.
A compact sports banner slides and fades in below the scoreboard with the prop
icon, uppercase prop name and player's name, then eases away. Turbo's arm pump,
Portal's reaching gesture and Wall's signal have a short windup, drive and smooth
settle. These gestures yield to ball-contact, airborne and defensive poses. Existing
portal transfer, team trails, sounds and deployed wall remain authoritative.
The effects use pooled geometry and the existing shared material and icon atlas.

Basketball Lock-on does not take over cameras. A golden floor ring, rising rings
and hollow aura identify the shooter during the actual one-second aim. They
clear on release or interruption. Other prop focus yields during Lock-on aiming
and its assisted flight so defenders keep their ordinary view and controls.

An unspent prop has a small matching icon beside the character's overhead name.
The icon changes on replacement and disappears on consumption or inventory
clear. It never represents an already-active boost or spent shot. Crowded labels
retain the icon beside the compact number; their collision bounds include it.
Spectators and other sports have no held-prop icon.

The host replicates activation type, actor, sequence and timestamp with the
existing sport snapshot. Stale/duplicate inventory requests retain the original
validation. Late clients use the remaining presentation time rather than
restarting old effects. Matching **protocol 82** players are required.

## Earlier court delivery verification

Build a complete candidate with `Tools/Build/Build-PropFocusReview.ps1`.
`Tools/Build/Test-PropFocus.ps1` accepts `-Sport Football` or `Basketball`,
`-Mode Offline` or `Local`, and `-SpectatorHost`. Tests drive actual owner requests,
inventory, three camera modes, continued movement/time, post-animation heading,
manual release and Lock-on warning cleanup. Captures and logs are kept under
ignored `Builds/PropFocusQA/`. Existing prop and identity/camera suites cover
counterplay, inventory retention and label placement regressions.

The 24 October 2026 delivery passed **1,477 assertions** across local multiplayer
football (including a spectator host), local multiplayer basketball, an additional
1280 x 800 football presentation run, the existing football/basketball ability
regressions, all four sports' identity/camera checks, and shared island rules.
Rendered captures were inspected for the ribbon, aura and held-prop labels.
The ability suites verified Lock-on interruption and Portal interception while
the new presentation was installed. Test fixtures wait for the actual sport
session and dismiss the shared tutorial before driving gameplay.

Run folders and counts are indexed in
`Builds/PropFocusQA/verification-runs.json`. The complete validated Windows player
is in `Builds/WindowsFinal/`; its file hashes were checked against the candidate,
and `LATEST-BUILD.txt` records the exact source/build scope. Matching protocol 82
players are required. Physical Android appearance/FPS/touch and live WAN sessions
have not been verified.

The official AndroidSubmission build measured **113,756,604 bytes**, compared with
the opening local APK's **113,344,522 bytes**: **+412,082 bytes**, leaving
**886,243,396 bytes** below the strict 1,000,000,000-byte boundary. Its signature,
ZIP CRC, 1,241-entry audit and five-scene catalog/bundle integrity passed. This
combined build includes shared UI/tutorial work; the size delta is not solely
the prop feature. Prop presentation adds no new texture or model assets.
See [build size](BUILD-SIZE.md) and [APK audit](APK-SIZE-AUDIT.md).

Compiled source verification matches the feature's runtime sources in both
deliveries. Later concurrent UI/tutorial and development fixture edits are
outside the respective built snapshots; `Builds/PropFocusQA/source-scope.json`
records the exact compared paths. These limits do not affect the prop runtime,
and unrelated working changes were preserved.

Previous local delivery recovery batches:
`Legacy/20261024-141036-440-prop-focus-android/` and
`Legacy/20261024-142638-674-prop-focus-windows/`. Superseded review runs are in
`Legacy/20261024-143255-034-prop-focus-qa/`; the archive records sizes, hashes and
recovery paths. The current successful runs remain under `Builds/`.
