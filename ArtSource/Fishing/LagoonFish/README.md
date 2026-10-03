# Lagoon fish authoring

`LagoonFish.blend` contains the editable gold-and-teal fish master and the two mobile delivery meshes. One design is reused at three sizes; these are not separate fish species.

- Master: 12,792 triangles.
- LOD0: 3,192 triangles; LOD1: 1,248 triangles.
- Nominal source length: 1 metre, nose along Unity +Z after FBX import.
- Delivery prefab lengths: 0.4, 0.9 and 1.8 metres.
- Vertex colours supply the body gradient, stripe, fins and facial detail. No external image or linked-library dependencies are used.
- The master stays outside Unity; only the two LOD meshes ship. Runtime shader tail motion is cosmetic and can be disabled with `_TailMotion`.

Rebuild with [the generator](../../../Tools/Blender/build_lagoon_fish.py), launched through Blender as described in [Setup](../../../Docs/SETUP.md). Its default repository root is derived from the script location, not the working directory; `--root` is an explicit alternate-checkout override. FBX export uses relative dependency mode. Reopening the saved Blender file and parsing FBX path elements verifies there are no active external dependencies. `model-audit.json` records geometry counts and SHA-256 hashes with paths relative to the repository root.

The generator replaces its own master/exports. Preserve manual variants under new names before regeneration. For placement, Unity preparation and verification see [Fishing presentation](../../../Docs/FISHING-PRESENTATION.md).

Native saved-file location and FBX exporter/header provenance are not active dependencies. The format-aware checks above found no image, linked-library or FBX external file dependencies; no binary bytes were replaced to rewrite metadata.
