# Golf rim scoring, celebration and final countdown

Current follow-up: [smooth countdown, circular address and pond surface support](GOLF-ADDRESS-AND-COUNTDOWN-20261018.md) supersedes the earlier terrain-only cart support and per-frame countdown behavior described below.

## Scope

The 21 moving mechanisms were rolled back. Existing Golf scene and environment
prefab are restored to the pre-mechanism HEAD contents; the retained map still
contains its static obstacles and six ponds. Shot strengths, charging, swing
animation, water recovery, score authority and existing match deadline stay intact.

## Gameplay and UI

- The Golf-only Leave island control and ISLAND GREENS station badge are hidden.
- The shot inset camera is no longer created or contacted. Main third-person
  aiming and shot-camera behavior is retained.
- Cart body sweeps and overlap checks ignore only authored `Pond bank` mesh
  colliders under a GolfCourse. Wheels still sample the original terrain and
  enforce shoreline/slope support. Balls and walking characters retain pond
  collision and ball water-return rules.
- Cup capture radius is cup radius plus ball radius: touching the rim counts.
  The vertical upper limit is ball radius plus 0.025 m. A finite-cylinder
  segment intersection catches fast rolling and falling contacts; high flyovers
  are rejected. Existing authoritative EnterHole validates owner and target hole.
- Host-confirmed Cup/Finish events spawn a pooled 2.6-second mesh celebration:
  gold/aqua spirals, rising rays, white-bordered gold stars, aqua bubbles and
  glints. Four pooled effects maximum; flags/backgrounds are unchanged.
- The final countdown uses the existing replicated deadline. At trigger it is
  centered for 0.5 s, then eases to the upper-left over 0.8 s. Dynamic seconds
  decrement from the shared time. The original clock dial wiggles on each tick;
  reduced-motion mode suppresses the wiggle. Late observers use elapsed server
  time rather than replaying a stale 30-second introduction.

## Art

[Original and editable delivery masters](../ArtSource/Golf/UI/FinalCountdown/)
include the user reference, a clean plate and matching numeric atlas. The exact
original image is used at 30 seconds; other values reuse the same composition
with generated matching glyphs. Fixed art is not redrawn in generic UI shapes.
Pixel identity of newly generated digits is not claimed. Provenance and prompts
are recorded in `provenance.json`. Built-in imagegen was used for plate/digit
preparation. `GolfPolishBuild.Prepare` derives Resources assets using portable
paths and explicit sprite imports; no authoring master is shipped through Resources.

## Checks and delivery

Current focused evidence lives in ignored `Builds/GolfPolishQA/`.
Final focused checks passed: 85 offline, 42 host, 44 client. Screenshots and
reports: `Run-20261018-190816/`. Complete tested player promoted to
`Builds/WindowsFinal/` with per-file hashes. AndroidSubmission APK: 88,540,314
bytes, v2 signature verified, headroom 911,459,686 bytes. See
[verification](VERIFICATION.md) and [size audit](APK-SIZE-AUDIT.md).
Windows rendering is not physical Android visual/FPS validation.
