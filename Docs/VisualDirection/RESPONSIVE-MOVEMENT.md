# Responsive forward/backward movement — 2026-09-24

## Fix

The previous motor committed to a stationary 0.35–0.55-second turn and queued changed directions behind it. Repeated up/down input could therefore produce endless pivots without travel. That animation-driven movement lock has been removed.

Every active command now moves in the latest camera-relative input direction on the next simulation tick. Speed eases in, but is retained through a reversal. Translation does not wait for facing, braking or a clip to complete. Body yaw smooths independently toward current input, discarding obsolete rotational momentum on reversal. Releasing input cancels facing immediately; new input interrupts stop easing immediately. The stick dead zone, 4/7 m/s speed limits, camera controls, collision handling and server authority remain.

The authored turn curves now provide a masked torso/head expression over the running animation. The running hips, legs and arms continue throughout. The runtime does not invoke planted-foot IK or enter the full-body Turn state; both remain available for standalone authoring review. Turn-pose progress follows actual yaw, not a fixed timer. No direction queue remains.

## Verification

- `TurnAudit.Run` passed 24 rapid-reversal scenarios at 30/60/120 fps, switching every 1/3/6/12 frames, with both 20% and full-strength input. All 5,040 active simulation frames moved in their requested direction. Three seconds of full-strength input produced 20.65 m of path travel regardless of switch cadence or frame rate.
- Eighteen signed-angle starts responded on their first tick. Release canceled facing; fresh input interrupted braking. Held input settled to the requested direction, sprint release reached 4 m/s, and dead-zone/airborne checks passed.
- The Animator audit verifies that the turn expression changes the head while leaving running ankle rotations and hip position unchanged. The base state remains Run during turning.
- `Tools/Build/Test-Turn.ps1 -Video` passed all three sports and both LODs, including 540 built-player frames of alternating up/down commands with **zero stalled frames and zero wrong-direction frames**. This measures traveled path, not net displacement: equal opposite input should naturally return near its starting position.
- Host and client both observed motion during turning. Their script includes direction reversals every 0.10 seconds as well as held directions. Network protocol 5 prevents an older client from applying the former stationary-turn presentation.
- `Tools/Build/Test-Idle.ps1` passed preview animation, both LODs, all sports, first-person hiding, stop phases, heel-lift interruption, rapid restarts, three idle loops and local multiplayer idle regression.

Motor/Animator evidence: `Builds/TurnQA/editor-audit.txt` and `responsive-editor.log`. Built gameplay/network evidence: `Builds/TurnQA/Run-20260924-164202`. Idle regression: `Builds/IdleQA/Run-20260924-164219`.

The updated `Builds/TurnQA/forward-backward-gameplay.mp4` includes normal direction changes and rapid up/down input in the real player. `responsive-gameplay-sheet.jpg` contains inspected start/turn and rapid-input frames. The full-body Blender turn previews remain authoring references; they no longer describe a gameplay movement lock.

## Limits and reproduction

Windows and Android development builds completed successfully. `adb devices` reported no connected phone; physical-device testing was not performed.

Use matching protocol 5 builds on all multiplayer participants. Network transport/server authority still adds ordinary network latency; this change removes animation-induced waiting and does not introduce client prediction. Physical Android performance and WAN latency/loss remain separate tests.

Rebuild with `Tools/Build/Build.ps1 -Target Scene`, then `-Target Windows` or `-Target Android`. Run `TurnAudit.Run` through Unity batch mode, then `Tools/Build/Test-Turn.ps1 -Video` and `Tools/Build/Test-Idle.ps1`. Package gameplay captures with `python Tools/Blender/package_turn_preview.py --gameplay Builds/TurnQA/Run-<timestamp>`.

See [Turn authoring](TURN-AUTHORING.md) for the saved Blender source and the updated animation layering workflow.
