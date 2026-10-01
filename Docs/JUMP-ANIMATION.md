# Rainbow Sprinter jump

Jump with **Space** or the **Jump** touch button while exploring any of the four islands. WASD and the movement stick continue to steer throughout flight and landing, including immediate reversals. **E** tackles in football; **F** selects clockwise Sky-Sail travel at the station. Space no longer submits a selected menu button; Enter still does.

The revised animation starts with a grounded knee bend and arm backswing, then extends the legs as the arms drive upward. Feet stay beneath or slightly behind the hips in flight, extend before contact and absorb the landing. The former forward foot tuck and disconnected arm motion have been replaced. The 80 ms grounded preparation includes the push: physical takeoff aligns with the extended pose, rather than starting the arm swing after departure. Input shows the preparation immediately and horizontal control stays active throughout.

A normal jump rises 1.2 metres with gravity of 22 m/s², about 0.66 seconds of flight and 0.22 seconds of landing recovery. Landing animation never locks movement. Coyote jumps and buffered rebounds skip the extra grounded preparation. Root motion remains disabled: the capsule owns travel and facing, so changing direction cannot fight an animation trajectory.

`JumpMotor` integrates vertical displacement, with 100 ms of ledge grace and a 120 ms press buffer. Only new key/pointer presses request jumps. An early airborne press expires; a press just before landing can start the next jump on contact. There is no double jump. Collision sweeps use steps no larger than 1/60 second, capped at 0.25 seconds of catch-up per frame. Ceilings stop ascent, walls block motion, and step climbing is disabled in flight. Tackle, hit reactions and shared travel prevent new jump requests. An airborne player cannot start or receive a ground tackle.

The existing `LocomotionMotor` continues to own horizontal input. The highest Animator layer plays the saved jump with time driven by grounded preparation, vertical velocity and actual landing contact. It blends over idle/run and the turn expression; it adds no per-frame object creation or rig rebuilding.

Network protocol **11** extends `JumpSnapshot` with grounded preparation progress. Reliable owner-only jump/tackle requests are sent immediately on their input frame, independently of the 30 Hz movement send interval. The host validates and simulates physics; guests render replicated preparation, flight and landing. This retains server authority and does not implement client prediction or promise zero internet latency. All room participants need matching builds.

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

All **126 Windows checks passed**: 40 jump/room, 24 tackle/room and 62 turning/room checks. Current evidence uses `Builds/JumpQA/Run-20261001-203848/`, `Builds/TackleQA/Run-20261001-203321/` and `Builds/TurnQA/Run-20261001-203333/`. The motor audit covers height at 20/30/60/120 FPS, immediate preparation feedback, cancellation, landing, duplicate airborne requests, ceilings, coyote time, buffered landing and blocked input. The Windows player probe exercises real capsule collisions, direction reversal, both LODs, animation blending, all four island buttons and two-player host/guest replication. Tackle regression checks cover action recovery, opponents, walls and network reactions.

The final side, front and airborne-reversal recording is `Builds/JumpNaturalQA/jump-review.mp4`; its source frames are in `GameMotionFinal/` alongside build, dependency and size evidence. This fixed-rate 60 FPS recording reviews motion and is not a frame-rate benchmark. The development player can reproduce it with `-probe -jumpReview <output-directory> -exitAfter 15`.

See [current APK measurement](BUILD-SIZE.md) and [APK audit](APK-SIZE-AUDIT.md) for the measured build budget. Windows captures and LAN probes do not qualify Android appearance, phone frame rate, touch ergonomics or WAN latency.

The final signed APK measured **85,957,194 bytes**, versus **87,153,874** immediately before this motion revision; **14,042,806 bytes** remain to the hard ceiling. Most of that reduction is concurrent fishing asset optimization. The jump clip itself is 37,124 serialized bytes (24,231-byte ZIP estimate). The 75 MB development target remains unmet by 10,957,194 bytes. Rejected poses, superseded captures and the portable fixture are recoverable from `Legacy/20261001-204131-970-jump-motion-revision/`; the earlier task archive remains `Legacy/20261001-195347-370-jump-validation/`. No Git staging or publishing was performed.
