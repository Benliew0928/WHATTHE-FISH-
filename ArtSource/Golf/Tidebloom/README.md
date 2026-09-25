# Tidebloom Island

Editable master: `Island_Tidebloom.blend`. The original flat placeholder is archived at `Legacy/ArtSource/Golf/Island.blend`.

Art direction uses `Docs/VisualDirection/golf-cartoon-concept.png` as a theme reference: rounded limestone, lime fairways, warm cream sand, coral flags, broad palms and turquoise water. The island layout is original: one long curved fairway, five shallow sand bowls, a raised destination green, a side practice green, rope-lined arrival tee, a west sea arch and an east spring cascade. These are environment assets; golf strokes, scoring and ball physics are not implemented by this change.

## Authoring

Run Blender 5.1 with `--background --python Tools/Blender/build_golf.py -- --render` from the repository root. The deterministic generator writes the master, the live `GolfIsland.fbx`, seven PNG maps, a Unity import manifest, and four source renders. It only regenerates golf. Run `Tools/Blender/audit_golf.py` in Blender to validate saved geometry and texture packing without regenerating.

The terrain is a continuous radial mesh split into 16 sectors. Plan coordinates are Blender X/Y, with Z as height, in metres. The coastline has an organic radius of roughly 170–215 m; this replaces the old flat 420 m rounded square. Terrain rises into the green, while sand bowls have shallow walkable slopes. The master has named collections for terrain, rocks, palms, garden patches, water, course furniture and background landmarks. Review cameras and the sun stay out of FBX export.

## Unity integration

`GolfBuilder.cs` maps the `TB_` slots to URP/Lit materials, builds terrain and limestone MeshColliders, simplified palm obstacles and 192 invisible shoreline segments. The generated manifest supplies ten spawn positions and the coastline, so these no longer rely on hardcoded square dimensions. The FBX import reverses both plan axes; the Unity builder rotates the art child 180 degrees while keeping manifest coordinates in X/Z. Preserve that conversion if changing the exporter.

The 2048 px course color map contains rough, fairway stripes, green collars, sand rims and shoreline variation. Small shared maps provide grain, limestone and water detail; two radial gradient maps blend shallow water into the sea. Images are packed in the master and also exported as PNGs, so runtime materials do not depend on Blender procedural nodes. The water view scrolls texture coordinates at runtime. Foliage is batched into spatial patches with distance culling; terrain sectors remain independently cullable. This is desktop-verified art, not an Android performance certification.

Use the generator for layout edits that affect spawns/coast/collision. Exporting hand-edited geometry alone does not recalculate the manifest. For visual mesh-only edits, `Tools/Blender/export_assets.py` references this master and preserves its textures, and only exports the current golf asset.


