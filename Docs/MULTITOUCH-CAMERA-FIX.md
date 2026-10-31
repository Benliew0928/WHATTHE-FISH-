# Multitouch camera jump

## Cause

[Unity documents](https://docs.unity.cn/Documentation/ScriptReference/Input-simulateMouseWithTouches.html)
that legacy touch-to-mouse simulation maps two concurrent fingers to right mouse.
The shared camera previously accepted `GetMouseButton(1)` and Mouse X/Y axes
without checking for touches. While the UI drag already held a specific pointer
ID, this extra channel could turn the camera from simulated mouse movement.
Large mouse displacement on contact changes therefore caused a visible jump.
A desktop right drag could also reach both the UI look pad and the mouse branch.

## Change

- [PlayerView](../Game/Assets/_Game/Shared/Player/PlayerView.cs) rejects the mouse
  look channel while any touch is present. A fresh mouse hold anchors its first
  frame, including the transition after touch release. Existing mouse sensitivity
  is retained after anchoring. Camera handoffs, focus loss, pause and disabling
  release mouse state and pending look movement.
- [TouchPad](../Game/Assets/_Game/Shared/Player/TouchPad.cs) accepts left/touch
  presses only, holds one pointer ID, anchors on down without applying its event
  delta, and accumulates only that owner's position change. Duplicate-position
  events add nothing. Cancellation, hiding and application lifecycle resets
  release ownership. The movement joystick retains its existing radius/scaling.
- Touch-to-mouse simulation is not globally disabled. Gameplay physics, rules,
  networking and the existing camera sensitivity are unchanged.

## Verification and scope

The opt-in development review `-touchCameraReview <output>` sends touches through
real StandaloneInputModule routing over the actual HUD. An injected legacy mouse
channel also reports right-button/large-axis values with two fingers, reproducing
the conflicting input combination. It checks both finger orders, joystick/action
buttons, exact owner pixel deltas, touch-index reordering, a third finger, no
handoff to an already-held foreign finger, cancellation, modal closure, mouse
anchoring/sensitivity, duplicate events, right-button separation, focus, pause
and a disabled look pad.

Each of Football, Basketball, Golf and Fishing passes 42 assertions in English,
Simplified Chinese and Malay: **504 final assertions**. Current renders at 16:9
and 4:3 were reviewed in all languages. Initial hidden-window backbuffer captures
were black; the review now uses the existing explicit camera/canvas renderer.
The initial assertion summary is retained, final valid captures replace the
intermediate runs, and no duplicate checks are counted.

All 3,682 localization entries/font coverage checks pass. No visible wording or
art changed. The development review and injection hook are absent from the APK.
All 546 Windows and 390 Android compiled project-source hashes match.
This validates simulated routing and resulting camera state, not physical phone
OS input, mobile FPS, battery or touch feel. Install the updated APK and verify
walking plus look/action, lifting either finger first and repeated two-finger
contacts on the actual phone.

## Deliveries and retained output

## Multitouch camera input fix - 31 October 2026

AndroidSubmission: **168,015,003 -> 168,007,727 bytes (-7,276)**;
remaining headroom **831,992,273 bytes** below 1,000,000,000.
SHA-256: **3BB37EBDACE02CDF535C14127CBD5464CCCB8A137C45F148E12FD94BAB1B9A25**.
ZIP CRC, all 1,538 exact audit entries, five-scene bundle integrity, ARM64,
v2 signature and 16 KiB alignment pass. Largest compressed entries: islands
45,934,627; IL2CPP 13,346,193; Vivox 10,240,163; Unity 9,828,173 bytes.
Entry deltas were reviewed; no asset quality settings were reduced.

The shared camera now rejects touch-emulated mouse look and re-anchors new
mouse holds. Finger drags keep their owner and derive travel from positions,
with clean cancellation/focus/pause resets. Existing touch-to-mouse simulation
remains available to other controls. No player-visible text or art was added.

**504 runtime assertions** pass across four sports and all three languages,
using real StandaloneInputModule pointer routing plus a simulated legacy mouse
channel. The 12 final HUD captures were visually reviewed. All 3,682 localization
entries pass. All **546 Windows / 390 Android** compiled source hashes match.
The complete **335-file Windows candidate** was promoted and hash-verified,
with two retained awards launchers. The redundant candidate and initial capture
runs were removed after validation. No new project snapshot was created.

Evidence: `Builds/TouchCameraQA/`; [cause and validation details](MULTITOUCH-CAMERA-FIX.md).
The checks simulate mobile input on Windows; physical phone touch, quality and
FPS remain unverified. Source is `9e6ec1c` plus this fix. No commit/push was made.


- Android rollback: `Legacy/20261031-222914-651-touch-camera-android/`.
- Windows rollback: `Legacy/20261031-223643-627-touch-camera-windows/`.
- Current players/APK and compact final evidence remain under ignored `Builds/`.
- `cleanup-result.json` lists the verified candidate and 12 initial run folders
  removed under the generated-output rule. The complete candidate was compared
  file by file to WindowsFinal before deletion. Shared content/Unity caches and
  pre-existing evidence were preserved.
- The small task Java temporary directory is retained while Gradle can hold native
  handles. No redundant complete player/project copy from this task remains.
