# Centre-spot football — 1 October 2026

The supplied glossy football is installed in the additive [football scene](../Game/Assets/_Game/Scenes/SkySail_Football.unity) and [football environment prefab](../Game/Assets/_Game/Prefabs/Environments/FootballEnvironment.prefab). It now has Rigidbody physics and basic kicking; see [the football prototype](FOOTBALL-PROTOTYPE.md). Placement and resize records below describe the preceding static-prop integration.

## Current size — doubled on 1 October

At the user's request, the delivered diameter is now **0.44 metres**, twice the original 0.22 metres in each axis. The source FBX and editable master retain their original geometry and authoring dimensions. `SoccerBallBuilder` scales the visual model and sets the sphere collider radius to **0.22 metres**. Placement remains at pitch centre, with measured bottom clearance **0.000503 metres**. The new Unity scene/prefab audit passed; close and centre-circle renders were inspected. The 22 cm APK and superseded captures/audits were preserved under ignored `Legacy/20261001-153944-180-soccer-double-size/`.

The resized AndroidSubmission build and v2 signature verification passed. Measured APK size changed from **83,765,822 to 83,766,938 bytes (+1,116 bytes)**, leaving **16,233,062 bytes** under the ceiling and exceeding the working target by **8,766,938 bytes**. SHA-256: `B1300F631C9B10D5B09675A1B2B9C2E35FF3B624C8D856B55C76059CC366634D`. The populated packing audit under `Builds/SizeAudit/latest/` confirms the same 550,636 serialized mesh bytes and 473,398 estimated ZIP bytes; material sizes and the largest contributors are unchanged. Close/centre views retain the rounded panel joins. Physical-phone checks remain pending.

The command-runner attempt and its copied stale report were superseded by the successful existing-editor build and archived to `Legacy/20261001-154330-684-soccer-size-build/`. Current captures and signature evidence remain in `Builds/SoccerBall/`. No Git staging, commit or push was performed. Records below describe the initial 22 cm integration; its old APK/audits, including `soccer-first-pack/`, are now retained in the resize archive above.

## Assets and regeneration

- Delivery: [soccer-glossy-unity.fbx](../Game/Assets/_Game/Art/Football/Ball/soccer-glossy-unity.fbx), 1,078,236 bytes; SHA-256 `3861646c278b318716ae26915a716abc2ad6fbe707f89ea6f98fa856f771c06e`.
- Editable master: [SoccerGlossy.blend](../ArtSource/Football/Ball/SoccerGlossy.blend).
- Shared prefab: [SoccerBall.prefab](../Game/Assets/_Game/Prefabs/Football/SoccerBall.prefab).
- Two URP Lit materials retain the ivory/charcoal pigments and smoothness. No texture maps or external image/library dependencies are required. The FBX geometry is unchanged: 46,080 triangles. Unity imports custom normals, low mesh compression and no animation, with mesh CPU readability disabled.
- [prepare_soccer_ball.py](../Tools/Blender/prepare_soccer_ball.py) audits the FBX and recreates the editable master using Python with `bpy`, or Blender background mode. Its default paths resolve from the script location. An explicit `--input` can supply a new external FBX only when the delivery file does not exist.
- Unity menu **WHATTHE FISH? → Football → Install centre-spot soccer ball** installs and verifies the scene/prefab. [SoccerBallBuilder](../Game/Assets/_Game/Editor/SoccerBallBuilder.cs) is also called by `FootballBuilder`, so regeneration retains the ball. Stop Play mode and save any loaded football scene before installation.

## Verification

Unity compilation, installation and scene serialization passed. The audit found exactly one ball at local pitch centre `(0, 0)` in X/Z, diameter `(0.22, 0.22, 0.22)` metres, and bottom clearance **0.000502 metres** above the authored pitch mesh. The sphere collider has radius 0.11 metres on the default layer, avoiding the walkable-floor layer. The model uses two valid URP materials.

Inspected Unity-rendered close and centre-circle captures: round silhouette, retained panel seams, ivory/charcoal colouring, correct centre-spot placement and scale. The white polygon beneath the ball is the existing painted centre spot. Captures/audits are local evidence under ignored `Builds/SoccerBall/`.

Format-aware FBX inspection found no active texture filenames; the saved Blender master has no image or library dependencies. Exporter provenance and saved Blender UI history may contain historical paths, but are not active dependencies. Portable-path and repository-readiness checks passed. Regeneration also passed in a copied checkout path containing spaces while Python's working directory was an unrelated system temporary directory; the FBX hash and dimensions matched.

AndroidSubmission passed from the normally launched Unity editor; APK v2 signature verification passed. The actual APK is **83,765,822 bytes**, leaving **16,234,178 bytes** below the strict ceiling. It is **8,765,822 bytes above** the development target. The last documented pre-ball APK was **83,282,808 bytes**; the increase against that reference is **483,014 bytes**. That APK was absent locally at task start, so this is not a controlled same-session before/after comparison. [BUILD-SIZE.md](BUILD-SIZE.md) records the hash, build details and the two earlier command-runner Gradle loopback failures. Physical-phone appearance, performance and gameplay interaction remain untested.

Superseded installation-error evidence and the completed portable-regeneration scratch copy were archived to ignored `Legacy/20261001-015840-802-soccer-ball/`. Current captures and audit records remain under `Builds/SoccerBall/`. No Git staging, commit or push was performed.

Failed packaging attempts were retained in `Legacy/20261001-020445-182-soccer-android-retry/` and `Legacy/20261001-152734-112-soccer-android-complete/`; the current APK and successful build evidence remain in `Builds/`.

The successful incremental build reported `Not rebuilding Data files -- no changes`, leaving its `packed-assets.csv` empty. The original complete packing report was recovered from the first attempt (which failed later in Gradle) into `Builds/SizeAudit/soccer-first-pack/`; its failed build result is retained explicitly. Comparing those unchanged data offsets against the successful APK estimates **473,398 bytes** for the ball mesh and **1,294 bytes** for its two materials. Per-asset DEFLATE estimates differ from Unity's ZIP chunk compression; only the actual APK length is authoritative.

The largest estimated art contributors remain shared timber (2.25 MB), character albedo (1.99 MB), golf structures (1.98 MB), character mesh (1.57 MB) and football stadium geometry (1.56 MB). Actual ZIP entries include `libil2cpp.so` at 10,932,912 compressed bytes and `libunity.so` at 9,029,858 bytes. The ball adds no texture set or duplicated authoring master to the player. Geometry reduction was avoided to retain its panel joins; low mesh compression and disabled CPU readability are applied at import.

The temporary Editor-only report reader and empty incremental estimate were archived to `Legacy/20261001-153109-098-soccer-audit-helper/` after use. The current populated audit remains under `Builds/SizeAudit/soccer-first-pack/` alongside the successful APK's current ZIP-entry report in `latest/`.
