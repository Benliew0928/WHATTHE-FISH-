# Open type player labels and activity cameras

The approved second design is shared by Football, Basketball, Golf and Fishing.
Your own overhead label reads **You**, with no name, number or profile portrait.
Other players have a small team-colored number tab and an outlined white name.
Blue tabs have rounded corners; Red tabs have squarer corners. Individual sports
use a neutral mint tab. Full profiles remain available by tapping peer labels or
opening the existing Players panel.

Labels use the animated head anchor and update after camera presentation. Views
remain bound to their athlete, so roster changes cannot exchange label owners.
Names have a bounded width and truncate on Unicode grapheme boundaries. Crowded
labels shorten to the number, use a short leader with at most 12 logical pixels
of horizontal correction and 18 pixels of lift, or hide when space is unavailable.
Brief compact-state retention and stable slot selection reduce jumping. The
safe area, scoreboards, active HUD controls and Fishing aim point are reserved.
World occlusion, travel and first-person rules still suppress overhead labels.
Chat bubbles reserve the label rectangles. Local Football stamina stays on the
Dash control; peer overhead meters appear only during exertion/recovery.

## Camera choices

The third camera option in Golf settings is now **Course**, and in Fishing it is
**Lagoon**. Eye level and Follow remain available. Fishing's camera shortcut now
cycles through all three choices; Fishing still restores the previous sport's
camera preference on departure.

- **Golf:** a 28–42 degree roaming angle replaces the generic 58 degree view.
  Aim frames the golfer, ball and current landing guide; putting uses a closer
  composition. Accepted ball contact starts the existing flight sequence, with
  a wider view and terrain context in Course mode. Skip/settling restores Course.
  The buggy's third camera uses a 32 degree driving view.
- **Fishing:** the view frames the pier and casting water. Once a line is out,
  it includes the angler's feet and the moving hook, with extra space above the
  bottom fight panel. Heading stays fixed through the cast/fight so opposing
  rod controls keep consistent left/right meaning. Cancel returns to casting.
  The existing Follow fight camera and manual Eye level view remain intact.

Both views use presented world positions, camera obstruction checks and lens
restoration. Football and Basketball retain their existing ball-following
stadium camera. These are presentation changes; authoritative sport rules,
room travel and shared chat history remain unchanged.

## Reproducing the checks

### First-person body visibility — 25 October 2026

Golf and Fishing keep the equipped body/arms visible in Eye level. The fused
head and original hair now use an imported rest-pose mask in UV2.w, applied only
to the local body's forward/depth/normal passes. Shared identity materials stay
unmasked. Separate hair/scalp attachments become shadow-only while the body is
visible in first person; the original head still contributes to the shadow.
The mask includes the grey underside of the source hair, below the old face
region. The equipped Eye level camera sits 0.28 units toward the gaze direction
from its existing height, keeping the open neck outside the downward view.
The former Golf/Fishing head-bone scale collapse is removed, preserving normal
skinning, grip shapes and the third-person silhouette. Follow, Course/Lagoon
and Golf aiming restore the complete character. Other sports retain their
existing first-person body visibility rules.

The source FBX, texture artwork, bone weights and geometry are unchanged. Both
imported LODs carry the additional mask, and the generated identity library
dependency record is refreshed by the normal build. No player-facing strings
are introduced.

Build the focused candidate with `FirstPersonBodyBuild.Build`; then run
[Test-FirstPersonBody.ps1](../Tools/Build/Test-FirstPersonBody.ps1) with
`-PlayerPath` pointing to `Builds/FirstPersonBodyQA/Player/WhatTheFish.exe`.
The opt-in development probe checks both LODs of all ten appearances, unchanged
head scale/shared materials/grip shapes, unmasked hand/finger vertices, pitch
extremes, third-person recovery
and Golf aim/cancel transitions. It captures the old bone-collapse path and the
corrected body from the same camera. Evidence lives in ignored
`Builds/FirstPersonBodyQA/`; phone rendering and performance require device QA.

Verification: **1,976 runtime assertions passed** across Golf and Fishing at
960×540 and 1600×900 (505 Golf + 483 Fishing at each size). Rendered captures
show feet/body at the bottom edge, no head/hair underside or open neck blocking
the view, and intact shadows. Both LOD hand/finger regions remain unmasked.
The 1600×900 checks used the newer complete shared-source Windows candidate,
including concurrent awards work; all 509 compiled source documents matched.
Evidence: `Run-20261025-202754-915/`, `Run-20261025-203022-554/`, and
`mesh-hand-validation.log` under that QA folder.

### Shared player/camera checks

Build the candidate with the Unity editor entry point
`WhatTheFish.Editor.PlayerIdentityCameraBuild.Build`; use `.Scripts` only after
a full candidate exists with the same assets. It writes the complete player to
`Builds/PlayerIdentityCameraQA/Player/` relative to the repository.

Run [Test-PlayerIdentityCamera.ps1](../Tools/Build/Test-PlayerIdentityCamera.ps1)
against that candidate using `-PlayerPath`, or omit that parameter to check
`Builds/WindowsFinal/WhatTheFish.exe`. It exercises all four sports by default.
Use `-Sport`, `-Width` and `-Height` for narrower checks. The runner derives its
repository root from its script location and works from an unrelated directory.

The development-only review covers real Golf swings/flight/skip, Fishing
cast/bite/hook/reel/cancel, lens restoration, moving-camera label attachment,
crowds, peer identity visibility, world occlusion and first-person/travel hiding.
Raw logs, screenshots and compiled-source hashes remain under ignored
`Builds/PlayerIdentityCameraQA/`. Release size records are in
[BUILD-SIZE.md](BUILD-SIZE.md) and [APK-SIZE-AUDIT.md](APK-SIZE-AUDIT.md).

On 23 October 2026, **935 assertions passed**: 60 focused checks across all four
sports at 1600x900, 40 additional checks at 1280x800/1920x864, 191 Fishing gameplay
checks, 466 host/guest identity checks, 63 room-chat UI checks and 115 court-camera
checks. Both peers also agreed on all ten identities/uniforms in each team sport.
The full 334-file Windows player was promoted with exact payload hash comparison.
The final four-sport captures are in `Run-20261023-211139-123/` below the QA folder.
Superseded deliveries and draft captures were archived in
`Legacy/20261023-212308-021-open-type-activity-cameras/`.

Rendered Windows checks establish these layouts and behaviors, not physical
phone appearance or frame rate. Those require device validation.
