# Basketball model and physics foundation — 1 October 2026

The basketball is imported into the existing Rally arena as one reusable rigid prop. This stage prepares the model, physical contacts and synchronized movement. It does not add pickup, possession, dribbling, shooting controls, scoring, sound or particles.

## Source and delivery

The [original Meshy GLB](../ArtSource/Basketball/Ball/Meshy_AI_Basketball_1001104636_texture.glb) is 9,094,056 bytes and remains unchanged. SHA-256: `e638b7c32a5e1239ec3fc963a714cb659bf2d7c8b3ac898fbe141f79d1bf434d`. It contains one mesh and three embedded 2048-square images, with no external dependencies. Its centred vertex radii range from 0.91467 to 0.96214 in source units. Simply reducing its polygons would retain an uneven silhouette and seams.

The [Blender generator](../Tools/Blender/build_basketball_ball.py) therefore rebuilds an analytical sphere, a regular eight-panel pattern and pebbled rubber maps, retaining Meshy as visual provenance. [The native master](../ArtSource/Basketball/Ball/Basketball.blend) remains editable; delivery files are in [the Unity art folder](../Game/Assets/_Game/Art/Basketball/Ball/).

| Property | Delivery |
| --- | --- |
| Diameter / pivot / object scale | 0.24 m / sphere centre / unit scale |
| Near / distant triangles | 2,976 / 720 |
| Materials | One URP Lit material, metallic 0, smoothness 0.24 |
| Textures | Colour and tangent normal, each 1024×512, mipmapped, Android ASTC 6×6 |
| Authoring maps | 2048×1024, outside Unity Assets |
| Geometry checks | Closed manifold, outward normals, no degenerate triangles, radius error below 1e-7 m |
| Active dependencies | Relative Blender image references; FBX image basenames resolve beside the FBX |

The generator parses and rewrites FBX Texture/Video path properties with Blender's FBX tools, then verifies every other parsed payload is unchanged. It reopens the saved master and reimports the FBX, checking texture availability, geometry scale, triangle counts and finite UVs. A fixed exporter `Original|FileName` value (`/foobar.fbx`) is retained only as nonfunctional provenance; it is not an image/library dependency. The [audit and integrity hashes](../ArtSource/Basketball/Ball/model-audit.json) record this exception.

## Physics and world integration

[BasketballBall.prefab](../Game/Assets/_Game/Prefabs/Basketball/BasketballBall.prefab) separates the Rigidbody root, visual meshes and EffectsAnchor. One mathematical sphere collider gives consistent contacts regardless of texture seams. Initial tuning uses a 0.62 kg mass, hollow-shell inertia, restitution 0.78, friction, damping and a 100 rad/s spin limit. These are starting gameplay values, not a certified physical basketball calibration.

The ball uses speculative continuous collision detection. In an isolated drop, sweep CCD allowed the centre to reach 0.055 m above the floor before bouncing; speculative contacts prevented that penetration. See Unity's [collision mode guidance](https://docs.unity.com/en-us/engine/6000.0/manual/physics-section/physics-overview/collision-section/collision-detection/choose-collision-detection-mode). Both actual arena rims have 32 capsule segments and box proxies for the backboards. The decorative nets remain open to the ball.

The offline player or room host simulates physics. Guests keep a kinematic ball, disable local ball collision and interpolate host poses at approximately 20 updates per second. Pose resets carry a sequence number so clients snap after a teleport instead of interpolating across the arena. Server state is carried by the existing host NetworkAthlete; protocol version 9 prevents older builds joining this layout. `Place` is an authority-only foundation API with finite-input checks and speed limits, not a guest-controlled shooting RPC.

The ball freezes outside basketball exploration and during Sky-Sail travel. Streaming releases it when leaving the arena and creates exactly one on return. No island, room ownership or shared travel model is replaced. Future gameplay should add server-validated possession and shot requests, then scoring triggers and independent effects/sound.

## Reproduce validation

1. Run Blender with `--background --factory-startup --python-exit-code 1 --python Tools/Blender/build_basketball_ball.py`. Optionally append `-- --render`.
2. Run Unity batch method `BasketballBallBuilder.BuildWindows` to prepare assets and build the current Windows scenes without rebuilding unrelated art.
3. Run `Tools/Build/Test-BasketballBall.ps1 -Mode Offline`, then `-Mode Local` for host/guest validation. The suite uses a development player and writes evidence beneath ignored `Builds/BasketballModel/20261001/`.
4. Run `Tools/Build/Build.ps1 -Target AndroidSubmission`, inspect `Builds/SizeAudit/latest/`, and run `Tools/Build/Check-TaskReady.ps1 -RequireApk`.

The Blender pipeline also runs from a copied checkout whose name contains spaces, with an unrelated working directory and no root override. Delivery texture hashes and geometry reports must match, and saved dependencies must resolve within that copy. Temporary clone files and failed intermediate reviews are recoverable from the task's ignored Legacy archive.

The final Windows review passed **53 checks**: 22 offline, 20 host and 11 guest. The 2 m drop rebounded at 4.873 m/s, rolling covered 2.962 m with changing orientation, and 35 m/s floor/backboard tests rebounded without passing through their colliders. Clean shots passed both rims; offset shots struck them. Unloading and returning created exactly one ball. The guest received 383 snapshots, remained kinematic and followed the host within the measured interpolation window (maximum 0.569 m during the deliberate fast-shot sequence; this is not a network-latency guarantee).

Close, reverse and court-distance Windows captures were inspected. Current local evidence is in `Builds/BasketballModel/20261001/Offline-191820/`, `Local-191919/`, `Blender/`, `model-final-audit.log`, `portable-clone-final.log`, `portability-result.json` and `windows-build-verified.log`. Scene name inventories confirmed existing objects were retained. Superseded attempts and the tested clone fixture were archived to `Legacy/20261001-192124-020-basketball-ball/`, with a recovery manifest. No source GLB, current native master or delivery asset was archived.

Windows checks validate model appearance and gameplay logic on this PC. They do not establish Android ASTC appearance, IL2CPP networking, device RAM, frame rate or thermals. Physical phone testing remains required. Current APK measurements and the unmet 75 MB development target are recorded in [BUILD-SIZE.md](BUILD-SIZE.md).
