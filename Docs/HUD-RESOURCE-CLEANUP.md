# Simple HUD resource cleanup — 8 October 2026

The [sports HUD](SPORTS-HUD.md) uses procedural geometry and existing fonts.
Removing its icons, highlights, captions and decorative map shapes adds no
bitmap assets. Measurements of the delivered APK are recorded in
[BUILD-SIZE.md](BUILD-SIZE.md) and [APK-SIZE-AUDIT.md](APK-SIZE-AUDIT.md).

## Dependency audit and removal

`HudResourceAudit.Run` inventories every material texture assignment, scene
and Resources dependency, unreferenced art candidate and forced shader.
Evidence is retained under ignored `Builds/SimpleHudQA/`.

- All current gameplay and menu Resources assets are used, including fonts,
  equipment, football rules and motion, aiming materials and menu art. Font
  licence texts remain with their assets.
- No assigned material texture was an absent shader property. The large
  terrain controls, character textures, timber, stadiums and island structures
  remain used; they were not removed to claim a smaller source folder.
- Mobile materials, meshes and vegetation often have no direct scene reference
  because build processors substitute them. Their generators and manifests
  remain active, along with the high-quality authoring sources.
- Removed the forced `Legacy Shaders/Diffuse` and `Sprites/Default` entries.
  The game uses URP materials and UI graphics; searches found no SpriteRenderer,
  runtime shader lookup or generator use of either shader. Dependency-based
  inclusion remains available. UI and reflection-cube shaders stay included.
- Archived the unreferenced auto-import material
  `Game/Assets/_Game/Art/Golf/Club/Materials/MidnightIron_BaseColor.mat` and its
  metadata. Its GUID and path have no active references, it is absent from the
  scene/Resources dependency graph and APK, and `GolfClubBuilder` explicitly
  assigns `MidnightIron.mat` to every renderer. The FBX disables material
  import and has no external material mappings. This removal is repository
  cleanup, not claimed APK savings. Club meshes, textures and masters remain.

## Redundant animation data

Seven full sampled animation imports now use conservative keyframe reduction:
idle, four turns, slide tackle and tackle hit. Original FBXs and authoring
masters are unchanged. `ProjectBuilder.Setup` applies the same configuration
on regeneration; `AnimationDeliveryOptimizer.Prepare` repeats the comparison
against uncompressed imports before accepting the settings.

The comparison samples every transform in the character hierarchy at 120 Hz,
including clip endpoints. It checks world position, rotation, scale, duration,
loop flags and key counts. All seven clips stay under the 1 mm position,
0.1° rotation and 0.001 scale-error limits. The largest observed position
difference is **0.714 mm**. This is measured delivery compression, not a claim
that authored motion is unused. Unchanged sampled source data remains editable.

See [VERIFICATION.md](VERIFICATION.md) for the final builds and runtime checks.
