# Fishing camera, natural swimming and rope strain

19 October 2026. This follows the [shared-lagoon delivery](FISHING-SHARED-LAGOON.md).
`main` was fetched and pulled before editing; the remote and local HEAD were
both `9bb55646fb6ae0f168445c17ee9cc55d81e1a2f2`, with no incoming changes.
Existing uncommitted Fishing work is included and preserved. No files were
staged, committed or pushed by this task.

## Player experience

- Eye-level camera stays at the player's eyes throughout a fight, including
  Left/Right holds and release. Manual looking remains available. Follow and
  Island views retain their existing fighting camera and return behavior.
- The shared pool's 30 fish follow independent, seeded wandering paths. Smooth
  curves connect changing destinations; fish turn gradually instead of orbiting
  the pond together. Hooked movement, forces, splashes and catches are retained.
- The extra filled gauge beside the rope is replaced by a small line pictogram:
  curved and mint when loose, straighter and gold as strain rises, then taut and
  red with an exclamation mark near snapping. The actual rope uses the same
  color scale. Reduced motion keeps the warning while suppressing vibration.
  The existing centre Catch/Tension card and Left/Right rules remain.

Fish routes are calculated from fish ID and shared server time, so clients do
not choose different random targets. Cubic B-splines keep velocity and
acceleration continuous and remain inside the waypoint boundary. The room
protocol is **70**; everyone in a room must use the updated build.

No new bitmap, model, shader, audio clip or package was added. The line cue is
drawn at runtime and reuses the sports UI colors. No active art was removed.

To review the change, enter Fishing, open the gear and choose **Eye level**.
Close settings, cast and hook a fish, then hold Left/Right and drag to look.
The view should stay at eye level. Release both controls to cool the line and
watch its color and the small line shape relax. Before casting, watch several
fish together: their routes should cross and change direction independently.

## Evidence and delivery

Current task evidence is under
`Builds/FishingNatural/Task-20261019-052154Z/`. Build outputs and raw evidence are
ignored local files. The opening APK was measured directly at **87,581,220
bytes**, rather than using the older size-document entry.

The Windows candidate retains the complete previously validated native player;
the guarded script refresh checks that its scene data, packaged catalog and
islands bundle have identical hashes. Source hashes record the current 467
C# files. Rendered fixtures use the production camera, rod, line and HUD.
The screenshot check waits until the end of the frame, after the rod's
LateUpdate pose and first-person head hiding.

Final release measurements and verification are recorded in
[Build size](BUILD-SIZE.md) and [APK size audit](APK-SIZE-AUDIT.md).
Run the delivered Windows player with
`Builds/WindowsFinal/Explore-Fishing.cmd`; install the fresh self-contained
`Builds/Android/WhatTheFish-release.apk` for Android testing.

Physical Android touch behavior, appearance, frame rate and two-computer/WAN
multiplayer require device testing. Windows and local host/guest checks do not
establish those results.
