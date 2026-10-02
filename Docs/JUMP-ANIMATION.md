# Rainbow Sprinter jump

Jump with **Space** or the **Jump** touch button while exploring any of the four islands. WASD and the movement stick steer throughout flight and landing. Releasing input carries the current momentum; new input bends or reverses travel with bounded acceleration. **E** tackles in football; **F** selects clockwise Sky-Sail travel at the station. Space no longer submits a selected menu button; Enter still does.

The revised animation starts with a grounded knee bend and arm backswing, then extends the legs as the arms drive upward. Feet stay beneath or slightly behind the hips in flight, extend before contact and absorb the landing. The former forward foot tuck and disconnected arm motion have been replaced. The 80 ms grounded preparation includes the push: physical takeoff aligns with the extended pose, rather than starting the arm swing after departure. Input shows the preparation immediately and horizontal control stays active throughout.

A normal jump rises 1.2 metres with gravity of 22 m/s², about 0.66 seconds of flight and 0.22 seconds of landing recovery. Landing animation never locks movement. Coyote jumps and buffered rebounds skip the extra grounded preparation. Root motion remains disabled: the capsule owns travel and facing, so changing direction cannot fight an animation trajectory.

`JumpMotor` integrates vertical displacement, with 100 ms of ledge grace and a 120 ms press buffer. Only new key/pointer presses request jumps. An early airborne press expires; a press just before landing can start the next jump on contact. There is no double jump. Collision sweeps use steps no larger than 1/60 second, capped at 0.25 seconds of catch-up per frame. Ceilings stop ascent, walls block motion, and step climbing is disabled in flight. Tackle, hit reactions and shared travel prevent new jump requests. An airborne player cannot start or receive a ground tackle.

The existing `LocomotionMotor` owns horizontal velocity. Running momentum is retained through the grounded jump preparation, even when the stick is released on the jump press. In flight, release applies exponential drag of 1.25/s in world space: a 7 m/s sprint coasts about 2.20 m over the next 400 ms and slows to 4.25 m/s. Losing the touch sprint flag or rotating the camera does not erase or redirect that velocity. Active air input accelerates toward its requested vector at up to 65 m/s², including diagonals and analog input, without exceeding the requested full sprint speed. A full-speed 180-degree reversal takes roughly 0.22 seconds and starts responding on the first simulation step.

Landing restores 50 m/s² ground braking; the first 160 ms of ground steering uses bounded acceleration of 90 m/s² to avoid a sharp velocity change at contact. Normal grounded direction changes remain responsive. Wall contacts remove only momentum into the obstacle and preserve tangential travel. Tackle, hit reactions, teleports and shared travel reset motion through the existing reset path. A standing jump has no horizontal drift.

The highest Animator layer plays the saved jump with time driven by grounded preparation, vertical velocity and actual landing contact. It blends over idle/run and the turn expression; it adds no per-frame object creation or rig rebuilding.

The current network protocol is **12**, including basketball gameplay. `JumpSnapshot` carries grounded preparation progress; momentum uses the existing host-authoritative movement and transform replication without changing the wire format. Reliable owner-only jump/tackle requests are sent immediately on their input frame, independently of the 30 Hz movement send interval. The host validates and simulates physics; guests render replicated preparation, flight and landing. This retains server authority and does not implement client prediction or promise zero internet latency. All room participants need matching builds.

## Editable source and regeneration

- Master: [RainbowSprinterJump.blend](../ArtSource/Shared/Characters/RainbowSprinterJump.blend), `RainbowAnimationControls`, saved `Jump` action, frames 1–61 at 60 FPS.
- Generator: [jump_rainbow_sprinter.py](../Tools/Blender/jump_rainbow_sprinter.py). `setup` creates a separate master only when absent; `export` reads the saved action and does not overwrite authoring work; `audit` checks skeleton and foot targets. `revise` explicitly replaces the master with the revised authored sequence; use it only when that replacement is intended. `preview` renders the saved poses with a temporary physical-height track.
- Delivery: [RainbowSprinterJump.fbx](../Game/Assets/_Game/Art/RainbowSprinterJump.fbx), animation-only, with the existing rig/materials/textures shared. Unity uses keyframe reduction, 0.3 rotation error and 0.1 position/scale error.
- [JumpAnimationBuilder](../Game/Assets/_Game/Editor/JumpAnimationBuilder.cs) installs the `Jump` layer on the existing controller and runs motor checks before normal player builds. It refreshes the imported frame range when an export changes duration, preserving the complete recovery. In an open editor, use **WHATTHE FISH? → Prepare jump animation** after replacing the exported clip.

Set `BLENDER_PATH` locally as described in [Setup](SETUP.md). From the repository root:

```powershell
& $env:BLENDER_PATH --background --factory-startup --python Tools/Blender/jump_rainbow_sprinter.py -- setup
& $env:BLENDER_PATH --background --factory-startup --python Tools/Blender/jump_rainbow_sprinter.py -- export
Tools/Build/Build.ps1 -Target Windows
Tools/Build/Test-Jump.ps1
Tools/Build/Test-Tackle.ps1
Tools/Build/Build.ps1 -Target AndroidSubmission
Tools/Build/Check-TaskReady.ps1 -RequireApk
```

The new master retains the existing source's nonfunctional provenance/UI history. Format-aware inspection verifies its active image dependencies are relative, exist in the copied checkout and use no linked libraries. The exported FBX has no texture/video dependencies. Export and pose hashing were also checked in a copied checkout containing spaces, launched from an unrelated directory.

## Verification

All **287 Windows player checks passed**: 140 jump/momentum/room, 24 tackle/room, 62 turning/room and 61 offline basketball gameplay checks. Current evidence uses `Builds/JumpQA/Run-20261001-223853/`, `Builds/TackleQA/Run-20261001-223858/`, `Builds/TurnQA/Run-20261001-224046/` and `Builds/BasketballGameplayQA/Offline-20261001-224049/`. Release is checked in eight directions at 20/30/60/120 FPS, during preparation, ascent and descent, with the touch sprint flag cleared and command heading changed by 137 degrees. Coasting stays in its original world direction, lands and stops; standing jumps do not drift. Both network players observe release momentum and active reversals. Existing height, collision, LOD, action, island and animation checks also pass.

Another **65 editor motor checks** cover 16 headings at four frame rates, bounded steering acceleration/speed, analog input, stopping on landing, resets, wall tangents and irregular frame intervals. The analytic 400 ms coast measures about 2.2034 m across all tested headings and rates. The real capsule test releases 200 ms after the jump press and travels roughly 2.76–2.78 m before landing, with small collision-step differences between frame rates.

The momentum recording is `Builds/MomentumQA/momentum-review.mp4`, with forward/diagonal release and a fresh steering input during flight. It uses scripted input in the actual Windows player. Reproduce the source captures with `-probe -jumpReview <output-directory> -momentumReview -exitAfter 18`. Build, size, motor and test summaries remain alongside it under ignored Builds.

The earlier animation review is `Builds/JumpNaturalQA/jump-review.mp4`, showing the side/front poses and airborne reversal before the momentum update. Its source frames are in `GameMotionFinal/` alongside that revision's evidence. Both reviews use fixed-rate 60 FPS capture and are not frame-rate benchmarks. The pose review can be reproduced with `-probe -jumpReview <output-directory> -exitAfter 15`.

See [current APK measurement](BUILD-SIZE.md) and [APK audit](APK-SIZE-AUDIT.md) for the measured build budget. Windows captures and LAN probes do not qualify Android appearance, phone frame rate, touch ergonomics or WAN latency.

The momentum build's signed APK measured **85,970,874 bytes**, versus **85,970,822** before the update: **+52 bytes**, with **14,029,126 bytes** to the hard ceiling. The jump clip remains 37,124 serialized bytes (24,231-byte ZIP estimate). The 75 MB development target remains unmet by 10,970,874 bytes. No assets were changed or archived in the momentum update. Earlier rejected poses and fixtures remain recoverable from `Legacy/20261001-204131-970-jump-motion-revision/` and `Legacy/20261001-195347-370-jump-validation/`. No Git staging or publishing was performed.
