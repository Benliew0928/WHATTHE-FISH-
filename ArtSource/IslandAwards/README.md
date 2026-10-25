# Island awards references and geometric masters

User-provided visual references:

- [Fishing](Fishing-reference.png), SHA-256 `18F07F450E313B6BDEFBE4C537737936E257E5564A26967D07CFCAB22B8E43EA`.
- [Golf](Golf-reference.png), SHA-256 `4B70CEDD70FA5A51B07D7A72A2AF27C719AD567A50BF5F6D0109213A1EDBDE72`.

These original images are retained unchanged. The actual island environment
continues to supply the background; the references are not screen backdrops.
Three-dimensional reconstruction preserves the circular cream plinths,
gold/silver/bronze inset tops, winged star numbers and blue trim. Fishing adds
the central plank bridge, wave relief, rope piles, lanterns and life rings.
Golf adds stone steps, blue handrails, rope rails, shrubs, flowers and rocks.
The result is a geometric interpretation of the supplied single-view artwork,
not a pixel-identical image from every camera angle.

The editable master is
[`IslandAwardsStage.cs`](../../Game/Assets/_Game/Shared/UI/IslandAwardsStage.cs).
`WhatTheFish.Editor.IslandAwardsBuild.Prepare` bakes merged colour meshes and
materials into `Game/Assets/_Game/Art/IslandAwards/` and the two prefabs into
`Game/Assets/_Game/Resources/IslandAwards/`. Regeneration updates existing assets
in place, preserving their GUIDs. All mesh/material/font references are local
Unity asset GUIDs; there are no external Blender/FBX dependencies.

Only baked assets are loaded for presentation. No live mesh construction,
colliders or gameplay physics are added by the stage. Character meshes and
celebration poses reuse the game's existing render-only actors. The mist is a
single dynamic 42-quad mesh using the existing radial-alpha effect material.
The baked Fishing stage contains 31 colour meshes / 53,789 vertices; Golf
contains 29 / 37,135. Number labels reuse the existing font with a depth-tested runtime material. The extended bridge accommodates the wider single-file queue. Smoke reuses its
managed buffers after warm-up instead of allocating particle corner arrays
every frame. No new full-screen texture or video is included in the player.
