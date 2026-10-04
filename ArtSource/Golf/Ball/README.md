# Rounded golf ball

The current Golf ball uses the user-supplied `Originals/golf-ball-rounded.fbx`.
That original is retained byte-for-byte: SHA-256
`F900EB7B74E2CE0AD9DBFD9BAA2E1D26155FD091CF86E3D56FA2DAB039F57998`.
It contains one closed **87,048-triangle** mesh with geometric dimples, an ivory
material and no texture or external-file dependencies. The supplied exporter
labels its numeric Visibility property as a double; the generator interprets
that value without modifying the original.

`RoundedBall.blend` retains the normalized high-quality mesh and both delivery
LODs. The source outer radius is **21.5 mm**. The Unity prefab's visual child
uses `GolfBall.VisualScale = 3`, giving a **129 mm gameplay diameter** and a
matching **64.5 mm physics radius** without modifying either source mesh. Near LOD
retains **15,998 triangles** and the source dimples. Below 1.2% screen height,
the **720-triangle** far LOD uses a smooth round silhouette. Both share one
ivory material. No imported texture is needed. Geometry, dependencies and
current file hashes are recorded in [delivery-audit.json](delivery-audit.json).

From any working directory, run Python with `bpy` and the full script path, or
Blender in background mode:

```text
python Tools/Blender/prepare_golf_ball.py --render
python Tools/Blender/audit_golf_ball.py
```

Paths default to the repository containing the script; `--root` selects another
checkout. Blender arguments go after its `--` separator for the generator.
The existing equipment generator delegates Ball regeneration to this script.
The old Meshy source and its baked maps remain in `ArtSource/Golf/Equipment/`
as authoring provenance; they do not define the current delivery mesh.

`GolfBallBuilder.Prepare` updates the existing
[`Ball.prefab`](../../../Game/Assets/_Game/Prefabs/Golf/Ball.prefab), preserving its
asset identity and sphere collider. Live match balls keep using that prefab's
shared meshes through `GolfMatchManager`. The physics component, owner,
stroke/target rules, recovery, swing timing and networking retain their existing
behavior; tee height and cup detection follow the configured gameplay radius.
The builder also runs during normal player builds, and the equipment builder
uses it rather than restoring the previous ball.

The delivery has no active absolute file dependencies. Inactive Blender exporter
SceneInfo provenance and saved Blender UI history may contain local paths;
format-aware checks record those as historical metadata. Current model views,
import audits and gameplay evidence stay under ignored `Builds/GolfBallRoundedQA/`.
